# [RASM_API_SHARPGLTF]

`SharpGLTF` owns glTF 2.0 schema I/O, typed scene and mesh authoring, and runtime scene instancing: `SharpGLTF.Core` mints the read/write contexts and the `ModelRoot` logical-resource model, `SharpGLTF.Toolkit` folds typed vertex fragments through scene, mesh, and material builders into a `ModelRoot`, and `SharpGLTF.Runtime` templatizes a `Schema2.Scene` for per-instance animation decode. Core carries the extension framework but no geometry codec — Draco and meshopt encode ride sibling packages that rewrite the authored buffer views. Two folders compose the Core, Toolkit, and Runtime carriers — `Rasm.Bim` the exchange authoring and decode legs, `Rasm.Compute` the tile-partition composition root — and `ExtensionsFactory` is process-global mutable registration state both cross, so the whole distribution homes here beside its Tiles3D emitter surface at `api-sharpgltf-3dtiles.md`.

## [01]-[PUBLIC_TYPES]

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
- [05]-[WRITESET]: `WriteSettings` — `MergeBuffers` (default `true`, merges `LogicalBuffers` pre-serialize), `BuffersMaxSize` (merged-chunk byte cap, glTF-only when merging), `JsonIndented`/`JsonOptions` (STJ `JsonWriterOptions`), `ImageWriting` (`ResourceWriteMode`: `BufferView` embeds GLB-native, `EmbeddedAsBase64` embeds glTF-JSON only), `ImageWriteCallback` (per-image override), `JsonPostprocessor` (raw-JSON transform pass), `Validation` (`ValidationMode`, both read and write).

