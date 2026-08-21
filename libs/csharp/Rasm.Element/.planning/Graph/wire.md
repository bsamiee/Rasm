# [ELEMENT_WIRE]

`ElementWire` owns the proto-first `rasm.element.v1` graph crossing. `ElementGraphWire` and `GraphDeltaWire` mirror closed seam unions; `WireCodec` owns per-case transcription; `Encode` lowers valid values; `DecodeGraph` and `DecodeDelta` re-admit hostile input on `Fin<T>`.

Content keys cross verbatim — `NodeId` as X32 text, `UInt128` as big-endian bytes — and every `NodeWire` carries the authoritative id-inclusive address minted under the active header tolerance. Decode reuses value admissions, graphs enter through `GraphDelta.AdmitOnto`, deltas prove `NormalForm(Op)` first, and `WireLimits` owns parse budgets and address verification.

`RedactionScope` clears encoded fields and carries the manifest, so unstable-node addresses remain evidence yet serve no OCC. Measures carry SI magnitude, quantity token, and dimension exponents. `GraphCrossing` composes the kernel message-envelope owner and admits the Protobuf event format over the wire body.

## [01]-[INDEX]

- [02]-[WIRE_CODEC]: the corpus contract header with its union-mirror roster and frozen-number ledger, the `CrossingFamily` union-arity owner with the static-init parity fold, the `[KEY_CODECS]` and shared decode gates, the cross-family `Option` carriers, `ElementWire` encode/decode boundary, `WireLimits`, and the key, depth, and evolution laws. The per-family transcriptions are FIVE partial parts of this ONE `[Mapper]` class: [NODE_PAYLOAD](payload.md) the node/edge envelopes and header/object payloads, [VALUE_CODEC](value.md) the recursive value and evidence-envelope plane, [SUBSTANCE_CODEC](substance.md) the material plane, [EVIDENCE_CODEC](evidence.md) the assessment/observation plane, [RASTER_CODEC](raster.md) the coverage/georeference plane.
- [03]-[EGRESS_REDACTION]: the `rasm.element` sensitivity taxonomy over the wire's classified columns, the `ClassifiedColumn` roster carrying each column group's `FieldMask` and identity verdict, and `RedactionScope` — the presence-clearing egress policy and its `RedactionManifestWire` receipt.
- [04]-[EVENT_ENVELOPE]: the `GraphEventType` closed crossing vocabulary over the kernel grammar and `GraphCrossing` — the mint composing `Rasm/Domain/event#ENVELOPE_MINT`, the Protobuf-format frame pair, the content-key `subject`, and the handling grade the egress scope derives.

## [02]-[WIRE_CODEC]

- Owner: the corpus-homed `rasm/element/v1/element.proto` `rasm.element.v1` contract — the language-neutral message roster `Grpc.Tools` compiles for C# (`GrpcServices=None`, message codegen only) and `buf`/`protoc-gen-es` + `grpcio-tools` compile for the TypeScript/Python peers, every compiler reading the one corpus root so the descriptor names this file identically at all three; `WireCodec` the `[Mapper]` static transcription family owning every per-case seam↔wire field mapping; `SeamConverters` the PUBLIC peer-reachable converter set (identity key codecs, cross-family `Option` carriers, the `MeasureValue`/`MeasureBand` encode legs) WireCodec composes via `[UseStaticMapper]` and a W3 peer mapper reaches the same way; `ElementWire` the boundary owner railing decode onto `Fin<T>`; `WireLimits` the parameterized decode-budget policy record.
- Cases: every closed seam union crosses as a `oneof` mirroring its cases 1:1 — `NodeWire` the eight `Node` payloads, `RelationshipWire` the six edge kinds, `PropertyValueWire` the recursive fourteen-case value family, `MaterialUsageWire` the explicit none/layer/profile usage family, `MaterialCompositionWire` the four composition arms, and `MaterialPropertySetWire` the engineering-property family. Generated keyed owners cross by key; absence is field presence, never a numeric or unset-oneof sentinel.
- Law: the corpus file is the ONE proto spelling — this page carries the header fence, the union-mirror roster, and the frozen-number ledger, never a second transcription of the message roster, and the `CrossingFamily` rows tie each family's arm count to its generated oneof enum so a case landing on either side alone throws at first codec touch; the rows and the roster table move as one edit.
- Law: `WireCodec` is ONE `[Mapper]` partial class spread across six pages by MESSAGE FAMILY — the attribute, the parity fold, the key codecs, the shared gates, and the cross-family carriers ride HERE; each family page carries a `- Cases:` line citing its census row, and a member landing on a family page lands its census/ledger row here in the same edit (the named loss of the single-file reading, closed by this pairing).
- Exemption: `CoverageSample` is the one `[Union]` that never crosses — a transient read result never seated on a node and never in `CanonicalBytes` — so the mirror closes at seven families.
- Entry: `Encode(ElementGraph, scope)` mints every node address under `graph.Header.Tolerance` before applying scoped redaction.
- Entry: `Encode(node, tolerance)` lowers ONE node under an explicit tolerance — the leg whose exact `NodeWire` ProtoJSON the persistence merge diffs and patches.
- Entry: `Encode(GraphDelta, basis)` mints added and revised addresses from the explicit active header basis.
- Entry: A delta reheader supplies revision tolerance; the basis supplies each revised node's before-address tolerance.
- Entry: `DecodeGraph` and `DecodeDelta` re-admit values, structure, and carried-address verification on `Fin<T>`.
- Auto: `WireCodec` combines Mapperly's explicit member diagnostics with generated union/protobuf case dispatch. Decode re-mints a `MeasureValue` through `OfSi`, re-admits its `MeasureBand`, re-admits material-usage direction/cardinal tokens, and recursively re-admits every `PropertyValue`; no generated-code `Get` throw is part of the boundary contract. `ToPropertySet` keeps TWELVE per-case bodies where every sibling decode collapses to a row table, because the arms share no generative structure to derive from: each names a distinct wire message, a distinct factory arity, and a distinct accumulating slot set, so a case-keyed row table carries the same twelve bodies behind twelve distinct closure types and trades the generated `PropertySetOneofCase` switch's compile-time exhaustiveness for a lookup miss — the switch IS the table here.
- Receipt: `ElementGraphWire` carries each node's authoritative address beside its payload and active header.
- Receipt: `GraphDeltaWire` carries authoritative addresses for added nodes and both sides of every revision.
- Receipt: Verified decode checks carried addresses outside `redaction.unstable_node_ids` and rails drift as `AddressUnstable`.
- Packages: Google.Protobuf (`IMessage<T>`/`MessageParser<T>`/`CodedInputStream.CreateWithLimits`/`ByteString`/`RepeatedField<T>`/`MessageExtensions` write family), Grpc.Tools (the `<Protobuf>` MSBuild item, `GrpcServices=None`, `PrivateAssets=all` — build-only, never a runtime surface), Riok.Mapperly (`[Mapper]`/`[UserMapping]`/`[MappingTarget]`/`[MapProperty]`/`[MapperIgnoreSource]`/`[UseStaticMapper]` and `MappingConversionType` policy over the Thinktecture `Create`/`Value` key codecs), NodaTime.Serialization.Protobuf (`NodaExtensions`/`ProtobufExtensions` registered WHOLESALE through `[UseStaticMapper]`, so `ToTimestamp`/`ToInstant`/`ToProtobufDuration`/`ToNodaDuration` cross with no per-member row), LanguageExt.Core (`Fin`/`Seq`/`Option` and the accumulating `Traverse` over `Validation<Error,_>` the admission folds collapse to `Fin` at their gate), Thinktecture.Runtime.Extensions (the generated total `Switch` encode dispatch).
- Auto: `WireLimits.Default` carries its two budgets as DECLARED POLICY VALUES, each naming the axis it bounds — the size ceiling bounds the WHOLE-SNAPSHOT transfer axis (an `ElementGraph` crosses in one message; there is no chunked graph transport), the recursion ceiling the one recursive family (`PropertyValue.List`/`Table`; every other message is flat) and it sits under protobuf's own default recursion limit of 100 so the seam refuses before the parser does; both are tuned numbers a deployment re-declares through `Of`, and a payload past either is refused by policy rather than by construction.
- Growth: a new union case is one corpus `oneof` arm at the ledger's next free number, one roster-and-census edit here, and one `WireCodec` case mapping — the parity census refuses a half-landed pair; a new payload column is one append-only numbered field; a new peer runtime is one codegen lane over the same `.proto`; a new decode budget is one `WireLimits` column carrying the axis it bounds.
- Boundary: Peers retain `NodeWire.content_address`; they never re-mint it from a decoded payload.
- Boundary: `content_address` is the 16-byte big-endian `ContentAddress.Of(node, activeTolerance)` value.
- Boundary: A manifest-listed unstable node retains the source address as evidence but cannot use it for edit OCC.
- Boundary: Recursive values parse under `WireLimits`, decoded values re-cross owner gates, and descriptors evolve append-only.

