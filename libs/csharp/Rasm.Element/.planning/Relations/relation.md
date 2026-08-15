# [ELEMENT_RELATION]

`Relationship` `[Union]` owns the neutral objectified-edge algebra: five library-neutral edge kinds carrying typed `NodeId` endpoints and a neutral sub-kind discriminant the `Bake` fold dispatches on, with a `Generic` passthrough carrying wire name, endpoints, and attribute map so NO relationship is ever dropped. IFC relationship names, directionality, inverse semantics, and the long-tail families live in the `Rasm.Bim` `SemanticProjector`, so the seam never re-opens the IFC-schema strata the `Classification` collapse already closed.

## [01]-[INDEX]

- [02]-[EDGE_ALGEBRA]: `Relationship` the `[Union]` five-neutral-case edge algebra with its `Generic` passthrough, the `RelationshipKind` neutral CASE discriminant and the per-case `SubKind` vocabularies, the uniform consumer accessor surface (`Kind`/`Relating`/`Related`/`Members`/`DirectedPairs`/`IsContainment`/`Endpoints`/`Touches`/`ToCanonicalBytes(tolerance)`), and the `MaterialUsage` occurrence payload the `Associate` edge carries.

## [02]-[EDGE_ALGEBRA]

- Owner: `Relationship` the `[Union]` neutral objectified-edge algebra and uniform accessor surface; generated keyed vocabularies own every case/sub-kind/direction token; schema list order rides the `Compose` edge's `Ordinal`; `MaterialUsage` carries explicit optional cardinal placement and SI length measures, never numeric unset sentinels.
- Cases: `Compose` (a `Whole`→`Part` decomposition with a `ComposeKind` flavor — aggregate/nest/contain/reference) · `Assign` (a `Subject`→`Definition` binding with an `AssignKind` flavor — property-definition/type-definition/group/assessment/observation) · `Associate` (a `Subject`→`Resource` binding — a `Composition/material#MATERIAL_COMPOSITION` material node carrying a `MaterialUsage` occurrence payload, or an `Appearance` presentation node or a `Geospatial/coverage#COVERAGE_NODE` `Coverage` field node carrying `MaterialUsage.None` — the IFC `IfcRelAssociates` base; the structural `Graph/delta#GRAPH_DELTA` `LegalLink` admits Material/Appearance/Coverage and the `Bake` fold reads each resource kind) · `Connect` (a `From`↔`To` connectivity with a `ConnectKind` flavor, an optional realizing node, and an optional content-keyed `Interface` — the shared `IfcConnectionGeometry`/space-boundary surface the blob store carries, resolved by content key through the `Graph/element#NODE_MODEL` `GeometrySource.ResolveFootprint` leg, never inline coordinates) · `Void` (a `Host`→`Feature` opening with a `VoidKind` flavor — void/fill) · `Generic` (a `WireName` + endpoints + attribute map + an ordered `Participants` roster carrying the n-ary IFC member lists with role/ordinal, so no IFC relationship is dropped); the closed five-kind algebra with its passthrough.
- Entry: the case constructors are the typed edge admissions (`new Relationship.Compose(whole, part, ComposeKind.Aggregate)`, `new Relationship.Associate(subject, material, usage)`); `Kind` projects the neutral `RelationshipKind` case token a consumer routes on, `Relating`/`Related` the endpoint reads and `Endpoints` the `(Relating, Related)` pair the structural law and traversal take, `Members` every node the edge involves (binary endpoints, a `Connect`'s realizing intermediary, a `Generic` edge's `Participants` roster, and its `PropertyValue.References` attribute-buried ids) and `DirectedPairs` the directed adjacency it contributes (endpoints, realizing, and a `Generic` edge's source→participant legs so an n-ary member is topology-reachable — never the buried attribute refs), `IsContainment` the spatial-containment predicate a Persistence spatial-structure query filters on; `Touches(nodeId)` tests `Members` membership for the incidence index and the `DropNode` cascade; `ToCanonicalBytes(tolerance)` projects the standalone edge bytes a content-3-way merge keys on under the model tolerance, the SAME projection `CanonicalBytes(writer)` composes into the graph content key — the tolerance threaded so a `Generic` edge's `PropertyValue.Measure` attributes quantize to `Header.Tolerance` exactly as the node measures do (the five typed cases carry no Measure and are tolerance-insensitive).
- Auto: ONE private `Ends` generated total `Switch` projects each case's relating/related pair with the `Connect` realizing intermediary (the primary correspondence, DERIVED_LOGIC), and `Endpoints`/`Relating`/`Related`/`Members`/`DirectedPairs`/`Touches` all DERIVE from it — so the incidence index (`Graph/element#ELEMENT_GRAPH`) and the structural legality (`Graph/delta#GRAPH_DELTA`) read one accessor and the endpoint law is declared at exactly one dispatch site, never three parallel Switches restating the five binary pairs; `Kind` projects through the generated `Map` (case → precomputed constant row — no throwaway lambdas, the constant dual of the func-form `Switch` the allocating `Remap` takes); the neutral sub-kinds round-trip their token at the wire and drive the `Bake` descent (`Compose.Aggregate`/`Nest`/`Contain` descend into parts, `Assign.TypeDefinition` binds the type for inheritance, `Assign.PropertyDefinition`/`Assessment` attach the bag/receipt, `Assign.Observation` attaches the measured series off the OCCURRENCE alone — a `Component` is a catalogue entry and is never instrumented, so the type fold skips this sub-kind where it gathers every other, `Associate` folds the material with its usage); the `Assign.TypeDefinition` edge carries the named type→occurrence inheritance the `Bake` fold applies over the type's standardized data — single fields occurrence-overrides-type, `Seq` fields (materials/assessments/classifications) union + dedup-by-key — distinct from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the bag `Merge` owns, so the seam binds the type once through this one neutral row and the `Bake` realizes the full standardized inheritance; the `Generic` attribute map carries the IFC-specific fields the Bim projector preserved so a round-trip re-authors the original relation.
- Receipt: the `Relationship` is the typed edge a `GraphDelta` adds/removes and the `Bake` fold traverses; the `MaterialUsage` on an `Associate` edge is the occurrence geometric binding a host materializes (a layer set's direction, offset, and reference extent, a profile's cardinal point) so a wall and its mirror share one `LayerSet` composition with two `Associate` usages; the `Generic` passthrough is the round-trip guarantee — every IFC relationship the projector cannot map to a neutral case rides `Generic` so an import→export cycle drops nothing.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`), LanguageExt.Core (`Map`/`Option`/`Fin`), `Rasm` (content-key seed + the `Op` op-key).
- Growth: a new graph-relationship semantic is one neutral case or one sub-kind row (a `Reference` compose flavor, a new `AssignKind`) — an `AssignKind` row lands only WITH a seam-typed target shape, so the process/actor/resource/control assignments (`IfcRelAssignsToProcess`/`Actor`/`Resource`/`Control`) mint NO rows and ride `Generic` with `Participants` (the Bim projector's passthrough rows already carry them, and no Process/Actor node exists on the seam for a typed row to bind); a new IFC relationship maps onto an existing neutral case or rides `Generic`, never a new seam case; a new occurrence usage is one `MaterialUsage` arm; the algebra is closed at five neutral kinds with the passthrough, the IFC roster living in the Bim projector.
- Boundary: `Relationship` is the NEUTRAL edge algebra — the rejected seventeen-typed-`IfcRel*`-case design is the deleted form, because it re-opened the IFC-schema strata the `Classification` collapse closed; the IFC names, directionality, inverse semantics, and the eight stranded families live in the `Rasm.Bim` `SemanticProjector`, mapping each `IfcRel*` onto a neutral case with its typed payload or the `Generic` passthrough, and the seam carries no `IfcRel*` spelling; the discrimination is two-level — the base `RelationshipKind Kind` is the neutral CASE token a topology/merge consumer routes on (a uniform read with NO union switch, the flat edge column Persistence persists), the per-case `SubKind` the NEUTRAL graph-semantic flavor the `Bake` reads (whole/part flavor, assignment flavor), neither the IFC roster, and a per-case `Kind` shadowing a base `Kind` is the deleted form; `Members`/`Touches` include a `Connect`'s realizing intermediary, a `Generic` edge's `Participants` roster, AND its buried attribute references (the `PropertyValue.References` dual of `Remap`) so a `DropNode` cascade never strands a live reference, while `DirectedPairs` stays endpoints-plus-realizing with the `Generic` source→participant legs (a buried attribute reference is not a directed adjacency leg); the `Connect.Interface` content key is a BLOB reference, not a node — it rides `CanonicalBytes` (presence-prefixed) and the wire, never `Members`/`DirectedPairs`/`Remap`, and decodes through the one `GeometrySource.ResolveFootprint` leg so the connection interface never mints a parallel decode port; the `Associate` edge carries the `MaterialUsage` occurrence payload — the type-level `Composition/material#MATERIAL_COMPOSITION` set carries the shared layer/profile structure, the edge the per-occurrence geometric binding — so usage never duplicates onto the composition; classification is a generic value ON the `Object` node, NOT an edge, so the seam carries no classification-association relationship; the `Generic` passthrough guarantees no relationship is dropped, so a round-trip through the seam preserves every edge.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Relations;

// --- [TYPES] ------------------------------------------------------------------------------
// RelationshipKind is the neutral CASE discriminant — which of the six edge kinds an edge is, the flat token a
// topology/merge consumer (Rasm.Persistence Query/Version) routes on through `edge.Kind` without switching on the
// union type. SubKind (ComposeKind/AssignKind/ConnectKind/VoidKind) carries the per-case flavor the Bake fold reads.
[SmartEnum<string>]
public sealed partial class RelationshipKind {
 public static readonly RelationshipKind Compose = new("compose");
 public static readonly RelationshipKind Assign = new("assign");
 public static readonly RelationshipKind Associate = new("associate");
 public static readonly RelationshipKind Connect = new("connect");
 public static readonly RelationshipKind Void = new("void");
 public static readonly RelationshipKind Generic = new("generic");
}

// Neutral sub-kind vocabularies the Bake fold dispatches on — NOT the IFC roster. The Bim
// projector maps IfcRelAggregates→Compose/Aggregate, IfcRelDefinesByType→Assign/TypeDefinition, etc.
[SmartEnum<string>]
public sealed partial class ComposeKind {
 public static readonly ComposeKind Aggregate = new("aggregate"); // whole decomposes into parts
 public static readonly ComposeKind Nest = new("nest"); // nested membership; schema list order rides the edge's Ordinal
 public static readonly ComposeKind Contain = new("contain"); // spatial containment
 public static readonly ComposeKind Reference = new("reference"); // non-owning spatial reference
}

[SmartEnum<string>]
public sealed partial class AssignKind {
 public static readonly AssignKind PropertyDefinition = new("property-definition"); // attaches a PropertySet/QuantitySet bag
 public static readonly AssignKind TypeDefinition = new("type-definition"); // the occurrence→Type-Object bind the owning Component projection authors; the Bake fold resolves it into the named type→occurrence inheritance (single fields occurrence-overrides-type, Seq materials/assessments/classifications union+dedup-by-key), NEVER a parallel DefinesByType case — IfcRelDefinesByType rides THIS neutral row
 public static readonly AssignKind Group = new("group"); // group/system/zone membership
 public static readonly AssignKind Assessment = new("assessment"); // attaches an Assessment receipt
 public static readonly AssignKind Observation = new("observation"); // attaches an Assessment/observation ObservationSeries — OCCURRENCE-only, so the Bake type fold never gathers it
}

// ConnectKind names the connection MEDIUM only — realization rides the Connect case's own Option<NodeId> Realizing
// field, never a fourth row: a Realizing row re-describes that field (MODAL_ARITY's rejected parallel discriminant)
// and contradicts it (SubKind=Realizing with Realizing=None). Medium (element/path/port) and realization
// (direct/via-intermediate) are orthogonal axes: IfcRelConnectsWithRealizingElements subtypes the medium-less
// IfcRelConnectsElements base directly, so it rides Element with Realizing=Some — never a false "path" stamp and
// never a medium of its own.
[SmartEnum<string>]
public sealed partial class ConnectKind {
 public static readonly ConnectKind Element = new("element"); // bare element adjacency — the concrete IfcRelConnectsElements base, no path/port medium
 public static readonly ConnectKind Path = new("path"); // path-element connectivity (walls, members)
 public static readonly ConnectKind Port = new("port"); // MEP port connectivity
}

[SmartEnum<string>]
public sealed partial class VoidKind {
 public static readonly VoidKind Void = new("void"); // host carved by a feature (opening, recess)
 public static readonly VoidKind Fill = new("fill"); // feature filled by an element (door in opening)
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

// CardinalPoint keys the profile-placement reference grid — the GeometryGym IfcCardinalPointReference reciprocal —
// on the IFC integer (1..9 bounding-box grid, 10..19 the centroidal/shear-axis references a non-rectangular section
// places on). SmartEnum, never a guarded int, because the seam admits this value FROM a raw IFC integer, TryGet
// bridging it; absence stays outside the vocabulary as Option<CardinalPoint>.
[SmartEnum<int>]
public sealed partial class CardinalPoint {
 public static readonly CardinalPoint BottomLeft = new(1);
 public static readonly CardinalPoint BottomCentre = new(2);
 public static readonly CardinalPoint BottomRight = new(3);
 public static readonly CardinalPoint MidLeft = new(4);
 public static readonly CardinalPoint Mid = new(5);            // the IFC default reference point
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
  TryGet(reference, out CardinalPoint? point) && point is { } p ? Fin.Succ(p) : ElementFault.ValueRejected(key, $"<cardinal-point-out-of-grid:{reference}>");
}

// --- [MODELS] -----------------------------------------------------------------------------
// MaterialUsage carries the occurrence material usage the Associate edge holds — the per-occurrence geometric
// binding the type-level MaterialComposition set omits. CLASS-root [Union] with [Equatable] seated PER NESTED CASE (the
// [GRAPH_FAMILY] form — a root seat leaves case members reference-comparing) so a changed OffsetFromReferenceLine
// flips the case's generated equality and the 3-way merge localizes it through the CASE comparer after
// discrimination, never a whole-edge replacement lost to reference identity.
[Union]
public abstract partial class MaterialUsage {
 private MaterialUsage() { }

 [Equatable] public sealed partial class None : MaterialUsage;

 // LayerSet carries the occurrence layer-set placement as generated direction/sense rows and optional length
 // measures. Absence is Option, never NaN; SI normalization and finiteness stay the MeasureValue invariant.
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
   Lengths(offsetFromReferenceLine, referenceExtent, key)
    .Map(_ => (MaterialUsage)new LayerSet(direction, sense, offsetFromReferenceLine, referenceExtent));
 }

 // ProfileSet admits the optional cardinal grid and optional length extent through their owners.
 [Equatable]
 public sealed partial class ProfileSet : MaterialUsage {
  public Option<CardinalPoint> CardinalPoint { get; }
  public Option<MeasureValue> ReferenceExtent { get; }

  private ProfileSet(Option<CardinalPoint> cardinalPoint, Option<MeasureValue> referenceExtent) =>
   (CardinalPoint, ReferenceExtent) = (cardinalPoint, referenceExtent);

  // Spell the absent second length `Option<MeasureValue>.None`: the sibling `MaterialUsage.None` case TYPE is in
  // scope inside this owner and outranks the `Prelude.None` import, so the bare spelling binds a type where the
  // signature wants a value.
  public static Fin<MaterialUsage> Of(Option<int> cardinalPoint, Option<MeasureValue> referenceExtent, Op key) =>
   from point in cardinalPoint.Match(Some: reference => CardinalPoint.Of(reference, key).Map(static value => Some(value)), None: static () => Fin.Succ(Option<CardinalPoint>.None))
   from _ in Lengths(referenceExtent, Option<MeasureValue>.None, key)
   select (MaterialUsage)new ProfileSet(point, referenceExtent);
 }

 // MaterialUsage co-locates its canonical projection here (the PropertyValue/MaterialComposition shape): case
 // ordinal then the typed geometric payload, the CardinalPoint written as its grid-integer Key — the Associate
 // edge composes it through one call so the usage owns its own content contribution, never inlined per edge.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  none: _ => w.Ordinal(0),
  layerSet: u => { w.Ordinal(1).String(u.Direction.Key).String(u.Sense.Key); WriteMeasure(w, u.OffsetFromReferenceLine); return WriteMeasure(w, u.ReferenceExtent); },
  profileSet: u => { w.Ordinal(2).Bool(u.CardinalPoint.IsSome); u.CardinalPoint.IfSome(point => w.Ordinal(point.Key)); return WriteMeasure(w, u.ReferenceExtent); });

 private static Fin<Unit> Lengths(Option<MeasureValue> first, Option<MeasureValue> second, Op key) =>
  Seq(first, second).Choose(static measure => measure).ForAll(static measure => measure.Dimension == Dimension.LengthDim)
   ? Fin.Succ(unit)
   : ElementFault.ValueRejected(key, "<material-usage-measure-not-length>");

 private static CanonicalWriter WriteMeasure(CanonicalWriter writer, Option<MeasureValue> measure) {
  writer.Bool(measure.IsSome);
  measure.IfSome(value => writer.Measure(value));
  return writer;
 }
}

