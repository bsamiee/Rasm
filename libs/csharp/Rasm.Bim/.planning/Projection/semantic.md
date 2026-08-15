# [BIM_SEMANTIC_PROJECTOR]

`Rasm.Bim` is the SOLE GeometryGym/IFC owner and the IFC arm of the `Rasm.Element` seam. This page owns the INGRESS half of the one `SemanticProjector : IElementProjection` that lowers a live GeometryGym `DatabaseIfc` into a seam `GraphDelta` (the `Project` fold), the `PropertyLowering` value-narrowing the seam delegates to it, and the `IfcLegality : IGraphConstraint` that decides IFC-semantic RELATIONSHIP legality the seam's structural `GraphDelta` switch cannot. The relationship-lowering half (`IfcRelKind`/`EdgeProjection`) lives at `Projection/relations#RELATION_ALGEBRA` and the IFC re-author half (`Emit`/`Sniff`) at `Projection/egress#IFC_EGRESS` — the SAME `partial class SemanticProjector`, split by concern: `Project` composes `EdgeProjection.All` to land the neutral edges, and `Emit` reverses both the node lowering and the relation roster. The projector replaces the retired `BimModel.Project`/`BimElement` fold: where the old owner produced a second stored element record keyed by GlobalId, the projector produces seam `Node`s (`Object` occurrence/type, `PropertySet`, `QuantitySet`, `Material`) and neutral `Relationship` edges that `Assemble` folds into the canonical `ElementGraph`, so "has it all" is one `Bake` read on the seam graph and GeometryGym never leaks below the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the kernel geometry by content-hash reference, never a RhinoCommon type, never an in-process BRep evaluation.

The element identity is established HERE (the IFC is the source of element identity for an ingested model): `Project` mints a NEUTRAL rooted `NodeId` per `IfcRoot` and records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute. `IfcTypeObject` identity is then admitted through `IIfcTypeReconciler`: a resolver hit reuses the canonical Materials Type Object, and a miss keeps the IFC type imported/ad-hoc with preserved material/profile signatures in a `PropertySource.Import` bag. The owner-mints-its-identity law still holds — Materials mints canonical Component Types, Bim preserves IFC source identity and never forges a catalogue row from a name/profile string. The two `ReleaseVersion`/`ModelView` worlds meet at the projector and nowhere else: the seam `ReleaseVersion`/`ModelView` `[SmartEnum]`s are the model `Header` currency, and the GeometryGym `ReleaseVersion`/`ModelView` enums are the IFC-text codec leg `Project`/`Emit`/`Sniff` own — the page aliases the GeometryGym pair (`GGRelease`/`GGView`) so the unqualified names resolve to the seam, and `ReleaseLower`/`ReleaseRaise` are the one lowering pair the leak never escapes. Both lowerings are RAILED through the frozen `Model/elements#TAXONOMY_EMITTER` `ReleaseMap`, split by direction across the one partial class: `ReleaseLower` (this page) reads `ReleaseMap.Lower` and rails `BimFault.CodecReject` on an unmapped `GGRelease` member, and the egress half's `ReleaseRaise` (`Projection/egress#IFC_EGRESS`) reads `ReleaseMap.Raise` — the identity-name-derived inverse, so the two directions can never drift — railing a seam schema with no GG writer (`Ifc5`); the prior `?? ReleaseVersion.Ifc4X3Add2` / `: GGRelease.IFC4X3_ADD2` silent coercions are the deleted masked-error form (`IFC4X4_DRAFT` excluded by law).

## [01]-[INDEX]

- [02]-[SEMANTIC_PROJECTOR]: `SemanticProjector : IElementProjection` and its `Project` fold lowering a live `DatabaseIfc` into one seam `GraphDelta` — the neutral rooted identity mint [H6], `IIfcTypeReconciler`/`IIfcProfileStore` type admission, the `Object`/`PropertySet`/`QuantitySet`/`Material` node lowering, the `UnitScale` native→SI regime, the `PropertyLowering` value narrowing, and the `FidelityLog` drop ledger both halves thread.
- [03]-[GRAPH_LEGALITY]: `IfcLegality : IGraphConstraint` deciding IFC RELATIONSHIP legality over the delta's added edges — the spatial-containment, aggregation, void/fill, and type-definition rules, resolved against `delta.AddedNodes ∪ graph` and accumulating onto `Validation<Error,Unit>` [M3].

## [02]-[SEMANTIC_PROJECTOR]

- Owner: `SemanticProjector` the `IElementProjection` capturing one live GeometryGym `DatabaseIfc`, the `IIfcProfileStore`, and the `Semantics/properties#PROPERTY_TEMPLATES` `TemplateScope` definition-set policy internally — every one a ctor-held capability, never a per-fold parameter — and lowering the database to a seam `GraphDelta` in `Project`; `PropertyLowering` the Bim-internal value-narrowing the seam delegates to it (the seam forbids an IFC `IfcValue`/dataType crossing its signature, so the `IfcProperty`→`PropertyValue` and `IfcPhysicalSimpleQuantity`→`MeasureValue` narrowing is Bim's), every magnitude coerced native-unit→SI through the one per-projection `UnitScale` because GG never pre-coerces; `OwnerStamp` the `IfcOwnerHistory`→seam `OwnerHistory` projection; `StepHeaderOf` the `STEPFileInformation`→seam `StepHeader` projection; `ReleaseLower` the ingress GeometryGym→seam schema lowering railed through the frozen `ReleaseMap.Lower` (`Fin<T>`, `BimFault.CodecReject` on an unmapped member — no silent coercion; the raise direction is the egress half's `ReleaseRaise` over `ReleaseMap.Raise`) and `ViewLower` the explicit-member MVD lowering.
- Entry: `SemanticProjector.Project(ProjectionContext ctx)` folds the captured `DatabaseIfc` into one `GraphDelta` over `ctx.Key` — it mints a NEUTRAL rooted `NodeId` per `IfcRoot` through the kernel static `Rasm.Element/Graph/element#NODE_MODEL` `NodeId.Rooted()` mint (the `IObjectFactory` floor — `ProjectionContext` exposes only `For`/`Owns`, never a mint pass-through), records the compressed IFC `GlobalId` as the node's 1:1 `ExternalId` projection attribute [H6], reconciles each `IfcTypeObject` through `IIfcTypeReconciler`, preserves imported/ad-hoc type material/profile signatures through `IIfcProfileStore`, and content-keys every non-rooted material node through `MaterialProjection.Project` — threaded the ONE per-projection `UnitScale` so a layer thickness and a profile offset coerce native→SI on the same entry every bag magnitude takes — over its kernel seed-zero `XxHash128` of `Node.ToCanonicalBytes`; the Eurocode regime and the bSDD hosted-version pins are ctor-held composition values beside the `TemplateScope`, so the `StructuralProjection.Attrs` load arm and the egress dictionary URIs both read one elected policy rather than a per-call-site election; `Fin<T>` aborts on a missing `IfcProject` root or a dangling spatial host (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) and on an out-of-map GG release (`BimFault.CodecReject` off the railed `ReleaseLower`), the ingress class lookup PERMISSIVE — an unrostered/IFC4-new leaf takes the `Model/elements#IFC_CLASS` `IfcClass.BuildingElementProxy` ROW for kind/span vocabulary reads through `TryGet(entityType).IfNone(BuildingElementProxy)` while its seam `Classification("ifc", …)` code RETAINS the ORIGINAL entity name (the deferred class gate evaluates the retained identity — a foreign class never silently exports as a proxy, a genuine `IfcBuildingElementProxy` stamps its own rostered key) (the generated roster carries the retired `*StandardCase` subtypes as committed rows, so the raw name resolves; `IfcClass.Proxy` binds the REAL deprecated `IfcProxy` entity under the mechanical render law, never the fallback) so one unknown entity never aborts the import, class validity deferred to the `Emit` egress gate [PREDEFINED_TOKEN_RULING][H8] — the fault lifting BARE (the band IS the `Expected` `Code`, no `.ToError()` hop). The element identity is established HERE (the IFC is the source of element identity), so the projector ignores `ctx.ElementIds` (the aspect-projector NodeId set) and PUBLISHES the minted ids in the delta for sibling projectors to attach `Associate` edges against.
- Auto: `Project` walks the captured `db.Project` once — `ObjectNode` lands every NON-TYPE `IfcObjectDefinition`→`Object.Occurrence` (products AND the `IfcProject` context root the `Model/spatial#SPATIAL_STRUCTURE` tree resolves as its `SpatialClass.IsRoot` node, the `IfcGroup` subtree the `Model/zones#ZONE_GRAPH` overlay reads, and the process/control/actor/resource families the rostered assignment/sequence edges reference — a product-only sweep stranded every such edge on a nodeless endpoint); `AdmitType` lands each `IfcTypeObject` through a `TypeNodeSeed` that either copies the canonical resolver `Node.Object` or preserves the imported IFC type with a `PropertySource.Import` source bag. `ObjectProjection.Rooted` then rebinds each type `GlobalId` to the emitted type id before `Classify`, `Bags`, and `EdgeProjection.All` resolve endpoints. The generic `Classification("ifc", classKey)` (the IFC entity type as a classification, never `IfcClass` on the node) resolves through the permissive `IfcClass.TryGet(entityType)` ingress over the generated roster for IFC-sourced nodes — a rostered class stamps its row key, an unrostered one stamps the verbatim entity name while the `BuildingElementProxy` row supplies only kind/span behavior; `PredefinedType` reads off the entity's per-class predefined property; the keyed `RepresentationContentHash` map (`Model/elements#REPRESENTATION_KEYS` `IfcRepresentation.Keys`, ONE polymorphic content-keyer over `IfcObjectDefinition`) [M2], `OwnerStamp` `OwnerHistory` [H9], and `IfcClass.Span` schema window [H8] stay on the IFC-sourced node path; `Bags` lands EVERY rooted `IfcPropertySetDefinition` — `IfcPropertySet`, the `IfcPreDefinedPropertySet` family (each concrete minting its node so the already-landed `DefinesProperties` `Assign` edge never dangles, its publicly-readable scalars lowered through `PreDefinedRows`), and `IfcElementQuantity` whose `IfcPhysicalComplexQuantity` children flatten value-lossless under dot-path keys beside one prefix-keyed `Properties/property#PROPERTY_BAG` `GroupIdentity` row per complex group — as `PropertySet`/`QuantitySet` bag nodes whose typed values the `PropertyLowering` narrowing fills — every magnitude coerced native-unit→SI through the ONE per-projection `UnitScale` (one `UnitAxis` per base axis, built off `IfcUnitAssignment.ScaleSI` and the `DatabaseIfc.ScaleAngle()` plane-angle read, each pairing that factor with the DECLARED unit TOKEN read off the assignment's own `IfcNamedUnit` — a factor alone cannot name the unit it came from, so no surface back-infers a unit from a float; the dimensional-factor generalization of the composition owner's `LengthScale`; the model `Tolerance` coerces by the same length factor before it grids any content hash) — and whose `Semantics/properties#PROPERTY_TEMPLATES` `PropertyInheritance.ModeOf` `InheritanceMode` is stamped at ingest [H1] under the ctor-held `TemplateScope` (the definition set whose `templatetype` declarations decide the mode — a handover ingest reads the COBie catalogue, an unstated one the buildingSMART standard row) so the seam `Bake` applies type→occurrence precedence wholly within the seam; `Materials` lands `Material` nodes through `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection.Project` under that same `UnitScale` (the layer thicknesses and profile reference-axis offsets crossing it native→SI) and their imported `HasProperties` material Psets through `MaterialProjection.ImportedPsets` as content-minted `PropertySet` bag nodes under `PropertySource.Import` (an `IfcMaterialProperties` is not `IfcRoot` — the bag node id is `NodeId.Content` over its own canonical bytes), each bound by one `Assign.PropertyDefinition` edge the `MaterialEdges` fold lands; `ConnectionProjection.All` (`Semantics/connection#CONNECTION_DETAIL`) lands the realizing-element detail bags and their edges in the same concat; `SourceBag` synthesizes the entity-attribute Import bags Capture attaches — the `IfcDistributionPort` `FlowDirection`/`SystemType` pair the `Model/systems#SYSTEM_TRACE` directed trace reads, the `Model/structural#STRUCTURAL_PROJECTION` definition bags (member/connection/activity/load-group/load-case/result-group/analysis-model, each lowered through `StructuralProjection.Attrs` under the fold head's `UnitScale`, the ctor-held `EurocodePolicy`, and the ctor-held `IIfcProfileStore` the eccentricity constraint geometry content-keys through), and the `Model/spatial#LINEAR_POSITIONING` station rows; the `USERDEFINED` label takes no bag at all — `UserLabel` reads `IfcObject.ObjectType` for an occurrence and `IfcElementType.ElementType` for a type straight onto the seam `Object` node's `ObjectType` column the egress `StampPredefined` re-stamps, so the user-defined type designation round-trips independent of `Name` with no bag row and no attachment edge between the ends; the analytical Axis/FootPrint geometry is content-keyed in `Representations` by `IfcRepresentation.Keys` (never inlined on the node), `Rasm.Compute` resolving it one-hop by content key from the blob store; `GeoReferenceProjector.Project` lands the `Header.Reference` geo frame off the same per-projection `UnitScale` — the MODEL regime the site elevation alone rides [M1]; `EdgeProjection.All` lands every `IfcRel*` neutral edge [NEUTRAL_EDGE_RULING] — the decomposition/connection/assignment/void families, the property/quantity attachment, the structural member↔connection/member↔activity `Generic` edges (the `StructuralProjection.Attrs` 6-DOF restraint + full load family + `LoadKind`/`Case` and the `AtStart` discriminant riding the payload), the space↔surface `Generic` edges, and the material `Associate` edges with the occurrence-usage payload [OCCURRENCE_USAGE_RULING].
- Receipt: the `GraphDelta` is the projector's whole contribution — a merge over the canonical `ElementGraph` that `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` folds with the other projectors' deltas; the rooted/reconciled `NodeId` map keyed by `GlobalId` is the identity table aspect projectors attach against and `Emit` reverses; `SemanticProjector.Fidelity` is the typed round-trip fidelity receipt — every named bounded drop (`FidelityDrop`) both halves incur, COUNTED and entity-anchored on one per-exchange ledger, so a receiving party reads "which drops, how many, on which entities" per exchange, `Review/versioning` stores it beside the commit, and each drop law is a testable observable instead of a prose promise; the ledger is fold-accumulated VALUE state — a drop site RETURNS its fact beside its value in a `Noted<A>` and the parent joins its children's `FidelityLog`s monoidally, so the whole accumulation is the fold's own state and the run lands it ONCE at the fold edge through `Land` — one producer in one run threads and needs no cell, and the per-drop `Atom` swap it replaces re-ran its transition on every CAS retry while forcing every drop site into a mutating pass-through; the seam `Project` signature stays untouched — the receipt rides the projector instance, because the instance IS the exchange and the two halves are its two runs.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new extracted IFC entity family is one `Extract<T>` arm on the `Project` fold landing its seam node; a new IFC value kind is one `PropertyLowering` arm, a new measure type is one `MeasureDimensions` row carrying its seam `Dimension` and its `CoercionAxis`, a coercion that is not a per-axis scale is one more `CoercionAxis` row, a newly declarable unit is one `UnitAxis.UnitTokens` row, and a new physical-quantity entity is one `QuantityTypes` row beside its `Projection/egress#IFC_EGRESS` raiser; a new relationship is one `IfcRelKind` row the `EdgeProjection` reads (`Projection/relations#RELATION_ALGEBRA`); a new schema version is one `ReleaseMap` row the railed `ReleaseLower`/`ReleaseRaise` resolve and the `Model/elements#IFC_CLASS` span validates; never a second element record beside the seam graph and never a per-entity projector type.
- Boundary: the projector is the ONE GeometryGym→seam lowering — the retired `BimModel.Project` produced a second stored `BimElement` keyed by `GlobalId`, and any owner that re-stores the element off the seam graph is the deleted form; `Project` reads the LIVE `db.Project.Extract<T>()` entity graph, never the `Exchange/import#IMPORT_RAIL` decoded rows, because those rows carry mesh geometry alone and projecting them drops the whole relationship roster, the `OwnerHistory`, and the `StepHeader`; GeometryGym is captured INTERNALLY (the `DatabaseIfc` field) and an `IfcProduct`/`IfcRel*`/`DatabaseIfc` type crossing the `IElementProjection.Project` signature is the named seam violation — the seam holds only `Node`/`Relationship`/`GraphDelta`; the rooted `NodeId` is a neutral kernel-minted id and the compressed IFC `GlobalId` is the node's `ExternalId` projection attribute (1:1) [H6] for IFC-sourced nodes, while canonical type hits rebind the IFC `GlobalId` only in `ObjectProjection.Rooted` and reuse the resolver's Materials Type Object identity; the IFC GUID never becomes the node identity and the from-scratch authoring path mints its own neutral id; the value-narrowing is Bim's (`PropertyLowering`) because an `IfcValue`/dataType string crossing a seam signature is the deleted form — the seam carries only the typed `PropertyValue`/`MeasureValue` cases; geometry is referenced by `RepresentationContentHash` only [M2] and an in-process BRep evaluation or a RhinoCommon handle is the named seam violation — the analytical Axis/FootPrint geometry is content-keyed in `Representations` by `IfcRepresentation.Keys` [M2] and NEVER inlined as a coordinate field on the `Object` node (an inline `Vector3`/`BoundaryPolygon`/`Axis` member is the deleted §4-RT-M2 violation), `Rasm.Compute` resolving the analytical axis/footprint one-hop by content key from the blob store; a Bim in-process BRep evaluation is the named seam violation and the seam carries the structural/spatial CONNECTIVITY on the neutral `Relationship.Generic` edges instead; `Emit` is a Bim-INTERNAL method on the projector, NOT an `IElementProjection` member, because IFC egress is one runtime's wire concern and the seam owns only ingress projection.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
global using Rasm.Bim.Projection;
global using Rasm.Bim.Semantics;

