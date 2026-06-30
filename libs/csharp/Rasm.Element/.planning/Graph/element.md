# [ELEMENT_GRAPH]

The authoritative thing: `ElementGraph` = `Header` + `Nodes: FrozenDictionary<NodeId, Node>` + `Edges: ImmutableArray<Relationship>` + a built-once incidence index, and the consumer-facing `Element` is a DERIVED FOLD `Bake(objectNode)` over the reachable subgraph — never a second stored record. This cures the migration source's "typed data stranded off the element": a `Bake`-derived `Element` carries its material, property/quantity bags, assessments, appearance, coverages, and composed parts as flat fields a consumer reads in one hop — "has it all" is one fold, not a join across ten owners. The graph is the property-graph IFC mirror: every IFC entity is a `Node` (`Object`/`Material`/`PropertySet`/`QuantitySet`/`Assessment`/`Appearance`/`Coverage`), every IFC relationship a `Relations/relation#EDGE_ALGEBRA` `Relationship`, and the consumer reads neither — it reads the baked `Element`. The `NodeId` is the ONE identity owner: a rooted node carries a NEUTRAL kernel-minted stable id (a Guid-v7 sortable identity, NOT an IFC GlobalId — the compressed GlobalId is a Bim-stored attribute re-emitted at `Emit`), a non-rooted node (material, property set, representation) carries a kernel `XxHash128` content hash over its canonical bytes. The seam splits the graph by PHASE: the live authoring/delta path is an `ImmutableDictionary` HAMT (O(log n) structural sharing — `Graph/delta#GRAPH_DELTA` owns it), and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` + the incidence index + the memoized `Bake` + the `QuikGraph` view, all built once at the freeze boundary). The page composes every sibling node-payload owner (`Composition/material`, `Properties/property`, `Properties/quantity`, `Assessment/assessment`, `Geospatial/coverage`, `Classification/classification`, `Geospatial/reference`), the `Projection/address#CANONICAL_WRITER` for `ToCanonicalBytes`, `QuikGraph` for the topology view, `Generator.Equals` for the snapshot structural equality and member diff, the kernel `XxHash128` for content identity, and `NodaTime` for the header instant. A missing node or a structural violation rails `Projection/fault#FAULT_BAND` `ElementFault`.

## [01]-[INDEX]

- [01]-[NODE_MODEL]: the `NodeId` `[ValueObject<string>]` (rooted Guid-v7 / non-rooted content hash), the `Node` `[Union]` seven-case property-graph vocabulary, the `ToCanonicalBytes` shared canonical projection, the node-payload component types (`ReleaseVersion`/`ModelView`/`StepHeader`/`OwnerHistory`/`SchemaSpan`/`RepresentationContentHash`/`ObjectKind`/`PredefinedType`/`AppearanceSummary`), and the analytical-geometry decode vocabulary (the seam-owned host-free `Vector3` coordinate, the `AxisCurve`/`FootprintPolygon` shapes a content key decodes to, and the `GeometrySource` resolution port the seam owns the contract for and an app wires over the blob store) — the `Object` referencing its analytical `Axis`/`FootPrint` geometry BY CONTENT KEY through the `RepresentationContentHash` keyed map a `Rasm.Compute` structural/energy runner resolves through a `GeometrySource` from the blob store, never inline coordinate geometry on the seam node.
- [02]-[ELEMENT_GRAPH]: the `Header`, the `ElementGraph` frozen read snapshot with the built-once incidence index and `QuikGraph` topology view, the `Element` derived-fold result, the memoized `Bake` fold applying the `InheritanceMode` type→occurrence precedence wholly within the seam, and the `SectionOf(member)` M7 accessor reading the baked neutral `SectionProperties` off a member's `ProfileSet` composition.

## [02]-[NODE_MODEL]

