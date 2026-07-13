# [RASM_BIM_API_SHARPGLTF_3DTILES]

`SharpGLTF.Ext.3DTiles` extends `SharpGLTF.Core` with the Cesium 3D Tiles
next-generation extension surface: `EXT_structural_metadata` schema/class/property
authoring, `EXT_mesh_features` and `EXT_instance_features` feature-ID binding, and the
property table/texture/attribute storage builders, all in the `SharpGLTF.Schema2.Tiles3D`
namespace and registered through the `SharpGLTF.Core` extension factory.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpGLTF.Ext.3DTiles`
- package: `SharpGLTF.Ext.3DTiles`
- license: MIT
- assembly: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D` (extension types, schema model, storage builders, feature-ID family)
- namespace: `SharpGLTF.Schema2` (`Tiles3DExtensions` registration/binding statics; `ComponentCount` static size queries)
- namespace: `SharpGLTF.Memory` (`BinaryTable` static binary encoder)
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0; the net10.0 consumer binds the `lib/net10.0` asset
- asset: IL-only AnyCPU managed assembly; no native binaries
- dependency: `SharpGLTF.Core` >= (the extension factory + `ModelRoot`/`MeshPrimitive`/`Node` it attaches to)
- dependency: `OneOf` >= (transitive; surfaced only on the `FeatureIDBuilder` ctor)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Tiles3D — structural-metadata schema model
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SYMBOL]                          | [CAPABILITY]                                                             |
| :-----: | :-------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `EXTStructuralMetadataRoot`       | `EXT_structural_metadata` model-root extension; owns schema and tables   |
|  [02]   | `StructuralMetadataSchema`        | embedded schema; `Id`, `Version`, `Name`, `Description`, class/enum maps |
|  [03]   | `StructuralMetadataClass`         | named class; holds `StructuralMetadataClassProperty` definitions         |
|  [04]   | `StructuralMetadataClassProperty` | typed property definition; `With<Type>` selectors set the data type      |
|  [05]   | `StructuralMetadataEnum`          | enum definition; named integer values with descriptions                  |
|  [06]   | `StructuralMetadataEnumValue`     | one `Name`/`Value`/`Description` triple inside an enum                   |

[PUBLIC_TYPE_SCOPE]: Tiles3D — property storage builders
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SYMBOL]                             | [CAPABILITY]                                                                        |
| :-----: | :----------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `PropertyTable`                      | per-feature value store; `ClassName`, `Count`, property accessors                   |
|  [02]   | `PropertyTableProperty`              | one table column; `SetValues<T>` / `SetArrayValues<T>` binary encode                |
|  [03]   | `PropertyTexture`                    | texture-channel value store; `CreateProperty` binds texture channels                |
|  [04]   | `PropertyTextureProperty`            | one texture-backed property; `Channels`, `Texture`                                  |
|  [05]   | `PropertyAttribute`                  | vertex-attribute value store; `CreateProperty` binds an attribute                   |
|  [06]   | `PropertyAttributeProperty`          | one attribute-backed property; `Attribute` accessor name                            |
|  [07]   | `ExtStructuralMetadataMeshPrimitive` | per-`MeshPrimitive` `EXT_structural_metadata` carrier (members in `[01]-[EXTPRIM]`) |

- [01]-[EXTPRIM]: `ExtStructuralMetadataMeshPrimitive` — `PropertyCount`/`AttributeCount`, `Add`/`GetTexture`, `Add`/`GetAttribute`; the primitive-level store the `AddPropertyTexture`/`AddPropertyAttribute` entrypoints write into.

