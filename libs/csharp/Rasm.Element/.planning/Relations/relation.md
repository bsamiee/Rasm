# [ELEMENT_RELATION]

The neutral objectified-edge algebra: one `Relationship` `[Union]` over FIVE library-neutral edge kinds — `Compose` (whole/part decomposition), `Assign` (subject↔definition binding), `Associate` (subject↔resource binding — a material with occurrence usage, or an appearance/coverage resource), `Connect` (element↔element connectivity), `Void` (host↔feature opening) — plus a `Generic` passthrough carrying a wire name, the endpoints, and an attribute map so NO relationship is ever dropped. This REPLACES the rejected seventeen-typed-`IfcRel*`-case design: the seam carries a neutral edge algebra a graph reasons over, while ALL IFC relationship names, directionality, inverse semantics, and the long-tail families (`ConnectsStructuralMember`/`ConnectsPortToElement`, `SpaceBoundary` 1st/2nd level, `Covers*`, `ConnectsPathElements` wall-join priorities, `Declares`, `AssignsToControl`/`Process`/`Product`/`Actor`, `InterferesElements`) live in the `Rasm.Bim` `SemanticProjector`, riding the typed payload of a neutral case or the `Generic` passthrough — so the seam never re-opens the IFC-schema strata the `Classification` collapse already closed. Each edge carries typed `NodeId` endpoints and a small NEUTRAL sub-kind discriminant the `Bake` fold dispatches on (a `Compose.Aggregate` descends into parts, an `Assign.TypeDefinition` binds the type `Object` for the named type→occurrence inheritance, an `Assign.Assessment` attaches a receipt), never the IFC roster. The `Associate` (material) edge carries a typed `MaterialUsage` occurrence payload: a `LayerSet` usage (direction/sense/offset/extent) or a `ProfileSet` usage (cardinal-point/extent), the per-occurrence geometric binding the type-level `Composition/material#MATERIAL_COMPOSITION` set does not carry. Classification is NOT an edge — it is a generic value ON the `Object` node, so the seam carries no classification-association relationship. The page composes `Graph/element#NODE_MODEL` `NodeId` for endpoints and `Properties/property#PROPERTY_VALUE` for the `Generic` attribute map; an endpoint-kind violation rails `Projection/fault#FAULT_BAND` `ElementFault.RelationshipInvalid` at `Graph/delta#GRAPH_DELTA` `Apply`.

## [01]-[INDEX]

- [01]-[EDGE_ALGEBRA]: the `Relationship` `[Union]` five-neutral-case edge algebra plus the `Generic` passthrough, the `RelationshipKind` neutral CASE discriminant and the per-case `SubKind` vocabularies, the uniform consumer accessor surface (`Kind`/`Relating`/`Related`/`Members`/`DirectedPairs`/`IsContainment`/`Endpoints`/`Touches`/`ToCanonicalBytes(tolerance)`), and the `MaterialUsage` occurrence payload the `Associate` edge carries.

## [02]-[EDGE_ALGEBRA]