- Owner: `NodeId` the `[ValueObject<string>]` identity owner over the `IObjectFactory` floor; `Node` the `[Union]` seven-case property-graph vocabulary carrying the shared `ToCanonicalBytes` projection; the node-payload component types the cases compose.
- Cases: `Object` (the IfcObjectDefinition mirror — `ObjectKind` occurrence/type, optional `ExternalId` (the Bim-stored IFC GlobalId, re-emitted at `Emit`), the generic primary `Classification` (the entity-class-keying pair every query/egress/diff reads) PLUS the `Classifications` set of additional standard-system references (IFC permits MULTIPLE `IfcRelAssociatesClassification` per object — Uniclass + OmniClass simultaneously — so the secondary refs ride a `Seq<Classification>` rather than a lossy single field), first-class `PredefinedType` token value-object, name/tag, the `RepresentationContentHash` keyed map content-hashing EVERY geometry — the heavy display `Body` AND the lightweight analytical `Axis` (idealized structural line) and `FootPrint` (space-boundary surface polygon) a discipline resolves by content key, never inline coordinates — optional `OwnerHistory`, schema `SchemaSpan`; NO `GeoReference`) · `Material` (a `Composition/material#MATERIAL_COMPOSITION` `MaterialId` + composition + property sets) · `PropertySet`/`QuantitySet` (a `Properties/property#PROPERTY_BAG` named bag with its `InheritanceMode`) · `Assessment` (an `Assessment/assessment#ASSESSMENT_NODE` receipt) · `Appearance` (a content-keyed `AppearanceSummary`) · `Coverage` (a `Geospatial/coverage#COVERAGE_NODE` raster/field grid); the closed property-graph node family.
- Entry: `NodeId.Rooted()` mints a neutral sortable rooted id (Guid v7); `NodeId.Content(canonicalBytes)` mints a non-rooted content-hash id through the kernel `ContentHash` entry, `NodeId.OfContent(contentAddress)` mints one from a precomputed `ContentAddress` without re-hashing ONLY when that address IS the node's own content self-hash (`ContentAddress.Of(node.ToCanonicalBytes(tolerance))`), never from a foreign key like an `Assessment.InputKey` (which is a payload field the node's own `ToCanonicalBytes` folds, not the node id); `node.Id` reads any case's id through the abstract override; `node.ToCanonicalBytes(tolerance)` projects the case's semantic content (NO id) into the canonical bytes the `NodeId.Content` mint and the `Projection/address#CONTENT_ADDRESS` diff SHARE.
- Auto: each case carries `NodeId Id` as a positional override of the union's abstract `Id`, so `node.Id` reads without a switch; `ToCanonicalBytes` dispatches the generated total `Switch` writing each case's semantic content (an `Object` its kind/classification/predefined/name/tag/representations/span; a `Material` its key/composition/properties; a bag its sorted name→value entries; a measure quantized to the tolerance) into the `Projection/address#CANONICAL_WRITER`, the id excluded so a non-rooted node's id derives from its own bytes without circularity; the rooted `Object` mints its id once at authoring (Guid v7), the IFC GlobalId staying a Bim-stored projection attribute re-emitted at `Emit`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`IObjectFactory`), LanguageExt.Core (`Option`/`Seq`/`Map`), NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-hash entry the `NodeId.Content` mint composes). The neutral `Vector3` the `AxisCurve`/`FootprintPolygon` analytical shapes carry is SEAM-OWNED (the kernel `Rasm.Vectors` coordinate is the host `Vector3d` the seam Boundary forbids; no neutral kernel triple exists), so the seam mints its own host-free coordinate AND its full vector algebra (`Length`/`Distance`/`Dot`/`Cross`/`Unit` + the `UnitX`/`UnitY`/`UnitZ`/`Zero` constants + the `+`/`-`/`*` operators) — the `Rasm.Bim` scan-to-BIM orientation classifier (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) and the `Rasm.Compute` structural load-vector folds compose THIS one coordinate rather than a kernel/host vector, so a phantom kernel `Vector3` or a `System.Numerics.Vector3` crossing the analytical-shape math is the deleted host leak.
- Growth: a new node concept is one `Node` case carrying its payload type (a `Schedule`/`Task` node lands here only if 4D becomes a real target); a new object axis is one column on the `Object` case; a new node-payload component is one type on its owning sibling page; never a parallel node family and never a second identity scheme — the `NodeId` is the one owner, `MaterialId` a node attribute, not a parallel key.
- Boundary: `NodeId` is the ONE identity owner — a rooted node carries a neutral Guid-v7 id (NOT an IFC GlobalId, which is a Bim-stored attribute re-emitted at `Emit`), a non-rooted node a kernel `XxHash128` content hash over `ToCanonicalBytes`, and a second key scheme or a `MaterialId`-as-node-key is the deleted form; the `Object` carries the generic primary `Classification` (the entity-class-keying pair) plus the `Classifications` `Seq<Classification>` of additional standard-system references (the multiple `IfcRelAssociatesClassification` IFC admits — a single field would drop a co-applied Uniclass/OmniClass, the deleted lossy form) and the first-class `PredefinedType` token (validity is a Bim egress gate, not a seam invariant), the `RepresentationContentHash` keyed map (M2: the `Body`/`Axis`/`Box`/`FootPrint` content hashes — EVERY geometry, the heavy display body AND the lightweight analytical axis/footprint the structural/energy disciplines resolve one-hop by content key from the blob store THROUGH the seam `GeometrySource` port — the seam owns the decode CONTRACT (`UInt128` content key → the seam-neutral `AxisCurve`/`FootprintPolygon` `Vector3` shape) and an app wires the IMPLEMENTATION over the object-store byte-stream, so a runner pulls `member.Representations.Axis`/`.FootPrint` through the resolver rather than reading a node coordinate field; the analytical coordinate lives ONLY inside the decoded `AxisCurve`/`FootprintPolygon` (over the seam-owned host-free `Vector3`, never the kernel's RhinoCommon `Vector3d`), so a coordinate field ON the node — an inline `Vector3`/`Point3d`, a host BRep, a RhinoCommon handle, or a stored `AxisCurve`/`FootprintPolygon` — is the named seam violation, the deleted form) and the optional `OwnerHistory` + `SchemaSpan`, but NO `GeoReference` (that rides the `Header` and `Coverage`); `ToCanonicalBytes` is the ONE canonical projection the id mint and the diff share (fixed IEEE-754 LE, tolerance-quantized measures, explicit attribute order), so a node hashes identically across runtimes; the seam carries no IFC entity-class roster (the `IfcClass` vocabulary and the `PredefinedType` valid-set are the Bim projector's), the `Object` carrying the neutral classification and predefined token the projector resolves.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeId {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();

 // A rooted node carries a neutral sortable identity (Guid v7), NOT an IFC GlobalId.
 public static NodeId Rooted() => Create(Guid.CreateVersion7().ToString("N"));

 // A non-rooted node carries the kernel `ContentHash` seed-zero entry over its canonical bytes —
 // the SAME projection the ContentAddress diff shares, so identity is content-stable cross-runtime.
 public static NodeId Content(ReadOnlySpan<byte> canonicalBytes) =>
 Create(ContentHash.Of(canonicalBytes).ToString("X32", System.Globalization.CultureInfo.InvariantCulture));

 // Mint a non-rooted id from a PRECOMPUTED `ContentAddress` WITHOUT re-hashing — formats the UInt128 identically to
 // Content(bytes), so OfContent(addr) == Content(bytes) holds ONLY when addr == ContentAddress.Of(node.ToCanonicalBytes(tolerance)),
 // i.e. the address IS the node's OWN content self-hash. The valid callers carry a precomputed address THAT IS the node
 // self-hash (a node whose canonical bytes were already hashed once and the UInt128 carried forward to avoid a second pass).
 // It is NOT a back-door for a FOREIGN key: an `Assessment.InputKey` (the hash of the assessed INPUTS, not the assessment
 // node's content) is a payload FIELD the node's OWN ToCanonicalBytes folds, NEVER the node id — an Assessment node is
 // minted `NodeId.Content(node.ToCanonicalBytes(tolerance))` (the self-hash the Projection/address#CONTENT_ADDRESS Verify
 // dual recomputes), so OfContent(InputKey) would store an id Verify can never reproduce, the deleted form.
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

// The IFC predefined-type token a first-class typed value on the Object node (C6): the SEAM owns the token
// (Bim retired its copy), VALIDITY is a Bim EGRESS gate — Emit resolves the IfcClass row from the classification
// code and runs AdmitPredefined against the frozen valid set, never a seam invariant. NotDefined is the IFC default.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PredefinedType {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim().ToUpperInvariant();
 public static readonly PredefinedType NotDefined = Create("NOTDEFINED");
 public string Token => Value;
}

// The ISO 10303-21 STEP header carried on the model Header — the FILE_DESCRIPTION/FILE_NAME/FILE_SCHEMA sections
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

// The schema-version span a node is valid across, validated at Emit against Header.ReleaseVersion.
public readonly record struct SchemaSpan(ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn) {
 public static SchemaSpan From(ReleaseVersion introduced) => new(introduced, None);
}

// The geometry reference is a keyed map RepresentationIdentifier → content hash (M2: axis/body/box/footprint),
// neutral-named (no IFC leak), EVERY geometry — heavy display body AND the lightweight analytical Axis (the idealized
// structural-member line) and FootPrint (the space-boundary surface polygon) the structural/energy disciplines read —
// held by content hash in the blob store and resolved one-hop by content key, NEVER inline coordinate geometry on the
// node (the seam carries no host geometry type and no raw Vector3/Point3d coordinate field). Body/Axis/Box/FootPrint
// are the standard IFC RepresentationIdentifier reads a consumer resolves; an absent identifier is None.
public readonly record struct RepresentationContentHash(Map<string, UInt128> ByIdentifier) {
 public static readonly RepresentationContentHash Empty = new(Map<string, UInt128>());
 public Option<UInt128> Body => ByIdentifier.Find("Body");
 public Option<UInt128> Axis => ByIdentifier.Find("Axis");
 public Option<UInt128> Box => ByIdentifier.Find("Box");
 public Option<UInt128> FootPrint => ByIdentifier.Find("FootPrint");
 public RepresentationContentHash With(string identifier, UInt128 hash) => this with { ByIdentifier = ByIdentifier.AddOrUpdate(identifier, hash) };
}

// The SEAM-OWNED host-neutral coordinate the analytical shapes carry: a flat double XYZ, NOT a RhinoCommon Point3d/Vector3d
// (the kernel `Rasm.Vectors` coordinate is the Rhino struct — a HOST type the seam Boundary forbids on a node) and NOT a
// kernel `Vector3` (no neutral kernel triple exists; `using Rasm;` brings in none). The seam is the LOWEST-AEC stratum below
// Bim/Compute, so it OWNS this neutral coordinate the way it owns Dimension/GeoReference — pure data, zero host dependency,
// `double`-domain (a coordinate is the geometry's native scalar, never a unit-bearing MeasureValue). The Rasm.Bim projector
// maps a Rhino/GeometryGym point ONTO this at the boundary, the Rasm.Compute runner reads it back — neither side re-mints a
// parallel coordinate, and a host coordinate type crossing the analytical-shape signature is the deleted §4-RT-M2 leak. This
// is NOT inline coordinate geometry ON a node (the named violation): a Vector3 lives ONLY inside the AxisCurve/FootprintPolygon
// a GeometrySource DECODES from the blob store by content key, never as an Object-node field. The seam carries the WHOLE
// host-neutral coordinate algebra (Length/Distance/Dot/Cross/Unit + the axis constants) because the Rasm.Bim scan-to-BIM
// orientation classifier folds it (`Vector3.Dot(normal.Unit, Vector3.UnitZ)` discriminates a vertical patch from a wall) and
// the Rasm.Compute structural runner derives a load magnitude through `Vector3.Distance(w, default)` — both compose this ONE
// seam coordinate rather than a kernel/host vector (no neutral kernel `Vector3` exists; `using Rasm;` brings in none, the
// kernel `Rasm.Vectors` triple being the RhinoCommon `Vector3d` the seam Boundary forbids), so a `System.Numerics.Vector3`
// or a RhinoCommon `Vector3d` crossing the analytical-shape math is the deleted host leak. Unit DEGENERACY-guards a zero
// vector to Zero rather than dividing by a zero length, so a degenerate fitted axis never produces a NaN orientation.
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

// The lightweight ANALYTICAL geometry the projector content-keys into RepresentationContentHash under "Axis"/"FootPrint"
// [M2] and an above-seam discipline (a Rasm.Compute structural/energy runner) RESOLVES one-hop by that content key from
// the blob store — the seam-neutral Vector3 only, NEVER a host Brep/Mesh/Point3d and NEVER inlined on the Object node.
// AxisCurve is the idealized structural-member line (start/end + a non-degenerate local up); FootprintPolygon the
// space-boundary surface ring. These are the seam-owned shapes the producer's IfcRepresentation.Keys hash and the consumer's
// GeometrySource decode bytes back into — the ONE analytical-geometry vocabulary shared by every projector and every analysis
// runner, so neither side re-declares a parallel MemberAxis/BoundaryPolygon (the deleted parallel shapes).
public readonly record struct AxisCurve(Vector3 Start, Vector3 End, Vector3 Up) {
 public double Length => Vector3.Distance(Start, End);
}

public readonly record struct FootprintPolygon(Seq<Vector3> Ring) {
 public static readonly FootprintPolygon Empty = new(Seq<Vector3>());
 public bool IsEmpty => Ring.IsEmpty;
}

// The geometry-resolution PORT [M2]: the seam owns the CONTRACT (content key -> decoded analytical shape), the app wires
// the IMPLEMENTATION (a decoder over the Rasm.Persistence object-store Fetch byte-stream — the same wiring-is-app-owned law
// the projector registration follows), and an above-seam runner that holds no host geometry threads this resolver to pull
// the analytical axis/footprint by `member.Representations.Axis`/`.FootPrint` rather than reading a phantom node field. Axis
// and footprint decode to GENUINELY DISTINCT shapes (a line `AxisCurve` vs a ring `FootprintPolygon`), so the port carries
// ONE typed decode leg per analytical-geometry KIND — two type-distinct resolvers the matching `Axis`/`Footprint` reads
// compose, NOT a `Get`/`GetById`/`GetByName` arity-proliferation over one concept (the discriminant is the return TYPE the
// blob decodes to, irreducible to a single `Func` without a wasteful wrapper union the Persistence blobstore's two distinct
// `Get(key) -> decode` calls confirm) and never a stored coordinate on the node — a missing/undecodable blob is None, so a
// runner rails its own typed input-missing fault rather than defaulting a coordinate. Empty.None is the inert wiring a
// closed-form route (which reads no geometry) threads, the dual of the Assessment ResultBlob egress sink the spine threads.
public readonly record struct GeometrySource(
 Func<UInt128, Option<AxisCurve>> ResolveAxis, Func<UInt128, Option<FootprintPolygon>> ResolveFootprint) {
 public static readonly GeometrySource None = new(static _ => Option<AxisCurve>.None, static _ => Option<FootprintPolygon>.None);
 public Option<AxisCurve> Axis(RepresentationContentHash representations) => representations.Axis.Bind(ResolveAxis);
 public Option<FootprintPolygon> Footprint(RepresentationContentHash representations) => representations.FootPrint.Bind(ResolveFootprint);
}

// Appearance node summary: a content-keyed reference to the full BSDF (authored in Rasm.Materials) plus the
// neutral canonical PBR scalars a consumer reads flat without the full lobe graph. The SEAM owns the AppearanceKey
// derivation through Of — the kernel seed-zero XxHash128 over the canonical PBR bytes via the Projection/address
// CanonicalWriter -> ContentAddress.Of — so the Rasm.Materials MaterialProjector and the Rasm.Bim AppearanceProjection
// compose ONE factory and mint the SAME key for one surface (a local CanonicalWriter beside this factory in either peer
// is the byte-order divergence defect). Transmissive is the REFRACTIVE flag DISTINCT from Opacity (alpha): an opaque-alpha
// glass still transmits, so a GLB KHR_materials_transmission channel reads it apart from the alpha. Opacity AND Transmissive
// are load-bearing in the KEY even though the Graph/element#NODE_MODEL Node.ToCanonicalBytes appearance arm hashes only
// the AppearanceKey (the key already folds them) — two appearances differing only in alpha or in the refractive flag get
// distinct AppearanceKeys and so distinct Node.Appearance ids.
public readonly record struct AppearanceSummary(
 UInt128 AppearanceKey, double BaseColorR, double BaseColorG, double BaseColorB,
 double Metallic, double Roughness, double Opacity, bool Transmissive) {
 // The ONE seam-owned appearance content-key factory both the Rasm.Materials MaterialWire.Summary lowering and the
 // Rasm.Bim AppearanceProjection.Project lowering compose: write the neutral PBR vector (base R/G/B + metallic +
 // roughness + opacity + transmissive) through the seam CanonicalWriter and mint the AppearanceKey via ContentAddress.Of
 // (the kernel seed-zero XxHash128, the ONE hasher). tolerance 0.0 hashes the raw IEEE bits of the appearance scalars —
 // they are not Header-quantized measures — and the writer canonicalizes -0.0/NaN/inf, so the key is cross-runtime stable.
 public static AppearanceSummary Of(double r, double g, double b, double metallic, double roughness, double opacity, bool transmissive, double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.Double(r).Double(g).Double(b).Double(metallic).Double(roughness).Double(opacity).Bool(transmissive);
  return new AppearanceSummary(ContentAddress.Of(w.ToBytes().Span).Value, r, g, b, metallic, roughness, opacity, transmissive);
 }
}

// --- [MODELS] -----------------------------------------------------------------------------
// A CLASS-root [Union] (the [GRAPH_FAMILY] form), NOT a record-root: a class-root union surrenders
// Thinktecture's record-generated equality, so structural equality AND the member-level structured diff
// ride Generator.Equals [Equatable] (the ONE generated equality aspect for the shapes Thinktecture does not
// own — never stacked on a record-root union). [Equatable] is LOAD-BEARING here, not decorative: the
// ElementGraph [Equatable] map walks its node values, and Generator.Equals DRILLS into a nested value only
// when that value is itself [Equatable] — so a changed Node member surfaces as a Nodes[id].<member> path in
// ElementGraph.EqualityComparer.Default.Inequalities (the member granularity the Rasm.Persistence 3-way
// StructuralMerge keys its RFC 6902 patch egress on) rather than a whole-node delta. A record-root Node would
// be an opaque equality leaf to Generator.Equals — ElementGraph.Inequalities would report Nodes[id] whole-node,
// collapsing the member-granular merge to whole-node replacement, the deleted form. The collection-valued
// members carry [UnorderedEquality]/[OrderedEquality] so the bag/set/sequence semantics nest into the graph
// diff. The drill descends ONE structural-equality link at a time, so the deepest paths the merge keys on
// (Nodes[id].Properties[2].Thickness inside a MaterialComposition.LayerSet, Nodes[id].Bag.Values[name]
// inside a PropertyBag, Nodes[id].Payload.Results[name] inside an AssessmentPayload) require each INTERMEDIATE
// payload owner — MaterialComposition/MaterialPropertySet (class-root [Union]+[Equatable]), MaterialLayer/
// MaterialConstituent/SectionProperties ([Equatable] record-structs) (Composition/material), PropertyBag (the
// [Equatable] ValueBag, Properties/property), AssessmentPayload ([Equatable] record, Assessment/assessment),
// CoverageGrid (Geospatial/coverage) — to carry a STRUCTURAL-equality member set the diff descends THROUGH; the
// drill then BOTTOMS at the atomic value-equality LEAF — a MeasureValue (a native-equality readonly record struct,
// Properties/quantity, compared atomically by record value-equality — Nodes[id].Properties[2].Thickness is the
// leaf, never .Thickness.Si) or a PropertyValue (a RECORD-root [Union] whose Thinktecture record-generated value
// equality is correct, the leaf at Bag.Values[name], an IFC Pset value replaced wholesale not merged into).
// MeasureValue/PropertyValue therefore carry NEITHER [Equatable] NOR a deeper descent (adding [Equatable] to a
// record-struct/record-root union re-derives the same field compare the record already gives — the rejected
// redundant-ceremony form): they are the merge leaves, not drill owners. An owner that is an opaque equality leaf
// where the merge needs descent (a record-root union or a plain record where a member-granular sub-diff is
// required) is the boundary where the drill stops at Nodes[id].<that-member> and the merge's Append falls back to
// the two content-signature pointers. Each Node case is a sealed CLASS exposing NodeId Id as a positional override
// of the union's abstract Id, equality fully [Equatable]-owned.
[Union]
[Equatable]
public abstract partial class Node {
 private Node() { }

 public abstract NodeId Id { get; init; }

 public sealed partial class Object(
 NodeId id, ObjectKind kind, Option<string> externalId, Classification classification, PredefinedType predefinedType,
 string name, string tag, RepresentationContentHash representations,
 Option<OwnerHistory> history, SchemaSpan span, Seq<Classification> classifications = default) : Node {
  public override NodeId Id { get; init; } = id;
  public ObjectKind Kind { get; } = kind;
  public Option<string> ExternalId { get; } = externalId;
  public Classification Classification { get; } = classification;
  public PredefinedType PredefinedType { get; } = predefinedType;
  public string Name { get; } = name;
  public string Tag { get; } = tag;
  public RepresentationContentHash Representations { get; } = representations;
  public Option<OwnerHistory> History { get; } = history;
  public SchemaSpan Span { get; } = span;
  [property: UnorderedEquality] public Seq<Classification> Classifications { get; } = classifications;
 }
 public sealed partial class Material(NodeId id, MaterialId materialKey, MaterialComposition composition, Seq<MaterialPropertySet> properties) : Node {
  public override NodeId Id { get; init; } = id;
  public MaterialId MaterialKey { get; } = materialKey;
  public MaterialComposition Composition { get; } = composition;
  [property: UnorderedEquality] public Seq<MaterialPropertySet> Properties { get; } = properties;
 }
 public sealed partial class PropertySet(NodeId id, PropertyBag bag) : Node { public override NodeId Id { get; init; } = id; public PropertyBag Bag { get; } = bag; }
 public sealed partial class QuantitySet(NodeId id, QuantityBag bag) : Node { public override NodeId Id { get; init; } = id; public QuantityBag Bag { get; } = bag; }
 public sealed partial class Assessment(NodeId id, AssessmentPayload payload) : Node { public override NodeId Id { get; init; } = id; public AssessmentPayload Payload { get; } = payload; }
 public sealed partial class Appearance(NodeId id, AppearanceSummary summary) : Node { public override NodeId Id { get; init; } = id; public AppearanceSummary Summary { get; } = summary; }
 public sealed partial class Coverage(NodeId id, CoverageGrid grid) : Node { public override NodeId Id { get; init; } = id; public CoverageGrid Grid { get; } = grid; }

 // The ONE canonical value codec — the id is EXCLUDED (a non-rooted id derives from these bytes), measures quantize
 // to the tolerance, attribute order is explicit, and the diff + the id mint share it. Each complex payload delegates
 // to its OWNER's CanonicalBytes (Composition/material MaterialComposition + MaterialPropertySet, Properties/property
 // PropertyValue, Assessment/assessment AssessmentPayload, Geospatial/coverage CoverageGrid) so the projection is never
 // re-derived per case; geometry rides the content-hashed Representations map (its content keys ARE the geometry
 // identity), never inline coordinates. PROVENANCE
 // is excluded — OwnerHistory (who/when, H9) is a separate additive axis, not content, so a re-stamp never forks the id;
 // the lazy caches (incidence/QuikGraph/Bake memo) are likewise outside the byte projection.
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
 CanonicalWriter w = new(tolerance);
 Switch(
 @object: o => { w.Ordinal(0); w.String(o.Kind.Key); w.Bool(o.ExternalId.IsSome); o.ExternalId.IfSome(e => w.String(e)); w.String(o.Classification.System); w.String(o.Classification.Code); w.String(o.Classification.Edition); w.Ordinal(o.Classifications.Count); foreach (var c in o.Classifications.OrderBy(static x => x.System, StringComparer.Ordinal).ThenBy(static x => x.Code, StringComparer.Ordinal).ThenBy(static x => x.Edition, StringComparer.Ordinal)) { w.String(c.System); w.String(c.Code); w.String(c.Edition); } w.String(o.PredefinedType.Token); w.String(o.Name); w.String(o.Tag); w.Ordinal(o.Representations.ByIdentifier.Count); foreach (var (k, h) in o.Representations.ByIdentifier.OrderBy(static p => p.Key, StringComparer.Ordinal)) { w.String(k); w.U128(h); } w.String(o.Span.IntroducedIn.Key); w.Bool(o.Span.RemovedIn.IsSome); o.Span.RemovedIn.IfSome(r => w.String(r.Key)); },
 material: m => { w.Ordinal(1); w.String(m.MaterialKey.Value); m.Composition.CanonicalBytes(w); w.Ordinal(m.Properties.Count); foreach (var p in m.Properties.OrderBy(static p => p.Discipline.Key, StringComparer.Ordinal)) { p.CanonicalBytes(w); } },
 propertySet: p => { w.Ordinal(2); w.String(p.Bag.SetName); w.String(p.Bag.Inheritance.Key); foreach (var (n, v) in p.Bag.Values.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); v.CanonicalBytes(w); } },
 quantitySet: q => { w.Ordinal(3); w.String(q.Bag.SetName); w.String(q.Bag.Inheritance.Key); foreach (var (n, m) in q.Bag.Values.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); w.Measure(m); } },
 assessment: a => { w.Ordinal(4); a.Payload.CanonicalBytes(w); },
 appearance: a => { w.Ordinal(5); w.U128(a.Summary.AppearanceKey); },
 coverage: c => { w.Ordinal(6); c.Grid.CanonicalBytes(w); });
 return w.ToBytes();
 }

 // Re-stamp the node's OWN identity to a SPECIFIC new id, the rest of the payload intact — the endpoint-alignment
 // re-stamp a Rasm.Persistence Reconcile takes when it assigns a rooted node a durable id and a Bim re-identify
 // compose, so no consumer hand-rolls a per-case copy. DISTINCT from Remap: Relabel sets the OWN id to `id`
 // (the buried references stay as-is), Remap rewrites EVERY id — own AND buried — by a function (the dual of the
 // edge algebra, where Relationship has only Remap because an objectified edge owns no re-stampable identity, while
 // a Node owns one so it carries BOTH). A class-root [Union] case has NO compiler-generated `with`, so each arm
 // RECONSTRUCTS its case — the func-form generated total Switch (each arm a Func<TCase,Node> invoked ONLY for the
 // matched case), NOT Map: Thinktecture's Map maps cases to PRECOMPUTED constant values without lambdas (it cannot
 // carry an allocating per-case reconstruction and would build all seven arms eagerly), so the func-form Switch is
 // the doctrinal owner of allocating dispatch (the SAME form Relations/relation#EDGE_ALGEBRA Relationship.Remap takes).
 // Exhaustive over the closed seven-case family (an eighth case breaks the build here), the payload carried positionally
 // so a case gaining a field breaks loudly rather than silently dropping it. The id is the ONLY field that changes; for
 // a non-rooted node the caller is responsible for re-minting from the new content (Relabel is the rooted-node/endpoint-alignment rewrite).
 public Node Relabel(NodeId id) => Switch<Node>(
  @object: o => new Object(id, o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.Name, o.Tag, o.Representations, o.History, o.Span, o.Classifications),
  material: m => new Material(id, m.MaterialKey, m.Composition, m.Properties),
  propertySet: p => new PropertySet(id, p.Bag),
  quantitySet: q => new QuantitySet(id, q.Bag),
  assessment: a => new Assessment(id, a.Payload),
  appearance: a => new Appearance(id, a.Summary),
  coverage: c => new Coverage(id, c.Grid));

 // Re-map EVERY node-id the node carries through `map` (an unmapped id passes through unchanged) — the seam-owned
 // node-id rewrite a Rasm.Persistence Version/merge#STRUCTURAL_MERGE Reconcile composes when it rewrites a
 // re-ingested graph's freshly-minted ids onto durable ids, the EXACT dual of Relations/relation#EDGE_ALGEBRA
 // Relationship.Remap: BOTH the node's own Id (map(Id)) AND every graph-node reference BURIED in a payload are
 // rewritten, not the own id alone. The ONLY payloads that bury a graph-node NodeId are the PropertySet/QuantitySet
 // bags through a Properties/property#PROPERTY_VALUE PropertyValue.Reference(NodeId) (the IfcPropertyReferenceValue
 // arm a Reference resolves through the Nodes index) — so the PropertySet arm composes the ONE PropertyValue.Remap(map)
 // owner over each bag value through the bag record `with` over its Values map (the QuantityBag's MeasureValue carries
 // no NodeId, so its bag passes through verbatim) exactly as the Relationship.Remap Generic arm composes PropertyValue
 // .Remap over its Attributes map, closing the SAME stranded-reference defect on the node side that the edge side
 // already closes; an endpoints-only node rewrite that re-stamped only the own id would dangle a bag's Reference a
 // re-ingest remapped (the deleted form). MaterialId is a value-object key NOT a NodeId, and MaterialComposition/
 // MaterialPropertySet/AssessmentPayload/CoverageGrid/AppearanceSummary carry no buried NodeId, so those arms rewrite
 // the own id alone. Same func-form total Switch over the closed seven-case family (a new case breaks the build here,
 // never a silent pass-through that would strand a buried id).
 public Node Remap(Func<NodeId, NodeId> map) => Switch<Node>(
  @object: o => new Object(map(o.Id), o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.Name, o.Tag, o.Representations, o.History, o.Span, o.Classifications),
  material: m => new Material(map(m.Id), m.MaterialKey, m.Composition, m.Properties),
  propertySet: p => new PropertySet(map(p.Id), p.Bag with { Values = p.Bag.Values.Map((_, v) => v.Remap(map)) }),
  quantitySet: q => new QuantitySet(map(q.Id), q.Bag),
  assessment: a => new Assessment(map(a.Id), a.Payload),
  appearance: a => new Appearance(map(a.Id), a.Summary),
  coverage: c => new Coverage(map(c.Id), c.Grid));
}
```

## [03]-[ELEMENT_GRAPH]

- Owner: `Header` the model header (`ReleaseVersion` + `ModelView` + `Geospatial/reference#GEO_REFERENCE` `GeoReference` + `Tolerance` + `Instant` + `StepHeader`) carrying the ONE semantic-header `CanonicalBytes` projection both the `Projection/address#CONTENT_ADDRESS` `OfGraph` snapshot key and the `Graph/delta#GRAPH_DELTA` `GraphDelta.ToCanonicalBytes` header contribution compose (the projection owned once, never re-spelled per call site); `ElementGraph` the frozen read snapshot carrying the nodes, edges, the built-once incidence index, and the memoized `Bake`; `Element` the derived-fold "has it all" result; `MaterialBinding` the material-plus-usage pair `Bake` folds from an `Associate` edge.
- Entry: `ElementGraph.Of(header, nodes, edges)` builds the frozen snapshot — `ToFrozenDictionary` over the nodes, the incidence index grouping every edge by both endpoints, the lazy `QuikGraph` `BidirectionalGraph` topology view, and an empty `Bake` memo; `Genesis(header)` seeds the empty header-only snapshot a model-creating session or a Marten stream rehydrate builds onto, and `Apply(delta, key)` advances a snapshot by a validated `Graph/delta#GRAPH_DELTA` `GraphDelta` (the persistence rehydrate + live-apply entry) `Fin<T>` railing `ElementFault.NodeAbsent` on a corrupt delta whose added edge names an absent endpoint; `Bake(objectId, key)` folds the reachable subgraph from an `Object` node into an `Element`, memoized by `objectId` within the snapshot (a new snapshot from a `Graph/delta#GRAPH_DELTA` carries a fresh memo), `Fin<T>` railing `ElementFault.NodeAbsent` on an absent root and `ElementFault.RelationshipInvalid` on a cyclic `Compose` chain (a `Compose` ancestry set threaded through the fold); `Topology()` reads the cached `QuikGraph` view a reachability/topological-order/LCA consumer composes; the read accessors `ObjectNodes`/`Find`/`Material`/`MaterialsOf`/`CompositionOf`/`PropertiesOf`/`SectionOf` enumerate the object roots and resolve a node (raw or typed by case) and the material/composition/property/section subgraph a member binds — the polymorphic surface a `Rasm.Compute` analysis route reads the concrete graph through, the discipline reads (loads/supports/spaces/areas) composing in Compute from these primitives.
- Auto: `Of` builds the incidence index once at the freeze boundary so `Bake` reads an object's edges in O(degree) rather than scanning every edge; `Bake` resolves the `Object`, folds its incidence edges — `Assign.PropertyDefinition` resolves a `PropertySet`/`QuantitySet` node into the occurrence bag, `Assign.TypeDefinition` resolves the type `Object` and gathers its bags for the `InheritanceMode` merge, `Assign.Assessment` collects the `Assessment` receipt, `Associate` resolves a `Material` node (with its `MaterialUsage`) into a `MaterialBinding`, an `Appearance` node into the summary, or a `Coverage` node into the grid, and `Compose.Aggregate`/`Nest`/`Contain` recurse into child `Object`s as `Parts` (the non-owning `Reference` excluded, and the host-feature `Void`/`Connect` and group/system relationships read through `EdgesAt`/`Topology` rather than baked into a flat field) — then merges each occurrence bag with its type bag via `Properties/property#PROPERTY_BAG` `PropertyBag.Merge` so the type→occurrence precedence applies once wholly within the seam; the `QuikGraph` view and the `Bake` memo are built lazily and excluded from equality so the frozen snapshot stays structurally compared by its nodes and edges, and no `with` copy can exist — `ElementGraph` is a sealed CLASS, not a record, so the compiler emits no copy that could alias those lazily-built caches across snapshots (the misuse is compile-impossible, not a runtime throw) — only `Of`/`Genesis`/`Apply` mint a fresh one.
- Receipt: the `Element` is the one flat record a consumer reads — `element.Properties.Find(name)`, `element.Materials`, `element.Assessments`, `element.Appearance`, `element.Coverages`, `element.Parts` — "has it all" in one `Bake`, never a join across the graph; the `ElementGraph` is the immutable read snapshot Persistence persists and the projectors assemble onto, its `Generator.Equals` structural equality and `Inequalities` member diff feeding the Persistence 3-way `StructuralMerge`; the `QuikGraph` topology view answers reachability/containment/LCA for a consumer without a second graph.
- Packages: `Generator.Equals` (`[Equatable]` snapshot equality + `Inequalities` diff), QuikGraph (`BidirectionalGraph`/`SEdge` topology view + `AlgorithmExtensions`), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin`), System.Collections.Frozen/Immutable, NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key).
- Growth: a new derived element field is one column on `Element` the `Bake` fold populates from an existing edge kind; a new edge semantic the fold reads is one arm in `Bake`; the working/frozen split keeps the live delta path in the HAMT (`Graph/delta`) and the read path in the frozen snapshot, so neither grows the other; never a second stored `Element` record beside the graph.
- Boundary: the `Element` is a DERIVED FOLD, never a stored record — the migration source's parallel `BimElement`/`Materials.Element` records are the deleted form, the one flat read coming from `Bake` over the graph so typed data is never stranded off the element; the graph splits by PHASE — the live authoring/delta path is an `ImmutableDictionary` HAMT (`Graph/delta` owns it for O(log n) structural sharing) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` at the freeze boundary), so a mutable working graph is never confused with a frozen read snapshot; the incidence index and the `QuikGraph` view are built ONCE per snapshot and the `Bake` memo is keyed by object within the snapshot, invalidated only by a new snapshot from a delta, so a re-`Bake` is O(1) and a graph edit is O(log n); the type→occurrence inheritance applies once in `Bake` via the stamped `InheritanceMode`, never per call site; the `Header` carries the `GeoReference` and the `StepHeader`, the `Object` nodes carry the `OwnerHistory` and the `SchemaSpan`, so the model's provenance and schema span ride the graph, not a side channel.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Header(
 ReleaseVersion Schema, ModelView View, GeoReference Reference, double Tolerance, Instant At, StepHeader Step) {
 public static Header Default(Instant at) =>
 new(ReleaseVersion.Ifc4X3Add2, ModelView.Ifc4Reference, GeoReference.Identity, 1e-6, at, StepHeader.Empty);

 // The ONE semantic-header content projection both the Projection/address#CONTENT_ADDRESS OfGraph snapshot key and the
 // Graph/delta#GRAPH_DELTA GraphDelta.ToCanonicalBytes header contribution compose, so a header's bytes are owned ONCE
 // here rather than re-spelled byte-for-byte at each call site (the deleted duplicated projection). The SEMANTIC identity
 // only — schema, model view, tolerance, and the full Geospatial/reference#GEO_REFERENCE GeoReference (Epsg the CRS
 // identity, the resolved name excluded) — the StepHeader/Instant PROVENANCE is EXCLUDED (the graph-altitude mirror of the
 // node-level OwnerHistory exclusion), so a re-export under a new timestamp/author never forks the snapshot identity.
 public void CanonicalBytes(CanonicalWriter w) {
 w.String(Schema.Key).String(View.Key).Double(Tolerance);
 Reference.CanonicalBytes(w);
 }
}