[PUBLIC_TYPE_SCOPE]: Schema2 scene graph and logical resources

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :----------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Scene`                        | class         | root nodes of a scene                                                  |
|  [02]   | `Node`                         | class         | scene-graph node (mesh, skin, TRS, children); members in `[02]-[NODE]` |
|  [03]   | `Mesh`                         | class         | set of `MeshPrimitive`; `Primitives`, `LogicalParent`                  |
|  [04]   | `MeshPrimitive`                | class         | geometry, material, attribute accessors; members in `[04]-[PRIM]`      |
|  [05]   | `Accessor`                     | class         | typed buffer-view element view; members in `[05]-[ACCESSOR]`           |
|  [06]   | `BufferView`                   | class         | contiguous `Buffer` subset; members in `[06]-[BUFVIEW]`                |
|  [07]   | `Buffer`                       | class         | raw binary blob (internal or external URI); members in `[07]-[BUFFER]` |
|  [08]   | `Material`                     | class         | PBR metallic-roughness and channel parameters through `FindChannel`    |
|  [09]   | `MaterialChannel`              | struct        | channel projection carrying texture or parameter values                |
|  [10]   | `Texture`                      | class         | texture and sampler binding                                            |
|  [11]   | `TextureSampler`               | class         | wrap and filter modes                                                  |
|  [12]   | `Image`                        | class         | image data; URI or buffer-view embedded                                |
|  [13]   | `Skin`                         | class         | joints and inverse-bind matrices for a skeletal mesh                   |
|  [14]   | `Animation`                    | class         | keyframe animation; owns channels and per-channel samplers             |
|  [15]   | `AnimationChannel`             | class         | binds a sampler to a node property; owns keyframes and channel target  |
|  [16]   | `AnimationChannelTarget`       | class         | the channel target descriptor: animated `Node` plus `PropertyPath`     |
|  [17]   | `MaterialPBRMetallicRoughness` | class         | metallic-roughness parameter block behind the `Material` channels      |

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

- [12]-[ATTRIBUTE_FORMAT]: readonly fields `Encoding`/`Dimensions`/`Normalized`/`ByteSize` plus `ByteSizePadded`; the static rows `Byte1`, `Float1`, `Float2`, `Float3`, `Float4`, `Float2x2`, `Float3x3`, `Float4x4` name a layout without spelling the `(EncodingType, DimensionType)` pair — `Float1` is the scalar declaration a custom per-vertex ordinal encodes through.

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

Every fragment interface derives `IVertexReflection`, so its own members are always the declared set PLUS `GetEncodingAttributes()`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                                      |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `IVertexReflection` | interface     | `GetEncodingAttributes()` → `IEnumerable<KeyValuePair<string, AttributeFormat>>`; the shared base |
|  [02]   | `IVertexGeometry`   | interface     | `: IVertexReflection`; members in `[04]-[VGEOMETRY]` — declares NO `Validate()`                   |
|  [03]   | `IVertexMaterial`   | interface     | `: IVertexReflection`; members in `[04]-[VMATERIAL]` — declares NO `Validate()`                   |
|  [04]   | `IVertexSkinning`   | interface     | `: IVertexReflection`; members in `[04]-[VSKINNING]`                                              |
|  [05]   | `IVertexCustom`     | interface     | `: IVertexMaterial`; members in `[04]-[VCUSTOM]` — the ONE interface declaring `Validate()`       |

- [04]-[VGEOMETRY]: `GetPosition()`, `TryGetNormal(out Vector3)`, `TryGetTangent(out Vector4)`, `SetPosition(in Vector3)`, `SetNormal(in Vector3)`, `SetTangent(in Vector4)`, `ApplyTransform(in Matrix4x4)`, and the morph pair `Subtract(IVertexGeometry)` → `VertexGeometryDelta` + `Add(in VertexGeometryDelta)`.
- [04]-[VMATERIAL]: `MaxColors`/`MaxTextCoords`, `GetColor(int)`/`GetTexCoord(int)`, `SetColor(int, Vector4)`/`SetTexCoord(int, Vector2)`, and the morph pair `Subtract(IVertexMaterial)` → `VertexMaterialDelta` + `Add(in VertexMaterialDelta)` — the two morph members are MANDATORY on every material fragment; `VertexEmpty` answers them `VertexMaterialDelta.Zero` and an empty body, the shape a channel-free stamp fragment copies.
- [04]-[VSKINNING]: `MaxBindings`, `JointsLow`/`JointsHigh`/`WeightsLow`/`WeightsHigh` (`Vector4`), `GetBinding(int)` → `(int Index, float Weight)`, `GetBindings()` → `SparseWeight8`, `SetBindings(in SparseWeight8)` and the `params (int, float)[]` overload.
- [04]-[VCUSTOM]: the `IVertexMaterial` set PLUS `CustomAttributes` (`IEnumerable<string>`), `Validate()`, `TryGetCustomAttribute(string, out object)`, `SetCustomAttribute(string, object)`. The toolkit ships NO concrete `IVertexCustom` implementor, so a `_FEATURE_ID_n` fragment owes the whole three-deep set — reflection, material, morph pair, custom, `Validate` — and declares its accessor layout through `GetEncodingAttributes` (`AttributeFormat.Float1` for a scalar feature ordinal, `Float2` beside it for a `TEXCOORD_0`), assembled through the `VertexBuilder<TvG,TvM,TvS>` `(in TvG, in TvM)` ctor overload.

[PUBLIC_TYPE_SCOPE]: Toolkit material and morph builders

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `MaterialBuilder`     | class         | root material; shader, alpha mode, double-sided, fallback             |
|  [02]   | `ChannelBuilder`      | class         | material channel; `TextureBuilder` and scalar parameters              |
|  [03]   | `TextureBuilder`      | class         | texture reference; primary/fallback images, transform, coord set      |
|  [04]   | `ImageBuilder`        | class         | in-memory image content with optional alternate write file name       |
|  [05]   | `AlphaMode`           | enum          | `OPAQUE`, `MASK`, `BLEND` — UPPERCASE members, not Pascal-cased       |
|  [06]   | `KnownChannel`        | enum          | typed channel key; `Diffuse`/`SpecularGlossiness` marked obsolete     |
|  [07]   | `KnownProperty`       | enum          | typed channel parameter key (`RGB`, `RGBA`, `MetallicFactor`, …)      |
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

## [02]-[ENTRYPOINTS]

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
|  [08]   | `WriteContext.WriteImage(string, MemoryImage)`             | instance | writes one satellite image to output   |
|  [09]   | `ModelRoot.GetJSON(bool) -> string`                        | instance | full JSON text, indented on `true`     |
|  [10]   | `ModelRoot.GetJsonPreview() -> string`                     | instance | JSON text preview without side effects |

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

[ENTRYPOINT_SCOPE]: Core Material — string-keyed channel authoring
- Core carries no `KnownChannel` (that enum is Toolkit) and its KHR material extension classes stay `internal`, so the public Core PBR surface is the string-keyed channel API on the `MaterialChannel` value struct — the read-side counterpart of the Toolkit fluent binders below.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Material.Channels -> IEnumerable<MaterialChannel>`            | property | all active channels on the material            |
|  [02]   | `Material.FindChannel(string) -> MaterialChannel?`             | instance | resolves one channel by key string             |
|  [03]   | `MaterialChannel.GetFactor(string)`/`SetFactor(string, float)` | instance | channel scalar factor get and set              |
|  [04]   | `MaterialChannel.SetTexture(int, Image, Image?, …)`            | instance | channel texture set with wrap and filter       |
|  [05]   | `MaterialChannel.SetTransform(Vector2, Vector2, float, int?)`  | instance | per-channel KHR_texture_transform UV transform |

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

