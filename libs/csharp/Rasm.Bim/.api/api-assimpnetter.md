# [RASM_BIM_API_ASSIMPNETTER]

`AssimpNetter` wraps the native Open Asset Import Library, owning scene and mesh exchange for the formats no other Bim codec covers: FBX (`.fbx`) and Collada (`.dae`) import and export with the standalone 3MF read leg. Every call funnels through one disposable `AssimpContext` driving the `Scene`→`Node`→`Mesh`→`Material` model, with `PostProcessSteps` the post-import transform algebra and `PostProcessPreset` its common packagings.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AssimpNetter`
- package: `AssimpNetter` (MIT)
- assembly: `AssimpNetter` (XML doc `AssimpNet.xml`)
- namespace: `Assimp`
- asset: multi-target; the net10.0 consumer binds the `lib/net6.0` asset forward with zero managed dependencies
- native: one osx-arm64 `libassimp.dylib` ships and loads at context construction — RID-coupled, no managed fallback
- rail: scene-exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context and post-processing

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]                          |
| :-----: | :------------------ | :------------- | :------------------------------------ |
|  [01]   | `AssimpContext`     | sealed class   | disposable import/export/convert root |
|  [02]   | `PostProcessSteps`  | flags enum     | post-import transform algebra         |
|  [03]   | `PostProcessPreset` | static class   | preset `PostProcessSteps` bundles     |
|  [04]   | `PropertyConfig`    | abstract class | named importer/exporter config value  |
|  [05]   | `ExcludeComponent`  | flags enum     | component mask for `RemoveComponent`  |

- [POST_PROCESS_STEPS]: `Triangulate` `JoinIdenticalVertices` `GenerateNormals` `GenerateSmoothNormals` `ForceGenerateNormals` `DropNormals` `CalculateTangentSpace` `MakeLeftHanded` `FlipUVs` `FlipWindingOrder` `PreTransformVertices` `GlobalScale` `LimitBoneWeights` `SplitByBoneCount` `Debone` `OptimizeMeshes` `OptimizeGraph` `FindDegenerates` `FindInvalidData` `FindInstances` `RemoveComponent` `RemoveRedundantMaterials` `GenerateUVCoords` `TransformUVCoords` `EmbedTextures` `GenerateBoundingBoxes` `ValidateDataStructure` `ImproveCacheLocality` `SortByPrimitiveType` `SplitLargeMeshes` `FixInFacingNormals`.
- [POST_PROCESS_PRESET]: `ConvertToLeftHanded` `TargetRealTimeFast` `TargetRealTimeQuality` `TargetRealTimeMaximumQuality`.

[PUBLIC_TYPE_SCOPE]: scene graph model

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]         | [CAPABILITY]                                    |
| :-----: | :---------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `Scene`                 | sealed class          | root graph — `RootNode`, content collections    |
|  [02]   | `Node`                  | sealed class          | named transform node, recursive `Children`      |
|  [03]   | `Mesh`                  | sealed class          | vertex/normal/tangent/face geometry             |
|  [04]   | `Face`                  | sealed class          | primitive vertex-index list (`Indices`)         |
|  [05]   | `Bone` / `VertexWeight` | sealed class + struct | skinning offset matrix and per-vertex weight    |
|  [06]   | `Metadata`              | sealed class          | `Dictionary<string, Metadata.Entry>` key-values |
|  [07]   | `BoundingBox`           | record struct         | per-mesh AABB from `GenerateBoundingBoxes`      |

- [SCENE]: `Meshes` `Materials` `Lights` `Cameras` `Textures` `Animations` collections with `Has*`/`*Count`; `FindBone`, `GetEmbeddedTexture`, `Metadata`.
- [MESH]: `Vertices` `Normals` `Tangents` `BiTangents` are `List<Vector3>`; `Bones`, `PrimitiveType`, `MaterialIndex`; `TextureCoordinateChannels[set]`/`VertexColorChannels[set]` are per-set arrays read behind `*ChannelCount`.
- `Node.Transform`: an `Assimp.Matrix4x4` column-vector struct distinct from `System.Numerics.Matrix4x4` — the numerics conversion transposes; `MeshIndices` index `Scene.Meshes` up the `Parent` chain.
- `Metadata.Entry`: `readonly record struct (MetaDataType DataType, object Data)` with typed `DataAs<T>()`; carries authoring-tool unit and up-axis facts.

[PUBLIC_TYPE_SCOPE]: material and texture model

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Material`                  | sealed class  | color slots, `GetMaterialTexture`, `HasProperty` |
|  [02]   | `PBRMaterialProperties`     | nested class  | metallic-roughness `TextureSlot` view            |
|  [03]   | `TextureSlot`               | struct        | file path, `TextureType`, UV, wrap, blend, op    |
|  [04]   | `TextureType`               | enum          | texture-semantic selector                        |
|  [05]   | `EmbeddedTexture`           | sealed class  | in-scene compressed bytes or raw `Texel[]`       |
|  [06]   | `ShadingMode` / `BlendMode` | enum          | classic shading model and blend-mode selectors   |
|  [07]   | `MaterialProperty`          | sealed class  | raw typed property the accessors wrap            |