public readonly record struct MaterialBinding(Node.Material Material, MaterialUsage Usage);

[Equatable]
public sealed partial record Element(
 NodeId Id, ObjectKind Kind, Option<string> ExternalId, Classification Classification, PredefinedType PredefinedType, string Name, string Tag,
 RepresentationContentHash Representations,
 [property: UnorderedEquality] Seq<MaterialBinding> Materials,
 [property: UnorderedEquality] Seq<PropertyBag> Properties,
 [property: UnorderedEquality] Seq<QuantityBag> Quantities,
 [property: UnorderedEquality] Seq<AssessmentPayload> Assessments,
 Option<AppearanceSummary> Appearance,
 [property: UnorderedEquality] Seq<CoverageGrid> Coverages,
 [property: OrderedEquality] Seq<Element> Parts,
 Option<OwnerHistory> History,
 [property: UnorderedEquality] Seq<Classification> Classifications = default);

// A sealed CLASS, not a record: equality is owned by Generator.Equals `[Equatable]` (the `[UnorderedEquality]` node map
// + `[OrderedEquality]` edge array), and a class has NO compiler-generated `with` — so the misuse a record would admit
// (a `with` aliasing the lazily-built incidence index, QuikGraph view, and bake memo BY REFERENCE, surfacing a stale
// baked Element from the wrong snapshot) is COMPILE-IMPOSSIBLE rather than a runtime throw. Only Of/Genesis/Apply mint a
// fresh snapshot (each rebuilding the caches with an empty memo); the live mutation path is the `Graph/delta#GRAPH_DELTA`
// WorkingGraph HAMT, never a copy of the frozen snapshot.
[Equatable]
public sealed partial class ElementGraph {
 [property: UnorderedEquality] public FrozenDictionary<NodeId, Node> Nodes { get; }
 [property: OrderedEquality] public ImmutableArray<Relationship> Edges { get; }
 public Header Header { get; }

