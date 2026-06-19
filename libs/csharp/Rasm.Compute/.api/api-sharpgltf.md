# [RASM_COMPUTE_API_SHARPGLTF]

`SharpGLTF` supplies glTF 2.0 schema read/write (`SharpGLTF.Core`), high-level
scene/mesh/material builders (`SharpGLTF.Toolkit`), and runtime decode/instancing
for game-engine integration (`SharpGLTF.Runtime`) across three coordinated packages.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Schema2 — model root and I/O contexts
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                                            |
| :-----: | :------------------- | :---------------------------------------------------------------------- |
|   [1]   | `ModelRoot`          | root object for a glTF asset; owns all logical collections              |
|   [2]   | `ReadContext`        | context for reading glTF/GLB from file, stream, or bytes                |
|   [3]   | `ReadSettings`       | validation policy and URI-resolution options for read                   |
|   [4]   | `WriteContext`       | context for writing glTF/GLB to file, stream, or callback               |
|   [5]   | `WriteSettings`      | validation and buffer-merge options for write                           |
|   [6]   | `ExtensionsFactory`  | global extension registry; `RegisterExtension<TParent,TExt>` adds types |
|   [7]   | `LogicalChildOfRoot` | base for all logical resources (mesh, material, accessor, etc.)         |

[PUBLIC_TYPE_SCOPE]: Schema2 — scene graph and logical resources
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :-------------------------------------------------- |
|   [1]   | `Scene`                        | root nodes of a scene                               |
|   [2]   | `Node`                         | scene-graph node; carries mesh, skin, TRS, children |
|   [3]   | `Mesh`                         | set of `MeshPrimitive` objects for rendering        |
|   [4]   | `MeshPrimitive`                | geometry + material; carries attribute accessors    |
|   [5]   | `Accessor`                     | typed view into a buffer view; scalar/vec/matrix    |
|   [6]   | `BufferView`                   | contiguous subset of a `Buffer`                     |
|   [7]   | `Buffer`                       | raw binary blob; internal or external URI           |
|   [8]   | `Material`                     | material appearance; channels and PBR parameters    |
|   [9]   | `MaterialChannel`              | sub-channel carrying texture or parameter values    |
|  [10]   | `MaterialPBRMetallicRoughness` | PBR metallic-roughness parameter block              |
|  [11]   | `Texture`                      | texture + sampler binding                           |
|  [12]   | `TextureSampler`               | wrap and filter modes                               |
|  [13]   | `Image`                        | image data; URI or buffer-view embedded             |
|  [14]   | `Skin`                         | joints and inverse-bind matrices for skeletal mesh  |
|  [15]   | `Animation`                    | keyframe animation; channels + samplers             |
|  [16]   | `AnimationChannel`             | binds an animation sampler to a node property       |
|  [17]   | `AnimationSampler`             | timestamps + interpolated output values             |
|  [18]   | `AnimationChannelTarget`       | descriptor of the animated node + property path     |

[PUBLIC_TYPE_SCOPE]: Schema2 — scene graph extensions
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]            | [CAPABILITY]                                      |
| :-----: | :------------------ | :------------------------------------------------ |
|   [1]   | `MeshGpuInstancing` | KHR_mesh_gpu_instancing extension; instance attrs |
|   [2]   | `PunctualLight`     | KHR_lights_punctual: directional, point, spot     |