public readonly record struct RelationshipParticipant(NodeId Node, string Role, Option<int> Ordinal);

// Relationship declares a CLASS-root [Union] (the [GRAPH_FAMILY] form): a class root surrenders Thinktecture's
// record-generated equality, so structural equality AND the member-level diff ride Generator.Equals [Equatable] seated PER NESTED CASE — a
// root seat is the compile-proven silent form whose case members reference-compare. Every EqualityComparer
// <Relationship>.Default fold reads the case's generated Equals override, and the Persistence 3-way
// StructuralMerge localizes a changed edge (an Associate.Usage, a [UnorderedEquality] Generic.Attributes delta)
// through the CASE comparer after discrimination, never a whole-edge replacement lost to reference identity.
[Union]
public abstract partial class Relationship {
 // Relationship seeds the Members memo HERE because an edge is immutable and every incidence build, DropNode
 // cascade, and Touches probe reads that set — a Generic edge's recursive attribute walk re-derived per probe is
 // exactly the cost the frozen graph's built-once index exists to avoid. [IgnoreEquality] keeps a derived cache out
 // of the structural diff the Persistence 3-way merge drills.
 private Relationship() => members = new(MembersOf);

 [IgnoreEquality] private readonly Lazy<Seq<NodeId>> members;

 [Equatable] public sealed partial class Compose(NodeId whole, NodeId part, ComposeKind subKind, Option<int> ordinal = default) : Relationship { public NodeId Whole { get; } = whole; public NodeId Part { get; } = part; public ComposeKind SubKind { get; } = subKind; public Option<int> Ordinal { get; } = ordinal; }
 [Equatable] public sealed partial class Assign(NodeId subject, NodeId definition, AssignKind subKind) : Relationship { public NodeId Subject { get; } = subject; public NodeId Definition { get; } = definition; public AssignKind SubKind { get; } = subKind; }
 [Equatable] public sealed partial class Associate(NodeId subject, NodeId resource, MaterialUsage usage) : Relationship { public NodeId Subject { get; } = subject; public NodeId Resource { get; } = resource; public MaterialUsage Usage { get; } = usage; }
 // Interface is the OPTIONAL content-keyed connection-interface geometry (IfcConnectionGeometry / the space-boundary
 // surface an energy model runs on) — a blob-store key resolved through the Graph/element#NODE_MODEL
 // GeometrySource.ResolveFootprint leg, NEVER a NodeId (it rides no Members/DirectedPairs/Remap) and never inline coordinates.
 [Equatable] public sealed partial class Connect(NodeId from, NodeId to, ConnectKind subKind, Option<NodeId> realizing, Option<UInt128> interfaceKey = default) : Relationship { public NodeId From { get; } = from; public NodeId To { get; } = to; public ConnectKind SubKind { get; } = subKind; public Option<NodeId> Realizing { get; } = realizing; public Option<UInt128> Interface { get; } = interfaceKey; }
 [Equatable] public sealed partial class Void(NodeId host, NodeId feature, VoidKind subKind) : Relationship { public NodeId Host { get; } = host; public NodeId Feature { get; } = feature; public VoidKind SubKind { get; } = subKind; }
 [Equatable] public sealed partial class Generic(string wireName, NodeId source, NodeId target, Map<PropertyName, PropertyValue> attributes, Seq<RelationshipParticipant> participants = default) : Relationship { public string WireName { get; } = wireName; public NodeId Source { get; } = source; public NodeId Target { get; } = target; [UnorderedEquality] public Map<PropertyName, PropertyValue> Attributes { get; } = attributes; [OrderedEquality] public Seq<RelationshipParticipant> Participants { get; } = participants; }

