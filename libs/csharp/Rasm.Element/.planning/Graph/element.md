# [ELEMENT_GRAPH]

`ElementGraph` IS the authoritative thing — `Header` + `Nodes: FrozenDictionary<NodeId, Node>` + `Edges: ImmutableArray<Relationship>` + a built-once incidence index — and the consumer-facing `Element` DERIVES as the FOLD `Bake(objectNode)` over the reachable subgraph, never a second stored record. `Bake` lands material, property/quantity bags, assessments, observation series, appearance, coverages, composed parts, and the inherited `Component` (`Element.Type`/`Element.TypeId`) as flat fields a consumer reads in one hop — "has it all" is one fold, not a join across ten owners.

`ElementGraph` mirrors IFC as a property graph: every IFC entity is a `Node`, every IFC relationship a `Relations/relation#EDGE_ALGEBRA` `Relationship`, and the consumer reads neither — it reads the baked `Element`. `NodeId` OWNS identity over one regime: an OCCURRENCE `Object` carries Guid-v7 placement identity (the compressed IFC GlobalId is a Bim-stored attribute re-emitted at `Emit`), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its volatile-excluded canonical seed, and a non-rooted node a content hash over its full canonical bytes — never a second identity scheme.

PHASE splits the graph: live authoring rides the `TrackingHashMap` HAMT `Graph/delta#GRAPH_DELTA` owns, and `ElementGraph` is the FROZEN read snapshot — `ToFrozenDictionary`, the incidence index, the memoized `Bake`, and the `QuikGraph` view, all built once at the freeze boundary. `Projection/fault#FAULT_BAND` `ElementFault` rails every missing node and every structural violation.

## [01]-[INDEX]

- [02]-[NODE_MODEL]: `NodeId.Of(NodeSeed)` one-regime identity over the `NodeSeed` mint-regime witness — Guid-v7 placement, deterministic type seed, streamed content hash, precomputed wrap — the `Node` `[Union]` property-graph vocabulary with its shared `CanonicalBytes` projection and `Seed(tolerance)` witness, the node-payload component types, and the `RepresentationSlot`-rowed analytical-geometry decode behind the `GeometrySource` port.
- [03]-[ELEMENT_GRAPH]: `ElementGraph` frozen read snapshot with its built-once incidence index and `QuikGraph` view, the memoized `Bake` fold deriving `Element` under the named type→occurrence inheritance, and the incidence accessor family — sections, materials, containment, groups, measured-series rollups.
- [04]-[FEDERATION]: `Federate` unions N tagged source graphs onto one coordination `Header` with collision discrimination and a `FederationReceipt`; `Extract` slices the downward closure of a root set into its own graph under the SOURCE `Header`.

## [02]-[NODE_MODEL]

- Owner: `NodeId` the `[ValueObject<string>]` identity owner over the `IObjectFactory` floor, minting through the ONE `Of(NodeSeed)` dispatch; `NodeSeed` the `[Union]` mint-regime witness (`Placement`/`TypeSeed`/`Content`/`Precomputed`) `Node.Seed(tolerance)` publishes and the address `Verify` dual reads; `Node` the `[Union]` eight-case property-graph vocabulary carrying the shared `CanonicalBytes` writer projection; the node-payload component types the cases compose.
- Cases: the closed eight-case property-graph node family — `Object` · `Material` · `PropertySet` · `QuantitySet` · `Assessment` · `Appearance` · `Coverage` · `Observation`.
- Cases: `Object` is the IfcObjectDefinition mirror: `ObjectKind` occurrence/type, optional `ExternalId` (the Bim-stored IFC GlobalId, re-emitted at `Emit`), first-class `PredefinedType` token value-object, name/tag, optional `OwnerHistory`, schema `SchemaSpan`, and NO `GeoReference` (model georeferencing is a `Header` fact).
- Cases: `Object` carries TWO classification columns — the primary `Classification` (the entity-class-keying pair every query, egress, and diff reads) beside the `Classifications` set of additional standard-system references, because IFC permits MULTIPLE `IfcRelAssociatesClassification` per object (Uniclass and OmniClass simultaneously) and a single field is lossy.
- Cases: `Object`'s `RepresentationContentHash` keyed map content-hashes EVERY geometry — the heavy display `Body` AND the lightweight analytical `Axis` (idealized structural line) and `FootPrint` (space-boundary surface polygon) a discipline resolves by content key — never inline coordinates.
- Cases: `Material` carries a `Composition/material#MATERIAL_COMPOSITION` `MaterialId` with its composition and property sets; `PropertySet`/`QuantitySet` a `Properties/property#PROPERTY_BAG` named bag with its `InheritanceMode`; `Assessment` an `Assessment/assessment#ASSESSMENT_NODE` receipt; `Appearance` a content-keyed `AppearanceSummary`; `Coverage` a `Geospatial/coverage#COVERAGE_NODE` raster/field grid.
- Cases: `Observation` carries an `Assessment/observation#OBSERVATION_SERIES` measured sensor series — the computed assessment's sibling evidence modality, its samples content-keyed by reference.
- Entry: `NodeId.Of(NodeSeed)` is the ONE mint over the regime witness — `Placement` a sortable Guid-v7 for an OCCURRENCE `Object`; `TypeSeed(component, tolerance)` the deterministic Type id STREAMED over the volatile-excluded `WriteIdentity` segments through the one tolerance-bound `ContentAddress` entry, so identical `Component`s dedup to one Type with no byte materialization; `Content(node, tolerance)` the non-rooted content hash streamed over `node.CanonicalBytes(w)`; `Precomputed(address)` the no-rehash wrap ONLY for a node's OWN content self-hash, never a foreign key like an `Assessment.InputKey` (a payload field the canonical fold treats as content); `node.Id` reads any case's id through the abstract override; `node.CanonicalBytes(w)` projects the case's semantic content (NO id) into the writer the id mint, the `Projection/address#CONTENT_ADDRESS` diff, and the delta fold SHARE; `node.Seed(tolerance)` publishes the witness `Verify` dispatches on.
- Auto: each case carries `NodeId Id` as a positional override of the union's abstract `Id`, so `node.Id` reads without a switch; `CanonicalBytes` dispatches the generated total `Switch` writing each case's semantic content (an `Object` its kind/classification/predefined/name/tag/representations/span; a `Material` its key/composition/properties; a bag its set name, inheritance key, and count-prefixed sorted name→value entries, a quantity bag its count-prefixed sorted `GroupIdentity` run beside them; a measure quantized to the tolerance) into the kernel `CanonicalWriter` under the `Projection/address#IMPLEMENTATION_LAW` codec, the id excluded so a non-rooted node's id derives from its own bytes without circularity; a rooted `Object` mints its id once at authoring — an OCCURRENCE its Guid-v7 placement identity, a TYPE its DETERMINISTIC mint over the `WriteIdentity` segments (the volatile `Representations` AND secondary `Classifications` excluded so a later geometry attach or standard-classification stamp never re-keys the Type and identical `Component`s dedup to one Type) — the IFC GlobalId staying a Bim-stored projection attribute re-emitted at `Emit`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`IObjectFactory`), LanguageExt.Core (`Option`/`Seq`/`Map`), NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-hash entry the `NodeSeed.Content` mint composes). `Rasm.Element.Graph` OWNS the neutral `Vector3` the `AxisCurve`/`FootprintPolygon` analytical shapes carry (the kernel `Rasm.Numerics` coordinate is the host `Vector3d` the seam Boundary forbids; no neutral kernel triple exists), so the seam mints its own host-free coordinate AND its full vector algebra (`Length`/`Distance`/`Dot`/`Cross`/`Unit` + the `UnitX`/`UnitY`/`UnitZ`/`Zero` constants + the `+`/`-`/`*` operators) — the `Rasm.Bim` scan-to-BIM orientation classifier (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) and the `Rasm.Compute` structural load-vector folds compose THIS one coordinate rather than a kernel/host vector, so a phantom kernel `Vector3` or a `System.Numerics.Vector3` crossing the analytical-shape math is the deleted host leak.
- Growth: a new node concept is one `Node` case carrying its payload type, the payload owning its own `CanonicalBytes` contribution so the arm is one ordinal and one delegation (the `Observation` series landed exactly this way; a `Schedule`/`Task` node lands here only if 4D becomes a real target); a new object axis is one column on the `Object` case; a new node-payload component is one type on its owning sibling page; never a parallel node family and never a second identity scheme — the `NodeId` is the one owner, `MaterialId` a node attribute, not a parallel key. New object columns land with their wire field and their presence-delimited `CanonicalBytes` contribution in one edit under the additive contract-evolution law — `ObjectType` is the landed instance, carrying the IFC-canonical `(PredefinedType = USERDEFINED, ObjectType = label)` designation for BOTH object kinds — the Bim `Projection/semantic` `UserLabel` ingress reads it off `IfcObject.ObjectType` or `IfcElementType.ElementType` and `Projection/egress` `StampPredefined` re-stamps the matching slot, one column for the exact round-trip.
- Boundary: `NodeId` is the ONE identity owner: occurrence roots use Guid-v7 placement identity, type roots hash the representation-excluded type seed, and non-rooted nodes hash full canonical content. `Object` carries the primary and co-applied classifications, `PredefinedType`, content-keyed representations, owner history, and schema span; geometry stays behind `GeometrySource`, model georeferencing stays on `Header`, and IFC rosters stay in the Bim projector. `CanonicalBytes(w)` is the shared id/diff projection, and bag source rank participates in property/quantity node identity. `AppearanceSummary` is FROZEN at its seven-value preimage, carried as ONE `AppearanceVector` admitted through `Of(vector, key) -> Fin<AppearanceSummary>`: a peer carrying a richer appearance fact — a baked texture-set key, an environment binding, a UV transform, a measured refractive index — hangs it behind the `AppearanceKey` on its own wire, because an eighth column re-keys every stored `Node.Appearance` and forks the Bim dedup key in the same edit.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

// --- [TYPES] ------------------------------------------------------------------------------
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

    // Of is the ONE mint over the NodeSeed regime witness — the regime is recoverable from the VALUE, so the
    // address Verify dual re-mints by the same dispatch instead of re-deriving a regime from node.Kind. Placement
    // mints the sortable random Guid-v7 an OCCURRENCE roots on (its identity IS its unique placement, never an IFC
    // GlobalId — that is a Bim-stored attribute); TypeSeed mints the DETERMINISTIC Type id over the volatile-excluded
    // canonical seed so identical Components dedup to one Type; Content hashes a non-rooted node's full canonical
    // bytes; Precomputed wraps a node's OWN content self-hash carried forward — never a foreign key like an
    // Assessment.InputKey, which the node's own canonical fold treats as payload and Verify could never reproduce.
    // All content-shaped arms ride the kernel seed-zero XxHash128 — the ONE hasher.
    public static NodeId Of(NodeSeed seed) => seed.Switch<NodeId>(
    placement: static _ => Create(Guid.CreateVersion7().ToString("N")),
    typeSeed: static s => Minted(ContentAddress.Of(s.Node, s.Tolerance, static (n, w) => Node.WriteIdentity(w, n))),
    content: static s => Minted(ContentAddress.Of(s.Node, s.Tolerance, static (n, w) => n.CanonicalBytes(w))),
    precomputed: static s => Minted(s.Address));

    private static NodeId Minted(ContentAddress address) =>
    Create(address.Value.ToString("X32", System.Globalization.CultureInfo.InvariantCulture));
}

// The mint-regime witness: WHICH law minted a node's id, recoverable from the value — Node.Seed(tolerance)
// publishes it and Projection/address#CONTENT_ADDRESS Verify dispatches on it (Placement vacuous, every other arm
// re-mints and compares). A TRANSIENT witness, never persisted or diffed, so no Generator.Equals seat applies (the
// GRAPH_FAMILY drill law binds stored family members alone). Precomputed exists for the mint side only — a verify
// re-derivation always re-projects the Content fold, so a foreign precomputed key can never verify. The
// content-shaped cases carry (node, tolerance) and STREAM through the one tolerance-bound ContentAddress entry —
// no byte materialization rides a mint.
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

// PredefinedType seats the IFC predefined-type token as a first-class typed value on the Object node (C6): the SEAM owns it
// (Bim retired its copy), VALIDITY is a Bim EGRESS gate — Emit resolves the IfcClass row from the classification
// code and runs AdmitPredefined against the frozen valid set, never a seam invariant. NotDefined is the IFC default.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PredefinedType {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim().ToUpperInvariant();
    public static readonly PredefinedType NotDefined = Create("NOTDEFINED");
    public string Token => Value;
}

