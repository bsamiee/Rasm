# [RASM_BIM_API_SHARPGLTF_3DTILES]

`SharpGLTF.Ext.3DTiles` extends `SharpGLTF.Core` with the Cesium 3D Tiles
next-generation extension surface: `EXT_structural_metadata` schema/class/property
authoring, `EXT_mesh_features` and `EXT_instance_features` feature-ID binding, and the
property table/texture/attribute storage builders, all in the `SharpGLTF.Schema2.Tiles3D`
namespace and registered through the `SharpGLTF.Core` extension factory.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SharpGLTF.Ext.3DTiles`
- package: `SharpGLTF.Ext.3DTiles`
- assembly: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- namespace: `SharpGLTF.Schema2`
- namespace: `SharpGLTF.Memory`
- asset: net10.0, net8.0, net6.0, netstandard2.1, netstandard2.0
- dependency: `SharpGLTF.Core` >= 1.0.6
- dependency: `OneOf` >= 3.0.271
- rail: geometry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Tiles3D — structural-metadata schema model
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SYMBOL]                          | [RAIL]   | [CAPABILITY]                                                             |
| :-----: | :-------------------------------- | :------- | :----------------------------------------------------------------------- |
|   [1]   | `EXTStructuralMetadataRoot`       | geometry | `EXT_structural_metadata` model-root extension; owns schema and tables   |
|   [2]   | `StructuralMetadataSchema`        | geometry | embedded schema; `Id`, `Version`, `Name`, `Description`, class/enum maps |
|   [3]   | `StructuralMetadataClass`         | geometry | named class; holds `StructuralMetadataClassProperty` definitions         |
|   [4]   | `StructuralMetadataClassProperty` | geometry | typed property definition; `With<Type>` selectors set the data type      |
|   [5]   | `StructuralMetadataEnum`          | geometry | enum definition; named integer values with descriptions                  |
|   [6]   | `StructuralMetadataEnumValue`     | geometry | one `Name`/`Value`/`Description` triple inside an enum                   |

[PUBLIC_TYPE_SCOPE]: Tiles3D — property storage builders
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                         |
| :-----: | :-------------------------- | :------- | :------------------------------------------------------------------- |
|   [1]   | `PropertyTable`             | geometry | per-feature value store; `ClassName`, `Count`, property accessors    |
|   [2]   | `PropertyTableProperty`     | geometry | one table column; `SetValues<T>` / `SetArrayValues<T>` binary encode |
|   [3]   | `PropertyTexture`           | geometry | texture-channel value store; `CreateProperty` binds texture channels |
|   [4]   | `PropertyTextureProperty`   | geometry | one texture-backed property; `Channels`, `Texture`                   |
|   [5]   | `PropertyAttribute`         | geometry | vertex-attribute value store; `CreateProperty` binds an attribute    |
|   [6]   | `PropertyAttributeProperty` | geometry | one attribute-backed property; `Attribute` accessor name             |

[PUBLIC_TYPE_SCOPE]: Tiles3D — mesh and instance feature IDs
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [RAIL]   | [CAPABILITY]                                                             |
| :-----: | :---------------------------- | :------- | :----------------------------------------------------------------------- |
|   [1]   | `FeatureIDBuilder`            | geometry | mutable feature-ID descriptor; constructs from count + attribute/texture |
|   [2]   | `IMeshFeatureIDInfo`          | geometry | feature-ID contract; `FeatureCount`, `NullFeatureId`, `Attribute`, etc.  |
|   [3]   | `MeshExtMeshFeatures`         | geometry | `EXT_mesh_features` primitive extension; `CreateFeatureID`               |
|   [4]   | `MeshExtMeshFeatureID`        | geometry | one mesh feature-ID set; carries `PropertyTableIndex`, `Label`           |
|   [5]   | `MeshExtMeshFeatureIDTexture` | geometry | texture-backed feature ID; `Texture`, `GetChannels`/`SetChannels`        |
|   [6]   | `MeshExtInstanceFeatures`     | geometry | `EXT_instance_features` node extension; `CreateFeatureID`                |
|   [7]   | `MeshExtInstanceFeatureID`    | geometry | one instance feature-ID set bound to GPU-instanced node attributes       |

[PUBLIC_TYPE_SCOPE]: Tiles3D — metadata enums and binary helpers
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`, `SharpGLTF.Schema2`, `SharpGLTF.Memory`
- rail: geometry

