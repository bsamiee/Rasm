# [ELEMENT_GRAPH]

The authoritative thing: `ElementGraph` = `Header` + `Nodes: FrozenDictionary<NodeId, Node>` + `Edges: ImmutableArray<Relationship>` + a built-once incidence index, and the consumer-facing `Element` is a DERIVED FOLD `Bake(objectNode)` over the reachable subgraph — never a second stored record. This cures the migration source's "typed data stranded off the element": a `Bake`-derived `Element` carries its material, property/quantity bags, assessments, appearance, coverages, composed parts, and the inherited `Component` (`Element.Type`/`Element.TypeId`) as flat fields a consumer reads in one hop — "has it all" is one fold, not a join across ten owners. The graph is the property-graph IFC mirror: every IFC entity is a `Node` (`Object`/`Material`/`PropertySet`/`QuantitySet`/`Assessment`/`Appearance`/`Coverage`), every IFC relationship a `Relations/relation#EDGE_ALGEBRA` `Relationship`, and the consumer reads neither — it reads the baked `Element`. The `NodeId` is the ONE identity owner over one regime with two rooted seedings: an OCCURRENCE `Object` carries a Guid-v7 placement identity (NOT an IFC GlobalId — the compressed GlobalId is a Bim-stored attribute re-emitted at `Emit`), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its `Representations`-excluded canonical seed (so identical `Component`s dedup to one Type and IFC round-trip is stable, a later geometry attach never re-keying it), and a non-rooted node (material, property set, representation) a kernel `XxHash128` content hash over its full canonical bytes — never a second identity scheme. The seam splits the graph by PHASE: the live authoring/delta path is an `ImmutableDictionary` HAMT (O(log n) structural sharing — `Graph/delta#GRAPH_DELTA` owns it), and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` + the incidence index + the memoized `Bake` + the `QuikGraph` view, all built once at the freeze boundary). The page composes every sibling node-payload owner (`Composition/material`, `Properties/property`, `Properties/quantity`, `Assessment/assessment`, `Geospatial/coverage`, `Classification/classification`, `Geospatial/reference`), the `Projection/address#CANONICAL_WRITER` for `ToCanonicalBytes`, `QuikGraph` for the topology view, `Generator.Equals` for the snapshot structural equality and member diff, the kernel `XxHash128` for content identity, and `NodaTime` for the header instant. A missing node or a structural violation rails `Projection/fault#FAULT_BAND` `ElementFault`.

## [01]-[INDEX]