```proto signature
// Header of the corpus-homed graph contract file `rasm/element/v1/element.proto` — the rasm.element.v1 descriptor
// source buf gates and all three runtimes compile off the one corpus root. Package and namespace are declared HERE
// and nowhere else: the package qualifies every FullName a peer parser selects on, and the namespace DERIVES from
// it — rasm.<family>.v1 stamps Rasm.<Family>. The message roster, its field numbers, its presence marks, and its
// comment law live in that file alone; this page carries the header, the union mirror, and the frozen-number ledger.
syntax = "proto3";

package rasm.element.v1;

option csharp_namespace = "Rasm.Element";
```

Every closed seam union crosses as one `oneof` mirroring its cases 1:1, and the mirror roster's two ordinal columns
are INDEPENDENT: the canonical ordinal is the owner's `CanonicalBytes` content-key tag (next free at the owner, never
a renumber) and the proto field number appends in its own oneof — `MaterialUsage` and `MaterialPropertySet` prove no
function maps one onto the other, so a derivation computing `field = ordinal + k` silently re-tags live arms.

| [INDEX] | [FAMILY]              | [WIRE_ONEOF]                           | [ARMS] | [CANON_ORDINALS] | [PROTO_FIELDS] |
| :-----: | :-------------------- | :------------------------------------- | -----: | :--------------- | :------------- |
|  [01]   | `Node`                | `NodeWire.payload`                     |      8 | `0..7`           | `2..9`         |
|  [02]   | `Relationship`        | `RelationshipWire.edge`                |      6 | `0..5`           | `1..6`         |
|  [03]   | `PropertyValue`       | `PropertyValueWire.value`              |     14 | `0..13`          | `1..14`        |
|  [04]   | `TemporalValue`       | `TemporalWire.value`                   |      5 | `0..4`           | `1..5`         |
|  [05]   | `MaterialUsage`       | `MaterialUsageWire.usage`              |      3 | `0, 1, 2`        | `3, 1, 2`      |
|  [06]   | `MaterialComposition` | `MaterialCompositionWire.composition`  |      4 | `0..3`           | `1..4`         |
|  [07]   | `MaterialPropertySet` | `MaterialPropertySetWire.property_set` |     12 | `0..11`          | `2..13`        |
|  [08]   | `CoverageSample`      | (none)                                 |      2 | —                | —              |

[SEAM_OWNER]:
- `Node`: `Graph/element#NODE_MODEL`
- `Relationship`: `Relations/relation#EDGE_ALGEBRA`
- `PropertyValue`: `Properties/property#PROPERTY_VALUE`
- `TemporalValue`: `Properties/property#PROPERTY_VALUE`
- `MaterialUsage`: `Relations/relation#EDGE_ALGEBRA`
- `MaterialComposition`: `Composition/material#MATERIAL_COMPOSITION`
- `MaterialPropertySet`: `Composition/material#MATERIAL_PROPERTY`
- `CoverageSample`: `Geospatial/coverage#COVERAGE_NODE`

- [03]-[PROPERTY_VALUE]: `PropertyValue` is the ONE recursive family, bounded by `WireLimits`, never a seam re-check.
- [05]-[MATERIAL_USAGE]: proto fields are PERMUTED against the canon ordinals — `None` is the explicit `google.protobuf.Empty` arm appended at 3, and an unset `usage` oneof is malformed foreign input.
- [07]-[MATERIAL_PROPERTY_SET]: proto fields are PERMUTED against the canon ordinals — `Orthotropic` is canon 6, field 3.
- [08]-[COVERAGE_SAMPLE]: EXEMPT as a transient read result — never seated on a node, never in `CanonicalBytes`, never crossing the wire.
- Envelope columns ride the frozen-number ledger below; a family absent from that ledger brackets no oneof.

The frozen-number ledger holds exactly what no derivation reaches — reserved numbers, envelope brackets, positional
arities, integer discriminants, and per-owner exceptions. Retired NAMES ride this ledger as data: the corpus file
reserves numbers alone, and adding name reservations there would move the frozen descriptor digest.

| [INDEX] | [SITE]                      | [FROZEN_FACT]                                                                                            |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `ProfileSetWire`            | `reserved 1, 2`                                                                                          |
|  [02]   | `CoverageWire`              | `reserved 3, 9`                                                                                          |
|  [03]   | `CoverageBandWire`          | `reserved 3`; `sample_type = 12`                                                                         |
|  [04]   | `OverviewLevelWire`         | `reserved 1, 2, 3`                                                                                       |
|  [05]   | `NodeWire`                  | `id = 1`, payload `2..9`, `content_address = 10`                                                         |
|  [06]   | `MaterialPropertySetWire`   | `evidence = 1`, arms `2..13`                                                                             |
|  [07]   | `AcousticWire` fields 1-2   | arity = the `AcousticBand` roster count                                                                  |
|  [08]   | `EnvironmentalWire.impacts` | arity = `ImpactCategory` × `LifecycleStage`, ROW-MAJOR                                                   |
|  [09]   | `CellLatticeWire.affine`    | exactly 12                                                                                               |
|  [10]   | `SectionPropertiesWire`     | 19 positional measure columns + `monosymmetry_factor = 20`                                               |
|  [11]   | integer discriminants       | `sint32` default; `int32` on both `priority`, `uint32` on `ColorBinWire` channels, `sint64` on `ceiling` |
|  [12]   | `canonical_unit`            | ABSENT on `MeasureValueWire`; `= 11` on `ObservationWire`                                                |
|  [13]   | `VerticalCrs`               | flattened — `vertical_datum = 10` + `vertical_epsg = 13` on `GeoReferenceWire`                           |
|  [14]   | explicit-presence flips     | `FireWire.reaction = 1`, `EnvironmentalWire.recycled_content = 3`, `.end_of_life_recovery = 4`           |
|  [15]   | `SeriesStatisticsWire`      | moment group appended `7..12`; presence on `stat_mass = 9` gates the whole group                         |
|  [16]   | `PropertyEvidenceWire`      | `grade = 4` (the `SourceRank` int vocabulary), `attested = 5`, `run = 6` (rides `ProvenanceWire`)        |
|  [17]   | `HeaderWire` unit scheme    | `unit_scheme = 7` the Overrides map; `axes = 8` + `culture = 9` + `format = 10` appended (S-E2)          |

