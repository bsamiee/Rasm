# [MATERIALS_COMPONENT]

THE POLYMORPHIC COMPONENT OWNER, THE CANONICAL `ComputedSection` RECEIPT, and THE FAMILY GROWTH AXIS. One `Component` is the canonical standardized-TYPE concept every steel/timber member, masonry/CMU/IGU unit, and rebar/fastener/connector/joint part parameterizes — marrying a cross-section (a `Component` FIELD, never a peer) to a material over a primary/minor discriminant, plus the family vocabulary its layout schedule reads; one `ComponentFamily` `[SmartEnum<string>]` closes the family-kind axis at NINE {masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint}, each carrying its `ComponentClass` — the primary space-bounding members `steel`/`timber` projecting the `IfcBuiltElement` supertype with geometry-on-type and one-occurrence-one-piece, the minor standardized parts (the other seven) projecting `IfcElementComponent` with one-type-many-pieces. A `Component` is NEVER a per-material class: a brick is a `Component` in the `masonry` family, a steel section a `Component` in the `steel` family, a #5 rebar a `Component` in the `reinforcement` family, a joist hanger a `Component` in the `connector` family, an 8 mm fillet weld a `Component` in the `joint` family, differing only in the `ComponentSection` arm and the family discriminant — never a `BrickProfile`/`SteelSection`/`Rebar`/`Hanger`/`Weld` type. The owner reuses the seam `Rasm.Element/Graph/element#NODE_MODEL` `ObjectKind.Type` identity regime (the ONE `Projection/component#COMPONENT_PROJECTOR` mints the deterministic-rooted Type `Object` and stamps its `Classification`/`PredefinedType` off this owner's `ComponentSection` egress projections); the cross-section machinery — the `ParametricSection` Green's-theorem solver and the `ComputedSection` capacity rail — is Component-internal. The OOP capsule lives at the boundary (the `[ValueObject]` `ComponentId` key, the `[ValueObject]` dimensional columns, the `[ValidationError]`-derived `ComponentFault` band); the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` catalogue projection).

This owner also defines the ONE canonical `ComputedSection` — the twenty-field strong-AND-weak-axis stiffness receipt the `Rasm.Compute` design-code checks read off the seam, its NAME seam-canonical (the `Rasm.Element/Composition/material#MATERIAL_COMPOSITION` `SectionProperties` lifts onto it column-for-column). EVERY profiled family fills it through the SAME `VividOrange.Sections.SectionProperties` Green's-theorem polygon integral, never a per-family closed-form literal: the parametric families (`cmu` hollow net section, `timber` rectangle, a built-up composite) integrate through `ParametricSection.Rectangle`/`Hollow` over a built `Perimeter`, and the `steel` family integrates the catalogued `IProfile` and FILLS the warping / shear-area / plastic columns the elastic integral cannot yield. The page composes the `Rasm` kernel `PositiveMagnitude`/`Dimension`/`UnitInterval` for the dimensional value-objects (the kernel atoms living in `Rasm.Vectors`, never `Rasm.Domain`), the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` row a `Component.AppearanceId` carries, the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt a `Component.CapacityKey` reads, and `Construction/layout#ASSEMBLY_FOLD` for the placement stream a course / schedule / pattern reads over the SAME realized `Resolve` fold. Each family vocabulary lives on its own sibling page (`masonry`/`steel`/`cmu`/`timber`/`glazing`/`reinforcement`/`fastener`/`connector`/`joint`); this page owns the one `Component` shape, the closed family axis, the `ComponentId` key, the `ComponentFault` band, the `ComponentSection` cross-section FIELD, the relocated `Coring` void-class owner, the canonical `ComputedSection` receipt + the shared `ParametricSection` solver, the `ComponentCatalogue` registered-row + section table, and the `[M7]` one-hop `ComponentResolution` that dereferences the seam `ProfileRef` to a `(Component, Option<ComputedSection>)` receipt so a structural consumer never re-runs the section integral per call.

## [01]-[INDEX]

- [02]-[COMPONENT_OWNER]: the `Component` canonical standardized-type shape, the `ComponentClass` primary/minor discriminant, the `ComponentFamily` nine-family axis, the `ComponentId` `family.designation` key, the band-2300 `ComponentFault` `[Union]`, the `ComponentSection` `[Union]` cross-section FIELD with its `Family`/`Class`/`IfcEntity`/`PredefinedToken`/`CrossNominalMm` projections, the relocated `Coring`/`CoringClass` void-class owner, the `ComponentUnit` shared dimensional value-object, the `ComponentAuthority`/`ComponentStandard` standards vocabulary, the canonical `ComputedSection` twenty-field receipt + the shared `ParametricSection` polygon-integral solver, the `Component.Of` polymorphic admission, and the `ComponentCatalogue` registered-row + section table folding all nine families.
- [03]-[COMPONENT_RESOLUTION]: the seam-`ProfileRef` one-hop `ComponentResolution` — the `ResolvedComponent` `(Component, Option<ComputedSection>)` receipt and the frozen `Build`/`Resolve` cache the seam `MaterialComposition.ProfileSet` dereferences ([M7]).

## [02]-[COMPONENT_OWNER]

