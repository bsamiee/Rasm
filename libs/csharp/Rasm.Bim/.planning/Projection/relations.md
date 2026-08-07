# [BIM_RELATION_ALGEBRA]

The IFC relationship vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner: the `IfcRelKind` `[SmartEnum<string>]` row-driven `IfcRel*` roster (the `[UseDelegateFromConstructor]` neutral edge CONSTRUCTOR each name lowers through, plus the relating/related IFC inverse-attribute names directionality round-trips on), and the `EdgeProjection` fold lowering every relationship family onto a NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship` edge. This page is the relationship half of the `Projection/semantic#SEMANTIC_PROJECTOR` ingress projector — the `Project` fold composes `EdgeProjection.All` to land every `IfcRel*` neutral edge — and the SAME roster reverses at the `Projection/egress#IFC_EGRESS` `ReauthorRelationships` re-author through the `IfcRelKind.ForNeutral` reverse index and `IfcRelKind.Author`. The whole IFC relationship vocabulary — every `IfcRel*` name, its directionality, and its inverse — plus the eight families the retired owner stranded ride the neutral edge algebra [NEUTRAL_EDGE_RULING], so no relationship is dropped and the seam stays IFC-schema-free. GeometryGym leaks no relationship case below the seam: the typed case carries only its `SubKind`, the IFC wire-name + inverse living on the `IfcRelKind` ROW, and only `Generic` carries the wire-name and the per-edge attribute payload.

The roster is ONE owner widened on one axis — a new IFC relationship is one `IfcRelKind` row carrying its edge constructor and its inverse-attribute names, plus, when it carries a typed payload, one dedicated `EdgeProjection` arm — never a parallel `RelationshipKind` on the seam and never a per-relationship projector type. The roster covers the FULL concrete node-to-node `IfcRelationship` subtree of the live GeometryGym surface — decompile-censused, so an unrostered node-to-node family cannot exist silently — and the payload-bearing SUBTYPES (`IfcRelAssignsToGroupByFactor`, `IfcRelConnectsWithEccentricity`, `IfcRelOverridesProperties`, `IfcRelSpaceBoundary1stLevel`/`2ndLevel`) ride their base-class `Extract<T>` arms by construction. The structural member↔connection/member↔activity idealization, the space↔surface boundary graph, the property/quantity attachment, and the material occurrence-usage ride DEDICATED `EdgeProjection` folds onto neutral `Generic`/`Assign.PropertyDefinition`/`Associate` edges, not the generic row path, so a `Rasm.Compute` runner reads the structural/energy connectivity off the seam graph by wire-name and the `Projection/egress#IFC_EGRESS` `Emit` re-authors the covered families exactly as they were read. Ordered nesting is the ONE ingest exception: `IfcRelNests.RelatedObjects` is a schema `LIST`, so the `Decomposition` fold routes every nest through `Relationship.Generic` carrying the per-parent-continuous zero-based `SemanticProjector.NestOrdinal` attribute — the typed `Compose{Nest}` case and its canonical bytes stay byte-identical for authored graphs, and egress re-orders the fan-in from the attribute before `Author`.

## [01]-[INDEX]

