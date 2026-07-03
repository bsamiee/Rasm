# [RASM_BIM_API_USD]

`UniversalSceneDescription` is the managed OpenUSD scene-graph codec backing the `Exchange/format#FORMAT_TABLE` `InterchangeCodec.UsdStage` slot — a faithful SWIG-generated C# binding to the Pixar USD C++ runtime exposing the full `pxr` namespace (Sdf layers, Usd stage/prim/property, the `UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux` typed schemas, the `Gf` math and `Vt` typed-array value types). It owns the read AND write of `.usd`/`.usda`/`.usdc`/`.usdz` through `UsdStage` — layer composition (sublayers, references, payloads, variants, inherits, specializes), prim/attribute authoring on a chosen `UsdEditTarget`, time-sampled values, and full export/flatten. In the Bim codec table USD is a scene-graph PEER, never a BIM-semantic replacement: USD carries the geometry/shading/instancing scene, the GeometryGym IFC graph carries the BIM semantics, and the two coexist at the `format#FORMAT_TABLE` row. The codec load fault lifts to `BimFault.CodecReject` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UniversalSceneDescription`
- package: `UniversalSceneDescription` (version 7.3.4, direct pin)
- license: MPL-2.0 (`EggyStudio/UniversalSceneDescription`); `requireLicenseAcceptance=true` — the file-level reciprocity is satisfied by referencing the unmodified NuGet binaries, never modifying its source files
- assembly: `UniversalSceneDescription.dll` AND `USD.NET.dll` → both bind at `lib/net10.0/` (the exact consumer TFM); the public USD surface lives across both under one `pxr` namespace
- namespace: `pxr` (the entire USD binding — `Ar`/`Gf`/`Ndr`/`Pcp`/`Sdf`/`Sdr`/`Tf`/`Usd`/`UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux`/`Vt`); `SdfValueTypeNames` is the value-type-name table
- asset: a managed SWIG wrapper over per-RID native USD libraries — `runtimes/{osx-arm64,osx-x64,linux-x64,win-x64}/native` ship `libusd_*.dylib`/`.so`/`.dll` plus the Pixar `usd/plugInfo.json` plugin tree and `libAlembic` (`osx-arm64` verified); a stage op with no matching RID native payload + plugin tree faults at native load
- runtime law: the SWIG binding handles are `IDisposable` over native USD objects (`UsdStage`/`UsdPrim`/`SdfLayer`/`Vt*Array`/`VtValue` and every `pxr` handle); `SWIGTYPE_p_*` and `*PINVOKE` are interop plumbing and stay out of canonical owners
- rail: `format#FORMAT_TABLE` (the `usd-stage` codec; scene-graph peer to the IFC semantic graph)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stage and layer composition roots
- rail: format#USD_STAGE

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :----------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `UsdStage`               | stage root         | the composed scene — open/create/save/export over a layer stack |
|  [02]   | `SdfLayer` / `SdfLayerHandle` | layer            | one `.usd*` layer of opinions (the root/session/sublayers)  |
|  [03]   | `SdfPath`                | scene-graph path   | the prim/property address (`/World/Mesh.points`)            |
|  [04]   | `UsdEditTarget`          | edit target        | which layer authored opinions land in                       |
|  [05]   | `UsdTimeCode`            | time code          | a time-sampled value coordinate (`Default()`/`EarliestTime()`/`(double)`) |
|  [06]   | `UsdStagePopulationMask` / `UsdStageLoadRules` | load policy | masked/partial stage population and payload load rules |
|  [07]   | `UsdPrimRange`           | traversal range    | the prim iteration a stage `Traverse` yields                |
|  [08]   | `Usd_PrimFlagsPredicate` | traversal filter   | the active/defined/loaded predicate a filtered traversal takes |

