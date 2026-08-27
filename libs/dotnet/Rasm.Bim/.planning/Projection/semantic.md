# [BIM_SEMANTIC_PROJECTOR]

`Rasm.Bim` is the SOLE GeometryGym/IFC owner and the IFC arm of the `Rasm.Element` contract. This page owns the INGRESS half of the one `SemanticProjector : IElementProjection` that lowers a live GeometryGym `DatabaseIfc` into a shared `GraphDelta`, and the `IfcLegality : IGraphConstraint` that decides IFC-semantic RELATIONSHIP legality the contract's structural `GraphDelta` switch cannot. The projector produces shared `Node`s (`Object` occurrence/type, `PropertySet`, `QuantitySet`, `Material`) and neutral `Relationship` edges that `Assemble` folds into the canonical `ElementGraph`, so "has it all" is one `Bake` read on the element graph and GeometryGym never leaks below the boundary. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds kernel geometry by content-hash reference, never a RhinoCommon type and never an in-process BRep evaluation.

Element identity is established HERE — the IFC is the source of element identity for an ingested model — so `Project` mints a NEUTRAL rooted `NodeId` per `IfcRoot` and records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute [H6]; `IfcTypeObject` identity admits through `IIfcTypeReconciler`, a resolver hit reusing the canonical Materials Type Object and a miss keeping the IFC type imported with preserved material/profile signatures in an `EvidenceGrade.Import` bag. The relationship-lowering half is `Projection/relations#RELATION_ALGEBRA`, the IFC re-author half `Projection/egress#IFC_EGRESS`, the value and unit lowering `Projection/value#PROPERTY_LOWERING` and `Projection/value#UNIT_INGRESS`, and the drop ledger `Projection/fidelity#FIDELITY_LEDGER` — the SAME `partial class SemanticProjector`, split by concern. Both `ReleaseVersion`/`ModelView` worlds meet at this projector and nowhere else: the page aliases the GeometryGym pair (`GGRelease`/`GGView`) so unqualified names resolve to the contract, and `ReleaseLower`/`ReleaseRaise` are the one lowering pair result-returning through the frozen `Model/elements#IFC_CLASS` `ReleaseMap`.

## [01]-[INDEX]

- [02]-[SEMANTIC_PROJECTOR]: `SemanticProjector : IElementProjection` and its `Ingest` fold lowering a live `DatabaseIfc` into one shared `GraphDelta` beside its `FidelityLog` — the neutral rooted identity mint [H6], `IIfcTypeReconciler`/`IIfcProfileStore` type admission, the one `[Mapper]` entity-to-`Node.Object` mapper, the `PropertySet`/`QuantitySet`/`Material` bag lowering, and the declared `UnitScheme` regime every magnitude coerces through.
- [03]-[GRAPH_LEGALITY]: `IfcLegality : IGraphConstraint` deciding IFC RELATIONSHIP legality over the delta's added edges — the spatial-containment, aggregation, void/fill, and type-definition rules, resolved against `delta.AddedNodes ∪ graph` and accumulating onto `Validation<Error,Unit>` [M3].

## [02]-[SEMANTIC_PROJECTOR]

- Owner: `SemanticProjector` the `IElementProjection` capturing one live `DatabaseIfc`, the `IIfcProfileStore`, the `Semantics/properties#PROPERTY_TEMPLATES` `TemplateScope`, the `EurocodePolicy` regime, and the `BsddPins` hosted-dictionary pins internally — every one a ctor-held capability, never a per-fold parameter; `IfcSeat` the four computed columns one entity read yields; `IfcBoundaryMapper` the ONE `[Mapper]` crossing a GG entity to a shared `Node.Object`; `IfcIngest` the shared `GraphDelta` under construction beside the `GlobalId`-keyed identity table; `OwnerStamp`/`StepHeaderOf` the `IfcOwnerHistory`/`STEPFileInformation` projections; `ReleaseLower`/`ViewLower` the ingress schema and MVD lowerings.
- Entry: `SemanticProjector.Ingest(ProjectionContext ctx)` is the Bim-internal fold returning `Fin<(GraphDelta Delta, FidelityLog Fidelity)>` — the delta AND the run's whole ledger as ONE value; `SemanticProjector.Project(ctx)` is the shared member, the same fold with the ledger dropped because `IElementProjection` holds only `Fin<GraphDelta>`. It mints a NEUTRAL rooted `NodeId` per `IfcRoot` through the kernel `Rasm.Element/Graph/element#NODE_MODEL` `NodeId.Of(new NodeSeed.Placement())` mint (`ProjectionContext` exposes only `For`/`Owns`, never a mint pass-through), records the compressed `GlobalId` as the node's 1:1 `ExternalId` [H6], reconciles each `IfcTypeObject`, preserves imported material/profile signatures through `IIfcProfileStore`, and content-keys every non-rooted material node through `MaterialProjection.Project`. Element identity is established here, so the projector ignores `ctx.ElementIds` and PUBLISHES the minted ids for sibling projectors to attach `Associate` edges against.
- Law: the ledger is the fold's OWN state — every drop-capable leg returns `WriterT<FidelityLog, Fin, A>` and the ledger is a RETURNED value, so one fact has one owner; the mutable field beside a returned value it replaces gave the ledger two write orders that diverged on any partial or repeated run. The four legs after the rooted map — connection details, bags, materials, edges — are INDEPENDENT and accumulate applicatively, so one malformed Pset no longer hides a malformed material, a dangling edge, and a broken connection detail behind a first-fail result.
- Auto: `Ingest` walks the captured `db.Project` once — the occurrence sweep is OBJECT-DEFINITION-WIDE, not product-wide, because the `IfcProject` context root (the `Model/spatial#SPATIAL_STRUCTURE` tree's `SpatialClass.IsRoot` node), the `IfcGroup` subtree the `Model/zones#ZONE_GRAPH` overlay reads, and the process/control/actor/resource families the rostered assignment and sequence edges reference all need nodes, and a product-only sweep stranded every such edge on a nodeless endpoint. `Bags` lands EVERY rooted `IfcPropertySetDefinition` — `IfcPropertySet`, the `IfcPreDefinedPropertySet` family (each concrete minting its node so the already-landed `DefinesProperties` `Assign` edge never dangles), and `IfcElementQuantity` whose `IfcPhysicalComplexQuantity` children flatten value-lossless under dot-path keys beside one prefix-keyed `Properties/property#PROPERTY_BAG` `GroupIdentity` row per complex group — each bag stamped with the `PropertyInheritance.ModeOf` `InheritanceMode` at ingest [H1] under the ctor-held `TemplateScope`, so the shared `Bake` applies type-to-occurrence precedence wholly within the contract. `Materials` lands `Material` nodes and their imported `HasProperties` Psets as content-minted bag nodes under `EvidenceGrade.Import`; `SourceBag` synthesizes the entity-attribute Import bags (`IfcDistributionPort` flow pair, the `Model/structural#STRUCTURAL_PROJECTION` definition bags, the `Model/spatial#LINEAR_POSITIONING` station rows, the context root's `Phase`/`LongName` labels); `ConnectionProjection.All` lands the realizing-element detail bags and edges; `GeoReferenceProjector.Project` lands the `Header.Reference` geo frame [M1]; `EdgeProjection.All` lands every `IfcRel*` neutral edge [NEUTRAL_EDGE_RULING].
- Output: the `GraphDelta` is the projector's whole contribution — a merge over the canonical `ElementGraph` that `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` folds with the other projectors' deltas; the rooted map keyed by `GlobalId` is the identity table aspect projectors attach against and `Emit` reverses; the `FidelityLog` is the ingest run's counted drop ledger `Review/versioning#VERSION_GRAPH` stores beside the commit. The EGRESS run's ledger lands with `Projection/egress#IFC_EGRESS` and is not on this entry until it does.
- Packages: GeometryGymIFC_Core, Rasm.Element, Riok.Mapperly, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new extracted IFC entity family is one `Extract<T>` arm on the fold landing its graph node; a new predefined-pset concrete is one `PreDefinedReads` row; a new relationship is one `IfcRelKind` row (`Projection/relations#RELATION_ALGEBRA`); a new schema version is one `ReleaseMap` row the result-returning `ReleaseLower`/`ReleaseRaise` resolve; never a second element record beside the element graph and never a per-entity projector type.
- Boundary: the projector is the ONE GeometryGym-to-contract lowering — the retired `BimModel.Project` produced a second stored `BimElement` keyed by `GlobalId`, and any owner re-storing the element off the element graph is the deleted form; `Ingest` reads the LIVE `db.Project.Extract<T>()` entity graph, never the `Exchange/import#IMPORT_PIPELINE` decoded rows, because those carry mesh geometry alone and projecting them drops the whole relationship roster, the `OwnerHistory`, and the `StepHeader`; GeometryGym is captured INTERNALLY and an `IfcProduct`/`IfcRel*`/`DatabaseIfc` type crossing `IElementProjection.Project` is the named contract violation; the rooted `NodeId` is a neutral kernel-minted id and the IFC GUID never becomes node identity [H6]; the value narrowing is `Projection/value#PROPERTY_LOWERING`'s because an `IfcValue` or dataType string crossing a contract signature is the deleted form; the unit regime is the shared `UnitScheme` built ONCE per projection at `Projection/value#UNIT_INGRESS` and threaded from the fold head — a per-entity rebuild re-reads one project regime per row and falls to SI on a null database; geometry is referenced by `RepresentationContentHash` only [M2] and an inline `Vector3`/`BoundaryPolygon`/`Axis` member is the deleted §4-RT-M2 violation, `Rasm.Compute` resolving the analytical axis and footprint one-hop by content key; ingress class lookup is PERMISSIVE so one unknown entity never aborts an import, class validity deferred to the `Emit` egress gate [PREDEFINED_TOKEN_RULING][H8]; `Emit` is Bim-INTERNAL, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the contract owns only ingress projection.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
global using Rasm.Bim.Projection;
global using Rasm.Bim.Semantics;

