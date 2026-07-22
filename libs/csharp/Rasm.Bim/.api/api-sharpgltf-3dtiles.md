# [RASM_BIM_API_SHARPGLTF_3DTILES]

`SharpGLTF.Ext.3DTiles` owns the Cesium 3D Tiles metadata surface over `SharpGLTF.Core`: `EXT_structural_metadata` schema-class-property authoring, `EXT_mesh_features` and `EXT_instance_features` feature-ID binding, and the property table, texture, and attribute storage builders. Every type lives in `SharpGLTF.Schema2.Tiles3D` and registers through the Core `ExtensionsFactory`, overlaying metadata on the `ModelRoot`/`MeshPrimitive`/`Node` graph Core authors.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpGLTF.Ext.3DTiles`
- package: `SharpGLTF.Ext.3DTiles` (MIT)
- assembly: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D` (extension types, schema model, storage builders, feature-ID family)
- namespace: `SharpGLTF.Schema2` (`Tiles3DExtensions` registration and binding statics, `ComponentCount` size queries)
- namespace: `SharpGLTF.Memory` (`BinaryTable` binary encoder)
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; net10.0 consumer binds `lib/net10.0`; IL-only AnyCPU, no native binaries
- depends: `SharpGLTF.Core` (the `ExtensionsFactory` and the `ModelRoot`/`MeshPrimitive`/`Node` parents), `OneOf` (transitive, only on the `FeatureIDBuilder` ctor)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: structural-metadata schema model

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `EXTStructuralMetadataRoot`       | class         | `EXT_structural_metadata` model-root extension; owns schema and tables   |
|  [02]   | `StructuralMetadataSchema`        | class         | embedded schema; `Id`, `Version`, `Name`, `Description`, class/enum maps |
|  [03]   | `StructuralMetadataClass`         | class         | named class holding `StructuralMetadataClassProperty` definitions        |
|  [04]   | `StructuralMetadataClassProperty` | class         | typed property definition; `With<Type>` selectors set the data type      |
|  [05]   | `StructuralMetadataEnum`          | class         | enum definition; named integer values with descriptions                  |
|  [06]   | `StructuralMetadataEnumValue`     | class         | one `Name`/`Value`/`Description` triple inside an enum                   |

[PUBLIC_TYPE_SCOPE]: property storage builders

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `PropertyTable`                      | class         | per-feature value store; `ClassName`, `Count`, property accessors    |
|  [02]   | `PropertyTableProperty`              | class         | one table column; `SetValues<T>`/`SetArrayValues<T>` binary encode   |
|  [03]   | `PropertyTexture`                    | class         | texture-channel value store; `CreateProperty` binds texture channels |
|  [04]   | `PropertyTextureProperty`            | class         | one texture-backed property; `Channels`, `Texture`                   |
|  [05]   | `PropertyAttribute`                  | class         | vertex-attribute value store; `CreateProperty` binds an attribute    |
|  [06]   | `PropertyAttributeProperty`          | class         | one attribute-backed property; `Attribute` accessor name             |
|  [07]   | `ExtStructuralMetadataMeshPrimitive` | class         | per-`MeshPrimitive` `EXT_structural_metadata` carrier                |

- `ExtStructuralMetadataMeshPrimitive`: `PropertyCount`/`AttributeCount`, `AddTexture`/`GetTexture`, `AddAttribute`/`GetAttribute` — the primitive-level store `AddPropertyTexture`/`AddPropertyAttribute` write into.

[PUBLIC_TYPE_SCOPE]: mesh and instance feature IDs

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :---------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `FeatureIDBuilder`            | class         | mutable feature-ID descriptor; constructs from count + attribute/texture |
|  [02]   | `IMeshFeatureIDInfo`          | interface     | feature-ID contract; `FeatureCount`, `NullFeatureId`, `Attribute`        |
|  [03]   | `MeshExtMeshFeatures`         | class         | `EXT_mesh_features` primitive extension; `CreateFeatureID`               |
|  [04]   | `MeshExtMeshFeatureID`        | class         | one mesh feature-ID set; carries `PropertyTableIndex`, `Label`           |
|  [05]   | `MeshExtMeshFeatureIDTexture` | class         | texture-backed feature ID; `Texture`, `GetChannels`/`SetChannels`        |
|  [06]   | `MeshExtInstanceFeatures`     | class         | `EXT_instance_features` node extension; `CreateFeatureID`                |
|  [07]   | `MeshExtInstanceFeatureID`    | class         | one instance feature-ID set bound to GPU-instanced node attributes       |