- Owner: `Relationship` the `[Union]` neutral objectified-edge algebra carrying the uniform consumer accessor surface (`Kind`/`Relating`/`Related`/`Members`/`DirectedPairs`/`IsContainment`/`Endpoints`/`Touches`/`ToCanonicalBytes(tolerance)`); `RelationshipKind` the `[SmartEnum<string>]` neutral CASE discriminant a topology/merge consumer routes on; the `ComposeKind`/`AssignKind`/`ConnectKind`/`VoidKind` `[SmartEnum<string>]` per-case `SubKind` vocabularies the `Bake` fold dispatches on; `MaterialUsage` the `[Union]` occurrence-usage payload the `Associate` edge carries (owning its co-located `CanonicalBytes`); the `LayerSetDirection`/`DirectionSense` neutral geometric enums and `CardinalPoint` the `[SmartEnum<int>]` reference-grid vocabulary the `ProfileSet` usage admits a raw IFC integer through.
- Cases: `Compose` (a `Whole`→`Part` decomposition with a `ComposeKind` flavor — aggregate/nest/contain/reference) · `Assign` (a `Subject`→`Definition` binding with an `AssignKind` flavor — property-definition/type-definition/group/assessment) · `Associate` (a `Subject`→`Resource` binding — a `Composition/material#MATERIAL_COMPOSITION` material node carrying a `MaterialUsage` occurrence payload, or an `Appearance` presentation node or a `Geospatial/coverage#COVERAGE_NODE` `Coverage` field node carrying `MaterialUsage.None` — the IFC `IfcRelAssociates` base; the structural `Graph/delta#GRAPH_DELTA` `LegalLink` admits Material/Appearance/Coverage and the `Bake` fold reads each resource kind) · `Connect` (a `From`↔`To` connectivity with a `ConnectKind` flavor and an optional realizing node) · `Void` (a `Host`→`Feature` opening with a `VoidKind` flavor — void/fill) · `Generic` (a `WireName` + endpoints + attribute map passthrough so no IFC relationship is dropped); the closed five-kind algebra plus the passthrough.
- Entry: the case constructors are the typed edge admissions (`new Relationship.Compose(whole, part, ComposeKind.Aggregate)`, `new Relationship.Associate(subject, material, usage)`); `Kind` projects the neutral `RelationshipKind` case token a consumer routes on, `Relating`/`Related` the endpoint reads and `Endpoints` the `(Relating, Related)` pair the structural law and traversal take, `Members` every node the edge involves (binary endpoints, a `Connect`'s realizing intermediary, and a `Generic` edge's `PropertyValue.References` attribute-buried ids) and `DirectedPairs` the directed adjacency it contributes (endpoints plus realizing, never the buried refs), `IsContainment` the spatial-containment predicate a Persistence spatial-structure query filters on; `Touches(nodeId)` tests `Members` membership for the incidence index and the `DropNode` cascade; `ToCanonicalBytes(tolerance)` projects the standalone edge bytes a content-3-way merge keys on under the model tolerance, the SAME projection `CanonicalBytes(writer)` composes into the graph content key — the tolerance threaded so a `Generic` edge's `PropertyValue.Measure` attributes quantize to `Header.Tolerance` exactly as the node measures do (the five typed cases carry no Measure and are tolerance-insensitive).
- Auto: ONE private `Ends` generated total `Switch` projects each case's relating/related pair plus the `Connect` realizing intermediary (the primary correspondence, DERIVED_LOGIC), and `Endpoints`/`Relating`/`Related`/`Members`/`DirectedPairs`/`Touches` all DERIVE from it — so the incidence index (`Graph/element#ELEMENT_GRAPH`) and the structural legality (`Graph/delta#GRAPH_DELTA`) read one accessor and the endpoint law is declared at exactly one dispatch site, never three parallel Switches restating the five binary pairs; `Kind` projects through the generated `Map` (case → precomputed constant row — no throwaway lambdas, the constant dual of the func-form `Switch` the allocating `Remap` takes); the neutral sub-kinds round-trip their token at the wire and drive the `Bake` descent (`Compose.Aggregate`/`Nest`/`Contain` descend into parts, `Assign.TypeDefinition` binds the type for inheritance, `Assign.PropertyDefinition`/`Assessment` attach the bag/receipt, `Associate` folds the material plus its usage); the `Assign.TypeDefinition` edge carries the named type→occurrence inheritance the `Bake` fold applies over the type's standardized data — single fields occurrence-overrides-type, `Seq` fields (materials/assessments/classifications) union + dedup-by-key — distinct from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the bag `Merge` owns, so the seam binds the type once through this one neutral row and the `Bake` realizes the full standardized inheritance; the `Generic` attribute map carries the IFC-specific fields the Bim projector preserved so a round-trip re-authors the original relation.
- Receipt: the `Relationship` is the typed edge a `GraphDelta` adds/removes and the `Bake` fold traverses; the `MaterialUsage` on an `Associate` edge is the occurrence geometric binding a host materializes (a layer set's direction, offset, and reference extent, a profile's cardinal point) so a wall and its mirror share one `LayerSet` composition with two `Associate` usages; the `Generic` passthrough is the round-trip guarantee — every IFC relationship the projector cannot map to a neutral case rides `Generic` so an import→export cycle drops nothing.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum<int>]`), LanguageExt.Core (`Map`/`Option`/`Fin`), `Rasm` (content-key seed + the `Op` op-key).
- Growth: a new graph-relationship semantic is one neutral case or one sub-kind row (a `Reference` compose flavor, a new `AssignKind`); a new IFC relationship maps onto an existing neutral case or rides `Generic`, never a new seam case; a new occurrence usage is one `MaterialUsage` arm; the algebra is closed at five neutral kinds plus the passthrough, the IFC roster living in the Bim projector.
- Boundary: `Relationship` is the NEUTRAL edge algebra — the rejected seventeen-typed-`IfcRel*`-case design is the deleted form, because it re-opened the IFC-schema strata the `Classification` collapse closed; the IFC names, directionality, inverse semantics, and the eight stranded families live in the `Rasm.Bim` `SemanticProjector`, mapping each `IfcRel*` onto a neutral case with its typed payload or the `Generic` passthrough, and the seam carries no `IfcRel*` spelling; the discrimination is two-level — the base `RelationshipKind Kind` is the neutral CASE token a topology/merge consumer routes on (a uniform read with NO union switch, the flat edge column Persistence persists), the per-case `SubKind` the NEUTRAL graph-semantic flavor the `Bake` reads (whole/part flavor, assignment flavor), neither the IFC roster, and a per-case `Kind` shadowing a base `Kind` is the deleted form; `Members`/`Touches` include a `Connect`'s realizing intermediary AND a `Generic` edge's buried attribute references (the `PropertyValue.References` dual of `Remap`) so a `DropNode` cascade never strands a live reference, while `DirectedPairs` stays endpoints-plus-realizing (a buried attribute reference is not a directed adjacency leg); the `Associate` edge carries the `MaterialUsage` occurrence payload — the type-level `Composition/material#MATERIAL_COMPOSITION` set carries the shared layer/profile structure, the edge the per-occurrence geometric binding — so usage never duplicates onto the composition; classification is a generic value ON the `Object` node, NOT an edge, so the seam carries no classification-association relationship; the `Generic` passthrough guarantees no relationship is dropped, so a round-trip through the seam preserves every edge.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
// The neutral CASE discriminant — which of the six edge kinds an edge is, the flat token a topology/merge
// consumer (Rasm.Persistence Query/Version) routes on through `edge.Kind` without switching on the union type.
// Distinct from the per-case SubKind flavor (ComposeKind/AssignKind/ConnectKind/VoidKind) the Bake fold reads.
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
 public static readonly ComposeKind Nest = new("nest"); // ordered child sequence
 public static readonly ComposeKind Contain = new("contain"); // spatial containment
 public static readonly ComposeKind Reference = new("reference"); // non-owning spatial reference
}