 // Ends is the ONE primary case walk (DERIVED_LOGIC): each case's relating/related pair with the Connect realizing
 // intermediary (None elsewhere) — Endpoints/Relating/Related/Members/DirectedPairs/Touches all DERIVE from this
 // triple, so the five binary cases state their endpoint law exactly once and a new case extends exactly one
 // dispatch site instead of three parallel Switches restating the same pairs.
 (NodeId Relating, NodeId Related, Option<NodeId> Realizing) Ends => Switch(
 compose: static r => (r.Whole, r.Part, Option<NodeId>.None),
 assign: static r => (r.Subject, r.Definition, Option<NodeId>.None),
 associate: static r => (r.Subject, r.Resource, Option<NodeId>.None),
 connect: static r => (r.From, r.To, r.Realizing),
 @void: static r => (r.Host, r.Feature, Option<NodeId>.None),
 generic: static r => (r.Source, r.Target, Option<NodeId>.None));

 public (NodeId Relating, NodeId Related) Endpoints => Ends switch { var (relating, related, _) => (relating, related) };

 // Kind projects the neutral CASE token a topology/merge consumer routes on without a union switch — the flat column
 // Persistence persists and groups by. Generated Map (case → PRECOMPUTED constant), never Switch: every arm is a
 // singleton row, no throwaway `static _ => Const` lambdas — the constant dual of Remap's allocating func-form below.
 public RelationshipKind Kind => Map(
 compose: RelationshipKind.Compose,
 assign: RelationshipKind.Assign,
 associate: RelationshipKind.Associate,
 connect: RelationshipKind.Connect,
 @void: RelationshipKind.Void,
 generic: RelationshipKind.Generic);