using System.Collections.Frozen;
using System.Linq;
using System.Reflection;
using GeometryGym.Ifc;
using GeometryGym.STEP;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm;
using Rasm.Bim;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Riok.Mapperly.Abstractions;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;
using ModelView = Rasm.Element.Graph.ModelView;
using GGRelease = GeometryGym.Ifc.ReleaseVersion;
using GGView = GeometryGym.Ifc.ModelView;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct IfcMaterialSignature(
    string Name,
    string Category,
    Option<string> PsetKey);

public readonly record struct IfcProfileSignature(
    string Standard,
    string Designation,
    string IfcEntity,
    string StepKey);

public readonly record struct IfcTypeSignature(
    string GlobalId,
    string IfcEntity,
    string PredefinedType,
    string Name,
    Option<IfcMaterialSignature> Material,
    Option<IfcProfileSignature> Profile);

public readonly record struct IfcSeat(NodeId Id, ObjectKind Kind, Classification Classification, SchemaSpan Span);

// --- [SERVICES] ------------------------------------------------------------------------
public interface IIfcTypeReconciler {
    Fin<Option<Node.Object>> Resolve(IfcTypeSignature signature);
}

public interface IIfcProfileStore {
    Option<IfcProfileDef> Find(ProfileRef profile);
    Option<T> Find<T>(UInt128 contentKey) where T : BaseClassIfc;
    ProfileRef Preserve(IfcProfileDef profile);
    UInt128 Preserve(BaseClassIfc fragment);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct IfcIngest(GraphDelta Delta, Map<string, NodeId> Rooted) {
    public static IfcIngest Empty(Header header, Map<string, NodeId> rooted) =>
        new(GraphDelta.Empty.Reheader(header), rooted);

    public IfcIngest Capture(string globalId, Node.Object node, Option<PropertyBag> source, double tolerance) =>
        new(source.Map(bag => Bag(bag, tolerance))
                .Match(
                    Some: properties => Delta.Put(node).Put(properties)
                        .Link(new Relationship.Assign(node.Id, properties.Id, AssignKind.PropertyDefinition)),
                    None: () => Delta.Put(node)),
            Rooted.AddOrUpdate(globalId, node.Id));

    internal static Node.PropertySet Bag(PropertyBag bag, double tolerance) {
        var seed = new Node.PropertySet(NodeId.Of(new NodeSeed.Placement()), bag);
        return (Node.PropertySet)seed.Relabel(NodeId.Of(new NodeSeed.Content(seed, tolerance)));
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper]
[MapperIgnoreSource(nameof(IfcObjectDefinition.Description))]
[MapperIgnoreSource(nameof(IfcObjectDefinition.IsDecomposedBy))]
[MapperIgnoreSource(nameof(IfcObjectDefinition.Decomposes))]
internal static partial class IfcBoundaryMapper {
    [MapValue(nameof(Node.Object.Kind), ObjectKind.Type)]
    public static partial Node.Object Canonical(Node.Object resolved);

    [MapProperty(["seat", nameof(IfcSeat.Id)], [nameof(Node.Object.Id)])]
    [MapProperty(["seat", nameof(IfcSeat.Kind)], [nameof(Node.Object.Kind)])]
    [MapProperty(["seat", nameof(IfcSeat.Classification)], [nameof(Node.Object.Classification)])]
    [MapProperty(["seat", nameof(IfcSeat.Span)], [nameof(Node.Object.Span)])]
    [MapProperty([nameof(IfcObjectDefinition.GlobalId)], [nameof(Node.Object.ExternalId)], Use = nameof(ExternalId))]
    [MapProperty([nameof(IfcObjectDefinition.Name)], [nameof(Node.Object.Name)], Use = nameof(Label))]
    [MapProperty([nameof(IfcObjectDefinition.OwnerHistory)], [nameof(Node.Object.History)], Use = nameof(OwnerStamp))]
    [MapPropertyFromSource(nameof(Node.Object.PredefinedType), Use = nameof(Predefined))]
    [MapPropertyFromSource(nameof(Node.Object.ObjectType), Use = nameof(UserLabel))]
    [MapPropertyFromSource(nameof(Node.Object.Tag), Use = nameof(TagOf))]
    [MapPropertyFromSource(nameof(Node.Object.Representations), Use = nameof(Representations))]
    [MapperIgnoreTarget(nameof(Node.Object.Classifications))]
    public static partial Node.Object Lower(IfcObjectDefinition definition, IfcSeat seat);

    [UserMapping] static Option<string> ExternalId(string globalId) => Some(globalId);

    [UserMapping] static string Label(string? name) => PropertyLowering.Stated(name).IfNone("");

    [UserMapping] static string TagOf(IfcObjectDefinition definition) =>
        PropertyLowering.Stated((definition as IfcElement)?.Tag).IfNone("");

    [UserMapping] static Option<string> UserLabel(IfcObjectDefinition definition) => definition switch {
        IfcObject occurrence  => PropertyLowering.Stated(occurrence.ObjectType),
        IfcElementType type   => PropertyLowering.Stated(type.ElementType),
        _                     => None,
    };

    [UserMapping] static RepresentationContentHash Representations(IfcObjectDefinition definition) =>
        IfcRepresentation.Keys(definition);

    [UserMapping] internal static Option<OwnerHistory> OwnerStamp(IfcOwnerHistory? history) =>
        Optional(history).Map(static h => new OwnerHistory(
            OwningUser:        PropertyLowering.Stated(h.OwningUser?.Name).IfNone(""),
            OwningApplication: PropertyLowering.Stated(h.OwningApplication?.ApplicationFullName).IfNone(""),
            Created:           Instant.FromDateTimeUtc(DateTime.SpecifyKind(h.CreationDate, DateTimeKind.Utc)),
            Modified:          h.LastModifiedDate > DateTime.MinValue
                                   ? Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(h.LastModifiedDate, DateTimeKind.Utc)))
                                   : None,
            ChangeAction:      h.ChangeAction.ToString(),
            State:             h.State.ToString()));