using System.Collections.Frozen;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using GeometryGym.Ifc;
using GeometryGym.STEP;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm;
using Rasm.Bim;
using Rasm.Bim.Model;
using Rasm.Bim.Semantics;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;   // the seam schema currency the Header carries (alias wins over the
using ModelView = Rasm.Element.Graph.ModelView;             // GeometryGym.Ifc namespace import), so the unqualified names are seam
using GGRelease = GeometryGym.Ifc.ReleaseVersion;     // the IFC-text codec leg (Project/Emit/Sniff) — the ONLY GeometryGym
using GGView = GeometryGym.Ifc.ModelView;             // currency, never crossing into the Header

namespace Rasm.Bim.Projection;

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

// The port is Bim-DECLARED and app-PROVIDED: the [WIRE] arrow on both architecture maps is a composition-root
// adapter folding IfcTypeSignature onto the Rasm.Materials ComponentResolution/ComponentCatalogue one-hop and
// re-shaping the hit as a ReconciledType — Materials never references Bim and Bim never references Materials
// (the peers align by contract, never by package edge); a resolver miss keeps the imported/ad-hoc path.
public interface IIfcTypeReconciler {
    Fin<Option<ReconciledType>> Resolve(IfcTypeSignature signature, Op key);
}

// The content-addressed STEP-fragment store: the profile pair is the ProfileRef-keyed view the Materials seam reads, and
// the fragment pair the general lane — one Preserve overload per input shape (MODAL_ARITY), the profile overload deriving
// its ContentKey through the same fragment write. The fragment lane carries EVERY preserved IfcConnectionGeometry —
// geometry the inline prohibition keeps off edge attrs [M2] — so the mandatory IfcRelConnectsWithEccentricity
// ConnectionConstraint and the optional element-connection / space-boundary interface surface all content-key at ingest
// and reconstitute at egress instead of degrading to a bare binding.
public interface IIfcProfileStore {
    Option<IfcProfileDef> Find(ProfileRef profile);
    Option<T> Find<T>(UInt128 contentKey) where T : BaseClassIfc;
    ProfileRef Preserve(IfcProfileDef profile, Op key);
    UInt128 Preserve(BaseClassIfc fragment, Op key);
}

public readonly record struct TypeNodeSeed(
    string GlobalId,
    NodeId Id,
    Option<string> ExternalId,
    Classification Classification,
    PredefinedType PredefinedType,
    Option<string> ObjectType,
    string Name,
    string Tag,
    RepresentationContentHash Representations,
    Option<OwnerHistory> History,
    SchemaSpan Span,
    Option<PropertyBag> Source);

// The axis a measure's magnitude scales on — a ROW COLUMN, so the coercion derives from the frozen MeasureDimensions
// row at every site instead of a measure-type-name compare buried in one arithmetic member. Dimensional folds the
// model's per-axis base factors over the row's own seam Dimension exponents (dimensional analysis, so the table needs
// no per-row unit column); Angular reads the declared plane-angle factor, the one coercion no exponent vector can
// carry because radian and steradian are dimensionless in SI. A coercion that is not a pure per-axis scale is one
// more row here.
[SmartEnum]
public sealed partial class CoercionAxis {
    public static readonly CoercionAxis Dimensional = new(OverExponents);
    public static readonly CoercionAxis Angular = new(static (scale, _) => scale.Angle.Factor);

    [UseDelegateFromConstructor]
    public partial double Factor(UnitScale scale, Dimension dimension);

    static double OverExponents(UnitScale scale, Dimension dim) =>
        Math.Pow(scale.L.Factor, dim.Length) * Math.Pow(scale.M.Factor, dim.Mass) * Math.Pow(scale.T.Factor, dim.Time)
        * Math.Pow(scale.I.Factor, dim.Current) * Math.Pow(scale.Th.Factor, dim.Temperature) * Math.Pow(scale.N.Factor, dim.Amount)
        * Math.Pow(scale.J.Factor, dim.LuminousIntensity);
}

// One MeasureDimensions row: the seam Dimension the measure signs and the axis its magnitude coerces on. The pair
// travels together because the angle rows sign Dimensionless while coercing on the declared angle factor, so a bare
// Dimension cannot decide the coercion and a name compare beside it forks the decision across two sites.
public readonly record struct MeasureRow(Dimension Dimension, CoercionAxis Axis) {
    public static MeasureRow Of(Dimension dimension) => new(dimension, CoercionAxis.Dimensional);
    public static MeasureRow Length => Of(Dimension.LengthDim);
    public static MeasureRow Angle => new(Dimension.Dimensionless, CoercionAxis.Angular);
}

// One declared axis: its native->SI factor AND the DECLARATION that produced it. The token travels BESIDE the factor
// because a factor is a LOSSY projection of the declaration — 0.001 spells millimetre, milligram, and millisecond
// alike, and 1.0 spells both a declared metre and an undeclared axis — so a float back-inference can only guess which
// unit the egress DeclareUnits must re-author. ScaleSI answers 1.0 and the IfcUnitEnum indexer answers null for the
// same undeclared axis, so the pair degrades to the identity together and a factor never outlives its declaration.
public readonly record struct UnitAxis(double Factor, string Token) {
    public static readonly UnitAxis Si = new(1.0, "");

    public static UnitAxis Of(IfcUnitAssignment units, IfcUnitEnum axis) => new(units.ScaleSI(axis), Token(units, axis));

    // The declared spelling for one axis, "" when the assignment declares none — read as a SEPARATE member because
    // the plane-angle factor comes off DatabaseIfc while its declaration sits on the same assignment row.
    public static string Token(IfcUnitAssignment units, IfcUnitEnum axis) => units[axis] switch {
        IfcSIUnit si                     => UnitTokens.Find($"{si.Prefix}.{si.Name}").IfNone(""),
        IfcConversionBasedUnit converted => UnitTokens.Find(converted.Name).IfNone(""),
        _                                => "",
    };

    // The declaration -> its seam UnitsNet token, keyed on the DECLARATION and never on the factor: the IfcSIUnit
    // prefix+name pair (IfcSIPrefix.NONE for an unprefixed unit) or the IfcConversionBasedUnit's own lower-case
    // common-unit name. Keys derive from the GG enum members themselves, so a schema rename breaks the build rather
    // than silently missing a row; an unrostered declaration yields "" and lands the SI scheme, never a wrong unit.
    static readonly Map<string, string> UnitTokens = Map(
        ($"{IfcSIPrefix.NONE}.{IfcSIUnitName.METRE}", "Meter"),
        ($"{IfcSIPrefix.CENTI}.{IfcSIUnitName.METRE}", "Centimeter"),
        ($"{IfcSIPrefix.MILLI}.{IfcSIUnitName.METRE}", "Millimeter"),
        ($"{IfcConversionBasedUnit.CommonUnitName.foot}", "Foot"),
        ($"{IfcConversionBasedUnit.CommonUnitName.inch}", "Inch"),
        ($"{IfcConversionBasedUnit.CommonUnitName.US_survey_foot}", "UsSurveyFoot"));
}