- [01]-[PROFILE_SET]: retired primary-material and primary-profile scalars — the seam derives both off row zero, and a primary scalar beside it double-stores what row zero already carries.
- [02]-[COVERAGE]: retired six-coefficient grid descriptor — placement is the kernel lattice, crossing whole on `grid = 10`.
- [03]-[COVERAGE_BAND]: retired string raster-sample-type key; the SAME column name relanded as the kernel `ChannelDtype` `sint32` key PAST `palette = 11`, so declaration order and number order disagree here and nowhere else — the exemplar of append-past-the-burn.
- [04]-[OVERVIEW_LEVEL]: retired `width`/`height`/`cell_size` — each level carries its own lattice.
- [05]-[NODE]: `NodeWire` brackets the oneof, so a ninth payload case takes 11.
- [06]-[MATERIAL_PROPERTY_SET]: `MaterialPropertySetWire` seats `evidence` before the oneof, so a thirteenth property case takes 14.
- [07]-[ACOUSTIC]: both spectra are POSITIONAL over the band roster: a band row widens both runs with no wire declaration moving, and `Acoustic.Of` is the only arity gate.
- [08]-[IMPACTS]: `LifecycleStage` is the STRIDE — an impact row appends index-stably while a stage row RE-STRIDES every stored cell: the one coupling where vocabulary growth is a wire break wearing a data edit.
- [09]-[AFFINE]: row-major 3×4 — the fourth matrix row is the invariant `[0 0 0 1]`; `ToLattice` is the ONE wire-side arity gate, because a repeated field carries no fixed length.
- [10]-[SECTION_PROPERTIES]: `SectionColumns` at decode is the frozen order's second authority, and the two move as one.
- [11]-[INTEGER_DISCRIMINANT]: every deviation is a FROZEN wire fact recorded so a new integer column defaults to `sint32` and a deviation stays a named row, never a precedent — display channels range-gate at decode and `ceiling` is the one 64-bit census budget.
- [12]-[CANONICAL_UNIT]: `canonical_unit` keys per-OWNER, never per-type: measure decode re-mints the unit through `OfSi` so wire and canon agree by construction, while the series mints through `Rehydrate`'s trusted re-mint and carries the token verbatim — a type-keyed derivation drops the observation column and kills the sample lift.
- [13]-[VERTICAL_CRS]: `VerticalCrs` flattens its two-column identity onto the parent it re-admits through one `Admit`; `ProjectedCrs` takes its own message (`crs = 11`) because it owns derived peer-informative columns (`epsg`, `resolution`).
- [14]-[PRESENCE]: explicit presence keeps absence from aliasing a zero or a blank key — the one-time flip is absorbed into the baseline and `buf.yaml` carries no waiver for it; an `optional` mark on a MESSAGE column changes no presence semantics yet still mints a synthetic `_<field>` oneof the FILE gate never reports, so the frozen digest is that move's only witness.
- [15]-[STATISTICS_MOMENTS]: min/max/mean scalars alone cannot re-found variance/skewness/kurtosis, so the kernel `Stat` moment columns cross append-only and an elder payload without them decodes a figure-less summary — never a fabricated moment set.
- [16]-[EVIDENCE_GRADE]: grade crosses as the SAME int vocabulary `PropertySetWire.source_rank` already carries; an elder payload without it decodes `Catalogue` (the roster's floor and the owner's own defaulted-struct state), never a guessed rank; the `run` column reuses `ProvenanceWire` whole because the `EvidenceRun` columns ARE its columns.
- [17]-[UNIT_AXES]: `UnitScheme`'s S-E2 widening crosses append-only — the elder `unit_scheme` map stays the Overrides face and an empty axes run reads as SI, so no landed header re-encodes.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Numerics;
using System.Globalization;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Compliance.Classification;
using NodaTime.Serialization.Protobuf;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Riok.Mapperly.Abstractions;
using Band = Rasm.Numerics.Band;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;
using static Rasm.Element.Graph.SeamConverters;
// The kernel lattice-axis count and the seam's physical 7-vector both spell Dimension; the alias names the kernel
// reading so the enclosing-namespace Dimension stays the bare SI signature. The two other kernel Numerics types this
// page composes ride TYPE aliases for the same reason — a plain `using Rasm.Numerics;` would make every bare
// `Dimension` spelling ambiguous (CS0104) against the Properties one.
using CellLattice = Rasm.Numerics.CellLattice;
using LatticeAxis = Rasm.Numerics.Dimension;
using PerceptualColor = Rasm.Numerics.PerceptualColor;

namespace Rasm.Element.Graph;

// Csproj codegen item this contract realizes; ProtoRoot pins the corpus root so the descriptor name reads
// rasm/element/v1/element.proto, the one spelling the frozen snapshot and both peer minters carry:
//   <Protobuf Include="../../../tests/contracts/rasm/element/v1/element.proto"
//             ProtoRoot="../../../tests/contracts" GrpcServices="None" />

// --- [TYPES] ------------------------------------------------------------------------------
// The union-arity OWNER: one row per crossing family carrying the seam arm count AND the generated oneof-enum
// read, so the [02] roster table, the WireCodec parity fold, and the Graph/corpus forge's family-width arithmetic
// all read ONE declaration (the forge's hand consts and the ctor's tuple census were two spellings of this
// invariant). WireArms excludes the generated None member.
[SmartEnum<string>]
public sealed partial class CrossingFamily {
 public static readonly CrossingFamily Node = new("node", 8, static () => Arms<NodeWire.PayloadOneofCase>());
 public static readonly CrossingFamily Relationship = new("relationship", 6, static () => Arms<RelationshipWire.EdgeOneofCase>());
 public static readonly CrossingFamily PropertyValue = new("property-value", 14, static () => Arms<PropertyValueWire.ValueOneofCase>());
 public static readonly CrossingFamily TemporalValue = new("temporal-value", 5, static () => Arms<TemporalWire.ValueOneofCase>());
 public static readonly CrossingFamily MaterialUsage = new("material-usage", 3, static () => Arms<MaterialUsageWire.UsageOneofCase>());
 public static readonly CrossingFamily MaterialComposition = new("material-composition", 4, static () => Arms<MaterialCompositionWire.CompositionOneofCase>());
 public static readonly CrossingFamily MaterialPropertySet = new("material-property-set", 12, static () => Arms<MaterialPropertySetWire.PropertySetOneofCase>());

 public int Arms { get; }

 [UseDelegateFromConstructor]
 public partial int WireArms();

 static int Arms<T>() where T : struct, Enum => Enum.GetValues<T>().Length - 1;
}

// --- [MODELS] -----------------------------------------------------------------------------
// WireLimits owns size, recursion, and address-verification policy. Parse calls contain no budget literal: both
// defaults are DECLARED POLICY VALUES naming the axis they bound, and a deployment re-declares either through Of.
public sealed record WireLimits {
 // Bounds the WHOLE-SNAPSHOT transfer axis: an ElementGraph crosses in ONE message (there is no chunked graph
 // transport), so the budget clears the largest snapshot the seam admits and refuses beyond it.
 private const int SnapshotSizeCeiling = 512 << 20;

 // Bounds the ONE recursive family — PropertyValue.List/Table nest, every other message is flat — with headroom for
 // the envelope frames above it, and sits UNDER protobuf's own default recursion limit of 100 so a hostile nesting
 // depth is refused by this seam's declared budget rather than by the parser's.
 private const int NestedValueDepthCeiling = 96;

 private WireLimits(int sizeLimit, int recursionLimit, bool verifyAddresses) =>
  (SizeLimit, RecursionLimit, VerifyAddresses) = (sizeLimit, recursionLimit, verifyAddresses);

 public int SizeLimit { get; }
 public int RecursionLimit { get; }
 internal bool VerifyAddresses { get; init; }

 public static readonly WireLimits Default = new(SnapshotSizeCeiling, NestedValueDepthCeiling, verifyAddresses: false);
 public static readonly WireLimits Verified = Default.WithAddressVerification();

 public static Fin<WireLimits> Of(int sizeLimit, int recursionLimit, Op key) =>
  (In(sizeLimit, Band.Positive, "wire-size-limit", key),
   In(recursionLimit, Band.Positive, "wire-recursion-limit", key))
  .Apply((size, depth) => new WireLimits((int)size, (int)depth, verifyAddresses: false))
  .As().ToFin();

 public WireLimits WithAddressVerification() => this with { VerifyAddresses = true };
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Mapperly transcription family: source-generated per-case field mapping, key codecs hand-owned as
// [UserMapping] statics so identity NEVER re-derives — Mapperly transcribes shape, the seam owns identity.
// Encode case dispatch is the union's generated total Switch; decode dispatch is the generated PayloadCase/
// ValueCase closed enum ([MapDerivedType] is the class-hierarchy rail; a oneof envelope has no case base).
// RequiredMappingStrategy.Both proves BOTH sides complete — but source-side completeness is compiler-proved only while
// no [MapPropertyFromSource] reader lands here: one whole-source reader suppresses RMG020 for EVERY source member of
// that mapping, touched or not, so a reader-bearing mapping demotes its [MapperIgnoreSource] roster from compiler proof
// to authored inventory. Target-side RMG012 is unaffected.
// The two NodaTime.Serialization.Protobuf static mappers register the whole ToTimestamp/ToInstant/ToProtobufDuration/
// ToNodaDuration/ToDate/ToLocalDate family, so every plain temporal crossing generates with NO per-member codec row;
// the hand bodies below keep their explicit calls because each encodes a CHOICE — an Interval flattened to a bounded
// column pair, an Option presence write, an ISO pattern the wire fixes — not a plain conversion.
// EnabledConversions EXCLUDES ExplicitCast as a LOAD-BEARING guard, never hygiene: LanguageExt defines a THROWING
// explicit Option<T> -> T cast that the default conversion set binds and prefers OVER a registered user mapping,
// so the narrowed set is the one thing keeping an absent Option from throwing inside a generated body.
[Mapper(
 EnabledConversions = MappingConversionType.Constructor | MappingConversionType.ImplicitCast | MappingConversionType.Enumerable | MappingConversionType.Dictionary,
 RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(SeamConverters))]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.NodaExtensions))]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.ProtobufExtensions))]
