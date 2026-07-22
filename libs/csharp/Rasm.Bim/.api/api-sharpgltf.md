# [RASM_BIM_API_SHARPGLTF]

`SharpGLTF` owns glTF 2.0 schema I/O, typed scene and mesh authoring, and runtime scene instancing: `SharpGLTF.Core` mints the read/write contexts and the `ModelRoot` logical-resource model, `SharpGLTF.Toolkit` folds typed vertex fragments through scene, mesh, and material builders into a `ModelRoot`, and `SharpGLTF.Runtime` templatizes a `Schema2.Scene` for per-instance animation decode. Core carries the extension framework but no geometry codec — Draco and meshopt encode ride sibling packages that rewrite the authored buffer views.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpGLTF.Core`
- package: `SharpGLTF.Core` (MIT)
- assembly: `SharpGLTF.Core`
- namespace: `SharpGLTF.Schema2`, `SharpGLTF.Memory`, `SharpGLTF.Validation`, `SharpGLTF.Animations`, `SharpGLTF.IO`
- namespace: `SharpGLTF.Transforms` (owns `SparseWeight8`, `IGeometryTransform`)
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry

[PACKAGE_SURFACE]: `SharpGLTF.Toolkit`
- package: `SharpGLTF.Toolkit` (MIT)
- assembly: `SharpGLTF.Toolkit`
- namespace: `SharpGLTF.Scenes`, `SharpGLTF.Geometry`, `SharpGLTF.Geometry.VertexTypes`, `SharpGLTF.Materials`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry

[PACKAGE_SURFACE]: `SharpGLTF.Runtime`
- package: `SharpGLTF.Runtime` (MIT)
- assembly: `SharpGLTF.Runtime`
- namespace: `SharpGLTF.Runtime`, `SharpGLTF`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Schema2 model root and I/O contexts

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `ModelRoot`          | class         | glTF root; owns the typed read-lists in `[01]-[LOGICAL]`           |
|  [02]   | `ReadContext`        | class         | reads glTF/GLB from file, stream, or in-memory satellite map       |
|  [03]   | `ReadSettings`       | class         | validation policy and URI-resolution options for read              |
|  [04]   | `WriteContext`       | class         | writes glTF/GLB to file, stream, or callback                       |
|  [05]   | `WriteSettings`      | class         | write policy; members in `[05]-[WRITESET]`                         |
|  [06]   | `ExtensionsFactory`  | class         | static global extension registry                                   |
|  [07]   | `LogicalChildOfRoot` | class         | abstract base for logical resources; `LogicalParent` walks to root |

- [01]-[LOGICAL]: `ModelRoot` typed read-lists — `LogicalMeshes`, `LogicalBufferViews`, `LogicalBuffers`, `LogicalAccessors`, `LogicalMaterials`, `LogicalNodes`; each element's `LogicalParent` walks back to the owning root.
- [05]-[WRITESET]: `WriteSettings` — `MergeBuffers` (default `true`, merges `LogicalBuffers` pre-serialize), `BuffersMaxSize` (merged-chunk byte cap, glTF-only when merging), `JsonIndented`/`JsonOptions` (STJ writer options), `ImageWriting` (`ResourceWriteMode`: `BufferView` embeds GLB-native, `EmbeddedAsBase64` embeds glTF-JSON only), `ImageWriteCallback` (per-image override), `JsonPostprocessor` (raw-JSON transform pass), `Validation` (`ValidationMode`, both read and write).

[PUBLIC_TYPE_SCOPE]: Schema2 scene graph and logical resources

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :----------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Scene`            | class         | root nodes of a scene                                                  |
|  [02]   | `Node`             | class         | scene-graph node (mesh, skin, TRS, children); members in `[02]-[NODE]` |
|  [03]   | `Mesh`             | class         | set of `MeshPrimitive`; `Primitives`, `LogicalParent`                  |
|  [04]   | `MeshPrimitive`    | class         | geometry, material, attribute accessors; members in `[04]-[PRIM]`      |
|  [05]   | `Accessor`         | class         | typed buffer-view element view; members in `[05]-[ACCESSOR]`           |
|  [06]   | `BufferView`       | class         | contiguous `Buffer` subset; members in `[06]-[BUFVIEW]`                |
|  [07]   | `Buffer`           | class         | raw binary blob (internal or external URI); members in `[07]-[BUFFER]` |
|  [08]   | `Material`         | class         | PBR metallic-roughness and channel parameters through `FindChannel`    |
|  [09]   | `MaterialChannel`  | struct        | channel projection carrying texture or parameter values                |
|  [10]   | `Texture`          | class         | texture and sampler binding                                            |
|  [11]   | `TextureSampler`   | class         | wrap and filter modes                                                  |
|  [12]   | `Image`            | class         | image data; URI or buffer-view embedded                                |
|  [13]   | `Skin`             | class         | joints and inverse-bind matrices for a skeletal mesh                   |
|  [14]   | `Animation`        | class         | keyframe animation; owns channels and per-channel samplers             |
|  [15]   | `AnimationChannel` | class         | binds a sampler to a node property; owns keyframes and channel target  |