// StepHeader carries the ISO 10303-21 STEP header on the model Header — the FILE_DESCRIPTION/FILE_NAME/FILE_SCHEMA sections
// in full so an IFC import→export cycle preserves the provenance (authors, timestamp, preprocessor, schema) the
// Bim projector reads from DatabaseIfc; a skeletal three-string header is the lossy form.
public readonly record struct StepHeader(
    Seq<string> Descriptions, string Name, Instant TimeStamp, Seq<string> Authors,
    Seq<string> Organizations, string Preprocessor, string OriginatingSystem, Seq<string> Schema) {
    public static readonly StepHeader Empty = new(Seq<string>(), "", default, Seq<string>(), Seq<string>(), "", "", Seq<string>());
}

// IFC owner-history carried optionally on ROOTED nodes, re-emitted with a diff-derived ChangeAction;
// Modified is None until a first revision so a never-modified entity carries no sentinel timestamp.
public readonly record struct OwnerHistory(string OwningUser, string OwningApplication, Instant Created, Option<Instant> Modified, string ChangeAction, string State);

// SchemaSpan bounds the schema versions a node is valid across, validated at Emit against Header.ReleaseVersion.
public readonly record struct SchemaSpan(ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn) {
    public static SchemaSpan From(ReleaseVersion introduced) => new(introduced, None);
}

// RepresentationContentHash references geometry through a keyed map RepresentationSlot → content hash (M2), neutral-named (no IFC leak).
// EVERY geometry — the heavy display Body AND the analytical Axis/FootPrint — rides the blob store by content hash and
// resolves one-hop by key, NEVER inline coordinate geometry on the node. Body/Axis/Box/FootPrint are the standard
// IFC RepresentationIdentifier reads; an absent identifier is None.
public readonly record struct RepresentationContentHash(Map<RepresentationSlot, UInt128> ByIdentifier) {
    public static readonly RepresentationContentHash Empty = new(Map<RepresentationSlot, UInt128>());
    public Option<UInt128> At(RepresentationSlot slot) => ByIdentifier.Find(slot);
    public RepresentationContentHash With(RepresentationSlot slot, UInt128 hash) =>
        this with { ByIdentifier = ByIdentifier.AddOrUpdate(slot, hash) };
}

// The representation vocabulary as ROWS — all eleven contract kinds live on this one owner, so the in-process map
// and generated RepresentationKind stay closed together. Decode is optional because only Axis and FootPrint have a
// seam analytical carrier; every other kind remains an opaque blob key a specialized consumer resolves.
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

// The typed payload ResolveRepresentation answers with — a decoded analytical shape or the opaque blob key a heavy
// representation stays behind. A TRANSIENT read carrier (never persisted), so no Generator.Equals seat applies.
[Union]
public abstract partial class Representation {
    private Representation() { }

    public sealed partial class Line(AxisCurve curve) : Representation { public AxisCurve Curve { get; } = curve; }
    public sealed partial class Ring(FootprintPolygon polygon) : Representation { public FootprintPolygon Polygon { get; } = polygon; }
    public sealed partial class Blob(UInt128 key) : Representation { public UInt128 Key { get; } = key; }
}

// Vector3 seats the SEAM-OWNED host-neutral coordinate the analytical shapes carry: flat double XYZ, `double`-domain (a
// coordinate is the geometry's native scalar, never a unit-bearing MeasureValue). No neutral kernel triple exists and the kernel
// `Rasm.Numerics` coordinate IS the RhinoCommon `Vector3d` the seam Boundary forbids, so the seam mints this one
// coordinate PLUS its whole algebra (Length/Distance/Dot/Cross/Unit + axis constants) — the Rasm.Bim scan-to-BIM
// orientation classifier (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) and the Rasm.Compute load folds compose it;
// a `System.Numerics.Vector3` (float32) or a host `Vector3d` crossing the analytical-shape math is the deleted host
// leak. A Vector3 lives ONLY inside a decoded AxisCurve/FootprintPolygon, never as an Object-node field. Unit
// DEGENERACY-guards a zero vector to Zero, so a degenerate fitted axis never produces a NaN orientation.
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
    // The epsilon-proximity read every consumer re-derived as Distance-then-compare at its own call site.
    public static bool Near(Vector3 a, Vector3 b, double epsilon) => Distance(a, b) <= epsilon;
}

// RepresentationContentHash content-keys the lightweight ANALYTICAL geometry under "Axis"/"FootPrint" [M2]:
// AxisCurve the idealized structural-member line (start/end + a non-degenerate local up), FootprintPolygon the
// space-boundary surface — one shell ring plus its interior-ring run — the ONE analytical vocabulary every projector
// hashes and every runner's GeometrySource decodes back into (seam-neutral Vector3 only, NEVER a host
// Brep/Mesh/Point3d, NEVER inlined on the Object node), so neither side re-declares a parallel
// MemberAxis/BoundaryPolygon.
public readonly record struct AxisCurve(Vector3 Start, Vector3 End, Vector3 Up) {
    public double Length => Vector3.Distance(Start, End);
}

// Holes carries the interior-ring run beside the shell — a courtyard, atrium, or lightwell is a hole ring, and a
// carrier that cannot express one silently counts every court as conditioned floor area in every energy lower.
// Area is the seam's one Newell fold — shell minus holes, each ring |Σ vᵢ×vᵢ₊₁|/2, planar-exact and axis-free —
// so no consumer re-derives ring arithmetic beside the shape that owns the rings.
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

// PlacementTransform is the object's own placement frame — the IfcAxis2Placement3D triple lowered onto the seam's own
// coordinate: the Location origin, the Axis local-Z direction, and the RefDirection local-X. It is the ONE placement
// fact the graph carries; the geometry stays content-keyed, so a rigid move edits this column and touches no representation.
//
// EXCLUDED from the Object's canonical-bytes preimage by the SAME law that excludes OwnerHistory: a pure rigid move
// must NOT re-key the node. The Rasm.Bim review diff keys placement in its OWN bucket and reads a moved element as
// Moved; folding the frame into content identity would mint a fresh id for every relocation and destroy exactly that
// discriminant, turning every move into an unrelated add-plus-remove pair. The column therefore owns its own
// CanonicalBytes for the diff bucket and the wire to compose, and no Object arm calls it.
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct PlacementTransform {
    public Vector3 Location { get; }
    public Vector3 Axis { get; }
    public Vector3 RefDirection { get; }

    // Apply lifts a LOCAL coordinate through the frame into model space — the one transform every consumer
    // re-derived by hand: unit local axes (Z the Axis, X the RefDirection, Y their right-handed cross) scaled by the
    // local ordinates off the Location origin.
    public Vector3 Apply(Vector3 local) {
        Vector3 z = Axis.Unit;
        Vector3 x = RefDirection.Unit;
        Vector3 y = Vector3.Cross(z, x);
        return Location + (x * local.X) + (y * local.Y) + (z * local.Z);
    }

    // CanonicalBytes projects the frame's own content — nine ordered doubles under the shared IEEE-754 canon, so the
    // diff bucket and the wire read one encoding and a below-tolerance jitter never reads as a distinct frame downstream.
    public void CanonicalBytes(CanonicalWriter w) =>
        w.Double(Location.X).Double(Location.Y).Double(Location.Z)
         .Double(Axis.X).Double(Axis.Y).Double(Axis.Z)
         .Double(RefDirection.X).Double(RefDirection.Y).Double(RefDirection.Z);
}

// GeometrySource seats the geometry-resolution PORT [M2]: the seam owns the CONTRACT (content key -> decoded analytical
// shape), the app wires the IMPLEMENTATION over the Rasm.Persistence object-store byte-stream, and an above-seam runner
// pulls the analytical axis/footprint by `member.Representations.Axis`/`.FootPrint` rather than reading a phantom node
// field. Axis and footprint decode to GENUINELY DISTINCT shapes (a line vs a ring), so the port carries ONE typed decode
// leg per KIND — the discriminant is the return TYPE, not a Get/GetById arity family. A Connect edge's Interface content
// key (the space-boundary/connection surface ring) resolves through the SAME ResolveFootprint leg, never a third port. A
// missing/undecodable blob is None (the runner rails its own typed input-missing fault, never a defaulted coordinate);
// None is the inert wiring a closed-form route threads. ResolveFootprint decodes the interior-ring run alongside the
// shell — a blob whose payload carries hole rings fills Holes and a shell-only blob decodes to an empty run, so a
// consumer never learns hole-ness from a second port.
public readonly record struct GeometrySource(
    Func<UInt128, Option<AxisCurve>> ResolveAxis, Func<UInt128, Option<FootprintPolygon>> ResolveFootprint) {
    public static readonly GeometrySource None = new(static _ => Option<AxisCurve>.None, static _ => Option<FootprintPolygon>.None);
    public Option<AxisCurve> Axis(RepresentationContentHash representations) => representations.At(RepresentationSlot.Axis).Bind(ResolveAxis);
    public Option<FootprintPolygon> Footprint(RepresentationContentHash representations) => representations.At(RepresentationSlot.FootPrint).Bind(ResolveFootprint);

    // ResolveRepresentation is the ONE typed content-key→geometry entry (S-E8) — the slot row owns the decode, and
    // an absent identifier rails APART from an unresolvable blob so a consumer distinguishes never-authored from
    // store-miss instead of reading both as one None.
    public Fin<Representation> ResolveRepresentation(Node.Object node, RepresentationSlot slot, Op key) =>
        node.Representations.At(slot).Match(
            Some: hash => slot.Decode(this, hash).Match(
                Some: Fin.Succ,
                None: () => Fin.Fail<Representation>(new ElementFault.ValueRejected(key, $"<representation-unresolvable:{slot.Key}:{node.Id.Value}>"))),
            None: () => new ElementFault.ValueRejected(key, $"<representation-absent:{slot.Key}:{node.Id.Value}>"));
}

// Appearance node summary: a content-keyed reference to the full BSDF (authored in Rasm.Materials) plus the neutral
// canonical PBR scalars a consumer reads flat. The SEAM owns the AppearanceKey derivation through Of, so the
// Rasm.Materials and Rasm.Bim lowerings compose ONE factory and mint the SAME key for one surface (a local
// CanonicalWriter beside this factory in either peer is the byte-order divergence defect). Transmissive is the
// REFRACTIVE flag DISTINCT from Opacity (an opaque-alpha glass still transmits — the GLB KHR_materials_transmission
// read); both are load-bearing in the KEY, so two appearances differing only in alpha or refraction get distinct
// Node.Appearance ids even though the appearance canonical arm hashes only the AppearanceKey.
//
// AppearanceVector carries the seven-value neutral PBR vector as ONE value — four 8-arg signatures repeated the
// column run before it existed. The ORDER of its members IS the frozen preimage order.
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

    // Flat channel reads — the pre-vector column spellings every wire encode and Bim lowering keeps reading.
    public double BaseColorR => Vector.BaseColorR;
    public double BaseColorG => Vector.BaseColorG;
    public double BaseColorB => Vector.BaseColorB;
    public double Metallic => Vector.Metallic;
    public double Roughness => Vector.Roughness;
    public double Opacity => Vector.Opacity;
    public bool Transmissive => Vector.Transmissive;

    // Of IS the ONE seam-owned appearance content-key factory both the Rasm.Materials MaterialWire.Summary lowering
    // and the Rasm.Bim AppearanceProjection.Project lowering compose: write the neutral PBR vector through the seam
    // CanonicalWriter and mint the AppearanceKey via ContentAddress.Of (the kernel seed-zero XxHash128, the ONE
    // hasher). The six unit channels admit through ONE Indexed span scan, each offender named by slot index and
    // value. Tolerance 0.0 hashes the raw IEEE bits — appearance scalars are not Header-quantized measures — and the
    // writer canonicalizes -0.0/NaN/inf, so the key is cross-runtime stable.
    public static Fin<AppearanceSummary> Of(AppearanceVector vector, Op key) =>
        Indexed(
            [vector.BaseColorR, vector.BaseColorG, vector.BaseColorB, vector.Metallic, vector.Roughness, vector.Opacity],
            static channel => channel is >= 0.0 and <= 1.0, key, "appearance-channel")
         .Map(_ => Minted(vector))
         .ToFin();

    // The decode-side dual: re-mint and compare, so a persisted summary whose stored key disagrees with its own
    // channel bytes refuses instead of entering the graph wearing a foreign identity.
    public static Fin<AppearanceSummary> Rehydrate(UInt128 appearanceKey, AppearanceVector vector, Op key) =>
        Of(vector, key).Bind(summary =>
            summary.AppearanceKey == appearanceKey
                ? Fin.Succ(summary)
                : new ElementFault.AddressUnstable(key, "<appearance-key-mismatch>"));

    // THE SEVEN-VALUE PREIMAGE IS FROZEN: BaseColorR/G/B, Metallic, Roughness, Opacity, Transmissive, in that order
    // — an eighth column re-keys every Node.Appearance in every stored snapshot AND forks the Rasm.Bim dedup key, so
    // a richer appearance fact (a baked texture-set key, an IBL binding, a UV transform, a measured IOR) rides the
    // payload BEHIND this key on the peer's own wire, never a column here. Streams through the ONE tolerance-bound
    // ContentAddress entry — no byte materialization.
    private static AppearanceSummary Minted(AppearanceVector vector) =>
        new(ContentAddress.Of(vector, 0.0, static (v, w) =>
            w.Double(v.BaseColorR).Double(v.BaseColorG).Double(v.BaseColorB)
             .Double(v.Metallic).Double(v.Roughness).Double(v.Opacity).Bool(v.Transmissive)).Value, vector);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Node declares a CLASS-root [Union] (the [GRAPH_FAMILY] form), NOT a record-root: equality and the member-level structured diff