    [UserMapping] internal static PredefinedType Predefined(IfcObjectDefinition definition) =>
        (PredefinedReads.Value.TryGetValue(definition.GetType(), out Func<IfcObjectDefinition, Option<string>>? read)
            ? read(definition)
            : None)
            .Filter(static token => !string.Equals(token, "NOTDEFINED", StringComparison.OrdinalIgnoreCase))
            .Match(Some: PredefinedType.Create, None: static () => PredefinedType.NotDefined);

    static readonly Lazy<FrozenDictionary<Type, Func<IfcObjectDefinition, Option<string>>>> PredefinedReads =
        new(static () => typeof(BaseClassIfc).Assembly.GetTypes()
            .Where(static type => typeof(IfcObjectDefinition).IsAssignableFrom(type)
                && type.GetProperty(nameof(IfcWall.PredefinedType)) is { CanRead: true })
            .ToFrozenDictionary(
                static type => type,
                static type => TokenReader(type.GetProperty(nameof(IfcWall.PredefinedType))!)));

    static Func<IfcObjectDefinition, Option<string>> TokenReader(PropertyInfo property) =>
        definition => PropertyLowering.Stated(property.GetValue(definition)?.ToString());
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed partial class SemanticProjector(
    DatabaseIfc db, IIfcTypeReconciler typeReconciler, IIfcProfileStore profiles, Option<TemplateScope> scope = default,
    Option<EurocodePolicy> eurocode = default, Option<BsddPins> pins = default) : IElementProjection {
    readonly IIfcProfileStore profiles = profiles;

    readonly Option<EurocodePolicy> eurocode = eurocode;

    readonly BsddPins pins = pins.IfNone(BsddPins.Default);

    readonly TemplateScope templates = scope.IfNone(TemplateScope.Standard);

    // --- [ENTRY]

    public Fin<GraphDelta> Project(ProjectionContext ctx) => Ingest(ctx).Map(static run => run.Delta);

    public Fin<(GraphDelta Delta, FidelityLog Fidelity)> Ingest(ProjectionContext ctx) {
        return Optional(db.Project).ToFin(new BimFault.Refused(BimScope.Projection, BimReason.DanglingReference, string.Join(':', new object?[] { "ifc-project-root-miss" }))).Bind(project => {
            Map<string, NodeId> rooted = project.Extract<IfcRoot>().AsIterable()
                .Fold(Map<string, NodeId>(), static (map, root) => map.AddOrUpdate(root.GlobalId, NodeId.Of(new NodeSeed.Placement())));
            UnitScheme scheme = IfcUnits.SchemeOf(db);
            double tolerance = scheme.Coerce(db.Tolerance, QuantityType.Length, Dimension.LengthDim);
            return
                from geo in GeoReferenceProjector.Project(project, scheme, key)
                from schema in ReleaseLower(db.Release, key)
                let header = new Header(schema, ViewLower(db.ModelView), geo, tolerance, ctx.At, StepHeaderOf(db)) { Units = scheme }
                from objects in Objects(project, header, rooted, tolerance, scheme, key)
                from run in (
                    ConnectionProjection.All(project, objects.Rooted, tolerance, scheme, key).ToValidation(),
                    Fidelity.Run(Bags(project, objects.Rooted, scheme, key)).ToValidation(),
                    Fidelity.Run(Materials(project, objects.Rooted, tolerance, scheme, key)).ToValidation(),
                    Fidelity.Run(EdgeProjection.All(project, objects.Rooted, tolerance, scheme, eurocode, templates, profiles, key)).ToValidation())
                    .Apply(static (details, bags, materials, edges) => (
                        Nodes: bags.Value + materials.Value + details.Map(static detail => detail.Bag),
                        Edges: edges.Value + details.Map(static detail => detail.Edge),
                        Log: bags.Log + materials.Log + edges.Log))
                    .As().ToFin()
                let landed = run.Edges.Fold(
                    run.Nodes.Fold(objects.Delta, static (delta, node) => delta.Put(node)),
                    static (delta, edge) => delta.Link(edge))
                from classified in Classify(project, objects.Rooted, landed)
                select (classified, run.Log);
        });
    }

    // --- [NODE_LOWERING]

    Fin<IfcIngest> Objects(IfcProject project, Header header, Map<string, NodeId> rooted, double tolerance, UnitScheme scheme) {
        Map<string, IfcMaterialSelect> materials = MaterialIndex(project);
        return (
            project.Extract<IfcObjectDefinition>().AsIterable()
                .Filter(static definition => definition is not IfcTypeObject)
                .ToSeq()
                .Traverse(definition => (SourceBag(definition, scheme, key).ToValidation(),
                                         SeatOf(definition, rooted, ObjectKind.Occurrence, key).ToValidation())
                    .Apply((source, seat) => (GlobalId: definition.GlobalId,
                                              Node: IfcBoundaryMapper.Lower(definition, seat),
                                              Source: source)).As()).As(),
            project.Extract<IfcTypeObject>().AsIterable().ToSeq()
                .Traverse(type => AdmitType(type, materials, rooted, key).ToValidation()).As())
            .Apply(static (occurrences, types) => occurrences + types)
            .As().ToFin()
            .Map(rows => rows.Fold(IfcIngest.Empty(header, rooted), (ingest, row) =>
                Error.New(row.GlobalId.Message, row.GlobalId)));
    }

    static Fin<IfcSeat> SeatOf(IfcObjectDefinition definition, Map<string, NodeId> rooted, ObjectKind kind) {
        string entity = ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _);
        Option<IfcClass> row = IfcClass.TryGet(entity);
        return Classification.Of(row.Map(static r => r.Key).IfNone(entity))
            .Map(classification => new IfcSeat(
                Id:             rooted[definition.GlobalId],
                Kind:           kind,
                Classification: classification,
                Span:           row.IfNone(IfcClass.BuildingElementProxy).Span));
    }