[PUBLIC_TYPE_SCOPE]: prim, property, and composition arcs
- rail: format#USD_PRIM

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]      | [RAIL]                                                    |
| :-----: | :-------------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `UsdPrim`                   | prim               | a scene-graph node (typed schema + children + properties) |
|  [02]   | `UsdObject`                 | object base        | `GetStage`/`GetPath`/`GetPrim`/`GetName` over prim+property |
|  [03]   | `UsdProperty` / `UsdAttribute` / `UsdRelationship` | property | a prim's typed attribute or relationship       |
|  [04]   | `UsdReferences` / `UsdPayloads` | composition arc | external/internal reference and deferred payload arcs    |
|  [05]   | `UsdInherits` / `UsdSpecializes` | composition arc | class inheritance and specialization arcs              |
|  [06]   | `UsdVariantSets` / `UsdVariantSet` | variant arc  | named variant sets and the selected variant              |
|  [07]   | `SdfReference` / `SdfPayload` / `UsdListPosition` | arc value | the reference/payload value and list-edit position |
|  [08]   | `UsdModelAPI` / `UsdCollectionAPI` | applied API   | model-kind metadata and membership collections           |

[PUBLIC_TYPE_SCOPE]: typed geometry and shading schemas
- rail: format#USD_SCHEMA
- note: the `UsdGeom*` schemas are typed views over a `UsdPrim` (each `Define(stage, path)`s the prim and exposes `Get*Attr`/`Create*Attr`); the schema hierarchy is `UsdGeomImageable → Xformable → Boundable → Gprim → PointBased → {Mesh, Points, Curves, NurbsPatch}`.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :---------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `UsdGeomMesh`                             | mesh schema        | points + faceVertexCounts/Indices + subdivision scheme |
|  [02]   | `UsdGeomPointBased`                       | point-based base   | `GetPointsAttr`/`CreatePointsAttr` + normals/velocities |
|  [03]   | `UsdGeomXformable` / `UsdGeomXformOp`     | transformable      | the ordered xform-op stack on a prim                   |
|  [04]   | `UsdGeomXform` / `UsdGeomScope`           | grouping schema    | transform group / pure namespace group                 |
|  [05]   | `UsdGeomPoints` / `UsdGeomBasisCurves` / `UsdGeomNurbsCurves` / `UsdGeomNurbsPatch` | point/curve schema | point cloud, basis/NURBS curves, NURBS patch |
|  [06]   | `UsdGeomCube` / `Sphere` / `Cylinder` / `Cone` / `Capsule` / `Plane` | gprim | the analytic primitive schemas                         |
|  [07]   | `UsdGeomPrimvar` / `UsdGeomPrimvarsAPI`   | primvar            | interpolated per-vertex/face/uniform attribute data    |
|  [08]   | `UsdGeomPointInstancer`                   | instancer          | point-instanced prototype scattering                   |
|  [09]   | `UsdGeomSubset`                           | face subset        | a named face-index subset (per-material binding)       |
|  [10]   | `UsdShadeMaterial` / `UsdShadeShader` / `UsdShadeNodeGraph` | shade network | the material/shader/node-graph shading prims |
|  [11]   | `UsdShadeMaterialBindingAPI` / `UsdShadeInput` / `UsdShadeOutput` / `UsdShadeConnectableAPI` | shade binding | material binding and connectable I/O |
|  [12]   | `UsdGeomBBoxCache` / `UsdGeomXformCache`  | compute cache      | cached world-bound and world-transform computation     |
|  [13]   | `UsdSkelRoot` / `UsdSkelSkeleton` / `UsdSkelBindingAPI` / `UsdLuxRectLight` / `UsdLuxDomeLight` | skel/light schema | skeletal animation and lighting schemas |