// ride Generator.Equals [Equatable] seated PER NESTED CASE — a root seat is the compile-proven silent form whose
// case members reference-compare while only root-declared members generate. Each case's generated Equals override
// makes the polymorphic verdict correct, so the Nodes map's DictionaryEqualityComparer and every
// EqualityComparer<Node>.Default fold read case equality; member-grain drill into a changed node runs the CASE's
// own EqualityComparer.Inequalities after discrimination, because a slot typed as the abstract root is an
// equality leaf no compile-time projection can descend. Collection members carry [UnorderedEquality] so bag and
// set semantics nest rather than falling to reference identity; the intermediate payload owners carry [Equatable]
// for the descent and the drill BOTTOMS at the native value-equality leaves MeasureValue/PropertyValue, which
// carry NEITHER [Equatable] nor a deeper descent ([04] STRUCTURAL_EQUALITY owns the full drill law). Each case is
// a sealed CLASS exposing NodeId Id as a positional override of the union's abstract Id.
[Union]
public abstract partial class Node {
    private Node() { }

    public abstract NodeId Id { get; init; }

    // PascalCase primary-ctor parameters: every projector constructs this case with PascalCase NAMED arguments
    // (Id:, Kind:, ...) — named args bind to PARAMETER names, so the parameters carry the corpus spelling and the
    // same-name property initializers read the shadowing parameter (the C# primary-ctor idiom records generate).
    [Equatable]
    public sealed partial class Object(
    NodeId Id, ObjectKind Kind, Option<string> ExternalId, Classification Classification, PredefinedType PredefinedType,
    Option<string> ObjectType, string Name, string Tag, RepresentationContentHash Representations,
    Option<OwnerHistory> History, SchemaSpan Span, Seq<Classification> Classifications = default,
    Option<PlacementTransform> Placement = default) : Node {
        // [IgnoreEquality] AT THE OWNER: the NodeId re-mints per ingest, so every direct node-vs-node structured
        // diff (the egress ChangeAction verdict, the reimport Reconcile) matches on stable identity and must not
        // read a fresh id as a member change — and under the graph's keyed node map the comparison is vacuous
        // anyway. It binds AT THE OWNER, so a per-consumer member-name filter roster is the deleted form.
        [IgnoreEquality]
        public override NodeId Id { get; init; } = Id;
        public ObjectKind Kind { get; } = Kind;
        public Option<string> ExternalId { get; } = ExternalId;
        public Classification Classification { get; } = Classification;
        public PredefinedType PredefinedType { get; } = PredefinedType;
        // ObjectType carries the IFC-canonical `(PredefinedType = USERDEFINED, ObjectType = label)` pair: the label is the
        // node's own user-defined type name, meaningless without its discriminant, so it sits beside it rather than in a bag.
        // Absent for every enumerated PredefinedType, which is why it is `Option` and not an empty-string sentinel; an
        // egress re-deriving the label from `Name` is the deleted form. ONE column serves BOTH kinds — an occurrence's
        // `IfcObject.ObjectType` and a type's `IfcElementType.ElementType` are the same concept on two entity slots,
        // which the Bim ingress reads through one two-arm `UserLabel` and the egress re-stamps through the matching
        // pair; the type signature bag carries no label row, so routing the type side there is the same `Name`
        // substitution under another name. Absence-carrying like `ExternalId`, so no string-equality attribute applies.
        public Option<string> ObjectType { get; } = ObjectType;
        // Ordinal is DECLARED, not inherited: CanonicalWriter.String writes these bytes verbatim and every
        // sort over them is StringComparer.Ordinal, so a comparer drift here would rule two nodes equal whose
        // content ids differ — equality and content identity fork at exactly the members the merge keys on.
        [property: StringEquality(StringComparison.Ordinal)] public string Name { get; } = Name;
        [property: StringEquality(StringComparison.Ordinal)] public string Tag { get; } = Tag;
        public RepresentationContentHash Representations { get; } = Representations;
        public Option<OwnerHistory> History { get; } = History;
        public SchemaSpan Span { get; } = Span;
        [property: UnorderedEquality] public Seq<Classification> Classifications { get; } = Classifications;
        // Placement is the object's frame — carried, diffed in its own bucket, and EXCLUDED from the content-key
        // preimage so a rigid move is a Moved verdict rather than a re-keyed node (WriteObject states the law).
        public Option<PlacementTransform> Placement { get; } = Placement;

        // Whole classification set across BOTH storage columns — the primary entity-class pair and the
        // secondary standard references — deduplicated so a primary repeated in the secondary set reads once. It
        // seats HERE beside the two columns it closes, never on the Classification axis: the split is this node's
        // storage decision, and a read on the axis owner would have to know a shape only this owner declares. ONE
        // pass under the same `(System, Code, Edition)` identity key the Bake union folds on, so the two dedup laws
        // are one; concatenation ORDER carries the precedence, so the primary leads and DistinctBy drops its echo.
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

    // Seed publishes the mint-regime WITNESS the address Verify dual dispatches on. The regime axis is ROOTEDNESS,
    // which only the Object case carries (occurrence = random placement, type = deterministic identity seed) — every
    // payload case is content-minted BY LAW, so the content arm is the stated owner of that default, and a new
    // payload case lands on it by construction rather than by a forgotten edit.
    public NodeSeed Seed(double tolerance) => this is Node.Object o
    ? o.Kind == ObjectKind.Occurrence ? new NodeSeed.Placement() : new NodeSeed.TypeSeed(o, tolerance)
    : new NodeSeed.Content(this, tolerance);

    // CanonicalBytes IS the ONE canonical value projection — the id is EXCLUDED (a non-rooted id derives from these
    // bytes), measures quantize to the writer's tolerance, attribute order is explicit, and the diff + the id mint
    // share it (the mint streams it through ContentAddress.Of; the delta fold composes it inline behind String(id)).
    // Each complex payload delegates to its OWNER's CanonicalBytes so the projection is never re-derived per case;
    // geometry rides the content-hashed Representations map, never inline coordinates. PROVENANCE is excluded —
    // OwnerHistory is a separate additive axis, not content, so a re-stamp never forks the id; lazy caches likewise
    // sit outside the projection.
    public void CanonicalBytes(CanonicalWriter w) =>
    Switch(
    @object: o => WriteObject(w, o),
    // Mechanical and Orthotropic share Discipline.Structural, so the discipline-key sort TIES them and a stable sort would
    // leak Seq insertion order into the node bytes — two [UnorderedEquality]-equal Material nodes minting distinct content
    // ids. The per-property DIGEST tiebreak (each property's own canonical fold through the ONE tolerance-bound
    // ContentAddress entry) is TOTAL, so a same-discipline pair orders identically regardless of insertion order; a
    // material carrying one set per discipline never ties, so its bytes are unchanged.
    material: m => { w.Ordinal(1); w.String(m.MaterialKey.Value); m.Composition.CanonicalBytes(w); w.Ordinal(m.Properties.Count); foreach (var p in m.Properties.OrderBy(static p => p.Discipline.Key, StringComparer.Ordinal).ThenBy(p => ContentAddress.Of(p, tolerance, static (row, k) => row.CanonicalBytes(k)).Value)) { p.CanonicalBytes(w); } },
    // Ordinal(count) prefixes each bag, the self-delimiting precondition every raw-append consumer relies on — ContentAddress.Of(Node)
    // and the GraphDelta node sections concat String(id)+Raw(bytes), so an UNCOUNTED trailing row run would absorb the
    // following segment's bytes (two distinct deltas, one hash): the Projection/address#IMPLEMENTATION_LAW count-prefix law.
    // Property-set arms write NO group run: a property bag nests through the Properties/property#PROPERTY_VALUE Complex
    // case, so its Groups map is empty by construction and a group block here encodes a constant.
    propertySet: p => { w.Ordinal(2); w.String(p.Bag.SetName); w.String(p.Bag.Inheritance.Key); w.Ordinal(p.Bag.Source.Key); w.Ordinal(p.Bag.Values.Count); foreach (var (n, v) in p.Bag.Values.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); v.CanonicalBytes(w); } },
    // Quantity arms append the count-prefixed GROUP run after the value run — the grouping identity is
    // identity-bearing, so two bags carrying identical measures under different Discrimination/Quality/Usage key
    // apart; each of the three Option columns writes presence-first (the Connect.Interface presence-prefix idiom),
    // so an unstated qualifier and an empty spelling can never encode alike.
    quantitySet: q => { w.Ordinal(3); w.String(q.Bag.SetName); w.String(q.Bag.Inheritance.Key); w.Ordinal(q.Bag.Source.Key); w.Ordinal(q.Bag.Values.Count); foreach (var (n, m) in q.Bag.Values.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); w.Measure(m); } w.Ordinal(q.Bag.Groups.Count); foreach (var (prefix, group) in q.Bag.Groups.OrderBy(static e => e.Key, StringComparer.Ordinal)) { w.String(prefix); w.Bool(group.Discrimination.IsSome); group.Discrimination.IfSome(d => w.String(d)); w.Bool(group.Quality.IsSome); group.Quality.IfSome(x => w.String(x)); w.Bool(group.Usage.IsSome); group.Usage.IfSome(u => w.String(u)); } },
    assessment: a => { w.Ordinal(4); a.Payload.CanonicalBytes(w); },
    appearance: a => { w.Ordinal(5); w.U128(a.Summary.AppearanceKey); },
    coverage: c => { w.Ordinal(6); c.Grid.CanonicalBytes(w); },
    // ObservationSeries delegates its STREAM identity (sensor, aspect, quantity triple, sampling, cadence, deployment
    // instant); its chunk run and derived summary stay OUT, so a live stream appending a block mutates this node in
    // place rather than re-keying it on every sample batch.
    observation: o => { w.Ordinal(7); o.Series.CanonicalBytes(w); });

    // The Object canonical layout is FIVE segments at frozen positions (the parity corpus pins the byte order):
    // identity head, the volatile secondary-classifications run, identity mid, the volatile representations run,
    // identity tail. The two consumers are NAMED COMPOSITIONS over those segments instead of one body forked by a
    // boolean a call site has to decode: WriteObject (the full content hash) writes all five; WriteIdentity (the
    // deterministic Type-id seed) writes the identity three — byte-for-byte the prior includeVolatile pair. The
    // volatile blocks attach AFTER a Type is identified (geometry attaches later; a Uniclass/OmniClass stamp lands
    // post-mint and must never re-key the Type — the PRIMARY Classification stays in the seed as entity-class
    // identity). Placement is EXCLUDED from BOTH paths — the OwnerHistory-exclusion law, plus one more: folding the
    // frame would re-key a rigid move and destroy the Rasm.Bim diff's Moved verdict outright; the frame owns its own
    // CanonicalBytes for that bucket and the wire.
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
    w.String(o.PredefinedType.Token)
     .Optional(o.ObjectType, static (label, run) => run.String(label))
     .String(o.Name).String(o.Tag);

    static void IdentityTail(CanonicalWriter w, Node.Object o) =>
    w.String(o.Span.IntroducedIn.Key)
     .Optional(o.Span.RemovedIn, static (removed, run) => run.String(removed.Key));

    // The three-column sort stays EXPLICITLY ordinal — a tuple key under Comparer.Default falls to the culture
    // comparison string.CompareTo hides, and a culture-ordered run forks the bytes across runtimes.
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

    // Re-stamp the node's OWN identity to a SPECIFIC new id, payload intact — the endpoint-alignment re-stamp a
    // Rasm.Persistence Reconcile and a Bim re-identify compose. DISTINCT from Remap: Relabel sets the own id (buried
    // references stay), Remap rewrites EVERY id by a function. A class-root [Union] case has NO compiler-generated
    // `with`, so each arm RECONSTRUCTS its case through the func-form generated total Switch — NOT Map, which takes
    // PRECOMPUTED constant values and cannot carry an allocating per-case reconstruction. Exhaustive over the closed
    // eight-case family, payload carried positionally so a case gaining a field breaks loudly. For a non-rooted node the
    // caller re-mints from the new content (Relabel is the rooted-node/endpoint-alignment rewrite).
    public Node Relabel(NodeId id) => Switch<Node>(
        @object: o => new Object(id, o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.ObjectType, o.Name, o.Tag, o.Representations, o.History, o.Span, o.Classifications, o.Placement),
        material: m => new Material(id, m.MaterialKey, m.Composition, m.Properties),
        propertySet: p => new PropertySet(id, p.Bag),
        quantitySet: q => new QuantitySet(id, q.Bag),
        assessment: a => new Assessment(id, a.Payload),
        appearance: a => new Appearance(id, a.Summary),
        coverage: c => new Coverage(id, c.Grid),
        observation: o => new Observation(id, o.Series));

    // Re-map EVERY node-id the node carries — own Id AND every graph-node reference BURIED in a payload — the rewrite a
    // Rasm.Persistence Reconcile composes onto durable ids, the exact dual of Relationship.Remap. The ONLY payloads that
    // bury a graph-node NodeId are the PropertySet bags through PropertyValue.Reference, so that arm composes the ONE
    // PropertyValue.Remap owner over each bag value (the QuantityBag's MeasureValue carries no NodeId; MaterialId is a
    // value-object key, not a NodeId; the remaining payloads bury none — those arms rewrite the own id alone). An
    // own-id-only rewrite that dangled a bag's Reference is the deleted form. Same func-form total Switch over the
    // closed family (a new case breaks the build, never a silent pass-through stranding a buried id).
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
- Entry: `ElementGraph.Of(header, nodes, edges)` builds the frozen snapshot — `ToFrozenDictionary` over the nodes, the incidence index grouping every edge by every node its `Members` touch, the `MaterialId`-keyed material index, the demand-built `(EdgeFilter, EdgeOrientation)`-keyed `QuikGraph` view cache over `TypedEdge` legs, and an empty `Bake` memo; `Genesis(header)` seeds the empty header-only snapshot a model-creating session or a Marten stream rehydrate builds onto; `Apply(delta, key)` advances a snapshot by a validated `Graph/delta#GRAPH_DELTA` `GraphDelta` (the persistence rehydrate + live-apply entry), `Fin<T>` railing `ElementFault.NodeAbsent` on a corrupt delta whose added edge names an absent member — either binary endpoint or a `Connect`'s realizing intermediary, the full `Relationship.Members` closure.
- Entry: `Bake(objectId, key)` folds the reachable subgraph from an `Object` node into an `Element`, memoized by `objectId` within the snapshot (a new snapshot from a `Graph/delta#GRAPH_DELTA` carries a fresh memo), `Fin<T>` railing `ElementFault.NodeAbsent` on an absent root and `ElementFault.RelationshipInvalid` on a cyclic `Compose` chain (a `Compose` ancestry set threaded through the fold); `View(filter, orientation)` is the ONE kind-and-orientation scope — memoized per row pair, `TypedEdge` legs carrying the edge so a kind-aware traversal reads it off the leg — with `Topology()` its `(All, Forward)` one-hop; a `Func`-predicate scope died because it could never KEY the cache Persistence's `TopologyView` and Bim's `SpatialStructure` demand; rooted spatial ancestry is Bim `Model/spatial` `SpatialStructure.Ancestry`'s alone (E-E9) — the `Compose` graph is MULTI-PARENT and the real law is Contain-then-Aggregate precedence, which the seam's deleted `ContainmentPath` contradicted.
- Entry: the read accessors `ObjectNodes`/`Find`/`Find<T>`/`Material(MaterialId)`/`MaterialsOf`/`CompositionOf`/`PropertiesOf`/`SectionOf` enumerate the object roots and resolve a node (raw or typed by case) and the material/composition/property/section subgraph a member binds — `MaterialsOf` carrying the one-hop type-resolved fallback the other three compose (an occurrence with no own material/profile reads its `Component`'s), the Op-FREE `SectionOf(member)` signature FROZEN.
- Entry: the group family `GroupsOf`/`MembersOf` resolves the `Assign.Group` memberships (every element in system X, the zones a space belongs to) as Op-free incidence reads, and `ObservationsUnder(root, key)` rolls the measured series over the same OWNING `Compose` closure `BakeParts` recurses so a whole answers for its parts' sensors — together the polymorphic surface a `Rasm.Compute` analysis route, a Persistence index pass, and an AppUi model tree read the concrete graph through, the discipline reads (loads/supports/spaces/areas) composing in Compute from these primitives.
- Auto: `Of` builds the incidence index, the `MaterialId`-keyed material index the `Material(key)` read serves off, and a topology containing every node, including isolated vertices. `Bake` folds one root's incidence: property definitions become bags, assessments become receipts, observations become measured series off the occurrence alone, associations become material/appearance/coverage values, owning compositions recurse into parts, and `Assign.TypeDefinition` applies the named type inheritance once. Topology and memo ride the sealed `ElementGraph` as lazy equality-excluded caches; only `Of`, `Genesis`, and `Apply` mint snapshots.
- Receipt: the `Element` is the one flat record a consumer reads — `element.Properties.Find(name)`, `element.Materials`, `element.Assessments`, `element.Observations`, `element.Appearance`, `element.Coverages`, `element.Parts`, and `element.TypeId` (the inherited `Component`, the generator's type-representation recovery key), with `ObservationsUnder` the whole-over-parts measured rollup beside them — "has it all" in one `Bake`, never a join across the graph, and the computed-versus-measured commissioning read is `element.Assessments` beside `element.Observations` off one baked root rather than a historian join; the `ElementGraph` is the immutable read snapshot Persistence persists and the projectors assemble onto, its `Generator.Equals` structural equality and `Inequalities` member diff feeding the Persistence 3-way `StructuralMerge`; the keyed `View` cache answers reachability and topological order for a consumer without a second graph; the containment breadcrumb is Bim `SpatialStructure.Ancestry`'s (E-E9).
- Packages: `Generator.Equals` (`[Equatable]` snapshot equality, `[StringEquality]`/`[UnorderedEquality]`/`[IgnoreEquality]` member policies, `Inequalities` diff, and the generated `EqualityComparer.Default` reused as the LINQ/`HashSet` key comparer outside generated code), QuikGraph (`BidirectionalGraph` over the seam `TypedEdge`, `AlgorithmExtensions`; kind scoping is the `EdgeFilter`-keyed view cache, so no per-call predicate wrapper materializes), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin`), System.Collections.Frozen/Immutable, NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key).
- Growth: a new derived element field is one column on `Element` the `Bake` fold populates from an existing edge kind; a new edge semantic the fold reads is one arm in `Bake`; a new type-inherited `Seq` is one `UnionBy` arm in the named inheritance, a new occurrence-overrides-type single field one fall-back guard; the working/frozen split keeps the live delta path in the HAMT (`Graph/delta`) and the read path in the frozen snapshot, so neither grows the other; never a second stored `Element` record beside the graph, never a second identity scheme for the deterministic Type id.
- Boundary: the `Element` is a DERIVED FOLD, never a stored record — one flat read comes from `Bake` over the graph, and a parallel stored element record beside it is the deleted form.
- Boundary: the graph splits by PHASE — the live authoring/delta path is a `TrackingHashMap` HAMT (`Graph/delta` owns it for O(log n) structural sharing and the change record its `Diff` reads) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` at the freeze boundary), so a mutable working graph is never confused with a frozen read snapshot.
- Boundary: the incidence index, the material index, and the `QuikGraph` view are built ONCE per snapshot and the `Bake` memo is keyed by object within the snapshot, invalidated only by a new snapshot from a delta, so a re-`Bake` is O(1) and a graph edit is O(log n).
- Boundary: RULED LOSS (W3 gate) — the `View`/`Bake` memos carry NO receipt or gauge hook, and per-model view-build observability has no producer by DESIGN: the memos are cache MECHANICS on a frozen VALUE (lazy, equality-excluded), not domain facts, so no `ElementFact` case, `ElementPoint` row, or instrument threads into the pure read — and the census proves zero series consumers anywhere (Persistence deleted its per-model build/memo receipts as FORGERIES it could not witness; every live memo-observability estate sits at its OWN cache owner — the store cache's `MemoHit`/`MemoMiss` slots, the nesting engine's census counters). A wanting consumer today reads the federated `store.topology.build` receipt plus `VertexCount`/`EdgeCount` off the returned view; if a real series consumer ever lands (a board row naming view-build latency with a declared bound), the seat is one `GaugedSpan` around the view mint at the CALLER owning that budget — never a hook inside the frozen snapshot.
- Boundary: the NAMED type→occurrence inheritance applies once in `Bake` — single fields occurrence-overrides-type, the materials/assessments/classifications `Seq`s union+dedup-by-key — and is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the `PropertyBag.Merge` owns, which stays bag-only.
- Boundary: the observations and coverages `Seq`s are deliberately NOT inherited, which the `GatherFamily` capability sets state as data (`Occurrence` admits both, `Component` neither): a `Component` is a catalogue entry no instrument is mounted on and no field is sampled over, so a type-borne series claims every realization reports one sensor's data.
- Boundary: the `MaterialsOf`/`SectionOf` type-resolved fallback is ONE hop (a `Component` is not itself typed), so the FROZEN Op-free `SectionOf(member)` signature `Rasm.Compute` reads is untouched.
- Boundary: a TYPE `Object`'s deterministic id excludes the volatile `Representations`, so a geometry attach re-keys neither the Type node nor the cached `Bake`.
- Boundary: the `Header` carries the `GeoReference`, the `StepHeader`, and the `UnitScheme` (the `IfcUnitAssignment` unit-presentation declaration — canonical-bytes-excluded, so display units never fork identity), and the `Object` nodes carry the `OwnerHistory` and the `SchemaSpan`, so the model's provenance, declared units, and schema span ride the graph rather than a side channel.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// GatherFamily is the OPTIONAL-family axis the shared incidence gather admits by (kernel S8 capability set, never
// boolean columns): an OCCURRENCE gathers every family; a COMPONENT omits the two a catalogue entry cannot own —
// no instrument is mounted on a Component, so a type-borne ObservationSeries claims every realization reports one
// sensor's data, and a Coverage field is sampled over a PLACED occurrence, never a catalogue row. A third gathering
// shape is one more named set; the deleted form is two near-identical hand-rolled folds differing by omitted arms.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GatherFamily : ICapability<GatherFamily> {
    public static readonly GatherFamily Observations = new("observations");
    public static readonly GatherFamily Coverages = new("coverages");

    public static readonly CapabilitySet<GatherFamily> Occurrence = CapabilitySet<GatherFamily>.Of(Observations, Coverages);
    public static readonly CapabilitySet<GatherFamily> Component = CapabilitySet<GatherFamily>.None;
}

