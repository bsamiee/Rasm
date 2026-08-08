# [BIM_EXPORT_RAIL]

`BimExport.Export` folds one TOTAL `InterchangeCodec.Switch` over the `ExportPayload` union — `Soup` the flat `ImportedGeometry` triangle carrier, `Scene` the content-keyed per-element `ElementScene` — dispatching GLB through SharpGLTF under Draco/meshopt encode, `.bim` through the `dotbim` instancing wire, FBX/Collada through `AssimpContext.ExportToBlob`, OpenUSD through `UniversalSceneDescription` `UsdStage`, and 3D-Tiles `.subtree` availability through `SubtreeCreator`; the Switch mirrors `import#IMPORT_RAIL`, so a new codec row BREAKS this call site at compile time.

`BimExport.Author` mints the per-element glTF scene as a `GlbScene` — one `NodeBuilder` per element NAMED by its seam GlobalId, one logical mesh per distinct content key, N repeats travelling as N nodes over ONE mesh with `EXT_mesh_gpu_instancing` a policy threshold — so the `GlobalId`→`Node` index `TileMetadata` and `AnimateSchedule` bind against is MINTED HERE, never caller-walked.

IFC STEP/XML/JSON never re-authors here: `ExportIfc` DELEGATES to the seam `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit` — the ONE Bim-internal `ElementGraph`→`DatabaseIfc` re-author — this rail OWNING only the artifact seal (`ExportArtifact` with the Compute content key) and reading serialization off the `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column.

Settled vocabulary arrives from the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`/`Element` (a consumer reads the baked `Element`, never a stored record), the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` carrier the `import#IMPORT_RAIL` produces and `BimIo.ImportIfc` re-decode, the `format#FORMAT_AXIS` codec/extension rows, and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` content key; a sealed `ExportArtifact` feeds that Compute seam, and every emit stays HOST-LOCAL.

## [01]-[INDEX]

- [02]-[EXPORT_RAIL]: artifact emit — the `ExportPayload` `Soup`/`Scene` union through one TOTAL `InterchangeCodec.Switch`; the `GltfChannel` canonical-channel roster and the `MaterialFinish`/`ChannelImage` pooled material identity binding every texture map onto one `MaterialBuilder`; the IFC leg DELEGATING to the seam `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, this rail owning only the `ExportArtifact` seal and the `InterchangeFormat.Serialization` column read.
- [03]-[TILE_METADATA]: per-tile `EXT_structural_metadata` schema/class/property-table over the seam `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, not a stored record), bound through `EXT_mesh_features` over the `Staged`-authored per-vertex `_FEATURE_ID_0` row stamps the `GlbScene.Rows` index names.
- [04]-[BIM_LOD]: `Meshopt.Simplify`/`SimplifySloppy` build the per-element LOD pyramid, `Meshopt.BuildMeshlets` bands meshlet residency, and each LOD carries the content key the `Rasm.Compute/Runtime/codecs#TILE_PARTITION` pyramid addresses.
- [05]-[SCHEDULE_ANIMATION]: `AnimateSchedule` bakes the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element glTF visibility/scale keyframe tracks through `ModelRoot.CreateAnimation` and the `KHR_node_visibility` channel over the `Author`-minted `GlbScene` `GlobalId`→`Node` index, the in-progress tint riding the `KHR_animation_pointer` material base-colour channel — a 4D schedule exports as one animated GLB a web viewer scrubs.
- [06]-[ROUNDTRIP]: `RoundTrip` folds an `ElementGraph` emit→`BimIo.ImportIfc` schema-sniffed re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations into one lossless-verification matrix, witnessing per-element fidelity by the seam content key joined on the 1:1 `ExternalId` and naming the divergent members through the `Generator.Equals` structured diff.
- [07]-[TILE_AVAILABILITY]: `TileAvailability` authors the 3D-Tiles 1.1 implicit-tiling `.subtree` availability bitstream over the `subtree` `SubtreeCreator`/`SubtreeCreator3D`/`Tile`/`Tile3D`/`MortonIndex` surface and witnesses it back through `SubtreeReader.ReadSubtree` onto a content-keyed `SubtreeReceipt`, completing the tileset side the `SharpGLTF.Ext.3DTiles` per-tile content leg cannot reach and retiring the hand-rolled implicit-tiling bitstream.
- [08]-[COBIE_EMIT]: `CobieEmit.Export` the COBie FM-handover XLSX author — a transient `CobieModel` folded `Instances.New<T>` from the seam `ElementGraph` (facility/floor/space, type/component, `CobieAttribute` rows) and sealed through `ExportToTable`, content-keyed off the kernel `ContentHash`; never a held xBIM `IModel` and never the parallel xBIM IFC reader.

## [02]-[EXPORT_RAIL]

- Owner: `BimExport` — the export fold over `InterchangeFormat`, one TOTAL generated `InterchangeCodec.Switch` over the `ExportPayload` union (`Soup(ImportedGeometry)` | `Scene(ElementScene)`), the IFC STEP/XML/JSON leg DELEGATING to the seam `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit` (this rail seals the bytes, the projector owns the re-author); `ElementScene` the per-element carrier — a `Map<UInt128, ImportedGeometry>` content-keyed mesh pool with placed `ElementInstance` rows (GlobalId, name, classification code, mesh key, rigid `Matrix4x4` placement, `MaterialFinish`) — so repeated geometry travels ONCE; `GltfChannel` the `[SmartEnum<string>]` roster projecting a CANONICAL texture-channel name onto the glTF `KnownChannel` targets it binds and the `format#FORMAT_AXIS` `KhrExtension` it obliges; `MaterialFinish` the pooled material identity pairing the seam `Rasm.Element/Graph/element#NODE_MODEL` `AppearanceSummary` with its `ChannelImage` bindings and its `DoubleSided` render-representation bit, and owning the whole `MaterialBuilder` mint; `ChannelImage` one bound glTF texture map (its resolved `GltfChannel`, already-encoded bytes beside the core-container fallback bytes an extension-obliging primary degrades to, UV set, wrap pair, min/mag filter pair, optional `Semantics/appearance#APPEARANCE_PROJECTION` `UvTransform` frame, and the `KhrExtension` row its container obliges); `GlbScene` the `Author`-minted `(ModelRoot, Map<string, Node>, Map<string, int>)` triple carrying the per-element node index AND the `GlobalId`→feature-row index downstream legs bind; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `BimExport.Export` is the ONE entry, its `ExportPayload` case discriminating flat-soup from per-element emit per MODAL_ARITY (a `bool perElement` knob beside the value is the rejected form); `BimExport.Author` mints the `GlbScene` the metadata/animation legs decorate before `Emit` seals it; `BimExport.ExportIfc` carries the IFC serialization — its `graph` a seam read snapshot, its `projector` the Bim-internal IFC-egress owner the app wires, its `EmitContext` riding through whole (the diff-prior `ChangeAction` snapshot [H9], the scoped partial-export selection, the declared unit regime), the profile store staying the projector's ctor-held capability. `Fin<T>` aborts on a write-capability miss (`Model/faults#FAULT_BAND` `BimFault.CodecReject`), a route miss the total `Switch` names with its owning rail in the message, an empty scene pool or an arena re-mint refusal the `ExportPayload.Flat` flatten surfaces, or a captured serialization/predefined-gate fault the projector lowers (`BimFault.ModelRejected`/`BimFault.UnmappedClass`), each typed case (band 2600, `Expected`-derived) lifting BARE onto the rail with no `.ToError()` hop.
- Auto: `GlbBytes` switches on `InterchangePolicy.Compression` — `KhrEncoder.None` routes `Soup` through the single-mesh `SceneOf` and `Scene` through `Author`, both writing the GLB container, while `Draco` and `Meshopt` bypass the container: neither compression codec takes a glTF `ModelRoot`, so the compression leg REPLACES the GLB write rather than post-processing it, and a per-element `Scene` flattens through `ElementScene.Soup` because the raw streams carry no scene graph (per-element structure rides the GLB arms only). `.bim` pools distinct geometry by content key and places each `ElementInstance`, so instancing survives the wire. IFC selects no serialization writer here — `ExportIfc` reads the `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column (`Some` exactly on the GeometryGym rows, its value the `Projection/egress#IFC_EGRESS` `IfcWireForm` row carrying serialization AND container together) and hands the seam `ElementGraph` and that wire form to `SemanticProjector.Emit`, which re-authors the whole graph and returns the IFC BYTES this rail seals — plain STEP, the zipped `.ifczip` container, ifcXML, or ifcJSON, each the row's own seal, so no transcode and no zip happens on this rail; no `DatabaseIfc` is constructed on this page.
- Receipt: the `ModelEmit` receipt case carries the format key, codec key, emitted byte count, and the `ExportArtifact.ContentKey` the Compute addressing seam computes, symmetric to the import `ModelLoad` case; emission rides the sink port at the composition edge.
- Events: a sealed `ExportArtifact` mints the `Exchange/events#EVENTS` `BimEvent.ArtifactMinted` row at the composing rail's edge — the Compute-computed `ContentKey`, the `InterchangeFormat.Key`, and the emitted byte count off the `ModelEmit` receipt — subject the `key:kind` artifact address; envelope seal and transport are the events owner's, never a second emit path here.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, CommunityToolkit.HighPerformance, AssimpNetter, dotbim, UniversalSceneDescription, Rasm.Element, Rasm (the kernel `Drawing` arena — `EncodedGeometry.Descriptors`/`Channel`, `Encode.Of`, the `EncodingChannel` roster, `ChannelDtype.Unpack`, `ContentHash`/`CanonicalWriter`), NodaTime, LanguageExt.Core
- Growth: a new managed export is one arm on the TOTAL `InterchangeCodec.Switch` — the compiler forces the arm the moment the codec row lands (the `dotBim` instancing arm joined the SharpGltf/SceneExchange/UsdStage arms this way), never a per-format exporter family and never a silent ladder tail; a new emit modality is one `ExportPayload` case every codec arm is compiler-forced to route; a new IFC serialization is the `InterchangeFormat.Serialization` column value on one GeometryGym row; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row the `Writables` narrowing admits; a new BOUND texture channel is one `GltfChannel` row naming its `KnownChannel` targets and its obliged extension — `ChannelImage.Of` admits it, `MaterialFinish.Author` binds it with zero arm, and the `Obliges` union declares the row — a new appearance FACTOR is one column on the seam `AppearanceSummary` read once in `Shaded`, a new RENDER-REPRESENTATION toggle (the sidedness bit is the landed one) is one column on `MaterialFinish` its own `Key` seed frames, and a new sampler, degradation, or transform axis is one column on `ChannelImage` the `MaterialFinish.Key` `Sampled` frame folds so the pool keeps discriminating it, never a second material builder; a new compression encoder is one `KhrEncoder` arm on the `GlbBytes` fold; a new PER-VERTEX attribute is one kernel `EncodingChannel` row — the descriptor-addressed `Lane` read, the `ElementScene.Pooled` union fold, and the `MeshoptLane` arity column carry it with no arm here, and giving it its own filter-coded meshopt stream is one `MeshoptLane` row; a new assimp export target is one scene-exchange row whose KEY is the `ExportToBlob` `exportFormatId` (`IsExportFormatSupported`-guarded).
- Boundary: dispatch is the generated exhaustive `Switch` — every codec row declares its export route or its route-naming fault (`ExportIfc` for GeometryGym, `Semantics/geospatial#VECTOR_INGEST` `GeoVector.Write` for geospatial, the companion bridge for native/IGES), so no row falls into a stale miss tail. GLB emission is deterministic byte layout — `SceneBuilderSchema2Settings` strided/merged buffers, the `GpuMeshInstancingMinCount` threshold, and `MergeBuffers` before write — so the same geometry always emits the same bytes the Compute content-key addresses. Every per-vertex attribute reaches this rail through the seam carrier's ONE kernel `EncodedGeometry` arena and is read by DESCRIPTOR through the page's single `Lane`/`Required` pair, each lane lifted to floats by its own `ChannelDtype` — so a hand-derived stride, a per-channel column read, and a second lane arena on this rail are all deleted forms, and a widened channel arity reaches every arm as data. Absence is a MISSING DESCRIPTOR, never an empty buffer: the parameterization axis the mesh-builder layout, the Draco attribute set, the meshopt stream roster, and the LOD weight vector each discriminate on is the seam's own evidence, so a length probe — which a zero-filled forged unwrap passes — is unspellable here. `ElementScene.Pooled` re-mints the pooled arena through the kernel `Encode.Of` over the UNION of its entries' declared channels (an entry lacking a lane leaves its range at that channel's zero, mirroring the import rail's own pool builder), so the flatten rides `Fin` and the arena's arity screen and per-lane round-trip witness gate the pooled result rather than a hand-assembled carrier reaching a codec. `Author` is the ONE `GlobalId`→`Node` index minter (nodes named by the seam `Object.ExternalId`, read back from `ModelRoot.LogicalNodes`), the ONE feature-row stamp minter (`GlbScene.Rows` ordinals stamped `_FEATURE_ID_0` at `Staged` — the only point the vertex layout is open), AND the ONE material minter (`ElementInstance.Finish` authors its own pooled `MaterialBuilder` — the seam `AppearanceSummary` as linear factors and every bound map through its own `ChannelImage.Bind`, materials pooled per distinct finish KEY so a textured element never inherits its untextured neighbour's material, uniform-finish repeats keeping theirs — the GLB arm erasing the color the dotbim arm round-trips was the deleted asymmetry); a caller-walked scene graph, a second index mint, a post-hoc attribute write, or an image ENCODE on this rail is the deleted form — the texture bytes arrive already sealed by their owner and this rail binds them. `AppearanceSummary` sources every glTF FACTOR and holds each channel in its declared domain: base colour is scene-linear and enters `baseColorFactor` unencoded (routing the display-referred dotbim byte tint into that linear slot is the same unlinearized pass-through `Semantics/appearance#APPEARANCE_PROJECTION` names as deleted on the ingest side), metalness and roughness are written on EVERY material because the glTF factor defaults are both 1.0 and an unwritten material renders as rough metal, opacity below unity selects `AlphaMode.BLEND`, and the `Transmissive` bit writes `KHR_materials_transmission` and NEVER alpha mode — so an opaque-alpha glass round-trips its transmission exactly as the IFC `IfcSurfaceStyleRefraction` egress does. Sidedness is the one RENDER-REPRESENTATION toggle beside those factors and it rides `MaterialFinish.DoubleSided` rather than a summary column, because it selects which faces the material paints where every summary channel answers how a painted face reflects — its producer is the `Semantics/appearance#APPEARANCE_PROJECTION` `StyledAppearance.DoubleSided` bit the source's own `IfcSurfaceSide` declares, it frames into the pool `Key` so a two-sided and a one-sided element never share a `MaterialBuilder`, and `WithDoubleSide` writes on every material because glTF's default is FALSE and an unwritten thin panel culls from inside the model. `GltfChannel` alone owns the canonical-channel-to-`KnownChannel` correspondence: a call site choosing a `KnownChannel` is the unowned projection the roster deletes, a canonical name with no row REFUSES at `ChannelImage.Of` rather than lighting a nearest slot, and the `orm` pack binds ONE `ImageBuilder` onto both the occlusion and metallic-roughness channels because glTF reads one image through two references. `KhrExtension` rows roster only extensions a finish fills, deleting the phantom row — the roster is the caller's declared write capability, the payload's `Obliges` is the truth, and the two union at registration. `EXT_mesh_gpu_instancing` collapse is a POLICY threshold because a gpu-merged node loses its per-node visibility/metadata identity (the 4D/metadata pipeline runs `GpuInstancingMinCount: 0`, the streaming-tile pipeline raises it — a policy value, never a code fork). `KhrExtension` in-box rows serialize through SharpGLTF's own schema types with no registration call (the process-global `ExtensionsFactory` carries the in-box KHR/EXT set; the per-row `Registrar` closure exists ONLY for a caller-supplied custom extension, and every in-box row carries `None` there); registration sweeps the UNION of the `InterchangePolicy` roster and the payload's own `ElementScene.Obliges` rows NARROWED through `KhrExtension.Writables`, because a bound KTX2 or transform-bearing map obliges its extension whether or not the caller listed it while a read-only vocabulary row must never register as write capability, and the four `format#FORMAT_AXIS` texture rows realize exactly here — `KHR_texture_transform` through `TextureBuilder.WithTransform`, `KHR_texture_basisu`/`EXT_texture_webp`/`MSFT_texture_dds` through the container the sealed bytes already carry (`TextureBuilder.PrimaryImage` reads PNG, JPG, DDS, WEBP, and KTX2), so no texture row is a capability flag with no realizing arm. Each of those three container rows binds its `ChannelImage.Fallback` bytes through `TextureBuilder.WithFallbackImage` — the PNG-or-JPG-only degradation SharpGLTF guards — so a viewer that never negotiated the extension resolves a core texture rather than an unresolvable reference, and a container-obliging map bound with no fallback is an extension the consumer must have; a fallback beside a core PNG/JPG primary is the deleted second copy of the same bytes. Every binding writes the sampler's min/mag pair rather than leaving SharpGLTF's unset `DEFAULT`, because an unset minification filter hands mip selection to the consumer and a KTX2 pyramid the press paid to build may never be sampled — trilinear admission defaults state the law, and a non-interpolating data plane states `NEAREST` at its own admission. `KHR_draco_mesh_compression` and `KHR_meshopt_compression` carry a `KhrEncoder` discriminant rather than a SharpGLTF schema type because SharpGLTF ships no compression encoder — `Openize.Drako` owns the Draco encode and `Alimer.Bindings.MeshOptimizer` the meshopt encode, both quantizing to the `InterchangePolicy` bit budget, a glTF `ModelRoot` passed to either the rejected form because neither package owns a glTF model type. `.bim` and USD arms cross a temp path because their `Save`/`Export` are path-bound (no stream overload) — the temp file is deleted in the same expression and never escapes the capsule. IFC egress is NOT this rail's — `ExportIfc` delegates to `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, and a hand-rolled `IfcBuildingElementProxy` re-author (the deleted `IfcBytes` form) is a SECOND IFC-egress owner the seam forbids; the `GlobalId` round-trips 1:1 from `Object.ExternalId` inside `Emit` (never a fresh GUID per export), making export idempotent under the Compute content-key. `ExportIfc` retains only the `CanExport` gate and the `Serialization` column read (a `None` column IS the non-IFC-row fault, the deleted `SerializationOf` ladder now row data), the column's `IfcWireForm` value carrying the container so a `.ifczip` artifact differs from a `.ifc` one by its row alone — a rail-side `ZipArchive` over emitted text, or a `Encoding.UTF8.GetBytes` hop over a returned string, is the deleted form. Chunked-field and structural-delta codecs stay at `Rasm.Compute/Runtime/codecs` consumed at the seam.