- Owner: `Component` over the closed `ComponentFamily` axis discriminated by `ComponentClass`; `ComponentId` the `family.designation` key; `ComponentFault` `[Union]` band 2300; `ComparerAccessors.StringOrdinal` accessor; `ComponentSection` the `[Union]` cross-section FIELD carrying the per-family section arms plus the `Family`/`Class`/`IfcEntity`/`PredefinedToken`/`CrossNominalMm` projections; `Coring`/`CoringClass` the relocated cross-family void-class owner; `ComponentUnit` the shared dimensional value-object; `ComputedSection` the canonical twenty-field section receipt; `ParametricSection` the shared polygon-integral solver; `ComponentCatalogue` the registered `ComponentId`→`Component` row table AND the `ComponentId`→`ComputedSection` section table.
- Cases: one `Component` shape across all nine families — `Family` (the discriminant), `Designation` (the `ComponentId` key, `family.designation`), `Section` (the `ComponentSection` arm projecting the family columns), `Coring` (the void-class row, engineering-`None` for every non-masonry/non-cmu family), `Standard` (the regional source receipt over a bounded `ComponentAuthority`), `CapacityKey` (the `MaterialId` whose `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` row carries the engineering capacity the design seam reads), `AppearanceId` (the `graph#MATERIAL_LIBRARY` `MaterialId` row the appearance projection reads). The `CapacityKey`/`AppearanceId` slots are INDEPENDENT — a galvanized fastener whose capacity steel and finish coincide on `metal.steel` is the common case, but a coated rebar (capacity `metal.steel`, appearance an epoxy-coat row) keeps them distinct, so neither derives from the other. Family {masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint}, closed at NINE; `anchor` is a `FastenerKind` arm inside the fastener vocabulary, never a family; a family is a `ComponentFamily` ROW, never a `Component` subtype. The section receipt is one `ComputedSection` shape across all profiled families — the same twenty columns whether sourced from a polygon integral (parametric) or a catalogued `IProfile` (steel).
- Entry: `public static Fin<Component> Of(ComponentFamily family, string designation, ComponentSection section, Coring coring, ComponentStandard standard, MaterialId capacityKey, MaterialId appearanceId, Op key)` — ONE polymorphic admission (the `Profile.Of` + `ConnectionItem.Of` merge), never a `GetById`/`GetByFamily` family; `Fin<T>` aborts on a malformed designation (`ComponentFault.Designation`, key-correlated), a family/section discriminant mismatch (`ComponentFault.Family`), a non-positive `CrossNominalMm` cross dimension (`ComponentFault.Dimension` — a non-positive section column is a dimensional fault, the `Capacity` slot reserved for the section-CAPACITY-SOLVE), or a void fraction outside `[0,1)` (`ComponentFault.Coring`). `ComponentCatalogue.Build(context)` folds every family's `ComponentId`→`Component` row builder into the one frozen registry, `ComponentCatalogue.Sections(context)` folds every profiled family's `ComponentId`→`ComputedSection` section map into the one frozen section table, `ComponentCatalogue.Lookup(rows, id, key)` resolves a registered `ComponentId` to its catalogue `Component`, and the same `Of` admits an ad-hoc item through the row validation a registered row passes.
- Packages: Rasm.Vectors (project — `PositiveMagnitude` the double-backed `> 0` finite magnitude for every length column, `Dimension` the `>= 1` discrete count for layers/thread-starts/gauge, `UnitInterval` the `[0,1]` fraction — the kernel value-object atoms, VERIFIED-LOCAL in `Rasm.Vectors`, NEVER `Rasm.Domain`), Rasm.Domain (project — `Op`/`Context`/`Expected`, the `AcceptValidated` admission extension), Rasm.Element (project — `MaterialId` the appearance/capacity rows reference, `ProfileRef` the seam handle the `[03]` resolves, `SectionProperties` the neutral seam receipt the `MaterialComposition.ProfileSet` case carries onto which `ComputedSection` lifts), VividOrange.Sections.SectionProperties + VividOrange.Profiles.Perimeter + VividOrange.Geometry (the shared parametric section bridge — `new Perimeter(outerEdge, voidEdges)` over `LocalPolyline2d`/`LocalPoint2d` fed to `new SectionProperties(IProfile)`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject]` generators at their deepest surface — generated total `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`); cite `libs/csharp/.api` (the `thinktecture-runtime-extensions`, `unitsnet` substrate catalogues) AND `Rasm.Materials/.api` (the `vividorange-sections-sectionproperties`/`vividorange-profiles-catalogue`/`vividorange-sections` family catalogues).
- Growth: a new standardized component is one `Component` row in the matching `ComponentFamily`; a new family is one `ComponentFamily` case carrying its `ComponentClass`, its `ComponentSection` arm, and its `BuildXRows` catalogue builder on its own sibling page folded into `ComponentCatalogue.Build` (and its `ComponentId`→`ComputedSection` map into `ComponentCatalogue.Sections` when profiled); a new fault is one `ComponentFault` case; a new structural-design column is one `ComputedSection` field every profiled family fills; a new void class is one `Coring` row binding its `CoringClass` — never a `BrickProfile`/`SteelSection`/`Rebar`/`Hanger`/`Weld` parallel type, never a per-family `Component` variant, never a parallel narrow section receipt. The axis is closed at nine: `joint` is the deliberate continuous-connection widening (weld/adhesive/stud carrying no thread or bar diameter), `connector` the fabricated-framing-hardware family.
- Boundary: `Component` is the ONE standardized-type concept — a per-material class is the deleted form; the `ComponentSection` `[Union]` carries each family's section so the one shape never branches into per-family types, every length column composing the `Rasm.Vectors` kernel `PositiveMagnitude` so a fractional millimetre (an AISC web thickness, an 11.3 mm `10M` bar, a 9.525 mm 3/8in bolt, a metric joint module) admits without the truncation an int-backed count forces, the `Dimension` carrier reserved for discrete counts (bar layers, thread starts, sheet gauge). `ComponentFault` is the one fault every `Fin.Fail` reads (nine disjoint slots), an `Expected`-derived `Error` (`IValidationError<ComponentFault>`) whose 2300 band IS the `Expected` `Code` so a bare typed case lifts directly onto the `Fin<T>` rail, disjoint from `ConstructionFault` 2350 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 / `ProjectionFault` 2470 / seam `ElementFault.ValueRejected` 2500 — the former `ConnectionFault` band 2360 is RETIRED into 2300, the merge dedup'ing `Family`/`Capacity` and folding the non-positive-nominal admission onto `Dimension`. The slots stay distinct because a band-by-`Expected.Code`+`Category` telemetry reader separates them: `Dimension` a non-positive/non-finite length (incl. a non-positive section column), `Coring` a void fraction outside `[0,1)`, `Family` a family/section/registration mismatch, `Bond` a masonry COURSE-PATTERN fault, `Mortar` a masonry MORTAR-JOINT-SPEC fault (distinct from `Bond` exactly as `Section` is distinct from `Family`), `Section` a section-INTEGRAL failure (a degenerate net rectangle, a non-finite polygon-solver output), `Capacity` a section-CAPACITY-SOLVE failure (the `capacity#SECTION_CAPACITY` `InteractionDiagram` eager solve / a degenerate RC-timber-masonry design solve — distinct from `Section`'s elastic-integral failure), `Designation` a malformed `ComponentId`, `Grade` a registered-grade-band miss the family `SteelGrade`/`CmuStrength`/`TimberGrade`/`RebarGrade`/`FastenerGrade` derivation rails; this profile-tier `Mortar` SPEC slot stays distinct from the layout-tier `Construction/assembly#PLACEMENT_MODEL` `ConstructionFault.Joint` (band 2350) the run fold rails on a RESOLVED coordinating joint. The `ComponentSection` `IfcEntity`/`PredefinedToken` projections are the egress the `Projection/component#COMPONENT_PROJECTOR` reads to stamp the Type `Object`'s `Classification`/`PredefinedType`: the four discrete-part families project a verified concrete leaf (`reinforcement`→`IfcReinforcingBar` MAIN, `fastener`→`IfcMechanicalFastener` over `FastenerKind.IfcPredefinedType`, `connector`→`IfcDiscreteAccessory` over `ConnectorType.IfcAccessoryType`, `joint`→`IfcMechanicalFastener` for the welded stud else `IfcFastener` over `JointKind.IfcPredefinedType`), the five supertype-projecting families project the role-neutral `ComponentClass` IFC supertype (`IfcBuiltElement` for `steel`/`timber`, `IfcElementComponent` for `masonry`/`cmu`/`glazing`) with the `NOTDEFINED` predefined default — a supertype member's concrete leaf (`IfcBeam`/`IfcColumn`/`IfcWall`/`IfcPlate`) and predefined role are an occurrence/`Construction` refinement the Bim `AdmitPredefined` egress gate validates, never a cross-section property, because a W-shape serves as beam, column, or brace and the cross-section owns neither role nor placement. The `CapacityKey` capacity receipt is NOT re-derived here — it is the `MaterialId` whose `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` row (`DensityKgM3`/`YoungsModulusMpa`/`YieldStrengthMpa`/`PoissonsRatio`/`ThermalExpansionPerK`) the structural-design seam reads by key, so a bolt's proof load and a beam's yield are read once from the property library; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` the `AppearanceId` carries (the FROZEN stable column reconciled at the owner-agnostic `AppearanceKey`), never a component-specific surface. `ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical — the semantic rename STOPS at the Materials folder boundary; `Component` composes them unchanged. The relocated `Coring`/`CoringClass` is the cross-family void-class owner every family carries (masonry/cmu populate it from their void geometry, the rest carry `Coring.None`), the masonry vocabulary now COMPOSING it rather than owning it. The bend/hook geometry of a reinforcement bar and the stamped-plate solid of a framing connector are host-neutral scalar receipts the family pages own, the host materializing them at the `Construction/layout` boundary exactly as a `Placement` tuple, so this owner authors no host curve and no IFC entity.

The canonical `ComputedSection` is the ONE structural-section receipt the whole profiled axis shares — a per-family `W·D²/6` literal is the deleted form. The elastic columns (`AreaMm2`, both-axis second moment `IxMm4`/`IyMm4`, both-axis elastic modulus `SxMm3`/`SyMm3`, both-axis radius of gyration `RxMm`/`RyMm`) and the fire-exposed `HeatedPerimeterMm` come from the ONE `VividOrange.Sections.SectionProperties` polygon integral (`Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter`); the plastic moduli (`ZxMm3`/`ZyMm3`), the St-Venant torsion constant (`JMm4`), the warping constant (`IwMm6`, the EN 1993-1-1 §6.3.2 lateral-torsional-buckling input the bare `J` cannot supply — engineering-zero for the solid/closed parametric families, positive only for an OPEN thin-walled steel shape), the both-axis shear areas (`AvyMm2` the MAJOR-axis/web shear, `AvzMm2` the MINOR-axis/flange shear), AND the asymmetric-section LTB columns (`ShearCentreYMm`/`ShearCentreZMm` the centroid→shear-centre offsets, `MonosymmetryFactor` the EN 1993-1-1 NCCI SN030 β_y) the polygon solver does NOT expose are COMPUTED from the section geometry — the rectangle/hollow closed forms here for the parametric families (which place the shear centre AT the centroid, so all three asymmetry columns are engineering-zero), the `steel#STEEL_FAMILY` `SteelStiffness` per-topology algebra over the catalogued shape (which fills them non-zero ONLY for an open thin-walled channel/tee/angle) — so a steel W-shape and a glulam rectangle resolve the SAME twenty-field receipt and `Projection/component#COMPONENT_PROJECTOR` `SeamSection` lifts the whole set onto the seam `SectionProperties` (mm→SI, each a typed `MeasureValue` in the seam's declared order with `Iw` 5th and the three asymmetry columns last) without re-resolving or admitting VividOrange downstream. `DepthMm`/`WidthMm` are the bounding cross-section dimensions; `AxisDistanceMm` is the EN 1992-1-2 cover-to-reinforcement, engineering-zero for every non-RC section here — the RC reinforcement cover rides the `reinforcement#RC_SECTION` `ConcreteSectionProperties` path, the `ComputedSection` owner declaring the column the RC resolver fills, never a phantom this owner sources.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                          // CultureInfo.InvariantCulture (the ComponentId.Validate format provider)
using LanguageExt;
using Rasm.Vectors;                                  // PositiveMagnitude (>0 finite), Dimension (>=1 count), UnitInterval ([0,1]) — the kernel atoms in Rasm.Vectors (Atoms.cs), NOT Rasm.Domain
using Rasm.Domain;                                   // Op (boundary-admission key), Context, the AcceptValidated admission extension
using Rasm.Element;                                  // MaterialId (the appearance/capacity rows), ProfileRef + SectionProperties (the seam handle + neutral receipt the [03] resolves; seam-canonical, the rename STOPS here)
using Thinktecture;
using VividOrange.Geometry;                          // LocalPoint2d, LocalPolyline2d, ILocalPoint2d, ILocalPolyline2d (the Y-Z section-plane geometry)
using VividOrange.Profiles;                          // Perimeter, IProfile (the parametric section input)
using VividOrange.Sections.SectionProperties;        // SectionProperties polygon-integral solver over IProfile
using UnitsNet;                                      // Length (the section-plane coordinate quantity)
using Rasm.Materials.Component.Masonry;              // MasonryUnit (the masonry section arm payload — masonry COMPOSES the relocated Coring below)
using Rasm.Materials.Component.Cmu;                  // CmuSection
using Rasm.Materials.Component.Steel;                // SteelShape
using Rasm.Materials.Component.Timber;               // TimberSection
using Rasm.Materials.Component.Glazing;              // GlazingSection
using Rasm.Materials.Component.Reinforcement;        // RebarSection
using Rasm.Materials.Component.Fastener;             // FastenerSection, FastenerKind
using Rasm.Materials.Component.Connector;            // ConnectorSection, ConnectorType
using Rasm.Materials.Component.Joint;                // JointSection, JointKind
using Expected = Rasm.Domain.Expected;               // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

// component#COMPONENT_OWNER is the PARENT Rasm.Materials.Component — it owns Component/ComponentClass/ComponentFamily/
// ComponentId/ComponentFault/ComponentSection/Coring/ComponentUnit/ComponentStandard/ComponentAuthority/ComputedSection/
// ParametricSection/ComponentCatalogue and folds each family's own catalogue by the sub-namespace-qualified name
// (Masonry./Cmu./Steel./Timber./Glazing./Reinforcement./Fastener./Connector./Joint.ComponentCatalogue, each its own
// Rasm.Materials.Component.<Family> so the nine sibling ComponentCatalogue static classes are distinct types — the child
// leaf visible from the parent without a using). Coring/CoringClass RELOCATE here from the masonry child (the cross-family
// void-class owner): masonry now COMPOSES the parent Coring rather than owning it.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The component key: 'family.designation' (steel.W14X90, masonry.us-modular, reinforcement.rebar-no5-gr60) — the merge of
// the prior ProfileId 'family.name' and ConnectionId, the literal 'connection.' prefix retired in favour of the family
// prefix so every family keys uniformly. A '.'-separated non-empty token; the family discriminant rides the prefix.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ComponentId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.Contains('.') ? new ValidationError("<component-id requires 'family.designation'>") : null;
}

