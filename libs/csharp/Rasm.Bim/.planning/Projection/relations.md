# [BIM_RELATION_ALGEBRA]

The IFC relationship vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner: the `IfcRelKind` `[SmartEnum<string>]` row-driven `IfcRel*` roster (the neutral `EdgeAxis`+`SubKind` each name lowers onto, plus the relating/related IFC inverse-attribute names directionality round-trips on), and the `EdgeProjection` fold lowering every relationship family onto a NEUTRAL `Rasm.Element/Relations/relation#EDGE_ALGEBRA` `Relationship` edge. This page is the relationship half of the `Projection/semantic#SEMANTIC_PROJECTOR` ingress projector — the `Project` fold composes `EdgeProjection.All` to land every `IfcRel*` neutral edge — and the SAME roster reverses at the `Projection/egress#IFC_EGRESS` `ReauthorRelationships` re-author through the `IfcRelKind.ForNeutral` reverse index and `IfcRelKind.Author`. The whole IFC relationship vocabulary — every `IfcRel*` name, its directionality, and its inverse — plus the eight families the retired owner stranded ride the neutral edge algebra [NEUTRAL_EDGE_RULING], so no relationship is dropped and the seam stays IFC-schema-free. GeometryGym leaks no relationship case below the seam: the typed case carries only its `SubKind`, the IFC wire-name + inverse living on the `IfcRelKind` ROW, and only `Generic` carries the wire-name and the per-edge attribute payload.

The roster is ONE owner widened on one axis — a new IFC relationship is one `IfcRelKind` row carrying its axis, sub-kind, inverse-attribute names, and the `Inverted` flag, plus, when it carries a typed payload, one dedicated `EdgeProjection` arm — never a parallel `RelationshipKind` on the seam and never a per-relationship projector type. The roster covers the FULL concrete node-to-node `IfcRelationship` subtree of the live GeometryGym surface — decompile-censused, so an unrostered node-to-node family cannot exist silently — and the payload-bearing SUBTYPES (`IfcRelAssignsToGroupByFactor`, `IfcRelConnectsWithEccentricity`, `IfcRelOverridesProperties`, `IfcRelSpaceBoundary1stLevel`/`2ndLevel`) ride their base-class `Extract<T>` arms by construction. The structural member↔connection/member↔activity idealization, the space↔surface boundary graph, the property/quantity attachment, and the material occurrence-usage ride DEDICATED `EdgeProjection` folds onto neutral `Generic`/`Assign.PropertyDefinition`/`Associate` edges, not the generic row path, so a `Rasm.Compute` runner reads the structural/energy connectivity off the seam graph by wire-name and the `Projection/egress#IFC_EGRESS` `Emit` re-authors the covered families exactly as they were read. Ordered nesting is the ONE ingest exception: `IfcRelNests.RelatedObjects` is a schema `LIST`, so the `Decomposition` fold routes every nest through `Relationship.Generic` carrying the per-parent-continuous zero-based `SemanticProjector.NestOrdinal` attribute — the typed `Compose{Nest}` case and its canonical bytes stay byte-identical for authored graphs, and egress re-orders the fan-in from the attribute before `Author`.

## [01]-[INDEX]