// EdgeOrientation projects each admitted edge's directed legs FORWARD (whole→part, subject→definition,
// from→realizing→to) or ASCENDING (the reversed legs an ancestry climb walks) — the leg law owned as a row column,
// so a view cache keys on the row and no consumer hand-reverses pairs.
[SmartEnum<string>]
public sealed partial class EdgeOrientation {
    public static readonly EdgeOrientation Forward = new("forward", static edge => edge.DirectedPairs);
    public static readonly EdgeOrientation Ascending = new("ascending", static edge => edge.DirectedPairs.Map(static leg => (leg.To, leg.From)));

    [UseDelegateFromConstructor] public partial Seq<(NodeId From, NodeId To)> Legs(Relationship edge);
}

// EdgeFilter is the neutral edge-kind selection vocabulary the keyed view cache and every kind-scoped walk share —
// seated at the SEAM (down-strata law) so Persistence's TopologyView and Bim's SpatialStructure compose one roster
// instead of re-declaring predicates the cache cannot key on. Spatial is Contain-or-Aggregate (the IFC
// spatial-structure tree), DISTINCT from the ComposeKind.IsSpatial column (which marks Contain and the non-owning
// Reference): the tree walks OWNED spatial edges, and Reference is exactly the edge an ancestry must not climb.
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

// TypedEdge carries the admitted Relationship on the leg, so a kind-aware traversal reads the edge off the view
// with no id-pair lookback.
public readonly record struct TypedEdge(NodeId Source, NodeId Target, Relationship Edge) : QuikGraph.IEdge<NodeId>;

// --- [MODELS] -----------------------------------------------------------------------------
// Gathered is the flat result of ONE incidence pass over a subject's own edges — the seven families Bake reads off an
// occurrence and the five a Component contributes, under one shape so the occurrence merge reads type and occurrence
// sides through identical members. Appearance is single-valued (occurrence-overrides-type); the rest accumulate.
public readonly record struct Gathered(
    Seq<PropertyBag> Properties, Seq<QuantityBag> Quantities, Seq<BakedMaterial> Materials,
    Seq<AssessmentPayload> Assessments, Option<AppearanceSummary> Appearance,
    Seq<CoverageGrid> Coverages, Seq<ObservationSeries> Observations) {
    public static readonly Gathered Empty = new([], [], [], [], None, [], []);
}

