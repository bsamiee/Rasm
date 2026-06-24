# [RASM_BIM_API_SHARPGLTF]

`SharpGLTF` supplies glTF 2.0 schema read/write (`SharpGLTF.Core`), high-level
scene/mesh/material builders (`SharpGLTF.Toolkit`), and runtime decode/instancing
for game-engine integration (`SharpGLTF.Runtime`) across three coordinated packages.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpGLTF.Core`
- package: `SharpGLTF.Core`
- assembly: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- namespace: `SharpGLTF.Memory`
- namespace: `SharpGLTF.Validation`
- namespace: `SharpGLTF.Animations`
- namespace: `SharpGLTF.IO`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0
- rail: geometry

[PACKAGE_SURFACE]: `SharpGLTF.Toolkit`
- package: `SharpGLTF.Toolkit`
- assembly: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`
- namespace: `SharpGLTF.Geometry`
- namespace: `SharpGLTF.Geometry.VertexTypes`
- namespace: `SharpGLTF.Materials`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0
- rail: geometry

[PACKAGE_SURFACE]: `SharpGLTF.Runtime`
- package: `SharpGLTF.Runtime`
- assembly: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- namespace: `SharpGLTF`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Schema2 — model root and I/O contexts
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]             | [RAIL]   | [CAPABILITY]                                                            |
| :-----: | :------------------- | :------- | :---------------------------------------------------------------------- |
|  [01]   | `ModelRoot`          | geometry | root object for a glTF asset; owns all logical collections              |
|  [02]   | `ReadContext`        | geometry | context for reading glTF/GLB from file, stream, or bytes                |
|  [03]   | `ReadSettings`       | geometry | validation policy and URI-resolution options for read                   |
|  [04]   | `WriteContext`       | geometry | context for writing glTF/GLB to file, stream, or callback               |
|  [05]   | `WriteSettings`      | geometry | validation and buffer-merge options for write                           |
|  [06]   | `ExtensionsFactory`  | geometry | global extension registry; `RegisterExtension<TParent,TExt>` adds types |
|  [07]   | `LogicalChildOfRoot` | geometry | base for all logical resources (mesh, material, accessor, etc.)         |

[PUBLIC_TYPE_SCOPE]: Schema2 — scene graph and logical resources
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]           | [RAIL]   | [CAPABILITY]                                                                                                           |
| :-----: | :----------------- | :------- | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Scene`            | geometry | root nodes of a scene                                                                                                  |
|  [02]   | `Node`             | geometry | scene-graph node; carries mesh, skin, TRS, children                                                                    |
|  [03]   | `Mesh`             | geometry | set of `MeshPrimitive` objects for rendering                                                                           |
|  [04]   | `MeshPrimitive`    | geometry | geometry + material; carries attribute accessors                                                                       |
|  [05]   | `Accessor`         | geometry | typed view into a buffer view; scalar/vec/matrix                                                                       |
|  [06]   | `BufferView`       | geometry | contiguous subset of a `Buffer`                                                                                        |
|  [07]   | `Buffer`           | geometry | raw binary blob; internal or external URI                                                                              |
|  [08]   | `Material`         | geometry | material appearance; PBR metallic-roughness and channel parameters reached through `FindChannel`                       |
|  [09]   | `MaterialChannel`  | geometry | `readonly struct` channel projection; carries texture or parameter values                                              |
|  [10]   | `Texture`          | geometry | texture + sampler binding                                                                                              |
|  [11]   | `TextureSampler`   | geometry | wrap and filter modes                                                                                                  |
|  [12]   | `Image`            | geometry | image data; URI or buffer-view embedded                                                                                |
|  [13]   | `Skin`             | geometry | joints and inverse-bind matrices for skeletal mesh                                                                     |
|  [14]   | `Animation`        | geometry | keyframe animation; owns channels and per-channel samplers                                                             |
|  [15]   | `AnimationChannel` | geometry | binds an animation sampler to a node property; sampler keyframes and the channel target are reached through this owner |

[PUBLIC_TYPE_SCOPE]: Schema2 — scene graph extensions
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]            | [RAIL]   | [CAPABILITY]                                      |
| :-----: | :------------------ | :------- | :------------------------------------------------ |
|  [01]   | `MeshGpuInstancing` | geometry | KHR_mesh_gpu_instancing extension; instance attrs |
|  [02]   | `PunctualLight`     | geometry | KHR_lights_punctual: directional, point, spot     |

[PUBLIC_TYPE_SCOPE]: Schema2 — encoding enums and accessor descriptors
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`, `SharpGLTF.Memory`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                     |
| :-----: | :--------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `DimensionType`              | geometry | `SCALAR`, `VEC2`, `VEC3`, `VEC4`, `MAT2`, `MAT3`, `MAT4`         |
|  [02]   | `EncodingType`               | geometry | `BYTE`, `UBYTE`, `SHORT`, `USHORT`, `UINT`, `FLOAT`              |
|  [03]   | `IndexEncodingType`          | geometry | `UNSIGNED_BYTE`, `UNSIGNED_SHORT`, `UNSIGNED_INT`                |
|  [04]   | `ResourceWriteMode`          | geometry | `Default`, `SatelliteFile`, `EmbeddedAsBase64`, `BufferView`     |
|  [05]   | `AlphaMode`                  | geometry | `OPAQUE`, `MASK`, `BLEND`                                        |
|  [06]   | `PrimitiveType`              | geometry | `POINTS`, `LINES`, `TRIANGLES`, etc.                             |
|  [07]   | `AnimationInterpolationMode` | geometry | `LINEAR`, `STEP`, `CUBICSPLINE`                                  |
|  [08]   | `PropertyPath`               | geometry | animated property: `translation`, `rotation`, `scale`, `weights` |
|  [09]   | `MemoryAccessor`             | geometry | wraps a `BufferView` memory region; projects typed arrays        |
|  [10]   | `MemoryAccessInfo`           | geometry | describes item format: name, byte offset, stride, format         |
|  [11]   | `MemoryImage`                | geometry | in-memory image bytes; detects PNG/JPG/KTX2/DDS/WebP             |
|  [12]   | `AttributeFormat`            | geometry | encoding/decoding descriptor for vertex attribute bytes          |
|  [13]   | `BufferMode`                 | geometry | `ARRAY_BUFFER`, `ELEMENT_ARRAY_BUFFER` hints                     |
|  [14]   | `CameraType`                 | geometry | `PERSPECTIVE`, `ORTHOGRAPHIC`                                    |

