# [BIM_EXPORT_PIPELINE]

`BimExport.Export` folds one TOTAL `InterchangeCodec.Switch` over the `ExportPayload` union — `Soup` the flat `ImportedGeometry` triangle carrier, `Scene` the content-keyed per-element `ElementScene` — dispatching GLB through SharpGLTF under Draco/meshopt encode, `.bim` through the `dotbim` instancing wire, FBX/Collada through `AssimpContext.ExportToBlob`, OpenUSD through `UniversalSceneDescription` `UsdStage`, and 3D-Tiles `.subtree` availability through `SubtreeCreator`; the Switch mirrors `import#IMPORT_PIPELINE`, so a new codec row BREAKS this call site at compile time.

`BimExport.Author` mints the per-element glTF scene as a `GlbScene` — one `NodeBuilder` per element NAMED by its shared GlobalId, one logical mesh per distinct content key, N repeats travelling as N nodes over ONE mesh with `EXT_mesh_gpu_instancing` a policy threshold — so the `GlobalId`→`Node` index `TileMetadata` and `AnimateSchedule` bind against is MINTED HERE, never caller-walked.

IFC STEP/XML/JSON never re-authors here: `ExportIfc` DELEGATES to the shared `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit` — the ONE Bim-internal `ElementGraph`→`DatabaseIfc` re-author — this path OWNING only the artifact seal (`ExportArtifact` with the Compute content key) and reading serialization off the `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column.

Settled vocabulary arrives from the shared `Graph/element#ELEMENT_GRAPH` `ElementGraph`/`Element` (a consumer reads the baked `Element`, never a stored record), the shared `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` carrier the `import#IMPORT_PIPELINE` produces and `BimIo.ImportIfc` re-decode, the `format#FORMAT_AXIS` codec/extension rows, and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` content key; a sealed `ExportArtifact` feeds that Compute boundary, and every emit stays HOST-LOCAL.

## [01]-[INDEX]

- [02]-[EXPORT_PIPELINE]: artifact emit — the `ExportPayload` `Soup`/`Scene` union through one TOTAL `InterchangeCodec.Switch`; the `GltfChannel` canonical-channel roster and the `MaterialFinish`/`ChannelImage` pooled material identity binding every texture map onto one `MaterialBuilder`; the IFC leg DELEGATING to the shared `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, this path owning only the `ExportArtifact` seal and the `InterchangeFormat.Serialization` column read.
- [03]-[TILE_METADATA]: per-tile `EXT_structural_metadata` schema/class/property-table over the shared `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, not a stored record), bound through `EXT_mesh_features` over the `Staged`-authored per-vertex `_FEATURE_ID_0` row stamps the `GlbScene.Rows` index names.
- [04]-[BIM_LOD]: `Meshopt.Simplify`/`SimplifySloppy` build the per-element LOD pyramid, `Meshopt.BuildMeshlets` bands meshlet residency, and each LOD carries the content key the `Rasm.Compute/Runtime/tiles#TILE_PARTITION` pyramid addresses.
- [05]-[SCHEDULE_ANIMATION]: `AnimateSchedule` bakes the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element glTF visibility/scale keyframe tracks through `ModelRoot.CreateAnimation` and the `KHR_node_visibility` channel over the `Author`-minted `GlbScene` `GlobalId`→`Node` index, the in-progress tint riding the `KHR_animation_pointer` material base-colour channel — a 4D schedule exports as one animated GLB a web viewer scrubs.
- [06]-[ROUNDTRIP]: `RoundTrip` folds an `ElementGraph` emit→`BimIo.ImportIfc` schema-sniffed re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations into one lossless-verification matrix, witnessing per-element fidelity by the shared content key joined on the 1:1 `ExternalId` and naming the divergent members through the `Generator.Equals` structured diff.
- [07]-[TILE_AVAILABILITY]: `TileAvailability` authors the 3D-Tiles 1.1 implicit-tiling `.subtree` availability bitstream over the `subtree` `SubtreeCreator`/`SubtreeCreator3D`/`Tile`/`Tile3D`/`MortonIndex` surface and witnesses it back through `SubtreeReader.ReadSubtree` onto a content-keyed `SubtreeArtifact`, completing the tileset side the `SharpGLTF.Ext.3DTiles` per-tile content leg cannot reach and retiring the hand-rolled implicit-tiling bitstream.
- [08]-[COBIE_EMIT]: `CobieEmit.Export` the COBie FM-handover XLSX author — a transient `CobieModel` folded `Instances.New<T>` from the shared `ElementGraph` (facility/floor/space, type/component, `CobieAttribute` rows) and sealed through `ExportToTable`, content-keyed off the kernel `ContentHash`; never a held xBIM `IModel` and never the parallel xBIM IFC reader.
- [09]-[SAF_EMIT]: `SafEmit.Export` the SAF structural-analysis XLSX author — the shared `ElementGraph` lowered through the `Exchange/saf#SAF_EXCHANGE` `Workbook` fold onto the SAF `ExcelModel` under the caller's stated `AnnexRegime` design code, validated and written by the `SafCodec.Run` export leg, sealed content-keyed as the `ExportArtifact` the codec Switch routes to.

## [02]-[EXPORT_PIPELINE]

- Owner: `ExportTrait` the `[SmartEnum<string>]` realizing kernel `ICapability<ExportTrait>` — the ONE render-and-write-layout vocabulary the interchange policy, the material finish, and the schedule-animation policy each hold a subset of, with `ExportCorner` declaring the three per-carrier `CapabilityLaw` corner sets; `MeshLanes` the position/normal evidence admitted once on the path so no encode capsule re-probes the arena; `BimExport` — the export fold over `InterchangeFormat`, one TOTAL generated `InterchangeCodec.Switch` over the `ExportPayload` union (`Soup(ImportedGeometry)` | `Scene(ElementScene)`), the IFC STEP/XML/JSON leg DELEGATING to the shared `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit` (this path seals the bytes, the projector owns the re-author); `ElementScene` the per-element carrier — a `Map<UInt128, ImportedGeometry>` content-keyed mesh pool with placed `ElementInstance` rows (GlobalId, name, classification code, mesh key, rigid `Matrix4x4` placement, `MaterialFinish`) — so repeated geometry travels ONCE; `GltfChannel` the `[SmartEnum<string>]` roster projecting a CANONICAL texture-channel name onto the glTF `KnownChannel` targets it binds and the `format#FORMAT_AXIS` `KhrExtension` it obliges; `MaterialFinish` the pooled material identity pairing the shared `Rasm.Element/Graph/element#NODE_MODEL` `AppearanceSummary` with its `ChannelImage` bindings and the `ExportTrait` set carrying its render representation, and owning the whole `MaterialBuilder` mint; `ChannelImage` one bound glTF texture map (its resolved `GltfChannel`, already-encoded bytes beside the core-container fallback bytes an extension-obliging primary degrades to, UV set, wrap pair, min/mag filter pair, optional `Semantics/appearance#APPEARANCE_PROJECTION` `UvTransform` frame, and the `KhrExtension` row its container obliges); `GlbScene` the `Author`-minted `(ModelRoot, Map<string, Node>, Map<string, int>)` triple carrying the per-element node index AND the `GlobalId`→feature-row index downstream legs bind; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing boundary.
- Entry: `BimExport.Export` is the ONE entry, its `ExportPayload` case discriminating flat-soup from per-element emit per MODAL_ARITY (a `bool perElement` knob beside the value is the rejected form); `BimExport.Author` mints the `GlbScene` the metadata/animation legs decorate before `Emit` seals it; `BimExport.ExportIfc` carries the IFC serialization — its `graph` a contract read snapshot, its `projector` the Bim-internal IFC-egress owner the app wires, its `EmitContext` riding through whole (the diff-prior `ChangeAction` snapshot [H9], the scoped partial-export selection, the declared unit regime), the profile store staying the projector's ctor-held capability. `Fin<T>` aborts on an export-capability miss (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Codec`), a route miss the total `Switch` names with its owning path in the message, an empty scene pool, a dangling mesh key, or an absent position/normal lane the `ElementScene` admission accumulates, or an arena re-mint refusal the `ExportPayload.Flat` flatten surfaces, or a captured serialization/predefined-gate fault the projector lowers (`BimFault.Refused` with `BimReason.Rejected`/`BimFault.Refused` with `BimReason.Unmapped`), each typed case (band 2600, `Fault`-derived) lifting BARE onto the result with no `.ToError()` hop.
- Auto: `GlbBytes` switches on `InterchangePolicy.Compression` — `KhrEncoder.None` routes `Soup` through the single-mesh `SceneOf` and `Scene` through `Author`, both writing the GLB container, while `Draco` and `Meshopt` bypass the container: neither compression codec takes a glTF `ModelRoot`, so the compression leg REPLACES the GLB write rather than post-processing it, and a per-element `Scene` flattens through `ElementScene.Soup` because the raw streams carry no scene graph (per-element structure rides the GLB arms only). `.bim` pools distinct geometry by content key and places each `ElementInstance`, so instancing survives the wire. IFC selects no serialization writer here — `ExportIfc` reads the `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column (`Some` exactly on the GeometryGym rows, its value the `Projection/wireform#IFC_WIRE_FORM` `IfcWireForm` row carrying serialization AND container together) and hands the shared `ElementGraph` and that wire form to `SemanticProjector.Emit`, which re-authors the whole graph and returns the IFC BYTES this path seals — plain STEP, the zipped `.ifczip` container, ifcXML, or ifcJSON, each the row's own seal, so no transcode and no zip happens on this path; no `DatabaseIfc` is constructed on this page.
- Events: `ExportArtifact.Sealed` fires the `Model/observability#HOOKS` `rasm.bim.exchange.exported` point with `BimFact.Exported` — the Compute-computed `ContentKey`, the `InterchangeFormat.Key`, the emitted byte count off the sealed `ExportArtifact`, and the emit wall duration — so the ONE seal every format arm funnels through is the ONE fire site; the CloudEvents announcement is `Exchange/events#EVENT_PROJECTION`'s observe subscription over that point, subject the content key, and a message envelope minted at this path is the deleted form.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, CommunityToolkit.HighPerformance, AssimpNetter, dotbim, UniversalSceneDescription, Rasm.Element (the shared `ImportedGeometry`/`MeshBlock`/`MeshletBand` carrier), Rasm (the kernel `Drawing` arena — `EncodedGeometry.Descriptors`/`Channel`, `Encode.Of`, the `EncodingChannel` roster, `ChannelDtype.Unpack`, `ContentHash`/`CanonicalWriter`; `Tolerance`/`ToleranceLane`; `ICapability`/`CapabilitySet`/`CapabilityLaw`), NodaTime, LanguageExt.Core
- Growth: a new managed export is one arm on the TOTAL `InterchangeCodec.Switch` — the compiler forces the arm the moment the codec row lands (the `dotBim` instancing arm joined the SharpGltf/SceneExchange/UsdStage arms this way), never a per-format exporter family and never a silent ladder tail; a new emit modality is one `ExportPayload` case every codec arm is compiler-forced to route; a new IFC serialization is the `InterchangeFormat.Serialization` column value on one GeometryGym row; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row the `Writables` narrowing admits; a new BOUND texture channel is one `GltfChannel` row naming its `KnownChannel` targets and its obliged extension — `ChannelImage.Of` admits it, `MaterialFinish.Author` binds it with zero arm, and the `Obliges` union declares the row — a new appearance FACTOR is one column on the shared `AppearanceSummary` read once in `Shaded`, a new RENDER-REPRESENTATION toggle (the sidedness bit is the landed one) is one column on `MaterialFinish` its own `Key` seed frames, and a new sampler, degradation, or transform axis is one column on `ChannelImage` the `MaterialFinish.Key` `Sampled` frame folds so the pool keeps discriminating it, never a second material builder; a new compression encoder is one `KhrEncoder` arm on the `GlbBytes` fold; a new PER-VERTEX attribute is one kernel `EncodingChannel` row — the descriptor-addressed `Lane` read, the `ElementScene.Pooled` union fold, and the `MeshoptLane` arity column carry it with no arm here, and giving it its own filter-coded meshopt stream is one `MeshoptLane` row; a new assimp export target is one scene-exchange row whose KEY is the `ExportToBlob` `exportFormatId` (`IsExportFormatSupported`-guarded).
- Boundary: dispatch is the generated exhaustive `Switch` — every codec row declares its export route or its route-naming fault (`ExportIfc` for GeometryGym, `Semantics/vector#VECTOR_FOLD` `GeoVector.Write` for geospatial, `SafEmit.Export` for the graph-sourced SAF row, `CobieEmit.Export` for COBie, the companion bridge for native/IGES), so no row falls into a stale miss tail. GLB emission is deterministic byte layout — `SceneBuilderSchema2Settings` strided/merged buffers off the policy trait set, the `GpuMeshInstancingMinCount` threshold, and the merge before write — so the same geometry always emits the same bytes the Compute content-key addresses. Every per-vertex attribute reaches this path through the shared carrier's ONE kernel `EncodedGeometry` arena and is read by DESCRIPTOR through the page's single `Lane`/`Required` pair, each lane lifted to floats by its own `ChannelDtype` — so a hand-derived stride, a per-channel column read, and a second lane arena on this path are all deleted forms, and a widened channel arity reaches every arm as data. Absence is a MISSING DESCRIPTOR, never an empty buffer: the parameterization axis the mesh-builder layout, the Draco attribute set, the meshopt stream roster, and the LOD weight vector each discriminate on is the contract's own evidence, so a length probe — which a zero-filled forged unwrap passes — is unspellable here. `ElementScene.Pooled` re-mints the pooled arena through the kernel `Encode.Of` over the UNION of its entries' declared channels (an entry lacking a lane leaves its range at that channel's zero, mirroring the import path's own pool builder), so the flatten rides `Fin` and the arena's arity screen and per-lane round-trip witness gate the pooled result rather than a hand-assembled carrier reaching a codec. `Author` is the ONE `GlobalId`→`Node` index minter (nodes named by the shared `Object.ExternalId`, read back from `ModelRoot.LogicalNodes`), the ONE feature-row stamp minter (`GlbScene.Rows` ordinals stamped `_FEATURE_ID_0` at `Staged` — the only point the vertex layout is open), AND the ONE material minter (`ElementInstance.Finish` authors its own pooled `MaterialBuilder` — the shared `AppearanceSummary` as linear factors and every bound map through its own `ChannelImage.Bind`, materials pooled per distinct finish KEY so a textured element never inherits its untextured neighbour's material, uniform-finish repeats keeping theirs — the GLB arm erasing the color the dotbim arm round-trips was the deleted asymmetry); a caller-walked scene graph, a second index mint, a post-hoc attribute write, or an image ENCODE on this path is the deleted form — the texture bytes arrive already sealed by their owner and this path binds them. `AppearanceSummary` sources every glTF FACTOR and holds each channel in its declared domain: base colour is scene-linear and enters `baseColorFactor` unencoded (routing the display-referred dotbim byte tint into that linear slot is the same unlinearized pass-through `Semantics/appearance#APPEARANCE_PROJECTION` names as deleted on the ingest side), metalness and roughness are written on EVERY material because the glTF factor defaults are both 1.0 and an unwritten material renders as rough metal, opacity below unity selects `AlphaMode.BLEND`, and the `Transmissive` bit writes `KHR_materials_transmission` and NEVER alpha mode — so an opaque-alpha glass round-trips its transmission exactly as the IFC `IfcSurfaceStyleRefraction` egress does. Sidedness is the one RENDER-REPRESENTATION trait beside those factors and it rides the `MaterialFinish` `CapabilitySet<ExportTrait>` rather than a summary column, because it selects which faces the material paints where every summary channel answers how a painted face reflects — its producer is the `Semantics/appearance#APPEARANCE_PROJECTION` `StyledAppearance.DoubleSided` bit the source's own `IfcSurfaceSide` declares, it frames into the pool `Key` so a two-sided and a one-sided element never share a `MaterialBuilder`, and `WithDoubleSide` writes on every material because glTF's default is FALSE and an unwritten thin panel culls from inside the model. `GltfChannel` alone owns the canonical-channel-to-`KnownChannel` correspondence: a call site choosing a `KnownChannel` is the unowned projection the roster deletes, a canonical name with no row REFUSES at `ChannelImage.Of` rather than lighting a nearest slot, and the `orm` pack binds ONE `ImageBuilder` onto both the occlusion and metallic-roughness channels because glTF reads one image through two references. `KhrExtension` rows roster only extensions a finish fills, deleting the phantom row — the roster is the caller's declared write capability, the payload's `Obliges` is the truth, and the two union at registration. `EXT_mesh_gpu_instancing` collapse is a POLICY threshold because a gpu-merged node loses its per-node visibility/metadata identity (the 4D/metadata pipeline runs `GpuInstancingMinCount: 0`, the streaming-tile pipeline raises it — a policy value, never a code fork). `KhrExtension` in-box rows serialize through SharpGLTF's own schema types with no registration call (the process-global `ExtensionsFactory` carries the in-box KHR/EXT set; the per-row `Registrar` closure exists ONLY for a caller-supplied custom extension, and every in-box row carries `None` there); registration sweeps the UNION of the `InterchangePolicy` roster and the payload's own `ElementScene.Obliges` rows NARROWED through `KhrExtension.Writables`, because a bound KTX2 or transform-bearing map obliges its extension whether or not the caller listed it while a read-only vocabulary row must never register as write capability, and the four `format#FORMAT_AXIS` texture rows realize exactly here — `KHR_texture_transform` through `TextureBuilder.WithTransform`, `KHR_texture_basisu`/`EXT_texture_webp`/`MSFT_texture_dds` through the container the sealed bytes already carry (`TextureBuilder.PrimaryImage` reads PNG, JPG, DDS, WEBP, and KTX2), so no texture row is a capability flag with no realizing arm. Each of those three container rows binds its `ChannelImage.Fallback` bytes through `TextureBuilder.WithFallbackImage` — the PNG-or-JPG-only degradation SharpGLTF guards — so a viewer that never negotiated the extension resolves a core texture rather than an unresolvable reference, and a container-obliging map bound with no fallback is an extension the consumer must have; a fallback beside a core PNG/JPG primary is the deleted second copy of the same bytes. Every binding writes the sampler's min/mag pair rather than leaving SharpGLTF's unset `DEFAULT`, because an unset minification filter hands mip selection to the consumer and a KTX2 pyramid the press paid to build may never be sampled — trilinear admission defaults state the law, and a non-interpolating data plane states `NEAREST` at its own admission. `KHR_draco_mesh_compression` and `KHR_meshopt_compression` carry a `KhrEncoder` discriminant rather than a SharpGLTF schema type because SharpGLTF ships no compression encoder — `Openize.Drako` owns the Draco encode and `Alimer.Bindings.MeshOptimizer` the meshopt encode, both quantizing to the `InterchangePolicy` bit budget, a glTF `ModelRoot` passed to either the rejected form because neither package owns a glTF model type. `.bim` and USD arms cross a temp path because their `Save`/`Export` are path-bound (no stream overload) — the temp file is deleted in the same expression and never escapes the capsule. IFC egress is NOT this path's — `ExportIfc` delegates to `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, and a hand-rolled `IfcBuildingElementProxy` re-author (the deleted `IfcBytes` form) is a SECOND IFC-egress owner the contract forbids; the `GlobalId` round-trips 1:1 from `Object.ExternalId` inside `Emit` (never a fresh GUID per export), making export idempotent under the Compute content-key. `ExportIfc` retains only the export-capability gate and the `Serialization` column read (a `None` column IS the non-IFC-row fault, the deleted `SerializationOf` ladder now row data), the column's `IfcWireForm` value carrying the container so a `.ifczip` artifact differs from a `.ifc` one by its row alone — a result-side `ZipArchive` over emitted text, or a `Encoding.UTF8.GetBytes` hop over a returned string, is the deleted form. Chunked-field and structural-delta codecs stay at `Rasm.Compute/Runtime/codecs` consumed at the boundary.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using MeshOptimizer;
using NodaTime;
using pxr;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Schema2.Tiles3D;
using Thinktecture;
using Xbim.CobieExpress;
using Xbim.IO.CobieExpress;
using Xbim.IO.Table;
using Rasm.Bim.Model;
using Rasm.Bim.Planning;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;
using AssimpContext = Assimp.AssimpContext;
using Node = Rasm.Element.Graph.Node;

