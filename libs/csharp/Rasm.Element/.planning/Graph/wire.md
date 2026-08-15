# [ELEMENT_WIRE]

`ElementWire` owns the proto-first `rasm.element.v1` graph crossing. `ElementGraphWire` and `GraphDeltaWire` mirror closed seam unions; `WireCodec` owns per-case transcription; `Encode` lowers valid values; `DecodeGraph` and `DecodeDelta` re-admit hostile input on `Fin<T>`.

Content keys cross verbatim — `NodeId` as X32 text, `UInt128` as big-endian bytes — and every `NodeWire` carries the authoritative id-inclusive address minted under the active header tolerance. Decode reuses value admissions, graphs enter through `GraphDelta.AdmitOnto`, deltas prove `IsNormalForm` first, and `WireLimits` owns parse budgets and address verification.

`RedactionScope` clears encoded fields and carries the manifest, so unstable-node addresses remain evidence yet serve no OCC. Measures carry SI magnitude, quantity token, and dimension exponents. `GraphCrossing` composes the kernel message-envelope owner and admits the Protobuf event format over the wire body.

## [01]-[INDEX]

- [02]-[WIRE_CODEC]: the corpus contract header with its union-mirror roster and frozen-number ledger, `WireCodec` Mapperly transcription and key codecs with the static-init union parity census, `ElementWire` encode/decode boundary, `WireLimits`, and the key, depth, and evolution laws.
- [03]-[EGRESS_REDACTION]: the `rasm.element` sensitivity taxonomy over the wire's classified columns, the `ClassifiedColumn` roster carrying each column group's `FieldMask` and identity verdict, and `RedactionScope` — the presence-clearing egress policy and its `RedactionManifestWire` receipt.
- [04]-[EVENT_ENVELOPE]: the `GraphEventType` closed crossing vocabulary over the kernel grammar and `GraphCrossing` — the mint composing `Rasm/Domain/event#ENVELOPE_MINT`, the Protobuf-format frame pair, the content-key `subject`, and the handling grade the egress scope derives.

## [02]-[WIRE_CODEC]

- Owner: the corpus-homed `rasm/element/v1/element.proto` `rasm.element.v1` contract — the language-neutral message roster `Grpc.Tools` compiles for C# (`GrpcServices=None`, message codegen only) and `buf`/`protoc-gen-es` + `grpcio-tools` compile for the TypeScript/Python peers, every compiler reading the one corpus root so the descriptor names this file identically at all three; `WireCodec` the `[Mapper]` static transcription family owning every per-case seam↔wire field mapping; `ElementWire` the boundary owner railing decode onto `Fin<T>`; `WireLimits` the parameterized decode-budget policy record.
- Cases: every closed seam union crosses as a `oneof` mirroring its cases 1:1 — `NodeWire` the eight `Node` payloads, `RelationshipWire` the six edge kinds, `PropertyValueWire` the recursive fourteen-case value family, `MaterialUsageWire` the explicit none/layer/profile usage family, `MaterialCompositionWire` the four composition arms, and `MaterialPropertySetWire` the engineering-property family. Generated keyed owners cross by key; absence is field presence, never a numeric or unset-oneof sentinel.
- Law: the corpus file is the ONE proto spelling — this page carries the header fence, the union-mirror roster, and the frozen-number ledger, never a second transcription of the message roster, and the `[UNION_PARITY]` census below ties each family's arm count to its generated oneof enum so a case landing on either side alone throws at first codec touch; the census and the roster move as one edit.
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

| [INDEX] | [FAMILY]              | [SEAM_OWNER]                                | [WIRE_ONEOF]                           | [ARMS] | [CANON_ORDINALS] | [PROTO_FIELDS]                                                                          | [ENVELOPE_COLUMNS]                                                                                 |
| :-----: | :-------------------- | :------------------------------------------ | :------------------------------------- | -----: | :--------------- | :--------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Node`                | `Graph/element#NODE_MODEL`                  | `NodeWire.payload`                      |      8 | `0..7`           | `2..9`                                                                                    | `id = 1`, `content_address = 10`                                                                     |
|  [02]   | `Relationship`        | `Relations/relation#EDGE_ALGEBRA`           | `RelationshipWire.edge`                 |      6 | `0..5`           | `1..6`                                                                                    | (none)                                                                                               |
|  [03]   | `PropertyValue`       | `Properties/property#PROPERTY_VALUE`        | `PropertyValueWire.value`               |     14 | `0..13`          | `1..14`                                                                                   | (none) — the ONE recursive family, bounded by `WireLimits`, never a seam re-check                    |
|  [04]   | `TemporalValue`       | `Properties/property#PROPERTY_VALUE`        | `TemporalWire.value`                    |      5 | `0..4`           | `1..5`                                                                                    | (none)                                                                                               |
|  [05]   | `MaterialUsage`       | `Relations/relation#EDGE_ALGEBRA`           | `MaterialUsageWire.usage`               |      3 | `0, 1, 2`        | `3, 1, 2` — PERMUTED; `None` is the explicit `google.protobuf.Empty` arm, appended at 3   | (none) — an unset `usage` oneof is malformed foreign input                                           |
|  [06]   | `MaterialComposition` | `Composition/material#MATERIAL_COMPOSITION` | `MaterialCompositionWire.composition`   |      4 | `0..3`           | `1..4`                                                                                    | (none)                                                                                               |
|  [07]   | `MaterialPropertySet` | `Composition/material#MATERIAL_PROPERTY`    | `MaterialPropertySetWire.property_set`  |     12 | `0..11`          | `2..13` — PERMUTED; `Orthotropic` is canon 6, field 3                                     | `evidence = 1` — the root-declared column rides the envelope, never a per-arm repeat                 |
|  [08]   | `CoverageSample`      | `Geospatial/coverage#COVERAGE_NODE`         | (none)                                  |      2 | —                | —                                                                                         | EXEMPT — a transient read result: never seated on a node, never in `CanonicalBytes`, never crosses   |

The frozen-number ledger holds exactly what no derivation reaches — reserved numbers, envelope brackets, positional
arities, integer discriminants, and per-owner exceptions. Retired NAMES ride this ledger as data: the corpus file
reserves numbers alone, and adding name reservations there would move the frozen descriptor digest.