- [02]-[NODE]: `Node.WorldMatrix` (`Matrix4x4` local-to-world), `GetGpuInstancing()`/`UseGpuInstancing()` → `MeshGpuInstancing`, static `Flatten(IVisualNodeContainer)` → depth-first `IEnumerable<Node>`.
- [04]-[PRIM]: `MeshPrimitive.LogicalParent` (owning `Mesh`; `.LogicalParent.LogicalParent` reaches the `ModelRoot`), `GetVertexAccessor(string)`/`GetIndexAccessor()` → `Accessor`.
- [05]-[ACCESSOR]: `Accessor.AsScalarArray()`/`AsVector2Array()`/`AsVector3Array()`/`AsIndicesArray()` → `IAccessorArray<T>` over `SourceBufferView.Content`; `SetData(BufferView, int, int, AttributeFormat)`/`SetDataFrom(Accessor)` re-point the accessor. A bufferView-less accessor (the KHR_draco shape) backs no region, so a typed-view `Fill` writes nothing — the Draco write-back lane is `ModelRoot.UseBufferView` + `SetData`.
- [06]-[BUFVIEW]: `BufferView.Content` (`ArraySegment<byte>`, the raw bytes a compressed-view decode reads); `IsIndexBuffer`/`IsVertexBuffer` are the `BufferMode` target discriminants.
- [07]-[BUFFER]: `Buffer.Content` (`byte[]`, the whole model-backed buffer); the EXT_meshopt_compression slice reads `LogicalBuffers[i].Content` at the extension buffer/offset/length, never a fallback view's own region.

[PUBLIC_TYPE_SCOPE]: Schema2 scene-graph extensions

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                                |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `MeshGpuInstancing` | class         | KHR_mesh_gpu_instancing; `Count`, `GetLocalMatrix(int)`/`GetWorldMatrix(int)` → `Matrix4x4` |
|  [02]   | `PunctualLight`     | class         | KHR_lights_punctual: directional, point, spot                                               |

[PUBLIC_TYPE_SCOPE]: Schema2/Memory encoding enums and accessor descriptors

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
|  [12]   | `AttributeFormat`            | struct        | encode/decode descriptor for vertex attribute bytes              |
|  [13]   | `BufferMode`                 | enum          | `ARRAY_BUFFER`, `ELEMENT_ARRAY_BUFFER` hints                     |
|  [14]   | `CameraType`                 | enum          | `PERSPECTIVE`, `ORTHOGRAPHIC`                                    |

[PUBLIC_TYPE_SCOPE]: Memory typed array views

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                       |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `ScalarArray`     | struct        | `Memory<byte>` view over scalar accessor data                                      |
|  [02]   | `Vector2Array`    | struct        | `Memory<byte>` view over Vector2 accessor data                                     |
|  [03]   | `Vector3Array`    | struct        | Vector3 accessor view; `Fill(IEnumerable<Vector3>, int dstStart = 0)` writes back  |
|  [04]   | `Vector4Array`    | struct        | `Memory<byte>` view over Vector4 accessor data                                     |
|  [05]   | `QuaternionArray` | struct        | `Memory<byte>` view over quaternion accessor data                                  |
|  [06]   | `Matrix4x4Array`  | struct        | `Memory<byte>` view over matrix4x4 accessor data                                   |
|  [07]   | `IntegerArray`    | struct        | index accessor view; `Fill(IEnumerable<int>/<uint>, int dstStart = 0)` writes back |
|  [08]   | `ColorArray`      | struct        | `Memory<byte>` view over color accessor data                                       |