- [01]-[NODE_MODEL]: the `NodeId` `[ValueObject<string>]` (the one regime with two rooted seedings — Guid-v7 `Rooted()` occurrence / deterministic `RootedType(typeSeed)` Type — plus the non-rooted `Content` hash), the `Node` `[Union]` seven-case property-graph vocabulary, the `ToCanonicalBytes` shared canonical projection (and the `Object`'s `Representations`-excluded `ToTypeSeedBytes` the Type mint hashes), the node-payload component types (`ReleaseVersion`/`ModelView`/`StepHeader`/`OwnerHistory`/`SchemaSpan`/`RepresentationContentHash`/`ObjectKind`/`PredefinedType`/`AppearanceSummary`), and the analytical-geometry decode vocabulary (the seam-owned host-free `Vector3` coordinate, the `AxisCurve`/`FootprintPolygon` shapes, and the `GeometrySource` resolution port) — the `Object` references EVERY geometry BY CONTENT KEY through the `RepresentationContentHash` keyed map, never inline coordinate geometry on the seam node.
- [02]-[ELEMENT_GRAPH]: the `Header`, the `ElementGraph` frozen read snapshot with the built-once incidence index and `QuikGraph` topology view, the `Element` derived-fold result, the memoized `Bake` fold applying the NAMED type→occurrence inheritance wholly within the seam (single fields occurrence-overrides-type, the `BakedMaterial`/`AssessmentPayload`/`Classification` `Seq`s union+dedup-by-key, DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence), the `TypeBinding`/`Element.TypeId` recovery of the inherited `Component`, and the `SectionOf(member)`/`MaterialsOf` M7 accessors reading the baked neutral `SectionProperties` off a member's `ProfileSet` composition with a one-hop type-resolved fallback.

## [02]-[NODE_MODEL]

- Owner: `NodeId` the `[ValueObject<string>]` identity owner over the `IObjectFactory` floor; `Node` the `[Union]` seven-case property-graph vocabulary carrying the shared `ToCanonicalBytes` projection; the node-payload component types the cases compose.
- Cases: `Object` (the IfcObjectDefinition mirror — `ObjectKind` occurrence/type, optional `ExternalId` (the Bim-stored IFC GlobalId, re-emitted at `Emit`), the generic primary `Classification` (the entity-class-keying pair every query/egress/diff reads) PLUS the `Classifications` set of additional standard-system references (IFC permits MULTIPLE `IfcRelAssociatesClassification` per object — Uniclass + OmniClass simultaneously — so the secondary refs ride a `Seq<Classification>` rather than a lossy single field), first-class `PredefinedType` token value-object, name/tag, the `RepresentationContentHash` keyed map content-hashing EVERY geometry — the heavy display `Body` AND the lightweight analytical `Axis` (idealized structural line) and `FootPrint` (space-boundary surface polygon) a discipline resolves by content key, never inline coordinates — optional `OwnerHistory`, schema `SchemaSpan`; NO `GeoReference`) · `Material` (a `Composition/material#MATERIAL_COMPOSITION` `MaterialId` + composition + property sets) · `PropertySet`/`QuantitySet` (a `Properties/property#PROPERTY_BAG` named bag with its `InheritanceMode`) · `Assessment` (an `Assessment/assessment#ASSESSMENT_NODE` receipt) · `Appearance` (a content-keyed `AppearanceSummary`) · `Coverage` (a `Geospatial/coverage#COVERAGE_NODE` raster/field grid); the closed property-graph node family.
- Entry: `NodeId.Rooted()` mints a sortable placement rooted id (Guid v7) for an OCCURRENCE `Object`; `NodeId.RootedType(typeSeed)` mints the deterministic-rooted Type id from a `Component`'s `Representations`-excluded canonical seed (`Node.Object.ToTypeSeedBytes`) through the SAME kernel `ContentHash` `Content` composes, so identical `Component`s dedup to one Type; `NodeId.Content(canonicalBytes)` mints a non-rooted content-hash id through the kernel `ContentHash` entry, `NodeId.OfContent(contentAddress)` mints one from a precomputed `ContentAddress` without re-hashing ONLY when that address IS the node's own content self-hash (`ContentAddress.Of(node.ToCanonicalBytes(tolerance))`), never from a foreign key like an `Assessment.InputKey` (which is a payload field the node's own `ToCanonicalBytes` folds, not the node id); `node.Id` reads any case's id through the abstract override; `node.ToCanonicalBytes(tolerance)` projects the case's semantic content (NO id) into the canonical bytes the `NodeId.Content` mint and the `Projection/address#CONTENT_ADDRESS` diff SHARE.
- Auto: each case carries `NodeId Id` as a positional override of the union's abstract `Id`, so `node.Id` reads without a switch; `ToCanonicalBytes` dispatches the generated total `Switch` writing each case's semantic content (an `Object` its kind/classification/predefined/name/tag/representations/span; a `Material` its key/composition/properties; a bag its set name, inheritance key, and count-prefixed sorted name→value entries; a measure quantized to the tolerance) into the `Projection/address#CANONICAL_WRITER`, the id excluded so a non-rooted node's id derives from its own bytes without circularity; a rooted `Object` mints its id once at authoring — an OCCURRENCE its Guid-v7 placement identity, a TYPE its DETERMINISTIC `NodeId.RootedType` over `ToTypeSeedBytes` (the `WriteObject` projection with `includeRepresentations: false`, the volatile `Representations` excluded so a later geometry attach never re-keys the Type and identical `Component`s dedup to one Type) — the IFC GlobalId staying a Bim-stored projection attribute re-emitted at `Emit`.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`IObjectFactory`), LanguageExt.Core (`Option`/`Seq`/`Map`), NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-hash entry the `NodeId.Content` mint composes). The neutral `Vector3` the `AxisCurve`/`FootprintPolygon` analytical shapes carry is SEAM-OWNED (the kernel `Rasm.Vectors` coordinate is the host `Vector3d` the seam Boundary forbids; no neutral kernel triple exists), so the seam mints its own host-free coordinate AND its full vector algebra (`Length`/`Distance`/`Dot`/`Cross`/`Unit` + the `UnitX`/`UnitY`/`UnitZ`/`Zero` constants + the `+`/`-`/`*` operators) — the `Rasm.Bim` scan-to-BIM orientation classifier (`Vector3.Dot(normal.Unit, Vector3.UnitZ)`) and the `Rasm.Compute` structural load-vector folds compose THIS one coordinate rather than a kernel/host vector, so a phantom kernel `Vector3` or a `System.Numerics.Vector3` crossing the analytical-shape math is the deleted host leak.
- Growth: a new node concept is one `Node` case carrying its payload type (a `Schedule`/`Task` node lands here only if 4D becomes a real target); a new object axis is one column on the `Object` case; a new node-payload component is one type on its owning sibling page; never a parallel node family and never a second identity scheme — the `NodeId` is the one owner, `MaterialId` a node attribute, not a parallel key. NEXT-CAMPAIGN COLUMN ADD (decided; `NodeWire` frozen this campaign, lands with the wire unfreeze beside the `MaterialWire` columns): `Option<string> ObjectType` beside `ExternalId` on the `Object` case — the IFC-canonical `(PredefinedType=USERDEFINED, ObjectType=label)` occurrence label the Bim `Projection/semantic` `Predefined` ingress and `Projection/egress` `StampPredefined` compose for the exact round-trip (today the egress re-derives the label from `Name`, the named bounded drop; TYPES already preserve theirs through the signature bag), presence-delimited in `ToCanonicalBytes` under the injectivity law.
- Boundary: `NodeId` is the ONE identity owner over one regime with two rooted seedings — an OCCURRENCE `Object` a Guid-v7 placement id (NOT an IFC GlobalId, which is a Bim-stored attribute re-emitted at `Emit`), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its `Representations`-excluded `ToTypeSeedBytes` seed (so identical `Component`s dedup to one Type), a non-rooted node a kernel `XxHash128` content hash over its full `ToCanonicalBytes`, and a second identity owner or a `MaterialId`-as-node-key is the deleted form; the `Object` carries the generic primary `Classification` (the entity-class-keying pair) plus the `Classifications` `Seq<Classification>` of additional standard-system references (the multiple `IfcRelAssociatesClassification` IFC admits — a single field would drop a co-applied Uniclass/OmniClass, the deleted lossy form) and the first-class `PredefinedType` token (validity is a Bim egress gate, not a seam invariant), the `RepresentationContentHash` keyed map (M2: the `Body`/`Axis`/`Box`/`FootPrint` content hashes — EVERY geometry rides the blob store by content key and resolves one-hop THROUGH the seam `GeometrySource` port, the seam owning the decode CONTRACT and the app wiring the implementation, so a coordinate field ON the node — an inline `Vector3`/`Point3d`, a host BRep, a RhinoCommon handle, or a stored `AxisCurve`/`FootprintPolygon` — is the named seam violation) and the optional `OwnerHistory` + `SchemaSpan`, but NO `GeoReference` (that rides the `Header` and `Coverage`); `ToCanonicalBytes` is the ONE canonical projection the id mint and the diff share (fixed IEEE-754 LE, tolerance-quantized measures, explicit attribute order), so a node hashes identically across runtimes; the seam carries no IFC entity-class roster (the `IfcClass` vocabulary and the `PredefinedType` valid-set are the Bim projector's), the `Object` carrying the neutral classification and predefined token the projector resolves.

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

 // A rooted node carries a sortable placement identity (Guid v7) for an OCCURRENCE Object — its identity IS its unique
 // placement, NOT an IFC GlobalId (the compressed GlobalId is a Bim-stored attribute re-emitted at Emit). A TYPE Object
 // is rooted too, but DETERMINISTICALLY (RootedType), so identical Components dedup to one Type — one regime, two seedings.
 public static NodeId Rooted() => Create(Guid.CreateVersion7().ToString("N"));

 // The deterministic-rooted mint for a TYPE Object: the kernel `ContentHash` seed-zero XxHash128 (the ONE hasher
 // Content composes) over the Representations-EXCLUDED canonical seed (Node.Object.ToTypeSeedBytes), so identical
 // Components mint ONE Type id and a later geometry attach never re-keys it — the SAME regime as Rooted with a
 // content-derived seed in place of the random placement Guid. The Component projection composes RootedType; a model
 // author composes Rooted for an Occurrence.
 public static NodeId RootedType(ReadOnlySpan<byte> typeSeed) =>
 Create(ContentHash.Of(typeSeed).ToString("X32", System.Globalization.CultureInfo.InvariantCulture));

 // A non-rooted node carries the kernel `ContentHash` seed-zero entry over its canonical bytes —
 // the SAME projection the ContentAddress diff shares, so identity is content-stable cross-runtime.
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

// The geometry reference: a keyed map RepresentationIdentifier → content hash (M2), neutral-named (no IFC leak).
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

// The SEAM-OWNED host-neutral coordinate the analytical shapes carry: flat double XYZ, `double`-domain (a coordinate is
// the geometry's native scalar, never a unit-bearing MeasureValue). No neutral kernel triple exists and the kernel
// `Rasm.Vectors` coordinate IS the RhinoCommon `Vector3d` the seam Boundary forbids, so the seam mints this one
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

// The lightweight ANALYTICAL geometry content-keyed into RepresentationContentHash under "Axis"/"FootPrint" [M2]:
// AxisCurve the idealized structural-member line (start/end + a non-degenerate local up), FootprintPolygon the
// space-boundary surface ring — the ONE analytical vocabulary every projector hashes and every runner's GeometrySource
// decodes back into (seam-neutral Vector3 only, NEVER a host Brep/Mesh/Point3d, NEVER inlined on the Object node), so
// neither side re-declares a parallel MemberAxis/BoundaryPolygon.
public readonly record struct AxisCurve(Vector3 Start, Vector3 End, Vector3 Up) {
 public double Length => Vector3.Distance(Start, End);
}

public readonly record struct FootprintPolygon(Seq<Vector3> Ring) {
 public static readonly FootprintPolygon Empty = new(Seq<Vector3>());
 public bool IsEmpty => Ring.IsEmpty;
}

// The geometry-resolution PORT [M2]: the seam owns the CONTRACT (content key -> decoded analytical shape), the app wires
// the IMPLEMENTATION over the Rasm.Persistence object-store byte-stream, and an above-seam runner pulls the analytical
// axis/footprint by `member.Representations.Axis`/`.FootPrint` rather than reading a phantom node field. Axis and
// footprint decode to GENUINELY DISTINCT shapes (a line vs a ring), so the port carries ONE typed decode leg per KIND —
// the discriminant is the return TYPE, not a Get/GetById arity family. A missing/undecodable blob is None (the runner
// rails its own typed input-missing fault, never a defaulted coordinate); None is the inert wiring a closed-form route
// threads.
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
// A CLASS-root [Union] (the [GRAPH_FAMILY] form), NOT a record-root: equality and the member-level structured diff
// ride Generator.Equals [Equatable] — LOAD-BEARING, because the drill descends into a nested value only when that
// value is itself [Equatable], so a changed member surfaces as a Nodes[id].<member> path in
// ElementGraph.EqualityComparer.Default.Inequalities (the granularity the Rasm.Persistence 3-way StructuralMerge
// keys on) rather than a whole-node delta; a record-root Node would be an opaque equality leaf, collapsing the merge
// to whole-node replacement. Collection members carry [UnorderedEquality]/[OrderedEquality] so bag/set/sequence
// semantics nest into the diff; the intermediate payload owners carry [Equatable] for the descent and the drill
// BOTTOMS at the native value-equality leaves MeasureValue/PropertyValue, which carry NEITHER [Equatable] nor a
// deeper descent ([04] STRUCTURAL_EQUALITY owns the full drill law). Each case is a sealed CLASS exposing NodeId Id
// as a positional override of the union's abstract Id.
[Union]
[Equatable]
public abstract partial class Node {
 private Node() { }

 public abstract NodeId Id { get; init; }

 // PascalCase primary-ctor parameters: every projector constructs this case with PascalCase NAMED arguments
 // (Id:, Kind:, ...) — named args bind to PARAMETER names, so the parameters carry the corpus spelling and the
 // same-name property initializers read the shadowing parameter (the C# primary-ctor idiom records generate).
 public sealed partial class Object(
 NodeId Id, ObjectKind Kind, Option<string> ExternalId, Classification Classification, PredefinedType PredefinedType,
 string Name, string Tag, RepresentationContentHash Representations,
 Option<OwnerHistory> History, SchemaSpan Span, Seq<Classification> Classifications = default) : Node {
  public override NodeId Id { get; init; } = Id;
  public ObjectKind Kind { get; } = Kind;
  public Option<string> ExternalId { get; } = ExternalId;
  public Classification Classification { get; } = Classification;
  public PredefinedType PredefinedType { get; } = PredefinedType;
  public string Name { get; } = Name;
  public string Tag { get; } = Tag;
  public RepresentationContentHash Representations { get; } = Representations;
  public Option<OwnerHistory> History { get; } = History;
  public SchemaSpan Span { get; } = Span;
  [property: UnorderedEquality] public Seq<Classification> Classifications { get; } = Classifications;

  // The Representations-EXCLUDED canonical seed NodeId.RootedType hashes for the deterministic Type id: the SAME
  // WriteObject the full hash uses, with includeRepresentations: false, so seed and full hash agree byte-for-byte on
  // the stable identity columns and differ only by the volatile representations block the seed omits. The Component
  // projection composes it for a Kind == ObjectKind.Type node; an Occurrence is Guid-v7 rooted, never seeded.
  public ReadOnlyMemory<byte> ToTypeSeedBytes(double tolerance) {
   CanonicalWriter w = new(tolerance);
   WriteObject(w, this, includeRepresentations: false);
   return w.ToBytes();
  }
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
 @object: o => WriteObject(w, o, includeRepresentations: true),
 // Mechanical and Orthotropic share Discipline.Structural, so the discipline-key sort TIES them and a stable sort would
 // leak Seq insertion order into the node bytes — two [UnorderedEquality]-equal Material nodes minting distinct content
 // ids. The per-property full-byte tiebreak (each property's own CanonicalBytes, case ordinal first, ordered through
 // ContentAddress.ByteOrder) is TOTAL, so a same-discipline pair orders identically regardless of insertion order; a
 // material carrying one set per discipline never ties, so its bytes are unchanged.
 material: m => { w.Ordinal(1); w.String(m.MaterialKey.Value); m.Composition.CanonicalBytes(w); w.Ordinal(m.Properties.Count); foreach (var p in m.Properties.OrderBy(static p => p.Discipline.Key, StringComparer.Ordinal).ThenBy(p => { CanonicalWriter k = new(tolerance); p.CanonicalBytes(k); return k.ToBytes(); }, ContentAddress.ByteOrder)) { p.CanonicalBytes(w); } },
 // The bag count prefix is the self-delimiting precondition every raw-append consumer relies on — ContentAddress.Of(Node)
 // and the GraphDelta node sections concat String(id)+Raw(bytes), so an UNCOUNTED trailing row run would absorb the
 // following segment's bytes (two distinct deltas, one hash): the Projection/address#CANONICAL_WRITER count-prefix law.
 propertySet: p => { w.Ordinal(2); w.String(p.Bag.SetName); w.String(p.Bag.Inheritance.Key); w.Ordinal(p.Bag.Values.Count); foreach (var (n, v) in p.Bag.Values.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); v.CanonicalBytes(w); } },
 quantitySet: q => { w.Ordinal(3); w.String(q.Bag.SetName); w.String(q.Bag.Inheritance.Key); w.Ordinal(q.Bag.Values.Count); foreach (var (n, m) in q.Bag.Values.OrderBy(static e => e.Key.Value, StringComparer.Ordinal)) { w.String(n.Value); w.Measure(m); } },
 assessment: a => { w.Ordinal(4); a.Payload.CanonicalBytes(w); },
 appearance: a => { w.Ordinal(5); w.U128(a.Summary.AppearanceKey); },
 coverage: c => { w.Ordinal(6); c.Grid.CanonicalBytes(w); });
 return w.ToBytes();
 }

 // The Object canonical projection, factored so BOTH the full content hash (representations INCLUDED) and the
 // deterministic Type-id seed (representations EXCLUDED) compose ONE writer over the stable identity columns.
 // Representations are the ONLY conditional region — volatile because geometry attaches AFTER a Type is identified —
 // so the includeRepresentations: true path stays byte-for-byte the parity-corpus projection and the Type seed
 // differs only by the omitted block.
 static void WriteObject(CanonicalWriter w, Node.Object o, bool includeRepresentations) {
 w.Ordinal(0); w.String(o.Kind.Key); w.Bool(o.ExternalId.IsSome); o.ExternalId.IfSome(e => w.String(e)); w.String(o.Classification.System); w.String(o.Classification.Code); w.String(o.Classification.Edition); w.Ordinal(o.Classifications.Count); foreach (var c in o.Classifications.OrderBy(static x => x.System, StringComparer.Ordinal).ThenBy(static x => x.Code, StringComparer.Ordinal).ThenBy(static x => x.Edition, StringComparer.Ordinal)) { w.String(c.System); w.String(c.Code); w.String(c.Edition); } w.String(o.PredefinedType.Token); w.String(o.Name); w.String(o.Tag);
 if (includeRepresentations) { w.Ordinal(o.Representations.ByIdentifier.Count); foreach (var (k, h) in o.Representations.ByIdentifier.OrderBy(static p => p.Key, StringComparer.Ordinal)) { w.String(k); w.U128(h); } }
 w.String(o.Span.IntroducedIn.Key); w.Bool(o.Span.RemovedIn.IsSome); o.Span.RemovedIn.IfSome(r => w.String(r.Key));
 }

 // Re-stamp the node's OWN identity to a SPECIFIC new id, payload intact — the endpoint-alignment re-stamp a
 // Rasm.Persistence Reconcile and a Bim re-identify compose. DISTINCT from Remap: Relabel sets the own id (buried
 // references stay), Remap rewrites EVERY id by a function. A class-root [Union] case has NO compiler-generated
 // `with`, so each arm RECONSTRUCTS its case through the func-form generated total Switch — NOT Map, which takes
 // PRECOMPUTED constant values and cannot carry an allocating per-case reconstruction. Exhaustive over the closed
 // seven-case family, payload carried positionally so a case gaining a field breaks loudly. For a non-rooted node
 // the caller re-mints from the new content (Relabel is the rooted-node/endpoint-alignment rewrite).
 public Node Relabel(NodeId id) => Switch<Node>(
  @object: o => new Object(id, o.Kind, o.ExternalId, o.Classification, o.PredefinedType, o.Name, o.Tag, o.Representations, o.History, o.Span, o.Classifications),
  material: m => new Material(id, m.MaterialKey, m.Composition, m.Properties),
  propertySet: p => new PropertySet(id, p.Bag),
  quantitySet: q => new QuantitySet(id, q.Bag),
  assessment: a => new Assessment(id, a.Payload),
  appearance: a => new Appearance(id, a.Summary),
  coverage: c => new Coverage(id, c.Grid));

 // Re-map EVERY node-id the node carries — own Id AND every graph-node reference BURIED in a payload — the rewrite a
 // Rasm.Persistence Reconcile composes onto durable ids, the exact dual of Relationship.Remap. The ONLY payloads that
 // bury a graph-node NodeId are the PropertySet bags through PropertyValue.Reference, so that arm composes the ONE
 // PropertyValue.Remap owner over each bag value (the QuantityBag's MeasureValue carries no NodeId; MaterialId is a
 // value-object key, not a NodeId; the remaining payloads bury none — those arms rewrite the own id alone). An
 // own-id-only rewrite that dangled a bag's Reference is the deleted form. Same func-form total Switch over the
 // closed family (a new case breaks the build, never a silent pass-through stranding a buried id).
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

- Owner: `Header` the model header (`ReleaseVersion` + `ModelView` + `Geospatial/reference#GEO_REFERENCE` `GeoReference` + `Tolerance` + `Instant` + `StepHeader`) carrying the ONE semantic-header `CanonicalBytes` projection both the `Projection/address#CONTENT_ADDRESS` `OfGraph` snapshot key and the `Graph/delta#GRAPH_DELTA` `GraphDelta.ToCanonicalBytes` header contribution compose (the projection owned once, never re-spelled per call site); `ElementGraph` the frozen read snapshot carrying the nodes, edges, the built-once incidence index, and the memoized `Bake`; `Element` the derived-fold "has it all" result; `BakedMaterial` the material-plus-usage pair `Bake` folds from an `Associate` edge (the occurrence's own AND, via the named inheritance, the `Component`'s, unioned by `MaterialKey`); `TypeBinding` the named type→occurrence inheritance carrier `Bake` produces from the `Assign.TypeDefinition` resolution (the type id + the inherited `BakedMaterial` set / resolved `SectionProperties` / secondary `Classification`s), surfaced as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes.
- Entry: `ElementGraph.Of(header, nodes, edges)` builds the frozen snapshot — `ToFrozenDictionary` over the nodes, the incidence index grouping every edge by both endpoints, the lazy `QuikGraph` `BidirectionalGraph` topology view, and an empty `Bake` memo; `Genesis(header)` seeds the empty header-only snapshot a model-creating session or a Marten stream rehydrate builds onto, and `Apply(delta, key)` advances a snapshot by a validated `Graph/delta#GRAPH_DELTA` `GraphDelta` (the persistence rehydrate + live-apply entry) `Fin<T>` railing `ElementFault.NodeAbsent` on a corrupt delta whose added edge names an absent member — either binary endpoint or a `Connect`'s realizing intermediary, the full `Relationship.Members` closure; `Bake(objectId, key)` folds the reachable subgraph from an `Object` node into an `Element`, memoized by `objectId` within the snapshot (a new snapshot from a `Graph/delta#GRAPH_DELTA` carries a fresh memo), `Fin<T>` railing `ElementFault.NodeAbsent` on an absent root and `ElementFault.RelationshipInvalid` on a cyclic `Compose` chain (a `Compose` ancestry set threaded through the fold); `Topology()` reads the cached `QuikGraph` view a reachability/topological-order/LCA consumer composes; the read accessors `ObjectNodes`/`Find`/`Material`/`MaterialsOf`/`CompositionOf`/`PropertiesOf`/`SectionOf` enumerate the object roots and resolve a node (raw or typed by case) and the material/composition/property/section subgraph a member binds — `MaterialsOf` carrying the one-hop type-resolved fallback the other three compose (an occurrence with no own material/profile reads its `Component`'s), the Op-FREE `SectionOf(member)` signature FROZEN — the polymorphic surface a `Rasm.Compute` analysis route reads the concrete graph through, the discipline reads (loads/supports/spaces/areas) composing in Compute from these primitives.
- Auto: `Of` builds the incidence index once at the freeze boundary so `Bake` reads an object's edges in O(degree) rather than scanning every edge; `Bake` resolves the `Object` and folds its OWN incidence edges in ONE pass — `Assign.PropertyDefinition` resolves a `PropertySet`/`QuantitySet` node into the occurrence bag, `Assign.Assessment` collects the `Assessment` receipt, `Associate` resolves a `Material` node (with its `MaterialUsage`) into a `BakedMaterial`, an `Appearance` node into the summary, or a `Coverage` node into the grid, and `Compose.Aggregate`/`Nest`/`Contain` recurse into child `Object`s as `Parts` (the non-owning `Reference` excluded, and the host-feature `Void`/`Connect` and group/system relationships read through `EdgesAt`/`Topology` rather than baked into a flat field); `Assign.TypeDefinition` resolves the `Component` (`TypeResolutionOf`, ONE fold over the type's incidence gathering its property/quantity bags, `BakedMaterial` set, and `Assessment` receipts) and the NAMED type→occurrence inheritance applies once wholly within the seam — the bags merge via `Properties/property#PROPERTY_BAG` `PropertyBag.Merge` (the `InheritanceMode` value-bag precedence, UNCHANGED), the single fields occurrence-overrides-type (`PredefinedType`/`Name`/`Representations` falling back to the type on the IFC unset sentinel, the primary `Classification` the occurrence's own), and the `BakedMaterial`/`AssessmentPayload`/secondary-`Classification` `Seq`s union+dedup-by-key, the `TypeBinding` surfaced as `Element.Type`; the `QuikGraph` view and the `Bake` memo are built lazily and excluded from equality so the frozen snapshot stays structurally compared by its nodes and edges, and no `with` copy can exist — `ElementGraph` is a sealed CLASS, not a record, so the compiler emits no copy that could alias those lazily-built caches across snapshots (the misuse is compile-impossible, not a runtime throw) — only `Of`/`Genesis`/`Apply` mint a fresh one.
- Receipt: the `Element` is the one flat record a consumer reads — `element.Properties.Find(name)`, `element.Materials`, `element.Assessments`, `element.Appearance`, `element.Coverages`, `element.Parts`, and `element.TypeId` (the inherited `Component`, the generator's type-representation recovery key) — "has it all" in one `Bake`, never a join across the graph; the `ElementGraph` is the immutable read snapshot Persistence persists and the projectors assemble onto, its `Generator.Equals` structural equality and `Inequalities` member diff feeding the Persistence 3-way `StructuralMerge`; the `QuikGraph` topology view answers reachability/containment/LCA for a consumer without a second graph.
- Packages: `Generator.Equals` (`[Equatable]` snapshot equality + `Inequalities` diff), QuikGraph (`BidirectionalGraph`/`SEdge` topology view + `AlgorithmExtensions`), LanguageExt.Core (`Seq`/`Map`/`Option`/`Fin`), System.Collections.Frozen/Immutable, NodaTime (`Instant`), `Rasm` (the kernel `Op` op-key).
- Growth: a new derived element field is one column on `Element` the `Bake` fold populates from an existing edge kind; a new edge semantic the fold reads is one arm in `Bake`; a new type-inherited `Seq` is one `UnionBy` arm in the named inheritance, a new occurrence-overrides-type single field one fall-back guard; the working/frozen split keeps the live delta path in the HAMT (`Graph/delta`) and the read path in the frozen snapshot, so neither grows the other; never a second stored `Element` record beside the graph, never a second identity scheme for the deterministic Type id.
- Boundary: the `Element` is a DERIVED FOLD, never a stored record — the migration source's parallel `BimElement`/`Materials.Element` records are the deleted form, the one flat read coming from `Bake` over the graph so typed data is never stranded off the element; the graph splits by PHASE — the live authoring/delta path is an `ImmutableDictionary` HAMT (`Graph/delta` owns it for O(log n) structural sharing) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` at the freeze boundary), so a mutable working graph is never confused with a frozen read snapshot; the incidence index and the `QuikGraph` view are built ONCE per snapshot and the `Bake` memo is keyed by object within the snapshot, invalidated only by a new snapshot from a delta, so a re-`Bake` is O(1) and a graph edit is O(log n); the NAMED type→occurrence inheritance applies once in `Bake` — single fields occurrence-overrides-type, the materials/assessments/classifications `Seq`s union+dedup-by-key — DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode` value-bag precedence the `PropertyBag.Merge` owns (the named inheritance never extends `InheritanceMode`, which stays bag-only), and the `MaterialsOf`/`SectionOf` type-resolved fallback is one hop (a `Component` is not itself typed) so the FROZEN Op-free `SectionOf(member)` signature `Rasm.Compute` reads is untouched; a TYPE `Object`'s deterministic id excludes the volatile `Representations` so a geometry attach re-keys neither the Type node nor the cached `Bake`; the `Header` carries the `GeoReference` and the `StepHeader`, the `Object` nodes carry the `OwnerHistory` and the `SchemaSpan`, so the model's provenance and schema span ride the graph, not a side channel.

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

// The material-plus-usage pair the Bake fold derives from an Associate(Material) edge — the occurrence's own bindings
// AND, via the named type inheritance, the Component's, unioned by MaterialKey. The seam-baked accessor pair, distinct
// from the type→occurrence TypeBinding (the inherited Component data) so each altitude owns one name and no collision.
public readonly record struct BakedMaterial(Node.Material Material, MaterialUsage Usage);

// The Component (the Type Object) a baked Element inherits from — surfaced so a generator recovers WHICH standardized
// Component a piece realizes: the Type id (Element.TypeId reads it) plus the type-level data the occurrence inherited —
// the Component's BakedMaterial set, the resolved SectionProperties (the type's ProfileSet section, the M7 fallback
// SectionOf reads when the occurrence has no own profile), and the type's secondary classification refs. None on the
// Element when the occurrence carries no Assign.TypeDefinition edge (a bare occurrence baked from its own data alone).
// A DERIVED read carrier (recoverable from the graph), so it carries record value equality, not the [Equatable] merge
// drill — the Rasm.Persistence StructuralMerge keys on the ElementGraph nodes/edges, never on a baked Element.
public readonly record struct TypeBinding(NodeId TypeId, Seq<BakedMaterial> Materials, Option<SectionProperties> Section, Seq<Classification> Classifications);

[Equatable]
public sealed partial record Element(
 NodeId Id, ObjectKind Kind, Option<string> ExternalId, Classification Classification, PredefinedType PredefinedType, string Name, string Tag,
 RepresentationContentHash Representations,
 [property: UnorderedEquality] Seq<BakedMaterial> Materials,
 [property: UnorderedEquality] Seq<PropertyBag> Properties,
 [property: UnorderedEquality] Seq<QuantityBag> Quantities,
 [property: UnorderedEquality] Seq<AssessmentPayload> Assessments,
 Option<AppearanceSummary> Appearance,
 [property: UnorderedEquality] Seq<CoverageGrid> Coverages,
 [property: OrderedEquality] Seq<Element> Parts,
 Option<TypeBinding> Type,
 Option<OwnerHistory> History,
 [property: UnorderedEquality] Seq<Classification> Classifications = default) {
 // The Component a piece inherits, surfaced so a generator (and the Bim type-representation round-trip) recovers WHICH
 // standardized Component this occurrence realizes; None for a bare occurrence authored with no Assign.TypeDefinition edge.
 public Option<NodeId> TypeId => Type.Map(static t => t.TypeId);
}

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
 // intermediary AND a Generic edge's buried PropertyValue.Reference attribute resolve through EdgesAt — EdgesAt(n) ==
 // "every edge touching n", aligned with Touches and the DropNode cascade; an endpoints-only index would strand a
 // realizing or attribute reference the cascade still sweeps. Members dedup per edge: a self-looping Generic edge (the
 // one self-permissive kind — LegalLink rails every typed self-loop) or a buried ref coinciding with an endpoint lists
 // once per node, never twice in one EdgesAt array.
 incidence = edges
 .SelectMany(e => e.Members.Distinct().Select(m => (Node: m, Edge: e)))
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
 // EVERY member an added edge touches resolves in the result — the binary endpoints, a Connect's realizing
 // intermediary, AND a Generic edge's buried PropertyValue.Reference attribute (Relationship.Members, the same closure
 // the incidence index and the DropNode cascade key on) — railing
 // ElementFault.NodeAbsent so a corrupt stored delta never freezes a dangling graph; an endpoints-only guard that let a
 // dangling realizing or attribute reference into the topology view is the deleted under-check (the structural law ran at
 // WorkingGraph.Apply when the delta was produced).
 public Fin<ElementGraph> Apply(GraphDelta delta, Op key) {
 ElementGraph next = delta.ReplayOnto(this);
 return delta.AddedEdges
 .Choose(e => e.Members.Find(m => !next.Nodes.ContainsKey(m)))
 .Head
 .Match(
 Some: member => ElementFault.NodeAbsent(key, $"<replay-edge-member-absent:{member.Value}>"),
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

 // The member's DIRECTLY-associated material nodes — the Associate(Material) edges off ONE node — the occurrence-OR-type
 // projection MaterialsOf composes for both the occurrence and (one hop) its Component, so neither side re-spells it.
 Seq<Node.Material> DirectMaterialsOf(NodeId node) =>
 toSeq(EdgesAt(node)).Choose(e => e is Relationship.Associate r && r.Subject == node && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m ? Some(m) : None);

 // The Component (Type Object) a member binds via its Assign.TypeDefinition edge — the ONE-hop type resolution the
 // type-resolved read accessors AND the Bake named inheritance share; None for a bare occurrence with no Component.
 Option<NodeId> TypeObjectOf(NodeId member) =>
 toSeq(EdgesAt(member)).Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == member ? Some(a.Definition) : None).Head;

 // A member's associated materials, occurrence-direct with a TYPE-RESOLVED fallback: when the occurrence carries no own
 // Associate(Material) edge, resolve through its Component (the Assign.TypeDefinition type Object's OWN direct materials) —
 // ONE type-hop, never recursive (a Component is not itself typed). CompositionOf/PropertiesOf/SectionOf compose THIS one
 // accessor, so the type fallback propagates to all three through the single fallback point, never four duplicated arms.
 public Seq<Node.Material> MaterialsOf(NodeId member) {
 Seq<Node.Material> direct = DirectMaterialsOf(member);
 return direct.IsEmpty ? TypeObjectOf(member).Match(Some: DirectMaterialsOf, None: () => direct) : direct;
 }

 public Option<MaterialComposition> CompositionOf(NodeId member) => MaterialsOf(member).Head.Map(static m => m.Composition);

 // The FULL typed engineering-property profile a member's associated materials carry — the polymorphic property read a
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
 // ONE fold over the OCCURRENCE's incidence resolves its OWN edges flat — the Assign property/quantity/assessment bags
 // AND all three Associate resource kinds (Material+usage→BakedMaterial, Appearance, Coverage) — so EdgesAt(root) is
 // walked ONCE, never a Material/Appearance fold plus a third CoveragesOf re-scan of the same Associate dispatch (the
 // deleted COLLAPSE_SCAN repeated-arm form). The Associate resource kinds are the LegalLink Material/Appearance/Coverage
 // closure (Graph/delta#GRAPH_DELTA), so the three arms mirror that legality exactly.
 var (occProps, occQty, occMaterials, occAssessments, appearance, coverages) =
 toSeq(EdgesAt(root.Id)).Fold(
 (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>(), Mat: Seq<BakedMaterial>(), Asm: Seq<AssessmentPayload>(), App: Option<AppearanceSummary>.None, Cov: Seq<CoverageGrid>()),
 (acc, edge) => edge switch {
 Relationship.Assign a when a.Subject == root.Id && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) },
 Relationship.Assign a when a.Subject == root.Id && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) },
 Relationship.Assign a when a.Subject == root.Id && a.SubKind == AssignKind.Assessment && Nodes.TryGetValue(a.Definition, out var d) && d is Node.Assessment asm => acc with { Asm = acc.Asm.Add(asm.Payload) },
 Relationship.Associate r when r.Subject == root.Id && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m => acc with { Mat = acc.Mat.Add(new BakedMaterial(m, r.Usage)) },
 Relationship.Associate r when r.Subject == root.Id && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Appearance ap => acc with { App = ap.Summary },
 Relationship.Associate r when r.Subject == root.Id && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Coverage c => acc with { Cov = acc.Cov.Add(c.Grid) },
 _ => acc,
 });
 // The NAMED type→occurrence inheritance (Relations/relation#EDGE_ALGEBRA Assign.TypeDefinition): resolve the Component
 // (type Object), then merge occurrence-over-type — DISTINCT from the Properties/property#PROPERTY_BAG InheritanceMode
 // value-bag precedence (which stays the PropertyBag.Merge below). Single fields occurrence-overrides-type; the Seq
 // fields materials/assessments/classifications union + dedup-by-key. None for a bare occurrence (no Component bound).
 Option<(Node.Object Type, Seq<PropertyBag> Props, Seq<QuantityBag> Qty, Seq<BakedMaterial> Materials, Seq<AssessmentPayload> Assessments)> typeFold = TypeResolutionOf(root.Id);
 // Properties/Quantities: the EXISTING InheritanceMode value-bag merge (type-then-occurrence precedence via Merge) —
 // the named inheritance does NOT touch the bag-precedence the bag Merge owns, only the single fields and the Seq sets.
 Seq<PropertyBag> properties = MergeBagSets(typeFold.Map(static t => t.Props).IfNone(Seq<PropertyBag>()), occProps);
 Seq<QuantityBag> quantities = MergeBagSets(typeFold.Map(static t => t.Qty).IfNone(Seq<QuantityBag>()), occQty);
 // Materials/Assessments/Classifications: occurrence-precedence Seq union, dedup by key — the MaterialKey string, the
 // (Discipline, Route, InputKey) assessment cache triple, and the (System, Code, Edition) classification identity.
 Seq<BakedMaterial> materials = UnionBy(occMaterials, typeFold.Map(static t => t.Materials).IfNone(Seq<BakedMaterial>()), static b => b.Material.MaterialKey.Value);
 Seq<AssessmentPayload> assessments = UnionBy(occAssessments, typeFold.Map(static t => t.Assessments).IfNone(Seq<AssessmentPayload>()), static a => (a.Discipline.Key, a.Route.Value, a.InputKey));
 Seq<Classification> classifications = UnionBy(root.Classifications, typeFold.Map(static t => t.Type.Classifications).IfNone(Seq<Classification>()), static c => (c.System, c.Code, c.Edition));
 // Single fields are occurrence-overrides-type: the primary Classification is the occurrence's own (admission guarantees
 // a non-blank entity-class code), while PredefinedType/Name/Representations fall back to the Component when the
 // occurrence carries the IFC unset sentinel — a NOTDEFINED predefined defers to the type, a blank Name to the type
 // designation, an own-geometry-less occurrence to the type's mapped Representations.
 PredefinedType predefinedType = typeFold.Match(Some: t => root.PredefinedType == PredefinedType.NotDefined ? t.Type.PredefinedType : root.PredefinedType, None: () => root.PredefinedType);
 string name = typeFold.Match(Some: t => root.Name.Length > 0 ? root.Name : t.Type.Name, None: () => root.Name);
 RepresentationContentHash representations = typeFold.Match(Some: t => root.Representations.ByIdentifier.Count > 0 ? root.Representations : t.Type.Representations, None: () => root.Representations);
 // The TypeBinding surfaced on the baked Element (Element.TypeId reads its id): the type id, the inherited BakedMaterial
 // set, the type's resolved ProfileSet section (the M7 fallback SectionOf reads), and the type's secondary classification refs.
 Option<TypeBinding> typeBinding = typeFold.Map(static t => new TypeBinding(
 t.Type.Id, t.Materials,
 t.Materials.Choose(static m => m.Material.Composition is MaterialComposition.ProfileSet { Section: var s } ? s : Option<SectionProperties>.None).Head,
 t.Type.Classifications));
 return BakeParts(root.Id, key, ancestry).Map(parts => new Element(
 root.Id, root.Kind, root.ExternalId, root.Classification, predefinedType, name, root.Tag, representations,
 materials, properties, quantities, assessments, appearance,
 coverages, parts, typeBinding, root.History, classifications));
 }

 // The named type→occurrence inheritance resolution: the Assign.TypeDefinition edge resolved to the Component (type
 // Object), then ONE fold over the TYPE's incidence gathers every type-level datum — the property/quantity bags, the
 // BakedMaterial set, the Assessment receipts — in a SINGLE pass (the type's single fields and secondary Classifications
 // ride the resolved Object, the section derives from the type materials' ProfileSet). None for a bare occurrence with
 // no Component binding. The type carries NO further TypeDefinition edge (a Component is not itself typed), so this is a
 // single one-hop resolution, never a recursive type chain, and the type's data is gathered as DATA, never a recursive Bake.
 Option<(Node.Object Type, Seq<PropertyBag> Props, Seq<QuantityBag> Qty, Seq<BakedMaterial> Materials, Seq<AssessmentPayload> Assessments)> TypeResolutionOf(NodeId occurrence) =>
 TypeObjectOf(occurrence).Bind(typeId => Find<Node.Object>(typeId).Map(typeObj => {
 var (props, qty, mats, asms) = toSeq(EdgesAt(typeId)).Fold(
 (Props: Seq<PropertyBag>(), Qty: Seq<QuantityBag>(), Mat: Seq<BakedMaterial>(), Asm: Seq<AssessmentPayload>()),
 (acc, edge) => edge switch {
 Relationship.Assign a when a.Subject == typeId && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.PropertySet ps => acc with { Props = acc.Props.Add(ps.Bag) },
 Relationship.Assign a when a.Subject == typeId && a.SubKind == AssignKind.PropertyDefinition && Nodes.TryGetValue(a.Definition, out var d) && d is Node.QuantitySet qs => acc with { Qty = acc.Qty.Add(qs.Bag) },
 Relationship.Assign a when a.Subject == typeId && a.SubKind == AssignKind.Assessment && Nodes.TryGetValue(a.Definition, out var d) && d is Node.Assessment asm => acc with { Asm = acc.Asm.Add(asm.Payload) },
 Relationship.Associate r when r.Subject == typeId && Nodes.TryGetValue(r.Resource, out var res) && res is Node.Material m => acc with { Mat = acc.Mat.Add(new BakedMaterial(m, r.Usage)) },
 _ => acc,
 });
 return (typeObj, props, qty, mats, asms);
 }));

 // Set-union by SetName: each occurrence bag merges with its matching type bag (precedence via the ONE
 // ValueBag<V>.Merge the PropertyBag/QuantityBag global-using aliases share), and a type-only bag with no occurrence
 // counterpart is inherited as-is — one generic fold serves BOTH aliases, never a per-alias copy of one body.
 static Seq<ValueBag<V>> MergeBagSets<V>(Seq<ValueBag<V>> type, Seq<ValueBag<V>> occurrence) =>
 occurrence.Map(occ => type.Find(t => t.SetName == occ.SetName).Match(Some: t => ValueBag<V>.Merge(t, occ), None: () => occ))
 + type.Filter(t => !occurrence.Exists(o => o.SetName == t.SetName));

 // Occurrence-precedence Seq union, dedup by a value-equatable key: every occurrence entry kept (the occurrence is
 // authoritative), a type entry appended only when its key is absent (a type-internal duplicate skipped — the growing
 // accumulator IS the membership probe). The key is a string or a value tuple ((Discipline, Route, InputKey); (System,
 // Code, Edition)), so equality is STRUCTURAL with no separator-collision reasoning. The sets are a member's own
 // associations (a handful of rows), so the linear probe is total — no second collection type, no LanguageExt Set,
 // only the confirmed Seq combinators (Fold/Exists/Add).
 static Seq<T> UnionBy<T, K>(Seq<T> occurrence, Seq<T> type, Func<T, K> key) where K : IEquatable<K> =>
 type.Fold(occurrence, (acc, item) => acc.Exists(e => key(e).Equals(key(item))) ? acc : acc.Add(item));

 // The OWNING Compose children only — Aggregate (decomposition), Nest (ordered child sequence), and Contain (spatial
 // containment) recurse into Parts; the non-owning Reference flavor (IfcRelReferencedInSpatialStructure — an element
 // referenced in an additional spatial structure it is NOT contained by) is EXCLUDED, so a column contained in storey A
 // and referenced in storey B bakes as a Part of A alone, never duplicated onto B. Baking every Compose flavor is the
 // deleted form, contradicting the Bake prose (Aggregate/Nest/Contain are the parts) and double-counting referenced
 // elements. A dangling part id (a corrupt snapshot whose Compose edge names an undeclared node — unreachable in a
 // validated graph, LegalLink admits only present endpoints and Erase cascades) rails NodeAbsent through the recursive
 // Bake, never a presence pre-filter that silently truncates Parts and masks the corruption the fault band surfaces.
 Fin<Seq<Element>> BakeParts(NodeId whole, Op key, ImmutableHashSet<NodeId> ancestry) =>
 toSeq(EdgesAt(whole))
 .Choose(e => e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference ? Some(c.Part) : None)
 .TraverseM(part => Bake(part, key, ancestry)).As().Map(static parts => parts.ToSeq());
}
```

## [04]-[RESEARCH]

- [DERIVED_ELEMENT]: the consumer-facing `Element` is a `Bake` fold over the reachable subgraph, never a second stored record — this cures the migration source's stranded-data defect (the `Rasm.Bim` `BimElement` and `Rasm.Materials` `Element` were parallel records ID-referencing their property/material data, never joined); the fold reads the incidence edges, resolves the typed node payloads, applies the NAMED type→occurrence inheritance once (single fields occurrence-overrides-type, the materials/assessments/classifications `Seq`s union+dedup-by-key — distinct from the `InheritanceMode` value-bag precedence the `PropertyBag.Merge` owns), surfaces the inherited `Component` as `Element.Type`/`Element.TypeId`, and recurses the OWNING `Compose` children (`Aggregate`/`Nest`/`Contain`, never the non-owning `Reference`), so "has it all" is one flat read and a graph edit re-bakes in O(1) against the per-snapshot memo.
- [GRAPH_PHASE_SPLIT]: the graph splits by phase — the live authoring/delta path is an `ImmutableDictionary` HAMT (`Graph/delta#GRAPH_DELTA` owns it for O(log n) structural sharing across edits) and `ElementGraph` is the FROZEN read snapshot (`ToFrozenDictionary` + the incidence index + the `QuikGraph` view + the `Bake` memo, all built once at the freeze boundary) — so the working graph is never confused with the read snapshot, and the freeze boundary is where the analytical structures materialize; the incidence index (keyed by every node an edge's `Members` touches — so a `Connect`'s realizing intermediary resolves through `EdgesAt`, consistent with `Touches` and the `DropNode` cascade) gives `Bake` O(degree) edge access, the `QuikGraph` `BidirectionalGraph` (built from each edge's `DirectedPairs`, so reachability traverses THROUGH a realizing intermediary rather than an endpoints-only shortcut) answers global reachability/topological-order/LCA a consumer composes through `AlgorithmExtensions`, and both are built once per snapshot.
- [IDENTITY_AND_HASH]: the `NodeId` is ONE identity owner over one regime with two rooted seedings plus the non-rooted content hash — an OCCURRENCE `Object` a Guid-v7 placement id (sortable, kernel-minted), a TYPE `Object` a DETERMINISTIC kernel `XxHash128` over its `Representations`-excluded canonical seed (`Node.Object.ToTypeSeedBytes` through `NodeId.RootedType`, the SAME hasher `Content` composes), and a non-rooted node a kernel `XxHash128` content hash over its full `ToCanonicalBytes`; the compressed IFC GlobalId is a Bim-stored projection attribute re-emitted at `Emit`. The deterministic Type id excludes the volatile `Representations` (the `WriteObject` projection with `includeRepresentations: false`) so identical `Component`s dedup to one Type and a later geometry attach never re-keys it, while the FULL `Object` hash (representations INCLUDED) stays byte-for-byte the prior projection so the cross-runtime parity corpus is unperturbed — `ToCanonicalBytes` is the ONE canonical projection the non-rooted id mint and the `Projection/address#CONTENT_ADDRESS` diff share (fixed IEEE-754 LE bits, measures quantized to `Header.Tolerance`, explicit attribute order, id excluded), so a node's content identity is stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed — a float-bearing golden vector (an `IfcMaterialLayer`-shaped node) anchors the cross-runtime parity corpus, and the Type seed is a C#-side mint a peer READS as an opaque rooted id, never re-derives, `Graph/wire#WIRE_CODEC` the proto envelope that carries every id verbatim around the content the keys were minted from. A `PropertySet`/`QuantitySet`-bearing content key derives from the COUNTED bag layout — `Ordinal(count)` before the sorted rows, the `Projection/address#CANONICAL_WRITER` count-prefix law — the cross-runtime wire law the queued Python/TypeScript canonical-writer mirrors reproduce; an uncounted bag run is the deleted injectivity hole (a trailing run parsing as a prefix of the next raw-append segment).
- [TYPE_INHERITANCE]: the named type→occurrence inheritance is the seam-resolved realization of the `Relations/relation#EDGE_ALGEBRA` `Assign.TypeDefinition` bind — the `Component` projection (the owner that mints its Type) authors the occurrence→Type edge, and `Bake`'s `TypeResolutionOf` folds the `Component`'s standardized data (the property/quantity bags, the `BakedMaterial` set, the `Assessment` receipts, plus the type `Object`'s single fields and secondary classifications) in ONE pass, then merges occurrence-over-type with explicit per-field precedence: single fields occurrence-overrides-type (`PredefinedType`/`Name`/`Representations` falling back to the type on the IFC unset sentinel, the primary `Classification` the occurrence's own non-blank code), the materials/assessments/classifications `Seq`s union+dedup-by-key (the `MaterialKey` string; the `(Discipline, Route, InputKey)` assessment cache triple; the `(System, Code, Edition)` classification identity). This is DISTINCT from the `Properties/property#PROPERTY_BAG` `InheritanceMode`, which stays `PropertyBag`-value precedence (the bag `Merge`) and is never extended by the named dimension. The `TypeBinding` surfaces the inherited `Component` as `Element.Type` so `Element.TypeId` recovers which `Component` a piece realizes (the `Rasm.Bim` type-representation round-trip key), and `MaterialsOf` gains a one-hop type-resolved fallback `CompositionOf`/`PropertiesOf`/`SectionOf` compose (a minor part sharing one `Component`'s profile reads its section with no occurrence-direct association) WITHOUT perturbing the FROZEN Op-free `SectionOf(member)` signature `Rasm.Compute` reads — the fallback is a single type-hop (a `Component` is not itself typed), never a recursive type chain.
- [STRUCTURAL_EQUALITY]: `Generator.Equals` `[Equatable]` gives `ElementGraph`, the `Node` `[Union]`, and the `Relationship` `[Union]` deep structural equality (the `[UnorderedEquality]` node map, the `[OrderedEquality]` edge array, the per-case collection attributes) — all three are class-root `[Union]`/record owners so equality is `[Equatable]`-generated, never the record-root equality Thinktecture would otherwise own (the `[GRAPH_FAMILY]` law), and never stacked on a record-root union. The `Inequalities(before, after)` member-level diff feeds the `Rasm.Persistence` `StructuralMerge` 3-way reconcile: because `Node` and `Relationship` are `[Equatable]`, the diff drills PAST the node map into each changed node's members, localizing a change to `Nodes[id].<member>` so the merge operates at member granularity, never whole-node replacement. The drill descends one structural-equality link per hop, so the deepest paths (`Nodes[id].Properties[2].Thickness` through a `MaterialComposition.LayerSet`, `Nodes[id].Bag.Values[name]` through a `PropertyBag`, `Nodes[id].Payload.Results[name]` through an `AssessmentPayload`) require the INTERMEDIATE payload owners — `MaterialComposition`/`MaterialPropertySet` (class-root `[Union]`+`[Equatable]`), `MaterialLayer`/`MaterialConstituent`/`SectionProperties` (`[Equatable]` record-structs), `PropertyBag` (the `[Equatable]` `ValueBag`), `AssessmentPayload` (`[Equatable]` record), `CoverageGrid` — to carry a `[Equatable]` structural member set the diff descends THROUGH, and the drill then BOTTOMS at the atomic value-equality LEAF: a `MeasureValue` is a native-equality `readonly record struct` (`Properties/quantity#MEASURE_VALUE`) the `Generator.Equals` `DefaultEqualityComparer` compares atomically by its record value-equality — `Nodes[id].Properties[2].Thickness` is the leaf, never `.Thickness.Si` — and a `PropertyValue` is a RECORD-root `[Union]` whose Thinktecture record-generated value equality is correct, the leaf at `Nodes[id].Bag.Values[name]` (an IFC Pset value the merge replaces wholesale, never sub-merges into a `Bounded` bound). `MeasureValue`/`PropertyValue` carry NEITHER `[Equatable]` (it would redundantly re-derive the field compare the record already gives) NOR a deeper descent — they ARE the merge leaves; an owner that is an opaque equality leaf where the merge needs member-granular descent is the boundary where the drill stops at `Nodes[id].<that-member>` and the merge's `Append` falls back to the two content-signature pointers. `ElementGraph` is a sealed CLASS (its equality is `[Equatable]`-owned, NOT record-derived), so it has no compiler-generated `with` to alias the lazily-built caches — the snapshot is frozen by construction, and the `Bake` memo, the incidence index, and the `QuikGraph` view are unattributed fields excluded from `[Equatable]` so two snapshots compare by their nodes and edges, not their lazily-built analytical caches.