[PUBLIC_TYPE_SCOPE]: Schema2 — accessors, memory, and encoding enums
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`, `SharpGLTF.Memory`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                     |
| :-----: | :--------------------------- | :--------------------------------------------------------------- |
|   [1]   | `DimensionType`              | `SCALAR`, `VEC2`, `VEC3`, `VEC4`, `MAT2`, `MAT3`, `MAT4`         |
|   [2]   | `EncodingType`               | `BYTE`, `UBYTE`, `SHORT`, `USHORT`, `UINT`, `FLOAT`              |
|   [3]   | `IndexEncodingType`          | `UNSIGNED_BYTE`, `UNSIGNED_SHORT`, `UNSIGNED_INT`                |
|   [4]   | `ResourceWriteMode`          | `Default`, `SatelliteFile`, `EmbeddedAsBase64`, `BufferView`     |
|   [5]   | `AlphaMode`                  | `OPAQUE`, `MASK`, `BLEND`                                        |
|   [6]   | `PrimitiveType`              | `POINTS`, `LINES`, `TRIANGLES`, etc.                             |
|   [7]   | `AnimationInterpolationMode` | `LINEAR`, `STEP`, `CUBICSPLINE`                                  |
|   [8]   | `PropertyPath`               | animated property: `translation`, `rotation`, `scale`, `weights` |
|   [9]   | `MemoryAccessor`             | wraps a `BufferView` memory region; projects typed arrays        |
|  [10]   | `MemoryAccessInfo`           | describes item format: name, byte offset, stride, format         |
|  [11]   | `MemoryImage`                | in-memory image bytes; detects PNG/JPG/KTX2/DDS/WebP             |
|  [12]   | `ScalarArray`                | typed `Memory<byte>` view over scalar accessor data              |
|  [13]   | `Vector2Array`               | typed `Memory<byte>` view over Vector2 accessor data             |
|  [14]   | `Vector3Array`               | typed `Memory<byte>` view over Vector3 accessor data             |
|  [15]   | `Vector4Array`               | typed `Memory<byte>` view over Vector4 accessor data             |
|  [16]   | `QuaternionArray`            | typed `Memory<byte>` view over quaternion accessor data          |
|  [17]   | `Matrix4x4Array`             | typed `Memory<byte>` view over matrix4x4 accessor data           |
|  [18]   | `IntegerArray`               | typed `Memory<byte>` view over index accessor data               |

[PUBLIC_TYPE_SCOPE]: Schema2 — memory typed arrays (continued)
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Memory`, `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                            |
| :-----: | :---------------- | :------------------------------------------------------ |
|   [1]   | `ColorArray`      | typed `Memory<byte>` view over color accessor data      |
|   [2]   | `AttributeFormat` | encoding/decoding descriptor for vertex attribute bytes |
|   [3]   | `BufferMode`      | `ARRAY_BUFFER`, `ELEMENT_ARRAY_BUFFER` hints            |
|   [4]   | `CameraType`      | `PERSPECTIVE`, `ORTHOGRAPHIC`                           |

[PUBLIC_TYPE_SCOPE]: Schema2 — validation
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Validation`
- rail: geometry

| [INDEX] | [SYMBOL]            | [CAPABILITY]                                                           |
| :-----: | :------------------ | :--------------------------------------------------------------------- |
|   [1]   | `ValidationMode`    | `Skip`, `TryFix`, `Strict` — controls validation policy for read/write |
|   [2]   | `ValidationContext` | utility class used during model validation traversal                   |
|   [3]   | `ModelException`    | base exception from glTF serialization or validation                   |
|   [4]   | `SchemaException`   | invalid JSON document                                                  |
|   [5]   | `SemanticException` | invalid semantic values within a valid document                        |
|   [6]   | `LinkException`     | invalid inter-object relationships                                     |
|   [7]   | `DataException`     | invalid binary data                                                    |

[PUBLIC_TYPE_SCOPE]: Schema2 — material channel surface and extension capability
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

The KHR material extension classes (`MaterialClearCoat`, `MaterialSheen`, `MaterialVolume`, etc.) are `internal` in SharpGLTF.Core; the public surface for every PBR extension is the channel API. `Material.FindChannel(string)` and `Material.Channels` project `MaterialChannel` rows keyed by the `KnownChannel` enum, whose values carry each extension capability.