namespace Rasm.Bim;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExportTrait : ICapability<ExportTrait> {
    public static readonly ExportTrait MergeBuffers = new("merge-buffers");
    public static readonly ExportTrait StridedBuffers = new("strided-buffers");
    public static readonly ExportTrait LockBorder = new("lock-border");
    public static readonly ExportTrait GrowInPlace = new("grow-in-place");
    public static readonly ExportTrait DoubleSided = new("double-sided");
}

public static class ExportCorner {
    public static readonly CapabilityLaw<ExportTrait> Policy = CapabilityLaw<ExportTrait>.Forbidden(Seq(
        CapabilitySet<ExportTrait>.Of(ExportTrait.GrowInPlace), CapabilitySet<ExportTrait>.Of(ExportTrait.DoubleSided)));

    public static readonly CapabilityLaw<ExportTrait> Finish = new(Seq(
        CapabilitySet<ExportTrait>.None, CapabilitySet<ExportTrait>.Of(ExportTrait.DoubleSided)));

    public static readonly CapabilityLaw<ExportTrait> Animation = new(Seq(
        CapabilitySet<ExportTrait>.None, CapabilitySet<ExportTrait>.Of(ExportTrait.GrowInPlace)));
}

public sealed record InterchangePolicy(
    Tolerance Chord,
    Tolerance Distance,
    Tolerance Angle,
    int TriangleBudget,
    ReleaseVersion IfcSchema,
    StepProtocol StepProtocol,
    CapabilitySet<ExportTrait> Traits,
    ValidationMode Validation,
    KhrEncoder Compression,
    int QuantizationBits,
    Seq<(string Channel, float Weight)> AttributeWeights,
    int GpuInstancingMinCount,
    Seq<double> LodRatios,
    Seq<KhrExtension> Extensions) {
    public static InterchangePolicy Canonical => CanonicalRows.Value;
    public static InterchangePolicy Web => WebRows.Value;
    public static InterchangePolicy Pbr => PbrRows.Value;

    static Tolerance Band(ToleranceLane lane, double value) =>
        Tolerance.Of(lane, value, Op.Of(nameof(InterchangePolicy))).ThrowIfFail();

    static readonly Lazy<InterchangePolicy> CanonicalRows = new(static () => new(
        Chord: Band(ToleranceLane.Chord, 0.01), Distance: Band(ToleranceLane.Distance, 1e-6), Angle: Band(ToleranceLane.Angle, 1e-4),
        TriangleBudget: 8_388_608,
        IfcSchema: ReleaseVersion.Ifc4X3Add2, StepProtocol: StepProtocol.Ap242,
        Traits: CapabilitySet<ExportTrait>.Of(ExportTrait.MergeBuffers, ExportTrait.StridedBuffers, ExportTrait.LockBorder),
        Validation: ValidationMode.Strict,
        Compression: KhrEncoder.None, QuantizationBits: 14,
        AttributeWeights: Seq(("geometry_uv", 1.0f), ("geometry_normal", 0.1f)),
        GpuInstancingMinCount: 0,
        LodRatios: Seq(0.5, 0.25, 0.1, 0.05), Extensions: Seq<KhrExtension>()),
        LazyThreadSafetyMode.ExecutionAndPublication);

    static readonly Lazy<InterchangePolicy> WebRows = new(static () => Canonical with {
        Compression = KhrEncoder.Meshopt, QuantizationBits = 12, GpuInstancingMinCount = 16,
        Extensions = Seq(KhrExtension.MaterialsSpecular, KhrExtension.TextureBasisu, KhrExtension.TextureTransform),
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    static readonly Lazy<InterchangePolicy> PbrRows = new(static () => Canonical with {
        Extensions = Seq(KhrExtension.MaterialsClearcoat, KhrExtension.MaterialsTransmission, KhrExtension.MaterialsSheen,
            KhrExtension.MaterialsIridescence, KhrExtension.MaterialsAnisotropy, KhrExtension.MaterialsSpecular,
            KhrExtension.MaterialsDiffuseTransmission),
    }, LazyThreadSafetyMode.ExecutionAndPublication);
}

public sealed record ExportArtifact(
    InterchangeFormat Format,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GltfChannel {
    public static readonly GltfChannel BaseColor    = new("base_color",           Seq(KnownChannel.BaseColor));
    public static readonly GltfChannel Metalness    = new("base_metalness",       Seq(KnownChannel.MetallicRoughness));
    public static readonly GltfChannel Roughness    = new("specular_roughness",   Seq(KnownChannel.MetallicRoughness));
    public static readonly GltfChannel Occlusion    = new("occlusion",            Seq(KnownChannel.Occlusion));
    public static readonly GltfChannel Orm          = new("orm",                  Seq(KnownChannel.Occlusion, KnownChannel.MetallicRoughness));
    public static readonly GltfChannel Normal       = new("geometry_normal",      Seq(KnownChannel.Normal));
    public static readonly GltfChannel Emission     = new("emission_color",       Seq(KnownChannel.Emissive), null,
        Seq((KnownChannel.Emissive, KnownProperty.RGB, (object)Vector3.One)));
    public static readonly GltfChannel SpecularTint = new("specular_color",       Seq(KnownChannel.SpecularColor),            KhrExtension.MaterialsSpecular);
    public static readonly GltfChannel SpecularWeight = new("specular_weight",    Seq(KnownChannel.SpecularFactor),           KhrExtension.MaterialsSpecular);
    public static readonly GltfChannel CoatWeight   = new("coat_weight",          Seq(KnownChannel.ClearCoat),                KhrExtension.MaterialsClearcoat,
        Seq((KnownChannel.ClearCoat, KnownProperty.ClearCoatFactor, (object)1f)));
    public static readonly GltfChannel CoatRough    = new("coat_roughness",       Seq(KnownChannel.ClearCoatRoughness),       KhrExtension.MaterialsClearcoat,
        Seq((KnownChannel.ClearCoatRoughness, KnownProperty.RoughnessFactor, (object)1f)));
    public static readonly GltfChannel CoatNormal   = new("geometry_coat_normal", Seq(KnownChannel.ClearCoatNormal),          KhrExtension.MaterialsClearcoat);
    public static readonly GltfChannel Transmission = new("transmission_weight",  Seq(KnownChannel.Transmission),             KhrExtension.MaterialsTransmission,
        Seq((KnownChannel.Transmission, KnownProperty.TransmissionFactor, (object)1f)));
    public static readonly GltfChannel Subsurface   = new("subsurface_weight",    Seq(KnownChannel.DiffuseTransmissionFactor), KhrExtension.MaterialsDiffuseTransmission,
        Seq((KnownChannel.DiffuseTransmissionFactor, KnownProperty.DiffuseTransmissionFactor, (object)1f)));
    public static readonly GltfChannel FuzzColor    = new("fuzz_color",           Seq(KnownChannel.SheenColor),               KhrExtension.MaterialsSheen,
        Seq((KnownChannel.SheenColor, KnownProperty.RGB, (object)Vector3.One)));
    public static readonly GltfChannel FuzzRough    = new("fuzz_roughness",       Seq(KnownChannel.SheenRoughness),           KhrExtension.MaterialsSheen,
        Seq((KnownChannel.SheenRoughness, KnownProperty.RoughnessFactor, (object)1f)));
    public static readonly GltfChannel FilmWeight   = new("thin_film_weight",     Seq(KnownChannel.Iridescence),              KhrExtension.MaterialsIridescence,
        Seq((KnownChannel.Iridescence, KnownProperty.IridescenceFactor, (object)1f)));
    public static readonly GltfChannel FilmThick    = new("thin_film_thickness",  Seq(KnownChannel.IridescenceThickness),     KhrExtension.MaterialsIridescence);
    public static readonly GltfChannel Anisotropy   = new("specular_roughness_anisotropy", Seq(KnownChannel.Anisotropy),      KhrExtension.MaterialsAnisotropy,
        Seq((KnownChannel.Anisotropy, KnownProperty.AnisotropyStrength, (object)1f)));

    public Seq<KnownChannel> Targets { get; }
    public Option<KhrExtension> Extension { get; }
    public Seq<(KnownChannel Target, KnownProperty Property, object Value)> Units { get; }

    private GltfChannel(
        string key, Seq<KnownChannel> targets, KhrExtension? extension = null,
        Seq<(KnownChannel Target, KnownProperty Property, object Value)> units = default) : this(key) =>
        (Targets, Extension, Units) = (targets, Optional(extension), units);

    public static Option<GltfChannel> From(string channel) =>
        TryGet(channel, out GltfChannel? row) && row is not null ? Some(row) : Option<GltfChannel>.None;
}

public sealed record ChannelImage {
    private ChannelImage(
        GltfChannel channel, ReadOnlyMemory<byte> bytes, Option<ReadOnlyMemory<byte>> fallback, string name, int coordinateSet,
        TextureWrapMode wrapS, TextureWrapMode wrapT, TextureMipMapFilter minFilter, TextureInterpolationFilter magFilter,
        Option<UvTransform> transform, Option<KhrExtension> container, Option<double> thicknessSpanNm) =>
        (Channel, Bytes, Fallback, Name, CoordinateSet, WrapS, WrapT, MinFilter, MagFilter, Transform, Container, ThicknessSpanNm) =
        (channel, bytes, fallback, name, coordinateSet, wrapS, wrapT, minFilter, magFilter, transform, container, thicknessSpanNm);

    public GltfChannel Channel { get; }
    public ReadOnlyMemory<byte> Bytes { get; }
    public Option<ReadOnlyMemory<byte>> Fallback { get; }
    public string Name { get; }
    public int CoordinateSet { get; }
    public TextureWrapMode WrapS { get; }
    public TextureWrapMode WrapT { get; }
    public TextureMipMapFilter MinFilter { get; }
    public TextureInterpolationFilter MagFilter { get; }
    public Option<UvTransform> Transform { get; }
    public Option<KhrExtension> Container { get; }
    public Option<double> ThicknessSpanNm { get; }

    public static Option<ChannelImage> Of(
        string channel, ReadOnlyMemory<byte> bytes, string name, int coordinateSet = 0,
        TextureWrapMode wrapS = TextureWrapMode.REPEAT, TextureWrapMode wrapT = TextureWrapMode.REPEAT,
        TextureMipMapFilter minFilter = TextureMipMapFilter.LINEAR_MIPMAP_LINEAR,
        TextureInterpolationFilter magFilter = TextureInterpolationFilter.LINEAR,
        Option<ReadOnlyMemory<byte>> fallback = default,
        Option<UvTransform> transform = default, Option<double> thicknessSpanNm = default) =>
        GltfChannel.From(channel).Map(row => new ChannelImage(
            row, bytes, fallback, name, coordinateSet, wrapS, wrapT, minFilter, magFilter, transform, Sniffed(bytes.Span), thicknessSpanNm));

    static Option<KhrExtension> Sniffed(ReadOnlySpan<byte> payload) =>
        payload.Length >= 12 && payload[..12].SequenceEqual((ReadOnlySpan<byte>)[0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A])
            ? Some(KhrExtension.TextureBasisu)
            : payload.Length >= 12 && payload[..4].SequenceEqual("RIFF"u8) && payload[8..12].SequenceEqual("WEBP"u8)
                ? Some(KhrExtension.TextureWebp)
                : payload.Length >= 4 && payload[..4].SequenceEqual("DDS "u8)
                    ? Some(KhrExtension.TextureDds)
                    : Option<KhrExtension>.None;

    public MaterialBuilder Bind(MaterialBuilder material) {
        ImageBuilder image = ImageBuilder.From(Wrapped(Bytes), Name);
        Option<ImageBuilder> core = Container.IsSome
            ? Fallback.Map(bytes => ImageBuilder.From(Wrapped(bytes), $"{Name}-fallback"))
            : Option<ImageBuilder>.None;
        Channel.Targets.Iter(target => {
            TextureBuilder texture = material
                .UseChannel(target)
                .UseTexture()
                .WithPrimaryImage(image)
                .WithCoordinateSet(CoordinateSet)
                .WithSampler(WrapS, WrapT, MinFilter, MagFilter);
            core.IfSome(fallback => texture.WithFallbackImage(fallback));
            Transform.IfSome(uv => texture.WithTransform(uv.Offset, uv.Scale, (float)uv.Rotation));
        });
        Channel.Units.Iter(unit => material.WithChannelParam(unit.Target, unit.Property, unit.Value));
        ThicknessSpanNm.IfSome(span => material
            .WithChannelParam(KnownChannel.IridescenceThickness, KnownProperty.Minimum, 0f)
            .WithChannelParam(KnownChannel.IridescenceThickness, KnownProperty.Maximum, (float)span));
        return material;
    }

    public Seq<KhrExtension> Obliges =>
        Channel.Extension.ToSeq()
        + Container.ToSeq()
        + (Transform.IsSome ? Seq(KhrExtension.TextureTransform) : Seq<KhrExtension>());

    static ArraySegment<byte> Wrapped(ReadOnlyMemory<byte> bytes) =>
        MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> owned) ? owned : new ArraySegment<byte>(bytes.ToArray());
}

public sealed record MaterialFinish(Option<AppearanceSummary> Surface, Seq<ChannelImage> Images, CapabilitySet<ExportTrait> Traits) {
    public static readonly MaterialFinish White = new(Option<AppearanceSummary>.None, Seq<ChannelImage>(), CapabilitySet<ExportTrait>.None);

    public static Fin<MaterialFinish> Of(AppearanceSummary surface, CapabilitySet<ExportTrait> traits) =>
        ExportCorner.Finish.Admit(traits).Map(held => new MaterialFinish(Some(surface), Seq<ChannelImage>(), held));

    public Fin<uint> Rgba() =>
        Surface.Match(
            Some: s => AppearanceProjection.Bytes(s.BaseColorR, s.BaseColorG, s.BaseColorB, s.Opacity)
                .Map(static b => (uint)b.Red << 24 | (uint)b.Green << 16 | (uint)b.Blue << 8 | b.Alpha),
            None: static () => Fin.Succ(0xFFFFFFFFu));

    public UInt128 Key =>
        ContentAddress.Of(this, 0.0, static (finish, writer) => finish.Images
            .Fold(
                writer
                    .U128(finish.Surface.Match(Some: static s => s.AppearanceKey, None: static () => UInt128.Zero))
                    .String(finish.Traits.Wire)
                    .Ordinal(finish.Images.Count),
                static (w, image) => Sampled(w, image))).Value;

    static CanonicalWriter Sampled(CanonicalWriter writer, ChannelImage image) =>
        (image.Transform.Map(static uv => Seq<double>(uv.Offset.X, uv.Offset.Y, uv.Scale.X, uv.Scale.Y, uv.Rotation)).IfNone(Seq<double>())
         + image.ThicknessSpanNm.ToSeq())
            .Fold(
                writer.String(image.Channel.Key).Ordinal(image.Bytes.Length).U128(ContentHash.Of(image.Bytes.Span))
                    .Ordinal(image.Fallback.Map(static bytes => bytes.Length).IfNone(-1))
                    .U128(image.Fallback.Map(static bytes => ContentHash.Of(bytes.Span)).IfNone(UInt128.Zero))
                    .Ordinal(image.CoordinateSet).Ordinal((int)image.WrapS).Ordinal((int)image.WrapT)
                    .Ordinal((int)image.MinFilter).Ordinal((int)image.MagFilter)
                    .Ordinal(image.Transform.IsSome ? 5 : 0).Ordinal(image.ThicknessSpanNm.IsSome ? 1 : 0),
                static (framed, value) => framed.Double(value));

    public Seq<KhrExtension> Obliges =>
        (Images.Bind(static image => image.Obliges)
         + (Surface.Map(static s => s.Transmissive).IfNone(false) ? Seq(KhrExtension.MaterialsTransmission) : Seq<KhrExtension>()))
        .Distinct();

    public MaterialBuilder Author() => Images.Fold(Shaded(), static (material, image) => image.Bind(material));

    MaterialBuilder Shaded() {
        MaterialBuilder material = new MaterialBuilder($"finish-{Key:x32}")
            .WithMetallicRoughnessShader()
            .WithDoubleSide(Traits.Admits(ExportTrait.DoubleSided));
        return Surface.Match(
            Some: s => Refracted(
                material
                    .WithBaseColor(new Vector4((float)s.BaseColorR, (float)s.BaseColorG, (float)s.BaseColorB, (float)s.Opacity))
                    .WithMetallicRoughness((float)s.Metallic, (float)s.Roughness)
                    .WithAlpha(s.Opacity < 1.0 ? AlphaMode.BLEND : AlphaMode.OPAQUE),
                s),
            None: () => material.WithBaseColor(Vector4.One).WithMetallicRoughness(0f, 1f));
    }

    static MaterialBuilder Refracted(MaterialBuilder material, AppearanceSummary surface) =>
        surface.Transmissive
            ? material.WithChannelParam(KnownChannel.Transmission, KnownProperty.TransmissionFactor, 1f)
            : material;

}

public readonly record struct MeshLanes(float[] Positions, float[] Normals) {
    public static Fin<MeshLanes> Of(ImportedGeometry geometry) =>
        (Admit(geometry, EncodingChannel.Position), Admit(geometry, EncodingChannel.Normal))
            .Apply(static (positions, normals) => new MeshLanes(positions, normals)).As().ToFin();

    static Validation<Error, float[]> Admit(ImportedGeometry geometry, EncodingChannel channel) =>
        BimExport.Lane(geometry, channel)
            .ToValidation<Error>(new BimFault.Refused(BimScope.Export, BimReason.Unmapped, string.Join(':', new object?[] { "carrier-lane-absent", channel.Key, geometry.FormatKey })));
}

public sealed record ElementInstance(string GlobalId, string Name, string Class, UInt128 MeshKey, Matrix4x4 Placement, MaterialFinish Finish);

public sealed record ElementScene {
    private ElementScene(Map<UInt128, ImportedGeometry> pool, Map<UInt128, MeshLanes> lanes, Seq<ElementInstance> instances) =>
        (Pool, Lanes, Instances) = (pool, lanes, instances);

    public Map<UInt128, ImportedGeometry> Pool { get; }

    public Map<UInt128, MeshLanes> Lanes { get; }

    public Seq<ElementInstance> Instances { get; }

    public static Fin<ElementScene> Of(Map<UInt128, ImportedGeometry> pool, Seq<ElementInstance> instances) =>
        pool.IsEmpty
            ? Fin.Fail<ElementScene>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "element-scene-empty" })))
            : instances.Find(instance => !pool.ContainsKey(instance.MeshKey)).Match(
                Some: miss => Fin.Fail<ElementScene>(new BimFault.Refused(BimScope.Export, BimReason.DanglingReference, string.Join(':', new object?[] { "element-scene-mesh-miss", miss.GlobalId, miss.MeshKey.ToString("x32", CultureInfo.InvariantCulture) }))),
                None: () => toSeq(pool.AsIterable())
                    .Traverse(pair => MeshLanes.Of(pair.Value).Map(lanes => (pair.Key, lanes))).As()
                    .Map(rows => new ElementScene(pool, rows.ToMap(), instances)));

    public static Fin<ElementScene> Of(ImportedGeometry soup) =>
        MeshLanes.Of(soup).Map(lanes => Sole(soup, lanes));

    static ElementScene Sole(ImportedGeometry soup, MeshLanes lanes) {
        UInt128 pooled = ContentHash.Of(soup.Lanes.Payload.Span);
        return new ElementScene(
            Map((pooled, soup)), Map((pooled, lanes)),
            Seq(new ElementInstance("soup", "soup", "IfcBuildingElementProxy", pooled, Matrix4x4.Identity, MaterialFinish.White)));
    }

    public Seq<KhrExtension> Obliges => Instances.Bind(static instance => instance.Finish.Obliges).Distinct();

    public Fin<ImportedGeometry> Soup() => Pooled().Bind(pooled => pooled.Bake());

    public Fin<ImportedGeometry> Pooled() {
        var lead = Pool.Values.ToSeq()[0];
        var keys = Pool.Keys.ToSeq();
        var ordinals = keys.Select(static (k, i) => (k, i)).ToMap();
        int vertexTotal = Pool.Values.Sum(static m => m.VertexCount);
        int indexTotal = Pool.Values.Sum(static m => m.Indices.Length);
        Seq<(EncodingChannel Channel, float[] Raw)> lanes = toSeq(Pool.Values)
            .Bind(static m => m.Lanes.Descriptors.Map(static d => d.Channel)).Distinct()
            .Map(channel => (channel, new float[vertexTotal * channel.Arity])).Strict();
        long[] indices = new long[indexTotal];
        var blocks = new MeshBlock[keys.Count];
        var (vBase, iBase, slot) = (0, 0, 0);
        foreach (var pooled in keys) {
            var mesh = Pool[pooled];
            var entry = mesh.Blocks[0];
            foreach (var lane in lanes) {
                if (entry.Declared.Contains(lane.Channel) && BimExport.Lane(mesh, lane.Channel).Case is float[] source) {
                    source.CopyTo(lane.Raw.AsSpan(vBase * lane.Channel.Arity));
                }
            }
            var corners = mesh.Indices.Span;
            for (int c = 0; c < corners.Length; c++) { indices[iBase + c] = corners[c] + vBase; }
            blocks[slot] = new MeshBlock(vBase, mesh.VertexCount, iBase, corners.Length, entry.Declared, entry.Material);
            (vBase, iBase, slot) = (vBase + mesh.VertexCount, iBase + corners.Length, slot + 1);
        }
        var placed = Instances.Map(i => new MeshInstance(ordinals[i.MeshKey], i.Placement));
        return Encode.Of(vertexTotal, lanes).Map(arena => new ImportedGeometry(
            lead.FormatKey, arena, indices, vertexTotal, indexTotal / 3, toSeq(blocks), placed, lead.At));
    }
}

