# [BIM_SEMANTIC_PROJECTOR]

`Rasm.Bim` is the SOLE GeometryGym/IFC owner and the IFC arm of the `Rasm.Element` seam. This page owns the INGRESS half of the one `SemanticProjector : IElementProjection` that lowers a live GeometryGym `DatabaseIfc` into a seam `GraphDelta` (the `Project` fold), the `PropertyLowering` value-narrowing the seam delegates to it, and the `IfcLegality : IGraphConstraint` that decides IFC-semantic legality the seam's structural `GraphDelta` switch cannot. The relationship-lowering half (`IfcRelKind`/`EdgeProjection`) lives at `Projection/relations#RELATION_ALGEBRA` and the IFC re-author half (`Emit`/`Sniff`) at `Projection/egress#IFC_EGRESS` — the SAME `partial class SemanticProjector`, split by concern: `Project` composes `EdgeProjection.All` to land the neutral edges, and `Emit` reverses both the node lowering and the relation roster. The projector replaces the retired `BimModel.Project`/`BimElement` fold: where the old owner produced a second stored element record keyed by GlobalId, the projector produces seam `Node`s (`Object` occurrence/type, `PropertySet`, `QuantitySet`, `Material`) and neutral `Relationship` edges that `Assemble` folds into the canonical `ElementGraph`, so "has it all" is one `Bake` read on the seam graph and GeometryGym never leaks below the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the kernel geometry by content-hash reference, never a RhinoCommon type, never an in-process BRep evaluation.

The element identity is established HERE (the IFC is the source of element identity for an ingested model): `Project` mints a NEUTRAL rooted `NodeId` per `IfcRoot` and records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute, publishing the minted ids in the delta for the `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` to attach `Associate` edges against — the owner-mints-its-identity law meeting the seam: Materials mints the deterministic-rooted Type `Object` from a `Component`'s canonical content, Bim ingests `IfcTypeObject` into the SAME `ObjectKind.Type` node and `IfcElement` into an Occurrence, one Type representation authored and ingested unified. The two `ReleaseVersion`/`ModelView` worlds meet at the projector and nowhere else: the seam `ReleaseVersion`/`ModelView` `[SmartEnum]`s are the model `Header` currency, and the GeometryGym `ReleaseVersion`/`ModelView` enums are the IFC-text codec leg `Project`/`Emit`/`Sniff` own — the page aliases the GeometryGym pair (`GGRelease`/`GGView`) so the unqualified names resolve to the seam, and `ReleaseLower`/`ReleaseRaise` are the one lowering pair the leak never escapes.

## [01]-[INDEX]

- [01]-[SEMANTIC_PROJECTOR]: `SemanticProjector : IElementProjection`, the `Project` fold lowering `DatabaseIfc` into a `GraphDelta` — rooted `NodeId` mint with the 1:1 IFC `GlobalId` projection attribute [H6], the `Object` occurrence/type nodes carrying the generic `Classification`/`PredefinedType`/`RepresentationContentHash`, the `PropertySet`/`QuantitySet` bag nodes whose typed `PropertyValue`/`MeasureValue` the `PropertyLowering` narrowing fills and whose `InheritanceMode` is stamped at ingest [H1], the `OwnerHistory`/`StepHeader` projection [H9], and the schema span [H8]; the relationship lowering composes `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.All`.
- [02]-[GRAPH_LEGALITY]: `IfcLegality : IGraphConstraint` the IFC-semantic legality validator — containment-relating-must-be-spatial, `Void` element→opening, type-may-not-aggregate-occurrence, `DefinesByType` definition-must-be-type — accumulating onto `Validation<Error,Unit>` over the seam's structural invariants [M3].

## [02]-[SEMANTIC_PROJECTOR]

