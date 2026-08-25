# [ELEMENT_RELATION]

`Relationship` `[Union]` owns the neutral objectified-edge algebra: five library-neutral edge kinds carrying typed `NodeId` endpoints and a neutral sub-kind discriminant the `Bake` fold dispatches on, with a `Generic` passthrough carrying wire name, endpoints, and attribute map so NO relationship is ever dropped. IFC relationship names, directionality, inverse semantics, and the long-tail families live in the `Rasm.Bim` `SemanticProjector`, so the seam never re-opens the IFC-schema strata the `Classification` collapse already closed.

## [01]-[INDEX]

- [02]-[EDGE_ALGEBRA]: `Relationship` the `[Union]` five-neutral-case edge algebra with its `Generic` passthrough, the `RelationshipKind` neutral CASE discriminant and the per-case `SubKind` vocabularies, the uniform consumer accessor surface (`Kind`/`Relating`/`Related`/`Members`/`DirectedPairs`/`IsContainment`/`Endpoints`/`Touches`/`CanonicalBytes(writer)`), and the `MaterialUsage` occurrence payload the `Associate` edge carries.

## [02]-[EDGE_ALGEBRA]

- Owner: `Relationship` the `[Union]` neutral objectified-edge algebra and uniform accessor surface; generated keyed vocabularies own every case/sub-kind/direction token; schema list order rides the `Compose` edge's `Ordinal`; `MaterialUsage` carries explicit optional cardinal placement and SI length measures, never numeric unset sentinels.
- Cases: `Compose` (a `Whole`→`Part` decomposition with a `ComposeKind` flavor — aggregate/nest/contain/reference) · `Assign` (a `Subject`→`Definition` binding with an `AssignKind` flavor — property-definition/type-definition/group/assessment/observation) · `Associate` (a `Subject`→`Resource` binding — a `Composition/material#MATERIAL_COMPOSITION` material node carrying a `MaterialUsage` occurrence payload, or an `Appearance` presentation node or a `Geospatial/coverage#COVERAGE_NODE` `Coverage` field node carrying `MaterialUsage.Unbound` — the IFC `IfcRelAssociates` base; the structural `Graph/delta#GRAPH_DELTA` `LegalLink` admits Material/Appearance/Coverage and the `Bake` fold reads each resource kind) · `Connect` (a `From`↔`To` connectivity with a `ConnectKind` flavor, an optional realizing node, and an optional content-keyed `Interface` — the shared `IfcConnectionGeometry`/space-boundary surface the blob store carries, resolved by content key through the `Graph/element#NODE_MODEL` `GeometrySource.ResolveFootprint` leg, never inline coordinates) · `Void` (a `Host`→`Feature` opening with a `VoidKind` flavor — void/fill) · `Generic` (a `WireName` + endpoints + attribute map + an ordered `Participants` roster carrying the n-ary IFC member lists with role/ordinal, so no IFC relationship is dropped); the closed five-kind algebra with its passthrough.
- Entry: the case constructors are the typed edge admissions (`new Relationship.Compose(whole, part, ComposeKind.Aggregate)`, `new Relationship.Associate(subject, material, usage)`); `Kind` projects the neutral `RelationshipKind` case token a consumer routes on, `Relating`/`Related` the endpoint reads and `Endpoints` the `(Relating, Related)` pair the structural law and traversal take, `Members` every node the edge involves (binary endpoints, a `Connect`'s realizing intermediary, a `Generic` edge's `Participants` roster, and its `PropertyValue.References` attribute-buried ids) and `DirectedPairs` the directed adjacency it contributes (endpoints, realizing, and a `Generic` edge's source→participant legs so an n-ary member is topology-reachable — never the buried attribute refs), `IsContainment` the spatial-containment predicate a Persistence spatial-structure query filters on; `Touches(nodeId)` tests `Members` membership for the incidence index and the `DropNode` cascade; `CanonicalBytes(writer)` is the ONE edge projection — the graph content key composes it in place and the standalone edge digest a content-3-way merge keys on is `Projection/address#CONTENT_ADDRESS` `ContentAddress.Of(edge, tolerance)` streaming the same projection at the model grid (a byte-materializing `ToCanonicalBytes` twin here was the deleted form), the tolerance threaded so a `Generic` edge's `PropertyValue.Measure` attributes quantize to `Header.Tolerance` exactly as the node measures do (the five typed cases carry no Measure and are tolerance-insensitive).
- Auto: ONE private `Ends` generated total `Switch` projects each case's relating/related pair with the `Connect` realizing intermediary (the primary correspondence, DERIVED_LOGIC), and `Endpoints`/`Relating`/`Related`/`Members`/`DirectedPairs`/`Touches` all DERIVE from it — so the incidence index (`Graph/element#ELEMENT_GRAPH`) and the structural legality (`Graph/delta#GRAPH_DELTA`) read one accessor and the endpoint law is declared at exactly one dispatch site, never three parallel Switches restating the five binary pairs; `Kind` projects through the generated `Map` (case → precomputed constant row — no throwaway lambdas, the constant dual of the func-form `Switch` the allocating `Remap` takes); the neutral sub-kinds round-trip their token at the wire and drive the `Bake` descent (`Compose.Aggregate`/`Nest`/`Contain` descend into parts, `Assign.TypeDefinition` binds the type for inheritance, `Assign.PropertyDefinition`/`Assessment` attach the bag/assessment, `Assign.Observation` attaches the measured series off the OCCURRENCE alone — a `Component` is a catalogue entry and is never instrumented, so the type fold skips this sub-kind where it gathers every other, `Associate` folds the material with its usage); the `Assign.TypeDefinition` edge carries the named type→occurrence inheritance the `Bake` fold applies over the type's standardized data — single fields occurrence-overrides-type, `Seq` fields (materials/assessments/classifications) union + dedup-by-key — distinct from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the bag `Merge` owns, so the seam binds the type once through this one neutral row and the `Bake` realizes the full standardized inheritance; the `Generic` attribute map carries the IFC-specific fields the Bim projector preserved so a round-trip re-authors the original relation.
- Output: the `Relationship` is the typed edge a `GraphDelta` adds/removes and the `Bake` fold traverses; the `MaterialUsage` on an `Associate` edge is the occurrence geometric binding a host materializes (a layer set's direction, offset, and reference extent, a profile's cardinal point) so a wall and its mirror share one `LayerSet` composition with two `Associate` usages; the `Generic` passthrough is the round-trip guarantee — every IFC relationship the projector cannot map to a neutral case rides `Generic` so an import→export cycle drops nothing.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`), LanguageExt.Core (`Map`/`Option`/`Fin`), `Rasm` (content-key seed + the `Op` op-key).
- Growth: a new graph-relationship semantic is one neutral case or one sub-kind row (a `Reference` compose flavor, a new `AssignKind`) — an `AssignKind` row lands only WITH a seam-typed target shape, so the process/actor/resource/control assignments (`IfcRelAssignsToProcess`/`Actor`/`Resource`/`Control`) mint NO rows and ride `Generic` with `Participants` (the Bim projector's passthrough rows already carry them, and no Process/Actor node exists on the seam for a typed row to bind); a new IFC relationship maps onto an existing neutral case or rides `Generic`, never a new seam case; a new occurrence usage is one `MaterialUsage` arm; the algebra is closed at five neutral kinds with the passthrough, the IFC roster living in the Bim projector.
- Boundary: `Relationship` is the NEUTRAL edge algebra — the rejected seventeen-typed-`IfcRel*`-case design is the deleted form, because it re-opened the IFC-schema strata the `Classification` collapse closed; the IFC names, directionality, inverse semantics, and the eight stranded families live in the `Rasm.Bim` `SemanticProjector`, mapping each `IfcRel*` onto a neutral case with its typed payload or the `Generic` passthrough, and the seam carries no `IfcRel*` spelling; the discrimination is two-level — the base `RelationshipKind Kind` is the neutral CASE token a topology/merge consumer routes on (a uniform read with NO union switch, the flat edge column Persistence persists), the per-case `SubKind` the NEUTRAL graph-semantic flavor the `Bake` reads (whole/part flavor, assignment flavor), neither the IFC roster, and a per-case `Kind` shadowing a base `Kind` is the deleted form; `Members`/`Touches` include a `Connect`'s realizing intermediary, a `Generic` edge's `Participants` roster, AND its buried attribute references (the `PropertyValue.References` dual of `Remap`) so a `DropNode` cascade never strands a live reference, while `DirectedPairs` stays endpoints-plus-realizing with the `Generic` source→participant legs (a buried attribute reference is not a directed adjacency leg); the `Connect.Interface` content key is a BLOB reference, not a node — it rides `CanonicalBytes` (presence-prefixed) and the wire, never `Members`/`DirectedPairs`/`Remap`, and decodes through the one `GeometrySource.ResolveFootprint` leg so the connection interface never mints a parallel decode port; the `Associate` edge carries the `MaterialUsage` occurrence payload — the type-level `Composition/material#MATERIAL_COMPOSITION` set carries the shared layer/profile structure, the edge the per-occurrence geometric binding — so usage never duplicates onto the composition; classification is a generic value ON the `Object` node, NOT an edge, so the seam carries no classification-association relationship; the `Generic` passthrough guarantees no relationship is dropped, so a round-trip through the seam preserves every edge.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Relations;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class RelationshipKind {
 public static readonly RelationshipKind Compose = new("compose");
 public static readonly RelationshipKind Assign = new("assign");
 public static readonly RelationshipKind Associate = new("associate");
 public static readonly RelationshipKind Connect = new("connect");
 public static readonly RelationshipKind Void = new("void");
 public static readonly RelationshipKind Generic = new("generic");
}