[Union]
public abstract partial record ExportPayload {
    public sealed record Soup(ImportedGeometry Geometry) : ExportPayload;
    public sealed record Scene(ElementScene Elements) : ExportPayload;

    public Fin<ImportedGeometry> Flat() => Switch(
        state: key,
        soup:  static (_, s) => Fin.Succ(s.Geometry),
        scene: static (k, s) => s.Elements.Soup(k));
}

public sealed record GlbScene(
    ModelRoot Model, Map<string, SharpGLTF.Schema2.Node> Nodes, Map<string, int> Rows, Seq<KhrExtension> Extensions);

public static partial class BimExport {
    public static Fin<ExportArtifact> Export(InterchangeFormat format, ExportPayload payload, InterchangePolicy policy, IClock clock) =>
        InterchangeFormat.Admitted(format, InterchangeCapability.Export).Bind(row => row.Codec.Switch(
            sharpGltf:        () => GlbBytes(payload, policy).Map(bytes => Sealed(format, bytes, policy, clock.GetCurrentInstant())),
            dotBim:           () => DotBimBytes(payload).Map(bytes => Sealed(format, bytes, policy, clock.GetCurrentInstant())),
            sceneExchange:    () => Admitted(payload).Bind(pair => SceneBytes(format, pair.Geometry, pair.Lanes)).Map(bytes => Sealed(format, bytes, policy, clock.GetCurrentInstant())),
            usdStage:         () => Admitted(payload).Bind(pair => Encoded(() => UsdBytes(format, pair.Geometry, pair.Lanes), "usd-export")).Map(bytes => Sealed(format, bytes, policy, clock.GetCurrentInstant())),
            geometryGym:      () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "ifc-export-route", "use-ExportIfc", format.Key }))),
            geospatialVector: () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "geo-export-route", "GeoVector.Write", format.Key }))),
            geospatialRaster: () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Export.Key, format.Key }))),
            meshText:         () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Export.Key, format.Key }))),
            ply:              () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Export.Key, format.Key }))),
            pointCloud:       () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Export.Key, format.Key }))),
            acadSharp:        () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Export.Key, format.Key }))),
            stepIso10303:     () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "direction-unsupported", InterchangeCapability.Export.Key, format.Key }))),
            nativeCompanion:  () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Capability, string.Join(':', new object?[] { "export-needs-host", format.Key }))),
            igesAnsi:         () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Capability, string.Join(':', new object?[] { "export-needs-host", format.Key }))),
            saf:              () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "saf-export-graph-route", "use-SafEmit", format.Key }))),
            cobieXlsx:        () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "cobie-export-graph-route", "use-CobieEmit", format.Key }))),
            energyModel:      () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "energy-export-route", "EnergyExchange.Apply", format.Key }))),
            ifc5Pending:      () => Fin.Fail<ExportArtifact>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "export-catalogue-pending", format.Key })))));

    public static Fin<GlbScene> Author(ElementScene scene, InterchangePolicy policy) =>
        Try.lift(() => Staged(scene, policy)).Run().Bind(static inner => inner)
            ;

    public static Fin<ExportArtifact> Emit(GlbScene scene, InterchangeFormat format, InterchangePolicy policy, IClock clock) =>
        Try.lift(() => Sealed(format, WriteGlb(scene.Model, policy), policy, clock.GetCurrentInstant())).Run().Bind(static inner => inner)
            ;

    internal static Option<float[]> Lane(ImportedGeometry geometry, EncodingChannel channel) =>
        geometry.Blocks.ForAll(block => block.Declared.Contains(channel))
            ? Sliced(geometry, channel)
            : Option<float[]>.None;

    static Option<float[]> Sliced(ImportedGeometry geometry, EncodingChannel channel) =>
        geometry.Lanes.Descriptors.Find(descriptor => descriptor.Channel == channel).Map(descriptor => {
            float[] raw = new float[geometry.Lanes.Count * descriptor.Channel.Arity];
            descriptor.Channel.Dtype.Unpack(geometry.Lanes.Channel(channel).Span, raw);
            return raw;
        });

    internal static Fin<(ImportedGeometry Geometry, MeshLanes Lanes)> Admitted(ExportPayload payload) =>
        payload.Flat().Bind(flat => MeshLanes.Of(flat).Map(lanes => (flat, lanes)));

    static GlbScene Staged(ElementScene scene, InterchangePolicy policy) {
        var rows = scene.Instances.Select(static (instance, row) => (instance.GlobalId, row)).ToMap();
        int nullRow = scene.Instances.Count;
        var byMesh = scene.Instances
            .Select(static (instance, row) => (instance.MeshKey, Row: row, instance.Finish))
            .GroupBy(static pair => pair.MeshKey)
            .ToDictionary(
                static g => g.Key,
                g => (Stamp: g.Count() == 1 ? g.First().Row : nullRow,
                      Finish: g.Select(static pair => pair.Finish.Key).Distinct().Count() == 1 ? g.First().Finish : MaterialFinish.White));
        var materials = new Dictionary<UInt128, MaterialBuilder>();
        MaterialBuilder Finished(MaterialFinish finish) =>
            materials.TryGetValue(finish.Key, out MaterialBuilder? held) ? held : materials[finish.Key] = finish.Author();
        var pool = scene.Pool.Map((key, mesh) => {
            var (row, finish) = byMesh.GetValueOrDefault((nullRow, MaterialFinish.White));
            return MeshOf(mesh, scene.Lanes[key], Finished(finish), Some(row));
        });
        var builder = new SceneBuilder();
        scene.Instances.Iter(instance => {
            var node = new NodeBuilder(instance.GlobalId) { LocalMatrix = instance.Placement };
            builder.AddRigidMesh(pool[instance.MeshKey], node);
        });
        var model = builder.ToGltf2(new SceneBuilderSchema2Settings {
            UseStridedBuffers = policy.Traits.Admits(ExportTrait.StridedBuffers),
            GpuMeshInstancingMinCount = policy.GpuInstancingMinCount <= 0 ? int.MaxValue : policy.GpuInstancingMinCount,
        });
        return new GlbScene(
            model,
            model.LogicalNodes.AsIterable().Filter(static n => n.Name is { Length: > 0 }).Map(static n => (n.Name, n)).ToMap(),
            rows,
            Registered(policy, new ExportPayload.Scene(scene)));
    }

    static Fin<byte[]> SceneBytes(InterchangeFormat format, ImportedGeometry geometry, MeshLanes lanes) {
        using var context = new AssimpContext();
        return context.IsExportFormatSupported(format.Key)
            ? Encoded(() => Blob(context, format, geometry, lanes), "scene-export")
            : Fin.Fail<byte[]>(new BimFault.Refused(BimScope.Export, BimReason.Capability, string.Join(':', new object?[] { "scene-export-format", format.Key })));
    }

    static byte[] Blob(AssimpContext context, InterchangeFormat format, ImportedGeometry geometry, MeshLanes lanes) {
        var mesh = new Assimp.Mesh("bim") { MaterialIndex = 0 };
        for (int v = 0; v < geometry.VertexCount; v++) {
            int p = v * EncodingChannel.Position.Arity, n = v * EncodingChannel.Normal.Arity;
            mesh.Vertices.Add(new Vector3(lanes.Positions[p], lanes.Positions[p + 1], lanes.Positions[p + 2]));
            mesh.Normals.Add(new Vector3(lanes.Normals[n], lanes.Normals[n + 1], lanes.Normals[n + 2]));
        }
        var indices = geometry.Indices.Span;
        for (int t = 0; t < geometry.TriangleCount; t++) {
            mesh.Faces.Add(new Assimp.Face([(int)indices[t * 3], (int)indices[t * 3 + 1], (int)indices[t * 3 + 2]]));
        }
        var scene = new Assimp.Scene { RootNode = new Assimp.Node("root") };
        scene.Materials.Add(new Assimp.Material { Name = "default" });
        scene.Meshes.Add(mesh);
        scene.RootNode.MeshIndices.Add(0);
        return context.ExportToBlob(scene, format.Key).Data;
    }

    static Fin<byte[]> DotBimBytes(ExportPayload payload) =>
        payload.Switch(
                state: key,
                soup: static (k, s) => ElementScene.Of(s.Geometry, k),
                scene: static (_, s) => Fin.Succ(s.Elements))
            .Bind(scene => Wired(scene));

    static Fin<byte[]> Wired(ElementScene scene) {
        Map<UInt128, int> ordinals = scene.Pool.Keys.Select(static (k, index) => (k, index)).ToMap();
        List<dotbim.Mesh> meshes = scene.Pool.AsIterable().Map(pair => new dotbim.Mesh {
            MeshId = ordinals[pair.Key],
            Coordinates = [.. scene.Lanes[pair.Key].Positions.Select(static v => (double)v)],
            Indices = [.. pair.Value.Indices.ToArray().Select(static i => (int)i)],
        }).ToList();
        return scene.Instances.Traverse(instance => Placed(instance, ordinals)).As()
            .Bind(elements => Encoded(() => Written(meshes, elements.ToList()), "bim-export"));
    }

    static Fin<dotbim.Element> Placed(ElementInstance instance, Map<UInt128, int> ordinals) =>
        Matrix4x4.Decompose(instance.Placement, out var scale, out var rotation, out var translation)
        && Math.Abs(scale.X - 1f) <= RigidBand && Math.Abs(scale.Y - 1f) <= RigidBand && Math.Abs(scale.Z - 1f) <= RigidBand
            ? instance.Finish.Rgba().Map(rgba => new dotbim.Element {
                MeshId = ordinals[instance.MeshKey],
                Vector = new dotbim.Vector { X = translation.X, Y = translation.Y, Z = translation.Z },
                Rotation = new dotbim.Rotation { Qx = rotation.X, Qy = rotation.Y, Qz = rotation.Z, Qw = rotation.W },
                Guid = Rfc4122(instance.GlobalId),
                Type = instance.Class,
                Color = new dotbim.Color {
                    R = (int)(rgba >> 24 & 0xFF), G = (int)(rgba >> 16 & 0xFF),
                    B = (int)(rgba >> 8 & 0xFF), A = (int)(rgba & 0xFF),
                },
                Info = new Dictionary<string, string> { ["globalId"] = instance.GlobalId, ["name"] = instance.Name },
            })
            : Fin.Fail<dotbim.Element>(new BimFault.Refused(BimScope.Export, BimReason.Rejected, string.Join(':', new object?[] { "dotbim-nonrigid-placement", instance.GlobalId })));

    const float RigidBand = 1e-4f;

    static byte[] Written(List<dotbim.Mesh> meshes, List<dotbim.Element> elements) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bim");
        try {
            new dotbim.File { SchemaVersion = "1.1.0", Meshes = meshes, Elements = elements, Info = new Dictionary<string, string>() }.Save(path);
            return File.ReadAllBytes(path);
        } finally { File.Delete(path); }
    }

    static string Rfc4122(string globalId) {
        UInt128 key = ContentHash.Of(Encoding.UTF8.GetBytes(globalId));
        return new Guid(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref key, 1))).ToString();
    }

    static byte[] UsdBytes(InterchangeFormat format, ImportedGeometry geometry, MeshLanes lanes) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        try {
            using var stage = UsdStage.CreateNew(path);
            var mesh = UsdGeomMesh.Define(stage, new SdfPath("/Bim"));
            float[] verts = lanes.Positions;
            var points = new VtVec3fArray((uint)geometry.VertexCount);
            for (int v = 0; v < geometry.VertexCount; v++) {
                int p = v * EncodingChannel.Position.Arity;
                points[v] = new GfVec3f(verts[p], verts[p + 1], verts[p + 2]);
            }
            mesh.GetPointsAttr().Set(new VtValue(points), UsdTimeCode.Default());
            var counts = new VtIntArray((uint)geometry.TriangleCount);
            var corners = new VtIntArray((uint)(geometry.TriangleCount * 3));
            var indices = geometry.Indices.Span;
            for (int t = 0; t < geometry.TriangleCount; t++) {
                counts[t] = 3;
                corners[t * 3] = (int)indices[t * 3]; corners[t * 3 + 1] = (int)indices[t * 3 + 1]; corners[t * 3 + 2] = (int)indices[t * 3 + 2];
            }
            mesh.GetFaceVertexCountsAttr().Set(new VtValue(counts), UsdTimeCode.Default());
            mesh.GetFaceVertexIndicesAttr().Set(new VtValue(corners), UsdTimeCode.Default());
            stage.Save();
            return File.ReadAllBytes(path);
        } finally { File.Delete(path); }
    }

    public static Fin<ExportArtifact> ExportIfc(
        InterchangeFormat format, ElementGraph graph, SemanticProjector projector,
        InterchangePolicy policy, IClock clock, Option<EmitContext> context) =>
        InterchangeFormat.Admitted(format, InterchangeCapability.Export).Bind(row => row.Serialization
            .ToFin(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "ifc-export-codec-miss", format.Key })))
            .Bind(form => projector.Emit(graph, form, context)
                .Map(bytes => Sealed(row, bytes, policy, clock.GetCurrentInstant()))));

    static Fin<byte[]> GlbBytes(ExportPayload payload, InterchangePolicy policy) =>
        policy.Compression switch {
            KhrEncoder.Draco => Admitted(payload).Bind(pair => Encoded(() => DracoBytes(pair.Geometry, pair.Lanes, policy), "gltf-export")),
            KhrEncoder.Meshopt => Admitted(payload).Bind(pair => Encoded(() => MeshoptBytes(pair.Geometry, pair.Lanes, policy), "gltf-export")),
            KhrEncoder.None => Container(payload, policy).Bind(model => Encoded(() => WriteGlb(model, policy), "gltf-export")),
            var unknown => Fin.Fail<byte[]>(new BimFault.Refused(BimScope.Export, BimReason.Codec, string.Join(':', new object?[] { "khr-encoder-unrouted", unknown.ToString() }))),
        };

    static Fin<byte[]> Encoded(Func<byte[]> encode, string detail) =>
        Try.lift(encode).Run().Bind(static inner => inner);

    static Fin<ModelRoot> Container(ExportPayload payload, InterchangePolicy policy) =>
        payload.Switch(
            state: (policy),
            soup:  static (run, flat) => MeshLanes.Of(flat.Geometry, run.key).Map(lanes => SceneOf(flat.Geometry, lanes, run.policy)),
            scene: static (run, per) => Fin.Succ(Staged(per.Elements, run.policy).Model));

    static ModelRoot SceneOf(ImportedGeometry geometry, MeshLanes lanes, InterchangePolicy policy) {
        var scene = new SceneBuilder();
        scene.AddRigidMesh(MeshOf(geometry, lanes, MaterialFinish.White.Author(), Option<int>.None), AffineTransform.Identity);
        return scene.ToGltf2(new SceneBuilderSchema2Settings { UseStridedBuffers = policy.Traits.Admits(ExportTrait.StridedBuffers) });
    }

    const string FeatureIdAttribute = "_FEATURE_ID_0";

    readonly struct FeatureVertex(int row) : IVertexCustom {
        public int MaxColors => 0;
        public int MaxTextCoords => 0;
        public Vector4 GetColor(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        public Vector2 GetTexCoord(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        public void SetColor(int index, Vector4 color) { }
        public void SetTexCoord(int index, Vector2 coord) { }
        public VertexMaterialDelta Subtract(IVertexMaterial baseValue) => VertexMaterialDelta.Zero;
        public void Add(in VertexMaterialDelta delta) { }
        public IEnumerable<KeyValuePair<string, AttributeFormat>> GetEncodingAttributes() =>
            [new(FeatureIdAttribute, AttributeFormat.Float1)];
        public IEnumerable<string> CustomAttributes => [FeatureIdAttribute];
        public bool TryGetCustomAttribute(string name, out object value) =>
            (value = name == FeatureIdAttribute ? row : null!) is not null;
        public void SetCustomAttribute(string name, object value) { }
        public void Validate() { }
    }

    readonly struct FeatureUvVertex(int row, Vector2 uv) : IVertexCustom {
        public int MaxColors => 0;
        public int MaxTextCoords => 1;
        public Vector4 GetColor(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        public Vector2 GetTexCoord(int index) => index == 0 ? uv : throw new ArgumentOutOfRangeException(nameof(index));
        public void SetColor(int index, Vector4 color) { }
        public void SetTexCoord(int index, Vector2 coord) { }
        public VertexMaterialDelta Subtract(IVertexMaterial baseValue) => VertexMaterialDelta.Zero;
        public void Add(in VertexMaterialDelta delta) { }
        public IEnumerable<KeyValuePair<string, AttributeFormat>> GetEncodingAttributes() =>
            [new("TEXCOORD_0", AttributeFormat.Float2), new(FeatureIdAttribute, AttributeFormat.Float1)];
        public IEnumerable<string> CustomAttributes => [FeatureIdAttribute];
        public bool TryGetCustomAttribute(string name, out object value) =>
            (value = name == FeatureIdAttribute ? row : null!) is not null;
        public void SetCustomAttribute(string name, object value) { }
        public void Validate() { }
    }

    static IMeshBuilder<MaterialBuilder> MeshOf(ImportedGeometry geometry, MeshLanes lanes, MaterialBuilder material, Option<int> feature) =>
        (feature, Lane(geometry, EncodingChannel.Uv)) switch {
            ({ IsSome: true, Case: int row }, { Case: float[] uv }) => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, FeatureUvVertex, VertexEmpty>(geometry.FormatKey), geometry, lanes, material, i => new FeatureUvVertex(row, Uv(uv, i))),
            ({ IsSome: true, Case: int row }, _)                    => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, FeatureVertex, VertexEmpty>(geometry.FormatKey), geometry, lanes, material, _ => new FeatureVertex(row)),
            (_, { Case: float[] uv })                               => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexTexture1, VertexEmpty>(geometry.FormatKey), geometry, lanes, material, i => new VertexTexture1(Uv(uv, i))),
            _                                                       => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>(geometry.FormatKey), geometry, lanes, material, static _ => default),
        };

    static Vector2 Uv(float[] uvs, int index) {
        int u = index * EncodingChannel.Uv.Arity;
        return new(uvs[u], uvs[u + 1]);
    }

    static MeshBuilder<MaterialBuilder, VertexPositionNormal, TvM, VertexEmpty> Filled<TvM>(
        MeshBuilder<MaterialBuilder, VertexPositionNormal, TvM, VertexEmpty> mesh, ImportedGeometry geometry, MeshLanes lanes, MaterialBuilder material, Func<int, TvM> slot)
        where TvM : struct, IVertexMaterial {
        var primitive = mesh.UsePrimitive(material);
        float[] verts = lanes.Positions;
        float[] normals = lanes.Normals;
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            primitive.AddTriangle(
                Vertex(verts, normals, (int)indices[tri * 3], slot),
                Vertex(verts, normals, (int)indices[tri * 3 + 1], slot),
                Vertex(verts, normals, (int)indices[tri * 3 + 2], slot));
        }
        return mesh;
    }

    static VertexBuilder<VertexPositionNormal, TvM, VertexEmpty> Vertex<TvM>(float[] verts, float[] normals, int index, Func<int, TvM> slot)
        where TvM : struct, IVertexMaterial {
        int p = index * EncodingChannel.Position.Arity, n = index * EncodingChannel.Normal.Arity;
        return new VertexBuilder<VertexPositionNormal, TvM, VertexEmpty>(
            new VertexPositionNormal(verts[p], verts[p + 1], verts[p + 2], normals[n], normals[n + 1], normals[n + 2]), slot(index));
    }

    static byte[] WriteGlb(ModelRoot model, InterchangePolicy policy) {
        bool merge = policy.Traits.Admits(ExportTrait.MergeBuffers);
        if (merge) { model.MergeBuffers(); }
        return model.WriteGLB(new WriteSettings { MergeBuffers = merge, Validation = policy.Validation }).ToArray();
    }

    static byte[] DracoBytes(ImportedGeometry geometry, MeshLanes lanes, InterchangePolicy policy) {
        var mesh = new DracoMesh { NumPoints = geometry.VertexCount };
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Position, EncodingChannel.Position.Arity, lanes.Positions));
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Normal, EncodingChannel.Normal.Arity, lanes.Normals));
        if (Lane(geometry, EncodingChannel.Uv).Case is float[] uv) { mesh.AddAttribute(PointAttribute.Wrap(AttributeType.TexCoord, EncodingChannel.Uv.Arity, uv)); }
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            mesh.AddFace([(int)indices[tri * 3], (int)indices[tri * 3 + 1], (int)indices[tri * 3 + 2]]);
        }
        mesh.DeduplicateAttributeValues();
        return Draco.Encode(mesh, new DracoEncodeOptions {
            PositionBits = policy.QuantizationBits, NormalBits = policy.QuantizationBits,
            TextureCoordinateBits = policy.QuantizationBits,
            CompressionLevel = DracoCompressionLevel.Optimal,
        });
    }

    [SmartEnum<string>]
    public sealed partial class MeshoptLane {
        public static readonly MeshoptLane Position = new("POSITION",   EncodingChannel.Position, filter: "EXPONENTIAL", bit: 1);
        public static readonly MeshoptLane Normal   = new("NORMAL",     EncodingChannel.Normal,   filter: "OCTAHEDRAL",  bit: 2);
        public static readonly MeshoptLane Uv       = new("TEXCOORD_0", EncodingChannel.Uv,       filter: "EXPONENTIAL", bit: 4);

        public EncodingChannel Channel { get; }
        public string Filter { get; }
        public int Bit { get; }

        public int Components => Channel.Arity;

        private MeshoptLane(string key, EncodingChannel channel, string filter, int bit) : this(key) =>
            (Channel, Filter, Bit) = (channel, filter, bit);
    }

    static unsafe byte[] MeshoptBytes(ImportedGeometry geometry, MeshLanes lanes, InterchangePolicy policy) {
        Seq<(MeshoptLane Lane, float[] Source)> active =
            Seq((Lane: MeshoptLane.Position, Source: lanes.Positions),
                (Lane: MeshoptLane.Normal,   Source: lanes.Normals))
            + Lane(geometry, MeshoptLane.Uv.Channel).Map(static uv => (Lane: MeshoptLane.Uv, Source: uv)).ToSeq();
        Seq<MeshoptLane> lanes = active.Map(static row => row.Lane);
        int floats = lanes.Sum(static lane => lane.Components);
        nuint vertSize = (nuint)(floats * sizeof(float));
        nuint soupCount = (nuint)geometry.VertexCount;
        using var soupOwner = SpanOwner<float>.Allocate(geometry.VertexCount * floats);
        Span<float> soup = soupOwner.Span;
        int head = 0;
        foreach (var row in active) {
            int width = row.Lane.Components;
            for (int v = 0; v < geometry.VertexCount; v++) {
                row.Source.AsSpan(v * width, width).CopyTo(soup.Slice((v * floats) + head, width));
            }
            head += width;
        }
        using var soupIndexOwner = SpanOwner<uint>.Allocate(geometry.TriangleCount * 3);
        Span<uint> soupIndices = soupIndexOwner.Span;
        for (int i = 0; i < soupIndices.Length; i++) { soupIndices[i] = (uint)geometry.Indices.Span[i]; }
        nuint indexCount = (nuint)soupIndices.Length;
        using var remapOwner = SpanOwner<uint>.Allocate(geometry.VertexCount);
        Span<uint> remap = remapOwner.Span;
        nuint uniqueCount;
        fixed (uint* remapPtr = remap)
        fixed (uint* idxPtr = soupIndices)
        fixed (float* vSoup = soup) {
            uniqueCount = Meshopt.GenerateVertexRemap(remapPtr, idxPtr, indexCount, vSoup, soupCount, vertSize);
        }
        using var remappedOwner = SpanOwner<float>.Allocate((int)uniqueCount * floats);
        using var interleavedOwner = SpanOwner<float>.Allocate((int)uniqueCount * floats);
        using var indexOwner = SpanOwner<uint>.Allocate((int)indexCount);
        Span<float> remapped = remappedOwner.Span;
        Span<float> interleaved = interleavedOwner.Span;
        Span<uint> indices = indexOwner.Span;
        fixed (uint* remapPtr = remap)
        fixed (uint* idxSrc = soupIndices)
        fixed (uint* idxDst = indices)
        fixed (float* vSoup = soup)
        fixed (float* vRemap = remapped)
        fixed (float* vDstI = interleaved) {
            Meshopt.RemapVertexBuffer(vRemap, vSoup, soupCount, vertSize, remapPtr);
            Meshopt.RemapIndexBuffer(idxDst, idxSrc, indexCount, remapPtr);
            Meshopt.OptimizeVertexCache(idxDst, idxDst, indexCount, uniqueCount);
            Meshopt.OptimizeOverdraw(idxDst, idxDst, indexCount, vRemap, uniqueCount, vertSize, 1.05f);
            Meshopt.OptimizeVertexFetch(vDstI, idxDst, indexCount, vRemap, uniqueCount, vertSize);
        }
        var streams = new List<(MeshoptLane Lane, byte[] Bytes)>(lanes.Count);
        int lead = 0;
        foreach (var lane in lanes) {
            int width = lane.Components;
            using var laneOwner = SpanOwner<float>.Allocate((int)uniqueCount * width);
            Span<float> plane = laneOwner.Span;
            for (int v = 0; v < (int)uniqueCount; v++) {
                interleaved.Slice((v * floats) + lead, width).CopyTo(plane.Slice(v * width, width));
            }
            using var codedOwner = SpanOwner<byte>.Allocate((int)uniqueCount * width * sizeof(float));
            Span<byte> coded = codedOwner.Span;
            if (lane == MeshoptLane.Normal) { Meshopt.EncodeFilterOct(coded, policy.QuantizationBits, plane); }
            else { Meshopt.EncodeFilterExp(coded, policy.QuantizationBits, plane, EncodeExpMode.EncodeExpSharedVector); }
            nuint laneStride = (nuint)(width * sizeof(float));
            using var encodedOwner = SpanOwner<byte>.Allocate((int)Meshopt.EncodeVertexBufferBound(uniqueCount, laneStride));
            Span<byte> encoded = encodedOwner.Span;
            nuint written = Meshopt.EncodeVertexBufferLevel(encoded, coded[..((int)uniqueCount * (int)laneStride)], policy.QuantizationBits, version: -1);
            streams.Add((lane, encoded[..(int)written].ToArray()));
            lead += width;
        }
        using var iOwner = SpanOwner<byte>.Allocate((int)Meshopt.EncodeIndexBufferBound(indexCount, uniqueCount));
        Span<byte> iBuffer = iOwner.Span;
        nuint iLen;
        fixed (byte* iDst = iBuffer)
        fixed (uint* iSrc = indices) {
            iLen = Meshopt.EncodeIndexBuffer(iDst, (nuint)iBuffer.Length, iSrc, indexCount);
        }
        int mask = lanes.Fold(0, static (bits, lane) => bits | lane.Bit);
        return [
            .. BitConverter.GetBytes((int)uniqueCount), .. BitConverter.GetBytes((int)indexCount),
            .. BitConverter.GetBytes(mask), .. BitConverter.GetBytes(policy.QuantizationBits),
            .. BitConverter.GetBytes((int)iLen),
            .. streams.SelectMany(static s => (byte[])[.. BitConverter.GetBytes(s.Lane.Components), .. BitConverter.GetBytes(s.Bytes.Length)]),
            .. streams.SelectMany(static s => s.Bytes),
            .. iBuffer[..(int)iLen],
        ];
    }

    static Seq<KhrExtension> Registered(InterchangePolicy policy, ExportPayload payload) =>
        (policy.Extensions + payload.Switch(soup: static _ => Seq<KhrExtension>(), scene: static s => s.Elements.Obliges))
            .Distinct().Filter(static khr => KhrExtension.Writables.Contains(khr));

    internal static ExportArtifact Sealed(InterchangeFormat format, ReadOnlyMemory<byte> bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes,
            ContentAddress.Of((format, policy, bytes), 0.0, static (s, writer) => writer
                .String(s.format.Key).Double(s.policy.Chord.Value).Double(s.policy.Distance.Value).Double(s.policy.Angle.Value)
                .Raw(s.bytes.Span)).Value,
            bytes.Length, at);
}
```

## [03]-[TILE_METADATA]

- Owner: `TileMetadata` the per-tile `EXT_structural_metadata` author over the shared `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, never a stored record) — one embedded schema carrying the element's `Classification` code, `ExternalId` GlobalId, name, and (as growth) the baked property/quantity columns, one `PropertyTable` per-feature value store, and the `EXT_mesh_features` feature-ID binding tying each GLB primitive vertex span to its element row so the Cesium 3D Tiles web peer resolves per-element metadata at pick time.
- Entry: `TileMetadata.Attach(GlbScene scene, Seq<Element> elements)` authors the structural-metadata schema/class/property-table on the `Author`-minted GLB scene — the feature-ID VALUES are already in the model (the per-vertex `_FEATURE_ID_0` stamps `Staged` authored through the `FeatureVertex` fragment), so `Attach` only defines the schema and binds the table, never re-walking or re-stamping geometry; `Fin<T>` aborts on a registration fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected`) lifting BARE onto the `Fin<T>` result (band 2600, `Fault`-derived), no `.ToError()` hop; the per-tile metadata emit composes through the `Rasm.Compute` interchange codec `TILE_PARTITION` at the contract and `Rasm.Bim` authors the canonical schema shape and the extension surface.
- Auto: `Attach` defines the `Element` structural-metadata schema (one property per canonical column — `GlobalId` off `Element.ExternalId`, `Class` an `IfcClass` enumeration off `Element.Classification.Code`, `Name`, and as growth the baked-Pset columns off `Element.Properties`) and adds a per-feature `PropertyTable` whose ROWS ORDER BY the `GlbScene.Rows` ordinals — the one row space the `Staged` vertex stamps already index. Element semantics join by the shared GlobalId; an element-less row carries empty strings and the `Class` column's reserved `Unclassified` noData sentinel (a bare `IfNone(0)` silently claimed the first REAL `IfcClass` row). ONE `FeatureIDBuilder` binds per DISTINCT logical mesh with `nullFeatureId = Rows.Count`, so a shared-mesh repeat's null-row stamps resolve to "no feature" at pick rather than a wrong row.
- Output: the authored `EXT_structural_metadata` schema and `PropertyTable` are the per-tile semantic the web peer reads — the same shared `Element` vocabulary a consumer reads at the `Exchange/wire#WIRE_PROJECTION`, projected onto the binary tile metadata so a Cesium consumer resolves per-element BIM semantics at pick without a second metadata mint.
- Packages: SharpGLTF.Core, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new metadata column is one `UseProperty(name).With<Type>(...)` row on the embedded class fed from a baked `Element` field; a new feature-ID binding is one `FeatureIDBuilder` over the primitive; the `IfcClass` enumeration is one `UseEnumMetadata` row tracking the `IfcClass` vocabulary; never a hand-authored JSON metadata block and never a second per-tile metadata mint.
- Boundary: the per-tile metadata authors through the `SharpGLTF.Ext.3DTiles` `EXT_structural_metadata`/`EXT_mesh_features` surface — a hand-authored JSON `EXT_structural_metadata` block is the deleted form, the `StructuralMetadataClassProperty.With<Type>` selectors and the `PropertyTableProperty.SetValues<T>` binary encode own the schema and value storage; `Tiles3DExtensions.RegisterExtensions()` runs once before any author and the call is idempotent at the factory level; the per-feature semantic is the contract baked `Element` and a retired `BimElement` row crossing `Attach` is the deleted form (the element is the `Bake` fold over the `ElementGraph`, the `Classification` code resolved to the `IfcClass` enumeration, never a typed `IfcClass` on the row); the `IfcClass` column rides `UseEnumMetadata` so the closed BIM class vocabulary serializes by its enumeration rather than a free string; the tile-pyramid partitioning and streaming stay at `Rasm.Compute/Runtime/tiles#TILE_PARTITION` consumed at the boundary — `Rasm.Bim` admits the extension surface and the canonical schema shape, never the tile pyramid; the `OneOf<int, Texture>` feature-ID attribute selector is a transitive `OneOf` dependency consumed only by `FeatureIDBuilder` and no Bim code references it directly; the per-tile `Element` semantic is the same vocabulary the wire projection carries, never a second metadata vocabulary.

