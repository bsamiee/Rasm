# [RASM_BIM_API_SPECKLE]

`Speckle.Sdk` and `Speckle.Objects` supply the receive-side object-graph the `Exchange/import#SPECKLE_SEAM` folds onto the canonical carriers: the `Base` dynamic root with its `id`/`applicationId`/`speckle_type` identity, the `BaseExtensions` deduplicating `Flatten`/`TryGetDisplayValue`/`IsDisplayableObject` traversal, the `Units` metre-conversion surface, the `Mesh`/`Brep` display geometry, and the `DataObject` host-object family with its typed-parameter `properties` dictionary. This folder-local catalogue records the Bim-seam members — `Mesh.vertexNormals`, `DataObject.properties`, and the host-object subtypes — the cross-folder Persistence sync catalogue elides.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Speckle.Sdk`
- package: `Speckle.Sdk`
- version: `3.21.1`
- license: Apache-2.0
- assembly: `Speckle.Sdk`
- namespace: `Speckle.Sdk.Models`, `Speckle.Sdk.Models.Extensions`, `Speckle.Sdk.Models.GraphTraversal`, `Speckle.Sdk.Common`
- asset: net10.0, net8.0, netstandard2.0; the net10.0 consumer binds the `lib/net10.0` asset
- asset: IL-only managed assembly; ILRepacks its closure (GraphQL.Client, STJ) — the host-neutral exchange assembly consumes it, never the in-Rhino plugin ALC
- rail: interchange

[PACKAGE_SURFACE]: `Speckle.Objects`
- package: `Speckle.Objects`
- version: `3.21.1`
- license: Apache-2.0
- assembly: `Speckle.Objects`
- namespace: `Speckle.Objects.Geometry`, `Speckle.Objects.Data`, `Speckle.Objects`
- asset: net10.0, net8.0, netstandard2.0; the net10.0 consumer binds the `lib/net10.0` asset
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: object-graph root and traversal
- rail: interchange

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [ROLE]                                                                      |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `Base`                 | dynamic class | graph root; `DynamicBase`/`ISpeckleObject`, dynamic detached-member carrier |
|  [02]   | `BaseExtensions`       | static class  | dedup `Flatten`/`Traverse` walk plus display-value and parameter accessors  |
|  [03]   | `BaseRecursionBreaker` | delegate      | `bool (Base)` predicate halting the `Flatten`/`Traverse` descent            |
|  [04]   | `TraversalContext`     | class         | walk node carrying `Current`/`Parent`/`PropName` for spatial reconstruction |
|  [05]   | `Units`                | static class  | unit-string constants plus `GetConversionFactor` metre scaling              |

[PUBLIC_TYPE_SCOPE]: display geometry
- rail: interchange

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [ROLE]                                                                              |
| :-----: | :----------------- | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Mesh`             | geometry class | flat `vertices`/`vertexNormals` `List<double>`, length-prefixed `faces` `List<int>` |
|  [02]   | `Brep`             | geometry class | NURBS solid; `displayValue` `List<Mesh>` via `IDisplayValue<List<Mesh>>`            |
|  [03]   | `IDisplayValue<T>` | interface      | covariant `displayValue` contract over the display-mesh payload type                |

[PUBLIC_TYPE_SCOPE]: host-object data family
- rail: interchange

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [ROLE]                                                                   |
| :-----: | :--------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `DataObject`     | data class    | base host object; `IDataObject`/`IProperties`/`IDisplayValue<IReadOnlyList<Base>>`; `name`, `displayValue` `List<Base>`, `properties` dict |
|  [02]   | `RevitObject`    | data subtype  | adds `type`/`family`/`category`/`level`/`location`/`elements`            |
|  [03]   | `TeklaObject`    | data subtype  | adds `type`/`elements`                                                   |
|  [04]   | `ArchicadObject` | data subtype  | Archicad host element; `DataObject` parameter dictionary                 |
|  [05]   | `Civil3dObject`  | data subtype  | Civil3D host element; `DataObject` parameter dictionary                  |
|  [06]   | `AutocadObject`  | data subtype  | AutoCAD host element; `DataObject` parameter dictionary                  |
|  [07]   | `RhinoObject`    | data subtype  | Rhino host element; `DataObject` parameter dictionary                    |