    Fin<(string GlobalId, Node.Object Node, Option<PropertyBag> Source)> AdmitType(
        IfcTypeObject definition, Map<string, IfcMaterialSelect> materials, Map<string, NodeId> rooted) {
        IfcTypeSignature signature = TypeSignatureOf(definition, materials);
        return typeReconciler.Resolve(signature).Bind(resolved => resolved.Match(
            Some: type => Fin.Succ((definition.GlobalId, IfcBoundaryMapper.Canonical(type), Option<PropertyBag>.None)),
            None: () => SeatOf(definition, rooted, ObjectKind.Type).Map(seat =>
                (definition.GlobalId,
                 IfcBoundaryMapper.Lower(definition, seat),
                 Some(ImportedSource(signature))))));
    }

    static Map<string, IfcMaterialSelect> MaterialIndex(IfcProject project) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Fold(Map<string, IfcMaterialSelect>(), static (map, rel) =>
                Optional(rel.RelatingMaterial).Match(
                    Some: material => toSeq(rel.RelatedObjects.OfType<IfcRoot>()).Fold(map, (acc, root) => acc.AddOrUpdate(root.GlobalId, material)),
                    None: () => map));

    Fin<GraphDelta> Classify(IfcProject project, Map<string, NodeId> rooted, GraphDelta delta) =>
        project.Extract<IfcRelAssociatesClassification>().AsIterable().ToSeq()
            .Traverse(rel => Optional(rel.RelatingClassification as IfcClassificationReference).Match(
                Some: reference => ClassificationSystem.Ingest(reference, pins, key).ToValidation()
                    .Map(classification => classification.Map(value => (
                        Related: toSeq(rel.RelatedObjects.OfType<IfcRoot>()).Choose(root => rooted.Find(root.GlobalId)),
                        Value: value))),
                None: static () => Success<Error, Option<(Seq<NodeId> Related, Classification Value)>>(None)))
            .As().ToFin()
            .Map(rows => rows.Somes().Fold(Map<NodeId, Seq<Classification>>(), static (map, row) =>
                row.Related.Fold(map, (acc, id) => acc.AddOrUpdate(id, existing => existing.Add(row.Value), () => Seq(row.Value)))))
            .Map(byNode => delta.AddedNodes.Fold(delta, (acc, node) =>
                node is Node.Object o && byNode.Find(o.Id).Case is Seq<Classification> refs
                    ? acc.Put(new Node.Object(o.Id, o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.ObjectType, o.Name, o.Tag, o.Representations, o.History, o.Span, refs))
                    : acc));

    // --- [TYPE_SIGNATURE]