[PUBLIC_TYPE_SCOPE]: math and typed-array value types
- rail: format#USD_VALUE
- note: `Gf*` are the value math types; `Vt*Array` are the typed arrays USD attributes hold; `VtValue` is the type-erased value box `UsdAttribute.Get`/`Set` exchange; `TfToken` is the interned-string key for names/tokens.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :---------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `VtValue`                                 | type-erased value  | the value box every `UsdAttribute.Get`/`Set` exchanges |
|  [02]   | `VtVec3fArray` / `VtVec3dArray` / `VtIntArray` / `VtFloatArray` / `VtTokenArray` | typed array | the point/index/scalar/token attribute arrays; each declares the explicit conversion FROM `VtValue` (`(VtVec3fArray)value` / `(VtIntArray)value`) — the typed unbox the mesh-bridge reads after `UsdAttribute.Get(VtValue, …)` |
|  [03]   | `Vt_ArrayBase`                            | array base         | the base every `Vt*Array` derives (`size`/`resize`/indexer) |
|  [04]   | `GfVec3f` / `GfVec3d` / `GfVec2f`         | vector value       | the point/normal/uv component value types              |
|  [05]   | `GfMatrix4d` / `GfMatrix3d`               | matrix value       | the transform value types (`UsdGeomXformCache` output); `GfMatrix4d.GetRow(int)` → `GfVec4d` (indexer-addressable `this[int]` components) — the row-major double read the numerics narrow folds |
|  [06]   | `GfQuatf` / `GfRotation` / `GfBBox3d` / `GfRange3d` | math value | quaternion, rotation, bound, and range value types |
|  [07]   | `TfToken` / `TfTokenVector`               | interned token     | the name/key type for prim names, attribute names, schema ids |
|  [08]   | `SdfValueTypeName` / `SdfValueTypeNames`  | value-type name    | the attribute type-name (`Point3fArray`/`Normal3fArray`/`TexCoord2fArray`/`Token`/…) `CreateAttribute` takes |
|  [09]   | `SdfVariability` / `SdfSpecifier`         | attribute kind     | varying-vs-uniform and def/over/class specifier         |
|  [10]   | `ArResolverContext`                       | asset resolver     | the asset-resolution context a stage open binds         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage open, create, and export
- rail: format#USD_STAGE
- note: `UsdStage` is the codec root; `Open` reads a file/layer, `CreateNew`/`CreateInMemory` author a fresh stage, `Export`/`ExportToString`/`Flatten` write the composed result.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `UsdStage.Open(string filePath, UsdStage.InitialLoadSet load)` / `Open(SdfLayerHandle rootLayer, …)` | static open | reads a `.usd*` file or layer into a stage; `UsdStage.InitialLoadSet` is the nested payload-load enum (`LoadAll`/`LoadNone`) the open/create overloads take |
|  [02]   | `UsdStage.OpenMasked(string filePath, UsdStagePopulationMask mask, …)`           | static open    | partial-stage open under a population mask        |
|  [03]   | `UsdStage.CreateNew(string identifier, …)` / `CreateInMemory(…)`                 | static create  | authors a new on-disk or in-memory stage          |
|  [04]   | `UsdStage.Save()` / `SaveSessionLayers()` / `Reload()`                           | persist        | writes dirty layers / session layers / reloads    |
|  [05]   | `UsdStage.Export(string filename, bool addSourceFileComment, StdStringMap args)` / `ExportToString(out string)` | export | writes the composed stage to a file/string |
|  [06]   | `UsdStage.Flatten(bool addSourceFileComment)` → `SdfLayerHandle`                 | flatten        | composes the layer stack into one flat layer      |
|  [07]   | `UsdStage.GetRootLayer()` / `GetSessionLayer()` / `GetEditTarget()` / `SetEditTarget(UsdEditTarget)` | layer access | the root/session layers and the active edit target |
|  [08]   | `UsdGeom.UsdGeomGetStageUpAxis(UsdStage)` → `TfToken` / `UsdGeomSetStageUpAxis(UsdStage, TfToken)` → `bool` | stage metadata | the PER-STAGE `upAxis` metadatum (`"Y"` the USD default, `"Z"` the CAD/BIM export posture) — the import basis selects per stage off this token, the `format` row's Y-up `Frame` staying the default |
|  [09]   | `UsdGeom.UsdGeomGetStageMetersPerUnit(UsdStage)` → `double`                      | stage metadata | the per-stage linear-unit scale (`metersPerUnit`, default 0.01) beside the up-axis read |
|  [08]   | `UsdStage.MuteLayer(string)` / `SetStartTimeCode(double)` / `SetEndTimeCode(double)` / `SetTimeCodesPerSecond(double)` | stage meta | layer muting and the animation time range |
|  [09]   | `UsdStage.IsSupportedFile(string filePath)`                                      | probe          | format admissibility before open                  |

