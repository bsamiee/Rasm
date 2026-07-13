# [RASM_BIM_API_USD]

`UniversalSceneDescription` is the managed OpenUSD scene-graph codec backing the `Exchange/format#FORMAT_TABLE` `InterchangeCodec.UsdStage` slot — a faithful SWIG-generated C# binding to the Pixar USD C++ runtime exposing the full `pxr` namespace (Sdf layers, Usd stage/prim/property, the `UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux` typed schemas, the `Gf` math and `Vt` typed-array value types). It owns the read AND write of `.usd`/`.usda`/`.usdc`/`.usdz` through `UsdStage` — layer composition (sublayers, references, payloads, variants, inherits, specializes), prim/attribute authoring on a chosen `UsdEditTarget`, time-sampled values, and full export/flatten. In the Bim codec table USD is a scene-graph PEER, never a BIM-semantic replacement: USD carries the geometry/shading/instancing scene, the GeometryGym IFC graph carries the BIM semantics, and the two coexist at the `format#FORMAT_TABLE` row. The codec load fault lifts to `BimFault.CodecReject` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UniversalSceneDescription`
- package: `UniversalSceneDescription` (version, direct pin)
- license: MPL-2.0 (`EggyStudio/UniversalSceneDescription`); `requireLicenseAcceptance=true` — the file-level reciprocity is satisfied by referencing the unmodified NuGet binaries, never modifying its source files
- assembly: `UniversalSceneDescription.dll` AND `USD.NET.dll` → both bind at `lib/net10.0/` (the exact consumer TFM); the public USD surface lives across both under one `pxr` namespace
- namespace: `pxr` (the entire USD binding — `Ar`/`Gf`/`Ndr`/`Pcp`/`Sdf`/`Sdr`/`Tf`/`Usd`/`UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux`/`Vt`); `SdfValueTypeNames` is the value-type-name table
- asset: a managed SWIG wrapper over per-RID native USD libraries — `runtimes/{osx-arm64,osx-x64,linux-x64,win-x64}/native` ship `libusd_*.dylib`/`.so`/`.dll` plus the Pixar `usd/plugInfo.json` plugin tree and `libAlembic` (`osx-arm64` verified); a stage op with no matching RID native payload + plugin tree faults at native load
- runtime law: the SWIG binding handles are `IDisposable` over native USD objects (`UsdStage`/`UsdPrim`/`SdfLayer`/`Vt*Array`/`VtValue` and every `pxr` handle); `SWIGTYPE_p_*` and `*PINVOKE` are interop plumbing and stay out of canonical owners
- rail: `format#FORMAT_TABLE` (the `usd-stage` codec; scene-graph peer to the IFC semantic graph)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stage and layer composition roots
- rail: format#USD_STAGE

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [CAPABILITY]                                                     |
| :-----: | :--------------------------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `UsdStage`                                     | stage root       | the composed scene — open/create/save/export over a layer stack  |
|  [02]   | `SdfLayer` / `SdfLayerHandle`                  | layer            | one `.usd*` layer of opinions (root/session/sublayers)           |
|  [03]   | `SdfPath`                                      | scene-graph path | the prim/property address (`/World/Mesh.points`)                 |
|  [04]   | `UsdEditTarget`                                | edit target      | which layer authored opinions land in                            |
|  [05]   | `UsdTimeCode`                                  | time code        | time-sample coordinate (`Default()`/`EarliestTime()`/`(double)`) |
|  [06]   | `UsdStagePopulationMask` / `UsdStageLoadRules` | load policy      | masked/partial stage population and payload load rules           |
|  [07]   | `UsdPrimRange`                                 | traversal range  | the prim iteration a stage `Traverse` yields                     |
|  [08]   | `Usd_PrimFlagsPredicate`                       | traversal filter | the active/defined/loaded predicate a filtered traversal takes   |