 [IgnoreEquality] readonly FrozenDictionary<NodeId, ImmutableArray<Relationship>> incidence;
 [IgnoreEquality] readonly System.Collections.Concurrent.ConcurrentDictionary<NodeId, Element> bakeMemo = new();
 [IgnoreEquality] readonly Lazy<QuikGraph.BidirectionalGraph<NodeId, QuikGraph.SEdge<NodeId>>> topology;

 ElementGraph(Header header, FrozenDictionary<NodeId, Node> nodes, ImmutableArray<Relationship> edges) {
 (Header, Nodes, Edges) = (header, nodes, edges);
 // Index every NODE an edge touches (Relationship.Members), not just the binary endpoints, so a Connect's realizing
 // intermediary resolves through EdgesAt — EdgesAt(n) == "every edge touching n", aligned with Touches and the
 // DropNode cascade; an endpoints-only index would strand a realizing reference the cascade still sweeps.
 incidence = edges
 .SelectMany(e => e.Members.Map(m => (Node: m, Edge: e)))
 .GroupBy(static p => p.Node, static p => p.Edge)
 .ToFrozenDictionary(static g => g.Key, static g => g.ToImmutableArray());
 // Build the view from the directed adjacency each edge contributes (Relationship.DirectedPairs) — a binary edge is
 // one leg, a Connect carrying a realizing intermediary is the two legs From->Realizing->To — so reachability/LCA
 // traverse THROUGH the realizing node, never an endpoints-only From->To shortcut that hides it.
 topology = new(() => {
 var g = new QuikGraph.BidirectionalGraph<NodeId, QuikGraph.SEdge<NodeId>>(allowParallelEdges: true);
 foreach (var edge in edges) { foreach (var (from, to) in edge.DirectedPairs) { g.AddVerticesAndEdge(new QuikGraph.SEdge<NodeId>(from, to)); } }
 return g;
 });
 }