// Units is the Properties/quantity#MEASURE_VALUE UnitScheme — the IfcUnitAssignment presentation declaration
// (QuantityType token -> declared display-unit token) the Bim ingress lowers and the egress re-emits; the trailing
// default is the empty SI scheme, so every existing construction path is untouched.
public sealed record Header(
    ReleaseVersion Schema, ModelView View, GeoReference Reference, double Tolerance, Instant At, StepHeader Step) {
    // Units is an INIT property, not a defaulted positional — a `default` class slot smuggles null past the
    // boundary; the SI scheme is the declared floor every construction path holds until an ingress declares one.
    public UnitScheme Units { get; init; } = UnitScheme.Si;

    public static Header Default(Instant at) =>
    new(ReleaseVersion.Ifc4X3Add2, ModelView.Ifc4Reference, GeoReference.Identity, 1e-6, at, StepHeader.Empty);

    // SameGrid is the ONE bitwise-tolerance law — the grid every measure in a snapshot was quantized against — so
    // -0.0/0.0 and NaN re-headers read one verdict at the accumulator, the delta advance, and the federation gate.
    public bool SameGrid(Header other) =>
    BitConverter.DoubleToInt64Bits(Tolerance) == BitConverter.DoubleToInt64Bits(other.Tolerance);

    // CanonicalBytes IS the ONE semantic-header content projection both the Projection/address#CONTENT_ADDRESS OfGraph snapshot key and the
    // Graph/delta#GRAPH_DELTA GraphDelta.Address header contribution compose, so a header's bytes are owned ONCE
    // here rather than re-spelled byte-for-byte at each call site (the deleted duplicated projection). The SEMANTIC identity
    // only — schema, model view, tolerance, and the full Geospatial/reference#GEO_REFERENCE GeoReference (Epsg the CRS
    // identity, the resolved name excluded) — the StepHeader/Instant PROVENANCE and the UnitScheme PRESENTATION are
    // EXCLUDED (the graph-altitude mirror of the node-level OwnerHistory exclusion), so a re-export under a new
    // timestamp/author or a re-declared display unit never forks the snapshot identity.
    public void CanonicalBytes(CanonicalWriter w) {
    w.String(Schema.Key).String(View.Key).Double(Tolerance);
    Reference.CanonicalBytes(w);
    }
}

// BakedMaterial pairs material with usage, derived by the Bake fold from an Associate(Material) edge — the occurrence's own bindings
// AND, via the named type inheritance, the Component's, unioned by MaterialKey. The seam-baked accessor pair, distinct
// from the type→occurrence TypeBinding (the inherited Component data) so each altitude owns one name and no collision.
public readonly record struct BakedMaterial(Node.Material Material, MaterialUsage Usage);

// TypeBinding carries the Component (the Type Object) a baked Element inherits from — surfaced so a generator recovers
// WHICH standardized Component a piece realizes: the Type id (Element.TypeId reads it) plus the type-level data the
// occurrence inherited — the Component's BakedMaterial set, the resolved SectionProperties (the type's ProfileSet
// section, the M7 fallback SectionOf reads when the occurrence has no own profile), and the type's secondary
// classification refs. None on the Element when the occurrence carries no Assign.TypeDefinition edge (a bare occurrence
// baked from its own data alone). Bake DERIVES this read carrier (recoverable from the graph), so it carries record
// value equality, not the [Equatable] merge drill — the Rasm.Persistence StructuralMerge keys on the ElementGraph
// nodes/edges, never on a baked Element.
public readonly record struct TypeBinding(NodeId TypeId, Seq<BakedMaterial> Materials, Option<SectionProperties> Section, Seq<Classification> Classifications);

// Four members carry [IgnoreEquality] AT THE OWNER so every consumer's structured diff agrees by construction —
// Id is freshly-minted local identity, ExternalId is the federation JOIN key (comparing it inside a join-matched
// pair is vacuous-true), History is provenance churn, and Parts nest as their own diff rows: the Bim federation
// diff composes Element.EqualityComparer.Default.Inequalities BARE, and a call-site member-name filter roster is the
// deleted form the owner-side annotation forecloses.
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
    // TypeId reads the Component a piece inherits, surfaced so a generator (and the Bim type-representation round-trip)
    // recovers WHICH standardized Component this occurrence realizes; None for a bare occurrence authored with no Assign.TypeDefinition edge.
    public Option<NodeId> TypeId => Type.Map(static t => t.TypeId);
}

// ElementGraph seals as a CLASS, not a record: equality is owned by Generator.Equals `[Equatable]` (the `[UnorderedEquality]` node map
// beside the `[UnorderedEquality]` edge array), and a class has NO compiler-generated `with` — so the misuse a record would admit
// (a `with` aliasing the lazily-built incidence index, QuikGraph view, and bake memo BY REFERENCE, surfacing a stale
// baked Element from the wrong snapshot) is COMPILE-IMPOSSIBLE rather than a runtime throw. Only Of/Genesis/Apply mint a
// fresh snapshot (each rebuilding the caches with an empty memo); the live mutation path is the `Graph/delta#GRAPH_DELTA`
// WorkingGraph HAMT, never a copy of the frozen snapshot.
[Equatable]
public sealed partial class ElementGraph {
    // FrozenDictionary<K,V> implements IDictionary<K,V>, so [UnorderedEquality] routes to
    // DictionaryEqualityComparer<NodeId, Node> — key-matched entry comparison whose value side is Node's own
    // generated comparer and whose key side drives entry hashing. The alternative it forecloses is a
    // KeyValuePair multiset, where the element comparison falls to reflective ValueType.Equals.
    [property: UnorderedEquality] public FrozenDictionary<NodeId, Node> Nodes { get; }
    [property: UnorderedEquality] public ImmutableArray<Relationship> Edges { get; }
    public Header Header { get; }

    [IgnoreEquality] readonly FrozenDictionary<NodeId, ImmutableArray<Relationship>> incidence;
    // materials is the material-key index built at the SAME freeze boundary as incidence: Material(MaterialId) is a
    // keyed read a Bake fold and a Compute route take per member, and a whole-node-set scan per lookup makes that read
    // O(nodes). Grouped-then-first, never a bare ToFrozenDictionary: two Material nodes may carry one MaterialKey under
    // distinct compositions (their ids are content-derived and therefore distinct), and a duplicate key throws where the
    // head read the accessor already promised is total.
    [IgnoreEquality] readonly FrozenDictionary<MaterialId, Node.Material> materials;
    [IgnoreEquality] readonly System.Collections.Concurrent.ConcurrentDictionary<NodeId, Element> bakeMemo = new();
    // Views memoize per (filter, orientation) ROW PAIR — a demanded scope builds once per snapshot and an untouched
    // one stays an unbuilt entry; a Func-predicate scope could never key this cache, which is why the vocabulary is
    // rows (Persistence's TopologyView and Bim's SpatialStructure key the same pairs).
    [IgnoreEquality] readonly System.Collections.Concurrent.ConcurrentDictionary<(EdgeFilter Filter, EdgeOrientation Orientation), QuikGraph.BidirectionalGraph<NodeId, TypedEdge>> views = new();

    ElementGraph(Header header, FrozenDictionary<NodeId, Node> nodes, ImmutableArray<Relationship> edges) {
    (Header, Nodes, Edges) = (header, nodes, edges);
    materials = nodes.Values.OfType<Node.Material>()
    .GroupBy(static m => m.MaterialKey)
    .ToFrozenDictionary(static g => g.Key, static g => g.First());
    // Index every NODE an edge touches (Relationship.Members), not just the binary endpoints, so a Connect's realizing
    // intermediary, a Generic edge's Participants roster, AND its buried PropertyValue.Reference attribute resolve through EdgesAt — EdgesAt(n) ==
    // "every edge touching n", aligned with Touches and the DropNode cascade; an endpoints-only index would strand a
    // realizing or attribute reference the cascade still sweeps. Members dedup per edge: a self-looping Generic edge (the
    // one self-permissive kind — LegalLink rails every typed self-loop) or a buried ref coinciding with an endpoint lists
    // once per node, never twice in one EdgesAt array.
    incidence = edges
    .SelectMany(e => e.Members.Distinct().Select(m => (Node: m, Edge: e)))
    .GroupBy(static p => p.Node, static p => p.Edge)
    .ToFrozenDictionary(static g => g.Key, static g => g.ToImmutableArray());
    }

    public static ElementGraph Of(Header header, FrozenDictionary<NodeId, Node> nodes, ImmutableArray<Relationship> edges) => new(header, nodes, edges);

    // Genesis seeds the empty header-only snapshot a model-creating session or a Marten stream rehydrate starts from — the
    // graph the first GraphDelta (carrying its own Header) and the projector Assemble fold build onto, never a null seed.
    public static ElementGraph Genesis(Header header) => Of(header, FrozenDictionary<NodeId, Node>.Empty, []);

    // Advance a snapshot by a validated GraphDelta — the persistence rehydrate + live-apply entry a consumer takes
    // (the Marten inline projection folds the delta stream through it). `Graph/delta#GRAPH_DELTA` `ReplayOnto` re-applies the
    // already-validated delta raw under the delta's own Header when it carries one; Apply additionally guards that EVERY
    // member an added edge touches resolves in the result — the binary endpoints, a Connect's realizing intermediary, AND a
    // Generic edge's buried PropertyValue.Reference attribute (Relationship.Members, the same closure the incidence index
    // and the DropNode cascade key on) — railing ElementFault.NodeAbsent so a corrupt stored delta never freezes a dangling
    // graph; an endpoints-only guard that let a dangling realizing or attribute reference into the topology view is the
    // deleted under-check (the structural law ran at WorkingGraph.Apply when the delta was produced).
    public Fin<ElementGraph> Apply(GraphDelta delta, Op key) {
    ElementGraph next = delta.ReplayOnto(this);
    return delta.AddedEdges
    .Choose(e => e.Members.Find(m => !next.Nodes.ContainsKey(m)))
    .Head
    .Match(
    Some: member => new ElementFault.NodeAbsent(key, $"<replay-edge-member-absent:{member.Value}>"),
    None: () => Fin.Succ(next));
    }

    // ObjectNodes projects the object (element-root) nodes a consumer iterates to bake or index every element — the typed
    // projection over the node map a Rasm.Persistence Query/index pass folds, never a per-element re-scan of the whole node set.
    public Seq<Node.Object> ObjectNodes => toSeq(Nodes.Values).Choose(static n => n is Node.Object o ? Some(o) : None);

    public ImmutableArray<Relationship> EdgesAt(NodeId node) => incidence.GetValueOrDefault(node, []);

    public QuikGraph.BidirectionalGraph<NodeId, TypedEdge> Topology() => View(EdgeFilter.All, EdgeOrientation.Forward);

    // View is the ONE kind-and-orientation scope every traversal takes — built from the directed legs each admitted
    // edge contributes (a binary edge one leg, a realized Connect the two legs from->realizing->to, a Generic edge a
    // source->participant leg per roster member), so reachability traverses THROUGH a realizing intermediary and
    // reaches every n-ary participant. Each leg carries its Relationship, allowParallelEdges stays true (one node
    // pair legitimately carries several edges), and isolates survive via AddVertexRange.
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

    // --- [READ_ACCESSORS] -----------------------------------------------------------------
    // ElementGraph exposes the polymorphic read surface a Rasm.Compute analysis route reads the concrete graph through — resolve a node
    // (raw or typed by case), and the material/composition/property/section subgraph a member binds. Compute composes its
    // discipline reads (loads/supports off the structural Connect/Generic edges, spaces/bounding-surfaces off the
    // space-boundary Generic edges, the analytical axis/footprint geometry resolved BY CONTENT KEY from member.Representations
    // .Axis/.FootPrint, areas off the quantity bags) from these primitives + EdgesAt/Topology/Bake — the Bim projector bakes
    // that structural/energy subgraph at ingest; the seam owns the material+section reads (it owns those nodes), the
    // discipline physics lives in Compute, never here.
    // Find owns the ONE keyed node resolution (raw and case-typed); every interior read composes it — a raw
    // TryGetValue-plus-cast beside it was the same law respelled per call site.
    public Option<Node> Find(NodeId id) => Nodes.TryGetValue(id, out Node? n) ? Some(n) : None;

    public Option<T> Find<T>(NodeId id) where T : Node => Find(id).Bind(static n => n is T t ? Some(t) : None);

    // Keyed off the freeze-boundary index, never a node scan: this read fires per member inside Bake and per
    // discipline route in Compute, so a scan would make one baked element cost O(members x nodes). The NodeId-keyed
    // twin was Find<Node.Material> verbatim and died to the one-hop law.
    public Option<Node.Material> Material(MaterialId key) =>
    materials.TryGetValue(key, out Node.Material? m) ? Some(m) : None;