`KHR_node_visibility` and `KHR_animation_pointer` are in-box scene-graph extensions reached only through these channels, never named — the same `internal`-extension policy the material/texture rows hold, so the visibility channel's `KhrExtension` row carries `Registrar=None`.

[ENTRYPOINT_SCOPE]: SceneBuilder — mesh placement and output

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder)`                  | instance | mesh attached to an animatable node       |
|  [02]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, AffineTransform)`              | instance | mesh at a fixed world transform           |
|  [03]   | `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder, AffineTransform)` | instance | mesh relative to a node                   |
|  [04]   | `SceneBuilder.AddSkinnedMesh(IMeshBuilder<M>, Matrix4x4, NodeBuilder[])`   | instance | skinned mesh with a joint armature        |
|  [05]   | `SceneBuilder.AddCamera(CameraBuilder, NodeBuilder)`                       | instance | camera at a node or look-at framing       |
|  [06]   | `SceneBuilder.AddLight(LightBuilder, NodeBuilder)`                         | instance | punctual light at a node or transform     |
|  [07]   | `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings?) -> ModelRoot`          | instance | converts this builder to a `ModelRoot`    |
|  [08]   | `SceneBuilder.ToGltf2(IEnumerable<SceneBuilder>, settings)`                | static   | converts multiple scenes to a `ModelRoot` |
|  [09]   | `SceneBuilder.AddScene(SceneBuilder, Matrix4x4)`                           | instance | merges another scene with an offset       |
|  [10]   | `SceneBuilder.ApplyBasisTransform(Matrix4x4, string)`                      | instance | transforms all instances in this scene    |
|  [11]   | `SceneBuilder.FindArmatures()`                                             | instance | unique armature roots                     |

[ENTRYPOINT_SCOPE]: MeshBuilder and PrimitiveBuilder — primitive assembly

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `MeshBuilder.UsePrimitive(material, int)`                           | instance | creates or reuses a primitive for material |
|  [02]   | `MeshBuilder.TransformVertices(Func<VertexBuilder, VertexBuilder>)` | instance | transforms all vertices in place           |
|  [03]   | `PrimitiveBuilder.AddTriangle(v0, v1, v2)`                          | instance | adds a triangle from three typed vertices  |
|  [04]   | `PrimitiveBuilder.AddQuadrangle(v0, v1, v2, v3)`                    | instance | adds a quad, auto-split to two triangles   |
|  [05]   | `PrimitiveBuilder.AddLine(v0, v1)`                                  | instance | adds a line segment                        |
|  [06]   | `PrimitiveBuilder.AddPoint(v0)`                                     | instance | adds a point                               |
|  [07]   | `PrimitiveBuilder.UseVertex(ref VertexBuilder<vG,vM,vS>)`           | instance | adds or reuses a vertex, returns its index |

