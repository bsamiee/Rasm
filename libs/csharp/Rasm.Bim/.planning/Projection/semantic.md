# [BIM_SEMANTIC_PROJECTOR]

`Rasm.Bim` is the SOLE GeometryGym/IFC owner and the IFC arm of the `Rasm.Element` seam. This page owns the INGRESS half of the one `SemanticProjector : IElementProjection` that lowers a live GeometryGym `DatabaseIfc` into a seam `GraphDelta` (the `Project` fold), the `PropertyLowering` value-narrowing the seam delegates to it, and the `IfcLegality : IGraphConstraint` that decides IFC-semantic legality the seam's structural `GraphDelta` switch cannot. The relationship-lowering half (`IfcRelKind`/`EdgeProjection`) lives at `Projection/relations#RELATION_ALGEBRA` and the IFC re-author half (`Emit`/`Sniff`) at `Projection/egress#IFC_EGRESS` — the SAME `partial class SemanticProjector`, split by concern: `Project` composes `EdgeProjection.All` to land the neutral edges, and `Emit` reverses both the node lowering and the relation roster. The projector replaces the retired `BimModel.Project`/`BimElement` fold: where the old owner produced a second stored element record keyed by GlobalId, the projector produces seam `Node`s (`Object` occurrence/type, `PropertySet`, `QuantitySet`, `Material`) and neutral `Relationship` edges that `Assemble` folds into the canonical `ElementGraph`, so "has it all" is one `Bake` read on the seam graph and GeometryGym never leaks below the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the kernel geometry by content-hash reference, never a RhinoCommon type, never an in-process BRep evaluation.

The element identity is established HERE (the IFC is the source of element identity for an ingested model): `Project` mints a NEUTRAL rooted `NodeId` per `IfcRoot` and records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute. `IfcTypeObject` identity is then admitted through `IIfcTypeReconciler`: a resolver hit reuses the canonical Materials Type Object, and a miss keeps the IFC type imported/ad-hoc with preserved material/profile signatures in a `PropertySource.Import` bag. The owner-mints-its-identity law still holds — Materials mints canonical Component Types, Bim preserves IFC source identity and never forges a catalogue row from a name/profile string. The two `ReleaseVersion`/`ModelView` worlds meet at the projector and nowhere else: the seam `ReleaseVersion`/`ModelView` `[SmartEnum]`s are the model `Header` currency, and the GeometryGym `ReleaseVersion`/`ModelView` enums are the IFC-text codec leg `Project`/`Emit`/`Sniff` own — the page aliases the GeometryGym pair (`GGRelease`/`GGView`) so the unqualified names resolve to the seam, and `ReleaseLower`/`ReleaseRaise` are the one lowering pair the leak never escapes. Both lowerings are RAILED through the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap`, split by direction across the one partial class: `ReleaseLower` (this page) reads `ReleaseMap.Lower` and rails `BimFault.CodecReject` on an unmapped `GGRelease` member, and the egress half's `ReleaseRaise` (`Projection/egress#IFC_EGRESS`) reads `ReleaseMap.Raise` — the identity-name-derived inverse, so the two directions can never drift — railing a seam schema with no GG writer (`Ifc5`); the prior `?? ReleaseVersion.Ifc4X3Add2` / `: GGRelease.IFC4X3_ADD2` silent coercions are the deleted masked-error form (`IFC4X4_DRAFT` excluded by law).

## [01]-[INDEX]

