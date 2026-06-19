# [RASM_BIM_API_DRAKO]

`Openize.Drako` supplies managed Draco 3D geometry compression: encode and decode
for both `DracoMesh` (triangulated geometry) and `DracoPointCloud` (unstructured
points), with typed per-vertex `PointAttribute` channels, quantization bit controls,
and optional geometry metadata, exposing the full encode/decode surface through the
static `Draco` facade for Bim mesh interchange rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Openize.Drako`
- package: `Openize.Drako`
- assembly: `Openize.Drako`
- namespace: `Openize.Drako`
- asset: net8.0, net7.0, net6.0, netstandard2.1, net46
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: encode/decode facade
- rail: geometry

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [ROLE]                                                                                      |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `Draco`          | static facade | encode `DracoPointCloud`/`DracoMesh` → `byte[]`/stream; decode `byte[]` → `DracoPointCloud` |
|  [02]   | `DrakoException` | failure type  | thrown on decode format or constraint violation                                             |

[PUBLIC_TYPE_SCOPE]: geometry model
- rail: geometry

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [ROLE]                                                            |
| :-----: | :------------------ | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `DracoPointCloud`   | mutable class   | root point cloud; owns attribute list and geometry metadata       |
|  [02]   | `DracoMesh`         | mutable class   | extends `DracoPointCloud`; owns indexed triangle face list        |
|  [03]   | `PointAttribute`    | attribute class | extends `GeometryAttribute`; carries typed attribute buffer data  |
|  [04]   | `GeometryAttribute` | base class      | attribute descriptor: type, data type, components, stride, offset |

[PUBLIC_TYPE_SCOPE]: attribute vocabulary
- rail: geometry

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CASES]                                                                                                                         |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `AttributeType`            | enum          | `Invalid`, `Position`, `Normal`, `Color`, `TexCoord`, `Generic`, `NamedAttributesCount`                                         |
|  [02]   | `DataType`                 | enum          | `INVALID`, `INT8`, `UINT8`, `INT16`, `UINT16`, `INT32`, `UINT32`, `INT64`, `UINT64`, `FLOAT32`, `FLOAT64`, `BOOL`, `TYPESCOUNT` |
|  [03]   | `MeshAttributeElementType` | enum          | `Vertex`, `Corner`, `Face`                                                                                                      |

[PUBLIC_TYPE_SCOPE]: encode options
- rail: geometry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [ROLE]                                                                    |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `DracoEncodeOptions`    | mutable class | quantization bits per attribute type, compression level, point-cloud mode |
|  [02]   | `DracoCompressionLevel` | enum          | `NoCompression`, `Fast`, `Standard`, `Optimal`                            |

[PUBLIC_TYPE_SCOPE]: metadata
- rail: geometry

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [ROLE]                                                        |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Metadata`         | mutable class | string-keyed `byte[]` entries and nested `SubMetadata` map    |
|  [02]   | `GeometryMetadata` | mutable class | extends `Metadata`; adds per-attribute-id `AttributeMetadata` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Draco` — encode/decode
- rail: geometry

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------- |
|  [01]   | `Draco.Decode(byte[] data)`                                   | static decode  | returns `DracoPointCloud` or `DracoMesh` |
|  [02]   | `Draco.Encode(DracoPointCloud m)`                             | static encode  | default options → `byte[]`               |
|  [03]   | `Draco.Encode(DracoPointCloud m, DracoEncodeOptions opts)`    | static encode  | explicit options → `byte[]`              |
|  [04]   | `Draco.Encode(DracoPointCloud m, DracoEncodeOptions, Stream)` | static encode  | writes encoded bytes to stream           |

[ENTRYPOINT_SCOPE]: `DracoPointCloud` — attribute and metadata management
- rail: geometry

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :----------------------------------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `AddAttribute(PointAttribute pa)`                                  | attribute add    | appends attribute; returns attribute id     |
|  [02]   | `AddAttribute(GeometryAttribute att, bool identityMapping, int n)` | attribute add    | appends with mapping policy and value count |
|  [03]   | `Attribute(int attId)`                                             | attribute access | returns `PointAttribute` by id              |
|  [04]   | `NumPoints` property                                               | point count      | get/set number of points in cloud           |
|  [05]   | `NumAttributes` property                                           | attribute count  | number of attributes on the cloud           |
|  [06]   | `DeduplicateAttributeValues()`                                     | deduplication    | removes duplicate attribute values in-place |
|  [07]   | `DeduplicatePointIds()`                                            | deduplication    | merges duplicate point identities           |

[ENTRYPOINT_SCOPE]: `DracoMesh` — face management
- rail: geometry

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `NumFaces` property                    | face count     | get/set number of triangular faces          |
|  [02]   | `Indices` property                     | raw indices    | `IntList` of corner indices (3 per face)    |
|  [03]   | `AddFace(int[] face)`                  | face add       | appends triangle from 3-element array       |
|  [04]   | `SetFace(int faceId, int[] face)`      | face write     | overwrites face; grows list if needed       |
|  [05]   | `ReadFace(int faceId, int[] face)`     | face read      | reads triangle indices into provided array  |
|  [06]   | `ReadFace(int faceId, Span<int> face)` | face read      | reads triangle indices into span            |
|  [07]   | `GetAttributeElementType(int attId)`   | attribute meta | returns `MeshAttributeElementType` for attr |

[ENTRYPOINT_SCOPE]: `PointAttribute` — factory and data access
- rail: geometry

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `PointAttribute.Wrap(AttributeType, Span<Vector2>)`                        | static factory | wraps FLOAT32 2-component buffer                  |
|  [02]   | `PointAttribute.Wrap(AttributeType, Span<Vector3>)`                        | static factory | wraps FLOAT32 3-component buffer                  |
|  [03]   | `PointAttribute.Wrap(AttributeType, int components, float[])`              | static factory | wraps FLOAT32 N-component buffer                  |
|  [04]   | `PointAttribute(AttributeType, DataType, int, bool, int, int, DataBuffer)` | constructor    | full descriptor + buffer construction             |
|  [05]   | `NumUniqueEntries` property                                                | entry count    | unique attribute values in backing buffer         |
|  [06]   | `IdentityMapping` property                                                 | mapping mode   | `true` = identity mapping; `false` = explicit map |

[ENTRYPOINT_SCOPE]: `DracoEncodeOptions` — quantization tuning
- rail: geometry

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `PositionBits` property      | quantization   | bits for position attribute (default 11)        |
|  [02]   | `TextureCoordinateBits` prop | quantization   | bits for UV attribute (default 12)              |
|  [03]   | `NormalBits` property        | quantization   | bits for normal attribute (default 10)          |
|  [04]   | `ColorBits` property         | quantization   | bits for color attribute (default 10)           |
|  [05]   | `CompressionLevel` property  | level select   | `DracoCompressionLevel` case                    |
|  [06]   | `PointCloud` property        | mode flag      | forces point-cloud encoding regardless of faces |

## [04]-[IMPLEMENTATION_LAW]

[DRACO_TOPOLOGY]:
- namespace: `Openize.Drako`; 141 types across 5 namespaces (`Openize.Drako`, `.Utils`, `.Encoder`, `.Decoder`, `.Compression`); the public surface is the 13 root-namespace types catalogued here
- `DracoMesh` inherits `DracoPointCloud`; triangulated meshes are still point clouds with indexed faces
- `Draco.Decode` returns `DracoPointCloud` for point clouds and `DracoMesh` for triangulated meshes; callers discriminate by `is DracoMesh`
- `DracoEncodingMethod` and `EncodedGeometryType` are internal; encoder selection is automatic based on geometry type and compression level
- quantization defaults: `PositionBits=11`, `TextureCoordinateBits=12`, `NormalBits=10`, `ColorBits=10`, `CompressionLevel=Standard`
- attribute types: `Position`, `Normal`, `Color`, `TexCoord`, `Generic` are the five named types; `NamedAttributesCount` is a sentinel
- metadata: `Metadata.Entries` is string→byte[]; `GeometryMetadata.AttributeMetadata` maps attribute id → `Metadata`

[LOCAL_ADMISSION]:
- Encode intake: populate `DracoMesh` or `DracoPointCloud` with `PointAttribute.Wrap` factory calls; call `Draco.Encode` with explicit `DracoEncodeOptions`.
- Decode intake: `Draco.Decode(byte[])` returns `DracoPointCloud`; downcast to `DracoMesh` to access face indices.
- Attribute access: obtain `PointAttribute` via `DracoPointCloud.Attribute(id)`; read buffer via `PointAttribute.Buffer`.
- Error surface: `DrakoException` signals decode failure; no typed error rail beyond the base `Exception`.

[RAIL_LAW]:
- Package: `Openize.Drako`
- Owns: Draco 3D geometry encode/decode for mesh and point cloud geometry
- Accept: managed `Span<Vector2/3>` and `float[]` input through `PointAttribute.Wrap` factory; `byte[]` or `Stream` for encode output
- Reject: hand-rolled bit-packing, lower-level `DracoHeader` parsing, or internal encoder selection bypassing `Draco` facade