[SmartEnum<string>]
public sealed partial class ComposeKind {
 public static readonly ComposeKind Aggregate = new("aggregate", isOwning: true, isSpatial: false);
 public static readonly ComposeKind Nest = new("nest", isOwning: true, isSpatial: false);
 public static readonly ComposeKind Contain = new("contain", isOwning: true, isSpatial: true);
 public static readonly ComposeKind Reference = new("reference", isOwning: false, isSpatial: true);

 public bool IsOwning { get; }
 public bool IsSpatial { get; }
}

[SmartEnum<string>]
public sealed partial class AssignKind {
 public static readonly AssignKind PropertyDefinition = new("property-definition");
 public static readonly AssignKind TypeDefinition = new("type-definition");
 public static readonly AssignKind Group = new("group");
 public static readonly AssignKind Assessment = new("assessment");
 public static readonly AssignKind Observation = new("observation");
}

[SmartEnum<string>]
public sealed partial class ConnectKind {
 public static readonly ConnectKind Element = new("element");
 public static readonly ConnectKind Path = new("path");
 public static readonly ConnectKind Port = new("port");
}

[SmartEnum<string>]
public sealed partial class VoidKind {
 public static readonly VoidKind Void = new("void");
 public static readonly VoidKind Fill = new("fill");
}