[SmartEnum<string>]
public sealed partial class AssignKind {
 public static readonly AssignKind PropertyDefinition = new("property-definition"); // attaches a PropertySet/QuantitySet bag
 public static readonly AssignKind TypeDefinition = new("type-definition"); // the occurrence→Type-Object bind the owning Component projection authors; the Bake fold resolves it into the named type→occurrence inheritance (single fields occurrence-overrides-type, Seq materials/assessments/classifications union+dedup-by-key), NEVER a parallel DefinesByType case — IfcRelDefinesByType rides THIS neutral row
 public static readonly AssignKind Group = new("group"); // group/system/zone membership
 public static readonly AssignKind Assessment = new("assessment"); // attaches an Assessment receipt
}

// The connection MEDIUM only — realization is the Connect case's own Option<NodeId> Realizing field, NOT a fourth
// row: a Realizing row would re-describe the field (MODAL_ARITY's rejected parallel discriminant) and contradict it
// (SubKind=Realizing with Realizing=None). Medium (element/path/port) and realization (direct/via-intermediate) are
// orthogonal axes: IfcRelConnectsWithRealizingElements subtypes the medium-less IfcRelConnectsElements base directly,
// so it rides Element with Realizing=Some — never a false "path" stamp and never a medium of its own.
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

public enum LayerSetDirection : byte { Axis1 = 0, Axis2 = 1, Axis3 = 2 }
public enum DirectionSense : byte { Positive = 0, Negative = 1 }