[PUBLIC_TYPE_SCOPE]: Tiles3D — mesh and instance feature IDs
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                                             |
| :-----: | :---------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `FeatureIDBuilder`            | mutable feature-ID descriptor; constructs from count + attribute/texture |
|  [02]   | `IMeshFeatureIDInfo`          | feature-ID contract; `FeatureCount`, `NullFeatureId`, `Attribute`, etc.  |
|  [03]   | `MeshExtMeshFeatures`         | `EXT_mesh_features` primitive extension; `CreateFeatureID`               |
|  [04]   | `MeshExtMeshFeatureID`        | one mesh feature-ID set; carries `PropertyTableIndex`, `Label`           |
|  [05]   | `MeshExtMeshFeatureIDTexture` | texture-backed feature ID; `Texture`, `GetChannels`/`SetChannels`        |
|  [06]   | `MeshExtInstanceFeatures`     | `EXT_instance_features` node extension; `CreateFeatureID`                |
|  [07]   | `MeshExtInstanceFeatureID`    | one instance feature-ID set bound to GPU-instanced node attributes       |

[PUBLIC_TYPE_SCOPE]: Tiles3D — metadata enums and binary helpers
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`, `SharpGLTF.Schema2`, `SharpGLTF.Memory`
- rail: geometry

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                                                                 |
| :-----: | :---------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `ElementType`     | `SCALAR`, `VEC2`, `VEC3`, `VEC4`, `MAT2`, `MAT3`, `MAT4`, `STRING`, `BOOLEAN`, `ENUM`        |
|  [02]   | `DataType`        | component type: `INT8`–`UINT64`, `FLOAT32`, `FLOAT64`                                        |
|  [03]   | `IntegerType`     | integer component type: `INT8`–`UINT64`                                                      |
|  [04]   | `ArrayOffsetType` | array offset width: `UINT8`, `UINT16`, `UINT32`, `UINT64`                                    |
|  [05]   | `BinaryTable`     | (`SharpGLTF.Memory`) static binary encode for property storage; bytes, string/array offsets  |
|  [06]   | `ComponentCount`  | (`SharpGLTF.Schema2`) static size queries: `ByteSizeForComponentType`, `ElementCountForType` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Tiles3DExtensions — registration and binding
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2`
- owner: every `[SURFACE]` is a `Tiles3DExtensions` static extension method
- rail: geometry

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                        | [CAPABILITY]                                          |
| :-----: | :---------------------- | :-------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `RegisterExtensions`    | `()` (static)                                       | registers all Tiles3D extensions on the model factory |
|  [02]   | `UseStructuralMetadata` | `(this ModelRoot)`                                  | creates or reuses the `EXTStructuralMetadataRoot`     |
|  [03]   | `AddMeshFeatureIds`     | `(this MeshPrimitive, params IMeshFeatureIDInfo[])` | binds mesh feature IDs to a primitive                 |
|  [04]   | `AddInstanceFeatureIds` | `(this Node, params IMeshFeatureIDInfo[])`          | binds instance feature IDs to a GPU-instanced node    |
|  [05]   | `AddPropertyTexture`    | `(this MeshPrimitive, PropertyTexture)`             | attaches a property texture to a primitive            |
|  [06]   | `AddPropertyAttribute`  | `(this MeshPrimitive, PropertyAttribute)`           | attaches a property attribute to a primitive          |
|  [07]   | `SetCesiumOutline`      | `(this MeshPrimitive, IReadOnlyList<uint>, string)` | sets `CESIUM_primitive_outline` index data            |

[ENTRYPOINT_SCOPE]: EXTStructuralMetadataRoot — schema and table authoring
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- owner: `[01]`-`[05]` on `EXTStructuralMetadataRoot`; `[06]`-`[07]` on `StructuralMetadataSchema`; `[08]`-`[09]` on `StructuralMetadataClass`
- rail: geometry

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                           | [CAPABILITY]                          |
| :-----: | :--------------------- | :----------------------------------------------------- | :------------------------------------ |
|  [01]   | `UseEmbeddedSchema`    | `(string id)` or `()`                                  | creates or reuses the embedded schema |
|  [02]   | `TryGetEmbeddedSchema` | `(out StructuralMetadataSchema)`                       | reads the schema if present           |
|  [03]   | `AddPropertyTable`     | `(StructuralMetadataClass, int featureCount, string?)` | adds a per-feature property table     |
|  [04]   | `AddPropertyTexture`   | `(StructuralMetadataClass)` or `()`                    | adds a property texture store         |
|  [05]   | `AddPropertyAttribute` | `(StructuralMetadataClass)` or `()`                    | adds a property attribute store       |
|  [06]   | `UseClassMetadata`     | `(string key)`                                         | creates or reuses a class definition  |
|  [07]   | `UseEnumMetadata`      | `(string key, params (string,int)[])`                  | creates or reuses an enum definition  |
|  [08]   | `UseProperty`          | `(string key)`                                         | creates or reuses a class property    |
|  [09]   | `AddPropertyTable`     | `(int featureCount, string?)`                          | adds a table bound to this class      |

