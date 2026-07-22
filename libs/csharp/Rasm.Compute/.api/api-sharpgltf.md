# [RASM_COMPUTE_API_SHARPGLTF]

`SharpGLTF` supplies glTF 2.0 schema read/write through `SharpGLTF.Core`, typed scene, mesh, and material builders through `SharpGLTF.Toolkit`, and the 3D-Tiles per-tile `EXT_structural_metadata`/`EXT_mesh_features` extension emitter through `SharpGLTF.Ext.3DTiles`. Compute composes all three: the `TILE_PARTITION` leaf bodies author each tile's metadata and feature schema on the `ModelRoot`, and the tileset.json manifest rides that page's own `Utf8JsonWriter` fold. `Rasm.Bim` owns the runtime scene-decode-plus-instancing surface as its glTF-import rail, never re-derived Compute-side.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpGLTF.Core`
- package: `SharpGLTF.Core` (MIT)
- assembly: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`, `SharpGLTF.Memory`, `SharpGLTF.Validation`, `SharpGLTF.Animations`, `SharpGLTF.IO`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry

[PACKAGE_SURFACE]: `SharpGLTF.Toolkit`
- package: `SharpGLTF.Toolkit` (MIT)
- assembly: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`, `SharpGLTF.Geometry`, `SharpGLTF.Geometry.VertexTypes`, `SharpGLTF.Materials`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry

[PACKAGE_SURFACE]: `SharpGLTF.Ext.3DTiles`
- package: `SharpGLTF.Ext.3DTiles` (MIT)
- assembly: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry
- consumer: direct Compute — csproj `PackageReference` and README `[INTERCHANGE]` row; the per-tile `EXT_structural_metadata`/`EXT_mesh_features` extension schema is Compute's, not Bim's, layered onto Core's `ExtensionsFactory` through `Tiles3DExtensions.RegisterExtensions()`.

[PACKAGE_SURFACE]: `SharpGLTF.Runtime`
- package: `SharpGLTF.Runtime` (MIT)
- assembly: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`, `SharpGLTF`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry
- consumer: transitive through `SharpGLTF.Toolkit`; the runtime scene-instancing surface is `Rasm.Bim/.api/api-sharpgltf.md`'s glTF-import owner. Compute composes only the `MeshDecoder.EvaluateBoundingSphere`/`EvaluateBoundingBox` bounding-volume kernel.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Core Schema2 — model root and I/O contexts

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `ModelRoot`          | class         | glTF root object; owns all logical collections                          |
|  [02]   | `ReadContext`        | class         | reads glTF/GLB from file, stream, or bytes                              |
|  [03]   | `ReadSettings`       | class         | validation policy and URI-resolution options for read                   |
|  [04]   | `WriteContext`       | class         | writes glTF/GLB to file, stream, or callback                            |
|  [05]   | `WriteSettings`      | class         | write policy; members in `[05]-[WRITESET]`                              |
|  [06]   | `ExtensionsFactory`  | class         | global extension registry; `RegisterExtension<TParent,TExt>` adds types |
|  [07]   | `LogicalChildOfRoot` | class         | base for all logical resources; `LogicalParent` walks to root           |

- [05]-[WRITESET]: `WriteSettings` — `MergeBuffers` (default `true`, merges `LogicalBuffers` pre-serialize), `BuffersMaxSize` (merged-chunk byte cap, glTF-only when merging), `JsonIndented`/`JsonOptions` (STJ `JsonWriterOptions`), `ImageWriting` (`ResourceWriteMode`: `BufferView` embeds GLB-native, `EmbeddedAsBase64` embeds glTF-JSON only), `ImageWriteCallback` (per-image `ResourceWriteMode` override), `JsonPostprocessor` (raw-JSON transform pass), `Validation` (`ValidationMode`, read and write).