    // DirectMaterialsOf reads the member's DIRECTLY-associated material nodes — the Associate(Material) edges off ONE node — the
    // occurrence-OR-type projection MaterialsOf composes for both the occurrence and (one hop) its Component, so neither side re-spells it.
    Seq<Node.Material> DirectMaterialsOf(NodeId node) =>
    toSeq(EdgesAt(node)).Choose(e => e is Relationship.Associate r && r.Subject == node ? Find<Node.Material>(r.Resource) : None);

    // TypeObjectOf resolves the Component (Type Object) a member binds via its Assign.TypeDefinition edge — the ONE-hop type
    // resolution the type-resolved read accessors AND the Bake named inheritance share; None for a bare occurrence with no Component.
    Option<NodeId> TypeObjectOf(NodeId member) =>
    toSeq(EdgesAt(member)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == member ? Some(a.Definition) : None).Head;

    // MaterialsOf reads a member's associated materials, occurrence-direct with a TYPE-RESOLVED fallback: when the occurrence carries no own
    // Associate(Material) edge, resolve through its Component (the Assign.TypeDefinition type Object's OWN direct materials) —
    // ONE type-hop, never recursive (a Component is not itself typed). CompositionOf/PropertiesOf/SectionOf compose THIS one
    // accessor, so the type fallback propagates to all three through the single fallback point, never four duplicated arms.
    public Seq<Node.Material> MaterialsOf(NodeId member) {
    Seq<Node.Material> direct = DirectMaterialsOf(member);
    return direct.IsEmpty ? TypeObjectOf(member).Match(Some: DirectMaterialsOf, None: () => direct) : direct;
    }

    public Option<MaterialComposition> CompositionOf(NodeId member) => MaterialsOf(member).Head.Map(static m => m.Composition);

    // PropertiesOf reads the FULL typed engineering-property profile a member's associated materials carry — the polymorphic property read a
    // Rasm.Compute discipline route composes the Composition/material#MATERIAL_PROPERTY MaterialPropertyAccess accessors over
    // (graph.PropertiesOf(member).Mechanical / .Thermal / .ForDiscipline(Discipline.Fire)); a per-discipline MechanicalOf/
    // ThermalOf/AcousticOf accessor family — a naive 1-of-6 slice re-deriving the owner's `is`-cast — is the deleted form.
    // Inherits the MaterialsOf type-resolved fallback, so a member with no own material reads its Component's properties.
    public Seq<MaterialPropertySet> PropertiesOf(NodeId member) =>
    MaterialsOf(member).Bind(static m => m.Properties);

    // M7: the neutral section the Rasm.Materials projector baked onto a member's ProfileSet composition (WithSection),
    // read Op-FREE off the member's material — NO Bake, NO Op key — so a Rasm.Compute structural/fire runner (which holds
    // no Op) reads graph.SectionOf(member) DIRECTLY off the seam rather than re-deriving the ProfileSet traversal in a
    // discipline-local accessor or admitting VividOrange; the seam owns the section read (it owns the nodes). The Op-FREE
    // signature is FROZEN (Rasm.Compute reads it Op-free); the TYPE-RESOLVED fallback rides INSIDE the composed MaterialsOf —
    // an occurrence-direct ProfileSet wins, else the Component's ProfileSet section resolves (one hop), so a minor part
    // sharing one Component's profile reads the section with no occurrence-direct association, the signature unchanged.
    public Option<SectionProperties> SectionOf(NodeId member) =>
    MaterialsOf(member).Choose(static m => m.Composition is MaterialComposition.ProfileSet { Section: var s } ? s : Option<SectionProperties>.None).Head;

    // --- [GROUP_READS] ----------------------------------------------------------------------
    // GroupsOf/MembersOf own the group/system/zone membership family (a fire compartment, a thermal zone, an MEP
    // system, a load group all ride Assign.Group edges) — Op-FREE incidence reads. The spatial ancestry family
    // (ContainerOf/ContainmentPath) CEDED to Bim Model/spatial SpatialStructure.Ancestry: the seam walk was
    // Contain-only and took .Head of a multi-parent set while Bim declares the real Contain-then-Aggregate
    // precedence law — two disagreeing answers collapsed to the one owner (E-E9); an up-chain consumer walks
    // View(EdgeFilter.Spatial, EdgeOrientation.Ascending) or composes Bim's law. Aggregation stays the consumer's
    // composition: a zone rollup is MembersOf + Bake + MeasureValue.Sum, never a seam-owned report.
    public Seq<NodeId> GroupsOf(NodeId member) =>
    toSeq(EdgesAt(member)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.Group && a.Subject == member ? Some(a.Definition) : None);

    public Seq<NodeId> MembersOf(NodeId group) =>
    toSeq(EdgesAt(group)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.Group && a.Definition == group ? Some(a.Subject) : None);

    // ObservationsUnder is the measured-evidence rollup the group axis already has for its own membership: a whole's
    // series set is its own and every OWNING part's, over the SAME Compose closure BakeParts recurses, so a commissioning read reaches the
    // sensors mounted on a wall's layers, a riser's segments, or a storey's spaces from the whole rather than
    // re-walking the part tree per consumer. Bake is the one fold and this composes it — the Op rides through because
    // a dangling part id rails NodeAbsent exactly as BakeParts does, never a presence pre-filter that silently
    // under-reports the series a commissioning report is asked to be complete over.
    public Fin<Seq<ObservationSeries>> ObservationsUnder(NodeId root, Op key) =>
    Bake(root, key).Map(static element => Rollup(element));

    static Seq<ObservationSeries> Rollup(Element element) =>
    element.Parts.Fold(element.Observations, static (series, part) => series + Rollup(part));

    // --- [BAKE] ---------------------------------------------------------------------------
    // Bake IS the one derived fold: an Object node plus its reachable subgraph become a flat Element. The public entry seeds
    // an EMPTY Compose ancestry; the private overload threads it so a cyclic Compose chain (a corrupt delta replay or a
    // self-aggregating Object) rails ElementFault.RelationshipInvalid instead of recursing unbounded — the check precedes the
    // memo, so an in-progress (not-yet-memoized) ancestor is caught while a shared DAG child still memo-hits.
    public Fin<Element> Bake(NodeId objectId, Op key) => Bake(objectId, key, ImmutableHashSet<NodeId>.Empty);

    Fin<Element> Bake(NodeId objectId, Op key, ImmutableHashSet<NodeId> ancestry) =>
    ancestry.Contains(objectId)
    ? new ElementFault.RelationshipInvalid(key, $"<bake-compose-cycle:{objectId.Value}>")
    : bakeMemo.TryGetValue(objectId, out Element? cached)
    ? Fin.Succ(cached)
    : Find<Node.Object>(objectId).Match(
    Some: root => BakeObject(root, key, ancestry.Add(objectId)).Map(element => { bakeMemo[objectId] = element; return element; }),
    None: () => Fin.Fail<Element>(new ElementFault.NodeAbsent(key, $"<bake-root-absent:{objectId.Value}>")));

    Fin<Element> BakeObject(Node.Object root, Op key, ImmutableHashSet<NodeId> ancestry) {
    Gathered own = Gather(root.Id, GatherFamily.Occurrence);
    // TypeResolutionOf applies the NAMED type→occurrence inheritance (Relations/relation#EDGE_ALGEBRA Assign.TypeDefinition): resolve the Component
    // (type Object), then merge occurrence-over-type — DISTINCT from the Properties/property#PROPERTY_BAG InheritanceMode
    // value-bag precedence (which stays the PropertyBag.Merge below). Single fields occurrence-overrides-type; the Seq
    // fields materials/assessments/classifications union + dedup-by-key. None for a bare occurrence (no Component bound).
    Option<(Node.Object Type, Gathered Data)> typeFold = TypeResolutionOf(root.Id);
    // Properties/Quantities: the EXISTING InheritanceMode value-bag merge (type-then-occurrence precedence via Merge) — the
    // named inheritance does NOT touch the bag-precedence the bag Merge owns, only the single fields and the Seq sets.
    Seq<PropertyBag> properties = MergeBagSets(typeFold.Map(static t => t.Data.Properties).IfNone(Seq<PropertyBag>()), own.Properties);
    Seq<QuantityBag> quantities = MergeBagSets(typeFold.Map(static t => t.Data.Quantities).IfNone(Seq<QuantityBag>()), own.Quantities);
    // Materials/Assessments: occurrence-precedence Seq union through ONE Inherit fold row per family — dedup by the
    // MaterialKey string and the (Discipline, Route, InputKey) assessment cache triple.
    Seq<BakedMaterial> materials = Inherit(own.Materials, typeFold, static data => data.Materials, static b => b.Material.MaterialKey.Value);
    Seq<AssessmentPayload> assessments = Inherit(own.Assessments, typeFold, static data => data.Assessments, static a => (a.Discipline.Key, a.Route.Value, a.InputKey));
    // Primary-Classification exclusion runs ONCE, off the union — it rides Element.Classification as the entity-class
    // key, so repeating it in the secondary set double-reports it whichever side carried it.
    Seq<Classification> classifications = UnionBy(
        root.Classifications,
        typeFold.Map(static t => t.Type.Classifications).IfNone(Seq<Classification>()),
        static classification => (classification.System, classification.Code, classification.Edition))
        .Filter(classification => classification != root.Classification);
    // Single fields are occurrence-overrides-type: the primary Classification is the occurrence's own (admission guarantees
    // a non-blank entity-class code), while PredefinedType/Name/Representations fall back to the Component when the
    // occurrence carries the IFC unset sentinel — a NOTDEFINED predefined defers to the type, a blank Name to the type
    // designation, an own-geometry-less occurrence to the type's mapped Representations.
    PredefinedType predefinedType = typeFold.Match(Some: t => root.PredefinedType == PredefinedType.NotDefined ? t.Type.PredefinedType : root.PredefinedType, None: () => root.PredefinedType);
    string name = typeFold.Match(Some: t => root.Name.Length > 0 ? root.Name : t.Type.Name, None: () => root.Name);
    RepresentationContentHash representations = typeFold.Match(Some: t => root.Representations.ByIdentifier.Count > 0 ? root.Representations : t.Type.Representations, None: () => root.Representations);
    // Appearance is a single field, so it follows the same occurrence-overrides-type law: an occurrence with no own
    // Associate(Appearance) edge inherits the Component's styling (the type-level material appearance IFC round-trips).
    Option<AppearanceSummary> resolvedAppearance = own.Appearance.IsSome ? own.Appearance : typeFold.Bind(static t => t.Data.Appearance);
    // TypeBinding surfaces on the baked Element (Element.TypeId reads its id): the type id, the inherited BakedMaterial
    // set, the type's resolved ProfileSet section (the M7 fallback SectionOf reads), and the type's secondary classification refs.
    Option<TypeBinding> typeBinding = typeFold.Map(static t => new TypeBinding(
    t.Type.Id, t.Data.Materials,
    t.Data.Materials.Choose(static m => m.Material.Composition is MaterialComposition.ProfileSet { Section: var s } ? s : Option<SectionProperties>.None).Head,
    t.Type.Classifications));
    return BakeParts(root.Id, key, ancestry).Map(parts => new Element(
    root.Id, root.Kind, root.ExternalId, root.Classification, predefinedType, name, root.Tag, representations,
    materials, properties, quantities, assessments, resolvedAppearance,
    own.Coverages, own.Observations, parts, typeBinding, root.History, classifications));
    }

    // Gather is the ONE incidence pass every gathering caller takes — the occurrence root and the resolved Component
    // alike — admitted by the GatherFamily capability set that names which OPTIONAL families the subject can own.
    // EdgesAt(subject) walks ONCE; the subject/target guards factor to one place each (Landed for the Assign
    // definitions, Bound for the LegalLink Material/Appearance/Coverage resource closure), so the seven arms carry
    // ONLY their (sub-kind, target-case) correspondence and the two callers differ by a capability value rather than
    // by an arm roster one of them omits.
    Gathered Gather(NodeId subject, CapabilitySet<GatherFamily> families) =>
    toSeq(EdgesAt(subject)).Fold(Gathered.Empty, (acc, edge) => edge switch {
    Relationship.Assign a when a.Subject == subject => Find(a.Definition).Map(node => Landed(acc, a.SubKind, node, families)).IfNone(acc),
    Relationship.Associate r when r.Subject == subject => Find(r.Resource).Map(node => Bound(acc, node, r.Usage, families)).IfNone(acc),
    _ => acc,
    });