[PUBLIC_TYPE_SCOPE]: Validation

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `ValidationMode`    | enum          | `Skip`, `TryFix`, `Strict` read/write policy      |
|  [02]   | `ValidationContext` | struct        | validation-traversal state carrier                |
|  [03]   | `ModelException`    | class         | base for glTF serialization or validation failure |
|  [04]   | `SchemaException`   | class         | invalid JSON document                             |
|  [05]   | `SemanticException` | class         | invalid semantic values in a valid document       |
|  [06]   | `LinkException`     | class         | invalid inter-object relationships                |
|  [07]   | `DataException`     | class         | invalid binary data                               |

[INBOX_EXTENSION_SCOPE]: Schema2 KHR material extensions (PBR and shading)
- access: `internal` extension classes serialized in-box, authored and read through the public `Material`/`MaterialChannel` surface, never named directly

| [INDEX] | [EXTENSION]                           | [CAPABILITY]                  |
| :-----: | :------------------------------------ | :---------------------------- |
|  [01]   | `KHR_materials_unlit`                 | unlit shading                 |
|  [02]   | `KHR_materials_clearcoat`             | clear-coat layer              |
|  [03]   | `KHR_materials_transmission`          | optical transmission          |
|  [04]   | `KHR_materials_volume`                | sub-surface volume            |
|  [05]   | `KHR_materials_specular`              | specular reflectance strength |
|  [06]   | `KHR_materials_ior`                   | index of refraction           |
|  [07]   | `KHR_materials_iridescence`           | thin-film iridescence         |
|  [08]   | `KHR_materials_sheen`                 | fabric sheen layer            |
|  [09]   | `KHR_materials_anisotropy`            | anisotropic reflections       |
|  [10]   | `KHR_materials_emissive_strength`     | HDR emissive scale            |
|  [11]   | `KHR_materials_dispersion`            | spectral dispersion           |
|  [12]   | `KHR_materials_diffuse_transmission`  | diffuse transmission          |
|  [13]   | `KHR_materials_pbrSpecularGlossiness` | specular-gloss model          |

[PUBLIC_TYPE_SCOPE]: Schema2 KHR texture and metadata extensions

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `TextureTransform`   | class         | KHR_texture_transform; UV shift/scale per texture     |
|  [02]   | `XmpPackets`         | class         | KHR_xmp_json_ld model-level XMP metadata packet list  |
|  [03]   | `XmpPacketReference` | class         | KHR_xmp_json_ld per-entity XMP packet index reference |

[INBOX_EXTENSION_SCOPE]: Schema2 KHR/MSFT/EXT texture and animation extensions
- access: `internal` extension classes serialized in-box, reached through the public `Texture`/`TextureSampler`/`Animation` surface, never named directly

| [INDEX] | [EXTENSION]             | [CAPABILITY]                  |
| :-----: | :---------------------- | :---------------------------- |
|  [01]   | `KHR_texture_basisu`    | KTX2/Basis compressed texture |
|  [02]   | `MSFT_texture_dds`      | DirectDraw Surface texture    |
|  [03]   | `EXT_texture_webp`      | WebP texture                  |
|  [04]   | `KHR_animation_pointer` | JSON-pointer animation target |

[PUBLIC_TYPE_SCOPE]: Toolkit scene and mesh builders

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `SceneBuilder`                       | class         | root scene; holds instances referencing meshes, cameras, lights |
|  [02]   | `NodeBuilder`                        | class         | hierarchical armature node; animatable TRS, scale, rotation     |
|  [03]   | `InstanceBuilder`                    | class         | one renderable instance; content plus transform                 |
|  [04]   | `MeshBuilder<TMat,TvG,TvM,TvS>`      | class         | typed mesh builder; owns a `PrimitiveBuilder` per material      |
|  [05]   | `IMeshBuilder<TMat>`                 | interface     | mesh-builder contract for `SceneBuilder.AddRigidMesh`           |
|  [06]   | `PrimitiveBuilder<TMat,TvG,TvM,TvS>` | class         | builds point/line/triangle primitives                           |
|  [07]   | `VertexBuilder<TvG,TvM,TvS>`         | struct        | typed vertex: geometry, material, skinning fragments            |
|  [08]   | `VertexBufferColumns`                | class         | column-per-attribute vertex buffer; transpose layout            |
|  [09]   | `SceneBuilderSchema2Settings`        | struct        | `UseStridedBuffers`, buffer merge, GPU instancing threshold     |