// The primary/minor discriminant the unified paradigm introduces: a Primary component is a space-bounding structural member
// (steel/timber) whose geometry rides the TYPE and whose every occurrence is one physical piece, projecting the IfcBuiltElement
// supertype; a Minor component is a standardized PART (the other seven families) where one type is realized by MANY pieces,
// projecting IfcElementComponent. The concrete IFC leaf (IfcBeam/IfcWall for a Primary member, IfcReinforcingBar/IfcMechanicalFastener
// for a discrete part) is the ComponentSection.IfcEntity projection's concern (the discrete parts) or the occurrence/Construction
// role refinement (the supertype members); the supertype here is the role-neutral floor the five supertype-projecting families project.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentClass {
    public static readonly ComponentClass Primary = new("primary", ifcSupertype: "IfcBuiltElement");
    public static readonly ComponentClass Minor   = new("minor",   ifcSupertype: "IfcElementComponent");
    public string IfcSupertype { get; }
}

// The nine-family axis, each row carrying its ComponentClass: steel/timber are Primary space-bounding members, the other
// seven Minor standardized parts. anchor folds as a FastenerKind arm inside the fastener vocabulary (never a tenth family);
// joint is the continuous-connection widening, connector the fabricated-framing-hardware family. A family is a ROW, never
// a Component subtype — which families contribute a profiled ComputedSection (steel/cmu/timber) is the per-component
// ResolvedComponent.Section Option a consumer reads, not a redundant family flag.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentFamily {
    public static readonly ComponentFamily Masonry       = new("masonry",       ComponentClass.Minor);
    public static readonly ComponentFamily Cmu           = new("cmu",           ComponentClass.Minor);
    public static readonly ComponentFamily Steel         = new("steel",         ComponentClass.Primary);
    public static readonly ComponentFamily Timber        = new("timber",        ComponentClass.Primary);
    public static readonly ComponentFamily Glazing       = new("glazing",       ComponentClass.Minor);
    public static readonly ComponentFamily Reinforcement = new("reinforcement", ComponentClass.Minor);
    public static readonly ComponentFamily Fastener      = new("fastener",      ComponentClass.Minor);
    public static readonly ComponentFamily Connector     = new("connector",     ComponentClass.Minor);
    public static readonly ComponentFamily Joint         = new("joint",         ComponentClass.Minor);
    public ComponentClass Class { get; }
}

// The published standards body a regional ComponentStandard cites — a bounded vocabulary, never a free Authority string:
// a row per standards organization carrying its scope so a consumer dispatches on the authority rather than string-matching
// "ASTM C216"; a new authority is one row, never a parallel "standard" enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentAuthority {
    public static readonly ComponentAuthority Astm = new("ASTM", region: "us");          // ASTM C90/C216/C652 masonry & CMU, A615/A706 rebar, F3125 bolts
    public static readonly ComponentAuthority Aisc = new("AISC", region: "us");          // AISC Shapes Database
    public static readonly ComponentAuthority Aisi = new("AISI", region: "us");          // AISI S100 cold-formed
    public static readonly ComponentAuthority En   = new("EN",   region: "eu");          // EN 10365/338/14080/14374/1279/10080/898-1
    public static readonly ComponentAuthority Bs   = new("BS",   region: "uk");          // BS EN 771 masonry, BS 8666 rebar schedule
    public static readonly ComponentAuthority Din  = new("DIN",  region: "din");         // DIN 105 masonry
    public static readonly ComponentAuthority As   = new("AS",   region: "au");          // AS 4773 masonry
    public static readonly ComponentAuthority Is   = new("IS",   region: "is");          // IS 1077 masonry
    public static readonly ComponentAuthority Apa  = new("APA",  region: "us");          // APA PRG 320 CLT
    public static readonly ComponentAuthority Csa  = new("CSA",  region: "ca");          // CSA G30.18 rebar, CSA A23.3 concrete
    public string Region { get; }

    public static Fin<ComponentAuthority> Parse(string token, Op key) =>
        TryGet(token, out ComponentAuthority? a) && a is { } v ? Fin.Succ(v) : ComponentFault.Family(key, $"<component-authority-unknown:{token}>");
}

// The cross-family void class — RELOCATED here from the masonry vocabulary (every family carries a Coring, masonry/cmu the
// real void rows, the rest Coring.None). The ASTM C652 / C90 net-area threshold a SolidFraction falls into: C652 fixes the
// hollow classes (H40V <=25% void at <=40% net, H60V) and C62/C216 the solid (>=75% net); NetAreaFloor is the minimum
// net-area ratio the class admits, so cmu#CMU_FAMILY ToCoring buckets a net-area fraction onto the matching row.
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