```csharp
public static class TileMetadata {
    public static Fin<GlbScene> Attach(GlbScene scene, Seq<Element> elements) =>
        Try.lift(() => Author(scene, elements)).Run().Bind(static inner => inner);

    static GlbScene Author(GlbScene scene, Seq<Element> elements) {
        Tiles3DExtensions.RegisterExtensions();
        var byExternal = elements.Choose(static e => e.ExternalId.Map(ext => (ext, e))).ToMap();
        var slots = toSeq(scene.Rows.AsIterable().OrderBy(static pair => pair.Value).Select(pair => byExternal.Find(pair.Key)));
        var root = scene.Model.UseStructuralMetadata();
        var schema = root.UseEmbeddedSchema("rasm-element");
        var classIndex = IfcClass.Items.Select(static (row, i) => (row.Key, i)).ToMap();
        var classKinds = schema.UseEnumMetadata("IfcClass", [("Unclassified", -1), .. classIndex.AsIterable().Map(static pair => (pair.Key, pair.Value))]);
        var elementClass = schema.UseClassMetadata("Element");
        elementClass.UseProperty("GlobalId").WithStringType(noData: null, defaultValue: null);
        elementClass.UseProperty("Class").WithEnumeration(classKinds, noData: "Unclassified");
        elementClass.UseProperty("Name").WithStringType(noData: null, defaultValue: null);
        var table = root.AddPropertyTable(elementClass, slots.Count, "elements");
        table.UseProperty("GlobalId").SetValues([.. slots.Map(static slot => slot.Bind(static e => e.ExternalId).IfNone(""))]);
        table.UseProperty("Class").SetValues([.. slots.Map(slot => slot.Bind(e => classIndex.Find(e.Classification.Code)).IfNone(-1))]);
        table.UseProperty("Name").SetValues([.. slots.Map(static slot => slot.Map(static e => e.Name).IfNone(""))]);
        var features = new FeatureIDBuilder(slots.Count, attributeOrTexture: 0, propertyTable: table, channels: null, label: "elements", nullFeatureId: slots.Count);
        scene.Nodes.Values.AsIterable().Choose(static n => Optional(n.Mesh)).Distinct()
            .Iter(mesh => mesh.Primitives.AsIterable().Iter(primitive => primitive.AddMeshFeatureIds(features)));
        return scene;
    }
}
```