| [INDEX] | [SYMBOL]                                | [CAPABILITY]                                                                          |
| :-----: | :-------------------------------------- | :------------------------------------------------------------------------------------ |
|   [1]   | `Material.Channels`                     | public `IReadOnlyList<MaterialChannel>` of all active channels on the material        |
|   [2]   | `Material.FindChannel`                  | resolves one `MaterialChannel?` by channel key string                                 |
|   [3]   | `MaterialChannel`                       | struct carrying texture, parameter values, and channel key for one channel            |
|   [4]   | `KnownChannel`                          | channel-key enum: `BaseColor`, `MetallicRoughness`, `Normal`, `Occlusion`, `Emissive` |
|   [5]   | `KnownChannel.ClearCoat`                | KHR_materials_clearcoat (`ClearCoat`, `ClearCoatNormal`, `ClearCoatRoughness`)        |
|   [6]   | `KnownChannel.Transmission`             | KHR_materials_transmission; optical transmission                                      |
|   [7]   | `KnownChannel.SheenColor`               | KHR_materials_sheen (`SheenColor`, `SheenRoughness`)                                  |
|   [8]   | `KnownChannel.SpecularColor`            | KHR_materials_specular (`SpecularColor`, `SpecularFactor`)                            |
|   [9]   | `KnownChannel.VolumeThickness`          | KHR_materials_volume (`VolumeThickness`, `VolumeAttenuation`)                         |
|  [10]   | `KnownChannel.Iridescence`              | KHR_materials_iridescence (`Iridescence`, `IridescenceThickness`)                     |
|  [11]   | `KnownChannel.Anisotropy`               | KHR_materials_anisotropy; anisotropic reflections                                     |
|  [12]   | `KnownChannel.DiffuseTransmissionColor` | KHR_materials_diffuse_transmission                                                    |
|  [13]   | `TextureTransform`                      | KHR_texture_transform; public `ExtraProperties` UV shift/scale per texture            |

[PUBLIC_TYPE_SCOPE]: Schema2 — XMP metadata extensions
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                          |
| :-----: | :------------------- | :---------------------------------------------------- |
|   [1]   | `XmpPackets`         | KHR_xmp_json_ld model-level XMP metadata packet list  |
|   [2]   | `XmpPacketReference` | KHR_xmp_json_ld per-entity XMP packet index reference |

[PUBLIC_TYPE_SCOPE]: Toolkit — scene and mesh builders
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`, `SharpGLTF.Geometry`
- rail: geometry

| [INDEX] | [SYMBOL]                             | [CAPABILITY]                                                          |
| :-----: | :----------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | `SceneBuilder`                       | root scene; holds instances referencing meshes, cameras, lights       |
|   [2]   | `NodeBuilder`                        | hierarchical armature node; carries animatable TRS, scale, rotation   |
|   [3]   | `InstanceBuilder`                    | one renderable instance in the scene; content + transform             |
|   [4]   | `MeshBuilder<TMat,TvG,TvM,TvS>`      | typed mesh builder; owns `PrimitiveBuilder` per material              |
|   [5]   | `IMeshBuilder<TMat>`                 | interface for mesh builders used by `SceneBuilder.AddRigidMesh`       |
|   [6]   | `PrimitiveBuilder<TMat,TvG,TvM,TvS>` | builds point/line/triangle primitives; `AddTriangle`, `AddQuadrangle` |
|   [7]   | `VertexBuilder<TvG,TvM,TvS>`         | typed vertex struct: geometry + material + skinning fragments         |
|   [8]   | `VertexBufferColumns`                | column-per-attribute vertex buffer; transpose layout                  |
|   [9]   | `SceneBuilderSchema2Settings`        | conversion options: strided buffers, merge, GPU instancing threshold  |

[PUBLIC_TYPE_SCOPE]: Toolkit — vertex geometry fragments
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Geometry.VertexTypes`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :---------------------------------------------- |
|   [1]   | `VertexPosition`              | position-only geometry fragment                 |
|   [2]   | `VertexPositionNormal`        | position + normal geometry fragment             |
|   [3]   | `VertexPositionNormalTangent` | position + normal + tangent geometry fragment   |
|   [4]   | `VertexGeometryDelta`         | morph-target position/normal/tangent delta      |
|   [5]   | `VertexEmpty`                 | empty material or skinning fragment placeholder |
|   [6]   | `VertexColor1`                | 1-color material fragment                       |
|   [7]   | `VertexColor2`                | 2-color material fragment                       |
|   [8]   | `VertexTexture1`              | 1-UV material fragment                          |
|   [9]   | `VertexTexture2`              | 2-UV material fragment                          |
|  [10]   | `VertexColor1Texture1`        | 1-color + 1-UV material fragment                |
|  [11]   | `VertexColor1Texture2`        | 1-color + 2-UV material fragment                |
|  [12]   | `VertexColor2Texture1`        | 2-color + 1-UV material fragment                |
|  [13]   | `VertexColor2Texture2`        | 2-color + 2-UV material fragment                |
|  [14]   | `VertexMaterialDelta`         | morph-target color + UV delta                   |
|  [15]   | `VertexJoints4`               | 4-joint skinning fragment                       |
|  [16]   | `VertexJoints8`               | 8-joint skinning fragment                       |
|  [17]   | `IVertexGeometry`             | interface for geometry fragments                |
|  [18]   | `IVertexMaterial`             | interface for material fragments                |