[PUBLIC_TYPE_SCOPE]: Schema2 — typed memory array views
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Memory`
- rail: geometry

| [INDEX] | [SYMBOL]          | [RAIL]   | [CAPABILITY]                                            |
| :-----: | :---------------- | :------- | :------------------------------------------------------ |
|  [01]   | `ScalarArray`     | geometry | typed `Memory<byte>` view over scalar accessor data     |
|  [02]   | `Vector2Array`    | geometry | typed `Memory<byte>` view over Vector2 accessor data    |
|  [03]   | `Vector3Array`    | geometry | typed `Memory<byte>` view over Vector3 accessor data    |
|  [04]   | `Vector4Array`    | geometry | typed `Memory<byte>` view over Vector4 accessor data    |
|  [05]   | `QuaternionArray` | geometry | typed `Memory<byte>` view over quaternion accessor data |
|  [06]   | `Matrix4x4Array`  | geometry | typed `Memory<byte>` view over matrix4x4 accessor data  |
|  [07]   | `IntegerArray`    | geometry | typed `Memory<byte>` view over index accessor data      |
|  [08]   | `ColorArray`      | geometry | typed `Memory<byte>` view over color accessor data      |

[PUBLIC_TYPE_SCOPE]: Schema2 — validation
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Validation`
- rail: geometry

| [INDEX] | [SYMBOL]            | [RAIL]   | [CAPABILITY]                                                           |
| :-----: | :------------------ | :------- | :--------------------------------------------------------------------- |
|  [01]   | `ValidationMode`    | geometry | `Skip`, `TryFix`, `Strict` — controls validation policy for read/write |
|  [02]   | `ValidationContext` | geometry | utility class used during model validation traversal                   |
|  [03]   | `ModelException`    | geometry | base exception from glTF serialization or validation                   |
|  [04]   | `SchemaException`   | geometry | invalid JSON document                                                  |
|  [05]   | `SemanticException` | geometry | invalid semantic values within a valid document                        |
|  [06]   | `LinkException`     | geometry | invalid inter-object relationships                                     |
|  [07]   | `DataException`     | geometry | invalid binary data                                                    |

[INBOX_EXTENSION_SCOPE]: Schema2 — KHR material extensions (PBR and shading)
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- access: `internal` extension classes serialized in-box; authored and read through the public `Material` and `MaterialChannel` surface, never named directly
- rail: geometry

| [INDEX] | [EXTENSION]                           | [RAIL]   | [CAPABILITY]                  |
| :-----: | :------------------------------------ | :------- | :---------------------------- |
|  [01]   | `KHR_materials_unlit`                 | geometry | unlit shading                 |
|  [02]   | `KHR_materials_clearcoat`             | geometry | clear-coat layer              |
|  [03]   | `KHR_materials_transmission`          | geometry | optical transmission          |
|  [04]   | `KHR_materials_volume`                | geometry | sub-surface volume            |
|  [05]   | `KHR_materials_specular`              | geometry | specular reflectance strength |
|  [06]   | `KHR_materials_ior`                   | geometry | index of refraction           |
|  [07]   | `KHR_materials_iridescence`           | geometry | thin-film iridescence         |
|  [08]   | `KHR_materials_sheen`                 | geometry | fabric sheen layer            |
|  [09]   | `KHR_materials_anisotropy`            | geometry | anisotropic reflections       |
|  [10]   | `KHR_materials_emissive_strength`     | geometry | HDR emissive scale            |
|  [11]   | `KHR_materials_dispersion`            | geometry | spectral dispersion           |
|  [12]   | `KHR_materials_diffuse_transmission`  | geometry | diffuse transmission          |
|  [13]   | `KHR_materials_pbrSpecularGlossiness` | geometry | specular-gloss model          |

