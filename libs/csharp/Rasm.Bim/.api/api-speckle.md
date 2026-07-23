# [RASM_BIM_API_SPECKLE]

`Speckle.Sdk` and `Speckle.Objects` own the receive-side `Base` object-graph: the dynamic graph root, the deduplicating `Flatten` traversal, the display-mesh geometry, the metre-conversion surface, and the `DataObject` host-object family over a typed-parameter dictionary. Every member folds onto the canonical Bim carriers at the exchange import seam, and a non-display `Brep`/`Surface`/`Curve` with no `displayValue` hands off to the Compute tessellation companion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Speckle.Sdk`
- package: `Speckle.Sdk` (Apache-2.0)
- assembly: `Speckle.Sdk`
- namespace: `Speckle.Sdk.Models`, `Speckle.Sdk.Models.Extensions`, `Speckle.Sdk.Models.GraphTraversal`, `Speckle.Sdk.Common`
- asset: net10.0, net8.0, netstandard2.0; the net10.0 consumer binds `lib/net10.0`
- abi: IL-only managed assembly ILRepacking its closure (GraphQL.Client, STJ); the host-neutral exchange assembly binds it, never the in-Rhino plugin ALC
- rail: interchange

[PACKAGE_SURFACE]: `Speckle.Objects`
- package: `Speckle.Objects` (Apache-2.0)
- assembly: `Speckle.Objects`
- namespace: `Speckle.Objects.Geometry`, `Speckle.Objects.Data`, `Speckle.Objects`
- asset: net10.0, net8.0, netstandard2.0; the net10.0 consumer binds `lib/net10.0`
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: object-graph root and traversal

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :--------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Base`                 | dynamic class | graph root; `DynamicBase`/`ISpeckleObject` detached-member carrier |
|  [02]   | `BaseExtensions`       | static class  | dedup traversal, display-value, and parameter accessors            |
|  [03]   | `BaseRecursionBreaker` | delegate      | `bool(Base)` descent predicate nested in `BaseExtensions`          |
|  [04]   | `TraversalContext`     | class         | walk node reconstructing spatial containment                       |
|  [05]   | `Units`                | static class  | unit-string constants and the metre-scaling factor                 |

[PUBLIC_TYPE_SCOPE]: display geometry

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]                                                     |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Mesh`             | geometry class | flat `vertices`/`vertexNormals` doubles, length-prefixed `faces` |
|  [02]   | `Brep`             | geometry class | NURBS solid carrying `displayValue` `List<Mesh>`                 |
|  [03]   | `IDisplayValue<T>` | interface      | covariant `displayValue` contract over the payload type          |

[PUBLIC_TYPE_SCOPE]: host-object data family

Every subtype extends `DataObject`, inheriting `name`/`displayValue`/`properties` over host-specific typed columns.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `DataObject`         | data class    | base host object; `IDataObject`/`IProperties`/`IDisplayValue<…>` |
|  [02]   | `RevitObject`        | data subtype  | adds `type`/`family`/`category`/`level`/`location`/`elements`    |
|  [03]   | `TeklaObject`        | data subtype  | adds `type`/`elements`                                           |
|  [04]   | `ArcgisObject`       | data subtype  | ArcGIS host element                                              |
|  [05]   | `ArchicadObject`     | data subtype  | Archicad host element                                            |
|  [06]   | `AutocadObject`      | data subtype  | AutoCAD host element                                             |
|  [07]   | `Civil3dObject`      | data subtype  | Civil3D host element                                             |
|  [08]   | `EtabsObject`        | data subtype  | ETABS host element                                               |
|  [09]   | `MicrostationObject` | data subtype  | MicroStation host element                                        |
|  [10]   | `NavisworksObject`   | data subtype  | Navisworks host element                                          |
|  [11]   | `RhinoObject`        | data subtype  | Rhino host element                                               |
|  [12]   | `TsdObject`          | data subtype  | Tekla Structural Designer host element                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Base` — identity and count

| [INDEX] | [SURFACE]                 | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------ | :------- | :------------------------------------------------- |
|  [01]   | `id`                      | property | `string?` content-hash node id, null until decoded |
|  [02]   | `applicationId`           | property | `string?` host-application stable id               |
|  [03]   | `speckle_type`            | property | `string` `[SpeckleType]` token, get-only           |
|  [04]   | `GetTotalChildrenCount()` | instance | `long` total detached descendant count             |

[ENTRYPOINT_SCOPE]: `BaseExtensions` — traversal and display

`BaseExtensions` extends `Base` with static traversal and display accessors.

| [INDEX] | [SURFACE]                                      | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `Flatten(Base, BaseRecursionBreaker?)`         | static  | `IEnumerable<Base>` dedup walk caching on `Base.id` |
|  [02]   | `Traverse(Base, BaseRecursionBreaker)`         | static  | `IEnumerable<Base>` breaker-gated depth-first walk  |
|  [03]   | `TraverseWithPath(Base, BaseRecursionBreaker)` | static  | `IEnumerable<(string[], Base)>` path-carrying walk  |
|  [04]   | `TryGetDisplayValue(Base)`                     | static  | `IReadOnlyList<Base>?` display-node list            |
|  [05]   | `TryGetDisplayValue<T>(Base)`                  | static  | `IReadOnlyList<T>?` typed list, `T : Base`          |
|  [06]   | `IsDisplayableObject(Base)`                    | static  | `bool`, true when a display value exists            |
|  [07]   | `TryGetName(Base)`                             | static  | `string?` node display name                         |