[PUBLIC_TYPE_SCOPE]: prim, property, and composition arcs
- rail: format#USD_PRIM

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]   | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `UsdPrim`                                          | prim            | a scene-graph node (typed schema + children/properties)     |
|  [02]   | `UsdObject`                                        | object base     | `GetStage`/`GetPath`/`GetPrim`/`GetName` over prim+property |
|  [03]   | `UsdProperty` / `UsdAttribute` / `UsdRelationship` | property        | a prim's typed attribute or relationship                    |
|  [04]   | `UsdReferences` / `UsdPayloads`                    | composition arc | external/internal reference and deferred payload arcs       |
|  [05]   | `UsdInherits` / `UsdSpecializes`                   | composition arc | class inheritance and specialization arcs                   |
|  [06]   | `UsdVariantSets` / `UsdVariantSet`                 | variant arc     | named variant sets and the selected variant                 |
|  [07]   | `SdfReference` / `SdfPayload` / `UsdListPosition`  | arc value       | the reference/payload value and list-edit position          |
|  [08]   | `UsdModelAPI` / `UsdCollectionAPI`                 | applied API     | model-kind metadata and membership collections              |

[PUBLIC_TYPE_SCOPE]: typed geometry schemas
- rail: format#USD_SCHEMA
- note: the `UsdGeom*` schemas are typed views over a `UsdPrim` (each `Define(stage, path)`s the prim and exposes `Get*Attr`/`Create*Attr`); the hierarchy is `UsdGeomImageable → Xformable → Boundable → Gprim → PointBased → {Mesh, Points, Curves, NurbsPatch}`

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY]    | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `UsdGeomMesh`                                                        | mesh schema      | points + faceVertexCounts/Indices            |
|  [02]   | `UsdGeomPointBased`                                                  | point-based base | `GetPointsAttr`/`CreatePointsAttr` + normals |
|  [03]   | `UsdGeomXformable` / `UsdGeomXformOp`                                | transformable    | the ordered xform-op stack on a prim         |
|  [04]   | `UsdGeomXform` / `UsdGeomScope`                                      | grouping schema  | transform group / pure namespace group       |
|  [05]   | `UsdGeomPoints`                                                      | point schema     | the point-cloud schema                       |
|  [06]   | `UsdGeomBasisCurves` / `UsdGeomNurbsCurves` / `UsdGeomNurbsPatch`    | curve/patch      | basis/NURBS curves and NURBS patch           |
|  [07]   | `UsdGeomCube` / `Sphere` / `Cylinder` / `Cone` / `Capsule` / `Plane` | gprim            | the analytic primitive schemas               |
|  [08]   | `UsdGeomPrimvar` / `UsdGeomPrimvarsAPI`                              | primvar          | per-vertex/face/uniform interpolation        |
|  [09]   | `UsdGeomPointInstancer`                                              | instancer        | point-instanced prototype scattering         |
|  [10]   | `UsdGeomSubset`                                                      | face subset      | named face-index subset (per-material)       |

[PUBLIC_TYPE_SCOPE]: shading, skeletal, light, and compute-cache schemas
- rail: format#USD_SCHEMA
- note: each is a typed view over a `UsdPrim`; material binding rides `UsdShadeMaterialBindingAPI`, and world-space caches ride `UsdGeomBBoxCache`/`UsdGeomXformCache`

| [INDEX] | [SYMBOL]                                                      | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------ | :------------ | :------------------------------------------------- |
|  [01]   | `UsdShadeMaterial` / `UsdShadeShader` / `UsdShadeNodeGraph`   | shade network | the material/shader/node-graph shading prims       |
|  [02]   | `UsdShadeMaterialBindingAPI`                                  | shade binding | material binding (`Bind`)                          |
|  [03]   | `UsdShadeInput` / `UsdShadeOutput` / `UsdShadeConnectableAPI` | shade I/O     | connectable input/output I/O                       |
|  [04]   | `UsdGeomBBoxCache` / `UsdGeomXformCache`                      | compute cache | cached world-bound and world-transform computation |
|  [05]   | `UsdSkelRoot` / `UsdSkelSkeleton` / `UsdSkelBindingAPI`       | skel schema   | skeletal animation schemas                         |
|  [06]   | `UsdLuxRectLight` / `UsdLuxDomeLight`                         | light schema  | rect/dome lighting schemas                         |