 public static ElementGraph Of(Header header, FrozenDictionary<NodeId, Node> nodes, ImmutableArray<Relationship> edges) => new(header, nodes, edges);

 // The empty header-only snapshot a model-creating session or a Marten stream rehydrate seeds from — the graph
 // the first GraphDelta (carrying its own Header) and the projector Assemble fold build onto, never a null seed.
 public static ElementGraph Genesis(Header header) => Of(header, FrozenDictionary<NodeId, Node>.Empty, []);

 // Advance a snapshot by a validated GraphDelta — the persistence rehydrate + live-apply entry a consumer takes
 // (the Marten inline projection folds the delta stream through it). `Graph/delta#GRAPH_DELTA` `ReplayOnto` re-applies
 // the already-validated delta raw under the delta's own Header when it carries one; Apply additionally guards that
 // every added edge's endpoints resolve in the result, railing ElementFault.NodeAbsent so a corrupt stored delta never
 // freezes a dangling graph (the structural endpoint-kind law ran when the delta was produced at WorkingGraph.Apply).
 public Fin<ElementGraph> Apply(GraphDelta delta, Op key) {
 ElementGraph next = delta.ReplayOnto(this);
 return delta.AddedEdges
 .Find(e => !next.Nodes.ContainsKey(e.Endpoints.Relating) || !next.Nodes.ContainsKey(e.Endpoints.Related))
 .Match(
 Some: e => ElementFault.NodeAbsent(key, $"<replay-edge-endpoint-absent:{e.Endpoints.Relating.Value}->{e.Endpoints.Related.Value}>"),
 None: () => Fin.Succ(next));
 }

