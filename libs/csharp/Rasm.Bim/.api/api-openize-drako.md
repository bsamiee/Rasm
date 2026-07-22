# [RASM_BIM_API_DRAKO]

`Openize.Drako` owns managed Draco 3D geometry compression: the `Draco` facade encodes and decodes `DracoMesh` (triangulated) and `DracoPointCloud` (unstructured points) over typed per-vertex `PointAttribute` channels, per-attribute quantization bit controls, and optional geometry metadata. It feeds the Compute glTF export rail as the `KHR_draco_mesh_compression` codec leg.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Openize.Drako`
- package: `Openize.Drako` (commercial Openize)
- assembly: `Openize.Drako`
- namespace: `Openize.Drako`
- asset: net8.0, net7.0, net6.0, netstandard2.1, net46 — `net10.0` consumer binds `lib/net8.0`
- asset: IL-only AnyCPU managed, no `runtimes/` native; the bound net8.0 group has zero package deps (`System.Memory`/`System.Numerics.Vectors` ride the net46 fallback only)
- license: requireLicenseAcceptance; admitted for the Compute `EXPORT_RAIL`, outside-Rhino only
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: encode/decode facade

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                                                |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `Draco`          | static facade | encode `DracoPointCloud`/`DracoMesh` → `byte[]`/stream; decode `byte[]` → `DracoPointCloud` |
|  [02]   | `DrakoException` | failure type  | thrown on decode format or constraint violation                                             |

[PUBLIC_TYPE_SCOPE]: geometry model

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                      |
| :-----: | :------------------ | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `DracoPointCloud`   | mutable class   | root point cloud; owns attribute list and geometry metadata       |
|  [02]   | `DracoMesh`         | mutable class   | extends `DracoPointCloud`; owns indexed triangle face list        |
|  [03]   | `PointAttribute`    | attribute class | extends `GeometryAttribute`; carries typed attribute buffer data  |
|  [04]   | `GeometryAttribute` | base class      | attribute descriptor: type, data type, components, stride, offset |

[PUBLIC_TYPE_SCOPE]: attribute enums — element / channel / scalar-width vocabulary

| [INDEX] | [SYMBOL]                   | [CASES]                                                                                 |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `AttributeType`            | `Invalid`, `Position`, `Normal`, `Color`, `TexCoord`, `Generic`, `NamedAttributesCount` |
|  [02]   | `DataType`                 | scalar width; roster in `[02]-[DATATYPE]`                                               |
|  [03]   | `MeshAttributeElementType` | `Vertex`, `Corner`, `Face`                                                              |

- [02]-[DATATYPE]: `INVALID`, `INT8`, `UINT8`, `INT16`, `UINT16`, `INT32`, `UINT32`, `INT64`, `UINT64`, `FLOAT32`, `FLOAT64`, `BOOL`, `TYPESCOUNT`.

[PUBLIC_TYPE_SCOPE]: encode options

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `DracoEncodeOptions`    | mutable class | quantization bits per attribute type, compression level, point-cloud mode |
|  [02]   | `DracoCompressionLevel` | enum          | `NoCompression`, `Fast`, `Standard`, `Optimal`                            |

[PUBLIC_TYPE_SCOPE]: metadata

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                                |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `Metadata`         | mutable class | `Entries` `Dictionary<string, byte[]>`; nested `SubMetadata` `Dictionary<string, Metadata>` |
|  [02]   | `GeometryMetadata` | mutable class | extends `Metadata`; `AttributeMetadata` `Dictionary<int, Metadata>` keyed by attribute id   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Draco` — encode/decode

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `Draco.Decode(byte[])`                                      | static  | returns `DracoPointCloud` or `DracoMesh` |
|  [02]   | `Draco.Encode(DracoPointCloud)`                             | static  | default options → `byte[]`               |
|  [03]   | `Draco.Encode(DracoPointCloud, DracoEncodeOptions)`         | static  | explicit options → `byte[]`              |
|  [04]   | `Draco.Encode(DracoPointCloud, DracoEncodeOptions, Stream)` | static  | writes encoded bytes to stream           |

[ENTRYPOINT_SCOPE]: `DracoPointCloud` — attribute and metadata management

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `AddAttribute(PointAttribute)`                            | instance | canonical `virtual` add the encode intake calls            |
|  [02]   | `AddAttribute(GeometryAttribute, bool, int)`              | instance | explicit identity-mapping + value count                    |
|  [03]   | `Attribute(int) -> PointAttribute`                        | instance | resolve a `PointAttribute` by id                           |
|  [04]   | `NumPoints`                                               | property | get/set number of points in the cloud                      |
|  [05]   | `NumAttributes`                                           | property | number of attributes on the cloud                          |
|  [06]   | `DeduplicateAttributeValues()`                            | instance | collapse duplicate attribute values in-place before encode |
|  [07]   | `DeduplicatePointIds()`                                   | instance | merge duplicate point identities                           |
|  [08]   | `GetNamedAttribute(AttributeType, int) -> PointAttribute` | instance | i-th attribute of a named type (decode read path)          |
|  [09]   | `GetNamedAttributeId(AttributeType, int) -> int`          | instance | presence gate before filling a vertex accessor             |