[PUBLIC_TYPE_SCOPE]: Toolkit — vertex fragment interfaces
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Geometry.VertexTypes`
- rail: geometry

| [INDEX] | [SYMBOL]          | [CAPABILITY]                             |
| :-----: | :---------------- | :--------------------------------------- |
|   [1]   | `IVertexSkinning` | interface for skinning fragments         |
|   [2]   | `IVertexCustom`   | interface for custom attribute fragments |

[PUBLIC_TYPE_SCOPE]: Toolkit — material and morph builders
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Materials`, `SharpGLTF.Geometry`
- rail: geometry

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                                          |
| :-----: | :-------------------- | :-------------------------------------------------------------------- |
|   [1]   | `MaterialBuilder`     | root material; sets shader, alpha mode, double-sided, fallback        |
|   [2]   | `ChannelBuilder`      | material channel; holds `TextureBuilder` and scalar parameter values  |
|   [3]   | `TextureBuilder`      | texture reference; primary + fallback images, transform, coord set    |
|   [4]   | `ImageBuilder`        | in-memory image content with optional alternate write file name       |
|   [5]   | `AlphaMode`           | `Opaque`, `Mask`, `Blend` (Toolkit-local mirror of Schema2 enum)      |
|   [6]   | `KnownProperty`       | enumeration of channel parameter keys (BaseColor, Metallic, etc.)     |
|   [7]   | `IMorphTargetBuilder` | interface for setting per-vertex morph target deltas                  |
|   [8]   | `MorphTargetBuilder`  | mesh-level morph target; `SetVertexDelta` by position or geometry key |
|   [9]   | `CameraBuilder`       | perspective or orthographic camera; `ZNear`, `ZFar`, `VerticalFOV`    |
|  [10]   | `LightBuilder`        | directional, point, or spot light with `Color`, `Intensity`, `Range`  |

[PUBLIC_TYPE_SCOPE]: Runtime — scene template and instancing
- package: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- rail: geometry

The `*Template` classes (`SceneTemplate` excepted), `ArmatureTemplate`, `NodeTemplate`, `DrawableTemplate`, and `MaterialTemplate` are `internal`; the public runtime surface is `SceneTemplate` plus the `*Instance` mutable-state types and `RuntimeOptions`.

| [INDEX] | [SYMBOL]           | [CAPABILITY]                                                                            |
| :-----: | :----------------- | :-------------------------------------------------------------------------------------- |
|   [1]   | `SceneTemplate`    | templatized scene from a `Schema2.Scene`; `CreateInstance` makes `SceneInstance` copies |
|   [2]   | `SceneInstance`    | independent mutable state of a `SceneTemplate`; owns `ArmatureInstance`                 |
|   [3]   | `ArmatureInstance` | per-instance bone transform state; `SetAnimationFrame`, `SetPoseTransforms`             |
|   [4]   | `NodeInstance`     | per-instance node transform state; `LocalMatrix`, `ModelMatrix`                         |
|   [5]   | `DrawableInstance` | struct: `Template` (what) + `Transform` (where) + `InstanceCount`                       |
|   [6]   | `RuntimeOptions`   | `IsolateMemory`, `GpuMeshInstancing`, `ExtrasConverterCallback`                         |