[PUBLIC_TYPE_SCOPE]: Schema2 — KHR texture and metadata extensions
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]             | [RAIL]   | [CAPABILITY]                                              |
| :-----: | :------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `TextureTransform`   | geometry | KHR_texture_transform; public; UV shift/scale per texture |
|  [02]   | `XmpPackets`         | geometry | KHR_xmp_json_ld model-level XMP metadata packet list      |
|  [03]   | `XmpPacketReference` | geometry | KHR_xmp_json_ld per-entity XMP packet index reference     |

[INBOX_EXTENSION_SCOPE]: Schema2 — KHR/MSFT/EXT texture and animation extensions
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- access: `internal` extension classes serialized in-box; reached through the public `Texture`, `TextureSampler`, and `Animation` surface, never named directly
- rail: geometry

| [INDEX] | [EXTENSION]             | [RAIL]   | [CAPABILITY]                  |
| :-----: | :---------------------- | :------- | :---------------------------- |
|  [01]   | `KHR_texture_basisu`    | geometry | KTX2/Basis compressed texture |
|  [02]   | `MSFT_texture_dds`      | geometry | DirectDraw Surface texture    |
|  [03]   | `EXT_texture_webp`      | geometry | WebP texture                  |
|  [04]   | `KHR_animation_pointer` | geometry | JSON-pointer animation target |

[PUBLIC_TYPE_SCOPE]: Toolkit — scene and mesh builders
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`, `SharpGLTF.Geometry`
- rail: geometry

| [INDEX] | [SYMBOL]                             | [RAIL]   | [CAPABILITY]                                                          |
| :-----: | :----------------------------------- | :------- | :-------------------------------------------------------------------- |
|  [01]   | `SceneBuilder`                       | geometry | root scene; holds instances referencing meshes, cameras, lights       |
|  [02]   | `NodeBuilder`                        | geometry | hierarchical armature node; carries animatable TRS, scale, rotation   |
|  [03]   | `InstanceBuilder`                    | geometry | one renderable instance in the scene; content + transform             |
|  [04]   | `MeshBuilder<TMat,TvG,TvM,TvS>`      | geometry | typed mesh builder; owns `PrimitiveBuilder` per material              |
|  [05]   | `IMeshBuilder<TMat>`                 | geometry | interface for mesh builders used by `SceneBuilder.AddRigidMesh`       |
|  [06]   | `PrimitiveBuilder<TMat,TvG,TvM,TvS>` | geometry | builds point/line/triangle primitives; `AddTriangle`, `AddQuadrangle` |
|  [07]   | `VertexBuilder<TvG,TvM,TvS>`         | geometry | typed vertex struct: geometry + material + skinning fragments         |
|  [08]   | `VertexBufferColumns`                | geometry | column-per-attribute vertex buffer; transpose layout                  |
|  [09]   | `SceneBuilderSchema2Settings`        | geometry | conversion options: strided buffers, merge, GPU instancing threshold  |

`PackedMeshBuilder<TMat>` is `internal`; it packs `IMeshBuilder` collections into a Schema2 mesh during `ToGltf2` and is never named by a consumer.

[PUBLIC_TYPE_SCOPE]: Toolkit — vertex geometry and material fragments
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Geometry.VertexTypes`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [RAIL]   | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `VertexPosition`              | geometry | position-only geometry fragment                 |
|  [02]   | `VertexPositionNormal`        | geometry | position + normal geometry fragment             |
|  [03]   | `VertexPositionNormalTangent` | geometry | position + normal + tangent geometry fragment   |
|  [04]   | `VertexGeometryDelta`         | geometry | morph-target position/normal/tangent delta      |
|  [05]   | `VertexEmpty`                 | geometry | empty material or skinning fragment placeholder |
|  [06]   | `VertexColor1`                | geometry | 1-color material fragment                       |
|  [07]   | `VertexColor2`                | geometry | 2-color material fragment                       |
|  [08]   | `VertexTexture1`              | geometry | 1-UV material fragment                          |
|  [09]   | `VertexTexture2`              | geometry | 2-UV material fragment                          |
|  [10]   | `VertexColor1Texture1`        | geometry | 1-color + 1-UV material fragment                |
|  [11]   | `VertexColor1Texture2`        | geometry | 1-color + 2-UV material fragment                |
|  [12]   | `VertexColor2Texture1`        | geometry | 2-color + 1-UV material fragment                |
|  [13]   | `VertexColor2Texture2`        | geometry | 2-color + 2-UV material fragment                |
|  [14]   | `VertexMaterialDelta`         | geometry | morph-target color + UV delta                   |
|  [15]   | `VertexJoints4`               | geometry | 4-joint skinning fragment                       |
|  [16]   | `VertexJoints8`               | geometry | 8-joint skinning fragment                       |