// The profile-placement reference grid — the GeometryGym IfcCardinalPointReference reciprocal, keyed on the
// IFC integer (1..9 bounding-box grid, 0 unset, 10..19 the centroidal/shear-axis references a non-rectangular
// section places on). A SmartEnum, not a guarded int, because the seam admits this value FROM a raw IFC integer
// (TryGet is the bridge); the LayerSetDirection/DirectionSense enums arrive already typed off the IFC enum, so they stay plain.
[SmartEnum<int>]
public sealed partial class CardinalPoint {
 public static readonly CardinalPoint Default = new(0);        // unset — GeometryGym falls back to Mid at egress
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
// The occurrence material usage the Associate edge carries — the per-occurrence geometric binding the type-level
// MaterialComposition set does not carry. CLASS-root [Union] + [Equatable] (the [GRAPH_FAMILY] form) so Associate.Usage
// is drillable: a changed OffsetFromReferenceLine surfaces as Edges[i].Usage.<member> in the 3-way merge, never a
// whole-edge replacement.
[Union]
[Equatable]
public abstract partial class MaterialUsage {
 private MaterialUsage() { }

 public sealed partial class None : MaterialUsage;

 // The IfcMaterialLayerSetUsage geometric binding the type-level LayerSet composition does not carry: stacking
 // Direction/Sense, the signed OffsetFromReferenceLine off the element reference line, and the ReferenceExtent a host
 // materializes an asymmetric buildup against. ReferenceExtent is bare coordinate-space placement (NaN when unset,
 // canonicalized at hashing) — the same raw-scalar shape as ProfileSet, never a dimensioned MeasureValue, never dropped.
 public sealed partial class LayerSet(LayerSetDirection direction, DirectionSense sense, double offsetFromReferenceLine, double referenceExtent) : MaterialUsage {
  public LayerSetDirection Direction { get; } = direction;
  public DirectionSense Sense { get; } = sense;
  public double OffsetFromReferenceLine { get; } = offsetFromReferenceLine;
  public double ReferenceExtent { get; } = referenceExtent;
 }

 // The profile occurrence usage. The private ctor forces every admission through Of so the CardinalPoint is always
 // a resolved grid position; ReferenceExtent is bare coordinate-space placement data (NaN when unset, canonicalized
 // at hashing) — a raw coordinate scalar, not a dimensioned MeasureValue.
 public sealed partial class ProfileSet : MaterialUsage {
  public CardinalPoint CardinalPoint { get; }
  public double ReferenceExtent { get; }

  private ProfileSet(CardinalPoint cardinalPoint, double referenceExtent) =>
   (CardinalPoint, ReferenceExtent) = (cardinalPoint, referenceExtent);

  public static Fin<MaterialUsage> Of(int cardinalPoint, double referenceExtent, Op key) =>
   CardinalPoint.Of(cardinalPoint, key).Map(point => (MaterialUsage)new ProfileSet(point, referenceExtent));

  // The infallible re-hydration escape the Persistence/Bim decoders reconstruct an already-validated edge through
  // off a resolved CardinalPoint (the persisted integer re-admitted through CardinalPoint before this seed).
  internal static ProfileSet Seed(CardinalPoint cardinalPoint, double referenceExtent) => new(cardinalPoint, referenceExtent);
 }

 // The canonical projection co-located on the usage owner (the PropertyValue/MaterialComposition shape): case
 // ordinal then the typed geometric payload, the CardinalPoint written as its grid-integer Key — the Associate
 // edge composes it through one call so the usage owns its own content contribution, never inlined per edge.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  none: _ => w.Ordinal(0),
  layerSet: u => w.Ordinal(1).Ordinal((int)u.Direction).Ordinal((int)u.Sense).Double(u.OffsetFromReferenceLine).Double(u.ReferenceExtent),
  profileSet: u => w.Ordinal(2).Ordinal(u.CardinalPoint.Key).Double(u.ReferenceExtent));
}