[PUBLIC_TYPE_SCOPE]: Core Schema2 — scene graph and logical resources

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Scene`                        | class         | root nodes of a scene                               |
|  [02]   | `Node`                         | class         | scene-graph node; carries mesh, skin, TRS, children |
|  [03]   | `Mesh`                         | class         | set of `MeshPrimitive` objects for rendering        |
|  [04]   | `MeshPrimitive`                | class         | geometry + material; carries attribute accessors    |
|  [05]   | `Accessor`                     | class         | typed view into a buffer view; scalar/vec/matrix    |
|  [06]   | `BufferView`                   | class         | contiguous subset of a `Buffer`                     |
|  [07]   | `Buffer`                       | class         | raw binary blob; internal or external URI           |
|  [08]   | `Material`                     | class         | PBR appearance; channels through `FindChannel`      |
|  [09]   | `MaterialChannel`              | struct        | sub-channel carrying texture or parameter values    |
|  [10]   | `MaterialPBRMetallicRoughness` | class         | PBR metallic-roughness parameter block              |
|  [11]   | `Texture`                      | class         | texture + sampler binding                           |
|  [12]   | `TextureSampler`               | class         | wrap and filter modes                               |
|  [13]   | `Image`                        | class         | image data; URI or buffer-view embedded             |
|  [14]   | `Skin`                         | class         | joints and inverse-bind matrices for skeletal mesh  |
|  [15]   | `Animation`                    | class         | keyframe animation; channels + samplers             |
|  [16]   | `AnimationChannel`             | class         | binds an animation sampler to a node property       |
|  [17]   | `AnimationSampler`             | class         | timestamps + interpolated output values             |
|  [18]   | `AnimationChannelTarget`       | class         | descriptor of the animated node + property path     |

[PUBLIC_TYPE_SCOPE]: Core Schema2 — scene-graph extensions

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `MeshGpuInstancing` | class         | KHR_mesh_gpu_instancing extension; instance attrs |
|  [02]   | `PunctualLight`     | class         | KHR_lights_punctual: directional, point, spot     |

[PUBLIC_TYPE_SCOPE]: Core Schema2/Memory — accessors and encoding enums

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `DimensionType`              | enum          | `SCALAR`, `VEC2`, `VEC3`, `VEC4`, `MAT2`, `MAT3`, `MAT4`         |
|  [02]   | `EncodingType`               | enum          | `BYTE`, `UBYTE`, `SHORT`, `USHORT`, `UINT`, `FLOAT`              |
|  [03]   | `IndexEncodingType`          | enum          | `UNSIGNED_BYTE`, `UNSIGNED_SHORT`, `UNSIGNED_INT`                |
|  [04]   | `ResourceWriteMode`          | enum          | `Default`, `SatelliteFile`, `EmbeddedAsBase64`, `BufferView`     |
|  [05]   | `AlphaMode`                  | enum          | `OPAQUE`, `MASK`, `BLEND`                                        |
|  [06]   | `PrimitiveType`              | enum          | `POINTS`, `LINES`, `TRIANGLES`, and the strip/fan forms          |
|  [07]   | `AnimationInterpolationMode` | enum          | `LINEAR`, `STEP`, `CUBICSPLINE`                                  |
|  [08]   | `PropertyPath`               | enum          | animated property: `translation`, `rotation`, `scale`, `weights` |
|  [09]   | `MemoryAccessor`             | class         | wraps a `BufferView` region; projects typed arrays               |
|  [10]   | `MemoryAccessInfo`           | struct        | item format: name, byte offset, stride, format                   |
|  [11]   | `MemoryImage`                | struct        | in-memory image bytes; detects PNG/JPG/KTX2/DDS/WebP             |
|  [12]   | `ScalarArray`                | struct        | `Memory<byte>` view over scalar accessor data                    |
|  [13]   | `Vector2Array`               | struct        | `Memory<byte>` view over Vector2 accessor data                   |
|  [14]   | `Vector3Array`               | struct        | `Memory<byte>` view over Vector3 accessor data                   |
|  [15]   | `Vector4Array`               | struct        | `Memory<byte>` view over Vector4 accessor data                   |
|  [16]   | `QuaternionArray`            | struct        | `Memory<byte>` view over quaternion accessor data                |
|  [17]   | `Matrix4x4Array`             | struct        | `Memory<byte>` view over matrix4x4 accessor data                 |
|  [18]   | `IntegerArray`               | struct        | `Memory<byte>` view over index accessor data                     |

[PUBLIC_TYPE_SCOPE]: Core Memory — typed array views and format descriptors

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `ColorArray`      | struct        | `Memory<byte>` view over color accessor data        |
|  [02]   | `AttributeFormat` | struct        | encode/decode descriptor for vertex attribute bytes |
|  [03]   | `BufferMode`      | enum          | `ARRAY_BUFFER`, `ELEMENT_ARRAY_BUFFER` hints        |
|  [04]   | `CameraType`      | enum          | `PERSPECTIVE`, `ORTHOGRAPHIC`                       |

[PUBLIC_TYPE_SCOPE]: Core Validation

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `ValidationMode`    | enum          | `Skip`, `TryFix`, `Strict` read/write policy      |
|  [02]   | `ValidationContext` | struct        | validation-traversal state carrier                |
|  [03]   | `ModelException`    | class         | base for glTF serialization or validation failure |
|  [04]   | `SchemaException`   | class         | invalid JSON document                             |
|  [05]   | `SemanticException` | class         | invalid semantic values within a valid document   |
|  [06]   | `LinkException`     | class         | invalid inter-object relationships                |
|  [07]   | `DataException`     | class         | invalid binary data                               |

[PUBLIC_TYPE_SCOPE]: Core Schema2 — KHR texture and metadata extensions

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `TextureTransform`   | class         | KHR_texture_transform; UV shift/scale per texture     |
|  [02]   | `XmpPackets`         | class         | KHR_xmp_json_ld model-level XMP metadata packet list  |
|  [03]   | `XmpPacketReference` | class         | KHR_xmp_json_ld per-entity XMP packet index reference |

[PUBLIC_TYPE_SCOPE]: Toolkit — scene and mesh builders

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `SceneBuilder`                       | class         | root scene; holds instances referencing meshes, cameras, lights |
|  [02]   | `NodeBuilder`                        | class         | hierarchical armature node; animatable TRS, scale, rotation     |
|  [03]   | `InstanceBuilder`                    | class         | one renderable instance; content + transform                    |
|  [04]   | `MeshBuilder<TMat,TvG,TvM,TvS>`      | class         | typed mesh builder; owns a `PrimitiveBuilder` per material      |
|  [05]   | `IMeshBuilder<TMat>`                 | interface     | mesh-builder contract for `SceneBuilder.AddRigidMesh`           |
|  [06]   | `PrimitiveBuilder<TMat,TvG,TvM,TvS>` | class         | builds point/line/triangle primitives                           |
|  [07]   | `VertexBuilder<TvG,TvM,TvS>`         | struct        | typed vertex: geometry + material + skinning fragments          |
|  [08]   | `VertexBufferColumns`                | class         | column-per-attribute vertex buffer; transpose layout            |
|  [09]   | `SceneBuilderSchema2Settings`        | struct        | strided buffers, buffer merge, GPU instancing threshold         |

[PUBLIC_TYPE_SCOPE]: Toolkit — vertex geometry and material fragments

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `VertexPosition`              | struct        | position-only geometry fragment                 |
|  [02]   | `VertexPositionNormal`        | struct        | position + normal geometry fragment             |
|  [03]   | `VertexPositionNormalTangent` | struct        | position + normal + tangent geometry fragment   |
|  [04]   | `VertexGeometryDelta`         | struct        | morph-target position/normal/tangent delta      |
|  [05]   | `VertexEmpty`                 | struct        | empty material or skinning fragment placeholder |
|  [06]   | `VertexColor1`                | struct        | 1-color material fragment                       |
|  [07]   | `VertexColor2`                | struct        | 2-color material fragment                       |
|  [08]   | `VertexTexture1`              | struct        | 1-UV material fragment                          |
|  [09]   | `VertexTexture2`              | struct        | 2-UV material fragment                          |
|  [10]   | `VertexColor1Texture1`        | struct        | 1-color + 1-UV material fragment                |
|  [11]   | `VertexColor1Texture2`        | struct        | 1-color + 2-UV material fragment                |
|  [12]   | `VertexColor2Texture1`        | struct        | 2-color + 1-UV material fragment                |
|  [13]   | `VertexColor2Texture2`        | struct        | 2-color + 2-UV material fragment                |
|  [14]   | `VertexMaterialDelta`         | struct        | morph-target color + UV delta                   |
|  [15]   | `VertexJoints4`               | struct        | 4-joint skinning fragment                       |
|  [16]   | `VertexJoints8`               | struct        | 8-joint skinning fragment                       |
|  [17]   | `IVertexGeometry`             | interface     | geometry-fragment contract                      |
|  [18]   | `IVertexMaterial`             | interface     | material-fragment contract                      |

[PUBLIC_TYPE_SCOPE]: Toolkit — vertex fragment interfaces

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :---------------- | :------------ | :--------------------------------- |
|  [01]   | `IVertexSkinning` | interface     | skinning-fragment contract         |
|  [02]   | `IVertexCustom`   | interface     | custom-attribute fragment contract |

[PUBLIC_TYPE_SCOPE]: Toolkit — material and morph builders

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `MaterialBuilder`     | class         | root material; shader, alpha mode, double-sided, fallback             |
|  [02]   | `ChannelBuilder`      | class         | material channel; `TextureBuilder` and scalar parameter values        |
|  [03]   | `TextureBuilder`      | class         | texture reference; primary/fallback images, transform, coord set      |
|  [04]   | `ImageBuilder`        | class         | in-memory image content with optional alternate write file name       |
|  [05]   | `AlphaMode`           | enum          | `Opaque`, `Mask`, `Blend` (Toolkit-local mirror of the Schema2 enum)  |
|  [06]   | `KnownChannel`        | enum          | typed channel key `MaterialBuilder.UseChannel` discriminates on       |
|  [07]   | `KnownProperty`       | enum          | channel parameter key (`BaseColor`, `Metallic`, …)                    |
|  [08]   | `IMorphTargetBuilder` | interface     | per-vertex morph-delta contract                                       |
|  [09]   | `MorphTargetBuilder`  | class         | mesh-level morph target; `SetVertexDelta` by position or geometry key |
|  [10]   | `CameraBuilder`       | class         | perspective or orthographic camera; `ZNear`, `ZFar`, `VerticalFOV`    |
|  [11]   | `LightBuilder`        | class         | directional, point, or spot light; `Color`, `Intensity`, `Range`      |

`MaterialBuilder.UseChannel(KnownChannel)` authors each channel; every non-base value binds its `KHR_materials_<name>` in-box PBR extension by name, and `Diffuse` with `SpecularGlossiness` both key `KHR_materials_pbrSpecularGlossiness`.

[PUBLIC_TYPE_SCOPE]: Ext.3DTiles — structural-metadata and mesh-features (Compute-owned)

Each `*Property` type is the per-property companion of its owning class, table, texture, or attribute; `MeshExtMeshFeatures` carries per-primitive feature ids, `MeshExtInstanceFeatures` per-instance.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `EXTStructuralMetadataRoot` | class         | model-level `EXT_structural_metadata` root; owns the schema + property tables   |
|  [02]   | `StructuralMetadataSchema`  | class         | holds `StructuralMetadataClass` + `StructuralMetadataEnum` definitions          |
|  [03]   | `StructuralMetadataClass`   | class         | a metadata class + its typed `*Property` set (the tile semantic schema)         |
|  [04]   | `StructuralMetadataEnum`    | class         | enumerated metadata value domains (`StructuralMetadataEnumValue` entries)       |
|  [05]   | `PropertyTable`             | class         | per-feature columnar metadata store, feature-id-indexed rows                    |
|  [06]   | `PropertyTexture`           | class         | per-texel metadata encoded in a texture channel                                 |
|  [07]   | `PropertyAttribute`         | class         | per-vertex metadata encoded in a vertex attribute                               |
|  [08]   | `MeshExtMeshFeatures`       | class         | `EXT_mesh_features` per-primitive feature-id sets (vertex attribute or texture) |
|  [09]   | `MeshExtInstanceFeatures`   | class         | per-instance (GPU-instancing) feature ids                                       |
|  [10]   | `IMeshFeatureIDInfo`        | interface     | feature-id descriptor contract the `Add*FeatureIds` take                        |
|  [11]   | `FeatureIDBuilder`          | class         | feature-id descriptor builder                                                   |
|  [12]   | `CesiumPrimitiveOutline`    | class         | Cesium outline extension + the `ExtStructuralMetadataMeshPrimitive` binding     |

[StructuralMetadataClassProperty component enums]: `DataType` `ElementType` `IntegerType` `ArrayOffsetType`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Core ModelRoot and ReadContext — read

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `ModelRoot.Load(string, ReadSettings?)`                 | static   | reads glTF or GLB from a file path          |
|  [02]   | `ModelRoot.ParseGLB(ArraySegment<byte>, ReadSettings?)` | static   | parses GLB from a byte segment              |
|  [03]   | `ModelRoot.ReadGLB(Stream, ReadSettings?)`              | static   | reads GLB from a stream                     |
|  [04]   | `ModelRoot.GetSatellitePaths(string)`                   | static   | satellite file paths for a glTF path        |
|  [05]   | `ReadContext.ReadSchema2(string)`                       | instance | reads a context-relative resource or stream |
|  [06]   | `ReadContext.ReadTextSchema2(Stream)`                   | instance | forces text glTF parse                      |
|  [07]   | `ReadContext.ReadBinarySchema2(Stream)`                 | instance | forces binary GLB parse                     |
|  [08]   | `ReadContext.IdentifyBinaryContainer(Stream) -> bool`   | static   | whether a stream is glTF or GLB             |

[ENTRYPOINT_SCOPE]: Core ModelRoot and WriteContext — write

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `ModelRoot.Save(string, WriteSettings?)`                   | instance | writes glTF or GLB by file extension   |
|  [02]   | `ModelRoot.SaveGLB(string, WriteSettings?)`                | instance | writes binary GLB to file              |
|  [03]   | `ModelRoot.SaveGLTF(string, WriteSettings?)`               | instance | writes text glTF to file               |
|  [04]   | `ModelRoot.WriteGLB(WriteSettings?) -> ArraySegment<byte>` | instance | serializes GLB to a byte segment       |
|  [05]   | `ModelRoot.WriteGLB(Stream, WriteSettings?)`               | instance | writes GLB to a stream                 |
|  [06]   | `WriteContext.WriteTextSchema2(string, ModelRoot)`         | instance | writes text schema to context output   |
|  [07]   | `WriteContext.WriteBinarySchema2(string, ModelRoot)`       | instance | writes binary schema to context output |
|  [08]   | `WriteContext.WriteImage(string, MemoryImage)`             | instance | writes one image to context output     |
|  [09]   | `ModelRoot.GetJSON(bool) -> string`                        | instance | returns full JSON text                 |
|  [10]   | `ModelRoot.GetJsonPreview() -> string`                     | instance | JSON text preview without side effects |

[ENTRYPOINT_SCOPE]: Core ModelRoot — construction and mutation

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `ModelRoot.CreateModel()`                                      | factory  | creates an empty `ModelRoot`               |
|  [02]   | `ModelRoot.DeepClone()`                                        | instance | creates a full structural clone            |
|  [03]   | `ModelRoot.UseScene(string)`                                   | instance | creates or reuses a named or indexed scene |
|  [04]   | `ModelRoot.CreateMesh(string)`                                 | instance | creates a new logical mesh                 |
|  [05]   | `ModelRoot.CreateMaterial(string)`                             | instance | creates a new logical material             |
|  [06]   | `ModelRoot.CreateAccessor(string)`                             | instance | creates a new logical accessor             |
|  [07]   | `ModelRoot.CreateAnimation(string)`                            | instance | creates a new animation                    |
|  [08]   | `ModelRoot.UseBuffer(byte[])`                                  | instance | creates or reuses a buffer                 |
|  [09]   | `ModelRoot.UseBufferView(Buffer, int, int?, int, BufferMode?)` | instance | creates or reuses a buffer view            |
|  [10]   | `ModelRoot.MergeBuffers(int?)`                                 | instance | consolidates logical buffers               |
|  [11]   | `ModelRoot.IsolateMemory()`                                    | instance | refreshes internal memory buffers          |
|  [12]   | `ModelRoot.ApplyBasisTransform(Matrix4x4, string)`             | instance | applies a world transform to all scenes    |
|  [13]   | `ModelRoot.UseImage(MemoryImage)`                              | instance | creates or reuses an image                 |
|  [14]   | `ModelRoot.UseTexture(Image, TextureSampler?)`                 | instance | creates or reuses a texture                |
|  [15]   | `ModelRoot.UseTextureSampler(wrap, filter)`                    | instance | creates or reuses a texture sampler        |
|  [16]   | `ModelRoot.CreateSkin(string)`                                 | instance | creates a new skin                         |
|  [17]   | `ModelRoot.CreatePunctualLight(string, PunctualLightType)`     | instance | creates a KHR punctual light               |

[ENTRYPOINT_SCOPE]: Core Material — string-keyed channel authoring

Core carries no `KnownChannel` (that enum is Toolkit); the public Core PBR-extension surface is the string-keyed channel API on the `MaterialChannel` value struct, the KHR material extension classes staying `internal`.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Material.Channels -> IEnumerable<MaterialChannel>`            | property | all active channels on the material            |
|  [02]   | `Material.FindChannel(string) -> MaterialChannel?`             | instance | resolves one channel by key string             |
|  [03]   | `MaterialChannel.GetFactor(string)`/`SetFactor(string, float)` | instance | channel scalar factor get/set                  |
|  [04]   | `MaterialChannel.SetTexture(int, Image, Image?, …)`            | instance | channel texture set with wrap/filter           |
|  [05]   | `MaterialChannel.SetTransform(Vector2, Vector2, float, int?)`  | instance | per-channel KHR_texture_transform UV transform |