[PUBLIC_TYPE_SCOPE]: Toolkit — vertex fragment interfaces
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Geometry.VertexTypes`
- rail: geometry

| [INDEX] | [SYMBOL]          | [RAIL]   | [CAPABILITY]                             |
| :-----: | :---------------- | :------- | :--------------------------------------- |
|  [01]   | `IVertexGeometry` | geometry | interface for geometry fragments         |
|  [02]   | `IVertexMaterial` | geometry | interface for material fragments         |
|  [03]   | `IVertexSkinning` | geometry | interface for skinning fragments         |
|  [04]   | `IVertexCustom`   | geometry | interface for custom attribute fragments |

[PUBLIC_TYPE_SCOPE]: Toolkit — material and morph builders
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Materials`, `SharpGLTF.Geometry`
- rail: geometry

| [INDEX] | [SYMBOL]              | [RAIL]   | [CAPABILITY]                                                                                                   |
| :-----: | :-------------------- | :------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `MaterialBuilder`     | geometry | root material; sets shader, alpha mode, double-sided, fallback                                                 |
|  [02]   | `ChannelBuilder`      | geometry | material channel; holds `TextureBuilder` and scalar parameter values                                           |
|  [03]   | `TextureBuilder`      | geometry | texture reference; primary + fallback images, transform, coord set                                             |
|  [04]   | `ImageBuilder`        | geometry | in-memory image content with optional alternate write file name                                                |
|  [05]   | `AlphaMode`           | geometry | `Opaque`, `Mask`, `Blend` (Toolkit-local mirror of Schema2 enum)                                               |
|  [06]   | `KnownProperty`       | geometry | enumeration of channel parameter keys (BaseColor, Metallic, etc.)                                              |
|  [07]   | `IMorphTargetBuilder` | geometry | interface for setting per-vertex morph target deltas; the per-primitive implementation behind it is `internal` |
|  [08]   | `MorphTargetBuilder`  | geometry | mesh-level morph target; `SetVertexDelta` by position or geometry key                                          |
|  [09]   | `CameraBuilder`       | geometry | perspective or orthographic camera; `ZNear`, `ZFar`, `VerticalFOV`                                             |
|  [10]   | `LightBuilder`        | geometry | directional, point, or spot light with `Color`, `Intensity`, `Range`                                           |

[PUBLIC_TYPE_SCOPE]: Runtime — scene template and instancing
- package: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- rail: geometry

The template types (`ArmatureTemplate`, `NodeTemplate`, `DrawableTemplate` and its rigid/skinned subtypes, `MaterialTemplate`) are `internal`; a consumer reaches the templatized scene only through `SceneTemplate` and drives per-instance state through the public instance types below. `DrawableInstance.Template` exposes the internal drawable through the public `IDrawableTemplate` contract.

| [INDEX] | [SYMBOL]           | [RAIL]   | [CAPABILITY]                                                                |
| :-----: | :----------------- | :------- | :-------------------------------------------------------------------------- |
|  [01]   | `SceneTemplate`    | geometry | templatized scene from a `Schema2.Scene`; creates `SceneInstance` copies    |
|  [02]   | `SceneInstance`    | geometry | independent mutable state of a `SceneTemplate`; owns `ArmatureInstance`     |
|  [03]   | `ArmatureInstance` | geometry | per-instance bone transform state; `SetAnimationFrame`, `SetPoseTransforms` |
|  [04]   | `NodeInstance`     | geometry | per-instance node transform state; `LocalMatrix`, `ModelMatrix`             |
|  [05]   | `DrawableInstance` | geometry | struct: `Template` (what) + `Transform` (where) + `InstanceCount`           |
|  [06]   | `RuntimeOptions`   | geometry | `IsolateMemory`, `GpuMeshInstancing`, `ExtrasConverterCallback`             |

[PUBLIC_TYPE_SCOPE]: Runtime — mesh decode contracts
- package: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [RAIL]   | [CAPABILITY]                                                             |
| :-----: | :---------------------------- | :------- | :----------------------------------------------------------------------- |
|  [01]   | `IMeshDecoder<TMat>`          | geometry | mesh decode interface; name, extras, logical index, primitives           |
|  [02]   | `IMeshPrimitiveDecoder`       | geometry | primitive decode interface; positions, normals, UVs, colors, skin        |
|  [03]   | `IMeshPrimitiveDecoder<TMat>` | geometry | typed variant carrying material reference                                |
|  [04]   | `MeshDecoder`                 | geometry | static utility; `Decode()` extension on `Mesh` and `IReadOnlyList<Mesh>` |