[PUBLIC_TYPE_SCOPE]: metadata enums and binary helpers; `BinaryTable` lives in `SharpGLTF.Memory`, `ComponentCount` in `SharpGLTF.Schema2`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :---------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `ElementType`     | enum          | `SCALAR`, `VEC2`, `VEC3`, `VEC4`, `MAT2`, `MAT3`, `MAT4`, `STRING`, `BOOLEAN`, `ENUM` |
|  [02]   | `DataType`        | enum          | component type: `INT8`–`UINT64`, `FLOAT32`, `FLOAT64`                                 |
|  [03]   | `IntegerType`     | enum          | integer component type: `INT8`–`UINT64`                                               |
|  [04]   | `ArrayOffsetType` | enum          | array offset width: `UINT8`, `UINT16`, `UINT32`, `UINT64`                             |
|  [05]   | `BinaryTable`     | class         | static binary encode for property storage; bytes, string and array offsets            |
|  [06]   | `ComponentCount`  | class         | static size queries: `ByteSizeForComponentType`, `ElementCountForType`                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Tiles3DExtensions` static extension methods on `ModelRoot`/`MeshPrimitive`/`Node`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `RegisterExtensions()`                                         | static  | register every owned extension on the Core factory |
|  [02]   | `UseStructuralMetadata(ModelRoot)`                             | static  | create or reuse the `EXTStructuralMetadataRoot`    |
|  [03]   | `AddMeshFeatureIds(MeshPrimitive, IMeshFeatureIDInfo[])`       | static  | bind mesh feature IDs to a primitive               |
|  [04]   | `AddInstanceFeatureIds(Node, IMeshFeatureIDInfo[])`            | static  | bind instance feature IDs to a GPU-instanced node  |
|  [05]   | `AddPropertyTexture(MeshPrimitive, PropertyTexture)`           | static  | attach a property texture to a primitive           |
|  [06]   | `AddPropertyAttribute(MeshPrimitive, PropertyAttribute)`       | static  | attach a property attribute to a primitive         |
|  [07]   | `SetCesiumOutline(MeshPrimitive, IReadOnlyList<uint>, string)` | static  | set `CESIUM_primitive_outline` index data          |

[ENTRYPOINT_SCOPE]: schema and table authoring on `EXTStructuralMetadataRoot`, `StructuralMetadataSchema`, `StructuralMetadataClass`

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `EXTStructuralMetadataRoot.UseEmbeddedSchema(string)`                              | instance | create or reuse the embedded schema |
|  [02]   | `EXTStructuralMetadataRoot.TryGetEmbeddedSchema(out StructuralMetadataSchema)`     | instance | read the schema when present        |
|  [03]   | `EXTStructuralMetadataRoot.AddPropertyTable(StructuralMetadataClass, int, string)` | instance | add a per-feature property table    |
|  [04]   | `EXTStructuralMetadataRoot.AddPropertyTexture(StructuralMetadataClass)`            | instance | add a property texture store        |
|  [05]   | `EXTStructuralMetadataRoot.AddPropertyAttribute(StructuralMetadataClass)`          | instance | add a property attribute store      |
|  [06]   | `StructuralMetadataSchema.UseClassMetadata(string)`                                | instance | create or reuse a class definition  |
|  [07]   | `StructuralMetadataSchema.UseEnumMetadata(string, (string,int)[])`                 | instance | create or reuse an enum definition  |
|  [08]   | `StructuralMetadataClass.UseProperty(string)`                                      | instance | create or reuse a class property    |
|  [09]   | `StructuralMetadataClass.AddPropertyTable(int, string)`                            | instance | add a table bound to this class     |

[ENTRYPOINT_SCOPE]: `StructuralMetadataClassProperty` fluent type selectors

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `WithStringType(string, string)`                  | instance | select `STRING` element type             |
|  [02]   | `WithBooleanType()`                               | instance | select `BOOLEAN` element type            |
|  [03]   | `WithUInt8Type(byte?, byte?)`                     | instance | select `SCALAR`/`UINT8` component type   |
|  [04]   | `WithInt32Type(int?, int?)`                       | instance | select `SCALAR`/`INT32` component type   |
|  [05]   | `WithFloat32Type(float?, float?)`                 | instance | select `SCALAR`/`FLOAT32` component type |
|  [06]   | `WithVector3Type(Vector3?, Vector3?)`             | instance | select `VEC3` element type               |
|  [07]   | `WithEnumeration(StructuralMetadataEnum, string)` | instance | select `ENUM` element type               |
|  [08]   | `WithUInt8ArrayType(int?, byte?)`                 | instance | select fixed or variable `UINT8` array   |
|  [09]   | `WithNormalized(bool)`                            | instance | mark integer values normalized           |
|  [10]   | `WithRequired(bool)`                              | instance | mark the property required               |

[ENTRYPOINT_SCOPE]: feature-ID and property-value binding
- `new FeatureIDBuilder(int featureCount, OneOf<int,Texture>?, PropertyTable?, IReadOnlyList<int>?, string?, int?)` builds the feature-ID descriptor.
- `ExtraProperties.GetExtension<T>()` on `MeshPrimitive`/`Node`/`ModelRoot` recovers a bound extension after import.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `PropertyTable.UseProperty(StructuralMetadataClassProperty)`          | instance | create or reuse a table column      |
|  [02]   | `PropertyTableProperty.SetValues<T>(T[])`                             | instance | encode scalar values to binary      |
|  [03]   | `PropertyTableProperty.SetArrayValues<T>(List<List<T>>)`              | instance | encode jagged arrays to binary      |
|  [04]   | `PropertyTexture.CreateProperty(string, Texture, IReadOnlyList<int>)` | instance | bind texture channels to a property |
|  [05]   | `PropertyAttribute.CreateProperty(string)`                            | instance | bind a vertex attribute             |
|  [06]   | `MeshExtMeshFeatures.CreateFeatureID(IMeshFeatureIDInfo)`             | instance | add a mesh feature-ID set           |
|  [07]   | `MeshExtInstanceFeatures.CreateFeatureID(IMeshFeatureIDInfo)`         | instance | add an instance feature-ID set      |
|  [08]   | `MeshExtMeshFeatureID.UseTexture()`                                   | instance | create the feature-ID texture       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Tiles3DExtensions.RegisterExtensions()` registers every owned extension on the `SharpGLTF.Core` `ExtensionsFactory`, idempotent and run once before any read or write touching a Tiles3D extension.
- Three parents carry the extensions — `ModelRoot` holds `EXTStructuralMetadataRoot`, `MeshPrimitive` holds `MeshExtMeshFeatures`, `Node` holds `MeshExtInstanceFeatures` — and `ExtraProperties.GetExtension<T>()` recovers each after import.
- Metadata authors through one fold: `ModelRoot.UseStructuralMetadata()` → `UseEmbeddedSchema(id)` → `UseClassMetadata(name)` → `UseProperty(name).With<Type>(...)`; array selectors fix length when `count` is set and vary it when null, and `WithNormalized`/`WithRequired`/`WithEnumeration` refine the property.
- Storage authors through three parallel folds: `AddPropertyTable(class, featureCount, name)` → `UseProperty(key).SetValues<T>`/`SetArrayValues<T>`; `AddPropertyTexture(class).CreateProperty(key, texture, channels)`; `AddPropertyAttribute(class).CreateProperty(key)`; `BinaryTable` and `ComponentCount` encode byte width, string and array offsets, and element counts.
- Feature IDs bind through `new FeatureIDBuilder(...)` → `MeshPrimitive.AddMeshFeatureIds` or `Node.AddInstanceFeatureIds`; the `OneOf<int, Texture>` selects a vertex-attribute index or a feature-ID texture, and `PropertyTableIndex` links a set to its property table so per-feature metadata resolves at read time.