[PUBLIC_TYPE_SCOPE]: Toolkit vertex geometry and material fragments

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

[PUBLIC_TYPE_SCOPE]: Toolkit vertex fragment interfaces

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :---------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `IVertexGeometry` | interface     | geometry-fragment contract                                                                |
|  [02]   | `IVertexMaterial` | interface     | `MaxColors`/`MaxTextCoords`, `GetColor(int)`/`GetTexCoord(int)`, `SetColor`/`SetTexCoord` |
|  [03]   | `IVertexSkinning` | interface     | skinning-fragment contract                                                                |
|  [04]   | `IVertexCustom`   | interface     | custom-attribute fragment (`: IVertexMaterial`); members in `[04]-[VCUSTOM]`              |

- [04]-[VCUSTOM]: `IVertexCustom.CustomAttributes` (`IEnumerable<string>`), `TryGetCustomAttribute(string, out object)`, `SetCustomAttribute(string, object)`; a `_FEATURE_ID_n` custom-attribute fragment implements the `IVertexMaterial`/`IVertexReflection` pair, assembled through the `VertexBuilder<TvG,TvM,TvS>` `(in TvG, in TvM)` ctor overload.

[PUBLIC_TYPE_SCOPE]: Toolkit material and morph builders

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `MaterialBuilder`     | class         | root material; shader, alpha mode, double-sided, fallback             |
|  [02]   | `ChannelBuilder`      | class         | material channel; `TextureBuilder` and scalar parameters              |
|  [03]   | `TextureBuilder`      | class         | texture reference; primary/fallback images, transform, coord set      |
|  [04]   | `ImageBuilder`        | class         | in-memory image content with optional alternate write file name       |
|  [05]   | `AlphaMode`           | enum          | `Opaque`, `Mask`, `Blend` (Toolkit-local mirror of the Schema2 enum)  |
|  [06]   | `KnownChannel`        | enum          | typed channel key the channel mutators discriminate on                |
|  [07]   | `KnownProperty`       | enum          | channel parameter key (`BaseColor`, `Metallic`, …)                    |
|  [08]   | `IMorphTargetBuilder` | interface     | per-vertex morph-delta contract                                       |
|  [09]   | `MorphTargetBuilder`  | class         | mesh-level morph target; `SetVertexDelta` by position or geometry key |
|  [10]   | `CameraBuilder`       | class         | perspective or orthographic camera; `ZNear`, `ZFar`, `VerticalFOV`    |
|  [11]   | `LightBuilder`        | class         | directional, point, or spot light; `Color`, `Intensity`, `Range`      |

[PUBLIC_TYPE_SCOPE]: Runtime scene template and instancing
- access: `ArmatureTemplate`, `NodeTemplate`, `DrawableTemplate` (and rigid/skinned subtypes), `MaterialTemplate` are `internal`; a consumer reaches the templatized scene through `SceneTemplate` and drives per-instance state through the public instance types, with `DrawableInstance.Template` exposing the internal drawable through the public `IDrawableTemplate`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `SceneTemplate`    | class         | templatized scene from a `Schema2.Scene`; creates `SceneInstance` copies |
|  [02]   | `SceneInstance`    | class         | independent mutable state of a `SceneTemplate`; owns `ArmatureInstance`  |
|  [03]   | `ArmatureInstance` | class         | per-instance bone transform state                                        |
|  [04]   | `NodeInstance`     | class         | per-instance node transform state; `LocalMatrix`, `ModelMatrix`          |
|  [05]   | `DrawableInstance` | struct        | `Template` (what), `Transform` (where), `InstanceCount`                  |
|  [06]   | `RuntimeOptions`   | class         | `IsolateMemory`, `GpuMeshInstancing`, `ExtrasConverterCallback`          |