// --- [SERVICES] ---------------------------------------------------------------------------
// The S-E7 peer-reachable converter set: the identity key codecs, the cross-family Option carriers, and the
// MeasureValue/MeasureBand encode legs every peer [Mapper] would otherwise re-spell. WireCodec composes it via
// [UseStaticMapper] and each wire page reaches it `using static`; W3 Materials/Bim mappers add
// [UseStaticMapper(typeof(SeamConverters))] and delete their copies — the name `WireCodec` stays ONE declaration
// corpus-wide (their same-named classes rename).
public static partial class SeamConverters {
 // --- [KEY_CODECS] — verbatim crossings, never re-minted
 [UserMapping] public static string ToWire(NodeId id) => id.Value;
 [UserMapping] public static NodeId ToNodeId(string wire) => NodeId.Create(wire);
 [UserMapping] public static string ToWire(MaterialId id) => id.Value;
 [UserMapping] public static ByteString ToWire(UInt128 key) {
  Span<byte> be = stackalloc byte[16];
  BinaryPrimitives.WriteUInt128BigEndian(be, key);           // the persisted XxHash128 canonical form
  return ByteString.CopyFrom(be);
 }

 [UserMapping] public static UInt128 ToKey(ByteString wire) => BinaryPrimitives.ReadUInt128BigEndian(wire.Span);

 // --- [CARRIER_CODECS] — the Option crossings Mapperly bridges are MESSAGE-shaped alone: a protoc message property
 // admits null as unset, so a nullable return that skips assignment IS the absence write. The scalar/string shape
 // owns NO carrier here by law — protoc's Has*/Clear* pattern sits behind a null-rejecting setter, so a nullable
 // return cannot express its absence and every such column writes presence by hand ([PRESENCE_SHELLS] above).
 // Each carrier keeps its per-T Match body: the struct-element rows would read default(T) off ValueUnsafe, and the
 // projections differ per element, so a generic carrier has nothing lawful to collapse onto.
 // Absent measured columns (the product-only U-value) leave their optional message unset, never a zero-SI measure.
 [UserMapping] public static MeasureValueWire? ToWire(Option<MeasureValue> value) => value.Match<MeasureValueWire?>(static m => ToWire(m), static () => null);
 // Absent sampled curves (an undeclared reduction, λ(θ), or hygrothermal table) leave their optional message
 // unset, never an empty run the arity gate would refuse at the far end.
 [UserMapping] public static SampledCurveWire? ToWire(Option<SampledCurve> curve) => curve.Match<SampledCurveWire?>(static c => WireCodec.ToWire(c), static () => null);
 // The absence carrier over the registered NodaExtensions conversion — the presence decision is this seam's, the
 // conversion the static mapper's, so an absent instant leaves its proto3 optional unset.
 [UserMapping] public static Timestamp? ToWire(Option<NodaTime.Instant> at) => at.Match<Timestamp?>(static i => i.ToTimestamp(), static () => null);

 // MeasureValue crosses as its identity columns; the wire NEVER carries CanonicalUnit — decode re-mints it
 // through the OfSi registry resolve and re-attaches the band, so wire and canon agree by construction.
 // Encode stays the total [UserMapping]; DECODE is Fin — the OfSi finite gate below.
 [UserMapping] public static MeasureValueWire ToWire(MeasureValue m) {
  MeasureValueWire w = new() {
   QuantityType = m.Type.Value, Si = m.Si,
   DimLength = m.Dimension.Length, DimMass = m.Dimension.Mass, DimTime = m.Dimension.Time,
   DimCurrent = m.Dimension.Current, DimTemperature = m.Dimension.Temperature,
   DimAmount = m.Dimension.Amount, DimLuminousIntensity = m.Dimension.LuminousIntensity,
  };
  m.Uncertainty.IfSome(b => w.Uncertainty = ToWire(b));
  return w;
 }

 // Fin-railed decode re-enters generated type admission and the owner's SI gate under the wire operation key.
 // The null arm is the FAMILY-WIDE Present gate: every required measure column on every arm funnels through this
 // one member, so an unset hostile message names itself on the rail instead of dereferencing in the funnel.
 internal static Fin<MeasureValue> ToMeasure(MeasureValueWire? w, Op key) =>
  w is null
   ? new KernelFault.InvalidValue("element-wire.measure", "required message is absent", Some(key))
   : from type in key.AcceptValidated<QuantityType>(w.QuantityType)
     from measure in MeasureValue.OfSi(
      type,
      Dimension.Create(w.DimLength, w.DimMass, w.DimTime, w.DimCurrent, w.DimTemperature, w.DimAmount, w.DimLuminousIntensity),
      w.Si,
      key: key)
     from admitted in w.Uncertainty is null
      ? Fin.Succ(measure)
      : ToBand(w.Uncertainty, key).Bind(band => measure.WithUncertainty(band, key))
     select admitted;

 [UserMapping] public static MeasureBandWire ToWire(MeasureBand band) {
  MeasureBandWire w = new() { Kind = band.Kind.Key, LowerSi = band.LowerSi, UpperSi = band.UpperSi };
  band.StandardDeviationSi.IfSome(sd => w.StandardDeviationSi = sd); band.CoverageFactor.IfSome(k => w.CoverageFactor = k); return w;
 }
}

internal static partial class WireCodec {
 // --- [UNION_PARITY] — one fold over the CrossingFamily roster: a seam case or a corpus oneof arm landing without
 // its counterpart throws at first codec touch instead of skewing silently. The ROWS own the arm counts; this ctor
 // owns only the fold, and the corpus forge reads the same rows (its four hand consts died with the second census).
 static WireCodec() {
  foreach (CrossingFamily family in CrossingFamily.Items) {
   if (family.Arms != family.WireArms()) {
    throw new InvalidOperationException($"<wire-union-parity:{family.Key}:{family.Arms}:{family.WireArms()}>");
   }
  }
 }

 static Fin<T> Iso<T>(NodaTime.Text.IPattern<T> pattern, string token, Op key) =>
  pattern.Parse(token) is { Success: true } parsed
   ? Fin.Succ(parsed.Value)
   : new KernelFault.InvalidValue("element-wire.temporal", $"parse {token}", Some(key));

 // ONE half-open gate for every paired presence flag — a window missing one end, a range missing one bound.
 static Fin<Unit> BothOrNeither(bool left, bool right, string column, Op key) =>
  left == right ? Fin.Succ(unit) : new KernelFault.InvalidValue($"element-wire.{column}", "carry both presence columns or neither", Some(key));

 static Fin<Option<NodaTime.LocalDate>> ToDate(bool present, string iso, Op key) =>
  Opt(present, iso).Traverse(token => Iso(NodaTime.Text.LocalDatePattern.Iso, token, key)).As();

 // Proto3 carries MESSAGE presence as nullness, so a column the schema declares non-optional still arrives unset
 // from a hostile producer and the residual funnel would report its dereference as an opaque throw. Present names
 // the missing column on the rail instead, and ToInterval pairs it with the ORDER proof the flattened window needs:
 // the NodaTime two-Instant constructor throws on a reversed pair and would fire before any seam gate reads it.
 static Fin<T> Present<T>(T? w, string column, Op key) where T : class =>
  w is not null ? Fin.Succ(w) : new KernelFault.InvalidValue($"element-wire.{column}", "required message is absent", Some(key));

 static Fin<NodaTime.Interval> ToInterval(
  Google.Protobuf.WellKnownTypes.Timestamp? start, Google.Protobuf.WellKnownTypes.Timestamp? end, string column, Op key) =>
  from opened in Present(start, $"{column}.start", key)
  from closed in Present(end, $"{column}.end", key)
  from window in opened.ToInstant() <= closed.ToInstant()
   ? Fin.Succ(new NodaTime.Interval(opened.ToInstant(), closed.ToInstant()))
   : new KernelFault.InvalidValue($"element-wire.{column}", "window start must not follow its end", Some(key))
  select window;

 // Absence is total through the Option traversal — None yields the rail's own Pure, so no Match arm pair rides
 // mid-pipeline and the presence decision is one lift, never a hand branch per site.
 static Fin<Option<MeasureValue>> OptMeasure(MeasureValueWire? w, Op key) =>
  Optional(w).Traverse(m => ToMeasure(m, key)).As();

 static Fin<Option<SampledCurve>> OptCurve(SampledCurveWire? w, Op key) =>
  Optional(w).Traverse(c => SampledCurve.Of(c.Axis.ToArray(), c.Values.ToArray(), key)).As();

 // ONE presence lift for every generated Has*/value pair — the element type rides the value, never a per-type twin.
 static Option<T> Opt<T>(bool present, T value) => present ? Some(value) : None;