## [04]-[BIM_LOD]

- Owner: `BimLod` the per-element LOD-pyramid leg ADDITIVE to the export path — one progressive-detail chain per element derived through the catalogued `Meshopt.Simplify`/`SimplifySloppy` decimation keyed by target triangle ratio, with the meshlet residency band through `Meshopt.BuildMeshlets` for the WebGPU raster path; the shared `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `MeshletBand` carrying each cluster with its cull sphere and normal cone (the local twin DELETED onto that owner, the meshopt `Meshlet`/`Bounds` ABI staying behind the `Cluster` arm); `LodLevel` the per-level record carrying the decimated index buffer, the target ratio, and the per-LOD content key the `Rasm.Compute/Runtime/tiles#TILE_PARTITION` pyramid content-addresses.
- Entry: `BimLod.Pyramid(ImportedGeometry geometry, InterchangePolicy policy)` derives the LOD chain over the policy's ratio schedule (each level a `Meshopt.Simplify` at decreasing target index count, falling back to `Meshopt.SimplifySloppy` when the error threshold cannot be met), and `BimLod.Meshlets(ImportedGeometry geometry)` clusters the residency band through `Meshopt.BuildMeshlets` (bounded by `Meshopt.BuildMeshletsBound`, optimized per meshlet through `Meshopt.OptimizeMeshlet`) — `Fin<T>` aborts on a degenerate decimation captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected`) lifting BARE onto the `Fin<T>` result (band 2600, `Fault`-derived), no `.ToError()` hop; each level seals its own `ExportArtifact.ContentKey` so the web peer streams each LOD by view distance, the `TileMetadata` per-tile semantic riding each level unchanged.
- Output: each `LodLevel` carries its target ratio, resulting triangle count, the world-space `WorldError` deviation (`Meshopt.Simplify`'s relative `result_error` × `SimplifyScale` — solver evidence the result keeps, a discarded `out` error being the deleted form), and the per-LOD content key — the same `InterchangeIdentity` the full-resolution `ExportArtifact` seals, computed per level so the `Rasm.Compute/Runtime/tiles#TILE_PARTITION` pyramid content-addresses every detail level and the cross-libs `WEB_GEOMETRY_RESIDENCY_WIRE` splat/meshlet manifest the AppUi projection mints streams each LOD by view distance against a real per-level error bound.
- Packages: Alimer.Bindings.MeshOptimizer, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm.Element (the shared `MeshletBand`/`MeshBlock` band), Rasm
- Growth: a new detail level is one ratio on the `InterchangePolicy.LodRatios` policy column (the schedule is policy data, never a fence-local constant), each landing one content-keyed `LodLevel` row on the pyramid; a new meshlet column is one field on the shared `MeshletBand`, never a second band beside it; the per-tile `TileMetadata` semantic rides each LOD unchanged; never a per-element full-resolution emit and never a second LOD or residency owner.
- Boundary: the LOD decimation is `Alimer.Bindings.MeshOptimizer`'s — `Meshopt.Simplify` (error-threshold decimation with `SimplificationOptions` flags) and `Meshopt.SimplifySloppy` (aggressive fallback) over the optimized indexed buffer own the LOD chain, and a hand-rolled edge-collapse decimator is the deleted form; the meshlet residency rides `Meshopt.BuildMeshlets` (allocated via `BuildMeshletsBound`, optimized per meshlet via `OptimizeMeshlet`) so the WebGPU raster path consumes the package-owned meshlet partition, never a hand-rolled cluster algorithm; the per-LOD content key meets `Rasm.Compute/Runtime/tiles#TILE_PARTITION` at the boundary — `Rasm.Bim` derives the per-element pyramid and seals each level's content key, the tile-pyramid partitioning and streaming stay at Compute consumed at the boundary; the residency band feeds the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest the AppUi projection mints, never a second residency owner; the LOD leg composes the same `ImportedGeometry` triangle-soup the `EXPORT_PIPELINE` `SceneOf` reads, never a second geometry carrier.

```csharp
public sealed record LodLevel(
    int Level, double TargetRatio, int TriangleCount, double WorldError,
    double Acmr, double Overdraw, double Overfetch,
    ReadOnlyMemory<uint> Indices, UInt128 ContentKey);


public static class BimLod {
    public static Fin<Seq<LodLevel>> Pyramid(ImportedGeometry geometry, InterchangePolicy policy) =>
        MeshLanes.Of(geometry).Bind(lanes =>
            Try.lift(() => Levels(geometry, lanes, policy)).Run().Bind(static inner => inner));

    static unsafe Seq<LodLevel> Levels(ImportedGeometry geometry, MeshLanes lanes, InterchangePolicy policy) {
        var source = new uint[geometry.Indices.Length];
        for (int i = 0; i < source.Length; i++) { source[i] = (uint)geometry.Indices.Span[i]; }
        float[] verts = lanes.Positions;
        nuint vertexCount = (nuint)geometry.VertexCount;
        nuint vertexStride = (nuint)(EncodingChannel.Position.Arity * sizeof(float));
        var (attributes, weights) = Attributes(geometry, lanes, policy);
        float scale;
        fixed (float* vPtr = verts) { scale = Meshopt.SimplifyScale(vPtr, vertexCount, vertexStride); }
        return policy.LodRatios.Map((ratio, level) =>
            Decimate(source, verts, attributes, weights, vertexCount, vertexStride, scale, ratio, level, geometry.FormatKey, policy));
    }

    static (float[] Lanes, float[] Weights) Attributes(ImportedGeometry geometry, MeshLanes lanes, InterchangePolicy policy) {
        var rows = policy.AttributeWeights
            .Choose(row => (row.Channel switch {
                    "geometry_normal" => Some((EncodingChannel.Normal, Source: lanes.Normals)),
                    "geometry_uv" => BimExport.Lane(geometry, EncodingChannel.Uv).Map(uv => (EncodingChannel.Uv, Source: uv)),
                    _ => Option<(EncodingChannel, float[] Source)>.None,
                })
                .Map(pair => (pair.Item1.Arity, row.Weight, pair.Source)))
            .ToSeq();
        int stride = rows.Sum(static row => row.Arity);
        var lanes = new float[geometry.VertexCount * stride];
        var weights = new float[stride];
        int lead = 0;
        foreach (var row in rows) {
            for (int v = 0; v < geometry.VertexCount; v++) {
                row.Source.AsSpan(v * row.Arity, row.Arity).CopyTo(lanes.AsSpan((v * stride) + lead, row.Arity));
            }
            weights.AsSpan(lead, row.Arity).Fill(row.Weight);
            lead += row.Arity;
        }
        return (lanes, weights);
    }

    static unsafe LodLevel Decimate(
        uint[] source, float[] verts, float[] attributes, float[] weights,
        nuint vertexCount, nuint vertexStride, float scale, double ratio, int level, string formatKey, InterchangePolicy policy) {
        nuint sourceCount = (nuint)source.Length;
        nuint targetCount = (nuint)((long)source.Length * ratio);
        nuint attributeCount = (nuint)weights.Length;
        nuint attributeStride = (nuint)(weights.Length * sizeof(float));
        var options = policy.Traits.Admits(ExportTrait.LockBorder) ? SimplificationOptions.SimplifyLockBorder : SimplificationOptions.None;
        var destination = new uint[source.Length];
        nuint resultCount = Meshopt.SimplifyWithAttributes(
            destination, source, verts, vertexStride, attributes, attributeStride, weights, attributeCount,
            ReadOnlySpan<byte>.Empty, targetCount, 0.01f, options, out float resultError);
        if (resultCount > targetCount) {
            fixed (uint* dst = destination)
            fixed (uint* src = source)
            fixed (float* vPtr = verts) {
                resultCount = Meshopt.SimplifySloppy(dst, src, sourceCount, vPtr, vertexCount, vertexStride, (byte*)null, targetCount, 0.05f, &resultError);
            }
        }
        var indices = destination.AsSpan(0, (int)resultCount).ToArray();
        var cache = Meshopt.AnalyzeVertexCache(indices, vertexCount, cacheSize: 16, warpSize: 0, primGroupSize: 0);
        var overdraw = Meshopt.AnalyzeOverdraw(indices, verts, vertexStride);
        var fetch = Meshopt.AnalyzeVertexFetch(indices, vertexCount, vertexStride);
        return new LodLevel(level, ratio, (int)resultCount / 3, resultError * scale,
            cache.acmr, overdraw.overdraw, fetch.overfetch, indices,
            ContentAddress.Of((formatKey, level, policy, indices), 0.0, static (s, writer) => writer
                .String($"{s.formatKey}:lod{s.level}").Double(s.policy.Chord.Value).Double(s.policy.Distance.Value).Double(s.policy.Angle.Value)
                .Raw(MemoryMarshal.AsBytes(s.indices.AsSpan()))).Value);
    }

    public static unsafe Fin<Seq<MeshletBand>> Meshlets(ImportedGeometry geometry) =>
        MeshLanes.Of(geometry).Bind(lanes =>
            Try.lift(() => Cluster(geometry, lanes)).Run().Bind(static inner => inner));

    static unsafe Seq<MeshletBand> Cluster(ImportedGeometry geometry, MeshLanes lanes) {
        var indices = new uint[geometry.Indices.Length];
        for (int i = 0; i < indices.Length; i++) { indices[i] = (uint)geometry.Indices.Span[i]; }
        float[] verts = lanes.Positions;
        nuint indexCount = (nuint)indices.Length;
        nuint vertexCount = (nuint)geometry.VertexCount;
        nuint vertexStride = (nuint)(EncodingChannel.Position.Arity * sizeof(float));
        const nuint maxVertices = 64, maxTriangles = 124;
        nuint bound = Meshopt.BuildMeshletsBound(indexCount, maxVertices, maxTriangles);
        var meshlets = new Meshlet[(int)bound];
        var meshletVertices = new uint[(int)bound * (int)maxVertices];
        var meshletTriangles = new byte[(int)bound * (int)maxTriangles * 3];
        var bounds = new Bounds[(int)bound];
        nuint count;
        fixed (Meshlet* mPtr = meshlets)
        fixed (uint* mvPtr = meshletVertices)
        fixed (byte* mtPtr = meshletTriangles)
        fixed (uint* iPtr = indices)
        fixed (float* vPtr = verts) {
            count = Meshopt.BuildMeshlets(mPtr, mvPtr, mtPtr, iPtr, indexCount, vPtr, vertexCount, vertexStride, maxVertices, maxTriangles, 0.0f);
            for (nuint m = 0; m < count; m++) {
                Meshopt.OptimizeMeshlet(&mvPtr[mPtr[m].vertex_offset], &mtPtr[mPtr[m].triangle_offset], mPtr[m].triangle_count, mPtr[m].vertex_count);
                bounds[(int)m] = Meshopt.ComputeMeshletBounds(
                    meshletVertices.AsSpan((int)mPtr[m].vertex_offset, (int)mPtr[m].vertex_count),
                    meshletTriangles.AsSpan((int)mPtr[m].triangle_offset, (int)mPtr[m].triangle_count * 3),
                    verts, vertexStride);
            }
        }
        var lead = geometry.Blocks[0];
        return toSeq(meshlets.AsSpan(0, (int)count).ToArray().Select((meshlet, m) => new MeshletBand(
            new MeshBlock(
                (int)meshlet.vertex_offset, (int)meshlet.vertex_count,
                (int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3,
                lead.Declared, lead.Material),
            (int)meshlet.vertex_count, (int)meshlet.triangle_count,
            new Graph.Vector3(bounds[m].center[0], bounds[m].center[1], bounds[m].center[2]),
            bounds[m].radius,
            new Graph.Vector3(bounds[m].cone_axis[0], bounds[m].cone_axis[1], bounds[m].cone_axis[2]),
            bounds[m].cone_cutoff)));
    }
}
```

## [05]-[SCHEDULE_ANIMATION]

- Owner: `ScheduleAnimation` the 4D-emit leg ADDITIVE to the export path — one glTF `Animation` baking the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element keyframe tracks: each `ConstructionTask`'s scheduled `Interval` drives a per-element visibility track (the element is invisible before its task starts and visible from its task start) with an optional scale track (the element grows from a zero-scale point to its full scale across its task window) so a viewer scrubs the construction sequence on the GLB timeline, and an optional in-progress base-colour track tints the element across its window through the material `KHR_animation_pointer` channel glTF's absent per-node colour property forces; `AnimationTrack` the per-element keyframe record carrying the element `GlobalId`, the appear-time and full-time seconds, the glTF `Node` the element's mesh binds to, and the logical material the colour track bound or the refusal a pooled material earns.
- Entry: `BimExport.AnimateSchedule(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy)` bakes the schedule into the scene model's animation set — projecting each `ConstructionTask` `Interval` bound onto its glTF time-in-seconds through `policy.SecondsOf(Instant moment, Instant projectStart)` (the bound mapped to the timeline via the NodaTime `Duration` from the project start, scaled by `policy.SecondsPerDay`), resolving each assigned element's glTF `Node` through the `Author`-minted `GlbScene` `GlobalId→Node` index (the element `GlobalId` is the shared `Graph/element#ELEMENT_GRAPH` `Object.ExternalId`; `Author` names each node by it, so the 4D leg binds the scene emitted — the retired caller-supplied index parameter is GONE), and authoring one `KHR_node_visibility` visibility channel (the `GrowInPlace` scale channel and the `policy.Tint` material base-colour channel when set) per element through the SharpGLTF `Animation.CreateVisibilityChannel`/`CreateScaleChannel`/`CreateMaterialPropertyChannel` keyframe surface — `Fin<T>` aborts on a SharpGLTF authoring fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected`) lifting BARE onto the `Fin<T>` result (band 2600, `Fault`-derived), no `.ToError()` hop; the animation and the `Planning/schedule#SCHEDULE` `ConstructionState.At` snapshot share one `Interval`-to-`Instant` time axis so a scrub at glTF time `t` shows exactly the element set `ConstructionState.At` resolves at the inverse instant (the schedule owner is the `BimModel`→`ElementGraph` cross-file alignment point its rebuild settles).
- Auto: `AnimateSchedule` registers `KHR_node_visibility`, creates one `Animation`, and folds each element's assigned task windows into an `AnimationTrack` (appear = earliest `Interval.Start`, full = latest `Interval.End`, so a multi-task element appears at its earliest task). Each element gets a visibility track popping in at its appear time under the `STEP` interpolation the `bool` channel forces, the optional scale track grows it from zero across its window under `LINEAR`, and the optional tint track drives its material's `baseColorFactor` from the policy's active factor at appear to the material's OWN authored factor at settle — bound only where one node references one material no second node shares, because `Staged` pools materials on the finish key and a shared material tints every repeat on one element's schedule. `policy.SecondsOf` projects an `Interval` bound onto the float-seconds axis the `ConstructionState.At` snapshot reads, so the keyframe author and the snapshot never carry two clocks; the scene returns with `LogicalAnimations` populated so `Emit` seals the animated GLB and the `TileMetadata` semantic rides each frame unchanged.
- Output: the `Seq<AnimationTrack>` is the 4D-emit evidence — each row carries the element `GlobalId`, the appear/full seconds, the bound `Node` logical index so the Cesium/three.js timeline scrub resolves the construction state at any timeline instant, and the `TintedMaterial` index the colour channel bound (absent where the policy carried no tint or a pooled material refused the 1:1 binding, so a reader tells an untinted run from a refused element instead of reading silence); the animated GLB the `WriteGlb` emits is the streamed 4D timeline a web viewer plays, the `Planning/schedule#SCHEDULE` `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` re-keying the animation only on a re-sequenced plan.
- Packages: SharpGLTF.Core, SharpGLTF.Runtime, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new NODE-scoped keyframe channel — a translation track lowering an element into place, a rotation track swinging it — is one `Animation.Create*Channel` arm on the same fold; a new MATERIAL-scoped channel is one `CreateMaterialPropertyChannel` pointer tail beside `BaseColorPointer`, because glTF declares no per-node colour or factor property and every material-valued track is a `KHR_animation_pointer` path onto `/materials/{index}/…`, authorable only on a material no second node references; a new interpolation mode rides the SharpGLTF `bool linear` channel knob; a new time-axis or tint policy is one column on `ScheduleAnimationPolicy` and a new render trait one `ExportTrait` row its corner law admits; never a per-element `Animation` instance, never a hand-authored glTF animation JSON block, and never a second time axis beside the `ConstructionState.At` `Interval`.
- Boundary: keyframes ride the SharpGLTF `ModelRoot.CreateAnimation` + `Animation.Create*Channel` surface — a hand-authored glTF `animations[]`/`samplers[]`/`channels[]` JSON block is the deleted form. `KHR_node_visibility` drives the per-element visibility keyframe, so the `bool` track is the settled `format#FORMAT_AXIS` `KhrExtension.NodeVisibility` row registered once through the factory — a custom visibility-by-opacity hack is the deleted form. `KHR_animation_pointer` drives the colour track through `CreateMaterialPropertyChannel`, and its `format#FORMAT_AXIS` `KhrExtension.AnimationPointer` row registers exactly when the policy carries a tint, so the row never advertises a capability the run does not exercise; a per-node colour track is UNSPELLABLE — glTF's node channels are translation, rotation, scale, weights, and visibility alone, so a colour is a material property and a hand-authored `KHR_materials_*` factor track beside the pointer channel is the deleted form. SharpGLTF.Runtime is already csproj-referenced and already exercised — `import#IMPORT_PIPELINE` decodes each logical mesh through its `IMeshDecoder<Material>` surface — so this leg needs no new package and no new `InterchangeFormat` row. Animation time is the `Planning/schedule#SCHEDULE` `ConstructionTask.Interval` projected to seconds; a second clock on the export side is the named contract violation, the `ConstructionState.At` snapshot and the keyframe author reading one `Interval`-to-`Instant` axis. Per-element glTF `Node` resolves through the `Author`-minted `GlbScene` index (nodes NAMED by the shared `Object.ExternalId`) — a caller-supplied index parameter, a re-walked scene graph, or a second index mint is the deleted form; a 4D-emit fault lowers onto `Model/faults#FAULT_BAND` `BimFault`.

```csharp
public sealed record ScheduleAnimationPolicy(
    double SecondsPerDay, CapabilitySet<ExportTrait> Traits, Duration KeyframeGap, Option<Vector4> Tint = default) {
    public static readonly ScheduleAnimationPolicy Default = new(
        SecondsPerDay: 1.0, Traits: CapabilitySet<ExportTrait>.None, KeyframeGap: Duration.FromMilliseconds(1));
    public static readonly ScheduleAnimationPolicy Growing = Default with { Traits = CapabilitySet<ExportTrait>.Of(ExportTrait.GrowInPlace) };
    public static readonly ScheduleAnimationPolicy Tinted = Default with { Tint = Some(new Vector4(1f, 0.62f, 0.09f, 1f)) };

    public float SecondsOf(Instant moment, Instant projectStart) =>
        (float)((moment - projectStart).TotalDays * SecondsPerDay);

    public float GapSeconds => (float)(KeyframeGap.TotalDays * SecondsPerDay);
}

public sealed record AnimationTrack(string GlobalId, float AppearSeconds, float FullSeconds, int NodeIndex, Option<int> TintedMaterial);

public static partial class BimExport {
    const string BaseColorPointer = "pbrMetallicRoughness/baseColorFactor";

    const string BaseColorChannel = "BaseColor";

    public static Fin<Seq<AnimationTrack>> AnimateSchedule(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy) =>
        Try.lift(() => Tracks(scene, network, policy)).Run().Bind(static inner => inner)
            ;

    static Seq<AnimationTrack> Tracks(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy) {
        var projectStart = network.Tasks.Min(static t => t.Scheduled.Start);
        var animation = scene.Model.CreateAnimation("construction-sequence");
        var taskWindow = network.Tasks.Fold(Map<string, Interval>(), static (held, task) => held.TryAdd(task.GlobalId, task.Scheduled));
        var references = scene.Nodes.Values.AsIterable()
            .Choose(static node => Optional(node.Mesh))
            .Bind(static mesh => mesh.Primitives.AsIterable()
                .Choose(static primitive => Optional(primitive.Material)).Map(static material => material.LogicalIndex).ToSeq().Distinct())
            .ToSeq()
            .GroupBy(static index => index)
            .Select(static group => (group.Key, group.Count()))
            .ToMap();
        var windows = network.Assignments
            .Bind(a => taskWindow.Find(a.TaskGlobalId)
                .Map(window => a.ElementGlobalIds.Map(id => (Id: id, Window: window))).IfNone(Seq<(string, Interval)>()))
            .GroupBy(static row => row.Item1)
            .Select(static g => (g.Key, toSeq(g.Select(static row => row.Item2))))
            .ToMap();
        return windows
            .Choose((globalId, spans) => scene.Nodes.Find(globalId)
                .Map(node => Track(scene, animation, node, globalId, spans, projectStart, policy, references)))
            .ToSeq();
    }

    static AnimationTrack Track(
        GlbScene scene, Animation animation, SharpGLTF.Schema2.Node node, string globalId, Seq<Interval> windows,
        Instant projectStart, ScheduleAnimationPolicy policy, Map<int, int> references) {
        float appear = windows.Min(w => policy.SecondsOf(w.Start, projectStart));
        float full = windows.Max(w => policy.SecondsOf(w.End, projectStart));
        float settled = Math.Max(full, appear + policy.GapSeconds);
        animation.CreateVisibilityChannel(node, new Dictionary<float, bool> {
            [Math.Max(0f, appear - policy.GapSeconds)] = false,
            [appear] = true,
        });
        if (policy.Traits.Admits(ExportTrait.GrowInPlace)) {
            animation.CreateScaleChannel(node, new Dictionary<float, Vector3> {
                [appear] = Vector3.Zero,
                [settled] = Vector3.One,
            });
        }
        return new AnimationTrack(globalId, appear, full, node.LogicalIndex, Tinted(scene, animation, node, appear, settled, policy, references));
    }

    static Option<int> Tinted(
        GlbScene scene, Animation animation, SharpGLTF.Schema2.Node node, float appear, float settled,
        ScheduleAnimationPolicy policy, Map<int, int> references) =>
        from active in policy.Tint
        from mesh in Optional(node.Mesh)
        from bound in Sole(mesh)
        where references.Find(bound.LogicalIndex).IfNone(0) == 1
        let material = scene.Model.LogicalMaterials[bound.LogicalIndex]
        from authored in Optional(material.FindChannel(BaseColorChannel)).Map(static channel => channel.Color)
        select Channelled(animation, material, appear, settled, active, authored);

    static Option<SharpGLTF.Schema2.Material> Sole(SharpGLTF.Schema2.Mesh mesh) =>
        mesh.Primitives.AsIterable().Choose(static primitive => Optional(primitive.Material)).ToSeq().Distinct() is { Count: 1 } single
            ? single.Head
            : Option<SharpGLTF.Schema2.Material>.None;

    static int Channelled(Animation animation, SharpGLTF.Schema2.Material material, float appear, float settled, Vector4 active, Vector4 authored) {
        animation.CreateMaterialPropertyChannel(material, BaseColorPointer, new Dictionary<float, Vector4> {
            [appear] = active,
            [settled] = authored,
        });
        return material.LogicalIndex;
    }
}
```

## [06]-[ROUNDTRIP]

- Owner: `RoundTrip` the lossless verification matrix folding a shared `ElementGraph` emit→re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations into a typed `RoundTripReport` that witnesses per-element AND per-property field fidelity by the contract's structured member diff joined on the 1:1 `ExternalId` GlobalId, so the codec proves losslessness rather than asserting it; `RoundTripReport` the per-format matrix partitioned by `InterchangeFormat` carrying the lossless-element count, the dropped-element set, and the per-element divergent-member set.
- Entry: `RoundTrip.Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` runs the source graph through one IFC serialization and back — emitting through the `EXPORT_PIPELINE` `BimExport.ExportIfc` (which delegates to `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`), re-decoding the artifact bytes through the `import#IMPORT_PIPELINE` `BimIo.ImportIfc` (the ONE `DatabaseIfc` decode owner — its `IfcWireForm.Sniff` schema sniff constructs the ifcXML/ifcJSON database at the EMITTED `ReleaseVersion`, so the reimport lands at the schema the export wrote, never the GeometryGym default [H8]; a page-local `new DatabaseIfc()` re-decode is the deleted form), re-projecting through a fresh `SemanticProjector(db, reconciler, profiles)` and folding the delta onto a `Genesis(source.Header)` seed through the shared `Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` (the `IfcLegality` constraint admitting the re-imported edges), then comparing the source and reimported graphs by baked-element member diff — `Fin<T>` aborts on a codec reject, a re-decode fault, or a predefined-gate reject in either leg (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Codec`/`Refused/BimReason.Rejected`/`Refused/BimReason.Unmapped`) lifting BARE onto the `Fin<T>` result (band 2600, `Fault`-derived), no `.ToError()` hop; `RoundTrip.Matrix(ElementGraph source, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` lifts the verify over the IFC STEP/XML/JSON triad (`InterchangeFormat.Ifc`/`IfcXml`/`IfcJson`) onto the per-format `Map<string, RoundTripReport>` fidelity matrix so a single call witnesses which serialization preserves which field.
- Auto: `Verify` emits the graph through one IFC serialization, re-decodes through `BimIo.ImportIfc`, re-projects and assembles the reimported `ElementGraph`, then folds the source-vs-reimported comparison through the shared diff — each rooted `Object` baked into an `Element` keyed by its 1:1 `ExternalId` GlobalId (the `NodeId` is freshly minted each re-ingest [H6], so the join is the GlobalId), a no-divergence element lossless, a divergence naming its changed members through the `Generator.Equals` `Inequalities` composed BARE — the noise axes (`Id`, `ExternalId`, `History`, `Parts`) are `[IgnoreEquality]` at the `Rasm.Element` owner, so no call-site filter roster exists — a source GlobalId absent from the reimport dropped. `RoundTripReport` reads the lossless count, the per-element divergent-member set (down to the exact `Properties[..].DataType`/`Quantities[..].Unit` path), and the dropped set; the geometry leg crosses the `tessellation#TESSELLATION_BRIDGE` companion, so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the companion. `Matrix` lifts `Verify` over the `InterchangeFormat` triad, keying the per-format reports so one matrix compares serializations.
- Output: the `RoundTripReport` per format is the codec-fidelity evidence — a per-format fidelity matrix proving which serialization preserves which field, an interchange-policy losslessness witness, and a codec regression oracle; the STEP report typically reads the highest match ratio (the canonical IFC physical file), the XML/JSON reports surfacing any serialization-specific field loss, and the divergent-member set the exact members a round-trip drops.
- Packages: GeometryGymIFC_Core, Rasm.Element, Generator.Equals, LanguageExt.Core, NodaTime, Rasm
- Growth: a new serialization format is one `InterchangeFormat` row the `Matrix` triad widens to; a new fidelity dimension (a placement-key match, a coverage round-trip) is one column on `RoundTripReport` over the same baked-element diff; a new comparison basis rides the existing `Generator.Equals` `Inequalities`; never a second element-comparison surface, never a per-format report record family, and never a parallel fidelity store.
- Boundary: the round-trip fold reuses the contract's `Generator.Equals` `Inequalities` member diff as the fidelity metric rather than minting a second element-comparison surface — a field-by-field string compare or a `Seq("content")` placeholder is the deleted form, the structured diff naming the EXACT divergent member path; the cycle composes the `EXPORT_PIPELINE` `ExportIfc` egress (itself delegating to `SemanticProjector.Emit`) and the `import#IMPORT_PIPELINE` `BimIo.ImportIfc` re-decode (the schema-sniffed `DatabaseIfc` owner — a THIRD page-local decode copy beside import/wire was the deleted form, and its missing sniff mis-reported the fidelity matrix at the wrong schema) folded through the shared `ProjectionAssembly.Assemble`, never the retired `BimModel.Project`/`IfcSemanticModel` lossy-row path and never a hand-rolled IFC re-author; the join is the stable 1:1 `ExternalId` GlobalId because the rooted `NodeId` is freshly minted each ingest [H6], and a NodeId-keyed join is the deleted form; the geometry leg crosses the `tessellation#TESSELLATION_BRIDGE` companion so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the same companion, and the verification couples to no host geometry type; the `RoundTripReport` is partitioned by `InterchangeFormat` over the one baked-element diff and a per-format `StepReport`/`XmlReport`/`JsonReport` class family is the deleted form; a round-trip rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` result (band 2600, `Fault`-derived), with no `.ToError()` hop.

```csharp
public sealed record RoundTripReport(
    string Format,
    int SourceCount,
    int LosslessCount,
    Seq<string> DroppedGlobalIds,
    Map<string, Seq<string>> LossyFields) {
    public double MatchRatio => SourceCount == 0 ? 1.0 : (double)LosslessCount / SourceCount;
    public bool Lossless => LosslessCount == SourceCount && DroppedGlobalIds.IsEmpty;
}

public static class RoundTrip {
    static readonly Seq<InterchangeFormat> IfcTriad =
        toSeq(InterchangeFormat.Items.Where(static f => f.RoundTrippable && f.Serialization.IsSome));

    public static Fin<RoundTripReport> Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        BimExport.ExportIfc(format, source, new SemanticProjector(new DatabaseIfc(), reconciler, profiles), InterchangePolicy.Canonical, clock, Option<EmitContext>.None, ctx.Key)
            .Bind(artifact => BimIo.ImportIfc(format, artifact.Bytes, ctx.Key))
            .Bind(db => ProjectionAssembly.Assemble(
                ProjectionSuite.Of(
                    Seq<IElementProjection>(new SemanticProjector(db, reconciler, profiles)),
                    Seq(ConstraintRegistration.Of(new IfcLegality()))),
                ElementGraph.Genesis(source.Header), ctx))
            .Map(static r => r.Graph)
            .Bind(reimported => Compare(format.Key, source, reimported, ctx.Key));

    public static Fin<Map<string, RoundTripReport>> Matrix(ElementGraph source, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        IfcTriad.TraverseM(format => Verify(source, format, ctx, clock, reconciler, profiles).Map(report => (format.Key, report))).As()
            .Map(static rows => rows.ToMap());

    static Fin<RoundTripReport> Compare(string formatKey, ElementGraph source, ElementGraph reimported) =>
        (ElementsByExternal(source).ToValidation(), ElementsByExternal(reimported).ToValidation())
            .Apply((sourceElements, reimportedElements) => {
                var dropped = sourceElements.Keys.Filter(id => !reimportedElements.ContainsKey(id)).ToSeq();
                var lossy = sourceElements
                    .Choose((id, element) => reimportedElements.Find(id)
                        .Map(other => Divergence(element, other))
                        .Filter(static fields => fields.IsEmpty == false));
                return new RoundTripReport(
                    formatKey, sourceElements.Count, sourceElements.Count - dropped.Count - lossy.Count, dropped, lossy);
            }).As().ToFin();

    static Fin<Map<string, Element>> ElementsByExternal(ElementGraph graph) =>
        graph.ObjectNodes
            .Choose(static o => o.ExternalId.Map(external => (External: external, o.Id)))
            .TraverseM(row => graph.Bake(row.Id).Map(element => (row.External, element))).As()
            .Map(static rows => rows.ToMap());

    static Seq<string> Divergence(Element source, Element reimported) =>
        toSeq(Element.EqualityComparer.Default.Inequalities(source, reimported))
            .Map(static i => i.Path.ToString());
}
```

## [07]-[TILE_AVAILABILITY]

- Owner: `TileAvailability` the 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream author over the `subtree` package — the tileset AVAILABILITY structure (the Morton-ordered tile/content/child-subtree bitstreams telling a 3D-Tiles client which implicit nodes exist) the `SharpGLTF.Ext.3DTiles` `[3]-[TILE_METADATA]` per-tile CONTENT author cannot reach, the two meeting at the shared Morton tile index; `TileNode` the scheme-neutral per-tile authoring coordinate — `Lod` the subdivision level (mapped onto the quadtree `subtree.Tile.Z` level field or the octree `subtree.Tile3D.Level`), `X`/`Y` the in-level position, `Z` the octree vertical axis (unused under the `Quadtree` scheme, where `subtree.Tile` carries no spatial third axis), with the `Available`/`ContentUri`/`GeometricError` columns the `subtree.Tile` node carries; `SubtreeArtifact` the authored binary beside the facts decoded back out of it and the kernel content key addressing it — retiring the hand-rolled implicit-tiling bitstream.
- Entry: `TileAvailability.Author(Seq<TileNode> tiles, ImplicitSubdivisionScheme scheme)` folds the tile list into the `.subtree` binary and READS IT BACK through `SubtreeReader.ReadSubtree` before returning, so the `SubtreeArtifact` it yields carries decoded facts and a bitstream that lost a node faults here rather than streaming nothing at a client, the `scheme` discriminant selecting the authoring root — `SubtreeCreator.GenerateSubtreefile(List<Tile>)` for `Quadtree` (each `TileNode` projected through `TileOf` onto `subtree.Tile(z: node.Lod, x, y, available)` so the LOD lands in the `Tile.Z` level field the Morton author folds on, carrying its `ContentUri`/`GeometricError`) and `SubtreeCreator3D.GenerateSubtreefile(List<Tile3D>)` for `Octree` (each projected through `TileOf3D` onto `subtree.Tile3D(level: node.Lod, x, y, z: node.Z)` so the octree gains its third spatial axis) — `Fin<T>` aborts on a degenerate tile list captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected`) lifting BARE onto the `Fin<T>` result (band 2600, `Fault`-derived), no `.ToError()` hop; `TileAvailability.AuthorMany(Seq<TileNode> tiles, ImplicitSubdivisionScheme scheme)` lifts to the matching `GenerateSubtreefiles` (the `Dictionary<Tile, byte[]>`/`Dictionary<Tile3D, byte[]>` multi-subtree overflow form) when the tileset exceeds one subtree's level budget, keying each binary by its root tile's `(Level, X, Y, Z)` coordinate (the library builds each root through `new Tile(level, x, y)`/`new Tile3D(level, x, y, z)`, so the root key reads the level-and-position identity, never the auxiliary `Tile.Lod` the author leaves zero).
- Auto: `Author` maps each `TileNode` onto the `subtree.Tile` node (or `subtree.Tile3D` under the `Octree` scheme), authors the binary availability bitstream, and witnesses it — the re-read `Subtree` record's `ContentAvailability` bit at each node's own `LevelOffset` + `MortonOrder` address must equal that node's `Available`, a uniform stream answering from its `*Constant` where the reader leaves the `BitArray` null, and a divergence faulting with the offending Morton positions named. `TileAvailability` is deliberately NOT the assertion target: `SubtreeCreator` derives it as the ancestor CLOSURE of the content set, so it answers "does an implicit node exist here" where content answers "does this node carry a payload", and `SubtreeArtifact` counts each stream separately. `MortonIndex` buckets each tile's availability by its level and sets the bit cell at its `X`/`Y`(`/Z`) position, so tile and content availability order identically — a tile is "available with content" exactly when both bitstreams set the same Morton position, the same index the `[3]-[TILE_METADATA]` tile content keys off. Multi-subtree tilesets re-base child coordinates so the child-subtree availability pointers resolve.
- Output: `SubtreeArtifact` carries the subdivision scheme, the authored level depth, the DECODED tile- and content-availability set counts, the `.subtree` bytes, and the kernel content key — measured off the emitted bitstream rather than re-reported off the input, so a caller reads what the client will read. Key minting rides the kernel seed-zero `ContentHash` over the shared `CanonicalWriter` fold, the one-hasher law `Sealed` and the `BimLod` per-level keys observe, so a tileset's availability binary and its glTF tile content address in ONE content space; the multi-subtree `AuthorMany` form returns the package's own `Fill`-padded per-root binaries, which are subtree-local bitstreams no whole-tileset node set addresses.
- Packages: subtree, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new subdivision scheme is the `ImplicitSubdivisionScheme.Quadtree`/`Octree` discriminant the `SubtreeCreator`/`SubtreeCreator3D` pair already carries; a new availability column is one field on `TileNode` the `subtree.Tile` node exposes; a new decoded fact is one column on `SubtreeArtifact` read off the re-parsed `Subtree` record, never a value copied forward from the input; a multi-subtree overflow is the existing `GenerateSubtreefiles` form; never a hand-rolled Morton/bitstream codec and never a second availability authoring path beside `subtree`.
- Boundary: the `.subtree` availability authoring is `subtree`'s — `SubtreeCreator`/`SubtreeCreator3D` `GenerateSubtreefile`/`GenerateSubtreefiles`, the `Tile`/`Tile3D` authoring nodes, the `MortonOrder.Encode2D`/`Encode3D` z-order index composed with `LevelOffset.GetLevelOffset` into the one bit address, and `SubtreeReader.ReadSubtree` driving the `Author` round-trip witness own the bitstream, and a hand-rolled implicit-tiling bitstream or a hand-rolled Morton address beside the package's own is the retired form; the content/availability split is the law — `SharpGLTF.Ext.3DTiles` (`[3]-[TILE_METADATA]`) authors the per-tile glTF CONTENT and `EXT_structural_metadata`, `subtree` authors the tileset AVAILABILITY indexing which implicit nodes exist, the two meeting at the shared Morton tile index and never duplicating the availability logic; the tileset.json root hierarchy and the per-tile bounding-volume geometry stay outside this owner (the `subtree` package carries no tileset.json and no geometry), and the tile-pyramid partitioning/streaming stay at `Rasm.Compute/Runtime/tiles#TILE_PARTITION` consumed at the boundary — `Rasm.Bim` authors the availability binary and the content glTF, never the pyramid.

```csharp
public sealed record TileNode(int Lod, int X, int Y, bool Available, string ContentUri, double GeometricError, int Z = 0);

public sealed record SubtreeArtifact(
    subtree.ImplicitSubdivisionScheme Scheme, int Levels, int AvailableTiles, int ContentTiles,
    ReadOnlyMemory<byte> Bytes, UInt128 ContentKey);

public static class TileAvailability {
    public static Fin<SubtreeArtifact> Author(Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme) =>
        Try.lift(() => scheme == subtree.ImplicitSubdivisionScheme.Octree
                ? subtree.SubtreeCreator3D.GenerateSubtreefile(tiles.Map(TileOf3D).ToList())
                : subtree.SubtreeCreator.GenerateSubtreefile(tiles.Map(TileOf).ToList())).Run().Bind(static inner => inner)
            .Bind(binary => Witness(binary, tiles, scheme));

    static Fin<SubtreeArtifact> Witness(byte[] binary, Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme) {
        int levels = tiles.Max(static node => node.Lod) + 1;
        int cells = subtree.LevelOffset.GetLevelOffset(levels, scheme);
        return Try.lift(() => subtree.SubtreeReader.ReadSubtree(new MemoryStream(binary, writable: false))).Run().Bind(static inner => inner)
            .Bind(read => tiles
                .Filter(node => Bit(read.ContentAvailability, read.ContentAvailabilityConstant, Position(node, scheme)) != node.Available)
                .Map(node => Position(node, scheme)) is var divergent && divergent.IsEmpty
                ? Fin.Succ(new SubtreeArtifact(
                    scheme, levels,
                    Set(read.TileAvailability, read.TileAvailabilityConstant, cells),
                    Set(read.ContentAvailability, read.ContentAvailabilityConstant, cells),
                    binary,
                    ContentHash.Of((scheme, binary), static (s, writer) => writer.String($"subtree:{s.scheme}").Raw(s.binary))))
                : Fin.Fail<SubtreeArtifact>(
                    new BimFault.Refused(BimScope.Export, BimReason.Rejected, string.Join(':', new object?[] { "subtree-availability-mismatch", divergent.Count.ToString(), string.Join(',', divergent.Take(4)) }))));
    }

    static int Position(TileNode node, subtree.ImplicitSubdivisionScheme scheme) =>
        subtree.LevelOffset.GetLevelOffset(node.Lod, scheme)
        + (int)(scheme == subtree.ImplicitSubdivisionScheme.Octree
            ? subtree.MortonOrder.Encode3D((ulong)node.X, (ulong)node.Y, (ulong)node.Z)
            : subtree.MortonOrder.Encode2D((uint)node.X, (uint)node.Y));

    static bool Bit(System.Collections.BitArray? bits, int constant, int at) =>
        bits is { } array ? at < array.Length && array[at] : constant != 0;

    static int Set(System.Collections.BitArray? bits, int constant, int cells) =>
        bits is { } array ? array.Cast<bool>().Count(static bit => bit) : constant != 0 ? cells : 0;

    public static Fin<Map<(int Level, int X, int Y, int Z), byte[]>> AuthorMany(Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme) =>
        Try.lift(() => scheme == subtree.ImplicitSubdivisionScheme.Octree
                ? subtree.SubtreeCreator3D.GenerateSubtreefiles(tiles.Map(TileOf3D).ToList())
                    .Select(static pair => ((pair.Key.Level, pair.Key.X, pair.Key.Y, pair.Key.Z), pair.Value)).ToMap()
                : subtree.SubtreeCreator.GenerateSubtreefiles(tiles.Map(TileOf).ToList())
                    .Select(static pair => ((pair.Key.Z, pair.Key.X, pair.Key.Y, 0), pair.Value)).ToMap()).Run().Bind(static inner => inner);

    static subtree.Tile TileOf(TileNode node) =>
        new(node.Lod, node.X, node.Y, node.Available) { ContentUri = node.ContentUri, GeometricError = node.GeometricError };

    static subtree.Tile3D TileOf3D(TileNode node) =>
        new(node.Lod, node.X, node.Y, node.Z) { Available = node.Available };
}
```

## [08]-[COBIE_EMIT]

- Owner: `CobieEmit` the COBie 2.4 FM-handover author — a TRANSIENT `Xbim.IO.CobieExpress` `CobieModel` authored `Instances.New<T>` inside one transaction FROM the shared `ElementGraph` (never a held xBIM `IModel` authority beside the GeometryGym semantic authority, and never the `IfcToCoBieExpressExchanger` parallel xBIM-IFC reader — the element graph IS the source), sealed to the XLSX deliverable through the store's `ExportToTable` bridge.
- Entry: `CobieEmit.Export(ElementGraph graph, Instant at)` → `Fin<CobieHandover>` (the sealed artifact beside the typed `CobieDegrade` roster) — folds the `Model/spatial#SPATIAL_STRUCTURE` view onto `CobieFacility`/`CobieFloor`/`CobieSpace`, each baked element onto `CobieComponent` (its reconciled type onto `CobieType`, deduplicated by the type node), and each Pset row onto a `CobieAttribute` per the `Semantics/properties#PROPERTY_TEMPLATES` template vocabulary (the template supplying the COBie attribute name and the shared `PropertyValue.Render` the value text); the artifact content key mints through the kernel seed-zero `ContentHash.Of` over the shared `CanonicalWriter` fold — the one content space every Exchange artifact addresses, never a second identity scheme.
- Auto: authoring is ONE `BeginTransaction` scope committed once; the spatial containment restores from the shared `Compose.Contain` edges so a component lands on its floor/space, the type join rides the `Assign.TypeDefinition` edge, and an element with no spatial host lands facility-scoped rather than dropping; `CobieAttribute` values render through the SAME shared typed-value family the IFC egress raises, so a COBie cell and a Pset re-emit never disagree.
- Output: the sealed `ExportArtifact` carries the XLSX bytes, the `InterchangeFormat` row, and the kernel content key — the FM-handover deliverable the CDE registers beside the IFC emit of the same graph.
- Packages: Xbim.CobieExpress, Xbim.IO.CobieExpress, Xbim.CobieExpress.Exchanger (transitively the `EntityFactoryCobieExpress` schema factory), Rasm.Element, Rasm, LanguageExt.Core.
- Growth: a new COBie sheet is one `Instances.New<T>` fold arm over an existing contract read (a `CobieSystem` arm off the `Model/systems#SYSTEM_TRACE` rows, a `CobieZone` arm off `Model/zones#ZONE_GRAPH`, a `CobieContact` arm off `OwnerHistory`); a new attribute source is one template row on the properties owner — never a second COBie model, never a per-sheet exporter family.
- Boundary: the element graph is the ONLY source — `IfcToCoBieExpressExchanger` (the xBIM IFC→COBie exchanger) reads an xBIM `IModel`, a PARALLEL IFC stack to the GeometryGym authority, so composing it stands a second IFC reader (the named violation; the exchanger package is admitted for its schema factory only); the `CobieModel` is construct→author→export→dispose inside `Export` — a cached/held store is the deleted form; `properties.md` is the source VOCABULARY (the template names and datatypes), never re-derived here; the content key is the kernel `ContentHash` + shared `CanonicalWriter` (a `Rasm.Compute` `InterchangeIdentity` mint is the deleted downward strata reference).

```csharp

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CobieReason {
    public static readonly CobieReason FacilityMissing = new("facility-missing");
    public static readonly CobieReason ElementUnbakeable = new("element-unbakeable");
    public static readonly CobieReason TemplateUnmapped = new("template-unmapped");
    public static readonly CobieReason TypeUnresolved = new("type-unresolved");
    public static readonly CobieReason ValueUnrenderable = new("value-unrenderable");
}

public readonly record struct CobieDegrade(CobieReason Reason, string Subject, Option<Error> Cause = default);

public sealed record CobieHandover(ExportArtifact Artifact, Seq<CobieDegrade> Degrades);

public static class CobieEmit {
    public static Fin<CobieHandover> Export(ElementGraph graph, Instant at) =>
        Try.lift(() => {
            using var model = new CobieModel();
            Seq<CobieDegrade> degrades;
            using (var txn = model.BeginTransaction("rasm-cobie")) {
                degrades = Author(model, graph);
                txn.Commit();
            }
            using var stream = new MemoryStream();
            model.ExportToTable(stream, ExcelTypeEnum.XLSX, out string report);
            return new CobieHandover(
                BimExport.Sealed(InterchangeFormat.Cobie, stream.ToArray(), InterchangePolicy.Canonical, at),
                report.Length > 0 ? degrades.Add(new CobieDegrade(CobieReason.TemplateUnmapped, report)) : degrades);
        }).Run().Bind(static inner => inner);

    static Seq<CobieDegrade> Author(CobieModel model, ElementGraph graph) =>
        graph.ObjectNodes.Find(static o => o.Classification.Code == IfcClass.Building.Key).Match(
            None: () => Seq(new CobieDegrade(CobieReason.FacilityMissing, graph.Header.Schema.ToString())),
            Some: root => Registered(model, graph, root));

    static Seq<CobieDegrade> Registered(CobieModel model, ElementGraph graph, Node.Object root) {
        CobieFacility facility = model.Instances.New<CobieFacility>(f => Named(f, root));
        HashMap<NodeId, CobieSpace> spaces = Parts(graph, root.Id, IfcClass.BuildingStorey).Fold(
            HashMap<NodeId, CobieSpace>(),
            (held, storey) => {
                CobieFloor floor = model.Instances.New<CobieFloor>(f => { Named(f, storey); f.Facility = facility; });
                return Parts(graph, storey.Id, IfcClass.Space).Fold(held, (inner, space) =>
                    inner.Add(space.Id, model.Instances.New<CobieSpace>(s => { Named(s, space); s.Floor = floor; })));
            });
        Seq<Node.Object> occurrences = graph.ObjectNodes.Filter(static o => o.Kind == ObjectKind.Occurrence);
        Map<(string Code, string Token), Map<string, PropertyTemplate>> templates =
            occurrences.Map(static o => (o.Classification.Code, o.PredefinedType.ToValue())).Distinct()
                .Fold(Map<(string, string), Map<string, PropertyTemplate>>(), (acc, pair) =>
                    IfcClass.TryGet(pair.Code).Match(
                        None: () => acc,
                        Some: cls => acc.Add(pair, PropertyKey.Resolve(
                            cls,
                            Optional(pair.Token).Filter(static t => t.Length > 0 && t != PredefinedType.NotDefined.Token),
                            graph.Header.Schema, TemplateScope.Cobie, None))));
        var types = new Dictionary<NodeId, CobieType>();
        return occurrences
            .Filter(node => !spaces.ContainsKey(node.Id)
                && node.Classification.Code != IfcClass.Building.Key
                && node.Classification.Code != IfcClass.BuildingStorey.Key)
            .Fold(Seq<CobieDegrade>(), (log, node) => Landed(model, graph, node, spaces, types, templates, log));
    }

    static Seq<CobieDegrade> Landed(
        CobieModel model, ElementGraph graph, Node.Object node, HashMap<NodeId, CobieSpace> spaces,
        Dictionary<NodeId, CobieType> types, Map<(string Code, string Token), Map<string, PropertyTemplate>> templates,
        Seq<CobieDegrade> log) =>
        graph.Bake(node.Id).Match(
            Succ: baked => Landed(model, graph, node, spaces, types, templates, log, baked),
            Fail: error => log.Add(new CobieDegrade(CobieReason.ElementUnbakeable, Identity(node), Some(error))));

    static Seq<CobieDegrade> Landed(
        CobieModel model, ElementGraph graph, Node.Object node, HashMap<NodeId, CobieSpace> spaces,
        Dictionary<NodeId, CobieType> types, Map<(string Code, string Token), Map<string, PropertyTemplate>> templates,
        Seq<CobieDegrade> log, Element baked) {
        CobieComponent component = model.Instances.New<CobieComponent>(c => {
            Named(c, node);
            c.TagNumber = node.Tag;
            c.AssetIdentifier = Identity(node);
        });
        Host(graph, node, spaces).IfSome(space => component.Spaces.Add(space));
        Seq<CobieDegrade> typed = TypeOf(model, graph, node, types).Match(
            Some: type => { component.Type = type; return log; },
            None: () => log.Add(new CobieDegrade(CobieReason.TypeUnresolved, Identity(node))));
        Map<string, PropertyTemplate> resolved = templates
            .Find((node.Classification.Code, node.PredefinedType.ToValue()))
            .IfNone(Map<string, PropertyTemplate>());
        return baked.Properties.Fold(typed, (held, bag) => Attributes(model, component, bag, resolved, held));
    }

    static void Named(CobieAsset asset, Node.Object node) {
        asset.Name = node.Name;
        asset.Description = node.Classification.Code;
        asset.ExternalId = Identity(node);
    }

    static string Identity(Node.Object node) =>
        node.ExternalId.IfNone(node.Id.ToValue());

    static Option<CobieSpace> Host(ElementGraph graph, Node.Object node, HashMap<NodeId, CobieSpace> spaces) =>
        toSeq(graph.EdgesAt(node.Id)).Choose(e =>
            e is Relationship.Compose c && c.Part == node.Id && c.SubKind != ComposeKind.Reference
                ? spaces.Find(c.Whole)
                : None).Head;

    static Option<CobieType> TypeOf(CobieModel model, ElementGraph graph, Node.Object node, Dictionary<NodeId, CobieType> held) =>
        toSeq(graph.EdgesAt(node.Id)).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == node.Id
                ? graph.Find<Node.Object>(a.Definition)
                : None).Head
            .Map(type => held.TryGetValue(type.Id, out CobieType? seated)
                ? seated
                : held[type.Id] = model.Instances.New<CobieType>(t => Named(t, type)));

    static Seq<CobieDegrade> Attributes(
        CobieModel model, CobieAsset asset, PropertyBag bag, Map<string, PropertyTemplate> templates, Seq<CobieDegrade> log) =>
        bag.Values.AsIterable().Fold(log, (held, row) =>
            templates.Find($"{bag.SetName}.{row.Key}").Match(
                None: () => held.Add(new CobieDegrade(CobieReason.TemplateUnmapped, $"{bag.SetName}.{row.Key}")),
                Some: template => {
                    CobieAttribute attribute = model.Instances.New<CobieAttribute>(a => {
                        a.Name = template.Code;
                        a.Description = bag.SetName;
                        a.Unit = template.Unit.IfNone(() => template.SiDimension.Bind(static d => d.SiSymbol).IfNone(""));
                    });
                    asset.Attributes.Add(attribute);
                    return Valued(attribute, row.Value)
                        ? held
                        : held.Add(new CobieDegrade(CobieReason.ValueUnrenderable, $"{bag.SetName}.{row.Key}"));
                }));

    static bool Valued(CobieAttribute attribute, PropertyValue value) => value.Switch(
        state: attribute,
        measure:    static (a, m) => { a.Set(m.Value.Si); return true; },
        boolean:    static (a, b) => { a.Set(b.Value); return true; },
        text:       static (a, t) => { a.Set(t.Value); return true; },
        enumerated: static (a, e) => { a.Set(string.Join(',', e.Selected)); return true; },
        temporal:   static (a, t) => t.Value is TemporalValue.Stamp stamp && Set(a, stamp),
        reference:  static (a, r) => { a.Set(r.Value); return true; });

    static bool Set(CobieAttribute attribute, TemporalValue.Stamp stamp) {
        attribute.Set(stamp.At.ToDateTimeUtc());
        return true;
    }

    static Seq<Node.Object> Parts(ElementGraph graph, NodeId whole, IfcClass @class) =>
        toSeq(graph.EdgesAt(whole)).Choose(e =>
            e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference
                ? graph.Find<Node.Object>(c.Part).Filter(o => o.Classification.Code == @class.Key)
                : None);
}
```

## [09]-[SAF_EMIT]

- Owner: `SafEmit` the SAF structural-analysis XLSX author — GRAPH-SOURCED like the COBie leg and sharing its office-spreadsheet media type: the shared `ElementGraph` lowers through the ONE `Exchange/saf#SAF_EXCHANGE` `Workbook(graph, geometry, regime, key)` fold (the `Correspondence` rows its member spine, geometry crossing ONLY by content key through the shared `GeometrySource` port, the stated `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime` electing the workbook's `ExcelNationalCode` design code), and the `SafCodec.Run` export leg validates the model and writes the workbook bytes this path seals.
- Entry: `SafEmit.Export(ElementGraph graph, GeometrySource geometry, Option<AnnexRegime> regime, SafServices services, Instant at)` → `Fin<ExportArtifact>` — the artifact content key mints through the ONE `Sealed` funnel every format arm shares, so the `rasm.bim.exchange.exported` observe point fires for a SAF emit exactly as for a GLB one and `BimFact.Exported` reads identically at the composition edge.
- Auto: the lowering and its named negatives are the structural owner's — the eccentricity STEP fragment, the thermal gradient rows, and the EN combination roster name no SAF cell and stay off the workbook by that owner's stated arms; validation severity gates inside the `Saf` export leg, so an Error-carrying model refuses typed before any byte is sealed.
- Packages: StructuralAnalysisFormat, Rasm.Element, Rasm, NodaTime, LanguageExt.Core.
- Growth: a new SAF worksheet is one arm on the structural owner's `Workbook`/`Author` folds beside its roster row — this path gains nothing; a new SAF schema version is the `SafServices.Target` value the composition states, never a version knob minted here.
- Boundary: the element graph is the ONLY source and the `Exchange/saf#SAF_EXCHANGE` `Workbook` fold the ONLY lowering — a second Generic-edge walker or a result-side `ExcelModel` assembly is the deleted parallel form; the SAF service contracts cross only as the wired `SafServices` dependency surface `import#IMPORT_PIPELINE` declares, so ONE wiring serves both directions; stream custody is one `MemoryStream` inside the entry sealed through `BimExport.Sealed` under `InterchangePolicy.Canonical` exactly as the COBie author seals — `IExcelExportService.Export` writes the stream directly, so no path-bound temp file crosses this leg.

```csharp
public static class SafEmit {
    public static Fin<ExportArtifact> Export(
        ElementGraph graph, GeometrySource geometry, Option<AnnexRegime> regime, SafServices services, Instant at) =>
        SafCodec.Workbook(graph, geometry, regime).Bind(model => {
            using MemoryStream stream = new();
            return SafCodec.Run(
                    new SafOp.Export(stream, model, services.Target),
                    services.Imports, services.Exports, services.Validator)
                .Map(_ => BimExport.Sealed(InterchangeFormat.Saf, stream.ToArray(), InterchangePolicy.Canonical, at));
        });
}
```

## [10]-[RESEARCH]

(none)