[PUBLIC_TYPE_SCOPE]: Runtime — mesh decode contracts
- package: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                                           |
| :-----: | :---------------------------- | :--------------------------------------------------------------------- |
|   [1]   | `IMeshDecoder<TMat>`          | mesh decode interface; name, extras, logical index, primitives         |
|   [2]   | `IMeshPrimitiveDecoder`       | primitive decode interface; positions, normals, UVs, colors, skin      |
|   [3]   | `IMeshPrimitiveDecoder<TMat>` | typed variant carrying material reference                              |
|   [4]   | `MeshDecoder`                 | static utility; `Decode()` extension on `Mesh` / `IReadOnlyList<Mesh>` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ModelRoot — read
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                          | [CAPABILITY]                                 |
| :-----: | :------------------------------------ | :------------------------------------ | :------------------------------------------- |
|   [1]   | `ModelRoot.Load`                      | `(string, ReadSettings?)`             | reads glTF or GLB from file path             |
|   [2]   | `ModelRoot.ParseGLB`                  | `(ArraySegment<byte>, ReadSettings?)` | parses GLB from byte array                   |
|   [3]   | `ModelRoot.ReadGLB`                   | `(Stream, ReadSettings?)`             | reads GLB from a stream                      |
|   [4]   | `ReadContext.ReadSchema2`             | `(string)` or `(Stream)`              | reads from context-relative file or stream   |
|   [5]   | `ReadContext.ReadTextSchema2`         | `(Stream)`                            | forces text glTF parse                       |
|   [6]   | `ReadContext.ReadBinarySchema2`       | `(Stream)`                            | forces binary GLB parse                      |
|   [7]   | `ReadContext.IdentifyBinaryContainer` | `(Stream)`                            | returns whether stream is glTF or GLB        |
|   [8]   | `ModelRoot.GetSatellitePaths`         | `(string)`                            | returns satellite file paths for a glTF path |

[ENTRYPOINT_SCOPE]: ModelRoot — write
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                  | [CAPABILITY]                                   |
| :-----: | :-------------------------------- | :---------------------------- | :--------------------------------------------- |
|   [1]   | `ModelRoot.Save`                  | `(string, WriteSettings?)`    | writes glTF or GLB by file extension           |
|   [2]   | `ModelRoot.SaveGLB`               | `(string, WriteSettings?)`    | writes binary GLB to file                      |
|   [3]   | `ModelRoot.SaveGLTF`              | `(string, WriteSettings?)`    | writes text glTF to file                       |
|   [4]   | `ModelRoot.WriteGLB`              | `(WriteSettings?)` → `byte[]` | serializes GLB to byte array                   |
|   [5]   | `ModelRoot.WriteGLB`              | `(Stream, WriteSettings?)`    | writes GLB to stream                           |
|   [6]   | `WriteContext.WriteTextSchema2`   | `(string, ModelRoot)`         | writes text schema to context output           |
|   [7]   | `WriteContext.WriteBinarySchema2` | `(string, ModelRoot)`         | writes binary schema to context output         |
|   [8]   | `ModelRoot.GetJsonPreview`        | `()`                          | returns JSON text preview without side effects |

