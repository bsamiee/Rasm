# [ELEMENT_GRAPH]

`ElementGraph` IS the authoritative thing — `Header` + `Nodes: FrozenDictionary<NodeId, Node>` + `Edges: ImmutableArray<Relationship>` + a built-once incidence index — and the consumer-facing `Element` DERIVES as the FOLD `Bake(objectNode)` over the reachable subgraph, never a second stored record. `Bake` lands material, property/quantity bags, assessments, observation series, appearance, coverages, composed parts, and the inherited `Component` (`Element.Type`/`Element.TypeId`) as flat fields a consumer reads in one hop — "has it all" is one fold, not a join across ten owners.

`ElementGraph` mirrors IFC as a property graph: every IFC entity is a `Node`, every IFC relationship a `Relations/relation#EDGE_ALGEBRA` `Relationship`, and the consumer reads neither — it reads the baked `Element`. `NodeId` OWNS identity over one regime: an OCCURRENCE `Object` carries Guid-v7 placement identity (the compressed IFC GlobalId is a Bim-stored attribute re-emitted at `Emit`), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its volatile-excluded canonical seed, and a non-rooted node a content hash over its full canonical bytes — never a second identity scheme.

PHASE splits the graph: live authoring rides the `TrackingHashMap` HAMT `Graph/delta#GRAPH_DELTA` owns, and `ElementGraph` is the FROZEN read snapshot — `ToFrozenDictionary`, the incidence index, the memoized `Bake`, and the `QuikGraph` view, all built once at the freeze boundary. `Projection/fault#FAULT_BAND` `ElementFault` carries every missing node and every structural violation.

## [01]-[INDEX]

- [02]-[NODE_MODEL]: `NodeId.Of(NodeSeed)` one-regime identity over the `NodeSeed` mint-regime witness — Guid-v7 placement, deterministic type seed, streamed content hash, precomputed wrap — the `Node` `[Union]` property-graph vocabulary with its shared `CanonicalBytes` projection and `Seed(tolerance)` witness, the node-payload component types, and the `RepresentationSlot`-rowed analytical-geometry decode behind the `GeometrySource` port.
- [03]-[ELEMENT_GRAPH]: `ElementGraph` frozen read snapshot with its built-once incidence index and `QuikGraph` view, the memoized `Bake` fold deriving `Element` under the named type→occurrence inheritance, and the incidence accessor family — sections, materials, containment, groups, measured-series rollups.
- [04]-[FEDERATION]: `Federate` unions N tagged source graphs onto one coordination `Header` with collision discrimination and a `FederationCensus`; `Extract` slices the downward closure of a root set into its own graph under the SOURCE `Header`.

## [02]-[NODE_MODEL]

- Owner: `NodeId` the `[ValueObject<string>]` identity owner over the `IObjectFactory` floor, minting through the ONE `Of(NodeSeed)` dispatch; `NodeSeed` the `[Union]` mint-regime witness (`Placement`/`TypeSeed`/`Content`/`Precomputed`) `Node.Seed(tolerance)` publishes and the address `Verify` dual reads; `Node` the `[Union]` eight-case property-graph vocabulary carrying the shared `CanonicalBytes` writer projection; the node-payload component types the cases compose.
- Cases: the closed eight-case property-graph node family — `Object` · `Material` · `PropertySet` · `QuantitySet` · `Assessment` · `Appearance` · `Coverage` · `Observation`.
- Cases: `Object` is the IfcObjectDefinition mirror: `ObjectKind` occurrence/type, optional `ExternalId` (the Bim-stored IFC GlobalId, re-emitted at `Emit`), first-class `PredefinedType` token value-object, name/tag, optional `OwnerHistory`, schema `SchemaSpan`, and NO `GeoReference` (model georeferencing is a `Header` fact).
- Cases: `Object` carries TWO classification columns — the primary `Classification` (the entity-class-keying pair every query, egress, and diff reads) beside the `Classifications` set of additional standard-system references, because IFC permits MULTIPLE `IfcRelAssociatesClassification` per object (Uniclass and OmniClass simultaneously) and a single field is lossy.
- Cases: `Object`'s `RepresentationContentHash` keyed map content-hashes EVERY geometry — the heavy display `Body` AND the lightweight analytical `Axis` (idealized structural line) and `FootPrint` (space-boundary surface polygon) a discipline resolves by content key — never inline coordinates.
- Cases: `Material` carries a `Composition/material#MATERIAL_COMPOSITION` `MaterialId` with its composition and property sets; `PropertySet`/`QuantitySet` a `Properties/property#PROPERTY_BAG` named bag with its `InheritanceMode`; `Assessment` an `Assessment/assessment#ASSESSMENT_NODE` payload; `Appearance` a content-keyed `AppearanceSummary`; `Coverage` a `Geospatial/coverage#COVERAGE_NODE` raster/field grid.
- Cases: `Observation` carries an `Assessment/observation#OBSERVATION_SERIES` measured sensor series — the computed assessment's sibling evidence modality, its samples content-keyed by reference.
- Entry: `NodeId.Of(NodeSeed)` is the ONE mint over the regime witness — `Placement` a sortable Guid-v7 for an OCCURRENCE `Object`; `TypeSeed(component, tolerance)` the deterministic Type id STREAMED over the volatile-excluded `WriteIdentity` segments through the one tolerance-bound `ContentAddress` entry, so identical `Component`s dedup to one Type with no byte materialization; `Content(node, tolerance)` the non-rooted content hash streamed over `node.CanonicalBytes(w)`; `Precomputed(address)` the no-rehash wrap ONLY for a node's OWN content self-hash, never a foreign key like an `Assessment.InputKey` (a payload field the canonical fold treats as content); `node.Id` reads any case's id through the abstract override; `node.CanonicalBytes(w)` projects the case's semantic content (NO id) into the writer the id mint, the `Projection/address#CONTENT_ADDRESS` diff, and the delta fold SHARE; `node.Seed(tolerance)` publishes the witness `Verify` dispatches on.
- Auto: each case carries `NodeId Id` as a positional override of the union's abstract `Id`, so `node.Id` reads without a switch; `CanonicalBytes` dispatches the generated total `Switch` writing each case's semantic content (an `Object` its kind/classification/predefined/name/tag/representations/span; a `Material` its key/composition/properties; a bag its set name, inheritance key, and count-prefixed sorted name→value entries, a quantity bag its count-prefixed sorted `GroupIdentity` run beside them; a measure quantized to the tolerance) into the kernel `CanonicalWriter` under the `Projection/address#IMPLEMENTATION_LAW` codec, the id excluded so a non-rooted node's id derives from its own bytes without circularity; a rooted `Object` mints its id once at authoring — an OCCURRENCE its Guid-v7 placement identity, a TYPE its DETERMINISTIC mint over the `WriteIdentity` segments (the volatile `Representations` AND secondary `Classifications` excluded so a later geometry attach or standard-classification stamp never re-keys the Type and identical `Component`s dedup to one Type) — the IFC GlobalId staying a Bim-stored projection attribute re-emitted at `Emit`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`IObjectFactory`), LanguageExt.Core (`Option`/`Seq`/`Map`), NodaTime (`Instant`). `Rasm.Element.Graph` OWNS the neutral `Vector3` the `AxisCurve`/`FootprintPolygon` analytical shapes carry (the kernel `Rasm.Numerics` coordinate is the host `Vector3d` the contract Boundary forbids; no neutral kernel triple exists), so the contract mints its own host-free coordinate AND its full vector algebra (`Length`/`Distance`/`Dot`/`Cross`/`Unit` + the `UnitX`/`UnitY`/`UnitZ`/`Zero` constants + the `+`/`-`/`*` operators) — the `Rasm.Bim` scan-to-BIM orientation classifier (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) and the `Rasm.Compute` structural load-vector folds compose THIS one coordinate rather than a kernel/host vector, so a phantom kernel `Vector3` or a `System.Numerics.Vector3` crossing the analytical-shape math is the deleted host leak.
- Growth: a new node concept is one `Node` case carrying its payload type, the payload owning its own `CanonicalBytes` contribution so the arm is one ordinal and one delegation (the `Observation` series landed exactly this way; a `Schedule`/`Task` node lands here only if 4D becomes a real target); a new object axis is one column on the `Object` case; a new node-payload component is one type on its owning sibling page; never a parallel node family and never a second identity scheme — the `NodeId` is the one owner, `MaterialId` a node attribute, not a parallel key. New object columns land with their wire field and their presence-delimited `CanonicalBytes` contribution in one edit under the additive contract-evolution law — `ObjectType` is the landed instance, carrying the IFC-canonical `(PredefinedType = USERDEFINED, ObjectType = label)` designation for BOTH object kinds — the Bim `Projection/semantic` `UserLabel` ingress reads it off `IfcObject.ObjectType` or `IfcElementType.ElementType` and `Projection/egress` `StampPredefined` re-stamps the matching slot, one column for the exact round-trip.
- Boundary: `NodeId` is the ONE identity owner: occurrence roots use Guid-v7 placement identity, type roots hash the representation-excluded type seed, and non-rooted nodes hash full canonical content. `Object` carries the primary and co-applied classifications, `PredefinedType`, content-keyed representations, owner history, and schema span; geometry stays behind `GeometrySource`, model georeferencing stays on `Header`, and IFC rosters stay in the Bim projector. `CanonicalBytes(w)` is the shared id/diff projection, and bag source rank participates in property/quantity node identity. `AppearanceSummary` is FROZEN at its seven-value preimage, carried as ONE `AppearanceVector` admitted through `Of(vector) -> Fin<AppearanceSummary>`: a peer carrying a richer appearance fact — a baked texture-set key, an environment binding, a UV transform, a measured refractive index — hangs it behind the `AppearanceKey` on its own wire, because an eighth column re-keys every stored `Node.Appearance` and forks the Bim dedup key in the same edit.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Graph;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim().ToUpperInvariant();
        validationError = value.Length == 32
            && UInt128.TryParse(value, System.Globalization.NumberStyles.AllowHexSpecifier,
                System.Globalization.CultureInfo.InvariantCulture, out _)
            ? validationError
            : new ValidationError("NodeId requires thirty-two hexadecimal digits");
    }

    public static NodeId Of(NodeSeed seed) => seed.Switch<NodeId>(
    placement: static _ => Create(Guid.CreateVersion7().ToString("N")),
    typeSeed: static s => Minted(ContentAddress.Of(s.Node, s.Tolerance, static w => Node.WriteIdentity(w))),
    content: static s => Minted(ContentAddress.Of(s.Node, s.Tolerance, static w => n.CanonicalBytes(w))),
    precomputed: static s => Minted(s.Address));

    private static NodeId Minted(ContentAddress address) =>
    Create(ContentHash.Hex(address.ToValue()).ToUpperInvariant());
}