[ENTRYPOINT_SCOPE]: MaterialBuilder — shader and channel configuration
- fluent: every surface returns `MaterialBuilder`, chaining shader selection, channel mutation, and fallback

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `WithMetallicRoughnessShader()`                         | instance | selects PBR metallic-roughness shader  |
|  [02]   | `WithSpecularGlossinessShader()`                        | instance | OBSOLETE — Khronos deprecated the ext  |
|  [03]   | `WithUnlitShader()`                                     | instance | selects KHR_materials_unlit shader     |
|  [04]   | `WithShader(string)`                                    | instance | selects a shader by name               |
|  [05]   | `UseChannel(KnownChannel) -> ChannelBuilder`            | instance | gets or creates a channel for mutation |
|  [06]   | `UseChannel(string) -> ChannelBuilder`                  | instance | OBSOLETE — prefer the typed overload   |
|  [07]   | `WithChannelParam(KnownChannel, KnownProperty, object)` | instance | sets a channel scalar parameter        |
|  [08]   | `WithChannelParam(KnownChannel, Vector4)`               | instance | OBSOLETE — use the typed-property form |
|  [09]   | `WithChannelImage(KnownChannel, ImageBuilder)`          | instance | binds a channel texture image          |
|  [10]   | `WithChannelImage(string, ImageBuilder)`                | instance | OBSOLETE — use the typed overload      |
|  [11]   | `WithAlpha(AlphaMode = OPAQUE, float alphaCutoff)`      | instance | sets alpha mode and mask cutoff        |
|  [12]   | `WithDoubleSide(bool)`                                  | instance | enables back-face rendering            |
|  [13]   | `WithFallback(MaterialBuilder)`                         | instance | chains a fallback material             |

[OBSOLETE_MATERIAL_SPELLINGS]: the `[Obsolete]` rows above are the live compiler's own verdict, not a style note — `WithChannelParam(KnownChannel, Vector4)` and `WithChannelImage(string, …)` both redirect to their typed successors, and `WithSpecularGlossinessShader` beside the `KnownChannel.Diffuse`/`SpecularGlossiness` members carries Khronos's own deprecation of `KHR_materials_pbrSpecularGlossiness`. A composing fence takes `WithBaseColor`/`WithMetallicRoughness`/`WithSpecularColor` or the typed `WithChannelParam(KnownChannel, KnownProperty, object)`.

[ENTRYPOINT_SCOPE]: MaterialBuilder — per-channel binders

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `WithBaseColor(Vector4)` · `WithBaseColor(ImageBuilder, Vector4?)`   | instance | base-colour factor and/or map            |
|  [02]   | `WithMetallicRoughness(float?, float?)` · `(ImageBuilder, …)`        | instance | metallic/roughness factors and/or map    |
|  [03]   | `WithNormal(ImageBuilder, float scale = 1)`                          | instance | normal map with scale                    |
|  [04]   | `WithOcclusion(ImageBuilder, float strength = 1)`                    | instance | occlusion map with strength              |
|  [05]   | `WithEmissive(Vector3, float)` · `(ImageBuilder, Vector3?, float)`   | instance | emissive factor and/or map with strength |
|  [06]   | `WithTransmission(ImageBuilder, float intensity)`                    | instance | KHR_materials_transmission               |
|  [07]   | `WithClearCoat(ImageBuilder, float)` · `…Normal` · `…Roughness`      | instance | KHR_materials_clearcoat channel trio     |
|  [08]   | `WithSpecularColor(ImageBuilder, Vector3?)` · `WithSpecularFactor`   | instance | KHR_materials_specular pair              |
|  [09]   | `WithVolumeThickness(ImageBuilder, float)` · `WithVolumeAttenuation` | instance | KHR_materials_volume pair                |
|  [10]   | `WithIridescence(ImageBuilder, float, float)` · `…Thickness`         | instance | KHR_materials_iridescence pair           |
|  [11]   | `WithAnisotropy(ImageBuilder, float strength, float rotation)`       | instance | KHR_materials_anisotropy                 |
|  [12]   | `WithDiffuseTransmissionFactor` · `WithDiffuseTransmissionColor`     | instance | KHR_materials_diffuse_transmission pair  |
|  [13]   | `WithMetallicRoughnessFallback(ImageBuilder, Vector4?, …)`           | instance | specular-glossiness compatibility path   |