[ENTRYPOINT_SCOPE]: ModelRoot — construction and mutation
- package: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                  | [CAPABILITY]                               |
| :-----: | :------------------------------ | :---------------------------- | :----------------------------------------- |
|   [1]   | `ModelRoot.CreateModel`         | `()` (static)                 | creates an empty `ModelRoot`               |
|   [2]   | `ModelRoot.DeepClone`           | `()`                          | creates a full structural clone            |
|   [3]   | `ModelRoot.UseScene`            | `(string)` or `(int)`         | creates or reuses a named scene            |
|   [4]   | `ModelRoot.CreateMesh`          | `(string)`                    | creates a new logical mesh                 |
|   [5]   | `ModelRoot.CreateMaterial`      | `(string)`                    | creates a new logical material             |
|   [6]   | `ModelRoot.CreateAccessor`      | `(string)`                    | creates a new logical accessor             |
|   [7]   | `ModelRoot.CreateAnimation`     | `(string)`                    | creates a new animation                    |
|   [8]   | `ModelRoot.UseBuffer`           | `(byte[])`                    | creates or reuses a buffer from byte array |
|   [9]   | `ModelRoot.UseBufferView`       | overloads                     | creates or reuses a buffer view            |
|  [10]   | `ModelRoot.MergeBuffers`        | `()` or `(int)`               | consolidates logical buffers               |
|  [11]   | `ModelRoot.IsolateMemory`       | `()`                          | refreshes internal memory buffers          |
|  [12]   | `ModelRoot.ApplyBasisTransform` | `(Matrix4x4, string)`         | applies world transform to all scenes      |
|  [13]   | `ModelRoot.UseImage`            | `(MemoryImage)`               | creates or reuses an image                 |
|  [14]   | `ModelRoot.UseTexture`          | `(Image, TextureSampler)`     | creates or reuses a texture                |
|  [15]   | `ModelRoot.UseTextureSampler`   | wrap/filter args              | creates or reuses a texture sampler        |
|  [16]   | `ModelRoot.CreateSkin`          | `(string)`                    | creates a new skin                         |
|  [17]   | `ModelRoot.CreatePunctualLight` | `(string, PunctualLightType)` | creates a KHR punctual light               |

[ENTRYPOINT_SCOPE]: SceneBuilder — mesh placement and output
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`
- rail: geometry

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                      | [CAPABILITY]                               |
| :-----: | :--------------------------------- | :------------------------------------------------ | :----------------------------------------- |
|   [1]   | `SceneBuilder.AddRigidMesh`        | `(IMeshBuilder<M>, NodeBuilder)`                  | adds mesh attached to animatable node      |
|   [2]   | `SceneBuilder.AddRigidMesh`        | `(IMeshBuilder<M>, AffineTransform)`              | adds mesh at fixed world transform         |
|   [3]   | `SceneBuilder.AddRigidMesh`        | `(IMeshBuilder<M>, NodeBuilder, AffineTransform)` | adds mesh relative to node                 |
|   [4]   | `SceneBuilder.ToGltf2`             | `()` or `(SceneBuilderSchema2Settings)`           | converts builder to `ModelRoot`            |
|   [5]   | `SceneBuilder.ToGltf2`             | `(IEnumerable<SceneBuilder>, settings)`           | converts multiple scenes to `ModelRoot`    |
|   [6]   | `SceneBuilder.AddScene`            | `(SceneBuilder, Matrix4x4)`                       | merges another scene with offset transform |
|   [7]   | `SceneBuilder.ApplyBasisTransform` | `(Matrix4x4, string)`                             | transforms all instances in this scene     |
|   [8]   | `SceneBuilder.FindArmatures`       | `()`                                              | returns unique armature roots              |

[ENTRYPOINT_SCOPE]: MeshBuilder — primitive assembly
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Geometry`
- rail: geometry

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                           | [CAPABILITY]                               |
| :-----: | :------------------------------------- | :------------------------------------- | :----------------------------------------- |
|   [1]   | `MeshBuilder<M,vG,vM,vS>.UsePrimitive` | `(material, verticesPerPrimitive)`     | creates or reuses a primitive for material |
|   [2]   | `MeshBuilder.TransformVertices`        | `(Func<VertexBuilder, VertexBuilder>)` | transforms all vertices in place           |
|   [3]   | `PrimitiveBuilder.AddTriangle`         | `(v0, v1, v2)`                         | adds a triangle from three typed vertices  |
|   [4]   | `PrimitiveBuilder.AddQuadrangle`       | `(v0, v1, v2, v3)`                     | adds a quad, auto-split to two triangles   |
|   [5]   | `PrimitiveBuilder.AddLine`             | `(v0, v1)`                             | adds a line segment                        |
|   [6]   | `PrimitiveBuilder.AddPoint`            | `(v0)`                                 | adds a point                               |
|   [7]   | `PrimitiveBuilder.UseVertex`           | `(ref VertexBuilder<vG,vM,vS>)`        | adds or reuses vertex, returns index       |

