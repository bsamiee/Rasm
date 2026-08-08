# [ELEMENT_GRAPH]

`ElementGraph` IS the authoritative thing — `Header` + `Nodes: FrozenDictionary<NodeId, Node>` + `Edges: ImmutableArray<Relationship>` + a built-once incidence index — and the consumer-facing `Element` DERIVES as the FOLD `Bake(objectNode)` over the reachable subgraph, never a second stored record. `Bake` lands material, property/quantity bags, assessments, observation series, appearance, coverages, composed parts, and the inherited `Component` (`Element.Type`/`Element.TypeId`) as flat fields a consumer reads in one hop — "has it all" is one fold, not a join across ten owners.

`ElementGraph` mirrors IFC as a property graph: every IFC entity is a `Node`, every IFC relationship a `Relations/relation#EDGE_ALGEBRA` `Relationship`, and the consumer reads neither — it reads the baked `Element`. `NodeId` OWNS identity over one regime: an OCCURRENCE `Object` carries Guid-v7 placement identity (the compressed IFC GlobalId is a Bim-stored attribute re-emitted at `Emit`), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its volatile-excluded canonical seed, and a non-rooted node a content hash over its full canonical bytes — never a second identity scheme.

PHASE splits the graph: live authoring rides the `TrackingHashMap` HAMT `Graph/delta#GRAPH_DELTA` owns, and `ElementGraph` is the FROZEN read snapshot — `ToFrozenDictionary`, the incidence index, the memoized `Bake`, and the `QuikGraph` view, all built once at the freeze boundary. `Projection/fault#FAULT_BAND` `ElementFault` rails every missing node and every structural violation.

## [01]-[INDEX]

- [02]-[NODE_MODEL]: `NodeId` one-regime identity — Guid-v7 occurrence, deterministic `RootedType`, non-rooted `Content` hash — the `Node` `[Union]` property-graph vocabulary with its shared `ToCanonicalBytes` projection, the node-payload component types, and the analytical-geometry decode vocabulary behind the `GeometrySource` port.
- [03]-[ELEMENT_GRAPH]: `ElementGraph` frozen read snapshot with its built-once incidence index and `QuikGraph` view, the memoized `Bake` fold deriving `Element` under the named type→occurrence inheritance, and the incidence accessor family — sections, materials, containment, groups, measured-series rollups.
- [04]-[FEDERATION]: `Federate` unions N tagged source graphs onto one coordination `Header` with collision discrimination and a `FederationReceipt`; `Extract` slices the downward closure of a root set into its own graph under the SOURCE `Header`.

## [02]-[NODE_MODEL]