 // Wire map keys are ORDINAL-distinct by protobuf's own parse, but PropertyName narrows to ordinal-ignore-case, so
 // two legal wire keys ("Length" beside "length") collide at admission — a REAL key-space narrowing railed typed
 // here rather than surfacing as an opaque residual throw the funnel re-labels.
 static Fin<Map<PropertyName, T>> Named<T>(Seq<(PropertyName Name, T Value)> pairs, Op key) =>
  pairs.Fold(Fin.Succ(Map<PropertyName, T>()), (acc, pair) => acc.Bind(m => m.ContainsKey(pair.Name)
   ? new KernelFault.InvalidValue(
      "element-wire.property-name", $"remain unique after ordinal-ignore-case admission; duplicate {pair.Name.Value}", Some(key))
   : Fin.Succ(m.Add(pair.Name, pair.Value))));

}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// ElementWire boundary: infallible Encode and Fin-railed Decode with one typed leg per wire kind.
// Wire messages are the byte surface; consumers compose the
// Google.Protobuf write family (WriteTo(IBufferWriter<byte>) / ToByteArray / WriteDelimitedTo) on the returned
// envelope directly — a forwarding byte wrapper here is the deleted form.
public static class ElementWire {
 // ONE graph encode carries the egress policy: an absent scope folds through RedactionScope.None, whose empty column
 // roster is the identity of Apply, so the unredacted and the scoped crossing run the same path and no sibling
 // EncodeRedacted forks it. Clearing runs on the JUST-ENCODED message, never on a caller's value.
 public static ElementGraphWire Encode(ElementGraph graph, Option<RedactionScope> scope = default) {
  ElementGraphWire wire = new() { Header = WireCodec.ToWire(graph.Header) };
  // FrozenDictionary declares NO enumeration order, so the node run EMITS NodeId-ordinal — the one order every
  // peer reproduces — while edges publish recording order by Graph/delta#GRAPH_DELTA law. The fill foreach is the
  // protobuf boundary's own mutable shape (the statement exemption), skipping the whole-graph intermediate Seq an
  // AddRange over a mapped run would materialize.
  foreach (Node node in graph.Nodes.Values.OrderBy(static n => n.Id.Value, StringComparer.Ordinal)) {
   wire.Nodes.Add(WireCodec.ToWire(node, graph.Header.Tolerance));
  }
  wire.Edges.AddRange(graph.Edges.Select(WireCodec.ToWire));
  return scope.IfNone(RedactionScope.None).Apply(wire);
 }

 public static NodeWire Encode(Node node, double tolerance) => WireCodec.ToWire(node, tolerance);

 public static GraphDeltaWire Encode(GraphDelta delta, Header basis) {
  Header revision = delta.Header.IfNone(basis);
  GraphDeltaWire wire = new();
  // The five sections publish RECORDING order (Graph/delta#GRAPH_DELTA law); the span foreach fills each
  // whole-graph run without materializing the intermediate Seq a mapped AddRange would build per section.
  foreach (Node node in delta.AddedNodes.AsSpan()) { wire.AddedNodes.Add(WireCodec.ToWire(node, revision.Tolerance)); }
  foreach (NodeId id in delta.RemovedNodes.AsSpan()) { wire.RemovedNodeIds.Add(id.Value); }
  foreach ((Node before, Node after) in delta.RevisedNodes.AsSpan()) {
   wire.RevisedNodes.Add(new NodeRevisionWire {
    Before = WireCodec.ToWire(before, basis.Tolerance),
    After = WireCodec.ToWire(after, revision.Tolerance),
   });
  }
  foreach (Relationship edge in delta.AddedEdges.AsSpan()) { wire.AddedEdges.Add(WireCodec.ToWire(edge)); }
  foreach (Relationship edge in delta.RemovedEdges.AsSpan()) { wire.RemovedEdges.Add(WireCodec.ToWire(edge)); }
  delta.Header.IfSome(h => wire.Header = WireCodec.ToWire(h));
  return wire;
 }

 // Parse under the explicit-limits reader (the ONE hostile-payload depth/size gate), re-admit every node, edge,
 // and header VALUE through the seam gates, then route the whole transcription through the graph's own STRUCTURAL
 // admission: the decoded snapshot enters as a Genesis-rooted GraphDelta through AdmitOnto, so LegalLink runs per
 // decoded edge — an absent endpoint rails NodeAbsent, an illegal endpoint-kind pair RelationshipInvalid, a
 // duplicate link DeltaConflict — exactly as the in-process Link path; a decoder-trusted ElementGraph.Of over
 // foreign edges is the deleted form (the wire is not a validated producer). Then optionally sweep the address
 // complement of the crossing's redaction manifest. The protobuf parse fault is a BOUNDARY exception
 // (InvalidProtocolBufferException) crosses the kernel capture funnel with its original cause.
 //
 // The duplicate-id gate runs FIRST, over the RAW wire ids: two same-id wire nodes coalesce silently through the
 // PutNode upsert, so the conflict must rail before anything trusts the transcription — and a string scan is one
 // pass over the cheapest column on the message, where gating after ToNode pays every payload admission on a
 // hostile duplicate-stuffed graph before rejecting it.
 //
 // Node and edge admission ACCUMULATE: the two runs are independent of each other and each element is independent
 // within its run, so the applicative Traverse over Validation reports every malformed node AND every malformed edge
 // of a hostile payload in ONE failure, collapsing to Fin once at the structural gate — a first-failure TraverseM
 // would make a four-hundred-defect payload a four-hundred-round conversation.
 public static Fin<ElementGraph> DecodeGraph(Stream payload, WireLimits limits, Op key) =>
  Parse(ElementGraphWire.Parser, payload, limits, key).Bind(wire => Funnel(key, () =>
   toSeq(wire.Nodes).Map(static n => n.Id).Distinct().Count != wire.Nodes.Count
    ? new ElementFault.DeltaConflict(key, "<wire-node-duplicate-id>")
    : WireCodec.ToHeader(wire.Header, key).Bind(header =>
       (toSeq(wire.Nodes).Traverse(n => AdmitNode(
          n, header.Tolerance,
          limits.VerifyAddresses && (wire.Redaction is null || !wire.Redaction.UnstableNodeIds.Contains(n.Id)), key)
         .ToValidation()).As(),
        toSeq(wire.Edges).Traverse(e => WireCodec.ToEdge(e, key).ToValidation()).As())
        .Apply(static (nodes, edges) => (Nodes: nodes, Edges: edges)).As().ToFin()
        .Bind(admitted =>
         admitted.Edges.Fold(admitted.Nodes.Fold(GraphDelta.Empty.Reheader(header), static (delta, node) => delta.Put(node)), static (delta, edge) => delta.Link(edge))
          .AdmitOnto(ElementGraph.Genesis(header), key)
          .Map(static step => step.Graph)))));

 // Verified decode checks the authoritative carried address and the node's own content-derived identity. A redaction
 // manifest suppresses this leg for the ids it declares unstable; consumers retain that roster as OCC ineligibility.
 static Fin<Node> AdmitNode(NodeWire wire, double tolerance, bool verify, Op key) =>
  WireCodec.ToNode(wire, key).Bind(node =>
   !verify ? Fin.Succ(node)
   : wire.ContentAddress.Length != 16
    ? new KernelFault.InvalidValue("element-wire.content-address", $"carry 16 bytes for {wire.Id}; received {wire.ContentAddress.Length}", Some(key))
    : WireCodec.ToKey(wire.ContentAddress) != ContentAddress.Of(node, tolerance).Value
     ? new ElementFault.AddressUnstable(key, $"<wire-content-address-mismatch:{wire.Id}>")
     : ContentAddress.Verify(node, tolerance, key).Map(_ => node));

 // Decoded deltas re-cross the NormalForm(Op) shape gate — the unique-per-id normal form Merge produces is an
 // OBLIGATION on a foreign transcription, never assumed — and the gate's accumulated tokens name WHICH conjunct the
 // payload broke, so this boundary reports a double-entry id or edge as the owner's own denormal reason rather than
 // minting an opaque one. Its ONLY sanctioned application is AdmitOnto — ReplayOnto trusts a delta the seam's own
 // algebra produced, which a wire payload is not, so the structural edge law runs when the foreign delta lands.
 // Its four node and edge sections are independent runs over independent elements, so they admit through the SAME
 // accumulating Traverse the snapshot leg takes and join applicatively — one failure carrying every defect across all
 // four sections — before the delta shape gate runs.
 public static Fin<GraphDelta> DecodeDelta(Stream payload, Header basis, WireLimits limits, Op key) =>
  Parse(GraphDeltaWire.Parser, payload, limits, key).Bind(wire => Funnel(key, () =>
   Optional(wire.Header).Traverse(h => WireCodec.ToHeader(h, key)).As()
   .Bind(header => {
    Header revision = header.IfNone(basis);
    return (toSeq(wire.AddedNodes).Traverse(n => AdmitNode(n, revision.Tolerance, limits.VerifyAddresses, key).ToValidation()).As(),
     toSeq(wire.RevisedNodes).Traverse(r => AdmitNode(r.Before, basis.Tolerance, limits.VerifyAddresses, key)
      .Bind(b => AdmitNode(r.After, revision.Tolerance, limits.VerifyAddresses, key).Map(a => (Before: b, After: a)))
      .ToValidation()).As(),
     toSeq(wire.AddedEdges).Traverse(e => WireCodec.ToEdge(e, key).ToValidation()).As(),
     toSeq(wire.RemovedEdges).Traverse(e => WireCodec.ToEdge(e, key).ToValidation()).As())
     .Apply(static (added, revised, addedEdges, removedEdges) => (added, revised, addedEdges, removedEdges)).As().ToFin()
     .Map(sections => new GraphDelta(
      sections.added, toSeq(wire.RemovedNodeIds).Map(NodeId.Create), sections.revised,
      sections.addedEdges, sections.removedEdges, header))
     .Bind(delta => delta.NormalForm(key).ToFin().Map(_ => delta));
   })));