```csharp signature
public sealed record InterchangePolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    ReleaseVersion IfcSchema,
    StepProtocol StepProtocol,
    bool MergeBuffers,
    bool StridedBuffers,
    ValidationMode Validation,
    KhrEncoder Compression,
    int QuantizationBits,
    Seq<(string Channel, float Weight)> AttributeWeights,
    bool LockBorder,
    int GpuInstancingMinCount,
    Seq<double> LodRatios,
    Seq<KhrExtension> Extensions) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.Ifc4X3Add2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
        Validation: ValidationMode.Strict,
        Compression: KhrEncoder.None, QuantizationBits: 14,
        // Attribute weights are the simplifier's per-lane error budget in the SAME units the attribute carries. A UV
        // weight near unity makes a texel-space seam as expensive to cross as a world-space one, so the
        // collapse stops at the discontinuity instead of migrating vertices across it; normals ride an order
        // lower because a shaded-normal deviation is recoverable and a smeared map is not. A channel with no row
        // is unweighted, so a source carrying no unwrap costs nothing.
        AttributeWeights: Seq(("geometry_uv", 1.0f), ("geometry_normal", 0.1f)),
        LockBorder: true,
        GpuInstancingMinCount: 0,
        LodRatios: Seq(0.5, 0.25, 0.1, 0.05), Extensions: Seq<KhrExtension>());
    public static readonly InterchangePolicy Web = Canonical with {
        Compression = KhrEncoder.Meshopt, QuantizationBits = 12, GpuInstancingMinCount = 16,
        Extensions = Seq(KhrExtension.MaterialsSpecular, KhrExtension.TextureBasisu, KhrExtension.TextureTransform),
    };
    // Each policy roster declares a caller's write capability while the payload's own Obliges carries the truth; they
    // union at registration, so every rostered row must be a row this rail can FILL. Each Pbr row names a
    // GltfChannel the finish binds a map through or a factor Author writes. KHR_materials_volume, _dispersion,
    // _ior, _emissive_strength and KHR_lights_punctual carry no row.
    //
    // VOLUME is settled at the PLANE altitude, not the factor one, because the corpus roster does carry a
    // volume-shaped channel. KHR_materials_volume exposes exactly one texture, thicknessTexture, whose quantity is the
    // DISTANCE THROUGH the shell — a per-geometry measure no channel in the closed roster carries, so
    // KnownChannel.VolumeThickness has no canonical name to bind from. The one volume channel the roster does carry,
    // `subsurface_radius`, is a three-band mean free path landing on attenuationColor and attenuationDistance, and the
    // extension defines both as FACTORS with no texture at all — so a `subsurface_radius` GltfChannel row would
    // be a row that can never bind a map, and the seam summary carries no radius column for Author to write either.
    // Neither end of the extension has a filler, which is why it is absent rather than declared-and-dark. Dispersion,
    // IOR and emissive strength are the plain factor cases — the seam summary drops the refraction magnitude and the
    // dispersion factor, and the finish carries no luminance column: a bound emission map writes its unit RGB factor
    // (core glTF, no extension) while SharpGLTF's own Emissive channel seeds EmissiveStrength at 1, so no
    // KHR_materials_emissive_strength block ever serializes and the row is struck from Pbr — this rail authors no
    // light. Declaring a capability nothing can exercise is a row that governs nothing; each returns the moment a
    // finish column or a scene arm carries its value — a luminance column on MaterialFinish arms emissive strength,
    // and a `volume_thickness` channel at the frozen roster is the whole arming condition for the volume row.

    public static readonly InterchangePolicy Pbr = Canonical with {
        Extensions = Seq(KhrExtension.MaterialsClearcoat, KhrExtension.MaterialsTransmission, KhrExtension.MaterialsSheen,
            KhrExtension.MaterialsIridescence, KhrExtension.MaterialsAnisotropy, KhrExtension.MaterialsSpecular,
            KhrExtension.MaterialsDiffuseTransmission),
    };
}

public sealed record ExportArtifact(
    InterchangeFormat Format,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At);

// GltfChannel rosters the CANONICAL-channel to glTF binding. The corpus texture vocabulary is closed snake_case
// (Rasm.Materials/Raster/set#TEXTURE_SET declares it, Semantics/appearance#APPEARANCE_PROJECTION TextureMode
// resolves an IFC mode token onto it) and glTF covers a SUBSET of that space, so this roster is the ONE
// projection between them: a KnownChannel chosen at a call site is the unowned correspondence it deletes,
// because two composing edges reading one canonical key onto two glTF channels light different slots and
// nothing raises. Targets is a LIST because the `orm` pack is ONE image glTF reads through TWO channel
// references — occlusion from R, roughness and metalness from G and B — so the pack binds a single
// ImageBuilder onto both channels and a per-channel copy of identical bytes is the deleted form; `mra` has no
// row because the pack roster names `orm` as the only order a glTF consumer reads. Extension names the
// KhrExtension the channel obliges beyond core glTF, so binding a coat or sheen map registers its own
// extension and a material row can never be a capability flag with no realizing arm.
//
// Canonical channels glTF cannot express carry NO row, so ChannelImage.Of returns None at admission —
// `geometry_opacity` rides base colour's alpha rather than a channel of its own, `height`/`curvature` are
// authoring fields no glTF sampler reads, the tangent frames are vertex attributes, and `subsurface_radius` lands
// on a KHR_materials_volume attenuation pair the extension defines as factors carrying no texture — so the
// composing edge sees the refusal and records the unbound map instead of lighting a guessed slot.
//
// Units carries the FACTOR WRITES a binding performs, because a bound map MULTIPLIES its channel factor and the
// KHR extension factors default to ZERO (SharpGLTF's own MaterialValue seeding, decompile-verified: ClearCoatFactor
// 0, ClearCoatRoughness RoughnessFactor 0, TransmissionFactor 0, SheenColor RGB zero, SheenRoughness
// RoughnessFactor 0, IridescenceFactor 0, AnisotropyStrength 0, DiffuseTransmission factor, Emissive RGB zero) — a
// bound emission map on a zero factor renders black and a bound coat map has zero effect, silently. Each row spells the
// (target, property, unit value) triples its binding writes as DATA, so Bind folds the column and a new
// zero-default extension is one tuple on its row, never a finish arm.
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
    // KHR iridescence reads its thickness TEXTURE as a [0,1] factor lerping iridescenceThicknessMinimum..Maximum
    // (both nanometres, SharpGLTF defaults 100/400) — a THIRD thickness convention beside the frozen nm plane and the
    // .mtlx micrometre input. The producer normalizes the nm plane against the set's declared span before
    // sealing, and the binder writes Minimum=0 / Maximum=that span off the ChannelImage row's own ThicknessSpanNm
    // column; a raw nm plane bound as the factor is wrong by ~2.5 orders.
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

    // From reads the generated keyed lookup under the row comparer's own ordinal key: an unmatched canonical
    // name is None, never a nearest-channel guess, because a map bound to the wrong slot still renders and no
    // consumer can tell it from a correct one.
    public static Option<GltfChannel> From(string channel) =>
        TryGet(channel, out GltfChannel? row) && row is not null ? Some(row) : Option<GltfChannel>.None;
}

// One BOUND texture map on a glTF material: the ALREADY-ENCODED image bytes (SharpGLTF's TextureBuilder reads
// PNG/JPG/DDS/WEBP/KTX2 containers), the optional core-container FALLBACK bytes, the resolved GltfChannel row, the UV
// set the primitive samples it through, the wrap pair, the min/mag filter pair, and the optional
// KHR_texture_transform frame the Semantics/appearance#APPEARANCE_PROJECTION UvTransform
// owns. This rail ENCODES nothing: a texture-set owner seals the bytes and the composing edge hands them here, so the
// GLB author stays a binder and no image codec enters Rasm.Bim. Container is SNIFFED off the payload's own magic
// bytes (KTX2 -> TextureBasisu, WebP's RIFF -> TextureWebp, DDS -> TextureDds, PNG/JPG -> None) — a caller knob
// here is the same unowned call-site correspondence the KnownChannel law above deletes, because a WebP payload
// registered as basisu lights the wrong extension and nothing raises — and a transform-bearing row obliges
// TextureTransform; the union of those rows with the channel's own registers before the write, so no
// format#FORMAT_AXIS row serializes unregistered. A KTX2 payload arrives already wire-legal: the frozen
// wire-legality law refuses rawBcn and astc at the PRODUCING wire (TextureSetWire.Of guards every channel's
// payload class before the document exists), so the bytes this binder receives carry a Basis-transcodable or
// uncompressed payload by construction and the sniff here reads container magic alone — for extension
// registration, never a second legality gate re-deciding what the producer already proved.
public sealed record ChannelImage {
    private ChannelImage(
        GltfChannel channel, ReadOnlyMemory<byte> bytes, Option<ReadOnlyMemory<byte>> fallback, string name, int coordinateSet,
        TextureWrapMode wrapS, TextureWrapMode wrapT, TextureMipMapFilter minFilter, TextureInterpolationFilter magFilter,
        Option<UvTransform> transform, Option<KhrExtension> container, Option<double> thicknessSpanNm) =>
        (Channel, Bytes, Fallback, Name, CoordinateSet, WrapS, WrapT, MinFilter, MagFilter, Transform, Container, ThicknessSpanNm) =
        (channel, bytes, fallback, name, coordinateSet, wrapS, wrapT, minFilter, magFilter, transform, container, thicknessSpanNm);

    public GltfChannel Channel { get; }
    public ReadOnlyMemory<byte> Bytes { get; }
    // Fallback bytes realize the core-container degradation the format#FORMAT_AXIS texture rows promise:
    // SharpGLTF's FallbackImage admits PNG and JPG ALONE (its own guard rejects anything else), so a KTX2/WebP/DDS
    // primary pairs with core-format bytes and a viewer lacking KHR_texture_basisu / EXT_texture_webp /
    // MSFT_texture_dds resolves a texture instead of a dangling extension block. Binding reads this only where
    // Container is Some — a core primary needs no second copy of itself, while a KTX2-only binding is a hard
    // dependency on an extension whose absence renders untextured with no diagnostic.
    public Option<ReadOnlyMemory<byte>> Fallback { get; }
    public string Name { get; }
    // CoordinateSet names the UV set this map samples through, and its truth is the decode's own evidence:
    // tessellation#EXPLICIT_TESSELLATION lands the bound texture identity beside the coordinate lane on
    // ExplicitTessellation.Textures, so the composing edge resolves the set from THAT correspondence rather than
    // defaulting every binding to 0 — a model carrying two parameterizations then samples both maps through the
    // first one's coordinates and renders plausibly wrong.
    public int CoordinateSet { get; }
    public TextureWrapMode WrapS { get; }
    public TextureWrapMode WrapT { get; }
    // Minification and magnification ride these two columns. glTF leaves both sampler slots OPTIONAL and SharpGLTF
    // defaults each to its unset DEFAULT member, handing mip selection to the consumer — so a KTX2 whose press paid
    // for a pyramid can meet a runtime that never descends it. Admission defaults state trilinear
    // (LINEAR_MIPMAP_LINEAR minification, LINEAR magnification) so sampling is declared law, while a data map that
    // must not interpolate — an id or mask plane — states NEAREST at its own admission.
    public TextureMipMapFilter MinFilter { get; }
    public TextureInterpolationFilter MagFilter { get; }
    public Option<UvTransform> Transform { get; }
    public Option<KhrExtension> Container { get; }
    // ThicknessSpanNm carries the nm span the producer normalized the thin_film_thickness plane against — the wire's
    // own heightScale-class evidence, threaded from the TextureSetWire the composing edge holds. Read by the FilmThick
    // binding alone; inert on every other row, because only the iridescence-thickness lerp carries a min/max pair to fill.
    public Option<double> ThicknessSpanNm { get; }

    // Admission takes the CANONICAL channel name and refuses at the boundary, so a map naming a channel glTF
    // has no slot for never reaches a MaterialFinish and the composing edge accounts for it once, here.
    public static Option<ChannelImage> Of(
        string channel, ReadOnlyMemory<byte> bytes, string name, int coordinateSet = 0,
        TextureWrapMode wrapS = TextureWrapMode.REPEAT, TextureWrapMode wrapT = TextureWrapMode.REPEAT,
        TextureMipMapFilter minFilter = TextureMipMapFilter.LINEAR_MIPMAP_LINEAR,
        TextureInterpolationFilter magFilter = TextureInterpolationFilter.LINEAR,
        Option<ReadOnlyMemory<byte>> fallback = default,
        Option<UvTransform> transform = default, Option<double> thicknessSpanNm = default) =>
        GltfChannel.From(channel).Map(row => new ChannelImage(
            row, bytes, fallback, name, coordinateSet, wrapS, wrapT, minFilter, magFilter, transform, Sniffed(bytes.Span), thicknessSpanNm));

    // Sniffed spells the magic-byte correspondence once: KTX2 identifier, RIFF+WEBP, DDS fourcc; anything else is a
    // core PNG/JPG container obliging no extension row.
    static Option<KhrExtension> Sniffed(ReadOnlySpan<byte> payload) =>
        payload.Length >= 12 && payload[..12].SequenceEqual((ReadOnlySpan<byte>)[0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A])
            ? Some(KhrExtension.TextureBasisu)
            : payload.Length >= 12 && payload[..4].SequenceEqual("RIFF"u8) && payload[8..12].SequenceEqual("WEBP"u8)
                ? Some(KhrExtension.TextureWebp)
                : payload.Length >= 4 && payload[..4].SequenceEqual("DDS "u8)
                    ? Some(KhrExtension.TextureDds)
                    : Option<KhrExtension>.None;

    // Binding walks the channels: UseChannel opens (or creates) each target channel, UseTexture opens its
    // TextureBuilder, and the fluent sampler/transform members write the glTF sampler and the
    // KHR_texture_transform block. ONE ImageBuilder threads every target so a two-channel pack references one
    // image rather than two copies of identical bytes. A channel value rides the typed
    // WithChannelParam(KnownChannel, KnownProperty, object) or its per-channel With* sibling; the
    // (KnownChannel, Vector4) overload is the rejected spelling.
    public MaterialBuilder Bind(MaterialBuilder material) {
        ImageBuilder image = ImageBuilder.From(Wrapped(Bytes), Name);
        // Fallback bytes mint ONE ImageBuilder beside the primary and thread every target the same way, so a
        // two-channel pack references one fallback rather than two copies. Binding rides exactly when the primary's
        // container obliges an extension: a core PNG/JPG primary already resolves everywhere. WithFallbackImage
        // guards its own PNG/JPG contract and RAISES on anything else, and that raise funnels through the Author /
        // GlbBytes Try.lift onto the typed rail — a non-core fallback is a caller defect, never a silent bind.
        Option<ImageBuilder> core = Container.IsSome
            ? Fallback.Map(bytes => ImageBuilder.From(Wrapped(bytes), $"{Name}-fallback"))
            : Option<ImageBuilder>.None;
        // Iter, not Fold: the builder MUTATES and the accumulator never changes, so a fold spelling here reads
        // as construction while performing iteration — the honest verb is the iteration.
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
        // Unit-factor writes ride the row: a bound map multiplies its channel factor, and the KHR factors default to
        // ZERO, so the map is a no-op until its factor states unity — folded off the roster column, never an arm.
        Channel.Units.Iter(unit => material.WithChannelParam(unit.Target, unit.Property, unit.Value));
        // Iridescence-thickness lerp band: Minimum 0 / Maximum the producer's own normalization span, so the
        // [0,1] plane decodes to the same nanometres the producer encoded — SharpGLTF's 100/400 defaults decode a
        // normalized plane into a band the bytes never carried.
        ThicknessSpanNm.IfSome(span => material
            .WithChannelParam(KnownChannel.IridescenceThickness, KnownProperty.Minimum, 0f)
            .WithChannelParam(KnownChannel.IridescenceThickness, KnownProperty.Maximum, (float)span));
        return material;
    }

    // Obliges names the extension rows this binding carries: its channel's own row, its container row (when the
    // bytes are not a core PNG/JPG), and the transform row (when a UV frame rides), so RegisterExtensions sweeps the
    // UNION over every bound image rather than trusting an InterchangePolicy roster a caller may have under-declared.
    public Seq<KhrExtension> Obliges =>
        Channel.Extension.ToSeq()
        + Container.ToSeq()
        + (Transform.IsSome ? Seq(KhrExtension.TextureTransform) : Seq<KhrExtension>());

    // SharpGLTF's MemoryImage wraps an ArraySegment<byte> with no copy and ImageBuilder.From takes it through the
    // implicit segment conversion, so an array-backed ReadOnlyMemory projects straight through and only a
    // non-array-backed carrier (a native or pooled-owner buffer) pays the copy.
    static ArraySegment<byte> Wrapped(ReadOnlyMemory<byte> bytes) =>
        MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> owned) ? owned : new ArraySegment<byte>(bytes.ToArray());
}

// MaterialFinish pools MATERIAL identity as the SEAM neutral PBR summary beside the ordered channel bindings. That
// summary sources every factor — base colour, metalness, roughness, opacity, and the refractive bit — because the
// seam already froze it as the one appearance vector every producer mints (Rasm.Element/Graph/element#NODE_MODEL)
// and re-packing it into a display-referred byte tint drops four of its six values on the floor. Two instances
// sharing a summary but differing in one bound map are two materials, so the pool keys on the whole finish; an
// rgba-only key silently gave a textured element its untextured neighbour's material. Finishes carrying NO
// summary are the untinted default the flat-soup and mixed-repeat paths take.
// DoubleSided is the RENDER-REPRESENTATION toggle beside the reflectance vector, never a summary column: every
// AppearanceSummary channel answers how a painted face reflects, this one answers WHICH faces the material paints,
// and widening the frozen seven-value preimage to carry it re-keys every stored Node.Appearance. Its producer is the
// Semantics/appearance#APPEARANCE_PROJECTION StyledAppearance.DoubleSided bit the IFC IfcSurfaceSide attribute
// declares, so the exported glTF states the sidedness the source file stated rather than the format default —
// glTF doubleSided defaults FALSE, so a two-sided IFC style left unwritten culls every interior face and thin
// two-sided elements vanish when viewed from inside the model.
public sealed record MaterialFinish(Option<AppearanceSummary> Surface, Seq<ChannelImage> Images, bool DoubleSided) {
    public static readonly MaterialFinish White = new(Option<AppearanceSummary>.None, Seq<ChannelImage>(), DoubleSided: false);

    // doubleSided arrives REQUIRED beside the summary: its holder is the composition edge reading a
    // Semantics/appearance#APPEARANCE_PROJECTION StyledAppearance, which carries both, and a defaulted slot lets
    // whichever caller omits it assert single-sided over a source that declared otherwise.
    public static MaterialFinish Of(AppearanceSummary surface, bool doubleSided) =>
        new(Some(surface), Seq<ChannelImage>(), doubleSided);

    // dotbim's Color column is display-referred 0-255 RGBA, so the scene-linear summary projects through the ONE
    // package byte egress — Semantics/appearance#APPEARANCE_PROJECTION Bytes, itself the kernel federation
    // quantizer — so a dotbim byte, an IFC palette byte, and a content-key byte are one value and no second
    // rounding law forks the round-trip the IFC egress proves. The summary's channels are seam-admitted, so the
    // rail's refusal arm is structurally unreachable here and the Fin rides the caller's traverse untouched;
    // ALPHA crosses inside the kernel ingress linear by definition.
    public Fin<uint> Rgba(Op key) =>
        Surface.Match(
            Some: s => AppearanceProjection.Bytes(s.BaseColorR, s.BaseColorG, s.BaseColorB, s.Opacity, key)
                .Map(static b => (uint)b.Red << 24 | (uint)b.Green << 16 | (uint)b.Blue << 8 | b.Alpha),
            None: static () => Fin.Succ(0xFFFFFFFFu));

    // Key spans the WHOLE material identity: the seam AppearanceKey — itself the content hash over the entire
    // neutral vector, so no local canonicalization of six scalars exists here to fork — beside the sidedness bit
    // and every axis each binding writes onto its glTF texture, length- and presence-framed under the kernel
    // seed-zero hasher, so the key is stable across runs and two byte-identical finishes pool once. Sidedness
    // frames because it is a MaterialBuilder axis the pool must discriminate: two elements sharing a summary and
    // a map roster but differing in sidedness are two glTF materials, and a key blind to the bit hands whichever
    // element the pool saw second the first one's culling — the identical failure the pre-sampler rgba-only key
    // produced for wrap and filter.
    public UInt128 Key =>
        ContentHash.Of(Images
            .Fold(
                new CanonicalWriter(0.0)
                    .U128(Surface.Match(Some: static s => s.AppearanceKey, None: static () => UInt128.Zero))
                    .Ordinal(DoubleSided ? 1 : 0)
                    .Ordinal(Images.Count),
                static (writer, image) => Sampled(writer, image))
            .ToBytes().Span);

    // Every axis a binding writes IS material identity — the primary bytes, the fallback bytes, the UV set, the wrap
    // pair, the min/mag filter pair, the transform frame, and the iridescence span. A key over the primary bytes
    // alone pooled two distinct SAMPLERS onto one MaterialBuilder, so a CLAMP map inherited its REPEAT neighbour's
    // wrap and a trilinear map its nearest neighbour's filter with nothing raising. Each optional axis is
    // PRESENCE-framed by its own value COUNT ahead of its values, so an absent frame and a zero-valued one never
    // collide and the fold stays one expression over the roster rather than an arm per axis.
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

    // Every row this finish obliges: each binding's own union plus the transmission row a refractive summary
    // writes with no map behind it, so a glass element registers KHR_materials_transmission whether or not a
    // transmission map rides.
    public Seq<KhrExtension> Obliges =>
        (Images.Bind(static image => image.Obliges)
         + (Surface.Map(static s => s.Transmissive).IfNone(false) ? Seq(KhrExtension.MaterialsTransmission) : Seq<KhrExtension>()))
        .Distinct();

    // Author mints the ONE material. The summary's SCENE-LINEAR base colour and its opacity go straight into
    // baseColorFactor, which glTF defines as linear — feeding it the display-referred byte tint writes an sRGB
    // number into a linear slot and every exported element renders washed out, the same unlinearized
    // pass-through Semantics/appearance#APPEARANCE_PROJECTION names as the deleted form on the ingest side.
    // Metalness and roughness are written ALWAYS, because the glTF factor defaults are 1.0 and 1.0: a material
    // left unwritten renders as a rough METAL, so a summary-less finish states dielectric 0.0 explicitly rather
    // than inheriting the format default. Sub-unit opacity selects BLEND while transmission stays the
    // REFRACTIVE bit and never touches alpha mode — an opaque-alpha glass keeps OPAQUE and transmits, which is
    // exactly the Opacity-vs-Transmissive split the seam summary carries and the IFC refraction egress proves.
    // IOR stays deliberately unwritten: the thin summary drops the magnitude and SharpGLTF's own
    // IndexOfRefraction default is the 1.5 crown-glass neutral, so writing it serializes a
    // KHR_materials_ior block asserting the default.
    public MaterialBuilder Author() => Images.Fold(Shaded(), static (material, image) => image.Bind(material));

    // WithDoubleSide writes on EVERY material for the same reason metalness and roughness do: the glTF default is
    // a value (FALSE), not an absence, so leaving it unwritten asserts single-sided on every element rather than
    // deferring the choice — and the one class of element that most needs the bit (a zero-thickness curtain-wall
    // panel, a railing infill, a partition surface) is exactly the class a single-sided default renders invisible
    // from one side with nothing raising.
    MaterialBuilder Shaded() {
        MaterialBuilder material = new MaterialBuilder($"finish-{Key:x32}")
            .WithMetallicRoughnessShader()
            .WithDoubleSide(DoubleSided);
        return Surface.Match(
            Some: s => Refracted(
                material
                    .WithBaseColor(new Vector4((float)s.BaseColorR, (float)s.BaseColorG, (float)s.BaseColorB, (float)s.Opacity))
                    .WithMetallicRoughness((float)s.Metallic, (float)s.Roughness)
                    .WithAlpha(s.Opacity < 1.0 ? AlphaMode.BLEND : AlphaMode.OPAQUE),
                s),
            None: () => material.WithBaseColor(Vector4.One).WithMetallicRoughness(0f, 1f));
    }

    // Transmission writes only when the summary carries the refractive bit: a zero TransmissionFactor still
    // mints the channel and serializes KHR_materials_transmission, so an opaque element would ship an extension
    // block asserting no transmission. The bit is boolean at the seam, so full transmission is the one honest
    // magnitude and a measured factor arrives as a column on this finish the day the summary carries one.
    static MaterialBuilder Refracted(MaterialBuilder material, AppearanceSummary surface) =>
        surface.Transmissive
            ? material.WithChannelParam(KnownChannel.Transmission, KnownProperty.TransmissionFactor, 1f)
            : material;

}

// One placed element: the seam Object.ExternalId GlobalId, the baked name, the "ifc" Classification code, a
// content key selecting its pool mesh, the rigid placement, and the MaterialFinish carrying the seam appearance
// summary the GLB material author writes as linear factors and the dotbim Color column reads as its display
// 0xRRGGBBAA projection. N repeats of one geometry are N rows over ONE pool entry — instancing the flat soup
// erases — and the finish rides the ROW rather than the pool entry because two placements of one mesh may carry
// distinct finishes.
public sealed record ElementInstance(string GlobalId, string Name, string Class, UInt128 MeshKey, Matrix4x4 Placement, MaterialFinish Finish);

// Per-element carrier: a content-keyed mesh pool (each entry ONE baked single-block ImportedGeometry — a
// canonical soup for one distinct geometry) with the placement rows. Pooled re-describes the scene as the
// import carrier's Blocks/Instances overlay over ONE re-minted kernel arena; Soup flattens that through the
// seam's ONE Bake fold for the arms that carry no scene graph (Draco/meshopt streams, single-mesh scene rows)
// — a second transform loop beside ImportedGeometry.Bake is the deleted re-derivation. Both ride Fin because
// Encode.Of screens every re-minted lane's arity and witnesses its round-trip error.
public sealed record ElementScene {
    // ADMISSION is the factory, so the record's own ctor is private: every instance in a landed scene resolves a
    // pool entry BY CONSTRUCTION. The retired public ctor let a caller hand a placement naming a key the pool never
    // held, and the flatten then threw an unkeyed KeyNotFoundException out of a Fin-typed member — the same
    // dictionary indexer twice, once for the ordinal and once for the mesh.
    private ElementScene(Map<UInt128, ImportedGeometry> pool, Seq<ElementInstance> instances) =>
        (Pool, Instances) = (pool, instances);

    public Map<UInt128, ImportedGeometry> Pool { get; }
    public Seq<ElementInstance> Instances { get; }

    // Railed admission: an empty pool and a dangling MeshKey are the two ways a scene cannot flatten, and each
    // names its own row — the dangling arm carrying the element that referenced the absent key, so a caller reads
    // WHICH placement is unresolvable rather than that some index missed.
    public static Fin<ElementScene> Of(Map<UInt128, ImportedGeometry> pool, Seq<ElementInstance> instances, Op key) =>
        pool.IsEmpty
            ? Fin.Fail<ElementScene>(Detail.ElementSceneEmpty.At(key))
            : instances.Find(instance => !pool.ContainsKey(instance.MeshKey)).Match(
                Some: miss => Fin.Fail<ElementScene>(Detail.ElementSceneMeshMiss.At(
                    key, miss.GlobalId, miss.MeshKey.ToString("x32", CultureInfo.InvariantCulture))),
                None: () => Fin.Succ(new ElementScene(pool, instances)));

    // Single-element degrade for a flat soup reaching a per-element wire: one pool entry keyed by the whole
    // arena PAYLOAD — every declared lane, so two meshes agreeing on position and differing in unwrap or vertex
    // colour never collapse onto one entry — one identity placement, the untyped-proxy classification. TOTAL by
    // construction: the one instance names the one key this expression just minted.
    public static ElementScene Of(ImportedGeometry soup) {
        UInt128 key = ContentHash.Of(soup.Lanes.Payload.Span);
        return new ElementScene(Map((key, soup)),
            Seq(new ElementInstance("soup", "soup", "IfcBuildingElementProxy", key, Matrix4x4.Identity, MaterialFinish.White)));
    }

    // Obliges unions the extension rows this scene's own bound maps demand across every instance finish, deduped; the
    // declared write set reads THIS rather than the InterchangePolicy roster alone, so a caller who bound a
    // KTX2 map without listing KHR_texture_basisu still declares the extension the writer emits.
    public Seq<KhrExtension> Obliges => Instances.Bind(static instance => instance.Finish.Obliges).Distinct();

    public Fin<ImportedGeometry> Soup(Op key) => Pooled(key).Bind(pooled => pooled.Bake(key));

    // Each pool entry lands one MeshBlock, each ElementInstance one MeshInstance over its block ordinal — the
    // pooled ImportedGeometry carries the SAME sharing this scene does, so a consumer needing world-space
    // geometry calls the one seam Bake owner and a consumer preserving instancing reads the overlay. The pooled
    // lane set is the UNION of the entries' declared channels and each BLOCK carries its own declared subset, so
    // an entry that never declared a channel leaves its range untouched and no reader mistakes those ordinates
    // for values — per-vertex lockstep holds across the whole arena while the block's Declared column stays the
    // evidence. A channel NO entry declares is simply absent — a missing descriptor, never a column a consumer
    // length-probes. The fold names no channel, so a new EncodingChannel row reaches the pooled arena with zero
    // edit here. Each block also carries its entry's own Material forward: the retired form dropped it at the
    // flatten, so a per-element scene that round-tripped a multi-material USD mesh lost every shading key the
    // import arm had partitioned on.
    public Fin<ImportedGeometry> Pooled(Op key) {
        var lead = Pool.Values.Head();
        var keys = Pool.Keys.ToSeq();
        var ordinals = keys.Select(static (k, i) => (k, i)).ToMap();
        int vertexTotal = Pool.Values.Sum(static m => m.VertexCount);
        int indexTotal = Pool.Values.Sum(static m => m.Indices.Length);
        // Strict() forces the lane arenas ONCE: a lazily re-evaluated Map hands Encode.Of freshly-zeroed arrays
        // after the fold below filled a different set, so the mint witnesses an empty payload as lossless.
        Seq<(EncodingChannel Channel, float[] Raw)> lanes = toSeq(Pool.Values)
            .Bind(static m => m.Lanes.Descriptors.Map(static d => d.Channel)).Distinct()
            .Map(channel => (channel, new float[vertexTotal * channel.Arity])).Strict();
        long[] indices = new long[indexTotal];
        var blocks = new MeshBlock[keys.Count];
        var (vBase, iBase, slot) = (0, 0, 0);
        foreach (var pooled in keys) {
            var mesh = Pool[pooled];
            // Every entry is ONE baked single-block carrier, so its lead block IS its declared set and its shading
            // key; both ride forward onto the pooled block rather than re-deriving from the arena.
            var entry = mesh.Blocks.Head();
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
        return Encode.Of(vertexTotal, lanes, key).Map(arena => new ImportedGeometry(
            lead.FormatKey, arena, indices, vertexTotal, indexTotal / 3, toSeq(blocks), placed, lead.At));
    }
}

// One emit-modality axis: flat soup or per-element scene — every codec arm is compiler-forced to route both
// cases; a bool/perElement knob beside the payload is the rejected form (MODAL_ARITY).
[Union]
public abstract partial record ExportPayload {
    public sealed record Soup(ImportedGeometry Geometry) : ExportPayload;
    public sealed record Scene(ElementScene Elements) : ExportPayload;

    // Flattening rides Fin because a Scene re-mints its pooled arena and bakes it through the seam's own fold, and
    // Switch's state parameter carries the key rather than a captured closure, so both arms stay static.
    public Fin<ImportedGeometry> Flat(Op key) => Switch(
        state: key,
        soup:  static (_, s) => Fin.Succ(s.Geometry),
        scene: static (k, s) => s.Elements.Soup(k));
}

// Author-minted triple: the glTF model, the GlobalId->Node index, and the GlobalId->feature-row index —
// TileMetadata/AnimateSchedule bind against BOTH; the node index is READ BACK from ModelRoot.LogicalNodes by
// node name so it indexes the model actually emitted, and Rows carries the instance ordinal every uniquely-
// meshed vertex was stamped with at Staged time (the EXT_mesh_features property-table row).
public sealed record GlbScene(
    ModelRoot Model, Map<string, SharpGLTF.Schema2.Node> Nodes, Map<string, int> Rows, Seq<KhrExtension> Extensions);

public static partial class BimExport {
    // TOTAL codec dispatch (import#IMPORT_RAIL parity): a new InterchangeCodec row BREAKS this call site at
    // compile time; each non-emitting arm names its owning route — never a silent ladder tail.
    public static Fin<ExportArtifact> Export(InterchangeFormat format, ExportPayload payload, InterchangePolicy policy, IClock clock, Op key) =>
        InterchangeFormat.Admitted(format, InterchangeDirection.Export, key).Bind(row => row.Codec.Switch(
            sharpGltf:        () => GlbBytes(payload, policy, key).Map(bytes => Sealed(format, bytes, policy, clock.GetCurrentInstant())),
            dotBim:           () => Try.lift(() => Sealed(format, DotBimBytes(payload, key), policy, clock.GetCurrentInstant())).Run().MapFail(error => (Error)Detail.BimExport.At(key, error.Message)),
            sceneExchange:    () => payload.Flat(key).Bind(flat => Try.lift(() => Sealed(format, SceneBytes(format, flat), policy, clock.GetCurrentInstant())).Run().MapFail(error => (Error)Detail.SceneExport.At(key, error.Message))),
            usdStage:         () => payload.Flat(key).Bind(flat => Try.lift(() => Sealed(format, UsdBytes(format, flat), policy, clock.GetCurrentInstant())).Run().MapFail(error => (Error)Detail.UsdExport.At(key, error.Message))),
            geometryGym:      () => Fin.Fail<ExportArtifact>(Detail.IfcExportRoute.At(key, "use-ExportIfc", format.Key)),
            geospatialVector: () => Fin.Fail<ExportArtifact>(Detail.GeoExportRoute.At(key, "GeoVector.Write", format.Key)),
            geospatialRaster: () => Fin.Fail<ExportArtifact>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Export), format.Key)),
            meshText:         () => Fin.Fail<ExportArtifact>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Export), format.Key)),
            ply:              () => Fin.Fail<ExportArtifact>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Export), format.Key)),
            pointCloud:       () => Fin.Fail<ExportArtifact>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Export), format.Key)),
            acadSharp:        () => Fin.Fail<ExportArtifact>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Export), format.Key)),
            stepIso10303:     () => Fin.Fail<ExportArtifact>(Detail.DirectionUnsupported.At(key, nameof(InterchangeDirection.Export), format.Key)),
            nativeCompanion:  () => Fin.Fail<ExportArtifact>(Detail.ExportNeedsHost.At(key, format.Key)),
            igesAnsi:         () => Fin.Fail<ExportArtifact>(Detail.ExportNeedsHost.At(key, format.Key)),
            saf:              () => Fin.Fail<ExportArtifact>(Detail.ExportCataloguePending.At(key, format.Key)),
            cobieXlsx:        () => Fin.Fail<ExportArtifact>(Detail.CobieExportGraphRoute.At(key, "use-CobieEmit", format.Key)),
            ifc5Pending:      () => Fin.Fail<ExportArtifact>(Detail.ExportCataloguePending.At(key, format.Key))));

    // Per-element scene author — the ONE GlobalId->Node index minter. One MeshBuilder per distinct pool
    // key, one GlobalId-named NodeBuilder per instance (LocalMatrix = the rigid placement), repeats sharing
    // ONE logical mesh; the GpuMeshInstancingMinCount threshold collapses node fan-outs into
    // EXT_mesh_gpu_instancing (policy 0 = never — a gpu-merged node loses per-node visibility/metadata
    // identity, so the 4D/metadata pipeline keeps 0 and the streaming-tile pipeline raises it).
    public static Fin<GlbScene> Author(ElementScene scene, InterchangePolicy policy, Op key) =>
        Try.lift(() => Staged(scene, policy)).Run()
            .MapFail(error => (Error)Detail.SceneAuthor.At(key, error.Message));

    // Seals a decorated GlbScene (metadata attached, schedule animated) as the GLB artifact.
    public static Fin<ExportArtifact> Emit(GlbScene scene, InterchangeFormat format, InterchangePolicy policy, IClock clock, Op key) =>
        Try.lift(() => Sealed(format, WriteGlb(scene.Model, policy), policy, clock.GetCurrentInstant())).Run()
            .MapFail(error => (Error)Detail.GltfExport.At(key, error.Message));

    // Lane is the ONE arena read every emit arm on this page composes. The seam carrier holds each per-vertex
    // attribute as a descriptor-addressed slice of ONE kernel payload, so a site NAMES the EncodingChannel it wants
    // and takes back exactly Count × Arity floats lifted through that descriptor's OWN dtype — a float32 position
    // and a unorm8 colour read identically and no arm re-derives a stride or a byte offset. Absence is a MISSING
    // DESCRIPTOR, so the read answers None and the parameterization axis every arm below discriminates on stays
    // seam evidence rather than a length probe over a buffer a zero-fill could forge.
    internal static Option<float[]> Lane(ImportedGeometry geometry, EncodingChannel channel) =>
        geometry.Blocks.ForAll(block => block.Declared.Contains(channel))
            ? Sliced(geometry, channel)
            : Option<float[]>.None;

    // Presence is TWO facts, and every arm on this page needs both: the arena must carry the descriptor, and every
    // BLOCK inside it must have declared the channel. Pooled arenas are dense by construction, so a descriptor
    // alone answers Some for a carrier where only one of N blocks was ever mapped — and a partially-declaring
    // source then encoded, decimated, and bound against ranges that are the arena's zero rather than an authored
    // value. Gating the ONE reader is what carries that law to the mesh-builder layout, the Draco attribute set, the
    // meshopt stream roster, and the LOD weight vector with no per-arm test.
    static Option<float[]> Sliced(ImportedGeometry geometry, EncodingChannel channel) =>
        geometry.Lanes.Descriptors.Find(descriptor => descriptor.Channel == channel).Map(descriptor => {
            float[] raw = new float[descriptor.Floats];
            descriptor.Dtype.Unpack(geometry.Lanes.Channel(channel).Span, raw);
            return raw;
        });

    // Position and Normal are the two lanes EVERY decoded source declares, so an arm needing one takes it through
    // here: a carrier missing either is a malformed decode faulting loud inside that arm's own Try.lift funnel as
    // a typed BimFault, where a silent empty-array default would emit headless geometry a viewer renders as void.
    internal static float[] Required(ImportedGeometry geometry, EncodingChannel channel) =>
        Lane(geometry, channel).Case is float[] raw
            ? raw
            : throw new InvalidDataException($"<carrier-lane-absent:{channel.Key}:{geometry.FormatKey}>");

    // Feature-row stamp AND the whole material FINISH are authored HERE, at MeshBuilder time — the only point the
    // vertex layout and the primitive material are open: a pool mesh referenced by exactly ONE instance stamps
    // that instance's table row on every vertex and takes that instance's finish; a SHARED pool mesh stamps the
    // null row because EXT_mesh_features lives on the (shared) primitive and cannot carry per-node identity, yet
    // KEEPS its finish when every repeat agrees on one key (material is per-mesh, not per-node) — mixed-finish
    // repeats fall to the untinted default, and the GpuInstancingMinCount policy owns the identity trade (merge
    // repeats and re-bind per instance, or keep per-node visibility and accept null rows). The finish key is the
    // WHOLE material identity — the seam AppearanceKey plus every bound map — so a textured element and its
    // untextured neighbour never collapse onto one pooled MaterialBuilder the way an rgba-only key collapsed them.
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
        // Each finish owns its whole material mint — factors from the seam summary, every bound map through its
        // GltfChannel row — so this fold owns POOLING alone and a new factor or channel is a row on the finish,
        // never an arm here. MaterialBuilder equality is REFERENCE by default, so the pool keys on the finish's
        // own content key rather than trusting Equals.
        MaterialBuilder Finished(MaterialFinish finish) =>
            materials.TryGetValue(finish.Key, out MaterialBuilder? held) ? held : materials[finish.Key] = finish.Author();
        var pool = scene.Pool.Map((key, mesh) => {
            var (row, finish) = byMesh.GetValueOrDefault(key, (nullRow, MaterialFinish.White));
            return MeshOf(mesh, Finished(finish), Some(row));
        });
        var builder = new SceneBuilder();
        scene.Instances.Iter(instance => {
            var node = new NodeBuilder(instance.GlobalId) { LocalMatrix = instance.Placement };
            builder.AddRigidMesh(pool[instance.MeshKey], node);
        });
        var model = builder.ToGltf2(new SceneBuilderSchema2Settings {
            UseStridedBuffers = policy.StridedBuffers,
            GpuMeshInstancingMinCount = policy.GpuInstancingMinCount <= 0 ? int.MaxValue : policy.GpuInstancingMinCount,
        });
        return new GlbScene(
            model,
            model.LogicalNodes.AsIterable().Filter(static n => n.Name is { Length: > 0 }).Map(static n => (n.Name, n)).ToMap(),
            rows,
            Registered(policy, new ExportPayload.Scene(scene)));
    }

    // FBX/Collada emit through AssimpNetter — the `scene-exchange` codec; a Scene payload flattens (Assimp
    // per-element node authoring is the admission-gated growth). The row KEY is the exportFormatId
    // (`fbx`/`collada`), guarded against the live export matrix; glTF/GLB export stays on SharpGLTF so the
    // Draco/meshopt encode stacks on that path, not this one.
    static byte[] SceneBytes(InterchangeFormat format, ImportedGeometry geometry) {
        var mesh = new Assimp.Mesh("bim") { MaterialIndex = 0 };
        float[] verts = Required(geometry, EncodingChannel.Position);
        float[] normals = Required(geometry, EncodingChannel.Normal);
        for (int v = 0; v < geometry.VertexCount; v++) {
            int p = v * EncodingChannel.Position.Arity, n = v * EncodingChannel.Normal.Arity;
            mesh.Vertices.Add(new Vector3(verts[p], verts[p + 1], verts[p + 2]));
            mesh.Normals.Add(new Vector3(normals[n], normals[n + 1], normals[n + 2]));
        }
        var indices = geometry.Indices.Span;
        for (int t = 0; t < geometry.TriangleCount; t++) {
            mesh.Faces.Add(new Assimp.Face([(int)indices[t * 3], (int)indices[t * 3 + 1], (int)indices[t * 3 + 2]]));
        }
        var scene = new Assimp.Scene { RootNode = new Assimp.Node("root") };
        scene.Materials.Add(new Assimp.Material { Name = "default" });
        scene.Meshes.Add(mesh);
        scene.RootNode.MeshIndices.Add(0);
        using var context = new AssimpContext();
        return context.IsExportFormatSupported(format.Key)
            ? context.ExportToBlob(scene, format.Key).Data
            : throw new NotSupportedException($"<scene-export-format:{format.Key}>");
    }

    // .bim emit through dotbim — the ONLY wire preserving instancing: distinct geometry pools ONCE as Mesh
    // rows, every ElementInstance a placed Element. File.Save is path-bound (`.bim`-enforced, no stream
    // overload), so the bytes cross a temp path exactly as UsdBytes does. Element.Guid demands RFC-4122 text,
    // and the seam GlobalId is 22-char IFC-compressed — the Guid is minted deterministically from
    // XxHash128(GlobalId) and the verbatim GlobalId rides Info["globalId"], so identity round-trips losslessly
    // and re-export is byte-stable. A non-rigid placement faults loud: the dotbim wire carries no scale.
    static byte[] DotBimBytes(ExportPayload payload, Op key) {
        var scene = payload.Switch(soup: static s => ElementScene.Of(s.Geometry), scene: static s => s.Elements);
        var ordinals = scene.Pool.Keys.Select(static (k, index) => (k, index)).ToMap();
        var meshes = scene.Pool.AsIterable().Map(pair => new dotbim.Mesh {
            MeshId = ordinals[pair.Key],
            Coordinates = [.. Required(pair.Value, EncodingChannel.Position).Select(static v => (double)v)],
            Indices = [.. pair.Value.Indices.ToArray().Select(static i => (int)i)],
        }).ToList();
        var elements = scene.Instances.Map(instance => {
            if (!Matrix4x4.Decompose(instance.Placement, out var scale, out var rotation, out var translation)
                || Math.Abs(scale.X - 1f) > 1e-4f || Math.Abs(scale.Y - 1f) > 1e-4f || Math.Abs(scale.Z - 1f) > 1e-4f) {
                throw new InvalidDataException($"<dotbim-nonrigid-placement:{instance.GlobalId}>");
            }
            // One packed word binds per element off the kernel byte leg and the four lanes unpack it; the
            // rail's refusal arm is seam-discharged (the summary's channels are unit-gated at AppearanceSummary.Of),
            // so the collapse rides this boundary capsule's own throw funnel like every other fault in the body.
            uint rgba = instance.Finish.Rgba(key).ThrowIfFail();
            return new dotbim.Element {
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
            };
        }).ToList();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bim");
        try {
            new dotbim.File { SchemaVersion = "1.1.0", Meshes = meshes, Elements = elements, Info = new Dictionary<string, string>() }.Save(path);
            return File.ReadAllBytes(path);
        } finally { File.Delete(path); }
    }

    // Rfc4122 renders the ONE federation key as the text the dotbim wire demands, never a second digest: the
    // kernel seed-zero ContentHash over the GlobalId bytes IS those 128 bits, so the deterministic Guid and
    // every other content key in the estate share one hasher. A local XxHash128 call here forked the content
    // space this package's own ruling seals — the same defect its Review/diff owner already names deleted.
    static string Rfc4122(string globalId) {
        UInt128 key = ContentHash.Of(Encoding.UTF8.GetBytes(globalId));
        return new Guid(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref key, 1))).ToString();
    }

    // USD emit through UniversalSceneDescription — the `usd-stage` codec. One UsdStage authors a UsdGeomMesh
    // prim (points VtVec3fArray, faceVertexCounts/Indices VtIntArray through the typed-array Set seam), exports
    // to the temp path, and reads the bytes; a Scene payload flattens (per-prim element authoring over
    // UsdGeomXformable.AddXformOp is the admission-gated growth); USD is a scene-graph peer, never re-deriving
    // BIM semantics. A usdz row is import-only: the binding ships no .usdz packaging member.
    static byte[] UsdBytes(InterchangeFormat format, ImportedGeometry geometry) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        try {
            using var stage = UsdStage.CreateNew(path);
            var mesh = UsdGeomMesh.Define(stage, new SdfPath("/Bim"));
            float[] verts = Required(geometry, EncodingChannel.Position);
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

    // IFC egress is the seam's Projection/egress#IFC_EGRESS SemanticProjector.Emit — the ONE Bim-internal
    // ElementGraph->DatabaseIfc re-author (the PredefinedType egress gate + schema span [PREDEFINED_TOKEN_RULING][H8], the 1:1 GlobalId
    // round-trip [H6], the diff-derived OwnerHistory ChangeAction against `prior` [H9], the material/classification/
    // relationship re-author). This rail OWNS only the CanExport capability gate, the format#FORMAT_AXIS
    // InterchangeFormat.Serialization column read (None IS the non-IFC-row rejection — the retired SerializationOf
    // ladder as row data; the column's IfcWireForm value carries the CONTAINER beside the serialization, so the
    // `.ifczip` row seals a zipped STEP through the projector's own writer and no ZipArchive exists on this rail),
    // and the ExportArtifact content-key seal; the hand-rolled IfcBuildingElementProxy re-author
    // is the DELETED form (a lossy second IFC-egress owner). The app wires the projector (the seam owns Assemble,
    // app owning the wiring); the EmitContext carrier rides through whole — the diff-prior, the scoped trade-package
    // slice, and the declared unit regime — and the profile store is the projector's ingress-part capture-promoted
    // field, never a re-passed parameter.
    public static Fin<ExportArtifact> ExportIfc(
        InterchangeFormat format, ElementGraph graph, SemanticProjector projector,
        InterchangePolicy policy, IClock clock, Option<EmitContext> context, Op key) =>
        InterchangeFormat.Admitted(format, InterchangeDirection.Export, key).Bind(row => row.Serialization.Match(
            None: () => Fin.Fail<ExportArtifact>(Detail.IfcExportCodecMiss.At(key, format.Key)),
            Some: form => projector.Emit(graph, form, key, context)
                .Map(bytes => Sealed(row, bytes, policy, clock.GetCurrentInstant()))));

    // RegisterExtensions keeps its own registration rail ahead of the encode. The raw Draco/meshopt streams carry
    // no scene graph, so BOTH payload cases flatten there — one Bake through the seam's own fold, ON the rail
    // because the flatten re-mints the arena and re-witnesses every lane — while the container arm routes Soup
    // through the single-mesh SceneOf and Scene through the per-element Staged author with no flatten at all.
    static Fin<byte[]> GlbBytes(ExportPayload payload, InterchangePolicy policy, Op key) =>
        policy.Compression switch {
            KhrEncoder.Draco => payload.Flat(key).Bind(flat => Encoded(() => DracoBytes(flat, policy), key)),
            KhrEncoder.Meshopt => payload.Flat(key).Bind(flat => Encoded(() => MeshoptBytes(flat, policy), key)),
            KhrEncoder.None => Encoded(() => WriteGlb(payload.Switch(
                     soup:  s => SceneOf(s.Geometry, policy),
                     scene: s => Staged(s.Elements, policy).Model), policy), key),
            var unknown => Fin.Fail<byte[]>(Detail.KhrEncoderUnrouted.At(key, unknown.ToString())),   // a new encoder REFUSES, never a silent uncompressed container
        };

    // ONE native-fault funnel the three encode arms share: Openize.Drako raises DrakoException, SharpGLTF's
    // ToGltf2/WriteGLB raise ModelException, and a malformed carrier raises from Required — each lifting BARE as a
    // typed BimFault.CodecReject, symmetric with the bim/scene/usd arms and never escaping the Fin<T> rail.
    static Fin<byte[]> Encoded(Func<byte[]> encode, Op key) =>
        Try.lift(encode).Run().MapFail(error => (Error)Detail.GltfExport.At(key, error.Message));

    static ModelRoot SceneOf(ImportedGeometry geometry, InterchangePolicy policy) {
        var scene = new SceneBuilder();
        // MaterialFinish.White mints the untinted default, never a bare MaterialBuilder: a builder constructed
        // here inherits glTF's unity metallic and roughness defaults and every flat-soup export renders as rough
        // metal, which is exactly the hole the finish's explicit dielectric write closes.
        scene.AddRigidMesh(MeshOf(geometry, MaterialFinish.White.Author(), Option<int>.None), AffineTransform.Identity);
        return scene.ToGltf2(new SceneBuilderSchema2Settings { UseStridedBuffers = policy.StridedBuffers });
    }

    // Toolkit custom-attribute fragments (api-sharpgltf IVertexCustom/IVertexMaterial, the vertex-fragment seam).
    // MaxTextCoords is a COMPILE-TIME property of the fragment type — the Toolkit reads it to decide whether the
    // primitive writes TEXCOORD_0 — so the mapped and unmapped layouts are two types by construction, never one
    // type with a runtime flag; the FEATURE row is a column on BOTH because the property-table stamp is orthogonal
    // to the parameterization. A single fragment writing a zero UV for an unmapped mesh is the forged-attribute
    // form: a consumer cannot tell a fabricated (0,0) unwrap from an authored one.
    // Two TYPES rather than one arity-flagged fragment because the type IS the emitted vertex layout: MeshBuilder
    // takes the fragment as a type argument, so the unmapped and mapped layouts are distinguishable only by being
    // distinct types, and the member sets stay side by side because a struct inherits no implementation base —
    // there is no shared owner to fold them onto, and an interface default would put the arity back on a runtime
    // read the layout already fixed. MaxColors is zero on both: the property-table stamp IS the element identity
    // and per-vertex colour rides its own seam lane onto its own layout. The single custom attribute is the
    // _FEATURE_ID_0 ordinal EXT_mesh_features reads. Both getters THROW past their declared arity rather than
    // returning a zero vector, because a silent (0,0) is the same forged attribute the two-type split exists to
    // refuse; every setter is a no-op return, since SharpGLTF's own assembly writes fragments back through them
    // and a throwing setter would abort a write the builder is entitled to perform over values the constructor already fixed.
    // IVertexCustom runs THREE interfaces deep — IVertexCustom : IVertexMaterial : IVertexReflection — so a
    // fragment owes the morph pair (Subtract/Add over VertexMaterialDelta), the encoding declaration
    // (GetEncodingAttributes), and Validate — Validate is IVertexCustom's, NOT IVertexGeometry's, which declares no
    // such member. Its morph pair is the SharpGLTF VertexEmpty shape verbatim: a stamp-only fragment has no
    // interpolable channel, so Subtract returns VertexMaterialDelta.Zero and Add is a no-op — a delta computed off the
    // ordinal would interpolate a property-table ROW INDEX across a morph and address the wrong element.
    // GetEncodingAttributes DECLARES the emitted accessor layout the Toolkit encodes against, so the feature ordinal
    // writes as the scalar Float1 EXT_mesh_features reads and the mapped layout declares its TEXCOORD_0 beside it —
    // an unimplemented declaration writes the attribute under a guessed default format.
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

    // One triangle-soup MeshBuilder every arm shares, dispatched on ONE joint tuple pattern over the two
    // ORTHOGONAL layout axes — the property-table stamp (Some(row) per-element, None flat-soup) and the arena's
    // own Uv DESCRIPTOR. Four arms, one level: a nested ladder over discriminants available together is the
    // rejected shape. The UV axis is the seam's evidence, never a policy knob: the carrier declares an Uv lane
    // exactly when the source declared a parameterization (import#IMPORT_RAIL probes the TEXCOORD_0 accessor, the
    // IFC arm the tessellated face set's own texture map), so an unmapped element carries NO descriptor, emits the
    // untextured layout, and a mapped one carries TEXCOORD_0 the ChannelImage.CoordinateSet bindings sample
    // through. Both axes are presence patterns over the same Option shape — the emptiness probe a parallel-buffer
    // column forced is what a zero-filled lane could have forged past.
    static IMeshBuilder<MaterialBuilder> MeshOf(ImportedGeometry geometry, MaterialBuilder material, Option<int> feature) =>
        (feature, Lane(geometry, EncodingChannel.Uv)) switch {
            ({ IsSome: true, Case: int row }, { Case: float[] uv }) => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, FeatureUvVertex, VertexEmpty>(geometry.FormatKey), geometry, material, i => new FeatureUvVertex(row, Uv(uv, i))),
            ({ IsSome: true, Case: int row }, _)                    => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, FeatureVertex, VertexEmpty>(geometry.FormatKey), geometry, material, _ => new FeatureVertex(row)),
            (_, { Case: float[] uv })                               => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexTexture1, VertexEmpty>(geometry.FormatKey), geometry, material, i => new VertexTexture1(Uv(uv, i))),
            _                                                       => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>(geometry.FormatKey), geometry, material, static _ => default),
        };

    // Every lane arrives in the SAME vertex order the position lane uses, so this read is one index at the
    // channel's OWN declared arity — a stride literal beside the descriptor that already states it is the deleted
    // second fact, and a per-vertex lookup table beside it the deleted re-derivation.
    static Vector2 Uv(float[] uvs, int index) {
        int u = index * EncodingChannel.Uv.Arity;
        return new(uvs[u], uvs[u + 1]);
    }

    static MeshBuilder<MaterialBuilder, VertexPositionNormal, TvM, VertexEmpty> Filled<TvM>(
        MeshBuilder<MaterialBuilder, VertexPositionNormal, TvM, VertexEmpty> mesh, ImportedGeometry geometry, MaterialBuilder material, Func<int, TvM> slot)
        where TvM : struct, IVertexMaterial {
        var primitive = mesh.UsePrimitive(material);
        float[] verts = Required(geometry, EncodingChannel.Position);
        float[] normals = Required(geometry, EncodingChannel.Normal);
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

    // Write validation is the SAME axis the read sets: import#IMPORT_RAIL reads under ValidationMode.Strict, so a
    // document this rail authors — extension blocks, per-vertex feature stamps, an EXT_structural_metadata property
    // table, a KHR channel roster, every one a LinkException/SemanticException class — proves well-formed HERE
    // rather than in a downstream viewer, and the deterministic-bytes law is asserted against a validated model. The
    // mode is the policy column, not a literal: a compressed or partially-authored intermediate takes Skip on the
    // same value the read leg reads. GlbBytes' own Try.lift funnels the resulting ModelException onto the
    // typed rail, so validation adds no fault arm.
    static byte[] WriteGlb(ModelRoot model, InterchangePolicy policy) {
        if (policy.MergeBuffers) { model.MergeBuffers(); }
        return model.WriteGLB(new WriteSettings { MergeBuffers = policy.MergeBuffers, Validation = policy.Validation }).ToArray();
    }

    // PointAttribute.Wrap couples its point count to array.Length, so a pool rent (pow2-oversized backing) would
    // corrupt the attribute count — the lane read mints exactly Count × Arity floats off the descriptor, so the
    // coupling holds by construction and the pooled-staging law binds only the count-explicit meshopt kernel below.
    static byte[] DracoBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        var mesh = new DracoMesh { NumPoints = geometry.VertexCount };
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Position, EncodingChannel.Position.Arity, Required(geometry, EncodingChannel.Position)));
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Normal, EncodingChannel.Normal.Arity, Required(geometry, EncodingChannel.Normal)));
        // TEXCOORD_0 rides the same Wrap seam as position and normal — an unparameterized source declares NO Uv
        // descriptor, so it adds no attribute rather than a zero-filled one.
        if (Lane(geometry, EncodingChannel.Uv).Case is float[] uv) { mesh.AddAttribute(PointAttribute.Wrap(AttributeType.TexCoord, EncodingChannel.Uv.Arity, uv)); }
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            mesh.AddFace([(int)indices[tri * 3], (int)indices[tri * 3 + 1], (int)indices[tri * 3 + 2]]);
        }
        mesh.DeduplicateAttributeValues();   // the corner-expanded soup repeats each value 3-6x; collapse before encode per the catalogue law
        return Draco.Encode(mesh, new DracoEncodeOptions {
            PositionBits = policy.QuantizationBits, NormalBits = policy.QuantizationBits,
            // TextureCoordinateBits takes the SAME budget the geometry lanes take rather than the package's own 12-bit
            // default: a bit budget the caller narrowed for a streaming tile must narrow every lane it encodes,
            // and a lane silently held at a package default is the quantization the policy column does not govern.
            TextureCoordinateBits = policy.QuantizationBits,
            CompressionLevel = DracoCompressionLevel.Optimal,
        });
    }

    // MeshoptLane rosters the frame's attribute streams: the glTF attribute name as its KEY, the kernel
    // EncodingChannel the seam arena supplies it from, the meshopt FILTER token its encode applies, and the row's
    // lane bit. The filter is the whole point of the extension's layout — a raw float32 stream through an entropy
    // coder realizes none of the compression the format exists for, and the quantization the caller budgeted
    // governs nothing. Position rides the exponent filter (shared-vector, so the three components keep one
    // exponent and a millimetre model and a kilometre one both spend their bits on mantissa), normal the
    // octahedral filter (two components reconstruct the unit vector, so half the lane disappears), UV the exponent
    // filter at the same budget. Import already decodes every one of these tokens (import#IMPORT_RAIL MeshoptView's
    // filter switch), so the frame is decodable by the rail's own reader rather than a second grammar.
    [SmartEnum<string>]
    public sealed partial class MeshoptLane {
        public static readonly MeshoptLane Position = new("POSITION",   EncodingChannel.Position, filter: "EXPONENTIAL", bit: 1);
        public static readonly MeshoptLane Normal   = new("NORMAL",     EncodingChannel.Normal,   filter: "OCTAHEDRAL",  bit: 2);
        public static readonly MeshoptLane Uv       = new("TEXCOORD_0", EncodingChannel.Uv,       filter: "EXPONENTIAL", bit: 4);

        public EncodingChannel Channel { get; }
        public string Filter { get; }
        public int Bit { get; }

        // Component count IS the carrier lane's declared arity — ONE fact, so a widened channel cannot drift
        // from the descriptor the encode actually reads and the frame header stays honest.
        public int Components => Channel.Arity;

        private MeshoptLane(string key, EncodingChannel channel, string filter, int bit) : this(key) =>
            (Channel, Filter, Bit) = (channel, filter, bit);
    }

    // Pooled staging end to end: every transient buffer rents through SpanOwner<T> (the meshopt pinned-pointer
    // surface takes EXPLICIT counts, never array lengths, so pool-oversized rents are safe) and only the final
    // self-delimiting frame allocates — the eight per-export staging arrays were the LOH churn the admitted
    // CommunityToolkit.HighPerformance owner deletes. The interleave is ROSTER-DRIVEN rather than a fixed vertex
    // struct: each active stream writes its own arity at its own running lead, so a fourth lane costs one roster
    // row and the arena's typed absence (NO Uv descriptor) drops its lane whole — a zero-filled UV stream inside
    // an encoded frame is the forged-attribute form a decoder cannot tell from an authored unwrap. Position and
    // Normal read through Required, so a carrier missing either refuses here rather than encoding a frame a viewer
    // reads as headless geometry.
    static unsafe byte[] MeshoptBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        Seq<(MeshoptLane Lane, float[] Source)> active =
            Seq((Lane: MeshoptLane.Position, Source: Required(geometry, MeshoptLane.Position.Channel)),
                (Lane: MeshoptLane.Normal,   Source: Required(geometry, MeshoptLane.Normal.Channel)))
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
        // One filter-coded stream PER LANE, exactly the layout EXT_meshopt_compression declares and the import
        // rail's per-view filter switch already decodes — the single interleaved stream forced one filter on
        // every lane, so an octahedral normal and an exponent-coded position could not coexist and neither ran.
        // EncodeVertexBufferLevel takes the bit budget as a real argument, so policy.QuantizationBits governs the
        // meshopt arm exactly as it governs Draco; version -1 keeps the process-wide EncodeVertexVersion.
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
        // Self-delimiting frame: a fixed header (uniqueCount, indexCount, laneMask, quantization bits, iLen)
        // then one (componentCount, byteLength) pair per lane in roster order, then the lane streams and the
        // index stream. Every DecodeVertexBuffer/DecodeFilter* call needs its element count, its stride, and its
        // byte length, and every lane needs its filter token — the mask recovers the roster rows, so a frame
        // omitting any of them is undecodable (the deleted form) and a lane roster row is the only growth axis.
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

    // Written extensions are the UNION of the policy roster and the payload's own obliged rows, deduped,
    // NARROWED through the format axis's declared write capability — `KhrExtension.Writables` — so a read-only
    // vocabulary row (an import-classified `_ior` or `_pbrSpecularGlossiness`) never enters as write support it
    // cannot fill, per the folder phantom-extension ruling. A caller who bound a KTX2 or transform-bearing map
    // without listing its extension is covered by the payload half, and a writable policy row with no binding
    // still enters because a downstream leg may author it. A Soup payload obliges nothing (a flat soup carries no
    // finish), so the union collapses to the policy roster there. This is a NARROWING, not a registration call:
    // SharpGLTF.Core's process-global ExtensionsFactory already carries every in-box KHR/EXT row and the writer
    // emits the block either way, so the retired per-row Registrar/Fin rail had no failure arm any row could reach
    // and every caller paid a Bind for a value that was always Succ.
    static Seq<KhrExtension> Registered(InterchangePolicy policy, ExportPayload payload) =>
        (policy.Extensions + payload.Switch(soup: static _ => Seq<KhrExtension>(), scene: static s => s.Elements.Obliges))
            .Distinct().Filter(static khr => KhrExtension.Writables.Contains(khr));

    // Artifact content key: the kernel seed-zero ContentHash over the seam CanonicalWriter fold of the format
    // key, the quality triple, and the emitted bytes — the one-hasher law every sibling key observes (reconstruct/
    // tessellation); minting through the Rasm.Compute InterchangeIdentity was the deleted downward strata reference.
    internal static ExportArtifact Sealed(InterchangeFormat format, ReadOnlyMemory<byte> bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes,
            ContentHash.Of(new CanonicalWriter(0.0)
                .String(format.Key).Double(policy.Deflection).Double(policy.Tolerance).Double(policy.AngleTolerance)
                .Raw(bytes.Span).ToBytes().Span),
            bytes.Length, at);
}
```