 // The object (element-root) nodes a consumer iterates to bake or index every element — the typed projection over
 // the node map a Rasm.Persistence Query/index pass folds, never a per-element re-scan of the whole node set.
 public Seq<Node.Object> ObjectNodes => toSeq(Nodes.Values).Choose(static n => n is Node.Object o ? Some(o) : None);

 public ImmutableArray<Relationship> EdgesAt(NodeId node) => incidence.GetValueOrDefault(node, []);

 public QuikGraph.BidirectionalGraph<NodeId, QuikGraph.SEdge<NodeId>> Topology() => topology.Value;

 // --- [READ_ACCESSORS] -----------------------------------------------------------------
 // The polymorphic read surface a Rasm.Compute analysis route reads the concrete graph through — resolve a node
 // (raw or typed by case), and the material/composition/property/section subgraph a member binds. Compute composes its
 // discipline reads (loads/supports off the structural Connect/Generic edges, spaces/bounding-surfaces off the
 // space-boundary Generic edges, the analytical axis/footprint geometry resolved BY CONTENT KEY from member.Representations
 // .Axis/.FootPrint, areas off the quantity bags) from these primitives + EdgesAt/Topology/Bake — the Bim projector bakes
 // that structural/energy subgraph at ingest; the seam owns the material+section reads (it owns those nodes), the
 // discipline physics lives in Compute, never here.
 public Option<Node> Find(NodeId id) => Nodes.TryGetValue(id, out Node? n) ? Some(n) : None;