    IfcTypeSignature TypeSignatureOf(IfcTypeObject definition, Map<string, IfcMaterialSelect> materials) {
        Option<IfcMaterialSelect> relatingMaterial = materials.Find(definition.GlobalId);
        string token = IfcBoundaryMapper.Predefined(definition).Token;
        return new(
            definition.GlobalId,
            ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _),
            token == "USERDEFINED" && PropertyLowering.Stated((definition as IfcElementType)?.ElementType).Case is string label ? label : token,
            PropertyLowering.Stated(definition.Name).IfNone(""),
            MaterialSignatureOf(relatingMaterial),
            ProfileSignatureOf(relatingMaterial, key));
    }

    const string MaterialFamilyPrefix = "Pset_Material";

    static Option<IfcMaterialSignature> MaterialSignatureOf(Option<IfcMaterialSelect> relatingMaterial) =>
        relatingMaterial.Bind(MaterialOf).Map(static material => new IfcMaterialSignature(
            PropertyLowering.Stated(material.Name).IfNone(""),
            PropertyLowering.Stated(material.Category).IfNone(""),
            material.HasProperties.AsIterable().ToSeq()
                .Choose(static pset => PropertyLowering.Stated(pset.Name))
                .Find(static name => name.StartsWith(MaterialFamilyPrefix, StringComparison.Ordinal))));

    Option<IfcProfileSignature> ProfileSignatureOf(Option<IfcMaterialSelect> relatingMaterial) =>
        relatingMaterial.Bind(ProfileOf).Map(profile => {
            ProfileRef preserved = profiles.Preserve(profile, key);
            return new IfcProfileSignature(
                Standard: preserved.Standard,
                Designation: PropertyLowering.Stated(preserved.Designation)
                    .IfNone(() => PropertyLowering.Stated(profile.ProfileName).IfNone("")),
                IfcEntity: ParserIfc.IdentifyIfcClass(profile.GetType().Name, out _),
                StepKey: preserved.ContentKey.ToString());
        });

    static Option<IfcMaterial> MaterialOf(IfcMaterialSelect entity) => entity switch {
        IfcMaterial material => Some(material),
        IfcMaterialLayerSetUsage usage => Optional(usage.ForLayerSet).Bind(MaterialOf),
        IfcMaterialProfileSetUsage usage => Optional(usage.ForProfileSet).Bind(MaterialOf),
        IfcMaterialLayerSet layerSet => Optional(layerSet.MaterialLayers.FirstOrDefault()?.Material),
        IfcMaterialProfileSet profileSet => Optional(profileSet.MaterialProfiles.FirstOrDefault()?.Material),
        IfcMaterialConstituentSet constituentSet => Optional(constituentSet.MaterialConstituents.FirstOrDefault()?.Material),
        _ => Option<IfcMaterial>.None
    };

    static Option<IfcProfileDef> ProfileOf(IfcMaterialSelect entity) => entity switch {
        IfcMaterialProfileSet profileSet => Optional(profileSet.CompositeProfile ?? profileSet.MaterialProfiles.FirstOrDefault()?.Profile),
        IfcMaterialProfileSetUsage usage => Optional(usage.ForProfileSet?.CompositeProfile ?? usage.ForProfileSet?.MaterialProfiles.FirstOrDefault()?.Profile),
        _ => Option<IfcProfileDef>.None
    };

    // --- [SOURCE_BAGS]

    internal static readonly string TypeSignatureSet = "IfcTypeSignature";

    internal static readonly string PortAttributeSet = "IfcDistributionPort";
    internal static readonly string StructuralDefinitionSet = "IfcStructuralDefinition";
    internal static readonly string PositioningAttributeSet = "IfcLinearPositioning";
    internal static readonly string ProjectAttributeSet = "IfcProjectContext";

    internal static readonly PropertyName Phase = FactoryBridge.Row("Phase");
    internal static readonly PropertyName LongName = FactoryBridge.Row("LongName");

    public static class SignatureRows {
        public static readonly PropertyName GlobalId = FactoryBridge.Row("GlobalId");
        public static readonly PropertyName IfcEntity = FactoryBridge.Row("IfcEntity");
        public static readonly PropertyName PredefinedType = FactoryBridge.Row("PredefinedType");
        public static readonly PropertyName Name = FactoryBridge.Row("Name");
        public static readonly PropertyName MaterialName = FactoryBridge.Row("MaterialName");
        public static readonly PropertyName MaterialCategory = FactoryBridge.Row("MaterialCategory");
        public static readonly PropertyName MaterialStandard = FactoryBridge.Row("MaterialStandard");
        public static readonly PropertyName MaterialGrade = FactoryBridge.Row("MaterialGrade");
        public static readonly PropertyName ProfileStandard = FactoryBridge.Row("ProfileStandard");
        public static readonly PropertyName ProfileDesignation = FactoryBridge.Row("ProfileDesignation");
        public static readonly PropertyName ProfileEntity = FactoryBridge.Row("ProfileEntity");
        public static readonly PropertyName ProfileStepKey = FactoryBridge.Row("ProfileStepKey");
    }

    Fin<Option<PropertyBag>> SourceBag(IfcObjectDefinition definition, UnitScheme scheme) =>
        definition switch {
            IfcDistributionPort port => Fin.Succ(Some(new PropertyBag(
                PortAttributeSet,
                Map(
                    (PortRows.FlowDirection, (PropertyValue)new PropertyValue.Text(port.FlowDirection.ToString())),
                    (PortRows.SystemType, new PropertyValue.Text(port.SystemType.ToString()))),
                InheritanceMode.OccurrenceWins,
                EvidenceGrade.Import))),
            IfcStructuralItem or IfcStructuralActivity or IfcStructuralLoadGroup or IfcStructuralResultGroup or IfcStructuralAnalysisModel =>
                StructuralProjection.Attrs(definition, scheme, eurocode, profiles, key).Map(attrs => attrs.IsEmpty
                    ? Option<PropertyBag>.None
                    : Some(new PropertyBag(StructuralDefinitionSet, attrs, InheritanceMode.OccurrenceWins, EvidenceGrade.Import))),
            IfcAlignmentSegment or IfcReferent or IfcProduct { ObjectPlacement: IfcLinearPlacement } =>
                PositioningProjection.Attrs(definition, scheme, key).Map(attrs => attrs.IsEmpty
                    ? Option<PropertyBag>.None
                    : Some(new PropertyBag(PositioningAttributeSet, attrs, InheritanceMode.OccurrenceWins, EvidenceGrade.Import))),
            IfcContext context => Fin.Succ(
                Seq((Row: Phase, Value: PropertyLowering.Stated(context.Phase)),
                    (Row: LongName, Value: PropertyLowering.Stated(context.LongName)))
                    .Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => row.Value.Match(
                        Some: value => bag.Add(row.Row, new PropertyValue.Text(value)),
                        None: () => bag))
                    is { IsEmpty: false } rows
                    ? Some(new PropertyBag(ProjectAttributeSet, rows, InheritanceMode.OccurrenceWins, EvidenceGrade.Import))
                    : Option<PropertyBag>.None),
            _ => Fin.Succ(Option<PropertyBag>.None),
        };

    static PropertyBag ImportedSource(IfcTypeSignature signature) =>
        new PropertyBag(
            TypeSignatureSet,
            Seq((Row: SignatureRows.MaterialName, Value: signature.Material.Map(static m => m.Name)),
                (Row: SignatureRows.MaterialCategory, Value: signature.Material.Map(static m => m.Category)),
                (Row: SignatureRows.MaterialStandard, Value: signature.Material.Bind(static m => m.Standard)),
                (Row: SignatureRows.MaterialGrade, Value: signature.Material.Bind(static m => m.Grade)),
                (Row: SignatureRows.ProfileStandard, Value: signature.Profile.Map(static p => p.Standard)),
                (Row: SignatureRows.ProfileDesignation, Value: signature.Profile.Map(static p => p.Designation)),
                (Row: SignatureRows.ProfileEntity, Value: signature.Profile.Map(static p => p.IfcEntity)),
                (Row: SignatureRows.ProfileStepKey, Value: signature.Profile.Map(static p => p.StepKey)))
            .Fold(
                Map<PropertyName, PropertyValue>()
                    .Add(SignatureRows.GlobalId, new PropertyValue.Text(signature.GlobalId))
                    .Add(SignatureRows.IfcEntity, new PropertyValue.Text(signature.IfcEntity))
                    .Add(SignatureRows.PredefinedType, new PropertyValue.Text(signature.PredefinedType))
                    .Add(SignatureRows.Name, new PropertyValue.Text(signature.Name)),
                static (bag, row) => row.Value.Match(
                    Some: value => bag.Add(row.Row, new PropertyValue.Text(value)),
                    None: () => bag)),
            InheritanceMode.TypeDrivenOnly,
            EvidenceGrade.Import);

    // --- [BAG_LOWERING]

    WriterT<FidelityLog, Fin, Seq<Node>> Bags(IfcProject project, Map<string, NodeId> rooted, UnitScheme scheme) =>
        from properties in project.Extract<IfcPropertySet>().AsIterable().ToSeq().Traverse(ps =>
            ps.HasProperties.Values.AsIterable().ToSeq()
                .Traverse(property => PropertyLowering.Lower(property, rooted, scheme, key)
                    .Map(lowered => (Name: PropertyName.Create(PropertyLowering.Stated(property.Name).IfNone("")), Value: lowered))).As()
                .Map(rows => BagNode(rooted[ps.GlobalId], ps.Name, rows, Binding(ps)))).As()
        from predefined in project.Extract<IfcPreDefinedPropertySet>().AsIterable().ToSeq().Traverse(set =>
            PreDefinedRows(set, scheme, key).Map(rows => BagNode(rooted[set.GlobalId], set.Name, rows, Binding(set)))).As()
        from quantities in Fidelity.Lift(project.Extract<IfcElementQuantity>().AsIterable().ToSeq().TraverseM(eq =>
            FlattenQuantities(eq.Quantities.Values, "", scheme, (Map<PropertyName, MeasureValue>(), Map<string, GroupIdentity>()), key)
                .Map(flat => (Node)new Node.QuantitySet(rooted[eq.GlobalId], new QuantityBag(
                    PropertyLowering.Stated(eq.Name).IfNone(""), flat.Values,
                    PropertyInheritance.ModeOf(PropertyLowering.Stated(eq.Name).IfNone(""), Binding(eq), templates),
                    EvidenceGrade.Import, flat.Groups)))).As())
        select properties + predefined + quantities;

    Node BagNode(NodeId id, string? name, Seq<(PropertyName Name, PropertyValue Value)> rows, TypeBinding binding) {
        string label = PropertyLowering.Stated(name).IfNone("");
        return new Node.PropertySet(id, new PropertyBag(
            label,
            rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)),
            PropertyInheritance.ModeOf(label, binding, templates),
            EvidenceGrade.Import));
    }

    static TypeBinding Binding(IfcPropertySetDefinition set) =>
        set.DefinesType.Any() ? TypeBinding.TypeBound : TypeBinding.Occurrence;

    static Fin<(Map<PropertyName, MeasureValue> Values, Map<string, GroupIdentity> Groups)> FlattenQuantities(
        IEnumerable<IfcPhysicalQuantity> quantities, string prefix, UnitScheme scheme,
        (Map<PropertyName, MeasureValue> Values, Map<string, GroupIdentity> Groups) bag) =>
        toSeq(quantities).FoldM(bag, (acc, quantity) => quantity switch {
            IfcPhysicalSimpleQuantity simple => PropertyLowering.Measure(simple, scheme)
                .Map(value => acc with {
                    Values = acc.Values.AddOrUpdate(PropertyName.Create($"{prefix}{PropertyLowering.Stated(simple.Name).IfNone("")}"), value)
                }),
            IfcPhysicalComplexQuantity complex => FlattenQuantities(
                complex.HasQuantities, $"{prefix}{PropertyLowering.Stated(complex.Name).IfNone("")}.", scheme,
                acc with {
                    Groups = acc.Groups.AddOrUpdate($"{prefix}{PropertyLowering.Stated(complex.Name).IfNone("")}", GroupOf(complex))
                }),
            _ => Fin.Fail<(Map<PropertyName, MeasureValue>, Map<string, GroupIdentity>)>(new BimFault.Refused(BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "quantity-kind-unmapped", quantity.GetType().Name }))),
        }).As();

    static GroupIdentity GroupOf(IfcPhysicalComplexQuantity complex) =>
        new(PropertyLowering.Stated(complex.Discrimination),
            PropertyLowering.Stated(complex.Quality),
            PropertyLowering.Stated(complex.Usage));

    static readonly FrozenDictionary<Type, Func<IfcPreDefinedPropertySet, UnitScheme, Seq<(string Key, Fin<PropertyValue> Value)>>> PreDefinedReads =
        new Dictionary<Type, Func<IfcPreDefinedPropertySet, UnitScheme, Seq<(string Key, Fin<PropertyValue> Value)>>> {
            [typeof(IfcDoorPanelProperties)] = static (set, scheme, key) => DoorPanel((IfcDoorPanelProperties)set, scheme),
            [typeof(IfcWindowPanelProperties)] = static (set, scheme, key) => ((IfcWindowPanelProperties)set) switch {
                var p => Frame(p.FrameDepth, p.FrameThickness, p.OperationType.ToString(), p.PanelPosition.ToString(), scheme, key),
            },
            [typeof(IfcPermeableCoveringProperties)] = static (set, scheme, key) => ((IfcPermeableCoveringProperties)set) switch {
                var p => Frame(p.FrameDepth, p.FrameThickness, p.OperationType.ToString(), p.PanelPosition.ToString(), scheme, key),
            },
            [typeof(IfcWindowLiningProperties)] = static (set, scheme, key) => WindowLining((IfcWindowLiningProperties)set, scheme),
        }.ToFrozenDictionary();

    static Seq<(string Key, Fin<PropertyValue> Value)> DoorPanel(IfcDoorPanelProperties p, UnitScheme scheme) =>
        Seq(Scalar("PanelDepth", p.PanelDepth, Dimension.LengthDim, scheme),
            Scalar("PanelWidth", p.PanelWidth, Dimension.Dimensionless, scheme),
            Token("PanelOperation", p.OperationType.ToString()),
            Token("PanelPosition", p.PanelPosition.ToString()));

    static Seq<(string Key, Fin<PropertyValue> Value)> Frame(double depth, double thickness, string operation, string position, UnitScheme scheme) =>
        Seq(Scalar("FrameDepth", depth, Dimension.LengthDim, scheme),
            Scalar("FrameThickness", thickness, Dimension.LengthDim, scheme),
            Token("OperationType", operation), Token("PanelPosition", position));

    static Seq<(string Key, Fin<PropertyValue> Value)> WindowLining(IfcWindowLiningProperties p, UnitScheme scheme) =>
        Seq("LiningDepth", "LiningThickness", "TransomThickness", "MullionThickness")
            .Zip(Seq(p.LiningDepth, p.LiningThickness, p.TransomThickness, p.MullionThickness))
            .Map(row => Scalar(row.Item1, row.Item2, Dimension.LengthDim, scheme))
        + Seq("FirstTransomOffset", "SecondTransomOffset", "FirstMullionOffset", "SecondMullionOffset")
            .Zip(Seq(p.FirstTransomOffset, p.SecondTransomOffset, p.FirstMullionOffset, p.SecondMullionOffset))
            .Map(row => Scalar(row.Item1, row.Item2, Dimension.Dimensionless, scheme));

    static WriterT<FidelityLog, Fin, Seq<(PropertyName Name, PropertyValue Value)>> PreDefinedRows(IfcPreDefinedPropertySet set, UnitScheme scheme) =>
        (PreDefinedReads.TryGetValue(set.GetType(), out Func<IfcPreDefinedPropertySet, UnitScheme, Seq<(string Key, Fin<PropertyValue> Value)>>? read)
            ? Fidelity.Clean(read(set, scheme))
            : Fidelity.Drop(FidelityDrop.PredefinedPsetOpaque,
                PropertyLowering.Stated(set.Name).IfNone(() => set.GetType().Name), Seq<(string Key, Fin<PropertyValue> Value)>()))
        .Bind(rows => Fidelity.Lift(rows.TraverseM(row => row.Value.Map(value => (row.Key, value))).As()))
        .Map(static values => values
            .Filter(static row => row.value switch { PropertyValue.Text text => text.Value.Length > 0, _ => true })
            .Map(static row => (FactoryBridge.Row(row.Key), row.value)));

    static (string, Fin<PropertyValue>) Scalar(string name, double native, Dimension dimension, UnitScheme scheme) =>
        (name, double.IsFinite(native)
            ? MeasureValue.OfSi(dimension, scheme.Coerce(native, QuantityType.OfDimension(dimension), dimension))
                .Map(static value => (PropertyValue)new PropertyValue.Measure(value))
            : Fin.Succ<PropertyValue>(new PropertyValue.Text("")));

    static (string, Fin<PropertyValue>) Token(string name, string value) => (name, Fin.Succ<PropertyValue>(new PropertyValue.Text(value)));

    // --- [MATERIAL_LOWERING]

    WriterT<FidelityLog, Fin, Seq<Node>> Materials(IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScheme scheme) {
        var relating = project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Choose(static rel => Optional(rel.RelatingMaterial));
        return
            from materials in Fidelity.Lift(relating.ToSeq()
                .TraverseM(select => MaterialProjection.Project(select, tolerance, profiles, scheme, key)).As())
            from imported in relating.Choose(DefinitionOf).ToSeq().Distinct()
                .Traverse(definition => MaterialProjection.ImportedPsets(definition, rooted, scheme, templates, key)
                    .Map(bags => bags.Map(bag => (Node)IfcIngest.Bag(bag, tolerance)))).As()
            select toSeq(materials.Map(static m => (Node)m)
                .Concat(imported.Flatten())
                .DistinctBy(static node => node.Id));
    }

    internal static Option<IfcMaterialDefinition> DefinitionOf(IfcMaterialSelect select) => select switch {
        IfcMaterialLayerSetUsage usage   => Optional((IfcMaterialDefinition?)usage.ForLayerSet),
        IfcMaterialProfileSetUsage usage => Optional((IfcMaterialDefinition?)usage.ForProfileSet),
        IfcMaterialDefinition definition => Some(definition),
        _                                => None,
    };


    // --- [HEADER_LOWERING]

    static StepHeader StepHeaderOf(DatabaseIfc database) =>
        database.OriginatingFileInformation is { } info
            ? new StepHeader(
                Descriptions:  toSeq(info.FileDescriptions),
                Name:          PropertyLowering.Stated(info.FileName).IfNone(""),
                TimeStamp:     Instant.FromDateTimeUtc(DateTime.SpecifyKind(info.TimeStamp, DateTimeKind.Utc)),
                Authors:       toSeq(info.Author),
                Organizations: toSeq(info.Organization),
                Preprocessor:  PropertyLowering.Stated(info.PreProcessorVersion).IfNone(""),
                OriginatingSystem: PropertyLowering.Stated(info.OriginatingSystem).IfNone(""),
                Schema:        Seq(database.Release.ToString()))
            : StepHeader.Empty with { Schema = Seq(database.Release.ToString()) };

    internal static Fin<ReleaseVersion> ReleaseLower(GGRelease release) =>
        ReleaseMap.Lower.TryGetValue(release, out ReleaseVersion? lowered) && lowered is { } version
            ? Fin.Succ(version)
            : Fin.Fail<ReleaseVersion>(new BimFault.Refused(BimScope.Model, BimReason.Codec, string.Join(':', new object?[] { "release-unmapped", release })));

    static ModelView ViewLower(GGView view) => view switch {
        GGView.Ifc4Reference or GGView.IFC4X3Reference => ModelView.Ifc4Reference,
        GGView.Ifc4DesignTransfer                      => ModelView.DesignTransfer,
        GGView.IFC4X3AlignmentBasedView                => ModelView.Alignment,
        _                                              => ModelView.Coordination,
    };
}
```

## [03]-[GRAPH_LEGALITY]

- Owner: `IfcLegality` the `IGraphConstraint` deciding IFC-semantic RELATIONSHIP legality the contract's structural `GraphDelta` switch cannot [M3] — the contract enforces only structural invariants (an edge endpoint resolves, an endpoint kind is legal), and which entity may relate to which is Bim's, depended UP on through the `IGraphConstraint` contract. Class and predefined-token VALIDITY is not here: the `Emit` egress gate owns the whole token vocabulary [PREDEFINED_TOKEN_RULING], because ingress admits tokens bare and a second validity owner at composition time forks the vocabulary between the two ends.
- Entry: `IfcLegality.Validate(GraphDelta delta, ElementGraph graph)` accumulates every IFC-legality violation the delta's `AddedEdges` carry onto `Validation<Error,Unit>` — applicative, so an authoring pass sees all rejects in one apply and never the first-fail short-circuit a `Fin` result gives.
- Auto: each endpoint rule is a `Model/query#ELEMENT_SET` `BimTerm` the ONE query evaluator (`ElementQuery.Verdict`) decides, so the gate carries no matcher of its own and a broken structure the rule meets rides out as verdict evidence rather than as a bare false; the rules dispatch on the contract's NEUTRAL case + sub-kind (the contract carries no `IfcRel*` case) — a `Contain` `Compose` edge requires its `Whole` to resolve a spatial-container row on the SIBLING vocabularies (`Model/spatial#SPATIAL_STRUCTURE` `SpatialClass.IsContainer` for the site/building/storey/space and IFC4.3 facility containers, `Model/zones#ZONE_GRAPH` `BimZoneKind.IsSpatial` for `IfcSpatialZone` — the disjoint partition, a private re-listed leaf set being the deleted drift form that faulted every 4.3 infrastructure containment); an `Aggregate` `Compose` edge may not have a `Type` object as its `Whole`; a spatial-to-spatial edge must nest downward per `SpatialClass.CanContain`; a `Void` edge dispatches its SUB-KIND — `VoidKind.Void` requires its `Feature` to be a feature subtraction, `VoidKind.Fill` requires its `Host` to be one, because the `Fills` row reads relating=opening so the OPENING sits in the `Host` slot and a blanket `Feature` check rejected every legal fill; a `TypeDefinition` `Assign` edge requires its `Definition` to be a `Type` object.
- Law: an endpoint resolves over `Endpoints` — the delta's OWN `AddedNodes` UNIONED with the merged `graph`, added winning — because a delta landing a storey and its containment edge in one merge must see the node the delta itself adds; a graph-only lookup faults every same-delta endpoint and makes the gate un-runnable on a first import. The two outcomes are DISTINCT faults: an ABSENT endpoint is `Refused/BimReason.DanglingReference` (the merge is malformed) and a FAILED predicate is `Refused/BimReason.Rejected` (legal STEP, illegal IFC semantics) — one detail for both hid a broken merge inside a vocabulary complaint and sent a federation manager to the wrong end.
- Packages: Rasm.Element (`Query/predicate#ELEMENT_PREDICATE` the term algebra and `MatchVerdict`), `Model/query#ELEMENT_SET` (`ElementQuery.Verdict`, `BimLeaf`), LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new IFC-legality rule is one arm on the `Rule` switch; a new spatial container is one `SpatialClass`/`BimZoneKind` row on its OWNING sibling vocabulary and this gate widens with zero edits; a feature-subtraction class is one `Subtraction` row; the structural invariants stay the contract's `GraphDelta` switch and never migrate here.
- Boundary: `IfcLegality` decides IFC RELATIONSHIP legality ONLY — re-checking the contract's structural invariants here is the deleted form [M3] and class/token validity is the egress `Emit` gate whole [PREDEFINED_TOKEN_RULING]; the rules read the generic `Classification` branch and the `ObjectKind` through the query algebra, never an `IfcProduct` runtime type and never a second matcher, because GeometryGym stays captured in the projector; each `BimFault.Refused` lifts bare, `Error.Many` retains every member, and consumers discriminate the typed `Reason` payload rather than testing the shared leaf type twice.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim.Model;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;
using BimTerm = Rasm.Element.Query.Predicate<Rasm.Bim.Model.BimLeaf>;