- `AddAttribute`: both overloads return the new attribute id.
- `GetNamedAttribute` returns null and `GetNamedAttributeId` returns a negative id when the named attribute is absent.

[ENTRYPOINT_SCOPE]: `DracoMesh` — face management

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `NumFaces`                                                 | property | get/set number of triangular faces         |
|  [02]   | `Indices -> IntList`                                       | property | corner indices, 3 per face                 |
|  [03]   | `AddFace(int[])`                                           | instance | appends triangle from 3-element array      |
|  [04]   | `SetFace(int, int[])`                                      | instance | overwrites face; grows list if needed      |
|  [05]   | `ReadFace(int, int[])`                                     | instance | reads triangle indices into provided array |
|  [06]   | `ReadFace(int, Span<int>)`                                 | instance | reads triangle indices into span           |
|  [07]   | `GetAttributeElementType(int) -> MeshAttributeElementType` | instance | element type of an attribute               |

[ENTRYPOINT_SCOPE]: `PointAttribute` — factory and data access

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `PointAttribute.Wrap(AttributeType, Span<Vector2>)`                        | factory  | wraps a FLOAT32 2-component buffer             |
|  [02]   | `PointAttribute.Wrap(AttributeType, Span<Vector3>)`                        | factory  | wraps a FLOAT32 3-component buffer             |
|  [03]   | `PointAttribute.Wrap(AttributeType, int, float[])`                         | factory  | wraps a FLOAT32 N-component buffer             |
|  [04]   | `PointAttribute(AttributeType, DataType, int, bool, int, int, DataBuffer)` | ctor     | full descriptor + buffer construction          |
|  [05]   | `NumUniqueEntries`                                                         | property | unique attribute values in the backing buffer  |
|  [06]   | `IdentityMapping`                                                          | property | `true` identity mapping; `false` explicit map  |
|  [07]   | `GetValueAsVector3(int) -> Vector3`                                        | instance | read an attribute-value index as a `Vector3`   |
|  [08]   | `MappedIndex(int) -> int`                                                  | instance | map a point index to its attribute-value index |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DracoMesh: DracoPointCloud` — a triangulated mesh is a point cloud with an indexed face list; `Draco.Decode` returns `DracoMesh` for meshes and `DracoPointCloud` for clouds, discriminated by `is DracoMesh`.
- `Draco` facade drives the internal `.Encoder`/`.Decoder`/`.Compression` pipelines; encoder selection (`DracoEncodingMethod`, `EncodedGeometryType`) resolves automatically from geometry type and compression level.
- consumer surface is the root `Openize.Drako` namespace with the `Openize.Drako.Utils` carriers reached through it — `DataBuffer` (the `PointAttribute` buffer and `GeometryAttribute.Buffer`) and `IntList` (`DracoMesh.Indices`).
- encode quantization defaults: `PositionBits=11`, `TextureCoordinateBits=12`, `NormalBits=10`, `ColorBits=10`, `CompressionLevel=Standard`.
- `AttributeType.Invalid` and `NamedAttributesCount` bound the `Position`..`Generic` named channels as sentinels.

[STACKING]:
- `SharpGLTF.Core`(`.api/api-sharpgltf.md`): `Draco.Encode(mesh, opts) -> byte[]` is the bufferView payload SharpGLTF references under the `KHR_draco_mesh_compression` extension.
- `Alimer.Bindings.MeshOptimizer`(`.api/api-alimer-meshoptimizer.md`): the sibling `EXT_meshopt_compression` leg; one export-codec dispatch row selects Draco vs meshopt by extension policy, both emitting `byte[]`/`Span<byte>` the same glTF buffer writer absorbs.
- within-lib: project the canonical triangle-soup (positions/normals/UVs + index buffer) through `PointAttribute.Wrap(AttributeType.Position, Span<Vector3>)` / `Wrap(AttributeType.TexCoord, Span<Vector2>)` and `DracoMesh.AddFace(int[])`, feeding Draco intake from the same buffers meshopt consumes; a Compute codec rail wraps `Draco.Encode`/`Draco.Decode` in the `Fin`/`Eff` rail, maps `DrakoException` to a typed codec fault, and emits the pre/post byte-count compression-ratio receipt.

[LOCAL_ADMISSION]:
- Encode: populate `DracoMesh`/`DracoPointCloud` through `PointAttribute.Wrap` factories, then `Draco.Encode` with explicit `DracoEncodeOptions`.
- Decode: `Draco.Decode(byte[])` returns `DracoPointCloud`; downcast to `DracoMesh` for face indices.
- Read attribute buffers through `DracoPointCloud.Attribute(id).Buffer`.

[RAIL_LAW]:
- Package: `Openize.Drako`
- Owns: Draco encode/decode for mesh and point-cloud geometry with typed attribute channels and metadata
- Accept: managed `Span<Vector2/3>` and `float[]` intake through `PointAttribute.Wrap`; `byte[]` or `Stream` for encode egress
- Reject: hand-rolled bit-packing, direct `DracoHeader` parsing, or encoder selection bypassing the `Draco` facade