`VertexNormalsFactory` and `VertexTangentsFactory` are `internal` static kernels (smooth-normal and MikkTSpace-tangent generation); they run inside the decode path and are not consumer-callable.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ModelRoot — read
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                          | [CAPABILITY]                                 |
| :-----: | :------------------------------------ | :------------------------------------ | :------------------------------------------- |
|  [01]   | `ModelRoot.Load`                      | `(string, ReadSettings?)`             | reads glTF or GLB from file path             |
|  [02]   | `ModelRoot.ParseGLB`                  | `(ArraySegment<byte>, ReadSettings?)` | parses GLB from byte array                   |
|  [03]   | `ModelRoot.ReadGLB`                   | `(Stream, ReadSettings?)`             | reads GLB from a stream                      |
|  [04]   | `ReadContext.ReadSchema2`             | `(string)` or `(Stream)`              | reads from context-relative file or stream   |
|  [05]   | `ReadContext.ReadTextSchema2`         | `(Stream)`                            | forces text glTF parse                       |
|  [06]   | `ReadContext.ReadBinarySchema2`       | `(Stream)`                            | forces binary GLB parse                      |
|  [07]   | `ReadContext.IdentifyBinaryContainer` | `(Stream)`                            | returns whether stream is glTF or GLB        |
|  [08]   | `ModelRoot.GetSatellitePaths`         | `(string)`                            | returns satellite file paths for a glTF path |

[ENTRYPOINT_SCOPE]: ModelRoot — write
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                              | [CAPABILITY]                                   |
| :-----: | :-------------------------------- | :---------------------------------------- | :--------------------------------------------- |
|  [01]   | `ModelRoot.Save`                  | `(string, WriteSettings?)`                | writes glTF or GLB by file extension           |
|  [02]   | `ModelRoot.SaveGLB`               | `(string, WriteSettings?)`                | writes binary GLB to file                      |
|  [03]   | `ModelRoot.SaveGLTF`              | `(string, WriteSettings?)`                | writes text glTF to file                       |
|  [04]   | `ModelRoot.WriteGLB`              | `(WriteSettings?)` → `ArraySegment<byte>` | serializes GLB to a byte segment               |
|  [05]   | `ModelRoot.WriteGLB`              | `(Stream, WriteSettings?)`                | writes GLB to stream                           |
|  [06]   | `WriteContext.WriteTextSchema2`   | `(string, ModelRoot)`                     | writes text schema to context output           |
|  [07]   | `WriteContext.WriteBinarySchema2` | `(string, ModelRoot)`                     | writes binary schema to context output         |
|  [08]   | `ModelRoot.GetJsonPreview`        | `()`                                      | returns JSON text preview without side effects |

[ENTRYPOINT_SCOPE]: ModelRoot — construction and mutation
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                  | [CAPABILITY]                               |
| :-----: | :------------------------------ | :---------------------------- | :----------------------------------------- |
|  [01]   | `ModelRoot.CreateModel`         | `()` (static)                 | creates an empty `ModelRoot`               |
|  [02]   | `ModelRoot.DeepClone`           | `()`                          | creates a full structural clone            |
|  [03]   | `ModelRoot.UseScene`            | `(string)` or `(int)`         | creates or reuses a named scene            |
|  [04]   | `ModelRoot.CreateMesh`          | `(string)`                    | creates a new logical mesh                 |
|  [05]   | `ModelRoot.CreateMaterial`      | `(string)`                    | creates a new logical material             |
|  [06]   | `ModelRoot.CreateAccessor`      | `(string)`                    | creates a new logical accessor             |
|  [07]   | `ModelRoot.CreateAnimation`     | `(string)`                    | creates a new animation                    |
|  [08]   | `ModelRoot.UseBuffer`           | `(byte[])`                    | creates or reuses a buffer from byte array |
|  [09]   | `ModelRoot.UseBufferView`       | overloads                     | creates or reuses a buffer view            |
|  [10]   | `ModelRoot.MergeBuffers`        | `()` or `(int)`               | consolidates logical buffers               |
|  [11]   | `ModelRoot.IsolateMemory`       | `()`                          | refreshes internal memory buffers          |
|  [12]   | `ModelRoot.ApplyBasisTransform` | `(Matrix4x4, string)`         | applies world transform to all scenes      |
|  [13]   | `ModelRoot.UseImage`            | `(MemoryImage)`               | creates or reuses an image                 |
|  [14]   | `ModelRoot.UseTexture`          | `(Image, TextureSampler)`     | creates or reuses a texture                |
|  [15]   | `ModelRoot.UseTextureSampler`   | wrap/filter args              | creates or reuses a texture sampler        |
|  [16]   | `ModelRoot.CreateSkin`          | `(string)`                    | creates a new skin                         |
|  [17]   | `ModelRoot.CreatePunctualLight` | `(string, PunctualLightType)` | creates a KHR punctual light               |