[SmartEnum<string>]
public sealed partial class LayerSetDirection {
 public static readonly LayerSetDirection Axis1 = new("axis-1");
 public static readonly LayerSetDirection Axis2 = new("axis-2");
 public static readonly LayerSetDirection Axis3 = new("axis-3");
}

[SmartEnum<string>]
public sealed partial class DirectionSense {
 public static readonly DirectionSense Positive = new("positive");
 public static readonly DirectionSense Negative = new("negative");
}

[SmartEnum<int>]
public sealed partial class CardinalPoint {
 public static readonly CardinalPoint BottomLeft = new(1);
 public static readonly CardinalPoint BottomCentre = new(2);
 public static readonly CardinalPoint BottomRight = new(3);
 public static readonly CardinalPoint MidLeft = new(4);
 public static readonly CardinalPoint Mid = new(5);
 public static readonly CardinalPoint MidRight = new(6);
 public static readonly CardinalPoint TopLeft = new(7);
 public static readonly CardinalPoint TopCentre = new(8);
 public static readonly CardinalPoint TopRight = new(9);
 public static readonly CardinalPoint Centroid = new(10);
 public static readonly CardinalPoint CentroidBottom = new(11);
 public static readonly CardinalPoint CentroidLeft = new(12);
 public static readonly CardinalPoint CentroidRight = new(13);
 public static readonly CardinalPoint CentroidTop = new(14);
 public static readonly CardinalPoint ShearCentre = new(15);
 public static readonly CardinalPoint ShearBottom = new(16);
 public static readonly CardinalPoint ShearLeft = new(17);
 public static readonly CardinalPoint ShearRight = new(18);
 public static readonly CardinalPoint ShearTop = new(19);