// A CLASS-root [Union] (the [GRAPH_FAMILY] form): a class root surrenders Thinktecture's record-generated equality,
// so structural equality AND the member-level diff ride Generator.Equals [Equatable] — never stacked on a record root.
// LOAD-BEARING: the ElementGraph [OrderedEquality] edge walk drills into a changed edge only when the edge type is
// itself [Equatable], so the Persistence 3-way StructuralMerge localizes to Edges[i].<member> (a changed
// Associate.Usage, a [UnorderedEquality] Generic.Attributes delta), not whole-edge replacement.
[Union]
[Equatable]
public abstract partial class Relationship {
 private Relationship() { }

 public sealed partial class Compose(NodeId whole, NodeId part, ComposeKind subKind) : Relationship { public NodeId Whole { get; } = whole; public NodeId Part { get; } = part; public ComposeKind SubKind { get; } = subKind; }
 public sealed partial class Assign(NodeId subject, NodeId definition, AssignKind subKind) : Relationship { public NodeId Subject { get; } = subject; public NodeId Definition { get; } = definition; public AssignKind SubKind { get; } = subKind; }
 public sealed partial class Associate(NodeId subject, NodeId resource, MaterialUsage usage) : Relationship { public NodeId Subject { get; } = subject; public NodeId Resource { get; } = resource; public MaterialUsage Usage { get; } = usage; }
 public sealed partial class Connect(NodeId from, NodeId to, ConnectKind subKind, Option<NodeId> realizing) : Relationship { public NodeId From { get; } = from; public NodeId To { get; } = to; public ConnectKind SubKind { get; } = subKind; public Option<NodeId> Realizing { get; } = realizing; }
 public sealed partial class Void(NodeId host, NodeId feature, VoidKind subKind) : Relationship { public NodeId Host { get; } = host; public NodeId Feature { get; } = feature; public VoidKind SubKind { get; } = subKind; }
 public sealed partial class Generic(string wireName, NodeId relating, NodeId related, Map<PropertyName, PropertyValue> attributes) : Relationship { public string WireName { get; } = wireName; public NodeId Relating { get; } = relating; public NodeId Related { get; } = related; [UnorderedEquality] public Map<PropertyName, PropertyValue> Attributes { get; } = attributes; }

 // The ONE primary case walk (DERIVED_LOGIC): each case's relating/related pair PLUS the Connect realizing
 // intermediary (None elsewhere) — Endpoints/Relating/Related/Members/DirectedPairs/Touches all DERIVE from this
 // triple, so the five binary cases state their endpoint law exactly once and a new case extends exactly one
 // dispatch site instead of three parallel Switches restating the same pairs.
 (NodeId Relating, NodeId Related, Option<NodeId> Realizing) Ends => Switch(
 compose: static r => (r.Whole, r.Part, Option<NodeId>.None),
 assign: static r => (r.Subject, r.Definition, Option<NodeId>.None),
 associate: static r => (r.Subject, r.Resource, Option<NodeId>.None),
 connect: static r => (r.From, r.To, r.Realizing),
 @void: static r => (r.Host, r.Feature, Option<NodeId>.None),
 generic: static r => (r.Relating, r.Related, Option<NodeId>.None));

 public (NodeId Relating, NodeId Related) Endpoints => Ends switch { var (relating, related, _) => (relating, related) };

 // The neutral CASE token a topology/merge consumer routes on without a union switch — the flat column Persistence
 // persists and groups by. Generated Map (case → PRECOMPUTED constant), NOT Switch: every arm is a singleton row,
 // no throwaway `static _ => Const` lambdas — the constant dual of Remap's allocating func-form below.
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
 // (Remap rewrites it, Members sweeps it) — never the asymmetry that stranded a dangling Reference on a surviving Generic
 // edge. DirectedPairs stays endpoints-plus-realizing (an attribute reference is not a directed adjacency leg).
 public Seq<NodeId> Members =>
  (Ends switch { var (relating, related, realizing) => Seq(relating, related) + realizing.ToSeq() })
  + (this is Generic g ? g.Attributes.Values.ToSeq().Bind(static v => v.References()) : Seq<NodeId>());