 // Endpoint reads a consumer takes without destructuring the Endpoints pair.
 public NodeId Relating => Endpoints.Relating;
 public NodeId Related => Endpoints.Related;

 // Every distinct node the edge involves — the binary endpoints, a Connect's realizing intermediary, AND every graph-node
 // id BURIED in a Generic edge's PropertyValue attributes (the recursive Properties/property#PROPERTY_VALUE
 // PropertyValue.References dual of Remap) — the cascade/orphan reachability set the incidence index, the DropNode
 // cascade, and the Apply replay guard read, so a buried attribute Reference is live for renumber AND cascade in lockstep
 // (Remap rewrites it, Members sweeps it) and a surviving Generic edge can never carry a dangling Reference.
 // DirectedPairs stays endpoints-plus-realizing (an attribute reference is not a directed adjacency leg).
 public Seq<NodeId> Members => members.Value;

 Seq<NodeId> MembersOf() =>
  (Ends switch { var (relating, related, realizing) => Seq(relating, related) + realizing.ToSeq() })
  + (this is Generic g ? g.Participants.Map(static participant => participant.Node) + g.Attributes.Values.ToSeq().Bind(static v => v.References()) : Seq<NodeId>());

 // DirectedPairs contributes the edge's directed adjacency to a topology view — one pair for a binary edge, the two
 // realizing legs (From→Realizing→To) for a Connect carrying a realizing intermediary.
 public Seq<(NodeId From, NodeId To)> DirectedPairs => (Ends switch {
 var (relating, related, realizing) => realizing.Match(
  Some: via => Seq((relating, via), (via, related)),
  None: () => Seq((relating, related))),
 }) + (this is Generic generic
  ? generic.Participants.Filter(participant => participant.Node != generic.Source && participant.Node != generic.Target)
    .Map(participant => (generic.Source, participant.Node))
  : Seq<(NodeId, NodeId)>());