    // The (AssignKind, target-case) rows — a definition edge lands its payload where the pair matches and is inert
    // elsewhere (a QuantitySet under an Assessment sub-kind gathers nothing rather than mis-filing).
    static Gathered Landed(Gathered acc, AssignKind kind, Node definition, CapabilitySet<GatherFamily> families) =>
    (kind, definition) switch {
    (var k, Node.PropertySet ps) when k == AssignKind.PropertyDefinition => acc with { Properties = acc.Properties.Add(ps.Bag) },
    (var k, Node.QuantitySet qs) when k == AssignKind.PropertyDefinition => acc with { Quantities = acc.Quantities.Add(qs.Bag) },
    (var k, Node.Assessment payload) when k == AssignKind.Assessment => acc with { Assessments = acc.Assessments.Add(payload.Payload) },
    (var k, Node.Observation series) when k == AssignKind.Observation && families.Admits(GatherFamily.Observations) => acc with { Observations = acc.Observations.Add(series.Series) },
    // TypeDefinition and Group are RESOLVED elsewhere (TypeResolutionOf, GroupsOf/MembersOf) — inert here by law.
    _ => acc,
    };

    // The Associate resource rows — the LegalLink Material/Appearance/Coverage closure (Graph/delta#GRAPH_DELTA),
    // so the three arms mirror that legality exactly. Appearance is single-valued (occurrence-overrides-type).
    static Gathered Bound(Gathered acc, Node resource, MaterialUsage usage, CapabilitySet<GatherFamily> families) =>
    resource switch {
    Node.Material m => acc with { Materials = acc.Materials.Add(new BakedMaterial(m, usage)) },
    Node.Appearance ap => acc with { Appearance = ap.Summary },
    Node.Coverage c when families.Admits(GatherFamily.Coverages) => acc with { Coverages = acc.Coverages.Add(c.Grid) },
    _ => acc,
    };

    // TypeResolutionOf resolves the named type→occurrence inheritance: the Assign.TypeDefinition edge resolved to the Component
    // (type Object), then the SAME Gather under the Component policy — so the type's data is gathered as DATA in one pass,
    // never a recursive Bake, while the type's single fields and secondary Classifications ride the resolved Object
    // and the section derives from the type materials' ProfileSet. None for a bare occurrence with no Component binding. The
    // type carries NO further TypeDefinition edge (a Component is not itself typed), so this is a single one-hop resolution.
    Option<(Node.Object Type, Gathered Data)> TypeResolutionOf(NodeId occurrence) =>
    TypeObjectOf(occurrence).Bind(typeId =>
    Find<Node.Object>(typeId).Map(typeObj => (Type: typeObj, Data: Gather(typeId, GatherFamily.Component))));

    // Set-union by SetName: each occurrence bag merges with its matching type bag (precedence via the ONE
    // ValueBag<V>.Merge the PropertyBag/QuantityBag global-using aliases share), and a type-only bag with no occurrence
    // counterpart is inherited as-is — one generic fold serves BOTH aliases, never a per-alias copy of one body.
    static Seq<ValueBag<V>> MergeBagSets<V>(Seq<ValueBag<V>> type, Seq<ValueBag<V>> occurrence) =>
    occurrence.Map(occ => type.Find(t => t.SetName == occ.SetName).Match(Some: t => ValueBag<V>.Merge(t, occ), None: () => occ))
    + type.Filter(t => !occurrence.Exists(o => o.SetName == t.SetName));

    // Occurrence-precedence union: concatenation ORDER carries the precedence (occurrence entries lead, so
    // first-wins keeps them and drops the type's counterpart) and DistinctBy carries the dedup, including the
    // type-internal duplicate. The comparer is a POLICY VALUE rather than an IEquatable<K> bound, so a key that
    // is itself a generated-equality owner passes `K.EqualityComparer.Default` and reuses the exact semantics
    // its members declare — the generated comparers keying LINQ outside generated code — while a value-tuple or
    // string key passes none and takes the default. An Exists-fold spelling of first-wins is the deleted form: it
    // probes membership quadratically and admits no comparer at all.
    static Seq<T> UnionBy<T, K>(Seq<T> occurrence, Seq<T> type, Func<T, K> key, IEqualityComparer<K>? comparer = null) =>
    toSeq((occurrence + type).AsEnumerable().DistinctBy(key, comparer));

    // Inherit is the one named fold a type-inherited Seq family rides: the occurrence's own entries lead, the
    // Component's family (projected off the gathered data) follows, first-wins under the family's own key.
    static Seq<T> Inherit<T, K>(Seq<T> own, Option<(Node.Object Type, Gathered Data)> typeFold, Func<Gathered, Seq<T>> family, Func<T, K> key) =>
    UnionBy(own, typeFold.Map(t => family(t.Data)).IfNone(Seq<T>()), key);

    // BakeParts takes the OWNING Compose children only — Aggregate (decomposition), Nest (ordered child sequence), and Contain (spatial
    // containment) recurse into Parts; the non-owning Reference flavor (IfcRelReferencedInSpatialStructure — an element
    // referenced in an additional spatial structure it is NOT contained by) is EXCLUDED, so a column contained in storey A
    // and referenced in storey B bakes as a Part of A alone, never duplicated onto B. Baking every Compose flavor is the
    // deleted form, contradicting the Bake prose (Aggregate/Nest/Contain are the parts) and double-counting referenced
    // elements. Parts order is DETERMINISTIC: the Compose Ordinal (the IFC Nest list order) ranks first, ordinal-less
    // children follow id-ordered. Equality does not read it ([IgnoreEquality] Parts nest as their own diff rows) — the
    // sort exists so every EGRESS reading Parts positionally (the IFC Nest list re-emit, the model-tree render, the
    // table row family) reproduces one order, which raw incidence order cannot promise across two builds of one graph.
    // NodeAbsent rails a dangling part id (a corrupt snapshot whose Compose edge names an undeclared node — unreachable in a
    // validated graph, LegalLink admits only present endpoints and Erase cascades) through the recursive Bake, never a
    // presence pre-filter that silently truncates Parts and masks the corruption the fault band surfaces.
    Fin<Seq<Element>> BakeParts(NodeId whole, Op key, ImmutableHashSet<NodeId> ancestry) =>
    toSeq(toSeq(EdgesAt(whole))
    .Choose(e => e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference ? Some((c.Part, c.Ordinal)) : None)
    .OrderBy(static p => p.Ordinal.IsSome ? 0 : 1).ThenBy(static p => p.Ordinal.IfNone(0)).ThenBy(static p => p.Part.Value, StringComparer.Ordinal))
    .TraverseM(p => Bake(p.Part, key, ancestry)).As().Map(static parts => parts.ToSeq());
}
```

## [04]-[FEDERATION]

- Owner: `ElementGraph.Federate` the static cross-model union over a tagged source set under one caller-supplied coordination `Header`; `ElementGraph.Extract` the instance slice over a root set; `FederationReceipt` the union evidence carrying one `FederationSource` row per source beside the union totals and the merged tally.
- Entry: `Federate(sources, coordination, key)` takes `Seq<(string Source, ElementGraph Graph)>` — the source tag being the caller's own model label, never a seam-minted id — one coordination `Header`, and the kernel `Op`, returning `Fin<(ElementGraph Graph, FederationReceipt Receipt)>`.
- Entry: `Extract(roots, key)` takes `Seq<NodeId>` and the kernel `Op`, returning `Fin<ElementGraph>` under the SOURCE `Header` unchanged — a slice is the same model narrowed, never a re-coordinated one.
- Auto: every refusal accumulates through the kernel admission-slot algebra over `Validation<Error,_>` and collapses to `Fin<T>` once at the return, so a federation attempt reports every divergent source and every colliding id in ONE failure rather than the first it meets.
- Auto: both entries mint through ONE `GraphDelta` carrying the union (or slice) as `AddedNodes`/`AddedEdges` with a `Reheader`, run through `AdmitOnto(Genesis(header), key)` — the sanctioned validating mint, so `LegalLink` re-crosses every foreign edge; a raw `ElementGraph.Of` over foreign edges is the deleted form, because it freezes a topology no structural law admitted.
- Law: the three refusal axes are the source set being EMPTY, a source `Header.Tolerance` differing BITWISE from the coordination tolerance, and a source `Header.Reference` differing STRUCTURALLY from the coordination reference under `GeoReference`'s own value equality; each fault detail names the source tag and both sides' values.
- Law: id collision discriminates by MINTING REGIME, not by payload alone — a rooted OCCURRENCE id (`Node.Object { Kind: Occurrence }`) shared across two sources is ALWAYS `DeltaConflict`, because a Guid-v7 placement identity carries no content preimage and a repeat is corruption; a content-derived or type-derived id repeats legitimately, so equal payloads under `EqualityComparer<Node>.Default` merge as the dedup the id regime exists for and unequal payloads fault naming the id and both source tags.
- Law: an edge JOINS an `Extract` slice only when EVERY id in its `Members` is inside the closure, and the closure is what guarantees it: expansion follows `DirectedPairs` DOWNWARD (whole→part, subject→definition, from→to) and pulls in each reached edge's FULL `Members`, so a buried `PropertyValue.Reference` target and a `Connect`'s realizing intermediary ride in with the edge and no slice can dangle.
- Receipt: `FederationReceipt` is EVIDENCE, never graph content — per source the tag, the snapshot `ContentAddress`, the source header's provenance columns (schema, model view, instant, STEP name), and the node and edge counts; then the union totals and the merged tally the dedup produced, derived from the rows against the union so it cannot disagree with what the graph holds.
- Packages: LanguageExt.Core (`Seq`/`Option`/`Fin`/`Validation` + the tuple `.Apply` join and the `.Traverse` run fold), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph` the per-source snapshot key), `Rasm` (the kernel `Op` and `Rasm/Domain/validation#ADMISSION_SLOTS`), BCL inbox (`BitConverter.DoubleToInt64Bits` the bitwise tolerance comparison).
- Growth: a new coordination axis is one refusal slot beside the tolerance and reference gates; a new union law is one arm in `Unify`; a new slice direction is one predicate on the frontier expansion — never a second union entrypoint and never a per-source header column on the graph.
- Boundary: `Connect.Interface` is a `UInt128` blob key riding no `Members`, so an extracted slice carries the key while its blob resolution stays SOURCE-bound through the owning `GeometrySource` port; a slice does not copy geometry, and a consumer resolving an extracted interface reaches the source's own store.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// One provenance row per federated source. The Address is the source snapshot's own ContentAddress.OfGraph key, so a
// receipt names exactly which model state produced the union and a re-run over drifted sources reads as a distinct
// row set. The header columns demote the source Header to PROVENANCE here — the union graph carries ONE semantic
// header, and a per-source Header roster on ElementGraph is the deleted form (it widens the one spine every consumer
// reads for a federation-only concern).
public sealed record FederationSource(
    string Tag, ContentAddress Address, ReleaseVersion Schema, ModelView View, Instant At, string Step,
    int NodeCount, int EdgeCount);