- [MATERIAL]: `ColorDiffuse`/`ColorAmbient`/`ColorSpecular` (`Vector4`) slots; nested `PBRMaterialProperties` and `ShaderMaterialProperties`.
- [PBR_SLOTS]: `TextureBaseColor` `TextureMetalness` `TextureRoughness` `TextureNormalCamera` `TextureEmissionColor`, each a `TextureSlot` behind a `HasTexture*` guard.

[PUBLIC_TYPE_SCOPE]: animation, format introspection, IO, and logging

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------ | :------------- | :------------------------------------------- |
|  [01]   | `Animation`                                             | sealed class   | TRS and morph channel tracks                 |
|  [02]   | `NodeAnimationChannel`                                  | sealed class   | per-node `VectorKey`/`QuaternionKey` keys    |
|  [03]   | `MeshMorphAnimationChannel` / `MeshAnimationAttachment` | sealed class   | vertex-morph tracks and morph targets        |
|  [04]   | `ExportFormatDescription`                               | sealed class   | `FormatId`, `Description`, `FileExtension`   |
|  [05]   | `ImporterDescription`                                   | sealed class   | name, extensions, `ImporterFeatureFlags`     |
|  [06]   | `IOSystem` / `IOStream`                                 | abstract class | custom VFS byte-source seams                 |
|  [07]   | `LogStream` / `ConsoleLogStream`                        | class          | native-log capture via `LoggingCallback`     |
|  [08]   | `DefaultLogStream`                                      | flags enum     | built-in `File`/`StdOut`/`StdErr`/`Debugger` |
|  [09]   | `ExportDataBlob`                                        | sealed class   | in-memory `Data` (`byte[]`), `Name`, chain   |
|  [10]   | `AssimpException`                                       | class          | native-failure throw the boundary lowers     |

- `ExportDataBlob.NextBlob`: chains to the next `ExportDataBlob` in a multi-format export; `HasData` guards `Data`, `ToStream`/`FromStream` bridge a `Stream`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `AssimpContext` import

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `ImportFile(string, PostProcessSteps) -> Scene`                   | instance | scene from a file path               |
|  [02]   | `ImportFileFromStream(Stream, PostProcessSteps, string) -> Scene` | instance | scene from a stream with format hint |
|  [03]   | `GetSupportedImportFormats() -> string[]`                         | instance | importable extensions                |
|  [04]   | `GetImporterDescriptions() -> ImporterDescription[]`              | instance | per-importer capability descriptors  |
|  [05]   | `GetImporterDescriptionFor(string) -> ImporterDescription`        | instance | importer handling one extension      |
|  [06]   | `IsImportFormatSupported(string) -> bool`                         | instance | guard before import                  |

[ENTRYPOINT_SCOPE]: `AssimpContext` export

`exportFormatId` is one `GetSupportedExportFormats()[i].FormatId` (`fbx`, `collada`, `gltf2`, `glb2`, `obj`, `stl`); the `PostProcessSteps` argument is a pre-export transform applied before serialization.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `ExportFile(Scene, string, string, PostProcessSteps) -> bool`     | instance | scene to a file                    |
|  [02]   | `ExportToBlob(Scene, string, PostProcessSteps) -> ExportDataBlob` | instance | scene to an in-memory blob         |
|  [03]   | `GetSupportedExportFormats() -> ExportFormatDescription[]`        | instance | exportable formats with `FormatId` |
|  [04]   | `IsExportFormatSupported(string) -> bool`                         | instance | guard before export                |