## [03]-[TILE_METADATA]

- Owner: `TileMetadata` the per-tile `EXT_structural_metadata` author over the seam `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, never a stored record) — one embedded schema carrying the element's `Classification` code, `ExternalId` GlobalId, name, and (as growth) the baked property/quantity columns, one `PropertyTable` per-feature value store, and the `EXT_mesh_features` feature-ID binding tying each GLB primitive vertex span to its element row so the Cesium 3D Tiles web peer resolves per-element metadata at pick time.
- Entry: `TileMetadata.Attach(GlbScene scene, Seq<Element> elements, Op key)` authors the structural-metadata schema/class/property-table on the `Author`-minted GLB scene — the feature-ID VALUES are already in the model (the per-vertex `_FEATURE_ID_0` stamps `Staged` authored through the `FeatureVertex` fragment), so `Attach` only defines the schema and binds the table, never re-walking or re-stamping geometry; `Fin<T>` aborts on a registration fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; the per-tile metadata emit composes through the `Rasm.Compute` interchange codec `TILE_PARTITION` at the seam and `Rasm.Bim` authors the canonical schema shape and the extension surface.
- Auto: `Attach` defines the `Element` structural-metadata schema (one property per canonical column — `GlobalId` off `Element.ExternalId`, `Class` an `IfcClass` enumeration off `Element.Classification.Code`, `Name`, and as growth the baked-Pset columns off `Element.Properties`) and adds a per-feature `PropertyTable` whose ROWS ORDER BY the `GlbScene.Rows` ordinals — the one row space the `Staged` vertex stamps already index. Element semantics join by the seam GlobalId; an element-less row carries empty strings and the `Class` column's reserved `Unclassified` noData sentinel (a bare `IfNone(0)` silently claimed the first REAL `IfcClass` row). ONE `FeatureIDBuilder` binds per DISTINCT logical mesh with `nullFeatureId = Rows.Count`, so a shared-mesh repeat's null-row stamps resolve to "no feature" at pick rather than a wrong row.
- Receipt: the authored `EXT_structural_metadata` schema and `PropertyTable` are the per-tile semantic the web peer reads — the same seam `Element` vocabulary a consumer reads at the `Exchange/wire#WIRE_PROJECTION`, projected onto the binary tile metadata so a Cesium consumer resolves per-element BIM semantics at pick without a second metadata mint.
- Packages: SharpGLTF.Core, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new metadata column is one `UseProperty(name).With<Type>(...)` row on the embedded class fed from a baked `Element` field; a new feature-ID binding is one `FeatureIDBuilder` over the primitive; the `IfcClass` enumeration is one `UseEnumMetadata` row tracking the `IfcClass` vocabulary; never a hand-authored JSON metadata block and never a second per-tile metadata mint.
- Boundary: the per-tile metadata authors through the `SharpGLTF.Ext.3DTiles` `EXT_structural_metadata`/`EXT_mesh_features` surface — a hand-authored JSON `EXT_structural_metadata` block is the deleted form, the `StructuralMetadataClassProperty.With<Type>` selectors and the `PropertyTableProperty.SetValues<T>` binary encode own the schema and value storage; `Tiles3DExtensions.RegisterExtensions()` runs once before any author and the call is idempotent at the factory level; the per-feature semantic is the seam baked `Element` and a retired `BimElement` row crossing `Attach` is the deleted form (the element is the `Bake` fold over the `ElementGraph`, the `Classification` code resolved to the `IfcClass` enumeration, never a typed `IfcClass` on the row); the `IfcClass` column rides `UseEnumMetadata` so the closed BIM class vocabulary serializes by its enumeration rather than a free string; the tile-pyramid partitioning and streaming stay at `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` admits the extension surface and the canonical schema shape, never the tile pyramid; the `OneOf<int, Texture>` feature-ID attribute selector is a transitive `OneOf` dependency consumed only by `FeatureIDBuilder` and no Bim code references it directly; the per-tile `Element` semantic is the same vocabulary the wire projection carries, never a second metadata vocabulary.