 public static Fin<CardinalPoint> Of(int reference, Op key) =>
  key.Row<int, CardinalPoint>(reference);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial class MaterialUsage {
 private MaterialUsage() { }

 [Equatable] public sealed partial class Unbound : MaterialUsage;

 [Equatable]
 public sealed partial class LayerSet : MaterialUsage {
  private LayerSet(LayerSetDirection direction, DirectionSense sense, Option<MeasureValue> offsetFromReferenceLine, Option<MeasureValue> referenceExtent) =>
   (Direction, Sense, OffsetFromReferenceLine, ReferenceExtent) = (direction, sense, offsetFromReferenceLine, referenceExtent);

  public LayerSetDirection Direction { get; }
  public DirectionSense Sense { get; }
  public Option<MeasureValue> OffsetFromReferenceLine { get; }
  public Option<MeasureValue> ReferenceExtent { get; }

  public static Fin<MaterialUsage> Of(
   LayerSetDirection direction, DirectionSense sense,
   Option<MeasureValue> offsetFromReferenceLine, Option<MeasureValue> referenceExtent, Op key) =>
   Accumulate(Seq(
     Length(offsetFromReferenceLine, "offset-from-reference-line", key),
     Length(referenceExtent, "reference-extent", key)))
    .Map(_ => (MaterialUsage)new LayerSet(direction, sense, offsetFromReferenceLine, referenceExtent))
    .ToFin();
 }

 [Equatable]
 public sealed partial class ProfileSet : MaterialUsage {
  public Option<CardinalPoint> CardinalPoint { get; }
  public Option<MeasureValue> ReferenceExtent { get; }

  private ProfileSet(Option<CardinalPoint> cardinalPoint, Option<MeasureValue> referenceExtent) =>
   (CardinalPoint, ReferenceExtent) = (cardinalPoint, referenceExtent);

  public static Fin<MaterialUsage> Of(Option<int> cardinalPoint, Option<MeasureValue> referenceExtent, Op key) =>
   from point in cardinalPoint.TraverseM(reference => CardinalPoint.Of(reference, key)).As()
   from _ in Length(referenceExtent, "reference-extent", key).ToFin()
   select (MaterialUsage)new ProfileSet(point, referenceExtent);
 }

 public void CanonicalBytes(CanonicalWriter w) => Switch(
  unbound: _ => w.Ordinal(0),
  layerSet: u => w.Ordinal(1).String(u.Direction.Key).String(u.Sense.Key)
   .Optional(u.OffsetFromReferenceLine, static (value, writer) => writer.Measure(value))
   .Optional(u.ReferenceExtent, static (value, writer) => writer.Measure(value)),
  profileSet: u => w.Ordinal(2)
   .Optional(u.CardinalPoint, static (point, writer) => writer.Ordinal(point.Key))
   .Optional(u.ReferenceExtent, static (value, writer) => writer.Measure(value)));

 private static Validation<Error, Unit> Length(Option<MeasureValue> measure, string slot, Op key) =>
  Gate(measure.ForAll(static value => value.Dimension == Dimension.LengthDim), key, $"<material-usage-measure-not-length:{slot}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d));
}

[ValueObject<string>]
public sealed partial class WireName {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim();
  validationError = value.Length == 0 ? new ValidationError("<wire-name-blank>") : validationError;
 }
}

[ValueObject<string>]
public sealed partial class RoleName {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim();
  validationError = value.Length == 0 ? new ValidationError("<role-name-blank>") : validationError;
 }
}

public readonly record struct RelationshipParticipant(NodeId Node, RoleName Role, Option<int> Ordinal);

[Union]
public abstract partial class Relationship {
 private Relationship() { members = new(MembersOf); pairs = new(PairsOf); touch = new(() => [.. members.Value]); }

 [IgnoreEquality] private readonly Lazy<Seq<NodeId>> members;
 [IgnoreEquality] private readonly Lazy<Seq<(NodeId From, NodeId To)>> pairs;
 [IgnoreEquality] private readonly Lazy<System.Collections.Generic.HashSet<NodeId>> touch;

