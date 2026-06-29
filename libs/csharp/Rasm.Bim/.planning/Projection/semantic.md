# [BIM_SEMANTIC_PROJECTOR]

`Rasm.Bim` is the SOLE GeometryGym/IFC owner and the IFC arm of the `Rasm.Element` seam. This page owns the one `SemanticProjector : IElementProjection` that lowers a live GeometryGym `DatabaseIfc` into a seam `GraphDelta` (the `Project` ingress) and re-authors a seam `ElementGraph` back into IFC STEP/XML/JSON bytes (the `Emit` egress, a Bim-INTERNAL operation, never a seam member), plus the `IfcLegality : IGraphConstraint` that decides IFC-semantic legality the seam's structural `GraphDelta` switch cannot. The projector replaces the retired `BimModel.Project`/`BimElement` fold: where the old owner produced a second stored element record keyed by GlobalId, the projector produces seam `Node`s (`Object` occurrence/type, `PropertySet`, `QuantitySet`, `Material`) and neutral `Relationship` edges that `Assemble` folds into the canonical `ElementGraph`, so "has it all" is one `Bake` read on the seam graph and GeometryGym never leaks below the seam. The whole IFC relationship vocabulary — every `IfcRel*` name, its directionality, and its inverse — plus the eight families the prior owner stranded ride the NEUTRAL edge algebra [C5], so no relationship is dropped and the seam stays IFC-schema-free. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the kernel geometry by content-hash reference, never a RhinoCommon type, never an in-process BRep evaluation.

The two `ReleaseVersion`/`ModelView` worlds meet HERE and nowhere else: the seam `ReleaseVersion`/`ModelView` `[SmartEnum]`s are the model `Header` currency, and the GeometryGym `ReleaseVersion`/`ModelView` enums are the IFC-text codec leg `Project`/`Emit`/`Sniff` own — the page aliases the GeometryGym pair (`GGRelease`/`GGView`) so the unqualified names resolve to the seam, and `ReleaseLower`/`ReleaseRaise` are the one lowering pair the leak never escapes.

## [01]-[INDEX]

- [01]-[SEMANTIC_PROJECTOR]: `SemanticProjector : IElementProjection`, the `Project` fold lowering `DatabaseIfc` into a `GraphDelta` — rooted `NodeId` mint with the 1:1 IFC `GlobalId` projection attribute [H6], the `Object` occurrence/type nodes carrying the generic `Classification`/`PredefinedType`/`RepresentationContentHash`, the `PropertySet`/`QuantitySet` bag nodes whose typed `PropertyValue`/`MeasureValue` the `PropertyLowering` narrowing fills and whose `InheritanceMode` is stamped at ingest [H1], the `OwnerHistory`/`StepHeader` projection [H9], and the schema span [H8].
- [02]-[RELATION_ALGEBRA]: `IfcRelKind` the full `IfcRel*` roster `[SmartEnum<string>]` (the neutral `EdgeAxis`+`SubKind` it lowers onto, plus the relating/related IFC inverse-attribute names directionality round-trips on), and the `EdgeProjection` fold lowering every relationship onto a neutral `Relationship` edge — the FanOut/Pair generic families, the inverted `Assign` arms (`DefinesByType`/`AssignsToGroup`), the realizing `Connect`, the `DefinesProperties` property/quantity attachment, the `Structural` member↔connection/member↔activity `Generic` edges (the `StructuralProjection.Attrs` 6-DOF restraint + full load family + `LoadKind`/`Case` payload and the `AtStart` discriminant, the axis baked by the `Enrich` pass), the `SpatialBoundaries` space↔surface `Generic` edges, and the `MaterialEdges` `Associate` material edge carrying the occurrence-usage payload [C7].
- [03]-[IFC_EGRESS]: `SemanticProjector.Emit` the Bim-internal `ElementGraph` → IFC bytes re-author — the `ReleaseRaise` schema target, the `PredefinedType` egress gate over the frozen valid set and schema span → `BimFault.UnmappedClass` [C6], the `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` `ChangeAction` re-stamp [H9], `ReauthorMaterials` (`MaterialProjection.AuthorComposition`/`AuthorUsage` per node + `Associate` usage [C7]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` + `IfcRelDefinesByProperties` round-trip), `ReauthorClassifications` (`ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the neutral edge → `IfcRel*` row-driven re-author) — the seam-graph egress that REPLACES the retired `Rasm.Materials` material wires, plus the `FILE_SCHEMA`/`schema_identifier` `Sniff` [H8].
- [04]-[GRAPH_LEGALITY]: `IfcLegality : IGraphConstraint` the IFC-semantic legality validator — containment-relating-must-be-spatial, `Void` element→opening, type-may-not-aggregate-occurrence — accumulating onto `Validation<Error,Unit>` over the seam's structural invariants [M3].

## [02]-[SEMANTIC_PROJECTOR]