[PUBLIC_TYPE_SCOPE]: math and typed-array value types
- rail: format#USD_VALUE
- note: `Gf*` are value math types; the typed arrays are `VtVec3fArray`/`VtVec3dArray`/`VtIntArray`/`VtFloatArray`/`VtTokenArray`, each declaring the explicit `(VtVec3fArray)value` unbox from `VtValue`; `VtValue` is the type-erased box `UsdAttribute.Get`/`Set` exchanges; `TfToken` is the interned-string key

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `VtValue`                                           | type-erased value | the value box every `UsdAttribute.Get`/`Set` exchanges       |
|  [02]   | `Vt*Array`                                          | typed array       | point/index/scalar/token arrays; mesh-bridge unbox           |
|  [03]   | `Vt_ArrayBase`                                      | array base        | the `Vt*Array` base (`size`/`resize`/indexer)                |
|  [04]   | `GfVec3f` / `GfVec3d` / `GfVec2f`                   | vector value      | the point/normal/uv component value types                    |
|  [05]   | `GfMatrix4d` / `GfMatrix3d`                         | matrix value      | transform values; `GfMatrix4d.GetRow(int)` → `GfVec4d`       |
|  [06]   | `GfQuatf` / `GfRotation` / `GfBBox3d` / `GfRange3d` | math value        | quaternion, rotation, bound, range value types               |
|  [07]   | `TfToken` / `TfTokenVector`                         | interned token    | name/key type for prim/attribute names, schema ids           |
|  [08]   | `SdfValueTypeName` / `SdfValueTypeNames`            | value-type name   | type-name `CreateAttribute` takes (`Point3fArray`/`Token`/…) |
|  [09]   | `SdfVariability` / `SdfSpecifier`                   | attribute kind    | varying-vs-uniform and def/over/class specifier              |
|  [10]   | `ArResolverContext`                                 | asset resolver    | the asset-resolution context a stage open binds              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage open, create, and export
- rail: format#USD_STAGE
- note: surfaces are `UsdStage` methods unless prefixed `UsdGeom.`; `Open` reads a file/layer, `CreateNew`/`CreateInMemory` author a fresh stage, `Export`/`ExportToString`/`Flatten` write it, and `InitialLoadSet` (`LoadAll`/`LoadNone`) is the nested payload-load enum

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------------------------------ |
|  [01]   | `Open(string, InitialLoadSet)` / `Open(SdfLayerHandle, …)`          | static open    | reads a `.usd*` file or layer into a stage        |
|  [02]   | `OpenMasked(string, UsdStagePopulationMask, …)`                     | static open    | partial-stage open under a population mask        |
|  [03]   | `CreateNew(string identifier, …)` / `CreateInMemory(…)`             | static create  | authors a new on-disk or in-memory stage          |
|  [04]   | `Save()` / `SaveSessionLayers()` / `Reload()`                       | persist        | writes dirty / session layers / reloads           |
|  [05]   | `Export(string, bool, StdStringMap)` / `ExportToString(out string)` | export         | writes the composed stage to a file/string        |
|  [06]   | `Flatten(bool addSourceFileComment)` → `SdfLayerHandle`             | flatten        | composes the layer stack into one flat layer      |
|  [07]   | `GetRootLayer` / `GetSessionLayer`                                  | layer access   | the root and session layers                       |
|  [08]   | `GetEditTarget` / `SetEditTarget(UsdEditTarget)`                    | layer access   | reads/sets the active edit target                 |
|  [09]   | `UsdGeom.UsdGeomGetStageUpAxis(UsdStage)` → `TfToken`               | stage metadata | per-stage `upAxis` (`"Y"` default, `"Z"` CAD/BIM) |
|  [10]   | `UsdGeom.UsdGeomSetStageUpAxis(UsdStage, TfToken)` → `bool`         | stage metadata | authors the per-stage `upAxis` token              |
|  [11]   | `UsdGeom.UsdGeomGetStageMetersPerUnit(UsdStage)` → `double`         | stage metadata | per-stage linear-unit scale (`metersPerUnit`)     |
|  [12]   | `MuteLayer(string)`                                                 | stage meta     | layer muting                                      |
|  [13]   | `SetStartTimeCode` / `SetEndTimeCode` / `SetTimeCodesPerSecond`     | stage meta     | the animation time range (`double`)               |
|  [14]   | `IsSupportedFile(string filePath)`                                  | probe          | format admissibility before open                  |