- Owner: `NodeId` the `[ValueObject<string>]` identity owner over the `IObjectFactory` floor; `Node` the `[Union]` eight-case property-graph vocabulary carrying the shared `ToCanonicalBytes` projection; the node-payload component types the cases compose.
- Cases: the closed eight-case property-graph node family — `Object` · `Material` · `PropertySet` · `QuantitySet` · `Assessment` · `Appearance` · `Coverage` · `Observation`.
- Cases: `Object` is the IfcObjectDefinition mirror: `ObjectKind` occurrence/type, optional `ExternalId` (the Bim-stored IFC GlobalId, re-emitted at `Emit`), first-class `PredefinedType` token value-object, name/tag, optional `OwnerHistory`, schema `SchemaSpan`, and NO `GeoReference` (model georeferencing is a `Header` fact).
- Cases: `Object` carries TWO classification columns — the primary `Classification` (the entity-class-keying pair every query, egress, and diff reads) beside the `Classifications` set of additional standard-system references, because IFC permits MULTIPLE `IfcRelAssociatesClassification` per object (Uniclass and OmniClass simultaneously) and a single field is lossy.
- Cases: `Object`'s `RepresentationContentHash` keyed map content-hashes EVERY geometry — the heavy display `Body` AND the lightweight analytical `Axis` (idealized structural line) and `FootPrint` (space-boundary surface polygon) a discipline resolves by content key — never inline coordinates.
- Cases: `Material` carries a `Composition/material#MATERIAL_COMPOSITION` `MaterialId` with its composition and property sets; `PropertySet`/`QuantitySet` a `Properties/property#PROPERTY_BAG` named bag with its `InheritanceMode`; `Assessment` an `Assessment/assessment#ASSESSMENT_NODE` receipt; `Appearance` a content-keyed `AppearanceSummary`; `Coverage` a `Geospatial/coverage#COVERAGE_NODE` raster/field grid.
- Cases: `Observation` carries an `Assessment/observation#OBSERVATION_SERIES` measured sensor series — the computed assessment's sibling evidence modality, its samples content-keyed by reference.
- Entry: `NodeId.Rooted()` mints a sortable placement rooted id (Guid v7) for an OCCURRENCE `Object`; `NodeId.RootedType(typeSeed)` mints the deterministic-rooted Type id from a `Component`'s volatile-excluded canonical seed (`Node.Object.ToTypeSeedBytes`) through the SAME kernel `ContentHash` `Content` composes, so identical `Component`s dedup to one Type; `NodeId.Content(canonicalBytes)` mints a non-rooted content-hash id through the kernel `ContentHash` entry, `NodeId.OfContent(contentAddress)` mints one from a precomputed `ContentAddress` without re-hashing ONLY when that address IS the node's own content self-hash (`ContentAddress.Of(node.ToCanonicalBytes(tolerance))`), never from a foreign key like an `Assessment.InputKey` (which is a payload field the node's own `ToCanonicalBytes` folds, not the node id); `node.Id` reads any case's id through the abstract override; `node.ToCanonicalBytes(tolerance)` projects the case's semantic content (NO id) into the canonical bytes the `NodeId.Content` mint and the `Projection/address#CONTENT_ADDRESS` diff SHARE.
- Auto: each case carries `NodeId Id` as a positional override of the union's abstract `Id`, so `node.Id` reads without a switch; `ToCanonicalBytes` dispatches the generated total `Switch` writing each case's semantic content (an `Object` its kind/classification/predefined/name/tag/representations/span; a `Material` its key/composition/properties; a bag its set name, inheritance key, and count-prefixed sorted name→value entries, a quantity bag its count-prefixed sorted `GroupIdentity` run beside them; a measure quantized to the tolerance) into the `Projection/address#CANONICAL_WRITER`, the id excluded so a non-rooted node's id derives from its own bytes without circularity; a rooted `Object` mints its id once at authoring — an OCCURRENCE its Guid-v7 placement identity, a TYPE its DETERMINISTIC `NodeId.RootedType` over `ToTypeSeedBytes` (the `WriteObject` projection with `includeVolatile: false`, the volatile `Representations` AND secondary `Classifications` excluded so a later geometry attach or standard-classification stamp never re-keys the Type and identical `Component`s dedup to one Type) — the IFC GlobalId staying a Bim-stored projection attribute re-emitted at `Emit`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`IObjectFactory`), LanguageExt.Core (`Option`/`Seq`/`Map`), NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-hash entry the `NodeId.Content` mint composes). `Rasm.Element.Graph` OWNS the neutral `Vector3` the `AxisCurve`/`FootprintPolygon` analytical shapes carry (the kernel `Rasm.Numerics` coordinate is the host `Vector3d` the seam Boundary forbids; no neutral kernel triple exists), so the seam mints its own host-free coordinate AND its full vector algebra (`Length`/`Distance`/`Dot`/`Cross`/`Unit` + the `UnitX`/`UnitY`/`UnitZ`/`Zero` constants + the `+`/`-`/`*` operators) — the `Rasm.Bim` scan-to-BIM orientation classifier (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) and the `Rasm.Compute` structural load-vector folds compose THIS one coordinate rather than a kernel/host vector, so a phantom kernel `Vector3` or a `System.Numerics.Vector3` crossing the analytical-shape math is the deleted host leak.
- Growth: a new node concept is one `Node` case carrying its payload type, the payload owning its own `CanonicalBytes` contribution so the arm is one ordinal and one delegation (the `Observation` series landed exactly this way; a `Schedule`/`Task` node lands here only if 4D becomes a real target); a new object axis is one column on the `Object` case; a new node-payload component is one type on its owning sibling page; never a parallel node family and never a second identity scheme — the `NodeId` is the one owner, `MaterialId` a node attribute, not a parallel key. New object columns land with their wire field and their presence-delimited `ToCanonicalBytes` contribution in one edit under the additive contract-evolution law — `ObjectType` is the landed instance, carrying the IFC-canonical `(PredefinedType = USERDEFINED, ObjectType = label)` designation for BOTH object kinds — the Bim `Projection/semantic` `UserLabel` ingress reads it off `IfcObject.ObjectType` or `IfcElementType.ElementType` and `Projection/egress` `StampPredefined` re-stamps the matching slot, one column for the exact round-trip.
- Boundary: `NodeId` is the ONE identity owner: occurrence roots use Guid-v7 placement identity, type roots hash the representation-excluded type seed, and non-rooted nodes hash full canonical content. `Object` carries the primary and co-applied classifications, `PredefinedType`, content-keyed representations, owner history, and schema span; geometry stays behind `GeometrySource`, model georeferencing stays on `Header`, and IFC rosters stay in the Bim projector. `ToCanonicalBytes` is the shared id/diff projection, and bag source rank participates in property/quantity node identity. `AppearanceSummary` is FROZEN at its seven-value preimage and its `Of(r, g, b, metallic, roughness, opacity, transmissive, key) -> Fin<AppearanceSummary>` arity: a peer carrying a richer appearance fact — a baked texture-set key, an environment binding, a UV transform, a measured refractive index — hangs it behind the `AppearanceKey` on its own wire, because an eighth column re-keys every stored `Node.Appearance` and forks the Bim dedup key in the same edit.

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
using static Rasm.Element.Projection.AdmissionSlots;

namespace Rasm.Element.Graph;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();

    // Rooted() mints the sortable placement identity (Guid v7) an OCCURRENCE Object roots on — its identity IS its unique
    // placement, NOT an IFC GlobalId (the compressed GlobalId is a Bim-stored attribute re-emitted at Emit). A TYPE Object
    // is rooted too, but DETERMINISTICALLY (RootedType), so identical Components dedup to one Type — one regime, two seedings.
    public static NodeId Rooted() => Create(Guid.CreateVersion7().ToString("N"));

    // RootedType mints a TYPE Object deterministically through the kernel `ContentHash` seed-zero XxHash128 (the ONE hasher
    // Content composes) over the Representations-EXCLUDED canonical seed (Node.Object.ToTypeSeedBytes), so identical
    // Components mint ONE Type id and a later geometry attach never re-keys it — the SAME regime as Rooted with a
    // content-derived seed in place of the random placement Guid. The Component projection composes RootedType; a model
    // author composes Rooted for an Occurrence.
    public static NodeId RootedType(ReadOnlySpan<byte> typeSeed) =>
    Create(ContentHash.Of(typeSeed).ToString("X32", System.Globalization.CultureInfo.InvariantCulture));

    // Content hashes a non-rooted node through the kernel `ContentHash` seed-zero entry over its canonical bytes,
    // sharing the SAME projection the ContentAddress diff reads, so identity is content-stable cross-runtime.
    public static NodeId Content(ReadOnlySpan<byte> canonicalBytes) =>
    Create(ContentHash.Of(canonicalBytes).ToString("X32", System.Globalization.CultureInfo.InvariantCulture));

    // Mint a non-rooted id from a PRECOMPUTED `ContentAddress` WITHOUT re-hashing — valid ONLY when the address IS the
    // node's OWN content self-hash (the bytes hashed once, the UInt128 carried forward), so OfContent(addr) == Content(bytes).
    // NEVER a back-door for a FOREIGN key: an `Assessment.InputKey` is a payload FIELD the node's own ToCanonicalBytes
    // folds, so OfContent(InputKey) would store an id the Projection/address#CONTENT_ADDRESS Verify dual can never
    // reproduce — the deleted form.
    public static NodeId OfContent(ContentAddress address) =>
    Create(address.Value.ToString("X32", System.Globalization.CultureInfo.InvariantCulture));
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

// RepresentationContentHash references geometry through a keyed map RepresentationIdentifier → content hash (M2), neutral-named (no IFC leak).
// EVERY geometry — the heavy display Body AND the analytical Axis/FootPrint — rides the blob store by content hash and
// resolves one-hop by key, NEVER inline coordinate geometry on the node. Body/Axis/Box/FootPrint are the standard
// IFC RepresentationIdentifier reads; an absent identifier is None.
public readonly record struct RepresentationContentHash(Map<string, UInt128> ByIdentifier) {
    public static readonly RepresentationContentHash Empty = new(Map<string, UInt128>());
    public Option<UInt128> Body => ByIdentifier.Find("Body");
    public Option<UInt128> Axis => ByIdentifier.Find("Axis");
    public Option<UInt128> Box => ByIdentifier.Find("Box");
    public Option<UInt128> FootPrint => ByIdentifier.Find("FootPrint");
    public RepresentationContentHash With(string identifier, UInt128 hash) => this with { ByIdentifier = ByIdentifier.AddOrUpdate(identifier, hash) };
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
    public Option<AxisCurve> Axis(RepresentationContentHash representations) => representations.Axis.Bind(ResolveAxis);
    public Option<FootprintPolygon> Footprint(RepresentationContentHash representations) => representations.FootPrint.Bind(ResolveFootprint);
}

// Appearance node summary: a content-keyed reference to the full BSDF (authored in Rasm.Materials) plus the neutral
// canonical PBR scalars a consumer reads flat. The SEAM owns the AppearanceKey derivation through Of, so the
// Rasm.Materials and Rasm.Bim lowerings compose ONE factory and mint the SAME key for one surface (a local
// CanonicalWriter beside this factory in either peer is the byte-order divergence defect). Transmissive is the
// REFRACTIVE flag DISTINCT from Opacity (an opaque-alpha glass still transmits — the GLB KHR_materials_transmission
// read); both are load-bearing in the KEY, so two appearances differing only in alpha or refraction get distinct
// Node.Appearance ids even though the appearance canonical arm hashes only the AppearanceKey.
//
// THE SEVEN-VALUE PREIMAGE IS FROZEN: BaseColorR/G/B · Metallic · Roughness · Opacity · Transmissive, in that
// order, are the whole AppearanceKey preimage — an eighth column re-keys every Node.Appearance in every stored
// snapshot AND forks the Rasm.Bim dedup key the IFC surface-style projector mints against, so a richer appearance
// fact (a baked texture-set key, an IBL environment binding, a UV transform, a measured IOR) rides the payload
// BEHIND this key on the peer's own wire, never a column here. Of's arity IS that freeze: eight parameters — the
// seven preimage values plus the Op key the ValueRejected rail correlates on — returning Fin<AppearanceSummary>,
// so every composing peer spells one call and a bare non-Fin or tolerance-bearing spelling is the drift defect.
public sealed record AppearanceSummary {
    private AppearanceSummary(
        UInt128 appearanceKey, double baseColorR, double baseColorG, double baseColorB,
        double metallic, double roughness, double opacity, bool transmissive) =>
        (AppearanceKey, BaseColorR, BaseColorG, BaseColorB, Metallic, Roughness, Opacity, Transmissive) =
            (appearanceKey, baseColorR, baseColorG, baseColorB, metallic, roughness, opacity, transmissive);

    public UInt128 AppearanceKey { get; }
    public double BaseColorR { get; }
    public double BaseColorG { get; }
    public double BaseColorB { get; }
    public double Metallic { get; }
    public double Roughness { get; }
    public double Opacity { get; }
    public bool Transmissive { get; }

    // Of IS the ONE seam-owned appearance content-key factory both the Rasm.Materials MaterialWire.Summary lowering and the
    // Rasm.Bim AppearanceProjection.Project lowering compose: write the neutral PBR vector (base R/G/B + metallic +
    // roughness + opacity + transmissive) through the seam CanonicalWriter and mint the AppearanceKey via ContentAddress.Of
    // (the kernel seed-zero XxHash128, the ONE hasher). tolerance 0.0 hashes the raw IEEE bits of the appearance scalars —
    // they are not Header-quantized measures — and the writer canonicalizes -0.0/NaN/inf, so the key is cross-runtime stable.
    public static Fin<AppearanceSummary> Of(
        double r, double g, double b, double metallic, double roughness, double opacity, bool transmissive, Op key) =>
        Channels(r, g, b, metallic, roughness, opacity)
            ? Fin.Succ(Create(r, g, b, metallic, roughness, opacity, transmissive))
            : ElementFault.ValueRejected(key, "<appearance-channel-out-of-unit-range>");

    public static Fin<AppearanceSummary> Rehydrate(
        UInt128 appearanceKey, double r, double g, double b,
        double metallic, double roughness, double opacity, bool transmissive, Op key) =>
        Of(r, g, b, metallic, roughness, opacity, transmissive, key).Bind(summary =>
            summary.AppearanceKey == appearanceKey
                ? Fin.Succ(summary)
                : ElementFault.AddressUnstable(key, "<appearance-key-mismatch>"));

    private static AppearanceSummary Create(
        double r, double g, double b, double metallic, double roughness, double opacity, bool transmissive) {
        CanonicalWriter writer = new(0.0);
        writer.Double(r).Double(g).Double(b).Double(metallic).Double(roughness).Double(opacity).Bool(transmissive);
        return new AppearanceSummary(ContentAddress.Of(writer.ToBytes().Span).Value, r, g, b, metallic, roughness, opacity, transmissive);
    }

    private static bool Channels(double r, double g, double b, double metallic, double roughness, double opacity) =>
        r is >= 0.0 and <= 1.0 && g is >= 0.0 and <= 1.0 && b is >= 0.0 and <= 1.0
        && metallic is >= 0.0 and <= 1.0 && roughness is >= 0.0 and <= 1.0 && opacity is >= 0.0 and <= 1.0;
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
        // ObjectType carries the IFC-canonical `(PredefinedType = USERDEFINED, ObjectType = label)` pair: the label is
        // the node's own user-defined type name, meaningless without its discriminant, so it sits beside it rather than in a bag.
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

        // ToTypeSeedBytes projects the volatile-EXCLUDED canonical seed NodeId.RootedType hashes for the deterministic Type id: the SAME
        // WriteObject the full hash uses, with includeVolatile: false, so seed and full hash agree byte-for-byte on the
        // stable identity columns and differ only by the volatile Representations and secondary Classifications blocks the seed omits. The Component
        // projection composes it for a Kind == ObjectKind.Type node; an Occurrence is Guid-v7 rooted, never seeded.
        public ReadOnlyMemory<byte> ToTypeSeedBytes(double tolerance) {
            CanonicalWriter w = new(tolerance);
            WriteObject(w, this, includeVolatile: false);
            return w.ToBytes();
        }
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

    // ToCanonicalBytes IS the ONE canonical value codec — the id is EXCLUDED (a non-rooted id derives from these bytes), measures quantize
    // to the tolerance, attribute order is explicit, and the diff + the id mint share it. Each complex payload delegates
    // to its OWNER's CanonicalBytes (Composition/material MaterialComposition + MaterialPropertySet, Properties/property
    // PropertyValue, Assessment/assessment AssessmentPayload, Geospatial/coverage CoverageGrid) so the projection is never
    // re-derived per case; geometry rides the content-hashed Representations map (its content keys ARE the geometry
    // identity), never inline coordinates. PROVENANCE
    // is excluded — OwnerHistory (who/when, H9) is a separate additive axis, not content, so a re-stamp never forks the id;
    // lazy caches (incidence/QuikGraph/Bake memo) likewise sit outside the byte projection.
    public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
    CanonicalWriter w = new(tolerance);
    Switch(
    @object: o => WriteObject(w, o, includeVolatile: true),
    // Mechanical and Orthotropic share Discipline.Structural, so the discipline-key sort TIES them and a stable sort would
    // leak Seq insertion order into the node bytes — two [UnorderedEquality]-equal Material nodes minting distinct content
    // ids. The per-property full-byte tiebreak (each property's own CanonicalBytes, case ordinal first, ordered through
    // ContentAddress.ByteOrder) is TOTAL, so a same-discipline pair orders identically regardless of insertion order; a
    // material carrying one set per discipline never ties, so its bytes are unchanged.
    material: m => { w.Ordinal(1); w.String(m.MaterialKey.Value); m.Composition.CanonicalBytes(w); w.Ordinal(m.Properties.Count); foreach (var p in m.Properties.OrderBy(static p => p.Discipline.Key, StringComparer.Ordinal).ThenBy(p => { CanonicalWriter k = new(tolerance); p.CanonicalBytes(k); return k.ToBytes(); }, ContentAddress.ByteOrder)) { p.CanonicalBytes(w); } },
    // Ordinal(count) prefixes each bag, the self-delimiting precondition every raw-append consumer relies on — ContentAddress.Of(Node)
    // and the GraphDelta node sections concat String(id)+Raw(bytes), so an UNCOUNTED trailing row run would absorb the
    // following segment's bytes (two distinct deltas, one hash): the Projection/address#CANONICAL_WRITER count-prefix law.
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
    return w.ToBytes();
    }

    // WriteObject factors the Object canonical projection so BOTH the full content hash (volatile columns INCLUDED) and the
    // deterministic Type-id seed (volatile columns EXCLUDED) compose ONE writer over the stable identity columns.
    // TWO conditional regions, both volatile because they attach AFTER a Type is identified: the Representations block
    // (geometry attaches later) and the SECONDARY Classifications set (a Type-borne standard classification — a
    // Uniclass/OmniClass stamp — lands post-mint and must never re-key the Type; the PRIMARY Classification stays in the
    // seed as the entity-class identity). The includeVolatile: true path stays byte-for-byte the parity-corpus
    // projection and the Type seed differs only by the omitted blocks.
    //
    // Placement is EXCLUDED from BOTH paths — the OwnerHistory-exclusion law, for the same reason and one more: a
    // rigid move would re-key the node, and the Rasm.Bim review diff that keys placement in its own bucket could no
    // longer report Moved at all, because the moved node would no longer be the same node. Placement carries its own
    // CanonicalBytes for that bucket and the wire; no arm here calls it.
    static void WriteObject(CanonicalWriter w, Node.Object o, bool includeVolatile) {
    w.Ordinal(0); w.String(o.Kind.Key); w.Bool(o.ExternalId.IsSome); o.ExternalId.IfSome(e => w.String(e)); w.String(o.Classification.System); w.String(o.Classification.Code); w.String(o.Classification.Edition);
    if (includeVolatile) { w.Ordinal(o.Classifications.Count); foreach (var c in o.Classifications.OrderBy(static x => x.System, StringComparer.Ordinal).ThenBy(static x => x.Code, StringComparer.Ordinal).ThenBy(static x => x.Edition, StringComparer.Ordinal)) { w.String(c.System); w.String(c.Code); w.String(c.Edition); } }
    w.String(o.PredefinedType.Token); w.Bool(o.ObjectType.IsSome); o.ObjectType.IfSome(t => w.String(t)); w.String(o.Name); w.String(o.Tag);
    if (includeVolatile) { w.Ordinal(o.Representations.ByIdentifier.Count); foreach (var (k, h) in o.Representations.ByIdentifier.OrderBy(static p => p.Key, StringComparer.Ordinal)) { w.String(k); w.U128(h); } }
    w.String(o.Span.IntroducedIn.Key); w.Bool(o.Span.RemovedIn.IsSome); o.Span.RemovedIn.IfSome(r => w.String(r.Key));
    }

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

- Owner: `Header` the model header (`ReleaseVersion` + `ModelView` + `Geospatial/reference#GEO_REFERENCE` `GeoReference` + `Tolerance` + `Instant` + `StepHeader` + the `Properties/quantity#MEASURE_VALUE` `UnitScheme` presentation declaration) carrying the ONE semantic-header `CanonicalBytes` projection both the `Projection/address#CONTENT_ADDRESS` `OfGraph` snapshot key and the `Graph/delta#GRAPH_DELTA` `GraphDelta.ToCanonicalBytes` header contribution compose (the projection owned once, never re-spelled per call site); `ElementGraph` the frozen read snapshot carrying the nodes, edges, the built-once incidence index, and the memoized `Bake`; `Element` the derived-fold "has it all" result; `BakedMaterial` the material-plus-usage pair `Bake` folds from an `Associate` edge (the occurrence's own AND, via the named inheritance, the `Component`'s, unioned by `MaterialKey`); `TypeBinding` the named type→occurrence inheritance carrier `Bake` produces from the `Assign.TypeDefinition` resolution (the type id + the inherited `BakedMaterial` set / resolved `SectionProperties` / secondary `Classification`s), surfaced as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes.
- Entry: `ElementGraph.Of(header, nodes, edges)` builds the frozen snapshot — `ToFrozenDictionary` over the nodes, the incidence index grouping every edge by every node its `Members` touch, the `MaterialId`-keyed material index, the lazy `QuikGraph` `BidirectionalGraph` topology view over `TaggedEdge<NodeId, Relationship>` legs, and an empty `Bake` memo; `Genesis(header)` seeds the empty header-only snapshot a model-creating session or a Marten stream rehydrate builds onto; `Apply(delta, key)` advances a snapshot by a validated `Graph/delta#GRAPH_DELTA` `GraphDelta` (the persistence rehydrate + live-apply entry), `Fin<T>` railing `ElementFault.NodeAbsent` on a corrupt delta whose added edge names an absent member — either binary endpoint or a `Connect`'s realizing intermediary, the full `Relationship.Members` closure.
- Entry: `Bake(objectId, key)` folds the reachable subgraph from an `Object` node into an `Element`, memoized by `objectId` within the snapshot (a new snapshot from a `Graph/delta#GRAPH_DELTA` carries a fresh memo), `Fin<T>` railing `ElementFault.NodeAbsent` on an absent root and `ElementFault.RelationshipInvalid` on a cyclic `Compose` chain (a `Compose` ancestry set threaded through the fold); `Topology()` reads the cached `QuikGraph` view — `TaggedEdge<NodeId, Relationship>` legs, so a kind-aware traversal reads the edge off its own tag — a reachability or topological-order consumer composes, and `TopologyOf(admits)` scopes that one view to an edge predicate through `FilteredBidirectionalGraph` without a second materialization; the rooted ancestry read is `ContainmentPath`, since the `Compose` graph is MULTI-PARENT (a part aggregated by one whole and contained by another) and no rooted-tree ancestor algorithm binds it.
- Entry: the read accessors `ObjectNodes`/`Find`/`Material`/`MaterialsOf`/`CompositionOf`/`PropertiesOf`/`SectionOf` enumerate the object roots and resolve a node (raw or typed by case) and the material/composition/property/section subgraph a member binds — `MaterialsOf` carrying the one-hop type-resolved fallback the other three compose (an occurrence with no own material/profile reads its `Component`'s), the Op-FREE `SectionOf(member)` signature FROZEN.
- Entry: the spatial/group family `ContainerOf`/`ContainmentPath`/`GroupsOf`/`MembersOf` resolves the `Compose.Contain` breadcrumb and the `Assign.Group` memberships (which storey contains this column, every element in system X, the zones a space belongs to) as Op-free incidence reads, and `ObservationsUnder(root, key)` rolls the measured series over the same OWNING `Compose` closure `BakeParts` recurses so a whole answers for its parts' sensors — together the polymorphic surface a `Rasm.Compute` analysis route, a Persistence index pass, and an AppUi model tree read the concrete graph through, the discipline reads (loads/supports/spaces/areas) composing in Compute from these primitives.
- Auto: `Of` builds the incidence index, the `MaterialId`-keyed material index the `Material(key)` read serves off, and a topology containing every node, including isolated vertices. `Bake` folds one root's incidence: property definitions become bags, assessments become receipts, observations become measured series off the occurrence alone, associations become material/appearance/coverage values, owning compositions recurse into parts, and `Assign.TypeDefinition` applies the named type inheritance once. Topology and memo ride the sealed `ElementGraph` as lazy equality-excluded caches; only `Of`, `Genesis`, and `Apply` mint snapshots.
- Receipt: the `Element` is the one flat record a consumer reads — `element.Properties.Find(name)`, `element.Materials`, `element.Assessments`, `element.Observations`, `element.Appearance`, `element.Coverages`, `element.Parts`, and `element.TypeId` (the inherited `Component`, the generator's type-representation recovery key), with `ObservationsUnder` the whole-over-parts measured rollup beside them — "has it all" in one `Bake`, never a join across the graph, and the computed-versus-measured commissioning read is `element.Assessments` beside `element.Observations` off one baked root rather than a historian join; the `ElementGraph` is the immutable read snapshot Persistence persists and the projectors assemble onto, its `Generator.Equals` structural equality and `Inequalities` member diff feeding the Persistence 3-way `StructuralMerge`; the `QuikGraph` topology view answers reachability and topological order for a consumer without a second graph, the rooted containment breadcrumb reading `ContainmentPath`.
- Packages: `Generator.Equals` (`[Equatable]` snapshot equality, `[StringEquality]`/`[UnorderedEquality]`/`[IgnoreEquality]` member policies, `Inequalities` diff, and the generated `EqualityComparer.Default` reused as the LINQ/`HashSet` key comparer outside generated code), QuikGraph (`BidirectionalGraph`/`TaggedEdge` topology view, `FilteredBidirectionalGraph` kind scoping, `AlgorithmExtensions`), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin`), System.Collections.Frozen/Immutable, NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key).
- Growth: a new derived element field is one column on `Element` the `Bake` fold populates from an existing edge kind; a new edge semantic the fold reads is one arm in `Bake`; a new type-inherited `Seq` is one `UnionBy` arm in the named inheritance, a new occurrence-overrides-type single field one fall-back guard; the working/frozen split keeps the live delta path in the HAMT (`Graph/delta`) and the read path in the frozen snapshot, so neither grows the other; never a second stored `Element` record beside the graph, never a second identity scheme for the deterministic Type id.
- Boundary: the `Element` is a DERIVED FOLD, never a stored record — one flat read comes from `Bake` over the graph, and a parallel stored element record beside it is the deleted form.
- Boundary: the graph splits by PHASE — the live authoring/delta path is a `TrackingHashMap` HAMT (`Graph/delta` owns it for O(log n) structural sharing and the change record its `Diff` reads) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` at the freeze boundary), so a mutable working graph is never confused with a frozen read snapshot.
- Boundary: the incidence index, the material index, and the `QuikGraph` view are built ONCE per snapshot and the `Bake` memo is keyed by object within the snapshot, invalidated only by a new snapshot from a delta, so a re-`Bake` is O(1) and a graph edit is O(log n).
- Boundary: the NAMED type→occurrence inheritance applies once in `Bake` — single fields occurrence-overrides-type, the materials/assessments/classifications `Seq`s union+dedup-by-key — and is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the `PropertyBag.Merge` owns, which stays bag-only.
- Boundary: the observations and coverages `Seq`s are deliberately NOT inherited, which the `GatherPolicy` row states as data: a `Component` is a catalogue entry no instrument is mounted on and no field is sampled over, so a type-borne series claims every realization reports one sensor's data.
- Boundary: the `MaterialsOf`/`SectionOf` type-resolved fallback is ONE hop (a `Component` is not itself typed), so the FROZEN Op-free `SectionOf(member)` signature `Rasm.Compute` reads is untouched.
- Boundary: a TYPE `Object`'s deterministic id excludes the volatile `Representations`, so a geometry attach re-keys neither the Type node nor the cached `Bake`.
- Boundary: the `Header` carries the `GeoReference`, the `StepHeader`, and the `UnitScheme` (the `IfcUnitAssignment` unit-presentation declaration — canonical-bytes-excluded, so display units never fork identity), and the `Object` nodes carry the `OwnerHistory` and the `SchemaSpan`, so the model's provenance, declared units, and schema span ride the graph rather than a side channel.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// GatherPolicy is the ONE row the shared incidence gather reads. An OCCURRENCE gathers every family; a COMPONENT omits
// the two a catalogue entry cannot own — no instrument is mounted on a Component, so a type-borne ObservationSeries claims
// every realization reports one sensor's data, and a Coverage field is sampled over a PLACED occurrence, never over a
// catalogue row. The two gathering shapes are columns on one row, so a third is one more row and neither the fold nor
// its arms fork; the deleted form is two near-identical hand-rolled folds differing only in the arms one omits.
[SmartEnum]
public sealed partial class GatherPolicy {
    public static readonly GatherPolicy Occurrence = new(observations: true, coverages: true);
    public static readonly GatherPolicy Component = new(observations: false, coverages: false);

    public bool Observations { get; }
    public bool Coverages { get; }
}

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
    ReleaseVersion Schema, ModelView View, GeoReference Reference, double Tolerance, Instant At, StepHeader Step,
    UnitScheme Units = default) {
    public static Header Default(Instant at) =>
    new(ReleaseVersion.Ifc4X3Add2, ModelView.Ifc4Reference, GeoReference.Identity, 1e-6, at, StepHeader.Empty);

    // CanonicalBytes IS the ONE semantic-header content projection both the Projection/address#CONTENT_ADDRESS OfGraph snapshot key and the
    // Graph/delta#GRAPH_DELTA GraphDelta.ToCanonicalBytes header contribution compose, so a header's bytes are owned ONCE
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
    [IgnoreEquality] readonly Lazy<QuikGraph.BidirectionalGraph<NodeId, QuikGraph.TaggedEdge<NodeId, Relationship>>> topology;

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
    // Build the view from the directed adjacency each edge contributes (Relationship.DirectedPairs) — a binary edge is
    // one leg, a Connect carrying a realizing intermediary is the two legs From->Realizing->To, a Generic edge adds a
    // source->participant leg per roster member — so reachability traverses THROUGH the realizing node and reaches every
    // n-ary participant, never an endpoints-only From->To shortcut that hides them. Each leg carries its Relationship
    // as the edge TAG: an untagged leg erases RelationshipKind and forces every kind-aware traversal to hand-roll the
    // id-pair-back-to-edge resolution, where the tag hands a FilteredBidirectionalGraph predicate the edge outright.
    // allowParallelEdges stays true — one node pair legitimately carries several edges (an Assign beside a Connect,
    // two Compose flavors), and a Connect's two realizing legs share the intermediary vertex.
    topology = new(() => {
    QuikGraph.BidirectionalGraph<NodeId, QuikGraph.TaggedEdge<NodeId, Relationship>> graph = new(allowParallelEdges: true);
    graph.AddVertexRange(nodes.Keys);
    foreach (Relationship edge in edges) { foreach ((NodeId from, NodeId to) in edge.DirectedPairs) { graph.AddEdge(new QuikGraph.TaggedEdge<NodeId, Relationship>(from, to, edge)); } }
    return graph;
    });
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
    Some: member => ElementFault.NodeAbsent(key, $"<replay-edge-member-absent:{member.Value}>"),
    None: () => Fin.Succ(next));
    }

    // ObjectNodes projects the object (element-root) nodes a consumer iterates to bake or index every element — the typed
    // projection over the node map a Rasm.Persistence Query/index pass folds, never a per-element re-scan of the whole node set.
    public Seq<Node.Object> ObjectNodes => toSeq(Nodes.Values).Choose(static n => n is Node.Object o ? Some(o) : None);

    public ImmutableArray<Relationship> EdgesAt(NodeId node) => incidence.GetValueOrDefault(node, []);

    public QuikGraph.BidirectionalGraph<NodeId, QuikGraph.TaggedEdge<NodeId, Relationship>> Topology() => topology.Value;

    // TopologyOf is the kind-scoped walk a Compose closure or a containment sweep takes — QuikGraph's zero-copy
    // predicate view over the ONE built topology, never a second graph materialized per kind. The predicate reads the edge off the leg's
    // own Tag, so a caller filters on Kind/SubKind directly (`edge => edge.IsContainment`) with no id-pair lookback.
    public QuikGraph.Predicates.FilteredBidirectionalGraph<
    NodeId,
    QuikGraph.TaggedEdge<NodeId, Relationship>,
    QuikGraph.BidirectionalGraph<NodeId, QuikGraph.TaggedEdge<NodeId, Relationship>>> TopologyOf(Func<Relationship, bool> admits) =>
    new(topology.Value, static _ => true, leg => admits(leg.Tag));

    // --- [READ_ACCESSORS] -----------------------------------------------------------------
    // ElementGraph exposes the polymorphic read surface a Rasm.Compute analysis route reads the concrete graph through — resolve a node
    // (raw or typed by case), and the material/composition/property/section subgraph a member binds. Compute composes its
    // discipline reads (loads/supports off the structural Connect/Generic edges, spaces/bounding-surfaces off the
    // space-boundary Generic edges, the analytical axis/footprint geometry resolved BY CONTENT KEY from member.Representations
    // .Axis/.FootPrint, areas off the quantity bags) from these primitives + EdgesAt/Topology/Bake — the Bim projector bakes
    // that structural/energy subgraph at ingest; the seam owns the material+section reads (it owns those nodes), the
    // discipline physics lives in Compute, never here.
    public Option<Node> Find(NodeId id) => Nodes.TryGetValue(id, out Node? n) ? Some(n) : None;

    public Option<T> Find<T>(NodeId id) where T : Node => Find(id).Bind(static n => n is T t ? Some(t) : None);

    public Option<Node.Material> Material(NodeId id) => Find<Node.Material>(id);

    // Keyed off the freeze-boundary index, never a node scan: this read fires per member inside Bake and per
    // discipline route in Compute, so a scan would make one baked element cost O(members x nodes).
    public Option<Node.Material> Material(MaterialId key) =>
    materials.TryGetValue(key, out Node.Material? m) ? Some(m) : None;

    // DirectMaterialsOf reads the member's DIRECTLY-associated material nodes — the Associate(Material) edges off ONE node — the
    // occurrence-OR-type projection MaterialsOf composes for both the occurrence and (one hop) its Component, so neither side re-spells it.
    Seq<Node.Material> DirectMaterialsOf(NodeId node) =>
    toSeq(EdgesAt(node)).Choose(e => e is Relationship.Associate r && r.Subject == node && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m ? Some(m) : None);

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

    // --- [SPATIAL_AND_GROUP_READS] ----------------------------------------------------------
    // ContainerOf/ContainmentPath/GroupsOf/MembersOf own the containment and group read family every BIM consumer takes off the
    // frozen snapshot — the spatial breadcrumb (space → storey → building → site off the Compose.Contain edges) and the
    // group/system/zone memberships (a fire compartment, a thermal zone, an MEP system, a load group all ride Assign.Group
    // edges) — Op-FREE incidence reads, the same one-owner discipline MaterialsOf/SectionOf set for the material subgraph; a
    // per-consumer EdgesAt hand-walk with case tests is the deleted form. Aggregation stays the consumer's composition: a
    // zone rollup is MembersOf + Bake + MeasureValue.Sum, never a seam-owned report.
    public Option<NodeId> ContainerOf(NodeId member) =>
    toSeq(EdgesAt(member)).Choose(e => e is Relationship.Compose { SubKind: var k } c && k == ComposeKind.Contain && c.Part == member ? Some(c.Whole) : None).Head;

    // Nearest-first containment chain — the model-tree breadcrumb and the analysis-scope filter. The seen-set bounds a
    // corrupt cyclic snapshot into termination (the Op-free read never rails; Bake owns the railed cycle fault).
    public Seq<NodeId> ContainmentPath(NodeId member) => Containers(member, ImmutableHashSet.Create(member));

    Seq<NodeId> Containers(NodeId node, ImmutableHashSet<NodeId> seen) =>
    ContainerOf(node).Filter(c => !seen.Contains(c)).Match(
    Some: c => Seq(c) + Containers(c, seen.Add(c)),
    None: () => Seq<NodeId>());

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
    ? ElementFault.RelationshipInvalid(key, $"<bake-compose-cycle:{objectId.Value}>")
    : bakeMemo.TryGetValue(objectId, out Element? cached)
    ? Fin.Succ(cached)
    : Nodes.TryGetValue(objectId, out Node? node) && node is Node.Object root
    ? BakeObject(root, key, ancestry.Add(objectId)).Map(element => { bakeMemo[objectId] = element; return element; })
    : ElementFault.NodeAbsent(key, $"<bake-root-absent:{objectId.Value}>");

    Fin<Element> BakeObject(Node.Object root, Op key, ImmutableHashSet<NodeId> ancestry) {
    Gathered own = Gather(root.Id, GatherPolicy.Occurrence);
    // TypeResolutionOf applies the NAMED type→occurrence inheritance (Relations/relation#EDGE_ALGEBRA Assign.TypeDefinition): resolve the Component
    // (type Object), then merge occurrence-over-type — DISTINCT from the Properties/property#PROPERTY_BAG InheritanceMode
    // value-bag precedence (which stays the PropertyBag.Merge below). Single fields occurrence-overrides-type; the Seq
    // fields materials/assessments/classifications union + dedup-by-key. None for a bare occurrence (no Component bound).
    Option<(Node.Object Type, Gathered Data)> typeFold = TypeResolutionOf(root.Id);
    // Properties/Quantities: the EXISTING InheritanceMode value-bag merge (type-then-occurrence precedence via Merge) — the
    // named inheritance does NOT touch the bag-precedence the bag Merge owns, only the single fields and the Seq sets.
    Seq<PropertyBag> properties = MergeBagSets(typeFold.Map(static t => t.Data.Properties).IfNone(Seq<PropertyBag>()), own.Properties);
    Seq<QuantityBag> quantities = MergeBagSets(typeFold.Map(static t => t.Data.Quantities).IfNone(Seq<QuantityBag>()), own.Quantities);
    // Materials/Assessments/Classifications: occurrence-precedence Seq union, dedup by key — the MaterialKey string, the
    // (Discipline, Route, InputKey) assessment cache triple, and the (System, Code, Edition) classification identity.
    Seq<BakedMaterial> materials = UnionBy(own.Materials, typeFold.Map(static t => t.Data.Materials).IfNone(Seq<BakedMaterial>()), static b => b.Material.MaterialKey.Value);
    Seq<AssessmentPayload> assessments = UnionBy(own.Assessments, typeFold.Map(static t => t.Data.Assessments).IfNone(Seq<AssessmentPayload>()), static a => (a.Discipline.Key, a.Route.Value, a.InputKey));
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
    // alike — parameterized by the GatherPolicy row that names which optional families the subject can own.
    // EdgesAt(subject) is walked ONCE for the Assign property/quantity/assessment/observation definitions AND all three
    // Associate resource kinds (Material+usage→BakedMaterial, Appearance, Coverage), never a per-family re-scan of the
    // same dispatch, and the two callers differ by a policy value rather than by an arm roster one of them omits. The Associate resource
    // kinds are the LegalLink Material/Appearance/Coverage closure (Graph/delta#GRAPH_DELTA), so the three arms mirror
    // that legality exactly.
    Gathered Gather(NodeId subject, GatherPolicy policy) =>
    toSeq(EdgesAt(subject)).Fold(Gathered.Empty, (acc, edge) => edge switch {
    Relationship.Assign a when a.Subject == subject && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.PropertySet ps => acc with { Properties = acc.Properties.Add(ps.Bag) },
    Relationship.Assign a when a.Subject == subject && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.QuantitySet qs => acc with { Quantities = acc.Quantities.Add(qs.Bag) },
    Relationship.Assign a when a.Subject == subject && a.SubKind == AssignKind.Assessment && Nodes.TryGetValue(a.Definition, out var d) && d is Node.Assessment asm => acc with { Assessments = acc.Assessments.Add(asm.Payload) },
    Relationship.Assign a when policy.Observations && a.Subject == subject && a.SubKind == AssignKind.Observation && Nodes.TryGetValue(a.Definition, out var d) && d is Node.Observation obs => acc with { Observations = acc.Observations.Add(obs.Series) },
    Relationship.Associate r when r.Subject == subject && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m => acc with { Materials = acc.Materials.Add(new BakedMaterial(m, r.Usage)) },
    Relationship.Associate r when r.Subject == subject && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Appearance ap => acc with { Appearance = ap.Summary },
    Relationship.Associate r when policy.Coverages && r.Subject == subject && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Coverage c => acc with { Coverages = acc.Coverages.Add(c.Grid) },
    _ => acc,
    });

    // TypeResolutionOf resolves the named type→occurrence inheritance: the Assign.TypeDefinition edge resolved to the Component
    // (type Object), then the SAME Gather under the Component policy — so the type's data is gathered as DATA in one pass,
    // never a recursive Bake, while the type's single fields and secondary Classifications ride the resolved Object
    // and the section derives from the type materials' ProfileSet. None for a bare occurrence with no Component binding. The
    // type carries NO further TypeDefinition edge (a Component is not itself typed), so this is a single one-hop resolution.
    Option<(Node.Object Type, Gathered Data)> TypeResolutionOf(NodeId occurrence) =>
    TypeObjectOf(occurrence).Bind(typeId =>
    Find<Node.Object>(typeId).Map(typeObj => (Type: typeObj, Data: Gather(typeId, GatherPolicy.Component))));

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
- Auto: every refusal accumulates through the `Projection/fault#ADMISSION_SLOTS` slot algebra over `Validation<Error,_>` and collapses to `Fin<T>` once at the return, so a federation attempt reports every divergent source and every colliding id in ONE failure rather than the first it meets.
- Auto: both entries mint through ONE `GraphDelta` carrying the union (or slice) as `AddedNodes`/`AddedEdges` with a `Reheader`, run through `AdmitOnto(Genesis(header), key)` — the sanctioned validating mint, so `LegalLink` re-crosses every foreign edge; a raw `ElementGraph.Of` over foreign edges is the deleted form, because it freezes a topology no structural law admitted.
- Law: the three refusal axes are the source set being EMPTY, a source `Header.Tolerance` differing BITWISE from the coordination tolerance, and a source `Header.Reference` differing STRUCTURALLY from the coordination reference under `GeoReference`'s own value equality; each fault detail names the source tag and both sides' values.
- Law: id collision discriminates by MINTING REGIME, not by payload alone — a rooted OCCURRENCE id (`Node.Object { Kind: Occurrence }`) shared across two sources is ALWAYS `DeltaConflict`, because a Guid-v7 placement identity carries no content preimage and a repeat is corruption; a content-derived or type-derived id repeats legitimately, so equal payloads under `EqualityComparer<Node>.Default` merge as the dedup the id regime exists for and unequal payloads fault naming the id and both source tags.
- Law: an edge JOINS an `Extract` slice only when EVERY id in its `Members` is inside the closure, and the closure is what guarantees it: expansion follows `DirectedPairs` DOWNWARD (whole→part, subject→definition, from→to) and pulls in each reached edge's FULL `Members`, so a buried `PropertyValue.Reference` target and a `Connect`'s realizing intermediary ride in with the edge and no slice can dangle.
- Receipt: `FederationReceipt` is EVIDENCE, never graph content — per source the tag, the snapshot `ContentAddress`, the source header's provenance columns (schema, model view, instant, STEP name), and the node and edge counts; then the union totals and the merged tally the dedup produced, derived from the rows against the union so it cannot disagree with what the graph holds.
- Packages: LanguageExt.Core (`Seq`/`Option`/`Fin`/`Validation` + the tuple `.Apply` join and the `.Traverse` run fold), `Projection/fault#ADMISSION_SLOTS` (`Gate`/`Accumulate`), `Projection/address#CONTENT_ADDRESS` (`ContentAddress.OfGraph` the per-source snapshot key), `Rasm` (the kernel `Op` op-key), BCL inbox (`BitConverter.DoubleToInt64Bits` the bitwise tolerance comparison).
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
    (Gate(sources.Count > 0, key, "<federate-empty-source-set>"),
     Accumulate(sources.Map(source => Aligned(source, coordination, key))),
     Unified(sources, key))
    .Apply(static (_, _, union) => union).As();

    // Tolerance compares BITWISE, not by ==: the coordination grid is what every measure in the union was quantized
    // against, so two doubles that differ in the last bit are two grids, and the ULP-tolerant comparison admits a
    // source whose measure bytes cannot be re-derived. Frame alignment is the upstream reprojection leg's job, so the
    // reference axis REFUSES rather than reconciles, and the detail names both sides' resolution mode.
    static Validation<Error, Unit> Aligned((string Source, ElementGraph Graph) source, Header coordination, Op key) =>
    Accumulate(Seq(
    Gate(BitConverter.DoubleToInt64Bits(source.Graph.Header.Tolerance) == BitConverter.DoubleToInt64Bits(coordination.Tolerance), key,
    $"<federate-tolerance-divergent:{source.Source}:{source.Graph.Header.Tolerance:R}:{coordination.Tolerance:R}>"),
    Gate(source.Graph.Header.Reference.Equals(coordination.Reference), key,
    $"<federate-reference-divergent:{source.Source}:{source.Graph.Header.Reference.Resolution.Key}:{coordination.Reference.Resolution.Key}>")));

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
    ? ElementFault.DeltaConflict(key, $"<federate-node-collision:{id.Value}:{claims[0].Tag}:{rival}>")
    : claims[0].Node;

    static bool Collides(Node held, Node rival) =>
    held is Node.Object { Kind: var kind } && kind == ObjectKind.Occurrence
    || !EqualityComparer<Node>.Default.Equals(held, rival);

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
    ? ElementFault.NodeAbsent(key, $"<extract-root-absent:{absent.Value}>")
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
- [INCIDENCE_INDEX]: incidence keys by every node an edge's `Members` touches — a `Connect`'s realizing intermediary resolves through `EdgesAt`, consistent with `Touches` and the `DropNode` cascade — so `Bake` reads edges in O(degree), built once per snapshot.
- [TOPOLOGY_VIEW]: `Topology()` builds the `QuikGraph` `BidirectionalGraph` over `TaggedEdge<NodeId, Relationship>` legs from each edge's `DirectedPairs`, tagged with the edge itself, so reachability traverses THROUGH a realizing intermediary and a kind-scoped walk reads the tag off the leg through `AlgorithmExtensions`; `TopologyOf` hands that walk a `FilteredBidirectionalGraph` predicate view, and ROOTED ancestry is `ContainmentPath`'s alone because the `Compose` graph is multi-parent.
- [IDENTITY_AND_HASH]: `NodeId` OWNS identity ALONE over one regime with two rooted seedings and the non-rooted content hash — an OCCURRENCE `Object` a Guid-v7 placement id (sortable, kernel-minted), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its volatile-excluded canonical seed (`Node.Object.ToTypeSeedBytes` through `NodeId.RootedType`, the SAME hasher `Content` composes), and a non-rooted node a kernel `XxHash128` content hash over its full `ToCanonicalBytes`; the compressed IFC GlobalId is a Bim-stored projection attribute re-emitted at `Emit`. Deterministic Type ids exclude the volatile `Representations` AND the secondary `Classifications` set (the `WriteObject` projection with `includeVolatile: false` — the PRIMARY `Classification` stays in the seed as entity-class identity) so identical `Component`s dedup to one Type and a later geometry attach or standard-classification stamp never re-keys it, while the FULL `Object` hash (volatile columns INCLUDED) stays byte-for-byte the prior projection so the cross-runtime parity corpus is unperturbed — `ToCanonicalBytes` is the ONE canonical projection the non-rooted id mint and the `Projection/address#CONTENT_ADDRESS` diff share (fixed IEEE-754 LE bits, measures quantized to `Header.Tolerance`, explicit attribute order, id excluded), so a node's content identity is stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed — a float-bearing golden vector (an `IfcMaterialLayer`-shaped node) anchors the cross-runtime parity corpus, and the Type seed is a C#-side mint a peer READS as an opaque rooted id, never re-derives, `Graph/wire#WIRE_CODEC` the proto envelope that carries every id verbatim around the content the keys were minted from. Every `PropertySet`/`QuantitySet`-bearing content key derives from the COUNTED bag layout — `Ordinal(count)` before the sorted rows, the `Projection/address#CANONICAL_WRITER` count-prefix law — the cross-runtime wire law the queued Python/TypeScript canonical-writer mirrors reproduce; an uncounted bag run is the deleted injectivity hole (a trailing run parsing as a prefix of the next raw-append segment).
- [TYPE_INHERITANCE]: `Bake` resolves the named type→occurrence inheritance from the `Relations/relation#EDGE_ALGEBRA` `Assign.TypeDefinition` bind — the `Component` projection (the owner that mints its Type) authors the occurrence→Type edge, and `Bake`'s `TypeResolutionOf` folds the `Component`'s standardized data (the property/quantity bags, the `BakedMaterial` set, the `Assessment` receipts, the type `Object`'s single fields, and its secondary classifications) in ONE pass, then merges occurrence-over-type with explicit per-field precedence: single fields occurrence-overrides-type (`PredefinedType`/`Name`/`Representations`/`Appearance` falling back to the type on the IFC unset sentinel, the primary `Classification` the occurrence's own non-blank code), the materials/assessments/classifications `Seq`s union+dedup-by-key (the `MaterialKey` string; the `(Discipline, Route, InputKey)` assessment cache triple; the `(System, Code, Edition)` classification identity). This is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode`, which stays `PropertyBag`-value precedence (the bag `Merge`) and is never extended by the named dimension. `TypeBinding` surfaces the inherited `Component` as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes (the `Rasm.Bim` type-representation round-trip key), and `MaterialsOf` gains a one-hop type-resolved fallback `CompositionOf`/`PropertiesOf`/`SectionOf` compose (a minor part sharing one `Component`'s profile reads its section with no occurrence-direct association) WITHOUT perturbing the FROZEN Op-free `SectionOf(member)` signature `Rasm.Compute` reads — the fallback is a single type-hop (a `Component` is not itself typed), never a recursive type chain.
- [STRUCTURAL_EQUALITY]: `[Equatable]` owns deep equality for `ElementGraph`, every nested `Node` and `Relationship` CASE, and every drillable intermediate payload — the union roots carry no seat, because a root seat is the compile-proven silent form whose case members reference-compare — so `Inequalities(before, after)` localizes changes below the node map and member-grain drill into a case runs that case's own comparer after discrimination. `MeasureValue` and `PropertyValue` are atomic record-value leaves. Sealed, `ElementGraph` excludes the incidence index, topology, and bake memo from equality and exposes no record copy that aliases caches. Three member policies are DECLARED rather than inherited, because each one agrees with the canonical projection by inheritance alone and a member edit breaks that agreement with no signal. `[StringEquality(StringComparison.Ordinal)]` binds every string the `CanonicalWriter` writes verbatim — a culture-sensitive or case-insensitive comparer rules two nodes equal whose canonical bytes differ, forking equality from content identity at the one place the merge and the id mint must agree. `[UnorderedEquality]` on `Nodes` routes to `DictionaryEqualityComparer<NodeId, Node>` because `FrozenDictionary<TKey,TValue>` implements `IDictionary<TKey,TValue>` — key-matched entry comparison with `EqualityComparer<Node>.Default` on the value side dispatching each case's generated `Equals` override, NOT a `KeyValuePair` multiset, whose element comparison falls to reflective `ValueType.Equals`; the same comparer keys every `Distinct`/`GroupBy`/`HashSet` reuse outside generated code, so a fold deduplicating nodes never spells a second equality. `[PrecisionEquality]` is REFUSED on every float-bearing member here on two structural grounds, not preference: the generator omits precision members from `GetHashCode` ENTIRELY, so a payload distinguished only by tolerance-compared scalars hashes to one bucket across the whole graph; and every double this page carries is either a `MeasureValue` already quantized to `Header.Tolerance` by the canonical projection — a second tolerance beside it forks the one quantization the cross-runtime parity corpus depends on — or an `AppearanceSummary` channel that is PREIMAGE to a frozen content key, where tolerance-equality rules two nodes equal whose `AppearanceKey`s differ and breaks the content-address contract outright.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