[ENTRYPOINT_SCOPE]: StructuralMetadataClassProperty — type selectors
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- owner: every `[SURFACE]` is a `StructuralMetadataClassProperty` fluent selector
- rail: geometry

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                               | [CAPABILITY]                              |
| :-----: | :------------------- | :----------------------------------------- | :---------------------------------------- |
|  [01]   | `WithStringType`     | `(string? noData, string? defaultValue)`   | selects `STRING` element type             |
|  [02]   | `WithBooleanType`    | `()`                                       | selects `BOOLEAN` element type            |
|  [03]   | `WithUInt8Type`      | `(byte? noData, byte? defaultValue)`       | selects `SCALAR`/`UINT8` component type   |
|  [04]   | `WithInt32Type`      | `(int? noData, int? defaultValue)`         | selects `SCALAR`/`INT32` component type   |
|  [05]   | `WithFloat32Type`    | `(float? noData, float? defaultValue)`     | selects `SCALAR`/`FLOAT32` component type |
|  [06]   | `WithVector3Type`    | `(Vector3? noData, Vector3? defaultValue)` | selects `VEC3` element type               |
|  [07]   | `WithEnumeration`    | `(StructuralMetadataEnum, string? noData)` | selects `ENUM` element type               |
|  [08]   | `WithUInt8ArrayType` | `(int? count, byte? noData)`               | selects fixed/variable `UINT8` array      |
|  [09]   | `WithNormalized`     | `(bool normalized)`                        | marks integer values as normalized        |
|  [10]   | `WithRequired`       | `(bool required)`                          | marks the property required               |

[ENTRYPOINT_SCOPE]: feature-ID and property-value binding
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- ctor: `new FeatureIDBuilder(int featureCount, OneOf<int,Texture>? attributeOrTexture, PropertyTable?, IReadOnlyList<int>?, string? label, int? nullFeatureId)` builds a feature-ID descriptor
- read: `ExtraProperties.GetExtension<T>() where T: JsonSerializable` on `MeshPrimitive`/`Node`/`ModelRoot` reads a bound extension after import — parent mapping in `[FEATURE_IDS]`
- rail: geometry

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]                                          | [CAPABILITY]                         |
| :-----: | :---------------------------------------- | :---------------------------------------------------- | :----------------------------------- |
|  [01]   | `PropertyTable.UseProperty`               | `(StructuralMetadataClassProperty)` or `(string key)` | creates or reuses a table column     |
|  [02]   | `PropertyTableProperty.SetValues<T>`      | `(params T[])`                                        | encodes scalar values to binary      |
|  [03]   | `PropertyTableProperty.SetArrayValues<T>` | `(List<List<T>>)`                                     | encodes jagged arrays to binary      |
|  [04]   | `PropertyTexture.CreateProperty`          | `(string key, Texture, IReadOnlyList<int>? channels)` | binds texture channels to a property |
|  [05]   | `PropertyAttribute.CreateProperty`        | `(string key)`                                        | binds a vertex attribute             |
|  [06]   | `MeshExtMeshFeatures.CreateFeatureID`     | `(IMeshFeatureIDInfo)` or `()`                        | adds a mesh feature-ID set           |
|  [07]   | `MeshExtInstanceFeatures.CreateFeatureID` | `(IMeshFeatureIDInfo)` or `()`                        | adds an instance feature-ID set      |
|  [08]   | `MeshExtMeshFeatureID.UseTexture`         | `()`                                                  | creates the feature-ID texture       |