[Union]
public abstract partial class NodeSeed {
    private NodeSeed() { }

    public sealed partial class Placement : NodeSeed;
    public sealed partial class TypeSeed(Node.Object node, double tolerance) : NodeSeed { public Node.Object Node { get; } = node; public double Tolerance { get; } = tolerance; }
    public sealed partial class Content(Node node, double tolerance) : NodeSeed { public Node Node { get; } = node; public double Tolerance { get; } = tolerance; }
    public sealed partial class Precomputed(ContentAddress address) : NodeSeed { public ContentAddress Address { get; } = address; }
}

[SmartEnum<string>]
public sealed partial class ReleaseVersion {
    public static readonly ReleaseVersion Ifc2X3 = new("IFC2X3");
    public static readonly ReleaseVersion Ifc4 = new("IFC4");
    public static readonly ReleaseVersion Ifc4X1 = new("IFC4X1");
    public static readonly ReleaseVersion Ifc4X3 = new("IFC4X3");
    public static readonly ReleaseVersion Ifc4X3Add2 = new("IFC4X3_ADD2");
    public static readonly ReleaseVersion Ifc5 = new("IFC5");
}

[SmartEnum<string>]
public sealed partial class ModelView {
    public static readonly ModelView Ifc4Reference = new("ReferenceView");
    public static readonly ModelView DesignTransfer = new("DesignTransferView");
    public static readonly ModelView Coordination = new("CoordinationView");
    public static readonly ModelView Alignment = new("AlignmentView");
}