[PUBLIC_TYPE_SCOPE]: Runtime mesh decode contracts
- access: `VertexNormalsFactory`/`VertexTangentsFactory` are `internal` static kernels (smooth-normal, MikkTSpace-tangent) running inside the decode path, not consumer-callable

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `IMeshDecoder<TMat>`          | interface     | mesh decode; name, extras, logical index, primitives                                |
|  [02]   | `IMeshPrimitiveDecoder`       | interface     | `GetPosition(int)`/`GetNormal(int)` untransformed, `TriangleIndices`, UV/color/skin |
|  [03]   | `IMeshPrimitiveDecoder<TMat>` | interface     | typed variant carrying a material reference                                         |
|  [04]   | `MeshDecoder`                 | class         | static utility; `Decode()` extension on `Mesh` and `IReadOnlyList<Mesh>`            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ModelRoot and ReadContext — read

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `ModelRoot.Load(string, ReadSettings?)`                     | static   | reads glTF or GLB from a file path                          |
|  [02]   | `ModelRoot.ParseGLB(ArraySegment<byte>, ReadSettings?)`     | static   | parses GLB from a byte segment                              |
|  [03]   | `ModelRoot.ReadGLB(Stream, ReadSettings?)`                  | static   | reads GLB from a stream                                     |
|  [04]   | `ModelRoot.GetSatellitePaths(string)`                       | static   | satellite file paths for a glTF path                        |
|  [05]   | `ReadContext.ReadSchema2(string)`                           | instance | reads a context-relative resource name or stream            |
|  [06]   | `ReadContext.ReadTextSchema2(Stream)`                       | instance | forces text glTF parse                                      |
|  [07]   | `ReadContext.ReadBinarySchema2(Stream)`                     | instance | forces binary GLB parse                                     |
|  [08]   | `ReadContext.IdentifyBinaryContainer(Stream) -> bool`       | static   | whether a stream is glTF or GLB                             |
|  [09]   | `ReadContext.ReadJson(Stream) -> string`                    | static   | GLB JSON chunk; raw-DOM read for a dropped extension        |
|  [10]   | `ReadContext.ReadJsonBytes(Stream) -> ReadOnlyMemory<byte>` | static   | raw JSON bytes of the GLB chunk                             |
|  [11]   | `ReadContext.Validation`                                    | property | per-context `ValidationMode`; `Skip` admits a fallback view |

- `ReadContext.CreateFromDictionary(IReadOnlyDictionary<string, ArraySegment<byte>>, bool)` (static factory) — file-system-free multi-part `.gltf` decode from an in-memory satellite map.

[ENTRYPOINT_SCOPE]: ModelRoot and WriteContext — write

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `ModelRoot.Save(string, WriteSettings?)`                   | instance | writes glTF or GLB by file extension   |
|  [02]   | `ModelRoot.SaveGLB(string, WriteSettings?)`                | instance | writes binary GLB to file              |
|  [03]   | `ModelRoot.SaveGLTF(string, WriteSettings?)`               | instance | writes text glTF to file               |
|  [04]   | `ModelRoot.WriteGLB(WriteSettings?) -> ArraySegment<byte>` | instance | serializes GLB to a byte segment       |
|  [05]   | `ModelRoot.WriteGLB(Stream, WriteSettings?)`               | instance | writes GLB to a stream                 |
|  [06]   | `WriteContext.WriteTextSchema2(string, ModelRoot)`         | instance | writes text schema to context output   |
|  [07]   | `WriteContext.WriteBinarySchema2(string, ModelRoot)`       | instance | writes binary schema to context output |
|  [08]   | `ModelRoot.GetJsonPreview() -> string`                     | instance | JSON text preview without side effects |

[ENTRYPOINT_SCOPE]: ModelRoot — construction and mutation

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `ModelRoot.CreateModel()`                                      | factory  | creates an empty `ModelRoot`               |
|  [02]   | `ModelRoot.DeepClone()`                                        | instance | full structural clone                      |
|  [03]   | `ModelRoot.UseScene(string)`                                   | instance | creates or reuses a named or indexed scene |
|  [04]   | `ModelRoot.CreateMesh(string)`                                 | instance | creates a logical mesh                     |
|  [05]   | `ModelRoot.CreateMaterial(string)`                             | instance | creates a logical material                 |
|  [06]   | `ModelRoot.CreateAccessor(string)`                             | instance | creates a logical accessor                 |
|  [07]   | `ModelRoot.CreateAnimation(string)`                            | instance | creates an animation                       |
|  [08]   | `ModelRoot.UseBuffer(byte[])`                                  | instance | creates or reuses a buffer                 |
|  [09]   | `ModelRoot.UseBufferView(Buffer, int, int?, int, BufferMode?)` | instance | creates or reuses a buffer view            |
|  [10]   | `ModelRoot.MergeBuffers(int?)`                                 | instance | consolidates logical buffers               |
|  [11]   | `ModelRoot.IsolateMemory()`                                    | instance | refreshes internal memory buffers          |
|  [12]   | `ModelRoot.ApplyBasisTransform(Matrix4x4, string)`             | instance | applies a world transform to all scenes    |
|  [13]   | `ModelRoot.UseImage(MemoryImage)`                              | instance | creates or reuses an image                 |
|  [14]   | `ModelRoot.UseTexture(Image, TextureSampler?)`                 | instance | creates or reuses a texture                |
|  [15]   | `ModelRoot.UseTextureSampler(wrap, filter)`                    | instance | creates or reuses a texture sampler        |
|  [16]   | `ModelRoot.CreateSkin(string)`                                 | instance | creates a skin                             |
|  [17]   | `ModelRoot.CreatePunctualLight(string, PunctualLightType)`     | instance | creates a KHR punctual light               |