 // Residual-throw funnel over protobuf/generated mapping code; typed inner faults pass untouched.
 static Fin<T> Funnel<T>(Op key, Func<Fin<T>> decode) =>
  key.Catch(decode);

 static Fin<T> Parse<T>(MessageParser<T> parser, Stream payload, WireLimits limits, Op key) where T : class, IMessage<T> =>
  key.Catch(() => Fin.Succ(parser.ParseFrom(CodedInputStream.CreateWithLimits(payload, limits.SizeLimit, limits.RecursionLimit))));
}
```

## [03]-[EGRESS_REDACTION]

- Owner: `ElementClassification` the `rasm.element` taxonomy's two `DataClassification` keys; `ClassifiedColumn` the `[SmartEnum<string>]` roster — one row per classified column group, carrying its owning `MessageDescriptor`, the `FieldMask` over that owner, its `DataClassificationSet`, and its identity verdict; `RedactionScope` the `WireLimits`-sibling egress policy record.
- Cases: `Commercial` claims the `CostWire` columns, the `EnvironmentalWire` impact matrix, and `PropertyEvidenceWire` — evidence rides the `MaterialPropertySetWire` ENVELOPE, so one row reaches all twelve property cases. `Personal` claims the `ObjectWire` audit row, `StepHeaderWire.authors`/`organizations`, `ProvenanceWire.author`/`correlation`, and `SensorProvenanceWire.serial`.
- Law: identity splits the roster on the canonical-bytes preimage its OWNERS already fix, never on a re-derivation here — `Composition/material#MATERIAL_PROPERTY` `CaseBytes` folds the evidence envelope and the `Cost`/`Environmental` columns into every `Node.Material` key, so those three rows are IDENTITY-BEARING and clearing one re-keys its node; `Graph/element#NODE_MODEL` `WriteObject` excludes `OwnerHistory` and `Projection/address#CONTENT_ADDRESS` `OfGraph` excludes the `StepHeader`/`Provenance` provenance slots, so those four rows are IDENTITY-INERT and clearing one moves no key and owes no manifest row.
- Entry: `RedactionScope.Of(policy, DataClassificationSet, key)` claims every row whose own set the request contains — `DataClassificationSet` keys on WHOLE-set equality, so containment reads as the union fixing the request — and rails a blank policy or a request claiming no row; `scope.Apply(wire)` is the whole egress effect and `RedactionScope.None` its identity element.
- Auto: protobuf's own path grammar admits NO segment past a repeated field, so only the singular header spine validates root-relative against `ElementGraphWire` and every column reached through `nodes` declares its mask against its OWNING descriptor, the clearing walk carrying the traversal the mask cannot express; the walk descends singular and repeated message fields alone, which is total over the roster because a classified column is declared on a typed payload message and a map value is a generated map-entry.
- Receipt: `RedactionManifestWire` is the crossing's egress receipt — policy identity, the owner-qualified path roster, and the node ids the clearing re-keyed. A cleared column WITHOUT explicit presence (`FieldDescriptor.HasPresence` is false for every scalar in the roster) reads as its proto3 default, so the DECLARED roster — never the message — is the presence record separating a cleared column from an authored default.
- Packages: Microsoft.Extensions.Compliance.Abstractions (`DataClassification(taxonomy, value)`/`DataClassificationSet`/`Union` — the contract assembly ALONE, so this seam mints classification keys and resolves no `Redactor`), Google.Protobuf (`FieldMask.FromString<T>`/`Normalize`/`Paths`, `MessageDescriptor.Fields.InFieldNumberOrder`/`FindFieldByName`, `FieldDescriptor.IsMap`/`IsRepeated`/`FieldType`/`Accessor`, `IFieldAccessor.Clear`/`GetValue`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `Items` roster), LanguageExt.Core (`Fin`/`Seq`/`Option`).
- Growth: a new classified column is one `ClassifiedColumn` row naming its owner, its paths, its classification, and its identity verdict; a new sensitivity class is one `DataClassification` key on the taxonomy; a new policy is one `Of` call — never a per-policy scope type, never a second walk, and never a redactor token substituted for a cleared value.
- Boundary: the mechanism is PRESENCE CLEARING on the encoded message and nothing else — no `Redactor` resolves, no HMAC pseudonym crosses, and no re-derived identity space mints, so a redacted crossing PRESERVES its source content keys and a partner reference off the source model still resolves; the policy touches the wire message alone and never an `ElementGraph`, so an in-process consumer of the same graph is unaffected; and a redacted crossing is a DISTINCT byte stream from its unredacted twin, so the `Graph/corpus` parity vectors are forged unredacted and a redacted stream is never a parity input.
- Boundary: `unstable_node_ids` makes the retained source `content_address` unusable as an edit OCC precondition.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The seam's own compliance taxonomy. The two keys are minted as ordinary (taxonomy, value) pairs off the CONTRACT
// assembly, so an app-tier redactor registration and this wire policy select rows from ONE vocabulary while this page
// stays free of the redaction runtime — the classification is the SELECTOR here and the cleared field path is the effect.
public static class ElementClassification {
 public const string Taxonomy = "rasm.element";

 public static readonly DataClassification Commercial = new(Taxonomy, "commercial");
 public static readonly DataClassification Personal = new(Taxonomy, "personal");
}

// --- [TABLES] -----------------------------------------------------------------------------
// One row per classified column GROUP. Owner is the message the columns live on; Mask is the FieldMask over that
// owner, so the roster's path algebra never has to express a segment past a repeated field — the walk owns traversal
// and the mask owns selection. IdentityBearing is read off the owners' canonical-bytes preimages, never re-derived.
[SmartEnum<string>]
public sealed partial class ClassifiedColumn {
 public static readonly ClassifiedColumn Cost = new("cost",
  Owned<CostWire>(), Declared<CostWire>("basis,currency,supply_per_unit,install_per_unit,lifecycle_per_unit"),
  ElementClassification.Commercial, identityBearing: true);

 public static readonly ClassifiedColumn Environmental = new("environmental",
  Owned<EnvironmentalWire>(), Declared<EnvironmentalWire>("basis,impacts,recycled_content,end_of_life_recovery"),
  ElementClassification.Commercial, identityBearing: true);

 // Evidence rides the MaterialPropertySetWire envelope, so ONE row reaches every one of the twelve property cases.
 public static readonly ClassifiedColumn PropertyEvidence = new("property-evidence",
  Owned<PropertyEvidenceWire>(), Declared<PropertyEvidenceWire>("source,reference,valid_until"),
  ElementClassification.Commercial, identityBearing: true);

 // The audit row clears WHOLE off its owner rather than column by column: history carries explicit message presence,
 // so the cleared crossing is absence-honest with no manifest row owed.
 public static readonly ClassifiedColumn OwnerHistory = new("owner-history",
  Owned<ObjectWire>(), Declared<ObjectWire>("history"), ElementClassification.Personal, identityBearing: false);

 public static readonly ClassifiedColumn StepAuthorship = new("step-authorship",
  Owned<StepHeaderWire>(), Declared<StepHeaderWire>("authors,organizations"),
  ElementClassification.Personal, identityBearing: false);

 public static readonly ClassifiedColumn ComputeProvenance = new("compute-provenance",
  Owned<ProvenanceWire>(), Declared<ProvenanceWire>("author,correlation"),
  ElementClassification.Personal, identityBearing: false);

 public static readonly ClassifiedColumn SensorSerial = new("sensor-serial",
  Owned<SensorProvenanceWire>(), Declared<SensorProvenanceWire>("serial"),
  ElementClassification.Personal, identityBearing: false);