// The model's native-unit <-> SI coercion record, built ONCE per projection off the context IfcUnitAssignment — the
// Semantics/composition#MATERIAL_COMPOSITION LengthScale generalized to every base axis, because GeometryGym stores
// EVERY magnitude in the model's declared units, never pre-coerced (a Revit mm export delivers mm lengths; treating
// them as already-SI is the mm-vs-metre import trap the composition owner names). An SI or unitless model is the
// identity record. Coerce and Declare are the two directions of
// ONE transform — ingress native->SI and egress SI->declared, exact inverses by construction under the same
// two-direction law the frozen ReleaseMap holds — and they are the whole coercion, so no call site multiplies a
// bare factor of its own. A measure whose OWN carrier declares a unit (IfcPropertySingleValue.Unit,
// IfcPhysicalSimpleQuantity.Unit — both public IfcUnit reads, both SIFactor()-bearing) overrides the project regime
// at that measure alone, and an IfcConversionBasedUnitWithOffset overrides it AFFINELY through its ConversionOffset,
// so a Celsius-declared temperature coerces by offset-then-factor instead of by factor alone.
public readonly record struct UnitScale(
    UnitAxis L, UnitAxis M, UnitAxis T, UnitAxis I, UnitAxis Th, UnitAxis N, UnitAxis J, UnitAxis Angle) {
    public static readonly UnitScale Si = new(
        UnitAxis.Si, UnitAxis.Si, UnitAxis.Si, UnitAxis.Si, UnitAxis.Si, UnitAxis.Si, UnitAxis.Si, UnitAxis.Si);

    public static UnitScale Of(DatabaseIfc db) =>
        Optional(db.Context?.UnitsInContext).Match(
            None: () => Si,
            Some: units => new UnitScale(
                UnitAxis.Of(units, IfcUnitEnum.LENGTHUNIT), UnitAxis.Of(units, IfcUnitEnum.MASSUNIT),
                UnitAxis.Of(units, IfcUnitEnum.TIMEUNIT), UnitAxis.Of(units, IfcUnitEnum.ELECTRICCURRENTUNIT),
                UnitAxis.Of(units, IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT), UnitAxis.Of(units, IfcUnitEnum.AMOUNTOFSUBSTANCEUNIT),
                UnitAxis.Of(units, IfcUnitEnum.LUMINOUSINTENSITYUNIT),
                // The plane-angle factor is the DatabaseIfc read — the assignment publishes no angle scale — while
                // its declaration is the assignment's own PLANEANGLEUNIT row, so the pair still lands whole.
                new UnitAxis(db.ScaleAngle(), UnitAxis.Token(units, IfcUnitEnum.PLANEANGLEUNIT))));

    // The declared-unit override resolved to its (offset, factor) affine pair: the two IfcUnit select branches both
    // publish SIFactor(), and the offset branch is the ONE non-multiplicative carrier the schema declares.
    static (double Offset, double Factor) Declared(IfcUnit? unit) => unit switch {
        IfcConversionBasedUnitWithOffset affine => (affine.ConversionOffset, affine.SIFactor()),
        IfcNamedUnit named                      => (0.0, named.SIFactor()),
        IfcDerivedUnit derived                  => (0.0, derived.SIFactor()),
        _                                       => (0.0, double.NaN),
    };

    public double Coerce(double native, MeasureRow row, IfcUnit? declared) =>
        Declared(declared) is { Factor: var factor } affine && double.IsFinite(factor)
            ? (native + affine.Offset) * factor
            : native * row.Axis.Factor(this, row.Dimension);

    public double Declare(double si, MeasureRow row, IfcUnit? declared) =>
        Declared(declared) is { Factor: var factor } affine && double.IsFinite(factor)
            ? si / factor - affine.Offset
            : si / row.Axis.Factor(this, row.Dimension);
}

// The named bounded-drop vocabulary — one row per drop law the two projector halves legislate, so every drop the
// pipe incurs is a COUNTED, anchor-bearing observable instead of a prose promise: a receiving party reads "which
// drops, how many, on which entities" per exchange. A new bounded drop is one row plus one Noted.Drop at its site.
[SmartEnum<string>]
public sealed partial class FidelityDrop {
    public static readonly FidelityDrop StringIdentity       = new("string-identity");        // IfcText/IfcIdentifier narrows to Text; re-emits IfcLabel
    public static readonly FidelityDrop MeasureUnmapped      = new("measure-unmapped");       // off-MeasureDimensions measure type preserved as Text
    public static readonly FidelityDrop MeasureFlattened     = new("measure-flattened");      // egress raise fell to the bare IfcReal
    public static readonly FidelityDrop ReferenceResource    = new("reference-resource");     // non-rooted IfcObjectReferenceSelect target not round-tripped
    public static readonly FidelityDrop GroupFactor          = new("group-factor");           // IfcRelAssignsToGroupByFactor.Factor rider not carried
    public static readonly FidelityDrop EccentricityDegraded = new("eccentricity-degraded");  // store-missed ConnectionConstraint re-authors the base binding
    public static readonly FidelityDrop LinearPlacement      = new("linear-placement");       // station rows land, the IfcLinearPlacement entity re-anchors from content-keyed geometry
    public static readonly FidelityDrop AssessmentSkipped    = new("assessment-skipped");     // Rasm-native Assign.Assessment deliberately not IFC-authored
    public static readonly FidelityDrop PredefinedPsetOpaque = new("predefined-pset-opaque"); // internal-field predefined-pset scalars unreadable (bag mints empty)
    public static readonly FidelityDrop StructuralResidue    = new("structural-residue");     // StructuralProjection.Author left the row unconsumed (line/planar/temperature action, trapezoid, displacement)
    public static readonly FidelityDrop GeoLevelLowered      = new("geo-level-lowered");      // anisotropic map frame authored isotropically — a pre-IFC4X3_ADD2 target carries no IfcMapConversionScaled
}

// One fact per drop occurrence: the row names the law, the anchor names the entity (GlobalId, set name, or wire
// name) a federation manager acts on.
public readonly record struct FidelityFact(FidelityDrop Drop, string Anchor);

// The drop ledger as fold-accumulated VALUE state: a MONOID over the fact stream, so a leaf narrowing GROWS its own
// log from Empty and a parent JOINS its children's — the whole accumulation is the fold's own state, landing once at
// the run edge. The deleted form was an Atom<Seq<FidelityFact>> the projector held and every drop site swapped
// through: one CAS per drop, a transition that re-ran on every retry, and a mutating pass-through wedged into every
// expression arm that could drop.
public readonly record struct FidelityLog(Seq<FidelityFact> Facts) {
    public static readonly FidelityLog Empty = new(Seq<FidelityFact>());

    public FidelityLog Note(FidelityDrop drop, string anchor) => new(Facts.Add(new FidelityFact(drop, anchor)));

    public static FidelityLog operator +(FidelityLog left, FidelityLog right) => new(left.Facts.Concat(right.Facts));
}

// A narrowed value beside the log it grew — the writer rail every drop-capable lowering returns on, so a drop is a
// RETURNED fact rather than a side effect. Bind joins the logs monoidally and Join sequences a traversed Seq, so a
// rail step never re-threads by hand and a TraverseM keeps its shape.
public readonly record struct Noted<A>(FidelityLog Log, A Value) {
    public Noted<B> Map<B>(Func<A, B> f) => new(Log, f(Value));

    public Noted<B> Bind<B>(Func<A, Noted<B>> f) => f(Value) switch { var next => new(Log + next.Log, next.Value) };
}

public static class Noted {
    public static Noted<A> Clean<A>(A value) => new(FidelityLog.Empty, value);

    public static Noted<A> Drop<A>(FidelityDrop drop, string anchor, A value) =>
        new(FidelityLog.Empty.Note(drop, anchor), value);

    public static Noted<Seq<A>> Join<A>(Seq<Noted<A>> rows) =>
        new(rows.Fold(FidelityLog.Empty, static (log, row) => log + row.Log), rows.Map(static row => row.Value));
}

// The typed round-trip fidelity receipt — the drop ledger as an artifact: per-drop counts plus the anchored fact
// rows, so "3 group-by-factor memberships lost their factor, 214 IfcText identities re-emit as IfcLabel, 0 geometry
// drops" is per-exchange evidence Review/versioning stores beside the commit and every drop law becomes testable.
public sealed record FidelityReceipt(Map<FidelityDrop, int> Counts, Seq<FidelityFact> Facts) {
    public static FidelityReceipt Of(FidelityLog log) =>
        new(log.Facts.Fold(Map<FidelityDrop, int>(), static (map, fact) => map.AddOrUpdate(fact.Drop, n => n + 1, () => 1)),
            log.Facts);