namespace Rasm.Bim.Projection;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class IfcLegality : IGraphConstraint {

    static readonly BimTerm SpatialWhole = BimLeaf.Classified(
        (toSeq(SpatialClass.Items).Filter(static s => s.IsContainer).Map(static s => s.Key)
         + toSeq(BimZoneKind.Items).Filter(static z => z.IsSpatial).Map(static z => z.Key))
            .Map(code => Classification.Of(ElementQuery.IfcSystem, code, Gate).ThrowIfFail()));

    static readonly BimTerm Subtraction = BimLeaf.Classified(
        Seq("IfcOpeningElement", "IfcVoidingFeature")
            .Map(code => Classification.Of(ElementQuery.IfcSystem, code, Gate).ThrowIfFail()));

    static readonly BimTerm OccurrenceWhole = BimLeaf.Of(new ElementLeaf.ByKind(ObjectKind.Occurrence));
    static readonly BimTerm TypeDefinition = BimLeaf.Of(new ElementLeaf.ByKind(ObjectKind.Type));

    readonly record struct Endpoints(Map<NodeId, Node.Object> Added, ElementGraph Graph) {
        public static Endpoints Of(GraphDelta delta, ElementGraph graph) =>
            new(delta.AddedNodes.Fold(Map<NodeId, Node.Object>(), static (map, node) =>
                node is Node.Object obj ? map.AddOrUpdate(obj.Id, obj) : map), graph);

        public Option<Node.Object> Find(NodeId id) =>
            Added.Find(id) is { IsSome: true } added ? added : Graph.Find<Node.Object>(id);
    }