[SmartEnum<string>]
public sealed partial class ObjectKind {
    public static readonly ObjectKind Occurrence = new("occurrence");
    public static readonly ObjectKind Type = new("type");
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PredefinedType {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim().ToUpperInvariant();
    public static readonly PredefinedType NotDefined = Create("NOTDEFINED");
}

public readonly record struct StepHeader(
    Seq<string> Descriptions, string Name, Instant TimeStamp, Seq<string> Authors,
    Seq<string> Organizations, string Preprocessor, string OriginatingSystem, Seq<string> Schema) {
    public static readonly StepHeader Empty = new(Seq<string>(), "", default, Seq<string>(), Seq<string>(), "", "", Seq<string>());
}

public readonly record struct OwnerHistory(string OwningUser, string OwningApplication, Instant Created, Option<Instant> Modified, string ChangeAction, string State);

public readonly record struct SchemaSpan(ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn) {
    public static SchemaSpan From(ReleaseVersion introduced) => new(introduced, None);
}

public readonly record struct RepresentationContentHash(Map<RepresentationSlot, UInt128> ByIdentifier) {
    public static readonly RepresentationContentHash Empty = new(Map<RepresentationSlot, UInt128>());
    public Option<UInt128> At(RepresentationSlot slot) => ByIdentifier.Find(slot);
    public RepresentationContentHash With(RepresentationSlot slot, UInt128 hash) =>
        this with { ByIdentifier = ByIdentifier.AddOrUpdate(slot, hash) };
}

[SmartEnum<int>]
public sealed partial class RepresentationSlot {
    public static readonly RepresentationSlot Body = new(1, "body", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Axis = new(2, "axis", static (source, hash) => source.ResolveAxis(hash).Map(static curve => (Representation)new Representation.Line(curve)));
    public static readonly RepresentationSlot FootPrint = new(3, "foot-print", static (source, hash) => source.ResolveFootprint(hash).Map(static ring => (Representation)new Representation.Ring(ring)));
    public static readonly RepresentationSlot Box = new(4, "box", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Annotation = new(5, "annotation", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Surface = new(6, "surface", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Profile = new(7, "profile", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Clearance = new(8, "clearance", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Cog = new(9, "cog", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Lighting = new(10, "lighting", static (source, hash) => Some((Representation)new Representation.Blob(hash)));
    public static readonly RepresentationSlot Reference = new(11, "reference", static (source, hash) => Some((Representation)new Representation.Blob(hash)));

    public string Token { get; }

    [UseDelegateFromConstructor] public partial Option<Representation> Decode(GeometrySource source, UInt128 hash);
}

[Union]
public abstract partial class Representation {
    private Representation() { }

    public sealed partial class Line(AxisCurve curve) : Representation { public AxisCurve Curve { get; } = curve; }
    public sealed partial class Ring(FootprintPolygon polygon) : Representation { public FootprintPolygon Polygon { get; } = polygon; }
    public sealed partial class Blob(UInt128 key) : Representation { public UInt128 Key { get; } = key; }
}

public readonly record struct Vector3(double X, double Y, double Z) {
    public static readonly Vector3 Zero = new(0d, 0d, 0d);
    public static readonly Vector3 UnitX = new(1d, 0d, 0d);
    public static readonly Vector3 UnitY = new(0d, 1d, 0d);
    public static readonly Vector3 UnitZ = new(0d, 0d, 1d);
    public double Length => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
    public Vector3 Unit { get { double m = Length; return m > 0d ? new Vector3(X / m, Y / m, Z / m) : Zero; } }
    public static double Distance(Vector3 a, Vector3 b) => (a - b).Length;
    public static double Dot(Vector3 a, Vector3 b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
    public static Vector3 Cross(Vector3 a, Vector3 b) => new((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator *(Vector3 a, double s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3 operator /(Vector3 a, double s) => new(a.X / s, a.Y / s, a.Z / s);
    public static bool Near(Vector3 a, Vector3 b, double epsilon) => Distance(a, b) <= epsilon;
}

public readonly record struct AxisCurve(Vector3 Start, Vector3 End, Vector3 Up) {
    public double Length => Vector3.Distance(Start, End);
}

public readonly record struct FootprintPolygon(Seq<Vector3> Ring, Seq<Seq<Vector3>> Holes = default) {
    public static readonly FootprintPolygon Empty = new(Seq<Vector3>());
    public bool IsEmpty => Ring.IsEmpty;
    public double Area => RingArea(Ring) - Holes.Fold(0d, static (sum, hole) => sum + RingArea(hole));

    static double RingArea(Seq<Vector3> ring) =>
        ring.Count < 3
            ? 0d
            : ring.Map((vertex, index) => Vector3.Cross(vertex, ring[(index + 1) % ring.Count]))
                  .Fold(Vector3.Zero, static (normal, cross) => normal + cross).Length * 0.5;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct PlacementTransform {
    public Vector3 Location { get; }
    public Vector3 Axis { get; }
    public Vector3 RefDirection { get; }

    public Vector3 Apply(Vector3 local) {
        Vector3 z = Axis.Unit;
        Vector3 x = RefDirection.Unit;
        Vector3 y = Vector3.Cross(z, x);
        return Location + (x * local.X) + (y * local.Y) + (z * local.Z);
    }

    public void CanonicalBytes(CanonicalWriter w) =>
        w.Double(Location.X).Double(Location.Y).Double(Location.Z)
         .Double(Axis.X).Double(Axis.Y).Double(Axis.Z)
         .Double(RefDirection.X).Double(RefDirection.Y).Double(RefDirection.Z);
}

public readonly record struct GeometrySource(
    Func<UInt128, Option<AxisCurve>> ResolveAxis, Func<UInt128, Option<FootprintPolygon>> ResolveFootprint) {
    public static readonly GeometrySource None = new(static _ => Option<AxisCurve>.None, static _ => Option<FootprintPolygon>.None);
    public Option<AxisCurve> Axis(RepresentationContentHash representations) => representations.At(RepresentationSlot.Axis).Bind(ResolveAxis);
    public Option<FootprintPolygon> Footprint(RepresentationContentHash representations) => representations.At(RepresentationSlot.FootPrint).Bind(ResolveFootprint);

    public Fin<Representation> ResolveRepresentation(Node.Object node, RepresentationSlot slot) =>
        node.Representations.At(slot)
            .ToFin(new ElementFault.ValueRejected($"<representation-absent:{slot.Key}:{node.Id.ToValue()}>"))
            .Bind(hash => slot.Decode(this, hash)
                .ToFin(new ElementFault.ValueRejected($"<representation-unresolvable:{slot.Key}:{node.Id.ToValue()}>")));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct AppearanceVector {
    public double BaseColorR { get; }
    public double BaseColorG { get; }
    public double BaseColorB { get; }
    public double Metallic { get; }
    public double Roughness { get; }
    public double Opacity { get; }
    public bool Transmissive { get; }
}

public sealed record AppearanceSummary {
    private AppearanceSummary(UInt128 appearanceKey, AppearanceVector vector) =>
        (AppearanceKey, Vector) = (appearanceKey, vector);

    public UInt128 AppearanceKey { get; }
    public AppearanceVector Vector { get; }

    public double BaseColorR => Vector.BaseColorR;
    public double BaseColorG => Vector.BaseColorG;
    public double BaseColorB => Vector.BaseColorB;
    public double Metallic => Vector.Metallic;
    public double Roughness => Vector.Roughness;
    public double Opacity => Vector.Opacity;
    public bool Transmissive => Vector.Transmissive;

    public static Fin<AppearanceSummary> Of(AppearanceVector vector) =>
        Indexed(
            [vector.BaseColorR, vector.BaseColorG, vector.BaseColorB, vector.Metallic, vector.Roughness, vector.Opacity],
            static channel => channel is >= 0.0 and <= 1.0, "appearance-channel")
         .Map(_ => Minted(vector))
         .ToFin();

    public static Fin<AppearanceSummary> Rehydrate(UInt128 appearanceKey, AppearanceVector vector) =>
        Of(vector).Bind(summary =>
            summary.AppearanceKey == appearanceKey
                ? Fin.Succ(summary)
                : new ElementFault.AddressUnstable("<appearance-key-mismatch>"));

    private static AppearanceSummary Minted(AppearanceVector vector) =>
        new(ContentAddress.Of(vector, 0.0, static (v, w) =>
            w.Double(v.BaseColorR).Double(v.BaseColorG).Double(v.BaseColorB)
             .Double(v.Metallic).Double(v.Roughness).Double(v.Opacity).Bool(v.Transmissive)).ToValue(), vector);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial class Node {
    private Node() { }

    public abstract NodeId Id { get; init; }

    [Equatable]
    public sealed partial class Object(
    NodeId Id, ObjectKind Kind, Option<string> ExternalId, Classification Classification, PredefinedType PredefinedType,
    Option<string> ObjectType, string Name, string Tag, RepresentationContentHash Representations,
    Option<OwnerHistory> History, SchemaSpan Span, Seq<Classification> Classifications = default,
    Option<PlacementTransform> Placement = default) : Node {
        [IgnoreEquality]
        public override NodeId Id { get; init; } = Id;
        public ObjectKind Kind { get; } = Kind;
        public Option<string> ExternalId { get; } = ExternalId;
        public Classification Classification { get; } = Classification;
        public PredefinedType PredefinedType { get; } = PredefinedType;
        public Option<string> ObjectType { get; } = ObjectType;
        [property: StringEquality(StringComparison.Ordinal)] public string Name { get; } = Name;
        [property: StringEquality(StringComparison.Ordinal)] public string Tag { get; } = Tag;
        public RepresentationContentHash Representations { get; } = Representations;
        public Option<OwnerHistory> History { get; } = History;
        public SchemaSpan Span { get; } = Span;
        [property: UnorderedEquality] public Seq<Classification> Classifications { get; } = Classifications;
        public Option<PlacementTransform> Placement { get; } = Placement;

        public Seq<Classification> AllClassifications =>
            toSeq((Seq(Classification) + Classifications).AsEnumerable()
                .DistinctBy(static classification => (classification.System, classification.Code, classification.Edition)));

    }
    [Equatable]
    public sealed partial class Material(NodeId id, MaterialId materialKey, MaterialComposition composition, Seq<MaterialPropertySet> properties) : Node {
        [IgnoreEquality] public override NodeId Id { get; init; } = id;
        public MaterialId MaterialKey { get; } = materialKey;
        public MaterialComposition Composition { get; } = composition;
        [UnorderedEquality] public Seq<MaterialPropertySet> Properties { get; } = properties;
    }
    [Equatable] public sealed partial class PropertySet(NodeId id, PropertyBag bag) : Node { [IgnoreEquality] public override NodeId Id { get; init; } = id; public PropertyBag Bag { get; } = bag; }
    [Equatable] public sealed partial class QuantitySet(NodeId id, QuantityBag bag) : Node { [IgnoreEquality] public override NodeId Id { get; init; } = id; public QuantityBag Bag { get; } = bag; }
    [Equatable] public sealed partial class Assessment(NodeId id, AssessmentPayload payload) : Node { [IgnoreEquality] public override NodeId Id { get; init; } = id; public AssessmentPayload Payload { get; } = payload; }
    [Equatable] public sealed partial class Appearance(NodeId id, AppearanceSummary summary) : Node { [IgnoreEquality] public override NodeId Id { get; init; } = id; public AppearanceSummary Summary { get; } = summary; }
    [Equatable] public sealed partial class Coverage(NodeId id, CoverageGrid grid) : Node { [IgnoreEquality] public override NodeId Id { get; init; } = id; public CoverageGrid Grid { get; } = grid; }
    [Equatable] public sealed partial class Observation(NodeId id, ObservationSeries series) : Node { [IgnoreEquality] public override NodeId Id { get; init; } = id; public ObservationSeries Series { get; } = series; }

    public NodeSeed Seed(double tolerance) => this is Node.Object o
    ? o.Kind == ObjectKind.Occurrence ? new NodeSeed.Placement() : new NodeSeed.TypeSeed(o, tolerance)
    : new NodeSeed.Content(this, tolerance);

    public void CanonicalBytes(CanonicalWriter w) =>
    Switch(
    @object: o => WriteObject(w, o),
    material: m => { w.Ordinal(1); w.String(m.MaterialKey.ToValue()); m.Composition.CanonicalBytes(w); w.Ordinal(m.Properties.Count); foreach (var p in m.Properties.OrderBy(static p => p.Discipline.Key, StringComparer.Ordinal).ThenBy(p => ContentAddress.Of(p, tolerance, static (row, k) => row.CanonicalBytes(k)).ToValue())) { p.CanonicalBytes(w); } },
    propertySet: p => { w.Ordinal(2); w.String(p.Bag.SetName); w.String(p.Bag.Inheritance.Key); w.Ordinal(p.Bag.Source.Key); w.Ordinal(p.Bag.Values.Count); foreach (var (n, v) in p.Bag.Values.OrderBy(static e => e.Key.ToValue(), StringComparer.Ordinal)) { w.String(n.ToValue()); v.CanonicalBytes(w); } },
    quantitySet: q => { w.Ordinal(3); w.String(q.Bag.SetName); w.String(q.Bag.Inheritance.Key); w.Ordinal(q.Bag.Source.Key); w.Ordinal(q.Bag.Values.Count); foreach (var (n, m) in q.Bag.Values.OrderBy(static e => e.Key.ToValue(), StringComparer.Ordinal)) { w.String(n.ToValue()); w.Measure(m); } w.Ordinal(q.Bag.Groups.Count); foreach (var (prefix, group) in q.Bag.Groups.OrderBy(static e => e.Key, StringComparer.Ordinal)) { w.String(prefix); w.Bool(group.Discrimination.IsSome); group.Discrimination.IfSome(d => w.String(d)); w.Bool(group.Quality.IsSome); group.Quality.IfSome(x => w.String(x)); w.Bool(group.Usage.IsSome); group.Usage.IfSome(u => w.String(u)); } },
    assessment: a => { w.Ordinal(4); a.Payload.CanonicalBytes(w); },
    appearance: a => { w.Ordinal(5); w.U128(a.Summary.AppearanceKey); },
    coverage: c => { w.Ordinal(6); c.Grid.CanonicalBytes(w); },
    observation: o => { w.Ordinal(7); o.Series.CanonicalBytes(w); });

    static void WriteObject(CanonicalWriter w, Node.Object o) {
    IdentityHead(w, o);
    VolatileClassifications(w, o);
    IdentityMid(w, o);
    VolatileRepresentations(w, o);
    IdentityTail(w, o);
    }

    internal static void WriteIdentity(CanonicalWriter w, Node.Object o) {
    IdentityHead(w, o);
    IdentityMid(w, o);
    IdentityTail(w, o);
    }

    static void IdentityHead(CanonicalWriter w, Node.Object o) =>
    w.Ordinal(0).String(o.Kind.Key)
     .Optional(o.ExternalId, static (id, run) => run.String(id))
     .String(o.Classification.System).String(o.Classification.Code).String(o.Classification.Edition);

    static void IdentityMid(CanonicalWriter w, Node.Object o) =>
    w.String(o.PredefinedType.ToValue())
     .Optional(o.ObjectType, static (label, run) => run.String(label))
     .String(o.Name).String(o.Tag);

    static void IdentityTail(CanonicalWriter w, Node.Object o) =>
    w.String(o.Span.IntroducedIn.Key)
     .Optional(o.Span.RemovedIn, static (removed, run) => run.String(removed.Key));

    static void VolatileClassifications(CanonicalWriter w, Node.Object o) =>
    w.Rows(
     toSeq(o.Classifications.AsEnumerable()
      .OrderBy(static x => x.System, StringComparer.Ordinal)
      .ThenBy(static x => x.Code, StringComparer.Ordinal)
      .ThenBy(static x => x.Edition, StringComparer.Ordinal)),
     static (c, run) => run.String(c.System).String(c.Code).String(c.Edition));

    static void VolatileRepresentations(CanonicalWriter w, Node.Object o) =>
    w.Sorted(o.Representations.ByIdentifier.ToSeq(), static pair => pair.Key.Key, Comparer<int>.Default,
     static (pair, run) => run.Ordinal(pair.Key.Key).U128(pair.Value));

    public Node Relabel(NodeId id) => Switch<Node>(
        @object: o => new Object(id, o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.ObjectType, o.Name, o.Tag, o.Representations, o.History, o.Span, o.Classifications, o.Placement),
        material: m => new Material(id, m.MaterialKey, m.Composition, m.Properties),
        propertySet: p => new PropertySet(id, p.Bag),
        quantitySet: q => new QuantitySet(id, q.Bag),
        assessment: a => new Assessment(id, a.Payload),
        appearance: a => new Appearance(id, a.Summary),
        coverage: c => new Coverage(id, c.Grid),
        observation: o => new Observation(id, o.Series));

    public Node Remap(Func<NodeId, NodeId> map) => Switch<Node>(
        @object: o => new Object(map(o.Id), o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.ObjectType, o.Name, o.Tag, o.Representations, o.History, o.Span, o.Classifications, o.Placement),
        material: m => new Material(map(m.Id), m.MaterialKey, m.Composition, m.Properties),
        propertySet: p => new PropertySet(map(p.Id), p.Bag with { Values = p.Bag.Values.Map((_, v) => v.Remap(map)) }),
        quantitySet: q => new QuantitySet(map(q.Id), q.Bag),
        assessment: a => new Assessment(map(a.Id), a.Payload),
        appearance: a => new Appearance(map(a.Id), a.Summary),
        coverage: c => new Coverage(map(c.Id), c.Grid),
        observation: o => new Observation(map(o.Id), o.Series));
}
```

## [03]-[ELEMENT_GRAPH]

- Owner: `Header` the model header (`ReleaseVersion` + `ModelView` + `Geospatial/reference#GEO_REFERENCE` `GeoReference` + `Tolerance` + `Instant` + `StepHeader` + the `Properties/quantity#MEASURE_VALUE` `UnitScheme` presentation declaration) carrying the ONE semantic-header `CanonicalBytes` projection both the `Projection/address#CONTENT_ADDRESS` `OfGraph` snapshot key and the `Graph/delta#GRAPH_DELTA` `GraphDelta.Address` header contribution compose (the projection owned once, never re-spelled per call site) and the `SameGrid` bitwise-tolerance law every grid gate reads; `ElementGraph` the frozen read snapshot carrying the nodes, edges, the built-once incidence index, the `(EdgeFilter, EdgeOrientation)`-keyed view cache, and the memoized `Bake`; `Element` the derived-fold "has it all" result; `BakedMaterial` the material-plus-usage pair `Bake` folds from an `Associate` edge (the occurrence's own AND, via the named inheritance, the `Component`'s, unioned by `MaterialKey`); `TypeBinding` the named type→occurrence inheritance carrier `Bake` produces from the `Assign.TypeDefinition` resolution (the type id + the inherited `BakedMaterial` set / resolved `SectionProperties` / secondary `Classification`s), surfaced as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes.
- Entry: `ElementGraph.Of(header, nodes, edges)` builds the frozen snapshot — `ToFrozenDictionary` over the nodes, the incidence index grouping every edge by every node its `Members` touch, the `MaterialId`-keyed material index, the demand-built `(EdgeFilter, EdgeOrientation)`-keyed `QuikGraph` view cache over `TypedEdge` legs, and an empty `Bake` memo; `Genesis(header)` seeds the empty header-only snapshot a model-creating session or a Marten stream rehydrate builds onto; `Apply(delta)` advances a snapshot by a validated `Graph/delta#GRAPH_DELTA` `GraphDelta` (the persistence rehydrate + live-apply entry), `Fin<T>` refusing `ElementFault.NodeAbsent` on a corrupt delta whose added edge names an absent member — either binary endpoint or a `Connect`'s realizing intermediary, the full `Relationship.Members` closure.
- Entry: `Bake(objectId)` folds the reachable subgraph from an `Object` node into an `Element`, memoized by `objectId` within the snapshot (a new snapshot from a `Graph/delta#GRAPH_DELTA` carries a fresh memo), `Fin<T>` refusing `ElementFault.NodeAbsent` on an absent root and `ElementFault.RelationshipInvalid` on a cyclic `Compose` chain (a `Compose` ancestry set threaded through the fold); `View(filter, orientation)` is the ONE kind-and-orientation scope — memoized per row pair, `TypedEdge` legs carrying the edge so a kind-aware traversal reads it off the leg — with `Topology()` its `(All, Forward)` one-hop; a `Func`-predicate scope died because it could never KEY the cache Persistence's `TopologyView` and Bim's `SpatialStructure` demand; rooted spatial ancestry is Bim `Model/spatial` `SpatialStructure.Ancestry`'s alone (E-E9) — the `Compose` graph is MULTI-PARENT and the real law is Contain-then-Aggregate precedence, which the contract's deleted `ContainmentPath` contradicted.
- Entry: the read accessors `ObjectNodes`/`Find`/`Find<T>`/`Material(MaterialId)`/`MaterialsOf`/`CompositionOf`/`PropertiesOf`/`SectionOf` enumerate the object roots and resolve a node (raw or typed by case) and the material/composition/property/section subgraph a member binds — `MaterialsOf` carrying the one-hop type-resolved fallback the other three compose (an occurrence with no own material/profile reads its `Component`'s), the `SectionOf(member)` signature FROZEN.
- Entry: the group family `GroupsOf`/`MembersOf` resolves the `Assign.Group` memberships (every element in system X, the zones a space belongs to) as incidence reads, and `ObservationsUnder(root)` rolls the measured series over the same OWNING `Compose` closure `BakeParts` recurses so a whole answers for its parts' sensors — together the polymorphic surface a `Rasm.Compute` analysis route, a Persistence index pass, and an AppUi model tree read the concrete graph through, the discipline reads (loads/supports/spaces/areas) composing in Compute from these primitives.
- Auto: `Of` builds the incidence index, the `MaterialId`-keyed material index the `Material()` read serves off, and a topology containing every node, including isolated vertices. `Bake` folds one root's incidence: property definitions become bags, assessments land flat, observations become measured series off the occurrence alone, associations become material/appearance/coverage values, owning compositions recurse into parts, and `Assign.TypeDefinition` applies the named type inheritance once. Topology and memo ride the sealed `ElementGraph` as lazy equality-excluded caches; only `Of`, `Genesis`, and `Apply` mint snapshots.
- Output: the `Element` is the one flat record a consumer reads — `element.Properties.Find(name)`, `element.Materials`, `element.Assessments`, `element.Observations`, `element.Appearance`, `element.Coverages`, `element.Parts`, and `element.TypeId` (the inherited `Component`, the generator's type-representation recovery key), with `ObservationsUnder` the whole-over-parts measured rollup beside them — "has it all" in one `Bake`, never a join across the graph, and the computed-versus-measured commissioning read is `element.Assessments` beside `element.Observations` off one baked root rather than a historian join; the `ElementGraph` is the immutable read snapshot Persistence persists and the projectors assemble onto, its `Generator.Equals` structural equality and `Inequalities` member diff feeding the Persistence 3-way `StructuralMerge`; the keyed `View` cache answers reachability and topological order for a consumer without a second graph; the containment breadcrumb is Bim `SpatialStructure.Ancestry`'s (E-E9).
- Packages: `Generator.Equals` (`[Equatable]` snapshot equality, `[StringEquality]`/`[UnorderedEquality]`/`[IgnoreEquality]` member policies, `Inequalities` diff, and the generated `EqualityComparer.Default` reused as the LINQ/`HashSet` key comparer outside generated code), QuikGraph (`BidirectionalGraph` over the shared `TypedEdge`, `AlgorithmExtensions`; kind scoping is the `EdgeFilter`-keyed view cache, so no per-call predicate wrapper materializes), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin`), System.Collections.Frozen/Immutable, NodaTime (`Instant`).
- Growth: a new derived element field is one column on `Element` the `Bake` fold populates from an existing edge kind; a new edge semantic the fold reads is one arm in `Bake`; a new type-inherited `Seq` is one `UnionBy` arm in the named inheritance, a new occurrence-overrides-type single field one fall-back guard; the working/frozen split keeps the live delta path in the HAMT (`Graph/delta`) and the read path in the frozen snapshot, so neither grows the other; never a second stored `Element` record beside the graph, never a second identity scheme for the deterministic Type id.
- Boundary: the `Element` is a DERIVED FOLD, never a stored record — one flat read comes from `Bake` over the graph, and a parallel stored element record beside it is the deleted form.
- Boundary: the graph splits by PHASE — the live authoring/delta path is a `TrackingHashMap` HAMT (`Graph/delta` owns it for O(log n) structural sharing and the change record its `Diff` reads) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` at the freeze boundary), so a mutable working graph is never confused with a frozen read snapshot.
- Boundary: the incidence index, the material index, and the `QuikGraph` view are built ONCE per snapshot and the `Bake` memo is keyed by object within the snapshot, invalidated only by a new snapshot from a delta, so a re-`Bake` is O(1) and a graph edit is O(log n).
- Boundary: RULED LOSS (W3 gate) — the `View`/`Bake` memos carry NO fact or gauge hook, and per-model view-build observability has no producer by DESIGN: the memos are cache MECHANICS on a frozen VALUE (lazy, equality-excluded), not domain facts, so no `ElementFact` case, `ElementPoint` row, or instrument threads into the pure read — and the census proves zero series consumers anywhere (Persistence deleted its per-model build/memo series as FORGERIES it could not witness; every live memo-observability module sits at its OWN cache owner — the store cache's `MemoHit`/`MemoMiss` slots, the nesting engine's census counters). A wanting consumer today reads the federated `store.topology.build` series plus `VertexCount`/`EdgeCount` off the returned view; if a real series consumer ever lands (a board row naming view-build latency with a declared bound), the seat is one `GaugedSpan` around the view mint at the CALLER owning that budget — never a hook inside the frozen snapshot.
- Boundary: the NAMED type→occurrence inheritance applies once in `Bake` — single fields occurrence-overrides-type, the materials/assessments/classifications `Seq`s union+dedup-by-key — and is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the `PropertyBag.Merge` owns, which stays bag-only.
- Boundary: the observations and coverages `Seq`s are deliberately NOT inherited, which the `GatherFamily` capability sets state as data (`Occurrence` admits both, `Component` neither): a `Component` is a catalogue entry no instrument is mounted on and no field is sampled over, so a type-borne series claims every realization reports one sensor's data.
- Boundary: the `MaterialsOf`/`SectionOf` type-resolved fallback is ONE hop (a `Component` is not itself typed), so the FROZEN `SectionOf(member)` signature `Rasm.Compute` reads is untouched.
- Boundary: a TYPE `Object`'s deterministic id excludes the volatile `Representations`, so a geometry attach re-keys neither the Type node nor the cached `Bake`.
- Boundary: the `Header` carries the `GeoReference`, the `StepHeader`, and the `UnitScheme` (the `IfcUnitAssignment` unit-presentation declaration — canonical-bytes-excluded, so display units never fork identity), and the `Object` nodes carry the `OwnerHistory` and the `SchemaSpan`, so the model's provenance, declared units, and schema span ride the graph rather than a side channel.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GatherFamily : ICapability<GatherFamily> {
    public static readonly GatherFamily Observations = new("observations");
    public static readonly GatherFamily Coverages = new("coverages");

    public static readonly CapabilitySet<GatherFamily> Occurrence = CapabilitySet<GatherFamily>.Of(Observations, Coverages);
    public static readonly CapabilitySet<GatherFamily> Component = CapabilitySet<GatherFamily>.None;
}

[SmartEnum<string>]
public sealed partial class EdgeOrientation {
    public static readonly EdgeOrientation Forward = new("forward", static edge => edge.DirectedPairs);
    public static readonly EdgeOrientation Ascending = new("ascending", static edge => edge.DirectedPairs.Map(static leg => (leg.To, leg.From)));

    [UseDelegateFromConstructor] public partial Seq<(NodeId From, NodeId To)> Legs(Relationship edge);
}

[SmartEnum<string>]
public sealed partial class EdgeFilter {
    public static readonly EdgeFilter All = new("all", static _ => true);
    public static readonly EdgeFilter Composition = new("composition", static r => r.Kind == RelationshipKind.Compose);
    public static readonly EdgeFilter Containment = new("containment", static r => r.IsContainment);
    public static readonly EdgeFilter Spatial = new("spatial", static r => r is Relationship.Compose { SubKind: var flavor } && (flavor == ComposeKind.Contain || flavor == ComposeKind.Aggregate));
    public static readonly EdgeFilter Connection = new("connection", static r => r.Kind == RelationshipKind.Connect);
    public static readonly EdgeFilter Void = new("void", static r => r.Kind == RelationshipKind.Void);
    public static readonly EdgeFilter Assignment = new("assignment", static r => r.Kind == RelationshipKind.Assign);

    [UseDelegateFromConstructor] public partial bool Admit(Relationship edge);
}

public readonly record struct TypedEdge(NodeId Source, NodeId Target, Relationship Edge) : QuikGraph.IEdge<NodeId>;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Gathered(
    Seq<PropertyBag> Properties, Seq<QuantityBag> Quantities, Seq<BakedMaterial> Materials,
    Seq<AssessmentPayload> Assessments, Option<AppearanceSummary> Appearance,
    Seq<CoverageGrid> Coverages, Seq<ObservationSeries> Observations) {
    public static readonly Gathered Empty = new([], [], [], [], None, [], []);
}

public sealed record Header(
    ReleaseVersion Schema, ModelView View, GeoReference Reference, double Tolerance, Instant At, StepHeader Step) {
    public UnitScheme Units { get; init; } = UnitScheme.Si;

    public static Header Default(Instant at) =>
    new(ReleaseVersion.Ifc4X3Add2, ModelView.Ifc4Reference, GeoReference.Identity, 1e-6, at, StepHeader.Empty);

    public bool SameGrid(Header other) =>
    BitConverter.DoubleToInt64Bits(Tolerance) == BitConverter.DoubleToInt64Bits(other.Tolerance);

    public void CanonicalBytes(CanonicalWriter w) {
    w.String(Schema.Key).String(View.Key).Double(Tolerance);
    Reference.CanonicalBytes(w);
    }
}

public readonly record struct BakedMaterial(Node.Material Material, MaterialUsage Usage);

public readonly record struct TypeBinding(NodeId TypeId, Seq<BakedMaterial> Materials, Option<SectionProperties> Section, Seq<Classification> Classifications);

[Equatable]
public sealed partial record Element(
    [property: IgnoreEquality] NodeId Id, ObjectKind Kind, [property: IgnoreEquality] Option<string> ExternalId,
    Classification Classification, PredefinedType PredefinedType, string Name, string Tag,
    RepresentationContentHash Representations,
    [property: UnorderedEquality] Seq<BakedMaterial> Materials,
    [property: UnorderedEquality] Seq<PropertyBag> Properties,
    [property: UnorderedEquality] Seq<QuantityBag> Quantities,
    [property: UnorderedEquality] Seq<AssessmentPayload> Assessments,
    Option<AppearanceSummary> Appearance,
    [property: UnorderedEquality] Seq<CoverageGrid> Coverages,
    [property: UnorderedEquality] Seq<ObservationSeries> Observations,
    [property: IgnoreEquality] Seq<Element> Parts,
    Option<TypeBinding> Type,
    [property: IgnoreEquality] Option<OwnerHistory> History,
    [property: UnorderedEquality] Seq<Classification> Classifications = default) {
    public Option<NodeId> TypeId => Type.Map(static t => t.TypeId);
}

[Equatable]
public sealed partial class ElementGraph {
    [property: UnorderedEquality] public FrozenDictionary<NodeId, Node> Nodes { get; }
    [property: UnorderedEquality] public ImmutableArray<Relationship> Edges { get; }
    public Header Header { get; }

    [IgnoreEquality] readonly FrozenDictionary<NodeId, ImmutableArray<Relationship>> incidence;
    [IgnoreEquality] readonly FrozenDictionary<MaterialId, Node.Material> materials;
    [IgnoreEquality] readonly System.Collections.Concurrent.ConcurrentDictionary<NodeId, Element> bakeMemo = new();
    [IgnoreEquality] readonly System.Collections.Concurrent.ConcurrentDictionary<(EdgeFilter Filter, EdgeOrientation Orientation), QuikGraph.BidirectionalGraph<NodeId, TypedEdge>> views = new();

    ElementGraph(Header header, FrozenDictionary<NodeId, Node> nodes, ImmutableArray<Relationship> edges) {
    (Header, Nodes, Edges) = (header, nodes, edges);
    materials = nodes.Values.OfType<Node.Material>()
    .GroupBy(static m => m.MaterialKey)
    .ToFrozenDictionary(static g => g.Key, static g => g.First());
    incidence = edges
    .SelectMany(e => e.Members.Distinct().Select(m => (Node: m, Edge: e)))
    .GroupBy(static p => p.Node, static p => p.Edge)
    .ToFrozenDictionary(static g => g.Key, static g => g.ToImmutableArray());
    }

    public static ElementGraph Of(Header header, FrozenDictionary<NodeId, Node> nodes, ImmutableArray<Relationship> edges) => new(header, nodes, edges);

    public static ElementGraph Genesis(Header header) => Of(header, FrozenDictionary<NodeId, Node>.Empty, []);

    public Fin<ElementGraph> Apply(GraphDelta delta) {
    ElementGraph next = delta.ReplayOnto(this);
    return delta.AddedEdges
    .Choose(e => e.Members.Find(m => !next.Nodes.ContainsKey(m)))
    .Head
    .Match(
    Some: member => new ElementFault.NodeAbsent($"<replay-edge-member-absent:{member.ToValue()}>"),
    None: () => Fin.Succ(next));
    }

    public Seq<Node.Object> ObjectNodes => toSeq(Nodes.Values).Choose(static n => n is Node.Object o ? Some(o) : None);

    public ImmutableArray<Relationship> EdgesAt(NodeId node) => incidence.GetValueOrDefault(node, []);

    public QuikGraph.BidirectionalGraph<NodeId, TypedEdge> Topology() => View(EdgeFilter.All, EdgeOrientation.Forward);

    public QuikGraph.BidirectionalGraph<NodeId, TypedEdge> View(EdgeFilter filter, EdgeOrientation orientation) =>
    views.GetOrAdd((filter, orientation), pair => {
    QuikGraph.BidirectionalGraph<NodeId, TypedEdge> view = new(allowParallelEdges: true, vertexCapacity: Nodes.Count);
    view.AddVertexRange(Nodes.Keys);
    foreach (Relationship edge in Edges) {
    if (!pair.Filter.Admit(edge)) { continue; }
    foreach ((NodeId from, NodeId to) in pair.Orientation.Legs(edge)) { view.AddEdge(new TypedEdge(from, to, edge)); }
    }
    return view;
    });

    // --- [READ_ACCESSORS] --------------------------------------------------------------
    public Option<Node> Find(NodeId id) => Nodes.TryGetValue(id, out Node? n) ? Some(n) : None;

    public Option<T> Find<T>(NodeId id) where T : Node => Find(id).Bind(static n => n is T t ? Some(t) : None);

    public Option<Node.Material> Material(MaterialId key) =>
    materials.TryGetValue(key, out Node.Material? m) ? Some(m) : None;

    Seq<Node.Material> DirectMaterialsOf(NodeId node) =>
    toSeq(EdgesAt(node)).Choose(e => e is Relationship.Associate r && r.Subject == node ? Find<Node.Material>(r.Resource) : None);

    Option<NodeId> TypeObjectOf(NodeId member) =>
    toSeq(EdgesAt(member)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == member ? Some(a.Definition) : None).Head;

    public Seq<Node.Material> MaterialsOf(NodeId member) {
    Seq<Node.Material> direct = DirectMaterialsOf(member);
    return direct.IsEmpty ? TypeObjectOf(member).Match(Some: DirectMaterialsOf, None: () => direct) : direct;
    }

    public Option<MaterialComposition> CompositionOf(NodeId member) => MaterialsOf(member).Head.Map(static m => m.Composition);

    public Seq<MaterialPropertySet> PropertiesOf(NodeId member) =>
    MaterialsOf(member).Bind(static m => m.Properties);

    public Option<SectionProperties> SectionOf(NodeId member) =>
    MaterialsOf(member).Choose(static m => m.Composition is MaterialComposition.ProfileSet { Section: var s } ? s : Option<SectionProperties>.None).Head;

    // --- [GROUP_READS] -----------------------------------------------------------------
    public Seq<NodeId> GroupsOf(NodeId member) =>
    toSeq(EdgesAt(member)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.Group && a.Subject == member ? Some(a.Definition) : None);

    public Seq<NodeId> MembersOf(NodeId group) =>
    toSeq(EdgesAt(group)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.Group && a.Definition == group ? Some(a.Subject) : None);

    public Fin<Seq<ObservationSeries>> ObservationsUnder(NodeId root) =>
    Bake(root).Map(static element => Rollup(element));

    static Seq<ObservationSeries> Rollup(Element element) =>
    element.Parts.Fold(element.Observations, static (series, part) => series + Rollup(part));

    // --- [BAKE] ------------------------------------------------------------------------
    public Fin<Element> Bake(NodeId objectId) => Bake(objectId, ImmutableHashSet<NodeId>.Empty);

    Fin<Element> Bake(NodeId objectId, ImmutableHashSet<NodeId> ancestry) =>
    ancestry.Contains(objectId)
    ? new ElementFault.RelationshipInvalid($"<bake-compose-cycle:{objectId.ToValue()}>")
    : bakeMemo.TryGetValue(objectId, out Element? cached)
    ? Fin.Succ(cached)
    : Find<Node.Object>(objectId)
    .ToFin(new ElementFault.NodeAbsent($"<bake-root-absent:{objectId.ToValue()}>"))
    .Bind(root => BakeObject(root, ancestry.Add(objectId)).Map(element => { bakeMemo[objectId] = element; return element; }));

    Fin<Element> BakeObject(Node.Object root, ImmutableHashSet<NodeId> ancestry) {
    Gathered own = Gather(root.Id, GatherFamily.Occurrence);
    Option<(Node.Object Type, Gathered Data)> typeFold = TypeResolutionOf(root.Id);
    Seq<PropertyBag> properties = MergeBagSets(typeFold.Map(static t => t.Data.Properties).IfNone(Seq<PropertyBag>()), own.Properties);
    Seq<QuantityBag> quantities = MergeBagSets(typeFold.Map(static t => t.Data.Quantities).IfNone(Seq<QuantityBag>()), own.Quantities);
    Seq<BakedMaterial> materials = Inherit(own.Materials, typeFold, static data => data.Materials, static b => b.Material.MaterialKey.ToValue());
    Seq<AssessmentPayload> assessments = Inherit(own.Assessments, typeFold, static data => data.Assessments, static a => (a.Discipline.Key, a.Route.Value, a.InputKey));
    Seq<Classification> classifications = UnionBy(
        root.Classifications,
        typeFold.Map(static t => t.Type.Classifications).IfNone(Seq<Classification>()),
        static classification => (classification.System, classification.Code, classification.Edition))
        .Filter(classification => classification != root.Classification);
    PredefinedType predefinedType = typeFold.Match(Some: t => root.PredefinedType == PredefinedType.NotDefined ? t.Type.PredefinedType : root.PredefinedType, None: () => root.PredefinedType);
    string name = typeFold.Match(Some: t => root.Name.Length > 0 ? root.Name : t.Type.Name, None: () => root.Name);
    RepresentationContentHash representations = typeFold.Match(Some: t => root.Representations.ByIdentifier.Count > 0 ? root.Representations : t.Type.Representations, None: () => root.Representations);
    Option<AppearanceSummary> resolvedAppearance = own.Appearance.IsSome ? own.Appearance : typeFold.Bind(static t => t.Data.Appearance);
    Option<TypeBinding> typeBinding = typeFold.Map(static t => new TypeBinding(
    t.Type.Id, t.Data.Materials,
    t.Data.Materials.Choose(static m => m.Material.Composition is MaterialComposition.ProfileSet { Section: var s } ? s : Option<SectionProperties>.None).Head,
    t.Type.Classifications));
    return BakeParts(root.Id, key, ancestry).Map(parts => new Element(
    root.Id, root.Kind, root.ExternalId, root.Classification, predefinedType, name, root.Tag, representations,
    materials, properties, quantities, assessments, resolvedAppearance,
    own.Coverages, own.Observations, parts, typeBinding, root.History, classifications));
    }

    Gathered Gather(NodeId subject, CapabilitySet<GatherFamily> families) =>
    toSeq(EdgesAt(subject)).Fold(Gathered.Empty, (acc, edge) => edge switch {
    Relationship.Assign a when a.Subject == subject => Find(a.Definition).Map(node => Landed(acc, a.SubKind, node, families)).IfNone(acc),
    Relationship.Associate r when r.Subject == subject => Find(r.Resource).Map(node => Bound(acc, node, r.Usage, families)).IfNone(acc),
    _ => acc,
    });

    static Gathered Landed(Gathered acc, AssignKind kind, Node definition, CapabilitySet<GatherFamily> families) =>
    (kind, definition) switch {
    (var k, Node.PropertySet ps) when k == AssignKind.PropertyDefinition => acc with { Properties = acc.Properties.Add(ps.Bag) },
    (var k, Node.QuantitySet qs) when k == AssignKind.PropertyDefinition => acc with { Quantities = acc.Quantities.Add(qs.Bag) },
    (var k, Node.Assessment payload) when k == AssignKind.Assessment => acc with { Assessments = acc.Assessments.Add(payload.Payload) },
    (var k, Node.Observation series) when k == AssignKind.Observation && families.Admits(GatherFamily.Observations) => acc with { Observations = acc.Observations.Add(series.Series) },
    _ => acc,
    };

    static Gathered Bound(Gathered acc, Node resource, MaterialUsage usage, CapabilitySet<GatherFamily> families) =>
    resource switch {
    Node.Material m => acc with { Materials = acc.Materials.Add(new BakedMaterial(m, usage)) },
    Node.Appearance ap => acc with { Appearance = ap.Summary },
    Node.Coverage c when families.Admits(GatherFamily.Coverages) => acc with { Coverages = acc.Coverages.Add(c.Grid) },
    _ => acc,
    };

    Option<(Node.Object Type, Gathered Data)> TypeResolutionOf(NodeId occurrence) =>
    TypeObjectOf(occurrence).Bind(typeId =>
    Find<Node.Object>(typeId).Map(typeObj => (Type: typeObj, Data: Gather(typeId, GatherFamily.Component))));

    static Seq<ValueBag<V>> MergeBagSets<V>(Seq<ValueBag<V>> type, Seq<ValueBag<V>> occurrence) =>
    occurrence.Map(occ => type.Find(t => t.SetName == occ.SetName).Match(Some: t => ValueBag<V>.Merge(t, occ), None: () => occ))
    + type.Filter(t => !occurrence.Exists(o => o.SetName == t.SetName));

    static Seq<T> UnionBy<T, K>(Seq<T> occurrence, Seq<T> type, Func<T, K> key, IEqualityComparer<K>? comparer = null) =>
    toSeq((occurrence + type).AsEnumerable().DistinctBy(key, comparer));

    static Seq<T> Inherit<T, K>(Seq<T> own, Option<(Node.Object Type, Gathered Data)> typeFold, Func<Gathered, Seq<T>> family, Func<T, K> key) =>
    UnionBy(own, typeFold.Map(t => family(t.Data)).IfNone(Seq<T>()), key);

    Fin<Seq<Element>> BakeParts(NodeId whole, ImmutableHashSet<NodeId> ancestry) =>
    toSeq(toSeq(EdgesAt(whole))
    .Choose(e => e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference ? Some((c.Part, c.Ordinal)) : None)
    .OrderBy(static p => p.Ordinal.IsSome ? 0 : 1).ThenBy(static p => p.Ordinal.IfNone(0)).ThenBy(static p => p.Part.ToValue(), StringComparer.Ordinal))
    .TraverseM(p => Bake(p.Part, key, ancestry)).As().Map(static parts => parts.ToSeq());
}
```

## [04]-[FEDERATION]

- Owner: `ElementGraph.Federate` the static cross-model union over a tagged source set under one caller-supplied coordination `Header`; `ElementGraph.Extract` the instance slice over a root set; `FederationCensus` the per-source `FederationSource` rows beside the union totals and the merged tally.
- Entry: `Federate(sources, coordination)` takes `Seq<(string Source, ElementGraph Graph)>` — the source tag being the caller's own model label, never a contract-minted id — one coordination `Header`, and the kernel returning `Fin<(ElementGraph Graph, FederationCensus Census)>`.
- Entry: `Extract(roots)` takes `Seq<NodeId>` and the kernel returning `Fin<ElementGraph>` under the SOURCE `Header` unchanged — a slice is the same model narrowed, never a re-coordinated one.
- Auto: every refusal accumulates through the kernel admission-slot algebra over `Validation<Error,_>` and collapses to `Fin<T>` once at the return, so a federation attempt reports every divergent source and every colliding id in ONE failure rather than the first it meets.
- Auto: both entries mint through ONE `GraphDelta` carrying the union (or slice) as `AddedNodes`/`AddedEdges` with a `Reheader`, run through `AdmitOnto(Genesis(header))` — the sanctioned validating mint, so `LegalLink` re-crosses every foreign edge; a raw `ElementGraph.Of` over foreign edges is the deleted form, because it freezes a topology no structural law admitted.
- Law: the three refusal axes are the source set being EMPTY, a source `Header.Tolerance` differing BITWISE from the coordination tolerance, and a source `Header.Reference` differing STRUCTURALLY from the coordination reference under `GeoReference`'s own value equality; each fault detail names the source tag and both sides' values.
- Law: id collision discriminates by MINTING REGIME, not by payload alone — a rooted OCCURRENCE id (`Node.Object { Kind: Occurrence }`) shared across two sources is ALWAYS `DeltaConflict`, because a Guid-v7 placement identity carries no content preimage and a repeat is corruption; a content-derived or type-derived id repeats legitimately, so equal payloads under `EqualityComparer<Node>.Default` merge as the dedup the id regime exists for and unequal payloads fault naming the id and both source tags.
- Law: an edge JOINS an `Extract` slice only when EVERY id in its `Members` is inside the closure, and the closure is what guarantees it: expansion follows `DirectedPairs` DOWNWARD (whole→part, subject→definition, from→to) and pulls in each reached edge's FULL `Members`, so a buried `PropertyValue.Reference` target and a `Connect`'s realizing intermediary ride in with the edge and no slice can dangle.
- Output: `FederationCensus` is a census, never graph content — per source the tag, the snapshot `ContentAddress`, the source header's provenance columns (schema, model view, instant, STEP name), and the node and edge counts; then the union totals and the merged tally the dedup produced, derived from the rows against the union so it cannot disagree with what the graph holds.
- Packages: LanguageExt.Core (`Seq`/`Option`/`Fin`/`Validation` + the tuple `.Apply` join and the `.Traverse` run fold), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph` the per-source snapshot key), BCL inbox (`BitConverter.DoubleToInt64Bits` the bitwise tolerance comparison).
- Growth: a new coordination axis is one refusal slot beside the tolerance and reference gates; a new union law is one arm in `Unify`; a new slice direction is one predicate on the frontier expansion — never a second union entrypoint and never a per-source header column on the graph.
- Boundary: `Connect.Interface` is a `UInt128` blob key riding no `Members`, so an extracted slice carries the key while its blob resolution stays SOURCE-bound through the owning `GeometrySource` port; a slice does not copy geometry, and a consumer resolving an extracted interface reaches the source's own store.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record FederationSource(
    string Tag, ContentAddress Address, ReleaseVersion Schema, ModelView View, Instant At, string Step,
    int NodeCount, int EdgeCount);

public sealed record FederationCensus(
    Seq<FederationSource> Sources, int NodeCount, int EdgeCount, int MergedNodes, int MergedEdges);

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed partial class ElementGraph {
    public static Fin<(ElementGraph Graph, FederationCensus Census)> Federate(
    Seq<(string Source, ElementGraph Graph)> sources, Header coordination) =>
    Admitted(sources, coordination).ToFin().Bind(union =>
    (GraphDelta.Empty with { AddedNodes = union.Nodes, AddedEdges = union.Edges })
    .Reheader(coordination)
    .AdmitOnto(Genesis(coordination))
    .Map(step => (Graph: step.Graph, Census: CensusOf(sources, union.Nodes, union.Edges))));

    static Validation<Error, (Seq<Node> Nodes, Seq<Relationship> Edges)> Admitted(
    Seq<(string Source, ElementGraph Graph)> sources, Header coordination) =>
    (Gate(sources.Count > 0, "<federate-empty-source-set>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
     Accumulate(sources.Map(source => Aligned(source, coordination))),
     Unified(sources))
    .Apply(static (_, _, union) => union).As();

    static Validation<Error, Unit> Aligned((string Source, ElementGraph Graph) source, Header coordination) =>
    Accumulate(Seq(
    Gate(source.Graph.Header.SameGrid(coordination),
    $"<federate-tolerance-divergent:{source.Source}:{source.Graph.Header.Tolerance:R}:{coordination.Tolerance:R}>",
    static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(source.Graph.Header.Reference.Equals(coordination.Reference),
    $"<federate-reference-divergent:{source.Source}:{source.Graph.Header.Reference.Resolution.Key}:{coordination.Reference.Resolution.Key}>",
    static (k, d) => (Error)new ElementFault.ValueRejected(k, d))));

    static Validation<Error, (Seq<Node> Nodes, Seq<Relationship> Edges)> Unified(
    Seq<(string Source, ElementGraph Graph)> sources) =>
    toSeq(sources.Bind(static source => toSeq(source.Graph.Nodes.Values).Map(node => (Tag: source.Source, Node: node)))
    .AsEnumerable()
    .GroupBy(static claim => claim.Node.Id))
    .Traverse(group => Unify(toSeq(group)))
    .As()
    .Map(nodes => (Nodes: nodes.Strict(), Edges: Edged(sources)));

    static Validation<Error, Node> Unify(NodeId id, Seq<(string Tag, Node Node)> claims) =>
    claims.Tail.Find(claim => Collides(claims[0].Node, claim.Node)).Map(static claim => claim.Tag)
    is { IsSome: true, Case: string rival }
    ? new ElementFault.DeltaConflict($"<federate-node-collision:{id.ToValue()}:{claims[0].Tag}:{rival}>")
    : claims[0].Node;

    static bool Collides(Node held, Node rival) => Replayed(held) || Diverged(held, rival);

    static bool Replayed(Node held) => held is Node.Object { Kind: var kind } && kind == ObjectKind.Occurrence;

    static bool Diverged(Node held, Node rival) => !EqualityComparer<Node>.Default.Equals(held, rival);

    static Seq<Relationship> Edged(Seq<(string Source, ElementGraph Graph)> sources) =>
    toSeq(sources.Bind(static source => toSeq(source.Graph.Edges))
    .AsEnumerable()
    .Distinct(EqualityComparer<Relationship>.Default));

    static FederationCensus CensusOf(
    Seq<(string Source, ElementGraph Graph)> sources, Seq<Node> nodes, Seq<Relationship> edges) {
    Seq<FederationSource> rows = sources.Map(static source => new FederationSource(
    Tag: source.Source,
    Address: ContentAddress.OfGraph(source.Graph),
    Schema: source.Graph.Header.Schema,
    View: source.Graph.Header.View,
    At: source.Graph.Header.At,
    Step: source.Graph.Header.Step.Name,
    NodeCount: source.Graph.Nodes.Count,
    EdgeCount: source.Graph.Edges.Length));
    return new FederationCensus(
    Sources: rows,
    NodeCount: nodes.Count,
    EdgeCount: edges.Count,
    MergedNodes: rows.Fold(0, static (sum, row) => sum + row.NodeCount) - nodes.Count,
    MergedEdges: rows.Fold(0, static (sum, row) => sum + row.EdgeCount) - edges.Count);
    }

    public Fin<ElementGraph> Extract(Seq<NodeId> roots) =>
    roots.Find(root => !Nodes.ContainsKey(root)) is { IsSome: true, Case: NodeId absent }
    ? new ElementFault.NodeAbsent($"<extract-root-absent:{absent.ToValue()}>")
    : Sliced(roots);

    Fin<ElementGraph> Sliced(Seq<NodeId> roots) {
    ImmutableHashSet<NodeId> closure = Closure(ImmutableHashSet.CreateRange(roots), roots.Distinct());
    return (GraphDelta.Empty with {
    AddedNodes = toSeq(closure).Choose(id => Find(id)),
    AddedEdges = toSeq(Edges).Filter(edge => edge.Members.ForAll(closure.Contains)),
    })
    .Reheader(Header)
    .AdmitOnto(Genesis(Header))
    .Map(static step => step.Graph);
    }

    ImmutableHashSet<NodeId> Closure(ImmutableHashSet<NodeId> admitted, Seq<NodeId> frontier) =>
    frontier.IsEmpty
    ? admitted
    : Reached(admitted, frontier) is var next && next.IsEmpty
    ? admitted
    : Closure(admitted.Union(next), next);

    Seq<NodeId> Reached(ImmutableHashSet<NodeId> admitted, Seq<NodeId> frontier) =>
    toSeq(frontier
    .Bind(node => toSeq(EdgesAt(node))
    .Filter(edge => edge.DirectedPairs.Exists(leg => leg.From == node))
    .Bind(static edge => edge.Members))
    .AsEnumerable()
    .Distinct())
    .Filter(member => !admitted.Contains(member));
}
```

## [05]-[IMPLEMENTATION_LAW]

- [DERIVED_ELEMENT]: `Bake` folds the reachable subgraph into the `Element`, never a second stored record — a parallel per-peer element record is the deleted form; the fold reads the incidence edges, resolves the typed node payloads, applies the NAMED type→occurrence inheritance once, surfaces the inherited `Component` as `Element.Type`/`Element.TypeId`, and recurses the OWNING `Compose` children (`Aggregate`/`Nest`/`Contain`, never the non-owning `Reference`), so "has it all" is one flat read and a graph edit re-bakes in O(1) against the per-snapshot memo.
- [GRAPH_PHASE_SPLIT]: PHASE splits the graph — live authoring/delta rides the `TrackingHashMap` HAMT (`Graph/delta#GRAPH_DELTA` owns it) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` + the incidence index + the `QuikGraph` view + the `Bake` memo, all built once at the freeze boundary), so the working graph is never confused with the read snapshot and the freeze boundary is where the analytical structures materialize.
- [NODE_RUN_ORDER]: `FrozenDictionary` declares NO enumeration order, so no byte-deriving reader walks `Nodes.Values` raw — `Projection/address#CONTENT_ADDRESS` `OfGraph` sorts node addresses ascending — while `Edges` publishes recording order off the ordered carrier `Graph/delta#GRAPH_DELTA` fixes.
- [INCIDENCE_INDEX]: incidence keys by every node an edge's `Members` touches — a `Connect`'s realizing intermediary resolves through `EdgesAt`, consistent with `Touches` and the `DropNode` cascade — so `Bake` reads edges in O(degree), built once per snapshot.
- [TOPOLOGY_VIEW]: `View(EdgeFilter, EdgeOrientation)` is the ONE scoped topology — memoized per row pair at the snapshot, built from each admitted edge's oriented `DirectedPairs` legs over shared `TypedEdge` (the leg carries its `Relationship`), so reachability traverses THROUGH a realizing intermediary, a kind-scoped walk reads the edge off the leg through `AlgorithmExtensions`, and an ancestry climb takes the `Ascending` orientation; `Topology()` is the `(All, Forward)` one-hop; ROOTED spatial ancestry is Bim `SpatialStructure.Ancestry`'s alone (E-E9) because the `Compose` graph is multi-parent and the precedence law lives there.
- [IDENTITY_AND_HASH]: `NodeId.Of(NodeSeed)` OWNS identity ALONE over one regime dispatch the `NodeSeed` witness makes recoverable from the value — an OCCURRENCE `Object` a Guid-v7 `Placement` id (sortable), a TYPE `Object` a DETERMINISTIC streamed digest over the `WriteIdentity` segments (`TypeSeed`, the SAME kernel seed-zero hasher every arm rides), a non-rooted node a streamed `Content` digest over its full `CanonicalBytes`, and `Precomputed` the own-self-hash wrap; the compressed IFC GlobalId is a Bim-stored projection attribute re-emitted at `Emit`. The Object layout is FIVE frozen segments — the `WriteIdentity` three plus the two volatile blocks (`Representations`, secondary `Classifications`) interleaved at fixed positions — so the Type seed excludes exactly the volatile blocks (the PRIMARY `Classification` stays as entity-class identity), identical `Component`s dedup to one Type, a later geometry attach or standard-classification stamp never re-keys it, and the FULL projection stays byte-for-byte stable — `CanonicalBytes(w)` is the ONE canonical projection the id mint, the `Projection/address#CONTENT_ADDRESS` diff, and the delta fold share (fixed IEEE-754 LE bits, measures quantized to `Header.Tolerance`, explicit attribute order, id excluded), so a node's content identity is stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed — and the Type seed is a .NET-side mint a peer READS as an opaque rooted id, never re-derives. Generated `NodeWire` support carries one node id verbatim for the persistence edit boundary. Every `PropertySet`/`QuantitySet`-bearing content key derives from the COUNTED bag layout — `Ordinal(count)` before the sorted rows, the `Projection/address#IMPLEMENTATION_LAW` count-prefix law — the cross-runtime wire law the queued Python/TypeScript canonical-writer mirrors reproduce; an uncounted bag run is the deleted injectivity hole (a trailing run parsing as a prefix of the next raw-append segment).
- [TYPE_INHERITANCE]: `Bake` resolves the named type→occurrence inheritance from the `Relations/relation#EDGE_ALGEBRA` `Assign.TypeDefinition` bind — the `Component` projection (the owner that mints its Type) authors the occurrence→Type edge, and `Bake`'s `TypeResolutionOf` folds the `Component`'s standardized data (the property/quantity bags, the `BakedMaterial` set, the `Assessment` payloads, the type `Object`'s single fields, and its secondary classifications) in ONE pass, then merges occurrence-over-type with explicit per-field precedence: single fields occurrence-overrides-type (`PredefinedType`/`Name`/`Representations`/`Appearance` falling back to the type on the IFC unset sentinel, the primary `Classification` the occurrence's own non-blank code), the materials/assessments/classifications `Seq`s union+dedup-by-key (the `MaterialKey` string; the `(Discipline, Route, InputKey)` assessment cache triple; the `(System, Code, Edition)` classification identity). This is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode`, which stays `PropertyBag`-value precedence (the bag `Merge`) and is never extended by the named dimension. `TypeBinding` surfaces the inherited `Component` as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes (the `Rasm.Bim` type-representation round-trip key), and `MaterialsOf` gains a one-hop type-resolved fallback `CompositionOf`/`PropertiesOf`/`SectionOf` compose (a minor part sharing one `Component`'s profile reads its section with no occurrence-direct association) WITHOUT perturbing the FROZEN `SectionOf(member)` signature `Rasm.Compute` reads — the fallback is a single type-hop (a `Component` is not itself typed), never a recursive type chain.
- [STRUCTURAL_EQUALITY]: `[Equatable]` owns deep equality for `ElementGraph`, every nested `Node` and `Relationship` CASE, and every drillable intermediate payload — the union roots carry no seat, because a root seat is the compile-proven silent form whose case members reference-compare — so `Inequalities(before, after)` localizes changes below the node map and member-grain drill into a case runs that case's own comparer after discrimination. `MeasureValue` and `PropertyValue` are atomic record-value leaves. Sealed, `ElementGraph` excludes the incidence index, topology, and bake memo from equality and exposes no record copy that aliases caches. Three member policies are DECLARED rather than inherited, because each one agrees with the canonical projection by inheritance alone and a member edit breaks that agreement with no signal. `[StringEquality(StringComparison.Ordinal)]` binds every string the `CanonicalWriter` writes verbatim — a culture-sensitive or case-insensitive comparer rules two nodes equal whose canonical bytes differ, forking equality from content identity at the one place the merge and the id mint must agree. `[UnorderedEquality]` on `Nodes` routes to `DictionaryEqualityComparer<NodeId, Node>` because `FrozenDictionary<TKey,TValue>` implements `IDictionary<TKey,TValue>` — key-matched entry comparison with `EqualityComparer<Node>.Default` on the value side dispatching each case's generated `Equals` override, NOT a `KeyValuePair` multiset, whose element comparison falls to reflective `ValueType.Equals`; the same comparer keys every `Distinct`/`GroupBy`/`HashSet` reuse outside generated code, so a fold deduplicating nodes never spells a second equality. `[PrecisionEquality]` is REFUSED on every float-bearing member here on two structural grounds, not preference: the generator omits precision members from `GetHashCode` ENTIRELY, so a payload distinguished only by tolerance-compared scalars hashes to one bucket across the whole graph; and every double this page carries is either a `MeasureValue` already quantized to `Header.Tolerance` by the canonical projection — a second tolerance beside it forks the one quantization the cross-runtime parity corpus depends on — or an `AppearanceSummary` channel that is PREIMAGE to a frozen content key, where tolerance-equality rules two nodes equal whose `AppearanceKey`s differ and breaks the content-address contract outright.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