// The void-class row a Component carries: the shared vocabulary spanning clay-brick (cored/perforated/cellular) AND
// concrete-masonry (hollow 2-cell / 3-cell) geometry, so cmu#CMU_FAMILY ToCoring buckets BOTH the 8-in 2-cell and the
// 12-in 3-cell unit onto a faithful row rather than forcing the 3-cell onto the 2-cell label. VoidFraction stays a double
// in [0,1): Component.Of guards `coring.VoidFraction is >= 0.0 and < 1.0`, a relational pattern over the raw key — the
// bound is the guard's, not a re-minted UnitInterval the pattern cannot read. A non-masonry/non-cmu Component carries
// Coring.None (a solid unit), so the field is total across the nine families.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coring {
    public static readonly Coring None             = new("none",                voidFraction: 0.00, classification: CoringClass.Solid);
    public static readonly Coring Frog             = new("frog",                voidFraction: 0.10, classification: CoringClass.Frogged);
    public static readonly Coring Cored3Hole       = new("cored-3-hole",        voidFraction: 0.20, classification: CoringClass.Cored);
    public static readonly Coring Cellular         = new("cellular",            voidFraction: 0.35, classification: CoringClass.Cellular);
    public static readonly Coring Perforated10Cell = new("perforated-10-cell",  voidFraction: 0.42, classification: CoringClass.Perforated);
    public static readonly Coring Hollow3Cell      = new("hollow-3-cell",       voidFraction: 0.47, classification: CoringClass.Hollow);   // the 12-in 3-cell CMU void class — more web material than the 2-cell unit
    public static readonly Coring Hollow2Cell      = new("hollow-2-cell",       voidFraction: 0.50, classification: CoringClass.Hollow);
    public double VoidFraction { get; }
    public CoringClass Classification { get; }
    public double NetAreaFraction => 1.0 - VoidFraction;   // the C90 net-area ratio the structural capacity scales by
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The component-sub-domain fault band (2300): Expected-derived over the kernel Rasm.Domain.Expected so band 2300 IS the
// Expected Code and a typed case lifts BARE onto Fin<T>/Validation<Error,T> (no .ToError() hop). The kernel base ctor is
// PARAMETERLESS (Code a virtual Error member, Message abstract, Category virtual), so band 2300 is a `Code => 2300` override
// and `Message => Detail`, and the per-case Category override drives FaultExtensions.Category(error). This band MERGES the
// prior ProfileFault 2300 and ConnectionFault 2360 under the one Component owner: 2360 is RETIRED, Family/Capacity dedup'd,
// and the connection non-positive-nominal admission folds onto Dimension (a non-positive section column is a Dimension
// fault; Capacity is reserved for the capacity-SOLVE). The nine slots are disjoint: Dimension a non-positive/non-finite
// length, Coring a void fraction outside [0,1), Family a family/section/registration mismatch, Bond a masonry COURSE-PATTERN
// fault, Mortar a masonry MORTAR-JOINT-SPEC fault, Section a section-INTEGRAL failure, Capacity a section-CAPACITY-SOLVE
// failure, Designation a malformed ComponentId, Grade a registered-grade-band miss. The band is disjoint from ConstructionFault
// 2350 / kernel GeometryFault 2400 / MaterialFault 2450 / ProjectionFault 2470 / seam ElementFault 2500. [SkipUnionOps]
// skips the generated implicit-conversion ops (every case carries an explicit Op) and emits NO per-case factory, so the band
// declares its own: a nested `…Case` record carries the data and a same-name-less static factory returns the Expected-derived
// base so the case lifts BARE onto Fin<T> with no `new` and no .ToError() hop (a same-named nested type + method is CS0102).
// Create routes the unspecific case under a boundary-admission Op.
[SkipUnionOps]
[Union]
public abstract partial record ComponentFault : Expected, IValidationError<ComponentFault> {
    private ComponentFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => 2300;
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
// The shared dimensional value-object the unit families compose (masonry's MasonryUnit, the cmu/timber ToUnit projections):
// width/height/length plus the coursing module, each column a kernel PositiveMagnitude admitted ONCE through ComponentUnit.Of,
// so the interior carries proven-positive magnitudes and no length re-validates. A non-positive/non-finite column rails the
// dimensional admission (key-correlated) at the ONE admission, never a sentinel that seeds a degenerate unit.
public readonly record struct ComponentUnit(PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, PositiveMagnitude LengthMm, PositiveMagnitude CourseHeightMm) {
    // The unit aspect ratio the masonry#MASONRY_FAMILY BondGeometry.Admits tiling gate reads — length-over-HEIGHT (the
    // course-laying ratio a herringbone/diaper cell bounds), the ONE owner-provided derived projection the bond-fit gate
    // composes rather than re-spelling LengthMm.Value / HeightMm.Value inline. NOT length-over-width: the course tiles along
    // the bed, so height — not width — is the load-bearing aspect denominator.
    public double LengthOverHeight => LengthMm.Value / HeightMm.Value;
    public static Fin<ComponentUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: widthMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: heightMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: lengthMm)
        from c in key.AcceptValidated<PositiveMagnitude>(candidate: courseHeightMm)
        select new ComponentUnit(w, h, l, c);
}

// The regional source receipt: the region token + the bounded standards Authority + the as-published coordinating joint
// thickness a masonry/cmu coursing reads (the steel/timber/glazing/connection families pass StandardJointThicknessMm = 0
// because a rolled/sawn/IGU/discrete component has no mortar joint — the column is a masonry/cmu coursing input the
// Construction/layout#ASSEMBLY_FOLD JointPolicy reads, NOT a structural section input). Region stays an explicit token
// because a masonry din/au/is regional row carries a region the single authority does not name.
public readonly record struct ComponentStandard(string Region, double StandardJointThicknessMm, ComponentAuthority Authority);

// The cross-section FIELD — the one Union carrying each family's section row plus the polymorphic egress projections the
// COMPONENT_PROJECTOR reads. The Connector arm payload is named `Hardware` because a case named Connector cannot carry a
// member named Connector (CS0542); every other arm names its payload by the part it carries. The arms reference the family
// section types from their sibling sub-namespaces (the using directives above), the section data captured ONCE at catalogue
// build where the family geometry lives.
[Union]
public abstract partial record ComponentSection {
    public sealed record Masonry(MasonryUnit Unit)             : ComponentSection;
    public sealed record Cmu(CmuSection Section)              : ComponentSection;
    public sealed record Steel(SteelShape Shape)             : ComponentSection;
    public sealed record Timber(TimberSection Section)        : ComponentSection;
    public sealed record Glazing(GlazingSection Section)      : ComponentSection;
    public sealed record Reinforcement(RebarSection Bar)      : ComponentSection;
    public sealed record Fastener(FastenerSection Bolt)       : ComponentSection;
    public sealed record Connector(ConnectorSection Hardware) : ComponentSection;
    public sealed record Joint(JointSection Continuous)       : ComponentSection;

    public ComponentFamily Family => Switch(
        masonry:       static _ => ComponentFamily.Masonry,
        cmu:           static _ => ComponentFamily.Cmu,
        steel:         static _ => ComponentFamily.Steel,
        timber:        static _ => ComponentFamily.Timber,
        glazing:       static _ => ComponentFamily.Glazing,
        reinforcement: static _ => ComponentFamily.Reinforcement,
        fastener:      static _ => ComponentFamily.Fastener,
        connector:     static _ => ComponentFamily.Connector,
        joint:         static _ => ComponentFamily.Joint);

    // The primary/minor structural class the section's family carries — read off the family axis, never a parallel section flag.
    public ComponentClass Class => Family.Class;

    // The single defining cross dimension every family projects — the DISAMBIGUATED nominal (renamed from the prior
    // ConnectionSection.NominalMm so the union projection no longer collides with the JointSection.NominalMm field the joint
    // arm reads). The discrete parts project their bar/thread/carried-member/throat nominal; the profiled members project
    // their governing cross dimension (the section depth for steel/timber, the unit wall-thickness width for masonry/cmu,
    // the IGU build thickness for glazing — the validated-build invariant guarantees a positive overall thickness).
    public PositiveMagnitude CrossNominalMm => Switch(
        masonry:       static m => m.Unit.WidthMm,
        cmu:           static c => c.Section.ActualWidthMm,
        steel:         static s => s.Shape.Section.DepthMm,
        timber:        static t => t.Section.DepthMm,
        glazing:       static g => g.Section.OverallThicknessMm,
        reinforcement: static r => r.Bar.DiameterMm,
        fastener:      static f => f.Bolt.ThreadDiameterMm,
        connector:     static c => c.Hardware.CarriedMemberWidthMm,
        joint:         static j => j.Continuous.NominalMm);

    // The gross bounding (width, depth) rectangle a FAMILY-AGNOSTIC concrete-outline consumer reads — the
    // reinforcement#RC_SECTION RcSectionBuilder.Of feeds it to ParametricSection.ProfileOf(widthMm, depthMm, key) to build
    // the rectangular concrete IProfile a reinforced-masonry/RC section reinforces over (a grouted CMU's actual width×height,
    // an RC beam's width×depth, a timber/steel member's breadth×depth). The four profiled-rectangle families (masonry unit,
    // cmu, steel, timber) project their true bounding pair; the layered/discrete families (glazing IGU, rebar/fastener/
    // connector/joint) — never a concrete outline a consumer feeds — project the CrossNominalMm square so the projection is
    // TOTAL, never a partial Option a caller must unwrap. ProfileOf's own (width,depth) admission re-guards positivity, so a
    // degenerate pair rails ComponentFault.Dimension at the section build, not here.
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm => Switch(
        masonry:       static m => (m.Unit.WidthMm, m.Unit.HeightMm),
        cmu:           static c => (c.Section.ActualWidthMm, c.Section.ActualHeightMm),
        steel:         static s => (s.Shape.Section.WidthMm, s.Shape.Section.DepthMm),
        timber:        static t => (t.Section.WidthMm, t.Section.DepthMm),
        glazing:       static g => (g.Section.OverallThicknessMm, g.Section.OverallThicknessMm),
        reinforcement: static r => (r.Bar.DiameterMm, r.Bar.DiameterMm),
        fastener:      static f => (f.Bolt.ThreadDiameterMm, f.Bolt.ThreadDiameterMm),
        connector:     static c => (c.Hardware.CarriedMemberWidthMm, c.Hardware.CarriedMemberWidthMm),
        joint:         static j => (j.Continuous.NominalMm, j.Continuous.NominalMm));

    // The neutral IFC entity class the seam Object node's Classification("ifc", code) carries (the COMPONENT_PROJECTOR stamps
    // it; the IfcClass roster + AdmitPredefined validity is Rasm.Bim's egress concern). The four discrete-part families project
    // a verified concrete leaf (GG api-geometrygym-ifc: IfcReinforcingBar : IfcReinforcingElement, IfcMechanicalFastener /
    // IfcFastener : IfcElementComponent, IfcDiscreteAccessory : IfcElementComponent — a fabricated framing connector IS the
    // IfcDiscreteAccessory it physically is, the welded shear stud an IfcMechanicalFastener, a weld/adhesive bead the
    // non-mechanical IfcFastener). The five supertype-projecting families (steel/timber/masonry/cmu/glazing) project the role-neutral
    // ComponentClass IFC supertype because a cross-section type owns neither the leaf role (a W-shape serves as beam/column/brace) nor placement — the concrete leaf
    // (IfcBeam/IfcColumn/IfcWall/IfcPlate) is an occurrence/Construction refinement the Bim egress gate resolves.
    public string IfcEntity => Switch(
        masonry:       static _ => ComponentFamily.Masonry.Class.IfcSupertype,
        cmu:           static _ => ComponentFamily.Cmu.Class.IfcSupertype,
        steel:         static _ => ComponentFamily.Steel.Class.IfcSupertype,
        timber:        static _ => ComponentFamily.Timber.Class.IfcSupertype,
        glazing:       static _ => ComponentFamily.Glazing.Class.IfcSupertype,
        reinforcement: static _ => "IfcReinforcingBar",
        fastener:      static _ => "IfcMechanicalFastener",
        connector:     static _ => "IfcDiscreteAccessory",
        joint:         static j => j.Continuous.Kind == JointKind.Stud ? "IfcMechanicalFastener" : "IfcFastener");