| [INDEX] | [SITE]                         | [FROZEN_FACT]                                                                       | [LAW]                                                                                                                                                                                                                                                                                                          |
| :-----: | :----------------------------- | :----------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ProfileSetWire`               | `reserved 1, 2`                                                                       | retired primary-material and primary-profile scalars — the seam derives both off row zero, and a primary scalar beside it double-stores what row zero already carries                                                                                                                                              |
|  [02]   | `CoverageWire`                 | `reserved 3`                                                                          | retired six-coefficient grid descriptor — the placement is the kernel lattice crossing whole on `grid = 10`                                                                                                                                                                                                        |
|  [03]   | `CoverageBandWire`             | `reserved 3`; `sample_type = 12`                                                      | retired string raster-sample-type key; the SAME column name relanded as the kernel `ChannelDtype` `sint32` key PAST `palette = 11`, so declaration order and number order disagree here and nowhere else — the exemplar of append-past-the-burn                                                                    |
|  [04]   | `OverviewLevelWire`            | `reserved 1, 2, 3`                                                                    | retired `width`/`height`/`cell_size` — the level carries its own lattice                                                                                                                                                                                                                                           |
|  [05]   | `NodeWire`                     | `id = 1`, payload `2..9`, `content_address = 10`                                      | the envelope brackets the oneof, so a ninth payload case takes 11                                                                                                                                                                                                                                                  |
|  [06]   | `MaterialPropertySetWire`      | `evidence = 1`, arms `2..13`                                                          | the envelope precedes the oneof, so a thirteenth property case takes 14                                                                                                                                                                                                                                            |
|  [07]   | `AcousticWire` fields 1 and 2  | arity = the `AcousticBand` roster count                                               | both spectra are POSITIONAL over the band roster: a band row widens both runs with no wire declaration moving, and `Acoustic.Of` is the only arity gate                                                                                                                                                            |
|  [08]   | `EnvironmentalWire.impacts`    | arity = `ImpactCategory` × `LifecycleStage`, ROW-MAJOR                                | `LifecycleStage` is the STRIDE — an impact row appends index-stably while a stage row RE-STRIDES every stored cell: the one coupling where vocabulary growth is a wire break wearing a data edit                                                                                                                   |
|  [09]   | `CellLatticeWire.affine`       | exactly 12                                                                            | row-major 3×4 — the fourth matrix row is the invariant `[0 0 0 1]`; the ONE wire-side arity gate (`ToLattice`), because a repeated field carries no fixed length                                                                                                                                                   |
|  [10]   | `SectionPropertiesWire`        | 19 positional measure columns + `monosymmetry_factor = 20`                            | the decode `SectionColumns` table is the frozen order's second authority and the two move as one                                                                                                                                                                                                                   |
|  [11]   | integer discriminants          | `sint32` default; `int32` on the two `priority` columns; `uint32` on the `ColorBinWire` channels; `sint64` on `ceiling` | the three deviations are FROZEN wire facts recorded so a new integer column defaults to `sint32` and a deviation stays a named row, never a precedent — the display channels range-gate at decode and the ceiling is the one 64-bit census budget                            |
|  [12]   | `canonical_unit`               | ABSENT on `MeasureValueWire`; `= 11` on `ObservationWire`                             | the rule keys per-OWNER, never per-type: measure decode re-mints the unit through `OfSi` so wire and canon agree by construction, while the series mints through `Rehydrate`'s trusted re-mint and carries the token verbatim — a type-keyed derivation drops the observation column and kills the sample lift     |
|  [13]   | `VerticalCrs`                  | flattened — `vertical_datum = 10` + `vertical_epsg = 13` on `GeoReferenceWire`        | a two-column identity flattens onto the parent it re-admits through one `Admit`; `ProjectedCrs` takes its own message (`crs = 11`) because it owns derived peer-informative columns (`epsg`, `resolution`)                                                                                                        |
|  [14]   | explicit-presence flips        | `FireWire.reaction = 1`, `EnvironmentalWire.recycled_content = 3`, `.end_of_life_recovery = 4` | the three scalar columns ride explicit presence so absence never aliases a zero or a blank key — the one-time `FIELD_SAME_CARDINALITY` waiver rides `buf.yaml` naming this law; an `optional` mark on a MESSAGE column changes no presence semantics yet still mints a synthetic `_<field>` oneof the FILE gate never reports, so the frozen digest is that move's only witness |

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
using static LanguageExt.Prelude;
// The kernel lattice-axis count and the seam's physical 7-vector both spell Dimension; the alias names the kernel
// reading so the enclosing-namespace Dimension stays the bare SI signature.
using LatticeAxis = Rasm.Numerics.Dimension;

namespace Rasm.Element.Graph;

// Csproj codegen item this contract realizes; ProtoRoot pins the corpus root so the descriptor name reads
// rasm/element/v1/element.proto, the one spelling the frozen snapshot and both peer minters carry:
//   <Protobuf Include="../../../tests/contracts/rasm/element/v1/element.proto"
//             ProtoRoot="../../../tests/contracts" GrpcServices="None" />

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
  sizeLimit > 0 && recursionLimit > 0
   ? Fin.Succ(new WireLimits(sizeLimit, recursionLimit, verifyAddresses: false))
   : ElementFault.ValueRejected(key, $"<wire-limits-invalid:{sizeLimit}:{recursionLimit}>");

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
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.NodaExtensions))]
[UseStaticMapper(typeof(NodaTime.Serialization.Protobuf.ProtobufExtensions))]
internal static partial class WireCodec {
 // --- [UNION_PARITY] — the drift tripwire the deleted proto mirror never was: one census row per crossing family
 // ties the [02] roster's arm count to the generated oneof enum's arity (the None member excluded), so a seam case
 // or a corpus oneof arm landing without its counterpart throws at first codec touch instead of skewing silently.
 static WireCodec() {
  ReadOnlySpan<(string Family, int Arms, int Wire)> census = [
   ("node", 8, Arms<NodeWire.PayloadOneofCase>()),
   ("relationship", 6, Arms<RelationshipWire.EdgeOneofCase>()),
   ("property-value", 14, Arms<PropertyValueWire.ValueOneofCase>()),
   ("temporal-value", 5, Arms<TemporalWire.ValueOneofCase>()),
   ("material-usage", 3, Arms<MaterialUsageWire.UsageOneofCase>()),
   ("material-composition", 4, Arms<MaterialCompositionWire.CompositionOneofCase>()),
   ("material-property-set", 12, Arms<MaterialPropertySetWire.PropertySetOneofCase>()),
  ];
  foreach ((string family, int arms, int wire) in census) {
   if (arms != wire) { throw new InvalidOperationException($"<wire-union-parity:{family}:{arms}:{wire}>"); }
  }
 }
 static int Arms<T>() where T : struct, Enum => Enum.GetValues<T>().Length - 1;

 // --- [KEY_CODECS] — verbatim crossings, never re-minted
 [UserMapping] internal static string ToWire(NodeId id) => id.Value;
 [UserMapping] internal static NodeId ToNodeId(string wire) => NodeId.Create(wire);
 [UserMapping] internal static string ToWire(MaterialId id) => id.Value;
 [UserMapping] internal static ByteString ToWire(UInt128 key) {
  Span<byte> be = stackalloc byte[16];
  BinaryPrimitives.WriteUInt128BigEndian(be, key);           // the persisted XxHash128 canonical form
  return ByteString.CopyFrom(be);
 }
 [UserMapping] internal static UInt128 ToKey(ByteString wire) => BinaryPrimitives.ReadUInt128BigEndian(wire.Span);

 // --- [CASE_TRANSCRIPTIONS] — Mapperly generates the flat-column width per case; every union-valued member rides
 // an explicit envelope fold below, every MESSAGE-shaped Option crossing rides a nullable-return [UserMapping]
 // carrier codec, every optional SCALAR/STRING column rides a hand IfSome presence write (the [PRESENCE_SHELLS]
 // law below), and [MapProperty] pins every seam→wire name seam so the generator never silently skips a member.
 [MapperIgnoreSource(nameof(Node.Material.Id))]
 [MapProperty(nameof(Node.Material.Properties), nameof(MaterialWire.PropertySets))]
 internal static partial MaterialWire ToWire(Node.Material node);
 // The property bag's Groups is empty by construction (its nesting is the PropertyValue.Complex case) and
 // PropertySetWire declares no counterpart, so the source member is ignored EXPLICITLY — RequiredMappingStrategy.Both
 // faults an unmapped source member, and that fault is the signal a group-bearing property bag would owe a wire field.
 [MapperIgnoreSource(nameof(PropertyBag.Groups))]
 [MapProperty(nameof(PropertyBag.Source), nameof(PropertySetWire.SourceRank))]
 internal static partial PropertySetWire ToWire(PropertyBag bag);
 [MapProperty(nameof(QuantityBag.Source), nameof(QuantitySetWire.SourceRank))]
 internal static partial QuantitySetWire ToWire(QuantityBag bag);
 internal static partial AppearanceWire ToWire(AppearanceSummary summary);
 internal static partial CoverageWire ToWire(CoverageGrid grid);
 [MapProperty(nameof(Relationship.Assign.Subject), nameof(AssignWire.SubjectId))]
 [MapProperty(nameof(Relationship.Assign.Definition), nameof(AssignWire.DefinitionId))]
 internal static partial AssignWire ToWire(Relationship.Assign edge);
 [MapProperty(nameof(Relationship.Associate.Subject), nameof(AssociateWire.SubjectId))]
 [MapProperty(nameof(Relationship.Associate.Resource), nameof(AssociateWire.ResourceId))]
 internal static partial AssociateWire ToWire(Relationship.Associate edge);
 [MapProperty(nameof(Relationship.Void.Host), nameof(VoidWire.HostId))]
 [MapProperty(nameof(Relationship.Void.Feature), nameof(VoidWire.FeatureId))]
 internal static partial VoidWire ToWire(Relationship.Void edge);
 [MapProperty(nameof(Header.Reference), nameof(HeaderWire.GeoReference))]
 [MapProperty(nameof(Header.Units), nameof(HeaderWire.UnitScheme))]
 internal static partial HeaderWire ToWire(Header header);
 internal static partial StepHeaderWire ToWire(StepHeader step);
 // LeastDimension re-derives from the Depth/Width pair and IsDoublySymmetric from the shear-centre offsets and the
 // mono-symmetry factor — stored columns that DO cross — so neither derived member crosses; a wire field for either
 // double-stores one fact, the same law the property-set rosters below hold.
 [MapperIgnoreSource(nameof(SectionProperties.LeastDimension))]
 [MapperIgnoreSource(nameof(SectionProperties.IsDoublySymmetric))]
 internal static partial SectionPropertiesWire ToWire(SectionProperties section);
 // Every property-set case ignores its non-crossing source members BY NAME, never by suppression: the base Evidence
 // column rides the ENVELOPE (MaterialPropertySetWire.evidence, the Switch fold below), the base Discipline read
 // is the case-to-discipline map the far end re-reads off the decoded case, and every DERIVED member (the isotropic
 // ShearModulus, the Environmental carbon projections, the Optical absorptance remainders) re-derives from the stored
 // columns that DO cross — a wire field for any of them would double-store one fact. The explicit roster keeps
 // RequiredMappingStrategy.Both's source-side RMG020 proof live for every stored column; the Acoustic/Damping arms
 // carry hand [UserMapping] bodies below, so no roster applies to them, and the Fire/Environmental/Hygrothermal/
 // Electrical arms ride [PRESENCE_SHELLS] whose optional scalar/string columns are roster-named HAND-CROSSED
 // members, never non-crossing ones.
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Mechanical.ShearModulus))]
 internal static partial MechanicalWire ToWire(MaterialPropertySet.Mechanical set);
 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Orthotropic.Discipline))]
 internal static partial OrthotropicWire ToWire(MaterialPropertySet.Orthotropic set);
 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Thermal.Discipline))]
 internal static partial ThermalWire ToWire(MaterialPropertySet.Thermal set);
 [MapperIgnoreSource(nameof(MaterialPropertySet.Cost.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Cost.Discipline))]
 internal static partial CostWire ToWire(MaterialPropertySet.Cost set);
 [MapperIgnoreSource(nameof(MaterialPropertySet.Durability.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Durability.Discipline))]
 internal static partial DurabilityWire ToWire(MaterialPropertySet.Durability set);
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.SolarAbsorptanceFront))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Optical.SolarAbsorptanceBack))]
 internal static partial OpticalWire ToWire(MaterialPropertySet.Optical set);
 internal static partial TimeSliceWire ToWire(TimeSlice slice);

 // --- [PRESENCE_SHELLS] — the seven cases carrying proto3 optional SCALAR/STRING columns. Those columns land on
 // protoc's Has*/Clear* pattern behind a NULL-REJECTING setter, so no nullable-return carrier can leave one unset —
 // a generated assignment fails on the scalar shape and throws on the string shape — and each such column is
 // [MapperIgnoreSource]-named on its generated Shell as a HAND-CROSSED member (the roster comment discriminates it
 // from a non-crossing ignore) while the ONE wrapper owns its IfSome presence writes; [UserMapping(Default = true)]
 // keeps the wrapper the pair's selected mapping beside its Shell. Envelope owns Id (NodeWire.id), so both node
 // payload mappings exclude it; AllClassifications is the node's own COMPUTED union of the primary and secondary
 // columns, both of which already cross on their own fields, so mapping it would double-store the primary.
 [MapperIgnoreSource(nameof(Node.Object.Id))]
 [MapperIgnoreSource(nameof(Node.Object.AllClassifications))]
 [MapperIgnoreSource(nameof(Node.Object.ExternalId))]
 [MapperIgnoreSource(nameof(Node.Object.ObjectType))]
 private static partial ObjectWire Shell(Node.Object node);
 [UserMapping(Default = true)] internal static ObjectWire ToWire(Node.Object node) {
  ObjectWire w = Shell(node); node.ExternalId.IfSome(v => w.ExternalId = v); node.ObjectType.IfSome(v => w.ObjectType = v); return w;
 }
 [MapProperty(nameof(AssessmentPayload.DependsOn), nameof(AssessmentWire.DependsOnIds))]
 [MapperIgnoreSource(nameof(AssessmentPayload.ResultBlob))]
 private static partial AssessmentWire Shell(AssessmentPayload payload);
 [UserMapping(Default = true)] internal static AssessmentWire ToWire(AssessmentPayload payload) {
  AssessmentWire w = Shell(payload); payload.ResultBlob.IfSome(k => w.ResultBlob = ToWire(k)); return w;
 }
 [MapProperty(nameof(Relationship.Connect.From), nameof(ConnectWire.FromId))]
 [MapProperty(nameof(Relationship.Connect.To), nameof(ConnectWire.ToId))]
 [MapperIgnoreSource(nameof(Relationship.Connect.Realizing))]
 [MapperIgnoreSource(nameof(Relationship.Connect.Interface))]
 private static partial ConnectWire Shell(Relationship.Connect edge);
 [UserMapping(Default = true)] internal static ConnectWire ToWire(Relationship.Connect edge) {
  ConnectWire w = Shell(edge); edge.Realizing.IfSome(r => w.RealizingId = r.Value); edge.Interface.IfSome(k => w.InterfaceKey = ToWire(k)); return w;
 }
 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Fire.Reaction))]
 private static partial FireWire Shell(MaterialPropertySet.Fire set);
 [UserMapping(Default = true)] internal static FireWire ToWire(MaterialPropertySet.Fire set) {
  FireWire w = Shell(set); set.Reaction.IfSome(r => w.Reaction = r.Key); return w;
 }
 // All three EN 13501-2 criteria are optional scalars, so the whole row is presence writes — the one nested message
 // the Fire shell reaches through a hand mapping rather than a generated one.
 [UserMapping] internal static FireResistanceWire ToWire(FireResistance resistance) {
  FireResistanceWire w = new(); resistance.LoadBearingMinutes.IfSome(m => w.LoadBearingMinutes = m); resistance.IntegrityMinutes.IfSome(m => w.IntegrityMinutes = m); resistance.InsulationMinutes.IfSome(m => w.InsulationMinutes = m); return w;
 }
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.Gwp))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.WholeLifeGwp))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.StageGwp))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.RecycledContent))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Environmental.EndOfLifeRecovery))]
 private static partial EnvironmentalWire Shell(MaterialPropertySet.Environmental set);
 [UserMapping(Default = true)] internal static EnvironmentalWire ToWire(MaterialPropertySet.Environmental set) {
  EnvironmentalWire w = Shell(set); set.RecycledContent.IfSome(v => w.RecycledContent = v); set.EndOfLifeRecovery.IfSome(v => w.EndOfLifeRecovery = v); return w;
 }
 [MapperIgnoreSource(nameof(MaterialPropertySet.Hygrothermal.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Hygrothermal.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Hygrothermal.WaterAbsorptionKgPerM2SqrtS))]
 private static partial HygrothermalWire Shell(MaterialPropertySet.Hygrothermal set);
 [UserMapping(Default = true)] internal static HygrothermalWire ToWire(MaterialPropertySet.Hygrothermal set) {
  HygrothermalWire w = Shell(set); set.WaterAbsorptionKgPerM2SqrtS.IfSome(v => w.WaterAbsorptionKgPerM2SqrtS = v); return w;
 }
 [MapperIgnoreSource(nameof(MaterialPropertySet.Electrical.Evidence))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Electrical.Discipline))]
 [MapperIgnoreSource(nameof(MaterialPropertySet.Electrical.MagneticPermeabilityRelative))]
 private static partial ElectricalWire Shell(MaterialPropertySet.Electrical set);
 [UserMapping(Default = true)] internal static ElectricalWire ToWire(MaterialPropertySet.Electrical set) {
  ElectricalWire w = Shell(set); set.MagneticPermeabilityRelative.IfSome(v => w.MagneticPermeabilityRelative = v); return w;
 }

 // Existing-target carrier codecs for the MapField members — hand-owned because the SOURCE is a LanguageExt Map:
 // the generator member-maps Map's Keys/Values PROPERTIES onto MapField's read-only Keys/Values collections and the
 // emitted Add throws, while a BCL dictionary source crosses clean, so the fill exists for the Map source shape,
 // never for the get-only target; keys cross as the PropertyName string, values recurse.
 [UserMapping] internal static void ToWire(Map<PropertyName, PropertyValue> values, [MappingTarget] MapField<string, PropertyValueWire> wire) { foreach (var (n, v) in values) { wire[n.Value] = ToWire(v); } }
 [UserMapping] internal static void ToWire(Map<PropertyName, MeasureValue> values, [MappingTarget] MapField<string, MeasureValueWire> wire) { foreach (var (n, m) in values) { wire[n.Value] = ToWire(m); } }
 // The group run keys on the dot-path prefix string (not a PropertyName), and each Option column writes CONDITIONALLY
 // so an unstated qualifier leaves its proto3 optional unset rather than crossing as an empty spelling.
 [UserMapping] internal static void ToWire(Map<string, GroupIdentity> groups, [MappingTarget] MapField<string, GroupIdentityWire> wire) { foreach (var (prefix, group) in groups) { GroupIdentityWire row = new(); group.Discrimination.IfSome(d => row.Discrimination = d); group.Quality.IfSome(q => row.Quality = q); group.Usage.IfSome(u => row.Usage = u); wire[prefix] = row; } }
 [UserMapping] internal static void ToWire(UnitScheme scheme, [MappingTarget] MapField<string, string> wire) { foreach (var (quantity, unit) in scheme.Display) { wire[quantity] = unit; } }

 [UserMapping] internal static ClassificationWire ToWire(Classification c) {
  ClassificationWire w = new() { System = c.System, Code = c.Code, Edition = c.Edition };
  c.Source.IfSome(s => w.Source = s); c.EditionDate.IfSome(d => w.EditionDate = NodaTime.Text.LocalDatePattern.Iso.Format(d)); c.Title.IfSome(t => w.Title = t); return w;
 }

 // --- [CARRIER_CODECS] — the Option crossings Mapperly bridges are MESSAGE-shaped alone: a protoc message property
 // admits null as unset, so a nullable return that skips assignment IS the absence write. The scalar/string shape
 // owns NO carrier here by law — protoc's Has*/Clear* pattern sits behind a null-rejecting setter, so a nullable
 // return cannot express its absence and every such column writes presence by hand ([PRESENCE_SHELLS] above).
 // Each carrier keeps its per-T Match body: the struct-element rows would read default(T) off ValueUnsafe, and the
 // projections differ per element, so a generic carrier has nothing lawful to collapse onto.
 // Absent measured columns (the product-only U-value) leave their optional message unset, never a zero-SI measure.
 [UserMapping] internal static MeasureValueWire? ToWire(Option<MeasureValue> value) => value.Match<MeasureValueWire?>(static m => ToWire(m), static () => null);
 // Absent sampled curves (an undeclared reduction, λ(θ), or hygrothermal table) leave their optional message
 // unset, never an empty run the arity gate would refuse at the far end.
 [UserMapping] internal static SampledCurveWire? ToWire(Option<SampledCurve> curve) => curve.Match<SampledCurveWire?>(static c => ToWire(c), static () => null);
 // The absence carrier over the registered NodaExtensions conversion — the presence decision is this seam's, the
 // conversion the static mapper's, so an absent instant leaves its proto3 optional unset.
 [UserMapping] internal static Timestamp? ToWire(Option<NodaTime.Instant> at) => at.Match<Timestamp?>(static i => i.ToTimestamp(), static () => null);

 [UserMapping] internal static void ToWire(RepresentationContentHash representations, [MappingTarget] MapField<string, ByteString> wire) { foreach (var (id, hash) in representations.ByIdentifier) { wire[id] = ToWire(hash); } }

 [UserMapping] internal static SchemaSpanWire ToWire(SchemaSpan span) {
  SchemaSpanWire w = new() { IntroducedIn = span.IntroducedIn.Key }; span.RemovedIn.IfSome(r => w.RemovedIn = r.Key); return w;
 }

 // Every column is a plain crossing once the static temporal mappers and the Option<Instant> carrier are registered,
 // so the audit row generates whole and only its ABSENCE decision stays hand-owned.
 internal static partial OwnerHistoryWire ToWire(OwnerHistory history);
 [UserMapping] internal static OwnerHistoryWire? ToWire(Option<OwnerHistory> history) => history.Match<OwnerHistoryWire?>(static h => ToWire(h), static () => null);

 // The placement frame flattens to its nine ordered columns by AUTO-FLATTEN — LocationX binds the Location.X source
 // path with zero configuration — and the frame's ABSENCE rides the same nullable-return carrier every optional
 // message crossing takes.
 internal static partial PlacementWire ToWire(PlacementTransform placement);
 [UserMapping] internal static PlacementWire? ToWire(Option<PlacementTransform> placement) => placement.Match<PlacementWire?>(static p => ToWire(p), static () => null);

 // Wire epsg/resolution columns are peer-informative derivations; blank ProjectedCrs strings stay unset.
 [UserMapping] internal static GeoReferenceWire ToWire(GeoReference geo) {
  GeoReferenceWire w = new() {
   Eastings = geo.Eastings, Northings = geo.Northings, OrthogonalHeight = geo.OrthogonalHeight,
   XAxisAbscissa = geo.XAxisAbscissa, XAxisOrdinate = geo.XAxisOrdinate,
   ScaleX = geo.ScaleX, ScaleY = geo.ScaleY, ScaleZ = geo.ScaleZ,
   GeodeticDatum = geo.GeodeticDatum,
  };
  geo.Vertical.IfSome(v => { w.VerticalDatum = v.Name; v.Epsg.IfSome(e => w.VerticalEpsg = e); });
  geo.Crs.IfSome(c => {
   ProjectedCrsWire p = new() { Name = c.Name, Resolution = c.Resolution.Key };
   c.Epsg.IfSome(e => p.Epsg = e);
   if (c.Wkt.Length > 0) { p.Wkt = c.Wkt; }
   if (c.MapProjection.Length > 0) { p.MapProjection = c.MapProjection; }
   if (c.MapZone.Length > 0) { p.MapZone = c.MapZone; }
   w.Crs = p;
  });
  geo.Epoch.IfSome(epoch => w.Epoch = epoch); return w;
 }

 [UserMapping] internal static ComposeWire ToWire(Relationship.Compose edge) {
  ComposeWire wire = new() { WholeId = edge.Whole.Value, PartId = edge.Part.Value, SubKind = edge.SubKind.Key };
  edge.Ordinal.IfSome(ordinal => wire.Ordinal = ordinal); return wire;
 }

 [UserMapping] internal static GenericWire ToWire(Relationship.Generic edge) {
  GenericWire wire = new() { WireName = edge.WireName, RelatingId = edge.Source.Value, RelatedId = edge.Target.Value };
  ToWire(edge.Attributes, wire.Attributes);
  wire.Participants.AddRange(edge.Participants.Map(participant => {
   RelationshipParticipantWire row = new() { NodeId = participant.Node.Value, Role = participant.Role };
   participant.Ordinal.IfSome(ordinal => row.Ordinal = ordinal); return row;
  }));
  return wire;
 }

 [UserMapping] internal static CoverageBandWire ToWire(CoverageBand band) {
  CoverageBandWire w = new() { Index = band.Index, Name = band.Name, SampleType = band.SampleType.Key, Role = band.Role.Key, Units = band.Units, Offset = band.Offset, Scale = band.Scale };
  band.NoData.IfSome(v => w.NoData = v);
  band.Range.IfSome(r => { w.RangeMin = r.Min; w.RangeMax = r.Max; });
  // The legend colour crosses through the SAME ToRgb quantizer CanonicalBytes takes, so the wire quadruple and the
  // content key are one projection — a second quantization here would let two runtimes agree on the key and disagree
  // on the swatch. The decoder re-admits through PerceptualColor.OfRgb, never a stored perceptual triple, because the
  // display quadruple is the only form both the key and every host palette surface already speak. Both calls stay
  // CONDITION-FREE for the same reason coverage#COVERAGE_NODE CanonicalBytes does: the kernel seats a viewing
  // condition on appearance-case payloads and never on ToRgb, and a gamut or observer argument admitted at either
  // end alone splits the wire from the key it is defined to agree with.
  w.Palette.AddRange(band.Palette.Map(static c => {
   (byte r, byte g, byte b, byte a) = c.Colour.ToRgb();
   return new ColorBinWire { Index = c.Index, R = r, G = g, B = b, A = a, Category = c.Category };
  }));
  return w;
 }

 // The kernel placement crosses as its twelve index-to-world coefficients plus the census and ceiling the decoder
 // re-admits with — the fourth matrix row is the invariant [0 0 0 1] and carries no information, so twelve IS the
 // whole affine and a thirteenth column would be a value the receiver already knows. The body stays hand because
 // the source is the KERNEL owner: its axis columns lower through .Value reads and its derived affine surface would
 // demote a generated partial to an ignore-roster inventory over a foreign package's members.
 [UserMapping] internal static CellLatticeWire ToWire(CellLattice lattice) {
  CellLatticeWire w = new() { Columns = lattice.Columns.Value, Rows = lattice.Rows.Value, Layers = lattice.Layers.Value, Ceiling = lattice.Ceiling };
  w.Affine.AddRange(lattice.Affine); return w;
 }

 [UserMapping] internal static ProvenanceWire ToWire(Provenance p) {
  ProvenanceWire w = new() { Author = p.Author, Tool = p.Tool, Version = p.Version, At = p.At.ToTimestamp(), Elapsed = p.Elapsed.ToProtobufDuration(), Attempt = p.Attempt };
  p.Window.IfSome(i => { w.WindowStart = i.Start.ToTimestamp(); w.WindowEnd = i.End.ToTimestamp(); });
  // `CorrelationId` carries the kernel's own `ISpanFormattable` "D" render, so the wire text and the
  // `Guid.TryParse` decode below stay one round-trippable spelling.
  p.Correlation.IfSome(c => w.Correlation = c.ToString("D", CultureInfo.InvariantCulture));
  return w;
 }

 [UserMapping] internal static DiagnosticWire? ToWire(Option<Diagnostic> diagnostic) => diagnostic.Match<DiagnosticWire?>(
  static d => { DiagnosticWire w = new() { Phase = d.Phase.Key, Kind = d.Kind.Key, Message = d.Message }; d.Code.IfSome(c => w.Code = c); return w; },
  static () => null);

 [UserMapping] internal static PropertyEvidenceWire ToWire(PropertyEvidence evidence) {
  PropertyEvidenceWire w = new() { Source = evidence.Source, Reference = evidence.Reference };
  evidence.ValidUntil.IfSome(d => w.ValidUntil = NodaTime.Text.LocalDatePattern.Iso.Format(d)); return w;
 }

 // MeasureValue crosses as its identity columns; the wire NEVER carries CanonicalUnit — decode re-mints it
 // through the OfSi registry resolve and re-attaches the band, so wire and canon agree by construction.
 // Encode stays the total [UserMapping]; DECODE is Fin — the OfSi finite gate below.
 [UserMapping] internal static MeasureValueWire ToWire(MeasureValue m) {
  MeasureValueWire w = new() {
   QuantityType = m.Type.Value, Si = m.Si,
   DimLength = m.Dimension.Length, DimMass = m.Dimension.Mass, DimTime = m.Dimension.Time,
   DimCurrent = m.Dimension.Current, DimTemperature = m.Dimension.Temperature,
   DimAmount = m.Dimension.Amount, DimLuminousIntensity = m.Dimension.LuminousIntensity,
  };
  m.Uncertainty.IfSome(b => w.Uncertainty = ToWire(b));
  return w;
 }
 // Fin-railed decode through the OWNER's OfSi finite gate — a hostile NaN/∞ scalar rails ValueRejected exactly as
 // an in-process SI-native mint does, never a decoder-local finite check; the keyless interior fault re-keys here.
 // The null arm is the FAMILY-WIDE Present gate: every required measure column on every arm funnels through this
 // one member, so an unset hostile message names itself on the rail instead of dereferencing in the funnel.
 internal static Fin<MeasureValue> ToMeasure(MeasureValueWire? w, Op key) =>
  w is null
   ? ElementFault.ValueRejected(key, "<wire-message-absent:measure>")
   : MeasureValue.OfSi(
    QuantityType.Create(w.QuantityType),
    Dimension.Create(w.DimLength, w.DimMass, w.DimTime, w.DimCurrent, w.DimTemperature, w.DimAmount, w.DimLuminousIntensity),
    w.Si)
   .MapFail(_ => (Error)ElementFault.ValueRejected(key, $"<wire-measure-non-finite:{w.QuantityType}>"))
   .Bind(m => w.Uncertainty is null
    ? Fin.Succ(m)
    : ToBand(w.Uncertainty, key).Bind(band => m.WithUncertainty(band, key)));
 [UserMapping] internal static MeasureBandWire ToWire(MeasureBand band) {
  MeasureBandWire w = new() { Kind = band.Kind.Key, LowerSi = band.LowerSi, UpperSi = band.UpperSi };
  band.StandardDeviationSi.IfSome(sd => w.StandardDeviationSi = sd); band.CoverageFactor.IfSome(k => w.CoverageFactor = k); return w;
 }
 [UserMapping] internal static Fin<MeasureBand> ToBand(MeasureBandWire w, Op key) =>
  UncertaintyKind.TryGet(w.Kind, out UncertaintyKind? kind) && kind is { } row
   ? MeasureBand.Admit(
      row, w.LowerSi, w.UpperSi,
      Opt(w.HasStandardDeviationSi, w.StandardDeviationSi), Opt(w.HasCoverageFactor, w.CoverageFactor), key)
   : ElementFault.ValueRejected(key, $"<wire-uncertainty-kind:{w.Kind}>");

 // One envelope fold per union uses generated total Switch; a new case breaks compilation.
 internal static NodeWire ToWire(Node node, double tolerance) {
  NodeWire wire = node.Switch<NodeWire>(
   @object: o => new() { Id = o.Id.Value, Object = ToWire(o) },
   material: m => new() { Id = m.Id.Value, Material = ToWire(m) },
   propertySet: p => new() { Id = p.Id.Value, PropertySet = ToWire(p.Bag) },
   quantitySet: q => new() { Id = q.Id.Value, QuantitySet = ToWire(q.Bag) },
   assessment: a => new() { Id = a.Id.Value, Assessment = ToWire(a.Payload) },
   appearance: a => new() { Id = a.Id.Value, Appearance = ToWire(a.Summary) },
   coverage: c => new() { Id = c.Id.Value, Coverage = ToWire(c.Grid) },
   observation: o => new() { Id = o.Id.Value, Observation = ToWire(o.Series) });
  wire.ContentAddress = ToWire(ContentAddress.Of(node, tolerance).Value);
  return wire;
 }

 // Hand-owned like ToWire(GeoReference): the Interval flattens to a bounded column PAIR and the census map keys on a
 // generated row, neither a shape Mapperly bridges. Both window ends are bounded by seam admission, so the columns
 // are unconditional and no presence flag stands in for an unbounded side.
 [UserMapping] internal static ObservationWire ToWire(ObservationSeries series) {
  ObservationWire w = new() {
   Sensor = series.Sensor.Value, Aspect = series.Aspect.Value, Observed = series.Observed.Value,
   DimLength = series.Signature.Length, DimMass = series.Signature.Mass, DimTime = series.Signature.Time,
   DimCurrent = series.Signature.Current, DimTemperature = series.Signature.Temperature,
   DimAmount = series.Signature.Amount, DimLuminousIntensity = series.Signature.LuminousIntensity,
   CanonicalUnit = series.CanonicalUnit, Sampling = series.Sampling.Key,
   WindowStart = series.Window.Start.ToTimestamp(), WindowEnd = series.Window.End.ToTimestamp(),
   Statistics = ToWire(series.Statistics), Provenance = ToWire(series.Provenance),
  };
  series.Cadence.IfSome(cadence => w.Cadence = cadence.ToProtobufDuration());
  w.Chunks.AddRange(series.Chunks.Map(static chunk => new ObservationChunkWire {
   WindowStart = chunk.Window.Start.ToTimestamp(), WindowEnd = chunk.Window.End.ToTimestamp(),
   SeriesKey = ToWire(chunk.SeriesKey), SampleCount = chunk.SampleCount,
  }));
  return w;
 }

 [UserMapping] internal static SensorProvenanceWire ToWire(SensorProvenance provenance) {
  SensorProvenanceWire w = new() { Manufacturer = provenance.Manufacturer, Model = provenance.Model, Serial = provenance.Serial };
  provenance.CalibratedAt.IfSome(date => w.CalibratedAt = NodaTime.Text.LocalDatePattern.Iso.Format(date)); provenance.Tolerance.IfSome(band => w.Tolerance = ToWire(band)); return w;
 }

 [UserMapping] internal static SeriesStatisticsWire ToWire(SeriesStatistics statistics) {
  SeriesStatisticsWire w = new() { Span = statistics.Span.ToProtobufDuration() };
  foreach ((ObservationGrade grade, int count) in statistics.Census) { w.Census[grade.Key] = count; }
  statistics.Minimum.IfSome(measure => w.Minimum = ToWire(measure)); statistics.Maximum.IfSome(measure => w.Maximum = ToWire(measure));
  statistics.Mean.IfSome(measure => w.Mean = ToWire(measure)); statistics.Total.IfSome(measure => w.Total = ToWire(measure)); return w;
 }

 internal static RelationshipWire ToWire(Relationship edge) => edge.Switch<RelationshipWire>(
  compose: e => new() { Compose = ToWire(e) },
  assign: e => new() { Assign = ToWire(e) },
  associate: e => new() { Associate = ToWire(e) },
  connect: e => new() { Connect = ToWire(e) },
  @void: e => new() { Void = ToWire(e) },
  generic: e => new() { Generic = ToWire(e) });

 internal static PropertyValueWire ToWire(PropertyValue value) => value.Switch<PropertyValueWire>(
  text: v => new() { Text = v.Value },
  measure: v => new() { Measure = ToWire(v.Value) },
  boolean: v => new() { Boolean = v.Value },
  logical: v => { LogicalWire l = new(); v.Value.IfSome(b => l.Value = b); return new() { Logical = l }; },
  enumerated: v => { EnumeratedWire e = new(); e.Selected.AddRange(v.Selected.Map(ToWire)); e.Allowed.AddRange(v.Allowed.Map(ToWire)); return new() { Enumerated = e }; },
  reference: v => { ReferenceWire r = new() { TargetId = v.Target.Value }; v.UsageName.IfSome(u => r.UsageName = u); return new() { Reference = r }; },
  bounded: v => { BoundedWire b = new(); v.Lower.IfSome(m => b.Lower = ToWire(m)); v.Upper.IfSome(m => b.Upper = ToWire(m)); v.SetPoint.IfSome(m => b.SetPoint = ToWire(m)); return new() { Bounded = b }; },
  list: v => { ListWire l = new(); l.Values.AddRange(v.Values.Map(ToWire)); return new() { List = l }; },
  table: v => { TableWire t = new() { Interpolation = v.Interp.Key }; t.Rows.AddRange(v.Rows.Map(r => new TableRowWire { Defining = ToWire(r.Defining), Defined = ToWire(r.Defined) })); return new() { Table = t }; },
  complex: v => { ComplexWire c = new() { UsageName = v.UsageName }; foreach (var (n, inner) in v.Properties) { c.Properties[n.Value] = ToWire(inner); } return new() { Complex = c }; },
  temporal: v => new() { Temporal = v.Value.Switch<TemporalWire>(
   date: static t => new() { Date = NodaTime.Text.LocalDatePattern.Iso.Format(t.Value) },
   moment: static t => new() { Moment = NodaTime.Text.LocalDateTimePattern.ExtendedIso.Format(t.Value) },
   time: static t => new() { Time = NodaTime.Text.LocalTimePattern.ExtendedIso.Format(t.Value) },
   span: static t => new() { Span = NodaTime.Text.PeriodPattern.Roundtrip.Format(t.Value) },
   stamp: static t => new() { Stamp = t.Value.ToTimestamp() }) },
  integer: static v => new() { Integer = ByteString.CopyFrom(v.Value.ToByteArray(isUnsigned: false, isBigEndian: true)) },
  number: static v => new() { Number = v.Value },
  binary: static v => new() { Binary = ByteString.CopyFrom(v.Value.ToArray()) });

 internal static MaterialUsageWire ToWire(MaterialUsage usage) => usage.Switch<MaterialUsageWire>(
  none: static _ => new() { None = new Google.Protobuf.WellKnownTypes.Empty() },
  layerSet: u => { LayerSetUsageWire wire = new() { Direction = u.Direction.Key, Sense = u.Sense.Key }; u.OffsetFromReferenceLine.IfSome(value => wire.OffsetFromReferenceLine = ToWire(value)); u.ReferenceExtent.IfSome(value => wire.ReferenceExtent = ToWire(value)); return new() { LayerSet = wire }; },
  profileSet: u => { ProfileSetUsageWire wire = new(); u.CardinalPoint.IfSome(value => wire.CardinalPoint = value.Key); u.ReferenceExtent.IfSome(value => wire.ReferenceExtent = ToWire(value)); return new() { ProfileSet = wire }; });

 // Every optional row column writes through explicit protobuf presence — an IfSome assignment, never a defaulted zero or
 // false that a decoder cannot distinguish from an author's real value.
 internal static MaterialCompositionWire ToWire(MaterialComposition composition) => composition.Switch<MaterialCompositionWire>(
  single: c => new() { Single = new SingleWire { MaterialKey = c.Material.Value } },
  layerSet: c => { LayerSetWire w = new(); w.Layers.AddRange(c.Layers.Map(static l => ToWire(l))); return new() { LayerSet = w }; },
  profileSet: c => { ProfileSetWire w = new(); w.Profiles.AddRange(c.Profiles.Map(static p => ToWire(p))); c.Composite.IfSome(r => w.Composite = ToWire(r)); c.Section.IfSome(s => w.Section = ToWire(s)); return new() { ProfileSet = w }; },
  constituentSet: c => { ConstituentSetWire w = new(); w.Constituents.AddRange(c.Constituents.Map(static x => new MaterialConstituentWire { MaterialKey = x.Material.Value, Category = x.Category, Fraction = x.Fraction, PartName = x.PartName })); return new() { ConstituentSet = w }; });

 internal static MaterialLayerWire ToWire(MaterialLayer layer) {
  MaterialLayerWire w = new() { MaterialKey = layer.Material.Value, Thickness = ToWire(layer.Thickness), LayerName = layer.LayerName, Category = layer.Category };
  layer.Priority.IfSome(p => w.Priority = p); layer.Ventilated.IfSome(v => w.Ventilated = v); return w;
 }

 internal static MaterialProfileWire ToWire(MaterialProfile profile) {
  MaterialProfileWire w = new() { MaterialKey = profile.Material.Value, Profile = ToWire(profile.Profile), Category = profile.Category };
  profile.Priority.IfSome(p => w.Priority = p); w.Offsets.AddRange(profile.Offsets.Map(static o => ToWire(o))); return w;
 }

 // ONE ProfileRef projection serves the row and the set-level composite — a second inline construction is the fork
 // that lets one leg drop the content key the Rehydrate gate re-checks.
 internal static ProfileRefWire ToWire(ProfileRef profile) =>
  new() { Standard = profile.Standard, Designation = profile.Designation, ContentKey = ToWire(profile.ContentKey) };

 // Evidence rides the envelope (the base-class column), each arm its generated flat mapping over the registered
 // Option carriers — the sampled-curve carrier included, so the reduction, λ(θ), and hygrothermal curve columns
 // generate; the Acoustic/Damping arms carry repeated spectra and a tuple flatten no carrier bridges, so their
 // bodies are owned here beside the fold.
 internal static MaterialPropertySetWire ToWire(MaterialPropertySet set) => set.Switch<MaterialPropertySetWire>(
  mechanical: x => new() { Evidence = ToWire(x.Evidence), Mechanical = ToWire(x) },
  orthotropic: x => new() { Evidence = ToWire(x.Evidence), Orthotropic = ToWire(x) },
  thermal: x => new() { Evidence = ToWire(x.Evidence), Thermal = ToWire(x) },
  acoustic: x => new() { Evidence = ToWire(x.Evidence), Acoustic = ToWire(x) },
  fire: x => new() { Evidence = ToWire(x.Evidence), Fire = ToWire(x) },
  environmental: x => new() { Evidence = ToWire(x.Evidence), Environmental = ToWire(x) },
  cost: x => new() { Evidence = ToWire(x.Evidence), Cost = ToWire(x) },
  damping: x => new() { Evidence = ToWire(x.Evidence), Damping = ToWire(x) },
  hygrothermal: x => new() { Evidence = ToWire(x.Evidence), Hygrothermal = ToWire(x) },
  durability: x => new() { Evidence = ToWire(x.Evidence), Durability = ToWire(x) },
  optical: x => new() { Evidence = ToWire(x.Evidence), Optical = ToWire(x) },
  electrical: x => new() { Evidence = ToWire(x.Evidence), Electrical = ToWire(x) });

 [UserMapping] internal static AcousticWire ToWire(MaterialPropertySet.Acoustic set) {
  AcousticWire w = new();
  w.AbsorptionSpectrum.AddRange(set.AbsorptionSpectrum); w.SoundReductionIndexDb.AddRange(set.SoundReductionIndexDb);
  set.DynamicStiffnessMNPerM3.IfSome(v => w.DynamicStiffnessMnPerM3 = v); set.FlowResistivityPaSPerM2.IfSome(v => w.FlowResistivityPaSPerM2 = v);
  set.LossFactor.IfSome(v => w.LossFactor = v); return w;
 }
 [UserMapping] internal static DampingWire ToWire(MaterialPropertySet.Damping set) {
  DampingWire w = new() { DampingRatio = set.DampingRatio };
  set.Rayleigh.IfSome(r => w.Rayleigh = new RayleighWire { AlphaPerS = r.AlphaPerS, BetaS = r.BetaS }); return w;
 }
 // Both repeated runs fill natively — the generator emits its own guarded fill from the two ImmutableArray columns.
 internal static partial SampledCurveWire ToWire(SampledCurve curve);

 // --- [DECODE_DISPATCH] — the generated closed PayloadCase/EdgeCase/ValueCase/UsageCase enums own decode
 // dispatch (an unset case rails ValueRejected, a new oneof arm surfaces as an unhandled enum member); every
 // value re-crosses the SAME seam gates an in-process author does — admitted, never trusted raw.
 internal static Fin<Node> ToNode(NodeWire w, Op key) {
  NodeId id = NodeId.Create(w.Id);                                     // verbatim — never re-derived
  return w.PayloadCase switch {
   NodeWire.PayloadOneofCase.Object => ToObject(id, w.Object, key),
   NodeWire.PayloadOneofCase.Material => ToMaterial(id, w.Material, key),
   NodeWire.PayloadOneofCase.PropertySet => ToBag(w.PropertySet, key).Map(bag => (Node)new Node.PropertySet(id, bag)),
   NodeWire.PayloadOneofCase.QuantitySet => ToBag(w.QuantitySet, key).Map(bag => (Node)new Node.QuantitySet(id, bag)),
   NodeWire.PayloadOneofCase.Assessment => ToAssessment(w.Assessment, key).Map(payload => (Node)new Node.Assessment(id, payload)),
   NodeWire.PayloadOneofCase.Appearance => AppearanceSummary.Rehydrate(
    ToKey(w.Appearance.AppearanceKey), w.Appearance.BaseColorR, w.Appearance.BaseColorG, w.Appearance.BaseColorB,
    w.Appearance.Metallic, w.Appearance.Roughness, w.Appearance.Opacity, w.Appearance.Transmissive, key)
    .Map(summary => (Node)new Node.Appearance(id, summary)),
   NodeWire.PayloadOneofCase.Coverage => ToCoverage(w.Coverage, key).Map(grid => (Node)new Node.Coverage(id, grid)),
   NodeWire.PayloadOneofCase.Observation => ToObservation(w.Observation, key).Map(series => (Node)new Node.Observation(id, series)),
   _ => ElementFault.ValueRejected(key, "<wire-node-payload-none>"),
  };
 }

 internal static Fin<Relationship> ToEdge(RelationshipWire w, Op key) => w.EdgeCase switch {
  RelationshipWire.EdgeOneofCase.Compose => Row(ComposeKind.TryGet(w.Compose.SubKind, out ComposeKind? ck), ck, w.Compose.SubKind, key)
   .Map(k => (Relationship)new Relationship.Compose(
    NodeId.Create(w.Compose.WholeId), NodeId.Create(w.Compose.PartId), k,
    Opt(w.Compose.HasOrdinal, w.Compose.Ordinal))),
  RelationshipWire.EdgeOneofCase.Assign => Row(AssignKind.TryGet(w.Assign.SubKind, out AssignKind? ak), ak, w.Assign.SubKind, key)
   .Map(k => (Relationship)new Relationship.Assign(NodeId.Create(w.Assign.SubjectId), NodeId.Create(w.Assign.DefinitionId), k)),
  RelationshipWire.EdgeOneofCase.Associate => ToUsage(w.Associate.Usage, key)
   .Map(u => (Relationship)new Relationship.Associate(NodeId.Create(w.Associate.SubjectId), NodeId.Create(w.Associate.ResourceId), u)),
  RelationshipWire.EdgeOneofCase.Connect => Row(ConnectKind.TryGet(w.Connect.SubKind, out ConnectKind? nk), nk, w.Connect.SubKind, key)
   .Map(k => (Relationship)new Relationship.Connect(NodeId.Create(w.Connect.FromId), NodeId.Create(w.Connect.ToId), k,
    Opt(w.Connect.HasRealizingId, w.Connect.RealizingId).Map(NodeId.Create),
    Opt(w.Connect.HasInterfaceKey, w.Connect.InterfaceKey).Map(ToKey))),
  RelationshipWire.EdgeOneofCase.Void => Row(VoidKind.TryGet(w.Void.SubKind, out VoidKind? vk), vk, w.Void.SubKind, key)
   .Map(k => (Relationship)new Relationship.Void(NodeId.Create(w.Void.HostId), NodeId.Create(w.Void.FeatureId), k)),
  RelationshipWire.EdgeOneofCase.Generic => ToValueMap(w.Generic.Attributes, key)
   .Map(attributes => (Relationship)new Relationship.Generic(
    w.Generic.WireName, NodeId.Create(w.Generic.RelatingId), NodeId.Create(w.Generic.RelatedId), attributes,
    toSeq(w.Generic.Participants).Map(participant => new RelationshipParticipant(
     NodeId.Create(participant.NodeId), participant.Role, Opt(participant.HasOrdinal, participant.Ordinal))))),
  _ => ElementFault.ValueRejected(key, "<wire-edge-none>"),
 };

 // Build the tree raw off the closed ValueCase, then ONE PropertyValue.Of at the envelope — Of recurses the
 // composites itself, so the structural admission runs exactly once over the whole decoded value.
 internal static Fin<PropertyValue> ToValue(PropertyValueWire w, Op key) => RawValue(w, key).Bind(v => PropertyValue.Of(v, key));

 static Fin<PropertyValue> RawValue(PropertyValueWire w, Op key) => w.ValueCase switch {
  PropertyValueWire.ValueOneofCase.Text => Fin.Succ((PropertyValue)new PropertyValue.Text(w.Text)),
  PropertyValueWire.ValueOneofCase.Measure => ToMeasure(w.Measure, key).Map(static m => (PropertyValue)new PropertyValue.Measure(m)),
  PropertyValueWire.ValueOneofCase.Boolean => Fin.Succ((PropertyValue)new PropertyValue.Boolean(w.Boolean)),
  PropertyValueWire.ValueOneofCase.Logical => Fin.Succ((PropertyValue)new PropertyValue.Logical(Opt(w.Logical.HasValue, w.Logical.Value))),
  PropertyValueWire.ValueOneofCase.Enumerated => toSeq(w.Enumerated.Selected).TraverseM(v => RawValue(v, key)).As().Bind(selected =>
   toSeq(w.Enumerated.Allowed).TraverseM(v => RawValue(v, key)).As().Map(allowed => (PropertyValue)new PropertyValue.Enumerated(selected, allowed))),
  PropertyValueWire.ValueOneofCase.Reference => Fin.Succ((PropertyValue)new PropertyValue.Reference(NodeId.Create(w.Reference.TargetId), Opt(w.Reference.HasUsageName, w.Reference.UsageName))),
  PropertyValueWire.ValueOneofCase.Bounded =>
   (OptMeasure(w.Bounded.Lower, key), OptMeasure(w.Bounded.Upper, key), OptMeasure(w.Bounded.SetPoint, key))
    .Apply(static (lower, upper, setPoint) => (PropertyValue)new PropertyValue.Bounded(lower, upper, setPoint)).As(),
  PropertyValueWire.ValueOneofCase.List => toSeq(w.List.Values).TraverseM(v => RawValue(v, key)).As().Map(vs => (PropertyValue)new PropertyValue.List(vs)),
  PropertyValueWire.ValueOneofCase.Table => Row(Interpolation.TryGet(w.Table.Interpolation, out Interpolation? rule), rule, w.Table.Interpolation, key)
   .Bind(interp => toSeq(w.Table.Rows).TraverseM(r => RawValue(r.Defining, key).Bind(d => RawValue(r.Defined, key).Map(x => (Defining: d, Defined: x)))).As()
    .Map(rows => (PropertyValue)new PropertyValue.Table(rows, interp))),
  PropertyValueWire.ValueOneofCase.Complex => toSeq(w.Complex.Properties).TraverseM(p => RawValue(p.Value, key).Map(v => (Name: PropertyName.Create(p.Key), Value: v))).As()
   .Bind(pairs => Named(pairs, key))
   .Map(properties => (PropertyValue)new PropertyValue.Complex(w.Complex.UsageName, properties)),
  PropertyValueWire.ValueOneofCase.Temporal => ToTemporal(w.Temporal, key).Map(static t => (PropertyValue)new PropertyValue.Temporal(t)),
  PropertyValueWire.ValueOneofCase.Integer => Fin.Succ((PropertyValue)new PropertyValue.Integer(new BigInteger(w.Integer.Span, isUnsigned: false, isBigEndian: true))),
  PropertyValueWire.ValueOneofCase.Number => Fin.Succ((PropertyValue)new PropertyValue.Number(w.Number)),
  PropertyValueWire.ValueOneofCase.Binary => Fin.Succ((PropertyValue)new PropertyValue.Binary(toSeq(w.Binary.ToByteArray()))),
  _ => ElementFault.ValueRejected(key, "<wire-value-none>"),
 };

 // TemporalValue arms re-admit through NodaTime ISO patterns (the seam Iso() canon reversed); a malformed
 // token rails ValueRejected, the epoch stamp rides the Timestamp adapter untouched.
 static Fin<TemporalValue> ToTemporal(TemporalWire w, Op key) => w.ValueCase switch {
  TemporalWire.ValueOneofCase.Date => Iso(NodaTime.Text.LocalDatePattern.Iso, w.Date, key).Map(static v => (TemporalValue)new TemporalValue.Date(v)),
  TemporalWire.ValueOneofCase.Moment => Iso(NodaTime.Text.LocalDateTimePattern.ExtendedIso, w.Moment, key).Map(static v => (TemporalValue)new TemporalValue.Moment(v)),
  TemporalWire.ValueOneofCase.Time => Iso(NodaTime.Text.LocalTimePattern.ExtendedIso, w.Time, key).Map(static v => (TemporalValue)new TemporalValue.Time(v)),
  TemporalWire.ValueOneofCase.Span => Iso(NodaTime.Text.PeriodPattern.Roundtrip, w.Span, key).Map(static v => (TemporalValue)new TemporalValue.Span(v)),
  TemporalWire.ValueOneofCase.Stamp => Fin.Succ((TemporalValue)new TemporalValue.Stamp(w.Stamp.ToInstant())),
  _ => ElementFault.ValueRejected(key, "<wire-temporal-none>"),
 };

 static Fin<T> Iso<T>(NodaTime.Text.IPattern<T> pattern, string token, Op key) =>
  pattern.Parse(token) is { Success: true } parsed ? Fin.Succ(parsed.Value) : ElementFault.ValueRejected(key, $"<wire-temporal:{token}>");

 internal static Fin<MaterialUsage> ToUsage(MaterialUsageWire? w, Op key) => w?.UsageCase switch {
  MaterialUsageWire.UsageOneofCase.None => Fin.Succ((MaterialUsage)new MaterialUsage.None()),
  MaterialUsageWire.UsageOneofCase.LayerSet =>
   from direction in Row(LayerSetDirection.TryGet(w.LayerSet.Direction, out LayerSetDirection? direction), direction, w.LayerSet.Direction, key)
   from sense in Row(DirectionSense.TryGet(w.LayerSet.Sense, out DirectionSense? sense), sense, w.LayerSet.Sense, key)
   from offset in OptMeasure(w.LayerSet.OffsetFromReferenceLine, key)
   from extent in OptMeasure(w.LayerSet.ReferenceExtent, key)
   from usage in MaterialUsage.LayerSet.Of(direction, sense, offset, extent, key)
   select usage,
  MaterialUsageWire.UsageOneofCase.ProfileSet =>
   from extent in OptMeasure(w.ProfileSet.ReferenceExtent, key)
   from usage in MaterialUsage.ProfileSet.Of(Opt(w.ProfileSet.HasCardinalPoint, w.ProfileSet.CardinalPoint), extent, key)
   select usage,
  null => ElementFault.ValueRejected(key, "<wire-usage-unset>"),
  _ => ElementFault.ValueRejected(key, "<wire-usage-unknown>"),
 };

 // The three required header messages are INDEPENDENT admissions gated by name — proto3 message presence is
 // nullness, so each rides Present before its read — and the unit map lands through toMap: both key spaces are
 // ordinal and the parser already deduped, so no narrowing exists to gate.
 internal static Fin<Header> ToHeader(HeaderWire w, Op key) =>
  !ReleaseVersion.TryGet(w.Schema, out ReleaseVersion? schema) ? ElementFault.ValueRejected(key, $"<wire-schema:{w.Schema}>")
  : !ModelView.TryGet(w.View, out ModelView? view) ? ElementFault.ValueRejected(key, $"<wire-view:{w.View}>")
  : (Present(w.GeoReference, "header.geo_reference", key).Bind(geo => ToGeoReference(geo, key)),
     Present(w.At, "header.at", key),
     Present(w.Step, "header.step", key))
    .Apply((geo, at, step) => new Header(schema!, view!, geo, w.Tolerance, at.ToInstant(),
     new StepHeader(toSeq(step.Descriptions), step.Name, step.TimeStamp.ToInstant(), toSeq(step.Authors),
      toSeq(step.Organizations), step.Preprocessor, step.OriginatingSystem, toSeq(step.Schema)),
     new UnitScheme(toMap(toSeq(w.UnitScheme).Map(static p => (p.Key, p.Value)))))).As();

 // --- [DECODE_PAYLOADS] — per-payload re-admission over the verified seam factories.
 static Fin<Node> ToObject(NodeId id, ObjectWire w, Op key) =>
  !ObjectKind.TryGet(w.Kind, out ObjectKind? kind) ? ElementFault.ValueRejected(key, $"<wire-object-kind:{w.Kind}>")
  : Present(w.Classification, "object.classification", key).Bind(row => ToClassification(row, key)).Bind(primary =>
    toSeq(w.Classifications).TraverseM(c => ToClassification(c, key)).As().Bind(secondary =>
     Present(w.Span, "object.span", key).Bind(s => ToSpan(s, key)).Map(span => (Node)new Node.Object(
      id, kind!, Opt(w.HasExternalId, w.ExternalId), primary, PredefinedType.Create(w.PredefinedType),
      Opt(w.HasObjectType, w.ObjectType), w.Name, w.Tag,
      new RepresentationContentHash(toMap(toSeq(w.Representations).Map(static p => (p.Key, ToKey(p.Value))))),
      Optional(w.History).Map(h => new OwnerHistory(h.OwningUser, h.OwningApplication, h.Created.ToInstant(),
       Optional(h.Modified).Map(static m => m.ToInstant()), h.ChangeAction, h.State)),
      span, secondary, ToPlacement(w.Placement)))));

 // The frame re-admits through its own kernel factory: the nine columns are free reals under no seam gate (a placement
 // carries no tolerance and no invariant — the canonical-bytes exclusion at its owner is what makes it free), so
 // message presence answers the whole decision and no rail is owed. The bare Vector3 is the enclosing namespace's
 // seam coordinate, never the System.Numerics carrier the prelude also has in scope.
 static Option<PlacementTransform> ToPlacement(PlacementWire? w) =>
  w is null
   ? None
   : Some(PlacementTransform.Create(
      new Vector3(w.LocationX, w.LocationY, w.LocationZ),
      new Vector3(w.AxisX, w.AxisY, w.AxisZ),
      new Vector3(w.RefDirectionX, w.RefDirectionY, w.RefDirectionZ)));

 static Fin<Node> ToMaterial(NodeId id, MaterialWire w, Op key) =>
  Present(w.Composition, "material.composition", key).Bind(c => ToComposition(c, key)).Bind(composition =>
   toSeq(w.PropertySets).TraverseM(p => ToPropertySet(p, key)).As().Map(sets =>
    (Node)new Node.Material(id, MaterialId.Of(w.MaterialKey), composition, sets)));

 // Every arm re-enters the seam Of* admission (the row-count, thickness, priority-range, offset-arity, and normalization
 // gates hold for hostile wire bytes exactly as for an in-process author), and each optional row column reads through the
 // generated Has* presence probe — a defaulted zero priority or false ventilation never forges an author's value. The
 // ProfileSet arm admits the rows FIRST and stamps the baked section afterwards through WithSection, so the private-ctor
 // case is never constructed directly and the head-row derivation stays total.
 static Fin<MaterialComposition> ToComposition(MaterialCompositionWire w, Op key) => w.CompositionCase switch {
  MaterialCompositionWire.CompositionOneofCase.Single => Fin.Succ(MaterialComposition.OfSingle(MaterialId.Of(w.Single.MaterialKey))),
  MaterialCompositionWire.CompositionOneofCase.LayerSet =>
   toSeq(w.LayerSet.Layers).TraverseM(l => ToMeasure(l.Thickness, key).Map(t => new MaterialLayer(
     MaterialId.Of(l.MaterialKey), t, l.LayerName,
     Opt(l.HasPriority, l.Priority), l.Category,
     Opt(l.HasVentilated, l.Ventilated)))).As()
    .Bind(layers => MaterialComposition.OfLayerSet(layers, key)),
  MaterialCompositionWire.CompositionOneofCase.ProfileSet =>
   from profiles in toSeq(w.ProfileSet.Profiles).TraverseM(p => ToProfile(p, key)).As()
   from composite in Optional(w.ProfileSet.Composite).Traverse(c => ToProfileRef(c, key)).As()
   from admitted in MaterialComposition.OfProfileSet(profiles, key, composite)
   from section in Optional(w.ProfileSet.Section).Traverse(s => ToSection(s, key)).As()
   select section.Match(Some: admitted.WithSection, None: () => admitted),
  MaterialCompositionWire.CompositionOneofCase.ConstituentSet => MaterialComposition.OfConstituentSet(
   toSeq(w.ConstituentSet.Constituents).Map(c => new MaterialConstituent(MaterialId.Of(c.MaterialKey), c.Category, c.Fraction, c.PartName)), key),
  _ => ElementFault.ValueRejected(key, "<wire-composition-none>"),
 };

 // One compound-profile row: every offset re-crosses the MeasureValue finite gate beside the row's own ProfileRef admission.
 static Fin<MaterialProfile> ToProfile(MaterialProfileWire w, Op key) =>
  from row in Present(w.Profile, "profile.ref", key)
  from profile in ToProfileRef(row, key)
  from offsets in toSeq(w.Offsets).TraverseM(o => ToMeasure(o, key)).As()
  select new MaterialProfile(MaterialId.Of(w.MaterialKey), profile, Opt(w.HasPriority, w.Priority), w.Category, offsets);

 // ONE ProfileRef admission serves the row and the set-level composite: Rehydrate re-derives the content key off the
 // normalized (standard, designation) and rails when a persisted key disagrees, so no wire leg trusts a carried digest.
 static Fin<ProfileRef> ToProfileRef(ProfileRefWire w, Op key) =>
  ProfileRef.Rehydrate(w.Standard, w.Designation, ToKey(w.ContentKey), key);

 // ONE column table owns the section's measured run: each row pairs the wire slot's own name with its accessor, and
 // ROW POSITION is simultaneously the traversal order, the frozen SectionPropertiesWire field order, and the ctor
 // position — so a slot moves once and both directions follow. The slot name is load-bearing on the rail: a
 // non-finite column names ITSELF rather than reporting the quantity token nineteen columns share. The positional
 // rebuild survives because a C# constructor takes no splat; the table's own order is what pins it, and the arity is
 // proved by the table rather than restated.
 static readonly (string Slot, Func<SectionPropertiesWire, MeasureValueWire> Read)[] SectionColumns = [
  ("area", static w => w.Area), ("iyy", static w => w.Iyy), ("izz", static w => w.Izz), ("j", static w => w.J),
  ("iw", static w => w.Iw), ("wely", static w => w.Wely), ("welz", static w => w.Welz), ("wply", static w => w.Wply),
  ("wplz", static w => w.Wplz), ("av-y", static w => w.AvY), ("av-z", static w => w.AvZ),
  ("radius-of-gyration-major", static w => w.RadiusOfGyrationMajor), ("radius-of-gyration-minor", static w => w.RadiusOfGyrationMinor),
  ("depth", static w => w.Depth), ("width", static w => w.Width), ("heated-perimeter", static w => w.HeatedPerimeter),
  ("axis-distance", static w => w.AxisDistance), ("shear-centre-y", static w => w.ShearCentreY), ("shear-centre-z", static w => w.ShearCentreZ)];

 // Nineteen measure columns re-cross the OfSi finite gate, which a Mapperly partial cannot thread, and they accumulate:
 // a datasheet with three bad columns names all three, matching the owning admission's own accumulating shape.
 static Fin<SectionProperties> ToSection(SectionPropertiesWire w, Op key) =>
  toSeq(SectionColumns)
   .Traverse(column => Present(column.Read(w), $"section.{column.Slot}", key)
    .Bind(cell => ToMeasure(cell, key))
    .MapFail(_ => (Error)ElementFault.ValueRejected(key, $"<wire-section-column:{column.Slot}>"))
    .ToValidation())
   .As().ToFin()
   .Map(m => new SectionProperties(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15], m[16], m[17], m[18], w.MonosymmetryFactor));

 // Every arm re-enters the canonical MaterialPropertySet.Of* admission rail — the decoder NEVER constructs a case
 // directly, so the physical bounds, finite gates, matrix arity, and cross-field refinements the owner declares hold
 // for hostile wire bytes exactly as for an in-process author; the raw-double columns pass through verbatim and the
 // measured columns re-cross as admitted MeasureValues (or their SI scalars where the owner mints the type itself).
 static Fin<MaterialPropertySet> ToPropertySet(MaterialPropertySetWire w, Op key) =>
  Present(w.Evidence, "property-set.evidence", key)
   .Bind(e => ToDate(e.HasValidUntil, e.ValidUntil, key).Map(validUntil => new PropertyEvidence(e.Source, e.Reference, validUntil)))
   .Bind(evidence => {
   return w.PropertySetCase switch {
    MaterialPropertySetWire.PropertySetOneofCase.Mechanical =>
     (ToMeasure(w.Mechanical.Density, key), ToMeasure(w.Mechanical.YoungsModulus, key), ToMeasure(w.Mechanical.YieldStrength, key), ToMeasure(w.Mechanical.UltimateStrength, key), OptCurve(w.Mechanical.YoungsReduction, key), OptCurve(w.Mechanical.YieldReduction, key))
      .Apply(static (density, youngs, yield, ultimate, youngsReduction, yieldReduction) => (density, youngs, yield, ultimate, youngsReduction, yieldReduction)).As()
      .Bind(t => MaterialPropertySet.OfMechanical(t.density, t.youngs, t.yield, t.ultimate, w.Mechanical.PoissonsRatio, w.Mechanical.ThermalExpansionPerK, key, evidence, t.youngsReduction, t.yieldReduction)),
    MaterialPropertySetWire.PropertySetOneofCase.Orthotropic =>
     (ToMeasure(w.Orthotropic.Density, key), ToMeasure(w.Orthotropic.E1Parallel, key), ToMeasure(w.Orthotropic.E2Perpendicular, key), ToMeasure(w.Orthotropic.ShearModulus, key), ToMeasure(w.Orthotropic.Strength1Parallel, key), ToMeasure(w.Orthotropic.Strength2Perpendicular, key), OptCurve(w.Orthotropic.ModulusReduction, key), OptCurve(w.Orthotropic.StrengthReduction, key))
      .Apply(static (density, e1, e2, shear, f1, f2, modulusReduction, strengthReduction) => (density, e1, e2, shear, f1, f2, modulusReduction, strengthReduction)).As()
      .Bind(t => MaterialPropertySet.OfOrthotropic(t.density, t.e1, t.e2, t.shear, t.f1, t.f2, w.Orthotropic.ThermalExpansionPerK, key, evidence, t.modulusReduction, t.strengthReduction)),
    MaterialPropertySetWire.PropertySetOneofCase.Thermal =>
     (ToMeasure(w.Thermal.Conductivity, key), ToMeasure(w.Thermal.SpecificHeat, key), OptMeasure(w.Thermal.UValue, key), OptCurve(w.Thermal.ConductivityCurve, key))
      .Apply(static (conductivity, specificHeat, uValue, conductivityCurve) => (conductivity, specificHeat, uValue, conductivityCurve)).As()
      .Bind(t => MaterialPropertySet.OfThermal(t.conductivity, t.specificHeat, t.uValue, w.Thermal.VapourResistanceFactor, key, evidence, t.conductivityCurve)),
    MaterialPropertySetWire.PropertySetOneofCase.Acoustic => Acoustic.Of(
     w.Acoustic.AbsorptionSpectrum.ToArray(), w.Acoustic.SoundReductionIndexDb.ToArray(), key,
     Opt(w.Acoustic.HasDynamicStiffnessMnPerM3, w.Acoustic.DynamicStiffnessMnPerM3), Opt(w.Acoustic.HasFlowResistivityPaSPerM2, w.Acoustic.FlowResistivityPaSPerM2), Opt(w.Acoustic.HasLossFactor, w.Acoustic.LossFactor))
     .Map(spectrum => MaterialPropertySet.OfAcoustic(spectrum, evidence)),
    // Absent reactions ride the 2-arg OfFire (NotSpecified sub-classes by construction); a present token admits
    // its full EN 13501-1 classification, the three INDEPENDENT token gates accumulating applicatively so a
    // hostile record with a bad rating AND a bad sub-class names both in one failure.
    MaterialPropertySetWire.PropertySetOneofCase.Fire => Present(w.Fire.Resistance, "fire.resistance", key)
     .Bind(r => FireResistance.Of(
      Opt(r.HasLoadBearingMinutes, r.LoadBearingMinutes),
      Opt(r.HasIntegrityMinutes, r.IntegrityMinutes),
      Opt(r.HasInsulationMinutes, r.InsulationMinutes), key))
     .Bind(resistance => !w.Fire.HasReaction
      ? Fin.Succ(MaterialPropertySet.OfFire(None, resistance, evidence))
      : (FireRating.Parse(w.Fire.Reaction, key),
         Row(SmokeClass.TryGet(w.Fire.Smoke, out SmokeClass? sc), sc, w.Fire.Smoke, key),
         Row(DropletClass.TryGet(w.Fire.Droplets, out DropletClass? dc), dc, w.Fire.Droplets, key))
         .Apply((reaction, smoke, droplets) => MaterialPropertySet.OfFire(reaction, smoke, droplets, resistance, evidence)).As()),
    MaterialPropertySetWire.PropertySetOneofCase.Environmental => MeasurementBasis.Parse(w.Environmental.Basis, key).Bind(basis =>
     MaterialPropertySet.OfEnvironmental(basis, [.. w.Environmental.Impacts],
      Opt(w.Environmental.HasRecycledContent, w.Environmental.RecycledContent),
      Opt(w.Environmental.HasEndOfLifeRecovery, w.Environmental.EndOfLifeRecovery), key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Cost => MeasurementBasis.Parse(w.Cost.Basis, key).Bind(basis =>
     Currency.Parse(w.Cost.Currency, key).Bind(currency =>
      MaterialPropertySet.OfCost(basis, currency, w.Cost.SupplyPerUnit, w.Cost.InstallPerUnit, w.Cost.LifecyclePerUnit, key, evidence))),
    MaterialPropertySetWire.PropertySetOneofCase.Damping => MaterialPropertySet.OfDamping(
     w.Damping.DampingRatio, Optional(w.Damping.Rayleigh).Map(static r => (r.AlphaPerS, r.BetaS)), key, evidence),
    MaterialPropertySetWire.PropertySetOneofCase.Hygrothermal =>
     (ToMeasure(w.Hygrothermal.WaterContent80Rh, key), ToMeasure(w.Hygrothermal.FreeWaterSaturation, key),
      OptCurve(w.Hygrothermal.SorptionIsotherm, key), OptCurve(w.Hygrothermal.LiquidTransport, key), OptCurve(w.Hygrothermal.MoistureConductivity, key))
      .Apply(static (waterContent, saturation, sorption, liquid, conductivity) => (waterContent, saturation, sorption, liquid, conductivity)).As()
      .Bind(t => MaterialPropertySet.OfHygrothermal(w.Hygrothermal.Porosity, t.waterContent.Si, t.saturation.Si,
       Opt(w.Hygrothermal.HasWaterAbsorptionKgPerM2SqrtS, w.Hygrothermal.WaterAbsorptionKgPerM2SqrtS), key, evidence, t.sorption, t.liquid, t.conductivity)),
    MaterialPropertySetWire.PropertySetOneofCase.Durability =>
     ToMeasure(w.Durability.ChlorideDiffusion, key).Bind(chloride => MaterialPropertySet.OfDurability(
      w.Durability.CarbonationRateMmPerSqrtYear, chloride.Si, w.Durability.AgeingExponent, key, evidence)),
    MaterialPropertySetWire.PropertySetOneofCase.Optical => MaterialPropertySet.OfOptical(
     w.Optical.VisibleTransmittance, w.Optical.VisibleReflectanceFront, w.Optical.VisibleReflectanceBack, w.Optical.SolarTransmittance, w.Optical.SolarReflectanceFront, w.Optical.SolarReflectanceBack, w.Optical.ThermalIrTransmittance, w.Optical.ThermalIrEmissivityFront, w.Optical.ThermalIrEmissivityBack, key, evidence),
    // Both measured columns re-cross the decode measure gate, then pass their SI scalars into the owner's own
    // admission — resistivity re-entering the registry ElectricResistivity mint at its OhmMeter base, the breakdown
    // field the DielectricStrength OfSi mint (Ω·m and V/m ARE the SI bases, so both scalars cross verbatim — the
    // Durability chloride-diffusion shape); the optional μr rides the generated presence probe, never a defaulted unity.
    MaterialPropertySetWire.PropertySetOneofCase.Electrical =>
     (ToMeasure(w.Electrical.Resistivity, key), OptMeasure(w.Electrical.DielectricStrength, key))
      .Apply(static (resistivity, dielectric) => (resistivity, dielectric)).As()
      .Bind(t => MaterialPropertySet.OfElectrical(
       t.resistivity.Si, w.Electrical.RelativePermittivity, t.dielectric.Map(static m => m.Si),
       Opt(w.Electrical.HasMagneticPermeabilityRelative, w.Electrical.MagneticPermeabilityRelative), key, evidence)),
    _ => ElementFault.ValueRejected(key, "<wire-material-property-none>"),
   };
  });

 static Fin<AssessmentPayload> ToAssessment(AssessmentWire w, Op key) =>
  from discipline in Discipline.Parse(w.Discipline, key)
  from route in AnalysisRoute.Of(w.Route, key)
  from outcome in Row(AssessmentOutcome.TryGet(w.Outcome, out AssessmentOutcome? state), state, w.Outcome, key)
  from results in ToValueMap(w.Results, key)
  from diagnostic in ToDiagnostic(w.Diagnostic, key)
  from audit in Present(w.Provenance, "assessment.provenance", key)
  from provenance in ToProvenance(audit, key)
  from payload in AssessmentPayload.Rehydrate(
   discipline, route, ToKey(w.InputKey), outcome, results, diagnostic,
   Opt(w.HasResultBlob, w.ResultBlob).Map(ToKey), provenance, key,
   toSeq(w.DependsOnIds).Map(NodeId.Create))
  select payload;

 // ToObservation decodes the measured series: every token re-crosses its generated row gate, every required message
 // column and every flattened window rebuilds through the presence-and-order gate the BOUNDED NodaTime Interval both
 // seam ends require, and the whole run re-enters through Rehydrate — so the advancing-chunk, bracketing-window, and
 // census-coherence invariants re-prove against hostile input rather than riding the producer's word, and an unset
 // statistics or provenance message names itself on the rail instead of dereferencing inside the residual funnel.
 // Sample bytes stay in the object store; only content keys cross.
 static Fin<ObservationSeries> ToObservation(ObservationWire w, Op key) =>
  from sensor in SensorId.Of(w.Sensor, key)
  from sampling in Row(SamplingKind.TryGet(w.Sampling, out SamplingKind? kind), kind, w.Sampling, key)
  from window in ToInterval(w.WindowStart, w.WindowEnd, "observation.window", key)
  from chunks in toSeq(w.Chunks).TraverseM(chunk =>
   ToInterval(chunk.WindowStart, chunk.WindowEnd, "observation.chunk.window", key)
    .Map(span => new ObservationChunk(span, ToKey(chunk.SeriesKey), chunk.SampleCount))).As()
  from statistics in ToStatistics(w.Statistics, key)
  from provenance in ToSensorProvenance(w.Provenance, key)
  from series in ObservationSeries.Rehydrate(
   sensor, PropertyName.Create(w.Aspect), QuantityType.Create(w.Observed),
   Dimension.Create(w.DimLength, w.DimMass, w.DimTime, w.DimCurrent, w.DimTemperature, w.DimAmount, w.DimLuminousIntensity),
   w.CanonicalUnit, sampling,
   Optional(w.Cadence).Map(static c => c.ToNodaDuration()),
   window, chunks, statistics, provenance, key)
  select series;

 // Census keys re-cross the generated ObservationGrade gate, so an unknown grade rails rather than silently dropping
 // a bucket the completeness ratio then over-counts against; the summary message and its span column admit before
 // either read, since an absent summary is a decode refusal rather than an empty one.
 static Fin<SeriesStatistics> ToStatistics(SeriesStatisticsWire? w, Op key) =>
  from summary in Present(w, "observation.statistics", key)
  from span in Present(summary.Span, "observation.statistics.span", key)
  from census in toSeq(summary.Census).TraverseM(entry =>
   Row(ObservationGrade.TryGet(entry.Key, out ObservationGrade? grade), grade, entry.Key, key)
    .Map(row => (Grade: row, entry.Value))).As()
  from minimum in OptMeasure(summary.Minimum, key)
  from maximum in OptMeasure(summary.Maximum, key)
  from mean in OptMeasure(summary.Mean, key)
  from total in OptMeasure(summary.Total, key)
  select SeriesStatistics.Of(
   census.Fold(Map<ObservationGrade, int>(), static (map, entry) => map.AddOrUpdate(entry.Grade, entry.Value)),
   span.ToNodaDuration(), minimum, maximum, mean, total);

 static Fin<SensorProvenance> ToSensorProvenance(SensorProvenanceWire? w, Op key) =>
  from audit in Present(w, "observation.provenance", key)
  from calibrated in ToDate(audit.HasCalibratedAt, audit.CalibratedAt, key)
  from tolerance in Optional(audit.Tolerance).Traverse(band => ToBand(band, key)).As()
  select new SensorProvenance(audit.Manufacturer, audit.Model, audit.Serial, calibrated, tolerance);

 static Fin<CoverageGrid> ToCoverage(CoverageWire w, Op key) =>
  from kind in Row(CoverageKind.TryGet(w.Kind, out CoverageKind? row), row, w.Kind, key)
  from geo in Present(w.Crs, "coverage.crs", key)
  from crs in ToGeoReference(geo, key)
  from bands in toSeq(w.Bands).TraverseM(band => ToBand(band, key)).As()
  from grid in ToLattice(w.Grid, key)
  from overviews in toSeq(w.Overviews).TraverseM(overview =>
   ToLattice(overview.Grid, key).Map(lattice => new OverviewLevel(lattice, ToKey(overview.RasterKey), overview.BlockX, overview.BlockY))).As()
  from coverage in CoverageGrid.Of(
   kind, ToKey(w.RasterKey), grid, bands, crs, key,
   overviews,
   toSeq(w.Slices).Map(slice => new TimeSlice(slice.At.ToInstant(), ToKey(slice.RasterKey))),
   w.BaseBlockX, w.BaseBlockY)
  select coverage;

 // The placement RE-ADMITS through the kernel's own gate rather than crossing as trusted state: a wire whose affine
 // is non-invertible or whose census breaches the ceiling rails here, so a foreign encoder cannot hand this runtime
 // a lattice its own CellLattice.Of would refuse. The arity gate is the wire's, because a repeated field carries no
 // fixed length and a short affine would otherwise index past its own array; the census crosses the SAME rail through
 // AcceptValidated, because the generated Create THROWS on a non-positive axis and a foreign encoder owns that int.
 static Fin<CellLattice> ToLattice(CellLatticeWire? w, Op key) =>
  w is { Affine.Count: 12 } wire
   ? from columns in key.AcceptValidated<LatticeAxis>(candidate: wire.Columns)
     from rows in key.AcceptValidated<LatticeAxis>(candidate: wire.Rows)
     from layers in key.AcceptValidated<LatticeAxis>(candidate: wire.Layers)
     from lattice in CellLattice.Of([.. wire.Affine], columns, rows, layers, wire.Ceiling, key)
     select lattice
   : ElementFault.ValueRejected(key, $"<wire-lattice-affine-arity:{w?.Affine.Count ?? 0}>");

 // The two token gates are INDEPENDENT and accumulate applicatively; the half-open range and palette-overflow
 // gates then read the proved pair.
 static Fin<CoverageBand> ToBand(CoverageBandWire w, Op key) =>
  (Row(ChannelDtype.TryGet(w.SampleType, out ChannelDtype? st), st, $"{w.SampleType}", key),
   Row(BandRole.TryGet(w.Role, out BandRole? br), br, w.Role, key))
   .Apply(static (sampleType, role) => (sampleType, role)).As()
   .Bind(t =>
    w.HasRangeMin != w.HasRangeMax ? ElementFault.ValueRejected(key, "<wire-band-range-half-open>")
    : !w.Palette.All(static p => (p.R | p.G | p.B | p.A) <= 255u) ? ElementFault.ValueRejected(key, "<wire-band-palette-channel-overflow>")
    : toSeq(w.Palette).TraverseM(bin => PerceptualColor
       .OfRgb((byte)bin.R, (byte)bin.G, (byte)bin.B, alpha: bin.A / 255.0, key: key)
       .Map(colour => new ColorBin(bin.Index, colour, bin.Category))).As()
      .Map(palette => new CoverageBand(w.Index, w.Name, t.sampleType, t.role, Opt(w.HasNoData, w.NoData), w.Units, w.Offset, w.Scale,
       Opt(w.HasRangeMin, (w.RangeMin, w.RangeMax)), palette)));

 // A seam GeoReference is Identity (no CRS) or Admit-resolved (Some CRS) — the wire mirrors the closed pair: an
 // absent crs decodes ONLY to the exact Identity tuple (junk columns rail), a present crs re-admits in full; the
 // wire's derived epsg/resolution columns are peer-informative — the seam re-derives both through Admit.
 static Fin<GeoReference> ToGeoReference(GeoReferenceWire w, Op key) => GeoReference.Admit(
  w.Eastings, w.Northings, w.OrthogonalHeight,
  w.XAxisAbscissa, w.XAxisOrdinate, w.ScaleX, w.ScaleY, w.ScaleZ,
  w.GeodeticDatum, w.VerticalDatum,
  w.Crs?.Name ?? "", w.Crs?.Wkt ?? "", w.Crs?.MapProjection ?? "", w.Crs?.MapZone ?? "", key,
  Opt(w.HasEpoch, w.Epoch), Opt(w.HasVerticalEpsg, w.VerticalEpsg));

 // Absence is total through the Option traversal; a present diagnostic's two INDEPENDENT token gates accumulate.
 static Fin<Option<Diagnostic>> ToDiagnostic(DiagnosticWire? w, Op key) =>
  Optional(w).Traverse(d =>
   (Row(SolvePhase.TryGet(d.Phase, out SolvePhase? sp), sp, d.Phase, key),
    Row(FailureKind.TryGet(d.Kind, out FailureKind? fk), fk, d.Kind, key))
    .Apply(static (phase, kind) => (phase, kind)).As()
    .Bind(t => Diagnostic.Of(t.phase, t.kind, d.Message, key, Opt(d.HasCode, d.Code)))).As();

 // Message fields carry presence by nullness (proto3 message presence); the window is both-or-neither, and the
 // present pair rebuilds through the shared window gate so a reversed pair rails here rather than throwing inside
 // the NodaTime constructor. The instant and the elapsed span are required columns and admit by name.
 static Fin<Provenance> ToProvenance(ProvenanceWire w, Op key) {
  Guid correlation = default;
  return (w.WindowStart is null) != (w.WindowEnd is null)
   ? ElementFault.ValueRejected(key, "<wire-provenance-window-half-open>")
   : w.HasCorrelation && !Guid.TryParse(w.Correlation, out correlation)
    ? ElementFault.ValueRejected(key, $"<wire-provenance-correlation:{w.Correlation}>")
    : (from at in Present(w.At, "provenance.at", key)
       from elapsed in Present(w.Elapsed, "provenance.elapsed", key)
       from window in Optional(w.WindowStart).Traverse(start => ToInterval(start, w.WindowEnd, "provenance.window", key)).As()
       select new Provenance(w.Author, w.Tool, w.Version, at.ToInstant(), elapsed.ToNodaDuration(), window,
        Opt(w.HasCorrelation, CorrelationId.Create(correlation)), w.Attempt));
 }

 static Fin<PropertyBag> ToBag(PropertySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   ToValueMap(w.Values, key).Map(values => new PropertyBag(w.SetName, values, axes.Mode, axes.Rank)));

 static Fin<QuantityBag> ToBag(QuantitySetWire w, Op key) =>
  BagAxes(w.Inheritance, w.SourceRank, key).Bind(axes =>
   toSeq(w.Values).TraverseM(p => ToMeasure(p.Value, key).Map(m => (Name: PropertyName.Create(p.Key), Value: m))).As()
    .Bind(pairs => Named(pairs, key))
    .Map(values => new QuantityBag(w.SetName, values, axes.Mode, axes.Rank, ToGroups(w.Groups))));

 // The group run re-admits TOTAL: the three columns are free grouping text under no seam gate, so absence is the
 // whole decision each Has* presence pair answers and no rail is owed. A prefix naming no value row is admitted —
 // an authored group whose members a partial crossing omitted is data, not a malformed payload. The dot-path keys
 // are bare ORDINAL strings on both sides, so the parser-deduped run lands whole through toMap.
 static Map<string, GroupIdentity> ToGroups(IEnumerable<KeyValuePair<string, GroupIdentityWire>> entries) =>
  toMap(toSeq(entries).Map(static entry => (entry.Key, new GroupIdentity(
   Opt(entry.Value.HasDiscrimination, entry.Value.Discrimination),
   Opt(entry.Value.HasQuality, entry.Value.Quality),
   Opt(entry.Value.HasUsage, entry.Value.Usage)))));

 static Fin<(InheritanceMode Mode, PropertySource Rank)> BagAxes(string inheritance, int sourceRank, Op key) =>
  !InheritanceMode.TryGet(inheritance, out InheritanceMode? mode) ? ElementFault.ValueRejected(key, $"<wire-inheritance:{inheritance}>")
  : PropertySource.TryGet(sourceRank, out PropertySource? source) ? Fin.Succ((mode!, source!))
  : ElementFault.ValueRejected(key, $"<wire-source-rank:{sourceRank}>");

 static Fin<Map<PropertyName, PropertyValue>> ToValueMap(IEnumerable<KeyValuePair<string, PropertyValueWire>> entries, Op key) =>
  toSeq(entries).TraverseM(p => ToValue(p.Value, key).Map(v => (Name: PropertyName.Create(p.Key), Value: v))).As()
   .Bind(pairs => Named(pairs, key));

 static Fin<Classification> ToClassification(ClassificationWire w, Op key) =>
  ToDate(w.HasEditionDate, w.EditionDate, key).Bind(editionDate =>
   Classification.Of(w.System, w.Code, key, w.Edition,
    source: Opt(w.HasSource, w.Source), editionDate: editionDate, title: Opt(w.HasTitle, w.Title)));

 static Fin<SchemaSpan> ToSpan(SchemaSpanWire w, Op key) =>
  !ReleaseVersion.TryGet(w.IntroducedIn, out ReleaseVersion? introduced) ? ElementFault.ValueRejected(key, $"<wire-span-introduced:{w.IntroducedIn}>")
  : !w.HasRemovedIn ? Fin.Succ(new SchemaSpan(introduced!, None))
  : ReleaseVersion.TryGet(w.RemovedIn, out ReleaseVersion? removed) ? Fin.Succ(new SchemaSpan(introduced!, Some(removed!)))
  : ElementFault.ValueRejected(key, $"<wire-span-removed:{w.RemovedIn}>");

 static Fin<Option<NodaTime.LocalDate>> ToDate(bool present, string iso, Op key) =>
  Opt(present, iso).Traverse(token => Iso(NodaTime.Text.LocalDatePattern.Iso, token, key)).As();

 // Proto3 carries MESSAGE presence as nullness, so a column the schema declares non-optional still arrives unset
 // from a hostile producer and the residual funnel would report its dereference as an opaque throw. Present names
 // the missing column on the rail instead, and ToInterval pairs it with the ORDER proof the flattened window needs:
 // the NodaTime two-Instant constructor throws on a reversed pair and would fire before any seam gate reads it.
 static Fin<T> Present<T>(T? w, string column, Op key) where T : class =>
  w is not null ? Fin.Succ(w) : ElementFault.ValueRejected(key, $"<wire-message-absent:{column}>");

 static Fin<NodaTime.Interval> ToInterval(
  Google.Protobuf.WellKnownTypes.Timestamp? start, Google.Protobuf.WellKnownTypes.Timestamp? end, string column, Op key) =>
  from opened in Present(start, $"{column}.start", key)
  from closed in Present(end, $"{column}.end", key)
  from window in opened.ToInstant() <= closed.ToInstant()
   ? Fin.Succ(new NodaTime.Interval(opened.ToInstant(), closed.ToInstant()))
   : ElementFault.ValueRejected(key, $"<wire-window-reversed:{column}>")
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
   ? ElementFault.ValueRejected(key, $"<wire-property-name-collision:{pair.Name.Value}>")
   : Fin.Succ(m.Add(pair.Name, pair.Value))));

 // Untrusted wire token -> generated SmartEnum row: the generated TryGet composed once, the miss railed.
 static Fin<T> Row<T>(bool found, T? row, string token, Op key) where T : class =>
  found && row is not null ? Fin.Succ(row) : ElementFault.ValueRejected(key, $"<wire-token:{typeof(T).Name}:{token}>");
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
 // (InvalidProtocolBufferException) caught ONCE here and lowered to ValueRejected — never a leaked throw.
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
    ? ElementFault.DeltaConflict(key, "<wire-node-duplicate-id>")
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
          .Map(static step => step.Graph))))));

 // Verified decode checks the authoritative carried address and the node's own content-derived identity. A redaction
 // manifest suppresses this leg for the ids it declares unstable; consumers retain that roster as OCC ineligibility.
 static Fin<Node> AdmitNode(NodeWire wire, double tolerance, bool verify, Op key) =>
  WireCodec.ToNode(wire, key).Bind(node =>
   !verify ? Fin.Succ(node)
   : wire.ContentAddress.Length != 16
    ? ElementFault.AddressUnstable(key, $"<wire-content-address-width:{wire.Id}:{wire.ContentAddress.Length}>")
    : WireCodec.ToKey(wire.ContentAddress) != ContentAddress.Of(node, tolerance).Value
     ? ElementFault.AddressUnstable(key, $"<wire-content-address-mismatch:{wire.Id}>")
     : ContentAddress.Verify(node, tolerance, key).Map(_ => node));

 // Decoded deltas re-cross the IsNormalForm shape gate (a double-entry id or edge rails DeltaConflict — the
 // unique-per-id normal form Merge produces is an OBLIGATION on a foreign transcription, never assumed), and its
 // ONLY sanctioned application is AdmitOnto — ReplayOnto trusts a delta the seam's own algebra produced, which a
 // wire payload is not, so the structural edge law runs when the foreign delta lands on a graph.
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
     .Bind(delta => delta.IsNormalForm ? Fin.Succ(delta) : ElementFault.DeltaConflict(key, "<wire-delta-not-normal-form>"));
   }))));

 // Residual-throw funnel over protobuf/generated mapping code; typed inner faults pass untouched.
 static Fin<T> Funnel<T>(Op key, Func<Fin<T>> decode) =>
  key.Catch(decode).MapFail(e => e.IsExceptional ? (Error)ElementFault.ValueRejected(key, $"<wire-decode-throw:{e.Message}>") : e);

 static Fin<T> Parse<T>(MessageParser<T> parser, Stream payload, WireLimits limits, Op key) where T : class, IMessage<T> =>
  key.Catch(() => Fin.Succ(parser.ParseFrom(CodedInputStream.CreateWithLimits(payload, limits.SizeLimit, limits.RecursionLimit))))
   .MapFail(error => error.IsExceptional
    ? (Error)ElementFault.ValueRejected(key, $"<wire-parse:{error.Message}>")
    : error);
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
   ? ElementFault.ValueRejected(key, "<redaction-policy-blank>")
   : toSeq(ClassifiedColumn.Items).Filter(column => classes.Union(column.Classes).Equals(classes)) is { IsEmpty: false } claimed
    ? Fin.Succ(new RedactionScope(policy.Trim(), claimed))
    : ElementFault.ValueRejected(key, $"<redaction-scope-claims-nothing:{policy}>");

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
- [ADMISSION_AND_DEPTH_GATE]: `DecodeGraph` and `DecodeDelta` parse under positive `WireLimits`. Every decoded value re-crosses its owner gate before the aggregate reaches `AdmitOnto` or `IsNormalForm`. Duplicate node ids rail on a raw-id scan before value admission. Unset cases, unknown rows, invalid values, and illegal structure share the in-process typed rail.
- [EVENT_ENVELOPE]: `GraphCrossing` composes the kernel envelope owner whole — one mint, one Protobuf-framed encode, one decode — with `id` the composing rail's operation identity, `subject` the content key under the kernel `EventKey` spelling, `datacontenttype` derived from the body descriptor, and `dataclassification` from the egress scope; binding prefixes, content mode, and `dataref` residence own at the consuming binding; Protobuf streaming rides `WriteLengthPrefixedTo`/`WriteDelimitedTo`.
- [EGRESS_REDACTION]: a scoped crossing clears classified field paths on the encoded message and carries its `RedactionManifestWire`. Source content keys survive — no key re-derives over redacted bytes — and the verifying decode admits exactly the manifest-named nodes as declared-unstable while a drifted node outside that roster still faults `AddressUnstable`. A redacted crossing is a DISTINCT byte stream from its unredacted twin, so parity vectors are forged and compared unredacted and a redaction policy never enters a parity gate.
- [WIRE_BYTES_LAW]: wire bytes are a TRANSPORT stream, never the content-identity law — `ContentAddress.OfGraph` and the delta's own `ToCanonicalBytes` sort their sections and are the fingerprints the corpus reproduces. The encode leg is still deterministic wherever an order exists to own: the node run emits NodeId-ordinal because `FrozenDictionary` declares no enumeration order, and edge and delta runs publish recording order. `map<>` fields keep protobuf's unspecified cross-runtime order, so a byte-parity fixture over a map-bearing message is structurally unfreezable at three peers — sorted-repeated entry runs are the escalation form the day wire bytes must become an identity, not a landed one.
- [CONTRACT_EVOLUTION]: `rasm/element/v1/element.proto` is the descriptor source and the `[02]` frozen-number ledger is its append record — a new arm takes the ledger's next free number, never a re-derived one. Appended fields and new `oneof` arms are additive; renumbers, incompatible type changes, unreserved removals, and implicit-to-explicit presence flips on landed scalars are breaking (the three landed flips ride their one documented `buf.yaml` waiver). Whole-graph parity literals remain governed by `Graph/corpus`'s terminal research route until exact addresses exist.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
