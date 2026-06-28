# [BIM_SEMANTIC_PROJECTOR]

`Rasm.Bim` is the SOLE GeometryGym/IFC owner and the IFC arm of the `Rasm.Element` seam. This page owns the one `SemanticProjector : IElementProjection` that lowers a live GeometryGym `DatabaseIfc` into a seam `GraphDelta` (the `Project` ingress) and re-authors a seam `ElementGraph` back into IFC STEP/XML/JSON bytes (the `Emit` egress, a Bim-INTERNAL operation, never a seam member), plus the `IfcLegality : IGraphConstraint` that decides IFC-semantic legality the seam's structural `GraphDelta` switch cannot. The projector replaces the retired `BimModel.Project`/`BimElement` fold: where the old owner produced a second stored element record keyed by GlobalId, the projector produces seam `Node`s (`Object` occurrence/type, `PropertySet`, `QuantitySet`, `Material`, `Coverage`) and neutral `Relationship` edges that `Assemble` folds into the canonical `ElementGraph`, so "has it all" is one `Bake` read on the seam graph and GeometryGym never leaks below the seam. The whole IFC relationship vocabulary — every `IfcRel*` name, its directionality, and its inverse — plus the eight families the prior owner stranded ride the NEUTRAL edge payload [C5], so no relationship is dropped and the seam stays IFC-schema-free. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the kernel geometry by content-hash reference, never a RhinoCommon type, never an in-process BRep evaluation.

## [01]-[INDEX]

- [01]-[SEMANTIC_PROJECTOR]: `SemanticProjector : IElementProjection`, the `Project` fold lowering `DatabaseIfc` into a `GraphDelta` — rooted `NodeId` mint with the 1:1 IFC `GlobalId` projection attribute [H6], the `Object` occurrence/type nodes carrying the generic `Classification`/`PredefinedType`/`RepresentationContentHash`, the `PropertySet`/`QuantitySet` nodes stamped with `InheritanceMode` [H1], `OwnerHistory`/`StepHeader` projection [H9], the schema span [H8], and the `Enrich` analytical-geometry bake (the idealized structural member `Axis` + the space-boundary surface `BoundaryPolygon` stamped on the `Object` node as kernel `Vector3`, so `Rasm.Compute` reads `graph.AxisOf`/`surface.BoundaryPolygon` baked).
- [02]-[RELATION_ALGEBRA]: `IfcRelKind` the full `IfcRel*` roster `[SmartEnum<string>]` (name + neutral `EdgeAxis` + directionality + inverse-attribute + cardinality), the eight stranded families, and the `EdgeProjection` fold lowering every relationship onto a neutral `Relationship` edge carrying the IFC wire-name and attributes on the payload [C5][C7] — including the `DefinesProperties` property/quantity attachment (`Assign.PropertyDefinition`), the `Structural` idealization fold (member↔connection 6-DOF restraint + member↔activity load on the `Generic` edge payload), and the `SpatialBoundaries` fold (space↔bounding-surface on the `Generic` edge payload) the `Rasm.Compute` structural/energy runners read.
- [03]-[IFC_EGRESS]: `SemanticProjector.Emit` the Bim-internal `ElementGraph` → IFC bytes re-author — the `FILE_SCHEMA` sniff [H8], the `PredefinedType` egress gate over the frozen valid set and schema span → `BimFault.UnmappedClass` [C6], the `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` `ChangeAction` re-stamp [H9], the `ReauthorMaterials` seam `Material` subgraph egress (`MaterialProjection.AuthorComposition`/`AuthorUsage` per node + `Associate` usage [C7]), and the `ReauthorClassifications` element-classification egress (`ClassificationSystem.Author`) — the seam-graph egress that REPLACES the retired `Rasm.Materials` material wires.
- [04]-[GRAPH_LEGALITY]: `IfcLegality : IGraphConstraint` the IFC-semantic legality validator — containment-relating-must-be-spatial, `Voids` element→opening, type-may-not-aggregate-occurrence — accumulating onto `Validation<Error,Unit>` over the seam's structural invariants [M3].

## [02]-[SEMANTIC_PROJECTOR]