    // The verified PredefinedType token the seam Object node carries (the typed value the Bim egress validates against the
    // frozen IfcClass valid-set). The four discrete-part families DELEGATE to their family vocabulary field — a reinforcing
    // bar's MAIN role the catalogue default (the IfcReinforcingBarTypeEnum member, refined when the reinforcement role column
    // grows), the fastener token the FastenerKind.IfcPredefinedType (the IfcMechanicalFastenerTypeEnum members; nut/coupler
    // map to USERDEFINED since the enum has NO NUT member), the connector token the ConnectorType.IfcAccessoryType (the
    // IfcDiscreteAccessoryTypeEnum SHOE/BRACKET/ANCHORPLATE the connector IS — the attaching fastener's IfcMechanicalFastenerTypeEnum
    // rides the connector detail bag, not here), the joint token the JointKind.IfcPredefinedType (the welded stud
    // STUDSHEARCONNECTOR, a weld WELD, an adhesive GLUE). The five supertype-projecting families project NOTDEFINED — a supertype member's
    // predefined role is an occurrence/Construction refinement the Bim egress gate resolves, never a cross-section property.
    public string PredefinedToken => Switch(
        masonry:       static _ => "NOTDEFINED",
        cmu:           static _ => "NOTDEFINED",
        steel:         static _ => "NOTDEFINED",
        timber:        static _ => "NOTDEFINED",
        glazing:       static _ => "NOTDEFINED",
        reinforcement: static _ => "MAIN",
        fastener:      static f => f.Bolt.Kind.IfcPredefinedType,
        connector:     static c => c.Hardware.Type.IfcAccessoryType,
        joint:         static j => j.Continuous.Kind.IfcPredefinedType);
}

// The ONE standardized-type owner: the family discriminant, the ComponentId key, the cross-section FIELD, the void class, the
// regional standard, and the two INDEPENDENT MaterialId slots (CapacityKey the engineering-material Mechanical row, AppearanceId
// the render row — neither derived from the other). The IfcEntity/PredefinedToken read straight off the ComponentSection arm so
// the COMPONENT_PROJECTOR never re-derives the entity per family; the AppearanceId is the FROZEN stable column reconciled at the
// owner-agnostic AppearanceKey, never perturbed here.
public sealed record Component(
    ComponentFamily Family,
    ComponentId Designation,
    ComponentSection Section,
    Coring Coring,
    ComponentStandard Standard,
    MaterialId CapacityKey,
    MaterialId AppearanceId) {

    public string IfcEntity => Section.IfcEntity;
    public string PredefinedToken => Section.PredefinedToken;
    public ComponentClass Class => Family.Class;
    // The gross bounding (width, depth) rectangle a FAMILY-AGNOSTIC concrete-outline consumer reads — forwarded off the
    // ComponentSection arm so the reinforcement#RC_SECTION RcSectionBuilder.Of threads it into ParametricSection.ProfileOf
    // (widthMm, depthMm, key) for the rectangular concrete IProfile a reinforced-masonry/RC section reinforces over, never a
    // Component.Unit field the Component record does not carry (ComponentUnit is the family ToUnit projection, not a Component column).
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm => Section.GrossRectangleMm;

    // The polymorphic admission (the Profile.Of + ConnectionItem.Of merge): the designation validates into the ComponentId,
    // the section discriminant must match the family, the cross nominal must be positive (a Dimension fault, NOT Capacity —
    // the connection band's non-positive-nominal fold), and the void fraction is the one physical invariant the unit owns.
    // The dimensional column admission already happened in the family section/ComponentUnit.Of (the kernel PositiveMagnitude
    // rail), so Of re-guards only the invariants the Component itself owns — a Component cannot re-prove a PositiveMagnitude
    // the kernel admitted.
    public static Fin<Component> Of(ComponentFamily family, string designation, ComponentSection section, Coring coring, ComponentStandard standard, MaterialId capacityKey, MaterialId appearanceId, Op key) =>
        from id in ComponentId.Validate(designation, CultureInfo.InvariantCulture, out ComponentId? built) is { } error
            ? Fin.Fail<ComponentId>(ComponentFault.Designation(key, $"<malformed-designation:{designation}:{error.Message}>"))
            : Fin.Succ(built!)
        from matched in guard(section.Family == family, ComponentFault.Family(key, $"<family-section-mismatch:{family.Key}<>{section.Family.Key}>"))
        from positive in guard(section.CrossNominalMm.Value > 0.0, ComponentFault.Dimension(key, $"<non-positive-nominal:{section.CrossNominalMm.Value}>"))
        from voided in guard(coring.VoidFraction is >= 0.0 and < 1.0, ComponentFault.Coring(key, $"<void-fraction-out-of-range:{coring.VoidFraction:R}>"))
        select new Component(family, id, section, coring, standard, capacityKey, appearanceId);
}

// The ONE canonical section-property receipt every profiled family shares — the FULL structural-design + fire column set the
// Rasm.Compute design-code checks read off the seam, in SI millimetres, its NAME seam-canonical. The elastic columns
// (Area / strong-AND-weak-axis inertia IxMm4=Iyy,IyMm4=Izz / elastic modulus SxMm3=Wely,SyMm3=Welz / radius of gyration) and
// the fire-exposed HeatedPerimeterMm come from the ONE VividOrange polygon integral; the plastic moduli (ZxMm3=Wply,ZyMm3=Wplz),
// the St-Venant torsion constant (JMm4), the warping constant (IwMm6, the EN 1993-1-1 §6.3.2 lateral-torsional-buckling input
// the bare J cannot supply), and the both-axis shear areas (AvyMm2 the MAJOR-axis/web shear -> seam AvY, AvzMm2 the MINOR-axis/
// flange shear -> seam AvZ) the solver does NOT expose are COMPUTED from the section geometry (ParametricSection's rectangle/
// hollow closed forms for the parametric families which fill BOTH shear columns with the symmetric gross net area and so never
// observe the major/minor split; steel#STEEL_FAMILY SteelStiffness over the catalogued shape for the OPEN topologies where the
// split is load-bearing). DepthMm/WidthMm are the bounding dimensions; AxisDistanceMm is the EN 1992-1-2 cover-to-reinforcement
// (0 for a non-RC section, the RC value from the reinforcement#RC_SECTION ConcreteSectionProperties path). ShearCentreYMm/
// ShearCentreZMm/MonosymmetryFactor are the asymmetric-section LTB columns (0/0/0 for every doubly-symmetric/parametric family,
// the steel family filling them non-zero for an open channel/tee/angle). COMPONENT_PROJECTOR SeamSection lifts this whole
// set onto the twenty-field neutral seam SectionProperties (mm -> SI, each a typed MeasureValue in the seam's declared order
// with Iw 5th and the three asymmetry columns last), so a Rasm.Compute structural/fire runner reads graph.SectionOf(member)
// without re-resolving or admitting VividOrange.
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
    PositiveMagnitude AvyMm2,   // MAJOR-axis shear area (the web of an OPEN steel shape, the §G design web shear φVn reads it); the seam AvY
    PositiveMagnitude AvzMm2,   // MINOR-axis shear area (the two flanges); the seam AvZ. A symmetric parametric family (cmu/timber) fills BOTH with the gross net area, so it never observes the major/minor split — load-bearing only for the steel topologies, pinned HERE so a future family fills the pair consistently
    PositiveMagnitude DepthMm,
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeatedPerimeterMm,
    double AxisDistanceMm,
    double ShearCentreYMm,      // centroid→shear-centre offset on the y-axis (mm); the seam ShearCentreY. Engineering-zero for a doubly-symmetric I/HSS/solid section, non-zero for a channel (the PFC one-axis offset) — the EN 1993-1-1 §6.3.2 general LTB input
    double ShearCentreZMm,      // centroid→shear-centre offset on the z-axis (mm); the seam ShearCentreZ. Non-zero for a tee/angle, zero for a doubly-symmetric section — a plain double (the zero-admitting modeling AxisDistanceMm/IwMm6 share)
    double MonosymmetryFactor) {// the EN 1993-1-1 NCCI SN030 β_y mono-symmetry factor; the seam MonosymmetryFactor. Zero for a doubly-symmetric section, signed for a singly-symmetric one (a tee, an unequal-flange shape) — a plain double, NEVER a PositiveMagnitude (a β_y is signed and zero-valid)

    // The WEAK-axis radius the column-buckling check governs about — a derived read over the two solver radii, never a stored
    // column that drifts from the pair the polygon integral supplies (the steel#STEEL_FAMILY Capacity reads it).
    public double GoverningRadiusMm => Math.Min(RxMm.Value, RyMm.Value);
}
// IwMm6 is the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F warping constant (mm^6) the seam SectionProperties carries 5th (after J) —
// a plain double NOT a PositiveMagnitude (the SAME zero-admitting modeling as AxisDistanceMm) because a SOLID/CLOSED section's
// warping resistance is engineering-zero (ParametricSection yields 0.0 for every rectangle/hollow parametric family, and an
// OPEN thin-walled steel shape carries its own positive Iw from the steel-family SteelStiffness); the seam Warping map lifts it
// to MeasureValue.OfSi(QuantityType.Create("WarpingConstant"), Dimension.Create(6,0,0,0,0,0,0), mm6 * 1e-18). ShearCentreYMm/
// ShearCentreZMm (the centroid→shear-centre offsets) and MonosymmetryFactor (β_y) are the asymmetric-section LTB columns the
// seam SectionProperties carries last — all three plain doubles (signed, zero-valid) the SAME way AxisDistanceMm/IwMm6 are:
// engineering-zero for every doubly-symmetric/parametric family (ParametricSection yields 0/0/0 for every rectangle/hollow,
// so the seam IsDoublySymmetric reads them as symmetric), non-zero ONLY for an OPEN thin-walled channel/tee/angle the
// steel-family SteelStiffness computes from the section geometry — the EN 1993-1-1 §6.3.2 GENERAL LTB route's inputs a PFC/
// tee needs and a symmetric I-section leaves zero, so the SeamSection lift fills the three seam columns faithfully rather
// than hardcoding zero past an asymmetric steel shape. The seam ShearCentre lifts map them MeasureValue.OfSi(QuantityType
// .Length, Dimension.LengthDim, mm * 1e-3); MonosymmetryFactor crosses dimensionless.