[ENTRYPOINT_SCOPE]: Animation — keyframe channel authoring
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

`ModelRoot.CreateAnimation(name)` returns the `Animation` added to `LogicalAnimations`; the per-node keyframe channels below each allocate their own `AnimationSampler` (the `bool linear` selects `AnimationInterpolationMode.LINEAR`/`STEP`, the tuple overloads force `CUBICSPLINE`). The keyframe argument is the float-seconds → value map, so the time axis is supplied by the caller. `CreateVisibilityChannel` emits the `KHR_node_visibility` per-node boolean track and is `STEP`-interpolated by construction (no `linear` parameter); the extension is authored through this channel, never a named `JsonSerializable` class — so its `KhrExtension` row carries `Registrar=None`.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                              | [CAPABILITY]                                              |
| :-----: | :----------------------------------------- | :----------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Animation.CreateVisibilityChannel`        | `(Node, IReadOnlyDictionary<float, bool>)`                               | `KHR_node_visibility` per-node visibility, `STEP`         |
|  [02]   | `Animation.CreateScaleChannel`             | `(Node, IReadOnlyDictionary<float, Vector3>, bool linear = true)`        | per-node scale TRS track                                  |
|  [03]   | `Animation.CreateTranslationChannel`       | `(Node, IReadOnlyDictionary<float, Vector3>, bool linear = true)`        | per-node translation TRS track                            |
|  [04]   | `Animation.CreateRotationChannel`          | `(Node, IReadOnlyDictionary<float, Quaternion>, bool linear = true)`     | per-node rotation TRS track                               |
|  [05]   | `Animation.CreateMorphChannel`             | `(Node, IReadOnlyDictionary<float, TWeights>, int morphCount, bool)`     | per-node morph-weight track                               |
|  [06]   | `Animation.CreateMaterialPropertyChannel`  | `(Material, string propertyName, IReadOnlyDictionary<float, T>, bool)`   | `KHR_animation_pointer` material-channel track            |
|  [07]   | `Animation.DangerousCreatePointerChannel`  | `(string pointerPath, IReadOnlyDictionary<float, T>, bool, bool)`        | `KHR_animation_pointer` arbitrary-DOM target track        |

The scale/translation/rotation/morph channels each carry a second `(TangentIn, Value, TangentOut)` tuple-keyframe overload forcing `CUBICSPLINE` interpolation. `KHR_node_visibility` and `KHR_animation_pointer` are the in-box scene-rail extensions reached only through these channel members, never named — consistent with the `internal`-extension policy the material/texture rows above hold.

[ENTRYPOINT_SCOPE]: SceneBuilder — mesh placement and output
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`
- rail: geometry

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                      | [CAPABILITY]                               |
| :-----: | :--------------------------------- | :------------------------------------------------ | :----------------------------------------- |
|  [01]   | `SceneBuilder.AddRigidMesh`        | `(IMeshBuilder<M>, NodeBuilder)`                  | adds mesh attached to animatable node      |
|  [02]   | `SceneBuilder.AddRigidMesh`        | `(IMeshBuilder<M>, AffineTransform)`              | adds mesh at fixed world transform         |
|  [03]   | `SceneBuilder.AddRigidMesh`        | `(IMeshBuilder<M>, NodeBuilder, AffineTransform)` | adds mesh relative to node                 |
|  [04]   | `SceneBuilder.ToGltf2`             | `()` or `(SceneBuilderSchema2Settings)`           | converts builder to `ModelRoot`            |
|  [05]   | `SceneBuilder.ToGltf2`             | `(IEnumerable<SceneBuilder>, settings)`           | converts multiple scenes to `ModelRoot`    |
|  [06]   | `SceneBuilder.AddScene`            | `(SceneBuilder, Matrix4x4)`                       | merges another scene with offset transform |
|  [07]   | `SceneBuilder.ApplyBasisTransform` | `(Matrix4x4, string)`                             | transforms all instances in this scene     |
|  [08]   | `SceneBuilder.FindArmatures`       | `()`                                              | returns unique armature roots              |