[ENTRYPOINT_SCOPE]: MaterialBuilder — shader and channel configuration
- package: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Materials`
- rail: geometry

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]        | [CAPABILITY]                           |
| :-----: | :--------------------------------------------- | :------------------ | :------------------------------------- |
|   [1]   | `MaterialBuilder.WithMetallicRoughnessShader`  | `()`                | selects PBR metallic-roughness shader  |
|   [2]   | `MaterialBuilder.WithSpecularGlossinessShader` | `()`                | selects KHR specular-glossiness shader |
|   [3]   | `MaterialBuilder.WithUnlitShader`              | `()`                | selects KHR_materials_unlit shader     |
|   [4]   | `MaterialBuilder.WithShader`                   | `(string)`          | selects shader by name string          |
|   [5]   | `MaterialBuilder.WithFallback`                 | `(MaterialBuilder)` | chains a fallback material             |
|   [6]   | `MaterialBuilder.DoubleSided`                  | `bool` property     | enables back-face rendering            |

[ENTRYPOINT_SCOPE]: SceneTemplate — runtime decode
- package: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`
- rail: geometry

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                              | [CAPABILITY]                                       |
| :-----: | :-------------------------------------- | :---------------------------------------- | :------------------------------------------------- |
|   [1]   | `SceneTemplate.Create`                  | `(Scene, RuntimeOptions?)` (static)       | creates template from a `Schema2.Scene`            |
|   [2]   | `SceneTemplate.CreateInstance`          | `()`                                      | creates an independent `SceneInstance`             |
|   [3]   | `ArmatureInstance.SetAnimationFrame`    | `(int trackIndex, float time, bool loop)` | advances bone transforms to animation time         |
|   [4]   | `ArmatureInstance.SetPoseTransforms`    | `()`                                      | resets all bones to rest pose                      |
|   [5]   | `ArmatureInstance.SetLocalMatrix`       | `(string nodeName, Matrix4x4)`            | overrides a bone's local-space matrix              |
|   [6]   | `ArmatureInstance.SetModelMatrix`       | `(string nodeName, Matrix4x4)`            | overrides a bone's model-space matrix              |
|   [7]   | `MeshDecoder.Decode`                    | extension on `IEnumerable<Mesh>`          | returns `IReadOnlyList<IMeshDecoder<Material>>`    |
|   [8]   | `IMeshPrimitiveDecoder.GetPosition`     | `(int vertexIndex)`                       | returns `Vector3` position for vertex              |
|   [9]   | `IMeshPrimitiveDecoder.GetNormal`       | `(int vertexIndex)`                       | returns `Vector3` normal for vertex                |
|  [10]   | `IMeshPrimitiveDecoder.GetTangent`      | `(int vertexIndex)`                       | returns `Vector4` tangent for vertex               |
|  [11]   | `IMeshPrimitiveDecoder.GetTextureCoord` | `(int vertexIndex, int set)`              | returns `Vector2` UV for vertex                    |
|  [12]   | `IMeshPrimitiveDecoder.GetSkinWeights`  | `(int vertexIndex)`                       | returns `SparseWeight8` skin weights               |
|  [13]   | `IMeshPrimitiveDecoder.TriangleIndices` | property                                  | `IEnumerable<(int,int,int)>` triangle index tuples |

## [4]-[IMPLEMENTATION_LAW]

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
- draw loop: iterate `SceneInstance.DrawableInstances` → each `DrawableInstance.Template.LogicalMeshIndex` selects mesh, `DrawableInstance.Transform` carries `IGeometryTransform`
- mesh decode: `model.LogicalMeshes.Decode()` returns `IMeshDecoder<Material>[]`; each primitive exposes typed vertex accessors
- normal/tangent generation: `VertexBufferColumns.CalculateSmoothNormals` and `VertexBufferColumns.CalculateTangents` operate over primitive vertex/index pairs (the `VertexNormalsFactory`/`VertexTangentsFactory` helpers are internal)

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