- Owner: `SemanticProjector` the `IElementProjection` capturing one live GeometryGym `DatabaseIfc` internally and lowering it to a seam `GraphDelta` in `Project`; `PropertyLowering` the Bim-internal value-narrowing the seam delegates to it (the seam forbids an IFC `IfcValue`/dataType crossing its signature, so the `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing is Bim's); `OwnerStamp` the `IfcOwnerHistory`→seam `OwnerHistory` projection; `StepHeaderOf` the `STEPFileInformation`→seam `StepHeader` projection; `ReleaseLower`/`ViewLower` the GeometryGym→seam currency lowering.
- Entry: `SemanticProjector.Project(ProjectionContext ctx)` folds the captured `DatabaseIfc` into one `GraphDelta` over `ctx.Key` — it mints a NEUTRAL rooted `NodeId` per `IfcRoot` through the kernel static `Rasm.Element/Graph/element#NODE_MODEL` `NodeId.Rooted()` mint (the `IObjectFactory` floor — `ProjectionContext` exposes only `For`/`Owns`, never a mint pass-through), records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute [H6], and content-keys every non-rooted material node through `MaterialProjection.Project`'s kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`; `Fin<T>` aborts on a missing `IfcProject` root or a dangling spatial host (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`), the ingress class lookup PERMISSIVE — an unrostered/IFC4-new leaf lands the `Model/elements#IFC_CLASS` `IfcClass.Proxy` row through `TryGet().IfNone(Proxy)` so one unknown entity never aborts the import, class validity deferred to the `Emit` egress gate [C6][H8] — the fault lifting BARE (the band IS the `Expected` `Code`, no `.ToError()` hop). The element identity is established HERE (the IFC is the source of element identity), so the projector ignores `ctx.ElementIds` (the aspect-projector NodeId set) and PUBLISHES the minted ids in the delta for `Rasm.Materials/Projection/component#COMPONENT_PROJECTOR` to attach `Associate` edges against.
- Auto: `Project` walks the captured `db.Project` once — `ObjectNode` lands every `IfcProduct`→`Object.Occurrence` and `IfcTypeObject`→`Object.Type` node carrying the generic `Classification("ifc", classKey)` (the IFC entity type as a classification, never `IfcClass` on the node) resolved through the permissive `IfcClass.TryGet().IfNone(Proxy)` ingress, the `PredefinedType` token read off the entity's per-class predefined property, the keyed `RepresentationContentHash` map (`Model/elements#REPRESENTATION_KEYS` `IfcRepresentation.Keys`, ONE polymorphic content-keyer over `IfcObjectDefinition`) [M2], the `OwnerStamp` `OwnerHistory` [H9], and the `IfcClass.Span` schema window [H8]; `Bags` lands `IfcPropertySet`/`IfcElementQuantity`→`PropertySet`/`QuantitySet` bag nodes whose typed values the `PropertyLowering` narrowing fills and whose `Semantics/properties#PROPERTY_TEMPLATES` `PropertyInheritance.ModeOf` `InheritanceMode` is stamped at ingest [H1] so the seam `Bake` applies type→occurrence precedence wholly within the seam; `Materials` lands `Material` nodes through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.Project`; the analytical Axis/FootPrint geometry is content-keyed in `Representations` by `IfcRepresentation.Keys` (never inlined on the node), `Rasm.Compute` resolving it one-hop by content key from the blob store; `GeoReferenceProjector.Project` lands the `Header.GeoReference` [M1]; `EdgeProjection.All` lands every `IfcRel*` neutral edge [C5] — the decomposition/connection/assignment/void families, the property/quantity attachment, the structural member↔connection/member↔activity `Generic` edges (the `StructuralProjection.Attrs` 6-DOF restraint + full load family + `LoadKind`/`Case` and the `AtStart` discriminant riding the payload), the space↔surface `Generic` edges, and the material `Associate` edges with the occurrence-usage payload [C7].
- Receipt: the `GraphDelta` is the projector's whole contribution — a merge over the canonical `ElementGraph` that `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` folds with the other projectors' deltas; the minted rooted-`NodeId` set keyed by `GlobalId` is the identity table aspect projectors attach against and `Emit` reverses.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new extracted IFC entity family is one `Extract<T>` arm on the `Project` fold landing its seam node; a new IFC value kind is one `PropertyLowering` arm; a new relationship is one `IfcRelKind` row the `EdgeProjection` reads (`Projection/relations#RELATION_ALGEBRA`); a new schema version is one `ReleaseVersion` the `ReleaseLower` resolves and the `Model/elements#IFC_CLASS` span validates; never a second element record beside the seam graph and never a per-entity projector type.
- Boundary: the projector is the ONE GeometryGym→seam lowering — the retired `BimModel.Project` produced a second stored `BimElement` keyed by `GlobalId`, and any owner that re-stores the element off the seam graph is the deleted form; GeometryGym is captured INTERNALLY (the `DatabaseIfc` field) and an `IfcProduct`/`IfcRel*`/`DatabaseIfc` type crossing the `IElementProjection.Project` signature is the named seam violation — the seam holds only `Node`/`Relationship`/`GraphDelta`; the rooted `NodeId` is a neutral kernel-minted id and the compressed IFC `GlobalId` is the node's `ExternalId` projection attribute (1:1) [H6], so the IFC GUID never becomes the node identity and the from-scratch authoring path mints its own neutral id; the value-narrowing is Bim's (`PropertyLowering`) because an `IfcValue`/dataType string crossing a seam signature is the deleted form — the seam carries only the typed `PropertyValue`/`MeasureValue` cases; geometry is referenced by `RepresentationContentHash` only [M2] and an in-process BRep evaluation or a RhinoCommon handle is the named seam violation — the analytical Axis/FootPrint geometry is content-keyed in `Representations` by `IfcRepresentation.Keys` [M2] and NEVER inlined as a coordinate field on the `Object` node (an inline `Vector3`/`BoundaryPolygon`/`Axis` member is the deleted §4-RT-M2 violation), `Rasm.Compute` resolving the analytical axis/footprint one-hop by content key from the blob store; a Bim in-process BRep evaluation is the named seam violation and the seam carries the structural/spatial CONNECTIVITY on the neutral `Relationship.Generic` edges instead; `Emit` is a Bim-INTERNAL method on the projector, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the seam owns only ingress projection.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using System.Text;
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
            .Fold(Map<string, NodeId>(), static (map, root) => map.AddOrUpdate(root.GlobalId, NodeId.Rooted()));
        return GeoReferenceProjector.Project(project).Bind(geo => {
            var header = new Header(ReleaseLower(db.Release), ViewLower(db.ModelView), geo, db.Tolerance, ctx.At, StepHeaderOf(db));
            Seq<Node> nodes = Classify(project, rooted, Objects(project, rooted).Concat(Bags(project, rooted)).Concat(Materials(project, db.Tolerance, key)).ToSeq());
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
    // The analytical Axis/FootPrint geometry is content-keyed in Representations by IfcRepresentation.Keys (the ONE polymorphic
    // representation content-keyer maps every RepresentationIdentifier — Axis/Body/Box/FootPrint — to its content hash [M2]),
    // NEVER inlined as a coordinate field on the seam Object node (no Vector3/AxisCurve member exists — the deleted §4-RT-M2
    // violation); Rasm.Compute RESOLVES the analytical axis/footprint one-hop BY CONTENT KEY from the blob store.
    static Seq<Node> Objects(IfcProject project, Map<string, NodeId> rooted) =>
        project.Extract<IfcProduct>().AsIterable().Map(p => ObjectNode(p, ObjectKind.Occurrence, rooted))
            .Concat(project.Extract<IfcTypeObject>().AsIterable().Map(t => ObjectNode(t, ObjectKind.Type, rooted)))
            .ToSeq();

    // The standard-system classification set [4-RT cardinality]: IFC permits MULTIPLE IfcRelAssociatesClassification per
    // object (Uniclass + OmniClass co-applied), so each relation's IfcClassificationReference resolves through
    // Semantics/classification#CLASSIFICATION_AXIS ClassificationSystem.Ingest (lowering the IfcClassificationReference.Name
    // concept title onto seam Classification.Title) and accumulates onto every related rooted Object node's Classifications
    // set — the ("ifc", classKey) entity-class pair stays the node's PRIMARY Classification, the standard refs ride the set;
    // an unrostered source resolves None and is dropped here (it rides the relation-edge Generic passthrough), never a wrong
    // lowering. RelatingClassification (IfcClassificationSelect) + RelatedObjects (SET<IfcDefinitionSelect>) decompile-verified.
    static Seq<Node> Classify(IfcProject project, Map<string, NodeId> rooted, Seq<Node> nodes) {
        Map<NodeId, Seq<Classification>> byNode = project.Extract<IfcRelAssociatesClassification>().AsIterable()
            .Fold(Map<NodeId, Seq<Classification>>(), (map, rel) =>
                Optional(rel.RelatingClassification as IfcClassificationReference)
                    .Bind(ClassificationSystem.Ingest)
                    .Match(
                        Some: c => rel.RelatedObjects.OfType<IfcRoot>().Aggregate(map, (acc, related) =>
                            rooted.Find(related.GlobalId).Match(
                                Some: id => acc.AddOrUpdate(id, existing => existing.Add(c), () => Seq(c)),
                                None: () => acc)),
                        None: () => map));
        return byNode.IsEmpty
            ? nodes
            : nodes.Map(node => node is Node.Object o
                ? (Node)(o with { Classifications = byNode.Find(o.Id).IfNone(o.Classifications) })
                : node);
    }

    static Node ObjectNode(IfcObjectDefinition definition, ObjectKind kind, Map<string, NodeId> rooted) {
        IfcClass cls = IfcClass.TryGet(ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _)).IfNone(IfcClass.Proxy);
        return new Node.Object(
            Id:             rooted[definition.GlobalId],
            Kind:           kind,
            ExternalId:     Some(definition.GlobalId),
            Classification: Classification.Create("ifc", cls.Key, "", "", None, None),
            PredefinedType: Predefined(definition),
            Name:           definition.Name ?? "",
            Tag:            (definition as IfcElement)?.Tag ?? "",
            Representations: IfcRepresentation.Keys(definition),
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
                    (bag, p) => bag.AddOrUpdate(PropertyName.Create(p.Name ?? ""), PropertyLowering.Lower(p, rooted))),
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
                Schema:        Seq(database.Release.ToString()))
            : StepHeader.Empty with { Schema = Seq(database.Release.ToString()) };

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
// ten-case PropertyValue and an IfcPhysicalSimpleQuantity onto a MeasureValue over the seam Dimension. The seam forbids
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

    // The IfcProperty family -> the seam PropertyValue ten-case union: a single value narrows by its IfcValue shape (the
    // three-valued IfcLogical to the seam Logical, never coerced to a two-valued Boolean), an enumerated value carries its
    // SELECTED value LIST (EnumerationValues, the [1:?] cardinality) plus its allowed set (the optional EnumerationReference),
    // a reference value its target NodeId plus its UsageName, a bounded value its lower/upper/setpoint measures, a table value
    // its rows plus the IfcCurveInterpolationEnum curve rule, a list the recursive arm, and an IfcComplexProperty its UsageName
    // plus its named sub-property bag (HasProperties keyed by each sub-property Name) RECURSING Lower — so a layered glazing /
    // multi-component rating / bSDD complex template is the seam Complex arm, never dropped to Text; only a non-IfcProperty
    // residue falls to Text. The rooted map resolves a reference whose target is a rooted node; a non-rooted reference target
    // content-keys a NEUTRAL NodeId (never the IFC GlobalId AS node identity [H6]), the UsageName always carried so the
    // import->export round-trip never drops the three-valued logical, the curve rule, the usage name, or the nested bag.
    public static PropertyValue Lower(IfcProperty property, Map<string, NodeId> rooted) => property switch {
        IfcPropertySingleValue sv     => LowerValue(sv.NominalValue),
        IfcPropertyEnumeratedValue ev => new PropertyValue.Enumerated(
            ev.EnumerationValues.AsIterable().Map(static v => v.ValueString).ToSeq(),
            Optional(ev.EnumerationReference).Map(static r => r.EnumerationValues.AsIterable().Map(static v => v.ValueString).ToSeq()).IfNone(Seq<string>())),
        IfcPropertyReferenceValue rv  => new PropertyValue.Reference(
            Optional(rv.PropertyReference as IfcRoot).Bind(root => rooted.Find(root.GlobalId)).IfNone(() =>
                NodeId.Content(Encoding.UTF8.GetBytes(rv.PropertyReference is IfcRoot r ? $"ifcroot:{r.GlobalId}" : $"{rv.PropertyReference?.GetType().Name}:{rv.UsageName}"))),
            string.IsNullOrEmpty(rv.UsageName) ? Option<string>.None : Some(rv.UsageName)),
        IfcPropertyBoundedValue bv    => new PropertyValue.Bounded(MeasureOpt(bv.LowerBoundValue), MeasureOpt(bv.UpperBoundValue), MeasureOpt(bv.SetPointValue)),
        IfcPropertyListValue lv       => new PropertyValue.List(lv.ListValues.AsIterable().Map(LowerValue).ToSeq()),
        IfcPropertyTableValue tv      => new PropertyValue.Table(tv.DefiningValues.Zip(tv.DefinedValues,
            static (def, val) => ((PropertyValue)new PropertyValue.Text(def.ValueString), (PropertyValue)new PropertyValue.Text(val.ValueString))).ToSeq(),
            InterpolationOf(tv.CurveInterpolation)),
        IfcComplexProperty cp         => new PropertyValue.Complex(cp.UsageName,
            cp.HasProperties.Values.Aggregate(Map<PropertyName, PropertyValue>(),
                (bag, sub) => bag.AddOrUpdate(PropertyName.Create(sub.Name ?? ""), Lower(sub, rooted)))),
        _                             => new PropertyValue.Text(property.Name ?? ""),
    };

    // An IfcValue -> the seam PropertyValue scalar arm: a three-valued IfcLogical is the seam Logical (UNKNOWN -> None, never
    // coerced to a two-valued Boolean), a boolean-typed value (ValueType == bool) is Boolean, a measure value whose type the
    // dimension table carries is a typed Measure over its SI base, every other value its verbatim string — the
    // IfcValue.ValueType/Value/ValueString abstract members the narrowing reads off any IfcValue.
    static PropertyValue LowerValue(IfcValue? value) =>
        value is null                                                                                  ? new PropertyValue.Text("")
        : value is IfcLogical lg                                                                       ? new PropertyValue.Logical(LogicalOpt(lg.Logical))
        : value.ValueType == typeof(bool)                                                              ? new PropertyValue.Boolean(value.Value is bool b && b)
        : value is IfcMeasureValue m && MeasureDimensions.TryGetValue(m.GetType().Name, out var dim)   ? new PropertyValue.Measure(MeasureOf(m, dim))
        : new PropertyValue.Text(value.ValueString);

    static Option<MeasureValue> MeasureOpt(IfcValue? value) =>
        value is IfcMeasureValue m && MeasureDimensions.TryGetValue(m.GetType().Name, out var dim)
            ? Some(MeasureOf(m, dim))
            : None;

    // The IfcMeasureValue (already SI-base, the dimension resolved off the frozen MeasureDimensions table) -> the seam
    // MeasureValue through the SI-native OfSi factory: the QuantityType is the IFC MEASURE-TYPE NAME (IfcThermalTransmittanceMeasure,
    // IfcMassDensityMeasure, ...), NOT the dimension, because the seven-exponent vector is not injective over quantity types
    // (an IfcForceMeasure and an out-of-family measure can share a dimension) — so the measure-type identity round-trips and a
    // QTO accessor never false-matches. The kernel UnitsNet registry is bypassed (the IFC value is already coerced); a measure
    // type the frozen table does not carry stays Text upstream rather than claiming a wrong dimension.
    static MeasureValue MeasureOf(IfcMeasureValue measure, Dimension dimension) =>
        MeasureValue.OfSi(QuantityType.Create(measure.GetType().Name), dimension, AsDouble(measure.Value));

    // The three-valued IfcLogical -> the seam Logical's Option<bool>: TRUE/FALSE map to Some, UNKNOWN to None so the seam
    // models the third state a bool cannot; the egress RaiseLogical reverses it.
    static Option<bool> LogicalOpt(IfcLogicalEnum logical) => logical switch {
        IfcLogicalEnum.TRUE  => Some(true),
        IfcLogicalEnum.FALSE => Some(false),
        _                    => None,
    };

    // The IfcCurveInterpolationEnum -> the seam Interpolation token a Table value carries so a lookup-table consumer reads
    // the curve rule rather than re-inferring it; the egress RaiseInterp reverses it.
    static Interpolation InterpolationOf(IfcCurveInterpolationEnum curve) => curve switch {
        IfcCurveInterpolationEnum.LINEAR     => Interpolation.Linear,
        IfcCurveInterpolationEnum.LOG_LINEAR => Interpolation.LogLinear,
        IfcCurveInterpolationEnum.LOG_LOG    => Interpolation.LogLog,
        _                                    => Interpolation.NotDefined,
    };

    // An IfcPhysicalSimpleQuantity -> the seam MeasureValue [H2]: the IFC base-quantity value is ALREADY SI-base, so it
    // admits through the SI-native MeasureValue.OfSi(QuantityType, Dimension, double) factory (never the UnitsNet
    // Quantity.TryFrom/ToUnit registry, which is for raw unit-bearing wire values) — each subtype stamps its named
    // QuantityType so a QTO accessor (measure.Area/Volume/Weight/Time/Count) reads the geometry-true takeoff type-tagged
    // at ingest, never inferred from the dimension. The count is a dimensionless tally (QuantityType.Count), and an
    // unrecognized simple quantity yields the dimensionless MeasureValue.Zero.
    public static MeasureValue Measure(IfcPhysicalSimpleQuantity quantity) => quantity switch {
        IfcQuantityLength q => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, q.LengthValue),
        IfcQuantityArea q   => MeasureValue.OfSi(QuantityType.Area, Dimension.AreaDim, q.AreaValue),
        IfcQuantityVolume q => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, q.VolumeValue),
        IfcQuantityWeight q => MeasureValue.OfSi(QuantityType.Mass, Dimension.MassDim, q.WeightValue),
        IfcQuantityCount q  => MeasureValue.OfSi(QuantityType.Count, Dimension.Dimensionless, q.CountValue),
        IfcQuantityTime q   => MeasureValue.OfSi(QuantityType.Duration, Dimension.DurationDim, q.TimeValue),
        _                   => MeasureValue.Zero,
    };

    static double AsDouble(object? value) =>
        value is IConvertible c ? Convert.ToDouble(c, System.Globalization.CultureInfo.InvariantCulture) : 0.0;
}
```

## [03]-[GRAPH_LEGALITY]

- Owner: `IfcLegality` the `IGraphConstraint` deciding IFC-semantic legality the seam's structural `GraphDelta` switch cannot [M3] — the seam enforces only structural invariants (an edge endpoint resolves, an endpoint kind is legal), and IFC legality (which entity may relate to which) is Bim's, depended UP on through the `IGraphConstraint` contract.
- Entry: `IfcLegality.Validate(GraphDelta delta, ElementGraph graph) → Validation<Error,Unit>` accumulates every IFC-legality violation in the delta against the graph — `Success(unit)` when every rule holds, a `Fail` carrying the accumulated `Error` set otherwise; the validation is applicative (every violation reported at once, never short-circuit) so an authoring pass sees all rejects in one apply, the `BimFault.ModelRejected` arms surviving the `Error.Combine` because the band is `Expected`-derived.
- Auto: `Validate` reads the delta's `AddedEdges` and applies the closed legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no `IfcRel*` case) — a `Compose` edge with the `Contain` sub-kind requires its `Whole` to be a spatial `Object` (`IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace`/`IfcFacility`/`IfcFacilityPart`), a `Compose` edge with the `Aggregate` sub-kind may not have a `Type` object as its `Whole` (a type may not aggregate an occurrence), a `Void` edge requires its `Feature` to be an opening, an `Assign` edge with the `TypeDefinition` sub-kind requires its `Definition` to be a `Type` object; each rule lifts its failure onto `BimFault.ModelRejected` BARE and the applicative `Validation` accumulates them.
- Packages: Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new IFC-legality rule is one arm on the `Rule` switch the `Validate` fold applies; the structural invariants stay the seam's `GraphDelta` switch and never migrate here; never a per-rule validator type.
- Boundary: `IfcLegality` decides IFC-semantic legality ONLY — the structural invariants (endpoint resolution, endpoint-kind legality) are the seam's `GraphDelta` total switch and re-checking them here is the deleted form [M3]; the rules read the generic `Classification` code (`IfcSite`, `IfcOpeningElement`) and the `ObjectKind` (occurrence/type), never an `IfcProduct` runtime type (GeometryGym stays captured in the projector); the validation is applicative-accumulating so an authoring pass sees every reject at once, never the first-fail short-circuit a `Fin` rail would give; the `BimFault` lifts BARE (the band IS the `Expected` `Code`) so `error.IsType<BimFault.ModelRejected>()` survives the `Error.Combine`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// IfcLegality is the IGraphConstraint half the seam composes after its structural GraphDelta law [M3];
// it reads the seam NEUTRAL Relationship case + the generic Classification/ObjectKind, never a GeometryGym type.
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

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

## [04]-[RESEARCH]

- [PROJECTION_INVERSION]: the `SemanticProjector : IElementProjection` GeometryGym-internal capture and the IoC inversion (GeometryGym stays SOLE in Bim; Bim implements the seam interface so no GeometryGym edge points down into the seam) ground against `ELEMENT-REBUILD-PLAN.md` §4A/§4C and the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `IElementProjection.Project(ProjectionContext) → Fin<GraphDelta>` contract + `Assemble` fold; the `Project` reads the live `db.Project.Extract<T>()` graph (not the lossy import-rail rows) so the full relationship roster + `OwnerHistory` + `StepHeader` survive, the member spellings (`IfcRoot.GlobalId`/`OwnerHistory`, `IfcProduct`, `IfcTypeObject`, `ParserIfc.IdentifyIfcClass(string, out string)`, `DatabaseIfc.Project`/`Release`/`ModelView`/`Tolerance`/`OriginatingFileInformation`, `FactoryIfc.Construct`(the db-binding entity mint the egress invokes, over the static `BaseClassIfc.Construct`)/`FactoryIfc.OwnerHistoryAdded`, `BaseClassIfc.Extract<T>`) decompile-verified against the live GeometryGym 25.7.30 surface (`.api/api-geometrygym-ifc`); the predefined token reads off the entity's reflected per-class `PredefinedType` enum property (the api notes it is a strongly-typed per-class member, never on the class-name split for a live occurrence), and `IfcRepresentation.Keys` is the ONE polymorphic content-keyer over `IfcObjectDefinition` (`Model/elements#REPRESENTATION_KEYS`), there being no `KeysOf`/`MapKeys` family.
- [VALUE_NARROWING]: the `PropertyLowering` `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing grounds against `Rasm.Element/Properties/property#PROPERTY_VALUE` (the ten-case `Text`/`Measure`/`Boolean`/`Logical`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex` union) and `#PROPERTY_BAG` (the `PropertyBag`/`QuantityBag` the `Node.PropertySet`/`Node.QuantitySet` wrap) plus `Properties/quantity#MEASURE_VALUE` (the `Dimension` `[ComplexValueObject]` discriminator + the SI-native `MeasureValue.OfSi(QuantityType, Dimension, double)` factory [H2]) — the seam forbids an `IfcValue`/dataType string crossing its signature, so the narrowing is Bim's; the decompile-verified members are `IfcPropertySingleValue.NominalValue` (`IfcValue`) + `IfcValue.Value`/`ValueType`/`ValueString` (abstract), `IfcLogical.Logical` (`IfcLogicalEnum` `TRUE`/`FALSE`/`UNKNOWN`), `IfcPropertyEnumeratedValue.EnumerationValues` (`LIST<IfcValue>`) + `EnumerationReference` (`IfcPropertyEnumeration.EnumerationValues`), `IfcPropertyReferenceValue.UsageName`/`PropertyReference` (`IfcObjectReferenceSelect`), `IfcPropertyBoundedValue.UpperBoundValue`/`LowerBoundValue`/`SetPointValue`, `IfcPropertyListValue.ListValues`, `IfcPropertyTableValue.DefiningValues`/`DefinedValues`/`CurveInterpolation` (`IfcCurveInterpolationEnum`), `IfcComplexProperty.HasProperties` (`Dictionary<string, IfcProperty>`, recursed into the seam `Complex` arm keyed by each sub-property `Name`)/`UsageName` (`string`), and the `IfcQuantityLength.LengthValue`/`IfcQuantityArea.AreaValue`/`IfcQuantityVolume.VolumeValue`/`IfcQuantityWeight.WeightValue`/`IfcQuantityCount.CountValue`/`IfcQuantityTime.TimeValue` subtype values + `IfcPhysicalSimpleQuantity.Unit` (the base quantity ALREADY SI-base, admitted through `MeasureValue.OfSi` stamping the named `QuantityType`, never the UnitsNet registry); an IFC measure type the frozen `MeasureDimensions` table does not carry preserves its value as `Text` rather than claiming a wrong dimension, and the `Projection/egress#IFC_EGRESS` egress raises the typed value back through `RaiseProperty`/`RaiseQuantity` — a three-valued `Logical` re-emits a typed `IfcLogical`, an `Enumerated` its `IfcPropertyEnumeratedValue` selected list, a `Reference` an `IfcPropertyReferenceValue` carrying its `UsageName`, a `Table` an `IfcPropertyTableValue` carrying its `CurveInterpolation` — so the three-valued logical, the curve-interpolation rule, and the reference usage name survive the round-trip (the `IfcPropertySingleValue(db, name, IfcValue|bool|double|string)`, `IfcPropertyEnumeratedValue(db, name, IEnumerable<IfcValue>)`, `IfcPropertyReferenceValue(db, name)` + `UsageName`, `IfcPropertyTableValue(db, name)` + `CurveInterpolation`, `IfcLogical(IfcLogicalEnum)`, `IfcLabel(string)`/`IfcReal(double)`, and `IfcQuantity*(db, name, double)` ctors decompile-confirmed).
- [LEGALITY_SPLIT]: the `IfcLegality : IGraphConstraint` IFC-semantic legality (containment-relating-must-be-spatial / `Void` element→opening / type-may-not-aggregate-occurrence / `DefinesByType` definition-must-be-type) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M3 — net-new Rasm interfaces = 2 (`IElementProjection` + `IGraphConstraint`); the seam's structural `GraphDelta` switch enforces only endpoint-resolution and endpoint-kind legality, the IFC legality depended up on through the constraint contract and accumulated applicatively over `Validation<Error,Unit>`, the `BimFault.ModelRejected` lifting BARE (band 2600 IS the `Expected` `Code` per `Model/faults#FAULT_BAND`) so the arm survives the `Error.Combine`.