[ENTRYPOINT_SCOPE]: MeshBuilder — primitive assembly
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Geometry`
- rail: geometry

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                           | [CAPABILITY]                               |
| :-----: | :------------------------------------- | :------------------------------------- | :----------------------------------------- |
|  [01]   | `MeshBuilder<M,vG,vM,vS>.UsePrimitive` | `(material, verticesPerPrimitive)`     | creates or reuses a primitive for material |
|  [02]   | `MeshBuilder.TransformVertices`        | `(Func<VertexBuilder, VertexBuilder>)` | transforms all vertices in place           |
|  [03]   | `PrimitiveBuilder.AddTriangle`         | `(v0, v1, v2)`                         | adds a triangle from three typed vertices  |
|  [04]   | `PrimitiveBuilder.AddQuadrangle`       | `(v0, v1, v2, v3)`                     | adds a quad, auto-split to two triangles   |
|  [05]   | `PrimitiveBuilder.AddLine`             | `(v0, v1)`                             | adds a line segment                        |
|  [06]   | `PrimitiveBuilder.AddPoint`            | `(v0)`                                 | adds a point                               |

[ENTRYPOINT_SCOPE]: MaterialBuilder — shader and channel configuration
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Materials`
- rail: geometry

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]        | [CAPABILITY]                           |
| :-----: | :--------------------------------------------- | :------------------ | :------------------------------------- |
|  [01]   | `MaterialBuilder.WithMetallicRoughnessShader`  | `()`                | selects PBR metallic-roughness shader  |
|  [02]   | `MaterialBuilder.WithSpecularGlossinessShader` | `()`                | selects KHR specular-glossiness shader |
|  [03]   | `MaterialBuilder.WithUnlitShader`              | `()`                | selects KHR_materials_unlit shader     |
|  [04]   | `MaterialBuilder.WithShader`                   | `(string)`          | selects shader by name string          |
|  [05]   | `MaterialBuilder.WithFallback`                 | `(MaterialBuilder)` | chains a fallback material             |
|  [06]   | `MaterialBuilder.DoubleSided`                  | `bool` property     | enables back-face rendering            |

[ENTRYPOINT_SCOPE]: SceneTemplate — runtime decode
- package: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- rail: geometry

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                              | [CAPABILITY]                                       |
| :-----: | :-------------------------------------- | :---------------------------------------- | :------------------------------------------------- |
|  [01]   | `SceneTemplate.Create`                  | `(Scene, RuntimeOptions?)` (static)       | creates template from a `Schema2.Scene`            |
|  [02]   | `SceneTemplate.CreateInstance`          | `()`                                      | creates an independent `SceneInstance`             |
|  [03]   | `ArmatureInstance.SetAnimationFrame`    | `(int trackIndex, float time, bool loop)` | advances bone transforms to animation time         |
|  [04]   | `ArmatureInstance.SetPoseTransforms`    | `()`                                      | resets all bones to rest pose                      |
|  [05]   | `ArmatureInstance.SetLocalMatrix`       | `(string nodeName, Matrix4x4)`            | overrides a bone's local-space matrix              |
|  [06]   | `ArmatureInstance.SetModelMatrix`       | `(string nodeName, Matrix4x4)`            | overrides a bone's model-space matrix              |
|  [07]   | `MeshDecoder.Decode`                    | extension on `IReadOnlyList<Mesh>`        | returns `IMeshDecoder<Material>[]`                 |
|  [08]   | `IMeshPrimitiveDecoder.GetPosition`     | `(int vertexIndex)`                       | returns `Vector3` position for vertex              |
|  [09]   | `IMeshPrimitiveDecoder.GetNormal`       | `(int vertexIndex)`                       | returns `Vector3` normal for vertex                |
|  [10]   | `IMeshPrimitiveDecoder.GetTangent`      | `(int vertexIndex)`                       | returns `Vector4` tangent for vertex               |
|  [11]   | `IMeshPrimitiveDecoder.GetTextureCoord` | `(int vertexIndex, int set)`              | returns `Vector2` UV for vertex                    |
|  [12]   | `IMeshPrimitiveDecoder.GetSkinWeights`  | `(int vertexIndex)`                       | returns `SparseWeight8` skin weights               |
|  [13]   | `IMeshPrimitiveDecoder.TriangleIndices` | property                                  | `IEnumerable<(int,int,int)>` triangle index tuples |

## [04]-[IMPLEMENTATION_LAW]

[GLTF_IO]:
- read root: `ModelRoot.Load` (file) / `ModelRoot.ParseGLB` (bytes) / `ModelRoot.ReadGLB` (stream)
- write root: `ModelRoot.Save` selects format by extension; `ModelRoot.WriteGLB` produces bytes
- validation: `ReadSettings.Validation` and `WriteSettings.Validation` accept `ValidationMode.Skip`, `TryFix`, or `Strict`
- custom URI resolution: configure `ReadContext` with a file-reader delegate before calling `ReadSchema2`