// --- [OPERATIONS] --------------------------------------------------------------------------
// The shared parametric section-property bridge: one VividOrange.Sections.SectionProperties Green's-theorem integral over a
// built Perimeter (outer polyline + void edges) computes the elastic columns for a non-catalogued section, so a cmu hollow net
// section, a timber rectangle, and a built-up composite all integrate EXACTLY through the SAME solver the steel#STEEL_FAMILY
// runs over a catalogued IProfile — no per-family literal. The plastic/torsion/shear columns the elastic integral cannot yield
// arrive from the rectangle/hollow closed forms below (the three asymmetry columns ShearCentreYMm/ShearCentreZMm/Monosymmetry
// Factor engineering-zero for a doubly-symmetric rectangle), so the whole twenty-field ComputedSection is filled for a
// parametric family the way SteelStiffness fills it for a catalogued one.
public static class ParametricSection {
    public static Fin<ComputedSection> Rectangle(double widthMm, double depthMm, Op key) =>
        Solve(RectanglePerimeter(widthMm, depthMm, Seq<(double, double, double, double)>()), depthMm, widthMm,
            RectanglePlastics(widthMm, depthMm), key);

    // The hollow net section: the outer rectangle minus the inset cell voids — the exact net Area AND net inertia (cells
    // subtracted from the second moment), not an approximate gross-minus-cell-area scalar.
    public static Fin<ComputedSection> Hollow(double widthMm, double depthMm, Seq<(double X, double Y, double W, double H)> voids, Op key) =>
        Solve(RectanglePerimeter(widthMm, depthMm, voids), depthMm, widthMm, HollowPlastics(widthMm, depthMm, voids), key);

    // The gross-rectangle IProfile outline the reinforcement#RC_SECTION RcSection.Of consumer feeds to a VividOrange.Sections
    // ConcreteSection as the concrete outline — the family-agnostic admission threads its own gross width/depth (a grouted CMU's
    // actual width/height for reinforced masonry, an RC beam's width/depth), decoupled from the ComponentSection Union so the RC
    // solver and the gross section properties share one Perimeter. A catalogued steel section passes its own ICatalogue IProfile;
    // this is the gross rectangular outline only, railing ComponentFault.Dimension on a degenerate pair (the RcSection.Of consumer
    // threads the rail).
    public static Fin<IProfile> ProfileOf(double widthMm, double depthMm, Op key) =>
        widthMm > 0.0 && depthMm > 0.0
            ? Fin.Succ((IProfile)RectanglePerimeter(widthMm, depthMm, Seq<(double, double, double, double)>()))
            : ComponentFault.Dimension(key, $"<component-perimeter-degenerate:{widthMm:R}x{depthMm:R}>");

    // The plastic/torsion/shear columns the VividOrange ELASTIC polygon integral does not expose, COMPUTED from the rectangle
    // geometry (closed-form, EXACT for a solid rectangle): the plastic moduli Z = b·h²/4 (shape factor 1.5 over the elastic
    // W = b·h²/6), the St-Venant rectangle torsion constant J = a·b³·(1/3 − 0.21·(b/a)·(1 − (b/a)⁴/12)) with a/b the long/short
    // side (Roark Table 10.1, a/b ≥ 1), and the plastic shear area Av = A (EN 1993-1-1 §6.2.6(3): a solid section's shear area
    // is its gross area). A catalogued thin-walled steel shape carries its OWN SteelStiffness plastic/web-flange shear areas.
    static (double Zx, double Zy, double J, double Avy, double Avz) RectanglePlastics(double w, double d) {
        double area = w * d;
        return (w * d * d / 4.0, d * w * w / 4.0, Torsion(w, d), area, area);
    }

    // The hollow net columns via gross-minus-void superposition (the doubly-symmetric centred cells the cmu#CMU_FAMILY generates
    // straddle both centroidal bending axes, so the net plastic modulus is the gross minus each void's own plastic modulus and
    // the net torsion the gross minus each void's St-Venant constant; the net shear area is the gross minus the void material)
    // — a documented engineering net-section approximation for the perforated rectangle.
    static (double Zx, double Zy, double J, double Avy, double Avz) HollowPlastics(double w, double d, Seq<(double X, double Y, double W, double H)> voids) {
        double zx = w * d * d / 4.0 - voids.Sum(static v => v.W * v.H * v.H / 4.0);
        double zy = d * w * w / 4.0 - voids.Sum(static v => v.H * v.W * v.W / 4.0);
        double j = Torsion(w, d) - voids.Sum(static v => Torsion(v.W, v.H));
        double netArea = w * d - voids.Sum(static v => v.W * v.H);
        return (zx, zy, j, netArea, netArea);
    }

    // The Roark Table 10.1 St-Venant torsion constant for a solid rectangle, a/b the long/short side — the one closed form
    // RectanglePlastics and HollowPlastics share, so the rectangle torsion is computed ONCE not transcribed twice.
    static double Torsion(double a, double b) {
        double lng = Math.Max(a, b), sht = Math.Min(a, b);
        return lng * sht * sht * sht * (1.0 / 3.0 - 0.21 * (sht / lng) * (1.0 - Math.Pow(sht / lng, 4) / 12.0));
    }

    static Fin<ComputedSection> Solve(Perimeter perimeter, double depthMm, double widthMm, (double Zx, double Zy, double J, double Avy, double Avz) plastics, Op key) =>
        Admit(new SectionProperties((IProfile)perimeter), depthMm, widthMm, plastics, key);

    // The elastic columns + the fire-exposed Perimeter come from the VividOrange polygon integral; the plastic moduli / torsion /
    // shear areas the solver cannot yield arrive precomputed from the rectangle/hollow geometry; Depth/Width are the bounding
    // dimensions and AxisDistance is zero (a non-RC parametric section — the RC cover rides the reinforcement#RC_SECTION path).
    // Every column admits once through the kernel PositiveMagnitude rail, so a degenerate (non-positive net) section drops to
    // ComponentFault.Section rather than seeding a corrupt stiffness on the seam — the Section slot, never Dimension or Family.
    static Fin<ComputedSection> Admit(SectionProperties p, double depthMm, double widthMm, (double Zx, double Zy, double J, double Avy, double Avz) plastics, Op key) =>
        (from area in Section(p.Area.SquareMillimeters, key)
         from ix in Section(p.MomentOfInertiaYy.MillimetersToTheFourth, key)
         from iy in Section(p.MomentOfInertiaZz.MillimetersToTheFourth, key)
         from sx in Section(p.ElasticSectionModulusYy.CubicMillimeters, key)
         from sy in Section(p.ElasticSectionModulusZz.CubicMillimeters, key)
         from rx in Section(p.RadiusOfGyrationYy.Millimeters, key)
         from ry in Section(p.RadiusOfGyrationZz.Millimeters, key)
         from zx in Section(plastics.Zx, key)
         from zy in Section(plastics.Zy, key)
         from jj in Section(plastics.J, key)
         from avy in Section(plastics.Avy, key)
         from avz in Section(plastics.Avz, key)
         from depth in Section(depthMm, key)
         from width in Section(widthMm, key)
         from perim in Section(p.Perimeter.Millimeters, key)
         // IwMm6: 0.0 — a solid/closed RECTANGLE (every parametric family) has engineering-zero warping resistance; AxisDistanceMm:
         // 0.0 yields a zero cover for a non-RC section; ShearCentreYMm/ShearCentreZMm/MonosymmetryFactor: 0.0 — a doubly-symmetric
         // rectangle/hollow places its shear centre AT the centroid (zero offsets, zero β_y), EXACT not lossy. All five lift to a
         // zero MeasureValue (the three asymmetry columns reading as symmetric at the seam IsDoublySymmetric) at the SeamSection map.
         select new ComputedSection(area, ix, iy, sx, sy, rx, ry, zx, zy, jj, IwMm6: 0.0, avy, avz, depth, width, perim, AxisDistanceMm: 0.0, ShearCentreYMm: 0.0, ShearCentreZMm: 0.0, MonosymmetryFactor: 0.0));