 public MessageDescriptor Owner { get; }
 public FieldMask Mask { get; }
 public DataClassificationSet Classes { get; }
 public bool IdentityBearing { get; }

 // Clearing IS the redaction: the mask's own path resolves its descriptor row and IFieldAccessor.Clear writes the
 // proto3 default — dropping a presence-bearing message whole, zeroing a presence-less scalar, emptying a repeated
 // run. Answering the identity verdict is what lets the sweep decide a node's manifest row without a second probe.
 // The foreach is the protobuf boundary's own mutable shape, the named statement exemption on this page.
 public bool Clear(IMessage message) {
  foreach (string path in Mask.Paths) { Owner.FindFieldByName(path).Accessor.Clear(message); }
  return IdentityBearing;
 }

 // Roster rows are declaration-total: FromString<T> parses AND proves every path against the owning message's own
 // grammar, Normalize sorts, dedupes, and prunes subpaths, so a mistyped column is a construction defect at roster
 // materialization rather than a clear that silently reaches nothing. Owned reads the descriptor off the published
 // IMessage.Descriptor reflection entry, so the roster names no generated static.
 static FieldMask Declared<T>(string paths) where T : IMessage<T> => FieldMask.FromString<T>(paths).Normalize();
 static MessageDescriptor Owned<T>() where T : IMessage<T>, new() => new T().Descriptor;
}

// --- [MODELS] -----------------------------------------------------------------------------
// The egress policy record beside WireLimits: a policy identity plus the rows one requested classification set claims.
// None carries an EMPTY roster and is the identity of Apply, so an unscoped encode and a scoped one are one code path.
public sealed record RedactionScope {
 private RedactionScope(string policy, Seq<ClassifiedColumn> columns) => (Policy, Columns) = (policy, columns);

 public string Policy { get; }
 public Seq<ClassifiedColumn> Columns { get; }

 public static readonly RedactionScope None = new("", Seq<ClassifiedColumn>());

 // A row is claimed when the request CONTAINS its classification set; DataClassificationSet keys on whole-set
 // equality, so containment reads as the union fixing the request rather than as a member scan. A policy claiming no
 // row rails instead of crossing an unredacted stream under a redaction label.
 public static Fin<RedactionScope> Of(string policy, DataClassificationSet classes, Op key) =>
  string.IsNullOrWhiteSpace(policy)
   ? new KernelFault.InvalidValue("redaction-policy", "not be blank", Some(key))
   : toSeq(ClassifiedColumn.Items).Filter(column => classes.Union(column.Classes).Equals(classes)) is { IsEmpty: false } claimed
    ? Fin.Succ(new RedactionScope(policy.Trim(), claimed))
    : new ElementFault.ValueRejected(key, $"<redaction-scope-claims-nothing:{policy}>");

 // --- [OPERATIONS]
 // The whole egress effect: clear every claimed column reachable from the encoded message, then stamp the manifest.
 // The header spine's rows are identity-inert by the roster's own law, so its sweep yields no node id; each node's
 // sweep contributes its id exactly when an identity-bearing row was reached, which is the roster the verifying
 // decode admits as declared-unstable. Message mutation is the protobuf boundary's shape — the statement exemption —
 // and the empty-roster arm returns the message untouched, which is what makes None a true identity.
 public ElementGraphWire Apply(ElementGraphWire wire) {
  if (Columns.IsEmpty) { return wire; }
  _ = Sweep(wire.Header);
  Seq<string> unstable = toSeq(wire.Nodes).Fold(Seq<string>(), (roster, node) => Sweep(node) ? roster.Add(node.Id) : roster);
  RedactionManifestWire manifest = new() { Policy = Policy };
  manifest.ClearedPaths.AddRange(Columns.Bind(column => toSeq(column.Mask.Paths).Map(path => $"{column.Owner.Name}.{path}")));
  manifest.UnstableNodeIds.AddRange(unstable);
  wire.Redaction = manifest;
  return wire;
 }

 // ONE descriptor walk per message root, ANSWERING whether the walk reached an identity-bearing row: clear what this
 // message owns, then descend. The folds are strict and every join is the NON-short-circuiting `|`, because clearing
 // is an EFFECT — `||` would skip the descent the moment a row on this message already moved identity, leaving the
 // subtree uncleared while still reporting the verdict. The one short-circuit that IS wanted is the owner match,
 // which must not clear a message the row does not own. A new classified column is one roster row, no new traversal.
 bool Sweep(IMessage message) =>
  Columns.Fold(false, (moved, column) =>
   (ReferenceEquals(column.Owner, message.Descriptor) && column.Clear(message)) | moved)
   | Nested(message).Fold(false, (moved, child) => Sweep(child) | moved);