## [04]-[IMPLEMENTATION_LAW]

[EXTENSION_REGISTRATION]:
- `Tiles3DExtensions.RegisterExtensions()` registers `CESIUM_primitive_outline`, `EXT_instance_features`, `EXT_mesh_features`, and `EXT_structural_metadata` on the `SharpGLTF.Core` factory.
- registration is global and must run once before any read or write that uses a Tiles3D extension; the call is idempotent at the factory level.
- the extensions attach to three parents: `ModelRoot` carries `EXTStructuralMetadataRoot`, `MeshPrimitive` carries `MeshExtMeshFeatures`, and `Node` carries `MeshExtInstanceFeatures`.

[STRUCTURAL_METADATA]:
- author path: `model.UseStructuralMetadata()` → `UseEmbeddedSchema(id)` → `UseClassMetadata(name)` → `UseProperty(name).With<Type>(...)`.
- `With<Type>` selectors set the component or element type; `WithNormalized(true)` marks integer storage as normalized, and `WithRequired(true)` marks the property required.
- array variants `With<Type>ArrayType(count)` produce fixed-length arrays when `count` is set and variable-length arrays when it is null.
- enum properties bind through `UseEnumMetadata(key, values)` then `UseProperty(name).WithEnumeration(enum)`.

[PROPERTY_STORAGE]:
- table path: `root.AddPropertyTable(class, featureCount, name)` or `class.AddPropertyTable(featureCount, name)` → `UseProperty(key).SetValues<T>(...)` for scalars or `SetArrayValues<T>(...)` for jagged arrays.
- texture path: `root.AddPropertyTexture(class)` → `CreateProperty(key, texture, channels)`; `PropertyTextureProperty` exposes `Channels` and `Texture`.
- attribute path: `root.AddPropertyAttribute(class)` → `CreateProperty(key)`; `PropertyAttributeProperty.Attribute` names the source vertex attribute.
- `BinaryTable` and `ComponentCount` are the static helpers behind value encoding: byte width, string offsets, array offsets, and element counts.

[FEATURE_IDS]:
- mesh path: build a `FeatureIDBuilder(featureCount, attributeOrTexture, propertyTable, channels, label, nullFeatureId)` then `primitive.AddMeshFeatureIds(builder)`; the `OneOf<int, Texture>` argument selects a vertex attribute index or a feature-ID texture.
- instance path: `node.AddInstanceFeatureIds(builder)` binds feature IDs to a GPU-instanced node under `EXT_instance_features`.
- `PropertyTableIndex` links a feature-ID set to a property table so per-feature metadata resolves at read time.
- read path: after import, `primitive.GetExtension<MeshExtMeshFeatures>()`, `node.GetExtension<MeshExtInstanceFeatures>()`, and `model.GetExtension<EXTStructuralMetadataRoot>()` recover the bound extensions.

[LOCAL_ADMISSION]:
- Tiles3D admission enters through `Tiles3DExtensions.RegisterExtensions()` at startup, then `ModelRoot.UseStructuralMetadata()` for the schema authoring root.
- per-tile metadata emit composes through the `Rasm.Compute` interchange codec at `TILE_PARTITION`; `Rasm.Bim` admits the extension surface and the canonical schema shape.
- `OneOf` is a transitive dependency consumed only by `FeatureIDBuilder`; no `Rasm.Bim` code references it directly.

[RAIL_LAW]:
- Packages: `SharpGLTF.Ext.3DTiles`
- Owns: `EXT_structural_metadata`, `EXT_mesh_features`, `EXT_instance_features`, `CESIUM_primitive_outline`
- Accept: 3D Tiles metadata authoring, feature-ID binding, property storage encode
- Reject: glTF core read/write, mesh building, runtime decode, tile pyramid streaming