 public Option<T> Find<T>(NodeId id) where T : Node => Find(id).Bind(static n => n is T t ? Some(t) : None);

 public Option<Node.Material> Material(NodeId id) => Find<Node.Material>(id);

 public Option<Node.Material> Material(MaterialId key) =>
 toSeq(Nodes.Values).Choose(n => n is Node.Material m && m.MaterialKey == key ? Some(m) : None).Head;

 public Seq<Node.Material> MaterialsOf(NodeId member) =>
 toSeq(EdgesAt(member)).Choose(e => e is Relationship.Associate r && r.Subject == member && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m ? Some(m) : None);

 public Option<MaterialComposition> CompositionOf(NodeId member) => MaterialsOf(member).Head.Map(static m => m.Composition);

 // The FULL typed engineering-property profile a member's associated materials carry — the polymorphic property read a
 // Rasm.Compute discipline route composes the Composition/material#MATERIAL_PROPERTY MaterialPropertyAccess accessors over
 // (graph.PropertiesOf(member).Mechanical / .Thermal / .ForDiscipline(Discipline.Fire)); a per-discipline MechanicalOf/
 // ThermalOf/AcousticOf accessor family — a naive 1-of-6 slice re-deriving the owner's `is`-cast — is the deleted form.
 public Seq<MaterialPropertySet> PropertiesOf(NodeId member) =>
 MaterialsOf(member).Bind(static m => m.Properties);

 // M7: the neutral section the Rasm.Materials projector baked onto a member's ProfileSet composition (WithSection),
 // read Op-FREE off the member's directly-associated material — NO Bake, NO Op key — so a Rasm.Compute structural/fire
 // runner (which holds no Op) reads graph.SectionOf(member) DIRECTLY off the seam rather than re-deriving the ProfileSet
 // traversal in a discipline-local accessor or admitting VividOrange; the seam owns the section read (it owns the nodes).
 // A section is occurrence-direct (the ProfileRef is associated, not type-inherited), so no Bake-fold is needed.
 public Option<SectionProperties> SectionOf(NodeId member) =>
 MaterialsOf(member).Choose(static m => m.Composition is MaterialComposition.ProfileSet { Section: var s } ? s : Option<SectionProperties>.None).Head;