[ENTRYPOINT_SCOPE]: `AssimpContext` convert and configure

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `ConvertFromFileToFile(string, string, string) -> bool`             | instance | file→file transcode              |
|  [02]   | `ConvertFromFileToBlob(string, string) -> ExportDataBlob`           | instance | file→blob transcode              |
|  [03]   | `ConvertFromStreamToFile(Stream, string, string, string) -> bool`   | instance | stream→file transcode            |
|  [04]   | `ConvertFromStreamToBlob(Stream, string, string) -> ExportDataBlob` | instance | stream→blob transcode            |
|  [05]   | `SetConfig(PropertyConfig)`                                         | instance | set an importer/exporter config  |
|  [06]   | `RemoveConfig(string)` / `ContainsConfig(string) -> bool`           | instance | remove or query a config by name |
|  [07]   | `SetIOSystem(IOSystem)` / `RemoveIOSystem()`                        | instance | route IO through a custom VFS    |
|  [08]   | `Scale`, `XAxisRotation`, `YAxisRotation`, `ZAxisRotation`          | property | root transform applied on import |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AssimpContext` owns native handles as `IDisposable`; construct once per ingest, reuse for a batch, dispose in a `using`, and never share one instance across threads.
- Format support resolves through `IsImportFormatSupported`/`IsExportFormatSupported` before the call, mapping a miss to `BimFault.CapabilityMiss`; every `ImportFile*`/`ExportFile`/`Convert*` sits in a boundary catch lowering `AssimpException` onto `Fin<T>` via `BimFault.CodecReject.ToError()`.
- Bim ingest normalizes with `Triangulate | JoinIdenticalVertices | GenerateSmoothNormals | CalculateTangentSpace | GenerateUVCoords`; FBX and Collada arrive Y-up right-handed, so the handedness flip rides the per-importer `FrameNormalization` axis, never a blanket `MakeLeftHanded`.
- A mesh world matrix is the product of `Node.Transform` up the `Parent` chain; `PreTransformVertices` flattens it only where per-instance node identity is unused downstream.

[STACKING]:
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): an `ExportDataBlob.Data` payload folds through `XxHash128` for the `Exchange/export` snapshot content key, joining the same content-identity rail the glTF export keys on.
- `Exchange/import`: an imported `Assimp.Mesh` (`Vertices`/`Normals`/`Faces`) maps at the boundary onto the kernel `Rasm` mesh carrier; `Assimp.*` types never cross the seam.
- `Exchange/export`: a canonical scene rebuilds into an `Assimp.Scene` emitted via `ExportToBlob(scene, "fbx")` for the FBX and Collada legs.
- `Semantics/appearance` + `Rasm.Materials`: `Material.PBRMaterialProperties` slots project onto the host-neutral PBR record reconciled with the OpenPBR owner at the content-key seam.
- `Exchange/format`: `Scene.Metadata`/`Node.Metadata` (`Entry.DataAs<T>()`) feed per-importer `FrameNormalization` unit and up-axis coercion; this context registers the FBX, `.dae`, and 3MF `Detect` rows, one row per extension.

[LOCAL_ADMISSION]:
- FBX/Collada/3MF import enters through `AssimpContext.ImportFile*` then maps onto the canonical carrier in `Exchange/import`; export builds a canonical→`Assimp.Scene` then `ExportFile`/`ExportToBlob` in `Exchange/export`.
- Format capability resolves once through `GetSupported{Import,Export}Formats` into the `InterchangeFormat` table; the context is per-ingest and disposed.

[RAIL_LAW]:
- Package: `AssimpNetter`
- Owns: FBX (`.fbx`) and Collada (`.dae`) import/export with the standalone 3MF read leg over the assimp `Scene`/`Mesh`/`Node`/`Material` model and the `PostProcessSteps` transform algebra.
- Accept: scene and mesh exchange for formats no other Bim codec covers, post-import normalization, PBR-material slot extraction.
- Reject: glTF (`SharpGLTF`), PLY (`Ply.Net`), OBJ/STL (`geometry3Sharp`), DWG/DXF (`ACadSharp`), IFC/STEP semantics (`GeometryGym`), mesh-compression encode (`Openize.Drako`, `Alimer.MeshOptimizer`), and any `Assimp.*` type crossing the exchange boundary.