- Owner: `SemanticProjector` the `IElementProjection` capturing one live GeometryGym `DatabaseIfc` internally (the import rail's `Exchange/import#IMPORT_RAIL` `Database` decode) and lowering it to a seam `GraphDelta` in `Project`; `SchemaSniff` the `FILE_SCHEMA`/`schema_identifier` resolver run before the database is constructed so the schema is read off the file, never hardcoded; `OwnerStamp` the `IfcOwnerHistory`→seam `OwnerHistory` projection; `StepHeaderOf` the `STEPFileInformation`→seam `StepHeader` projection.
- Entry: `SemanticProjector.Project(ProjectionContext ctx)` folds the captured `DatabaseIfc` into one `GraphDelta` — it mints a NEUTRAL rooted `NodeId` per `IfcRoot` through `ctx.Rooted()` (the kernel `IObjectFactory` floor), records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute [H6], and content-keys every non-rooted node (`IfcMaterial`/`IfcProfileDef`/`IfcMaterialLayer`) through the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes()`; `Fin<T>` aborts on an unmapped entity class (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or a dangling spatial host (`BimFault.DanglingReference`) lowered with `.ToError()`. The element identity is established HERE (the IFC is the source of element identity), so the projector ignores `ctx.ElementIds` (the aspect-projector NodeId set) and PUBLISHES the minted ids in the delta for `Rasm.Materials/Projection/material#MATERIAL_PROJECTOR` to attach `Associate` edges against.
- Auto: `Project` walks the captured `db.Project` once — `Extract<IfcProduct>` lands an `Object.Occurrence` node carrying the generic `Classification("ifc", classKey)` (the IFC entity type as a classification, never `IfcClass` on the node) resolved through `Model/elements#ELEMENT_MODEL` `IfcClass.Resolve`, the `PredefinedType` token split by `ParserIfc.IdentifyIfcClass`, the keyed `RepresentationContentHash` map (`Model/elements#ELEMENT_MODEL` `RepresentationKeys`) [M2], and the `OwnerStamp` `OwnerHistory` [H9]; `Extract<IfcTypeObject>` lands an `Object.Type` node carrying the `IfcTypeProduct.RepresentationMaps` instanced-geometry content key; `Extract<IfcPropertySet>`/`Extract<IfcElementQuantity>` land `PropertySet`/`QuantitySet` bag nodes STAMPED with the `Semantics/properties#PROPERTY_SETS` `PropertyInheritance.ModeOf` `InheritanceMode` at ingest [H1] so the seam `Bake` applies the correct type→occurrence precedence wholly within the seam; `Extract<IfcRelAssociatesMaterial>` lands `Material` nodes through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.Project` and the `Associate` material edge carrying the occurrence-usage payload [C7]; the `Semantics/georeference#GEO_REFERENCE` `GeoReferenceProjector.Project` lands the `Header.GeoReference` [M1] and `Semantics/geospatial#GEOSPATIAL_SEAM` raster lands `Coverage` nodes; `EdgeProjection.All` lands every `IfcRel*` neutral edge [C5] — including the `DefinesProperties` `Assign.PropertyDefinition` bag attachment, the `Structural` member↔connection / member↔activity `Generic` edges (the 6-DOF restraint and applied force/moment riding the payload), and the `SpatialBoundaries` space↔surface `Generic` edges; finally `Enrich` bakes the structural member `Axis` and the space-boundary `BoundaryPolygon` onto the `Object` nodes so `Rasm.Compute` reads the structural idealization and the energy/spatial model fully baked off the seam graph.
- Receipt: the `GraphDelta` is the projector's whole contribution — a merge over the canonical `ElementGraph` that `Rasm.Element/Projection/projection#PROJECTION` `Assemble` folds with the other projectors' deltas; the minted rooted-`NodeId` set keyed by `GlobalId` is the identity table aspect projectors attach against and `Emit` reverses.
- Packages: GeometryGymIFC_Core, Rasm.Element, Riok.Mapperly, Generator.Equals, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new extracted IFC entity family is one `Extract<T>` arm on the `Project` fold landing its seam node; a new relationship is one `IfcRelKind` row the `EdgeProjection` reads (`[03]-[RELATION_ALGEBRA]`); a new schema version is one `ReleaseVersion` the `SchemaSniff` resolves and the `Model/elements#ELEMENT_MODEL` span validates; never a second element record beside the seam graph and never a per-entity projector type.
- Boundary: the projector is the ONE GeometryGym→seam lowering — the retired `BimModel.Project` produced a second stored `BimElement` keyed by `GlobalId`, and any owner that re-stores the element off the seam graph is the deleted form; GeometryGym is captured INTERNALLY (the `DatabaseIfc` field) and an `IfcProduct`/`IfcRel*`/`DatabaseIfc` type crossing the `IElementProjection.Project` signature is the named seam violation — the seam holds only `Node`/`Relationship`/`GraphDelta`; the rooted `NodeId` is a neutral kernel-minted id and the compressed IFC `GlobalId` is the node's `ExternalId` projection attribute (1:1) [H6], so the IFC GUID never becomes the node identity and the from-scratch authoring path mints its own neutral id; the non-rooted node id is the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes()` and a second hasher is the deleted form [H7]; geometry is referenced by `RepresentationContentHash` only [M2] and an in-process BRep evaluation or a RhinoCommon handle is the named seam violation; the projector reads the live `DatabaseIfc` (not the lossy import-rail rows) so the FULL relationship roster, `OwnerHistory`, and `StepHeader` survive — a flat row projection that drops the eight stranded families is the deleted form; `Emit` is a Bim-INTERNAL method on the projector, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the seam owns only ingress projection.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Text.Json.Nodes;
using GeometryGym.Ifc;
using GeometryGym.STEP;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [SERVICES] ---------------------------------------------------------------------------
// The one GeometryGym->seam lowering: the DatabaseIfc is captured internally (the IElementProjection
// contract holds only Node/Relationship/GraphDelta), and Project mints the neutral rooted identity while
// recording the IFC GlobalId as the node ExternalId 1:1 [H6]. Emit is Bim-internal, NOT a seam member.
public sealed class SemanticProjector(DatabaseIfc db) : IElementProjection {
    public Fin<GraphDelta> Project(ProjectionContext ctx) {
        var project = db.Project;
        if (project is null) {
            return Fin.Fail<GraphDelta>(new BimFault.DanglingReference("ifc-project-root-miss").ToError());
        }
        // The GlobalId->NodeId table: one neutral rooted mint per IfcRoot, the IFC GlobalId held as the
        // node ExternalId so re-ingest matches on the stored GlobalId (the Persistence diff/merge key) and
        // Emit reverses the 1:1 projection. IfcMaterial/IfcProfileDef are non-rooted (content-keyed, below).
        var rooted = project.Extract<IfcRoot>().AsIterable()
            .Fold(Map<string, NodeId>(), (map, root) => map.AddOrUpdate(root.GlobalId, ctx.Rooted()));
        var header = new Header(db.Release, db.ModelView, GeoReferenceProjector.Project(project),
            db.Tolerance, ctx.At, StepHeaderOf(db));
        return Objects(project, rooted)
            .Bind(objects => Bags(project, rooted)
                .Map(bags => objects.Concat(bags).Concat(Materials(project))))
            .Bind(nodes => EdgeProjection.All(project, rooted)
                .Map(edges => Enrich(nodes, project, rooted).Fold(GraphDelta.Reheader(header), static (delta, node) => delta.Put(node))
                    .Apply(seeded => edges.Fold(seeded, static (delta, edge) => delta.Link(edge)))));
    }

    // Bakes the neutral analytical coordinate geometry onto the Object nodes so Rasm.Compute reads everything baked:
    // a structural IfcStructuralCurveMember's idealized axis line (read by graph.AxisOf) and a space-boundary's
    // bounding-surface polygon (read by surface.BoundaryPolygon), lowered through the Bim-internal IfcRepresentation
    // geometry reader to kernel Vector3 and stamped on the seam Node.Object, so the structural/energy runners never
    // re-read IFC geometry; the heavy display geometry stays content-hashed on RepresentationContentHash, only the
    // lightweight analytical coordinates bake — a structural member carries its Axis, a bounding element its polygon.
    Seq<Node> Enrich(Seq<Node> nodes, IfcProject project, Map<string, NodeId> rooted) {
        var axes = project.Extract<IfcStructuralCurveMember>().AsIterable()
            .Choose(m => rooted.Find(m.GlobalId).Bind(id => IfcRepresentation.AxisOf(m, db.Tolerance).Map(a => (Id: id, Axis: a))))
            .Fold(Map<NodeId, AxisCurve>(), static (map, e) => map.AddOrUpdate(e.Id, e.Axis));
        var polygons = project.Extract<IfcRelSpaceBoundary>().AsIterable()
            .Choose(rel => Optional(rel.RelatedBuildingElement).Bind(el => rooted.Find(el.GlobalId))
                .Map(id => (Id: id, Poly: IfcRepresentation.BoundaryOf(rel, db.Tolerance))))
            .Filter(static e => e.Poly.Count > 0)
            .Fold(Map<NodeId, Seq<Vector3>>(), static (map, e) => map.AddOrUpdate(e.Id, e.Poly));
        return nodes.Map(node => node is Node.Object o
            ? o with { Axis = axes.Find(o.Id) | o.Axis, BoundaryPolygon = polygons.Find(o.Id).IfNone(o.BoundaryPolygon) }
            : node);
    }

    // Each IfcProduct -> Object.Occurrence; each IfcTypeObject -> Object.Type. The generic Classification
    // ("ifc", classKey) carries the entity type WITHOUT leaking IfcClass onto the node; the PredefinedType
    // token splits at ParserIfc.IdentifyIfcClass; RepresentationContentHash is the keyed geometry map [M2];
    // OwnerHistory rides optionally from IfcRoot.OwnerHistory [H9]; ExternalId is the 1:1 GlobalId [H6].
    static Fin<Seq<Node>> Objects(IfcProject project, Map<string, NodeId> rooted) =>
        project.Extract<IfcProduct>().AsIterable().Map(p =>
            IfcClass.Resolve(ParserIfc.IdentifyIfcClass(p.GetType().Name, out var token))
                .Map(cls => (Node)new Node.Object(
                    Id:         rooted[p.GlobalId],
                    Mode:       ObjectMode.Occurrence,
                    ExternalId: Some(p.GlobalId),
                    Class:      new Classification("ifc", cls.Key),
                    Predefined: PredefinedType.Admit(token, (p as IfcObject)?.ObjectType ?? ""),
                    Geometry:   IfcRepresentation.KeysOf(p, db.Tolerance, db.ToleranceAngleRadians),
                    Owner:      OwnerStamp(p.OwnerHistory),
                    Name:       p.Name ?? "",
                    Tag:        (p as IfcElement)?.Tag ?? "")))
            .Sequence().Map(static occ => occ.ToSeq())
            .Bind(occurrences => Types(project, rooted).Map(occurrences.Concat));

    static Fin<Seq<Node>> Types(IfcProject project, Map<string, NodeId> rooted) =>
        project.Extract<IfcTypeObject>().AsIterable().Map(t =>
            IfcClass.Resolve(ParserIfc.IdentifyIfcClass(t.GetType().Name, out var token))
                .Map(cls => (Node)new Node.Object(
                    Id:         rooted[t.GlobalId],
                    Mode:       ObjectMode.Type,
                    ExternalId: Some(t.GlobalId),
                    Class:      new Classification("ifc", cls.Key),
                    Predefined: PredefinedType.Admit(token, ""),
                    Geometry:   IfcRepresentation.MapKeys(t as IfcTypeProduct, db.Tolerance, db.ToleranceAngleRadians),
                    Owner:      OwnerStamp(t.OwnerHistory),
                    Name:       t.Name ?? "",
                    Tag:        "")))
            .Sequence().Map(static types => types.ToSeq());

    // PropertySet/QuantitySet bag nodes stamped with the InheritanceMode at ingest [H1] so the seam Bake
    // resolves type->occurrence precedence without re-reading IFC; the typed value (PropertyValue/MeasureValue)
    // is the seam's, the TEMPLATE (datatype/placement) is Semantics/properties' bSDD-resolved PropertyKey.
    static Fin<Seq<Node>> Bags(IfcProject project, Map<string, NodeId> rooted) {
        var properties = project.Extract<IfcPropertySet>().AsIterable().Map(ps => (Node)new Node.PropertySet(
            Id:          rooted[ps.GlobalId],
            ExternalId:  Some(ps.GlobalId),
            Name:        ps.Name ?? "",
            Mode:        PropertyInheritance.ModeOf(ps.Name ?? "", typeDriven: ps is { } and not null && IsTypeBound(ps)),
            Values:      ps.HasProperties.Values.OfType<IfcPropertySingleValue>()
                            .ToMap(static pv => new PropertyName(pv.Name ?? ""),
                                   static pv => PropertyValue.Of(pv.NominalValue?.ValueString ?? "", pv.NominalValue?.GetType().Name ?? "IfcText"))));
        var quantities = project.Extract<IfcElementQuantity>().AsIterable().Map(eq => (Node)new Node.QuantitySet(
            Id:          rooted[eq.GlobalId],
            ExternalId:  Some(eq.GlobalId),
            Name:        eq.Name ?? "",
            Mode:        PropertyInheritance.ModeOf(eq.Name ?? "", typeDriven: true),
            Values:      eq.Quantities.Values.OfType<IfcPhysicalSimpleQuantity>()
                            .ToMap(static q => new PropertyName(q.Name ?? ""),
                                   static q => MeasureValue.Of(q.MeasureValue?.Measure ?? 0d, q.Unit?.ToString() ?? ""))));
        return Fin.Succ(properties.Concat(quantities).ToSeq());
    }

    static bool IsTypeBound(IfcPropertySetDefinition set) =>
        set.DefinesType.Any() || set.IsDefinedBy.Any(static rel => rel.RelatingObject is IfcTypeObject);

    // Non-rooted material nodes are content-keyed (kernel seed-zero XxHash128 over ToCanonicalBytes [H7]),
    // never GlobalId-rooted; the composition fold (Single/LayerSet/ProfileSet/ConstituentSet) is the IFC
    // material algebra Semantics/composition owns, lifted onto the seam Material node here.
    static Seq<Node> Materials(IfcProject project) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Choose(static rel => MaterialProjection.Project(rel.RelatingMaterial as BaseClassIfc).ToOption())
            .Map(static material => (Node)Node.Material.Content(material))
            .DistinctBy(static node => node.Id)
            .ToSeq();

    // IfcOwnerHistory -> seam OwnerHistory [H9]: owning user/app, created/modified, change action, state.
    // Absent owner history yields None so a headerless model still projects; Emit re-derives ChangeAction.
    static Option<OwnerHistory> OwnerStamp(IfcOwnerHistory? history) =>
        Optional(history).Map(static h => new OwnerHistory(
            OwningUser:        h.OwningUser?.ToString() ?? "",
            OwningApplication: h.OwningApplication?.ApplicationFullName ?? "",
            Created:           Instant.FromUnixTimeSeconds(h.CreationDate),
            Modified:          h.LastModifiedDate > 0 ? Some(Instant.FromUnixTimeSeconds(h.LastModifiedDate)) : None,
            Change:            h.ChangeAction.ToString(),
            State:             h.State.ToString()));

    // STEPFileInformation -> seam StepHeader [H9]: FILE_DESCRIPTION/FILE_NAME/FILE_SCHEMA. A separate axis
    // from the Marten provenance; the IFC owner history is NOT substituted by the persistence stamp.
    static StepHeader StepHeaderOf(DatabaseIfc database) =>
        database.OriginatingFileInformation is { } info
            ? new StepHeader(
                Descriptions: info.FileDescriptions.ToSeq(),
                Name:         info.FileName ?? "",
                TimeStamp:    Instant.FromDateTimeUtc(DateTime.SpecifyKind(info.TimeStamp, DateTimeKind.Utc)),
                Authors:      info.Author.ToSeq(),
                Organizations: info.Organization.ToSeq(),
                Preprocessor: info.PreProcessorVersion ?? "",
                Originating:  info.OriginatingSystem ?? "",
                Schema:       Seq1(database.Release.ToString()))
            : StepHeader.Empty with { Schema = Seq1(database.Release.ToString()) };
}
```

## [03]-[RELATION_ALGEBRA]

- Owner: `IfcRelKind` the `[SmartEnum<string>]` carrying the WHOLE `IfcRel*` roster keyed on the relationship entity name — each row carrying the neutral `EdgeAxis` it folds onto (`Compose`/`Assign`/`Associate`/`Connect`/`Void`), the directionality (the relating-side and related-side IFC inverse-attribute names), and the `RelCardinality` (one-to-one, one-to-many, many-to-many) so directionality and inverse semantics survive on the neutral payload [C5]; `EdgeAxis` the Bim-side selector mapping a row onto the seam `Relationship` case; `EdgeProjection` the static fold lowering every relationship family onto neutral `Relationship` edges.
- Cases: the standard roster — `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure` (`Compose`), `IfcRelReferencedInSpatialStructure`/`IfcRelConnectsElements`/`IfcRelConnectsWithRealizingElements`/`IfcRelConnectsPorts`/`IfcRelSequence` (`Connect`), `IfcRelAssociatesMaterial`/`IfcRelDefinesByProperties`/`IfcRelDefinesByType`/`IfcRelAssociatesClassification` (`Associate`), `IfcRelAssignsToGroup`/`IfcRelServicesBuildings` (`Assign`), `IfcRelVoidsElement`/`IfcRelFillsElement` (`Void`) — PLUS the eight families the retired owner stranded: `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity`, `IfcRelConnectsPortToElement`, `IfcRelSpaceBoundary` (first/second level), `IfcRelCoversBldgElements`/`IfcRelCoversSpaces`, `IfcRelConnectsPathElements` (wall-join priorities), `IfcRelDeclares`, `IfcRelAssignsToControl`/`IfcRelAssignsToProcess`/`IfcRelAssignsToProduct`/`IfcRelAssignsToActor`, and `IfcRelInterferesElements` — each lands its neutral edge carrying the IFC wire-name and the directionality/inverse on the attribute payload, so no relationship is dropped and none re-opens the IFC schema strata leak on the seam.
- Entry: `EdgeProjection.All(IfcProject project, Map<string,NodeId> rooted)` folds every relationship family into a `Seq<Relationship>` — each `Extract<IfcRelXxx>` arm reads the relating/related `GlobalId`s, resolves them through the `rooted` map, looks up the `IfcRelKind` row for the axis/wire-name/cardinality, and constructs the neutral edge; a many-to-many relationship fans out one edge per (relating, related) pair carrying the ordinal on the payload; `Fin<T>` aborts on a dangling endpoint (`BimFault.DanglingReference`) lowered with `.ToError()`. The `DefinesProperties` fold lands the `IfcRelDefinesByProperties` property/quantity attachment as `Assign(subject, definition, AssignKind.PropertyDefinition)` so the seam `Bake` folds the bag into the element; the `Structural` fold lands `IfcRelConnectsStructuralMember` (member→connection, the 6-DOF restraint off `IfcStructuralConnection.AppliedCondition` riding the payload) and `IfcRelConnectsStructuralActivity` (item→load, the applied force/moment off `IfcStructuralActivity.AppliedLoad` riding the payload) as `Generic` edges; the `SpatialBoundaries` fold lands `IfcRelSpaceBoundary` (space→bounding-surface, the physical/virtual + internal/external flags and boundary level riding the payload) as `Generic` edges — the structural idealization and energy/spatial connectivity the `Rasm.Compute` runners traverse by wire-name.
- Auto: the `Compose`/`Assign`/`Associate`/`Connect`/`Void` axis maps onto the seam `Relationship` case through `EdgeAxis.Edge`; the IFC wire-name (`IfcRelAggregates`, `IfcRelVoidsElement`, …) rides `Relationship.Generic` ONLY where the neutral five-case algebra cannot carry the semantics (the wall-join priority on `IfcRelConnectsPathElements`, the space-boundary physical/virtual flag), otherwise the typed case carries the wire-name and inverse on its attribute bag; the material `Associate` edge threads the `Semantics/composition#MATERIAL_COMPOSITION` occurrence-usage payload [C7] (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`, `CardinalPoint`/`ReferenceExtent`) as the seam's typed `MaterialUsage` (`LayerSet`/`ProfileSet`) payload, never a `PropertyValue` attribute bag.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new IFC relationship is one `IfcRelKind` row plus one `Extract<T>` arm on `EdgeProjection.All`; a new directionality or inverse is one column on the row; the neutral five-case algebra absorbs the axis and `Generic` absorbs the residue — never a parallel `RelationshipKind` on the seam and never a dropped family.
- Boundary: the seam `Relationship` is the NEUTRAL five-case edge algebra plus `Generic` — the seam carries no typed `IfcRel*` case and re-introducing seventeen typed IFC cases is the deleted form the critique fixed [C5]; the IFC names/directionality/inverse and the eight stranded families live HERE on the Bim side and ride the neutral payload, so a relationship dropped at ingest is the named defect; directionality is preserved by the relating→related orientation the `IfcRelKind` row records, never inferred at the call site; the material occurrence-usage rides the `Associate` edge's typed `MaterialUsage` payload [C7] and a parallel usage node is the deleted form.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public enum EdgeAxis : byte { Compose = 0, Assign = 1, Associate = 2, Connect = 3, Void = 4 }

public enum RelCardinality : byte { OneToOne = 0, OneToMany = 1, ManyToMany = 2 }

// The whole IfcRel* roster: name -> neutral axis + directionality (relating/related inverse-attribute
// names) + cardinality. The eight families the prior owner stranded join here so EVERY relationship has a
// home; the neutral edge keeps the wire-name + inverse so IFC directionality round-trips without a seam
// IFC case [C5]. Ordering follows the IFC kernel->shared->domain relationship grouping.
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class IfcRelKind {
    public static readonly IfcRelKind Aggregates             = new("IfcRelAggregates",                 EdgeAxis.Compose,   "RelatingObject",          "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind Nests                  = new("IfcRelNests",                      EdgeAxis.Compose,   "RelatingObject",          "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind ContainedInStructure   = new("IfcRelContainedInSpatialStructure", EdgeAxis.Compose,   "RelatingStructure",       "RelatedElements",       RelCardinality.OneToMany);
    public static readonly IfcRelKind ReferencedInStructure  = new("IfcRelReferencedInSpatialStructure",EdgeAxis.Connect,   "RelatingStructure",       "RelatedElements",       RelCardinality.ManyToMany);
    public static readonly IfcRelKind ConnectsElements       = new("IfcRelConnectsElements",           EdgeAxis.Connect,   "RelatingElement",         "RelatedElement",        RelCardinality.OneToOne);
    public static readonly IfcRelKind ConnectsRealizing      = new("IfcRelConnectsWithRealizingElements",EdgeAxis.Connect,  "RelatingElement",         "RelatedElement",        RelCardinality.OneToOne);
    public static readonly IfcRelKind ConnectsPathElements   = new("IfcRelConnectsPathElements",       EdgeAxis.Connect,   "RelatingElement",         "RelatedElement",        RelCardinality.OneToOne);
    public static readonly IfcRelKind ConnectsPorts          = new("IfcRelConnectsPorts",              EdgeAxis.Connect,   "RelatingPort",            "RelatedPort",           RelCardinality.OneToOne);
    public static readonly IfcRelKind ConnectsPortToElement  = new("IfcRelConnectsPortToElement",      EdgeAxis.Connect,   "RelatingPort",            "RelatedElement",        RelCardinality.OneToOne);
    public static readonly IfcRelKind ConnectsStructMember   = new("IfcRelConnectsStructuralMember",   EdgeAxis.Connect,   "RelatingStructuralMember","RelatedStructuralConnection", RelCardinality.OneToOne);
    public static readonly IfcRelKind ConnectsStructActivity = new("IfcRelConnectsStructuralActivity", EdgeAxis.Connect,   "RelatingElement",         "RelatedStructuralActivity", RelCardinality.OneToOne);
    public static readonly IfcRelKind SpaceBoundary          = new("IfcRelSpaceBoundary",              EdgeAxis.Connect,   "RelatingSpace",           "RelatedBuildingElement", RelCardinality.OneToOne);
    public static readonly IfcRelKind InterferesElements     = new("IfcRelInterferesElements",         EdgeAxis.Connect,   "RelatingElement",         "RelatedElement",        RelCardinality.OneToOne);
    public static readonly IfcRelKind Sequence               = new("IfcRelSequence",                   EdgeAxis.Connect,   "RelatingProcess",         "RelatedProcess",        RelCardinality.OneToOne);
    public static readonly IfcRelKind AssociatesMaterial     = new("IfcRelAssociatesMaterial",         EdgeAxis.Associate, "RelatedObjects",          "RelatingMaterial",      RelCardinality.OneToMany);
    public static readonly IfcRelKind AssociatesClassif      = new("IfcRelAssociatesClassification",   EdgeAxis.Associate, "RelatedObjects",          "RelatingClassification",RelCardinality.OneToMany);
    public static readonly IfcRelKind DefinesByProperties    = new("IfcRelDefinesByProperties",        EdgeAxis.Associate, "RelatedObjects",          "RelatingPropertyDefinition", RelCardinality.OneToMany);
    public static readonly IfcRelKind DefinesByType          = new("IfcRelDefinesByType",              EdgeAxis.Associate, "RelatedObjects",          "RelatingType",          RelCardinality.OneToMany);
    public static readonly IfcRelKind CoversElements         = new("IfcRelCoversBldgElements",         EdgeAxis.Associate, "RelatingBuildingElement", "RelatedCoverings",      RelCardinality.OneToMany);
    public static readonly IfcRelKind CoversSpaces           = new("IfcRelCoversSpaces",               EdgeAxis.Associate, "RelatingSpace",           "RelatedCoverings",      RelCardinality.OneToMany);
    public static readonly IfcRelKind AssignsToGroup         = new("IfcRelAssignsToGroup",             EdgeAxis.Assign,    "RelatingGroup",           "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind AssignsToControl       = new("IfcRelAssignsToControl",           EdgeAxis.Assign,    "RelatingControl",         "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind AssignsToProcess       = new("IfcRelAssignsToProcess",           EdgeAxis.Assign,    "RelatingProcess",         "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind AssignsToProduct       = new("IfcRelAssignsToProduct",           EdgeAxis.Assign,    "RelatingProduct",         "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind AssignsToActor         = new("IfcRelAssignsToActor",             EdgeAxis.Assign,    "RelatingActor",           "RelatedObjects",        RelCardinality.OneToMany);
    public static readonly IfcRelKind Declares               = new("IfcRelDeclares",                   EdgeAxis.Assign,    "RelatingContext",         "RelatedDefinitions",    RelCardinality.OneToMany);
    public static readonly IfcRelKind ServicesBuildings      = new("IfcRelServicesBuildings",          EdgeAxis.Assign,    "RelatingSystem",          "RelatedBuildings",      RelCardinality.OneToMany);
    public static readonly IfcRelKind Voids                  = new("IfcRelVoidsElement",               EdgeAxis.Void,      "RelatingBuildingElement", "RelatedOpeningElement", RelCardinality.OneToOne);
    public static readonly IfcRelKind Fills                  = new("IfcRelFillsElement",               EdgeAxis.Void,      "RelatingOpeningElement",  "RelatedBuildingElement",RelCardinality.OneToOne);

    public EdgeAxis Axis { get; }
    public string Relating { get; }
    public string Related { get; }
    public RelCardinality Cardinality { get; }

    // The neutral edge constructor for this relationship: the axis selects the seam Relationship case, the
    // key is the IFC wire-name preserving directionality + inverse, attrs carries the per-edge payload.
    public Relationship Edge(NodeId relating, NodeId related, Map<PropertyName, PropertyValue> attrs) => Axis switch {
        EdgeAxis.Compose   => new Relationship.Compose(relating, related, Key, attrs),
        EdgeAxis.Assign    => new Relationship.Assign(relating, related, Key, attrs),
        EdgeAxis.Associate => new Relationship.Associate(relating, related, Key, attrs),
        EdgeAxis.Connect   => new Relationship.Connect(relating, related, Key, attrs),
        _                  => new Relationship.Void(relating, related, Key, attrs),
    };

    // The EGRESS inverse of Edge: construct this row's IFC relationship and assign the relating/related sides by the
    // SAME directionality names the ingress Extract reads (Relating/Related), so directionality + inverse round-trip and
    // the eight stranded families re-emit from the neutral payload [C5]. A SET-valued related side (OneToMany/ManyToMany)
    // appends every authored member; a single related side (OneToOne) takes the head. Row-driven: a new IFC relationship
    // is one IfcRelKind row, never a per-class egress arm. BaseClassIfc.Construct mints the entity by IFC class name.
    public Option<IfcRelationship> Author(DatabaseIfc db, IfcProduct relating, Seq<IfcProduct> related) {
        if (related.IsEmpty || BaseClassIfc.Construct(Key, db) is not IfcRelationship rel) {
            return Option<IfcRelationship>.None;
        }
        Type shape = rel.GetType();
        shape.GetProperty(Relating)?.SetValue(rel, relating);
        switch (shape.GetProperty(Related)) {
            case { } slot when slot.GetValue(rel) is System.Collections.IList set: related.Iter(member => set.Add(member)); break;
            case { CanWrite: true } slot:                                          slot.SetValue(rel, related[0]);          break;
        }
        return Some(rel);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class EdgeProjection {
    // One fold over the whole relationship roster: each IfcRel* family extracts its relating/related set,
    // resolves the NodeIds, and constructs the neutral edge through the IfcRelKind row. The one-to-many and
    // many-to-many families fan out one edge per related endpoint, the ordinal riding the attribute payload
    // so the ordered Nests/Aggregates sequence survives. A dangling endpoint faults [DanglingReference].
    public static Fin<Seq<Relationship>> All(IfcProject project, Map<string, NodeId> rooted) {
        var edges = Decomposition(project, rooted)
            .Concat(Associations(project, rooted))
            .Concat(Connections(project, rooted))
            .Concat(Assignments(project, rooted))
            .Concat(DefinesProperties(project, rooted))
            .Concat(Structural(project, rooted))
            .Concat(SpatialBoundaries(project, rooted));
        return edges.Sequence().Map(static rels => rels.Flatten().ToSeq());
    }

    // The property/quantity ATTACHMENT onto neutral Assign edges the seam Bake reads (AssignKind.PropertyDefinition):
    // IfcRelDefinesByProperties binds a PropertySet/ElementQuantity bag node to its occurrence elements, so the seam
    // Bake folds the bag into the element and a Rasm.Compute energy read resolves graph.ConditionedFloorArea off the
    // space's Qto_SpaceBaseQuantities QuantitySet — Subject = the related element, Definition = the bag node.
    static Seq<Fin<Seq<Relationship>>> DefinesProperties(IfcProject project, Map<string, NodeId> rooted) => Seq(
        project.Extract<IfcRelDefinesByProperties>().AsIterable()
            .SelectMany(rel => rel.RelatedObjects.Select(o =>
                from subject in Resolve(rooted, o.GlobalId)
                from definition in Resolve(rooted, (rel.RelatingPropertyDefinition as IfcRoot)?.GlobalId ?? "")
                select (Relationship)new Relationship.Assign(subject, definition, AssignKind.PropertyDefinition)))
            .Sequence().Map(static e => e.ToSeq()));

    // The structural-analysis idealization onto neutral Generic edges [C5]: IfcRelConnectsStructuralMember binds an
    // idealized member to its connection (the connection's 6-DOF restraint riding the edge attrs), IfcRelConnectsStructuralActivity
    // binds a load activity to a structural item (the applied force/moment riding the attrs) — so a Rasm.Compute frame
    // solve reads graph.SupportsOf/graph.LoadsOf off these edges + the member Axis baked on the Object node (Enrich).
    static Seq<Fin<Seq<Relationship>>> Structural(IfcProject project, Map<string, NodeId> rooted) => Seq(
        project.Extract<IfcRelConnectsStructuralMember>().AsIterable().Select(rel =>
            from m in Resolve(rooted, rel.RelatingStructuralMember?.GlobalId ?? "")
            from c in Resolve(rooted, rel.RelatedStructuralConnection?.GlobalId ?? "")
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructMember.Key, m, c, RestraintAttrs(rel.RelatedStructuralConnection)))
            .Sequence().Map(static e => e.ToSeq()),
        project.Extract<IfcRelConnectsStructuralActivity>().AsIterable().Select(rel =>
            from item in Resolve(rooted, (rel.RelatingElement as IfcRoot)?.GlobalId ?? "")
            from act in Resolve(rooted, rel.RelatedStructuralActivity?.GlobalId ?? "")
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructActivity.Key, item, act, LoadAttrs(rel.RelatedStructuralActivity)))
            .Sequence().Map(static e => e.ToSeq()));

    // The energy/spatial space-boundary graph onto neutral Generic edges [C5]: IfcRelSpaceBoundary binds a space to its
    // bounding building element, the physical/virtual + internal/external flags and boundary level riding the attrs and
    // the analytical boundary polygon baked onto the bounding-surface Object node (Enrich) — so a Rasm.Compute OSM build
    // reads graph.SpacesOf/graph.BoundingSurfacesOf and surface.BoundaryPolygon off the baked graph, never re-reading IFC.
    static Seq<Fin<Seq<Relationship>>> SpatialBoundaries(IfcProject project, Map<string, NodeId> rooted) => Seq(
        project.Extract<IfcRelSpaceBoundary>().AsIterable().Select(rel =>
            from s in Resolve(rooted, rel.RelatingSpace?.GlobalId ?? "")
            from e in Resolve(rooted, rel.RelatedBuildingElement?.GlobalId ?? "")
            select (Relationship)new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, s, e, BoundaryAttrs(rel)))
            .Sequence().Map(static e => e.ToSeq()));

    // The structural connection 6-DOF restraint -> neutral Boolean fixity attrs the Rasm.Compute MemberSupport reads:
    // a DOF is fixed when the IfcBoundaryNodeCondition translational/rotational stiffness select is present (rigid),
    // free when absent — the SI stiffness magnitude dropped, the seam carrying the fixity boolean the FE constraint reads.
    static Map<PropertyName, PropertyValue> RestraintAttrs(IfcStructuralConnection? connection) =>
        Optional(connection?.AppliedCondition).Bind(static c => Optional(c as IfcBoundaryNodeCondition)).Match(
            Some: n => Map(
                (PropertyName.Create("TranslationX"), Dof(n.TranslationalStiffnessX)),
                (PropertyName.Create("TranslationY"), Dof(n.TranslationalStiffnessY)),
                (PropertyName.Create("TranslationZ"), Dof(n.TranslationalStiffnessZ)),
                (PropertyName.Create("RotationX"),    Dof(n.RotationalStiffnessX)),
                (PropertyName.Create("RotationY"),    Dof(n.RotationalStiffnessY)),
                (PropertyName.Create("RotationZ"),    Dof(n.RotationalStiffnessZ))),
            None: () => Map<PropertyName, PropertyValue>());

    static PropertyValue Dof(object? stiffness) => new PropertyValue.Boolean(stiffness is not null);

    // The structural activity applied load -> neutral SI force/moment attrs the Rasm.Compute MemberLoad reads: the
    // IfcStructuralLoadSingleForce components and the source load token, an unmapped load yielding an empty bag.
    static Map<PropertyName, PropertyValue> LoadAttrs(IfcStructuralActivity? activity) =>
        Optional(activity?.AppliedLoad).Bind(static l => Optional(l as IfcStructuralLoadSingleForce)).Match(
            Some: f => Map(
                (PropertyName.Create("ForceX"),  Force(f.ForceX)),  (PropertyName.Create("ForceY"),  Force(f.ForceY)),  (PropertyName.Create("ForceZ"),  Force(f.ForceZ)),
                (PropertyName.Create("MomentX"), Force(f.MomentX)), (PropertyName.Create("MomentY"), Force(f.MomentY)), (PropertyName.Create("MomentZ"), Force(f.MomentZ)),
                (PropertyName.Create("Source"),  new PropertyValue.Text(activity?.Name ?? ""))),
            None: () => Map<PropertyName, PropertyValue>());

    static PropertyValue Force(double? si) => new PropertyValue.Measure(MeasureValue.OfSi(Dimension.ForceDim, si ?? 0.0));

    // The space-boundary physical/virtual + internal/external flags and the 1st/2nd-level discriminant -> neutral attrs
    // the Rasm.Compute energy build filters on (an external 2nd-level physical boundary is an envelope surface).
    static Map<PropertyName, PropertyValue> BoundaryAttrs(IfcRelSpaceBoundary rel) => Map(
        (PropertyName.Create("PhysicalOrVirtual"),  new PropertyValue.Text(rel.PhysicalOrVirtualBoundary.ToString())),
        (PropertyName.Create("InternalOrExternal"), new PropertyValue.Text(rel.InternalOrExternalBoundary.ToString())),
        (PropertyName.Create("BoundaryLevel"),      new PropertyValue.Text(rel is IfcRelSpaceBoundary2ndLevel ? "2nd" : "1st")));

    static Seq<Fin<Seq<Relationship>>> Decomposition(IfcProject project, Map<string, NodeId> rooted) => Seq(
        FanOut(project.Extract<IfcRelAggregates>(), IfcRelKind.Aggregates, rooted,
            static r => r.RelatingObject.GlobalId, static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelNests>(), IfcRelKind.Nests, rooted,
            static r => r.RelatingObject.GlobalId, static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelContainedInSpatialStructure>(), IfcRelKind.ContainedInStructure, rooted,
            static r => r.RelatingStructure?.GlobalId ?? "", static r => r.RelatedElements.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelReferencedInSpatialStructure>(), IfcRelKind.ReferencedInStructure, rooted,
            static r => r.RelatingStructure?.GlobalId ?? "", static r => r.RelatedElements.Select(static o => o.GlobalId)),
        Pair(project.Extract<IfcRelVoidsElement>(), IfcRelKind.Voids, rooted,
            static r => r.RelatingBuildingElement?.GlobalId ?? "", static r => r.RelatedOpeningElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelFillsElement>(), IfcRelKind.Fills, rooted,
            static r => r.RelatingOpeningElement?.GlobalId ?? "", static r => r.RelatedBuildingElement?.GlobalId ?? ""));

    static Seq<Fin<Seq<Relationship>>> Associations(IfcProject project, Map<string, NodeId> rooted) => Seq(
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .SelectMany(rel => MaterialEdges(rel, rooted)).Sequence().Map(static e => e.ToSeq()),
        FanOut(project.Extract<IfcRelAssociatesClassification>(), IfcRelKind.AssociatesClassif, rooted,
            static r => (r.RelatingClassification as IfcClassificationReference)?.Identification ?? "",
            static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelDefinesByType>(), IfcRelKind.DefinesByType, rooted,
            static r => r.RelatingType?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelCoversBldgElements>(), IfcRelKind.CoversElements, rooted,
            static r => r.RelatingBuildingElement?.GlobalId ?? "", static r => r.RelatedCoverings.Select(static c => c.GlobalId)),
        FanOut(project.Extract<IfcRelCoversSpaces>(), IfcRelKind.CoversSpaces, rooted,
            static r => r.RelatingSpace?.GlobalId ?? "", static r => r.RelatedCoverings.Select(static c => c.GlobalId)));

    static Seq<Fin<Seq<Relationship>>> Connections(IfcProject project, Map<string, NodeId> rooted) => Seq(
        Pair(project.Extract<IfcRelConnectsElements>().AsIterable().Where(static r => r is not IfcRelConnectsWithRealizingElements),
            IfcRelKind.ConnectsElements, rooted, static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelConnectsWithRealizingElements>(), IfcRelKind.ConnectsRealizing, rooted,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelConnectsPorts>(), IfcRelKind.ConnectsPorts, rooted,
            static r => r.RelatingPort?.GlobalId ?? "", static r => r.RelatedPort?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelConnectsPortToElement>(), IfcRelKind.ConnectsPortToElement, rooted,
            static r => r.RelatingPort?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelInterferesElements>(), IfcRelKind.InterferesElements, rooted,
            static r => r.RelatingElement?.GlobalId ?? "", static r => r.RelatedElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelSequence>(), IfcRelKind.Sequence, rooted,
            static r => r.RelatingProcess?.GlobalId ?? "", static r => r.RelatedProcess?.GlobalId ?? ""));

    static Seq<Fin<Seq<Relationship>>> Assignments(IfcProject project, Map<string, NodeId> rooted) => Seq(
        FanOut(project.Extract<IfcRelAssignsToGroup>(), IfcRelKind.AssignsToGroup, rooted,
            static r => r.RelatingGroup?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToControl>(), IfcRelKind.AssignsToControl, rooted,
            static r => (r.RelatingControl as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToProcess>(), IfcRelKind.AssignsToProcess, rooted,
            static r => (r.RelatingProcess as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToProduct>(), IfcRelKind.AssignsToProduct, rooted,
            static r => (r.RelatingProduct as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelAssignsToActor>(), IfcRelKind.AssignsToActor, rooted,
            static r => (r.RelatingActor as IfcRoot)?.GlobalId ?? "", static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelDeclares>(), IfcRelKind.Declares, rooted,
            static r => r.RelatingContext?.GlobalId ?? "", static r => r.RelatedDefinitions.Select(static o => o.GlobalId)));

    // The material Associate edge threads the occurrence-usage payload [C7]: the LayerSetUsage/ProfileSetUsage
    // direction/sense/offset/cardinal-point fold onto neutral PropertyValue attribute entries, so the type-level
    // composition Set stays on the Material node and the occurrence binding rides the edge.
    // The element->material Associate edge in the seam's typed shape (Subject = the element Object, Resource = the
    // material node, Usage = the typed seam MaterialUsage [C7]) — the directionality the seam LegalLink and the Bake
    // both read (Subject is the Object, Resource the Material), so the type-level MaterialComposition Set stays on the
    // Material node and the per-occurrence layer/profile usage rides the edge as MaterialUsage, never a Map attr bag.
    static IEnumerable<Fin<Relationship>> MaterialEdges(IfcRelAssociatesMaterial rel, Map<string, NodeId> rooted) =>
        rel.RelatedObjects.Select(o => Resolve(rooted, o.GlobalId)
            .Bind(element => Material(rel.RelatingMaterial as BaseClassIfc, rooted)
                .Map(material => (Relationship)new Relationship.Associate(element, material, UsageOf(rel.RelatingMaterial)))));

    static Fin<NodeId> Material(BaseClassIfc? select, Map<string, NodeId> rooted) =>
        MaterialProjection.Project(select).Map(static m => Node.Material.Content(m).Id);

    // The IFC occurrence material usage -> the seam's typed MaterialUsage [C7]: an IfcMaterialLayerSetUsage lowers to
    // MaterialUsage.LayerSet (direction/sense/offset), an IfcMaterialProfileSetUsage to MaterialUsage.ProfileSet
    // (cardinal-point/extent), a type-level set with no occurrence usage to MaterialUsage.None.
    static MaterialUsage UsageOf(IfcMaterialSelect select) => select switch {
        IfcMaterialLayerSetUsage u => new MaterialUsage.LayerSet(
            u.LayerSetDirection switch {
                IfcLayerSetDirectionEnum.AXIS1 => LayerSetDirection.Axis1,
                IfcLayerSetDirectionEnum.AXIS2 => LayerSetDirection.Axis2,
                _                              => LayerSetDirection.Axis3,
            },
            u.DirectionSense == IfcDirectionSenseEnum.POSITIVE ? DirectionSense.Positive : DirectionSense.Negative,
            u.OffsetFromReferenceLine),
        IfcMaterialProfileSetUsage u => new MaterialUsage.ProfileSet((int)u.CardinalPoint, u.ReferenceExtent),
        _ => new MaterialUsage.None(),
    };

    static Fin<Seq<Relationship>> FanOut<TRel>(IEnumerable<TRel> rels, IfcRelKind kind, Map<string, NodeId> rooted,
        Func<TRel, string> relating, Func<TRel, IEnumerable<string>> related) =>
        rels.SelectMany(rel => related(rel).Select((id, ordinal) =>
                from r in Resolve(rooted, relating(rel))
                from e in Resolve(rooted, id)
                select kind.Edge(r, e, Map((new PropertyName("Ordinal"), PropertyValue.Of(ordinal.ToString(), "IfcInteger"))))))
            .Sequence().Map(static e => e.ToSeq());

    static Fin<Seq<Relationship>> Pair<TRel>(IEnumerable<TRel> rels, IfcRelKind kind, Map<string, NodeId> rooted,
        Func<TRel, string> relating, Func<TRel, string> related) =>
        rels.Select(rel =>
                from r in Resolve(rooted, relating(rel))
                from e in Resolve(rooted, related(rel))
                select kind.Edge(r, e, Map<PropertyName, PropertyValue>()))
            .Sequence().Map(static e => e.ToSeq());

    static Fin<NodeId> Resolve(Map<string, NodeId> rooted, string globalId) =>
        rooted.Find(globalId).ToFin(new BimFault.DanglingReference($"edge-endpoint-miss:{globalId}").ToError());
}
```

## [04]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`→IFC bytes re-author — NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern; it resolves the target schema from `Header.ReleaseVersion`, constructs the `DatabaseIfc`, runs the `Model/elements#ELEMENT_MODEL` `PredefinedType` egress gate per object (publishing a `NodeId`→`IfcProduct` map), re-authors the entity graph — the `ReauthorMaterials` material subgraph (`Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per seam `Material` node + `Associate` usage edge [C7]), the `ReauthorClassifications` element classification (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and the neutral-edge `ReauthorRelationships` — and serializes; `SchemaSniff` the ingress counterpart reading `FILE_SCHEMA` (STEP) or `schema_identifier` (JSON) off the bytes BEFORE the database is constructed so the schema is never hardcoded [H8].
- Entry: `SemanticProjector.Emit(ElementGraph graph, FormatIfcSerialization format, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles)` re-authors the graph into IFC text — for each `Object` node it resolves the `IfcClass` row from the generic `Classification` code, runs `IfcClass.AdmitPredefined(token, objectType, graph.Header.Release)` validating the predefined token against the row's frozen valid set AND the schema span → `BimFault.UnmappedClass` [C6], constructs the entity at the resolved schema, assigns the `ExternalId` `GlobalId` (or mints one through `ParserIfc.EncodeGuid` for a from-scratch node) [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the `prior` snapshot Persistence supplies, matching the rooted node on its stable `ExternalId` GlobalId across re-ingest [H9] — publishing the `NodeId`→`IfcProduct` map `ReauthorMaterials`/`ReauthorClassifications` bind against; `ReauthorMaterials` authors each seam `Material` node's type-level definition + `MaterialPropertySet` Psets ONCE and the per-occurrence `MaterialUsage` [C7] over each `Associate` edge, the `profiles` resolver reconstituting a `ProfileSet`'s `IfcProfileDef` from the content-addressed STEP store; `Fin<T>` aborts on the gate or an unresolved profile, lowered with `.ToError()`; `SemanticProjector.Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format)` returns the `ReleaseVersion` for the import-rail to seed the database.
- Auto: `Emit` folds the `graph.Nodes` once — the `Object` egress gate resolves the `IfcClass` row, validates the predefined token and the `IntroducedIn`/`RemovedIn` schema span against `Header.Release` (an IFC4.3 infrastructure class targeting an IFC2x3 emit faults `BimFault.UnmappedClass` rather than writing an entity the schema forbids), folds the predefined token into the content hash, and constructs the entity; the `GlobalId` round-trips from `ExternalId` so a re-imported model re-emits its original GUIDs (1:1) [H6]; the `Compose`/`Assign`/`Connect`/`Void` `Relationship` edges re-author their `IfcRel*` entities by reading the wire-name off the edge key and the directionality off the `IfcRelKind` row, so the eight stranded families re-emit exactly as they were read [C5]; the seam `Material` subgraph re-authors through `ReauthorMaterials` (the type-level `IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` + the `IfcMaterialProperties` `Pset_Material*` rows ONCE per `Material` node, the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` over each `Associate` edge `MaterialUsage` [C7], and the `IfcRelAssociatesMaterial` onto the bound element) and the element classification through `ReauthorClassifications` (`IfcRelAssociatesClassification`/`IfcClassificationReference` per standard-system `Object` node) — the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers; the `OwnerHistory` re-emit derives the `ChangeAction` from the structural diff (`Generator.Equals` `Inequalities` over the prior snapshot, the rooted node matched on its stable 1:1 GlobalId `ExternalId` since the neutral `NodeId` is freshly minted each ingest — `ADDED`/`MODIFIED`/`NOCHANGE`) so the IFC owner history reflects the real edit, never a blanket `ADDED` [H9]; the `StepHeader` re-authors `FILE_DESCRIPTION`/`FILE_NAME`/`FILE_SCHEMA` from `Header.Step`.
- Auto: `Sniff` reads the schema off the bytes before `new DatabaseIfc` — the STEP `FILE_SCHEMA(('IFC4X3_ADD2'))` header line or the IFC-JSON `schema_identifier` member — mapping the token onto `ReleaseVersion` through `ReleaseVersionExtensions` and defaulting to `IFC4X3_ADD2` only when the header is absent, so the import never hardcodes 4x3 over a 2x3 file [H8].
- Packages: GeometryGymIFC_Core, Rasm.Element, Generator.Equals, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new emit format is one `FormatIfcSerialization` the `db.ToString` switch already carries; a new schema is one `ReleaseVersion` the `Sniff` map resolves and the span validates; the predefined gate is one `AdmitPredefined` call composed once per object — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract — exposing it on the seam is the named violation; the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the frozen valid set and the schema span) [C6], never a per-call regex and never silent acceptance of an out-of-schema predefined; the `GlobalId` is the node `ExternalId` round-tripped 1:1 [H6] and a freshly-minted GUID on a re-emitted node is the deleted form; the schema is sniffed [H8] and a hardcoded `IFC4X3_ADD2` over a foreign-schema file is the named defect; the `ChangeAction` is diff-derived [H9] and a blanket `ADDED` stamp is the deleted form; the material/composition/property/classification egress reads the seam graph ONLY — the `Material` node + the `Associate` edge `MaterialUsage` and the `Object` node `Classification` — and a `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing into `Emit` is the deleted form (those Materials wires are retired, the egress reading the projected subgraph), the type-level material definition authored ONCE per `Material` node and the per-occurrence usage wrapping it so a usage duplicated onto the type-level set is the deleted form [C7].

```csharp signature
public sealed partial class SemanticProjector {
    // The Bim-internal IFC egress: ElementGraph -> DatabaseIfc -> bytes. The PredefinedType egress gate [C6]
    // and the schema-span validation [H8] run per Object; the GlobalId round-trips 1:1 [H6]; the OwnerHistory
    // ChangeAction is diff-derived against the prior snapshot [H9]. The Object authoring publishes a NodeId->IfcProduct
    // map ReauthorMaterials and ReauthorClassifications read: ReauthorMaterials lowers each seam Material node
    // (Semantics/composition#MATERIAL_COMPOSITION MaterialProjection.AuthorComposition/AuthorUsage) onto the IFC
    // material-definition family + its Psets + the per-occurrence MaterialUsage [C7], and ReauthorClassifications lowers
    // each Object node's standard Classification (Semantics/classification#CLASSIFICATION_AXIS ClassificationSystem.Author)
    // onto IfcRelAssociatesClassification — the seam-graph egress that REPLACES the retired Rasm.Materials
    // MaterialAssignmentWire/MaterialPropertyWire carriers (read off the projected graph, never a Materials wire). The
    // profiles resolver reconstitutes a ProfileSet's IfcProfileDef from the content-addressed STEP store the
    // ProfileRef.ContentKey keys. Never a seam member.
    public Fin<string> Emit(ElementGraph graph, FormatIfcSerialization format, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles) {
        var target = new DatabaseIfc(false, graph.Header.Release) { ModelView = graph.Header.ModelView };
        // Re-ingest correlation [H6]: the neutral rooted NodeId is freshly minted each Project, so a re-imported
        // graph shares NO NodeId with the prior snapshot Persistence supplies — the diff-derived ChangeAction
        // matches a rooted node on the stable 1:1 GlobalId (Node.Object.ExternalId), indexed once here, falling
        // back to the NodeId only for a from-scratch node carrying no ExternalId.
        var priorByExternal = prior.Map(static p => p.Nodes.Values
                .Choose(static n => n is Node.Object o ? o.ExternalId.Map(ext => (Ext: ext, Node: o)) : None)
                .Fold(Map<string, Node.Object>(), static (m, e) => m.AddOrUpdate(e.Ext, e.Node)))
            .IfNone(Map<string, Node.Object>());
        return graph.Nodes.Values.Choose(static node => node is Node.Object obj ? Some(obj) : None)
            .Map(obj => Author(target, obj, graph.Header.Release, prior, priorByExternal).Map(product => (obj.Id, product)))
            .Sequence()
            .Map(static authored => toMap(authored))            // NodeId -> IfcProduct, the material/classification re-author binds against
            .Bind(products => ReauthorMaterials(target, graph, products, profiles)
                .Map(_ => {
                    ReauthorClassifications(target, graph, products);
                    ReauthorRelationships(target, graph, products);
                    ReauthorHeader(target, graph.Header.Step);
                    return target.ToString(format);
                }));
    }

    // The egress gate: resolve the IfcClass row from the generic Classification code, admit the predefined
    // token against the frozen valid set AND the schema span, fault UnmappedClass on a miss [C6][H8], then
    // construct the entity and round-trip the GlobalId [H6] + diff-derived OwnerHistory [H9].
    static Fin<IfcProduct> Author(DatabaseIfc target, Node.Object obj, ReleaseVersion schema, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) =>
        IfcClass.Resolve(obj.Class.Code)
            .Bind(cls => cls.AdmitPredefined(obj.Predefined.Token, obj.Name, schema)
                .Map(predefined => (Class: cls, Predefined: predefined)))
            .Map(row => {
                var entity = (IfcProduct)BaseClassIfc.Construct(row.Class.Key, target);
                entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(Guid.NewGuid()));
                entity.Name = obj.Name;
                obj.Owner.Iter(owner => entity.OwnerHistory = OwnerHistoryOf(target, owner, ChangeOf(obj, prior, priorByExternal)));
                return entity;
            });

    // The ChangeAction is the diff verdict against the prior snapshot through Generator.Equals member-level
    // Inequalities, never a blanket stamp [H9]: a rooted node matches the prior on the stable 1:1 GlobalId
    // (ExternalId) ACROSS re-ingest since the NodeId is freshly minted each ingest, falling back to the NodeId
    // only for a from-scratch node — absent prior -> ADDED, no member delta -> NOCHANGE, else MODIFIED.
    static IfcChangeActionEnum ChangeOf(Node.Object obj, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) =>
        (obj.ExternalId.Bind(ext => priorByExternal.Find(ext)).Map(static o => (Node)o) is { IsSome: true } byExternal
            ? byExternal
            : prior.Bind(graph => graph.Nodes.Find(obj.Id))).Match(
            None: () => IfcChangeActionEnum.ADDED,
            Some: before => Node.EqualityComparer.Default.Inequalities(before, obj).Any()
                ? IfcChangeActionEnum.MODIFIED
                : IfcChangeActionEnum.NOCHANGE);

    static IfcOwnerHistory OwnerHistoryOf(DatabaseIfc target, OwnerHistory owner, IfcChangeActionEnum change) {
        var history = target.Factory.OwnerHistoryAdded;
        history.ChangeAction = change;
        return history;
    }

    // The schema sniff [H8]: read FILE_SCHEMA / schema_identifier off the bytes before constructing the
    // database, mapping the token onto ReleaseVersion; default IFC4X3_ADD2 only when the header is absent.
    public static ReleaseVersion Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format) =>
        format == InterchangeFormat.IfcJson
            ? SchemaToken((JsonNode.Parse(bytes.Span) as JsonObject)?["schema_identifier"]?.ToString() ?? "")
            : SchemaToken(StepSchemaLine(bytes.Span));

    static ReleaseVersion SchemaToken(string token) =>
        token.Contains("IFC2X3", StringComparison.OrdinalIgnoreCase) ? ReleaseVersion.IFC2x3
        : token.Contains("IFC4X3", StringComparison.OrdinalIgnoreCase) ? ReleaseVersion.IFC4X3_ADD2
        : token.Contains("IFC4", StringComparison.OrdinalIgnoreCase) ? ReleaseVersion.IFC4A2
        : ReleaseVersion.IFC4X3_ADD2;

    static string StepSchemaLine(ReadOnlySpan<byte> bytes) {
        string header = System.Text.Encoding.ASCII.GetString(bytes[..Math.Min(bytes.Length, 4096)]);
        int start = header.IndexOf("FILE_SCHEMA", StringComparison.Ordinal);
        return start < 0 ? "" : header[start..Math.Min(header.Length, start + 64)];
    }

    // The seam Material subgraph -> IFC: each Material node authors its type-level definition + Psets ONCE through
    // Semantics/composition#MATERIAL_COMPOSITION MaterialProjection.AuthorComposition, then each incident Associate
    // edge authors the per-occurrence MaterialUsage [C7] wrapping the shared definition (AuthorUsage) and the
    // IfcRelAssociatesMaterial onto the bound element — so a wall and its mirror share one IfcMaterialLayerSet with
    // two IfcMaterialLayerSetUsage instances. This is the seam-graph egress that REPLACES the retired Rasm.Materials
    // MaterialAssignmentWire/MaterialPropertyWire carriers; the material reads off the projected graph, never a wire.
    static Fin<Unit> ReauthorMaterials(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products, Func<ProfileRef, Option<IfcProfileDef>> profiles) =>
        graph.Nodes.Values.Choose(static n => n is Node.Material m ? Some(m) : None)
            .Map(material => MaterialProjection.AuthorComposition(target, material, profiles).Map(definition => {
                graph.EdgesAt(material.Id)
                    .Choose(e => e is Relationship.Associate a && a.Resource == material.Id ? Some(a) : None)
                    .Iter(edge => products.Find(edge.Subject).IfSome(product => {
                        _ = new IfcRelAssociatesMaterial(MaterialProjection.AuthorUsage(definition, edge.Usage), Seq1((IfcDefinitionSelect)product));
                    }));
                return unit;
            }))
            .Sequence().Map(static _ => unit);

    // The element standard classification -> IFC: each Object node whose generic Classification is a standard system
    // authors the IfcRelAssociatesClassification/IfcClassificationReference through Semantics/classification#CLASSIFICATION_AXIS
    // ClassificationSystem.Author (None for the "ifc" entity-type code the Author above already resolved as the IfcClass).
    // This is the element-classification egress subsuming the retired material-wire MaterialPropertyWire.Classification half.
    static void ReauthorClassifications(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) =>
        graph.Nodes.Values.Choose(static n => n is Node.Object o ? Some(o) : None)
            .Iter(obj => products.Find(obj.Id).IfSome(product => {
                _ = ClassificationSystem.Author(target, (IfcDefinitionSelect)product, obj.Classification);
            }));

    // The neutral edge algebra -> IfcRel*: each Compose/Assign/Connect/Void edge re-authors its IFC relationship by the
    // IfcRelKind row wire-name + directionality, the eight stranded families re-emitting from the neutral Generic payload
    // [C5]; the Associate material/classification edges are authored by ReauthorMaterials/ReauthorClassifications, so they
    // resolve to None here and are skipped. Each edge re-authors per (relating, related) pair against the authored product
    // map — a OneToMany family thus re-emits one IfcRel* per part (denormalized but lossless), the row driving the rest.
    static void ReauthorRelationships(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) =>
        graph.Edges.AsIterable().Iter(edge => RelKindOf(edge).IfSome(kind =>
            products.Find(edge.Relating).IfSome(relating =>
                products.Find(edge.Related).IfSome(related =>
                    kind.Author(target, relating, Seq1(related))))));

    // The neutral seam edge -> its IfcRelKind row: the neutral sub-kind selects the row for the typed cases (the exact
    // inverse of IfcRelKind.Edge's axis lowering), the Generic passthrough carries the original IFC wire-name as its row
    // key, and an Associate edge returns None because the material/classification egress owns it. An unrostered Generic
    // wire-name resolves to None rather than re-authoring an entity the roster never declared.
    static Option<IfcRelKind> RelKindOf(Relationship edge) => edge switch {
        Relationship.Compose c => Some(c.SubKind == ComposeKind.Aggregate ? IfcRelKind.Aggregates
                                     : c.SubKind == ComposeKind.Nest      ? IfcRelKind.Nests
                                     : c.SubKind == ComposeKind.Contain   ? IfcRelKind.ContainedInStructure
                                     :                                      IfcRelKind.ReferencedInStructure),
        Relationship.Connect c => Some(c.SubKind == ConnectKind.Port      ? IfcRelKind.ConnectsPorts
                                     : c.SubKind == ConnectKind.Realizing ? IfcRelKind.ConnectsRealizing
                                     :                                      IfcRelKind.ConnectsElements),
        Relationship.Void v    => Some(v.SubKind == VoidKind.Fill ? IfcRelKind.Fills : IfcRelKind.Voids),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition => Some(IfcRelKind.DefinesByType),
        Relationship.Assign a when a.SubKind == AssignKind.Group          => Some(IfcRelKind.AssignsToGroup),
        Relationship.Generic g => IfcRelKind.TryGet(g.WireName, out IfcRelKind? row) && row is { } resolved ? Some(resolved) : Option<IfcRelKind>.None,
        _                      => Option<IfcRelKind>.None,
    };

    // The StepHeader -> the STEP physical-file header on the database: FILE_DESCRIPTION (FileDescriptions) and the
    // FILE_NAME fields (name/time-stamp/author/organization/preprocessor/originating-system) restored from the seam
    // header [H9], so an import -> export cycle preserves provenance instead of stripping it. FILE_SCHEMA already rides
    // target.Release (set at the DatabaseIfc construction from Header.Release in Emit), so the schema is restored there.
    static void ReauthorHeader(DatabaseIfc target, StepHeader header) {
        STEPFileInformation info = target.OriginatingFileInformation;
        info.FileDescriptions = header.Descriptions.ToList();
        info.FileName = header.Name;
        info.TimeStamp = header.TimeStamp.ToDateTimeUtc();
        info.Author = header.Authors.ToList();
        info.Organization = header.Organizations.ToList();
        info.PreProcessorVersion = header.Preprocessor;
        info.OriginatingSystem = header.OriginatingSystem;
    }
}
```

## [05]-[GRAPH_LEGALITY]

- Owner: `IfcLegality` the `IGraphConstraint` deciding IFC-semantic legality the seam's structural `GraphDelta` switch cannot [M3] — the seam enforces only structural invariants (an edge endpoint resolves, a node id is unique), and IFC legality (which entity may relate to which) is Bim's, depended UP on through the `IGraphConstraint` contract.
- Entry: `IfcLegality.Validate(GraphDelta delta, ElementGraph graph) → Validation<Error,Unit>` accumulates every IFC-legality violation in the delta against the graph — `Success(unit)` when every rule holds, a `Fail` carrying the accumulated `Error` set otherwise; the validation is applicative (every violation reported at once, never short-circuit) so an authoring pass sees all rejects in one apply.
- Auto: `Validate` reads the delta's `AddedEdges` and applies the closed legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no `IfcRel*` case) — a `Compose` edge with the `Contain` sub-kind requires its `Whole` to be a spatial `Object` (`IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace`), a `Compose` edge with the `Aggregate` sub-kind may not have a `Type` object as its `Whole` (a type may not aggregate an occurrence), a `Void` edge requires its `Feature` to be an opening, an `Assign` edge with the `TypeDefinition` sub-kind requires its `Definition` to be a `Type` object; each rule lowers its failure onto `BimFault.ModelRejected` through `.ToError()` and the applicative `Validation` accumulates them.
- Packages: Rasm.Element, GeometryGymIFC_Core, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new IFC-legality rule is one arm on the `Rule` switch the `Validate` fold applies; the structural invariants stay the seam's `GraphDelta` switch and never migrate here; never a per-rule validator type.
- Boundary: `IfcLegality` decides IFC-semantic legality ONLY — the structural invariants (endpoint resolution, id uniqueness) are the seam's `GraphDelta` total switch and re-checking them here is the deleted form [M3]; the rules read the generic `Classification` code (`IfcSite`, `IfcOpeningElement`) and the `ObjectKind` (occurrence/type), never an `IfcProduct` runtime type (GeometryGym stays captured in the projector); the validation is applicative-accumulating so an authoring pass sees every reject at once, never the first-fail short-circuit a `Fin` rail would give.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class IfcLegality : IGraphConstraint {
    static readonly FrozenSet<string> Spatial = new[] { "IfcSite", "IfcBuilding", "IfcBuildingStorey", "IfcSpace", "IfcFacility", "IfcFacilityPart" }.ToFrozenSet(StringComparer.Ordinal);

    // The delta's ADDED edges are the projection's new relationships; each is validated against the running graph and
    // the applicative Validation accumulates every IFC-legality violation at once (never a first-fail short-circuit).
    public Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph) =>
        delta.AddedEdges.Map(edge => Rule(edge, graph)).Fold(
            Success<Error, Unit>(unit),
            static (acc, rule) => (acc, rule).Apply(static (_, _) => unit).As());

    // The closed IFC-legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no IfcRel*
    // case, so the rule reads the neutral Compose/Void/Assign shape, never an IFC wire-name): containment-whole-must-be-
    // spatial, a type may not aggregate, Voids feature->opening, DefinesByType definition-must-be-type.
    static Validation<Error, Unit> Rule(Relationship edge, ElementGraph graph) => edge switch {
        Relationship.Compose c when c.SubKind == ComposeKind.Contain =>
            RequireClass(c.Whole, graph, Spatial.Contains, $"containment-whole-not-spatial:{c.Whole.Value}"),
        Relationship.Compose c when c.SubKind == ComposeKind.Aggregate =>
            RequireKind(c.Whole, graph, static kind => kind == ObjectKind.Occurrence, $"type-aggregates-occurrence:{c.Whole.Value}"),
        Relationship.Void v =>
            RequireClass(v.Feature, graph, static code => code is "IfcOpeningElement", $"voids-feature-not-opening:{v.Feature.Value}"),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition =>
            RequireKind(a.Definition, graph, static kind => kind == ObjectKind.Type, $"definesbytype-definition-not-type:{a.Definition.Value}"),
        _ => Success<Error, Unit>(unit),
    };

    static Validation<Error, Unit> RequireClass(NodeId id, ElementGraph graph, Func<string, bool> ok, string detail) =>
        graph.Find<Node.Object>(id).Filter(obj => ok(obj.Classification.Code)).Match(
            Some: _ => Success<Error, Unit>(unit),
            None: () => Fail<Error, Unit>(new BimFault.ModelRejected(detail).ToError()));

    static Validation<Error, Unit> RequireKind(NodeId id, ElementGraph graph, Func<ObjectKind, bool> ok, string detail) =>
        graph.Find<Node.Object>(id).Filter(obj => ok(obj.Kind)).Match(
            Some: _ => Success<Error, Unit>(unit),
            None: () => Fail<Error, Unit>(new BimFault.ModelRejected(detail).ToError()));
}
```

## [06]-[RESEARCH]

- [PROJECTION_INVERSION]: the `SemanticProjector : IElementProjection` GeometryGym-internal capture and the IoC inversion (GeometryGym stays SOLE in Bim; Bim implements the seam interface so no GeometryGym edge points down into the seam) ground against `ELEMENT-REBUILD-PLAN.md` §4A/§4C and the `Rasm.Element/Projection/projection#PROJECTION` `IElementProjection.Project(ProjectionContext) → Fin<GraphDelta>` contract + `Assemble` fold; the `Project` reads the live `db.Project.Extract<T>()` graph (not the lossy import-rail rows) so the full relationship roster + `OwnerHistory` + `StepHeader` survive, the member spellings (`IfcRoot.GlobalId`/`OwnerHistory`, `IfcProduct`, `IfcTypeProduct.RepresentationMaps`, `IfcRelAssignsToGroup.RelatingGroup`/`RelatedObjects`, `IfcRelDeclares.RelatingContext`/`RelatedDefinitions`, `IfcRelCoversBldgElements`/`IfcRelCoversSpaces`, `IfcRelConnectsPathElements`, `IfcRelSpaceBoundary`, `IfcRelInterferesElements`, `IfcRelAssignsToControl`/`Process`/`Product`/`Actor`) confirmed against `.api/api-geometrygym-ifc` relationship + grouping + structural-connection families.
- [RELATION_NEUTRALITY]: the neutral five-case `Relationship` algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void`) plus `Generic` passthrough grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C5 — the seam carries NO typed `IfcRel*` case (re-opening the IFC schema strata leak the Classification critique fixed), so all directionality/inverse and the eight stranded families ride the Bim-side `IfcRelKind` roster and the neutral edge attribute payload; the material occurrence-usage `LayerSetUsage`/`ProfileSetUsage` rides the `Associate` edge payload [C7], the type-level `MaterialComposition` Set staying on the `Material` node per `Semantics/composition#MATERIAL_COMPOSITION`.
- [EGRESS_GATE]: the `PredefinedType` egress gate (resolve the `IfcClass` row from `code`, run `AdmitPredefined` against the frozen valid set + the `IntroducedIn`/`RemovedIn` schema span → `BimFault.UnmappedClass`, fold the token into the content hash) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C6/H8 and the `Model/elements#ELEMENT_MODEL` `IfcClass`/`PredefinedType` owner; the `FILE_SCHEMA`/`schema_identifier` sniff before `new DatabaseIfc` grounds against `.api/api-geometrygym-ifc` `[AP242_STEP_READ]` schema-auto-resolve and `ParserIfc.EncodeGuid`/`DecodeGlobalID` for the 1:1 `GlobalId` round-trip [H6]; the diff-derived `ChangeAction` grounds against `Generator.Equals` `Inequalities` (`.api/api-generator-equals`) and the seam `OwnerHistory` value-object [H9].
- [MATERIAL_CLASSIFICATION_EGRESS]: the `ReauthorMaterials`/`ReauthorClassifications` egress grounds against `ELEMENT-REBUILD-PLAN.md` §6 (`Rasm.Materials` ripple — the `MaterialProjector` lowers the material subgraph onto the seam graph, `Rasm.Bim` reads it; the retired `MaterialAssignment`/value half is seam-owned) and §4-RT C7 (the `Associate` material edge carries the typed `MaterialUsage`; the type-level `MaterialComposition` set stays, usage is the occurrence binding) — so the material/composition/property/classification egress reads the projected seam `Material` node + the `Associate` edge `MaterialUsage` + the `Object` node `Classification`, REPLACING the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers (the `Semantics/connection#CONNECTION_WIRE` `ConnectionWire.AuthorMaterial` consumer that read them is dropped, the `ConnectionItemWire` connection-item egress remaining until a future Connection `IElementProjection` subsumes it). The egress GeometryGym surface (`IfcMaterialLayerSet(IEnumerable<IfcMaterialLayer>, string)`, `IfcMaterialLayerSetUsage(IfcMaterialLayerSet, IfcLayerSetDirectionEnum, IfcDirectionSenseEnum, double)`, `IfcMaterialProfileSet(string, IfcMaterialProfile)`, `IfcMaterialProfileSetUsage(IfcMaterialProfileSet, IfcCardinalPointReference)`, `IfcMaterialConstituentSet(string, IEnumerable<IfcMaterialConstituent>)`, `IfcMaterialProperties(string, IfcMaterialDefinition)`, `IfcRelAssociatesMaterial(IfcMaterialSelect, IEnumerable<IfcDefinitionSelect>)`, `IfcRelAssociatesClassification(IfcClassificationSelect, IfcDefinitionSelect)`) is decompile-verified against the live GeometryGym 25.7.30 surface (`.api/api-geometrygym-ifc` material + classification families), and the actual authoring lives in the `Semantics/composition#MATERIAL_COMPOSITION` and `Semantics/classification#CLASSIFICATION_AXIS` owners this `Emit` composes (co-located with the ingress projector, never re-implemented here).
- [LEGALITY_SPLIT]: the `IfcLegality : IGraphConstraint` IFC-semantic legality (containment-relating-must-be-spatial / `Voids` element→opening / type-may-not-aggregate-occurrence / `DefinesByType` relating-type) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M3 — net-new Rasm interfaces = 2 (`IElementProjection` + `IGraphConstraint`); the seam's structural `GraphDelta` switch enforces only endpoint-resolution and id-uniqueness, the IFC legality depended up on through the constraint contract and accumulated applicatively over `Validation<Error,Unit>`.
- [STRUCTURAL_ENERGY_LOWERING]: the `DefinesProperties`/`Structural`/`SpatialBoundaries` `EdgeProjection` folds plus the `Enrich` analytical bake lower the structural idealization and the energy/spatial model onto the seam graph so `Rasm.Compute` reads everything baked (`ELEMENT-REBUILD-PLAN.md` §4E "Analysis reads the concrete `ElementGraph` directly"). The structural-analysis family (`IfcStructuralCurveMember`/`IfcStructuralConnection`/`IfcStructuralActivity`, `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity`, `IfcStructuralConnection.AppliedCondition`→`IfcBoundaryNodeCondition`, `IfcStructuralActivity.AppliedLoad`→`IfcStructuralLoadSingleForce`) and the space-boundary family (`IfcRelSpaceBoundary.RelatingSpace`/`RelatedBuildingElement`/`PhysicalOrVirtualBoundary`/`InternalOrExternalBoundary`, `IfcRelSpaceBoundary2ndLevel`) ground against `.api/api-geometrygym-ifc` (structural-connection rows + `IfcRelConnectsElements.ConnectionGeometry`); the idealized member `Axis` (`IfcStructuralCurveMember` analytical curve) and the bounding-surface `BoundaryPolygon` (`IfcRelSpaceBoundary.ConnectionGeometry`) lower through the Bim-internal `IfcRepresentation.AxisOf`/`BoundaryOf` geometry readers (siblings to `IfcRepresentation.KeysOf`/`MapKeys`) to kernel `Vector3` baked on the seam `Graph/element#NODE_MODEL` `Node.Object.Axis`/`BoundaryPolygon` [the residual's "everything baked"]. Ripple counterpart: `Rasm.Element/Graph/element` (the `Node.Object.Axis`/`BoundaryPolygon` analytical carriers + `AxisCurve`), `Rasm.Compute/Analysis/structural` (the `graph.AxisOf`/`SupportsOf`/`LoadsOf` reads over the `Generic` structural edges + baked `Axis`), and `Rasm.Compute/Analysis/energy` (the `graph.SpacesOf`/`BoundingSurfacesOf`/`ConditionedFloorArea` reads over the `Generic` space-boundary edges + baked `BoundaryPolygon`). The seam carries the structural/space-boundary connectivity on the NEUTRAL `Relationship.Generic` wire-name payload [C5], never a typed IFC relationship case, so the strata stay IFC-schema-free.