[ENTRYPOINT_SCOPE]: Animation — keyframe channel authoring
- carry: each TRS/morph channel takes `(Node, IReadOnlyDictionary<float, TValue> keyframes, bool linear = true)` — the caller supplies the float-seconds → value map; the channel allocates its own `AnimationSampler`, `linear` selects `LINEAR`/`STEP`, and a `(TangentIn, Value, TangentOut)` tuple-keyframe overload forces `CUBICSPLINE`; the visibility channel omits `linear` and is `STEP` by construction

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `Animation.CreateVisibilityChannel`                            | instance | `bool` → `KHR_node_visibility` per-node track, `STEP`      |
|  [02]   | `Animation.CreateScaleChannel`                                 | instance | `Vector3` → per-node scale TRS track                       |
|  [03]   | `Animation.CreateTranslationChannel`                           | instance | `Vector3` → per-node translation TRS track                 |
|  [04]   | `Animation.CreateRotationChannel`                              | instance | `Quaternion` → per-node rotation TRS track                 |
|  [05]   | `Animation.CreateMorphChannel`                                 | instance | `TWeights` → per-node morph-weight track, `int morphCount` |
|  [06]   | `Animation.CreateMaterialPropertyChannel(Material, string, …)` | instance | `KHR_animation_pointer` material-channel track             |
|  [07]   | `Animation.DangerousCreatePointerChannel(string, …)`           | instance | `KHR_animation_pointer` arbitrary-DOM target track         |

`KHR_node_visibility` and `KHR_animation_pointer` are in-box scene-rail extensions reached only through these channels, never named — the same `internal`-extension policy the material/texture rows hold, so the visibility channel's `KhrExtension` row carries `Registrar=None`.