    public int CountOf(FidelityDrop drop) => Counts.Find(drop).IfNone(0);
    public bool Clean => Facts.IsEmpty;
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
public sealed partial class SemanticProjector(
    DatabaseIfc db, IIfcTypeReconciler typeReconciler, IIfcProfileStore profiles, Option<TemplateScope> scope = default,
    Option<EurocodePolicy> eurocode = default, Option<BsddPins> pins = default) : IElementProjection {
    // Capture-promotion: primary-ctor params scope to the DECLARING part only, so the one store is promoted to a field
    // the egress partial (Projection/egress ReauthorMaterials) reads — never a re-passed Emit parameter.
    readonly IIfcProfileStore profiles = profiles;

    // EurocodePolicy (Model/structural#STRUCTURAL_PROJECTION) fixes the regime the StructuralProjection.Attrs load
    // arm resolves its EN 1990 combination and partial factors under: composition-supplied because only the
    // composition knows the project's national annex and elected design situation, so the ONE value threads from this
    // seat down every Attrs call site — never a per-call-site election and never a per-entity re-derivation. None
    // emits the IFC-declared attributes alone.
    readonly Option<EurocodePolicy> eurocode = eurocode;

    // BsddPins (Semantics/classification#CLASSIFICATION_AXIS) supplies the hosted dictionary stem and shape BOTH
    // classification legs read — the ingest Classify resolving each IfcClassificationReference through Ingest and the
    // egress ReauthorClassifications deriving every authored dictionary URI: composition-supplied like the
    // TemplateScope, so a registry that re-publishes a dictionary version is one value passed here rather than a
    // durable-roster edit, and the two legs can never name different hosted versions.
    readonly BsddPins pins = pins.IfNone(BsddPins.Default);

    // TemplateScope (Semantics/properties#PROPERTY_TEMPLATES) carries the definition set the ingest bag classifier
    // resolves inheritance under: one optional context holding its own loaders, its canonical row read once here, so a
    // COBie-handover ingest stamps its bags off the handover catalogue while an unstated exchange takes the
    // buildingSMART standard set — never a per-call-site scope argument and never a bool cobie knob.
    readonly TemplateScope templates = scope.IfNone(TemplateScope.Standard);

    // The per-exchange ledger: each RUN — Project at ingest, Emit at egress — THREADS its own FidelityLog through
    // its own fold and lands it here ONCE at the run edge, so a drop is counted once per run and the fold stays
    // pure and replayable. One producer in one run needs no cell; the deleted Atom took a CAS per drop and re-ran
    // its transition on every retry. The seam Project signature stays untouched, the receipt riding the instance
    // because the projector instance IS the exchange.
    FidelityLog fidelity = FidelityLog.Empty;

    public FidelityReceipt Fidelity => FidelityReceipt.Of(fidelity);

    // The ONE run-edge write — the platform-forced seam where a threaded value re-enters instance state, so it is
    // the only statement body on this owner and every drop site upstream stays a fold step.
    T Land<T>(FidelityLog log, T value) {
        fidelity += log;
        return value;
    }

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
        double tolerance = scale.Coerce(db.Tolerance, MeasureRow.Length, null);
        return
            from geo in GeoReferenceProjector.Project(project, scale, key)
            from schema in ReleaseLower(db.Release, key)
            let header = new Header(schema, ViewLower(db.ModelView), geo, tolerance, ctx.At, StepHeaderOf(db), UnitsOf(scale))
            from objects in Objects(project, rooted, typeReconciler, profiles, tolerance, scale, eurocode, key)
            from details in ConnectionProjection.All(project, objects.Rooted, tolerance, scale, key)
            from bags in Bags(project, objects.Rooted, scale, templates, key)
            from materials in Materials(project, objects.Rooted, tolerance, scale, templates, profiles, key)
            let nodes = Classify(project, objects.Rooted, objects.Nodes
                .Concat(bags.Value)
                .Concat(materials.Value)
                .Concat(details.Map(static detail => detail.Bag)))
            from edges in EdgeProjection.All(project, objects.Rooted, tolerance, scale, eurocode, templates, profiles, key)
            let seeded = nodes.Fold(GraphDelta.Empty.Reheader(header), static (delta, node) => delta.Put(node))
            // The three drop-capable folds joined monoidally and landed ONCE — the run's whole ledger, never a
            // per-drop commit, so a re-run of this fold re-counts nothing that a partial run already banked.
            select Land(bags.Log + materials.Log + edges.Log,
                (edges.Value + objects.Edges + details.Map(static detail => detail.Edge))
                    .Fold(seeded, static (delta, edge) => delta.Link(edge)));
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
    // roster (IfcClass.Proxy binds the REAL schema-retired IfcProxy entity under the mechanical render law, never the fallback) so one
    // unknown entity never aborts the whole import — class validity is the Emit egress gate (AdmitPredefined), never here.
    // The analytical Axis/FootPrint geometry is content-keyed in Representations by IfcRepresentation.Keys (the ONE polymorphic
    // representation content-keyer maps every RepresentationIdentifier — Axis/Body/Box/FootPrint — to its content hash [M2]),
    // NEVER inlined as a coordinate field on the seam Object node (no Vector3/AxisCurve member exists — the deleted §4-RT-M2
    // violation); Rasm.Compute RESOLVES the analytical axis/footprint one-hop BY CONTENT KEY from the blob store.
    static Fin<ObjectProjection> Objects(IfcProject project, Map<string, NodeId> rooted, IIfcTypeReconciler reconciler, IIfcProfileStore profiles, double tolerance, UnitScale scale, Option<EurocodePolicy> eurocode, Op key) {
        Map<string, IfcMaterialSelect> materials = MaterialIndex(project);
        // The occurrence sweep rails through SourceBag because the structural-definition and linear-positioning
        // syntheses compose the Fin-railed StructuralProjection.Attrs(entity, scale, eurocode, profiles, key) /
        // PositioningProjection.Attrs(entity, scale, key) — a malformed structural or station measure faults typed
        // here, never a swallowed IfFail. Three capabilities thread from the fold head: `scale` the one per-projection
        // UnitScale, `eurocode` the ctor-held composition policy, and `profiles` the ONE content-addressed fragment
        // store the eccentricity ConnectionConstraint geometry content-keys through [M2] — so no arm rebuilds any of
        // them off its own entity and no second store opens beside the ctor-held one.
        return project.Extract<IfcObjectDefinition>().AsIterable()
            .Filter(static definition => definition is not IfcTypeObject)
            .ToSeq()
            .TraverseM(definition => SourceBag(definition, scale, eurocode, profiles, key).Map(source => (Definition: definition, Source: source)))
            .As()
            .Map(occurrences => occurrences.Fold(ObjectProjection.Empty(rooted), (projection, row) =>
                projection.Capture(row.Definition.GlobalId, ObjectNode(row.Definition, ObjectKind.Occurrence, rooted), row.Source, tolerance)))
            .Bind(occurrences => project.Extract<IfcTypeObject>().AsIterable().ToSeq()
                .TraverseM(type => AdmitType(type, materials, rooted, reconciler, profiles, key)).As()
                .Map(types => types.Fold(occurrences, (projection, seed) =>
                    projection.Capture(seed.GlobalId, TypeNode(seed), seed.Source, tolerance))));
    }

    // The relating-material index built ONCE per projection: object GlobalId -> the IfcMaterialSelect its
    // IfcRelAssociatesMaterial binds (the typed select, never a BaseClassIfc upcast) — the per-type FirstOrDefault
    // scan over every material relation was O(types x relations), the deleted quadratic form.
    static Map<string, IfcMaterialSelect> MaterialIndex(IfcProject project) =>
        project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Fold(Map<string, IfcMaterialSelect>(), static (map, rel) =>
                Optional(rel.RelatingMaterial).Match(
                    Some: material => toSeq(rel.RelatedObjects.OfType<IfcRoot>()).Fold(map, (acc, root) => acc.AddOrUpdate(root.GlobalId, material)),
                    None: () => map));

    // The standard-system classification set [4-RT cardinality]: IFC permits MULTIPLE IfcRelAssociatesClassification per
    // object (Uniclass + OmniClass co-applied), so each relation's IfcClassificationReference resolves through
    // Semantics/classification#CLASSIFICATION_AXIS ClassificationSystem.Ingest (lowering the IfcClassificationReference.Name
    // concept title onto seam Classification.Title) and accumulates onto every related rooted Object node's Classifications
    // set — the ("ifc", classKey) entity-class pair stays the node's PRIMARY Classification, the standard refs ride the set;
    // an unrostered source resolves None and is dropped here (it rides the relation-edge Generic passthrough), never a wrong
    // lowering. RelatingClassification (IfcClassificationSelect) + RelatedObjects (SET<IfcDefinitionSelect>) decompile-verified.
    // Instance member like ReauthorClassifications: Ingest reads the composition-supplied BsddPins for the dictionary
    // stem and shape, so BOTH classification legs resolve one ctor-held policy value and a per-call-site pin is the
    // deleted form that would let an ingest and its own egress disagree about which hosted version they name.
    Seq<Node> Classify(IfcProject project, Map<string, NodeId> rooted, Seq<Node> nodes) {
        Map<NodeId, Seq<Classification>> byNode = project.Extract<IfcRelAssociatesClassification>().AsIterable()
            .Fold(Map<NodeId, Seq<Classification>>(), (map, rel) =>
                Optional(rel.RelatingClassification as IfcClassificationReference)
                    .Bind(reference => ClassificationSystem.Ingest(reference, pins))
                    .Match(
                        Some: c => rel.RelatedObjects.OfType<IfcRoot>().Aggregate(map, (acc, related) =>
                            rooted.Find(related.GlobalId).Match(
                                Some: id => acc.AddOrUpdate(id, existing => existing.Add(c), () => Seq(c)),
                                None: () => acc)),
                        None: () => map));
        // A class-root [Union] Node case has NO compiler-generated `with` (the same law the Mint re-stamps honor), so the
        // classification stamp RECONSTRUCTS the Object through its public positional ctor — the whole-member copy with the
        // Classifications slot filled, the exact idiom the seam Relabel/Remap fences use.
        return byNode.IsEmpty
            ? nodes
            : nodes.Map(node => node is Node.Object o && byNode.Find(o.Id).Case is Seq<Classification> refs
                ? (Node)new Node.Object(o.Id, o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.ObjectType, o.Name, o.Tag, o.Representations, o.History, o.Span, refs)
                : node);
    }

    // The generated roster carries the retired *StandardCase/*ElementedCase subtypes as COMMITTED rows (their closed
    // SchemaSpan is the SOURCE entity's own window, stamped here on the node; the egress Resolve reads the folded
    // BASE row, so a 2x3 StandardCase re-emits as its surviving IFC4 class), so the raw-name TryGet resolves a 2x3
    // IfcWallStandardCase to its own row —
    // BuildingElementProxy supplies only the ROW behavior (kind/span vocabulary reads) for a genuinely unrostered
    // leaf, while the classification code RETAINS the ORIGINAL entity name, so the deferred egress class gate
    // evaluates the identity the file carried and a foreign class can never silently export as a proxy — a genuine
    // IfcBuildingElementProxy stamps its own rostered key and stays distinguishable from a permissive import.
    static Node.Object ObjectNode(IfcObjectDefinition definition, ObjectKind kind, Map<string, NodeId> rooted) {
        string entity = ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _);
        Option<IfcClass> row = IfcClass.TryGet(entity);
        IfcClass cls = row.IfNone(IfcClass.BuildingElementProxy);
        return new Node.Object(
            Id:             rooted[definition.GlobalId],
            Kind:           kind,
            ExternalId:     Some(definition.GlobalId),
            Classification: Classification.Create("ifc", row.Map(static r => r.Key).IfNone(entity), "", None, None, None),
            PredefinedType: Predefined(definition),
            ObjectType:     UserLabel(definition),
            Name:           definition.Name ?? "",
            Tag:            (definition as IfcElement)?.Tag ?? "",
            Representations: IfcRepresentation.Keys(definition),
            History:        OwnerStamp(definition.OwnerHistory),
            Span:           cls.Span);
    }

    // The USERDEFINED user-defined type designation, read off whichever slot the entity family owns — IfcObject.ObjectType
    // for an occurrence, IfcElementType.ElementType for a type — onto the ONE seam Graph/element#NODE_MODEL Object column
    // the egress StampPredefined re-stamps through the SAME two-arm dispatch. An empty or absent slot is None, so absence
    // is the seam's own presence-delimited canonical byte and never an empty-string sentinel; a Name substitution at
    // egress collapsed two same-named entities carrying distinct labels onto one, the defect this column closes at both
    // ends [PREDEFINED_TOKEN_RULING].
    static Option<string> UserLabel(IfcObjectDefinition definition) => definition switch {
        IfcObject { ObjectType.Length: > 0 } occurrence  => Some(occurrence.ObjectType),
        IfcElementType { ElementType.Length: > 0 } type  => Some(type.ElementType),
        _                                                => None,
    };

    static Node.Object TypeNode(TypeNodeSeed seed) =>
        new(
            Id:              seed.Id,
            Kind:            ObjectKind.Type,
            ExternalId:      seed.ExternalId,
            Classification:  seed.Classification,
            PredefinedType:  seed.PredefinedType,
            ObjectType:      seed.ObjectType,
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
            ObjectType:      type.Type.ObjectType,
            Name:            type.Type.Name,
            Tag:             type.Type.Tag,
            Representations: type.Type.Representations,
            History:         type.Type.History,
            Span:            type.Type.Span,
            Source:          Option<PropertyBag>.None);

    static TypeNodeSeed ImportedTypeSeed(IfcTypeObject definition, IfcTypeSignature signature, Map<string, NodeId> rooted) {
        string entity = ParserIfc.IdentifyIfcClass(definition.GetType().Name, out _);
        Option<IfcClass> row = IfcClass.TryGet(entity);
        IfcClass cls = row.IfNone(IfcClass.BuildingElementProxy);   // row behavior only — the classKey below retains the original entity name on a roster miss
        return new(
            GlobalId:        definition.GlobalId,
            Id:              rooted[definition.GlobalId],
            ExternalId:      Some(definition.GlobalId),
            Classification:  Classification.Create("ifc", row.Map(static r => r.Key).IfNone(entity), "", None, None, None),
            PredefinedType:  Predefined(definition),
            ObjectType:      UserLabel(definition),
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
        IfcMaterialProfileSet profileSet => Optional(profileSet.CompositeProfile ?? profileSet.MaterialProfiles.FirstOrDefault()?.Profile),
        IfcMaterialProfileSetUsage usage => Optional(usage.ForProfileSet?.CompositeProfile ?? usage.ForProfileSet?.MaterialProfiles.FirstOrDefault()?.Profile),
        _ => Option<IfcProfileDef>.None
    };

    // The projector-minted signature bag's well-known set name — the egress AuthorBag skips it by THIS symbol (the
    // NestOrdinal precedent), so reconciliation bookkeeping never exports as a phantom IfcPropertySet the source
    // file never carried.
    internal static readonly string TypeSignatureSet = "IfcTypeSignature";

    // The four synthesized entity-attribute bag set symbols — egress skips all beside TypeSignatureSet (the port,
    // structural, and project-context attributes re-author on the entity at Emit; the positioning rows are
    // ingest-landed station evidence whose IfcLinearPlacement re-author is the named bounded drop the fidelity
    // receipt counts — never a phantom Pset the source file never carried).
    internal static readonly string PortAttributeSet = "IfcDistributionPort";
    internal static readonly string StructuralDefinitionSet = "IfcStructuralDefinition";
    internal static readonly string PositioningAttributeSet = "IfcLinearPositioning";
    internal static readonly string ProjectAttributeSet = "IfcProjectContext";

    // The Rasm-authored bag ROW names this ingest stamps and a peer surface reads back by name: the port flow
    // pair composes the Element-declared PortRows statics (the BoundaryRows custody — Element declares, this
    // ingest stamps, the Model/systems trace reads; the prior two-site PropertyCategory.Seam.Row mints were the
    // fork-on-first-rename that custody deletes), while the context root's Phase/LongName header labels the
    // egress restamps stay this page's own Seam.Row mints — one producer, one reader, both HERE.
    internal static readonly PropertyName Phase = PropertyCategory.Seam.Row("Phase");
    internal static readonly PropertyName LongName = PropertyCategory.Seam.Row("LongName");

    // The type-signature bookkeeping bag rows: this ingest authors them and the Exchange/import reconcile reads
    // them back by name, so each is ONE PropertyName static under the same Seam.Row mint — a literal at either
    // end is the key-space fork the branch row-name custody ruling deletes.
    public static class SignatureRows {
        public static readonly PropertyName GlobalId = PropertyCategory.Seam.Row("GlobalId");
        public static readonly PropertyName IfcEntity = PropertyCategory.Seam.Row("IfcEntity");
        public static readonly PropertyName PredefinedType = PropertyCategory.Seam.Row("PredefinedType");
        public static readonly PropertyName Name = PropertyCategory.Seam.Row("Name");
        public static readonly PropertyName MaterialName = PropertyCategory.Seam.Row("MaterialName");
        public static readonly PropertyName MaterialCategory = PropertyCategory.Seam.Row("MaterialCategory");
        public static readonly PropertyName MaterialStandard = PropertyCategory.Seam.Row("MaterialStandard");
        public static readonly PropertyName MaterialGrade = PropertyCategory.Seam.Row("MaterialGrade");
        public static readonly PropertyName ProfileStandard = PropertyCategory.Seam.Row("ProfileStandard");
        public static readonly PropertyName ProfileDesignation = PropertyCategory.Seam.Row("ProfileDesignation");
        public static readonly PropertyName ProfileEntity = PropertyCategory.Seam.Row("ProfileEntity");
        public static readonly PropertyName ProfileStepKey = PropertyCategory.Seam.Row("ProfileStepKey");
    }

    // Entity-borne facts with no IfcPropertySet carrier land as synthesized Import bags through the SAME Capture
    // path the type-signature bag rides: the port flow attributes (the Model/systems#SYSTEM_TRACE directed-trace
    // inputs — an unsurfaced FlowDirection reads NOTDEFINED and degrades every trace to undirected reachability),
    // the structural definition bags (the Model/structural#STRUCTURAL_PROJECTION entity-level Attrs arms:
    // member/connection/activity/load-group/load-case/result-group/analysis-model — the Fin-railed
    // Attrs(entity, scale, eurocode, profiles, key) read under the fold head's OWN capabilities, so a malformed
    // structural measure faults typed instead of a swallowed IfFail), and the
    // linear-positioning station evidence, and the project context root's Phase/LongName header labels. The
    // USERDEFINED label is NOT here: it rides the seam Object node's own ObjectType column through UserLabel, so no
    // bag row, no PropertyDefinition edge, and no egress label index stand between the two ends of that round-trip.
    // An entity matching no arm, or an empty structural read, yields None — no empty-bag node.
    static Fin<Option<PropertyBag>> SourceBag(IfcObjectDefinition definition, UnitScale scale, Option<EurocodePolicy> eurocode, IIfcProfileStore profiles, Op key) =>
        definition switch {
            IfcDistributionPort port => Fin.Succ(Some(new PropertyBag(
                PortAttributeSet,
                Map(
                    (PortRows.FlowDirection, (PropertyValue)new PropertyValue.Text(port.FlowDirection.ToString())),
                    (PortRows.SystemType, new PropertyValue.Text(port.SystemType.ToString()))),
                InheritanceMode.OccurrenceWins,
                PropertySource.Import))),
            IfcStructuralItem or IfcStructuralActivity or IfcStructuralLoadGroup or IfcStructuralResultGroup or IfcStructuralAnalysisModel =>
                StructuralProjection.Attrs(definition, scale, eurocode, profiles, key).Map(attrs => attrs.IsEmpty
                    ? Option<PropertyBag>.None
                    : Some(new PropertyBag(StructuralDefinitionSet, attrs, InheritanceMode.OccurrenceWins, PropertySource.Import))),
            // The linear-positioning families — alignment segments, referents, and any linearly-placed product —
            // land their station evidence through the Model/spatial#LINEAR_POSITIONING deep reader, the same
            // Attrs idiom the structural arm rides; a non-positioning product yields the empty map, no bag.
            IfcAlignmentSegment or IfcReferent or IfcProduct { ObjectPlacement: IfcLinearPlacement } =>
                PositioningProjection.Attrs(definition, scale, key).Map(attrs => attrs.IsEmpty
                    ? Option<PropertyBag>.None
                    : Some(new PropertyBag(PositioningAttributeSet, attrs, InheritanceMode.OccurrenceWins, PropertySource.Import))),
            // The context root's own header attributes — the free-text Phase lifecycle label (the
            // Planning/schedule#SCHEDULE StageLabels admission interprets it; this lane carries it verbatim) and the
            // LongName display title — so the egress restamps both on the re-authored IfcProject; both blank yields
            // no bag node, and the empty-string GG default never mints a phantom row.
            IfcContext context => Fin.Succ(
                Seq((Row: Phase, Value: context.Phase), (Row: LongName, Value: context.LongName))
                    .Filter(static row => !string.IsNullOrWhiteSpace(row.Value))
                    .Fold(Map<PropertyName, PropertyValue>(), static (bag, row) =>
                        bag.Add(row.Row, new PropertyValue.Text(row.Value)))
                    is { IsEmpty: false } rows
                    ? Some(new PropertyBag(ProjectAttributeSet, rows, InheritanceMode.OccurrenceWins, PropertySource.Import))
                    : Option<PropertyBag>.None),
            _ => Fin.Succ(Option<PropertyBag>.None),
        };

    // An absent signature axis is an ABSENT KEY — the bag carries only present facts, so a consumer probes membership
    // rather than unwrapping a Present/Value ceremony (the deleted Complex-encoded Option wrapper).
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
            PropertySource.Import);