- [02]-[RELATION_ALGEBRA]: `IfcRelKind` the full `IfcRel*` roster `[SmartEnum<string>]` (the delegate-borne neutral edge constructor it lowers through, plus the relating/related IFC inverse-attribute names directionality round-trips on — decompile-censused over the complete concrete node-to-node `IfcRelationship` subtree), the `RelSlots` reflected fill surface the census and the `Author` share, and the `EdgeProjection` fold lowering every relationship onto a neutral `Relationship` edge — the FanOut/Pair generic families, the ordinal-carrying ordered-nest `Generic` arm, the inverted `Assign` arms (`DefinesByType`/`AssignsToGroup`), the bare and realizing `Connect` arms carrying the content-keyed `Interface` surface, the `DefinesProperties` property/quantity attachment, the `Structural` member↔connection/member↔activity `Generic` edges (the `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection.Attrs` two restraint families (member end release + joint support) + full load family + `LoadKind`/`Case` payload and the `StructuralProjection.AtStart` start/end discriminant, the eccentric subtype's mandatory `ConnectionConstraint` content-keyed through the store by that same owner; the analytical axis content-keyed in `Representations` by `Model/elements#REPRESENTATION_KEYS` `IfcRepresentation.Keys`, never baked onto the node), the `SpatialBoundaries` space↔surface `Generic` edges, and the `MaterialEdges` `Associate` material edge carrying the occurrence-usage payload [OCCURRENCE_USAGE_RULING] beside its per-imported-Pset `Assign.PropertyDefinition` bag binding.

## [02]-[RELATION_ALGEBRA]

- Owner: `IfcRelKind` the `[SmartEnum<string>]` carrying the WHOLE row-driven `IfcRel*` roster keyed on the relationship entity name — each row carrying the `[UseDelegateFromConstructor]` `Edge` constructor that BUILDS its neutral seam edge and the relating-side/related-side IFC inverse-attribute names directionality round-trips on [NEUTRAL_EDGE_RULING]; `RelSlots` the reflected relating-setter/related-slot/typed-`Add` triple resolved once per row; `EdgeProjection` the static fold lowering every relationship family onto neutral `Relationship` edges. The neutral cases carry a typed `SubKind`, never a wire-name or an attribute bag — the IFC wire-name + inverse live on the `IfcRelKind` ROW and reconstruct at egress through the reverse index, and only `Generic` carries the wire-name and the per-edge attribute payload.
- Cases: the row-driven roster — `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` (`Compose`), `IfcRelConnectsElements`/`IfcRelConnectsWithRealizingElements`/`IfcRelConnectsPorts` (`Connect`), `IfcRelDefinesByType`/`IfcRelAssignsToGroup` (`Assign`), `IfcRelVoidsElement`/`IfcRelFillsElement` (`Void`) — PLUS the families that ride `Generic` because no neutral sub-kind fits: `IfcRelConnectsPortToElement` (the port↔element shape distinct from port↔port), `IfcRelConnectsPathElements` (wall-join priorities), `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity`, `IfcRelConnectsStructuralElement` (the 2x3 element↔idealized-member binding), `IfcRelSpaceBoundary`, `IfcRelInterferesElements`, `IfcRelSequence`, `IfcRelCoversBldgElements`/`IfcRelCoversSpaces`, `IfcRelAssignsToControl`/`Process`/`Product`/`Actor`, `IfcRelDeclares`, `IfcRelServicesBuildings`, `IfcRelProjectsElement` (the additive-feature counterpart of `Voids`), `IfcRelAdheresToElement` (IFC4.3 surface features), `IfcRelFlowControlElements` (the GG port→controlled-element surface), `IfcRelPositions` (IFC4.3 positioning-element→products), `IfcRelDefinesByObject` (declaring-object typing) — each lands its neutral edge carrying the IFC wire-name and the directionality/inverse on the `Generic` payload, so no relationship among the covered node-to-node families is dropped and none re-opens the IFC schema strata leak on the seam. `IfcRelAssociatesMaterial` (the `Associate` material edge with its `MaterialUsage` payload), `IfcRelDefinesByProperties` (the `Assign.PropertyDefinition` bag attachment — its `IfcRelOverridesProperties` subtype rides the same `Extract<T>`), and the structural/space-boundary payloads ride DEDICATED folds, not the generic row path.
- Law: the row's edge constructor is the SINGLE owner of the row's neutral identity — the lowering axis, the neutral `SubKind`, and the directionality inversion all READ OFF it through one sentinel probe at type init, so no Bim-side axis enum stands beside the seam's own `RelationshipKind` and no five-arm dispatch or hand-kept `Inverted` column can drift from the arm that built the edge.
- Law: a census hit proves CALLABILITY, never a name — a row's related slot resolves the exact `Add(memberType)` overload the fan-out invokes when the slot is a SET and its setter when it is single-valued, and both the census and `Author` read that ONE `RelSlots` resolution; a name-only `GetMethod("Add")` matches several arities and throws the ambiguous match before naming which one it meant.
- Entry: `EdgeProjection.All(IfcProject project, Map<string,NodeId> rooted, double tolerance, UnitScale scale, Option<EurocodePolicy> eurocode, TemplateScope templates, IIfcProfileStore profiles, Op key)` folds every relationship family into a `Fin<Noted<Seq<Relationship>>>` — the fold's own fidelity facts RETURNED beside the edges (the `GroupFactors` scan over the unslotted `IfcRelAssignsToGroupByFactor` riders joined with the `MaterialProjection.ImportedPsets` narrowing log), never a ledger this page holds — `scale` the per-projection native→SI coercion every measured payload (structural spring/force, material usage offset/extent, material composition thickness) multiplies through and `eurocode` the projector's ctor-held annex-and-situation policy the `Structural` load arm resolves its EN 1990 factors under, both threaded from the `Projection/semantic#SEMANTIC_PROJECTOR` fold head so no arm rebuilds a regime off its own entity — the `FanOut`/`Pair` generic helpers read the relating/related `GlobalId`s, resolve them through the `rooted` map, look up the `IfcRelKind` row, and construct the neutral edge through `row.Edge`; the ordered `IfcRelNests` alone bypasses `row.Edge` and lands one `Relationship.Generic(IfcRelKind.Nests.Key, …)` per child carrying the zero-based `SemanticProjector.NestOrdinal` attribute (a `PropertyValue.Integer` ordinal) so `RelatedObjects` `LIST` order round-trips; the `Assign` arms (`DefinesByType`/`AssignsToGroup`) are INVERTED (the seam `Assign` is `Subject`(occurrence)→`Definition`(type/group), the inverse of the IFC relating→related), so they read each related occurrence as the subject and the relating type/group as the definition; the realizing `Connect` FANS OUT one edge per `RealizingElements` member over the same `(From, To)` pair — the seam `Connect.Realizing` option carries one realizer per edge, so a multi-realizer joint (plate + bolts) survives whole and egress re-groups the fan into ONE `IfcRelConnectsWithRealizingElements` carrying every member (the `.Head` slice that dropped every realizer past the first is the deleted form) — and BOTH element-connect arms content-key their optional `IfcConnectionGeometry` through `PreserveInterface` onto the seam `Connect.Interface` slot; `DefinesProperties` lands the `IfcRelDefinesByProperties` (whose `RelatingPropertyDefinition` is a SET) property/quantity attachment as `Assign(occurrence, definition, PropertyDefinition)` per (occurrence, definition) pair; `Structural` lands `IfcRelConnectsStructuralMember` (member→connection, the rel-level end-release and connection-level support restraint families off their own `AppliedCondition`→`IfcBoundaryNodeCondition` reads riding the payload) and `IfcRelConnectsStructuralActivity` (item→load, the applied force/moment off `IfcStructuralActivity.AppliedLoad`→`IfcStructuralLoadSingleForce` riding the payload) as `Generic` edges; `SpatialBoundaries` lands `IfcRelSpaceBoundary` (space→bounding-surface, the THREE-valued `BoundaryLevel` discriminant riding the payload — `"2nd"`/`"1st"` for the exact subtype the egress refined-construct re-authors, `""` for a base instance — beside the `InterfaceKey` attr carrying the boundary's own content-keyed `ConnectionGeometry`) as `Generic` edges; `MaterialEdges` lands the `Associate` material edges with the occurrence-usage payload [OCCURRENCE_USAGE_RULING] plus one `Assign(material, bag, PropertyDefinition)` edge per imported `MaterialProjection.ImportedPsets` bag — the bag node id re-derived through the SAME `SemanticProjector.PropertySetNode` content mint the node-side `Materials` fold takes, so both ends key identically without a shared table; `Fin<T>` aborts on a dangling endpoint (`BimFault.DanglingReference`).
- Auto: `row.Edge(relating, related)` invokes the row's OWN constructor delegate, which spells its seam case and its neutral `SubKind` (`ComposeKind.Aggregate`, `ConnectKind.Element`, `AssignKind.Group`, `VoidKind.Fill`) or builds a `Generic(Key, …)` carrying the IFC wire-name — every `Connect` construction spelling the seam's full five-argument arity with `Option<UInt128>.None` where the joint carries no interface surface, so no call site reads a defaulted slot; the structural/space-boundary `Generic` edges carry their typed payload (both restraint families' fixity booleans and SI springs, the SI force/moment measures, the boundary level) as `PropertyValue` attribute entries the `Rasm.Compute` runners read by wire-name; the material `Associate` edge threads the `Semantics/composition#MATERIAL_COMPOSITION` occurrence-usage payload [OCCURRENCE_USAGE_RULING] (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`, `CardinalPoint`/`ReferenceExtent`) as the seam's typed `MaterialUsage` (`LayerSet`/`ProfileSet`), never a `PropertyValue` attribute bag, the `ProfileSet` usage admitted through the seam `MaterialUsage.ProfileSet.Of` cardinal-point gate.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new IFC relationship is one `IfcRelKind` row (its edge constructor and its inverse-attribute names, both proven against the GeometryGym surface by the type-init census at first touch) plus, when it carries a payload, one dedicated `EdgeProjection` arm; a new directionality is the row's relating/related columns; the neutral edge algebra absorbs the case and `Generic` absorbs the residue — never a parallel `RelationshipKind` on the seam and never a dropped family among the covered node-to-node relationships. A relationship to a NON-node IFC resource (an `IfcRelAssociatesDocument`/`Library`/`Constraint`/`Approval`/`Dataset`/`ProfileDef`/`ProfileProperties` definition association, an `IfcRelDefinesByTemplate` template binding, an `IfcRelAssignsToResource`) is a SEPARATE Growth axis — a new seam `Node` case plus its row — never silently in scope of the covered set. A payload a rider subtype adds beyond its base edge is either SOLVED or a NAMED bounded drop, never silent: the eccentricity, the connection interface, and the space-boundary level are solved riders — the `IfcRelConnectsWithEccentricity` `ConnectionConstraint` content-keys through the store's STEP-fragment lane onto the structural owner's `Eccentricity` row and egress reconstitutes the exact subtype, the optional element-connection and space-boundary `ConnectionGeometry` content-key through that SAME lane onto the seam `Connect.Interface` slot and the `InterfaceKey` attr, and the three-valued `BoundaryLevel` attr drives the egress refined-construct — while `IfcRelAssignsToGroupByFactor.Factor` (no seam slot on the typed `Assign` case) and the `IfcRelOverridesProperties` override semantics (no `AssignKind` override row; re-emits as plain `IfcRelDefinesByProperties`) remain next-campaign payload/rows on the `NestOrdinal` precedent.
- Boundary: the seam `Relationship` is the NEUTRAL edge algebra plus `Generic` — the seam carries no typed `IfcRel*` case and re-introducing typed IFC cases is the deleted form [NEUTRAL_EDGE_RULING]; the IFC names/directionality/inverse and the long-tail families live HERE on the Bim side, the neutral case carrying only its `SubKind` and the IFC wire-name reconstructing at egress through the reverse index, the `Generic` passthrough carrying any residue of the covered node-to-node families so a dropped family among them is the named defect (a relationship to a non-node IFC resource is a Growth axis — a new seam `Node` case — never a covered residue); directionality is preserved by the row's relating/related orientation and, for the inverted `Assign` family, the row's DERIVED `Inverted` read the egress re-orients on (`ReauthorRelationships` swaps the seam `Subject`/`Definition` back to the IFC relating/related before `Author`), never inferred at the call site; the row's attribute names are the ONLY binding `Author` has and `Construct` registers the entity before either side sets, so an unresolvable name or an uncallable `Add` overload is a type-init refusal and never a per-emit silent no-op that leaves a half-bound `IfcRel*` in a delivered file, and `Author` rails `Fin` so an empty related set and an unconstructible class are two named authoring failures rather than one `None` a caller cannot read; the material occurrence-usage rides the `Associate` edge's typed `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and a parallel usage node is the deleted form; the structural/space-boundary connectivity rides the NEUTRAL `Generic` wire-name payload, never a typed IFC relationship case, so the strata stay IFC-schema-free — and the space boundary's interface surface therefore rides the `InterfaceKey` ATTR while an element connect's rides the typed `Connect.Interface` SLOT, because the seam `ConnectKind` medium vocabulary is closed at element/path/port and a space↔bounding-surface boundary is none of the three, so a fourth medium row minted to reach the typed slot is the deleted phantom; both keys name the same preserved STEP fragment in the one store; the nest order contract is the egress-declared `SemanticProjector.NestOrdinal` attribute on the `Generic("IfcRelNests", …)` edges — egress groups those edges by relating endpoint and authors ONE ordered `IfcRelNests` from the ordinal sort, while an authored `Compose{Nest}` edge keeps its frozen `(Compose, "nest")` reverse-index row, so the ingest routing and the typed case never collide.

```csharp signature
using System.Collections.Frozen;
using System.Reflection;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim;
using Rasm.Bim.Model;                                 // StructuralProjection + the EurocodePolicy regime its load arm resolves under
using Rasm.Bim.Semantics;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Projection;

// --- [MODELS] -----------------------------------------------------------------------------
// The reflected fill surface of ONE IfcRelKind row: the relating setter, the related slot, and — when the related slot
// is a SET — the EXACT Add(memberType) overload the fan-out invokes. Of resolves all three off the DECLARED property
// types and answers None on any miss, so the type-init census and the Author read ONE resolution and a census hit
// proves a call the author can actually make.
sealed record RelSlots(PropertyInfo Relating, PropertyInfo Related, Option<MethodInfo> Add) {
    public static Option<RelSlots> Of(IfcRelKind row) =>
        from shape in Optional(typeof(IfcRelationship).Assembly.GetType($"{typeof(IfcRelationship).Namespace}.{row.Key}"))
        from relating in Optional(shape.GetProperty(row.Relating)).Filter(static slot => slot.CanWrite)
        from related in Optional(shape.GetProperty(row.Related))
        from resolved in Adder(related.PropertyType) is { IsSome: true } add ? Some(new RelSlots(relating, related, add))
            : related.CanWrite ? Some(new RelSlots(relating, related, Option<MethodInfo>.None))
            : Option<RelSlots>.None
        select resolved;

    // A SET-valued slot is a GeometryGym SET<T> — an ICollection<T>, NEVER a System.Collections.IList — so the member
    // fill resolves the TYPED Add(T) overload off the collection interface's own argument. The name-only
    // GetMethod("Add") this replaces matches several arities and throws AmbiguousMatchException before naming which
    // one it meant, so a roster row passed a census whose Add the author then failed to invoke.
    static Option<MethodInfo> Adder(Type slotType) =>
        Optional(slotType.GetInterfaces().FirstOrDefault(static face =>
                face.IsGenericType && face.GetGenericTypeDefinition() == typeof(ICollection<>)))
            .Bind(face => Optional(slotType.GetMethod(nameof(ICollection<object>.Add), [face.GetGenericArguments()[0]])));
}

// The row-driven IfcRel* roster: name -> the neutral edge CONSTRUCTOR it lowers through + the relating/related IFC
// inverse-attribute names directionality round-trips on. The long-tail families join here on a Generic constructor so
// EVERY relationship has a home; the neutral edge keeps the wire-name + inverse so IFC directionality round-trips
// without a seam IFC case [NEUTRAL_EDGE_RULING]. Material/property/classification/structural/space-boundary ride dedicated
// EdgeProjection arms, not row.Edge. No axis enum stands beside the seam's own RelationshipKind: a Bim-side copy of the
// seam union's discriminant is the parallel-discriminant form the seam accessor law already deleted, and the row's
// lowering axis, sub-kind, and inversion all read off the ONE delegate below.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcRelKind {
    public static readonly IfcRelKind Aggregates             = new("IfcRelAggregates",                   "RelatingObject",            "RelatedObjects",              static (r, e) => new Relationship.Compose(r, e, ComposeKind.Aggregate));
    public static readonly IfcRelKind Nests                  = new("IfcRelNests",                        "RelatingObject",            "RelatedObjects",              static (r, e) => new Relationship.Compose(r, e, ComposeKind.Nest));
    public static readonly IfcRelKind ContainedInStructure   = new("IfcRelContainedInSpatialStructure",  "RelatingStructure",         "RelatedElements",             static (r, e) => new Relationship.Compose(r, e, ComposeKind.Contain));
    public static readonly IfcRelKind ReferencedInStructure  = new("IfcRelReferencedInSpatialStructure", "RelatingStructure",         "RelatedElements",             static (r, e) => new Relationship.Compose(r, e, ComposeKind.Reference));
    public static readonly IfcRelKind ConnectsElements       = new("IfcRelConnectsElements",             "RelatingElement",           "RelatedElement",              static (r, e) => new Relationship.Connect(r, e, ConnectKind.Element, Option<NodeId>.None, Option<UInt128>.None));
    public static readonly IfcRelKind ConnectsRealizing      = new("IfcRelConnectsWithRealizingElements","RelatingElement",           "RelatedElement",              static (r, e) => new Relationship.Connect(r, e, ConnectKind.Element, Option<NodeId>.None, Option<UInt128>.None));
    public static readonly IfcRelKind ConnectsPorts          = new("IfcRelConnectsPorts",                "RelatingPort",              "RelatedPort",                 static (r, e) => new Relationship.Connect(r, e, ConnectKind.Port, Option<NodeId>.None, Option<UInt128>.None));
    public static readonly IfcRelKind DefinesByType          = new("IfcRelDefinesByType",                "RelatingType",              "RelatedObjects",              static (r, e) => new Relationship.Assign(r, e, AssignKind.TypeDefinition));
    public static readonly IfcRelKind AssignsToGroup         = new("IfcRelAssignsToGroup",               "RelatingGroup",             "RelatedObjects",              static (r, e) => new Relationship.Assign(r, e, AssignKind.Group));
    public static readonly IfcRelKind Voids                  = new("IfcRelVoidsElement",                 "RelatingBuildingElement",   "RelatedOpeningElement",       static (r, e) => new Relationship.Void(r, e, VoidKind.Void));
    public static readonly IfcRelKind Fills                  = new("IfcRelFillsElement",                 "RelatingOpeningElement",    "RelatedBuildingElement",      static (r, e) => new Relationship.Void(r, e, VoidKind.Fill));
    public static readonly IfcRelKind ConnectsPortToElement  = new("IfcRelConnectsPortToElement",        "RelatingPort",              "RelatedElement",              Passthrough("IfcRelConnectsPortToElement"));
    public static readonly IfcRelKind ConnectsPathElements   = new("IfcRelConnectsPathElements",         "RelatingElement",           "RelatedElement",              Passthrough("IfcRelConnectsPathElements"));
    public static readonly IfcRelKind ConnectsStructMember   = new("IfcRelConnectsStructuralMember",     "RelatingStructuralMember",  "RelatedStructuralConnection", Passthrough("IfcRelConnectsStructuralMember"));
    public static readonly IfcRelKind ConnectsStructActivity = new("IfcRelConnectsStructuralActivity",   "RelatingElement",           "RelatedStructuralActivity",   Passthrough("IfcRelConnectsStructuralActivity"));
    public static readonly IfcRelKind ConnectsStructElement  = new("IfcRelConnectsStructuralElement",    "RelatingElement",           "RelatedStructuralMember",     Passthrough("IfcRelConnectsStructuralElement"));
    public static readonly IfcRelKind SpaceBoundary          = new("IfcRelSpaceBoundary",                "RelatingSpace",             "RelatedBuildingElement",      Passthrough("IfcRelSpaceBoundary"));
    public static readonly IfcRelKind Projects               = new("IfcRelProjectsElement",              "RelatingElement",           "RelatedFeatureElement",       Passthrough("IfcRelProjectsElement"));
    public static readonly IfcRelKind Adheres                = new("IfcRelAdheresToElement",             "RelatingElement",           "RelatedSurfaceFeatures",      Passthrough("IfcRelAdheresToElement"));
    // GG deviates from the schema attribute pair here (no RelatingFlowElement/RelatedControlElements on the
    // public surface) — the decompiled members ARE RelatingPort/RelatedElement, so the row records the REAL wire.
    public static readonly IfcRelKind FlowControl            = new("IfcRelFlowControlElements",          "RelatingPort",              "RelatedElement",              Passthrough("IfcRelFlowControlElements"));
    public static readonly IfcRelKind Positions              = new("IfcRelPositions",                    "RelatingPositioningElement","RelatedProducts",             Passthrough("IfcRelPositions"));
    public static readonly IfcRelKind DefinesByObject        = new("IfcRelDefinesByObject",              "RelatingObject",            "RelatedObjects",              Passthrough("IfcRelDefinesByObject"));
    public static readonly IfcRelKind InterferesElements     = new("IfcRelInterferesElements",           "RelatingElement",           "RelatedElement",              Passthrough("IfcRelInterferesElements"));
    public static readonly IfcRelKind Sequence               = new("IfcRelSequence",                     "RelatingProcess",           "RelatedProcess",              Passthrough("IfcRelSequence"));
    public static readonly IfcRelKind CoversElements         = new("IfcRelCoversBldgElements",           "RelatingBuildingElement",   "RelatedCoverings",            Passthrough("IfcRelCoversBldgElements"));
    public static readonly IfcRelKind CoversSpaces           = new("IfcRelCoversSpaces",                 "RelatingSpace",             "RelatedCoverings",            Passthrough("IfcRelCoversSpaces"));
    public static readonly IfcRelKind AssignsToControl       = new("IfcRelAssignsToControl",             "RelatingControl",           "RelatedObjects",              Passthrough("IfcRelAssignsToControl"));
    public static readonly IfcRelKind AssignsToProcess       = new("IfcRelAssignsToProcess",             "RelatingProcess",           "RelatedObjects",              Passthrough("IfcRelAssignsToProcess"));
    public static readonly IfcRelKind AssignsToProduct       = new("IfcRelAssignsToProduct",             "RelatingProduct",           "RelatedObjects",              Passthrough("IfcRelAssignsToProduct"));
    public static readonly IfcRelKind AssignsToActor         = new("IfcRelAssignsToActor",               "RelatingActor",             "RelatedObjects",              Passthrough("IfcRelAssignsToActor"));
    public static readonly IfcRelKind Declares               = new("IfcRelDeclares",                     "RelatingContext",           "RelatedDefinitions",          Passthrough("IfcRelDeclares"));
    public static readonly IfcRelKind ServicesBuildings      = new("IfcRelServicesBuildings",            "RelatingSystem",            "RelatedBuildings",            Passthrough("IfcRelServicesBuildings"));

    public string Relating { get; }
    public string Related { get; }

    // The neutral edge CONSTRUCTOR the row carries: the row IS its own lowering, so the axis it folds onto and the
    // neutral SubKind the typed case takes are spelled once, HERE, in the arm that builds the edge — never a column a
    // separate dispatch has to keep in step with. A payload-bearing family rides a dedicated EdgeProjection arm and
    // never this constructor; a realizing Connect's intermediary node is filled by that arm, so every Connect row
    // spells the seam's full five-argument arity with both Options None and no call site reads a defaulted slot.
    [UseDelegateFromConstructor]
    public partial Relationship Edge(NodeId relating, NodeId related);

    // The Generic passthrough constructor: the wire-name a long-tail family round-trips under plus an empty attribute
    // bag. The name is spelled inside the delegate because a static lambda cannot read the row it belongs to — so the
    // type-init census below PROVES the delegate's name equals the row's key, and a stale spelling dies at first touch
    // rather than round-tripping under a name the roster never declared.
    static Func<NodeId, NodeId, Relationship> Passthrough(string wireName) =>
        (relating, related) => new Relationship.Generic(wireName, relating, related, Map<PropertyName, PropertyValue>());

    // The two sentinel endpoints every derivation below probes a row's own Edge with. They never reach a graph: only
    // the constructed case's discriminants are read, so the delegate stays the ONE owner of the row's neutral identity.
    static readonly NodeId ProbeRelating = NodeId.Create("00000000000000000000000000000001");
    static readonly NodeId ProbeRelated = NodeId.Create("00000000000000000000000000000002");

    // The reflected slots each row's Author fills, resolved ONCE at type init off the DECLARED property types — the
    // census verdict and the Author cache are ONE resolution, so no emit re-probes and no admitted row can carry an
    // Add the author cannot invoke.
    static readonly FrozenDictionary<IfcRelKind, RelSlots> Slots;

    // Type-init census over the GeometryGym assembly (the egress MeasureMints resolution idiom, the BimZoneKind census
    // law), two verdicts in ONE pass because both read the same rows: every row's class resolves with a WRITABLE
    // relating slot and a FILLABLE related slot — the exact Add(memberType) overload when the slot is a SET, its
    // setter when it is single-valued — and every Generic row's delegate spells the row's own key. Author registers
    // the entity on the database through Construct BEFORE either side binds, so a miss cannot be undone at the call
    // site: the writer serializes the half-bound IfcRel* regardless, and a schema-invalid relationship in a delivered
    // file is indistinguishable from an authoring choice. A pin bump that renames an attribute, narrows a SET member
    // type, or seals a setter therefore dies HERE at first touch, which is exactly what the FlowControl row already
    // proves can happen: GG publishes RelatingPort/RelatedElement where the schema names another pair.
    static IfcRelKind() {
        Seq<(IfcRelKind Row, Option<RelSlots> Slots)> probed = Items.AsIterable().ToSeq().Map(static row => (Row: row, Slots: RelSlots.Of(row)));
        Seq<string> refused = probed.Filter(static p => p.Slots.IsNone).Map(static p => p.Row.Key)
            + probed.Filter(static p => p.Row.Edge(ProbeRelating, ProbeRelated) is Relationship.Generic g && g.WireName != p.Row.Key)
                    .Map(static p => p.Row.Key);
        if (!refused.IsEmpty) { throw new InvalidOperationException($"<rel-row-unbound:{string.Join(',', refused)}>"); }
        Slots = probed.Choose(static p => p.Slots.Map(slots => (p.Row, Slots: slots)))
                      .ToFrozenDictionary(static p => p.Row, static p => p.Slots);
    }

    // Directionality inversion is DERIVED from the row's own edge constructor, never a hand-kept column: the seam
    // Assign reads Subject(occurrence)->Definition(type/group) while every IFC assign relation reads
    // relating(definition)->related(occurrences), so an Assign-producing row is inverted BY CONSTRUCTION and every
    // other row already reads in IFC orientation. Egress re-inverts on this read before Author.
    public bool Inverted => Inversions.Value.Contains(this);

    static readonly Lazy<FrozenSet<IfcRelKind>> Inversions = new(static () =>
        Items.AsIterable().Filter(static row => row.Edge(ProbeRelating, ProbeRelated) is Relationship.Assign).ToFrozenSet());

    // The egress reverse index, DERIVED from the same edge constructors rather than a second neutral column: each row
    // is probed once and keyed on the SEAM's own (RelationshipKind, sub-kind) discriminant, so Edge and ForNeutral
    // cannot drift and no Bim-side axis vocabulary exists to fork. A Generic row yields no sub-kind and stays out
    // (Generic resolves by wire-name through TryGet), as does ConnectsRealizing: it shares the Connect "element"
    // sub-kind with ConnectsElements because realization is the seam Connect.Realizing FIELD, never a sub-kind row, so
    // egress disambiguates the two by the Realizing None/Some field, not this index. The direct ToFrozenDictionary IS
    // the uniqueness gate — a colliding (kind, sub-kind) pair fails at type initialization (the FaultBand registry
    // law), never a GroupBy/First mask silently electing a winner.
    static readonly Lazy<FrozenDictionary<(RelationshipKind, string), IfcRelKind>> ByNeutral = new(static () =>
        Items.AsIterable()
             .Filter(static row => row != ConnectsRealizing)
             .Choose(static row => NeutralKey(row).Map(neutral => (Neutral: neutral, Row: row)))
             .ToFrozenDictionary(static entry => entry.Neutral, static entry => entry.Row));

    static Option<(RelationshipKind Kind, string SubKind)> NeutralKey(IfcRelKind row) => row.Edge(ProbeRelating, ProbeRelated) switch {
        Relationship.Compose c => Some((RelationshipKind.Compose, c.SubKind.Key)),
        Relationship.Assign a  => Some((RelationshipKind.Assign, a.SubKind.Key)),
        Relationship.Connect c => Some((RelationshipKind.Connect, c.SubKind.Key)),
        Relationship.Void v    => Some((RelationshipKind.Void, v.SubKind.Key)),
        _                      => Option<(RelationshipKind Kind, string SubKind)>.None,
    };

    public static Option<IfcRelKind> ForNeutral(RelationshipKind kind, string subKind) =>
        ByNeutral.Value.TryGetValue((kind, subKind), out IfcRelKind? row) && row is { } resolved ? Some(resolved) : None;

    // The EGRESS author: construct this row's IFC relationship and bind the relating/related sides to the row's IFC
    // attribute names, so directionality + inverse round-trip and the long-tail families re-emit from the row
    // [NEUTRAL_EDGE_RULING] — the caller (ReauthorRelationships) feeds the endpoints already in IFC orientation, re-inverting the
    // inverted Assign family first. Fin, never Option: an empty related set and an unconstructible class are two
    // distinct authoring failures, and the silent None left the caller unable to say whether a relationship was
    // written at all while the half-authored entity reached the writer either way. Endpoints are
    // IfcObjectDefinition-wide: a DefinesByType relating side is an IfcTypeObject and an AssignsToGroup relating side
    // an IfcGroup, neither an IfcProduct — a product-typed signature is the deleted crash-cast form. The slot reads
    // are TOTAL by the type-init census, so this member never probes a name or an overload that might miss.
    // FactoryIfc.Construct mints the db-bound entity by class name (BaseClassIfc.Construct builds it, the factory
    // registers it on the database). The refined slot is the SUBTYPE-construct discriminant an edge attr carries and
    // the row cannot (the egress space-boundary level -> IfcRelSpaceBoundary1stLevel/2ndLevel); the row's attribute
    // names still fill the endpoints because a rider subtype shares its base's relating/related pair.
    public Fin<IfcRelationship> Author(DatabaseIfc db, IfcObjectDefinition relating, Seq<IfcObjectDefinition> related, Op key, Option<string> refined = default) {
        if (related.IsEmpty) {
            return Fin.Fail<IfcRelationship>(new BimFault.CodecReject(key, $"relation-related-empty:{Key}"));
        }
        string authored = refined.IfNone(Key);
        if (db.Factory.Construct(authored) is not IfcRelationship rel) {
            return Fin.Fail<IfcRelationship>(new BimFault.CodecReject(key, $"relation-unconstructible:{authored}"));
        }
        RelSlots slots = Slots[this];
        slots.Relating.SetValue(rel, relating);
        // A SET-valued related side fills member-by-member through the census-resolved Add(memberType); a
        // single-valued side takes the head through its setter. Insertion order is the fill order, which is what makes
        // the egress ordered-nest author's ordinal sort survive into IfcRelNests.RelatedObjects.
        slots.Add.Match(
            Some: add => related.Iter(member => add.Invoke(slots.Related.GetValue(rel), [member])),
            None: () => slots.Related.SetValue(rel, related[0]));
        return Fin.Succ(rel);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class EdgeProjection {
    // One fold over the whole relationship roster: the generic FanOut/Pair families through row.Edge, the inverted Assign
    // arms (DefinesByType/AssignsToGroup), the realizing Connect, and the dedicated payload-bearing folds (DefinesProperties/
    // Structural/SpatialBoundaries/MaterialEdges). A dangling endpoint faults [DanglingReference]. The fold RETURNS its
    // fidelity contribution on the Noted writer rail — the group-factor riders as a pure scan, the material bag lowering
    // as its own returned log — so no arm holds a ledger and the ingest half lands one log at the SEMANTIC_PROJECTOR
    // fold edge.
    public static Fin<Noted<Seq<Relationship>>> All(IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, Option<EurocodePolicy> eurocode, TemplateScope templates, IIfcProfileStore profiles, Op key) {
        Seq<Fin<Seq<Relationship>>> plain = Decomposition(project, rooted, key)
            .Concat(Connections(project, rooted, profiles, key))
            .Concat(Assignments(project, rooted, key))
            .Concat(Generics(project, rooted, key))
            .Concat(DefinesProperties(project, rooted, key))
            .Concat(Structural(project, rooted, scale, eurocode, profiles, key))
            .Concat(SpatialBoundaries(project, rooted, profiles, key));
        return from rows in plain.TraverseM(identity).As()
               from materials in MaterialEdges(project, rooted, tolerance, scale, templates, profiles, key)
               select new Noted<Seq<Relationship>>(GroupFactors(project) + materials.Log,
                   rows.Flatten().ToSeq() + materials.Value);
    }

    // The group-factor riders read as a PURE SCAN over the same subtype the Assign arm already lands: the ByFactor
    // Factor has no seam slot on the typed Assign case, so the membership edge lands whole and the rider is a returned
    // fact on the NestOrdinal precedent, never silent. Reading them here keeps every edge arm a clean
    // Fin<Seq<Relationship>> and puts this page's WHOLE ledger contribution in one expression — the deleted form
    // threaded a mutating log through the membership fold to note a fact the fold never used.
    static FidelityLog GroupFactors(IfcProject project) =>
        project.Extract<IfcRelAssignsToGroupByFactor>().AsIterable()
            .Fold(FidelityLog.Empty, static (log, rel) => log.Note(FidelityDrop.GroupFactor, rel.RelatingGroup?.GlobalId ?? ""));

    static Seq<Fin<Seq<Relationship>>> Decomposition(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        FanOut(project.Extract<IfcRelAggregates>(), IfcRelKind.Aggregates, rooted, key,
            static r => r.RelatingObject?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        // AMENDMENT ordered-nest carrier: RelatedObjects is a schema LIST, so INGEST routes every nest through
        // Generic stamping the ONE egress-declared SemanticProjector.NestOrdinal attribute as the seam Integer arm.
        // Ordinals are PER-PARENT CONTINUOUS across relations — IsNestedBy is a schema SET,
        // so a parent with N IfcRelNests would collide per-relation zero-based indices and the egress per-parent
        // merge would interleave nondeterministically; the parent-grouped running index keeps the merged order total.
        // The typed Compose{Nest} case and its canonical bytes stay byte-identical for authored graphs, whose
        // (Compose,"nest") reverse-index row still re-authors them; egress groups these Generic edges by relating
        // endpoint and orders RelatedObjects by the same constant — one owner, no drift.
        project.Extract<IfcRelNests>().AsIterable()
            .GroupBy(static rel => rel.RelatingObject?.GlobalId ?? "")
            .AsIterable()
            .SelectMany(group => group.SelectMany(static rel => rel.RelatedObjects).Select((child, ordinal) =>
                from parent in Resolve(rooted, group.Key, key)
                from part in Resolve(rooted, child.GlobalId, key)
                select (Relationship)new Relationship.Generic(IfcRelKind.Nests.Key, parent, part,
                    Map((SemanticProjector.NestOrdinal, (PropertyValue)new PropertyValue.Integer(ordinal))))))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()),
        FanOut(project.Extract<IfcRelContainedInSpatialStructure>(), IfcRelKind.ContainedInStructure, rooted, key,
            static r => r.RelatingStructure?.GlobalId ?? "", static r => r.RelatedElements.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelReferencedInSpatialStructure>(), IfcRelKind.ReferencedInStructure, rooted, key,
            static r => r.RelatingStructure?.GlobalId ?? "", static r => r.RelatedElements.Select(static o => o.GlobalId)),
        Pair(project.Extract<IfcRelVoidsElement>(), IfcRelKind.Voids, rooted, key,
            static r => r.RelatingBuildingElement?.GlobalId ?? "", static r => r.RelatedOpeningElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelFillsElement>(), IfcRelKind.Fills, rooted, key,
            static r => r.RelatingOpeningElement?.GlobalId ?? "", static r => r.RelatedBuildingElement?.GlobalId ?? ""),
        // The feature-attachment tail of IfcRelDecomposes: the additive projection (the Voids counterpart) and
        // the IFC4.3 surface-feature adherence — both Generic rows, neither a new seam case.
        Pair(project.Extract<IfcRelProjectsElement>(), IfcRelKind.Projects, rooted, key,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedFeatureElement?.GlobalId ?? ""),
        FanOut(project.Extract<IfcRelAdheresToElement>(), IfcRelKind.Adheres, rooted, key,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedSurfaceFeatures.Select(static f => f.GlobalId)));

    static Seq<Fin<Seq<Relationship>>> Connections(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        // Extract<IfcRelConnectsElements> returns its subclasses too — the realizing and path-element joins are handled by
        // their own arms (the realizing Connect below, the ConnectsPathElements Generic edge), so they are excluded here.
        // The arm leaves the generic Pair path because the base carries the OPTIONAL ConnectionGeometry — the joint's
        // physical interface surface — which PreserveInterface content-keys onto the seam Connect.Interface slot.
        project.Extract<IfcRelConnectsElements>().AsIterable()
            .Where(static r => r is not (IfcRelConnectsWithRealizingElements or IfcRelConnectsPathElements))
            .Select(rel =>
                from a in Resolve(rooted, rel.RelatingElement?.GlobalId ?? "", key)
                from b in Resolve(rooted, rel.RelatedElement?.GlobalId ?? "", key)
                select (Relationship)new Relationship.Connect(a, b, ConnectKind.Element, Option<NodeId>.None,
                    PreserveInterface(rel.ConnectionGeometry, profiles, key)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()),
        Pair(project.Extract<IfcRelConnectsPorts>(), IfcRelKind.ConnectsPorts, rooted, key,
            static r => r.RelatingPort?.GlobalId ?? "", static r => r.RelatedPort?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelConnectsPortToElement>(), IfcRelKind.ConnectsPortToElement, rooted, key,
            static r => r.RelatingPort?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelConnectsPathElements>(), IfcRelKind.ConnectsPathElements, rooted, key,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelInterferesElements>(), IfcRelKind.InterferesElements, rooted, key,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelSequence>(), IfcRelKind.Sequence, rooted, key,
            static r => r.RelatingProcess?.GlobalId ?? "", static r => r.RelatedProcess?.GlobalId ?? ""),
        // GG's decompiled IfcRelFlowControlElements surface is RelatingPort/RelatedElement (NOT the schema
        // attribute pair) — the row and this arm read the REAL members, so the edge lands wire-true.
        Pair(project.Extract<IfcRelFlowControlElements>(), IfcRelKind.FlowControl, rooted, key,
            static r => r.RelatingPort?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        // The 2x3 element<->idealized-member binding; Extract<IfcRelConnectsStructuralMember> separately returns
        // IfcRelConnectsWithEccentricity, whose edge rides the Structural fold — its mandatory ConnectionConstraint
        // content-keys through the store's STEP-fragment lane onto its owner-stamped Eccentricity row (never inlined [M2]).
        Pair(project.Extract<IfcRelConnectsStructuralElement>(), IfcRelKind.ConnectsStructElement, rooted, key,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedStructuralMember?.GlobalId ?? ""),
        // IfcRelConnectsWithRealizingElements subtypes the medium-less IfcRelConnectsElements base directly, so it rides
        // the seam ConnectKind.Element with the realizing member in the Connect.Realizing option — realization is the FIELD,
        // never a sub-kind row (a ConnectKind.Realizing token is the deleted phantom the seam vocabulary rejects) [NEUTRAL_EDGE_RULING].
        // RealizingElements is a SET<IfcElement> [1:?]: the fold FANS OUT one Connect edge per realizing member over the
        // same (From, To) pair — a moment connection realized by a plate AND its bolts lands N edges, one per realizer —
        // so the whole realizing family survives (the .Head slice dropped every member past the first, the closed
        // cardinality defect); egress re-groups the fan by endpoint pair into ONE relation carrying every member. A
        // schema-invalid EMPTY set and an unrooted realizer both fault typed; neither can masquerade as a base connect.
        project.Extract<IfcRelConnectsWithRealizingElements>().AsIterable().SelectMany(rel =>
            rel.RealizingElements.AsIterable().ToSeq() switch {
                { IsEmpty: true } => Seq(Fin.Fail<Relationship>(new BimFault.ModelRejected(
                    key,
                    $"realizing-elements-empty:{rel.GlobalId}"))),
                var members => members.Map(member =>
                    from a in Resolve(rooted, rel.RelatingElement?.GlobalId ?? "", key)
                    from b in Resolve(rooted, rel.RelatedElement?.GlobalId ?? "", key)
                    from r in Resolve(rooted, member.GlobalId, key)
                    select (Relationship)new Relationship.Connect(a, b, ConnectKind.Element, Some(r),
                        PreserveInterface(rel.ConnectionGeometry, profiles, key))),
            })
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The connection-interface preservation: an IfcConnectionGeometry is the joint's physical interface surface (a point,
    // curve, or surface the inline prohibition keeps off the seam [M2]), so it PRESERVES as a STEP fragment through the
    // store's content-keyed fragment lane — the Eccentricity-row idiom — and the key alone crosses. The geometry is
    // OPTIONAL on both carriers, so an absent one is plain topology; a dropped present one left every re-exported joint
    // and every 2nd-level energy boundary geometrically unlocated.
    static Option<UInt128> PreserveInterface(IfcConnectionGeometry? geometry, IIfcProfileStore profiles, Op key) =>
        Optional(geometry).Map(surface => profiles.Preserve(surface, key));

    // The Assign family is INVERTED: the seam Assign is Subject(occurrence)->Definition(type/group), the inverse of the
    // IFC relating(type/group)->related(occurrences), so each related occurrence is the subject and the relating the
    // definition. DefinesByType binds the type bags for inheritance; AssignsToGroup the group/system/zone membership.
    static Seq<Fin<Seq<Relationship>>> Assignments(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelDefinesByType>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from occ in Resolve(rooted, o.GlobalId, key)
            from typ in Resolve(rooted, rel.RelatingType?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Assign(occ, typ, AssignKind.TypeDefinition)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()),
        // Extract<IfcRelAssignsToGroup> returns the ByFactor subtype too and its membership edge lands here unchanged;
        // the unslotted Factor rider is the GroupFactors scan's fact, so this arm stays a pure edge fold.
        project.Extract<IfcRelAssignsToGroup>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from member in Resolve(rooted, o.GlobalId, key)
            from grp in Resolve(rooted, rel.RelatingGroup?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Assign(member, grp, AssignKind.Group)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The Generic long-tail: covering, declaration, the four non-group assignments, and building service — each a neutral
    // Generic edge carrying the IFC wire-name and the relating->related directionality, so the round-trip drops nothing.
    static Seq<Fin<Seq<Relationship>>> Generics(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        FanOut(project.Extract<IfcRelCoversBldgElements>(), IfcRelKind.CoversElements, rooted, key,
            static r => r.RelatingBuildingElement?.GlobalId ?? "", static r => r.RelatedCoverings.Select(static c => c.GlobalId)),
        FanOut(project.Extract<IfcRelCoversSpaces>(), IfcRelKind.CoversSpaces, rooted, key,
            static r => (r.RelatingSpace as IfcRoot)?.GlobalId ?? "", static r => r.RelatedCoverings.Select(static c => c.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToControl>(), IfcRelKind.AssignsToControl, rooted, key,
            static r => (r.RelatingControl as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToProcess>(), IfcRelKind.AssignsToProcess, rooted, key,
            static r => (r.RelatingProcess as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToProduct>(), IfcRelKind.AssignsToProduct, rooted, key,
            static r => (r.RelatingProduct as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToActor>(), IfcRelKind.AssignsToActor, rooted, key,
            static r => (r.RelatingActor as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelDeclares>(), IfcRelKind.Declares, rooted, key,
            static r => r.RelatingContext?.GlobalId ?? "", static r => r.RelatedDefinitions.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelServicesBuildings>(), IfcRelKind.ServicesBuildings, rooted, key,
            static r => r.RelatingSystem?.GlobalId ?? "", static r => r.RelatedBuildings.Select(static o => o.GlobalId)),
        // IFC4.3 linear-referencing placement (alignment/grid -> positioned products) and the declaring-object
        // typing family. Extract<IfcRelAssignsToGroup> above separately returns IfcRelAssignsToGroupByFactor —
        // the membership edge lands, the Factor has no seam slot on the typed Assign case (a named drop on the
        // NestOrdinal precedent, never silent).
        FanOut(project.Extract<IfcRelPositions>(), IfcRelKind.Positions, rooted, key,
            static r => r.RelatingPositioningElement?.GlobalId ?? "", static r => r.RelatedProducts.Select(static p => p.GlobalId)),
        FanOut(project.Extract<IfcRelDefinesByObject>(), IfcRelKind.DefinesByObject, rooted, key,
            static r => r.RelatingObject?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)));

    // The property/quantity ATTACHMENT onto neutral Assign.PropertyDefinition edges the seam Bake reads: the SET-valued
    // RelatingPropertyDefinition fans out one edge per (related occurrence, definition) pair so the seam Bake folds the bag
    // into the element — Subject = the related occurrence, Definition = the bag node. Extract<IfcRelDefinesByProperties>
    // returns the 2x3 IfcRelOverridesProperties subtype too, so an override binding lands its attachment edge here.
    static Seq<Fin<Seq<Relationship>>> DefinesProperties(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelDefinesByProperties>().AsIterable()
            .SelectMany(rel => rel.RelatedObjects.SelectMany(o => rel.RelatingPropertyDefinition.Select(def =>
                from subject in Resolve(rooted, o.GlobalId, key)
                from definition in Resolve(rooted, def.GlobalId, key)
                select (Relationship)new Relationship.Assign(subject, definition, AssignKind.PropertyDefinition))))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The structural-analysis idealization onto neutral Generic edges [NEUTRAL_EDGE_RULING]: IfcRelConnectsStructuralMember binds an
    // idealized member to its connection, IfcRelConnectsStructuralActivity binds a load activity to a structural item — the
    // TWO restraint families (the rel-level member END RELEASE and the connection-level joint SUPPORT, each fixity +
    // SI spring) and the full IfcStructuralLoad family are lowered through the DEDICATED
    // Model/structural#STRUCTURAL_PROJECTION StructuralProjection.Attrs owner (never a local boolean-only/single-force-only
    // reader) — the Fin-railed Attrs(rel, scale, eurocode, profiles, key) builds the WHOLE edge payload in ONE call
    // (both restraint families + skew frame + SupportedLength + AtStart; load + Station; the eccentricity content key),
    // so a local two-step over the connection/activity entity is the deleted form and a malformed structural measure
    // faults typed on this rail. Both regimes arrive from the SEMANTIC_PROJECTOR fold head — `scale` the one
    // per-projection UnitScale, `eurocode` its ctor-held composition policy — so neither is re-derived off rel.Database.
    // `profiles` is the SAME fragment lane this page's PreserveInterface takes, threaded down so the eccentric subtype's
    // MANDATORY ConnectionConstraint (an IfcConnectionGeometry the inline prohibition keeps off the attrs [M2])
    // content-keys inside its OWN owner and rides the owner-stamped Eccentricity row: a second eccentricity read here
    // would fork one payload across two writers. So a Rasm.Compute frame solve reads graph.SupportsOf/graph.LoadsOf off these
    // edges and resolves the analytical axis BY CONTENT KEY from Representations (content-keyed by IfcRepresentation.Keys,
    // never an Enrich bake) — never re-reading IFC, never a defaulted support joint or load case.
    static Seq<Fin<Seq<Relationship>>> Structural(IfcProject project, Map<string, NodeId> rooted, UnitScale scale, Option<EurocodePolicy> eurocode, IIfcProfileStore profiles, Op key) => Seq(
        project.Extract<IfcRelConnectsStructuralMember>().AsIterable().Select(rel =>
            from m in Resolve(rooted, rel.RelatingStructuralMember?.GlobalId ?? "", key)
            from c in Resolve(rooted, rel.RelatedStructuralConnection?.GlobalId ?? "", key)
            from attrs in StructuralProjection.Attrs(rel, scale, eurocode, profiles, key)
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructMember.Key, m, c, attrs))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()),
        project.Extract<IfcRelConnectsStructuralActivity>().AsIterable().Select(rel =>
            from item in Resolve(rooted, (rel.RelatingElement as IfcRoot)?.GlobalId ?? "", key)
            from act in Resolve(rooted, rel.RelatedStructuralActivity?.GlobalId ?? "", key)
            from attrs in StructuralProjection.Attrs(rel, scale, eurocode, profiles, key)
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructActivity.Key, item, act, attrs))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The energy/spatial space-boundary graph onto neutral Generic edges [NEUTRAL_EDGE_RULING]: IfcRelSpaceBoundary binds a space to its
    // bounding building element, the 1st/2nd-level discriminant riding the attrs (the physical/virtual + internal/external
    // enums are GeometryGym internal fields, absent from the public surface, so they are not lowered) — so a Rasm.Compute
    // OSM build reads graph.SpacesOf/graph.BoundingSurfacesOf off the baked graph. RelatingSpace is an
    // IfcSpaceBoundarySelect, so its GlobalId reads through the IfcRoot cast.
    static Seq<Fin<Seq<Relationship>>> SpatialBoundaries(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        project.Extract<IfcRelSpaceBoundary>().AsIterable().Select(rel =>
            from s in Resolve(rooted, (rel.RelatingSpace as IfcRoot)?.GlobalId ?? "", key)
            from e in Resolve(rooted, rel.RelatedBuildingElement?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, s, e, BoundaryAttrs(rel, profiles, key)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The material Associate edges [OCCURRENCE_USAGE_RULING]: each related element binds the projected Material node (the content-keyed id the
    // Materials fold lands) through an Associate edge carrying the occurrence MaterialUsage (the LayerSet direction/sense/
    // offset or the ProfileSet cardinal-point/extent), so a wall and its mirror share one LayerSet node with two usages;
    // the ProfileSet usage admits through the seam MaterialUsage.ProfileSet.Of cardinal-point gate. The material node and
    // usage bind ONCE PER RELATION and the related objects fan over the pair — the per-(rel, object) re-projection ran the
    // whole composition fold N times per relation, the deleted quadratic form the MaterialIndex law already names.
    // RelatingMaterial is schema-mandatory, so a null read is a malformed file faulting typed HERE (the node-side
    // Materials fold Optional-skips the same null — this edge rail is the one fault site, per the fault-site law).
    // Each relation's definition (SemanticProjector.DefinitionOf, the SAME unwrap the node-side fold walks) additionally
    // binds its imported HasProperties bags: the bag node id re-derives through SemanticProjector.PropertySetNode — the
    // deterministic content mint over the identical PropertyBag — so the Assign.PropertyDefinition edge keys the exact
    // node the Materials fold landed, no shared table between the two folds.
    static Fin<Noted<Seq<Relationship>>> MaterialEdges(
        IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, TemplateScope templates,
        IIfcProfileStore profiles, Op key) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable().Select(rel =>
            from relating in Optional(rel.RelatingMaterial).ToFin(new BimFault.ModelRejected(key, $"material-relation-unbound:{rel.GlobalId}"))
            from material in MaterialNode(relating, tolerance, profiles, scale, key)
            from usage in UsageOf(relating, scale, key)
            // ImportedPsets lowers foreign values, so it returns its own narrowing facts; the Noted log threads out
            // through this fold rather than into a cell the arm would have to hold.
            from bags in SemanticProjector.DefinitionOf(relating)
                .Map(definition => MaterialProjection.ImportedPsets(definition, rooted, scale, templates, key))
                .IfNone(Fin.Succ(Noted.Clean(Seq<PropertyBag>())))
            // RelatedObjects is a SET<IfcDefinitionSelect> — a SELECT interface, not an IfcRoot — so the endpoint reads
            // through the SAME IfcRoot cast every other select-sided arm takes and lands on the SAME Resolve gate, so
            // an unrooted or unprojected subject faults DanglingReference here like every other edge family instead of
            // spelling a GlobalId the select surface does not publish.
            from elements in rel.RelatedObjects.AsIterable().ToSeq().TraverseM(o => Resolve(rooted, (o as IfcRoot)?.GlobalId ?? "", key)).As()
            select bags.Map(rows => elements.Map(element => (Relationship)new Relationship.Associate(element, material, usage))
                .Concat(rows.Map(bag => (Relationship)new Relationship.Assign(
                    material, SemanticProjector.PropertySetNode(bag, tolerance).Id, AssignKind.PropertyDefinition))).ToSeq()))
            .AsIterable().TraverseM(identity).As()
            .Map(static rows => Noted.Join(rows.ToSeq()).Map(static edges => edges.Flatten().ToSeq()));

    static Fin<NodeId> MaterialNode(IfcMaterialSelect select, double tolerance, IIfcProfileStore profiles, UnitScale scale, Op key) =>
        MaterialProjection.Project(select, tolerance, profiles, scale, key).Map(static m => m.Id);

    // The IFC occurrence material usage -> the seam's typed MaterialUsage [OCCURRENCE_USAGE_RULING]: an IfcMaterialLayerSetUsage lowers to
    // MaterialUsage.LayerSet (direction/sense/offset/extent — all four IFC occurrence parameters), an
    // IfcMaterialProfileSetUsage to MaterialUsage.ProfileSet through the seam Of cardinal-point gate, a type-level set
    // with no occurrence usage to MaterialUsage.None. The ReferenceExtent (the layer-set size perpendicular to the layers,
    // IfcMaterialLayerSetUsage.ReferenceExtent decompile-confirmed .api/api-geometrygym-ifc row 12) is the 4th seam ctor
    // arg — without it an asymmetric wall finish (a reference line that does not bisect the buildup) is dropped at ingest.
    static Fin<MaterialUsage> UsageOf(IfcMaterialSelect select, UnitScale scale, Op key) => select switch {
        IfcMaterialLayerSetUsage u =>
            from offset in Length(u.OffsetFromReferenceLine, scale)
            from extent in Length(u.ReferenceExtent, scale)
            from usage in MaterialUsage.LayerSet.Of(
                u.LayerSetDirection switch {
                    IfcLayerSetDirectionEnum.AXIS1 => LayerSetDirection.Axis1,
                    IfcLayerSetDirectionEnum.AXIS2 => LayerSetDirection.Axis2,
                    _                              => LayerSetDirection.Axis3,
                },
                u.DirectionSense == IfcDirectionSenseEnum.POSITIVE ? DirectionSense.Positive : DirectionSense.Negative,
                offset, extent, key)
            select usage,
        IfcMaterialProfileSetUsage u =>
            from extent in Length(u.ReferenceExtent, scale)
            from usage in MaterialUsage.ProfileSet.Of(OptionalCardinal(u.CardinalPoint), extent, key)
            select usage,
        _ => Fin.Succ<MaterialUsage>(new MaterialUsage.None()),
    };

    static Fin<Option<MeasureValue>> Length(double native, UnitScale scale) =>
        double.IsFinite(native)
            ? MeasureValue.OfSi(Dimension.LengthDim, scale.Coerce(native, MeasureRow.Length, null)).Map(Some)
            : Fin.Succ(Option<MeasureValue>.None);

    static Option<int> OptionalCardinal(IfcCardinalPointReference point) =>
        point == IfcCardinalPointReference.MID ? Option<int>.None : Some((int)point);

    // The space-boundary level discriminant -> the egress-declared SemanticProjector.BoundaryLevel attr, THREE-valued
    // because the runtime type is: "2nd"/"1st" name the exact subtype the egress refined-construct re-authors, "" the
    // base-class instance (2ndLevel derives from 1stLevel, so the 2nd probe runs first — a level is never upgraded onto
    // a base instance). The GeometryGym IfcRelSpaceBoundary exposes RelatingSpace/RelatedBuildingElement/
    // ConnectionGeometry publicly but keeps PhysicalOrVirtualBoundary/InternalOrExternalBoundary on internal fields
    // with no public getter, so the level is the lone publicly-readable flag; the physical/virtual + internal/external
    // classification is not on the host surface and is never fabricated (an internal-field reflection read is the
    // fragile form this owner refuses). The boundary's own ConnectionGeometry — the surface a 2nd-level energy model
    // runs on — rides the SemanticProjector.InterfaceKey attr beside the level, the Eccentricity-row idiom: the boundary
    // keeps its Generic edge because the seam ConnectKind medium vocabulary is closed at element/path/port and a
    // space↔bounding-surface boundary is none of the three, so a fourth medium row would be the phantom this attr avoids.
    // The level attr is built ONCE and the optional interface key FOLDS onto it, so the two arms of the geometry
    // Option cannot disagree about what a boundary's level entry is — the duplicated construction was one edit away
    // from two spellings of the same fact.
    static Map<PropertyName, PropertyValue> BoundaryAttrs(IfcRelSpaceBoundary rel, IIfcProfileStore profiles, Op key) =>
        PreserveInterface(rel.ConnectionGeometry, profiles, key).Fold(
            Map((SemanticProjector.BoundaryLevel, (PropertyValue)new PropertyValue.Text(BoundaryLevelOf(rel)))),
            static (attrs, surface) => attrs.Add(SemanticProjector.InterfaceKey, new PropertyValue.Text(surface.ToString("X32"))));

    static string BoundaryLevelOf(IfcRelSpaceBoundary rel) =>
        rel is IfcRelSpaceBoundary2ndLevel ? "2nd" : rel is IfcRelSpaceBoundary1stLevel ? "1st" : "";

    // The generic relating->related fan-out (a one-to-many family), constructing one neutral edge per related endpoint
    // through the row; a many-related family fans out one edge per endpoint, the directionality the row records.
    static Fin<Seq<Relationship>> FanOut<TRel>(IEnumerable<TRel> rels, IfcRelKind kind, Map<string, NodeId> rooted, Op key,
        Func<TRel, string> relating, Func<TRel, IEnumerable<string>> related) =>
        rels.SelectMany(rel => related(rel).Select(id =>
                from r in Resolve(rooted, relating(rel), key)
                from e in Resolve(rooted, id, key)
                select kind.Edge(r, e)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq());

    static Fin<Seq<Relationship>> Pair<TRel>(IEnumerable<TRel> rels, IfcRelKind kind, Map<string, NodeId> rooted, Op key,
        Func<TRel, string> relating, Func<TRel, string> related) =>
        rels.Select(rel =>
                from r in Resolve(rooted, relating(rel), key)
                from e in Resolve(rooted, related(rel), key)
                select kind.Edge(r, e))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq());

    static Fin<NodeId> Resolve(Map<string, NodeId> rooted, string globalId, Op key) =>
        rooted.Find(globalId).ToFin(new BimFault.DanglingReference(key, $"edge-endpoint-miss:{globalId}"));
}
```

## [03]-[RESEARCH]

(none)