 [Equatable] public sealed partial class Compose(NodeId whole, NodeId part, ComposeKind subKind, Option<int> ordinal = default) : Relationship { public NodeId Whole { get; } = whole; public NodeId Part { get; } = part; public ComposeKind SubKind { get; } = subKind; public Option<int> Ordinal { get; } = ordinal; }
 [Equatable] public sealed partial class Assign(NodeId subject, NodeId definition, AssignKind subKind) : Relationship { public NodeId Subject { get; } = subject; public NodeId Definition { get; } = definition; public AssignKind SubKind { get; } = subKind; }
 [Equatable] public sealed partial class Associate(NodeId subject, NodeId resource, MaterialUsage usage) : Relationship { public NodeId Subject { get; } = subject; public NodeId Resource { get; } = resource; public MaterialUsage Usage { get; } = usage; }
 [Equatable] public sealed partial class Connect(NodeId from, NodeId to, ConnectKind subKind, Option<NodeId> realizing, Option<UInt128> interfaceKey = default) : Relationship { public NodeId From { get; } = from; public NodeId To { get; } = to; public ConnectKind SubKind { get; } = subKind; public Option<NodeId> Realizing { get; } = realizing; public Option<UInt128> Interface { get; } = interfaceKey; }
 [Equatable] public sealed partial class Void(NodeId host, NodeId feature, VoidKind subKind) : Relationship { public NodeId Host { get; } = host; public NodeId Feature { get; } = feature; public VoidKind SubKind { get; } = subKind; }
 [Equatable] public sealed partial class Generic(WireName wireName, NodeId source, NodeId target, Map<PropertyName, PropertyValue> attributes, Seq<RelationshipParticipant> participants = default) : Relationship { public WireName WireName { get; } = wireName; public NodeId Source { get; } = source; public NodeId Target { get; } = target; [UnorderedEquality] public Map<PropertyName, PropertyValue> Attributes { get; } = attributes; [OrderedEquality] public Seq<RelationshipParticipant> Participants { get; } = participants; }

 (NodeId Relating, NodeId Related, Option<NodeId> Realizing) Ends => Switch(
 compose: static r => (r.Whole, r.Part, Option<NodeId>.None),
 assign: static r => (r.Subject, r.Definition, Option<NodeId>.None),
 associate: static r => (r.Subject, r.Resource, Option<NodeId>.None),
 connect: static r => (r.From, r.To, r.Realizing),
 @void: static r => (r.Host, r.Feature, Option<NodeId>.None),
 generic: static r => (r.Source, r.Target, Option<NodeId>.None));

 public (NodeId Relating, NodeId Related) Endpoints => Ends switch { var (relating, related, _) => (relating, related) };

 public RelationshipKind Kind => Map(
 compose: RelationshipKind.Compose,
 assign: RelationshipKind.Assign,
 associate: RelationshipKind.Associate,
 connect: RelationshipKind.Connect,
 @void: RelationshipKind.Void,
 generic: RelationshipKind.Generic);

 public NodeId Relating => Endpoints.Relating;
 public NodeId Related => Endpoints.Related;

 public Seq<NodeId> Members => members.Value;

 Seq<NodeId> MembersOf() =>
  (Ends switch { var (relating, related, realizing) => Seq(relating, related) + realizing.ToSeq() })
  + (this is Generic g ? g.Participants.Map(static participant => participant.Node) + g.Attributes.Values.ToSeq().Bind(static v => v.References()) : Seq<NodeId>());

 public Seq<(NodeId From, NodeId To)> DirectedPairs => pairs.Value;

 Seq<(NodeId From, NodeId To)> PairsOf() => (Ends switch {
 var (relating, related, realizing) => realizing.Match(
  Some: via => Seq((relating, via), (via, related)),
  None: () => Seq((relating, related))),
 }) + (this is Generic generic
  ? generic.Participants.Filter(participant => participant.Node != generic.Source && participant.Node != generic.Target)
    .Map(participant => (generic.Source, participant.Node))
  : Seq<(NodeId, NodeId)>());

 public bool IsContainment => this is Compose { SubKind: { IsOwning: true, IsSpatial: true } };

 public bool Touches(NodeId node) => touch.Value.Contains(node);