    // The section-column admission: a positive finite SI-millimetre magnitude into the kernel PositiveMagnitude, the value-object's
    // own Fin RE-LABELLED to the Section fault slot (a degenerate net column is a SECTION failure, not a raw dimension input) so a
    // structural consumer reads the true cause; AcceptValidated already enforces > 0 finite.
    static Fin<PositiveMagnitude> Section(double mm, Op key) =>
        key.AcceptValidated<PositiveMagnitude>(candidate: mm).MapFail(_ => ComponentFault.Section(key, $"<section-column-nonpositive:{mm:R}>"));

    // The section plane is VividOrange.Geometry's Y-Z: a centred rectangle is four LocalPoint2d corners, each cell a void polyline;
    // Perimeter(outerEdge, voidEdges) closes the polygons the integral iterates (the (ILocalPolyline2d, IList<ILocalPolyline2d>) ctor).
    static Perimeter RectanglePerimeter(double w, double d, Seq<(double X, double Y, double W, double H)> voids) =>
        new(Loop(-w / 2, -d / 2, w, d),
            voids.Map(v => Loop(v.X - v.W / 2, v.Y - v.H / 2, v.W, v.H)).ToList<ILocalPolyline2d>());

    static ILocalPolyline2d Loop(double y0, double z0, double w, double h) =>
        new LocalPolyline2d(new List<ILocalPoint2d> {
            new LocalPoint2d(Length.FromMillimeters(y0), Length.FromMillimeters(z0)),
            new LocalPoint2d(Length.FromMillimeters(y0 + w), Length.FromMillimeters(z0)),
            new LocalPoint2d(Length.FromMillimeters(y0 + w), Length.FromMillimeters(z0 + h)),
            new LocalPoint2d(Length.FromMillimeters(y0), Length.FromMillimeters(z0 + h)),
        });
}