- Owner: `SemanticProjector` the `IElementProjection` capturing one live GeometryGym `DatabaseIfc` internally and lowering it to a seam `GraphDelta` in `Project`; `PropertyLowering` the Bim-internal value-narrowing the seam delegates to it (the seam forbids an IFC `IfcValue`/dataType crossing its signature, so the `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing is Bim's); `OwnerStamp` the `IfcOwnerHistory`→seam `OwnerHistory` projection; `StepHeaderOf` the `STEPFileInformation`→seam `StepHeader` projection; `ReleaseLower`/`ViewLower` the GeometryGym→seam currency lowering.
- Entry: `SemanticProjector.Project(ProjectionContext ctx)` folds the captured `DatabaseIfc` into one `GraphDelta` over `ctx.Key` — it mints a NEUTRAL rooted `NodeId` per `IfcRoot` through `ctx.Rooted()` (the kernel `IObjectFactory` floor), records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute [H6], and content-keys every non-rooted material node through `MaterialProjection.Project`'s kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`; `Fin<T>` aborts on a missing `IfcProject` root or a dangling spatial host (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`), the ingress class lookup PERMISSIVE — an unrostered/IFC4-new leaf lands the `Model/elements#IFC_CLASS` `IfcClass.Proxy` row through `TryGet().IfNone(Proxy)` so one unknown entity never aborts the import, class validity deferred to the `Emit` egress gate [C6][H8] — the fault lifting BARE (the band IS the `Expected` `Code`, no `.ToError()` hop). The element identity is established HERE (the IFC is the source of element identity), so the projector ignores `ctx.ElementIds` (the aspect-projector NodeId set) and PUBLISHES the minted ids in the delta for `Rasm.Materials/Projection/material#MATERIAL_PROJECTOR` to attach `Associate` edges against.
- Auto: `Project` walks the captured `db.Project` once — `ObjectNode` lands every `IfcProduct`→`Object.Occurrence` and `IfcTypeObject`→`Object.Type` node carrying the generic `Classification("ifc", classKey)` (the IFC entity type as a classification, never `IfcClass` on the node) resolved through the permissive `IfcClass.TryGet().IfNone(Proxy)` ingress, the `PredefinedType` token read off the entity's per-class predefined property, the keyed `RepresentationContentHash` map (`Model/elements#REPRESENTATION_KEYS` `IfcRepresentation.Keys`, ONE polymorphic content-keyer over `IfcObjectDefinition`) [M2], the `OwnerStamp` `OwnerHistory` [H9], and the `IfcClass.Span` schema window [H8]; `Bags` lands `IfcPropertySet`/`IfcElementQuantity`→`PropertySet`/`QuantitySet` bag nodes whose typed values the `PropertyLowering` narrowing fills and whose `Semantics/properties#PROPERTY_TEMPLATES` `PropertyInheritance.ModeOf` `InheritanceMode` is stamped at ingest [H1] so the seam `Bake` applies type→occurrence precedence wholly within the seam; `Materials` lands `Material` nodes through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.Project`; the `Enrich` pass bakes the idealized `AxisCurve` onto each structural curve member's `Object.Axis` off `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection.AxisOf`; `GeoReferenceProjector.Project` lands the `Header.GeoReference` [M1]; `EdgeProjection.All` lands every `IfcRel*` neutral edge [C5] — the decomposition/connection/assignment/void families, the property/quantity attachment, the structural member↔connection/member↔activity `Generic` edges (the `StructuralProjection.Attrs` 6-DOF restraint + full load family + `LoadKind`/`Case` and the `AtStart` discriminant riding the payload), the space↔surface `Generic` edges, and the material `Associate` edges with the occurrence-usage payload [C7].
- Receipt: the `GraphDelta` is the projector's whole contribution — a merge over the canonical `ElementGraph` that `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` folds with the other projectors' deltas; the minted rooted-`NodeId` set keyed by `GlobalId` is the identity table aspect projectors attach against and `Emit` reverses.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new extracted IFC entity family is one `Extract<T>` arm on the `Project` fold landing its seam node; a new IFC value kind is one `PropertyLowering` arm; a new relationship is one `IfcRelKind` row the `EdgeProjection` reads (`[03]-[RELATION_ALGEBRA]`); a new schema version is one `ReleaseVersion` the `ReleaseLower` resolves and the `Model/elements#IFC_CLASS` span validates; never a second element record beside the seam graph and never a per-entity projector type.
- Boundary: the projector is the ONE GeometryGym→seam lowering — the retired `BimModel.Project` produced a second stored `BimElement` keyed by `GlobalId`, and any owner that re-stores the element off the seam graph is the deleted form; GeometryGym is captured INTERNALLY (the `DatabaseIfc` field) and an `IfcProduct`/`IfcRel*`/`DatabaseIfc` type crossing the `IElementProjection.Project` signature is the named seam violation — the seam holds only `Node`/`Relationship`/`GraphDelta`; the rooted `NodeId` is a neutral kernel-minted id and the compressed IFC `GlobalId` is the node's `ExternalId` projection attribute (1:1) [H6], so the IFC GUID never becomes the node identity and the from-scratch authoring path mints its own neutral id; the value-narrowing is Bim's (`PropertyLowering`) because an `IfcValue`/dataType string crossing a seam signature is the deleted form — the seam carries only the typed `PropertyValue`/`MeasureValue` cases; geometry is referenced by `RepresentationContentHash` only [M2] and an in-process BRep evaluation or a RhinoCommon handle is the named seam violation — the analytical coordinate geometry the `Object` node CAN carry (`Axis`/`BoundaryPolygon`) is NOT baked here (extracting it is a geometry evaluation the no-in-process-geometry rule forbids; `Rasm.Compute` reads the content-keyed geometry and populates it), so a Bim geometry-read is the deleted form and the seam carries the structural/spatial CONNECTIVITY on the neutral `Relationship.Generic` edges instead; `Emit` is a Bim-INTERNAL method on the projector, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the seam owns only ingress projection.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
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
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.ReleaseVersion;   // the seam schema currency the Header carries (alias wins over the
using ModelView = Rasm.Element.ModelView;             // GeometryGym.Ifc namespace import), so the unqualified names are seam
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the IFC-text codec leg (Project/Emit/Sniff) — the ONLY GeometryGym
using GGView = GeometryGym.Ifc.ModelView;             // currency, never crossing into the Header

namespace Rasm.Bim;

// --- [SERVICES] ---------------------------------------------------------------------------
// The one GeometryGym->seam lowering: the DatabaseIfc is captured internally (the IElementProjection contract holds only
// Node/Relationship/GraphDelta), and Project mints the neutral rooted identity while recording the IFC GlobalId as the
// node ExternalId 1:1 [H6]. Emit is Bim-internal, NOT a seam member. Every fault lifts BARE off ctx.Key (band 2600 IS the
// Expected Code; no .ToError() hop) per Model/faults#FAULT_BAND.
public sealed partial class SemanticProjector(DatabaseIfc db) : IElementProjection {
    public Fin<GraphDelta> Project(ProjectionContext ctx) {
        Op key = ctx.Key;
        IfcProject? project = db.Project;
        if (project is null) {
            return Fin.Fail<GraphDelta>(new BimFault.DanglingReference(key, "ifc-project-root-miss"));
        }
        // The GlobalId->NodeId table: one neutral rooted mint per IfcRoot, the IFC GlobalId held as the node ExternalId
        // so re-ingest matches on the stored GlobalId (the Persistence diff/merge key) and Emit reverses the 1:1
        // projection. IfcMaterial/IfcProfileDef are non-rooted (content-keyed in MaterialProjection, below).
        var rooted = project.Extract<IfcRoot>().AsIterable()
            .Fold(Map<string, NodeId>(), (map, root) => map.AddOrUpdate(root.GlobalId, ctx.Rooted()));
        return GeoReferenceProjector.Project(project).Bind(geo => {
            var header = new Header(ReleaseLower(db.Release), ViewLower(db.ModelView), geo, db.Tolerance, ctx.At, StepHeaderOf(db));
            Seq<Node> nodes = Enrich(project, rooted,
                Objects(project, rooted).Concat(Bags(project, rooted)).Concat(Materials(project, db.Tolerance, key)).ToSeq());
            return EdgeProjection.All(project, rooted, db.Tolerance, key)
                .Map(edges => {
                    GraphDelta seeded = nodes.Fold(GraphDelta.Empty.Reheader(header), static (delta, node) => delta.Put(node));
                    return edges.Fold(seeded, static (delta, edge) => delta.Link(edge));
                });
        });
    }

    // Each IfcProduct -> Object.Occurrence and each IfcTypeObject -> Object.Type through ONE builder. The generic
    // Classification("ifc", classKey) carries the entity type WITHOUT leaking IfcClass onto the node; the PredefinedType
    // token reads off the entity's per-class predefined property; RepresentationContentHash is the keyed geometry map
    // [M2]; OwnerHistory rides optionally [H9]; ExternalId is the 1:1 GlobalId [H6]; Span is the class schema window [H8].
    // Ingress is PERMISSIVE: an unrostered/IFC4-new leaf lands the IfcClass.Proxy row through TryGet().IfNone(Proxy) so one
    // unknown entity never aborts the whole import — class validity is the Emit egress gate (AdmitPredefined), never here.
    // The analytical coordinate geometry (Axis/BoundaryPolygon) is NOT read in this generic builder (a generic Object lands
    // Axis None / BoundaryPolygon empty) — the structural curve member's idealized AxisCurve is baked by the separate Enrich
    // pass (below) off Model/structural#STRUCTURAL_PROJECTION AxisOf, the heavy display geometry staying content-hashed on
    // RepresentationContentHash; Rasm.Compute READS the baked Object.Axis off the seam, never re-deriving it from geometry.
    static Seq<Node> Objects(IfcProject project, Map<string, NodeId> rooted) =>
        project.Extract<IfcProduct>().AsIterable().Map(p => ObjectNode(p, ObjectKind.Occurrence, rooted))
            .Concat(project.Extract<IfcTypeObject>().AsIterable().Map(t => ObjectNode(t, ObjectKind.Type, rooted)))
            .ToSeq();

    static Node ObjectNode(IfcObjectDefinition definition, ObjectKind kind, Map<string, NodeId> rooted) {
        IfcClass cls = IfcClass.TryGet(ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _)).IfNone(IfcClass.Proxy);
        return new Node.Object(
            Id:             rooted[definition.GlobalId],
            Kind:           kind,
            ExternalId:     Some(definition.GlobalId),
            Classification: Classification.Create("ifc", cls.Key),
            PredefinedType: Predefined(definition),
            Name:           definition.Name ?? "",
            Tag:            (definition as IfcElement)?.Tag ?? "",
            Representations: IfcRepresentation.Keys(definition),
            BoundaryPolygon: Seq<Vector3>(),
            Axis:           Option<AxisCurve>.None,
            History:        OwnerStamp(definition.OwnerHistory),
            Span:           cls.Span);
    }

    // The predefined token is a strongly-typed per-class enum member (IfcWall.PredefinedType is IfcWallTypeEnum, etc.),
    // so a live occurrence carries it on a reflected PredefinedType property, NOT on the class-name split — the seam owns
    // the PredefinedType value-object and admits the token bare (validity is the Emit egress gate [C6]), an empty/NOTDEFINED
    // token folding to the IFC default.
    static PredefinedType Predefined(IfcObjectDefinition definition) {
        string token = definition.GetType().GetProperty("PredefinedType")?.GetValue(definition)?.ToString() ?? "";
        return string.IsNullOrWhiteSpace(token) || string.Equals(token, "NOTDEFINED", StringComparison.OrdinalIgnoreCase)
            ? PredefinedType.NotDefined
            : PredefinedType.Create(token);
    }

    // The structural analytical-line Enrich [Model/structural#STRUCTURAL_PROJECTION]: the generic Objects fold lands every
    // IfcStructuralItem as a generic Object node (Axis None — analytical geometry is not the general builder's concern); this
    // post-pass bakes the idealized AxisCurve onto each structural curve member's node off StructuralProjection.AxisOf, so a
    // Rasm.Compute frame solve reads graph.AxisOf off the node and never rails <member-axis-absent>. The lightweight line
    // only — the heavy display geometry stays content-hashed on RepresentationContentHash [M2]; a member with no analytical
    // edge keeps Axis None, and a non-structural node passes through untouched.
    static Seq<Node> Enrich(IfcProject project, Map<string, NodeId> rooted, Seq<Node> nodes) {
        Map<NodeId, AxisCurve> axes = toMap(project.Extract<IfcStructuralCurveMember>().AsIterable()
            .Choose(m => from id in rooted.Find(m.GlobalId)
                         from axis in StructuralProjection.AxisOf(m)
                         select (id, axis)));
        return nodes.Map(node => node is Node.Object o
            ? axes.Find(o.Id).Match(Some: axis => (Node)(o with { Axis = Some(axis) }), None: () => node)
            : node);
    }

    // PropertySet/QuantitySet bag nodes whose seam PropertyBag/QuantityBag carries the typed value the PropertyLowering
    // narrowing fills and the InheritanceMode the projector stamps at ingest [H1] so the seam Bake resolves type->occurrence
    // precedence without re-reading IFC; a Pset whose DefinesType inverse is non-empty is type-bound (the IFC type-driven
    // signal), and the Semantics/properties#PROPERTY_TEMPLATES PropertyInheritance.ModeOf classifies the set name onto the
    // seam InheritanceMode.
    static Seq<Node> Bags(IfcProject project, Map<string, NodeId> rooted) {
        var properties = project.Extract<IfcPropertySet>().AsIterable().Map(ps => (Node)new Node.PropertySet(
            rooted[ps.GlobalId],
            new PropertyBag(
                ps.Name ?? "",
                ps.HasProperties.Values.Aggregate(Map<PropertyName, PropertyValue>(),
                    static (bag, p) => bag.AddOrUpdate(PropertyName.Create(p.Name ?? ""), PropertyLowering.Lower(p))),
                PropertyInheritance.ModeOf(ps.Name ?? "", IsTypeBound(ps)))));
        var quantities = project.Extract<IfcElementQuantity>().AsIterable().Map(eq => (Node)new Node.QuantitySet(
            rooted[eq.GlobalId],
            new QuantityBag(
                eq.Name ?? "",
                eq.Quantities.Values.OfType<IfcPhysicalSimpleQuantity>().Aggregate(Map<PropertyName, MeasureValue>(),
                    static (bag, q) => bag.AddOrUpdate(PropertyName.Create(q.Name ?? ""), PropertyLowering.Measure(q))),
                PropertyInheritance.ModeOf(eq.Name ?? "", IsTypeBound(eq)))));
        return properties.Concat(quantities).ToSeq();
    }

    // The IFC type-driven signal: a property-set definition whose DefinesType inverse is non-empty is bound to a type
    // object (so its occurrence merge is type-driven), read off the GeometryGym SET<IfcTypeObject> inverse, never the
    // unrelated IsDefinedBy (the IfcRelDefinesByTemplate set).
    static bool IsTypeBound(IfcPropertySetDefinition set) => set.DefinesType.Any();

    // Non-rooted material nodes are content-keyed (kernel seed-zero XxHash128 over ToCanonicalBytes [H7]) by
    // MaterialProjection.Project, never GlobalId-rooted; the composition fold (Single/LayerSet/ProfileSet/ConstituentSet)
    // is the IFC material algebra Semantics/composition owns. A material-select entity that does not resolve is skipped
    // (the dangling reference surfaces at edge time), so a single malformed material never aborts the whole projection.
    static Seq<Node> Materials(IfcProject project, double tolerance, Op key) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Choose(rel => Optional(rel.RelatingMaterial as BaseClassIfc)
                .Bind(entity => MaterialProjection.Project(entity, tolerance, key).ToOption()))
            .Map(static m => (Node)m)
            .DistinctBy(static node => node.Id)
            .ToSeq();

    // IfcOwnerHistory -> seam OwnerHistory [H9]: owning user/app, created/modified (DateTime, NOT a unix long), change
    // action, state. Absent owner history yields None so a headerless model still projects; Emit re-derives ChangeAction.
    static Option<OwnerHistory> OwnerStamp(IfcOwnerHistory? history) =>
        Optional(history).Map(static h => new OwnerHistory(
            OwningUser:        h.OwningUser?.ToString() ?? "",
            OwningApplication: h.OwningApplication?.ApplicationFullName ?? "",
            Created:           Instant.FromDateTimeUtc(DateTime.SpecifyKind(h.CreationDate, DateTimeKind.Utc)),
            Modified:          h.LastModifiedDate > DateTime.MinValue
                                   ? Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(h.LastModifiedDate, DateTimeKind.Utc)))
                                   : None,
            ChangeAction:      h.ChangeAction.ToString(),
            State:             h.State.ToString()));

    // STEPFileInformation -> seam StepHeader [H9]: FILE_DESCRIPTION/FILE_NAME/FILE_SCHEMA. A separate axis from the
    // Marten provenance; the IFC owner history is NOT substituted by the persistence stamp.
    static StepHeader StepHeaderOf(DatabaseIfc database) =>
        database.OriginatingFileInformation is { } info
            ? new StepHeader(
                Descriptions:  info.FileDescriptions.ToSeq(),
                Name:          info.FileName ?? "",
                TimeStamp:     Instant.FromDateTimeUtc(DateTime.SpecifyKind(info.TimeStamp, DateTimeKind.Utc)),
                Authors:       info.Author.ToSeq(),
                Organizations: info.Organization.ToSeq(),
                Preprocessor:  info.PreProcessorVersion ?? "",
                OriginatingSystem: info.OriginatingSystem ?? "",
                Schema:        Seq1(database.Release.ToString()))
            : StepHeader.Empty with { Schema = Seq1(database.Release.ToString()) };

    // The two currency leaks meet here and nowhere else: the GeometryGym ReleaseVersion/ModelView lower onto the seam
    // SmartEnum by key match (the seam keys "IFC2X3"/"IFC4X3_ADD2" match the GeometryGym enum ToString case-insensitively),
    // and Emit raises the seam schema back to the GeometryGym enum the same way — the GeometryGym currency never reaching
    // the Header and the seam currency never reaching `new DatabaseIfc`.
    internal static ReleaseVersion ReleaseLower(GGRelease release) =>
        ReleaseVersion.Items.FirstOrDefault(v => string.Equals(v.Key, release.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? ReleaseVersion.Ifc4X3Add2;

    internal static GGRelease ReleaseRaise(ReleaseVersion schema) =>
        Enum.TryParse<GGRelease>(schema.Key, ignoreCase: true, out GGRelease raised) ? raised : GGRelease.IFC4X3_ADD2;

    static ModelView ViewLower(GGView view) {
        string name = view.ToString();
        return name.Contains("DesignTransfer", StringComparison.OrdinalIgnoreCase) ? ModelView.DesignTransfer
            : name.Contains("Alignment", StringComparison.OrdinalIgnoreCase)       ? ModelView.Alignment
            : name.Contains("Reference", StringComparison.OrdinalIgnoreCase)       ? ModelView.Ifc4Reference
            : ModelView.Coordination;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The Bim-internal IFC value narrowing the seam delegates to the projector: an IfcProperty narrows onto the seam's
// eight-case PropertyValue and an IfcPhysicalSimpleQuantity onto a MeasureValue over the seam Dimension. The seam forbids
// an IfcValue or a dataType string crossing its signature, so this narrowing is Bim's (the seam carries only the typed
// cases). A measured value's dimension reads off the IFC measure-type name through the frozen MeasureDimensions table
// (the H2 dimension support); an unmapped measure preserves its value as Text rather than claiming a wrong dimension.
internal static class PropertyLowering {
    static readonly FrozenDictionary<string, Dimension> MeasureDimensions = new Dictionary<string, Dimension>(StringComparer.Ordinal) {
        ["IfcLengthMeasure"] = Dimension.LengthDim, ["IfcPositiveLengthMeasure"] = Dimension.LengthDim, ["IfcNonNegativeLengthMeasure"] = Dimension.LengthDim,
        ["IfcAreaMeasure"] = Dimension.AreaDim, ["IfcVolumeMeasure"] = Dimension.VolumeDim, ["IfcMassMeasure"] = Dimension.MassDim,
        ["IfcTimeMeasure"] = Dimension.DurationDim, ["IfcForceMeasure"] = Dimension.ForceDim, ["IfcPressureMeasure"] = Dimension.PressureDim,
        ["IfcMassDensityMeasure"] = Dimension.DensityDim, ["IfcThermalTransmittanceMeasure"] = Dimension.ThermalTransmittance,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    // The IfcSimpleProperty family -> the seam PropertyValue eight-case union: a single value narrows by its IfcValue
    // shape, an enumerated value carries its chosen value plus its allowed set, a bounded value its lower/upper/setpoint
    // measures, a list/table the recursive arms; IfcPropertyReferenceValue and IfcComplexProperty fall to Text (the seam
    // Reference arm needs a resolved NodeId the bag fold does not carry), so no IFC property is dropped.
    public static PropertyValue Lower(IfcProperty property) => property switch {
        IfcPropertySingleValue sv     => LowerValue(sv.NominalValue),
        IfcPropertyEnumeratedValue ev => new PropertyValue.Enumerated(
            ev.EnumerationValues.AsIterable().HeadOrNone().Map(static v => v.ValueString).IfNone(""),
            ev.EnumerationValues.AsIterable().Map(static v => v.ValueString).ToSeq()),
        IfcPropertyBoundedValue bv    => new PropertyValue.Bounded(MeasureOpt(bv.LowerBoundValue), MeasureOpt(bv.UpperBoundValue), MeasureOpt(bv.SetPointValue)),
        IfcPropertyListValue lv       => new PropertyValue.List(lv.ListValues.AsIterable().Map(LowerValue).ToSeq()),
        IfcPropertyTableValue tv      => new PropertyValue.Table(tv.DefiningValues.Zip(tv.DefinedValues,
            static (def, val) => ((PropertyValue)new PropertyValue.Text(def.ValueString), (PropertyValue)new PropertyValue.Text(val.ValueString))).ToSeq()),
        _                             => new PropertyValue.Text(property.Name ?? ""),
    };

    // An IfcValue -> the seam PropertyValue scalar arm: a boolean-typed value (ValueType == bool) is Boolean, a measure
    // value whose type the dimension table carries is a typed Measure over its SI base, every other value its verbatim
    // string — the IfcValue.ValueType/Value/ValueString abstract members the narrowing reads off any IfcValue.
    static PropertyValue LowerValue(IfcValue? value) =>
        value is null                                                                                  ? new PropertyValue.Text("")
        : value.ValueType == typeof(bool)                                                              ? new PropertyValue.Boolean(value.Value is bool b && b)
        : value is IfcMeasureValue m && MeasureDimensions.TryGetValue(m.GetType().Name, out var dim)   ? new PropertyValue.Measure(new MeasureValue(dim, AsDouble(m.Value), dim.SiSymbol))
        : new PropertyValue.Text(value.ValueString);

    static Option<MeasureValue> MeasureOpt(IfcValue? value) =>
        value is IfcMeasureValue m && MeasureDimensions.TryGetValue(m.GetType().Name, out var dim)
            ? Some(new MeasureValue(dim, AsDouble(m.Value), dim.SiSymbol))
            : None;

    // An IfcPhysicalSimpleQuantity -> the seam MeasureValue over its canonical Dimension row [H2]: the IFC quantity value
    // is already SI-base, wrapped directly (never re-coerced through the UnitsNet registry, which is for raw unit-bearing
    // values). The six subtypes map onto the six base/derived dimensions; an unrecognized simple quantity yields Zero.
    public static MeasureValue Measure(IfcPhysicalSimpleQuantity quantity) => quantity switch {
        IfcQuantityLength q => new MeasureValue(Dimension.LengthDim, q.LengthValue, "m"),
        IfcQuantityArea q   => new MeasureValue(Dimension.AreaDim, q.AreaValue, "m2"),
        IfcQuantityVolume q => new MeasureValue(Dimension.VolumeDim, q.VolumeValue, "m3"),
        IfcQuantityWeight q => new MeasureValue(Dimension.MassDim, q.WeightValue, "kg"),
        IfcQuantityCount q  => new MeasureValue(Dimension.Dimensionless, q.CountValue, ""),
        IfcQuantityTime q   => new MeasureValue(Dimension.DurationDim, q.TimeValue, "s"),
        _                   => MeasureValue.Zero,
    };

    static double AsDouble(object? value) =>
        value is IConvertible c ? Convert.ToDouble(c, System.Globalization.CultureInfo.InvariantCulture) : 0.0;
}
```

## [03]-[RELATION_ALGEBRA]

- Owner: `IfcRelKind` the `[SmartEnum<string>]` carrying the WHOLE row-driven `IfcRel*` roster keyed on the relationship entity name — each row carrying the neutral `EdgeAxis` it folds onto (`Compose`/`Assign`/`Connect`/`Void`/`Generic`), the neutral `SubKind` token the typed case takes (the `ComposeKind`/`AssignKind`/`ConnectKind`/`VoidKind` key, empty for `Generic`), and the relating-side/related-side IFC inverse-attribute names directionality round-trips on [C5]; `EdgeAxis` the five neutral lowering targets; `EdgeProjection` the static fold lowering every relationship family onto neutral `Relationship` edges. The neutral cases carry a typed `SubKind`, never a wire-name or an attribute bag — the IFC wire-name + inverse live on the `IfcRelKind` ROW and reconstruct at egress through the reverse index, and only `Generic` carries the wire-name and the per-edge attribute payload.
- Cases: the row-driven roster — `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelReferencedInSpatialStructure` (`Compose`), `IfcRelConnectsElements`/`IfcRelConnectsWithRealizingElements`/`IfcRelConnectsPorts` (`Connect`), `IfcRelDefinesByType`/`IfcRelAssignsToGroup` (`Assign`), `IfcRelVoidsElement`/`IfcRelFillsElement` (`Void`) — PLUS the families that ride `Generic` because no neutral sub-kind fits: `IfcRelConnectsPortToElement` (the port↔element shape distinct from port↔port), `IfcRelConnectsPathElements` (wall-join priorities), `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity`, `IfcRelSpaceBoundary`, `IfcRelInterferesElements`, `IfcRelSequence`, `IfcRelCoversBldgElements`/`IfcRelCoversSpaces`, `IfcRelAssignsToControl`/`Process`/`Product`/`Actor`, `IfcRelDeclares`, `IfcRelServicesBuildings` — each lands its neutral edge carrying the IFC wire-name and the directionality/inverse on the `Generic` payload, so no relationship among the covered node-to-node families is dropped and none re-opens the IFC schema strata leak on the seam. `IfcRelAssociatesMaterial` (the `Associate` material edge with its `MaterialUsage` payload), `IfcRelDefinesByProperties` (the `Assign.PropertyDefinition` bag attachment), and the structural/space-boundary payloads ride DEDICATED folds, not the generic row path.
- Entry: `EdgeProjection.All(IfcProject project, Map<string,NodeId> rooted, double tolerance, Op key)` folds every relationship family into a `Seq<Relationship>` — the `FanOut`/`Pair` generic helpers read the relating/related `GlobalId`s, resolve them through the `rooted` map, look up the `IfcRelKind` row, and construct the neutral edge through `row.Edge`; the `Assign` arms (`DefinesByType`/`AssignsToGroup`) are INVERTED (the seam `Assign` is `Subject`(occurrence)→`Definition`(type/group), the inverse of the IFC relating→related), so they read each related occurrence as the subject and the relating type/group as the definition; the realizing `Connect` reads the `RealizingElements` head into the seam `Connect.Realizing` option; `DefinesProperties` lands the `IfcRelDefinesByProperties` (whose `RelatingPropertyDefinition` is a SET) property/quantity attachment as `Assign(occurrence, definition, PropertyDefinition)` per (occurrence, definition) pair; `Structural` lands `IfcRelConnectsStructuralMember` (member→connection, the 6-DOF restraint off `IfcStructuralConnection.AppliedCondition`→`IfcBoundaryNodeCondition` riding the payload) and `IfcRelConnectsStructuralActivity` (item→load, the applied force/moment off `IfcStructuralActivity.AppliedLoad`→`IfcStructuralLoadSingleForce` riding the payload) as `Generic` edges; `SpatialBoundaries` lands `IfcRelSpaceBoundary` (space→bounding-surface, the 1st/2nd-level discriminant riding the payload) as `Generic` edges; `MaterialEdges` lands the `Associate` material edges with the occurrence-usage payload [C7]; `Fin<T>` aborts on a dangling endpoint (`BimFault.DanglingReference`).
- Auto: `row.Edge(relating, related)` dispatches the `EdgeAxis` onto the seam case constructed with the row's neutral `SubKind` (a `Compose` of `ComposeKind.Get(SubKind)`, an `Assign` of `AssignKind.Get(SubKind)`, a `Connect` of `ConnectKind.Get(SubKind)` with no realizing node, a `Void` of `VoidKind.Get(SubKind)`) or a `Generic(Key, …)` carrying the IFC wire-name; the structural/space-boundary `Generic` edges carry their typed payload (the 6-DOF restraint booleans, the SI force/moment measures, the boundary level) as `PropertyValue` attribute entries the `Rasm.Compute` runners read by wire-name; the material `Associate` edge threads the `Semantics/composition#MATERIAL_COMPOSITION` occurrence-usage payload [C7] (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`, `CardinalPoint`/`ReferenceExtent`) as the seam's typed `MaterialUsage` (`LayerSet`/`ProfileSet`), never a `PropertyValue` attribute bag, the `ProfileSet` usage admitted through the seam `MaterialUsage.ProfileSet.Of` cardinal-point gate.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new IFC relationship is one `IfcRelKind` row (its axis, sub-kind, inverse-attribute names, and the `Inverted` flag that re-orients the egress for an inverted-direction family) plus, when it carries a payload, one dedicated `EdgeProjection` arm; a new directionality is the row's relating/related columns; the neutral five-axis algebra absorbs the case and `Generic` absorbs the residue — never a parallel `RelationshipKind` on the seam and never a dropped family among the covered node-to-node relationships. A relationship to a NON-node IFC resource (an `IfcRelAssociatesDocument`/`Library`/`Constraint`/`Approval` definition association, an `IfcRelAssignsToResource`) is a SEPARATE Growth axis — a new seam `Node` case plus its row — never silently in scope of the covered set.
- Boundary: the seam `Relationship` is the NEUTRAL edge algebra plus `Generic` — the seam carries no typed `IfcRel*` case and re-introducing seventeen typed IFC cases is the deleted form the critique fixed [C5]; the IFC names/directionality/inverse and the long-tail families live HERE on the Bim side, the neutral case carrying only its `SubKind` and the IFC wire-name reconstructing at egress through the reverse index, the `Generic` passthrough carrying any residue of the covered node-to-node families so a dropped family among them is the named defect (a relationship to a non-node IFC resource is a Growth axis — a new seam `Node` case — never a covered residue); directionality is preserved by the row's relating/related orientation and, for the inverted `Assign` family, the row's `Inverted` flag the egress re-orients on (`ReauthorRelationships` swaps the seam `Subject`/`Definition` back to the IFC relating/related before `Author`), never inferred at the call site; the material occurrence-usage rides the `Associate` edge's typed `MaterialUsage` payload [C7] and a parallel usage node is the deleted form; the structural/space-boundary connectivity rides the NEUTRAL `Generic` wire-name payload, never a typed IFC relationship case, so the strata stay IFC-schema-free.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The five neutral lowering targets — which seam Relationship case a row folds onto. Distinct from the seam's
// consumer-facing RelationshipKind: this is the Bim-side projector dispatch over which the Edge switch and the egress
// reverse index read.
public enum EdgeAxis : byte { Compose = 0, Assign = 1, Connect = 2, Void = 3, Generic = 4 }

// --- [MODELS] -----------------------------------------------------------------------------
// The row-driven IfcRel* roster: name -> neutral axis + the neutral SubKind token the typed case takes + the
// relating/related IFC inverse-attribute names directionality round-trips on. The long-tail families join here on the
// Generic axis so EVERY relationship has a home; the neutral edge keeps the wire-name + inverse so IFC directionality
// round-trips without a seam IFC case [C5]. Material/property/classification/structural/space-boundary ride dedicated
// EdgeProjection arms, not row.Edge.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcRelKind {
    public static readonly IfcRelKind Aggregates             = new("IfcRelAggregates",                  EdgeAxis.Compose, "aggregate", "RelatingObject",          "RelatedObjects");
    public static readonly IfcRelKind Nests                  = new("IfcRelNests",                       EdgeAxis.Compose, "nest",      "RelatingObject",          "RelatedObjects");
    public static readonly IfcRelKind ContainedInStructure   = new("IfcRelContainedInSpatialStructure", EdgeAxis.Compose, "contain",   "RelatingStructure",       "RelatedElements");
    public static readonly IfcRelKind ReferencedInStructure  = new("IfcRelReferencedInSpatialStructure",EdgeAxis.Compose, "reference", "RelatingStructure",       "RelatedElements");
    public static readonly IfcRelKind ConnectsElements       = new("IfcRelConnectsElements",            EdgeAxis.Connect, "path",      "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind ConnectsRealizing      = new("IfcRelConnectsWithRealizingElements",EdgeAxis.Connect,"realizing", "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind ConnectsPorts          = new("IfcRelConnectsPorts",               EdgeAxis.Connect, "port",      "RelatingPort",            "RelatedPort");
    public static readonly IfcRelKind ConnectsPortToElement  = new("IfcRelConnectsPortToElement",       EdgeAxis.Generic, "",          "RelatingPort",            "RelatedElement");
    public static readonly IfcRelKind ConnectsPathElements   = new("IfcRelConnectsPathElements",        EdgeAxis.Generic, "",          "RelatingElement",         "RelatedElement");
    public static readonly IfcRelKind ConnectsStructMember   = new("IfcRelConnectsStructuralMember",    EdgeAxis.Generic, "",          "RelatingStructuralMember","RelatedStructuralConnection");
    public static readonly IfcRelKind ConnectsStructActivity = new("IfcRelConnectsStructuralActivity",  EdgeAxis.Generic, "",          "RelatingElement",         "RelatedStructuralActivity");
    public static readonly IfcRelKind SpaceBoundary          = new("IfcRelSpaceBoundary",               EdgeAxis.Generic, "",          "RelatingSpace",           "RelatedBuildingElement");
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
    // IfcRel* — the exact inverse of Edge. The Generic axis is excluded (Generic resolves by wire-name through TryGet), so
    // ConnectsPortToElement (the port<->element shape distinct from the port<->port Connect) rides Generic and round-trips
    // by wire-name — the Connect "port" sub-kind is owned solely by ConnectsPorts, so (axis, sub-kind) keys are unique.
    static readonly Lazy<FrozenDictionary<(EdgeAxis, string), IfcRelKind>> ByNeutral = new(static () =>
        Items.Where(static r => r.Axis != EdgeAxis.Generic)
             .GroupBy(static r => (r.Axis, r.SubKind))
             .ToFrozenDictionary(static g => g.Key, static g => g.First()));

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
    // the row [C5] — the caller (ReauthorRelationships) feeds the endpoints already in IFC orientation, re-inverting the
    // Inverted Assign family first. Row-driven: a new IFC relationship is one row, never a per-class egress arm.
    // FactoryIfc.Construct mints the db-bound entity by class name (BaseClassIfc.Construct builds it, the factory registers
    // it on the database).
    public Option<IfcRelationship> Author(DatabaseIfc db, IfcProduct relating, Seq<IfcProduct> related) {
        if (related.IsEmpty || db.Factory.Construct(Key) is not IfcRelationship rel) {
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
    public static Fin<Seq<Relationship>> All(IfcProject project, Map<string, NodeId> rooted, double tolerance, Op key) {
        var edges = Decomposition(project, rooted, key)
            .Concat(Connections(project, rooted, key))
            .Concat(Assignments(project, rooted, key))
            .Concat(Generics(project, rooted, key))
            .Concat(DefinesProperties(project, rooted, key))
            .Concat(Structural(project, rooted, key))
            .Concat(SpatialBoundaries(project, rooted, key))
            .Concat(Seq(MaterialEdges(project, rooted, tolerance, key)));
        return edges.Sequence().Map(static rels => rels.Flatten().ToSeq());
    }

    static Seq<Fin<Seq<Relationship>>> Decomposition(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        FanOut(project.Extract<IfcRelAggregates>(), IfcRelKind.Aggregates, rooted, key,
            static r => r.RelatingObject.GlobalId, static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelNests>(), IfcRelKind.Nests, rooted, key,
            static r => r.RelatingObject.GlobalId, static r => r.RelatedObjects.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelContainedInSpatialStructure>(), IfcRelKind.ContainedInStructure, rooted, key,
            static r => r.RelatingStructure?.GlobalId ?? "", static r => r.RelatedElements.Select(static o => o.GlobalId)),
        FanOut(project.Extract<IfcRelReferencedInSpatialStructure>(), IfcRelKind.ReferencedInStructure, rooted, key,
            static r => r.RelatingStructure?.GlobalId ?? "", static r => r.RelatedElements.Select(static o => o.GlobalId)),
        Pair(project.Extract<IfcRelVoidsElement>(), IfcRelKind.Voids, rooted, key,
            static r => r.RelatingBuildingElement?.GlobalId ?? "", static r => r.RelatedOpeningElement?.GlobalId ?? ""),
        Pair(project.Extract<IfcRelFillsElement>(), IfcRelKind.Fills, rooted, key,
            static r => r.RelatingOpeningElement?.GlobalId ?? "", static r => r.RelatedBuildingElement?.GlobalId ?? ""));

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
        // The realizing Connect reads the realizing element head into the seam Connect.Realizing option [C5].
        project.Extract<IfcRelConnectsWithRealizingElements>().AsIterable().Select(rel =>
            from a in Resolve(rooted, rel.RelatingElement?.GlobalId ?? "", key)
            from b in Resolve(rooted, rel.RelatedElement?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Connect(a, b, ConnectKind.Realizing,
                rel.RealizingElements.AsIterable().HeadOrNone().Bind(re => rooted.Find(re.GlobalId))))
            .Sequence().Map(static e => e.ToSeq()));

    // The Assign family is INVERTED: the seam Assign is Subject(occurrence)->Definition(type/group), the inverse of the
    // IFC relating(type/group)->related(occurrences), so each related occurrence is the subject and the relating the
    // definition. DefinesByType binds the type bags for inheritance; AssignsToGroup the group/system/zone membership.
    static Seq<Fin<Seq<Relationship>>> Assignments(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelDefinesByType>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from occ in Resolve(rooted, o.GlobalId, key)
            from typ in Resolve(rooted, rel.RelatingType?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Assign(occ, typ, AssignKind.TypeDefinition)))
            .Sequence().Map(static e => e.ToSeq()),
        project.Extract<IfcRelAssignsToGroup>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from member in Resolve(rooted, o.GlobalId, key)
            from grp in Resolve(rooted, rel.RelatingGroup?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Assign(member, grp, AssignKind.Group)))
            .Sequence().Map(static e => e.ToSeq()));

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
            static r => r.RelatingSystem?.GlobalId ?? "", static r => r.RelatedBuildings.Select(static o => o.GlobalId)));

    // The property/quantity ATTACHMENT onto neutral Assign.PropertyDefinition edges the seam Bake reads: the SET-valued
    // RelatingPropertyDefinition fans out one edge per (related occurrence, definition) pair so the seam Bake folds the bag
    // into the element — Subject = the related occurrence, Definition = the bag node.
    static Seq<Fin<Seq<Relationship>>> DefinesProperties(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelDefinesByProperties>().AsIterable()
            .SelectMany(rel => rel.RelatedObjects.SelectMany(o => rel.RelatingPropertyDefinition.Select(def =>
                from subject in Resolve(rooted, o.GlobalId, key)
                from definition in Resolve(rooted, def.GlobalId, key)
                select (Relationship)new Relationship.Assign(subject, definition, AssignKind.PropertyDefinition))))
            .Sequence().Map(static e => e.ToSeq()));

    // The structural-analysis idealization onto neutral Generic edges [C5]: IfcRelConnectsStructuralMember binds an
    // idealized member to its connection, IfcRelConnectsStructuralActivity binds a load activity to a structural item — the
    // 6-DOF restraint (fixity + SI spring) and the full IfcStructuralLoad family are lowered through the DEDICATED
    // Model/structural#STRUCTURAL_PROJECTION StructuralProjection.Attrs owner (never a local boolean-only/single-force-only
    // reader), the restraint edge additionally carrying the StructuralProjection.AtStart start/end discriminant. So a
    // Rasm.Compute frame solve reads graph.SupportsOf/graph.LoadsOf off these edges and graph.AxisOf off the Enrich-baked
    // Object.Axis (Enrich, below) — never re-reading IFC, never a defaulted support joint or load case.
    static Seq<Fin<Seq<Relationship>>> Structural(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelConnectsStructuralMember>().AsIterable().Select(rel =>
            from m in Resolve(rooted, rel.RelatingStructuralMember?.GlobalId ?? "", key)
            from c in Resolve(rooted, rel.RelatedStructuralConnection?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructMember.Key, m, c,
                StructuralProjection.Attrs(rel.RelatedStructuralConnection).Add(
                    PropertyName.Create("AtStart"),
                    new PropertyValue.Boolean(StructuralProjection.AtStart(rel.RelatingStructuralMember as IfcStructuralCurveMember, rel.RelatedStructuralConnection)))))
            .Sequence().Map(static e => e.ToSeq()),
        project.Extract<IfcRelConnectsStructuralActivity>().AsIterable().Select(rel =>
            from item in Resolve(rooted, (rel.RelatingElement as IfcRoot)?.GlobalId ?? "", key)
            from act in Resolve(rooted, rel.RelatedStructuralActivity?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Generic(IfcRelKind.ConnectsStructActivity.Key, item, act,
                StructuralProjection.Attrs(rel.RelatedStructuralActivity)))
            .Sequence().Map(static e => e.ToSeq()));

    // The energy/spatial space-boundary graph onto neutral Generic edges [C5]: IfcRelSpaceBoundary binds a space to its
    // bounding building element, the 1st/2nd-level discriminant riding the attrs (the physical/virtual + internal/external
    // enums are GeometryGym internal fields, absent from the public surface, so they are not lowered) — so a Rasm.Compute
    // OSM build reads graph.SpacesOf/graph.BoundingSurfacesOf off the baked graph. RelatingSpace is an
    // IfcSpaceBoundarySelect, so its GlobalId reads through the IfcRoot cast.
    static Seq<Fin<Seq<Relationship>>> SpatialBoundaries(IfcProject project, Map<string, NodeId> rooted, Op key) => Seq(
        project.Extract<IfcRelSpaceBoundary>().AsIterable().Select(rel =>
            from s in Resolve(rooted, (rel.RelatingSpace as IfcRoot)?.GlobalId ?? "", key)
            from e in Resolve(rooted, rel.RelatedBuildingElement?.GlobalId ?? "", key)
            select (Relationship)new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, s, e, BoundaryAttrs(rel)))
            .Sequence().Map(static e => e.ToSeq()));

    // The material Associate edges [C7]: each related element binds the projected Material node (the content-keyed id the
    // Materials fold lands) through an Associate edge carrying the occurrence MaterialUsage (the LayerSet direction/sense/
    // offset or the ProfileSet cardinal-point/extent), so a wall and its mirror share one LayerSet node with two usages;
    // the ProfileSet usage admits through the seam MaterialUsage.ProfileSet.Of cardinal-point gate.
    static Fin<Seq<Relationship>> MaterialEdges(IfcProject project, Map<string, NodeId> rooted, double tolerance, Op key) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable().SelectMany(rel => rel.RelatedObjects.Select(o =>
            from element in Resolve(rooted, o.GlobalId, key)
            from material in MaterialNode(rel.RelatingMaterial, tolerance, key)
            from usage in UsageOf(rel.RelatingMaterial, key)
            select (Relationship)new Relationship.Associate(element, material, usage)))
            .Sequence().Map(static e => e.ToSeq());

    static Fin<NodeId> MaterialNode(IfcMaterialSelect select, double tolerance, Op key) =>
        MaterialProjection.Project(select as BaseClassIfc, tolerance, key).Map(static m => m.Id);

    // The IFC occurrence material usage -> the seam's typed MaterialUsage [C7]: an IfcMaterialLayerSetUsage lowers to
    // MaterialUsage.LayerSet (direction/sense/offset), an IfcMaterialProfileSetUsage to MaterialUsage.ProfileSet through
    // the seam Of cardinal-point gate, a type-level set with no occurrence usage to MaterialUsage.None.
    static Fin<MaterialUsage> UsageOf(IfcMaterialSelect select, Op key) => select switch {
        IfcMaterialLayerSetUsage u => Fin.Succ<MaterialUsage>(new MaterialUsage.LayerSet(
            u.LayerSetDirection switch {
                IfcLayerSetDirectionEnum.AXIS1 => LayerSetDirection.Axis1,
                IfcLayerSetDirectionEnum.AXIS2 => LayerSetDirection.Axis2,
                _                              => LayerSetDirection.Axis3,
            },
            u.DirectionSense == IfcDirectionSenseEnum.POSITIVE ? DirectionSense.Positive : DirectionSense.Negative,
            u.OffsetFromReferenceLine)),
        IfcMaterialProfileSetUsage u => MaterialUsage.ProfileSet.Of((int)u.CardinalPoint, u.ReferenceExtent, key),
        _ => Fin.Succ<MaterialUsage>(new MaterialUsage.None()),
    };

    // The space-boundary 1st/2nd-level discriminant -> a neutral attr the Rasm.Compute energy build filters on. The
    // GeometryGym IfcRelSpaceBoundary exposes RelatingSpace/RelatedBuildingElement/ConnectionGeometry publicly but keeps
    // PhysicalOrVirtualBoundary/InternalOrExternalBoundary on internal fields with no public getter, so the level is the
    // lone publicly-readable flag; the physical/virtual + internal/external classification is not on the host surface and
    // is never fabricated (an internal-field reflection read is the fragile form this owner refuses).
    static Map<PropertyName, PropertyValue> BoundaryAttrs(IfcRelSpaceBoundary rel) => Map(
        (PropertyName.Create("BoundaryLevel"), new PropertyValue.Text(rel is IfcRelSpaceBoundary2ndLevel ? "2nd" : "1st")));

    // The generic relating->related fan-out (a one-to-many family), constructing one neutral edge per related endpoint
    // through the row; a many-related family fans out one edge per endpoint, the directionality the row records.
    static Fin<Seq<Relationship>> FanOut<TRel>(IEnumerable<TRel> rels, IfcRelKind kind, Map<string, NodeId> rooted, Op key,
        Func<TRel, string> relating, Func<TRel, IEnumerable<string>> related) =>
        rels.SelectMany(rel => related(rel).Select(id =>
                from r in Resolve(rooted, relating(rel), key)
                from e in Resolve(rooted, id, key)
                select kind.Edge(r, e)))
            .Sequence().Map(static e => e.ToSeq());

    static Fin<Seq<Relationship>> Pair<TRel>(IEnumerable<TRel> rels, IfcRelKind kind, Map<string, NodeId> rooted, Op key,
        Func<TRel, string> relating, Func<TRel, string> related) =>
        rels.Select(rel =>
                from r in Resolve(rooted, relating(rel), key)
                from e in Resolve(rooted, related(rel), key)
                select kind.Edge(r, e))
            .Sequence().Map(static e => e.ToSeq());

    static Fin<NodeId> Resolve(Map<string, NodeId> rooted, string globalId, Op key) =>
        rooted.Find(globalId).ToFin(new BimFault.DanglingReference(key, $"edge-endpoint-miss:{globalId}"));
}
```

## [04]-[IFC_EGRESS]

- Owner: `SemanticProjector.Emit` the Bim-internal `ElementGraph`→IFC bytes re-author — NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern; it constructs the `DatabaseIfc` at the `ReleaseRaise` schema target, runs the `Model/elements#IFC_CLASS` `PredefinedType` egress gate per object (publishing a `NodeId`→`IfcProduct` map), and re-authors the entity graph — `ReauthorMaterials` (the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition`/`AuthorUsage` per seam `Material` node + `Associate` usage edge [C7]), `ReauthorProperties` (the `IfcPropertySet`/`IfcElementQuantity` rebuilt from the bag nodes + the `IfcRelDefinesByProperties` onto the elements), `ReauthorClassifications` (`Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author` per `Object` node), and `ReauthorRelationships` (the neutral-edge → `IfcRel*` row-driven re-author); `Sniff` the ingress counterpart reading `FILE_SCHEMA` (STEP) or `schema_identifier` (JSON) off the bytes BEFORE the database is constructed so the schema is never hardcoded [H8].
- Entry: `SemanticProjector.Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles)` re-authors the graph into IFC text — for each `Object` node it resolves the `IfcClass` row from the generic `Classification` code, runs `IfcClass.AdmitPredefined(token, objectType, schema, key)` validating the predefined token against the row's frozen valid set AND the schema span → `BimFault.UnmappedClass` [C6][H8], constructs the entity at the resolved schema, assigns the `ExternalId` `GlobalId` (or mints one through `ParserIfc.EncodeGuid` for a from-scratch node) [H6], and re-stamps the `OwnerHistory` with a `ChangeAction` diff-derived against the `prior` snapshot, matching the rooted node on its stable `ExternalId` GlobalId across re-ingest [H9] — publishing the `NodeId`→`IfcProduct` map the re-author folds bind against; `ReauthorMaterials` authors each seam `Material` node's type-level definition + `MaterialPropertySet` Psets ONCE and the per-occurrence `MaterialUsage` [C7] over each `Associate` edge, the `profiles` resolver reconstituting a `ProfileSet`'s `IfcProfileDef` from the content-addressed STEP store; `Fin<T>` aborts on the gate, lifting BARE; `SemanticProjector.Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format)` returns the GeometryGym `ReleaseVersion` the import-rail seeds the database with.
- Auto: `Emit` folds the `graph.Nodes` once — the `Object` egress gate resolves the `IfcClass` row, validates the predefined token and the `IntroducedIn`/`RemovedIn` schema span against `Header.Schema` (an IFC4.3 infrastructure class targeting an IFC2x3 emit faults `BimFault.UnmappedClass` rather than writing an entity the schema forbids), and constructs the entity; the `GlobalId` round-trips from `ExternalId` so a re-imported model re-emits its original GUIDs (1:1) [H6]; `ReauthorRelationships` re-authors the neutral `Compose`/`Connect`/`Void` edges and the `Assign.TypeDefinition`/`Group` edges by reverse-indexing the `IfcRelKind` row and the `Generic` long-tail by its wire-name through `IfcRelKind.Author`, the directionality reconstructing from the row's relating/related names — so the long-tail families re-emit exactly as they were read [C5] (the analytical structural/space-boundary `Generic` edges are skipped: they are `Rasm.Compute` enrichment re-derivable from the content-keyed geometry, not IFC-round-trip state); the seam `Material` subgraph re-authors through `ReauthorMaterials` and the property/quantity bags through `ReauthorProperties` (the `IfcPropertySingleValue`/`IfcQuantity*` rebuilt from each typed `PropertyValue`/`MeasureValue` + the `IfcRelDefinesByProperties` onto each bound element) — the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers; the `OwnerHistory` re-emit derives the `ChangeAction` from the structural record diff (the seam `Node` record equality over the prior snapshot, the rooted node matched on its stable 1:1 GlobalId `ExternalId` since the neutral `NodeId` is freshly minted each ingest — `ADDED`/`MODIFIED`/`NOCHANGE`) so the IFC owner history reflects the real edit [H9]; the `StepHeader` re-authors `FILE_DESCRIPTION`/`FILE_NAME` from `Header.Step`.
- Auto: `Sniff` reads the schema off the bytes before `new DatabaseIfc` — the STEP `FILE_SCHEMA(('IFC4X3_ADD2'))` header line or the IFC-JSON `schema_identifier` member — mapping the token onto the GeometryGym `ReleaseVersion` and defaulting to `IFC4X3_ADD2` only when the header is absent, so the import never hardcodes 4x3 over a 2x3 file [H8].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new emit format is one `FormatIfcSerialization` the `db.ToString` switch already carries; a new schema is one `ReleaseVersion` the `Sniff`/`ReleaseRaise` resolves and the span validates; the predefined gate is one `AdmitPredefined` call composed once per object; a new re-authored property kind is one `RaiseProperty` arm — never a per-class egress branch.
- Boundary: `Emit` is Bim-INTERNAL and absent from the `IElementProjection` contract — exposing it on the seam is the named violation; the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the frozen valid set and the schema span) [C6], never a per-call regex and never silent acceptance of an out-of-schema predefined; the `GlobalId` is the node `ExternalId` round-tripped 1:1 [H6] and a freshly-minted GUID on a re-emitted node is the deleted form; the schema is sniffed [H8] and a hardcoded `IFC4X3_ADD2` over a foreign-schema file is the named defect; the `ChangeAction` is diff-derived [H9] and a blanket `ADDED` stamp is the deleted form; the material/property/classification egress reads the seam graph ONLY — the `Material` node + the `Associate` edge `MaterialUsage`, the `PropertySet`/`QuantitySet` bag nodes, and the `Object` node `Classification` — and a `Rasm.Materials` wire carrier crossing into `Emit` is the deleted form (those Materials wires are retired), the type-level material definition authored ONCE per `Material` node and the per-occurrence usage wrapping it so a usage duplicated onto the type-level set is the deleted form [C7]; the GeometryGym `ReleaseVersion`/`ModelView` enums stay on this codec leg through `ReleaseRaise`/`Sniff` and a leak into the seam `Header` is the named defect.

```csharp signature
public sealed partial class SemanticProjector {
    // The Bim-internal IFC egress: ElementGraph -> DatabaseIfc -> bytes. The PredefinedType egress gate [C6] and the
    // schema-span validation [H8] run per Object; the GlobalId round-trips 1:1 [H6]; the OwnerHistory ChangeAction is
    // diff-derived against the prior snapshot [H9]. The Object authoring publishes a NodeId->IfcProduct map the re-author
    // folds bind against. The profiles resolver reconstitutes a ProfileSet's IfcProfileDef from the content-addressed STEP
    // store the ProfileRef.ContentKey keys. Never a seam member.
    public Fin<string> Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles) {
        var target = new DatabaseIfc(false, ReleaseRaise(graph.Header.Schema));
        // Re-ingest correlation [H6]: the neutral rooted NodeId is freshly minted each Project, so a re-imported graph
        // shares NO NodeId with the prior snapshot — the diff-derived ChangeAction matches a rooted node on the stable
        // 1:1 GlobalId (Node.Object.ExternalId), indexed once here, falling back to the NodeId for a from-scratch node.
        var priorByExternal = prior.Map(static p => p.Nodes.Values
                .Choose(static n => n is Node.Object o ? o.ExternalId.Map(ext => (Ext: ext, Node: o)) : None)
                .Fold(Map<string, Node.Object>(), static (m, e) => m.AddOrUpdate(e.Ext, e.Node)))
            .IfNone(Map<string, Node.Object>());
        return graph.Nodes.Values.Choose(static node => node is Node.Object obj ? Some(obj) : None)
            .Map(obj => Author(target, obj, graph.Header.Schema, key, prior, priorByExternal).Map(product => (Id: obj.Id, Product: product)))
            .Sequence()
            .Map(static authored => authored.Fold(Map<NodeId, IfcProduct>(), static (m, e) => m.AddOrUpdate(e.Id, e.Product)))
            .Bind(products => ReauthorMaterials(target, graph, products, profiles).Map(_ => {
                ReauthorProperties(target, graph, products);
                ReauthorClassifications(target, graph, products);
                ReauthorRelationships(target, graph, products);
                ReauthorHeader(target, graph.Header.Step);
                return target.ToString(format);
            }));
    }

    // The egress gate: resolve the IfcClass row from the generic Classification code, admit the predefined token against
    // the frozen valid set AND the schema span, fault UnmappedClass on a miss [C6][H8], then construct the entity and
    // round-trip the GlobalId [H6] + diff-derived OwnerHistory [H9]. The seam Object carries no IFC ObjectType, so a
    // USERDEFINED predefined falls back to the node Name as its label.
    static Fin<IfcProduct> Author(DatabaseIfc target, Node.Object obj, ReleaseVersion schema, Op key, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) =>
        IfcClass.Resolve(obj.Classification.Code, key)
            .Bind(cls => cls.AdmitPredefined(obj.PredefinedType.Token, obj.Name, schema, key)
                .Map(_ => {
                    var entity = (IfcProduct)target.Factory.Construct(cls.Key);
                    entity.GlobalId = obj.ExternalId.IfNone(() => ParserIfc.EncodeGuid(Guid.NewGuid()));
                    entity.Name = obj.Name;
                    obj.History.IfSome(_ => entity.OwnerHistory = OwnerHistoryOf(target, ChangeOf(obj, prior, priorByExternal)));
                    return entity;
                }));

    // The ChangeAction is the diff verdict against the prior snapshot through the id-normalized seam Node record equality,
    // never a blanket stamp [H9]: a rooted node matches the prior on the stable 1:1 GlobalId (ExternalId) ACROSS re-ingest since
    // the NodeId is freshly minted each ingest, falling back to the NodeId for a from-scratch node — absent prior ->
    // ADDED, structurally equal -> NOCHANGE, else MODIFIED.
    static IfcChangeActionEnum ChangeOf(Node.Object obj, Option<ElementGraph> prior, Map<string, Node.Object> priorByExternal) {
        Option<Node> before = obj.ExternalId.Bind(ext => priorByExternal.Find(ext)).Map(static o => (Node)o)
            | prior.Bind(graph => graph.Find(obj.Id));
        return before.Match(
            None: () => IfcChangeActionEnum.ADDED,
            // The prior rooted node carries a DIFFERENT freshly-minted NodeId each ingest [H6], so the structural compare
            // normalizes the id first — the raw record Equals would otherwise see every re-ingested node as MODIFIED and
            // the NOCHANGE arm would be dead.
            Some: b => b is Node.Object before2 && (before2 with { Id = obj.Id }).Equals(obj) ? IfcChangeActionEnum.NOCHANGE : IfcChangeActionEnum.MODIFIED);
    }

    static IfcOwnerHistory OwnerHistoryOf(DatabaseIfc target, IfcChangeActionEnum change) {
        IfcOwnerHistory history = target.Factory.OwnerHistoryAdded;
        history.ChangeAction = change;
        return history;
    }

    // The schema sniff [H8]: read FILE_SCHEMA / schema_identifier off the bytes before constructing the database, mapping
    // the token onto the GeometryGym ReleaseVersion; default IFC4X3_ADD2 only when the header is absent.
    public static GGRelease Sniff(ReadOnlyMemory<byte> bytes, InterchangeFormat format) =>
        format == InterchangeFormat.IfcJson
            ? SchemaToken((JsonNode.Parse(bytes.Span) as JsonObject)?["schema_identifier"]?.ToString() ?? "")
            : SchemaToken(StepSchemaLine(bytes.Span));

    static GGRelease SchemaToken(string token) =>
        token.Contains("IFC2X3", StringComparison.OrdinalIgnoreCase) ? GGRelease.IFC2x3
        : token.Contains("IFC4X3", StringComparison.OrdinalIgnoreCase) ? GGRelease.IFC4X3_ADD2
        : token.Contains("IFC4", StringComparison.OrdinalIgnoreCase) ? GGRelease.IFC4
        : GGRelease.IFC4X3_ADD2;

    static string StepSchemaLine(ReadOnlySpan<byte> bytes) {
        string header = System.Text.Encoding.ASCII.GetString(bytes[..Math.Min(bytes.Length, 4096)]);
        int start = header.IndexOf("FILE_SCHEMA", StringComparison.Ordinal);
        return start < 0 ? "" : header[start..Math.Min(header.Length, start + 64)];
    }

    // The seam Material subgraph -> IFC: each Material node authors its type-level definition + Psets ONCE through
    // MaterialProjection.AuthorComposition, then each incident Associate edge authors the per-occurrence MaterialUsage [C7]
    // wrapping the shared definition (AuthorUsage) and the IfcRelAssociatesMaterial onto the bound element — so a wall and
    // its mirror share one IfcMaterialLayerSet with two IfcMaterialLayerSetUsage instances. REPLACES the retired Materials
    // wire carriers; the material reads off the projected graph, never a wire.
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

    // The property/quantity bags -> IFC: each PropertySet/QuantitySet node authors its IfcPropertySet/IfcElementQuantity
    // from the typed bag ONCE, then each incident Assign.PropertyDefinition edge authors the IfcRelDefinesByProperties onto
    // the bound element — the round-trip the retired stringly BimElement.PropertyBinding never had, the typed PropertyValue/
    // MeasureValue raising to the IfcPropertySingleValue/IfcQuantity* the seam value carries.
    static void ReauthorProperties(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) {
        var bags = graph.Nodes.Values
            .Choose(n => AuthorBag(target, n).Map(set => (Id: n.Id, Set: set)))
            .Fold(Map<NodeId, IfcPropertySetDefinition>(), static (m, e) => m.AddOrUpdate(e.Id, e.Set));
        graph.Edges.AsIterable()
            .Choose(static e => e is Relationship.Assign a && a.SubKind == AssignKind.PropertyDefinition ? Some(a) : None)
            .Iter(a => bags.Find(a.Definition).IfSome(set => products.Find(a.Subject).IfSome(product => {
                _ = new IfcRelDefinesByProperties((IfcObjectDefinition)product, set);
            })));
    }

    // The empty-bag guard is load-bearing: the IfcPropertySet(name, IEnumerable)/IfcElementQuantity(name, IEnumerable)
    // ctors derive their database from the FIRST member (`members.First().mDatabase`), so an empty bag would throw at the
    // boundary — an empty Pset/Qto carries no IFC data, so it is skipped (its DefinesByProperties edge then resolves no
    // bag and re-authors nothing), lossless and exception-free, never a crashing `.First()` on the empty enumerable.
    static Option<IfcPropertySetDefinition> AuthorBag(DatabaseIfc target, Node node) => node switch {
        Node.PropertySet ps when !ps.Bag.Properties.IsEmpty => Some((IfcPropertySetDefinition)new IfcPropertySet(ps.Bag.SetName,
            ps.Bag.Properties.AsIterable().Map(kv => RaiseProperty(target, kv.Key, kv.Value)))),
        Node.QuantitySet qs when !qs.Bag.Quantities.IsEmpty => Some((IfcPropertySetDefinition)new IfcElementQuantity(qs.Bag.SetName,
            qs.Bag.Quantities.AsIterable().Map(kv => RaiseQuantity(target, kv.Key, kv.Value)))),
        _ => Option<IfcPropertySetDefinition>.None,
    };

    // The seam PropertyValue -> the IFC property: a Boolean/Measure authors its typed IfcPropertySingleValue ctor, every
    // other case its canonical Render string (the IfcPropertyEnumeratedValue/BoundedValue composites round-trip as text,
    // never dropped), so the typed value the PropertyLowering narrowed re-emits.
    static IfcProperty RaiseProperty(DatabaseIfc target, PropertyName name, PropertyValue value) => value switch {
        PropertyValue.Boolean b => new IfcPropertySingleValue(target, name.Value, b.Value),
        PropertyValue.Measure m => new IfcPropertySingleValue(target, name.Value, m.Value.Si),
        _                       => new IfcPropertySingleValue(target, name.Value, value.Render()),
    };

    // The seam MeasureValue -> the IFC physical quantity by its Dimension row: the SI magnitude authors the matching
    // IfcQuantity* (Area/Volume/Weight/Time/Count, defaulting to Length), so a base-quantity takeoff re-emits its Qto.
    static IfcPhysicalQuantity RaiseQuantity(DatabaseIfc target, PropertyName name, MeasureValue measure) =>
        measure.Dimension == Dimension.AreaDim     ? new IfcQuantityArea(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.VolumeDim ? new IfcQuantityVolume(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.MassDim   ? new IfcQuantityWeight(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.DurationDim ? new IfcQuantityTime(target, name.Value, measure.Si)
        : measure.Dimension == Dimension.Dimensionless ? new IfcQuantityCount(target, name.Value, measure.Si)
        : new IfcQuantityLength(target, name.Value, measure.Si);

    // The element standard classification -> IFC: each Object node authors its standard Classification through
    // ClassificationSystem.Author (which returns None for the "ifc" entity-type code the Author above already resolved as
    // the IfcClass, so the entity type never re-authors as a classification reference). This is the element-classification
    // egress subsuming the retired material-wire classification half.
    static void ReauthorClassifications(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) =>
        graph.Nodes.Values.Choose(static n => n is Node.Object o ? Some(o) : None)
            .Iter(obj => products.Find(obj.Id).IfSome(product =>
                ClassificationSystem.Author(target, (IfcDefinitionSelect)product, obj.Classification).IfSome(static _ => { })));

    // The neutral edge algebra -> IfcRel*: each typed Compose/Connect/Void edge and the Assign.TypeDefinition/Group edge
    // re-author their IFC relationship by the reverse-indexed IfcRelKind row, the Generic long-tail by its wire-name [C5];
    // the material/property/classification edges and the analytical structural/space-boundary Generic edges resolve to None
    // (authored by their dedicated re-author or re-derived by Rasm.Compute), so they are skipped here. Each edge re-authors
    // per (relating, related) pair against the authored product map — a one-to-many family thus re-emits one IfcRel* per
    // part (denormalized but lossless), the row driving the rest.
    static void ReauthorRelationships(DatabaseIfc target, ElementGraph graph, Map<NodeId, IfcProduct> products) =>
        graph.Edges.AsIterable().Iter(edge => RelKindOf(edge).IfSome(kind => {
            // The Inverted Assign family (DefinesByType/AssignsToGroup) stored the seam Subject(occurrence)->Definition
            // (the inverse of the IFC relating(type/group)->related), so egress re-inverts to the IFC orientation the
            // row's Relating/Related names expect — the round-trip directionality matching the ingest [C5]; every other
            // row already reads in IFC orientation, so the endpoints pass straight through.
            var (ifcRelating, ifcRelated) = kind.Inverted ? (edge.Related, edge.Relating) : (edge.Relating, edge.Related);
            products.Find(ifcRelating).IfSome(relating =>
                products.Find(ifcRelated).IfSome(related =>
                    kind.Author(target, (IfcProduct)relating, Seq1((IfcProduct)related)).IfSome(rel => {
                        // A realizing Connect re-authors its third endpoint onto IfcRelConnectsWithRealizingElements so the
                        // realizing intermediary round-trips, not just the From/To pair the row-driven Author binds.
                        if (edge is Relationship.Connect { Realizing: var realizing } && rel is IfcRelConnectsWithRealizingElements realized) {
                            realizing.Bind(products.Find).IfSome(re => { if (re is IfcElement element) { realized.RealizingElements.Add(element); } });
                        }
                    })));
        }));

    // The analytical Generic wire-names (structural connectivity + space boundaries) are NOT IFC-round-trip state — they
    // are Rasm.Compute enrichment re-derivable from the content-keyed geometry, so ReauthorRelationships skips them.
    static readonly FrozenSet<string> AnalyticalWires = new[] {
        IfcRelKind.ConnectsStructMember.Key, IfcRelKind.ConnectsStructActivity.Key, IfcRelKind.SpaceBoundary.Key,
    }.ToFrozenSet(StringComparer.Ordinal);

    // The neutral seam edge -> its IfcRelKind row: the typed cases reverse-index (axis, sub-kind) through ForNeutral (the
    // exact inverse of IfcRelKind.Edge), the Generic passthrough resolves by its IFC wire-name; an Associate/PropertyDefinition/
    // Assessment edge returns None (the material/property re-author owns it), as does an analytical Generic edge and an
    // unrostered wire-name (never re-authoring an entity the roster never declared).
    static Option<IfcRelKind> RelKindOf(Relationship edge) => edge switch {
        Relationship.Compose c => IfcRelKind.ForNeutral(EdgeAxis.Compose, c.SubKind.Key),
        Relationship.Connect c => IfcRelKind.ForNeutral(EdgeAxis.Connect, c.SubKind.Key),
        Relationship.Void v    => IfcRelKind.ForNeutral(EdgeAxis.Void, v.SubKind.Key),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition || a.SubKind == AssignKind.Group => IfcRelKind.ForNeutral(EdgeAxis.Assign, a.SubKind.Key),
        Relationship.Generic g when !AnalyticalWires.Contains(g.WireName) && IfcRelKind.TryGet(g.WireName, out IfcRelKind? row) && row is { } resolved => Some(resolved),
        _ => Option<IfcRelKind>.None,
    };

    // The StepHeader -> the STEP physical-file header on the database: FILE_DESCRIPTION (FileDescriptions) and the FILE_NAME
    // fields restored from the seam header [H9], so an import -> export cycle preserves provenance instead of stripping it.
    // FILE_SCHEMA already rides target.Release (set at the DatabaseIfc construction from Header.Schema), so the schema is
    // restored there.
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

- Owner: `IfcLegality` the `IGraphConstraint` deciding IFC-semantic legality the seam's structural `GraphDelta` switch cannot [M3] — the seam enforces only structural invariants (an edge endpoint resolves, an endpoint kind is legal), and IFC legality (which entity may relate to which) is Bim's, depended UP on through the `IGraphConstraint` contract.
- Entry: `IfcLegality.Validate(GraphDelta delta, ElementGraph graph) → Validation<Error,Unit>` accumulates every IFC-legality violation in the delta against the graph — `Success(unit)` when every rule holds, a `Fail` carrying the accumulated `Error` set otherwise; the validation is applicative (every violation reported at once, never short-circuit) so an authoring pass sees all rejects in one apply, the `BimFault.ModelRejected` arms surviving the `Error.Combine` because the band is `Expected`-derived.
- Auto: `Validate` reads the delta's `AddedEdges` and applies the closed legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no `IfcRel*` case) — a `Compose` edge with the `Contain` sub-kind requires its `Whole` to be a spatial `Object` (`IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace`/`IfcFacility`/`IfcFacilityPart`), a `Compose` edge with the `Aggregate` sub-kind may not have a `Type` object as its `Whole` (a type may not aggregate an occurrence), a `Void` edge requires its `Feature` to be an opening, an `Assign` edge with the `TypeDefinition` sub-kind requires its `Definition` to be a `Type` object; each rule lifts its failure onto `BimFault.ModelRejected` BARE and the applicative `Validation` accumulates them.
- Packages: Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new IFC-legality rule is one arm on the `Rule` switch the `Validate` fold applies; the structural invariants stay the seam's `GraphDelta` switch and never migrate here; never a per-rule validator type.
- Boundary: `IfcLegality` decides IFC-semantic legality ONLY — the structural invariants (endpoint resolution, endpoint-kind legality) are the seam's `GraphDelta` total switch and re-checking them here is the deleted form [M3]; the rules read the generic `Classification` code (`IfcSite`, `IfcOpeningElement`) and the `ObjectKind` (occurrence/type), never an `IfcProduct` runtime type (GeometryGym stays captured in the projector); the validation is applicative-accumulating so an authoring pass sees every reject at once, never the first-fail short-circuit a `Fin` rail would give; the `BimFault` lifts BARE (the band IS the `Expected` `Code`) so `error.IsType<BimFault.ModelRejected>()` survives the `Error.Combine`.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class IfcLegality : IGraphConstraint {
    static readonly FrozenSet<string> Spatial = new[] { "IfcSite", "IfcBuilding", "IfcBuildingStorey", "IfcSpace", "IfcFacility", "IfcFacilityPart" }.ToFrozenSet(StringComparer.Ordinal);
    static readonly Op Gate = Op.Of(name: nameof(IfcLegality));

    // The delta's ADDED edges are the projection's new relationships; each is validated against the running graph and the
    // applicative Validation accumulates every IFC-legality violation at once (never a first-fail short-circuit).
    public Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph) =>
        delta.AddedEdges.Map(edge => Rule(edge, graph)).Fold(
            Success<Error, Unit>(unit),
            static (acc, rule) => (acc, rule).Apply(static (_, _) => unit).As());

    // The closed IFC-legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no IfcRel* case,
    // so the rule reads the neutral Compose/Void/Assign shape, never an IFC wire-name): containment-whole-must-be-spatial,
    // a type may not aggregate, Voids feature->opening, DefinesByType definition-must-be-type.
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
            None: () => Fail<Error, Unit>(new BimFault.ModelRejected(Gate, detail)));

    static Validation<Error, Unit> RequireKind(NodeId id, ElementGraph graph, Func<ObjectKind, bool> ok, string detail) =>
        graph.Find<Node.Object>(id).Filter(obj => ok(obj.Kind)).Match(
            Some: _ => Success<Error, Unit>(unit),
            None: () => Fail<Error, Unit>(new BimFault.ModelRejected(Gate, detail)));
}
```

## [06]-[RESEARCH]

- [PROJECTION_INVERSION]: the `SemanticProjector : IElementProjection` GeometryGym-internal capture and the IoC inversion (GeometryGym stays SOLE in Bim; Bim implements the seam interface so no GeometryGym edge points down into the seam) ground against `ELEMENT-REBUILD-PLAN.md` §4A/§4C and the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `IElementProjection.Project(ProjectionContext) → Fin<GraphDelta>` contract + `Assemble` fold; the `Project` reads the live `db.Project.Extract<T>()` graph (not the lossy import-rail rows) so the full relationship roster + `OwnerHistory` + `StepHeader` survive, the member spellings (`IfcRoot.GlobalId`/`OwnerHistory`, `IfcProduct`, `IfcTypeObject`, `ParserIfc.IdentifyIfcClass(string, out string)`, `DatabaseIfc.Project`/`Release`/`ModelView`/`Tolerance`/`OriginatingFileInformation`, `FactoryIfc.Construct`(the db-binding entity mint the egress invokes, over the static `BaseClassIfc.Construct`)/`FactoryIfc.OwnerHistoryAdded`, `BaseClassIfc.Extract<T>`) decompile-verified against the live GeometryGym 25.7.30 surface (`.api/api-geometrygym-ifc`); the predefined token reads off the entity's reflected per-class `PredefinedType` enum property (the api notes it is a strongly-typed per-class member, never on the class-name split for a live occurrence), and `IfcRepresentation.Keys` is the ONE polymorphic content-keyer over `IfcObjectDefinition` (`Model/elements#REPRESENTATION_KEYS`), there being no `KeysOf`/`MapKeys` family.
- [RELATION_NEUTRALITY]: the neutral five-axis `Relationship` algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void`) plus `Generic` passthrough grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C5 — the seam carries NO typed `IfcRel*` case, so the `IfcRelKind` row carries the neutral `EdgeAxis`+`SubKind` it lowers onto, the typed case taking only its `SubKind` (`ComposeKind`/`AssignKind`/`ConnectKind`/`VoidKind` per `Rasm.Element/Relations/relation#EDGE_ALGEBRA`) and the IFC wire-name reconstructing at egress through the `ByNeutral` reverse index, while the long-tail families ride `Generic(wireName, …)`; the `Assign` family is INVERTED at ingest (the seam `Assign(Subject, Definition)` is the inverse of the IFC relating→related), the realizing `Connect` reads `IfcRelConnectsWithRealizingElements.RealizingElements` (`SET<IfcElement>`) into the seam `Connect.Realizing` option, and the material occurrence-usage `LayerSetUsage`/`ProfileSetUsage` rides the `Associate` edge `MaterialUsage` payload [C7] (the `ProfileSet` usage through the seam `MaterialUsage.ProfileSet.Of` cardinal-point gate), the `MaterialUsage.ProfileSet` private ctor forcing every admission through `Of`.
- [STRUCTURAL_ENERGY_LOWERING]: the `DefinesProperties`/`Structural`/`SpatialBoundaries` `EdgeProjection` folds lower the structural idealization and the energy/spatial CONNECTIVITY onto the seam graph so `Rasm.Compute` reads it off the neutral `Generic` edges (`ELEMENT-REBUILD-PLAN.md` §4E). The member surface is decompile-verified against the live GeometryGym 25.7.30 (`.api/api-geometrygym-ifc`): `IfcRelDefinesByProperties.RelatedObjects` (`SET<IfcObjectDefinition>`) + `RelatingPropertyDefinition` (a `SET<IfcPropertySetDefinition>`, fanned out per pair); `IfcRelConnectsStructuralMember.RelatingStructuralMember`/`RelatedStructuralConnection` and `IfcRelConnectsStructuralActivity.RelatingElement` (`IfcStructuralActivityAssignmentSelect`, read through the `IfcRoot` cast)/`RelatedStructuralActivity` are the edge endpoints the `Structural` fold resolves, the restraint/load PAYLOAD (the 6-DOF fixity + SI spring per DOF, the full `IfcStructuralLoad` family, and the neutral `LoadKind`/`Case` tokens) delegated to the dedicated `Model/structural#STRUCTURAL_PROJECTION` `StructuralProjection.Attrs` owner and the `AtStart` start/end discriminant to `StructuralProjection.AtStart` — never a local boolean-only/single-force reader; `IfcRelSpaceBoundary.RelatingSpace` (`IfcSpaceBoundarySelect`, read through the `IfcRoot` cast)/`RelatedBuildingElement` + the `IfcRelSpaceBoundary2ndLevel` discriminant (the `PhysicalOrVirtualBoundary`/`InternalOrExternalBoundary` enums are GeometryGym internal fields with no public getter, so the seam lowers only the publicly-readable boundary level, never an internal-field reflection read). The structural curve member's idealized `Object.Axis` IS baked by the `Enrich` pass off `StructuralProjection.AxisOf` (the lightweight topology-edge line); only the `Object.BoundaryPolygon` stays unextracted — materializing the heavy display polygon is the in-process geometry evaluation the projector boundary (and `Model/elements#REPRESENTATION_KEYS`) forbids, the display geometry staying content-hashed on `RepresentationContentHash` — so `Rasm.Compute` READS the baked axis off the seam rather than re-deriving it.
- [VALUE_NARROWING]: the `PropertyLowering` `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing grounds against `Rasm.Element/Properties/property#PROPERTY_VALUE` (the eight-case `Text`/`Measure`/`Boolean`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table` union) and `#PROPERTY_BAG` (the `PropertyBag`/`QuantityBag` the `Node.PropertySet`/`Node.QuantitySet` wrap) plus `Properties/quantity#MEASURE_VALUE` (the `Dimension` `[ComplexValueObject]` discriminator [H2]) — the seam forbids an `IfcValue`/dataType string crossing its signature, so the narrowing is Bim's; the decompile-verified members are `IfcPropertySingleValue.NominalValue` (`IfcValue`) + `IfcValue.Value`/`ValueType`/`ValueString` (abstract), `IfcPropertyEnumeratedValue.EnumerationValues` (`LIST<IfcValue>`), `IfcPropertyBoundedValue.UpperBoundValue`/`LowerBoundValue`/`SetPointValue`, `IfcPropertyListValue.ListValues`, `IfcPropertyTableValue.DefiningValues`/`DefinedValues`, and the `IfcQuantityLength.LengthValue`/`IfcQuantityArea.AreaValue`/`IfcQuantityVolume.VolumeValue`/`IfcQuantityWeight.WeightValue`/`IfcQuantityCount.CountValue`/`IfcQuantityTime.TimeValue` subtype values + `IfcPhysicalSimpleQuantity.Unit`; an IFC measure type the frozen `MeasureDimensions` table does not carry preserves its value as `Text` rather than claiming a wrong dimension, and the egress raises the typed value back through `RaiseProperty`/`RaiseQuantity` (the `IfcPropertySingleValue(db, name, bool|double|string)` and `IfcQuantity*(db, name, double)` ctors decompile-confirmed).
- [EGRESS_GATE]: the `PredefinedType` egress gate (resolve the `IfcClass` row from `code`, run `AdmitPredefined(token, objectType, schema, key)` against the frozen valid set + the `IntroducedIn`/`RemovedIn` schema span → `BimFault.UnmappedClass`) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C6/H8 and the `Model/elements#IFC_CLASS` `IfcClass.Resolve(string, Op)`/`AdmitPredefined(string, string, ReleaseVersion, Op)` owner (both `Op`-keyed); the schema gate ranks the SEAM `ReleaseVersion` chronologically (the `[SmartEnum]` has no ordinal), and the GeometryGym `ReleaseVersion` enum stays on the codec leg through `ReleaseRaise`/`Sniff` + `ParserIfc.EncodeGuid` for the 1:1 `GlobalId` round-trip [H6]; the diff-derived `ChangeAction` grounds against the seam `Node` record equality over the prior snapshot (a rooted node matched on the stable 1:1 GlobalId `ExternalId` since the `NodeId` is freshly minted each ingest) and `IfcOwnerHistory.CreationDate`/`LastModifiedDate` (`DateTime`, lowered through `Instant.FromDateTimeUtc`, never `FromUnixTimeSeconds`) [H9].
- [MATERIAL_PROPERTY_CLASSIFICATION_EGRESS]: the `ReauthorMaterials`/`ReauthorProperties`/`ReauthorClassifications` egress grounds against `ELEMENT-REBUILD-PLAN.md` §6 (the `Rasm.Materials` ripple — the `MaterialProjector` lowers the material subgraph, `Rasm.Bim` reads it) and §4-RT C7 (the `Associate` material edge carries the typed `MaterialUsage`; the type-level `MaterialComposition` set stays). `ReauthorMaterials` composes the `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.AuthorComposition(DatabaseIfc, Node.Material, Func<ProfileRef, Option<IfcProfileDef>>)`/`AuthorUsage(IfcMaterialDefinition, MaterialUsage)` (returning `Fin<IfcMaterialDefinition>`/`IfcMaterialSelect`); `ReauthorProperties` rebuilds the `IfcPropertySet(name, IEnumerable<IfcProperty>)`/`IfcElementQuantity(name, IEnumerable<IfcPhysicalQuantity>)` + `IfcRelDefinesByProperties(IfcObjectDefinition, IfcPropertySetDefinition)` (ctors decompile-confirmed) — the round-trip the retired stringly `BimElement.PropertyBinding`/`QuantityBinding` never had; `ReauthorClassifications` composes the `Semantics/classification#CLASSIFICATION_AXIS` `ClassificationSystem.Author(DatabaseIfc, IfcDefinitionSelect, Classification)` (returning `None` for the `"ifc"` entity-type code), authoring `IfcRelAssociatesClassification`/`IfcClassificationReference`; the `IfcRelAssociatesMaterial(IfcMaterialSelect, IEnumerable<IfcDefinitionSelect>)` egress is decompile-confirmed, the material reading off the projected seam `Material` node, never a retired Materials wire.
- [LEGALITY_SPLIT]: the `IfcLegality : IGraphConstraint` IFC-semantic legality (containment-relating-must-be-spatial / `Void` element→opening / type-may-not-aggregate-occurrence / `DefinesByType` definition-must-be-type) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M3 — net-new Rasm interfaces = 2 (`IElementProjection` + `IGraphConstraint`); the seam's structural `GraphDelta` switch enforces only endpoint-resolution and endpoint-kind legality, the IFC legality depended up on through the constraint contract and accumulated applicatively over `Validation<Error,Unit>`, the `BimFault.ModelRejected` lifting BARE (band 2600 IS the `Expected` `Code` per `Model/faults#FAULT_BAND`) so the arm survives the `Error.Combine`.
