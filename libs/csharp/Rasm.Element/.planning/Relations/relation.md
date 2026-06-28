# [ELEMENT_RELATION]

The neutral objectified-edge algebra: one `Relationship` `[Union]` over FIVE library-neutral edge kinds — `Compose` (whole/part decomposition), `Assign` (subject↔definition binding), `Associate` (subject↔material binding with occurrence usage), `Connect` (element↔element connectivity), `Void` (host↔feature opening) — plus a `Generic` passthrough carrying a wire name, the endpoints, and an attribute map so NO relationship is ever dropped. This REPLACES the rejected seventeen-typed-`IfcRel*`-case design: the seam carries a neutral edge algebra a graph reasons over, while ALL IFC relationship names, directionality, inverse semantics, and the long-tail families (`ConnectsStructuralMember`/`ConnectsPortToElement`, `SpaceBoundary` 1st/2nd level, `Covers*`, `ConnectsPathElements` wall-join priorities, `Declares`, `AssignsToControl`/`Process`/`Product`/`Actor`, `InterferesElements`) live in the `Rasm.Bim` `SemanticProjector`, riding the typed payload of a neutral case or the `Generic` passthrough — so the seam never re-opens the IFC-schema strata the `Classification` collapse already closed. Each edge carries typed `NodeId` endpoints and a small NEUTRAL sub-kind discriminant the `Bake` fold dispatches on (a `Compose.Aggregate` descends into parts, an `Assign.TypeDefinition` resolves the type's property bags for inheritance, an `Assign.Assessment` attaches a receipt), never the IFC roster. The `Associate` (material) edge carries a typed `MaterialUsage` occurrence payload: a `LayerSetUsage` (direction/sense/offset) or a `ProfileSetUsage` (cardinal-point/extent), the per-occurrence geometric binding the type-level `Composition/material#MATERIAL_COMPOSITION` set does not carry. Classification is NOT an edge — it is a generic value ON the `Object` node, so the seam carries no classification-association relationship. The page composes `Graph/element#NODE_MODEL` `NodeId` for endpoints and `Properties/property#PROPERTY_VALUE` for the `Generic` attribute map; an endpoint-kind violation rails `Projection/fault#FAULT_BAND` `ElementFault.RelationshipInvalid` at `Graph/delta#GRAPH_DELTA` `Apply`.

## [01]-[INDEX]

- [01]-[EDGE_ALGEBRA]: the `Relationship` `[Union]` five-neutral-case edge algebra plus the `Generic` passthrough, the `RelationshipKind` neutral CASE discriminant and the per-case `SubKind` vocabularies, the uniform consumer accessor surface (`Kind`/`Relating`/`Related`/`Members`/`DirectedPairs`/`IsContainment`/`Endpoints`/`Touches`/`ToCanonicalBytes()`), and the `MaterialUsage` occurrence payload the `Associate` edge carries.

## [02]-[EDGE_ALGEBRA]

- Owner: `Relationship` the `[Union]` neutral objectified-edge algebra carrying the uniform consumer accessor surface (`Kind`/`Relating`/`Related`/`Members`/`DirectedPairs`/`IsContainment`/`Endpoints`/`Touches`/`ToCanonicalBytes()`); `RelationshipKind` the `[SmartEnum<string>]` neutral CASE discriminant a topology/merge consumer routes on; the `ComposeKind`/`AssignKind`/`ConnectKind`/`VoidKind` `[SmartEnum<string>]` per-case `SubKind` vocabularies the `Bake` fold dispatches on; `MaterialUsage` the `[Union]` occurrence-usage payload the `Associate` edge carries; the `LayerSetDirection`/`DirectionSense`/`CardinalPoint` neutral geometric enums.
- Cases: `Compose` (a `Whole`→`Part` decomposition with a `ComposeKind` flavor — aggregate/nest/contain/reference) · `Assign` (a `Subject`→`Definition` binding with an `AssignKind` flavor — property-definition/type-definition/group/assessment) · `Associate` (a `Subject`→`Resource` binding — a `Composition/material#MATERIAL_NODE` material node carrying a `MaterialUsage` occurrence payload, or an `Appearance` presentation node carrying `MaterialUsage.None` — the IFC `IfcRelAssociates` base) · `Connect` (a `From`↔`To` connectivity with a `ConnectKind` flavor and an optional realizing node) · `Void` (a `Host`→`Feature` opening with a `VoidKind` flavor — void/fill) · `Generic` (a `WireName` + endpoints + attribute map passthrough so no IFC relationship is dropped); the closed five-kind algebra plus the passthrough.
- Entry: the case constructors are the typed edge admissions (`new Relationship.Compose(whole, part, ComposeKind.Aggregate)`, `new Relationship.Associate(subject, material, usage)`); `Kind` projects the neutral `RelationshipKind` case token a consumer routes on, `Relating`/`Related` the endpoint reads and `Endpoints` the `(Relating, Related)` pair the structural law and traversal take, `Members` every node the edge involves (binary endpoints plus a `Connect`'s realizing intermediary) and `DirectedPairs` the directed adjacency it contributes, `IsContainment` the spatial-containment predicate a Persistence spatial-structure query filters on; `Touches(nodeId)` tests `Members` membership for the incidence index and the `DropNode` cascade; `ToCanonicalBytes()` projects the standalone edge bytes a content-3-way merge keys on, the SAME projection `CanonicalBytes(writer)` composes into the graph content key.
- Auto: `Endpoints` dispatches the generated total `Switch` projecting each case's relating/related pair (a `Compose` whole→part, an `Assign` subject→definition, a `Generic` its endpoints) so the incidence index (`Graph/element#ELEMENT_GRAPH`) and the structural legality (`Graph/delta#GRAPH_DELTA`) read one accessor; the neutral sub-kinds round-trip their token at the wire and drive the `Bake` descent (`Compose.Aggregate`/`Nest`/`Contain` descend into parts, `Assign.TypeDefinition` resolves the type bags, `Assign.PropertyDefinition`/`Assessment` attach the bag/receipt, `Associate` folds the material plus its usage); the `Generic` attribute map carries the IFC-specific fields the Bim projector preserved so a round-trip re-authors the original relation.
- Receipt: the `Relationship` is the typed edge a `GraphDelta` adds/removes and the `Bake` fold traverses; the `MaterialUsage` on an `Associate` edge is the occurrence geometric binding a host materializes (a layer set's direction and offset, a profile's cardinal point) so a wall and its mirror share one `LayerSet` composition with two `Associate` usages; the `Generic` passthrough is the round-trip guarantee — every IFC relationship the projector cannot map to a neutral case rides `Generic` so an import→export cycle drops nothing.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`), LanguageExt.Core (`Map`/`Option`), `Rasm` (content-key seed).
- Growth: a new graph-relationship semantic is one neutral case or one sub-kind row (a `Reference` compose flavor, a new `AssignKind`); a new IFC relationship maps onto an existing neutral case or rides `Generic`, never a new seam case; a new occurrence usage is one `MaterialUsage` arm; the algebra is closed at five neutral kinds plus the passthrough, the IFC roster living in the Bim projector.
- Boundary: `Relationship` is the NEUTRAL edge algebra — the rejected seventeen-typed-`IfcRel*`-case design is the deleted form, because it re-opened the IFC-schema strata the `Classification` collapse closed; the IFC names, directionality, inverse semantics, and the eight stranded families live in the `Rasm.Bim` `SemanticProjector`, mapping each `IfcRel*` onto a neutral case with its typed payload or the `Generic` passthrough, and the seam carries no `IfcRel*` spelling; the discrimination is two-level — the base `RelationshipKind Kind` is the neutral CASE token a topology/merge consumer routes on (a uniform read with NO union switch, the flat edge column Persistence persists), the per-case `SubKind` the NEUTRAL graph-semantic flavor the `Bake` reads (whole/part flavor, assignment flavor), neither the IFC roster, and a per-case `Kind` shadowing a base `Kind` is the deleted form; `Members`/`Touches`/`DirectedPairs` include a `Connect`'s realizing intermediary so a `DropNode` cascade never strands a realizing reference; the `Associate` edge carries the `MaterialUsage` occurrence payload — the type-level `Composition/material#MATERIAL_COMPOSITION` set carries the shared layer/profile structure, the edge the per-occurrence geometric binding — so usage never duplicates onto the composition; classification is a generic value ON the `Object` node, NOT an edge, so the seam carries no classification-association relationship; the `Generic` passthrough guarantees no relationship is dropped, so a round-trip through the seam preserves every edge.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
 public static readonly AssignKind TypeDefinition = new("type-definition"); // binds the type object for inheritance
 public static readonly AssignKind Group = new("group"); // group/system/zone membership
 public static readonly AssignKind Assessment = new("assessment"); // attaches an Assessment receipt
}

[SmartEnum<string>]
public sealed partial class ConnectKind {
 public static readonly ConnectKind Path = new("path"); // path-element connectivity (walls, members)
 public static readonly ConnectKind Port = new("port"); // MEP port connectivity
 public static readonly ConnectKind Realizing = new("realizing"); // connection realized by an intermediate element
}

[SmartEnum<string>]
public sealed partial class VoidKind {
 public static readonly VoidKind Void = new("void"); // host carved by a feature (opening, recess)
 public static readonly VoidKind Fill = new("fill"); // feature filled by an element (door in opening)
}

public enum LayerSetDirection : byte { Axis1 = 0, Axis2 = 1, Axis3 = 2 }
public enum DirectionSense : byte { Positive = 0, Negative = 1 }

// --- [MODELS] -----------------------------------------------------------------------------
// The occurrence material usage the Associate edge carries: the per-occurrence geometric
// binding the type-level MaterialComposition set does not carry.
[Union]
public abstract partial record MaterialUsage {
 private MaterialUsage() { }

 public sealed record None : MaterialUsage;
 public sealed record LayerSet(LayerSetDirection Direction, DirectionSense Sense, double OffsetFromReferenceLine) : MaterialUsage;

 // The profile occurrence usage. CardinalPoint is constrained to the IFC IfcCardinalPointReference 1..9
 // reference grid at admission, so a downstream IfcMaterialProfileSetUsage only ever receives a typed,
 // in-range point — the private ctor forces every admission through Of (Acoustic.Of/CoverageGrid.Of shape).
 public sealed record ProfileSet : MaterialUsage {
  public int CardinalPoint { get; }
  public double ReferenceExtent { get; }

  private ProfileSet(int cardinalPoint, double referenceExtent) =>
   (CardinalPoint, ReferenceExtent) = (cardinalPoint, referenceExtent);

  public static Fin<MaterialUsage> Of(int cardinalPoint, double referenceExtent, Op key) =>
   cardinalPoint is < 1 or > 9
    ? ElementFault.ValueRejected(key, $"<profile-set-cardinal-point-out-of-range:{cardinalPoint}>")
    : Fin.Succ<MaterialUsage>(new ProfileSet(cardinalPoint, referenceExtent));

  // The infallible re-hydration escape the Persistence/Bim decoders reconstruct an already-validated edge through.
  internal static ProfileSet Seed(int cardinalPoint, double referenceExtent) => new(cardinalPoint, referenceExtent);
 }
}

[Union]
public abstract partial record Relationship {
 private Relationship() { }

 public sealed record Compose(NodeId Whole, NodeId Part, ComposeKind SubKind) : Relationship;
 public sealed record Assign(NodeId Subject, NodeId Definition, AssignKind SubKind) : Relationship;
 public sealed record Associate(NodeId Subject, NodeId Resource, MaterialUsage Usage) : Relationship;
 public sealed record Connect(NodeId From, NodeId To, ConnectKind SubKind, Option<NodeId> Realizing) : Relationship;
 public sealed record Void(NodeId Host, NodeId Feature, VoidKind SubKind) : Relationship;
 public sealed record Generic(string WireName, NodeId Relating, NodeId Related, Map<PropertyName, PropertyValue> Attributes) : Relationship;

 public (NodeId Relating, NodeId Related) Endpoints => Switch(
 compose: static r => (r.Whole, r.Part),
 assign: static r => (r.Subject, r.Definition),
 associate: static r => (r.Subject, r.Resource),
 connect: static r => (r.From, r.To),
 @void: static r => (r.Host, r.Feature),
 generic: static r => (r.Relating, r.Related));

 // The neutral CASE discriminant a topology/merge consumer routes on without switching on the union type —
 // the flat token Rasm.Persistence persists as an edge column and groups by in the 3-way StructuralMerge.
 public RelationshipKind Kind => Switch(
 compose: static _ => RelationshipKind.Compose,
 assign: static _ => RelationshipKind.Assign,
 associate: static _ => RelationshipKind.Associate,
 connect: static _ => RelationshipKind.Connect,
 @void: static _ => RelationshipKind.Void,
 generic: static _ => RelationshipKind.Generic);

 // Endpoint reads a consumer takes without destructuring the Endpoints pair.
 public NodeId Relating => Endpoints.Relating;
 public NodeId Related => Endpoints.Related;

 // Every distinct node the edge involves — the binary endpoints plus a Connect's realizing intermediary —
 // the cascade/orphan reachability set DropNode and the merge read (so a dropped realizing node cascades its edge).
 public Seq<NodeId> Members => Switch(
 compose: static r => Seq(r.Whole, r.Part),
 assign: static r => Seq(r.Subject, r.Definition),
 associate: static r => Seq(r.Subject, r.Resource),
 connect: static r => r.Realizing.Match(Some: n => Seq(r.From, r.To, n), None: () => Seq(r.From, r.To)),
 @void: static r => Seq(r.Host, r.Feature),
 generic: static r => Seq(r.Relating, r.Related));

 // The directed adjacency the edge contributes to a topology view — one pair for a binary edge, the two
 // realizing legs (From→Realizing→To) for a Connect carrying a realizing intermediary.
 public Seq<(NodeId From, NodeId To)> DirectedPairs => Switch(
 compose: static r => Seq((r.Whole, r.Part)),
 assign: static r => Seq((r.Subject, r.Definition)),
 associate: static r => Seq((r.Subject, r.Resource)),
 connect: static r => r.Realizing.Match(Some: n => Seq((r.From, n), (n, r.To)), None: () => Seq((r.From, r.To))),
 @void: static r => Seq((r.Host, r.Feature)),
 generic: static r => Seq((r.Relating, r.Related)));

 // Spatial containment — the IfcRelContainedInSpatialStructure shape a Persistence spatial-structure query filters:
 // a Compose edge with the Contain flavor (the decomposition Aggregate/Nest and the non-owning Reference are NOT containment).
 public bool IsContainment => this is Compose { SubKind: var k } && k == ComposeKind.Contain;

 public bool Touches(NodeId node) => Members.Exists(m => m == node);

 // The standalone canonical bytes a content-3-way merge keys an edge on — the SAME projection the graph content
 // address composes through CanonicalBytes (edges carry no tolerance-bearing measure, so the writer tolerance is 0).
 public ReadOnlyMemory<byte> ToCanonicalBytes() { CanonicalWriter w = new(0.0); CanonicalBytes(w); return w.ToBytes(); }

 // The canonical projection through the Projection/address#CONTENT_ADDRESS writer: case ordinal,
 // neutral sub-kind, endpoints, and the typed payload — the edge contributes to the graph content key.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
 compose: r => w.Ordinal(0).String(r.Whole.Value).String(r.Part.Value).String(r.SubKind.Key),
 assign: r => w.Ordinal(1).String(r.Subject.Value).String(r.Definition.Value).String(r.SubKind.Key),
 associate: r => { w.Ordinal(2).String(r.Subject.Value).String(r.Resource.Value); r.Usage.Switch(
 none: _ => w.Ordinal(0),
 layerSet: u => w.Ordinal(1).Ordinal((int)u.Direction).Ordinal((int)u.Sense).Double(u.OffsetFromReferenceLine),
 profileSet: u => w.Ordinal(2).Ordinal(u.CardinalPoint).Double(u.ReferenceExtent)); return w; },
 connect: r => { w.Ordinal(3).String(r.From.Value).String(r.To.Value).String(r.SubKind.Key).Bool(r.Realizing.IsSome); r.Realizing.IfSome(n => w.String(n.Value)); return w; },
 @void: r => w.Ordinal(4).String(r.Host.Value).String(r.Feature.Value).String(r.SubKind.Key),
 generic: r => { w.Ordinal(5).String(r.WireName).String(r.Relating.Value).String(r.Related.Value); foreach (var (n, v) in r.Attributes.OrderBy(static p => p.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); v.CanonicalBytes(w); } return w; });
}
```

## [03]-[RESEARCH]

- [NEUTRAL_EDGE_ALGEBRA]: the seam rejects the seventeen-typed-`IfcRel*`-case relationship union because it re-opens the IFC-schema strata leak the `Classification` collapse fixed — the seam carries a neutral five-kind algebra (`Compose`/`Assign`/`Associate`/`Connect`/`Void`) plus a `Generic` passthrough, and the `Rasm.Bim` `SemanticProjector` maps every `IfcRel*` onto a neutral case (with its typed payload) or rides `Generic(wireName, relating, related, attrs)` so nothing is dropped; the eight previously-stranded families (`ConnectsStructuralMember`/`Activity`, `ConnectsPortToElement`, `SpaceBoundary` 1st/2nd, `Covers*`, `ConnectsPathElements` wall-join priorities, `Declares`, `AssignsToControl`/`Process`/`Product`/`Actor`, `InterferesElements`) all ride the neutral payload or `Generic`, the IFC directionality and inverse semantics living wholly in the projector — verified against the GeometryGym `IfcRelationship` subtype surface before the projector mapping is final.
- [OCCURRENCE_USAGE]: the occurrence material usage rides the `Associate` edge — the IFC `IfcMaterialLayerSetUsage` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`) and `IfcMaterialProfileSetUsage` (`CardinalPoint`/`ReferenceExtent`) are the per-occurrence geometric binding, distinct from the type-level `IfcMaterialLayerSet`/`IfcMaterialProfileSet` the `Composition/material#MATERIAL_COMPOSITION` set carries — so two occurrences sharing one layer set carry two `Associate` edges with two `MaterialUsage` payloads, the composition shared once; the neutral `LayerSetDirection`/`DirectionSense` enums and the `CardinalPoint` carry the geometric parameters host-neutral, the `ProfileSet.Of` factory guarding the cardinal point to the IFC `IfcCardinalPointReference` 1..9 grid (railing `ElementFault.ValueRejected` and forcing every admission through `Of` via the private ctor) so a downstream `IfcMaterialProfileSetUsage` only ever receives an in-range point, the host materializing the layer offsets and the profile placement from the usage at the app root.
- [UNIFORM_ACCESSOR_SURFACE]: the union carries a uniform consumer surface so a `Rasm.Persistence` `Query` topology pass and a `Version` 3-way `StructuralMerge` read an edge WITHOUT a union switch — `Kind` (the neutral `RelationshipKind` case token persisted as a flat edge column and grouped on in merge), `Relating`/`Related`/`Endpoints`/`Members`/`DirectedPairs` (the endpoint and adjacency projections a topology view and an orphan/cascade sweep read), `IsContainment` (the spatial-structure predicate), and `ToCanonicalBytes()` (the standalone edge content key a content-3-way merge diffs on, the SAME projection `CanonicalBytes(writer)` composes into the graph address); the two-level discrimination keeps `Kind` (case) on the base and `SubKind` (flavor) on each case, so the consumer's `RelationshipKind`/`.Kind`/`.IsContainment` shape resolves to the seam's actual union rather than re-deriving a parallel discriminant.