 // The directed adjacency the edge contributes to a topology view — one pair for a binary edge, the two
 // realizing legs (From→Realizing→To) for a Connect carrying a realizing intermediary.
 public Seq<(NodeId From, NodeId To)> DirectedPairs => Ends switch {
 var (relating, related, realizing) => realizing.Match(
  Some: via => Seq((relating, via), (via, related)),
  None: () => Seq((relating, related))),
 };

 // Spatial containment — the IfcRelContainedInSpatialStructure shape a Persistence spatial-structure query filters:
 // a Compose edge with the Contain flavor (the decomposition Aggregate/Nest and the non-owning Reference are NOT containment).
 public bool IsContainment => this is Compose { SubKind: var k } && k == ComposeKind.Contain;

 public bool Touches(NodeId node) => Members.Exists(m => m == node);

 // The standalone edge bytes a content-3-way merge keys on — the SAME projection CanonicalBytes composes into the
 // graph content key, threading the MODEL tolerance (graph.Header.Tolerance via ElementGraph, the delta key, and the
 // OfGraph edge sort). The five typed cases carry no Measure and are tolerance-insensitive (Associate ReferenceExtent
 // is a bare coordinate the writer canonicalizes, never quantizes); a Generic edge's PropertyValue.Measure attributes
 // quantize through w.Measure — the prior tolerance-0 hardcode forked Generic edge keys below Header.Tolerance.
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) { CanonicalWriter w = new(tolerance); CanonicalBytes(w); return w.ToBytes(); }

 // The canonical projection through the Projection/address#CONTENT_ADDRESS writer: case ordinal,
 // neutral sub-kind, endpoints, and the typed payload — the edge contributes to the graph content key.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
 compose: r => w.Ordinal(0).String(r.Whole.Value).String(r.Part.Value).String(r.SubKind.Key),
 assign: r => w.Ordinal(1).String(r.Subject.Value).String(r.Definition.Value).String(r.SubKind.Key),
 associate: r => { w.Ordinal(2).String(r.Subject.Value).String(r.Resource.Value); r.Usage.CanonicalBytes(w); return w; },
 connect: r => { w.Ordinal(3).String(r.From.Value).String(r.To.Value).String(r.SubKind.Key).Bool(r.Realizing.IsSome); r.Realizing.IfSome(n => w.String(n.Value)); return w; },
 @void: r => w.Ordinal(4).String(r.Host.Value).String(r.Feature.Value).String(r.SubKind.Key),
 generic: r => { w.Ordinal(5).String(r.WireName).String(r.Relating.Value).String(r.Related.Value).Ordinal(r.Attributes.Count); foreach (var (n, v) in r.Attributes.OrderBy(static p => p.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); v.CanonicalBytes(w); } return w; });

 // Re-maps EVERY NodeId the edge carries (an unmapped id passes through unchanged): endpoints, a Connect's realizing
 // intermediary, AND every kernel reference buried in a Generic attribute — the Generic arm composes the ONE recursive
 // PropertyValue.Remap owner, never a verbatim Attributes pass that dangles a Reference (the deleted endpoints-only
 // rewrite). Class-root union cases have no `with`, so each arm reconstructs through the func-form Switch (Map carries
 // only precomputed constants); exhaustive over the closed six-case algebra — a new case breaks the rewrite at compile time.
 public Relationship Remap(Func<NodeId, NodeId> map) => Switch<Relationship>(
  compose: r => new Compose(map(r.Whole), map(r.Part), r.SubKind),
  assign: r => new Assign(map(r.Subject), map(r.Definition), r.SubKind),
  associate: r => new Associate(map(r.Subject), map(r.Resource), r.Usage),
  connect: r => new Connect(map(r.From), map(r.To), r.SubKind, r.Realizing.Map(map)),
  @void: r => new Void(map(r.Host), map(r.Feature), r.SubKind),
  generic: r => new Generic(r.WireName, map(r.Relating), map(r.Related), r.Attributes.Map((_, v) => v.Remap(map))));
}
```

## [03]-[RESEARCH]

- [NEUTRAL_EDGE_ALGEBRA]: the seam rejects the seventeen-typed-`IfcRel*`-case relationship union because it re-opens the IFC-schema strata leak the `Classification` collapse fixed — the seam carries a neutral five-kind algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void`) plus a `Generic` passthrough, and the `Rasm.Bim` `SemanticProjector` maps every `IfcRel*` onto a neutral case (with its typed payload) or rides `Generic(wireName, relating, related, attrs)` so nothing is dropped; the eight previously-stranded families (`ConnectsStructuralMember`/`Activity`, `ConnectsPortToElement`, `SpaceBoundary` 1st/2nd, `Covers*`, `ConnectsPathElements` wall-join priorities, `Declares`, `AssignsToControl`/`Process`/`Product`/`Actor`, `InterferesElements`) all ride the neutral payload or `Generic`, the IFC directionality and inverse semantics living wholly in the projector; the `Connect` medium vocabulary is three-row — `Element` the medium-less concrete `IfcRelConnectsElements` base (and its direct `IfcRelConnectsWithRealizingElements` subtype, whose realizing head rides `Connect.Realizing`, never a sub-kind row), `Path` the path-element join, `Port` the port join — so a bare element adjacency never force-stamps `path` and a realizing edge never mints a parallel medium — verified against the GeometryGym `IfcRelationship` subtype surface before the projector mapping is final.
- [OCCURRENCE_USAGE]: the occurrence material usage rides the `Associate` edge — the IFC `IfcMaterialLayerSetUsage` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`/`ReferenceExtent`) and `IfcMaterialProfileSetUsage` (`CardinalPoint`/`ReferenceExtent`) are the per-occurrence geometric binding, distinct from the type-level `IfcMaterialLayerSet`/`IfcMaterialProfileSet` the `Composition/material#MATERIAL_COMPOSITION` set carries — so two occurrences sharing one layer set carry two `Associate` edges with two `MaterialUsage` payloads, the composition shared once; the `LayerSet` usage's `ReferenceExtent` (the layer-set size perpendicular to the layers a host materializes the buildup against when the reference line does not bisect it) rides the case as a bare `NaN`-when-unset scalar the way the `ProfileSet` usage's does, so an asymmetric wall finish is never dropped at ingest; the neutral `LayerSetDirection`/`DirectionSense` enums and the `CardinalPoint` `[SmartEnum<int>]` carry the geometric parameters host-neutral, the `ProfileSet.Of` factory admitting the raw IFC integer reference through `CardinalPoint.Of` — the `TryGet` bridge over the GeometryGym `IfcCardinalPointReference` 0..19 reference grid (the bounding-box `1..9`, the unset `0`, the centroidal/shear references `10..19` a non-rectangular section places on), railing `ElementFault.ValueRejected` on an out-of-grid integer and forcing every admission through `Of` via the private ctor — so a downstream `IfcMaterialProfileSetUsage` only ever receives a resolved grid position, the host materializing the layer offsets and the profile placement from the usage at the app root.
- [UNIFORM_ACCESSOR_SURFACE]: the union carries a uniform consumer surface so a `Rasm.Persistence` `Query` topology pass and a `Version` 3-way `StructuralMerge` read an edge WITHOUT a union switch — `Kind` (the neutral `RelationshipKind` case token persisted as a flat edge column and grouped on in merge, projected through the generated `Map` because every arm is a precomputed constant row), `Relating`/`Related`/`Endpoints`/`DirectedPairs` (the endpoint and adjacency projections a topology view reads, ALL derived from the one private `Ends` case walk — the primary relating/related/realizing correspondence declared once, so the accessor family cannot drift case-by-case) and `Members` (that `Ends`-derived reachability set UNIONED with a `Generic` edge's `PropertyValue.References` attribute-buried ids the orphan/cascade sweep and the incidence index read), `IsContainment` (the spatial-structure predicate), and `ToCanonicalBytes(tolerance)` (the standalone edge content key a content-3-way merge diffs on under the model tolerance, the SAME projection `CanonicalBytes(writer)` composes into the graph address — the `Generic` arm's measure attributes quantizing to `Header.Tolerance`); the two-level discrimination keeps `Kind` (case) on the base and `SubKind` (flavor) on each case, so the consumer's `RelationshipKind`/`.Kind`/`.IsContainment` shape resolves to the seam's actual union rather than re-deriving a parallel discriminant.