[ENTRYPOINT_SCOPE]: SceneBuilder — mesh placement and output

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder)`                  | instance | mesh attached to an animatable node       |
|  [02]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, AffineTransform)`              | instance | mesh at a fixed world transform           |
|  [03]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder, AffineTransform)` | instance | mesh relative to a node                   |
|  [04]   | `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings?) -> ModelRoot`          | instance | converts this builder to a `ModelRoot`    |
|  [05]   | `SceneBuilder.ToGltf2(IEnumerable<SceneBuilder>, settings)`                | static   | converts multiple scenes to a `ModelRoot` |
|  [06]   | `SceneBuilder.AddScene(SceneBuilder, Matrix4x4)`                           | instance | merges another scene with an offset       |
|  [07]   | `SceneBuilder.ApplyBasisTransform(Matrix4x4, string)`                      | instance | transforms all instances in this scene    |
|  [08]   | `SceneBuilder.FindArmatures()`                                             | instance | unique armature roots                     |

[ENTRYPOINT_SCOPE]: MeshBuilder and PrimitiveBuilder — primitive assembly

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `MeshBuilder.UsePrimitive(material, int)`                           | instance | creates or reuses a primitive for material |
|  [02]   | `MeshBuilder.TransformVertices(Func<VertexBuilder, VertexBuilder>)` | instance | transforms all vertices in place           |
|  [03]   | `PrimitiveBuilder.AddTriangle(v0, v1, v2)`                          | instance | adds a triangle from three typed vertices  |
|  [04]   | `PrimitiveBuilder.AddQuadrangle(v0, v1, v2, v3)`                    | instance | adds a quad, auto-split to two triangles   |
|  [05]   | `PrimitiveBuilder.AddLine(v0, v1)`                                  | instance | adds a line segment                        |
|  [06]   | `PrimitiveBuilder.AddPoint(v0)`                                     | instance | adds a point                               |

[ENTRYPOINT_SCOPE]: MaterialBuilder — shader and channel configuration
- fluent: every surface returns `MaterialBuilder`, chaining shader selection, channel mutation, and fallback

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `WithMetallicRoughnessShader()`                         | instance | selects PBR metallic-roughness shader  |
|  [02]   | `WithSpecularGlossinessShader()`                        | instance | selects KHR specular-glossiness shader |
|  [03]   | `WithUnlitShader()`                                     | instance | selects KHR_materials_unlit shader     |
|  [04]   | `WithShader(string)`                                    | instance | selects a shader by name               |
|  [05]   | `UseChannel(KnownChannel/string) -> ChannelBuilder`     | instance | gets or creates a channel for mutation |
|  [06]   | `WithChannelParam(KnownChannel, KnownProperty, object)` | instance | sets a channel scalar parameter        |
|  [07]   | `WithChannelParam(KnownChannel, Vector4)`               | instance | sets a channel vector parameter        |
|  [08]   | `WithChannelImage(KnownChannel, ImageBuilder)`          | instance | binds a channel texture image          |
|  [09]   | `WithAlpha(AlphaMode = OPAQUE, float alphaCutoff)`      | instance | sets alpha mode and mask cutoff        |
|  [10]   | `WithDoubleSide(bool)`                                  | instance | enables back-face rendering            |
|  [11]   | `WithFallback(MaterialBuilder)`                         | instance | chains a fallback material             |

[ENTRYPOINT_SCOPE]: SceneTemplate and ArmatureInstance — runtime decode

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `SceneTemplate.Create(Scene, RuntimeOptions?)`         | factory  | templatizes a `Schema2.Scene`              |
|  [02]   | `SceneTemplate.CreateInstance()`                       | instance | creates an independent `SceneInstance`     |
|  [03]   | `ArmatureInstance.SetAnimationFrame(int, float, bool)` | instance | advances bone transforms to animation time |
|  [04]   | `ArmatureInstance.SetPoseTransforms()`                 | instance | resets all bones to rest pose              |
|  [05]   | `ArmatureInstance.SetLocalMatrix(string, Matrix4x4)`   | instance | overrides a bone's local-space matrix      |
|  [06]   | `ArmatureInstance.SetModelMatrix(string, Matrix4x4)`   | instance | overrides a bone's model-space matrix      |

[ENTRYPOINT_SCOPE]: MeshDecoder — decode reads
- carry: the `Get*` reads are extensions `(this IMeshPrimitiveDecoder, int vertexIdx[, int setIndex], IGeometryTransform xform)`

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `MeshDecoder.Decode(this Mesh/IReadOnlyList<Mesh>, RuntimeOptions?)` | static   | → `IMeshDecoder<Material>[]`                           |
|  [02]   | `MeshDecoder.GetPosition() -> Vector3`                               | static   | position, optionally transformed                       |
|  [03]   | `MeshDecoder.GetNormal()`/`GetTangent()`                             | static   | `Vector3`/`Vector4` normal/tangent, auto-gen if absent |
|  [04]   | `MeshDecoder.GetTextureCoord()`                                      | static   | `Vector2` UV for a texture set (+ `setIndex`)          |
|  [05]   | `MeshDecoder.GetColor()`                                             | static   | `Vector4` vertex color (+ `colorSetIndex`)             |
|  [06]   | `IMeshPrimitiveDecoder.GetSkinWeights(int)`                          | static   | → `SparseWeight8` (`SharpGLTF.Transforms`)             |
|  [07]   | `IMeshPrimitiveDecoder.TriangleIndices`                              | property | → `IEnumerable<(int,int,int)>` triangle index tuples   |

- `MeshDecoder.EvaluateBoundingSphere(this SceneTemplate, IMeshDecoder<Material>[], float)` (static) → `(Vector3 Center, float Radius)`, animation-aware.
- `MeshDecoder.EvaluateBoundingBox(this SceneInstance, IReadOnlyList<IMeshDecoder<TMat>>)` (static) → `(Vector3 Min, Vector3 Max)`, per-instance AABB after pose.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- I/O folds through `ModelRoot`: read enters `Load` (file), `ParseGLB` (bytes), or `ReadGLB` (stream); write enters `Save` (format by extension) or `WriteGLB` (bytes); `ReadSettings.Validation` and `WriteSettings.Validation` thread `ValidationMode` at both ends, and a custom URI resolver rides a `ReadContext` file-reader delegate set before `ReadSchema2`.
- Toolkit build folds `VertexBuilder<TvG,TvM,TvS>` (geometry + material + skinning fragment) → `MeshBuilder` → `SceneBuilder.AddRigidMesh` → `SceneBuilder.ToGltf2()` → `ModelRoot`; `SceneBuilderSchema2Settings` drives strided buffers, buffer merge, and GPU-instancing threshold, and `MaterialBuilder` mutates channels through `UseChannel(KnownChannel)`.
- Runtime decode folds `SceneTemplate.Create(scene)` → `CreateInstance()` → `SetAnimationFrame` per tick; `SceneInstance` enumerates `DrawableInstance` (its `Template.LogicalMeshIndex` selects the mesh, `Transform` carries the `IGeometryTransform`), and `LogicalMeshes.Decode()` yields `IMeshDecoder<Material>[]` whose normals and MikkTSpace tangents generate inside the decode under the `internal` `VertexNormalsFactory`/`VertexTangentsFactory` kernels.
- Every extension registers at `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` before any read or write that touches it; the KHR/MSFT/EXT material, texture, and scene extensions ship in-box, and a custom extension implements `JsonSerializable` and registers on the same factory.
- Core carries the extension framework but zero geometry codec: no type matches `KHR_draco_mesh_compression` or `EXT_meshopt_compression` in the assembly, so `RuntimeOptions.IsolateMemory`/`GpuMeshInstancing`/`ExtrasConverterCallback` is the single decode-policy carrier and encode routes to a sibling codec.

[STACKING]:
- `Openize.Drako`(`.api/api-openize-drako.md`) and `Alimer.Bindings.MeshOptimizer`(`.api/api-alimer-meshoptimizer.md`): the `ModelRoot` is authored uncompressed, then one export-codec dispatch row selects the Draco (`KHR_draco_mesh_compression`) or meshopt (`EXT_meshopt_compression`) encode leg, which rewrites the buffer-view payload — SharpGLTF owns the schema, the sibling owns the codec, both Compute-side outside Rhino.
- `SharpGLTF.Ext.3DTiles`(`.api/api-sharpgltf-3dtiles.md`): per-tile `EXT_structural_metadata`/`EXT_mesh_features` overlays register on the shared `ExtensionsFactory` and mutate the same `ModelRoot`/`MeshPrimitive`/`Node` this surface authors.
- `ProjNET`(`.api/api-projnet.md`): a decoded vertex span (`MeshDecoder.Decode` → `IMeshPrimitiveDecoder`) feeds the `Semantics/georeference` `MathTransform` batch reproject before frame normalization — the decode's `IGeometryTransform` arg and the ProjNET `Span<double>` batch are two stages of one ingest rail.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): a `ModelRoot.WriteGLB(WriteSettings) -> ArraySegment<byte>` segment feeds `XxHash3`/`XxHash128` through `Append` zero-copy — `XxHash3` the fast export-snapshot fingerprint, `XxHash128` the persisted GLB content key the `Rasm.Persistence` artifact index is addressed by, joining the same content-identity rail the IFC/CityJSON/FBX siblings hold.

[LOCAL_ADMISSION]:
- Export enters `SceneBuilder.ToGltf2()` → `ModelRoot.Save*`/`WriteGLB`; import enters `ModelRoot.Load*` or `ReadContext.ReadSchema2`; runtime evaluation enters `SceneTemplate.Create` → `CreateInstance` → animation frame drive.
- Extension admission registers at `ExtensionsFactory` before any read or write that uses that extension.

[RAIL_LAW]:
- Packages: `SharpGLTF.Core`, `SharpGLTF.Toolkit`, `SharpGLTF.Runtime`
- Owns: glTF read/write, typed mesh building, runtime scene instancing
- Accept: geometry exchange, asset authoring, runtime mesh evaluation
- Reject: rendering pipeline, GPU resource management, image decode, geometry codec