[ENTRYPOINT_SCOPE]: prim definition, traversal, and composition
- rail: format#USD_PRIM
- note: surfaces are `UsdStage` or `UsdPrim` methods; `DefinePrim`/`OverridePrim` author the namespace, `Traverse` walks it, and `GetReferences`/`GetPayloads`/`GetVariantSets` author the composition arcs

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `DefinePrim(SdfPath, TfToken typeName)` / `DefinePrim(SdfPath)`              | author         | defines (or returns) a typed prim      |
|  [02]   | `OverridePrim(SdfPath)` / `CreateClassPrim(SdfPath)` / `RemovePrim(SdfPath)` | author         | override / class prim / removal        |
|  [03]   | `GetPrimAtPath` / `GetObjectAtPath` / `GetDefaultPrim` / `SetDefaultPrim`    | navigate       | path lookup + default-prim access      |
|  [04]   | `Traverse()` / `Traverse(Usd_PrimFlagsPredicate)` / `TraverseAll()`          | traverse       | scene-graph walk → `UsdPrimRange`      |
|  [05]   | `GetChildren` / `GetParent` / `GetChild(TfToken)`                            | navigate       | prim children and parent               |
|  [06]   | `GetPath` / `GetName` / `GetTypeName`                                        | navigate       | prim identity                          |
|  [07]   | `GetReferences().AddReference(SdfReference, UsdListPosition)`                | compose        | external reference arc                 |
|  [08]   | `GetPayloads()`                                                              | compose        | deferred payload arc                   |
|  [09]   | `GetVariantSets().AddVariantSet(string)`                                     | compose        | variant set authoring                  |
|  [10]   | `GetVariantSet(string).SetVariantSelection(…)`                               | compose        | variant selection                      |
|  [11]   | `IsA(TfType)` / `HasAPI(TfType)` / `ApplyAPI(TfToken)`                       | schema query   | typed-schema + applied-API membership  |
|  [12]   | `IsActive()` / `SetActive(bool)`                                             | schema query   | prim active-state                      |
|  [13]   | `UsdPrim.Load(SdfPath)` / `Unload(SdfPath)`                                  | payload load   | prim-level payload activate/deactivate |
|  [14]   | `UsdStage.Load(SdfPath, UsdLoadPolicy)`                                      | payload load   | stage-level payload load               |