[ENTRYPOINT_SCOPE]: prim definition, traversal, and composition
- rail: format#USD_PRIM
- note: `DefinePrim`/`OverridePrim` author the namespace; `Traverse` walks it; `GetReferences`/`GetPayloads`/`GetVariantSets` author the composition arcs.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `UsdStage.DefinePrim(SdfPath path, TfToken typeName)` / `DefinePrim(SdfPath)`     | author         | defines (or returns) a typed prim                 |
|  [02]   | `UsdStage.OverridePrim(SdfPath)` / `CreateClassPrim(SdfPath)` / `RemovePrim(SdfPath)` | author    | an override / class prim / removal                |
|  [03]   | `UsdStage.GetPrimAtPath(SdfPath)` / `GetObjectAtPath(SdfPath)` / `GetDefaultPrim()` / `SetDefaultPrim(UsdPrim)` | navigate | path lookup and default-prim access |
|  [04]   | `UsdStage.Traverse()` / `Traverse(Usd_PrimFlagsPredicate)` / `TraverseAll()` → `UsdPrimRange` | traverse | filtered/unfiltered scene-graph walk         |
|  [05]   | `UsdPrim.GetChildren()` / `GetParent()` / `GetChild(TfToken)` / `GetPath()` / `GetName()` / `GetTypeName()` | navigate | prim hierarchy and identity            |
|  [06]   | `UsdPrim.GetReferences().AddReference(SdfReference, UsdListPosition)` / `GetPayloads()` | compose   | external reference / deferred payload arc         |
|  [07]   | `UsdPrim.GetVariantSets().AddVariantSet(string)` / `GetVariantSet(string).SetVariantSelection(…)` | compose | variant set authoring and selection       |
|  [08]   | `UsdPrim.IsA(TfType)` / `HasAPI(TfType)` / `ApplyAPI(TfToken)` / `IsActive()` / `SetActive(bool)` | schema query | typed-schema and applied-API membership   |
|  [09]   | `UsdPrim.Load(SdfPath)` / `UsdStage.Load(SdfPath, UsdLoadPolicy)` / `Unload(SdfPath)`          | payload load | activate/deactivate a payload subtree        |

[ENTRYPOINT_SCOPE]: attribute and primvar authoring + value IO
- rail: format#USD_VALUE
- note: `CreateAttribute` takes a `SdfValueTypeName`; `Set`/`Get` exchange through `VtValue` at an optional `UsdTimeCode`; the typed `Vt*Array` is the bulk point/index/uv payload.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `UsdPrim.CreateAttribute(TfToken name, SdfValueTypeName typeName, SdfVariability variability)` | author | authors a typed attribute on a prim        |
|  [02]   | `UsdPrim.GetAttribute(TfToken)` / `GetAttributes()` / `CreateRelationship(TfToken)` / `GetRelationship(TfToken)` | access | attribute/relationship lookup            |
|  [03]   | `UsdAttribute.Set(VtValue value, UsdTimeCode time)` / `Set(VtValue value)`       | write          | writes a (time-sampled) attribute value           |
|  [04]   | `UsdAttribute.Get(VtValue value, UsdTimeCode time)` / `GetTypeName()`            | read           | reads a (time-sampled) attribute value            |
|  [05]   | `UsdAttribute.GetNumTimeSamples()` / `GetTimeSamples()` / `ValueMightBeTimeVarying()` | time query | the time-sample inspection for animated data  |
|  [06]   | `new VtVec3fArray(uint n)` / `push_back(GfVec3f)` / `resize(uint)` / `size()` / `this[int]` | array build | the bulk point/normal payload array         |
|  [07]   | `UsdGeomPrimvarsAPI.Get(UsdStage, SdfPath).CreatePrimvar(TfToken, SdfValueTypeName, TfToken interpolation, int elementSize)` | primvar | per-vertex/face/uniform interpolated data |
|  [08]   | `new SdfPath(string path)` / `AppendChild(TfToken)` / `AppendProperty(TfToken)` / `IsPrimPath()` / `GetAsString()` | path build | scene-graph path construction              |