[MATERIAL_STATE_MEMBERS]: `MaterialBuilder` carries `AlphaMode`, `AlphaCutoff` (default `0.5f`), `DoubleSided`, `ShaderStyle`, `IndexOfRefraction` (default `1.5f`), `Dispersion`, `Channels` (`IReadOnlyCollection<ChannelBuilder>`), and `CompatibilityFallback`; the shader-style constants are `SHADERUNLIT`, `SHADERPBRMETALLICROUGHNESS`, and `SHADERPBRSPECULARGLOSSINESS`. Equality is REFERENCE by default — `AreEqualByContent`/`ContentComparer` is the content form — so a pool keyed on material identity supplies its own key rather than trusting `Equals`.

[ENTRYPOINT_SCOPE]: ChannelBuilder and TextureBuilder — texture binding; `ImageBuilder` converts implicitly from `byte[]`, `ArraySegment<byte>`, and a file path

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `ChannelBuilder.Key` · `.Parameters` · `.Texture`                          | instance | channel key, parameter set, bound texture |
|  [02]   | `ChannelBuilder.UseTexture() -> TextureBuilder`                            | instance | gets or creates the channel's texture     |
|  [03]   | `ChannelBuilder.GetValidTexture()` · `.RemoveTexture()`                    | instance | image-bearing probe; unbind               |
|  [04]   | `TextureBuilder.WithPrimaryImage(ImageBuilder)` · `.WithFallbackImage(…)`  | instance | primary and fallback image content        |
|  [05]   | `TextureBuilder.WithCoordinateSet(int)`                                    | instance | selects the sampled UV set                |
|  [06]   | `TextureBuilder.WithSampler(TextureWrapMode ws, TextureWrapMode wt)`       | instance | wrap pair; both filters left unset        |
|  [07]   | `WithSampler(…, TextureMipMapFilter min, TextureInterpolationFilter mag)`  | instance | the explicit min/mag filter pair          |
|  [08]   | `TextureBuilder.WithTransform(Vector2 offset, Vector2 scale, float, int?)` | instance | KHR_texture_transform frame               |
|  [09]   | `TextureBuilder.WithTransform(float ox, float oy, float sx, float sy, …)`  | instance | the scalar-argument transform form        |
|  [10]   | `ImageBuilder.From(MemoryImage, string name)`                              | factory  | in-memory image content                   |

[TEXTURE_BINDING_MEMBERS]: `TextureBuilder` carries `CoordinateSet`, `MinFilter`, `MagFilter`, `WrapS`/`WrapT` (both defaulting to `TextureWrapMode.REPEAT`), `PrimaryImage`, `FallbackImage`, and `Transform` (`TextureTransformBuilder` with `Offset`, `Scale`, `Rotation`, `CoordinateSetOverride`). `PrimaryImage` reads PNG, JPG, DDS, WEBP, and KTX2 while `FallbackImage` reads PNG and JPG alone, so a `KHR_texture_basisu`/`EXT_texture_webp`/`MSFT_texture_dds` primary pairs with a core-format fallback for a consumer lacking the extension. `MemoryImage` wraps an `ArraySegment<byte>` with no copy and exposes `FileExtension`, `MimeType`, `IsValid`, and `IsImageOfType(string)`; `KnownProperty` keys the channel parameter set (`Unknown`, `RGB`, `RGBA`, `Minimum`, `Maximum`, `NormalScale`, `OcclusionStrength`, `EmissiveStrength`, `IndexOfRefraction`, `MetallicFactor`, `RoughnessFactor`, `SpecularFactor`, `GlossinessFactor`, `ClearCoatFactor`, `ThicknessFactor`, `TransmissionFactor`, `IridescenceFactor`, `AttenuationDistance`, `DiffuseTransmissionFactor`, `AnisotropyStrength`, `AnisotropyRotation`). SharpGLTF encodes no image: the bytes arrive already sealed in their container. `TextureWrapMode` (`REPEAT`/`CLAMP_TO_EDGE`/`MIRRORED_REPEAT`) is public schema vocabulary a binder spells directly; `WithSampler`'s filter parameters are typed `TextureMipMapFilter min` and `TextureInterpolationFilter mag`, each defaulting to the unset `0`, so the two-argument wrap-only call compiles.