[ENTRYPOINT_SCOPE]: attribute and primvar authoring + value IO
- rail: format#USD_VALUE
- note: surfaces are `UsdPrim`/`UsdAttribute` methods; `CreateAttribute` takes a `SdfValueTypeName`, `Set`/`Get` exchange through `VtValue` at an optional `UsdTimeCode`, and the typed `Vt*Array` is the bulk payload

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `CreateAttribute(TfToken, SdfValueTypeName, SdfVariability)`                | author         | authors a typed attribute on a prim       |
|  [02]   | `GetAttribute(TfToken)` / `GetAttributes()`                                 | access         | attribute lookup                          |
|  [03]   | `CreateRelationship(TfToken)` / `GetRelationship(TfToken)`                  | access         | relationship lookup                       |
|  [04]   | `UsdAttribute.Set(VtValue, UsdTimeCode)` / `Set(VtValue)`                   | write          | writes a (time-sampled) attribute value   |
|  [05]   | `UsdAttribute.Get(VtValue, UsdTimeCode)` / `GetTypeName()`                  | read           | reads a (time-sampled) attribute value    |
|  [06]   | `GetNumTimeSamples()` / `GetTimeSamples()` / `ValueMightBeTimeVarying()`    | time query     | time-sample inspection for animated data  |
|  [07]   | `new VtVec3fArray(uint n)` / `push_back(GfVec3f)` / `resize(uint)`          | array build    | the bulk point/normal payload array       |
|  [08]   | `size()` / `this[int]`                                                      | array build    | array size and indexer                    |
|  [09]   | `UsdGeomPrimvarsAPI.CreatePrimvar(TfToken, SdfValueTypeName, TfToken, int)` | primvar        | per-vertex/face/uniform interpolated data |
|  [10]   | `new SdfPath(string)` / `AppendChild(TfToken)` / `AppendProperty(TfToken)`  | path build     | scene-graph path construction             |
|  [11]   | `IsPrimPath()` / `GetAsString()`                                            | path build     | path predicate and string form            |

[ENTRYPOINT_SCOPE]: typed geometry/shading schema authoring
- rail: format#USD_SCHEMA
- note: each schema `Define(stage, path)`s the prim and exposes `Get*Attr`/`Create*Attr`; `new UsdGeom*(UsdPrim)` wraps a traversed prim (import read); material binding is `UsdShadeMaterialBindingAPI.Apply(prim).Bind(...)`

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `UsdGeomMesh.Define(UsdStage, SdfPath)` / `new UsdGeomMesh(UsdPrim)`         | define/wrap    | mesh prim define + typed wrap     |
|  [02]   | `UsdGeomMesh.GetPointsAttr()` / `GetNormalsAttr()`                           | mesh attr      | points and normals (`PointBased`) |
|  [03]   | `UsdGeomMesh.CreatePointsAttr(VtValue, bool writeSparsely)`                  | mesh attr      | authors (sparse) points           |
|  [04]   | `UsdGeomMesh.GetFaceVertexCountsAttr()` / `GetFaceVertexIndicesAttr()`       | mesh attr      | face topology                     |
|  [05]   | `UsdGeomMesh.GetSubdivisionSchemeAttr()`                                     | mesh attr      | subdivision scheme                |
|  [06]   | `UsdGeomXformable.AddXformOp(Type, Precision, TfToken, bool)`                | xform          | adds an ordered transform op      |
|  [07]   | `UsdGeomXformable.AddTranslateOp(…)` / `AddRotateXYZOp(…)` / `AddScaleOp(…)` | xform          | convenience xform-op authors      |
|  [08]   | `UsdGeomXformable.SetXformOpOrder(…)`                                        | xform          | sets the xform-op order           |
|  [09]   | `UsdGeomImageable.MakeVisible(UsdTimeCode)` / `MakeInvisible(UsdTimeCode)`   | visibility     | visibility authoring              |
|  [10]   | `UsdGeomImageable.ComputeVisibility(…)`                                      | visibility     | resolves computed visibility      |
|  [11]   | `UsdShadeMaterial.Define(UsdStage, SdfPath)`                                 | shade          | material define                   |
|  [12]   | `UsdShadeMaterialBindingAPI.Bind(UsdShadeMaterial, TfToken, TfToken)`        | shade          | material bind                     |
|  [13]   | `UsdGeomXformCache.GetLocalToWorldTransform(UsdPrim)` → `GfMatrix4d`         | compute        | cached world transform            |
|  [14]   | `UsdGeomBBoxCache.ComputeWorldBound(UsdPrim)` → `GfBBox3d`                   | compute        | cached world bound                |

## [04]-[IMPLEMENTATION_LAW]