[ENTRYPOINT_SCOPE]: typed geometry/shading schema authoring
- rail: format#USD_SCHEMA
- note: each schema `Define(stage, path)`s the prim and exposes `Get*Attr`/`Create*Attr`; the xform-op stack and material binding are authored through the typed schema, never raw attributes.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `UsdGeomMesh.Define(UsdStage stage, SdfPath path)`; `new UsdGeomMesh(UsdPrim prim)`            | schema define / wrap | `Define` authors a new mesh prim (export); the `new UsdGeomMesh(UsdPrim)` ctor wraps a traversed prim onto the typed mesh schema (the import read path after `Traverse`) |
|  [02]   | `UsdGeomMesh.GetPointsAttr()` / `CreatePointsAttr(VtValue, bool writeSparsely)` / `GetNormalsAttr()` | mesh attr | points / sparse points / normals (`PointBased`) |
|  [03]   | `UsdGeomMesh.GetFaceVertexCountsAttr()` / `GetFaceVertexIndicesAttr()` / `GetSubdivisionSchemeAttr()` | mesh attr | topology and subdivision scheme          |
|  [04]   | `UsdGeomXformable.AddXformOp(UsdGeomXformOp.Type, UsdGeomXformOp.Precision, TfToken opSuffix, bool isInverseOp)` | xform | adds an ordered transform op |
|  [05]   | `UsdGeomXformable.AddTranslateOp(…)` / `AddRotateXYZOp(…)` / `AddScaleOp(…)` / `SetXformOpOrder(…)` | xform | the convenience xform-op authors            |
|  [06]   | `UsdGeomImageable.MakeVisible(UsdTimeCode)` / `MakeInvisible(UsdTimeCode)` / `ComputeVisibility(…)` | visibility | the visibility authoring               |
|  [07]   | `UsdShadeMaterial.Define(UsdStage, SdfPath)` / `UsdShadeMaterialBindingAPI.Apply(UsdPrim).Bind(UsdShadeMaterial, TfToken bindingStrength, TfToken materialPurpose)` | shade | material define and bind |
|  [08]   | `UsdGeomXformCache.GetLocalToWorldTransform(UsdPrim)` → `GfMatrix4d` / `UsdGeomBBoxCache.ComputeWorldBound(UsdPrim)` → `GfBBox3d` | compute | cached world transform / bound |

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
- Package: `UniversalSceneDescription` (7.3.4, MPL-2.0, `requireLicenseAcceptance`, managed `lib/net10.0` SWIG wrapper over per-RID native USD + Pixar plugin tree + Alembic; `osx-arm64` verified)
- Owns: the OpenUSD scene-graph read/write — `UsdStage` layer composition (sublayers/references/payloads/variants/inherits/specializes), prim/attribute authoring on an edit target, the `UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux` typed schemas, time-sampled values, and export/flatten
- Accept: a `.usd*` scene read/authored through `UsdStage` and the typed schemas, the mesh crossing the `VtVec3fArray`/`VtIntArray` seam to the kernel vocabulary, the frame normalized Y-up→Z-up through `FrameNormalization`, the shade network reconciled with the seam `AppearanceSummary`/OpenPBR, and federation/options composed on USD references/variants — coexisting with the IFC semantic graph as a scene-graph peer
- Reject: deriving BIM semantics (`BimElement`/`IfcClass`) from USD prim type names instead of the GeometryGym IFC graph; a raw attribute-name string beside the typed schema; a USD-local frame leaking past `FrameNormalization`; a USD↔glTF direct converter minted in Bim; a per-format USD reader family where one `UsdStage` discriminates by plugin; the `SWIGTYPE_p_*`/`*PINVOKE` interop types in canonical owners; a stage op with no matching native RID payload + plugin tree