// --- [TABLES] ------------------------------------------------------------------------------
// The ONE catalogue: the registered ComponentId -> Component rows (every one of the nine families contributes its BuildXRows)
// AND the registered ComponentId -> ComputedSection sections (the three profiled families steel/cmu/timber contribute their
// section maps), each computed ONCE at family build where its family section record is in scope. The section map is the M7
// substrate: ComponentResolution.Build joins the two by ComponentId, so a section is captured ONCE at the site that owns the
// family geometry, never reconstructed from a bare Component (which has discarded the catalogue identity, the cell voids, the
// topology) — the deleted Func<Component, Op, Fin<ComputedSection>> phantom. ComponentId's generated ordinal value-equality keys
// the frozen dictionaries (the [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>] above), so no explicit
// string comparer (a type mismatch on a ComponentId key) is threaded.
public static class ComponentCatalogue {
    public static FrozenDictionary<ComponentId, Component> Build(Context context) =>
        Masonry.ComponentCatalogue.BuildMasonryRows(context)
            .Concat(Cmu.ComponentCatalogue.BuildCmuRows(context))
            .Concat(Steel.ComponentCatalogue.BuildSteelRows(context))
            .Concat(Timber.ComponentCatalogue.BuildTimberRows(context))
            .Concat(Glazing.ComponentCatalogue.BuildGlazingRows(context))
            .Concat(Reinforcement.ComponentCatalogue.BuildRebarRows(context))
            .Concat(Fastener.ComponentCatalogue.BuildFastenerRows(context))
            .Concat(Connector.ComponentCatalogue.BuildConnectorRows(context))
            .Concat(Joint.ComponentCatalogue.BuildJointRows(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value);

    // The ComponentId -> ComputedSection table the M7 resolution caches off — folded from each PROFILED family's own section
    // map (steel#STEEL_FAMILY SteelSections, cmu#CMU_FAMILY CmuSections, timber#TIMBER_FAMILY TimberSections), each computed
    // ONCE at family build over its family section record. Masonry/glazing (a unit course / an IfcMaterialLayerSet) and the four
    // connection families (a discrete part / continuous joint, no ComputedSection) contribute none — they are absent from this
    // map, so ComponentResolution.Build joins their rows to Option.None, never a forged section.
    public static FrozenDictionary<ComponentId, ComputedSection> Sections(Context context) =>
        Steel.ComponentCatalogue.SteelSections
            .Concat(Cmu.ComponentCatalogue.CmuSections(context))
            .Concat(Timber.ComponentCatalogue.TimberSections(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value);

    public static Fin<Component> Lookup(FrozenDictionary<ComponentId, Component> rows, ComponentId id, Op key) =>
        rows.TryGetValue(id, out Component? row) && row is { } r ? Fin.Succ(r) : ComponentFault.Family(key, $"<unregistered-component:{id.Value}>");
}
```

## [03]-[COMPONENT_RESOLUTION]

- Owner: `ResolvedComponent` the one-hop `(Component, Option<ComputedSection>)` receipt; `ComponentResolution` the seam-`ProfileRef` resolver and the frozen resolution cache the `[M7]` consumers read.
- Cases: one `ResolvedComponent` shape across all nine families — a `Component` (the standardized type) plus an `Option<ComputedSection>` (the twenty-field stiffness receipt computed once through the family's section path, `Some` for a profiled structural member, `None` for a non-profiled one); a `ProfileRef` keys exactly one `ResolvedComponent`. A component registered in the catalogue but ABSENT from the section map (a masonry unit, a glazing IGU, any of the four connection families) resolves to its `Component` with `Option<ComputedSection>.None` — the seam-honest absence, NOT a fabricated all-zero section (a `PositiveMagnitude` rejects zero, so an all-zero `ComputedSection` is unrepresentable AND a forged-zero is the named anti-pattern), so the cache is total over every registered `ProfileRef` and a non-structural ref resolves without a fault. The `Option` aligns with the seam `MaterialComposition.ProfileSet(Material, Profile, Option<SectionProperties>)` the projector bakes: a `None` resolution bakes no section, a `Some` resolution bakes the lifted seam `SectionProperties`.
- Entry: `public static FrozenDictionary<ProfileRef, ResolvedComponent> Build(FrozenDictionary<ComponentId, Component> rows, FrozenDictionary<ComponentId, ComputedSection> sections)` pre-resolves every catalogued component to its `(Component, Option<ComputedSection>)` once — keyed by the SEAM `ProfileRef.Of(componentId.Value)` (`ProfileRef` STAYS seam-canonical, the rename STOPS at the Materials boundary), joining the two frozen maps the `ComponentCatalogue` already computed (a profiled member joins `Some(section)`, a non-profiled one `None`) — so resolution is an O(1) total join with no fault rail and no `Op` key (every row resolves; the integral ran once at catalogue build, in the family page that owns the geometry); `public static Fin<ResolvedComponent> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedComponent> table, Op key)` is the one-hop dereference the seam `Rasm.Element/Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.ProfileSet` consumer (`capacity#SECTION_CAPACITY`, the `Rasm.Compute` structural route) calls, `Fin<T>` aborting on an unregistered ref (`ComponentFault.Family`, the registration slot — distinct from the `ComponentFault.Section` integral failure that already fell out at catalogue build).
- Packages: Rasm.Element (project — `ProfileRef` the seam handle the composition carries, `SectionProperties` the neutral receipt the `MaterialComposition.ProfileSet` case the resolution feeds), Rasm.Domain (project — `Context`/`Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`); cite `libs/csharp/.api` substrate.
- Growth: a new resolvable attribute is one column on `ComputedSection` (a shear-centre offset, a monosymmetry parameter) the family section path fills; a new family's section participation is one entry in `ComponentCatalogue.Sections` (the family's own `ComponentId`→`ComputedSection` map), so a new family's section path is a catalogue contribution, never a resolver edit — the resolver owns the one-hop cache, the family pages own the section computation, and the `ComponentCatalogue` owns the join. There is NO `sectionOf` delegate: the section is data captured at build, not a function re-invoked at resolution.
- Boundary: `ComponentResolution` is the `[M7]` ONE-HOP — the seam `MaterialComposition.ProfileSet` carries a `ProfileRef` (the catalogue key the `Construction/assembly#MATERIAL_COMPOSITION` author wrapped from a `ComponentId`), and a structural consumer dereferences it ONCE through `Resolve` to the `(Component, Option<ComputedSection>)` receipt rather than re-running the `ParametricSection` Green's-theorem integral or the steel `SectionReader` catalogue lookup per design-check call; the `Build` cache is the frozen pre-resolution every catalogued component passes through, the section captured at the SAME catalogue-build site the family geometry lives (steel through its `SteelSections` map keyed by the `American`/`European` identity, the parametric families through their `CmuSections`/`TimberSections` maps over the built perimeter), so a steel W-shape and a glulam rectangle both carry their section into the one cache WITHOUT the resolver ever seeing a bare `Component` it cannot re-derive a section from — the deleted `Func<Component, Op, Fin<ComputedSection>>` phantom (a `Component` retains only its `ComponentSection` arm, having the catalogue identity / cell voids / topology a section integral needs only at the family-build site, so a `Component`-keyed `sectionOf` was unrealizable for steel and lossy for the parametric rectangle). The resolver owns NO section math (the family pages + `ParametricSection` own it) and NO seam type (`ProfileRef`/`SectionProperties` are the seam's) — it owns the one-hop dereference and the frozen cache. `Build` joins by membership in the section map: a `ComponentId` present in BOTH maps carries `Some(section)`; a `ComponentId` present only in the row map (a masonry/glazing/connection non-structural component) carries `None` so the cache is total and a non-structural `ProfileRef` resolves without a fault — the named defects being a structural consumer re-resolving a `ProfileRef` per call, a forged all-zero `ComputedSection` for a non-profiled ref (unrepresentable — a `PositiveMagnitude` rejects zero), and the silent-drop a `Choose`-over-`sectionOf` that vanished a structural component whose integral failed at build (now a build-time `ParametricSection.Admit` `ComponentFault.Section` the family `BuildXRows` `Choose` surfaces in its own page, never a swallowed gap the resolver mis-reports as "unregistered").

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op, Context
using Rasm.Element;                  // ProfileRef, SectionProperties (the seam handle + neutral receipt the MaterialComposition.ProfileSet case carries; seam-canonical, the rename STOPS here)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [MODELS] ------------------------------------------------------------------------------
// The one-hop resolution receipt: a ProfileRef dereferences to its Component AND its OPTIONAL computed section in one lookup,
// so a structural consumer reads the section without re-running the section integral per call (M7). Section is
// Some(canonical ComputedSection) for a profiled structural family (steel/cmu/timber), None for a non-profiled one (masonry
// course / glazing IfcMaterialLayerSet / the four connection families) — the seam-honest absence, never a forged all-zero
// section (a PositiveMagnitude rejects zero, so an all-zero ComputedSection is UNREPRESENTABLE). The Option mirrors the seam
// MaterialComposition.ProfileSet(Material, Profile, Option<SectionProperties>) the Projection/component projector bakes.
public readonly record struct ResolvedComponent(Component Component, Option<ComputedSection> Section);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentResolution {
    // Pre-resolves every catalogued component to its (Component, Option<ComputedSection>) once, keyed by the SEAM ProfileRef the
    // composition carries; the frozen table is the M7 one-hop — resolution is an O(1) lookup, the section a build-time JOIN of
    // the two frozen catalogue maps (rows + sections), NOT a Func re-invoked here. A component with a section entry
    // (steel/cmu/timber) joins Some(section); one with none (masonry/glazing/the connection families) joins None so the cache is
    // total over every registered ProfileRef. No section integral runs at resolution — it ran once at catalogue build in the
    // owning family.
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

## [04]-[RESEARCH]

- [COMPONENT_OWNER_MERGE]: REALIZED — the prior `Profile` and `ConnectionItem` owners collapse into ONE `Component` over the closed nine-family `ComponentFamily` axis, the cross-section a `Component` FIELD (`ComponentSection`) rather than a peer. A brick, a steel W-shape, a #5 rebar, a joist hanger, and a fillet weld are all `Component` rows differing only in the `ComponentSection` arm and the family discriminant — the `BrickProfile`/`SteelSection`/`Rebar`/`Hanger`/`Weld` parallel types and the prior dual Profile/Connection owners are the deleted forms. `Component.Of` is the one polymorphic admission (the `Profile.Of` + `ConnectionItem.Of` merge), `ComponentCatalogue.Build` the one fold over all nine `BuildXRows` builders. The `ComponentId` `family.designation` key unifies the prior `ProfileId` `family.name` and `ConnectionId`, the literal `connection.` prefix retired.
- [COMPONENT_CLASS_DISCRIMINANT]: REALIZED — `ComponentClass` `[SmartEnum<string>]` {Primary, Minor} is the new discriminant the unified paradigm introduces, each `ComponentFamily` row carrying its class (steel/timber Primary, the other seven Minor). Primary projects the `IfcBuiltElement` supertype (geometry-on-type, one occurrence one piece); Minor projects `IfcElementComponent` (one type, many pieces). The discriminant drives the `ComponentSection.IfcEntity` egress: the four discrete-part families project a verified concrete leaf, the five supertype-projecting families (steel/timber/masonry/cmu/glazing) the role-neutral class supertype with `NOTDEFINED`, the concrete leaf and role an occurrence/`Construction` refinement the Bim `AdmitPredefined` egress gate validates.
- [COMPONENT_SECTION_FIELD]: REALIZED — `ComponentSection` `[Union]` carries the nine family section arms (`Masonry`/`Cmu`/`Steel`/`Timber`/`Glazing`/`Reinforcement`/`Fastener`/`Connector`/`Joint`) plus the polymorphic `Family`/`Class`/`IfcEntity`/`PredefinedToken`/`CrossNominalMm` projections so the one `Component` shape never branches into per-family types. `CrossNominalMm` is the disambiguated nominal (the prior `ConnectionSection.NominalMm` renamed so the union projection no longer collides with the `JointSection.NominalMm` field the joint arm reads). The `Connector` arm payload is `Hardware` (a case named `Connector` cannot carry a member named `Connector`).
- [CANONICAL_COMPUTEDSECTION]: PRESERVED — `ComputedSection` is the ONE twenty-field section-property receipt the whole profiled axis shares, its NAME seam-canonical (the `Rasm.Element/Composition/material#MATERIAL_COMPOSITION` `SectionProperties` lifts onto it column-for-column, the three asymmetric-section LTB columns `ShearCentreYMm`/`ShearCentreZMm`/`MonosymmetryFactor` last). It is filled IDENTICALLY whether the section is a parametric rectangle/hollow (`ParametricSection` over a built `VividOrange.Profiles.Perimeter`, the three asymmetry columns engineering-zero) or a catalogued steel shape (`steel#STEEL_FAMILY` over the catalogued `IProfile` + `SteelStiffness` for the warping/shear/plastic AND the shear-centre-offset/mono-symmetry columns) — a per-family `W·D²/6` literal and a parallel narrow per-family section receipt are the deleted forms. Every column admits once through the kernel `PositiveMagnitude` rail, a degenerate net section railing `ComponentFault.Section`. The `ParametricSection` solver (Rectangle/Hollow/ProfileOf/Admit) stays Component-internal; `ProfileOf` is the gross-rectangle outline the `reinforcement#RC_SECTION` consumer feeds, decoupled from the section Union to a `(widthMm, depthMm)` admission.
- [COMPONENT_RESOLUTION_AS_JOIN]: REALIZED — `ComponentResolution.Build` resolves a seam `ProfileRef` to its `(Component, Option<ComputedSection>)` by JOINING the two frozen catalogue maps the `ComponentCatalogue` already computed (`ComponentId`→`Component` and `ComponentId`→`ComputedSection`), so the section is DATA captured once at the catalogue-build site where the family geometry lives — NOT a `Func<Component, Op, Fin<ComputedSection>>` delegate re-invoked at resolution. The three profiled families (steel/cmu/timber) expose their own build-time section maps; the masonry/glazing/connection families are absent and join `Option<ComputedSection>.None` so the cache is total. `ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical — the semantic rename STOPS at the Materials folder boundary.
- [FAULT_BAND_MERGE]: REALIZED — `ComponentFault` band 2300 merges the prior `ProfileFault` 2300 and `ConnectionFault` 2360 into nine disjoint slots {Dimension, Coring, Family, Bond, Mortar, Section, Capacity, Designation, Grade}, the 2360 band RETIRED, `Family`/`Capacity` dedup'd, and the connection non-positive-nominal admission folded onto `Dimension`. The Expected-derived bare-lift discipline and the disjoint-from-kernel invariant (2350/2400/2450/2470/2500) preserve.
- [CORING_RELOCATION]: REALIZED — `Coring`/`CoringClass` relocate from the masonry vocabulary to this Component owner as the cross-family void-class (every `Component` carries a `Coring`, masonry/cmu the real void rows, the other seven `Coring.None`). The masonry family now COMPOSES the parent `Coring` rather than owning it; the cmu `ToCoring` net-area bucketing reads the same shared vocabulary.
- [IFC_TYPE_IDENTITY]: the `Component` owner reuses the seam `Rasm.Element/Graph/element#NODE_MODEL` `ObjectKind.Type` identity regime — the ONE `Projection/component#COMPONENT_PROJECTOR` mints the deterministic-rooted Type `Object` from the `Component`'s canonical content and stamps its `Classification`/`PredefinedType` off the `ComponentSection.IfcEntity`/`PredefinedToken` projections, binding occurrences via the `Assign.TypeDefinition` edge. The projector itself (the `MaterialProjector` + `ConnectionProjector` merge) lives at `Projection/component#COMPONENT_PROJECTOR`, never re-minting an element identity for an occurrence the model author or Bim ingest already rooted.