    // The predefined token is a strongly-typed per-class enum member (IfcWall.PredefinedType is IfcWallTypeEnum, etc.),
    // so a live occurrence carries it on a reflected PredefinedType property, NOT on the class-name split — the seam owns
    // the PredefinedType value-object and admits the token bare (validity is the Emit egress gate [PREDEFINED_TOKEN_RULING]), an empty/NOTDEFINED
    // token folding to the IFC default. Its USERDEFINED partner label is the UserLabel read onto the node's own
    // ObjectType column — the two halves of the IFC-canonical (PredefinedType, ObjectType) pair land in one node write.
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
    // seam InheritanceMode under the projector's own TemplateScope — the definition set whose templatetype declarations
    // decide the mode, so a handover-scoped ingest reads the COBie catalogue's declaration rather than the standard
    // set's silence. EVERY rooted IfcPropertySetDefinition mints a node: the IfcPreDefinedPropertySet family (the
    // 2x3/4.0 door/window lining+panel records, retired at 4.3) is IfcRoot, so the DefinesProperties fold already lands its
    // Assign edge — a node-less predefined pset was a DANGLING endpoint that faulted the seam structural Link law on any
    // door/window model, not a tolerable drop — and its publicly-readable scalars lower through PreDefinedRows; an
    // IfcPhysicalComplexQuantity flattens its HasQuantities children under the dot-path {Complex.Name}.{child} key
    // while its Discrimination/Quality/Usage identity lands as one seam Properties/property#PROPERTY_BAG GroupIdentity
    // row keyed on that prefix, so the whole complex quantity — values AND grouping — round-trips.
    static Fin<Noted<Seq<Node>>> Bags(IfcProject project, Map<string, NodeId> rooted, UnitScale scale, TemplateScope templates, Op key) =>
        from properties in project.Extract<IfcPropertySet>().AsIterable().ToSeq().TraverseM(ps =>
            ps.HasProperties.Values.AsIterable().ToSeq()
                .TraverseM(property => PropertyLowering.Lower(property, rooted, scale, key)
                    .Map(lowered => (Name: PropertyName.Create(property.Name ?? ""), Lowered: lowered)))
                .As()
                .Map(rows => new Noted<Node>(
                    rows.Fold(FidelityLog.Empty, static (log, row) => log + row.Lowered.Log),
                    new Node.PropertySet(rooted[ps.GlobalId], new PropertyBag(
                        ps.Name ?? "",
                        rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Lowered.Value)),
                        PropertyInheritance.ModeOf(ps.Name ?? "", IsTypeBound(ps), templates), PropertySource.Import)))).As()
        from predefined in project.Extract<IfcPreDefinedPropertySet>().AsIterable().ToSeq().TraverseM(set =>
            PreDefinedRows(set, scale).Map(rows => new Noted<Node>(rows.Log,
                new Node.PropertySet(rooted[set.GlobalId], new PropertyBag(
                    set.Name ?? "", rows.Value.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)),
                    PropertyInheritance.ModeOf(set.Name ?? "", IsTypeBound(set), templates), PropertySource.Import)))).As()
        from quantities in project.Extract<IfcElementQuantity>().AsIterable().ToSeq().TraverseM(eq =>
            FlattenQuantities(eq.Quantities.Values, "", scale, (Map<PropertyName, MeasureValue>(), Map<string, GroupIdentity>()), key)
                .Map(flat => Noted.Clean<Node>(new Node.QuantitySet(rooted[eq.GlobalId], new QuantityBag(
                    eq.Name ?? "", flat.Values, PropertyInheritance.ModeOf(eq.Name ?? "", IsTypeBound(eq), templates), PropertySource.Import, flat.Groups))))).As()
        // Three traversals, three log fragments — Join sequences each Seq<Noted<Node>> into one Noted<Seq<Node>>, so
        // the caller receives one ledger for the whole bag fold rather than a per-set stream to re-collect.
        select Noted.Join(properties.Concat(predefined).Concat(quantities));

    // The complex-quantity flatten threads BOTH halves on one rail: a simple child lands at its dot-path key, a nested
    // IfcPhysicalComplexQuantity stamps its grouping identity under the prefix it extends and recurses — so the values are
    // lossless, the nesting is recoverable from the prefix tree, and the Discrimination/Quality/Usage identity survives as
    // seam data instead of a key spelling. The prior OfType<IfcPhysicalSimpleQuantity> sweep silently DROPPED every complex
    // child, and the prefix-only successor dropped the identity strings the egress rebuild now restores.
    static Fin<(Map<PropertyName, MeasureValue> Values, Map<string, GroupIdentity> Groups)> FlattenQuantities(
        IEnumerable<IfcPhysicalQuantity> quantities, string prefix, UnitScale scale,
        (Map<PropertyName, MeasureValue> Values, Map<string, GroupIdentity> Groups) bag, Op key) =>
        quantities.Aggregate(Fin.Succ(bag), (rail, quantity) => rail.Bind(acc => quantity switch {
            IfcPhysicalSimpleQuantity simple => PropertyLowering.Measure(simple, scale, key)
                .Map(value => acc with { Values = acc.Values.AddOrUpdate(PropertyName.Create($"{prefix}{simple.Name ?? ""}"), value) }),
            IfcPhysicalComplexQuantity complex => FlattenQuantities(
                complex.HasQuantities, $"{prefix}{complex.Name ?? ""}.", scale,
                acc with { Groups = acc.Groups.AddOrUpdate($"{prefix}{complex.Name ?? ""}", GroupOf(complex)) }, key),
            _ => Fin.Fail<(Map<PropertyName, MeasureValue>, Map<string, GroupIdentity>)>(new BimFault.CodecReject(key, $"quantity-kind-unmapped:{quantity.GetType().Name}")),
        }));

    // The complex quantity's grouping identity -> the seam GroupIdentity: Discrimination is schema-mandatory and
    // Quality/Usage optional, but GeometryGym backs all three with an EMPTY-STRING default rather than a null, so the
    // unset state reads as blank and lifts to None — carrying "" through would re-author a qualifier the source file
    // never wrote [foreign provider sentinels retire to Option at the projector read].
    static GroupIdentity GroupOf(IfcPhysicalComplexQuantity complex) =>
        new(Stated(complex.Discrimination), Stated(complex.Quality), Stated(complex.Usage));

    static Option<string> Stated(string value) => string.IsNullOrEmpty(value) ? None : Some(value);

    // The predefined-pset typed rows, PUBLIC-surface-true per subtype: the door panel, window panel, permeable covering,
    // and window lining expose their scalars publicly (decompile-verified) — lengths coerce native->SI through the
    // UnitScale length factor, the lining offsets are IfcNormalisedRatioMeasure dimensionless, the operation/position
    // enums land as Text tokens; IfcDoorLiningProperties keeps its scalars on INTERNAL fields with no public getter (the
    // mNominalDiameter package-watch precedent — its node still mints so the Assign edge resolves, the bag empty) and
    // IfcReinforcementDefinitionProperties' section definitions are cross-section geometry the inline prohibition keeps
    // off the bag [M2]. The NaN default of every unset GG scalar drops at the finite filter, blank tokens at the length filter.
    static Fin<Noted<Seq<(PropertyName Name, PropertyValue Value)>>> PreDefinedRows(IfcPreDefinedPropertySet set, UnitScale scale) {
        Noted<Seq<(string Key, Fin<PropertyValue> Value)>> rows = set switch {
            IfcDoorPanelProperties p => Noted.Clean(Seq(
                Length("PanelDepth", p.PanelDepth, scale), Ratio("PanelWidth", p.PanelWidth),
                Token("PanelOperation", p.OperationType.ToString()), Token("PanelPosition", p.PanelPosition.ToString()))),
            IfcWindowPanelProperties p => Noted.Clean(Seq(
                Length("FrameDepth", p.FrameDepth, scale), Length("FrameThickness", p.FrameThickness, scale),
                Token("OperationType", p.OperationType.ToString()), Token("PanelPosition", p.PanelPosition.ToString()))),
            IfcPermeableCoveringProperties p => Noted.Clean(Seq(
                Length("FrameDepth", p.FrameDepth, scale), Length("FrameThickness", p.FrameThickness, scale),
                Token("OperationType", p.OperationType.ToString()), Token("PanelPosition", p.PanelPosition.ToString()))),
            IfcWindowLiningProperties p => Noted.Clean(Seq(
                Length("LiningDepth", p.LiningDepth, scale), Length("LiningThickness", p.LiningThickness, scale),
                Length("TransomThickness", p.TransomThickness, scale), Length("MullionThickness", p.MullionThickness, scale),
                Ratio("FirstTransomOffset", p.FirstTransomOffset), Ratio("SecondTransomOffset", p.SecondTransomOffset),
                Ratio("FirstMullionOffset", p.FirstMullionOffset), Ratio("SecondMullionOffset", p.SecondMullionOffset))),
            // The internal-field concretes (IfcDoorLiningProperties, the reinforcement section definitions) mint an
            // empty bag — the COUNTED opaque drop, RETURNED beside the empty row set so the arm stays an expression
            // and the fact rides the same value the caller already folds.
            _ => Noted.Drop(FidelityDrop.PredefinedPsetOpaque, set.Name ?? set.GetType().Name, Seq<(string, Fin<PropertyValue>)>()),
        };
        return rows.Value.TraverseM(row => row.Value.Map(value => (row.Key, value))).As()
            .Map(values => new Noted<Seq<(PropertyName, PropertyValue)>>(rows.Log, values.Filter(static row => row.value switch {
                PropertyValue.Text text => text.Value.Length > 0,
                _ => true,
            }).Map(static row => (PropertyCategory.Seam.Row(row.Key), row.value)).ToSeq()));
    }

    static (string, Fin<PropertyValue>) Length(string name, double native, UnitScale scale) =>
        (name, double.IsFinite(native)
            ? MeasureValue.OfSi(Dimension.LengthDim, scale.Coerce(native, MeasureRow.Length, null))
                .Map(static value => (PropertyValue)new PropertyValue.Measure(value))
            : Fin.Succ<PropertyValue>(new PropertyValue.Text("")));

    static (string, Fin<PropertyValue>) Ratio(string name, double value) =>
        (name, double.IsFinite(value)
            ? MeasureValue.OfSi(Dimension.Dimensionless, value).Map(static measure => (PropertyValue)new PropertyValue.Measure(measure))
            : Fin.Succ<PropertyValue>(new PropertyValue.Text("")));

    static (string, Fin<PropertyValue>) Token(string name, string value) => (name, Fin.Succ<PropertyValue>(new PropertyValue.Text(value)));

    // The IFC type-driven signal: a property-set definition whose DefinesType inverse is non-empty is bound to a type
    // object (so its occurrence merge is type-driven), read off the GeometryGym SET<IfcTypeObject> inverse, never the
    // unrelated IsDefinedBy (the IfcRelDefinesByTemplate set).
    static bool IsTypeBound(IfcPropertySetDefinition set) => set.DefinesType.Any();

    // Non-rooted material nodes are content-keyed (kernel seed-zero XxHash128 over ToCanonicalBytes [H7]) by
    // MaterialProjection.Project, never GlobalId-rooted; the composition fold (Single/LayerSet/ProfileSet/ConstituentSet)
    // is the IFC material algebra Semantics/composition owns. The node-side ToOption skip is fault-site discipline,
    // not tolerance: the SAME Project failure aborts the projection typed at the MaterialEdges fold (relations.md
    // TraverseM), so the one malformed material faults ONCE at its edge, never twice. Each DISTINCT
    // IfcMaterialDefinition (the select unwrapped through DefinitionOf) additionally lands its imported HasProperties
    // material Psets through MaterialProjection.ImportedPsets as content-minted PropertySet bag nodes — an
    // IfcMaterialProperties is NOT IfcRoot (no GlobalId exists), so the bag node id is NodeId.Content over the node's
    // own canonical bytes, the SAME deterministic construction MaterialEdges re-derives to bind the
    // Assign.PropertyDefinition edge without a shared table.
    static Fin<Noted<Seq<Node>>> Materials(
        IfcProject project, Map<string, NodeId> rooted, double tolerance, UnitScale scale, TemplateScope templates,
        IIfcProfileStore profiles, Op key) {
        var relating = project.Extract<IfcRelAssociatesMaterial>().AsIterable()
            .Choose(static rel => Optional(rel.RelatingMaterial));
        return
            from materials in relating.ToSeq()
                .TraverseM(select => MaterialProjection.Project(select, tolerance, profiles, scale, key)).As()
            from imported in relating.Choose(DefinitionOf).ToSeq().Distinct()
                .TraverseM(definition => MaterialProjection.ImportedPsets(definition, rooted, scale, templates, key)
                    .Map(noted => noted.Map(bags => bags.Map(bag => PropertySetNode(bag, tolerance))))).As()
            // Only the imported-Pset arm can drop, so its logs join while the composition fold contributes none.
            select Noted.Join(imported).Map(bags => toSeq(materials.Map(static m => (Node)m)
                .Concat(bags.Flatten())
                .DistinctBy(static node => node.Id)));
    }

    // The select->definition unwrap for the imported-Pset walk: a usage carries its shared set, a definition is itself;
    // MaterialEdges composes this SAME unwrap, so both ends walk identical IfcMaterialDefinition instances.
    internal static Option<IfcMaterialDefinition> DefinitionOf(IfcMaterialSelect select) => select switch {
        IfcMaterialLayerSetUsage usage   => Optional((IfcMaterialDefinition?)usage.ForLayerSet),
        IfcMaterialProfileSetUsage usage => Optional((IfcMaterialDefinition?)usage.ForProfileSet),
        IfcMaterialDefinition definition => Some(definition),
        _                                => None,
    };

    // The content mint per imported material bag (the Semantics/composition Mint precedent): a class-root [Union] case
    // has no compiler-generated `with`, so the draft id re-stamps through the seam Node.Relabel over the node's own
    // canonical bytes — deterministic, so MaterialEdges derives the identical bag id from the identical PropertyBag.
    internal static Node PropertySetNode(PropertyBag bag, double tolerance) {
        var draft = new Node.PropertySet(NodeId.Content(default), bag);
        return draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
    }

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
                Descriptions:  toSeq(info.FileDescriptions),
                Name:          info.FileName ?? "",
                TimeStamp:     Instant.FromDateTimeUtc(DateTime.SpecifyKind(info.TimeStamp, DateTimeKind.Utc)),
                Authors:       toSeq(info.Author),
                Organizations: toSeq(info.Organization),
                Preprocessor:  info.PreProcessorVersion ?? "",
                OriginatingSystem: info.OriginatingSystem ?? "",
                Schema:        Seq(database.Release.ToString()))
            : StepHeader.Empty with { Schema = Seq(database.Release.ToString()) };

    // The declared-unit PRESENTATION lowering: the model's declared LENGTH token — READ off the assignment's own
    // IfcNamedUnit at UnitScale.Of, never back-inferred from the factor — lands on Header.Units, and the egress
    // DeclareUnits re-authors from that token, so a mm-declared model re-emits mm while the interior stays
    // SI-canonical. The deleted form matched the factor against IEEE literals: 0.001 cannot distinguish a
    // millimetre from a milligram, and 1.0 cannot distinguish a DECLARED metre from an undeclared axis, so the
    // inverse re-authored the model's units by guess. An unrostered or absent declaration keeps the empty scheme.
    static UnitScheme UnitsOf(UnitScale scale) =>
        scale.L.Token is { Length: > 0 } token
            ? new UnitScheme(Map((QuantityType.Length.Value, token)))
            : UnitScheme.Si;

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
// closed PropertyValue family and an IfcPhysicalSimpleQuantity onto a MeasureValue over the seam Dimension, every magnitude
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
    // table, both directions, the ReleaseMap law — so ingress narrowing and egress raising can never drift, and the
    // Model/structural#STRUCTURAL_PROJECTION reader signs every payload magnitude on the SAME rows, so an analysis
    // bag and a Pset row of one measure type carry one dimension, one coercion axis, and one round-trip identity.
    // GG's SURFACE closes this roster, not the IFC schema: IfcThermalResistanceMeasure and
    // IfcTemperatureRateOfChangeMeasure are absent from that surface and therefore carry NO row, so a caller reaching
    // for either gets the MeasureUnmapped Text drop rather than a row naming a type the assembly cannot produce.
    internal static readonly FrozenDictionary<string, MeasureRow> MeasureDimensions = new Dictionary<string, MeasureRow>(StringComparer.Ordinal) {
        // IfcMeasureValue family — SI base + dimensionless tokens
        ["IfcLengthMeasure"] = MeasureRow.Of(Dimension.LengthDim), ["IfcPositiveLengthMeasure"] = MeasureRow.Of(Dimension.LengthDim),
        ["IfcNonNegativeLengthMeasure"] = MeasureRow.Of(Dimension.LengthDim),
        ["IfcAreaMeasure"] = MeasureRow.Of(Dimension.AreaDim), ["IfcVolumeMeasure"] = MeasureRow.Of(Dimension.VolumeDim),
        ["IfcMassMeasure"] = MeasureRow.Of(Dimension.MassDim), ["IfcTimeMeasure"] = MeasureRow.Of(Dimension.DurationDim),
        ["IfcThermodynamicTemperatureMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, 0, 0, 1, 0, 0)),
        ["IfcElectricCurrentMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, 0, 1, 0, 0, 0)),
        ["IfcLuminousIntensityMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, 0, 0, 0, 0, 1)),
        // The two angle rows sign Dimensionless (SI carries no angle axis) and coerce on the declared angle factor
        ["IfcPlaneAngleMeasure"] = MeasureRow.Angle, ["IfcSolidAngleMeasure"] = MeasureRow.Angle,
        ["IfcCountMeasure"] = MeasureRow.Of(Dimension.Dimensionless), ["IfcNumericMeasure"] = MeasureRow.Of(Dimension.Dimensionless),
        ["IfcRatioMeasure"] = MeasureRow.Of(Dimension.Dimensionless), ["IfcPositiveRatioMeasure"] = MeasureRow.Of(Dimension.Dimensionless),
        ["IfcNormalisedRatioMeasure"] = MeasureRow.Of(Dimension.Dimensionless),
        // IfcDerivedMeasureValue family — structural. A planar force is force per unit AREA (N/m2) and a linear force
        // force per unit LENGTH (N/m): the two are one exponent apart, and the shared vector that once spelled both
        // mis-scaled every planar-force magnitude by the model's length factor.
        ["IfcForceMeasure"] = MeasureRow.Of(Dimension.ForceDim), ["IfcPressureMeasure"] = MeasureRow.Of(Dimension.PressureDim),
        ["IfcMassDensityMeasure"] = MeasureRow.Of(Dimension.DensityDim), ["IfcModulusOfElasticityMeasure"] = MeasureRow.Of(Dimension.PressureDim),
        ["IfcPlanarForceMeasure"] = MeasureRow.Of(Dimension.PressureDim), ["IfcLinearForceMeasure"] = MeasureRow.Of(Dimension.Create(0, 1, -2, 0, 0, 0, 0)),
        ["IfcLinearStiffnessMeasure"] = MeasureRow.Of(Dimension.Create(0, 1, -2, 0, 0, 0, 0)),
        ["IfcTorqueMeasure"] = MeasureRow.Of(Dimension.Create(2, 1, -2, 0, 0, 0, 0)),
        ["IfcRotationalStiffnessMeasure"] = MeasureRow.Of(Dimension.Create(2, 1, -2, 0, 0, 0, 0)),
        // Warping stiffness is a warping moment per unit twist — force x length2, ONE length exponent above the
        // rotational row above it; the sealed IfcBoundaryNodeConditionWarping read stamps its row under this type.
        ["IfcWarpingMomentMeasure"] = MeasureRow.Of(Dimension.Create(3, 1, -2, 0, 0, 0, 0)),
        ["IfcMomentOfInertiaMeasure"] = MeasureRow.Of(Dimension.Create(4, 0, 0, 0, 0, 0, 0)),
        ["IfcSectionModulusMeasure"] = MeasureRow.Of(Dimension.VolumeDim),
        // The subgrade-reaction ladder is THREE distinct types one exponent apart — a face reaction N/m3, an edge
        // reaction N/m2, a node reaction N/m — each the declared measure of a StiffnessSelect<T> arm the
        // Model/structural#STRUCTURAL_PROJECTION restraint reader stamps from, so omitting the edge pair forced its
        // magnitudes through a signature the table never signed. ForceDim now answers THREE names — a point force, an
        // edge rotational reaction (N.m/rad per m), and a line moment (N.m/m) — the sharpest case of this table's own
        // law that the measure-type NAME, never the vector, is the round-trip identity.
        ["IfcModulusOfSubgradeReactionMeasure"] = MeasureRow.Of(Dimension.Create(-2, 1, -2, 0, 0, 0, 0)),
        ["IfcModulusOfLinearSubgradeReactionMeasure"] = MeasureRow.Of(Dimension.PressureDim),
        ["IfcModulusOfRotationalSubgradeReactionMeasure"] = MeasureRow.Of(Dimension.ForceDim),
        ["IfcLinearMomentMeasure"] = MeasureRow.Of(Dimension.ForceDim),
        ["IfcMassPerLengthMeasure"] = MeasureRow.Of(Dimension.LinearDensityDim),
        ["IfcAreaDensityMeasure"] = MeasureRow.Of(Dimension.Create(-2, 1, 0, 0, 0, 0, 0)),
        // IfcDerivedMeasureValue family — thermal, energy, hygric, flow
        ["IfcThermalTransmittanceMeasure"] = MeasureRow.Of(Dimension.ThermalTransmittanceDim),
        ["IfcThermalAdmittanceMeasure"] = MeasureRow.Of(Dimension.ThermalTransmittanceDim),
        ["IfcThermalConductivityMeasure"] = MeasureRow.Of(Dimension.Create(1, 1, -3, 0, -1, 0, 0)),
        ["IfcSpecificHeatCapacityMeasure"] = MeasureRow.Of(Dimension.Create(2, 0, -2, 0, -1, 0, 0)),
        ["IfcThermalExpansionCoefficientMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, 0, 0, -1, 0, 0)),
        ["IfcHeatFluxDensityMeasure"] = MeasureRow.Of(Dimension.IrradianceDim),
        ["IfcPowerMeasure"] = MeasureRow.Of(Dimension.Create(2, 1, -3, 0, 0, 0, 0)),
        ["IfcEnergyMeasure"] = MeasureRow.Of(Dimension.Create(2, 1, -2, 0, 0, 0, 0)),
        ["IfcVolumetricFlowRateMeasure"] = MeasureRow.Of(Dimension.Create(3, 0, -1, 0, 0, 0, 0)),
        ["IfcMassFlowRateMeasure"] = MeasureRow.Of(Dimension.Create(0, 1, -1, 0, 0, 0, 0)),
        // The two hygric rows sign the schema's own derived-unit declarations, not the engineering conventions: vapor
        // permeability kg/(s m Pa) reduces to T¹, and moisture diffusivity is declared m3/s (L³T⁻¹), NOT the
        // diffusivity-conventional m2/s — the conventional vector mis-coerces by the model's length factor.
        ["IfcVaporPermeabilityMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, 1, 0, 0, 0, 0)),
        ["IfcMoistureDiffusivityMeasure"] = MeasureRow.Of(Dimension.Create(3, 0, -1, 0, 0, 0, 0)),
        ["IfcIsothermalMoistureCapacityMeasure"] = MeasureRow.Of(Dimension.Create(3, -1, 0, 0, 0, 0, 0)),
        ["IfcDynamicViscosityMeasure"] = MeasureRow.Of(Dimension.Create(-1, 1, -1, 0, 0, 0, 0)),
        ["IfcKinematicViscosityMeasure"] = MeasureRow.Of(Dimension.Create(2, 0, -1, 0, 0, 0, 0)),
        ["IfcMolecularWeightMeasure"] = MeasureRow.Of(Dimension.Create(0, 1, 0, 0, 0, -1, 0)),
        // IfcDerivedMeasureValue family — electrical, lighting, acoustic, motion
        ["IfcElectricVoltageMeasure"] = MeasureRow.Of(Dimension.Create(2, 1, -3, -1, 0, 0, 0)),
        ["IfcFrequencyMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, -1, 0, 0, 0, 0)),
        ["IfcRotationalFrequencyMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, -1, 0, 0, 0, 0)),
        ["IfcAngularVelocityMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, -1, 0, 0, 0, 0)),
        ["IfcLuminousFluxMeasure"] = MeasureRow.Of(Dimension.Create(0, 0, 0, 0, 0, 0, 1)),
        ["IfcIlluminanceMeasure"] = MeasureRow.Of(Dimension.Create(-2, 0, 0, 0, 0, 0, 1)),
        ["IfcSoundPowerMeasure"] = MeasureRow.Of(Dimension.Create(2, 1, -3, 0, 0, 0, 0)),
        ["IfcSoundPressureMeasure"] = MeasureRow.Of(Dimension.PressureDim),
        ["IfcLinearVelocityMeasure"] = MeasureRow.Of(Dimension.Create(1, 0, -1, 0, 0, 0, 0)),
        ["IfcAccelerationMeasure"] = MeasureRow.Of(Dimension.Create(1, 0, -2, 0, 0, 0, 0)),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    // The IfcProperty family -> the seam PropertyValue union: a single value narrows by its IfcValue shape (the
    // three-valued IfcLogical to the seam Logical, never coerced to a two-valued Boolean), an enumerated value carries its
    // SELECTED value LIST (EnumerationValues, the [1:?] cardinality) plus its allowed set (the optional EnumerationReference),
    // a reference value its target NodeId plus its UsageName, a bounded value its lower/upper/setpoint measures, a table value
    // its rows through the SAME LowerValue scalar narrowing the list arm takes (a typed table cell keeps its measure/logical
    // identity — the ValueString coercion that stripped every cell to Text was the one-correspondence DERIVED_LOGIC breach)
    // plus the IfcCurveInterpolationEnum curve rule, a list the recursive arm, and an IfcComplexProperty its UsageName
    // plus its named sub-property bag (HasProperties keyed by each sub-property Name) RECURSING Lower — so a layered glazing /
    // multi-component rating / bSDD complex template is the seam Complex arm, never dropped to Text; only a non-IfcProperty
    // residue falls to Text. The rooted map resolves a reference whose target is a rooted node; a non-rooted reference target
    // (an IfcObjectReferenceSelect resource — a table, an address, a time series — never projected as a node) content-keys an
    // IDENTITY-ONLY NodeId (never the IFC GlobalId AS node identity [H6]) — a NAMED bounded drop: the resource entity itself
    // does not round-trip, the UsageName always carried, so the cycle never drops the three-valued logical, the curve rule,
    // the usage name, or the nested bag.
    public static Fin<Noted<PropertyValue>> Lower(IfcProperty property, Map<string, NodeId> rooted, UnitScale scale, Op key) => property switch {
        IfcPropertySingleValue sv => LowerValue(sv.NominalValue, scale, sv.Unit),
        IfcPropertyEnumeratedValue ev =>
            ev.EnumerationValues.AsIterable().ToSeq().TraverseM(value => LowerValue(value, scale, null)).As()
                .Bind(values => Optional(ev.EnumerationReference)
                    .Match(
                        Some: reference => reference.EnumerationValues.AsIterable().ToSeq().TraverseM(value => LowerValue(value, scale, null)).As(),
                        None: static () => Fin.Succ(Seq<Noted<PropertyValue>>()))
                    .Map(allowed => Noted.Join(values).Bind(selected => Noted.Join(allowed)
                        .Map(sanctioned => (PropertyValue)new PropertyValue.Enumerated(selected, sanctioned))))),   // BOTH slots typed Seq<PropertyValue> — a measured/numeric IfcValue member keeps its discriminant through the same LowerValue rail, never a ValueString flattening; IfcPropertyEnumeratedValue declares no per-value unit, so the enumerated arm alone takes the project regime
        IfcPropertyReferenceValue rv => Fin.Succ(
            Optional(rv.PropertyReference as IfcRoot).Bind(root => rooted.Find(root.GlobalId)).Match(
                Some: id => Noted.Clean(id),
                // The non-rooted resource identity content-keys and its entity does not round-trip — the COUNTED
                // reference-resource drop, RETURNED beside the id so the arm stays an expression.
                None: () => Noted.Drop(FidelityDrop.ReferenceResource, rv.UsageName ?? rv.PropertyReference?.GetType().Name ?? "",
                    NodeId.Content(Encoding.UTF8.GetBytes(rv.PropertyReference is IfcRoot r ? $"ifcroot:{r.GlobalId}" : $"{rv.PropertyReference?.GetType().Name}:{rv.UsageName}"))))
                .Map(id => (PropertyValue)new PropertyValue.Reference(
                    id, string.IsNullOrEmpty(rv.UsageName) ? Option<string>.None : Some(rv.UsageName)))),
        IfcPropertyBoundedValue bv =>
            from lower in MeasureOpt(bv.LowerBoundValue, scale, bv.Unit)
            from upper in MeasureOpt(bv.UpperBoundValue, scale, bv.Unit)
            from setpoint in MeasureOpt(bv.SetPointValue, scale, bv.Unit)
            select Noted.Clean<PropertyValue>(new PropertyValue.Bounded(lower, upper, setpoint)),
        IfcPropertyListValue lv => lv.ListValues.AsIterable().ToSeq().TraverseM(value => LowerValue(value, scale, lv.Unit)).As()
            .Map(static values => Noted.Join(values).Map(static rows => (PropertyValue)new PropertyValue.List(rows))),
        // The two table columns declare SEPARATE units (DefiningUnit / DefinedUnit), so each cell coerces on its own
        // column's override — one shared unit read would rescale the defined column by the defining column's factor.
        IfcPropertyTableValue tv => toSeq(tv.DefiningValues.Zip(tv.DefinedValues))
            .TraverseM(pair =>
                from defining in LowerValue(pair.First, scale, tv.DefiningUnit)
                from defined in LowerValue(pair.Second, scale, tv.DefinedUnit)
                select defining.Bind(first => defined.Map(second => (first, second))))
            .As()
            .Map(rows => Noted.Join(rows).Map(cells => (PropertyValue)new PropertyValue.Table(cells, InterpolationOf(tv.CurveInterpolation)))),
        IfcComplexProperty cp => cp.HasProperties.Values.AsIterable().ToSeq()
            .TraverseM(sub => Lower(sub, rooted, scale, key).Map(lowered => (Name: PropertyName.Create(sub.Name ?? ""), Lowered: lowered)))
            .As()
            .Map(rows => new Noted<PropertyValue>(
                rows.Fold(FidelityLog.Empty, static (log, row) => log + row.Lowered.Log),
                new PropertyValue.Complex(cp.UsageName,
                    rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Lowered.Value))))),
        _ => Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Text(property.Name ?? ""))),
    };

    // An IfcValue -> the seam scalar family. The explicit numeric, binary, and temporal leaves retain their value-domain
    // discriminants; measure types retain both their IFC type name and SI-coerced magnitude. Only the IFC string family
    // shares Text because its subtype does not change the value domain consumed below the seam.
    static Fin<Noted<PropertyValue>> LowerValue(IfcValue? value, UnitScale scale, IfcUnit? declared) =>
        value is null                                                    ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Text("")))
        : value is IfcLogical lg                                         ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Logical(LogicalOpt(lg.Logical))))
        : value is IfcInteger integer                                    ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Integer(new BigInteger(integer.Magnitude))))
        : value is IfcReal number                                        ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Number(number.Magnitude)))
        : value is IfcBinary binary                                      ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Binary(toSeq(binary.Binary))))
        : value is IfcDate date                                          ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Temporal(new TemporalValue.Date(LocalDate.FromDateTime((DateTime)date.Value)))))
        : value is IfcDateTime moment                                    ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Temporal(new TemporalValue.Moment(LocalDateTime.FromDateTime((DateTime)moment.Value)))))
        : value is IfcTime time                                          ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Temporal(new TemporalValue.Time(LocalTime.FromDateTime((DateTime)time.Value)))))
        : value is IfcDuration span                                      ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Temporal(new TemporalValue.Span(
            Period.FromYears(span.Years) + Period.FromMonths(span.Months) + Period.FromDays(span.Days)
            + Period.FromHours(span.Hours) + Period.FromMinutes(span.Minutes)
            + Period.FromSeconds((long)Math.Truncate(span.Seconds))
            + Period.FromNanoseconds((long)((span.Seconds - Math.Truncate(span.Seconds)) * NodaConstants.NanosecondsPerSecond))))))
        : value is IfcTimeStamp stamp                                    ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Temporal(new TemporalValue.Stamp(Instant.FromUnixTimeSeconds((int)stamp.Value)))))
        : value.ValueType == typeof(bool)                                ? Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Boolean(value.Value is bool b && b)))
        : value is IfcMeasureValue or IfcDerivedMeasureValue
            && MeasureDimensions.TryGetValue(value.GetType().Name, out var row) ? MeasureOf(value, row, scale, declared).Map(static measure => Noted.Clean<PropertyValue>(new PropertyValue.Measure(measure)))
        // The two COUNTED identity narrows: an off-table measure type preserves its value as Text (never a wrong
        // dimension), and a non-Label IFC string subtype (IfcText/IfcIdentifier) narrows to Text and re-emits IfcLabel.
        // Each RETURNS its fact beside the value, so the arm stays an expression and the caller's fold banks it.
        : value is IfcMeasureValue or IfcDerivedMeasureValue            ? Fin.Succ(Noted.Drop<PropertyValue>(FidelityDrop.MeasureUnmapped, value.GetType().Name, new PropertyValue.Text(value.ValueString)))
        : value is IfcText or IfcIdentifier                             ? Fin.Succ(Noted.Drop<PropertyValue>(FidelityDrop.StringIdentity, value.GetType().Name, new PropertyValue.Text(value.ValueString)))
        : Fin.Succ(Noted.Clean<PropertyValue>(new PropertyValue.Text(value.ValueString)));

    static Fin<Option<MeasureValue>> MeasureOpt(IfcValue? value, UnitScale scale, IfcUnit? declared) =>
        value is IfcMeasureValue or IfcDerivedMeasureValue && MeasureDimensions.TryGetValue(value.GetType().Name, out var row)
            ? MeasureOf(value, row, scale, declared).Map(Some)
            : Fin.Succ(Option<MeasureValue>.None);

    // The measure value (NATIVE-unit, its row resolved off the frozen MeasureDimensions table) -> the seam
    // MeasureValue through the SI-native OfSi factory, the magnitude coerced through the ONE UnitScale.Coerce entry —
    // GG stores the raw declared-unit magnitude, so an uncoerced admit is the mm-vs-metre trap the composition owner
    // names. The carrier's own declared unit overrides the project regime when the property declares one, so a Pset
    // row authored in kN inside a newton-declared model reads its own factor rather than the project's. The
    // QuantityType is the IFC MEASURE-TYPE NAME (IfcThermalTransmittanceMeasure, IfcMassDensityMeasure, ...), NOT the
    // dimension, because the seven-exponent vector is not injective over quantity types (an IfcForceMeasure and an
    // out-of-family measure can share a dimension, and angle/ratio/count all sit at Dimensionless) — so the
    // measure-type identity round-trips and a QTO accessor never false-matches. The kernel UnitsNet registry is
    // bypassed (the row IS the coercion); a measure type the frozen table does not carry stays Text upstream rather
    // than claiming a wrong dimension.
    static Fin<MeasureValue> MeasureOf(IfcValue measure, MeasureRow row, UnitScale scale, IfcUnit? declared) =>
        MeasureValue.OfSi(QuantityType.Create(measure.GetType().Name), row.Dimension,
            scale.Coerce(AsDouble(measure.Value), row, declared));

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

    // IFC4.3's real-valued tally, which the registry names no quantity for — the OPEN QuantityType.Create mint the
    // seam sanctions, declared ONCE here so the ingress stamp and the Projection/egress raiser row read one spelling
    // and an IfcQuantityNumber never re-emits as the integral IfcQuantityCount.
    internal static readonly QuantityType Number = QuantityType.Create("Number");

    // The IfcQuantity* subtype -> its QTO quantity-type identity: the IFC-schema correspondence, and the ONLY fact
    // the subtype decides. The GG roster is these SEVEN concretes — the IFC4.3 IfcQuantityNumber included, whose
    // absence faulted every 4.3 model carrying one as quantity-kind-unmapped.
    static readonly FrozenDictionary<Type, QuantityType> QuantityTypes = new Dictionary<Type, QuantityType> {
        [typeof(IfcQuantityLength)] = QuantityType.Length, [typeof(IfcQuantityArea)] = QuantityType.Area,
        [typeof(IfcQuantityVolume)] = QuantityType.Volume, [typeof(IfcQuantityWeight)] = QuantityType.Mass,
        [typeof(IfcQuantityTime)] = QuantityType.Duration, [typeof(IfcQuantityCount)] = QuantityType.Count,
        [typeof(IfcQuantityNumber)] = Number,
    }.ToFrozenDictionary();

    // An IfcPhysicalSimpleQuantity -> the seam MeasureValue [H2]. The magnitude and its dimension come off the base's
    // OWN polymorphic IfcMeasureValue read resolved through the SAME frozen MeasureDimensions table the property lane
    // reads, so ONE construction and ONE Coerce serve both lanes and the seven per-subtype value-property spellings
    // never fan into seven constructions. IfcQuantityCount took a hand-skipped factor-free arm that its own empty
    // exponent vector already makes 1.0 — a fabricated exception, deleted. The quantity's declared Unit overrides the
    // project regime where IFC authors one. An unrostered simple quantity faults typed, never a fabricated zero.
    public static Fin<MeasureValue> Measure(IfcPhysicalSimpleQuantity quantity, UnitScale scale, Op key) =>
        QuantityTypes.TryGetValue(quantity.GetType(), out QuantityType? qto)
        && MeasureDimensions.TryGetValue(quantity.MeasureValue.GetType().Name, out MeasureRow row)
            ? MeasureValue.OfSi(qto, row.Dimension, scale.Coerce(AsDouble(quantity.MeasureValue.Value), row, quantity.Unit))
            : Fin.Fail<MeasureValue>(new BimFault.CodecReject(key, $"quantity-kind-unmapped:{quantity.GetType().Name}"));

    // A magnitude GG boxes as something no numeric conversion reaches is ABSENT, and absence spells NaN so
    // MeasureValue.OfSi's own finite gate refuses it on the rail this method already returns. A 0.0 fallback is
    // the forged measurement this spelling deletes: it admits, content-keys, and round-trips as a real reading,
    // so a wall publishes zero thickness and a assembly zero transmittance with nothing raising anywhere. The
    // guard is unreachable for every rostered measure type — GG boxes each as a numeric — which is exactly why a
    // silent zero would never be caught by a run: the arm only fires when the package's own storage changes.
    static double AsDouble(object? value) =>
        value is IConvertible c ? Convert.ToDouble(c, System.Globalization.CultureInfo.InvariantCulture) : double.NaN;
}
```

## [03]-[GRAPH_LEGALITY]

- Owner: `IfcLegality` the `IGraphConstraint` deciding IFC-semantic RELATIONSHIP legality the seam's structural `GraphDelta` switch cannot [M3] — the seam enforces only structural invariants (an edge endpoint resolves, an endpoint kind is legal), and which entity may relate to which is Bim's, depended UP on through the `IGraphConstraint` contract. Class and predefined-token VALIDITY is not here: the `Emit` egress gate owns the whole token vocabulary [PREDEFINED_TOKEN_RULING], because ingress admits tokens bare and a second validity owner at composition time forks the vocabulary between the two ends.
- Entry: `IfcLegality.Validate(GraphDelta delta, ElementGraph graph) → Validation<Error,Unit>` accumulates every IFC-legality violation the delta's `AddedEdges` carry, `Success(unit)` when every rule holds, a `Fail` carrying the accumulated `Error` set otherwise; the validation is applicative (every violation reported at once, never short-circuit) so an authoring pass sees all rejects in one apply, the `BimFault` arms surviving the `Error.Combine` because the band is `Expected`-derived.
- Auto: the rules dispatch on the seam's NEUTRAL case + sub-kind (the seam carries no `IfcRel*` case) — a `Compose` edge with the `Contain` sub-kind requires its `Whole` to resolve a spatial-container row on the SIBLING vocabularies (`Model/spatial#SPATIAL_STRUCTURE` `SpatialClass.IsContainer` for the site/building/storey/space + IFC4.3 facility/facility-part containers, `Model/zones#ZONE_GRAPH` `BimZoneKind.IsSpatial` for `IfcSpatialZone` — the disjoint partition; a private re-listed leaf set is the deleted drift form the spatial owner names, the six-row instance of which faulted every 4.3 infrastructure containment), a `Compose` edge with the `Aggregate` sub-kind may not have a `Type` object as its `Whole`, a spatial-to-spatial `Contain`/`Aggregate` edge (both endpoints resolving `SpatialClass` rows) must nest downward per `SpatialClass.CanContain` (`containment-rank-inverted` otherwise — a storey aggregating its site faults loud), a `Void` edge dispatches its SUB-KIND — `VoidKind.Void` requires its `Feature` (the ingest lands relating=host, related=feature) to be a feature subtraction (`IfcOpeningElement` or the 4.3 `IfcVoidingFeature`), `VoidKind.Fill` requires its `Host` to be one (the `Fills` row reads relating=opening, so the OPENING sits in the `Host` slot — a blanket `Feature` check rejected every legal fill) — and an `Assign` edge with the `TypeDefinition` sub-kind requires its `Definition` to be a `Type` object.
- Law: an endpoint resolves over `Endpoints` — the delta's OWN `AddedNodes` UNIONED with the merged `graph`, added winning — because a delta landing a storey and its containment edge in one merge must see the node the delta itself adds; a graph-only lookup faults every same-delta endpoint and makes the gate un-runnable on a first import. The two outcomes are DISTINCT faults: an ABSENT endpoint is `BimFault.DanglingReference` (the merge itself is malformed) and a FAILED predicate is `BimFault.ModelRejected` (legal STEP, illegal IFC semantics) — one detail for both hid a broken merge inside a vocabulary complaint and sent a federation manager to the wrong end.
- Packages: Rasm.Element, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new IFC-legality rule is one arm on the `Rule` switch; a new spatial container is one `SpatialClass`/`BimZoneKind` row on its OWNING sibling vocabulary (this gate widens with zero edits); a feature-subtraction class is one `Subtraction` row; the structural invariants stay the seam's `GraphDelta` switch and never migrate here; never a per-rule validator type and never a class or token roster read.
- Boundary: `IfcLegality` decides IFC RELATIONSHIP legality ONLY — the structural invariants (endpoint resolution, endpoint-kind legality) are the seam's `GraphDelta` total switch and re-checking them here is the deleted form [M3], and class/predefined-token validity is the egress `Emit` gate whole [PREDEFINED_TOKEN_RULING]; the rules read the generic `Classification` code (`IfcSite`, `IfcOpeningElement`) and the `ObjectKind` (occurrence/type), never an `IfcProduct` runtime type (GeometryGym stays captured in the projector); the validation is applicative-accumulating so an authoring pass sees every reject at once, never the first-fail short-circuit a `Fin` rail gives; each `BimFault` lifts BARE (the band IS the `Expected` `Code`) so `error.IsType<BimFault.ModelRejected>()` and `error.IsType<BimFault.DanglingReference>()` both survive the `Error.Combine` and separate the two failure modes downstream.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// IfcLegality is the IGraphConstraint half the seam composes after its structural GraphDelta law [M3];
// it reads the seam NEUTRAL Relationship case + the generic Classification/ObjectKind, never a GeometryGym type.
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim.Model;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Projection;

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

    // The endpoint domain: the delta's OWN added nodes UNIONED with the merged graph, ADDED winning. A delta landing a
    // storey and its containment edge in one merge resolves that storey from AddedNodes, so a graph-only lookup faults
    // every same-delta endpoint as dangling and makes this gate un-runnable on a first import; added wins because the
    // delta is the newer authority for a re-put node.
    readonly record struct Endpoints(Map<NodeId, Node.Object> Added, ElementGraph Graph) {
        public static Endpoints Of(GraphDelta delta, ElementGraph graph) =>
            new(delta.AddedNodes.Fold(Map<NodeId, Node.Object>(), static (map, node) =>
                node is Node.Object obj ? map.AddOrUpdate(obj.Id, obj) : map), graph);

        public Option<Node.Object> Find(NodeId id) =>
            Added.Find(id) is { IsSome: true } added ? added : Graph.Find<Node.Object>(id);
    }

    // The delta's ADDED EDGES validate the relationship law applicatively — every violation reported at once, never a
    // first-fail short-circuit — each lifting its BimFault BARE. Nodes carry no arm here: token validity is the egress
    // Emit gate whole, so this fold reads the added nodes only as the edge endpoints they resolve.
    public Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph) {
        Endpoints endpoints = Endpoints.Of(delta, graph);   // built ONCE per delta — a per-edge rebuild is quadratic
        return delta.AddedEdges.Map(edge => Rule(edge, endpoints))
            .Fold(Success<Error, Unit>(unit), static (acc, rule) => (acc, rule).Apply(static (_, _) => unit).As());
    }

    // The closed IFC-legality rule set dispatched on the seam's NEUTRAL case + sub-kind (the seam carries no IfcRel* case,
    // so the rule reads the neutral Compose/Void/Assign shape, never an IFC wire-name): containment-whole-must-be-spatial,
    // a type may not aggregate, the sub-kind-oriented Void/Fill feature-subtraction checks, DefinesByType definition-must-
    // be-type. The Void axis dispatches its SUB-KIND because the ingest is orientation-preserving: a Voids edge lands
    // relating=host/related=feature, a Fills edge relating=OPENING/related=filler — so Fill checks Host, never Feature.
    static Validation<Error, Unit> Rule(Relationship edge, Endpoints endpoints) => edge switch {
        Relationship.Compose c when c.SubKind == ComposeKind.Contain =>
            (RequireClass(c.Whole, endpoints, SpatialWhole, $"containment-whole-not-spatial:{c.Whole.Value}"),
             SpatialRank(c.Whole, c.Part, endpoints)).Apply(static (_, _) => unit).As(),
        Relationship.Compose c when c.SubKind == ComposeKind.Aggregate =>
            (RequireKind(c.Whole, endpoints, static kind => kind == ObjectKind.Occurrence, $"type-aggregates-occurrence:{c.Whole.Value}"),
             SpatialRank(c.Whole, c.Part, endpoints)).Apply(static (_, _) => unit).As(),
        Relationship.Void v when v.SubKind == VoidKind.Void =>
            RequireClass(v.Feature, endpoints, Subtraction.Contains, $"voids-feature-not-subtraction:{v.Feature.Value}"),
        Relationship.Void v when v.SubKind == VoidKind.Fill =>
            RequireClass(v.Host, endpoints, Subtraction.Contains, $"fills-host-not-subtraction:{v.Host.Value}"),
        Relationship.Assign a when a.SubKind == AssignKind.TypeDefinition =>
            RequireKind(a.Definition, endpoints, static kind => kind == ObjectKind.Type, $"definesbytype-definition-not-type:{a.Definition.Value}"),
        _ => Success<Error, Unit>(unit),
    };

    // The parent->child spatial rank law (Model/spatial#SPATIAL_STRUCTURE SpatialClass.CanContain): when BOTH
    // endpoints resolve SpatialClass rows the nesting runs downward (equal-rank facility parts legal, no child
    // root); an element endpoint (no spatial row) passes — the whole-side gate above owns it, and an UNRESOLVED
    // endpoint is that gate's DanglingReference rather than a second dangling report from here.
    static Validation<Error, Unit> SpatialRank(NodeId whole, NodeId part, Endpoints endpoints) =>
        SpatialOf(whole, endpoints)
            .Bind(w => SpatialOf(part, endpoints).Map(p => (Whole: w, Part: p)))
            .Match(
                None: () => Success<Error, Unit>(unit),
                Some: pair => pair.Whole.CanContain(pair.Part)
                    ? Success<Error, Unit>(unit)
                    : Fail<Error, Unit>(new BimFault.ModelRejected(Gate, $"containment-rank-inverted:{pair.Whole.Key}>{pair.Part.Key}")));

    static Option<SpatialClass> SpatialOf(NodeId id, Endpoints endpoints) =>
        endpoints.Find(id).Bind(static o => SpatialClass.TryGet(o.Classification.Code));

    // An ABSENT endpoint and a FAILED predicate are TWO faults, never one detail: an unresolvable id means the MERGE
    // is malformed (DanglingReference — the delta references a node neither it nor the graph carries), a resolved node
    // breaking the rule means the MODEL is malformed (ModelRejected — legal STEP, illegal IFC semantics). Collapsing
    // both onto one detail reported a broken merge as a vocabulary complaint and sent the fix to the wrong end.
    static Validation<Error, Unit> RequireClass(NodeId id, Endpoints endpoints, Func<string, bool> ok, string detail) =>
        endpoints.Find(id).Match(
            None: () => Fail<Error, Unit>(new BimFault.DanglingReference(Gate, $"endpoint-unresolved:{id.Value}")),
            Some: obj => ok(obj.Classification.Code)
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new BimFault.ModelRejected(Gate, detail)));

    static Validation<Error, Unit> RequireKind(NodeId id, Endpoints endpoints, Func<ObjectKind, bool> ok, string detail) =>
        endpoints.Find(id).Match(
            None: () => Fail<Error, Unit>(new BimFault.DanglingReference(Gate, $"endpoint-unresolved:{id.Value}")),
            Some: obj => ok(obj.Kind)
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(new BimFault.ModelRejected(Gate, detail)));
}
```

## [04]-[RESEARCH]

(none)