 public void CanonicalBytes(CanonicalWriter w) => Switch(
 compose: r => w.Ordinal(0).String(r.Whole.Value).String(r.Part.Value).String(r.SubKind.Key)
  .Optional(r.Ordinal, static (ordinal, run) => run.Ordinal(ordinal)),
 assign: r => w.Ordinal(1).String(r.Subject.Value).String(r.Definition.Value).String(r.SubKind.Key),
 associate: r => { w.Ordinal(2).String(r.Subject.Value).String(r.Resource.Value); r.Usage.CanonicalBytes(w); return w; },
 connect: r => w.Ordinal(3).String(r.From.Value).String(r.To.Value).String(r.SubKind.Key)
  .Optional(r.Realizing, static (node, run) => run.String(node.Value))
  .Optional(r.Interface, static (blob, run) => run.U128(blob)),
 @void: r => w.Ordinal(4).String(r.Host.Value).String(r.Feature.Value).String(r.SubKind.Key),
 generic: r => w.Ordinal(5).String(r.WireName.Value).String(r.Source.Value).String(r.Target.Value)
  .Sorted(r.Attributes.ToSeq(), static pair => pair.Key.Value, StringComparer.Ordinal,
   static (pair, run) => { run.String(pair.Key.Value); pair.Value.CanonicalBytes(run); })
  .Rows(r.Participants, static (participant, run) => run.String(participant.Node.Value).String(participant.Role.Value)
   .Optional(participant.Ordinal, static (ordinal, inner) => inner.Ordinal(ordinal))));

 public Relationship Remap(Func<NodeId, NodeId> map) => Switch<Relationship>(
  compose: r => new Compose(map(r.Whole), map(r.Part), r.SubKind, r.Ordinal),
  assign: r => new Assign(map(r.Subject), map(r.Definition), r.SubKind),
  associate: r => new Associate(map(r.Subject), map(r.Resource), r.Usage),
  connect: r => new Connect(map(r.From), map(r.To), r.SubKind, r.Realizing.Map(map), r.Interface),
  @void: r => new Void(map(r.Host), map(r.Feature), r.SubKind),
  generic: r => new Generic(r.WireName, map(r.Source), map(r.Target), r.Attributes.Map((_, v) => v.Remap(map)), r.Participants.Map(participant => participant with { Node = map(participant.Node) })));
}
```

## [03]-[IMPLEMENTATION_LAW]

- [NEUTRAL_EDGE_ALGEBRA]: `Relationship` carries a neutral five-kind algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void`) with a `Generic` passthrough, and the `Rasm.Bim` `SemanticProjector` maps every `IfcRel*` onto a neutral case with its typed payload or onto `Generic(wireName, relating, related, attrs)`, IFC directionality and inverse semantics living wholly in the projector — so no relationship drops and the seam never re-opens the IFC-schema strata leak `Classification` closed.
- [CONNECT_MEDIUM]: `ConnectKind` is three-row — `Element` the medium-less concrete `IfcRelConnectsElements` base (its direct `IfcRelConnectsWithRealizingElements` subtype riding `Connect.Realizing`, never a sub-kind row), `Path` the path-element join, `Port` the port join — so a bare element adjacency never force-stamps `path` and a realizing edge never mints a parallel medium.
- [CONNECT_INTERFACE]: `Connect.Interface` carries the `IfcConnectionGeometry`/space-boundary interface surface by content key, the Bim projector hashing that geometry into the blob store and stamping the key, so a `SpaceBoundary` 2nd-level crossing delivers the energy-model surface set typed instead of stranding it in a `Generic` attribute and an interface-less `Connect` stays plain topology.
- [OCCURRENCE_USAGE]: `Associate.Usage` carries the per-occurrence layer/profile placement distinct from the shared material composition. Direction and sense are keyed rows; offset and extent are optional length-typed `MeasureValue`s; cardinal placement is `Option<CardinalPoint>`. Wire shapes mirror those directly, so absence never occupies `0` or `NaN`, and every supplied scalar re-crosses the quantity invariant.
- [UNIFORM_ACCESSOR_SURFACE]: `Relationship` carries a uniform consumer surface, so a `Rasm.Persistence` `Query` topology pass and a `Version` 3-way `StructuralMerge` read an edge WITHOUT a union switch. One private `Ends` case walk declares the relating/related/realizing correspondence once and every endpoint, adjacency, reachability, containment, and canonical-bytes accessor derives from it, so the family cannot drift case-by-case.
- [TWO_LEVEL_DISCRIMINATION]: `Kind` stays the neutral `RelationshipKind` case token on the base — persisted as a flat edge column, grouped on in merge, projected through the generated `Map` over precomputed constant rows — and `SubKind` the flavor on each case, so a consumer's `.Kind`/`.IsContainment` read resolves to the seam's actual union rather than a parallel discriminant.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