 // Singular and repeated message fields are the whole descent: a classified column is declared on a typed payload
 // message, and a map field's value is a generated map-entry no roster row can own. An unset oneof arm reads null
 // through the accessor and drops out, so a node walks only its own case.
 static Seq<IMessage> Nested(IMessage message) =>
  toSeq(message.Descriptor.Fields.InFieldNumberOrder())
   .Filter(static field => field.FieldType is FieldType.Message && !field.IsMap)
   .Bind(field => field.Accessor.GetValue(message) switch {
    IMessage single => Seq(single),
    System.Collections.IEnumerable run => toSeq(run.Cast<IMessage>()),
    _ => Seq<IMessage>(),
   });
}
```

## [04]-[EVENT_ENVELOPE]

- Owner: `GraphEventType` the closed crossing vocabulary, each row carrying the `Rasm/Domain/event#EVENT_GRAMMAR` `EventType` its facts announce and the `EventSource` naming the producing capability; `GraphCrossing` the seam's composition of the kernel envelope owner — one mint, one Protobuf-framed encode, one decode, and the handling grade an egress scope derives.
- Entry: `GraphCrossing.Mint(crossing, subject, operation, at, body, ports, key)` composes `EventEnvelope.Mint` and returns its `Fin<CloudEvent>`; `Frame(envelope, key)` composes `EventEnvelope.Encode(EventFormat.Protobuf, …)` and `Admit(frame, key)` composes `EventEnvelope.Decode`, so the crossing owns which format it admits and the kernel owns every codec.
- Auto: `id` carries the PRODUCING RAIL's operation identity and `subject` the content key, so `(source, id)` is the uniqueness composite a dedup reads and two rails announcing one snapshot stay two events. `subject` renders through `EventKey.Render` — the kernel's ONE envelope content-key spelling — never `ContentAddress.ToValue()`, whose upper-case X32 is this seam's own protobuf and `NodeId` spelling and puts a second rendering of one key on one wire.
- Auto: `datacontenttype` DERIVES from the encoded message's own descriptor — `application/protobuf` carrying the `messageType` parameter off `IMessage.Descriptor.FullName` — so a consumer selects its parser from the attribute rather than from the topic it arrived on, and a renamed wire message moves the attribute with it. `dataschema` is the composing rail's registry binding and arrives as a value, because this seam runs no registry.
- Auto: `dataclassification` DERIVES from the egress scope through `#EGRESS_REDACTION`'s own roster — a scope claiming every `ClassifiedColumn` row grades `internal`, and every lesser scope (`RedactionScope.None` included) grades `restricted`, whose `DataGrade.Redact` column states the redaction route is still owed. A crossing therefore cannot announce a handling class its cleared-column roster contradicts.
- Receipt: the envelope IS the broker-lane metadata — the protobuf body is `Data` and the frame's `ContentType` is what a binding stamps — and a streaming consumer folds length-prefixed bodies (`MessageExtensions.WriteLengthPrefixedTo(IBufferWriter<byte>)` into a pooled sink, `WriteDelimitedTo` the stream-shaped sibling) one frame per crossing, deduped on `(source, id)`.
- Packages: Rasm (`Rasm.Domain` `EventEnvelope.Mint`/`.Encode`/`.Decode`, `EventMint`, `EventType`/`EventSource`/`EventKey`, `EventExtension`/`EventRoster`, `EventFormat.Protobuf`, `EventFrame`, `DataGrade`, `TraceCarrier`), CloudNative.CloudEvents (`CloudEvent` — the envelope value crossing this seam's signatures), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `TryGet`), LanguageExt.Core (`Fin`/`Option`/`Seq`), NodaTime (`Instant`), Google.Protobuf (`IMessage.Descriptor`, `MessageExtensions.ToByteArray`/`WriteLengthPrefixedTo`).
- Growth: a new crossing is one `GraphEventType` row carrying its own `EventType`, so a `Breaking` descriptor dial moves that row's major and old consumers keep matching their own; a new envelope dimension is one `EventExtension` row at the kernel owner and one `Extensions` entry here; a new broker lane is one binding row at its consuming owner, never a seam member.
- Boundary: the envelope carries metadata alone and the protobuf message is the body; bindings, content mode, prefixes, `dataref` residence, and delivery guarantees seat at the consuming owner. The creation-time trace arrives as a `TraceCarrier` VALUE the composing rail captured — this seam neither reads `Activity.Current` nor formats a `traceparent`, because the kernel mint owns the stamp and the propagator owns the format. `WireKind` stays the in-process decode dimension `Projection/observe` tags facts with; `GraphEventType` stays the transport crossing vocabulary.

| [INDEX] | [CROSSING] | [TYPE]                           | [SOURCE]                | [BODY]             |
| :-----: | :--------- | :------------------------------- | :---------------------- | :----------------- |
|  [01]   | `snapshot` | `rasm.element.graph.frozen.v1`   | `rasm:element/snapshot` | `ElementGraphWire` |
|  [02]   | `delta`    | `rasm.element.delta.appended.v1` | `rasm:element/delta`    | `GraphDeltaWire`   |

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Net.Mime;
using CloudNative.CloudEvents;
using Google.Protobuf;
using NodaTime;
using Rasm.Domain;

namespace Rasm.Element.Graph;

// --- [TYPES] ----------------------------------------------------------------------------------
// Closed crossing vocabulary. Each row carries the kernel `EventType` its facts announce rather than a literal
// token: `Of` assembles the four grammar segments, so a row cannot spell a type the estate grammar refuses and a
// major move is one argument on the row that owns it. `Source` names the producing CAPABILITY under the same
// grammar, so no host, deployment, or topic can enter the identity a consumer keys its subscription on.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GraphEventType {
 private const string Domain = "element";

 public static readonly GraphEventType Snapshot = new("snapshot", subject: "graph", fact: "frozen");
 public static readonly GraphEventType Delta = new("delta", subject: "delta", fact: "appended");

 private GraphEventType(string key, string subject, string fact) : this(key) =>
  (Type, Source) = (EventType.Of(Domain, subject, fact, major: 1), EventSource.Of(Domain, capability: key));

 public EventType Type { get; }

 public EventSource Source { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------------
// The composing rail's own contributions, so this seam never invents one: `Operation` is the producing rail's
// operation identity that `id` carries (a content digest there would make two rails announcing one snapshot into
// one event and drop the second), `Schema` the registry binding a serdes arrow resolved, `Trace` the creation-time
// pair the rail captured, and `Extensions` whatever rostered rows the rail adds. Every slot is a value, so this
// page reads no ambient clock, no ambient activity, and no registry.
public readonly record struct CrossingPorts(
 string Operation,
 Option<Uri> Schema,
 TraceCarrier Trace,
 Seq<(EventExtension Row, object Value)> Extensions);

// --- [OPERATIONS] -------------------------------------------------------------------------------
public static class GraphCrossing {
 // ONE mint: the kernel owner funnels construction, every rostered write, and `Validate()` through its own rail,
 // so this seam composes an admitted request and never touches a `CloudEvent` slot. `subject` and the handling
 // grade are the two rows this seam derives; every other value arrives admitted.
 public static Fin<CloudEvent> Mint(GraphEventType crossing, ContentAddress subject, Instant at,
   IMessage body, CrossingPorts ports, Op key, Option<RedactionScope> scope = default) =>
  EventEnvelope.Mint(
   new EventMint(
    Type: crossing.Type,
    Source: crossing.Source,
    Id: ports.Operation,
    Subject: Some(EventKey.Render(subject.Value)),
    Time: at,
    DataSchema: ports.Schema,
    DataContentType: Some(ContentType(body)),
    Data: body.ToByteArray(),
    Trace: ports.Trace,
    Extensions: ports.Extensions.Add((EventExtension.DataClassification, Grade(scope).Key))),
   key);

 // Structured self-contained frame: the kernel encode chooses the framing and hands back the carrier a binding
 // stamps, so a lane that carries one message whole needs no second encoder and a batch is the same call at a
 // higher arity. A binary-mode lane instead ships `Data` beside the binding's own attribute headers and reads
 // `datacontenttype` for its parser — one envelope, two placements, zero re-packs.
 public static Fin<EventFrame> Frame(Op key, params ReadOnlySpan<CloudEvent> envelopes) =>
  EventEnvelope.Encode(EventFormat.Protobuf, key, envelopes);

 public static Fin<Seq<CloudEvent>> Admit(EventFrame frame, Op key) => EventEnvelope.Decode(frame, key);

 // The handling class the crossing announces is the SCOPE's own answer: a scope claiming every classified row
 // ships a body whose commercial and personal columns are already cleared, and every lesser scope still carries
 // them, so `DataGrade.Redact` reads true exactly while the redaction route is owed. Deriving the grade forecloses
 // a crossing that labels itself clean while its manifest lists nothing.
 public static DataGrade Grade(Option<RedactionScope> scope) =>
  scope.Map(static claimed => claimed.Columns.Count == ClassifiedColumn.Items.Count).IfNone(false)
   ? DataGrade.Internal
   : DataGrade.Restricted;

 // Content type DERIVES from the message's own descriptor, so a consumer selects its parser from the attribute
 // rather than from the topic, and a renamed wire message moves the declaration with it.
 static string ContentType(IMessage body) =>
  new ContentType("application/protobuf") { Parameters = { ["messageType"] = body.Descriptor.FullName } }.ToString();
}
```

## [05]-[IMPLEMENTATION_LAW]

- [KEY_VERBATIM_LAW]: wire identities cross verbatim. `NodeId` uses X32 text; `UInt128` keys use big-endian fields while `CanonicalWriter.U128` remains little-endian hash input. Each peer normalizes once at decode and never substitutes a second digest.
- [NODE_OCC_ADDRESS]: `content_address` mints under the active header tolerance; delta encode requires its basis header.
- [CODEC_DIVISION]: `Grpc.Tools` emits messages, Mapperly emits field transcription, Thinktecture `Switch` owns seam-case encode dispatch, and protobuf case enums own decode dispatch. Reflection and parallel hand-written mappings are forbidden.
- [ADMISSION_AND_DEPTH_GATE]: `DecodeGraph` and `DecodeDelta` parse under positive `WireLimits`. Every decoded value re-crosses its owner gate before the aggregate reaches `AdmitOnto` or `NormalForm(Op)`. Duplicate node ids rail on a raw-id scan before value admission. Unset cases, unknown rows, invalid values, and illegal structure share the in-process typed rail.
- [EVENT_ENVELOPE]: `GraphCrossing` composes the kernel envelope owner whole — one mint, one Protobuf-framed encode, one decode — with `id` the composing rail's operation identity, `subject` the content key under the kernel `EventKey` spelling, `datacontenttype` derived from the body descriptor, and `dataclassification` from the egress scope; binding prefixes, content mode, and `dataref` residence own at the consuming binding; Protobuf streaming rides `WriteLengthPrefixedTo`/`WriteDelimitedTo`.
- [EGRESS_REDACTION]: `RedactionScope` clears classified field paths on the encoded message and carries its `RedactionManifestWire`. Source content keys survive — no key re-derives over redacted bytes — and the verifying decode admits exactly the manifest-named nodes as declared-unstable while a drifted node outside that roster still faults `AddressUnstable`. A redacted crossing is a DISTINCT byte stream from its unredacted twin, so parity vectors are forged and compared unredacted and a redaction policy never enters a parity gate.
- [WIRE_BYTES_LAW]: wire bytes are a TRANSPORT stream, never the content-identity law — `ContentAddress.OfGraph` and the delta's own `Address` sort their sections and are the fingerprints the corpus reproduces. The encode leg is still deterministic wherever an order exists to own: the node run emits NodeId-ordinal because `FrozenDictionary` declares no enumeration order, and edge and delta runs publish recording order. `map<>` fields keep protobuf's unspecified cross-runtime order, so a byte-parity fixture over a map-bearing message is structurally unfreezable at three peers — sorted-repeated entry runs are the escalation form the day wire bytes must become an identity, not a landed one.
- [CONTRACT_EVOLUTION]: `rasm/element/v1/element.proto` is the descriptor source and the `[02]` frozen-number ledger is its append record — a new arm takes the ledger's next free number, never a re-derived one. Appended fields and new `oneof` arms are additive; renumbers, incompatible type changes, unreserved removals, and implicit-to-explicit presence flips on landed scalars are breaking (the three landed flips ride their one documented `buf.yaml` waiver). Whole-graph parity literals remain governed by `Graph/corpus`'s terminal research route until exact addresses exist.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