 // --- [BAKE] ---------------------------------------------------------------------------
 // The one derived fold: an Object node plus its reachable subgraph become a flat Element. The public entry seeds an
 // EMPTY Compose ancestry; the private overload threads it so a cyclic Compose chain (a corrupt delta replay or a
 // self-aggregating Object) rails ElementFault.RelationshipInvalid instead of recursing unbounded — the check precedes
 // the memo, so an in-progress (not-yet-memoized) ancestor is caught while a shared DAG child still memo-hits.
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
 // ONE fold over the root's incidence list resolves EVERY edge the element baks flat — the Assign property/quantity/
 // assessment bags AND all three Associate resource kinds (Material+usage, Appearance, Coverage) — so EdgesAt(root) is
 // walked ONCE, never a Material/Appearance fold here plus a third CoveragesOf re-scan of the same Associate dispatch
 // (the deleted COLLAPSE_SCAN repeated-arm + split-concern form). The Associate resource kinds are the LegalLink
 // Material/Appearance/Coverage closure (Graph/delta#GRAPH_DELTA), so the three arms here mirror that legality exactly.
 var (occProps, occQty, materials, assessments, appearance, coverages) =
 toSeq(EdgesAt(root.Id)).Fold(
 (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>(), Mat: Seq<MaterialBinding>(), Asm: Seq<AssessmentPayload>(), App: Option<AppearanceSummary>.None, Cov: Seq<CoverageGrid>()),
 (acc, edge) => edge switch {
 Relationship.Assign a when a.Subject == root.Id && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) },
 Relationship.Assign a when a.Subject == root.Id && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) },
 Relationship.Assign a when a.Subject == root.Id && a.SubKind == AssignKind.Assessment && Nodes.TryGetValue(a.Definition, out var d) && d is Node.Assessment asm => acc with { Asm = acc.Asm.Add(asm.Payload) },
 Relationship.Associate r when r.Subject == root.Id && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m => acc with { Mat = acc.Mat.Add(new MaterialBinding(m, r.Usage)) },
 Relationship.Associate r when r.Subject == root.Id && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Appearance ap => acc with { App = ap.Summary },
 Relationship.Associate r when r.Subject == root.Id && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Coverage c => acc with { Cov = acc.Cov.Add(c.Grid) },
 _ => acc,
 });
 var typeBags = TypeBagsOf(root.Id);
 var properties = MergeBagSets(typeBags.Props, occProps);
 var quantities = MergeQuantityBagSets(typeBags.Qty, occQty);
 return BakeParts(root.Id, key, ancestry).Map(parts => new Element(
 root.Id, root.Kind, root.ExternalId, root.Classification, root.PredefinedType, root.Name, root.Tag, root.Representations,
 materials, properties, quantities, assessments, appearance,
 coverages, parts, root.History, root.Classifications));
 }

 // Gather the type object's bags (via the TypeDefinition edge) so the occurrence merge applies precedence.
 (Seq<PropertyBag> Props, Seq<QuantityBag> Qty) TypeBagsOf(NodeId occurrence) =>
 toSeq(EdgesAt(occurrence)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == occurrence ? Some(a.Definition) : None)
 .Head
 .Match(
 Some: typeId => toSeq(EdgesAt(typeId)).Fold(
 (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>()),
 (acc, e) => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == typeId && Nodes.TryGetValue(a.Definition, out var d)
 ? d switch { Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) }, Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) }, _ => acc }
 : acc),
 None: () => (Seq<PropertyBag>(), Seq<QuantityBag>()));

 // Set-union by SetName: each occurrence bag merges with its matching type bag (precedence via
 // PropertyBag.Merge), and a type-only bag with no occurrence counterpart is inherited as-is.
 static Seq<PropertyBag> MergeBagSets(Seq<PropertyBag> type, Seq<PropertyBag> occurrence) =>
 occurrence.Map(occ => type.Find(t => t.SetName == occ.SetName).Match(Some: t => PropertyBag.Merge(t, occ), None: () => occ))
 + type.Filter(t => !occurrence.Exists(o => o.SetName == t.SetName));

 static Seq<QuantityBag> MergeQuantityBagSets(Seq<QuantityBag> type, Seq<QuantityBag> occurrence) =>
 occurrence.Map(occ => type.Find(t => t.SetName == occ.SetName).Match(Some: t => QuantityBag.Merge(t, occ), None: () => occ))
 + type.Filter(t => !occurrence.Exists(o => o.SetName == t.SetName));

 // The OWNING Compose children only — Aggregate (decomposition), Nest (ordered child sequence), and Contain (spatial
 // containment) recurse into Parts; the non-owning Reference flavor (IfcRelReferencedInSpatialStructure — an element
 // referenced in an additional spatial structure it is NOT contained by) is EXCLUDED, so a column contained in storey A
 // and referenced in storey B bakes as a Part of A alone, never duplicated onto B. Baking every Compose flavor is the
 // deleted form, contradicting the Bake prose (Aggregate/Nest/Contain are the parts) and double-counting referenced elements.
 Fin<Seq<Element>> BakeParts(NodeId whole, Op key, ImmutableHashSet<NodeId> ancestry) =>
 toSeq(EdgesAt(whole))
 .Choose(e => e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference && Nodes.ContainsKey(c.Part) ? Some(c.Part) : None)
 .TraverseM(part => Bake(part, key, ancestry)).As().Map(static parts => parts.ToSeq());
}
```

## [04]-[RESEARCH]

- [DERIVED_ELEMENT]: the consumer-facing `Element` is a `Bake` fold over the reachable subgraph, never a second stored record — this cures the migration source's stranded-data defect (the `Rasm.Bim` `BimElement` and `Rasm.Materials` `Element` were parallel records ID-referencing their property/material data, never joined); the fold reads the incidence edges, resolves the typed node payloads, applies the type→occurrence inheritance once, and recurses the OWNING `Compose` children (`Aggregate`/`Nest`/`Contain`, never the non-owning `Reference`), so "has it all" is one flat read and a graph edit re-bakes in O(1) against the per-snapshot memo.
- [GRAPH_PHASE_SPLIT]: the graph splits by phase — the live authoring/delta path is an `ImmutableDictionary` HAMT (`Graph/delta#GRAPH_DELTA` owns it for O(log n) structural sharing across edits) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` + the incidence index + the `QuikGraph` view + the `Bake` memo, all built once at the freeze boundary) — so the working graph is never confused with the read snapshot, and the freeze boundary is where the analytical structures materialize; the incidence index (keyed by every node an edge's `Members` touches — so a `Connect`'s realizing intermediary resolves through `EdgesAt`, consistent with `Touches` and the `DropNode` cascade) gives `Bake` O(degree) edge access, the `QuikGraph` `BidirectionalGraph` (built from each edge's `DirectedPairs`, so reachability traverses THROUGH a realizing intermediary rather than an endpoints-only shortcut) answers global reachability/topological-order/LCA a consumer composes through `AlgorithmExtensions`, and both are built once per snapshot.
- [IDENTITY_AND_HASH]: the rooted `NodeId` is a neutral Guid-v7 (sortable, kernel-minted) and the non-rooted `NodeId` a kernel `XxHash128` content hash over `ToCanonicalBytes`, the compressed IFC GlobalId a Bim-stored projection attribute re-emitted at `Emit`; `ToCanonicalBytes` is the ONE canonical projection the id mint and the `Projection/address#CONTENT_ADDRESS` diff share (fixed IEEE-754 LE bits, measures quantized to `Header.Tolerance`, explicit attribute order, id excluded), so a node's content identity is stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed — a float-bearing golden vector (an `IfcMaterialLayer`-shaped node) anchors the cross-runtime parity corpus.
- [STRUCTURAL_EQUALITY]: `Generator.Equals` `[Equatable]` gives `ElementGraph`, the `Node` `[Union]`, and the `Relationship` `[Union]` deep structural equality (the `[UnorderedEquality]` node map, the `[OrderedEquality]` edge array, the per-case collection attributes) — all three are class-root `[Union]`/record owners so equality is `[Equatable]`-generated, never the record-root equality Thinktecture would otherwise own (the `[GRAPH_FAMILY]` law), and never stacked on a record-root union. The `Inequalities(before, after)` member-level diff feeds the `Rasm.Persistence` `StructuralMerge` 3-way reconcile: because `Node` and `Relationship` are `[Equatable]`, the diff drills PAST the node map into each changed node's members, localizing a change to `Nodes[id].<member>` so the merge operates at member granularity, never whole-node replacement. The drill descends one structural-equality link per hop, so the deepest paths (`Nodes[id].Properties[2].Thickness` through a `MaterialComposition.LayerSet`, `Nodes[id].Bag.Values[name]` through a `PropertyBag`, `Nodes[id].Payload.Results[name]` through an `AssessmentPayload`) require the INTERMEDIATE payload owners — `MaterialComposition`/`MaterialPropertySet` (class-root `[Union]`+`[Equatable]`), `MaterialLayer`/`MaterialConstituent`/`SectionProperties` (`[Equatable]` record-structs), `PropertyBag` (the `[Equatable]` `ValueBag`), `AssessmentPayload` (`[Equatable]` record), `CoverageGrid` — to carry a `[Equatable]` structural member set the diff descends THROUGH, and the drill then BOTTOMS at the atomic value-equality LEAF: a `MeasureValue` is a native-equality `readonly record struct` (`Properties/quantity#MEASURE_VALUE`) the `Generator.Equals` `DefaultEqualityComparer` compares atomically by its record value-equality — `Nodes[id].Properties[2].Thickness` is the leaf, never `.Thickness.Si` — and a `PropertyValue` is a RECORD-root `[Union]` whose Thinktecture record-generated value equality is correct, the leaf at `Nodes[id].Bag.Values[name]` (an IFC Pset value the merge replaces wholesale, never sub-merges into a `Bounded` bound). `MeasureValue`/`PropertyValue` carry NEITHER `[Equatable]` (it would redundantly re-derive the field compare the record already gives) NOR a deeper descent — they ARE the merge leaves; an owner that is an opaque equality leaf where the merge needs member-granular descent is the boundary where the drill stops at `Nodes[id].<that-member>` and the merge's `Append` falls back to the two content-signature pointers. `ElementGraph` is a sealed CLASS (its equality is `[Equatable]`-owned, NOT record-derived), so it has no compiler-generated `with` to alias the lazily-built caches — the snapshot is frozen by construction, and the `Bake` memo, the incidence index, and the `QuikGraph` view are unattributed fields excluded from `[Equatable]` so two snapshots compare by their nodes and edges, not their lazily-built analytical caches.