[STACKING]:
- `SharpGLTF`(`.api/api-sharpgltf`): every Tiles3D extension registers on the `SharpGLTF.Core` `ExtensionsFactory` and mutates the same `ModelRoot`/`MeshPrimitive`/`Node` Core authors — Core owns the glTF schema, this surface overlays the 3D Tiles metadata on the shared target.
- `Rasm.Compute` `TILE_PARTITION` interchange codec: per-tile `EXT_structural_metadata` emit lowers through it after `Rasm.Bim` authors the schema and feature bindings.
- `Rasm.Bim` tessellation emit: composes the metadata, storage, and feature-ID folds to author per-tile 3D Tiles metadata on the tessellated `ModelRoot` before Compute lowers it.

[LOCAL_ADMISSION]:
- Admission enters through `Tiles3DExtensions.RegisterExtensions()` at startup, then `ModelRoot.UseStructuralMetadata()` for the schema-authoring root; `Rasm.Bim` admits the extension surface and the canonical schema shape.

[RAIL_LAW]:
- Package: `SharpGLTF.Ext.3DTiles`
- Owns: `EXT_structural_metadata`, `EXT_mesh_features`, `EXT_instance_features`, `CESIUM_primitive_outline`
- Accept: 3D Tiles metadata authoring, feature-ID binding, property storage encode
- Reject: glTF core read/write, mesh building, runtime decode, tile pyramid streaming