[CHANNEL_FACTOR_DEFAULTS]: `MaterialValue.CreateDefaultProperties(KnownChannel)` seeds each channel's parameter defaults, decompile-verified — the KHR extension factors seed ZERO: `ClearCoat` `ClearCoatFactor` 0 · `ClearCoatRoughness` `RoughnessFactor` 0 · `Transmission` `TransmissionFactor` 0 · `SheenColor` `RGB` zero · `SheenRoughness` `RoughnessFactor` 0 · `Iridescence` `IridescenceFactor` 0 (+ `IndexOfRefraction` 1.3) · `Anisotropy` `AnisotropyStrength` 0 · `Emissive` `RGB` zero (+ `EmissiveStrength` 1) · `IridescenceThickness` `Minimum` 100 / `Maximum` 400 (nm). A bound texture on any of these channels MULTIPLIES the zero factor and renders as a no-op until the binder writes the unit factor through `WithChannelParam` — the core channels (`BaseColor` `RGBA` one, `MetallicRoughness` 1/1, `Normal` `NormalScale` 1, `Occlusion` `OcclusionStrength` 1) seed neutral and need no write.

[CHANNEL_KEY_ROSTER]: `KnownChannel` is the closed channel vocabulary `UseChannel`/`GetChannel`/`WithChannelParam`/`WithChannelImage` key on — `Normal`, `Occlusion`, `Emissive`, `BaseColor`, `MetallicRoughness`, `Diffuse` (`[Obsolete]`), `SpecularGlossiness` (`[Obsolete]`), `ClearCoat`, `ClearCoatNormal`, `ClearCoatRoughness`, `Transmission`, `SheenColor`, `SheenRoughness`, `SpecularColor`, `SpecularFactor`, `VolumeThickness`, `VolumeAttenuation`, `Iridescence`, `IridescenceThickness`, `Anisotropy`, `DiffuseTransmissionColor`, `DiffuseTransmissionFactor`. Each member is a distinct glTF texture and factor slot, so one source image feeding two slots — an occlusion-roughness-metalness pack reaching `Occlusion` beside `MetallicRoughness` — binds ONE `ImageBuilder` through two `UseChannel` calls rather than duplicating the bytes; the two obsolete members carry Khronos's `KHR_materials_pbrSpecularGlossiness` deprecation and are read-only import vocabulary.

[MATERIAL_STATE_WRITES]: the scalar material state writes through properties rather than `With*` members — `AlphaMode` and `AlphaCutoff` (`WithAlpha(AlphaMode = OPAQUE, float alphaCutoff = 0.5f)` is the fluent form), `DoubleSided` (`WithDoubleSide(bool)`), `IndexOfRefraction` (default `1.5f`, so writing that value serializes a `KHR_materials_ior` block asserting the default), and `Dispersion`. The factor defaults are the glTF spec's, NOT neutral: an unwritten `MetallicRoughness` channel renders metallic `1.0` and roughness `1.0`, so `WithMetallicRoughness(float?, float?)` is written on every dielectric material rather than left to the format.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- I/O folds through `ModelRoot`: read enters `Load` (file), `ParseGLB` (bytes), or `ReadGLB` (stream); write enters `Save` (format by extension) or `WriteGLB` (bytes); `ReadSettings.Validation` and `WriteSettings.Validation` thread `ValidationMode` at both ends, and a custom URI resolver rides a `ReadContext` file-reader delegate set before `ReadSchema2`.
- Toolkit build folds `VertexBuilder<TvG,TvM,TvS>` (geometry + material + skinning fragment) → `MeshBuilder` → `SceneBuilder.AddRigidMesh` → `SceneBuilder.ToGltf2()` → `ModelRoot`; `SceneBuilderSchema2Settings` drives strided buffers, buffer merge, and GPU-instancing threshold, `MaterialBuilder` mutates channels through `UseChannel(KnownChannel)`, and `VertexBufferColumns.CalculateSmoothNormals`/`CalculateTangents` generate the normal and tangent columns a source mesh omits.
- Runtime decode folds `SceneTemplate.Create(scene)` → `CreateInstance()` → `SetAnimationFrame` per tick; `SceneInstance` enumerates `DrawableInstance` (its `Template.LogicalMeshIndex` selects the mesh, `Transform` carries the `IGeometryTransform`), and `LogicalMeshes.Decode()` yields `IMeshDecoder<Material>[]` whose normals and MikkTSpace tangents generate inside the decode under the `internal` `VertexNormalsFactory`/`VertexTangentsFactory` kernels.
- Every extension registers at `ExtensionsFactory.RegisterExtension<TParent,TExt>(name, factory)` before any read or write that touches it, the `Func<TParent, JsonSerializable>` argument supplying the instance; the name-only overload carries `[Obsolete]` naming this one, and the package's own in-box registrations all take the factory. KHR, MSFT, and EXT material, texture, and scene extensions ship registered, and a custom extension implements `JsonSerializable` and registers on the same factory.
- Core carries the extension framework but zero geometry codec: no type matches `KHR_draco_mesh_compression` or `EXT_meshopt_compression` in the assembly, so `RuntimeOptions.IsolateMemory`/`GpuMeshInstancing`/`ExtrasConverterCallback` is the single decode-policy carrier and encode routes to a sibling codec.