- [01]-[RELATION_ALGEBRA]: `IfcRelKind` the full `IfcRel*` roster `[SmartEnum<string>]` (the neutral `EdgeAxis`+`SubKind` it lowers onto, plus the relating/related IFC inverse-attribute names directionality round-trips on — decompile-censused over the complete concrete node-to-node `IfcRelationship` subtree), the `EdgeAxis` five neutral lowering targets, and the `EdgeProjection` fold lowering every relationship onto a neutral `Relationship` edge — the FanOut/Pair generic families, the ordinal-carrying ordered-nest `Generic` arm, the inverted `Assign` arms (`DefinesByType`/`AssignsToGroup`), the realizing `Connect`, the `DefinesProperties` property/quantity attachment, the `Structural` member↔connection/member↔activity `Generic` edges (the `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection.Attrs` 6-DOF restraint + full load family + `LoadKind`/`Case` payload and the `StructuralProjection.AtStart` start/end discriminant, the eccentric subtype's mandatory `ConnectionConstraint` content-keyed through the store; the analytical axis content-keyed in `Representations` by `Model/elements#REPRESENTATION_KEYS` `IfcRepresentation.Keys`, never baked onto the node), the `SpatialBoundaries` space↔surface `Generic` edges, and the `MaterialEdges` `Associate` material edge carrying the occurrence-usage payload [OCCURRENCE_USAGE_RULING].

## [02]-[RELATION_ALGEBRA]

- Owner: `IfcRelKind` the `[SmartEnum<string>]` carrying the WHOLE row-driven `IfcRel*` roster keyed on the relationship entity name — each row carrying the neutral `EdgeAxis` it folds onto (`Compose`/`Assign`/`Connect`/`Void`/`Generic`), the neutral `SubKind` token the typed case takes (the `ComposeKind`/`AssignKind`/`ConnectKind`/`VoidKind` key, empty for `Generic`), and the relating-side/related-side IFC inverse-attribute names directionality round-trips on [NEUTRAL_EDGE_RULING]; `EdgeAxis` the five neutral lowering targets; `EdgeProjection` the static fold lowering every relationship family onto neutral `Relationship` edges. The neutral cases carry a typed `SubKind`, never a wire-name or an attribute bag — the IFC wire-name + inverse live on the `IfcRelKind` ROW and reconstruct at egress through the reverse index, and only `Generic` carries the wire-name and the per-edge attribute payload.
- Cases: the row-driven roster — `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` (`Compose`), `IfcRelConnectsElements`/`IfcRelConnectsWithRealizingElements`/`IfcRelConnectsPorts` (`Connect`), `IfcRelDefinesByType`/`IfcRelAssignsToGroup` (`Assign`), `IfcRelVoidsElement`/`IfcRelFillsElement` (`Void`) — PLUS the families that ride `Generic` because no neutral sub-kind fits: `IfcRelConnectsPortToElement` (the port↔element shape distinct from port↔port), `IfcRelConnectsPathElements` (wall-join priorities), `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity`, `IfcRelConnectsStructuralElement` (the 2x3 element↔idealized-member binding), `IfcRelSpaceBoundary`, `IfcRelInterferesElements`, `IfcRelSequence`, `IfcRelCoversBldgElements`/`IfcRelCoversSpaces`, `IfcRelAssignsToControl`/`Process`/`Product`/`Actor`, `IfcRelDeclares`, `IfcRelServicesBuildings`, `IfcRelProjectsElement` (the additive-feature counterpart of `Voids`), `IfcRelAdheresToElement` (IFC4.3 surface features), `IfcRelFlowControlElements` (the GG port→controlled-element surface), `IfcRelPositions` (IFC4.3 positioning-element→products), `IfcRelDefinesByObject` (declaring-object typing) — each lands its neutral edge carrying the IFC wire-name and the directionality/inverse on the `Generic` payload, so no relationship among the covered node-to-node families is dropped and none re-opens the IFC schema strata leak on the seam. `IfcRelAssociatesMaterial` (the `Associate` material edge with its `MaterialUsage` payload), `IfcRelDefinesByProperties` (the `Assign.PropertyDefinition` bag attachment — its `IfcRelOverridesProperties` subtype rides the same `Extract<T>`), and the structural/space-boundary payloads ride DEDICATED folds, not the generic row path.
- Entry: `EdgeProjection.All(IfcProject project, Map<string,NodeId> rooted, double tolerance, UnitScale scale, IIfcProfileStore profiles, FidelityLog log, Op key)` folds every relationship family into a `Seq<Relationship>` — `scale` the per-projection native→SI coercion every measured payload (structural spring/force, material usage offset/extent) multiplies through — the `FanOut`/`Pair` generic helpers read the relating/related `GlobalId`s, resolve them through the `rooted` map, look up the `IfcRelKind` row, and construct the neutral edge through `row.Edge`; the ordered `IfcRelNests` alone bypasses `row.Edge` and lands one `Relationship.Generic(IfcRelKind.Nests.Key, …)` per child carrying the zero-based `SemanticProjector.NestOrdinal` attribute (a `PropertyValue.Integer` ordinal) so `RelatedObjects` `LIST` order round-trips; the `Assign` arms (`DefinesByType`/`AssignsToGroup`) are INVERTED (the seam `Assign` is `Subject`(occurrence)→`Definition`(type/group), the inverse of the IFC relating→related), so they read each related occurrence as the subject and the relating type/group as the definition; the realizing `Connect` FANS OUT one edge per `RealizingElements` member over the same `(From, To)` pair — the seam `Connect.Realizing` option carries one realizer per edge, so a multi-realizer joint (plate + bolts) survives whole and egress re-groups the fan into ONE `IfcRelConnectsWithRealizingElements` carrying every member (the `.Head` slice that dropped every realizer past the first is the deleted form); `DefinesProperties` lands the `IfcRelDefinesByProperties` (whose `RelatingPropertyDefinition` is a SET) property/quantity attachment as `Assign(occurrence, definition, PropertyDefinition)` per (occurrence, definition) pair; `Structural` lands `IfcRelConnectsStructuralMember` (member→connection, the 6-DOF restraint off `IfcStructuralConnection.AppliedCondition`→`IfcBoundaryNodeCondition` riding the payload) and `IfcRelConnectsStructuralActivity` (item→load, the applied force/moment off `IfcStructuralActivity.AppliedLoad`→`IfcStructuralLoadSingleForce` riding the payload) as `Generic` edges; `SpatialBoundaries` lands `IfcRelSpaceBoundary` (space→bounding-surface, the THREE-valued `BoundaryLevel` discriminant riding the payload — `"2nd"`/`"1st"` for the exact subtype the egress refined-construct re-authors, `""` for a base instance) as `Generic` edges; `MaterialEdges` lands the `Associate` material edges with the occurrence-usage payload [OCCURRENCE_USAGE_RULING]; `Fin<T>` aborts on a dangling endpoint (`BimFault.DanglingReference`).
- Auto: `row.Edge(relating, related)` dispatches the `EdgeAxis` onto the seam case constructed with the row's neutral `SubKind` (a `Compose` of `ComposeKind.Get(SubKind)`, an `Assign` of `AssignKind.Get(SubKind)`, a `Connect` of `ConnectKind.Get(SubKind)` with no realizing node, a `Void` of `VoidKind.Get(SubKind)`) or a `Generic(Key, …)` carrying the IFC wire-name; the structural/space-boundary `Generic` edges carry their typed payload (the 6-DOF restraint booleans, the SI force/moment measures, the boundary level) as `PropertyValue` attribute entries the `Rasm.Compute` runners read by wire-name; the material `Associate` edge threads the `Semantics/composition#MATERIAL_COMPOSITION` occurrence-usage payload [OCCURRENCE_USAGE_RULING] (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`, `CardinalPoint`/`ReferenceExtent`) as the seam's typed `MaterialUsage` (`LayerSet`/`ProfileSet`), never a `PropertyValue` attribute bag, the `ProfileSet` usage admitted through the seam `MaterialUsage.ProfileSet.Of` cardinal-point gate.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new IFC relationship is one `IfcRelKind` row (its axis, sub-kind, inverse-attribute names, and the `Inverted` flag that re-orients the egress for an inverted-direction family) plus, when it carries a payload, one dedicated `EdgeProjection` arm; a new directionality is the row's relating/related columns; the neutral five-axis algebra absorbs the case and `Generic` absorbs the residue — never a parallel `RelationshipKind` on the seam and never a dropped family among the covered node-to-node relationships. A relationship to a NON-node IFC resource (an `IfcRelAssociatesDocument`/`Library`/`Constraint`/`Approval`/`Dataset`/`ProfileDef`/`ProfileProperties` definition association, an `IfcRelDefinesByTemplate` template binding, an `IfcRelAssignsToResource`) is a SEPARATE Growth axis — a new seam `Node` case plus its row — never silently in scope of the covered set. A payload a rider subtype adds beyond its base edge is either SOLVED or a NAMED bounded drop, never silent: the eccentricity and the space-boundary level are solved riders — the `IfcRelConnectsWithEccentricity` `ConnectionConstraint` content-keys through the store's STEP-fragment lane onto the `EccentricityKey` attr and egress reconstitutes the exact subtype, and the three-valued `BoundaryLevel` attr drives the egress refined-construct — while `IfcRelAssignsToGroupByFactor.Factor` (no seam slot on the typed `Assign` case) and the `IfcRelOverridesProperties` override semantics (no `AssignKind` override row; re-emits as plain `IfcRelDefinesByProperties`) remain next-campaign payload/rows on the `NestOrdinal` precedent.
- Boundary: the seam `Relationship` is the NEUTRAL edge algebra plus `Generic` — the seam carries no typed `IfcRel*` case and re-introducing typed IFC cases is the deleted form [NEUTRAL_EDGE_RULING]; the IFC names/directionality/inverse and the long-tail families live HERE on the Bim side, the neutral case carrying only its `SubKind` and the IFC wire-name reconstructing at egress through the reverse index, the `Generic` passthrough carrying any residue of the covered node-to-node families so a dropped family among them is the named defect (a relationship to a non-node IFC resource is a Growth axis — a new seam `Node` case — never a covered residue); directionality is preserved by the row's relating/related orientation and, for the inverted `Assign` family, the row's `Inverted` flag the egress re-orients on (`ReauthorRelationships` swaps the seam `Subject`/`Definition` back to the IFC relating/related before `Author`), never inferred at the call site; the material occurrence-usage rides the `Associate` edge's typed `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and a parallel usage node is the deleted form; the structural/space-boundary connectivity rides the NEUTRAL `Generic` wire-name payload, never a typed IFC relationship case, so the strata stay IFC-schema-free; the nest order contract is the egress-declared `SemanticProjector.NestOrdinal` attribute on the `Generic("IfcRelNests", …)` edges — egress groups those edges by relating endpoint and authors ONE ordered `IfcRelNests` from the ordinal sort, while an authored `Compose{Nest}` edge keeps its frozen `(Compose, "nest")` reverse-index row, so the ingest routing and the typed case never collide.

```csharp signature
using System.Collections.Frozen;
using System.Reflection;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim;
using Rasm.Bim.Semantics;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;

namespace Rasm.Bim.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// The five neutral lowering targets — which seam Relationship case a row folds onto. Distinct from the seam's
// consumer-facing RelationshipKind: this is the Bim-side projector dispatch over which the Edge switch and the egress
// reverse index read.
public enum EdgeAxis : byte { Compose = 0, Assign = 1, Connect = 2, Void = 3, Generic = 4 }

// --- [MODELS] -----------------------------------------------------------------------------
// The row-driven IfcRel* roster: name -> neutral axis + the neutral SubKind token the typed case takes + the
// relating/related IFC inverse-attribute names directionality round-trips on. The long-tail families join here on the
// Generic axis so EVERY relationship has a home; the neutral edge keeps the wire-name + inverse so IFC directionality
// round-trips without a seam IFC case [NEUTRAL_EDGE_RULING]. Material/property/classification/structural/space-boundary ride dedicated
// EdgeProjection arms, not row.Edge.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcRelKind {
    public static readonly IfcRelKind Aggregates             = new("IfcRelAggregates",                  EdgeAxis.Compose, "aggregate", "RelatingObject",          "RelatedObjects");
    public static readonly IfcRelKind Nests                  = new("IfcRelNests",                       EdgeAxis.Compose, "nest",      "RelatingObject",          "RelatedObjects");
    public static readonly IfcRelKind ContainedInStructure   = new("IfcRelContainedInSpatialStructure", EdgeAxis.Compose, "contain",   "RelatingStructure",       "RelatedElements");
    public static readonly IfcRelKind ReferencedInStructure  = new("IfcRelReferencedInSpatialStructure",EdgeAxis.Compose, "reference", "RelatingStructure",       "RelatedElements");
    public static readonly IfcRelKind ConnectsElements       = new("IfcRelConnectsElements",            EdgeAxis.Connect, "element",   "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind ConnectsRealizing      = new("IfcRelConnectsWithRealizingElements",EdgeAxis.Connect,"element",   "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind ConnectsPorts          = new("IfcRelConnectsPorts",               EdgeAxis.Connect, "port",      "RelatingPort",            "RelatedPort");
    public static readonly IfcRelKind ConnectsPortToElement  = new("IfcRelConnectsPortToElement",       EdgeAxis.Generic, "",          "RelatingPort",            "RelatedElement");
    public static readonly IfcRelKind ConnectsPathElements   = new("IfcRelConnectsPathElements",        EdgeAxis.Generic, "",          "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind ConnectsStructMember   = new("IfcRelConnectsStructuralMember",    EdgeAxis.Generic, "",          "RelatingStructuralMember","RelatedStructuralConnection");
    public static readonly IfcRelKind ConnectsStructActivity = new("IfcRelConnectsStructuralActivity",  EdgeAxis.Generic, "",          "RelatingElement",         "RelatedStructuralActivity");
    public static readonly IfcRelKind ConnectsStructElement  = new("IfcRelConnectsStructuralElement",   EdgeAxis.Generic, "",          "RelatingElement",         "RelatedStructuralMember");
    public static readonly IfcRelKind SpaceBoundary          = new("IfcRelSpaceBoundary",               EdgeAxis.Generic, "",          "RelatingSpace",           "RelatedBuildingElement");
    public static readonly IfcRelKind Projects               = new("IfcRelProjectsElement",             EdgeAxis.Generic, "",          "RelatingElement",         "RelatedFeatureElement");
    public static readonly IfcRelKind Adheres                = new("IfcRelAdheresToElement",            EdgeAxis.Generic, "",          "RelatingElement",         "RelatedSurfaceFeatures");
    // GG deviates from the schema attribute pair here (no RelatingFlowElement/RelatedControlElements on the
    // public surface) — the decompiled members ARE RelatingPort/RelatedElement, so the row records the REAL wire.
    public static readonly IfcRelKind FlowControl            = new("IfcRelFlowControlElements",         EdgeAxis.Generic, "",          "RelatingPort",            "RelatedElement");
    public static readonly IfcRelKind Positions              = new("IfcRelPositions",                   EdgeAxis.Generic, "",          "RelatingPositioningElement","RelatedProducts");
    public static readonly IfcRelKind DefinesByObject        = new("IfcRelDefinesByObject",             EdgeAxis.Generic, "",          "RelatingObject",          "RelatedObjects");
    public static readonly IfcRelKind InterferesElements     = new("IfcRelInterferesElements",          EdgeAxis.Generic, "",          "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind Sequence               = new("IfcRelSequence",                    EdgeAxis.Generic, "",          "RelatingProcess",         "RelatedProcess");
    public static readonly IfcRelKind CoversElements         = new("IfcRelCoversBldgElements",          EdgeAxis.Generic, "",          "RelatingBuildingElement", "RelatedCoverings");
    public static readonly IfcRelKind CoversSpaces           = new("IfcRelCoversSpaces",                EdgeAxis.Generic, "",          "RelatingSpace",           "RelatedCoverings");
    public static readonly IfcRelKind AssignsToControl       = new("IfcRelAssignsToControl",            EdgeAxis.Generic, "",          "RelatingControl",         "RelatedObjects");
    public static readonly IfcRelKind AssignsToProcess       = new("IfcRelAssignsToProcess",            EdgeAxis.Generic, "",          "RelatingProcess",         "RelatedObjects");
    public static readonly IfcRelKind AssignsToProduct       = new("IfcRelAssignsToProduct",            EdgeAxis.Generic, "",          "RelatingProduct",         "RelatedObjects");
    public static readonly IfcRelKind AssignsToActor         = new("IfcRelAssignsToActor",              EdgeAxis.Generic, "",          "RelatingActor",           "RelatedObjects");
    public static readonly IfcRelKind Declares               = new("IfcRelDeclares",                    EdgeAxis.Generic, "",          "RelatingContext",         "RelatedDefinitions");
    public static readonly IfcRelKind ServicesBuildings      = new("IfcRelServicesBuildings",           EdgeAxis.Generic, "",          "RelatingSystem",          "RelatedBuildings");
    public static readonly IfcRelKind DefinesByType          = new("IfcRelDefinesByType",               EdgeAxis.Assign,  "type-definition", "RelatingType",      "RelatedObjects", inverted: true);
    public static readonly IfcRelKind AssignsToGroup         = new("IfcRelAssignsToGroup",              EdgeAxis.Assign,  "group",     "RelatingGroup",           "RelatedObjects", inverted: true);
    public static readonly IfcRelKind Voids                  = new("IfcRelVoidsElement",                EdgeAxis.Void,    "void",      "RelatingBuildingElement", "RelatedOpeningElement");
    public static readonly IfcRelKind Fills                  = new("IfcRelFillsElement",                EdgeAxis.Void,    "fill",      "RelatingOpeningElement",  "RelatedBuildingElement");

    public EdgeAxis Axis { get; }
    public string SubKind { get; }
    public string Relating { get; }
    public string Related { get; }
    // The ingest builds the seam Assign(Subject=occurrence, Definition=type/group) — the INVERSE of the IFC
    // relating(type/group)->related(occurrence) — so the egress re-inverts to the IFC orientation the row's
    // Relating/Related names expect; true ONLY for the DefinesByType/AssignsToGroup Assign rows, every other row
    // already reads in IFC orientation. Directionality round-trips off this flag, never an inference at the call site.
    public bool Inverted { get; }

    // The key-chaining ctor the [SmartEnum<string>] generator's this(key) overload completes (the corpus
    // SmartEnum-with-fields shape): each static row supplies the neutral axis + sub-kind, the IFC relating/related
    // inverse-attribute names directionality round-trips on, and the Inverted flag for the two inverted Assign rows.
    private IfcRelKind(string key, EdgeAxis axis, string subKind, string relating, string related, bool inverted = false) : this(key) =>
        (Axis, SubKind, Relating, Related, Inverted) = (axis, subKind, relating, related, inverted);

    // The egress reverse index: a neutral typed edge (axis + sub-kind) resolves its IFC row so Emit re-authors the right
    // IfcRel* — the exact inverse of Edge. The Generic axis is excluded (Generic resolves by wire-name through TryGet), as
    // is ConnectsRealizing: it shares the Connect "element" sub-kind with ConnectsElements because realization is the seam
    // Connect.Realizing FIELD, never a sub-kind row, so egress disambiguates the two by the Realizing None/Some field, not
    // this index. The direct ToFrozenDictionary IS the uniqueness gate — a colliding (axis, sub-kind) pair fails at type
    // initialization (the FaultBand registry law), never a GroupBy/First mask silently electing a winner.
    static readonly Lazy<FrozenDictionary<(EdgeAxis, string), IfcRelKind>> ByNeutral = new(static () =>
        Items.Where(static r => r.Axis != EdgeAxis.Generic && r != ConnectsRealizing)
             .ToFrozenDictionary(static r => (r.Axis, r.SubKind)));

    public static Option<IfcRelKind> ForNeutral(EdgeAxis axis, string subKind) =>
        ByNeutral.Value.TryGetValue((axis, subKind), out IfcRelKind? row) && row is { } resolved ? Some(resolved) : None;

    // The neutral edge constructor: the axis selects the seam Relationship case constructed with the row's neutral
    // SubKind; the Generic axis carries the IFC wire-name + an empty attribute bag (a payload-bearing family rides a
    // dedicated EdgeProjection arm, never this generic path). The realizing Connect node is filled by the dedicated arm.
    public Relationship Edge(NodeId relating, NodeId related) => Axis switch {
        EdgeAxis.Compose => new Relationship.Compose(relating, related, ComposeKind.Get(SubKind)),
        EdgeAxis.Assign  => new Relationship.Assign(relating, related, AssignKind.Get(SubKind)),
        EdgeAxis.Connect => new Relationship.Connect(relating, related, ConnectKind.Get(SubKind), Option<NodeId>.None),
        EdgeAxis.Void    => new Relationship.Void(relating, related, VoidKind.Get(SubKind)),
        _                => new Relationship.Generic(Key, relating, related, Map<PropertyName, PropertyValue>()),
    };

    // The EGRESS author: construct this row's IFC relationship and assign the relating/related sides to the row's IFC
    // attribute names (Relating/Related), so directionality + inverse round-trip and the long-tail families re-emit from
    // the row [NEUTRAL_EDGE_RULING] — the caller (ReauthorRelationships) feeds the endpoints already in IFC orientation, re-inverting the
    // Inverted Assign family first. Endpoints are IfcObjectDefinition-wide: a DefinesByType relating side is an
    // IfcTypeObject and an AssignsToGroup relating side an IfcGroup, neither an IfcProduct — a product-typed signature
    // is the deleted crash-cast form. Row-driven: a new IFC relationship is one row, never a per-class egress arm.
    // FactoryIfc.Construct mints the db-bound entity by class name (BaseClassIfc.Construct builds it, the factory registers
    // it on the database). The refined slot is the SUBTYPE-construct discriminant an edge attr carries and the row
    // cannot (the egress space-boundary level -> IfcRelSpaceBoundary1stLevel/2ndLevel); the row's attribute names still
    // fill the endpoints because a rider subtype shares its base's relating/related pair.
    public Option<IfcRelationship> Author(DatabaseIfc db, IfcObjectDefinition relating, Seq<IfcObjectDefinition> related, Option<string> refined = default) {
        if (related.IsEmpty || db.Factory.Construct(refined.IfNone(Key)) is not IfcRelationship rel) {
            return Option<IfcRelationship>.None;
        }
        Type shape = rel.GetType();
        shape.GetProperty(Relating)?.SetValue(rel, relating);
        switch (shape.GetProperty(Related)) {
            // A SET-valued related side is a GeometryGym read-only SET<T> (an ICollection<T>, NOT a System.Collections.IList),
            // so the fan-out members fill through its one unambiguous public Add(T) reflected per member; a single-valued
            // related side is null-until-set and writable, so it takes the head.
            case { } slot when slot.GetValue(rel) is { } set && set.GetType().GetMethod("Add") is { } add:
                related.Iter(member => add.Invoke(set, [member]));
                break;
            case { CanWrite: true } slot:
                slot.SetValue(rel, related[0]);
                break;
        }
        return Some(rel);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class EdgeProjection {
    // One fold over the whole relationship roster: the generic FanOut/Pair families through row.Edge, the inverted Assign
    // arms (DefinesByType/AssignsToGroup), the realizing Connect, and the dedicated payload-bearing folds (DefinesProperties/
    // Structural/SpatialBoundaries/MaterialEdges). A dangling endpoint faults [DanglingReference].
    public static Fin<Seq<Relationship>> All(IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, IIfcProfileStore profiles, FidelityLog log, Op key) {
        var edges = Decomposition(project, rooted, key)
            .Concat(Connections(project, rooted, key))
            .Concat(Assignments(project, rooted, log, key))
            .Concat(Generics(project, rooted, key))
            .Concat(DefinesProperties(project, rooted, key))
            .Concat(Structural(project, rooted, profiles, key))
            .Concat(SpatialBoundaries(project, rooted, key))
            .Concat(Seq(MaterialEdges(project, rooted, tolerance, scale, profiles, key)));
        return edges.TraverseM(identity).As().Map(static rels => rels.Flatten().ToSeq());
    }

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

    static Seq<Fin<Seq<Relationship>>> Connections(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        // Extract<IfcRelConnectsElements> returns its subclasses too — the realizing and path-element joins are handled by
        // their own arms (the realizing Connect below, the ConnectsPathElements Generic edge), so they are excluded here.
        Pair(project.Extract<IfcRelConnectsElements>().AsIterable().Where(static r => r is not (IfcRelConnectsWithRealizingElements or IfcRelConnectsPathElements)),
            IfcRelKind.ConnectsElements, rooted, key, static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
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
        // content-keys through the store's STEP-fragment lane onto the EccentricityKey attr (never inlined [M2]).
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
                { IsEmpty: true } => Seq(FinFail<Relationship>(new BimFault.ModelRejected(
                    key,
                    $"realizing-elements-empty:{rel.GlobalId}"))),
                var members => members.Map(member =>
                    from a in Resolve(rooted, rel.RelatingElement?.GlobalId ?? "", key)
                    from b in Resolve(rooted, rel.RelatedElement?.GlobalId ?? "", key)
                    from r in Resolve(rooted, member.GlobalId, key)
                    select (Relationship)new Relationship.Connect(a, b, ConnectKind.Element, Some(r))),
            })
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The Assign family is INVERTED: the seam Assign is Subject(occurrence)->Definition(type/group), the inverse of the
    // IFC relating(type/group)->related(occurrences), so each related occurrence is the subject and the relating the
    // definition. DefinesByType binds the type bags for inheritance; AssignsToGroup the group/system/zone membership.
    static Seq<Fin<Seq<Relationship>>> Assignments(IfcProject project, Map<string, NodeId> rooted, FidelityLog log, Op key) => Seq(
        project.Extract<IfcRelDefinesByType>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from occ in Resolve(rooted, o.GlobalId, key)
            from typ in Resolve(rooted, rel.RelatingType?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Assign(occ, typ, AssignKind.TypeDefinition)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()),
        // The ByFactor subtype's Factor has no seam slot on the typed Assign case — the membership edge lands and the
        // rider is the COUNTED group-factor fact (the NestOrdinal precedent, never silent).
        project.Extract<IfcRelAssignsToGroup>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from member in Resolve(rooted, o.GlobalId, key)
            from grp in Resolve(rooted, rel is IfcRelAssignsToGroupByFactor
                ? log.Noted(FidelityDrop.GroupFactor, rel.RelatingGroup?.GlobalId ?? "", rel.RelatingGroup?.GlobalId ?? "")
                : rel.RelatingGroup?.GlobalId ?? "", key)
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
    // 6-DOF restraint (fixity + SI spring) and the full IfcStructuralLoad family are lowered through the DEDICATED
    // Model/structural#STRUCTURAL_PROJECTION StructuralProjection.Attrs owner (never a local boolean-only/single-force-only
    // reader) — the Fin-railed Attrs(rel, key) builds the WHOLE edge payload in ONE call (restraint + skew frame +
    // SupportedLength + AtStart; load + Station), so a local two-step over the connection/activity entity is the deleted
    // form and a malformed structural measure faults typed on this rail. The eccentric subtype
    // preserves its MANDATORY payload: an IfcRelConnectsWithEccentricity's ConnectionConstraint (an IfcConnectionGeometry
    // the inline prohibition keeps off the attrs [M2]) content-keys through the store's STEP-fragment lane and rides the
    // EccentricityKey attr — the ProfileRef/ProfileStepKey idiom — so egress reconstitutes the exact subtype instead of
    // degrading it to the base member binding (a bare eccentric subtype without its constraint is schema-invalid; the
    // silent downgrade was the closed loss). So a Rasm.Compute frame solve reads graph.SupportsOf/graph.LoadsOf off these
    // edges and resolves the analytical axis BY CONTENT KEY from Representations (content-keyed by IfcRepresentation.Keys,
    // never an Enrich bake) — never re-reading IFC, never a defaulted support joint or load case.
    static Seq<Fin<Seq<Relationship>>> Structural(IfcProject project, Map<string, NodeId> rooted, IIfcProfileStore profiles, Op key) => Seq(
        project.Extract<IfcRelConnectsStructuralMember>().AsIterable().Select(rel =>
            from m in Resolve(rooted, rel.RelatingStructuralMember?.GlobalId ?? "", key)
            from c in Resolve(rooted, rel.RelatedStructuralConnection?.GlobalId ?? "", key)
            from attrs in StructuralProjection.Attrs(rel, key)
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructMember.Key, m, c,
                rel is IfcRelConnectsWithEccentricity { ConnectionConstraint: { } constraint }
                    ? attrs.AddOrUpdate(SemanticProjector.EccentricityKey,
                        new PropertyValue.Text(profiles.Preserve(constraint, key).ToString()))
                    : attrs))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()),
        project.Extract<IfcRelConnectsStructuralActivity>().AsIterable().Select(rel =>
            from item in Resolve(rooted, (rel.RelatingElement as IfcRoot)?.GlobalId ?? "", key)
            from act in Resolve(rooted, rel.RelatedStructuralActivity?.GlobalId ?? "", key)
            from attrs in StructuralProjection.Attrs(rel, key)
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructActivity.Key, item, act, attrs))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The energy/spatial space-boundary graph onto neutral Generic edges [NEUTRAL_EDGE_RULING]: IfcRelSpaceBoundary binds a space to its
    // bounding building element, the 1st/2nd-level discriminant riding the attrs (the physical/virtual + internal/external
    // enums are GeometryGym internal fields, absent from the public surface, so they are not lowered) — so a Rasm.Compute
    // OSM build reads graph.SpacesOf/graph.BoundingSurfacesOf off the baked graph. RelatingSpace is an
    // IfcSpaceBoundarySelect, so its GlobalId reads through the IfcRoot cast.
    static Seq<Fin<Seq<Relationship>>> SpatialBoundaries(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelSpaceBoundary>().AsIterable().Select(rel =>
            from s in Resolve(rooted, (rel.RelatingSpace as IfcRoot)?.GlobalId ?? "", key)
            from e in Resolve(rooted, rel.RelatedBuildingElement?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, s, e, BoundaryAttrs(rel)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.ToSeq()));

    // The material Associate edges [OCCURRENCE_USAGE_RULING]: each related element binds the projected Material node (the content-keyed id the
    // Materials fold lands) through an Associate edge carrying the occurrence MaterialUsage (the LayerSet direction/sense/
    // offset or the ProfileSet cardinal-point/extent), so a wall and its mirror share one LayerSet node with two usages;
    // the ProfileSet usage admits through the seam MaterialUsage.ProfileSet.Of cardinal-point gate. The material node and
    // usage bind ONCE PER RELATION and the related objects fan over the pair — the per-(rel, object) re-projection ran the
    // whole composition fold N times per relation, the deleted quadratic form the MaterialIndex law already names.
    // RelatingMaterial is schema-mandatory, so a null read is a malformed file faulting typed HERE (the node-side
    // Materials fold Optional-skips the same null — this edge rail is the one fault site, per the fault-site law).
    static Fin<Seq<Relationship>> MaterialEdges(
        IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, IIfcProfileStore profiles, Op key) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable().Select(rel =>
            from relating in Optional(rel.RelatingMaterial).ToFin(new BimFault.ModelRejected(key, $"material-relation-unbound:{rel.GlobalId}"))
            from material in MaterialNode(relating, tolerance, profiles, key)
            from usage in UsageOf(relating, scale, key)
            from elements in rel.RelatedObjects.AsIterable().ToSeq().TraverseM(o => Resolve(rooted, o.GlobalId, key)).As()
            select elements.Map(element => (Relationship)new Relationship.Associate(element, material, usage)))
            .AsIterable().TraverseM(identity).As().Map(static e => e.Flatten().ToSeq());

    static Fin<NodeId> MaterialNode(IfcMaterialSelect select, double tolerance, IIfcProfileStore profiles, Op key) =>
        MaterialProjection.Project(select, tolerance, profiles, key).Map(static m => m.Id);

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
            ? MeasureValue.OfSi(Dimension.LengthDim, native * scale.Factor(Dimension.LengthDim)).Map(Some)
            : FinSucc(Option<MeasureValue>.None);

    static Option<int> OptionalCardinal(IfcCardinalPointReference point) =>
        point == IfcCardinalPointReference.MID ? Option<int>.None : Some((int)point);

    // The space-boundary level discriminant -> the egress-declared SemanticProjector.BoundaryLevel attr, THREE-valued
    // because the runtime type is: "2nd"/"1st" name the exact subtype the egress refined-construct re-authors, "" the
    // base-class instance (2ndLevel derives from 1stLevel, so the 2nd probe runs first — a level is never upgraded onto
    // a base instance). The GeometryGym IfcRelSpaceBoundary exposes RelatingSpace/RelatedBuildingElement/
    // ConnectionGeometry publicly but keeps PhysicalOrVirtualBoundary/InternalOrExternalBoundary on internal fields
    // with no public getter, so the level is the lone publicly-readable flag; the physical/virtual + internal/external
    // classification is not on the host surface and is never fabricated (an internal-field reflection read is the
    // fragile form this owner refuses).
    static Map<PropertyName, PropertyValue> BoundaryAttrs(IfcRelSpaceBoundary rel) => Map(
        (SemanticProjector.BoundaryLevel, (PropertyValue)new PropertyValue.Text(
            rel is IfcRelSpaceBoundary2ndLevel ? "2nd" : rel is IfcRelSpaceBoundary1stLevel ? "1st" : "")));

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

- [RELATION_NEUTRALITY]: the neutral `Relationship` algebra plus `Generic` passthrough keeps IFC names and directionality on the `IfcRelKind` row while payload-bearing families stay on their existing typed seam cases. The inverted `Assign` rows re-orient through the row policy, realizing connections fan over the complete realizing set and regroup at egress, and material usage rides the `Associate` payload. Ordered nests land `Generic("IfcRelNests", parent, child, {NestOrdinal})` with one per-parent-continuous `PropertyValue.Integer` ordinal, so the `LIST` order survives without overloading the physical-measure domain.
- [ROSTER_CENSUS]: the roster covers the FULL concrete node-to-node `IfcRelationship` subtree of the catalogued GeometryGym assembly, enumerated by decompile census — the additions over the prior roster are `IfcRelProjectsElement` (`RelatingElement`/`RelatedFeatureElement`), `IfcRelAdheresToElement` (`RelatingElement`/`RelatedSurfaceFeatures` `SET<IfcSurfaceFeature>`), `IfcRelFlowControlElements` (GG's REAL public pair `RelatingPort`/`RelatedElement`; the schema's `RelatingFlowElement`/`RelatedControlElements` are absent from the decompiled surface), `IfcRelPositions` (`RelatingPositioningElement`/`RelatedProducts` `SET<IfcProduct>`), `IfcRelDefinesByObject` (`RelatingObject`/`RelatedObjects` `SET<IfcObject>`), and `IfcRelConnectsStructuralElement` (`RelatingElement`/`RelatedStructuralMember`), every member decompile-verified. Rider subtypes land through their base `Extract<T>`: `IfcRelAssignsToGroupByFactor` (public `Factor`, no seam slot — named drop), `IfcRelConnectsWithEccentricity` (public settable `ConnectionConstraint` `IfcConnectionGeometry` and the public `(IfcStructuralMember, IfcStructuralConnection, IfcConnectionGeometry)` ctor, decompile-verified — the constraint content-keys through the store's STEP-fragment lane onto the `EccentricityKey` attr, never inlined [M2], and the egress refined-construct reconstitutes the exact subtype with its constraint restored from the store), `IfcRelOverridesProperties` (rides `DefinesProperties`, re-emitting as plain `IfcRelDefinesByProperties` — the override semantics await a seam `AssignKind` row, a named drop), `IfcRelSpaceBoundary1stLevel`/`2ndLevel` (the `SpatialBoundaries` three-valued `BoundaryLevel` discriminant — the egress refined-construct re-authors the EXACT subtype, so the level round-trips), and the 2x3 control tails `IfcRelAssignsTasks`/`IfcRelAssignsToProjectOrder` (ride `AssignsToControl`, re-emitting as the base wire-name — bounded). `IfcRelConnectsWithRealizingElements.RealizingElements` is a `SET<IfcElement>` `[1:?]` — the ingress fans one `Connect` edge per member and the egress re-groups by endpoint pair, so realizing cardinality is lossless (two separate realizing relations between one endpoint pair merge into one at egress — the per-pair denormalized-but-lossless law). The NON-node associations the Growth axis defends are the complete residue: `IfcRelAssociatesApproval`/`Constraint`/`Dataset`/`Document`/`Library`/`ProfileDef`/`ProfileProperties`, `IfcRelDefinesByTemplate`, `IfcRelAssignsToResource` (`IfcRelAssociatesClassification` is owned by `Semantics/classification#CLASSIFICATION_AXIS`, `IfcRelAssociatesMaterial` by the `MaterialEdges` fold); `IfcRelInteractionRequirements`/`IfcRelOccupiesSpaces` do not exist on the GG surface.
- [STRUCTURAL_ENERGY_LOWERING]: the `DefinesProperties`/`Structural`/`SpatialBoundaries` `EdgeProjection` folds lower the structural idealization and the energy/spatial CONNECTIVITY onto the seam graph so `Rasm.Compute` reads it off the neutral `Generic` edges (`ELEMENT-REBUILD-PLAN.md` §4E). The member surface is decompile-verified against the catalogued GeometryGym assembly (`.api/api-geometrygym-ifc`): `IfcRelDefinesByProperties.RelatedObjects` (`SET<IfcObjectDefinition>`) + `RelatingPropertyDefinition` (a `SET<IfcPropertySetDefinition>`, fanned out per pair); `IfcRelConnectsStructuralMember.RelatingStructuralMember`/`RelatedStructuralConnection` and `IfcRelConnectsStructuralActivity.RelatingElement` (`IfcStructuralActivityAssignmentSelect`, read through the `IfcRoot` cast)/`RelatedStructuralActivity` are the edge endpoints the `Structural` fold resolves, the restraint/load PAYLOAD (the 6-DOF fixity + SI spring per DOF, the full `IfcStructuralLoad` family, and the neutral `LoadKind`/`Case` tokens) delegated to the dedicated `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection.Attrs` owner and the `AtStart` start/end discriminant to `StructuralProjection.AtStart` — never a local boolean-only/single-force reader; `IfcRelSpaceBoundary.RelatingSpace` (`IfcSpaceBoundarySelect`, read through the `IfcRoot` cast)/`RelatedBuildingElement` + the `IfcRelSpaceBoundary2ndLevel` discriminant (the `PhysicalOrVirtualBoundary`/`InternalOrExternalBoundary` enums are GeometryGym internal fields with no public getter, so the seam lowers only the publicly-readable boundary level, never an internal-field reflection read). The structural curve member's idealized analytical axis is content-keyed in `Representations` by `IfcRepresentation.Keys` (the polymorphic representation content-keyer), NEVER inlined as a coordinate field on the `Object` node (an inline `Axis`/`BoundaryPolygon` member is the deleted §4-RT-M2 violation) — the heavy display geometry likewise content-hashed on `RepresentationContentHash`, materializing it in-process being the geometry evaluation the projector boundary (and `Model/elements#REPRESENTATION_KEYS`) forbids — so `Rasm.Compute` RESOLVES the analytical axis/footprint one-hop BY CONTENT KEY from the blob store rather than re-deriving it.