[ENTRYPOINT_SCOPE]: `Units` — metre conversion

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                |
| :-----: | :--------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `Meters`                                 | const   | `"m"` canonical kernel-frame target unit    |
|  [02]   | `Millimeters`/`Centimeters`/`Kilometers` | const   | `"mm"`/`"cm"`/`"km"` metric tokens          |
|  [03]   | `Inches`/`Feet`/`Yards`/`Miles`          | const   | `"in"`/`"ft"`/`"yd"`/`"mi"` imperial tokens |
|  [04]   | `GetConversionFactor(string?, string?)`  | static  | `double` source-to-target metre scale       |
|  [05]   | `IsUnitSupported(string)`                | static  | `bool` recognized-unit gate                 |

[ENTRYPOINT_SCOPE]: `Mesh` — display geometry

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :---------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `vertices`                    | property | `required List<double>` flat `x,y,z` triples         |
|  [02]   | `vertexNormals`               | property | `List<double>` flat per-vertex normals, may be empty |
|  [03]   | `faces`                       | property | `required List<int>` length-prefixed `[n, i0…]` runs |
|  [04]   | `units`                       | property | `required string` source unit                        |
|  [05]   | `colors`/`textureCoordinates` | property | `List<int>`/`List<double>` optional vertex channels  |
|  [06]   | `VerticesCount`               | property | `int` = `vertices.Count / 3`                         |
|  [07]   | `GetPoint(int)`               | instance | `Point` at the vertex index                          |

[ENTRYPOINT_SCOPE]: `DataObject` — host semantics

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `name`                                         | property | `required string` host element name                |
|  [02]   | `displayValue`                                 | property | `required List<Base>` renderable display nodes     |
|  [03]   | `properties`                                   | property | `required Dictionary<string, object?>` host params |
|  [04]   | `RevitObject.type`/`family`/`category`/`level` | property | typed Revit element classification                 |
|  [05]   | `RevitObject.elements`/`TeklaObject.elements`  | property | `List<RevitObject>`/`List<TeklaObject>` child set  |

[ENTRYPOINT_SCOPE]: `TraversalContext` — walk node

| [INDEX] | [SURFACE]  | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------- | :------- | :-------------------------------------------------- |
|  [01]   | `Current`  | property | `Base` node at this walk position                   |
|  [02]   | `Parent`   | property | `TraversalContext?` containment-reconstruction link |
|  [03]   | `PropName` | property | `string?` member name the parent reached through    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Base` is a `DynamicBase`; the seam reads typed members through the package surface and never reflects the dynamic bag.
- `Flatten` is the sole deduplicating traversal, caching on `Base.id`; the seam never hand-rolls a `DynamicBase` recursion.
- `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary; a per-type `is Mesh`/`is Brep` ladder is the rejected form.
- `Mesh.faces` fans each length-prefixed n-gon to a triangle fan at the boundary.
- `Mesh.vertexNormals` carries a normal channel only when `vertexNormals.Count == vertices.Count`.
- `Brep` ships no managed NURBS evaluator; a non-mesh `Brep`/`Surface`/`Curve` lacking `displayValue` routes to the tessellation companion.
- Host subtypes add typed columns over the one inherited `properties` dictionary.

[STACKING]:
- `dotbim`(`.api/api-dotbim`), `SharpGLTF`(`.api/api-sharpgltf`), `AssimpNetter`(`.api/api-assimpnetter`): every display `Mesh` fans to the shared canonical triangle carrier these codecs decode into, so a received Speckle model re-exports through any of them.
- `Exchange/import` seam: `root.Flatten()` splits to `OfType<Mesh>` geometry and `OfType<DataObject>` semantics onto the canonical carriers, `Units.GetConversionFactor(mesh.units, Units.Meters)` scaling every mesh to the kernel metre frame; a `displayValue`-less `Brep`/`Surface`/`Curve` hands to the Compute tessellation companion.

[LOCAL_ADMISSION]:
- Display fold: `root.Flatten` → per-node `TryGetDisplayValue?.OfType<Mesh>` → fan `faces` n-gons to triangles → scale by `Units.GetConversionFactor(mesh.units, Units.Meters)`.
- Semantic fold: `root.Flatten().OfType<DataObject>()` → project `name`/`speckle_type`/`applicationId`, flatten `properties` to typed parameter rows.
- Containment: walk the `TraversalContext.Parent` chain to reconstruct spatial node containment.

[RAIL_LAW]:
- Package: `Speckle.Sdk`, `Speckle.Objects`
- Owns: the receive-side `Base` object-graph, the dedup traversal, the display-mesh geometry, and the host-object semantic family
- Accept: an already-deserialized `Base` root through the package `Flatten`/`TryGetDisplayValue` surface
- Reject: a hand-rolled `Base`-graph recursion, a per-type display ladder, a managed Speckle BRep tessellator, and a second `IOperations.Receive` in the import seam