[STAGE_TOPOLOGY]:
- `UsdStage` is the composed view over a layer stack; `Open`/`CreateNew`/`CreateInMemory` enter it, `Save`/`Export`/`ExportToString`/`Flatten` write it, and the `usd-stage` codec reads and writes `.usd`/`.usda`/`.usdc`/`.usdz` through it (the file format is the extension's, resolved by the native plugin tree)
- the namespace is `UsdPrim` nodes addressed by `SdfPath`; `DefinePrim(path, typeName)` authors a typed prim, `OverridePrim` an over, `CreateClassPrim` a class; traversal is `Traverse(Usd_PrimFlagsPredicate)` → `UsdPrimRange`
- composition is arcs on a prim: `GetReferences().AddReference(SdfReference, UsdListPosition)`, `GetPayloads()` (deferred load via `Load`/`Unload`), `GetVariantSets().AddVariantSet(...)`, `GetInherits()`, `GetSpecializes()` — opinions land on the active `UsdEditTarget` (`GetEditTarget`/`SetEditTarget`), so a non-destructive override layer is the edit target, never a destructive flatten
- attributes are typed: `CreateAttribute(TfToken, SdfValueTypeName, SdfVariability)`; values exchange through `VtValue` at an optional `UsdTimeCode` (`Set`/`Get`), and bulk data rides a typed `Vt*Array` (`VtVec3fArray.push_back`/`size`/indexer); the typed schemas (`UsdGeomMesh.Define` + `GetPointsAttr`/`GetFaceVertexIndicesAttr`, `UsdGeomXformable.AddXformOp`, `UsdShadeMaterialBindingAPI.Bind`) are the authoring surface, never raw attribute strings
- the schema hierarchy is `UsdGeomImageable → Xformable → Boundable → Gprim → PointBased → {Mesh, Points, Curves}`; applied APIs (`UsdGeomPrimvarsAPI`, `UsdShadeMaterialBindingAPI`, `UsdCollectionAPI`) attach through `Apply(prim)`/`Get(stage, path)`
- every `pxr` handle is `IDisposable` over a native USD object — `UsdStage`, `SdfLayer`, `Vt*Array`, `VtValue`, and the schema views own native handles released by `Dispose`; the `SWIGTYPE_p_*`/`*PINVOKE` types are interop plumbing

[INTEGRATION_STACK]:
- codec row: `Exchange/format#FORMAT_TABLE` already carries `InterchangeCodec.UsdStage = new("usd-stage", managed: true, companion: false, …)` and the `usd`/`usdz` `InterchangeFormat` rows (media types `model/vnd.usd`/`model/vnd.usdz+zip`, `UpAxis`/`Handedness` columns); the `import#IMPORT_RAIL` `BimIo` and `export#EXPORT_RAIL` `BimExport` folds dispatch this codec by row data, never an `if (usd)` call-site branch
- scene-graph peer, NOT a BIM replacement: per the `format#FORMAT_TABLE` boundary law USD carries the scene (geometry/shading/instancing/lighting) while IFC carries the BIM semantics — a USD import lands a `UsdGeomMesh`/`UsdGeomXform` scene the kernel consumes as geometry, and the BIM element vocabulary (`Model/elements#ELEMENT_MODEL` `BimElement`/`IfcClass`) is the GeometryGym graph's, never re-derived from USD prim type names
- mesh bridge: a kernel `Rasm` mesh crosses to/from `UsdGeomMesh` through the typed-array seam — `GetPointsAttr().Get(VtValue)` yields a `VtVec3fArray` of `GfVec3f` points and `GetFaceVertexIndicesAttr()`/`GetFaceVertexCountsAttr()` the topology, marshaled to the kernel mesh vocabulary; the export half builds the `VtVec3fArray`/`VtIntArray` from the kernel mesh and writes them through `CreatePointsAttr()` + `GetFaceVertexIndicesAttr().Set(VtValue)` (USD authors values via the attribute `Set`, never a `Set*Attr` method), with `UsdGeomPrimvarsAPI.CreatePrimvar` carrying uv/color per-vertex data
- transform reconciliation: USD is Y-up by default while IFC is Z-up — the `format#FORMAT_TABLE` `FrameNormalization` row (the `usd` `UpAxis`/`Handedness` columns) coerces the imported `UsdGeomXformable` op stack / `UsdGeomXformCache.GetLocalToWorldTransform` `GfMatrix4d` onto the canonical kernel frame, so a USD scene lands in the same frame an IFC import does, never a USD-local frame leaking downstream
- shading seam: `UsdShadeMaterial`/`UsdShadeShader` (UsdPreviewSurface) maps to the `Semantics/appearance#APPEARANCE_PROJECTION` `AppearanceSummary` host-neutral PBR record, reconciled with the `csharp:Rasm.Materials` OpenPBR owner at the content-key seam — the USD shade network is read/authored through `UsdShadeMaterialBindingAPI`, not a parallel material model
- composition for federation: USD references/payloads/variants are the layered-federation mechanism — a per-discipline model is a sublayer or reference and a design option is a variant set, so the `Review/diff#MODEL_DIFF` federation and the `AppUi` variant switching compose on USD's native arc model rather than a re-minted overlay; `OpenMasked`/`UsdStageLoadRules` give partial-stage streaming for large scenes
- 3D-Tiles/glTF coexistence: USD and glTF (`api-sharpgltf`) are sibling scene codecs on the `format#FORMAT_TABLE` — a glTF export of a USD scene crosses the kernel mesh vocabulary both codecs share, never a USD↔glTF direct converter minted in Bim

[LOCAL_ADMISSION]:
- `UsdStage` is the one USD codec root; one stage reads and writes every `.usd*` variant by the native plugin tree, never a per-format `UsdaReader`/`UsdcReader` family
- prim/attribute authoring goes through the typed schemas (`UsdGeomMesh`, `UsdGeomXformable`, `UsdShadeMaterialBindingAPI`) and the `SdfValueTypeName`/`VtValue`/`Vt*Array` value path; a raw attribute-name string beside the typed schema is the rejected form
- the BIM semantics stay the GeometryGym IFC graph's — USD is the scene-graph peer, and deriving `BimElement`/`IfcClass` from USD prim names rather than the IFC graph is the named boundary violation
- the imported frame is normalized through the `format#FORMAT_TABLE` `FrameNormalization` Y-up→Z-up row; a USD-local frame leaking into the kernel is the rejected form
- native USD handles enter only through declared `IDisposable` roots and release deterministically; a stage op with no matching RID native payload + Pixar plugin tree faults at native load (`BimFault.CapabilityMiss`), and the `SWIGTYPE_p_*`/`*PINVOKE` interop types never enter canonical owners

[RAIL_LAW]:
- Package: `UniversalSceneDescription` (, MPL-2.0, `requireLicenseAcceptance`, managed `lib/net10.0` SWIG wrapper over per-RID native USD + Pixar plugin tree + Alembic; `osx-arm64` verified)
- Owns: the OpenUSD scene-graph read/write — `UsdStage` layer composition (sublayers/references/payloads/variants/inherits/specializes), prim/attribute authoring on an edit target, the `UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux` typed schemas, time-sampled values, and export/flatten
- Accept: a `.usd*` scene read/authored through `UsdStage` and the typed schemas, the mesh crossing the `VtVec3fArray`/`VtIntArray` seam to the kernel vocabulary, the frame normalized Y-up→Z-up through `FrameNormalization`, the shade network reconciled with the seam `AppearanceSummary`/OpenPBR, and federation/options composed on USD references/variants — coexisting with the IFC semantic graph as a scene-graph peer
- Reject: deriving BIM semantics (`BimElement`/`IfcClass`) from USD prim type names instead of the GeometryGym IFC graph; a raw attribute-name string beside the typed schema; a USD-local frame leaking past `FrameNormalization`; a USD↔glTF direct converter minted in Bim; a per-format USD reader family where one `UsdStage` discriminates by plugin; the `SWIGTYPE_p_*`/`*PINVOKE` interop types in canonical owners; a stage op with no matching native RID payload + plugin tree