| [INDEX] | [SYMBOL]          | [RAIL]   | [CAPABILITY]                                                                          |
| :-----: | :---------------- | :------- | :------------------------------------------------------------------------------------ |
|   [1]   | `ElementType`     | geometry | `SCALAR`, `VEC2`, `VEC3`, `VEC4`, `MAT2`, `MAT3`, `MAT4`, `STRING`, `BOOLEAN`, `ENUM` |
|   [2]   | `DataType`        | geometry | component type: `INT8`–`UINT64`, `FLOAT32`, `FLOAT64`                                 |
|   [3]   | `IntegerType`     | geometry | integer component type: `INT8`–`UINT64`                                               |
|   [4]   | `ArrayOffsetType` | geometry | array offset width: `UINT8`, `UINT16`, `UINT32`, `UINT64`                             |
|   [5]   | `BinaryTable`     | geometry | static binary encode for property storage; bytes, string/array offsets                |
|   [6]   | `ComponentCount`  | geometry | static size queries: `ByteSizeForComponentType`, `ElementCountForType`                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Tiles3DExtensions — registration and binding
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2`
- rail: geometry

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]                                        | [CAPABILITY]                                          |
| :-----: | :---------------------------------------- | :-------------------------------------------------- | :---------------------------------------------------- |
|   [1]   | `Tiles3DExtensions.RegisterExtensions`    | `()` (static)                                       | registers all Tiles3D extensions on the model factory |
|   [2]   | `Tiles3DExtensions.UseStructuralMetadata` | `(this ModelRoot)`                                  | creates or reuses the `EXTStructuralMetadataRoot`     |
|   [3]   | `Tiles3DExtensions.AddMeshFeatureIds`     | `(this MeshPrimitive, params IMeshFeatureIDInfo[])` | binds mesh feature IDs to a primitive                 |
|   [4]   | `Tiles3DExtensions.AddInstanceFeatureIds` | `(this Node, params IMeshFeatureIDInfo[])`          | binds instance feature IDs to a GPU-instanced node    |
|   [5]   | `Tiles3DExtensions.AddPropertyTexture`    | `(this MeshPrimitive, PropertyTexture)`             | attaches a property texture to a primitive            |
|   [6]   | `Tiles3DExtensions.AddPropertyAttribute`  | `(this MeshPrimitive, PropertyAttribute)`           | attaches a property attribute to a primitive          |
|   [7]   | `Tiles3DExtensions.SetCesiumOutline`      | `(this MeshPrimitive, IReadOnlyList<uint>, string)` | sets `CESIUM_primitive_outline` index data            |

[ENTRYPOINT_SCOPE]: EXTStructuralMetadataRoot — schema and table authoring
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]                                           | [CAPABILITY]                          |
| :-----: | :----------------------------------------------- | :----------------------------------------------------- | :------------------------------------ |
|   [1]   | `EXTStructuralMetadataRoot.UseEmbeddedSchema`    | `(string id)` or `()`                                  | creates or reuses the embedded schema |
|   [2]   | `EXTStructuralMetadataRoot.TryGetEmbeddedSchema` | `(out StructuralMetadataSchema)`                       | reads the schema if present           |
|   [3]   | `EXTStructuralMetadataRoot.AddPropertyTable`     | `(StructuralMetadataClass, int featureCount, string?)` | adds a per-feature property table     |
|   [4]   | `EXTStructuralMetadataRoot.AddPropertyTexture`   | `(StructuralMetadataClass)` or `()`                    | adds a property texture store         |
|   [5]   | `EXTStructuralMetadataRoot.AddPropertyAttribute` | `(StructuralMetadataClass)` or `()`                    | adds a property attribute store       |
|   [6]   | `StructuralMetadataSchema.UseClassMetadata`      | `(string key)`                                         | creates or reuses a class definition  |
|   [7]   | `StructuralMetadataSchema.UseEnumMetadata`       | `(string key, params (string,int)[])`                  | creates or reuses an enum definition  |
|   [8]   | `StructuralMetadataClass.UseProperty`            | `(string key)`                                         | creates or reuses a class property    |
|   [9]   | `StructuralMetadataClass.AddPropertyTable`       | `(int featureCount, string?)`                          | adds a table bound to this class      |

[ENTRYPOINT_SCOPE]: StructuralMetadataClassProperty — type selectors
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]                               | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------- | :----------------------------------------- | :---------------------------------------- |
|   [1]   | `StructuralMetadataClassProperty.WithStringType`     | `(string? noData, string? defaultValue)`   | selects `STRING` element type             |
|   [2]   | `StructuralMetadataClassProperty.WithBooleanType`    | `()`                                       | selects `BOOLEAN` element type            |
|   [3]   | `StructuralMetadataClassProperty.WithUInt8Type`      | `(byte? noData, byte? defaultValue)`       | selects `SCALAR`/`UINT8` component type   |
|   [4]   | `StructuralMetadataClassProperty.WithInt32Type`      | `(int? noData, int? defaultValue)`         | selects `SCALAR`/`INT32` component type   |
|   [5]   | `StructuralMetadataClassProperty.WithFloat32Type`    | `(float? noData, float? defaultValue)`     | selects `SCALAR`/`FLOAT32` component type |
|   [6]   | `StructuralMetadataClassProperty.WithVector3Type`    | `(Vector3? noData, Vector3? defaultValue)` | selects `VEC3` element type               |
|   [7]   | `StructuralMetadataClassProperty.WithEnumeration`    | `(StructuralMetadataEnum, string? noData)` | selects `ENUM` element type               |
|   [8]   | `StructuralMetadataClassProperty.WithUInt8ArrayType` | `(int? count, byte? noData)`               | selects fixed/variable `UINT8` array      |
|   [9]   | `StructuralMetadataClassProperty.WithNormalized`     | `(bool normalized)`                        | marks integer values as normalized        |
|  [10]   | `StructuralMetadataClassProperty.WithRequired`       | `(bool required)`                          | marks the property required               |

[ENTRYPOINT_SCOPE]: feature-ID and property-value binding
- package: `SharpGLTF.Ext.3DTiles`
- namespace: `SharpGLTF.Schema2.Tiles3D`
- rail: geometry

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]                                                                                                                         | [CAPABILITY]                                  |
| :-----: | :---------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|   [1]   | `new FeatureIDBuilder`                    | `(int featureCount, OneOf<int,Texture>? attributeOrTexture, PropertyTable?, IReadOnlyList<int>?, string? label, int? nullFeatureId)` | builds a feature-ID descriptor                |
|   [2]   | `PropertyTable.UseProperty`               | `(StructuralMetadataClassProperty)` or `(string key)`                                                                                | creates or reuses a table column              |
|   [3]   | `PropertyTableProperty.SetValues<T>`      | `(params T[])`                                                                                                                       | encodes scalar values to binary storage       |
|   [4]   | `PropertyTableProperty.SetArrayValues<T>` | `(List<List<T>>)`                                                                                                                    | encodes jagged array values to binary storage |
|   [5]   | `PropertyTexture.CreateProperty`          | `(string key, Texture, IReadOnlyList<int>? channels)`                                                                                | binds a texture channel set to a property     |
|   [6]   | `PropertyAttribute.CreateProperty`        | `(string key)`                                                                                                                       | binds a vertex attribute to a property        |
|   [7]   | `MeshExtMeshFeatures.CreateFeatureID`     | `(IMeshFeatureIDInfo)` or `()`                                                                                                       | adds a mesh feature-ID set                    |
|   [8]   | `MeshExtInstanceFeatures.CreateFeatureID` | `(IMeshFeatureIDInfo)` or `()`                                                                                                       | adds an instance feature-ID set               |
|   [9]   | `MeshExtMeshFeatureID.UseTexture`         | `()`                                                                                                                                 | creates the feature-ID texture binding        |
|  [10]   | `ExtraProperties.GetExtension<T>`         | `()` on `MeshPrimitive`, `Node`, or `ModelRoot`                                                                                      | reads a Tiles3D extension after import        |

## [4]-[IMPLEMENTATION_LAW]

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
