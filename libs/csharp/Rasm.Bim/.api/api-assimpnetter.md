# [RASM_BIM_API_ASSIMPNETTER]

`AssimpNetter` is the high-level managed wrapper over native Open Asset Import Library
(assimp): the scene/mesh exchange owner for the formats the other Bim codecs do not cover —
FBX (`.fbx`) and Collada (`.dae`) import/export plus the standalone 3MF read leg, retiring the
hand-rolled BCL `ThreeMfReader`. The whole API funnels through one disposable `AssimpContext`
whose `ImportFile*`/`ExportFile`/`ExportToBlob`/`ConvertFrom*` matrix drives the
`Scene`→`Node`→`Mesh`→`Material` model; `PostProcessSteps` is the post-import transform
algebra (triangulate, tangent-space, normal generation, coordinate-handedness) and
`PostProcessPreset` packages the common combinations.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AssimpNetter`
- package: `AssimpNetter`
- license: MIT wrapper over the BSD-3-Clause native assimp library
- assembly: `AssimpNetter` (object root namespace `Assimp`; the XML-doc file is `AssimpNet.xml`)
- namespace: `Assimp`
- asset: multi-target (net472, net48, net6.0, net8.0); the net10.0 consumer binds the `lib/net6.0` asset forward (zero net6 managed dependencies)
- native-runtime: a single osx-arm64 `libassimp.dylib` ships in the package and is loaded by the context; RID-coupled native, no managed fallback
- rail: scene-exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and post-processing
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:-------------------- |:------------- |:------------------------------------------------------------------------------------------------ |
| [01] | `AssimpContext` | scene-exchange | `sealed`, `IDisposable`; the single entrypoint owning import/export/convert, `Scale`/`X-Y-ZAxisRotation` root-transform, `PropertyConfigurations`, IO-system override, and format introspection |
| [02] | `PostProcessSteps` | scene-exchange | `[Flags]` enum of every post-import transform: `Triangulate`, `CalculateTangentSpace`, `GenerateNormals`/`GenerateSmoothNormals`/`ForceGenerateNormals`/`DropNormals`, `JoinIdenticalVertices`, `MakeLeftHanded`/`FlipUVs`/`FlipWindingOrder`, `PreTransformVertices`, `GlobalScale`, `LimitBoneWeights`/`SplitByBoneCount`/`Debone`, `OptimizeMeshes`/`OptimizeGraph`, `FindDegenerates`/`FindInvalidData`/`FindInstances`, `RemoveComponent`/`RemoveRedundantMaterials`, `GenerateUVCoords`/`TransformUVCoords`, `EmbedTextures`, `GenerateBoundingBoxes`, `ValidateDataStructure`, `ImproveCacheLocality`, `SortByPrimitiveType`, `SplitLargeMeshes`, `FixInFacingNormals` |
| [03] | `PostProcessPreset` | scene-exchange | static presets folding `PostProcessSteps`: `ConvertToLeftHanded`, `TargetRealTimeFast`, `TargetRealTimeQuality`, `TargetRealTimeMaximumQuality` |
| [04] | `PropertyConfig` | scene-exchange | a named importer/exporter configuration value (`SetConfig`/`RemoveConfig`/`ContainsConfig` on the context) |
| [05] | `ExcludeComponent` | scene-exchange | `[Flags]` component mask paired with `PostProcessSteps.RemoveComponent` (normals, colors, tangents, bones, animations,...) |

[PUBLIC_TYPE_SCOPE]: scene graph model
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:------------------ |:------------- |:------------------------------------------------------------------------------------------------ |
| [01] | `Scene` | scene-exchange | `sealed`; root container — `RootNode`, `SceneFlags`, and `Meshes`/`Materials`/`Lights`/`Cameras`/`Textures`/`Animations` lists with `Has*`/`*Count`; `FindBone`, `GetEmbeddedTexture`, `Metadata` |
| [02] | `Node` | scene-exchange | `Name`, `Transform` (`Assimp.Matrix4x4` — the binding's OWN column-vector matrix struct, DISTINCT from `System.Numerics.Matrix4x4`, so the numerics narrow is a transpose), `Parent`, `Children` (`NodeCollection`), `MeshIndices` (indices into `Scene.Meshes`); the recursive transform hierarchy |
| [03] | `Mesh` | scene-exchange | `Name`, `PrimitiveType`, `MaterialIndex`, `Vertices`/`Normals`/`Tangents`/`BiTangents` (`List<Vector3>`), `Faces`, `VertexColorChannels`/`TextureCoordinateChannels` (multi-channel arrays), `Bones`; `Has*`/`*Count` guards |
| [04] | `Face` | scene-exchange | one primitive's vertex-index list — `Indices` (`List<int>`) + `IndexCount` (`= Indices.Count`, the per-face fan-triangulation bound) and `HasIndices` |
| [05] | `Bone` / `VertexWeight` | scene-exchange | a skinning bone (offset matrix + `VertexWeight[]`) and per-vertex weight |
| [06] | `Metadata` | scene-exchange | `sealed` `Dictionary<string, Metadata.Entry>`; `Entry` is a `readonly record struct (MetaDataType DataType, object Data)` with typed `DataAs<T>()` — scene/node key-value metadata (units, up-axis, authoring tool) |
| [07] | `BoundingBox` | scene-exchange | per-mesh AABB populated by `PostProcessSteps.GenerateBoundingBoxes` |

[PUBLIC_TYPE_SCOPE]: material and texture model
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:--------------------- |:------------- |:--------------------------------------------------------------------------------------------- |
| [01] | `Material` | scene-exchange | property-bag material: `Name`, `ShadingMode`, `ColorDiffuse`/`Ambient`/`Specular` (`Vector4`), `GetMaterialTexture`/`HasProperty`, and the nested `PBRMaterialProperties` (BaseColor/Metalness/Roughness/Normal/Emission texture slots) + `ShaderMaterialProperties` |
| [02] | `Material.PBRMaterialProperties` | scene-exchange | the metallic-roughness PBR view: `HasTextureBaseColor`/`TextureBaseColor`, `TextureMetalness`, `TextureRoughness`, `TextureNormalCamera`, `TextureEmissionColor` (each a `TextureSlot`) |
| [03] | `TextureSlot` | scene-exchange | a texture binding: file path, `TextureType`, UV index, `TextureMapping`, `TextureWrapMode`, blend factor, `TextureOperation` |
| [04] | `TextureType` | scene-exchange | enum of texture semantics (`BaseColor`, `Diffuse`, `Normals`, `Metalness`, `Roughness`, `EmissionColor`,...) |
| [05] | `EmbeddedTexture` | scene-exchange | a texture stored in the scene (compressed bytes or raw `Texel[]`); reached via `Scene.GetEmbeddedTexture` |
| [06] | `ShadingMode` / `BlendMode` | scene-exchange | classic shading model and material blend-mode enums |
| [07] | `MaterialProperty` | scene-exchange | a raw typed material property key (`PropertyType`) the high-level accessors wrap |

[PUBLIC_TYPE_SCOPE]: animation, format introspection, IO, and logging
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:--------------------------- |:------------- |:--------------------------------------------------------------------------------------- |
| [01] | `Animation` | scene-exchange | a named animation with `NodeAnimationChannel`s (TRS keyframes) and morph channels |
| [02] | `NodeAnimationChannel` | scene-exchange | per-node `VectorKey`/`QuaternionKey` keyframes + pre/post `AnimationBehaviour` |
| [03] | `MeshMorphAnimationChannel` / `MeshAnimationAttachment` | scene-exchange | vertex-morph animation tracks and morph targets |
| [04] | `ExportFormatDescription` | scene-exchange | a supported export format descriptor: `FormatId` (the `exportFormatId` arg), `Description`, `FileExtension` |
| [05] | `ImporterDescription` | scene-exchange | a supported importer descriptor: name, file extensions, `ImporterFeatureFlags` |
| [06] | `IOSystem` / `IOStream` | scene-exchange | `abstract` custom virtual-filesystem seams; `SetIOSystem` routes imports through a custom byte source |
| [07] | `LogStream` / `ConsoleLogStream` / `DefaultLogStream` | scene-exchange | native-log capture seams (`LoggingCallback`) for diagnostics |
| [08] | `ExportDataBlob` | scene-exchange | in-memory export result — `Data` (`byte[]`, the `exportFormatId` payload) + `Name` and the `NextBlob` chained satellite blob (`HasData` guard); `BlobBinaryReader` reads it |
| [09] | `AssimpException` | scene-exchange | thrown on native import/export failure; the boundary catch that lowers to `BimFault.CodecReject` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: AssimpContext — import
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------- |:----------------------------------------------------- |:------------------------------------------ |
| [01] | `AssimpContext.ImportFile` | `(string file)` / `(string file, PostProcessSteps)` | import a scene from a file path |
| [02] | `AssimpContext.ImportFileFromStream` | `(Stream, string formatHint = null)` / `(Stream, PostProcessSteps, string formatHint = null)` | import from a stream with a format hint |
| [03] | `AssimpContext.GetSupportedImportFormats` | `()` → `string[]` | the importable extensions of the loaded native lib |
| [04] | `AssimpContext.GetImporterDescriptions` | `()` → `ImporterDescription[]` | per-importer capability descriptors |
| [05] | `AssimpContext.GetImporterDescriptionFor` | `(string fileExtension)` → `ImporterDescription` | the importer that handles an extension |
| [06] | `AssimpContext.IsImportFormatSupported` | `(string format)` → `bool` | guard before import |

[ENTRYPOINT_SCOPE]: AssimpContext — export
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

`exportFormatId` is one of `GetSupportedExportFormats()[i].FormatId` (e.g. `fbx`, `collada`,
`gltf2`, `glb2`, `obj`, `stl`); a `PostProcessSteps` argument is a PRE-export transform applied
to the scene before serialization.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------- |:------------------------------------------------------------------- |:------------------------------------------ |
| [01] | `AssimpContext.ExportFile` | `(Scene, string fileName, string exportFormatId)` / `(…, PostProcessSteps preProcessing)` | export a scene to a file in the chosen format |
| [02] | `AssimpContext.ExportToBlob` | `(Scene, string exportFormatId)` / `(…, PostProcessSteps)` → `ExportDataBlob` | export to an in-memory blob (no file) |
| [03] | `AssimpContext.GetSupportedExportFormats` | `()` → `ExportFormatDescription[]` | the exportable formats + their `FormatId`/extension |
| [04] | `AssimpContext.IsExportFormatSupported` | `(string format)` → `bool` | guard before export |

[ENTRYPOINT_SCOPE]: AssimpContext — convert and configure
- package: `AssimpNetter`
- namespace: `Assimp`
- rail: scene-exchange

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------- |:------------------------------------------------------------------------------------------- |:------------------------------------------ |
| [01] | `AssimpContext.ConvertFromFileToFile` | `(in, out, exportFormatId)` / `(in, out, exportFormatId, exportSteps)` / `(in, importSteps, out, exportFormatId, exportSteps)` | one-call file→file transcode |
| [02] | `AssimpContext.ConvertFromFileToBlob` | `(in, exportFormatId, …)` | file→blob transcode |
| [03] | `AssimpContext.ConvertFromStreamToFile` | `(Stream, importFormatHint, out, exportFormatId, …)` | stream→file transcode |
| [04] | `AssimpContext.ConvertFromStreamToBlob` | `(Stream, importFormatHint, exportFormatId, …)` | stream→blob transcode |
| [05] | `AssimpContext.SetConfig` / `RemoveConfig` / `ContainsConfig` | `(PropertyConfig)` / `(string)` | importer/exporter tuning |
| [06] | `AssimpContext.SetIOSystem` / `RemoveIOSystem` | `(IOSystem)` | route IO through a custom virtual filesystem |
| [07] | `AssimpContext.Scale` / `XAxisRotation` / `YAxisRotation` / `ZAxisRotation` | properties | a root-transform applied on import |

## [04]-[IMPLEMENTATION_LAW]

[CONTEXT_LIFECYCLE]:
- `AssimpContext` is `IDisposable` and owns native handles; construct once, reuse for a batch, dispose in a `using`. Do not share one instance across threads — the native importer is not concurrent.
- a non-supported format must be guarded with `IsImportFormatSupported`/`IsExportFormatSupported` (or matched against `GetSupportedImportFormats`/`GetSupportedExportFormats`) before the call, mapping a miss to `BimFault.CapabilityMiss` rather than catching `AssimpException`.
- wrap every `ImportFile*`/`ExportFile`/`Convert*` in a boundary catch for `AssimpException` and lower it onto `Fin<T>` via `BimFault.CodecReject.ToError()`; the native layer signals failure by throwing.

[IMPORT_NORMALIZATION]:
- the canonical import post-process for the Bim ingest is `Triangulate \| JoinIdenticalVertices \| GenerateSmoothNormals \| CalculateTangentSpace \| GenerateUVCoords` (or `PostProcessPreset.TargetRealTimeQuality`); FBX/Collada arrive Y-up/right-handed, so apply the handedness flip through the per-importer `FrameNormalization` axis, NOT a blanket `MakeLeftHanded`.
- `Scene.RootNode` is the transform root; the world matrix of a `Mesh` is the product of `Node.Transform` up the `Parent` chain — flatten with `PreTransformVertices` only when the per-instance node identity is not needed downstream.
- multi-channel data is array-shaped: `Mesh.TextureCoordinateChannels[set]` and `Mesh.VertexColorChannels[set]`; read `TextureCoordinateChannelCount`/`VertexColorChannelCount` before indexing.

[INTEGRATION_STACK]:
- format-table leg: this context is the `Exchange/format#INTERCHANGE_CODEC` slot for the FBX/Collada/3MF rows ONLY; `Exchange/import.md`'s `BimIo` fold dispatches glTF to `SharpGLTF` (`api-sharpgltf`), PLY to `Ply.Net` (`api-ply-net`), OBJ/STL to `geometry3Sharp` (`api-geometry3sharp`), DWG/DXF to `ACadSharp` (`api-acadsharp`), and FBX/`.dae`/3MF here — one `Detect` row per extension, no overlap.
- canonical-carrier leg: an imported `Assimp.Mesh` (`Vertices`/`Normals`/`Faces` as `List<Vector3>` + index `Face`s) is mapped at the boundary onto the kernel `Rasm` mesh carrier the rest of Bim uses; `AssimpNetter` types never leak past `Exchange/import` — internal code holds canonical shapes per the boundary-mapping law.
- re-export leg: a canonical scene is rebuilt into an `Assimp.Scene` (`RootNode` + `Meshes` + `Materials`) and emitted via `ExportToBlob(scene, "fbx")` for the `Exchange/export#EXPORT_ARTIFACT` FBX/Collada legs; glTF/GLB export stays on `SharpGLTF` so the Draco/meshopt encode (`api-openize-drako`, `api-alimer-meshoptimizer`) stacks on that path, not this one.
- material seam leg: `Material.PBRMaterialProperties` (BaseColor/Metalness/Roughness/Normal slots) projects onto the `Semantics/appearance#APPEARANCE_PROJECTION` host-neutral PBR record, reconciled with the `Rasm.Materials` OpenPBR owner at the content-key seam — the same target `SharpGLTF` material channels also feed.
- identity leg: an `ExportDataBlob` byte payload feeds `System.IO.Hashing` `XxHash3`/`XxHash128` via `Append` (substrate, `api-hashing`) for the `Exchange/export` snapshot content key — `XxHash3` is the fast in-process fingerprint, `XxHash128` (`GetCurrentHashAsUInt128`) the persisted, collision-resistant key the `Rasm.Persistence` artifact index is content-addressed by — so an FBX/Collada export participates in the same XxHash128-keyed content-identity rail as the glTF export.
- metadata leg: `Scene.Metadata`/`Node.Metadata` (`Metadata.Entry.DataAs<T>()`) carries authoring-tool unit + up-axis facts that inform `Exchange/format#FRAME_NORMALIZATION` per-importer coercion before the canonical carrier is built.

[LOCAL_ADMISSION]:
- FBX/Collada/3MF import enters through `AssimpContext.ImportFile*` then maps onto the canonical carrier in `Exchange/import`.
- FBX/Collada export enters through a canonical→`Assimp.Scene` build then `ExportFile`/`ExportToBlob` in `Exchange/export`.
- format capability is resolved once through `GetSupported{Import,Export}Formats` and recorded in the `InterchangeFormat` table; the context is constructed per-ingest and disposed.

[RAIL_LAW]:
- Package: `AssimpNetter`
- Owns: FBX (`.fbx`) + Collada (`.dae`) import/export and the standalone 3MF read leg via the native assimp `Scene`/`Mesh`/`Node`/`Material` model and the `PostProcessSteps` transform algebra
- Accept: scene/mesh exchange for the formats no other Bim codec covers, post-import normalization, PBR-material slot extraction
- Reject: glTF (SharpGLTF), PLY (Ply.Net), OBJ/STL (geometry3Sharp), DWG/DXF (ACadSharp), IFC/STEP semantics (GeometryGym), mesh compression encode (Openize.Drako / Alimer.MeshOptimizer), and leaking `Assimp.*` types past the exchange boundary