[STACKING]:
- `Openize.Drako`(`Rasm.Bim/.api/api-openize-drako.md`) and `Alimer.Bindings.MeshOptimizer`(`api-alimer-meshoptimizer.md`): the `ModelRoot` is authored uncompressed, then one export-codec dispatch row selects the Draco (`KHR_draco_mesh_compression`) or meshopt (`EXT_meshopt_compression`) encode leg, which rewrites the buffer-view payload — SharpGLTF owns the schema, the sibling owns the codec, both Compute-side outside Rhino.
- `SharpGLTF.Ext.3DTiles`(`api-sharpgltf-3dtiles.md`): per-tile `EXT_structural_metadata`/`EXT_mesh_features` overlays register on the shared `ExtensionsFactory` and mutate the same `ModelRoot`/`MeshPrimitive`/`Node` this surface authors.
- `ProjNET`(`Rasm.Bim/.api/api-projnet.md`): a decoded vertex span (`MeshDecoder.Decode` → `IMeshPrimitiveDecoder`) feeds the `Semantics/georeference` `MathTransform` batch reproject before frame normalization — the decode's `IGeometryTransform` arg and the ProjNET `Span<double>` batch are two stages of one ingest path.
- `System.IO.Hashing`(`api-hashing.md`): a `ModelRoot.WriteGLB(WriteSettings) -> ArraySegment<byte>` segment feeds `XxHash3`/`XxHash128` through `Append` zero-copy — `XxHash3` the fast export-snapshot fingerprint, `XxHash128` the persisted GLB content key the `Rasm.Persistence` artifact index is addressed by, joining the same content-identity path the IFC/CityJSON/FBX siblings hold.
- Bim consumer anchor: `Exchange/export` folds the Toolkit build head into `ModelRoot.Save*`/`WriteGLB`, and `Exchange/export#TILE_METADATA` is the one fence authoring the Tiles3D overlay on the tessellated `ModelRoot`.
- Compute consumer anchor: `Runtime/tiles#TILE_PARTITION` owns the tile pyramid, the geometric-error ladder, the content keys, and the tileset.json manifest through its own pooled `Utf8JsonWriter`, yielding one typed `LeafContent` per leaf naming a `{contentKey:x32}.glb` URI; it emits no glTF body, so `Tiles3DExtensions.RegisterExtensions()` at the composition root is its whole reach into this distribution and no `ModelRoot`, `SceneBuilder`, or `MeshDecoder` member is live there. `TileMetadata`, `PropertyTable`, and `MetadataProperty` at that page are Rasm records over the element graph, never the same-named `SharpGLTF.Schema2.Tiles3D` types — the Rasm columns lower onto the package types at the Bim authoring fence alone.

[LOCAL_ADMISSION]:
- Export enters `SceneBuilder.ToGltf2()` → `ModelRoot.Save*`/`WriteGLB`; import enters `ModelRoot.Load*` or `ReadContext.ReadSchema2`; runtime evaluation enters `SceneTemplate.Create` → `CreateInstance` → animation frame drive.
- Extension admission registers at `ExtensionsFactory` before any read or write that uses that extension, ONCE at a composition root — a per-tile or per-call registration is the deleted form.