    public Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph) {
        Endpoints endpoints = Endpoints.Of(delta, graph);
        return delta.AddedEdges.Traverse(edge => Rule(edge, endpoints)).As().Map(static _ => unit);
    }

    static Validation<Error, Unit> Rule(Relationship edge, Endpoints endpoints) => edge switch {
        Relationship.Compose c when c.SubKind == ComposeKind.Contain =>
            (Require(c.Whole, endpoints, SpatialWhole, "containment-whole-not-spatial", c.Whole.ToValue()),
             SpatialRank(c.Whole, c.Part, endpoints)).Apply(static (_, _) => unit).As(),
        Relationship.Compose c when c.SubKind == ComposeKind.Aggregate =>
            (Require(c.Whole, endpoints, OccurrenceWhole, "type-aggregates-occurrence", c.Whole.ToValue()),
             SpatialRank(c.Whole, c.Part, endpoints)).Apply(static (_, _) => unit).As(),
        Relationship.Void v when v.SubKind == VoidKind.Void =>
            Require(v.Feature, endpoints, Subtraction, "voids-feature-not-subtraction", v.Feature.ToValue()),
        Relationship.Void v when v.SubKind == VoidKind.Fill =>
            Require(v.Host, endpoints, Subtraction, "fills-host-not-subtraction", v.Host.ToValue()),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition =>
            Require(a.Definition, endpoints, TypeDefinition, "definesbytype-definition-not-type", a.Definition.ToValue()),
        _ => Success<Error, Unit>(unit),
    };

    static Validation<Error, Unit> SpatialRank(NodeId whole, NodeId part, Endpoints endpoints) =>
        SpatialOf(whole, endpoints)
            .Bind(w => SpatialOf(part, endpoints).Map(p => (Whole: w, Part: p)))
            .Match(
                None: () => Success<Error, Unit>(unit),
                Some: pair => pair.Whole.CanContain(pair.Part)
                    ? Success<Error, Unit>(unit)
                    : Fail<Error, Unit>(new BimFault.Refused(Gate, BimScope.Projection, BimReason.Rejected, string.Join(':', new object?[] { "containment-rank-inverted", pair.Whole.Key, pair.Part.Key }))));

    static Option<SpatialClass> SpatialOf(NodeId id, Endpoints endpoints) =>
        endpoints.Find(id).Bind(static o => SpatialClass.TryGet(o.Classification.Code));

    static Validation<Error, Unit> Require(NodeId id, Endpoints endpoints, BimTerm rule, string detail, string subject) =>
        endpoints.Find(id).Match(
            None: () => Fail<Error, Unit>(new BimFault.Refused(Gate, BimScope.Projection, BimReason.DanglingReference, string.Join(':', new object?[] { "endpoint-unresolved", id.ToValue() }))),
            Some: obj => ElementQuery.Verdict(endpoints.Graph, obj, rule) switch {
                { Holds: true } => Success<Error, Unit>(unit),
                { Faults: var faults } when faults.IsEmpty is false => Fail<Error, Unit>(Error.Many(faults)),
                _ => Fail<Error, Unit>(new BimFault.Refused(Gate, BimScope.Projection, BimReason.Rejected,
                    string.Join(':', new object?[] { detail, subject }))),
            });
}
```

## [04]-[RESEARCH]

(none)