```csharp signature
public static class TileMetadata {
    public static Fin<GlbScene> Attach(GlbScene scene, Seq<Element> elements, Op key) =>
        Try.lift(() => Author(scene, elements)).Run().MapFail(error => (Error)Detail.TileMetadata.At(key, error.Message));

    // Per-feature semantic is the seam baked `Element`: ExternalId GlobalId + the generic Classification code
    // resolved to the IfcClass enumeration + name (the Pset/Qto columns grow off the baked Element.Properties/
    // Quantities), so the tile carries the SAME vocabulary the wire projection does, never a second metadata mint.
    // Table rows are the Author-minted GlbScene.Rows instance ordinals — the SAME values Staged stamped into every
    // uniquely-meshed vertex's _FEATURE_ID_0 — so the feature-ID attribute and the property table index one row
    // space by construction; the element semantics join by GlobalId, a Rows entry with no baked element carries
    // empty columns (its stamp resolves, its semantics ride the wire). ONE FeatureIDBuilder binds per DISTINCT
    // logical mesh (the extension lives on the primitive — a per-node re-bind duplicates featureId sets on a
    // shared primitive); a shared mesh's null-row stamps resolve to nullFeatureId = Rows.Count, so a non-merged
    // repeat picks as "no feature" rather than mislabeling — per-element identity on repeats is the
    // GpuInstancingMinCount + EXT_instance_features arm, never a silent wrong row.
    static GlbScene Author(GlbScene scene, Seq<Element> elements) {
        Tiles3DExtensions.RegisterExtensions();
        var byExternal = elements.Choose(static e => e.ExternalId.Map(ext => (ext, e))).ToMap();
        var slots = toSeq(scene.Rows.AsIterable().OrderBy(static pair => pair.Value).Select(pair => byExternal.Find(pair.Key)));
        var root = scene.Model.UseStructuralMetadata();
        var schema = root.UseEmbeddedSchema("rasm-element");
        var classIndex = IfcClass.Items.Select(static (row, i) => (row.Key, i)).ToMap();
        // Enum table carries a reserved noData sentinel: an element-less row encodes -1 and picks as "no
        // class" — IfNone(0) silently claimed the FIRST real IfcClass row under noData: null, the deleted defect.
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

- Owner: `BimLod` the per-element LOD-pyramid leg ADDITIVE to the export rail — one progressive-detail chain per element derived through the catalogued `Meshopt.Simplify`/`SimplifySloppy` decimation keyed by target triangle ratio, with the `MeshletResidency` band through `Meshopt.BuildMeshlets` for the WebGPU raster path; `LodLevel` the per-level record carrying the decimated index buffer, the target ratio, and the per-LOD content key the `Rasm.Compute/Runtime/codecs#TILE_PARTITION` pyramid content-addresses.
- Entry: `BimLod.Pyramid(ImportedGeometry geometry, InterchangePolicy policy, Op key)` derives the LOD chain over the policy's ratio schedule (each level a `Meshopt.Simplify` at decreasing target index count, falling back to `Meshopt.SimplifySloppy` when the error threshold cannot be met), and `BimLod.Meshlets(ImportedGeometry geometry, Op key)` clusters the residency band through `Meshopt.BuildMeshlets` (bounded by `Meshopt.BuildMeshletsBound`, optimized per meshlet through `Meshopt.OptimizeMeshlet`) — `Fin<T>` aborts on a degenerate decimation captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; each level seals its own `ExportArtifact.ContentKey` so the web peer streams each LOD by view distance, the `TileMetadata` per-tile semantic riding each level unchanged.
- Receipt: each `LodLevel` carries its target ratio, resulting triangle count, the world-space `WorldError` deviation (`Meshopt.Simplify`'s relative `result_error` × `SimplifyScale` — the solver evidence the typed-receipt law keeps, a discarded `out` error is the deleted form), and the per-LOD content key — the same `InterchangeIdentity` the full-resolution `ExportArtifact` seals, computed per level so the `Rasm.Compute/Runtime/codecs#TILE_PARTITION` pyramid content-addresses every detail level and the cross-libs `WEB_GEOMETRY_RESIDENCY_WIRE` splat/meshlet manifest the AppUi projection mints streams each LOD by view distance against a real per-level error bound.
- Packages: Alimer.Bindings.MeshOptimizer, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new detail level is one ratio on the `InterchangePolicy.LodRatios` policy column (the schedule is policy data, never a fence-local constant), each landing one content-keyed `LodLevel` row on the pyramid; a new meshlet bound is one `MeshletResidency` band over the residency set; the per-tile `TileMetadata` semantic rides each LOD unchanged; never a per-element full-resolution emit and never a second LOD or residency owner.
- Boundary: the LOD decimation is `Alimer.Bindings.MeshOptimizer`'s — `Meshopt.Simplify` (error-threshold decimation with `SimplificationOptions` flags) and `Meshopt.SimplifySloppy` (aggressive fallback) over the optimized indexed buffer own the LOD chain, and a hand-rolled edge-collapse decimator is the deleted form; the meshlet residency rides `Meshopt.BuildMeshlets` (allocated via `BuildMeshletsBound`, optimized per meshlet via `OptimizeMeshlet`) so the WebGPU raster path consumes the package-owned meshlet partition, never a hand-rolled cluster algorithm; the per-LOD content key meets `Rasm.Compute/Runtime/codecs#TILE_PARTITION` at the seam — `Rasm.Bim` derives the per-element pyramid and seals each level's content key, the tile-pyramid partitioning and streaming stay at Compute consumed at the seam; the residency band feeds the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest the AppUi projection mints, never a second residency owner; the LOD leg composes the same `ImportedGeometry` triangle-soup the `EXPORT_RAIL` `SceneOf` reads, never a second geometry carrier.

```csharp signature
// Per-level receipt. WorldError is the solver's own deformation bound; Acmr/Overdraw/Overfetch are the three
// MEASURED draw-cost figures the analyzers return over the level's own index set — the typed-algorithm-receipt
// law that already keeps WorldError here keeps them beside it, because a streaming consumer selecting LODs by
// deviation alone can pick a level that deforms less and DRAWS worse than the parent it replaced.
public sealed record LodLevel(
    int Level, double TargetRatio, int TriangleCount, double WorldError,
    double Acmr, double Overdraw, double Overfetch,
    ReadOnlyMemory<uint> Indices, UInt128 ContentKey);

// One meshlet band row: the cluster and the sphere-plus-normal-cone bounds a mesh-shader path culls on. A
// Meshlet with no Bounds is a partition with NO selection criterion — the residency band the AppUi
// WEB_GEOMETRY_RESIDENCY_WIRE manifest consumes exists to be culled, and the quantized cone_axis_s8/
// cone_cutoff_s8 bytes are exactly the wire-sized payload that manifest carries.
public sealed record MeshletBand(Meshlet Meshlet, Bounds Bounds);

public static class BimLod {
    public static Fin<Seq<LodLevel>> Pyramid(ImportedGeometry geometry, InterchangePolicy policy, Op key) =>
        Try.lift(() => Levels(geometry, policy)).Run().MapFail(error => (Error)Detail.LodDecimate.At(key, error.Message));

    // Attributes builds the SAME interleave the encode path does — normals then UVs behind the position
    // lane — so the simplifier sees the whole vertex, not its position alone. Weights come off the policy roster
    // by canonical channel name, so a source declaring no unwrap contributes no lane and costs nothing.
    static unsafe Seq<LodLevel> Levels(ImportedGeometry geometry, InterchangePolicy policy) {
        var source = new uint[geometry.Indices.Length];
        for (int i = 0; i < source.Length; i++) { source[i] = (uint)geometry.Indices.Span[i]; }
        float[] verts = BimExport.Required(geometry, EncodingChannel.Position);
        nuint vertexCount = (nuint)geometry.VertexCount;
        nuint vertexStride = (nuint)(EncodingChannel.Position.Arity * sizeof(float));
        var (attributes, weights) = Attributes(geometry, policy);
        float scale;
        fixed (float* vPtr = verts) { scale = Meshopt.SimplifyScale(vPtr, vertexCount, vertexStride); }
        return policy.LodRatios.Map((ratio, level) =>
            Decimate(source, verts, attributes, weights, vertexCount, vertexStride, scale, ratio, level, geometry.FormatKey, policy));
    }

    // Attribute lanes in the policy roster's own order, each resolved to the kernel channel the arena addresses.
    // That roster is the SHARED decimation vocabulary the point-cloud pyramid also reads (reconstruct#RECONSTRUCT
    // weights `base_color` off the same column), so the mesh leg projects only the rows a per-vertex lane can
    // serve and a row the arena declares NO descriptor for contributes nothing — weight vector and interleave stay
    // index-aligned by construction, each lane's width the channel's own arity rather than a literal beside it.
    static (float[] Lanes, float[] Weights) Attributes(ImportedGeometry geometry, InterchangePolicy policy) {
        var rows = policy.AttributeWeights
            .Choose(row => (row.Channel switch {
                    "geometry_normal" => Some(EncodingChannel.Normal),
                    "geometry_uv" => Some(EncodingChannel.Uv),
                    _ => Option<EncodingChannel>.None,
                })
                .Bind(channel => BimExport.Lane(geometry, channel).Map(source => (channel.Arity, row.Weight, Source: source))))
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

    // target_error is RELATIVE to mesh extents under the options flag (0.01 = 1% deformation); SimplifyScale is the
    // relative->world conversion factor for the RECEIPT, never a budget multiplier — `0.01f * scale` passed a
    // world-sized budget where a fraction belongs, so every level hit its count under unbounded deformation (the
    // deleted defect). SimplifyWithAttributes is the ATTRIBUTE-AWARE collapse: a position-only simplifier migrates
    // vertices across a UV discontinuity, so LOD1 of a textured element renders its map smeared across the seam — the
    // exact failure this campaign exists to foreclose — and the weighted attribute error makes crossing the
    // seam cost what it costs. SimplifyLockBorder pins the open boundary so adjacent decimated elements keep a
    // watertight shared edge instead of opening gaps; the flag is the policy column because a whole-model
    // decimation wants it and an isolated-part one does not. The sloppy fallback stays position-only and
    // border-free by construction (the package ships no attribute form), so it is the LAST resort it always was
    // and its receipt reads the same measured figures.
    static unsafe LodLevel Decimate(
        uint[] source, float[] verts, float[] attributes, float[] weights,
        nuint vertexCount, nuint vertexStride, float scale, double ratio, int level, string formatKey, InterchangePolicy policy) {
        nuint sourceCount = (nuint)source.Length;
        nuint targetCount = (nuint)((long)source.Length * ratio);
        nuint attributeCount = (nuint)weights.Length;
        nuint attributeStride = (nuint)(weights.Length * sizeof(float));
        var options = policy.LockBorder ? SimplificationOptions.SimplifyLockBorder : SimplificationOptions.None;
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
        // Three analyzers read the level's OWN index set against the shared position lane, so the cost figures
        // are measured on the emitted level rather than inferred from its ratio.
        var cache = Meshopt.AnalyzeVertexCache(indices, vertexCount, cacheSize: 16, warpSize: 0, primGroupSize: 0);
        var overdraw = Meshopt.AnalyzeOverdraw(indices, verts, vertexStride);
        var fetch = Meshopt.AnalyzeVertexFetch(indices, vertexCount, vertexStride);
        var bytes = MemoryMarshal.AsBytes(indices.AsSpan());
        return new LodLevel(level, ratio, (int)resultCount / 3, resultError * scale,
            cache.acmr, overdraw.overdraw, fetch.overfetch, indices,
            ContentHash.Of(new CanonicalWriter(0.0)
                .String($"{formatKey}:lod{level}").Double(policy.Deflection).Double(policy.Tolerance).Double(policy.AngleTolerance)
                .Raw(bytes).ToBytes().Span));
    }

    public static unsafe Fin<Seq<MeshletBand>> Meshlets(ImportedGeometry geometry, Op key) =>
        Try.lift(() => Cluster(geometry)).Run().MapFail(error => (Error)Detail.MeshletBuild.At(key, error.Message));

    static unsafe Seq<MeshletBand> Cluster(ImportedGeometry geometry) {
        var indices = new uint[geometry.Indices.Length];
        for (int i = 0; i < indices.Length; i++) { indices[i] = (uint)geometry.Indices.Span[i]; }
        float[] verts = BimExport.Required(geometry, EncodingChannel.Position);
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
            // OptimizeMeshlet and ComputeMeshletBounds take the same two slices, so the cull
            // bounds derive inside the loop that already holds them — a second walk over the meshlet set is the
            // deleted re-derivation. Bounds are computed AFTER the per-meshlet optimize so the cone fits the
            // emitted triangle order.
            for (nuint m = 0; m < count; m++) {
                Meshopt.OptimizeMeshlet(&mvPtr[mPtr[m].vertex_offset], &mtPtr[mPtr[m].triangle_offset], mPtr[m].triangle_count, mPtr[m].vertex_count);
                bounds[(int)m] = Meshopt.ComputeMeshletBounds(
                    meshletVertices.AsSpan((int)mPtr[m].vertex_offset, (int)mPtr[m].vertex_count),
                    meshletTriangles.AsSpan((int)mPtr[m].triangle_offset, (int)mPtr[m].triangle_count * 3),
                    verts, vertexStride);
            }
        }
        return toSeq(meshlets.AsSpan(0, (int)count).ToArray()
            .Select((meshlet, m) => new MeshletBand(meshlet, bounds[m])));
    }
}
```

## [05]-[SCHEDULE_ANIMATION]

- Owner: `ScheduleAnimation` the 4D-emit leg ADDITIVE to the export rail — one glTF `Animation` baking the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element keyframe tracks: each `ConstructionTask`'s scheduled `Interval` drives a per-element visibility track (the element is invisible before its task starts and visible from its task start) with an optional scale track (the element grows from a zero-scale point to its full scale across its task window) so a viewer scrubs the construction sequence on the GLB timeline, and an optional in-progress base-colour track tints the element across its window through the material `KHR_animation_pointer` channel glTF's absent per-node colour property forces; `AnimationTrack` the per-element keyframe record carrying the element `GlobalId`, the appear-time and full-time seconds, the glTF `Node` the element's mesh binds to, and the logical material the colour track bound or the refusal a pooled material earns.
- Entry: `BimExport.AnimateSchedule(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy, Op key)` bakes the schedule into the scene model's animation set — projecting each `ConstructionTask` `Interval` bound onto its glTF time-in-seconds through `policy.SecondsOf(Instant moment, Instant projectStart)` (the bound mapped to the timeline via the NodaTime `Duration` from the project start, scaled by `policy.SecondsPerDay`), resolving each assigned element's glTF `Node` through the `Author`-minted `GlbScene` `GlobalId→Node` index (the element `GlobalId` is the seam `Graph/element#ELEMENT_GRAPH` `Object.ExternalId`; `Author` names each node by it, so the 4D leg binds the scene emitted — the retired caller-supplied index parameter is GONE), and authoring one `KHR_node_visibility` visibility channel (the `policy.Grow` scale channel and the `policy.Tint` material base-colour channel when set) per element through the SharpGLTF `Animation.CreateVisibilityChannel`/`CreateScaleChannel`/`CreateMaterialPropertyChannel` keyframe surface — `Fin<T>` aborts on a SharpGLTF authoring fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; the animation and the `Planning/schedule#SCHEDULE` `ConstructionState.At` snapshot share one `Interval`-to-`Instant` time axis so a scrub at glTF time `t` shows exactly the element set `ConstructionState.At` resolves at the inverse instant (the schedule owner is the `BimModel`→`ElementGraph` cross-file alignment point its rebuild settles).
- Auto: `AnimateSchedule` registers `KHR_node_visibility`, creates one `Animation`, and folds each element's assigned task windows into an `AnimationTrack` (appear = earliest `Interval.Start`, full = latest `Interval.End`, so a multi-task element appears at its earliest task). Each element gets a visibility track popping in at its appear time under the `STEP` interpolation the `bool` channel forces, the optional scale track grows it from zero across its window under `LINEAR`, and the optional tint track drives its material's `baseColorFactor` from the policy's active factor at appear to the material's OWN authored factor at settle — bound only where one node references one material no second node shares, because `Staged` pools materials on the finish key and a shared material tints every repeat on one element's schedule. `policy.SecondsOf` projects an `Interval` bound onto the float-seconds axis the `ConstructionState.At` snapshot reads, so the keyframe author and the snapshot never carry two clocks; the scene returns with `LogicalAnimations` populated so `Emit` seals the animated GLB and the `TileMetadata` semantic rides each frame unchanged.
- Receipt: the `Seq<AnimationTrack>` is the 4D-emit evidence — each row carries the element `GlobalId`, the appear/full seconds, the bound `Node` logical index so the Cesium/three.js timeline scrub resolves the construction state at any timeline instant, and the `TintedMaterial` index the colour channel bound (absent where the policy carried no tint or a pooled material refused the 1:1 binding, so a reader tells an untinted run from a refused element instead of reading silence); the animated GLB the `WriteGlb` emits is the streamed 4D timeline a web viewer plays, the `Planning/schedule#SCHEDULE` `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` re-keying the animation only on a re-sequenced plan.
- Packages: SharpGLTF.Core, SharpGLTF.Runtime, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new NODE-scoped keyframe channel — a translation track lowering an element into place, a rotation track swinging it — is one `Animation.Create*Channel` arm on the same fold; a new MATERIAL-scoped channel is one `CreateMaterialPropertyChannel` pointer tail beside `BaseColorPointer`, because glTF declares no per-node colour or factor property and every material-valued track is a `KHR_animation_pointer` path onto `/materials/{index}/…`, authorable only on a material no second node references; a new interpolation mode rides the SharpGLTF `bool linear` channel knob; a new time-axis or tint policy is one column on `ScheduleAnimationPolicy`; never a per-element `Animation` instance, never a hand-authored glTF animation JSON block, and never a second time axis beside the `ConstructionState.At` `Interval`.
- Boundary: keyframes ride the SharpGLTF `ModelRoot.CreateAnimation` + `Animation.Create*Channel` surface — a hand-authored glTF `animations[]`/`samplers[]`/`channels[]` JSON block is the deleted form. `KHR_node_visibility` drives the per-element visibility keyframe, so the `bool` track is the settled `format#FORMAT_AXIS` `KhrExtension.NodeVisibility` row registered once through the factory — a custom visibility-by-opacity hack is the deleted form. `KHR_animation_pointer` drives the colour track through `CreateMaterialPropertyChannel`, and its `format#FORMAT_AXIS` `KhrExtension.AnimationPointer` row registers exactly when the policy carries a tint, so the row never advertises a capability the run does not exercise; a per-node colour track is UNSPELLABLE — glTF's node channels are translation, rotation, scale, weights, and visibility alone, so a colour is a material property and a hand-authored `KHR_materials_*` factor track beside the pointer channel is the deleted form. SharpGLTF.Runtime is already csproj-referenced and already exercised — `import#IMPORT_RAIL` decodes each logical mesh through its `IMeshDecoder<Material>` surface — so this leg needs no new package and no new `InterchangeFormat` row. Animation time is the `Planning/schedule#SCHEDULE` `ConstructionTask.Interval` projected to seconds; a second clock on the export side is the named seam violation, the `ConstructionState.At` snapshot and the keyframe author reading one `Interval`-to-`Instant` axis. Per-element glTF `Node` resolves through the `Author`-minted `GlbScene` index (nodes NAMED by the seam `Object.ExternalId`) — a caller-supplied index parameter, a re-walked scene graph, or a second index mint is the deleted form; a 4D-emit fault lowers onto `Model/faults#FAULT_BAND` `BimFault`.

```csharp signature
public sealed record ScheduleAnimationPolicy(
    double SecondsPerDay, bool Grow, double EpsilonSeconds, Option<Vector4> Tint = default) {
    public static readonly ScheduleAnimationPolicy Default = new(SecondsPerDay: 1.0, Grow: false, EpsilonSeconds: 0.001);
    public static readonly ScheduleAnimationPolicy Growing = Default with { Grow = true };
    // Tint names the SCENE-LINEAR base-colour factor an element wears across its task window, settling on that
    // material's OWN authored factor at task end. Only the active factor is declared — settling reads off the
    // material, so a tinted playback returns every element to exactly the colour MaterialFinish.Author wrote and no
    // second colour vocabulary forks from the finish. This factor stays linear like every other glTF factor the rail
    // writes, so a display-referred tint would wash out exactly as an sRGB baseColorFactor does.
    public static readonly ScheduleAnimationPolicy Tinted = Default with { Tint = Some(new Vector4(1f, 0.62f, 0.09f, 1f)) };

    public float SecondsOf(Instant moment, Instant projectStart) =>
        (float)((moment - projectStart).TotalDays * SecondsPerDay);
}

// TintedMaterial is the tint track's own evidence: the logical material index the colour channel bound, or None
// where the policy carried no tint, the node bound no single material, or a pooled material serves a second node —
// so a receipt reader distinguishes an untinted run from an element the pooling refused, never a silent nothing.
public sealed record AnimationTrack(string GlobalId, float AppearSeconds, float FullSeconds, int NodeIndex, Option<int> TintedMaterial);

public static partial class BimExport {
    // KHR_animation_pointer targets a material through this tail: CreateMaterialPropertyChannel prefixes
    // `/materials/{LogicalIndex}/` and verifies the whole path against the model's own reflection DOM before the
    // sampler mints, so a misspelled property faults at author time rather than reading as an inert track in a viewer.
    const string BaseColorPointer = "pbrMetallicRoughness/baseColorFactor";

    // Schema2 keys the authored settle-factor read on this channel name.
    const string BaseColorChannel = "BaseColor";

    // Element GlobalId->glTF Node index is the Author-minted GlbScene (nodes named by the seam
    // Object.ExternalId): this leg binds keyframes onto the scene actually emitted — never a caller-supplied
    // index, never a re-walked scene graph, never a retired BimModel. The extension sweep is the policy's own
    // roster: KHR_node_visibility always, KHR_animation_pointer exactly when a tint rides, so no row registers
    // without the arm that fills it.
    // KHR_node_visibility and, under a tint, KHR_animation_pointer are the rows this leg fills; both serialize
    // through SharpGLTF's own in-box factory, so the leg AUTHORS and the declared write set stays the scene's own
    // GlbScene.Extensions column. The retired per-row Register/Fin hop bound a rail whose failure arm no row could
    // reach, so every caller paid a TraverseM for a value that was always Succ.
    public static Fin<Seq<AnimationTrack>> AnimateSchedule(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy, Op key) =>
        Try.lift(() => Tracks(scene, network, policy)).Run()
            .MapFail(error => (Error)Detail.ScheduleAnimation.At(key, error.Message));

    static Seq<AnimationTrack> Tracks(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy) {
        var projectStart = network.Tasks.Min(static t => t.Scheduled.Start);
        var animation = scene.Model.CreateAnimation("construction-sequence");
        // Tasks index ONCE by GlobalId — the per-assignment Tasks.Find linear scan was O(assignments·tasks).
        var taskWindow = network.Tasks.Fold(Map<string, Interval>(), static (held, task) => held.TryAdd(task.GlobalId, task.Scheduled));
        // Material reference census, ONE pass, per-node distinct: glTF carries no per-node colour, so a tint targets
        // a MATERIAL and reaches every node that material serves. Staged pools materials on the finish key, so a
        // repeat wearing its neighbour's finish shares its material — a tint there lights elements whose tasks never
        // started. The census is what lets the tint author only on a 1:1 correspondence and the receipt record the rest.
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
        float settled = Math.Max(full, appear + (float)policy.EpsilonSeconds);
        animation.CreateVisibilityChannel(node, new Dictionary<float, bool> {
            [Math.Max(0f, appear - (float)policy.EpsilonSeconds)] = false,
            [appear] = true,
        });
        if (policy.Grow) {
            animation.CreateScaleChannel(node, new Dictionary<float, Vector3> {
                [appear] = Vector3.Zero,
                [settled] = Vector3.One,
            });
        }
        return new AnimationTrack(globalId, appear, full, node.LogicalIndex, Tinted(scene, animation, node, appear, settled, policy, references));
    }

    // Material colour rides its own track. glTF defines no per-node colour TRS property, so an in-progress tint is
    // a KHR_animation_pointer channel on the material's own baseColorFactor rather than a node channel — exactly why
    // no `Create*Channel` arm can carry it. Resolution reads scene.Model.LogicalMaterials by logical index, the
    // roster read satisfying the channel's shared-logical-parent guard, and authoring happens ONLY where one node
    // binds one material no second node references; settling restores that material's authored factor so the
    // element ends the timeline in its own colour.
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

    // One material or none: a mesh whose primitives carry two materials has no single colour slot an element-scoped
    // track could own, so it takes the same refusal a pooled material takes.
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

- Owner: `RoundTrip` the lossless verification matrix folding a seam `ElementGraph` emit→re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations into a typed `RoundTripReport` that witnesses per-element AND per-property field fidelity by the seam's structured member diff joined on the 1:1 `ExternalId` GlobalId, so the codec proves losslessness rather than asserting it; `RoundTripReport` the receipt partitioned by `InterchangeFormat` carrying the lossless-element count, the dropped-element set, and the per-element divergent-member set.
- Entry: `RoundTrip.Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` runs the source graph through one IFC serialization and back — emitting through the `EXPORT_RAIL` `BimExport.ExportIfc` (which delegates to `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`), re-decoding the artifact bytes through the `import#IMPORT_RAIL` `BimIo.ImportIfc` (the ONE `DatabaseIfc` decode owner — its `SemanticProjector.Sniff` schema sniff constructs the ifcXML/ifcJSON database at the EMITTED `ReleaseVersion`, so the reimport lands at the schema the export wrote, never the GeometryGym default [H8]; a page-local `new DatabaseIfc()` re-decode is the deleted form), re-projecting through a fresh `SemanticProjector(db, reconciler, profiles)` and folding the delta onto a `Genesis(source.Header)` seed through the seam `Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` (the `IfcLegality` constraint admitting the re-imported edges), then comparing the source and reimported graphs by baked-element member diff — `Fin<T>` aborts on a codec reject, a re-decode fault, or a predefined-gate reject in either leg (`Model/faults#FAULT_BAND` `BimFault.CodecReject`/`ModelRejected`/`UnmappedClass`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; `RoundTrip.Matrix(ElementGraph source, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` lifts the verify over the IFC STEP/XML/JSON triad (`InterchangeFormat.Ifc`/`IfcXml`/`IfcJson`) onto the per-format `Map<string, RoundTripReport>` fidelity matrix so a single call witnesses which serialization preserves which field.
- Auto: `Verify` emits the graph through one IFC serialization, re-decodes through `BimIo.ImportIfc`, re-projects and assembles the reimported `ElementGraph`, then folds the source-vs-reimported comparison through the seam diff — each rooted `Object` baked into an `Element` keyed by its 1:1 `ExternalId` GlobalId (the `NodeId` is freshly minted each re-ingest [H6], so the join is the GlobalId), a no-divergence element lossless, a divergence naming its changed members through the `Generator.Equals` `Inequalities` composed BARE — the noise axes (`Id`, `ExternalId`, `History`, `Parts`) are `[IgnoreEquality]` at the `Rasm.Element` owner, so no call-site filter roster exists — a source GlobalId absent from the reimport dropped. `RoundTripReport` reads the lossless count, the per-element divergent-member set (down to the exact `Properties[..].DataType`/`Quantities[..].Unit` path), and the dropped set; the geometry leg crosses the `tessellation#TESSELLATION_BRIDGE` companion, so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the companion. `Matrix` lifts `Verify` over the `InterchangeFormat` triad, keying the per-format reports so one matrix compares serializations.
- Receipt: the `RoundTripReport` per format is the codec-fidelity evidence — a per-format fidelity matrix proving which serialization preserves which field, an interchange-policy losslessness witness, and a codec regression oracle; the STEP report typically reads the highest match ratio (the canonical IFC physical file), the XML/JSON reports surfacing any serialization-specific field loss, and the divergent-member set the exact members a round-trip drops.
- Packages: GeometryGymIFC_Core, Rasm.Element, Generator.Equals, LanguageExt.Core, NodaTime, Rasm
- Growth: a new serialization format is one `InterchangeFormat` row the `Matrix` triad widens to; a new fidelity dimension (a placement-key match, a coverage round-trip) is one column on `RoundTripReport` over the same baked-element diff; a new comparison basis rides the existing `Generator.Equals` `Inequalities`; never a second element-comparison surface, never a per-format report record family, and never a parallel fidelity store.
- Boundary: the round-trip fold reuses the seam's `Generator.Equals` `Inequalities` member diff as the fidelity metric rather than minting a second element-comparison surface — a field-by-field string compare or a `Seq("content")` placeholder is the deleted form, the structured diff naming the EXACT divergent member path; the cycle composes the `EXPORT_RAIL` `ExportIfc` egress (itself delegating to `SemanticProjector.Emit`) and the `import#IMPORT_RAIL` `BimIo.ImportIfc` re-decode (the schema-sniffed `DatabaseIfc` owner — a THIRD page-local decode copy beside import/wire was the deleted form, and its missing sniff mis-reported the fidelity matrix at the wrong schema) folded through the seam `ProjectionAssembly.Assemble`, never the retired `BimModel.Project`/`IfcSemanticModel` lossy-row path and never a hand-rolled IFC re-author; the join is the stable 1:1 `ExternalId` GlobalId because the rooted `NodeId` is freshly minted each ingest [H6], and a NodeId-keyed join is the deleted form; the geometry leg crosses the `tessellation#TESSELLATION_BRIDGE` companion so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the same companion, and the verification couples to no host geometry type; the `RoundTripReport` is partitioned by `InterchangeFormat` over the one baked-element diff and a per-format `StepReport`/`XmlReport`/`JsonReport` class family is the deleted form; a round-trip rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), with no `.ToError()` hop.

```csharp signature
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
    // Matrix rows DERIVE from the format#FORMAT_AXIS Serialization column narrowed by RoundTrippable, so a new
    // IFC wire form joins the fidelity matrix with zero edit here.
    static readonly Seq<InterchangeFormat> IfcTriad =
        toSeq(InterchangeFormat.Items.Where(static f => f.RoundTrippable && f.Serialization.IsSome));

    // Lossless cycle over the SEAM graph: ExportIfc (-> SemanticProjector.Emit) seals the IFC bytes,
    // BimIo.ImportIfc re-builds the live DatabaseIfc (the import rail's ONE decode owner — Sniff-schema'd, so the
    // ifcXML/ifcJSON reimport constructs at the EMITTED ReleaseVersion [H8], where the deleted page-local
    // new DatabaseIfc() copy silently reimported at the GG default and mis-reported the matrix), a fresh
    // SemanticProjector(db, reconciler, profiles) re-projects, and ProjectionAssembly.Assemble folds the delta onto a
    // Genesis(source.Header) seed under the IfcLegality constraint, yielding the AssemblyReceipt — the round-trip
    // keeps .Graph and Compare witnesses fidelity by the seam member diff. The egress projector's ctor db is unused by Emit
    // (it builds its own target from the graph header), so an empty DatabaseIfc seeds it.
    public static Fin<RoundTripReport> Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        BimExport.ExportIfc(format, source, new SemanticProjector(new DatabaseIfc(), reconciler, profiles), InterchangePolicy.Canonical, clock, Option<EmitContext>.None, ctx.Key)
            .Bind(artifact => BimIo.ImportIfc(format, artifact.Bytes, ctx.Key))
            .Bind(db => ProjectionAssembly.Assemble(
                ProjectionSuite.Of(
                    Seq<IElementProjection>(new SemanticProjector(db, reconciler, profiles)),
                    Seq(ConstraintRegistration.Of(new IfcLegality()))),
                ElementGraph.Genesis(source.Header), ctx))
            .Map(static r => r.Graph)
            .Map(reimported => Compare(format.Key, source, reimported, ctx.Key));

    public static Fin<Map<string, RoundTripReport>> Matrix(ElementGraph source, ProjectionContext ctx, IClock clock, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        IfcTriad.TraverseM(format => Verify(source, format, ctx, clock, reconciler, profiles).Map(report => (format.Key, report))).As()
            .Map(static rows => rows.ToMap());

    static RoundTripReport Compare(string formatKey, ElementGraph source, ElementGraph reimported, Op key) {
        var sourceElements = ElementsByExternal(source, key);
        var reimportedElements = ElementsByExternal(reimported, key);
        var dropped = sourceElements.Keys.Filter(id => !reimportedElements.ContainsKey(id)).ToSeq();
        var lossy = sourceElements
            .Choose((id, element) => reimportedElements.Find(id)
                .Map(other => Divergence(element, other))
                .Filter(static fields => fields.IsEmpty == false));
        return new RoundTripReport(formatKey, sourceElements.Count, sourceElements.Count - dropped.Count - lossy.Count, dropped, lossy);
    }

    // Bake every rooted Object element keyed by its stable 1:1 ExternalId GlobalId — the NodeId is freshly minted each
    // re-ingest [H6], so the join is the GlobalId, never the id; the baked Element folds in the Pset/Qto/material bags,
    // so the roundtrip witnesses FULL element fidelity (class/predefined/representations PLUS properties/quantities/materials).
    static Map<string, Element> ElementsByExternal(ElementGraph graph, Op key) =>
        graph.ObjectNodes
            .Choose(o => o.ExternalId.Bind(ext => graph.Bake(o.Id, key).ToOption().Map(element => (ext, element))))
            .ToMap();

    // Generator.Equals member-level structured diff names the divergent members (Properties[..].FireRating,
    // Materials[0].Composition.Layers[2].Thickness), so a serialization that drops a property data type or a
    // quantity unit surfaces the EXACT member, never a "content" placeholder; lossless iff the diff is empty.
    // Noise axes — freshly-minted Id, join-key ExternalId, provenance History, child-owned Parts — carry
    // [IgnoreEquality] AT the Rasm.Element owner (Review/diff law), so Inequalities composes BARE and a call-site
    // member-name filter roster is the deleted form the owner-side annotation forecloses.
    static Seq<string> Divergence(Element source, Element reimported) =>
        toSeq(Element.EqualityComparer.Default.Inequalities(source, reimported))
            .Map(static i => i.Path.ToString());
}
```

## [07]-[TILE_AVAILABILITY]

- Owner: `TileAvailability` the 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream author over the `subtree` package — the tileset AVAILABILITY structure (the Morton-ordered tile/content/child-subtree bitstreams telling a 3D-Tiles client which implicit nodes exist) the `SharpGLTF.Ext.3DTiles` `[3]-[TILE_METADATA]` per-tile CONTENT author cannot reach, the two meeting at the shared Morton tile index; `TileNode` the scheme-neutral per-tile authoring coordinate — `Lod` the subdivision level (mapped onto the quadtree `subtree.Tile.Z` level field or the octree `subtree.Tile3D.Level`), `X`/`Y` the in-level position, `Z` the octree vertical axis (unused under the `Quadtree` scheme, where `subtree.Tile` carries no spatial third axis), with the `Available`/`ContentUri`/`GeometricError` columns the `subtree.Tile` node carries; `SubtreeReceipt` the authored binary beside the facts decoded back out of it and the kernel content key addressing it — retiring the hand-rolled implicit-tiling bitstream.
- Entry: `TileAvailability.Author(Seq<TileNode> tiles, ImplicitSubdivisionScheme scheme, Op key)` folds the tile list into the `.subtree` binary and READS IT BACK through `SubtreeReader.ReadSubtree` before returning, so the `SubtreeReceipt` it yields carries decoded facts and a bitstream that lost a node faults here rather than streaming nothing at a client, the `scheme` discriminant selecting the authoring root — `SubtreeCreator.GenerateSubtreefile(List<Tile>)` for `Quadtree` (each `TileNode` projected through `TileOf` onto `subtree.Tile(z: node.Lod, x, y, available)` so the LOD lands in the `Tile.Z` level field the Morton author folds on, carrying its `ContentUri`/`GeometricError`) and `SubtreeCreator3D.GenerateSubtreefile(List<Tile3D>)` for `Octree` (each projected through `TileOf3D` onto `subtree.Tile3D(level: node.Lod, x, y, z: node.Z)` so the octree gains its third spatial axis) — `Fin<T>` aborts on a degenerate tile list captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; `TileAvailability.AuthorMany(Seq<TileNode> tiles, ImplicitSubdivisionScheme scheme, Op key)` lifts to the matching `GenerateSubtreefiles` (the `Dictionary<Tile, byte[]>`/`Dictionary<Tile3D, byte[]>` multi-subtree overflow form) when the tileset exceeds one subtree's level budget, keying each binary by its root tile's `(Level, X, Y, Z)` coordinate (the library builds each root through `new Tile(level, x, y)`/`new Tile3D(level, x, y, z)`, so the root key reads the level-and-position identity, never the auxiliary `Tile.Lod` the author leaves zero).
- Auto: `Author` maps each `TileNode` onto the `subtree.Tile` node (or `subtree.Tile3D` under the `Octree` scheme), authors the binary availability bitstream, and witnesses it — the re-read `Subtree` record's `ContentAvailability` bit at each node's own `LevelOffset` + `MortonOrder` address must equal that node's `Available`, a uniform stream answering from its `*Constant` where the reader leaves the `BitArray` null, and a divergence faulting with the offending Morton positions named. `TileAvailability` is deliberately NOT the assertion target: `SubtreeCreator` derives it as the ancestor CLOSURE of the content set, so it answers "does an implicit node exist here" where content answers "does this node carry a payload", and the receipt counts each stream separately. `MortonIndex` buckets each tile's availability by its level and sets the bit cell at its `X`/`Y`(`/Z`) position, so tile and content availability order identically — a tile is "available with content" exactly when both bitstreams set the same Morton position, the same index the `[3]-[TILE_METADATA]` tile content keys off. Multi-subtree tilesets re-base child coordinates so the child-subtree availability pointers resolve.
- Receipt: `SubtreeReceipt` carries the subdivision scheme, the authored level depth, the DECODED tile- and content-availability set counts, the `.subtree` bytes, and the kernel content key — measured off the emitted bitstream rather than re-reported off the input, so a caller reads what the client will read. Key minting rides the kernel seed-zero `ContentHash` over the seam `CanonicalWriter` fold, the one-hasher law `Sealed` and the `BimLod` per-level keys observe, so a tileset's availability binary and its glTF tile content address in ONE content space; the multi-subtree `AuthorMany` form returns the package's own `Fill`-padded per-root binaries, which are subtree-local bitstreams no whole-tileset node set addresses.
- Packages: subtree, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new subdivision scheme is the `ImplicitSubdivisionScheme.Quadtree`/`Octree` discriminant the `SubtreeCreator`/`SubtreeCreator3D` pair already carries; a new availability column is one field on `TileNode` the `subtree.Tile` node exposes; a new decoded fact is one column on `SubtreeReceipt` read off the re-parsed `Subtree` record, never a value copied forward from the input; a multi-subtree overflow is the existing `GenerateSubtreefiles` form; never a hand-rolled Morton/bitstream codec and never a second availability authoring path beside `subtree`.
- Boundary: the `.subtree` availability authoring is `subtree`'s — `SubtreeCreator`/`SubtreeCreator3D` `GenerateSubtreefile`/`GenerateSubtreefiles`, the `Tile`/`Tile3D` authoring nodes, the `MortonOrder.Encode2D`/`Encode3D` z-order index composed with `LevelOffset.GetLevelOffset` into the one bit address, and `SubtreeReader.ReadSubtree` driving the `Author` round-trip witness own the bitstream, and a hand-rolled implicit-tiling bitstream or a hand-rolled Morton address beside the package's own is the retired form; the content/availability split is the law — `SharpGLTF.Ext.3DTiles` (`[3]-[TILE_METADATA]`) authors the per-tile glTF CONTENT and `EXT_structural_metadata`, `subtree` authors the tileset AVAILABILITY indexing which implicit nodes exist, the two meeting at the shared Morton tile index and never duplicating the availability logic; the tileset.json root hierarchy and the per-tile bounding-volume geometry stay outside this owner (the `subtree` package carries no tileset.json and no geometry), and the tile-pyramid partitioning/streaming stay at `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` authors the availability binary and the content glTF, never the pyramid.

```csharp signature
public sealed record TileNode(int Lod, int X, int Y, bool Available, string ContentUri, double GeometricError, int Z = 0);

// Receipt pairs the authored availability binary with facts DECODED back out of it: subdivision scheme, authored
// level depth, and two set-bit counts read off the re-parsed bitstreams — measured from emitted bytes, never
// re-reported off input. ContentKey mints through the kernel seed-zero ContentHash over the seam CanonicalWriter
// fold, the one-hasher law every sibling artifact key observes, so a tileset's availability binary and its glTF tile
// content address in ONE content space and a Rasm.Compute InterchangeIdentity mint is the deleted downward reference.
public sealed record SubtreeReceipt(
    subtree.ImplicitSubdivisionScheme Scheme, int Levels, int AvailableTiles, int ContentTiles,
    ReadOnlyMemory<byte> Bytes, UInt128 ContentKey);

public static class TileAvailability {
    public static Fin<SubtreeReceipt> Author(Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme, Op key) =>
        Try.lift(() => scheme == subtree.ImplicitSubdivisionScheme.Octree
                ? subtree.SubtreeCreator3D.GenerateSubtreefile(tiles.Map(TileOf3D).ToList())
                : subtree.SubtreeCreator.GenerateSubtreefile(tiles.Map(TileOf).ToList())).Run()
            .MapFail(error => (Error)Detail.SubtreeAuthor.At(key, error.Message))
            .Bind(binary => Witness(binary, tiles, scheme, key));

    // Witness reads the emitted bitstream BACK and re-checks every input node against its own Morton bit, so
    // receipt counts are decoded facts and a codec regression surfaces here rather than in a client that streams
    // nothing. Bit addressing composes the subdivision level's own offset with the in-level z-order index —
    // LevelOffset plus MortonOrder, the package's own arithmetic, matching where the per-level bitstrings
    // concatenate — and BitArray round-trips index-for-index through the writer's CopyTo / new BitArray(byte[]) pair.
    // CONTENT availability carries the INPUT set: SubtreeCreator derives TILE availability as the ancestor CLOSURE
    // (every parent of an available cell is set), so both bitstreams answer different questions — checking input
    // against the tile stream fails on every interior level, checking the closure against content fails on every
    // parent — and this witness reads each against its own.
    static Fin<SubtreeReceipt> Witness(byte[] binary, Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme, Op key) {
        int levels = tiles.Max(static node => node.Lod) + 1;
        int cells = subtree.LevelOffset.GetLevelOffset(levels, scheme);
        return Try.lift(() => subtree.SubtreeReader.ReadSubtree(new MemoryStream(binary, writable: false))).Run()
            .MapFail(error => (Error)Detail.SubtreeReread.At(key, error.Message))
            .Bind(read => tiles
                .Filter(node => Bit(read.ContentAvailability, read.ContentAvailabilityConstant, Position(node, scheme)) != node.Available)
                .Map(node => Position(node, scheme)) is var divergent && divergent.IsEmpty
                ? Fin.Succ(new SubtreeReceipt(
                    scheme, levels,
                    Set(read.TileAvailability, read.TileAvailabilityConstant, cells),
                    Set(read.ContentAvailability, read.ContentAvailabilityConstant, cells),
                    binary,
                    ContentHash.Of(new CanonicalWriter(0.0).String($"subtree:{scheme}").Raw(binary).ToBytes().Span)))
                // Faulting names the divergent Morton POSITIONS, capped, so a codec regression diagnoses from this
                // message alone rather than by re-running the author.
                : Fin.Fail<SubtreeReceipt>(
                    Detail.SubtreeAvailabilityMismatch.At(key, divergent.Count.ToString(), string.Join(',', divergent.Take(4)))));
    }

    // Addressing composes the level's own prefix offset (quadtree 4^L, octree 8^L sums) with the in-level z-order
    // index the package's own encoder produces.
    static int Position(TileNode node, subtree.ImplicitSubdivisionScheme scheme) =>
        subtree.LevelOffset.GetLevelOffset(node.Lod, scheme)
        + (int)(scheme == subtree.ImplicitSubdivisionScheme.Octree
            ? subtree.MortonOrder.Encode3D((ulong)node.X, (ulong)node.Y, (ulong)node.Z)
            : subtree.MortonOrder.Encode2D((uint)node.X, (uint)node.Y));

    // Uniform availability collapses to a CONSTANT descriptor whose BitArray the reader leaves null, so that
    // constant IS the answer at every position; a null-array read falling through to false would report every
    // uniformly-available tileset as a mismatch.
    static bool Bit(System.Collections.BitArray? bits, int constant, int at) =>
        bits is { } array ? at < array.Length && array[at] : constant != 0;

    // Set-bit count over the decoded stream, byte padding included harmlessly because padding bits read false; a
    // constant descriptor answers the whole level-stack cell count rather than a zero the stream never carried.
    static int Set(System.Collections.BitArray? bits, int constant, int cells) =>
        bits is { } array ? array.Cast<bool>().Count(static bit => bit) : constant != 0 ? cells : 0;

    public static Fin<Map<(int Level, int X, int Y, int Z), byte[]>> AuthorMany(Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme, Op key) =>
        Try.lift<Map<(int Level, int X, int Y, int Z), byte[]>>(() => scheme == subtree.ImplicitSubdivisionScheme.Octree
                ? subtree.SubtreeCreator3D.GenerateSubtreefiles(tiles.Map(TileOf3D).ToList())
                    .Select(static pair => ((pair.Key.Level, pair.Key.X, pair.Key.Y, pair.Key.Z), pair.Value)).ToMap()
                : subtree.SubtreeCreator.GenerateSubtreefiles(tiles.Map(TileOf).ToList())
                    .Select(static pair => ((pair.Key.Z, pair.Key.X, pair.Key.Y, 0), pair.Value)).ToMap()).Run()
            .MapFail(error => (Error)Detail.SubtreeAuthorMany.At(key, error.Message));

    // node.Lod -> Tile.Z is the quadtree subdivision level MortonIndex folds availability on; X/Y the in-level cell.
    static subtree.Tile TileOf(TileNode node) =>
        new(node.Lod, node.X, node.Y, node.Available) { ContentUri = node.ContentUri, GeometricError = node.GeometricError };

    // node.Lod -> Tile3D.Level the octree level; node.Z -> Tile3D.Z the third spatial axis; Available is set post-ctor.
    static subtree.Tile3D TileOf3D(TileNode node) =>
        new(node.Lod, node.X, node.Y, node.Z) { Available = node.Available };
}
```

## [08]-[COBIE_EMIT]

- Owner: `CobieEmit` the COBie 2.4 FM-handover author — a TRANSIENT `Xbim.IO.CobieExpress` `CobieModel` authored `Instances.New<T>` inside one transaction FROM the seam `ElementGraph` (never a held xBIM `IModel` authority beside the GeometryGym semantic authority, and never the `IfcToCoBieExpressExchanger` parallel xBIM-IFC reader — the seam graph IS the source), sealed to the XLSX deliverable through the store's `ExportToTable` bridge.
- Entry: `CobieEmit.Export(ElementGraph graph, Instant at, Op key)` → `Fin<CobieHandover>` (the sealed artifact beside the typed `CobieDegrade` roster) — folds the `Model/spatial#SPATIAL_STRUCTURE` view onto `CobieFacility`/`CobieFloor`/`CobieSpace`, each baked element onto `CobieComponent` (its reconciled type onto `CobieType`, deduplicated by the type node), and each Pset row onto a `CobieAttribute` per the `Semantics/properties#PROPERTY_TEMPLATES` template vocabulary (the template supplying the COBie attribute name and the seam `PropertyValue.Render` the value text); the artifact content key mints through the kernel seed-zero `ContentHash.Of` over the seam `CanonicalWriter` fold — the one content space every Exchange artifact addresses, never a second identity scheme.
- Auto: authoring is ONE `BeginTransaction` scope committed once; the spatial containment restores from the seam `Compose.Contain` edges so a component lands on its floor/space, the type join rides the `Assign.TypeDefinition` edge, and an element with no spatial host lands facility-scoped rather than dropping; `CobieAttribute` values render through the SAME seam typed-value family the IFC egress raises, so a COBie cell and a Pset re-emit never disagree.
- Receipt: the sealed `ExportArtifact` carries the XLSX bytes, the `InterchangeFormat` row, and the kernel content key — the FM-handover deliverable the CDE registers beside the IFC emit of the same graph.
- Packages: Xbim.CobieExpress, Xbim.IO.CobieExpress, Xbim.CobieExpress.Exchanger (transitively the `EntityFactoryCobieExpress` schema factory), Rasm.Element, Rasm, LanguageExt.Core.
- Growth: a new COBie sheet is one `Instances.New<T>` fold arm over an existing seam read (a `CobieSystem` arm off the `Model/systems#SYSTEM_TRACE` rows, a `CobieZone` arm off `Model/zones#ZONE_GRAPH`, a `CobieContact` arm off `OwnerHistory`); a new attribute source is one template row on the properties owner — never a second COBie model, never a per-sheet exporter family.
- Boundary: the seam graph is the ONLY source — `IfcToCoBieExpressExchanger` (the xBIM IFC→COBie exchanger) reads an xBIM `IModel`, a PARALLEL IFC stack to the GeometryGym authority, so composing it stands a second IFC reader (the named violation; the exchanger package is admitted for its schema factory only); the `CobieModel` is construct→author→export→dispose inside `Export` — a cached/held store is the deleted form; `properties.md` is the source VOCABULARY (the template names and datatypes), never re-derived here; the content key is the kernel `ContentHash` + seam `CanonicalWriter` (a `Rasm.Compute` `InterchangeIdentity` mint is the deleted downward strata reference).

```csharp signature
// COBie FM-handover author over the seam graph: one transaction, one spatial-plus-component fold, one XLSX seal.
// Store and its transaction are TRANSIENT — authored, exported, disposed; identity rides the seam GlobalId
// (CobieReferencedObject.ExternalId) so the FM deliverable joins the IFC emit of the same graph.

// Degrade vocabulary: an authored fact the fold READ and could not land. Every row names its subject, so a
// handover reader reaches the element rather than a tally — the LowerLog discipline the energy legs already hold.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CobieReason {
    public static readonly CobieReason FacilityMissing = new("facility-missing");
    public static readonly CobieReason HostAbsent = new("host-absent");
    public static readonly CobieReason TemplateUnmapped = new("template-unmapped");
    public static readonly CobieReason TypeUnresolved = new("type-unresolved");
    public static readonly CobieReason ValueUnrenderable = new("value-unrenderable");
}

public readonly record struct CobieDegrade(CobieReason Reason, string Subject);

// CobieHandover pairs the sealed artifact with what the fold could not carry, so a caller reads a thin register
// as thin rather than as complete — the EnergyOutcome.Emitted shape applied to the FM leg.
public sealed record CobieHandover(ExportArtifact Artifact, Seq<CobieDegrade> Degrades);

public static class CobieEmit {
    public static Fin<CobieHandover> Export(ElementGraph graph, Instant at, Op key) =>
        Try.lift(() => {
            using var model = new CobieModel();
            Seq<CobieDegrade> degrades;
            using (var txn = model.BeginTransaction("rasm-cobie")) {
                degrades = Author(model, graph, key);
                txn.Commit();
            }
            using var stream = new MemoryStream();
            // ExportToTable's `out string report` is REQUIRED by the overload, and it carries the store's own
            // mapping diagnostics — discarding it dropped the only evidence a sheet failed to map.
            model.ExportToTable(stream, ExcelTypeEnum.XLSX, out string report);
            return new CobieHandover(
                BimExport.Sealed(InterchangeFormat.Cobie, stream.ToArray(), InterchangePolicy.Canonical, at),
                report.Length > 0 ? degrades.Add(new CobieDegrade(CobieReason.TemplateUnmapped, report)) : degrades);
        }).Run().MapFail(error => (Error)Detail.CobieEmit.At(key, error.Message));

    // ONE fold, three landings: the spatial view descends facility -> floor -> space by the seam Compose edges the
    // spatial rank already orders, each baked element lands a CobieComponent under the space (or the facility, when
    // no space hosts it) with its reconciled type deduped on the TYPE NODE rather than on a name, and every
    // PropertyBag row lands a CobieAttribute through the properties template read. A graph with no IfcBuilding
    // yields ONE degrade row and no register, because a COBie sheet with no facility is not a thin handover but an
    // unreadable one.
    static Seq<CobieDegrade> Author(CobieModel model, ElementGraph graph, Op key) {
        Option<Node.Object> building = graph.ObjectNodes.Find(static o => o.Classification.Code == IfcClass.Building.Key);
        if (building.Case is not Node.Object root) {
            return Seq(new CobieDegrade(CobieReason.FacilityMissing, graph.Header.Schema.ToString()));
        }
        CobieFacility facility = model.Instances.New<CobieFacility>(f => Named(f, root));
        var types = new Dictionary<NodeId, CobieType>();
        var log = Seq<CobieDegrade>();
        // Spaces index by their own node id so a component's host resolves in one hop; a storey with no space
        // still lands its floor, because a floor is FM structure whether or not it was subdivided.
        var spaces = new Dictionary<NodeId, CobieSpace>();
        foreach (Node.Object storey in Parts(graph, root.Id, IfcClass.BuildingStorey)) {
            CobieFloor floor = model.Instances.New<CobieFloor>(f => { Named(f, storey); f.Facility = facility; });
            foreach (Node.Object space in Parts(graph, storey.Id, IfcClass.Space)) {
                spaces[space.Id] = model.Instances.New<CobieSpace>(s => { Named(s, space); s.Floor = floor; });
            }
        }
        // Components are the BAKED elements — the Bake fold already resolves each element's attached bags, so the
        // attribute pass reads one composed value rather than re-walking EdgesAt with case tests per consumer.
        Seq<Node.Object> occurrences = graph.ObjectNodes.Filter(static o => o.Kind == ObjectKind.Occurrence).ToSeq();
        // Templates resolve ONCE per distinct (class, predefined) pair and every component of that pair reads the
        // SAME map — the Semantics/properties#PROPERTY_TEMPLATES resolution law, where a per-element re-resolution
        // re-loads the catalogue for every row. Cobie is the definition set a handover grades against (the scope row
        // pins the COBie superset and its schema), and the dictionary leg is None because no bSDD client crosses this
        // exporter — the offline buildingSMART floor resolving is exactly the degraded mode that owner declares.
        Map<(string Code, string Token), Map<string, PropertyTemplate>> templates =
            occurrences.Map(static o => (o.Classification.Code, o.PredefinedType.Token)).Distinct()
                .Fold(Map<(string, string), Map<string, PropertyTemplate>>(), (acc, pair) =>
                    IfcClass.TryGet(pair.Code).Match(
                        None: () => acc,
                        Some: cls => acc.Add(pair, PropertyKey.Resolve(
                            cls,
                            Optional(pair.Token).Filter(static t => t.Length > 0 && t != PredefinedType.NotDefined.Token),
                            graph.Header.Schema, TemplateScope.Cobie, None))));
        foreach (Node.Object node in occurrences) {
            if (spaces.ContainsKey(node.Id) || node.Classification.Code == IfcClass.Building.Key
                || node.Classification.Code == IfcClass.BuildingStorey.Key) {
                continue;
            }
            if (graph.Bake(node.Id, key).ToOption().Case is not Element baked) {
                log = log.Add(new CobieDegrade(CobieReason.HostAbsent, Identity(node)));
                continue;
            }
            CobieComponent component = model.Instances.New<CobieComponent>(c => {
                Named(c, node);
                c.TagNumber = node.Tag;
                c.AssetIdentifier = Identity(node);
            });
            Host(graph, node, spaces).IfSome(space => component.Spaces.Add(space));
            log = TypeOf(model, graph, node, types).Match(
                Some: type => { component.Type = type; return log; },
                None: () => log.Add(new CobieDegrade(CobieReason.TypeUnresolved, Identity(node))));
            Map<string, PropertyTemplate> resolved = templates
                .Find((node.Classification.Code, node.PredefinedType.Token))
                .IfNone(Map<string, PropertyTemplate>());
            log = baked.Properties.Fold(log, (held, bag) => Attributes(model, component, bag, resolved, held));
        }
        return log;
    }

    // Provenance and naming ride the SAME two members on every entity, because CobieReferencedObject heads the
    // spine that owns the external id and CobieAsset the name — one helper rather than four repeated pairs.
    static void Named(CobieAsset asset, Node.Object node) {
        asset.Name = node.Name;
        asset.Description = node.Classification.Code;
        asset.ExternalId = Identity(node);
    }

    // Identity resolves to the seam ExternalId GlobalId where the source carried one, else the node id — a COBie row
    // and the IFC emit of the same graph join on one key and a reconstructed element still reaches a stable cell.
    static string Identity(Node.Object node) =>
        node.ExternalId.IfNone(node.Id.Value.ToString());

    // Host resolves the component's host space — the nearest containing spatial node the Compose edges name; an
    // element hosted by a storey or the building alone lands facility-scoped rather than dropping, so an FM
    // register never loses a component to an unsubdivided floor.
    static Option<CobieSpace> Host(ElementGraph graph, Node.Object node, Dictionary<NodeId, CobieSpace> spaces) =>
        graph.EdgesAt(node.Id).Choose(e =>
            e is Relationship.Compose c && c.Part == node.Id && c.SubKind != ComposeKind.Reference
                && spaces.TryGetValue(c.Whole, out CobieSpace? space)
                ? Some(space)
                : None).Head;

    // Type dedup keys on the TYPE NODE, so N components sharing one reconciled type reference ONE CobieType — a
    // name-keyed dedup merged two distinct types that happened to share a label and split one type that did not.
    static Option<CobieType> TypeOf(CobieModel model, ElementGraph graph, Node.Object node, Dictionary<NodeId, CobieType> held) =>
        graph.EdgesAt(node.Id).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.TypeDefinition && a.Subject == node.Id
                ? graph.Find<Node.Object>(a.Definition)
                : None).Head
            .Map(type => held.TryGetValue(type.Id, out CobieType? seated)
                ? seated
                : held[type.Id] = model.Instances.New<CobieType>(t => Named(t, type)));

    // Pset rows lower onto CobieAttribute through the resolved template map: the template supplies the COBie
    // attribute NAME — its Code IS the bag key, so the owner carries no second name column — and its declared unit,
    // and the seam typed value picks the Set overload, so a COBie cell and a Pset re-emit raise the same value and no
    // cell carries a stringified number the sheet cannot compute on. The lookup rides the owner's OWN {Set}.{Code}
    // key grammar, which is already the subject an unmapped row degrades under.
    static Seq<CobieDegrade> Attributes(
        CobieModel model, CobieAsset asset, PropertyBag bag, Map<string, PropertyTemplate> templates, Seq<CobieDegrade> log) =>
        bag.Values.AsIterable().Fold(log, (held, row) =>
            templates.Find($"{bag.SetName}.{row.Key}").Match(
                None: () => held.Add(new CobieDegrade(CobieReason.TemplateUnmapped, $"{bag.SetName}.{row.Key}")),
                Some: template => {
                    CobieAttribute attribute = model.Instances.New<CobieAttribute>(a => {
                        a.Name = template.Code;
                        a.Description = bag.SetName;
                        // Unit takes the declared token where either source stated one, else the seam dimension's own
                        // SI symbol — the canonical emit unit the template owner names for exactly this absence.
                        // SiSymbol is itself Option (a composed dimension the roster does not name), so the fallback
                        // BINDS through both absences onto one blank rather than nesting an Option a COBie string
                        // cell cannot carry.
                        a.Unit = template.Unit.IfNone(() => template.SiDimension.Bind(static d => d.SiSymbol).IfNone(""));
                    });
                    asset.Attributes.Add(attribute);
                    return Valued(attribute, row.Value)
                        ? held
                        : held.Add(new CobieDegrade(CobieReason.ValueUnrenderable, $"{bag.SetName}.{row.Key}"));
                }));

    // Value lowering is ONE dispatch over the seam PropertyValue union onto the typed Set overloads
    // CobieAttribute publishes — a numeric quantity lands as a FloatValue the spreadsheet computes on, a boolean
    // as a BooleanValue, an instant as a DateTimeValue, never one text cell that makes an area and a fire rating the
    // same kind.
    static bool Valued(CobieAttribute attribute, PropertyValue value) => value.Switch(
        state: attribute,
        measure:    static (a, m) => { a.Set(m.Value.Si); return true; },
        boolean:    static (a, b) => { a.Set(b.Value); return true; },
        text:       static (a, t) => { a.Set(t.Value); return true; },
        enumerated: static (a, e) => { a.Set(string.Join(',', e.Selected)); return true; },
        temporal:   static (a, t) => t.Value is TemporalValue.Stamp stamp
            ? Do(() => a.Set(stamp.At.ToDateTimeUtc()))
            : false,
        reference:  static (a, r) => { a.Set(r.Value); return true; });

    static bool Do(Action set) { set(); return true; }

    // Transitive OWNING decomposition step, class-filtered — the same descent law the energy massing lower reads,
    // so the COBie spatial tree and the emitted energy model agree about which storey holds which space.
    static Seq<Node.Object> Parts(ElementGraph graph, NodeId whole, IfcClass @class) =>
        graph.EdgesAt(whole).Choose(e =>
            e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference
                ? graph.Find<Node.Object>(c.Part).Filter(o => o.Classification.Code == @class.Key)
                : None).ToSeq();
}
```

## [09]-[RESEARCH]

(none)