// Union evidence, never graph content: the source rows beside the union totals and the merged tally (how many node
// and edge claims the dedup collapsed). Merged is DERIVED from the rows against the union counts rather than counted
// during the fold, so it cannot disagree with what the union actually holds.
public sealed record FederationReceipt(
    Seq<FederationSource> Sources, int NodeCount, int EdgeCount, int MergedNodes, int MergedEdges);

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed partial class ElementGraph {
    // Federate unions N tagged source graphs onto ONE coordination Header. The tag is the CALLER's model label (a
    // discipline name, a file stem, a package id) — the seam mints none, because only the caller knows what a source
    // means, and the tag exists to make a refusal and a collision nameable in the receipt and the fault detail.
    public static Fin<(ElementGraph Graph, FederationReceipt Receipt)> Federate(
    Seq<(string Source, ElementGraph Graph)> sources, Header coordination, Op key) =>
    Admitted(sources, coordination, key).ToFin().Bind(union =>
    (GraphDelta.Empty with { AddedNodes = union.Nodes, AddedEdges = union.Edges })
    .Reheader(coordination)
    .AdmitOnto(Genesis(coordination), key)
    .Map(step => (Graph: step.Graph, Receipt: Receipted(sources, union.Nodes, union.Edges))));

    // Refusal axes join APPLICATIVELY with the id union, so an empty source set, a divergent tolerance, a divergent
    // reference, and every colliding id all report in ONE ManyErrors failure; a Bind chain hands back the first
    // divergence and hides the rest behind a re-run.
    static Validation<Error, (Seq<Node> Nodes, Seq<Relationship> Edges)> Admitted(
    Seq<(string Source, ElementGraph Graph)> sources, Header coordination, Op key) =>
    (Gate(sources.Count > 0, key, "<federate-empty-source-set>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
     Accumulate(sources.Map(source => Aligned(source, coordination, key))),
     Unified(sources, key))
    .Apply(static (_, _, union) => union).As();

    // Tolerance compares BITWISE, not by ==: the coordination grid is what every measure in the union was quantized
    // against, so two doubles that differ in the last bit are two grids, and the ULP-tolerant comparison admits a
    // source whose measure bytes cannot be re-derived. Frame alignment is the upstream reprojection leg's job, so the
    // reference axis REFUSES rather than reconciles, and the detail names both sides' resolution mode.
    static Validation<Error, Unit> Aligned((string Source, ElementGraph Graph) source, Header coordination, Op key) =>
    Accumulate(Seq(
    Gate(source.Graph.Header.SameGrid(coordination), key,
    $"<federate-tolerance-divergent:{source.Source}:{source.Graph.Header.Tolerance:R}:{coordination.Tolerance:R}>",
    static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(source.Graph.Header.Reference.Equals(coordination.Reference), key,
    $"<federate-reference-divergent:{source.Source}:{source.Graph.Header.Reference.Resolution.Key}:{coordination.Reference.Resolution.Key}>",
    static (k, d) => (Error)new ElementFault.ValueRejected(k, d))));

    // Group EVERY source's nodes by NodeId and unify each group; edges concat and dedup by the generated comparer,
    // where an edge repeated across sources is the same relationship stated twice and carries no identity of its own.
    static Validation<Error, (Seq<Node> Nodes, Seq<Relationship> Edges)> Unified(
    Seq<(string Source, ElementGraph Graph)> sources, Op key) =>
    toSeq(sources.Bind(static source => toSeq(source.Graph.Nodes.Values).Map(node => (Tag: source.Source, Node: node)))
    .AsEnumerable()
    .GroupBy(static claim => claim.Node.Id))
    .Traverse(group => Unify(group.Key, toSeq(group), key))
    .As()
    .Map(nodes => (Nodes: nodes.Strict(), Edges: Edged(sources)));

    // Id regime decides fault-versus-dedup: a rooted OCCURRENCE id is a random Guid-v7 placement identity with no
    // content preimage, so the same id under two sources is corruption and faults on collision ALONE — never a
    // rename, which would fabricate a placement, and never a payload compare, which would silently merge two
    // physically distinct occurrences whose columns happen to agree. Every other id is content-derived or
    // type-derived: equal payloads ARE the dedup the regime exists for, unequal payloads fault naming both tags.
    static Validation<Error, Node> Unify(NodeId id, Seq<(string Tag, Node Node)> claims, Op key) =>
    claims.Tail.Find(claim => Collides(claims[0].Node, claim.Node)).Map(static claim => claim.Tag)
    is { IsSome: true, Case: string rival }
    ? new ElementFault.DeltaConflict(key, $"<federate-node-collision:{id.Value}:{claims[0].Tag}:{rival}>")
    : claims[0].Node;

    // Two laws, named apart so a reader sees WHICH fired: Replayed — a rooted occurrence id has no content
    // preimage, so its repeat IS the corruption regardless of payload; Diverged — every other regime dedups equal
    // payloads and faults unequal ones.
    static bool Collides(Node held, Node rival) => Replayed(held) || Diverged(held, rival);

    static bool Replayed(Node held) => held is Node.Object { Kind: var kind } && kind == ObjectKind.Occurrence;

    static bool Diverged(Node held, Node rival) => !EqualityComparer<Node>.Default.Equals(held, rival);

    static Seq<Relationship> Edged(Seq<(string Source, ElementGraph Graph)> sources) =>
    toSeq(sources.Bind(static source => toSeq(source.Graph.Edges))
    .AsEnumerable()
    .Distinct(EqualityComparer<Relationship>.Default));

    static FederationReceipt Receipted(
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
    return new FederationReceipt(
    Sources: rows,
    NodeCount: nodes.Count,
    EdgeCount: edges.Count,
    MergedNodes: rows.Fold(0, static (sum, row) => sum + row.NodeCount) - nodes.Count,
    MergedEdges: rows.Fold(0, static (sum, row) => sum + row.EdgeCount) - edges.Count);
    }

    // Extract slices the reachable-downward closure of a root set into its own graph under the SOURCE Header — a
    // slice is this model narrowed, never a re-coordinated one, so its node ids, measure bytes, and content keys stay
    // interchangeable with the graph it came from. An absent root faults BEFORE any walk, so a typo never returns a
    // silently smaller slice.
    public Fin<ElementGraph> Extract(Seq<NodeId> roots, Op key) =>
    roots.Find(root => !Nodes.ContainsKey(root)) is { IsSome: true, Case: NodeId absent }
    ? new ElementFault.NodeAbsent(key, $"<extract-root-absent:{absent.Value}>")
    : Sliced(roots, key);

    Fin<ElementGraph> Sliced(Seq<NodeId> roots, Op key) {
    ImmutableHashSet<NodeId> closure = Closure(ImmutableHashSet.CreateRange(roots), roots.Distinct());
    return (GraphDelta.Empty with {
    // Find spells its argument rather than passing the method group: the typed Find<T> sibling shares the name, and a
    // group conversion here is one overload-resolution edit away from binding the wrong one.
    AddedNodes = toSeq(closure).Choose(id => Find(id)),
    // Edges join only with their WHOLE Members set inside — the closure pulls every reached edge's members in, so
    // this filter admits exactly the edges the walk reached and refuses one straddling the boundary.
    AddedEdges = toSeq(Edges).Filter(edge => edge.Members.ForAll(closure.Contains)),
    })
    .Reheader(Header)
    .AdmitOnto(Genesis(Header), key)
    .Map(static step => step.Graph);
    }

    // Frontier-driven monotone closure: each round expands ONLY the newly-admitted nodes, so the walk costs the sum
    // of admitted degree rather than a whole-set re-scan per round, and it settles the round a frontier admits nothing.
    ImmutableHashSet<NodeId> Closure(ImmutableHashSet<NodeId> admitted, Seq<NodeId> frontier) =>
    frontier.IsEmpty
    ? admitted
    : Reached(admitted, frontier) is var next && next.IsEmpty
    ? admitted
    : Closure(admitted.Union(next), next);

    // DOWNWARD only: an edge is traversed when the frontier node is a SOURCE of one of its DirectedPairs legs (whole
    // to part, subject to definition, from to realizing to to), so a part does not drag its parent whole into the
    // slice. Traversing an edge admits its FULL Members — the binary endpoints, a Connect's realizing intermediary,
    // a Generic edge's Participants roster, AND its buried PropertyValue.Reference targets — which is what makes the
    // whole-Members join condition satisfiable rather than a filter that silently drops half the slice's edges.
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
- [TOPOLOGY_VIEW]: `View(EdgeFilter, EdgeOrientation)` is the ONE scoped topology — memoized per row pair at the snapshot, built from each admitted edge's oriented `DirectedPairs` legs over seam `TypedEdge` (the leg carries its `Relationship`), so reachability traverses THROUGH a realizing intermediary, a kind-scoped walk reads the edge off the leg through `AlgorithmExtensions`, and an ancestry climb takes the `Ascending` orientation; `Topology()` is the `(All, Forward)` one-hop; ROOTED spatial ancestry is Bim `SpatialStructure.Ancestry`'s alone (E-E9) because the `Compose` graph is multi-parent and the precedence law lives there.
- [IDENTITY_AND_HASH]: `NodeId.Of(NodeSeed)` OWNS identity ALONE over one regime dispatch the `NodeSeed` witness makes recoverable from the value — an OCCURRENCE `Object` a Guid-v7 `Placement` id (sortable), a TYPE `Object` a DETERMINISTIC streamed digest over the `WriteIdentity` segments (`TypeSeed`, the SAME kernel seed-zero hasher every arm rides), a non-rooted node a streamed `Content` digest over its full `CanonicalBytes`, and `Precomputed` the own-self-hash wrap; the compressed IFC GlobalId is a Bim-stored projection attribute re-emitted at `Emit`. The Object layout is FIVE frozen segments — the `WriteIdentity` three plus the two volatile blocks (`Representations`, secondary `Classifications`) interleaved at fixed positions — so the Type seed excludes exactly the volatile blocks (the PRIMARY `Classification` stays as entity-class identity), identical `Component`s dedup to one Type, a later geometry attach or standard-classification stamp never re-keys it, and the FULL projection stays byte-for-byte the prior parity-corpus bytes — `CanonicalBytes(w)` is the ONE canonical projection the id mint, the `Projection/address#CONTENT_ADDRESS` diff, and the delta fold share (fixed IEEE-754 LE bits, measures quantized to `Header.Tolerance`, explicit attribute order, id excluded), so a node's content identity is stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed — a float-bearing golden vector (an `IfcMaterialLayer`-shaped node) anchors the cross-runtime parity corpus, and the Type seed is a C#-side mint a peer READS as an opaque rooted id, never re-derives. Generated `NodeWire` support carries one node id verbatim for the persistence edit seam. Every `PropertySet`/`QuantitySet`-bearing content key derives from the COUNTED bag layout — `Ordinal(count)` before the sorted rows, the `Projection/address#IMPLEMENTATION_LAW` count-prefix law — the cross-runtime wire law the queued Python/TypeScript canonical-writer mirrors reproduce; an uncounted bag run is the deleted injectivity hole (a trailing run parsing as a prefix of the next raw-append segment).
- [TYPE_INHERITANCE]: `Bake` resolves the named type→occurrence inheritance from the `Relations/relation#EDGE_ALGEBRA` `Assign.TypeDefinition` bind — the `Component` projection (the owner that mints its Type) authors the occurrence→Type edge, and `Bake`'s `TypeResolutionOf` folds the `Component`'s standardized data (the property/quantity bags, the `BakedMaterial` set, the `Assessment` receipts, the type `Object`'s single fields, and its secondary classifications) in ONE pass, then merges occurrence-over-type with explicit per-field precedence: single fields occurrence-overrides-type (`PredefinedType`/`Name`/`Representations`/`Appearance` falling back to the type on the IFC unset sentinel, the primary `Classification` the occurrence's own non-blank code), the materials/assessments/classifications `Seq`s union+dedup-by-key (the `MaterialKey` string; the `(Discipline, Route, InputKey)` assessment cache triple; the `(System, Code, Edition)` classification identity). This is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode`, which stays `PropertyBag`-value precedence (the bag `Merge`) and is never extended by the named dimension. `TypeBinding` surfaces the inherited `Component` as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes (the `Rasm.Bim` type-representation round-trip key), and `MaterialsOf` gains a one-hop type-resolved fallback `CompositionOf`/`PropertiesOf`/`SectionOf` compose (a minor part sharing one `Component`'s profile reads its section with no occurrence-direct association) WITHOUT perturbing the FROZEN Op-free `SectionOf(member)` signature `Rasm.Compute` reads — the fallback is a single type-hop (a `Component` is not itself typed), never a recursive type chain.
- [STRUCTURAL_EQUALITY]: `[Equatable]` owns deep equality for `ElementGraph`, every nested `Node` and `Relationship` CASE, and every drillable intermediate payload — the union roots carry no seat, because a root seat is the compile-proven silent form whose case members reference-compare — so `Inequalities(before, after)` localizes changes below the node map and member-grain drill into a case runs that case's own comparer after discrimination. `MeasureValue` and `PropertyValue` are atomic record-value leaves. Sealed, `ElementGraph` excludes the incidence index, topology, and bake memo from equality and exposes no record copy that aliases caches. Three member policies are DECLARED rather than inherited, because each one agrees with the canonical projection by inheritance alone and a member edit breaks that agreement with no signal. `[StringEquality(StringComparison.Ordinal)]` binds every string the `CanonicalWriter` writes verbatim — a culture-sensitive or case-insensitive comparer rules two nodes equal whose canonical bytes differ, forking equality from content identity at the one place the merge and the id mint must agree. `[UnorderedEquality]` on `Nodes` routes to `DictionaryEqualityComparer<NodeId, Node>` because `FrozenDictionary<TKey,TValue>` implements `IDictionary<TKey,TValue>` — key-matched entry comparison with `EqualityComparer<Node>.Default` on the value side dispatching each case's generated `Equals` override, NOT a `KeyValuePair` multiset, whose element comparison falls to reflective `ValueType.Equals`; the same comparer keys every `Distinct`/`GroupBy`/`HashSet` reuse outside generated code, so a fold deduplicating nodes never spells a second equality. `[PrecisionEquality]` is REFUSED on every float-bearing member here on two structural grounds, not preference: the generator omits precision members from `GetHashCode` ENTIRELY, so a payload distinguished only by tolerance-compared scalars hashes to one bucket across the whole graph; and every double this page carries is either a `MeasureValue` already quantized to `Header.Tolerance` by the canonical projection — a second tolerance beside it forks the one quantization the cross-runtime parity corpus depends on — or an `AppearanceSummary` channel that is PREIMAGE to a frozen content key, where tolerance-equality rules two nodes equal whose `AppearanceKey`s differ and breaks the content-address contract outright.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