[HOST_SUBTYPE_ROSTER]: `DataObject` subtypes
- `ArcgisObject`, `ArchicadObject`, `AutocadObject`, `Civil3dObject`, `EtabsObject`, `MicrostationObject`, `NavisworksObject`, `RevitObject`, `RhinoObject`, `TeklaObject`, `TsdObject` — each extends `DataObject` and carries the inherited `name`/`displayValue`/`properties` plus host-specific typed columns.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Base` — identity and count
- rail: interchange

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------ | :------------- | :---------------------------------------- |
|  [01]   | `id` property             | identity       | `string?`; the content-hash node id       |
|  [02]   | `applicationId` property  | identity       | `string?`; the host-application stable id |
|  [03]   | `speckle_type` property   | discriminant   | `string`; the `[SpeckleType]` type token  |
|  [04]   | `GetTotalChildrenCount()` | count          | `long`; total detached descendant count   |

[ENTRYPOINT_SCOPE]: `BaseExtensions` — traversal and display
- rail: interchange

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `Flatten(this Base, BaseRecursionBreaker? = null)`  | dedup walk     | `IEnumerable<Base>`; caches on `Base.id`            |
|  [02]   | `Traverse(this Base, BaseRecursionBreaker)`         | walk           | `IEnumerable<Base>`; breaker-gated descent          |
|  [03]   | `TraverseWithPath(this Base, BaseRecursionBreaker)` | walk           | `IEnumerable<(string[], Base)>`; path-carrying walk |
|  [04]   | `TryGetDisplayValue(this Base)`                     | display        | `IReadOnlyList<Base>?`; the display-node list       |
|  [05]   | `TryGetDisplayValue<T>(this Base) where T : Base`   | display        | `IReadOnlyList<T>?`; typed display-node list        |
|  [06]   | `IsDisplayableObject(this Base)`                    | predicate      | `bool`; true when a renderable display value exists |
|  [07]   | `TryGetName(this Base)`                             | name           | `string?`; the node display name                    |

[ENTRYPOINT_SCOPE]: `Units` — metre conversion
- rail: interchange

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `Meters` constant                               | unit token     | `"m"`; the canonical kernel-frame target unit |
|  [02]   | `Millimeters`/`Centimeters`/`Kilometers`        | unit token     | `"mm"`/`"cm"`/`"km"` metric tokens            |
|  [03]   | `Inches`/`Feet`/`Yards`/`Miles`                 | unit token     | `"in"`/`"ft"`/`"yd"`/`"mi"` imperial tokens   |
|  [04]   | `GetConversionFactor(string? from, string? to)` | scale          | `double`; the source-to-target metre scale    |
|  [05]   | `IsUnitSupported(string unit)`                  | predicate      | `bool`; recognized-unit gate                  |

[ENTRYPOINT_SCOPE]: `Mesh` — display geometry
- rail: interchange

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [RAIL]                                                         |
| :-----: | :---------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `vertices` property           | flat positions | `required List<double>`; flat `x,y,z` triples                  |
|  [02]   | `vertexNormals` property      | flat normals   | `List<double>`; flat `x,y,z` per vertex, empty when absent     |
|  [03]   | `faces` property              | n-gon indices  | `required List<int>`; length-prefixed `[n, i0, … i(n-1)]` runs |
|  [04]   | `units` property              | unit token     | `required string`; the mesh's source unit                      |
|  [05]   | `colors`/`textureCoordinates` | channels       | `List<int>`/`List<double>` optional vertex channels            |
|  [06]   | `VerticesCount` property      | count          | `int`; `vertices.Count / 3`                                    |
|  [07]   | `GetPoint(int index)`         | accessor       | `Point` at the vertex index                                    |

[ENTRYPOINT_SCOPE]: `DataObject` — host semantics
- rail: interchange

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `name` property                                | label          | `required string`; the host element name                    |
|  [02]   | `displayValue` property                        | geometry       | `required List<Base>`; the renderable display nodes         |
|  [03]   | `properties` property                          | parameters     | `required Dictionary<string, object?>`; the host parameters |
|  [04]   | `RevitObject.type`/`family`/`category`/`level` | host columns   | typed Revit element classification                          |
|  [05]   | `RevitObject.elements`/`TeklaObject.elements`  | nesting        | `List<RevitObject>`/`List<TeklaObject>` child set           |

[ENTRYPOINT_SCOPE]: `TraversalContext` — walk node
- rail: interchange

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `Current` property  | node           | `Base`; the node at this walk position                   |
|  [02]   | `Parent` property   | chain          | `TraversalContext?`; the containment-reconstruction link |
|  [03]   | `PropName` property | edge           | `string?`; the member name the parent reached through    |

## [04]-[IMPLEMENTATION_LAW]

[SPECKLE_TOPOLOGY]:
- `Base` is a `DynamicBase`: detached members live in a dynamic bag, so the seam reads typed members through the package surface and never reflects the dynamic bag directly
- `BaseExtensions.Flatten` is the single deduplicating traversal — it caches on `Base.id`, so the seam never re-walks the tree or hand-rolls a `DynamicBase.GetMembers` recursion
- `TryGetDisplayValue`/`IsDisplayableObject` own the displayable-node vocabulary; a per-type `is Mesh`/`is Brep` ladder is the rejected form
- `Mesh.faces` is a length-prefixed n-gon encoding (`[n, i0, … i(n-1)]` runs), not a flat triangle list; the seam fans each n-gon over a triangle fan at the boundary
- `Mesh.vertexNormals` is a flat `List<double>` parallel to `vertices`; a normal channel is present only when `vertexNormals.Count == vertices.Count`
- `Brep` carries `displayValue` `List<Mesh>` and no managed NURBS evaluator; a non-mesh `Brep`/`Surface`/`Curve` with no `displayValue` routes to the companion tessellation rail
- `DataObject.properties` is `Dictionary<string, object?>`; the host subtypes (`RevitObject`/`TeklaObject`/…) add typed columns over the same inherited parameter dictionary

[LOCAL_ADMISSION]:
- Display fold: `root.Flatten()` → per-node `TryGetDisplayValue()?.OfType<Mesh>()` → fan each `Mesh.faces` n-gon to triangles → scale by `Units.GetConversionFactor(mesh.units, Units.Meters)`.
- Semantic fold: `root.Flatten().OfType<DataObject>()` → project `name`/`speckle_type`/`applicationId` and flatten `properties` to typed parameter rows.
- Containment: read `TraversalContext.Parent` chain to reconstruct the spatial node containment.

[RAIL_LAW]:
- Package: `Speckle.Sdk`, `Speckle.Objects`
- Owns: the receive-side `Base` object-graph, the dedup traversal, the display-mesh geometry, and the host-object semantic family
- Accept: an already-deserialized `Base` root through the package-owned `Flatten`/`TryGetDisplayValue` surface
- Reject: a hand-rolled `Base`-graph recursion, a per-type display ladder, a managed Speckle BRep tessellator, and a second `IOperations.Receive` reference inside the import seam