- [01]-[SEMANTIC_PROJECTOR]: `SemanticProjector : IElementProjection`, the `Project` fold lowering `DatabaseIfc` into a `GraphDelta` — rooted `NodeId` mint with the 1:1 IFC `GlobalId` projection attribute [H6], reconciled `IfcTypeObject` admission through `IIfcTypeReconciler`, imported/ad-hoc type signature preservation through `IIfcProfileStore` + `PropertySource.Import`, the `Object` occurrence/type nodes carrying the generic `Classification`/`PredefinedType`/`RepresentationContentHash`, the `PropertySet`/`QuantitySet` bag nodes whose typed `PropertyValue`/`MeasureValue` the `PropertyLowering` narrowing fills — every magnitude native-unit→SI-coerced through the one per-projection `UnitScale` — and whose `InheritanceMode` is stamped at ingest [H1], the `OwnerHistory`/`StepHeader` projection [H9], and the schema span [H8]; the relationship lowering composes `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.All`.
- [02]-[GRAPH_LEGALITY]: `IfcLegality : IGraphConstraint` the IFC-semantic legality validator — the EDGE rules (containment-whole-must-be-spatial over the full concrete spatial-element leaf set, the `SpatialClass.CanContain` parent→child rank law on every spatial-to-spatial `Contain`/`Aggregate` edge, the sub-kind-oriented `Void`/`Fill` feature-subtraction checks, type-may-not-aggregate-occurrence, `DefinesByType` definition-must-be-type) PLUS the NODE vocabulary arms (every `"ifc"`-classified `Object` node resolves a generated `IfcClass` roster row and its predefined token is a member of the row's valid set) — accumulating onto `Validation<Error,Unit>` over the seam's structural invariants [M3], span/rank gating staying at egress.

## [02]-[SEMANTIC_PROJECTOR]

- Owner: `SemanticProjector` the `IElementProjection` capturing one live GeometryGym `DatabaseIfc` internally and lowering it to a seam `GraphDelta` in `Project`; `PropertyLowering` the Bim-internal value-narrowing the seam delegates to it (the seam forbids an IFC `IfcValue`/dataType crossing its signature, so the `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing is Bim's), every magnitude coerced native-unit→SI through the one per-projection `UnitScale` because GG never pre-coerces; `OwnerStamp` the `IfcOwnerHistory`→seam `OwnerHistory` projection; `StepHeaderOf` the `STEPFileInformation`→seam `StepHeader` projection; `ReleaseLower` the ingress GeometryGym→seam schema lowering railed through the frozen `ReleaseMap.Lower` (`Fin<T>`, `BimFault.CodecReject` on an unmapped member — no silent coercion; the raise direction is the egress half's `ReleaseRaise` over `ReleaseMap.Raise`) and `ViewLower` the explicit-member MVD lowering.
- Entry: `SemanticProjector.Project(ProjectionContext ctx)` folds the captured `DatabaseIfc` into one `GraphDelta` over `ctx.Key` — it mints a NEUTRAL rooted `NodeId` per `IfcRoot` through the kernel static `Rasm.Element/Graph/element#NODE_MODEL` `NodeId.Rooted()` mint (the `IObjectFactory` floor — `ProjectionContext` exposes only `For`/`Owns`, never a mint pass-through), records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute [H6], reconciles each `IfcTypeObject` through `IIfcTypeReconciler`, preserves imported/ad-hoc type material/profile signatures through `IIfcProfileStore`, and content-keys every non-rooted material node through `MaterialProjection.Project`'s kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`; `Fin<T>` aborts on a missing `IfcProject` root or a dangling spatial host (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) and on an out-of-map GG release (`BimFault.CodecReject` off the railed `ReleaseLower`), the ingress class lookup PERMISSIVE — an unrostered/IFC4-new leaf lands the `Model/elements#IFC_CLASS` `IfcClass.BuildingElementProxy` row through `TryGet(entityType).IfNone(BuildingElementProxy)` (the generated roster carries the retired `*StandardCase` subtypes as committed rows, so the raw name resolves; `IfcClass.Proxy` binds the REAL deprecated `IfcProxy` entity under the mechanical render law, never the fallback) so one unknown entity never aborts the import, class validity deferred to the `Emit` egress gate [C6][H8] — the fault lifting BARE (the band IS the `Expected` `Code`, no `.ToError()` hop). The element identity is established HERE (the IFC is the source of element identity), so the projector ignores `ctx.ElementIds` (the aspect-projector NodeId set) and PUBLISHES the minted ids in the delta for sibling projectors to attach `Associate` edges against.
- Auto: `Project` walks the captured `db.Project` once — `ObjectNode` lands every NON-TYPE `IfcObjectDefinition`→`Object.Occurrence` (products AND the `IfcProject` context root the `Model/spatial#SPATIAL_STRUCTURE` tree resolves as its `SpatialClass.IsRoot` node, the `IfcGroup` subtree the `Model/zones#ZONE_GRAPH` overlay reads, and the process/control/actor/resource families the rostered assignment/sequence edges reference — a product-only sweep stranded every such edge on a nodeless endpoint); `AdmitType` lands each `IfcTypeObject` through a `TypeNodeSeed` that either copies the canonical resolver `Node.Object` or preserves the imported IFC type with a `PropertySource.Import` source bag. `ObjectProjection.Rooted` then rebinds each type `GlobalId` to the emitted type id before `Classify`, `Bags`, and `EdgeProjection.All` resolve endpoints. The generic `Classification("ifc", classKey)` (the IFC entity type as a classification, never `IfcClass` on the node) resolves through the permissive `IfcClass.TryGet(entityType).IfNone(BuildingElementProxy)` ingress over the generated roster for IFC-sourced nodes; `PredefinedType` reads off the entity's per-class predefined property; the keyed `RepresentationContentHash` map (`Model/elements#REPRESENTATION_KEYS` `IfcRepresentation.Keys`, ONE polymorphic content-keyer over `IfcObjectDefinition`) [M2], `OwnerStamp` `OwnerHistory` [H9], and `IfcClass.Span` schema window [H8] stay on the IFC-sourced node path; `Bags` lands `IfcPropertySet`/`IfcElementQuantity`→`PropertySet`/`QuantitySet` bag nodes whose typed values the `PropertyLowering` narrowing fills — every magnitude coerced native-unit→SI through the ONE per-projection `UnitScale` (built once off `IfcUnitAssignment.ScaleSI` per base axis plus `ScaleAngle`, the dimensional-factor generalization of the composition owner's `LengthScale`; the model `Tolerance` coerces by the same length factor before it grids any content hash) — and whose `Semantics/properties#PROPERTY_TEMPLATES` `PropertyInheritance.ModeOf` `InheritanceMode` is stamped at ingest [H1] so the seam `Bake` applies type→occurrence precedence wholly within the seam; `Materials` lands `Material` nodes through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.Project`; `ConnectionProjection.All` (`Semantics/connection#CONNECTION_DETAIL`) lands the realizing-element detail bags and their edges in the same concat; `SourceBag` synthesizes the entity-attribute Import bags Capture attaches — the `IfcDistributionPort` `FlowDirection`/`SystemType` pair the `Model/systems#SYSTEM_TRACE` directed trace reads and the `Model/structural#STRUCTURAL_PROJECTION` definition bags (member/connection/activity/load-group/load-case/result-group/analysis-model); the analytical Axis/FootPrint geometry is content-keyed in `Representations` by `IfcRepresentation.Keys` (never inlined on the node), `Rasm.Compute` resolving it one-hop by content key from the blob store; `GeoReferenceProjector.Project` lands the `Header.GeoReference` [M1]; `EdgeProjection.All` lands every `IfcRel*` neutral edge [C5] — the decomposition/connection/assignment/void families, the property/quantity attachment, the structural member↔connection/member↔activity `Generic` edges (the `StructuralProjection.Attrs` 6-DOF restraint + full load family + `LoadKind`/`Case` and the `AtStart` discriminant riding the payload), the space↔surface `Generic` edges, and the material `Associate` edges with the occurrence-usage payload [C7].
- Receipt: the `GraphDelta` is the projector's whole contribution — a merge over the canonical `ElementGraph` that `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` folds with the other projectors' deltas; the rooted/reconciled `NodeId` map keyed by `GlobalId` is the identity table aspect projectors attach against and `Emit` reverses.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new extracted IFC entity family is one `Extract<T>` arm on the `Project` fold landing its seam node; a new IFC value kind is one `PropertyLowering` arm, a new measure type is one `MeasureDimensions` row (its coercion DERIVES from the row's exponent vector — no unit column), and a new unit axis is one `UnitScale` factor slot; a new relationship is one `IfcRelKind` row the `EdgeProjection` reads (`Projection/relations#RELATION_ALGEBRA`); a new schema version is one `ReleaseMap` row the railed `ReleaseLower`/`ReleaseRaise` resolve and the `Model/elements#IFC_CLASS` span validates; never a second element record beside the seam graph and never a per-entity projector type.
- Boundary: the projector is the ONE GeometryGym→seam lowering — the retired `BimModel.Project` produced a second stored `BimElement` keyed by `GlobalId`, and any owner that re-stores the element off the seam graph is the deleted form; GeometryGym is captured INTERNALLY (the `DatabaseIfc` field) and an `IfcProduct`/`IfcRel*`/`DatabaseIfc` type crossing the `IElementProjection.Project` signature is the named seam violation — the seam holds only `Node`/`Relationship`/`GraphDelta`; the rooted `NodeId` is a neutral kernel-minted id and the compressed IFC `GlobalId` is the node's `ExternalId` projection attribute (1:1) [H6] for IFC-sourced nodes, while canonical type hits rebind the IFC `GlobalId` only in `ObjectProjection.Rooted` and reuse the resolver's Materials Type Object identity; the IFC GUID never becomes the node identity and the from-scratch authoring path mints its own neutral id; the value-narrowing is Bim's (`PropertyLowering`) because an `IfcValue`/dataType string crossing a seam signature is the deleted form — the seam carries only the typed `PropertyValue`/`MeasureValue` cases; geometry is referenced by `RepresentationContentHash` only [M2] and an in-process BRep evaluation or a RhinoCommon handle is the named seam violation — the analytical Axis/FootPrint geometry is content-keyed in `Representations` by `IfcRepresentation.Keys` [M2] and NEVER inlined as a coordinate field on the `Object` node (an inline `Vector3`/`BoundaryPolygon`/`Axis` member is the deleted §4-RT-M2 violation), `Rasm.Compute` resolving the analytical axis/footprint one-hop by content key from the blob store; a Bim in-process BRep evaluation is the named seam violation and the seam carries the structural/spatial CONNECTIVITY on the neutral `Relationship.Generic` edges instead; `Emit` is a Bim-INTERNAL method on the projector, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the seam owns only ingress projection.

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

public readonly record struct IfcMaterialSignature(
    string Name,
    string Category,
    Option<string> Standard,
    Option<string> Grade,
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TypeTrust {
    public static readonly TypeTrust Canonical = new("canonical");
    public static readonly TypeTrust Imported = new("imported");
    public static readonly TypeTrust User = new("user");
}

public readonly record struct ReconciledType(
    Node.Object Type,
    MaterialId Material,
    ProfileRef Profile,
    Option<SectionProperties> Section,
    TypeTrust Trust);

public interface IIfcTypeReconciler {
    Fin<Option<ReconciledType>> Resolve(IfcTypeSignature signature, Op key);
}

public interface IIfcProfileStore {
    Option<IfcProfileDef> Find(ProfileRef profile);
    ProfileRef Preserve(IfcProfileDef profile, Op key);
}

public readonly record struct TypeNodeSeed(
    string GlobalId,
    NodeId Id,
    Option<string> ExternalId,
    Classification Classification,
    PredefinedType PredefinedType,
    string Name,
    string Tag,
    RepresentationContentHash Representations,
    Option<OwnerHistory> History,
    SchemaSpan Span,
    Option<PropertyBag> Source);

// The model's native-unit -> SI coercion record, built ONCE per projection off the context IfcUnitAssignment — the
// Semantics/composition#MATERIAL_COMPOSITION LengthScale generalized to every base axis, because GeometryGym stores
// EVERY magnitude in the model's declared units, never pre-coerced (a Revit mm export delivers mm lengths; treating
// them as already-SI is the mm-vs-metre import trap the composition owner names). Factor derives any measure's factor
// as the product of base-axis powers over the seam Dimension exponents — dimensional analysis, so the one
// MeasureDimensions table needs no per-row unit column — and the plane-angle rows (Dimensionless BY DESIGN) coerce
// through the ScaleAngle factor keyed by measure-type name. ScaleSI answers 1.0 for an undeclared axis, so an SI or
// unitless model is the identity record. NAMED bounded drops: a Celsius-declared thermodynamic temperature is affine
// (offset, not scale) and coerces by factor alone; a per-property/per-quantity IfcNamedUnit override (IfcValue.Unit /
// IfcPhysicalSimpleQuantity.Unit) rides the project factor, never its own — each a next-campaign row, never silent.
public readonly record struct UnitScale(double L, double M, double T, double I, double Th, double N, double J, double Angle) {
    public static readonly UnitScale Si = new(1, 1, 1, 1, 1, 1, 1, 1);

    public static UnitScale Of(DatabaseIfc db) =>
        Optional(db.Context?.UnitsInContext).Match(
            None: () => Si,
            Some: units => new UnitScale(
                units.ScaleSI(IfcUnitEnum.LENGTHUNIT), units.ScaleSI(IfcUnitEnum.MASSUNIT),
                units.ScaleSI(IfcUnitEnum.TIMEUNIT), units.ScaleSI(IfcUnitEnum.ELECTRICCURRENTUNIT),
                units.ScaleSI(IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT), units.ScaleSI(IfcUnitEnum.AMOUNTOFSUBSTANCEUNIT),
                units.ScaleSI(IfcUnitEnum.LUMINOUSINTENSITYUNIT), db.ScaleAngle()));

    public double Factor(Dimension dim, string measureType = "") =>
        measureType is "IfcPlaneAngleMeasure" or "IfcSolidAngleMeasure"
            ? Angle
            : Math.Pow(L, dim.Length) * Math.Pow(M, dim.Mass) * Math.Pow(T, dim.Time) * Math.Pow(I, dim.Current)
                * Math.Pow(Th, dim.Temperature) * Math.Pow(N, dim.Amount) * Math.Pow(J, dim.LuminousIntensity);
}

public sealed record ObjectProjection(Seq<Node> Nodes, Seq<Relationship> Edges, Map<string, NodeId> Rooted) {
    public static ObjectProjection Empty(Map<string, NodeId> rooted) =>
        new(Seq<Node>(), Seq<Relationship>(), rooted);

    public ObjectProjection Capture(string globalId, Node.Object node, Option<PropertyBag> source, double tolerance) =>
        source.Match(
            Some: bag => {
                var seed = new Node.PropertySet(NodeId.Rooted(), bag);
                // Class-root [Union] Node cases generate no `with` — the content re-stamp is the seam Node.Relabel.
                var properties = (Node.PropertySet)seed.Relabel(NodeId.Content(seed.ToCanonicalBytes(tolerance).Span));
                return this with {
                    Nodes = Nodes.Add(node).Add(properties),
                    Edges = Edges.Add(new Relationship.Assign(node.Id, properties.Id, AssignKind.PropertyDefinition)),
                    Rooted = Rooted.AddOrUpdate(globalId, node.Id)
                };
            },
            None: () => this with {
                Nodes = Nodes.Add(node),
                Rooted = Rooted.AddOrUpdate(globalId, node.Id)
            });
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The one GeometryGym->seam lowering: the DatabaseIfc is captured internally (the IElementProjection contract holds only
// Node/Relationship/GraphDelta), and Project mints the neutral rooted identity while recording the IFC GlobalId as the
// node ExternalId 1:1 [H6]. Emit is Bim-internal, NOT a seam member. Every fault lifts BARE off ctx.Key (band 2600 IS the
// Expected Code; no .ToError() hop) per Model/faults#FAULT_BAND.
public sealed partial class SemanticProjector(DatabaseIfc db, IIfcTypeReconciler typeReconciler, IIfcProfileStore profiles) : IElementProjection {
    // Capture-promotion: primary-ctor params scope to the DECLARING part only, so the one store is promoted to a field
    // the egress partial (Projection/egress ReauthorMaterials) reads — never a re-passed Emit parameter.
    readonly IIfcProfileStore profiles = profiles;

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
        // The seam is SI-canonical: the one per-projection UnitScale coerces every native-unit magnitude — property
        // and quantity values through PropertyLowering, and the geometric Tolerance itself (a mm model's Tolerance is
        // a mm value; an uncoerced tolerance mis-grids every content-hash quantization against the SI measures).
        UnitScale scale = UnitScale.Of(db);
        double tolerance = db.Tolerance * scale.L;
        return GeoReferenceProjector.Project(project, key).Bind(geo =>
            ReleaseLower(db.Release, key).Bind(schema => {
                var header = new Header(schema, ViewLower(db.ModelView), geo, tolerance, ctx.At, StepHeaderOf(db));
                return Objects(project, rooted, typeReconciler, profiles, tolerance, key).Bind(objects => {
                    Seq<(Node Bag, Relationship Edge)> details = ConnectionProjection.All(project, objects.Rooted, tolerance);
                    Seq<Node> nodes = Classify(project, objects.Rooted, objects.Nodes
                        .Concat(Bags(project, objects.Rooted, scale))
                        .Concat(Materials(project, tolerance, key))
                        .Concat(details.Map(static d => d.Bag)).ToSeq());
                    return EdgeProjection.All(project, objects.Rooted, tolerance, key)
                    .Map(edges => {
                        GraphDelta seeded = nodes.Fold(GraphDelta.Empty.Reheader(header), static (delta, node) => delta.Put(node));
                        return (edges + objects.Edges + details.Map(static d => d.Edge)).Fold(seeded, static (delta, edge) => delta.Link(edge));
                    });
                });
            }));
    }

    // Every NON-TYPE IfcObjectDefinition -> Object.Occurrence and each admitted IfcTypeObject -> Object.Type through
    // ObjectProjection.Capture. The occurrence sweep is OBJECT-DEFINITION-WIDE, not product-wide: the IfcProject context
    // root (the Model/spatial#SPATIAL_STRUCTURE SpatialClass.IsRoot node the spatial tree resolves and the
    // project->site Aggregates endpoint), the IfcGroup subtree (the Model/zones#ZONE_GRAPH grouping nodes —
    // systems/zones/load groups), and the process/control/actor/resource families the rostered
    // Sequence/AssignsToControl/Process/Actor edges reference — a product-only sweep stranded every one of those
    // edges on a nodeless endpoint and faulted the IfcLegality Aggregate rule on the project itself.
    // Canonical TypeNodeSeed copies the resolver's canonical seam Type Object; imported TypeNodeSeed preserves the IFC
    // type source and stamps PropertySource.Import. ObjectProjection.Rooted is rebound before Classify/Bags/EdgeProjection
    // read relationships, so DefinesByType edges point at the same canonical/imported Type node Project emitted.
    // The generic Classification("ifc", classKey) carries the entity type WITHOUT leaking IfcClass onto the node; the PredefinedType
    // token reads off the entity's per-class predefined property; RepresentationContentHash is the keyed geometry map
    // [M2]; OwnerHistory rides optionally [H9]; ExternalId is the 1:1 GlobalId for IFC-sourced nodes [H6];
    // canonical type hits keep the resolver Type Object identity; Span is the class schema window [H8].
    // Ingress is PERMISSIVE: an unrostered/IFC4-new leaf lands the IfcClass.BuildingElementProxy row through the raw-name TryGet over the generated
    // roster (IfcClass.Proxy binds the REAL deprecated IfcProxy entity under the mechanical render law, never the fallback) so one
    // unknown entity never aborts the whole import — class validity is the Emit egress gate (AdmitPredefined), never here.
    // The analytical Axis/FootPrint geometry is content-keyed in Representations by IfcRepresentation.Keys (the ONE polymorphic
    // representation content-keyer maps every RepresentationIdentifier — Axis/Body/Box/FootPrint — to its content hash [M2]),
    // NEVER inlined as a coordinate field on the seam Object node (no Vector3/AxisCurve member exists — the deleted §4-RT-M2
    // violation); Rasm.Compute RESOLVES the analytical axis/footprint one-hop BY CONTENT KEY from the blob store.
    static Fin<ObjectProjection> Objects(IfcProject project, Map<string, NodeId> rooted, IIfcTypeReconciler reconciler, IIfcProfileStore profiles, double tolerance, Op key) {
        Map<string, IfcMaterialSelect> materials = MaterialIndex(project);
        ObjectProjection occurrences = project.Extract<IfcObjectDefinition>().AsIterable()
            .Filter(static definition => definition is not IfcTypeObject)
            .Fold(ObjectProjection.Empty(rooted), (projection, definition) =>
                projection.Capture(definition.GlobalId, ObjectNode(definition, ObjectKind.Occurrence, rooted), SourceBag(definition), tolerance));
        return project.Extract<IfcTypeObject>().AsIterable().ToSeq()
            .TraverseM(type => AdmitType(type, materials, rooted, reconciler, profiles, key)).As()
            .Map(types => types.Fold(occurrences, (projection, seed) =>
                projection.Capture(seed.GlobalId, TypeNode(seed), seed.Source, tolerance)));
    }

    // The relating-material index built ONCE per projection: object GlobalId -> the IfcMaterialSelect its
    // IfcRelAssociatesMaterial binds (the typed select, never a BaseClassIfc upcast) — the per-type FirstOrDefault
    // scan over every material relation was O(types x relations), the deleted quadratic form.
    static Map<string, IfcMaterialSelect> MaterialIndex(IfcProject project) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Fold(Map<string, IfcMaterialSelect>(), static (map, rel) =>
                Optional(rel.RelatingMaterial).Match(
                    Some: material => rel.RelatedObjects.OfType<IfcRoot>().Fold(map, (acc, root) => acc.AddOrUpdate(root.GlobalId, material)),
                    None: () => map));

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

    // The generated roster carries the retired *StandardCase/*ElementedCase subtypes as COMMITTED rows (their closed
    // SchemaSpan gates them at egress), so the raw-name TryGet resolves a 2x3 IfcWallStandardCase to its own row —
    // BuildingElementProxy is reached only by a genuinely unrostered leaf, and the strict Resolve path Canonical-folds internally.
    static Node.Object ObjectNode(IfcObjectDefinition definition, ObjectKind kind, Map<string, NodeId> rooted) {
        IfcClass cls = IfcClass.TryGet(ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _)).IfNone(IfcClass.BuildingElementProxy);
        return new Node.Object(
            Id:             rooted[definition.GlobalId],
            Kind:           kind,
            ExternalId:     Some(definition.GlobalId),
            Classification: Classification.Create("ifc", cls.Key, "", None, None, None),
            PredefinedType: Predefined(definition),
            Name:           definition.Name ?? "",
            Tag:            (definition as IfcElement)?.Tag ?? "",
            Representations: IfcRepresentation.Keys(definition),
            History:        OwnerStamp(definition.OwnerHistory),
            Span:           cls.Span);
    }

    static Node.Object TypeNode(TypeNodeSeed seed) =>
        new(
            Id:              seed.Id,
            Kind:            ObjectKind.Type,
            ExternalId:      seed.ExternalId,
            Classification:  seed.Classification,
            PredefinedType:  seed.PredefinedType,
            Name:            seed.Name,
            Tag:             seed.Tag,
            Representations: seed.Representations,
            History:         seed.History,
            Span:            seed.Span);

    static Fin<TypeNodeSeed> AdmitType(IfcTypeObject definition, Map<string, IfcMaterialSelect> materials, Map<string, NodeId> rooted, IIfcTypeReconciler reconciler, IIfcProfileStore profiles, Op key) {
        IfcTypeSignature signature = TypeSignatureOf(definition, materials, profiles, key);
        return reconciler.Resolve(signature, key).Map(resolved =>
            resolved.Match(
                Some: type => CanonicalTypeSeed(definition.GlobalId, type),
                None: () => ImportedTypeSeed(definition, signature, rooted)));
    }

    static TypeNodeSeed CanonicalTypeSeed(string globalId, ReconciledType type) =>
        new(
            GlobalId:        globalId,
            Id:              type.Type.Id,
            ExternalId:      type.Type.ExternalId,
            Classification:  type.Type.Classification,
            PredefinedType:  type.Type.PredefinedType,
            Name:            type.Type.Name,
            Tag:             type.Type.Tag,
            Representations: type.Type.Representations,
            History:         type.Type.History,
            Span:            type.Type.Span,
            Source:          Option<PropertyBag>.None);

    static TypeNodeSeed ImportedTypeSeed(IfcTypeObject definition, IfcTypeSignature signature, Map<string, NodeId> rooted) {
        IfcClass cls = IfcClass.TryGet(ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _)).IfNone(IfcClass.BuildingElementProxy);
        return new(
            GlobalId:        definition.GlobalId,
            Id:              rooted[definition.GlobalId],
            ExternalId:      Some(definition.GlobalId),
            Classification:  Classification.Create("ifc", cls.Key, "", None, None, None),
            PredefinedType:  Predefined(definition),
            Name:            definition.Name ?? "",
            Tag:             (definition as IfcElement)?.Tag ?? "",
            Representations: IfcRepresentation.Keys(definition),
            History:         OwnerStamp(definition.OwnerHistory),
            Span:            cls.Span,
            Source:          Some(ImportedSource(signature)));
    }

    // The signature's sub-kind slot carries the EFFECTIVE type token: for a USERDEFINED type the IFC convention puts
    // the real token in ElementType, so the signature folds that label in — the reconciler matches the user vocabulary
    // ("PARTY-WALL"), never the unspecific "USERDEFINED" marker, and the label survives in the Import source bag.
    static IfcTypeSignature TypeSignatureOf(IfcTypeObject definition, Map<string, IfcMaterialSelect> materials, IIfcProfileStore profiles, Op key) {
        Option<IfcMaterialSelect> relatingMaterial = materials.Find(definition.GlobalId);
        string token = Predefined(definition).Token;
        return new(
            definition.GlobalId,
            ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _),
            token == "USERDEFINED" && (definition as IfcElementType)?.ElementType is { Length: > 0 } label ? label : token,
            definition.Name ?? "",
            MaterialSignatureOf(relatingMaterial),
            ProfileSignatureOf(relatingMaterial, profiles, key));
    }

    static Option<IfcMaterialSignature> MaterialSignatureOf(Option<IfcMaterialSelect> relatingMaterial) =>
        relatingMaterial.Bind(MaterialOf).Map(material => new IfcMaterialSignature(
            material.Name ?? "",
            material.Category ?? "",
            Option<string>.None,
            Option<string>.None,
            Option<string>.None));

    static Option<IfcProfileSignature> ProfileSignatureOf(Option<IfcMaterialSelect> relatingMaterial, IIfcProfileStore profiles, Op key) =>
        relatingMaterial.Bind(ProfileOf).Map(profile => {
            ProfileRef preserved = profiles.Preserve(profile, key);
            return new IfcProfileSignature(
                Standard: preserved.Standard,
                Designation: string.IsNullOrWhiteSpace(preserved.Designation) ? profile.ProfileName ?? "" : preserved.Designation,
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
        IfcMaterialProfileSet profileSet => Optional(profileSet.MaterialProfiles.FirstOrDefault()?.Profile),
        IfcMaterialProfileSetUsage usage => Optional(usage.ForProfileSet?.MaterialProfiles.FirstOrDefault()?.Profile),
        _ => Option<IfcProfileDef>.None
    };

    // The projector-minted signature bag's well-known set name — the egress AuthorBag skips it by THIS symbol (the
    // NestOrdinal precedent), so reconciliation bookkeeping never exports as a phantom IfcPropertySet the source
    // file never carried.
    internal static readonly string TypeSignatureSet = "IfcTypeSignature";

    // The two synthesized entity-attribute bag set symbols — egress skips both beside TypeSignatureSet (the
    // attributes re-author on the entity at Emit, never as a phantom Pset the source file never carried).
    internal static readonly string PortAttributeSet = "IfcDistributionPort";
    internal static readonly string StructuralDefinitionSet = "IfcStructuralDefinition";

    // Entity-borne facts with no IfcPropertySet carrier land as synthesized Import bags through the SAME Capture
    // path the type-signature bag rides: the port flow attributes (the Model/systems#SYSTEM_TRACE directed-trace
    // inputs — an unsurfaced FlowDirection reads NOTDEFINED and degrades every trace to undirected reachability)
    // and the structural definition bags (the Model/structural#STRUCTURAL_PROJECTION entity-level Attrs arms:
    // member/connection/activity/load-group/load-case/result-group/analysis-model). An entity with neither, or an
    // empty structural read, yields None — no empty-bag node.
    static Option<PropertyBag> SourceBag(IfcObjectDefinition definition) => definition switch {
        IfcDistributionPort port => Some(new PropertyBag(
            PortAttributeSet,
            Map(
                (PropertyName.Create("FlowDirection"), (PropertyValue)new PropertyValue.Text(port.FlowDirection.ToString())),
                (PropertyName.Create("SystemType"), new PropertyValue.Text(port.SystemType.ToString()))),
            InheritanceMode.OccurrenceWins,
            PropertySource.Import)),
        IfcStructuralItem or IfcStructuralActivity or IfcStructuralLoadGroup or IfcStructuralResultGroup or IfcStructuralAnalysisModel =>
            StructuralProjection.Attrs(definition) is { IsEmpty: false } attrs
                ? Some(new PropertyBag(StructuralDefinitionSet, attrs, InheritanceMode.OccurrenceWins, PropertySource.Import))
                : None,
        _ => None,
    };

    // An absent signature axis is an ABSENT KEY — the bag carries only present facts, so a consumer probes membership
    // rather than unwrapping a Present/Value ceremony (the deleted Complex-encoded Option wrapper).
    static PropertyBag ImportedSource(IfcTypeSignature signature) =>
        new PropertyBag(
            TypeSignatureSet,
            Seq(("MaterialName", signature.Material.Map(static m => m.Name)),
                ("MaterialCategory", signature.Material.Map(static m => m.Category)),
                ("MaterialStandard", signature.Material.Bind(static m => m.Standard)),
                ("MaterialGrade", signature.Material.Bind(static m => m.Grade)),
                ("ProfileStandard", signature.Profile.Map(static p => p.Standard)),
                ("ProfileDesignation", signature.Profile.Map(static p => p.Designation)),
                ("ProfileEntity", signature.Profile.Map(static p => p.IfcEntity)),
                ("ProfileStepKey", signature.Profile.Map(static p => p.StepKey)))
            .Fold(
                Map<PropertyName, PropertyValue>()
                    .Add(PropertyName.Create("GlobalId"), new PropertyValue.Text(signature.GlobalId))
                    .Add(PropertyName.Create("IfcEntity"), new PropertyValue.Text(signature.IfcEntity))
                    .Add(PropertyName.Create("PredefinedType"), new PropertyValue.Text(signature.PredefinedType))
                    .Add(PropertyName.Create("Name"), new PropertyValue.Text(signature.Name)),
                static (bag, row) => row.Item2.Match(
                    Some: value => bag.Add(PropertyName.Create(row.Item1), new PropertyValue.Text(value)),
                    None: () => bag)),
            InheritanceMode.TypeDrivenOnly,
            PropertySource.Import);

    // The predefined token is a strongly-typed per-class enum member (IfcWall.PredefinedType is IfcWallTypeEnum, etc.),
    // so a live occurrence carries it on a reflected PredefinedType property, NOT on the class-name split — the seam owns
    // the PredefinedType value-object and admits the token bare (validity is the Emit egress gate [C6]), an empty/NOTDEFINED
    // token folding to the IFC default. NAMED bounded drop: a USERDEFINED occurrence's ObjectType label has no seam
    // Node.Object slot, so it does not round-trip — the egress re-derives the label from the node Name; the carrier is
    // seam-owned Rasm.Element work (types preserve theirs through the signature bag).
    static PredefinedType Predefined(IfcObjectDefinition definition) {
        string token = definition.GetType().GetProperty(nameof(IfcWall.PredefinedType))?.GetValue(definition)?.ToString() ?? "";
        return string.IsNullOrWhiteSpace(token) || string.Equals(token, "NOTDEFINED", StringComparison.OrdinalIgnoreCase)
            ? PredefinedType.NotDefined
            : PredefinedType.Create(token);
    }

    // PropertySet/QuantitySet bag nodes whose seam PropertyBag/QuantityBag carries the typed value the PropertyLowering
    // narrowing fills and the InheritanceMode the projector stamps at ingest [H1] so the seam Bake resolves type->occurrence
    // precedence without re-reading IFC; a Pset whose DefinesType inverse is non-empty is type-bound (the IFC type-driven
    // signal), and the Semantics/properties#PROPERTY_TEMPLATES PropertyInheritance.ModeOf classifies the set name onto the
    // seam InheritanceMode. Two NAMED bounded drops on the NestOrdinal precedent, never silent: the IfcPreDefinedPropertySet
    // family (the 2x3/4.0 door/window lining+panel property records, 4.3-deprecated in favor of plain Psets) and the
    // IfcPhysicalComplexQuantity grouping (its simple children are not flattened — a carrier is a next-campaign row).
    static Seq<Node> Bags(IfcProject project, Map<string, NodeId> rooted, UnitScale scale) {
        var properties = project.Extract<IfcPropertySet>().AsIterable().Map(ps => (Node)new Node.PropertySet(
            rooted[ps.GlobalId],
            new PropertyBag(
                ps.Name ?? "",
                ps.HasProperties.Values.Aggregate(Map<PropertyName, PropertyValue>(),
                    (bag, p) => bag.AddOrUpdate(PropertyName.Create(p.Name ?? ""), PropertyLowering.Lower(p, rooted, scale))),
                PropertyInheritance.ModeOf(ps.Name ?? "", IsTypeBound(ps)),
                PropertySource.Import)));
        var quantities = project.Extract<IfcElementQuantity>().AsIterable().Map(eq => (Node)new Node.QuantitySet(
            rooted[eq.GlobalId],
            new QuantityBag(
                eq.Name ?? "",
                eq.Quantities.Values.OfType<IfcPhysicalSimpleQuantity>().Aggregate(Map<PropertyName, MeasureValue>(),
                    (bag, q) => bag.AddOrUpdate(PropertyName.Create(q.Name ?? ""), PropertyLowering.Measure(q, scale))),
                PropertyInheritance.ModeOf(eq.Name ?? "", IsTypeBound(eq)),
                PropertySource.Import)));
        return properties.Concat(quantities).ToSeq();
    }

    // The IFC type-driven signal: a property-set definition whose DefinesType inverse is non-empty is bound to a type
    // object (so its occurrence merge is type-driven), read off the GeometryGym SET<IfcTypeObject> inverse, never the
    // unrelated IsDefinedBy (the IfcRelDefinesByTemplate set).
    static bool IsTypeBound(IfcPropertySetDefinition set) => set.DefinesType.Any();

    // Non-rooted material nodes are content-keyed (kernel seed-zero XxHash128 over ToCanonicalBytes [H7]) by
    // MaterialProjection.Project, never GlobalId-rooted; the composition fold (Single/LayerSet/ProfileSet/ConstituentSet)
    // is the IFC material algebra Semantics/composition owns. The node-side ToOption skip is fault-site discipline,
    // not tolerance: the SAME Project failure aborts the projection typed at the MaterialEdges fold (relations.md
    // TraverseM), so the one malformed material faults ONCE at its edge, never twice.
    static Seq<Node> Materials(IfcProject project, double tolerance, Op key) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Choose(rel => Optional(rel.RelatingMaterial)
                .Bind(select => MaterialProjection.Project(select, tolerance, key).ToOption()))
            .Map(static m => (Node)m)
            .DistinctBy(static node => node.Id)
            .ToSeq();

    // IfcOwnerHistory -> seam OwnerHistory [H9]: owning user/app, created/modified (DateTime, NOT a unix long), change
    // action, state. Absent owner history yields None so a headerless model still projects; Emit re-derives ChangeAction.
    // OwningUser reads IfcPersonAndOrganization.Name (the decompile-verified TheOrganization.Name + ThePerson.Name
    // composition) — a GG entity ToString() emits its STEP record line, the serialization leak this owner refuses.
    static Option<OwnerHistory> OwnerStamp(IfcOwnerHistory? history) =>
        Optional(history).Map(static h => new OwnerHistory(
            OwningUser:        h.OwningUser?.Name ?? "",
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

    // The two currency leaks meet at this projector and nowhere else, RAILED through the ONE frozen
    // Model/elements#TAXONOMY_EMITTER ReleaseMap both legs read (the ingress half reads Lower here; the egress half's
    // ReleaseRaise reads ReleaseMap.Raise, its identity-name-derived inverse, on the Projection/egress fence): an
    // out-of-map GGRelease (IFC4X4_DRAFT, excluded by law) rails BimFault.CodecReject BARE — the prior `?? Ifc4X3Add2`
    // coercion silently rewrote an unknown schema and is the deleted masked-error form. The GeometryGym currency never
    // reaches the Header and the seam currency never reaches `new DatabaseIfc`.
    internal static Fin<ReleaseVersion> ReleaseLower(GGRelease release, Op key) =>
        ReleaseMap.Lower.TryGetValue(release, out ReleaseVersion? lowered) && lowered is { } seam
            ? Fin.Succ(seam)
            : Fin.Fail<ReleaseVersion>(new BimFault.CodecReject(key, $"release-unmapped:{release}"));

    // The GG MVD enum lowered by EXPLICIT member arms over the decompile-verified 11-member ModelView — the two
    // Reference MVDs land Ifc4Reference, DesignTransfer and AlignmentBasedView their seam rows; the final arm is the
    // DELIBERATE policy row folding Ifc2x3Coordination and the whole *NotAssigned family onto Coordination (an
    // unassigned MVD is the coordination-view default), never a Contains() heuristic over enum names.
    static ModelView ViewLower(GGView view) => view switch {
        GGView.Ifc4Reference or GGView.IFC4X3Reference => ModelView.Ifc4Reference,
        GGView.Ifc4DesignTransfer                      => ModelView.DesignTransfer,
        GGView.IFC4X3AlignmentBasedView                => ModelView.Alignment,
        _                                              => ModelView.Coordination,
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The Bim-internal IFC value narrowing the seam delegates to the projector: an IfcProperty narrows onto the seam's
// ten-case PropertyValue and an IfcPhysicalSimpleQuantity onto a MeasureValue over the seam Dimension, every magnitude
// coerced native-unit -> SI through the one per-projection UnitScale (GG never pre-coerces — the composition owner's
// mm-vs-metre law). The seam forbids an IfcValue or a dataType string crossing its signature, so this narrowing is
// Bim's (the seam carries only the typed cases). A measured value's dimension reads off the IFC measure-type name
// through the frozen MeasureDimensions table (the H2 dimension support); an unmapped measure preserves its value as
// Text rather than claiming a wrong dimension.
internal static class PropertyLowering {
    // GG splits the schema's two value SELECTs into SIBLING bases — IfcMeasureValue and IfcDerivedMeasureValue BOTH
    // derive IfcValue directly (decompile-verified), so the narrowing guard matches BOTH: a guard on IfcMeasureValue
    // alone dead-codes every derived row (Force/Pressure/Density/ThermalTransmittance and the whole MEP set), the
    // illusory-coverage defect this table's roster closed. Every row is a decompile-verified GG IfcValue type over
    // its SI base; Dimension.Create exponent order is (L, M, T, I, Θ, N, J). Angle/ratio/count rows are
    // Dimensionless BY DESIGN — the QuantityType measure-type name, not the vector, is the round-trip identity.
    // INTERNAL, not private: the Projection/egress typed-measure mint derives its raise table from these keys — one
    // table, both directions, the ReleaseMap law — so ingress narrowing and egress raising can never drift.
    internal static readonly FrozenDictionary<string, Dimension> MeasureDimensions = new Dictionary<string, Dimension>(StringComparer.Ordinal) {
        // IfcMeasureValue family — SI base + dimensionless tokens
        ["IfcLengthMeasure"] = Dimension.LengthDim, ["IfcPositiveLengthMeasure"] = Dimension.LengthDim, ["IfcNonNegativeLengthMeasure"] = Dimension.LengthDim,
        ["IfcAreaMeasure"] = Dimension.AreaDim, ["IfcVolumeMeasure"] = Dimension.VolumeDim, ["IfcMassMeasure"] = Dimension.MassDim,
        ["IfcTimeMeasure"] = Dimension.DurationDim, ["IfcThermodynamicTemperatureMeasure"] = Dimension.Create(0, 0, 0, 0, 1, 0, 0),
        ["IfcElectricCurrentMeasure"] = Dimension.Create(0, 0, 0, 1, 0, 0, 0), ["IfcLuminousIntensityMeasure"] = Dimension.Create(0, 0, 0, 0, 0, 0, 1),
        ["IfcPlaneAngleMeasure"] = Dimension.Dimensionless, ["IfcSolidAngleMeasure"] = Dimension.Dimensionless, ["IfcCountMeasure"] = Dimension.Dimensionless,
        ["IfcRatioMeasure"] = Dimension.Dimensionless, ["IfcPositiveRatioMeasure"] = Dimension.Dimensionless, ["IfcNormalisedRatioMeasure"] = Dimension.Dimensionless,
        // IfcDerivedMeasureValue family — structural
        ["IfcForceMeasure"] = Dimension.ForceDim, ["IfcPressureMeasure"] = Dimension.PressureDim, ["IfcMassDensityMeasure"] = Dimension.DensityDim,
        ["IfcModulusOfElasticityMeasure"] = Dimension.PressureDim, ["IfcPlanarForceMeasure"] = Dimension.PressureDim,
        ["IfcLinearForceMeasure"] = Dimension.Create(0, 1, -2, 0, 0, 0, 0), ["IfcLinearStiffnessMeasure"] = Dimension.Create(0, 1, -2, 0, 0, 0, 0),
        ["IfcTorqueMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0), ["IfcRotationalStiffnessMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        ["IfcMomentOfInertiaMeasure"] = Dimension.Create(4, 0, 0, 0, 0, 0, 0), ["IfcSectionModulusMeasure"] = Dimension.VolumeDim,
        ["IfcModulusOfSubgradeReactionMeasure"] = Dimension.Create(-2, 1, -2, 0, 0, 0, 0), ["IfcMassPerLengthMeasure"] = Dimension.Create(-1, 1, 0, 0, 0, 0, 0),
        ["IfcAreaDensityMeasure"] = Dimension.Create(-2, 1, 0, 0, 0, 0, 0),
        // IfcDerivedMeasureValue family — thermal, energy, hygric, flow
        ["IfcThermalTransmittanceMeasure"] = Dimension.ThermalTransmittanceDim, ["IfcThermalAdmittanceMeasure"] = Dimension.ThermalTransmittanceDim,
        ["IfcThermalConductivityMeasure"] = Dimension.Create(1, 1, -3, 0, -1, 0, 0), ["IfcSpecificHeatCapacityMeasure"] = Dimension.Create(2, 0, -2, 0, -1, 0, 0),
        ["IfcThermalExpansionCoefficientMeasure"] = Dimension.Create(0, 0, 0, 0, -1, 0, 0), ["IfcHeatFluxDensityMeasure"] = Dimension.Create(0, 1, -3, 0, 0, 0, 0),
        ["IfcPowerMeasure"] = Dimension.Create(2, 1, -3, 0, 0, 0, 0), ["IfcEnergyMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        ["IfcVolumetricFlowRateMeasure"] = Dimension.Create(3, 0, -1, 0, 0, 0, 0), ["IfcMassFlowRateMeasure"] = Dimension.Create(0, 1, -1, 0, 0, 0, 0),
        ["IfcVaporPermeabilityMeasure"] = Dimension.Create(0, 0, 1, 0, 0, 0, 0), ["IfcMoistureDiffusivityMeasure"] = Dimension.Create(3, 0, -1, 0, 0, 0, 0),
        ["IfcIsothermalMoistureCapacityMeasure"] = Dimension.Create(3, -1, 0, 0, 0, 0, 0), ["IfcDynamicViscosityMeasure"] = Dimension.Create(-1, 1, -1, 0, 0, 0, 0),
        ["IfcKinematicViscosityMeasure"] = Dimension.Create(2, 0, -1, 0, 0, 0, 0), ["IfcMolecularWeightMeasure"] = Dimension.Create(0, 1, 0, 0, 0, -1, 0),
        // IfcDerivedMeasureValue family — electrical, lighting, acoustic, motion
        ["IfcElectricVoltageMeasure"] = Dimension.Create(2, 1, -3, -1, 0, 0, 0), ["IfcFrequencyMeasure"] = Dimension.Create(0, 0, -1, 0, 0, 0, 0),
        ["IfcRotationalFrequencyMeasure"] = Dimension.Create(0, 0, -1, 0, 0, 0, 0), ["IfcAngularVelocityMeasure"] = Dimension.Create(0, 0, -1, 0, 0, 0, 0),
        ["IfcLuminousFluxMeasure"] = Dimension.Create(0, 0, 0, 0, 0, 0, 1), ["IfcIlluminanceMeasure"] = Dimension.Create(-2, 0, 0, 0, 0, 0, 1),
        ["IfcSoundPowerMeasure"] = Dimension.Create(2, 1, -3, 0, 0, 0, 0), ["IfcSoundPressureMeasure"] = Dimension.PressureDim,
        ["IfcLinearVelocityMeasure"] = Dimension.Create(1, 0, -1, 0, 0, 0, 0), ["IfcAccelerationMeasure"] = Dimension.Create(1, 0, -2, 0, 0, 0, 0),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    // The IfcProperty family -> the seam PropertyValue ten-case union: a single value narrows by its IfcValue shape (the
    // three-valued IfcLogical to the seam Logical, never coerced to a two-valued Boolean), an enumerated value carries its
    // SELECTED value LIST (EnumerationValues, the [1:?] cardinality) plus its allowed set (the optional EnumerationReference),
    // a reference value its target NodeId plus its UsageName, a bounded value its lower/upper/setpoint measures, a table value
    // its rows plus the IfcCurveInterpolationEnum curve rule, a list the recursive arm, and an IfcComplexProperty its UsageName
    // plus its named sub-property bag (HasProperties keyed by each sub-property Name) RECURSING Lower — so a layered glazing /
    // multi-component rating / bSDD complex template is the seam Complex arm, never dropped to Text; only a non-IfcProperty
    // residue falls to Text. The rooted map resolves a reference whose target is a rooted node; a non-rooted reference target
    // (an IfcObjectReferenceSelect resource — a table, an address, a time series — never projected as a node) content-keys an
    // IDENTITY-ONLY NodeId (never the IFC GlobalId AS node identity [H6]) — a NAMED bounded drop: the resource entity itself
    // does not round-trip, the UsageName always carried, so the cycle never drops the three-valued logical, the curve rule,
    // the usage name, or the nested bag.
    public static PropertyValue Lower(IfcProperty property, Map<string, NodeId> rooted, UnitScale scale) => property switch {
        IfcPropertySingleValue sv     => LowerValue(sv.NominalValue, scale),
        IfcPropertyEnumeratedValue ev => new PropertyValue.Enumerated(
            ev.EnumerationValues.AsIterable().Map(static v => v.ValueString).ToSeq(),
            Optional(ev.EnumerationReference).Map(static r => r.EnumerationValues.AsIterable().Map(static v => v.ValueString).ToSeq()).IfNone(Seq<string>())),
        IfcPropertyReferenceValue rv  => new PropertyValue.Reference(
            Optional(rv.PropertyReference as IfcRoot).Bind(root => rooted.Find(root.GlobalId)).IfNone(() =>
                NodeId.Content(Encoding.UTF8.GetBytes(rv.PropertyReference is IfcRoot r ? $"ifcroot:{r.GlobalId}" : $"{rv.PropertyReference?.GetType().Name}:{rv.UsageName}"))),
            string.IsNullOrEmpty(rv.UsageName) ? Option<string>.None : Some(rv.UsageName)),
        IfcPropertyBoundedValue bv    => new PropertyValue.Bounded(MeasureOpt(bv.LowerBoundValue, scale), MeasureOpt(bv.UpperBoundValue, scale), MeasureOpt(bv.SetPointValue, scale)),
        IfcPropertyListValue lv       => new PropertyValue.List(lv.ListValues.AsIterable().Map(v => LowerValue(v, scale)).ToSeq()),
        IfcPropertyTableValue tv      => new PropertyValue.Table(tv.DefiningValues.Zip(tv.DefinedValues,
            static (def, val) => ((PropertyValue)new PropertyValue.Text(def.ValueString), (PropertyValue)new PropertyValue.Text(val.ValueString))).ToSeq(),
            InterpolationOf(tv.CurveInterpolation)),
        IfcComplexProperty cp         => new PropertyValue.Complex(cp.UsageName,
            cp.HasProperties.Values.Aggregate(Map<PropertyName, PropertyValue>(),
                (bag, sub) => bag.AddOrUpdate(PropertyName.Create(sub.Name ?? ""), Lower(sub, rooted, scale)))),
        _                             => new PropertyValue.Text(property.Name ?? ""),
    };

    // An IfcValue -> the seam PropertyValue scalar arm: a three-valued IfcLogical is the seam Logical (UNKNOWN -> None, never
    // coerced to a two-valued Boolean), a boolean-typed value (ValueType == bool) is Boolean, a measure value whose type the
    // dimension table carries is a typed Measure coerced to SI — the guard matches BOTH sibling bases (IfcMeasureValue
    // AND IfcDerivedMeasureValue; each types Value as a boxed double) — every other value its verbatim string. NAMED
    // bounded drops: an integer-typed leaf (IfcInteger) and the string-typed subtypes (IfcText/IfcIdentifier, re-emitting
    // as IfcLabel) ride the Text arm value-verbatim, their IfcValue type identity lost across the round-trip — the frozen
    // ten-case seam union carries no integer or string-subtype case; a carrier is a next-campaign case.
    static PropertyValue LowerValue(IfcValue? value, UnitScale scale) =>
        value is null                                                    ? new PropertyValue.Text("")
        : value is IfcLogical lg                                         ? new PropertyValue.Logical(LogicalOpt(lg.Logical))
        : value.ValueType == typeof(bool)                                ? new PropertyValue.Boolean(value.Value is bool b && b)
        : value is IfcMeasureValue or IfcDerivedMeasureValue
            && MeasureDimensions.TryGetValue(value.GetType().Name, out var dim) ? new PropertyValue.Measure(MeasureOf(value, dim, scale))
        : new PropertyValue.Text(value.ValueString);

    static Option<MeasureValue> MeasureOpt(IfcValue? value, UnitScale scale) =>
        value is IfcMeasureValue or IfcDerivedMeasureValue && MeasureDimensions.TryGetValue(value.GetType().Name, out var dim)
            ? Some(MeasureOf(value, dim, scale))
            : None;

    // The measure value (NATIVE-unit, the dimension resolved off the frozen MeasureDimensions table) -> the seam
    // MeasureValue through the SI-native OfSi factory, the magnitude coerced by the UnitScale dimensional factor —
    // GG stores the raw declared-unit magnitude, so an uncoerced admit is the mm-vs-metre trap the composition owner
    // names. The QuantityType is the IFC MEASURE-TYPE NAME (IfcThermalTransmittanceMeasure, IfcMassDensityMeasure, ...),
    // NOT the dimension, because the seven-exponent vector is not injective over quantity types (an IfcForceMeasure and
    // an out-of-family measure can share a dimension, and angle/ratio/count all sit at Dimensionless) — so the
    // measure-type identity round-trips, the plane-angle factor keys off that name, and a QTO accessor never
    // false-matches. The kernel UnitsNet registry is bypassed (the factor IS the coercion); a measure type the frozen
    // table does not carry stays Text upstream rather than claiming a wrong dimension.
    static MeasureValue MeasureOf(IfcValue measure, Dimension dimension, UnitScale scale) =>
        MeasureValue.OfSi(QuantityType.Create(measure.GetType().Name), dimension,
            AsDouble(measure.Value) * scale.Factor(dimension, measure.GetType().Name));

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

    // An IfcPhysicalSimpleQuantity -> the seam MeasureValue [H2]: the IFC base-quantity value is a NATIVE-unit
    // magnitude coerced to SI by the UnitScale dimensional factor, then admitted through the SI-native
    // MeasureValue.OfSi(QuantityType, Dimension, double) factory (never the UnitsNet Quantity.TryFrom/ToUnit registry,
    // which is for raw unit-bearing wire values) — each subtype stamps its named QuantityType so a QTO accessor
    // (measure.Area/Volume/Weight/Time/Count) reads the geometry-true takeoff type-tagged at ingest, never inferred
    // from the dimension. The count is a dimensionless tally (QuantityType.Count, factor-free by its empty exponent
    // vector), and an unrecognized simple quantity yields the dimensionless MeasureValue.Zero.
    public static MeasureValue Measure(IfcPhysicalSimpleQuantity quantity, UnitScale scale) => quantity switch {
        IfcQuantityLength q => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, q.LengthValue * scale.Factor(Dimension.LengthDim)),
        IfcQuantityArea q   => MeasureValue.OfSi(QuantityType.Area, Dimension.AreaDim, q.AreaValue * scale.Factor(Dimension.AreaDim)),
        IfcQuantityVolume q => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, q.VolumeValue * scale.Factor(Dimension.VolumeDim)),
        IfcQuantityWeight q => MeasureValue.OfSi(QuantityType.Mass, Dimension.MassDim, q.WeightValue * scale.Factor(Dimension.MassDim)),
        IfcQuantityCount q  => MeasureValue.OfSi(QuantityType.Count, Dimension.Dimensionless, q.CountValue),
        IfcQuantityTime q   => MeasureValue.OfSi(QuantityType.Duration, Dimension.DurationDim, q.TimeValue * scale.Factor(Dimension.DurationDim)),
        _                   => MeasureValue.Zero,
    };

    static double AsDouble(object? value) =>
        value is IConvertible c ? Convert.ToDouble(c, System.Globalization.CultureInfo.InvariantCulture) : 0.0;
}
```

## [03]-[GRAPH_LEGALITY]

- Owner: `IfcLegality` the `IGraphConstraint` deciding IFC-semantic legality the seam's structural `GraphDelta` switch cannot [M3] — the seam enforces only structural invariants (an edge endpoint resolves, an endpoint kind is legal), and IFC legality (which entity may relate to which, and which entity classes and predefined tokens the schema vocabulary admits) is Bim's, depended UP on through the `IGraphConstraint` contract. The vocabulary arms are Gate 1 of the three-gate enforcement: Gate 0 is the emitter stamp audit (design-time), Gate 2 the per-token `AdmitPredefined` span gate (egress) — span/rank gating stays at egress because no emit schema exists at composition time.
- Entry: `IfcLegality.Validate(GraphDelta delta, ElementGraph graph) → Validation<Error,Unit>` accumulates every IFC-legality violation in the delta against the graph — the `AddedEdges` fold applies the relationship rule set AND the `AddedNodes` fold applies the `Vocabulary` arm, `Success(unit)` when every rule holds, a `Fail` carrying the accumulated `Error` set otherwise; the validation is applicative (every violation reported at once, never short-circuit) so an authoring pass sees all rejects in one apply, the `BimFault.ModelRejected` arms surviving the `Error.Combine` because the band is `Expected`-derived.
- Auto: the EDGE rules dispatch on the seam's NEUTRAL case + sub-kind (the seam carries no `IfcRel*` case) — a `Compose` edge with the `Contain` sub-kind requires its `Whole` to resolve a spatial-container row on the SIBLING vocabularies (`Model/spatial#SPATIAL_STRUCTURE` `SpatialClass.IsContainer` for the site/building/storey/space + IFC4.3 facility/facility-part containers, `Model/zones#ZONE_GRAPH` `BimZoneKind.IsSpatial` for `IfcSpatialZone` — the disjoint partition; a private re-listed leaf set is the deleted drift form the spatial owner names, the six-row instance of which faulted every 4.3 infrastructure containment), a `Compose` edge with the `Aggregate` sub-kind may not have a `Type` object as its `Whole`, a spatial-to-spatial `Contain`/`Aggregate` edge (both endpoints resolving `SpatialClass` rows) must nest downward per `SpatialClass.CanContain` (`containment-rank-inverted` otherwise — a storey aggregating its site faults loud), a `Void` edge dispatches its SUB-KIND — `VoidKind.Void` requires its `Feature` (the ingest lands relating=host, related=feature) to be a feature subtraction (`IfcOpeningElement` or the 4.3 `IfcVoidingFeature`), `VoidKind.Fill` requires its `Host` to be one (the `Fills` row reads relating=opening, so the OPENING sits in the `Host` slot — a blanket `Feature` check rejected every legal fill) — and an `Assign` edge with the `TypeDefinition` sub-kind requires its `Definition` to be a `Type` object; the NODE `Vocabulary` arm requires every `"ifc"`-classified `Object` node to resolve a generated roster row through the strict `IfcClass.Resolve` — the ONE `Canonical`-folding lookup, so a retired `*StandardCase` code token-checks against its BASE row's set instead of its own empty committed set, while abstract supertypes stay legal classification vocabulary — and its predefined token to be a member of the resolved row's `ValidPredefined` set, or empty/`NOTDEFINED`/`USERDEFINED`, or the set empty; each rule lifts its failure onto `BimFault.ModelRejected` BARE and the applicative `Validation` accumulates them.
- Packages: Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new IFC-legality edge rule is one arm on the `Rule` switch and a new node rule one arm beside `Vocabulary`; a new spatial container is one `SpatialClass`/`BimZoneKind` row on its OWNING sibling vocabulary (this gate widens with zero edits); a feature-subtraction class is one `Subtraction` row; the structural invariants stay the seam's `GraphDelta` switch and never migrate here; never a per-rule validator type.
- Boundary: `IfcLegality` decides IFC-semantic legality ONLY — the structural invariants (endpoint resolution, endpoint-kind legality) are the seam's `GraphDelta` total switch and re-checking them here is the deleted form [M3]; the rules read the generic `Classification` code (`IfcSite`, `IfcOpeningElement`) and the `ObjectKind` (occurrence/type), never an `IfcProduct` runtime type (GeometryGym stays captured in the projector); the vocabulary arm is composition-time MEMBERSHIP only — schema-span/rank admission is the egress `AdmitPredefined` gate, never re-checked here; the validation is applicative-accumulating so an authoring pass sees every reject at once, never the first-fail short-circuit a `Fin` rail would give; the `BimFault` lifts BARE (the band IS the `Expected` `Code`) so `error.IsType<BimFault.ModelRejected>()` survives the `Error.Combine`.

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
    // The containment-whole gate reads the two SIBLING spatial vocabularies — Model/spatial#SPATIAL_STRUCTURE
    // SpatialClass.IsContainer (site/building/storey/space + the 4.3 facility/facility-part containers) and the
    // Model/zones#ZONE_GRAPH IsSpatial grouping rows (IfcSpatialZone) — the disjoint partition of the spatial-element
    // set; a private re-listed spatial FrozenSet is the deleted drift form the spatial owner names (it forks the
    // vocabulary and silently under-covers the next facility row).
    static bool SpatialWhole(string code) =>
        SpatialClass.TryGet(code).Map(static spatial => spatial.IsContainer).IfNone(false)
            || BimZoneKind.TryGet(code).Map(static zone => zone.IsSpatial).IfNone(false);

    // Both IfcFeatureElementSubtraction concretes — an IfcOpeningElement-only check rejected legal 4.3 voiding features.
    static readonly FrozenSet<string> Subtraction = new[] { "IfcOpeningElement", "IfcVoidingFeature" }.ToFrozenSet(StringComparer.Ordinal);
    static readonly Op Gate = Op.Of(name: nameof(IfcLegality));

    // The delta's ADDED EDGES validate the relationship law and its ADDED NODES the vocabulary the same applicative
    // way — every violation reported at once (never a first-fail short-circuit), each lifting BimFault.ModelRejected BARE.
    public Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph) =>
        delta.AddedEdges.Map(edge => Rule(edge, graph))
            .Append(delta.AddedNodes.Map(static node => Vocabulary(node)))
            .Fold(Success<Error, Unit>(unit), static (acc, rule) => (acc, rule).Apply(static (_, _) => unit).As());

    // The closed IFC-legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no IfcRel* case,
    // so the rule reads the neutral Compose/Void/Assign shape, never an IFC wire-name): containment-whole-must-be-spatial,
    // a type may not aggregate, the sub-kind-oriented Void/Fill feature-subtraction checks, DefinesByType definition-must-
    // be-type. The Void axis dispatches its SUB-KIND because the ingest is orientation-preserving: a Voids edge lands
    // relating=host/related=feature, a Fills edge relating=OPENING/related=filler — so Fill checks Host, never Feature.
    static Validation<Error, Unit> Rule(Relationship edge, ElementGraph graph) => edge switch {
        Relationship.Compose c when c.SubKind == ComposeKind.Contain =>
            (RequireClass(c.Whole, graph, SpatialWhole, $"containment-whole-not-spatial:{c.Whole.Value}"),
             SpatialRank(c.Whole, c.Part, graph)).Apply(static (_, _) => unit).As(),
        Relationship.Compose c when c.SubKind == ComposeKind.Aggregate =>
            (RequireKind(c.Whole, graph, static kind => kind == ObjectKind.Occurrence, $"type-aggregates-occurrence:{c.Whole.Value}"),
             SpatialRank(c.Whole, c.Part, graph)).Apply(static (_, _) => unit).As(),
        Relationship.Void v when v.SubKind == VoidKind.Void =>
            RequireClass(v.Feature, graph, Subtraction.Contains, $"voids-feature-not-subtraction:{v.Feature.Value}"),
        Relationship.Void v when v.SubKind == VoidKind.Fill =>
            RequireClass(v.Host, graph, Subtraction.Contains, $"fills-host-not-subtraction:{v.Host.Value}"),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition =>
            RequireKind(a.Definition, graph, static kind => kind == ObjectKind.Type, $"definesbytype-definition-not-type:{a.Definition.Value}"),
        _ => Success<Error, Unit>(unit),
    };

    // The two vocabulary arms (Gate 1, composition-time): (1) every Object node classified in the "ifc" system must
    // resolve a generated roster row through the strict Resolve — the ONE Canonical-folding lookup, so a retired
    // *StandardCase code token-checks against its BASE row's set (the retired committed rows carry EMPTY sets; a raw
    // TryGet would unconstrain their tokens — the bypass this arm closes) while abstract supertypes stay legal
    // classification vocabulary (their gating is the egress Instantiable/span check); (2) its predefined token must be
    // a member of the resolved row's ValidPredefined set, or NOTDEFINED/USERDEFINED/empty, or the set is empty (the
    // schema not constraining it). Span/rank gating stays at EGRESS where the emit schema is chosen — no schema exists
    // at composition time.
    static Validation<Error, Unit> Vocabulary(Node node) =>
        node is not Node.Object obj || obj.Classification.System != "ifc"
            ? Success<Error, Unit>(unit)
            : IfcClass.Resolve(obj.Classification.Code, Gate).Match(
                Fail: _ => Fail<Error, Unit>(new BimFault.ModelRejected(Gate, $"vocabulary-class-miss:{obj.Classification.Code}")),
                Succ: row => obj.PredefinedType.Token is "" or "NOTDEFINED" or "USERDEFINED"
                        || row.ValidPredefined.IsEmpty
                        || row.ValidPredefined.Exists(p => p.Token == obj.PredefinedType.Token)
                    ? Success<Error, Unit>(unit)
                    : Fail<Error, Unit>(new BimFault.ModelRejected(Gate, $"vocabulary-token-reject:{row.Key}:{obj.PredefinedType.Token}")));

    // The parent->child spatial rank law (Model/spatial#SPATIAL_STRUCTURE SpatialClass.CanContain): when BOTH
    // endpoints resolve SpatialClass rows the nesting runs downward (equal-rank facility parts legal, no child
    // root); an element endpoint (no spatial row) passes — the whole-side gate above owns it.
    static Validation<Error, Unit> SpatialRank(NodeId whole, NodeId part, ElementGraph graph) =>
        SpatialOf(whole, graph)
            .Bind(w => SpatialOf(part, graph).Map(p => (Whole: w, Part: p)))
            .Match(
                None: () => Success<Error, Unit>(unit),
                Some: pair => pair.Whole.CanContain(pair.Part)
                    ? Success<Error, Unit>(unit)
                    : Fail<Error, Unit>(new BimFault.ModelRejected(Gate, $"containment-rank-inverted:{pair.Whole.Key}>{pair.Part.Key}")));

    static Option<SpatialClass> SpatialOf(NodeId id, ElementGraph graph) =>
        graph.Find<Node.Object>(id).Bind(static o => SpatialClass.TryGet(o.Classification.Code));

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

- [PROJECTION_INVERSION]: the `SemanticProjector : IElementProjection` GeometryGym-internal capture and the IoC inversion (GeometryGym stays SOLE in Bim; Bim implements the seam interface so no GeometryGym edge points down into the seam) ground against `ELEMENT-REBUILD-PLAN.md` §4A/§4C and the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `IElementProjection.Project(ProjectionContext) → Fin<GraphDelta>` contract + `Assemble` fold; the `Project` reads the live `db.Project.Extract<T>()` graph (not the lossy import-rail rows) so the full relationship roster + `OwnerHistory` + `StepHeader` survive, the member spellings (`IfcRoot.GlobalId`/`OwnerHistory`, `IfcProduct`, `IfcTypeObject`, `ParserIfc.IdentifyIfcClass(string, out string)`, `DatabaseIfc.Project`/`Release`/`ModelView`/`Tolerance`/`OriginatingFileInformation`, `FactoryIfc.Construct`(the db-binding entity mint the egress invokes, over the static `BaseClassIfc.Construct`)/`FactoryIfc.OwnerHistoryAdded`, `BaseClassIfc.Extract<T>`) decompile-verified against the live GeometryGym 25.7.30 surface (`.api/api-geometrygym-ifc`); the predefined token reads off the entity's reflected per-class `PredefinedType` enum property (the api notes it is a strongly-typed per-class member, never on the class-name split for a live occurrence), and `IfcRepresentation.Keys` is the ONE polymorphic content-keyer over `IfcObjectDefinition` (`Model/elements#REPRESENTATION_KEYS`), there being no `KeysOf`/`MapKeys` family. The schema-currency rail is decompile-grounded: `ReleaseLower` (this page) reads the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap.Lower` and the egress half's `ReleaseRaise` its identity-name-derived `ReleaseMap.Raise` inverse, each railing `BimFault.CodecReject` on an unmapped member (`IFC4X4_DRAFT` excluded by law; seam `Ifc5` has no GG writer) — the deleted `?? Ifc4X3Add2`/`: GGRelease.IFC4X3_ADD2` fallbacks silently coerced an unknown schema; `ViewLower` switches the decompile-verified 11-member GG `ModelView` enum (`Ifc4Reference`/`Ifc4DesignTransfer`/`IFC4X3Reference`/`IFC4X3AlignmentBasedView`/`Ifc2x3Coordination` + the six `*NotAssigned` members folding to `Coordination` by policy); `OwnerStamp` reads `IfcPersonAndOrganization.Name` (decompile-verified as the `TheOrganization.Name + ThePerson.Name` composition) because a GG entity `ToString()` emits its STEP record line; the classification stamp reads the raw-name `TryGet` over the GENERATED roster, whose committed rows carry the retired `*StandardCase`/`*ElementedCase` subtypes with closed spans — a 2x3 `IfcWallStandardCase` lands its own row (span-gated at egress), never the `BuildingElementProxy` fallback (`IfcClass.Proxy` binds the real deprecated `IfcProxy` entity).
- [VALUE_NARROWING]: the `PropertyLowering` `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing grounds against `Rasm.Element/Properties/property#PROPERTY_VALUE` (the ten-case `Text`/`Measure`/`Boolean`/`Logical`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex` union) and `#PROPERTY_BAG` (the `PropertyBag`/`QuantityBag` the `Node.PropertySet`/`Node.QuantitySet` wrap) plus `Properties/quantity#MEASURE_VALUE` (the `Dimension` `[ComplexValueObject]` discriminator + the SI-native `MeasureValue.OfSi(QuantityType, Dimension, double)` factory [H2]) — the seam forbids an `IfcValue`/dataType string crossing its signature, so the narrowing is Bim's; the decompile-verified members are `IfcPropertySingleValue.NominalValue` (`IfcValue`) + `IfcValue.Value`/`ValueType`/`ValueString` (abstract), `IfcLogical.Logical` (`IfcLogicalEnum` `TRUE`/`FALSE`/`UNKNOWN`), `IfcPropertyEnumeratedValue.EnumerationValues` (`LIST<IfcValue>`) + `EnumerationReference` (`IfcPropertyEnumeration.EnumerationValues`), `IfcPropertyReferenceValue.UsageName`/`PropertyReference` (`IfcObjectReferenceSelect`), `IfcPropertyBoundedValue.UpperBoundValue`/`LowerBoundValue`/`SetPointValue`, `IfcPropertyListValue.ListValues`, `IfcPropertyTableValue.DefiningValues`/`DefinedValues`/`CurveInterpolation` (`IfcCurveInterpolationEnum`), `IfcComplexProperty.HasProperties` (`Dictionary<string, IfcProperty>`, recursed into the seam `Complex` arm keyed by each sub-property `Name`)/`UsageName` (`string`), and the `IfcQuantityLength.LengthValue`/`IfcQuantityArea.AreaValue`/`IfcQuantityVolume.VolumeValue`/`IfcQuantityWeight.WeightValue`/`IfcQuantityCount.CountValue`/`IfcQuantityTime.TimeValue` subtype values (NATIVE-unit magnitudes — the `Semantics/composition#MATERIAL_COMPOSITION` `[UNIT_COERCION]` law: GG never pre-coerces, so each admits through `MeasureValue.OfSi` MULTIPLIED by the `UnitScale` dimensional factor over `IfcUnitAssignment.ScaleSI(IfcUnitEnum)` per base axis + `DatabaseIfc.ScaleAngle()` for the plane-angle rows, never the UnitsNet registry and never a raw already-SI assumption; the per-quantity/per-property `IfcNamedUnit` override and the Celsius affine offset are NAMED bounded drops riding the project factor); the measure hierarchy is decompile-verified as SIBLINGS — `IfcMeasureValue : IfcValue` AND `IfcDerivedMeasureValue : IfcValue`, each typing `Value` as a boxed `double` with `ValueType == typeof(double)` — so the narrowing guard matches BOTH bases (a guard on `IfcMeasureValue` alone dead-coded every derived row: `IfcForceMeasure`/`IfcPressureMeasure`/`IfcMassDensityMeasure`/`IfcThermalTransmittanceMeasure` all derive `IfcDerivedMeasureValue`, so a `Pset_WallCommon` `ThermalTransmittance` silently fell to `Text` under the old guard), and every `MeasureDimensions` row names a decompile-verified GG `IfcValue` type (`IfcThermalResistanceMeasure`/`IfcTemperatureRateOfChangeMeasure` do NOT exist on the surface and are excluded); an IFC measure type the frozen `MeasureDimensions` table does not carry preserves its value as `Text` rather than claiming a wrong dimension, and the `Projection/egress#IFC_EGRESS` egress raises the typed value back through `RaiseProperty`/`RaiseQuantity` — a three-valued `Logical` re-emits a typed `IfcLogical`, an `Enumerated` its `IfcPropertyEnumeratedValue` selected list, a `Reference` an `IfcPropertyReferenceValue` carrying its `UsageName`, a `Table` an `IfcPropertyTableValue` carrying its `CurveInterpolation` — so the three-valued logical, the curve-interpolation rule, and the reference usage name survive the round-trip (the `IfcPropertySingleValue(db, name, IfcValue|bool|double|string)`, `IfcPropertyEnumeratedValue(db, name, IEnumerable<IfcValue>)`, `IfcPropertyReferenceValue(db, name)` + `UsageName`, `IfcPropertyTableValue(db, name)` + `CurveInterpolation`, `IfcLogical(IfcLogicalEnum)`, `IfcLabel(string)`/`IfcReal(double)`, and `IfcQuantity*(db, name, double)` ctors decompile-confirmed).
- [LEGALITY_SPLIT]: the `IfcLegality : IGraphConstraint` IFC-semantic legality (containment-whole-must-be-spatial / the sub-kind-oriented `Void`/`Fill` feature-subtraction checks / type-may-not-aggregate-occurrence / `DefinesByType` definition-must-be-type, PLUS the two `Vocabulary` node arms per the campaign W6 mandate) grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M3 — net-new Rasm interfaces = 2 (`IElementProjection` + `IGraphConstraint`); the seam's structural `GraphDelta` switch enforces only endpoint-resolution and endpoint-kind legality, the IFC legality depended up on through the constraint contract and accumulated applicatively over `Validation<Error,Unit>`, the `BimFault.ModelRejected` lifting BARE (band 2600 IS the `Expected` `Code` per `Model/faults#FAULT_BAND`) so the arm survives the `Error.Combine`. The rule vocabulary is decompile-grounded: `IfcRelVoidsElement.RelatedOpeningElement` is typed `IfcFeatureElementSubtraction`, whose GG concretes are `IfcOpeningElement` AND `IfcVoidingFeature` (the `Subtraction` set — an `IfcOpeningElement`-only check rejected legal 4.3 voiding features); the seam `Void(Host, Feature, VoidKind)` case is orientation-preserving off the ingest rows (`Voids` relating=host, `Fills` relating=OPENING), so `Fill` checks `Host` — the prior blanket `v.Feature` opening check rejected every legal `IfcRelFillsElement` edge; the containment-whole gate consumes `SpatialClass.IsContainer` + `BimZoneKind.IsSpatial` off the owning sibling vocabularies (whose rows carry the 4.3 facility/facility-part leaves assembly-verified) rather than re-listing a private spatial set — the drift form the spatial owner names as deleted; the `Vocabulary` arm reads the generated `Model/elements#IFC_CLASS` `PredefinedRow`-shaped `ValidPredefined` (`p.Token`) through the strict `IfcClass.Resolve` — the `Canonical` fold is LOAD-BEARING at composition time because the retired `*StandardCase` committed rows carry EMPTY token sets, so a raw-name `TryGet` would unconstrain every token on a retired-subtype-classified node where the base row's set rejects it.