[WRITE_SETTINGS]:
- `MergeBuffers` (`bool`, default `true`) — merges all `ModelRoot.LogicalBuffers` into one before serialization
- `BuffersMaxSize` (`int`, default `int.MaxValue`) — splits the merged buffer into chunks at this byte cap; only applies when `MergeBuffers` is `true` and format is glTF (not GLB)
- `JsonIndented` (`bool`) — delegates to `JsonWriterOptions.Indented`; set `true` for human-readable output
- `JsonOptions` (`JsonWriterOptions`) — full STJ writer options; `JsonIndented` is a convenience forwarder into this field
- `ImageWriting` (`ResourceWriteMode`) — controls how images are stored: `Default` / `SatelliteFile` / `EmbeddedAsBase64` / `BufferView`; `BufferView` embeds images into the binary buffer (GLB-native form); `EmbeddedAsBase64` embeds into JSON only for glTF, not GLB
- `ImageWriteCallback` (`ImageWriterCallback`) — per-image hook that overrides `ImageWriting` for individual images; receives the image and returns `ResourceWriteMode`
- `JsonPostprocessor` (`JsonFilterCallback`) — post-processes the raw JSON text string before it is written; use for custom escape or transform passes
- `Validation` (`ValidationMode`) — validation strictness; applies at both read and write time through the matching settings class

[COMPRESSION_LAW]:
- `SharpGLTF.Core` 1.0.6 ships **no Draco encode surface and no meshopt encode surface** in this build; the package has zero types matching `KHR_draco_mesh_compression` or `EXT_meshopt_compression` in the decompiled assembly scope.
- KHR_draco_mesh_compression read-side support can be registered via `ExtensionsFactory.RegisterExtension<MeshPrimitive, TDracoExt>(name)` when a separately compiled Draco decode adapter is provided by the caller; the core library carries only the extension framework, not the codec.
- EXT_meshopt_compression and vertex quantization helpers are not present in Core 1.0.6; these are caller-provided or require a separate SharpGLTF extension package if available.
- Consumers requiring Draco decode must supply a `JsonSerializable`-derived extension class and register it before any read call; no encode path exists in this version.

[EXTENSION_REGISTRATION]:
- `ExtensionsFactory.RegisterExtension<TParent, TExt>(string name)` — registers a globally available extension type
- KHR extensions included in-box: clearcoat, transmission, volume, ior, iridescence, sheen, anisotropy, emissive strength, dispersion, diffuse transmission, unlit, specular, specular-gloss, mesh-gpu-instancing, lights-punctual, texture-transform, texture-basisu, texture-dds, texture-webp, animation-pointer, xmp-json-ld
- custom extensions: implement from `JsonSerializable`; register before any read/write call

[TOOLKIT_BUILD]:
- primary path: `MeshBuilder<M,vG,vM,vS>` → `SceneBuilder.AddRigidMesh` → `SceneBuilder.ToGltf2()` → `ModelRoot`
- vertex type selection: compose `VertexBuilder<TvG, TvM, TvS>` from geometry fragment + material fragment + skinning fragment
- material builder: `new MaterialBuilder(name).WithMetallicRoughnessShader()` then set channels via `UseChannel(key)`
- skinned mesh: add via `AddSkinnedMesh(mesh, Matrix4x4.Identity, joints)` where joints are `NodeBuilder` instances
- output settings: `SceneBuilderSchema2Settings` controls strided buffers, buffer merge, GPU instancing threshold

[RUNTIME_DECODE]:
- instancing path: `SceneTemplate.Create(schema2Scene)` → `SceneTemplate.CreateInstance()` → drive `SetAnimationFrame` per tick
- draw loop: enumerate `SceneInstance` (it is `IEnumerable<DrawableInstance>`) → each `DrawableInstance.Template.LogicalMeshIndex` selects the mesh, `DrawableInstance.Transform` carries `IGeometryTransform`
- mesh decode: `model.LogicalMeshes.Decode()` returns `IMeshDecoder<Material>[]`; each primitive exposes typed vertex accessors
- normal/tangent generation: the decode path computes smooth normals and MikkTSpace tangents through `internal` factory kernels; consumers read the generated values through `IMeshPrimitiveDecoder.GetNormal`/`GetTangent`

[LOCAL_ADMISSION]:
- geometry export enters through `SceneBuilder` → `ToGltf2()` → `ModelRoot.Save*` or `WriteGLB`.
- geometry import enters through `ModelRoot.Load*` or `ReadContext.ReadSchema2`.
- runtime scene evaluation enters through `SceneTemplate.Create` → `CreateInstance` → animation frame drive.
- extension admission must register at `ExtensionsFactory` before any read or write that uses that extension.

[RAIL_LAW]:
- Packages: `SharpGLTF.Core`, `SharpGLTF.Toolkit`, `SharpGLTF.Runtime`
- Owns: glTF 2.0 read/write, typed mesh building, runtime scene instancing
- Accept: geometry exchange, asset authoring, runtime mesh evaluation
- Reject: rendering pipeline, GPU resource management, image decode