 // Spatial containment — the IfcRelContainedInSpatialStructure shape a Persistence spatial-structure query filters:
 // a Compose edge with the Contain flavor (the decomposition Aggregate/Nest and the non-owning Reference are NOT containment).
 public bool IsContainment => this is Compose { SubKind: var k } && k == ComposeKind.Contain;

 public bool Touches(NodeId node) => Members.Exists(m => m == node);

 // ToCanonicalBytes projects the standalone edge bytes a content-3-way merge keys on — the SAME projection
 // CanonicalBytes composes into the graph content key, threading the MODEL tolerance. Associate usage lengths and
 // Generic measure attributes both
 // quantize through w.Measure, so the edge key obeys the same grid as node values.
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) { CanonicalWriter w = new(tolerance); CanonicalBytes(w); return w.ToBytes(); }

 // CanonicalBytes projects through the Projection/address#CONTENT_ADDRESS writer: case ordinal,
 // neutral sub-kind, endpoints, and the typed payload — the edge contributes to the graph content key.
 // Every optional ordinal writes through a LAMBDA, never the `w.Ordinal` method group: the writer's primitives all
 // return the writer for chaining, and a non-void method group has no conversion to the `Action<int>` IfSome takes.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
 compose: r => { w.Ordinal(0).String(r.Whole.Value).String(r.Part.Value).String(r.SubKind.Key).Bool(r.Ordinal.IsSome); r.Ordinal.IfSome(ordinal => w.Ordinal(ordinal)); return w; },
 assign: r => w.Ordinal(1).String(r.Subject.Value).String(r.Definition.Value).String(r.SubKind.Key),
 associate: r => { w.Ordinal(2).String(r.Subject.Value).String(r.Resource.Value); r.Usage.CanonicalBytes(w); return w; },
 connect: r => { w.Ordinal(3).String(r.From.Value).String(r.To.Value).String(r.SubKind.Key).Bool(r.Realizing.IsSome); r.Realizing.IfSome(n => w.String(n.Value)); w.Bool(r.Interface.IsSome); r.Interface.IfSome(k => w.U128(k)); return w; },
 @void: r => w.Ordinal(4).String(r.Host.Value).String(r.Feature.Value).String(r.SubKind.Key),
 generic: r => { w.Ordinal(5).String(r.WireName).String(r.Source.Value).String(r.Target.Value).Ordinal(r.Attributes.Count); foreach (KeyValuePair<PropertyName, PropertyValue> attribute in r.Attributes.OrderBy(static p => p.Key.Value, StringComparer.Ordinal)) { w.String(attribute.Key.Value); attribute.Value.CanonicalBytes(w); } w.Ordinal(r.Participants.Count); foreach (RelationshipParticipant participant in r.Participants) { w.String(participant.Node.Value).String(participant.Role).Bool(participant.Ordinal.IsSome); participant.Ordinal.IfSome(ordinal => w.Ordinal(ordinal)); } return w; });

 // Re-maps EVERY NodeId the edge carries (an unmapped id passes through unchanged): endpoints, a Connect's realizing
 // intermediary, AND every kernel reference buried in a Generic attribute — the Generic arm composes the ONE recursive
 // PropertyValue.Remap owner, never a verbatim Attributes pass that dangles a Reference (the deleted endpoints-only
 // rewrite). Class-root union cases have no `with`, so each arm reconstructs through the func-form Switch (Map carries
 // only precomputed constants); exhaustive over the closed six-case algebra — a new case breaks the rewrite at compile time.
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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