[ENTRYPOINT_SCOPE]: Toolkit SceneBuilder — mesh placement and output

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder)`                  | instance | mesh attached to an animatable node       |
|  [02]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, AffineTransform)`              | instance | mesh at a fixed world transform           |
|  [03]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder, AffineTransform)` | instance | mesh relative to a node                   |
|  [04]   | `SceneBuilder.AddSkinnedMesh(IMeshBuilder<M>, Matrix4x4, NodeBuilder[])`   | instance | skinned mesh with joint armature          |
|  [05]   | `SceneBuilder.AddCamera(CameraBuilder, NodeBuilder)`                       | instance | camera at a node or look-at framing       |
|  [06]   | `SceneBuilder.AddLight(LightBuilder, NodeBuilder)`                         | instance | punctual light at a node or transform     |
|  [07]   | `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings?) -> ModelRoot`          | instance | converts this builder to a `ModelRoot`    |
|  [08]   | `SceneBuilder.ToGltf2(IEnumerable<SceneBuilder>, settings)`                | static   | converts multiple scenes to a `ModelRoot` |
|  [09]   | `SceneBuilder.AddScene(SceneBuilder, Matrix4x4)`                           | instance | merges another scene with an offset       |
|  [10]   | `SceneBuilder.ApplyBasisTransform(Matrix4x4, string)`                      | instance | transforms all instances in this scene    |
|  [11]   | `SceneBuilder.FindArmatures()`                                             | instance | unique armature roots                     |

[ENTRYPOINT_SCOPE]: Toolkit MeshBuilder and PrimitiveBuilder — primitive assembly

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `MeshBuilder.UsePrimitive(material, int)`                           | instance | creates or reuses a primitive for material |
|  [02]   | `MeshBuilder.TransformVertices(Func<VertexBuilder, VertexBuilder>)` | instance | transforms all vertices in place           |
|  [03]   | `PrimitiveBuilder.AddTriangle(v0, v1, v2)`                          | instance | adds a triangle from three typed vertices  |
|  [04]   | `PrimitiveBuilder.AddQuadrangle(v0, v1, v2, v3)`                    | instance | adds a quad, auto-split to two triangles   |
|  [05]   | `PrimitiveBuilder.AddLine(v0, v1)`                                  | instance | adds a line segment                        |
|  [06]   | `PrimitiveBuilder.AddPoint(v0)`                                     | instance | adds a point                               |
|  [07]   | `PrimitiveBuilder.UseVertex(ref VertexBuilder<vG,vM,vS>)`           | instance | adds or reuses a vertex, returns its index |

[ENTRYPOINT_SCOPE]: Toolkit MaterialBuilder — shader and channel configuration
- fluent: every surface returns `MaterialBuilder`, chaining shader selection, channel mutation, and fallback

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `WithMetallicRoughnessShader()`                               | instance | selects PBR metallic-roughness shader    |
|  [02]   | `WithSpecularGlossinessShader()`                              | instance | selects KHR specular-glossiness shader   |
|  [03]   | `WithUnlitShader()`                                           | instance | selects KHR_materials_unlit shader       |
|  [04]   | `WithShader(string)`                                          | instance | selects a shader by name                 |
|  [05]   | `WithDoubleSide(bool)`                                        | instance | enables back-face rendering              |
|  [06]   | `WithAlpha(AlphaMode = OPAQUE, float alphaCutoff)`            | instance | sets alpha mode and mask cutoff          |
|  [07]   | `UseChannel(KnownChannel/string) -> ChannelBuilder`           | instance | gets or creates a channel for mutation   |
|  [08]   | `WithBaseColor(Vector4)` / `(ImageBuilder, Vector4?)`         | instance | sets base-color factor and/or texture    |
|  [09]   | `WithMetallicRoughness(float?, float?)` / `(ImageBuilder, …)` | instance | sets metallic-roughness factors/texture  |
|  [10]   | `WithNormal(ImageBuilder, float)`                             | instance | sets normal-map texture and scale        |
|  [11]   | `WithEmissive(Vector3, float)` / `(ImageBuilder, …)`          | instance | sets emissive factor + KHR strength      |
|  [12]   | `WithChannelParam(KnownChannel, KnownProperty, object)`       | instance | sets one channel scalar/vector parameter |
|  [13]   | `WithChannelImage(KnownChannel, ImageBuilder)`                | instance | sets one channel's primary image         |
|  [14]   | `WithFallback(MaterialBuilder)`                               | instance | chains a fallback material               |

[ENTRYPOINT_SCOPE]: Ext.3DTiles — leaf-body metadata and feature emit
- composition law: `Tiles3DExtensions.RegisterExtensions()` admits the extension types at the `ExtensionsFactory` before any `ModelRoot` write; the leaf-body emit then builds the schema on the model root and binds feature ids per primitive.
- receiver: every `[SURFACE]` is an extension method on `modelRoot`/`primitive`/`node`; `UseStructuralMetadata` returns `EXTStructuralMetadataRoot`, and `Add*FeatureIds` return the bound `MeshExtMeshFeatureID[]`/`MeshExtInstanceFeatureID[]`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `modelRoot.UseStructuralMetadata()`                        | instance | model-level metadata root           |
|  [02]   | `primitive.AddMeshFeatureIds(params IMeshFeatureIDInfo[])` | instance | binds per-primitive feature-id sets |
|  [03]   | `node.AddInstanceFeatureIds(params IMeshFeatureIDInfo[])`  | instance | binds per-instance feature ids      |
|  [04]   | `primitive.AddPropertyTexture(PropertyTexture)`            | instance | attaches per-texel metadata         |
|  [05]   | `primitive.AddPropertyAttribute(PropertyAttribute)`        | instance | attaches per-vertex metadata        |
|  [06]   | `primitive.SetCesiumOutline(IReadOnlyList<uint>, string)`  | instance | writes the Cesium outline extension |
|  [07]   | `primitive.SetCesiumOutline(Accessor)`                     | instance | Cesium outline from an accessor     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- I/O folds through `ModelRoot`: read enters `Load` (file), `ParseGLB` (bytes), or `ReadGLB` (stream); write enters `Save` (format by extension) or `WriteGLB` (bytes); `ReadSettings.Validation` and `WriteSettings.Validation` thread `ValidationMode` at both ends, and a custom URI resolver rides a `ReadContext` file-reader delegate set before `ReadSchema2`. WriteSettings members in `[05]-[WRITESET]`.
- Toolkit build folds `VertexBuilder<TvG,TvM,TvS>` (geometry + material + skinning fragment) → `MeshBuilder` → `SceneBuilder.AddRigidMesh` → `SceneBuilder.ToGltf2()` → `ModelRoot`; `SceneBuilderSchema2Settings` drives strided buffers, buffer merge, and GPU-instancing threshold, `MaterialBuilder` mutates channels through its fluent setters or `UseChannel(KnownChannel)`, and `VertexBufferColumns.CalculateSmoothNormals`/`CalculateTangents` generate normals and tangents.
- Ext.3DTiles emit folds `Tiles3DExtensions.RegisterExtensions()` at the `ExtensionsFactory` before any `ModelRoot` write; the `TILE_PARTITION` leaf bodies then build the `EXT_structural_metadata` schema on the root and bind feature ids per primitive and instance, while the tileset.json manifest rides the codec page's own `Utf8JsonWriter` fold.
- Every extension registers at `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` before any read or write that touches it; the KHR material and texture extensions ship in-box, reached through the public `Material`/`Texture` surface, and a custom extension implements `JsonSerializable` and registers on the same factory.
- Core carries the extension framework but zero geometry codec: no type matches `KHR_draco_mesh_compression` or `EXT_meshopt_compression`, so the encode leg routes to the sibling meshopt codec that rewrites the authored buffer views.

[STACKING]:
- `Alimer.Bindings.MeshOptimizer`(`.api/api-alimer-meshoptimizer.md`): the `ModelRoot` is authored uncompressed, then the `EXT_meshopt_compression` encode leg feeds the authored buffer views through `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` — SharpGLTF owns the schema, the sibling owns the codec.
- `SharpGLTF.Runtime`(`Rasm.Bim/.api/api-sharpgltf.md`): Compute composes only the bounding-volume kernel `MeshDecoder.EvaluateBoundingSphere(scene)` → `(Vector3 Center, float Radius)` and `EvaluateBoundingBox(scene)` → `(Vector3 Min, Vector3 Max)` that `TILE_PARTITION` reads for the `TileNode.BoundingVolume` octree bound; a hand-rolled vertex AABB sweep over the decoded mesh is the deleted form, and the runtime scene-instancing surface stays Bim's.
- within-lib: `TILE_PARTITION` emits its leaf bodies through Core's `ExtensionsFactory.RegisterExtension` write surface, Ext.3DTiles' `Tiles3DExtensions.RegisterExtensions()` metadata and feature schema, and the Toolkit `SceneBuilder`/`MeshBuilder` author path.

[LOCAL_ADMISSION]:
- Geometry export enters `SceneBuilder.ToGltf2()` → `ModelRoot.Save*`/`WriteGLB`; import enters `ModelRoot.Load*` or `ReadContext.ReadSchema2`.
- Extension admission registers at `ExtensionsFactory` before any read or write that uses it; Ext.3DTiles metadata and feature emit registers through `Tiles3DExtensions.RegisterExtensions()`.

[RAIL_LAW]:
- Packages: `SharpGLTF.Core`, `SharpGLTF.Toolkit`, `SharpGLTF.Ext.3DTiles` (direct Compute); `SharpGLTF.Runtime` (transitive, Bim-owned)
- Owns: glTF 2.0 read/write, typed mesh and material building, and the 3D-Tiles per-tile `EXT_structural_metadata`/`EXT_mesh_features` metadata and feature emit
- Accept: geometry exchange, asset authoring, and leaf-body metadata and feature authoring
- Reject: rendering pipeline, GPU resource management, image decode, and the runtime scene-instancing surface
