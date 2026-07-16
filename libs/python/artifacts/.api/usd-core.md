# [PY_ARTIFACTS_API_USD_CORE]

`usd-core` (dist `usd-core`, import `pxr`) supplies Pixar's OpenUSD stage-authoring + USDZ-packaging surface for the `scene/stage#STAGE` `UsdStage` owner: a `pxr.Usd.Stage` authoring root (`CreateNew`/`CreateInMemory`/`Open`/`DefinePrim`/`Save`/`Export`), the `pxr.UsdGeom` typed-prim schema family (`Xform`/`Mesh`/`Points`/`Camera`/`Scope`/`PointInstancer`/`Subset` + `SetStageUpAxis`/`SetStageMetersPerUnit`), the `pxr.UsdShade` material/shader graph with its `MaterialBindingAPI` binding mechanism, the `pxr.UsdSemantics` `LabelsAPI` semantic-labeling surface (the AEC-documentation telos plane: classification/discipline tags travel on the prim), the `pxr.Sdf` layer/asset-path/value-type substrate, the `pxr.Gf`/`pxr.Vt` math-value and typed-array vocabulary with the zero-copy `Vt.<Type>Array.FromNumpy(ndarray)` numpy bridge, and the `pxr.UsdUtils` packaging family (`CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage`/`ExtractUsdzPackage`/`ComputeAllDependencies`/`ModifyAssetPaths`/`ComputeUsdStageStats`). `scene/stage#STAGE`'s `StageOp.MeshAuthor` arm authors a `pxr.Usd.Stage` directly from the `scene/render#SCENE` `surface_arrays` numpy point/index buffer over `Vt.<Type>Array.FromNumpy` — the unconditional verified primary path, bypassing any render pass — then `UsdUtils.CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` packages the authored `.usdc` plus its dependency-asset closure into the `.usdz` AR/Apple deliverable. Its alternate `StageOp.RenderExport` arm writes the VTK-rendered scene to a `.usdc` layer through `vtkUSDExporter` over `Plotter.render_window`, but the official `vtk` wheel ships NO `vtkmodules.vtkIOUSD`/`vtkUSDExporter` (VTK's USD I/O requires a source build against OpenUSD — `libs/python/artifacts/.api/vtk.md`), so that front-half is the source-build-gated arm while `MeshAuthor` is the wheel-available path. It never re-implements the USD crate format (`.usdc` binary / `.usda` ASCII), the composition-arc layer stack, the Hydra render-graph, or the USDZ zip+alignment record the C++ runtime owns. Authoring/packaging only on the worker lane — the offscreen render stays at `scene/render#SCENE`/`pyvista`/`vtk`, and raw mesh-file interchange stays at `geometry/mesh`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `usd-core`
- package: `usd-core`
- import: `pxr` (dist name `usd-core`, import name `pxr`); the public names are the `pxr.{Usd,UsdGeom,UsdShade,UsdLux,UsdSkel,UsdSemantics,UsdMedia,UsdPhysics,Sdf,Sdr,Ar,Gf,Vt,Tf,Ts,Kind,Pcp,Plug,Trace,UsdUtils,...}` Boost.Python extension submodules, each `pxr/<Module>/__init__.py` re-exporting from its native `_<module>.so` — `import pxr` then `from pxr import Usd, UsdGeom, UsdUtils, Sdf, Gf, Vt`
- owner: `artifacts`
- rail: scene — `scene/stage#STAGE`
- version: `26.5` (`Usd.GetVersion()` -> `(0, 26, 5)`)
- license: `LicenseRef-TOST-1.0` — the Tomorrow Open Source Technology (modified Apache 2.0) license OpenUSD ships under; an OSS license admitting full commercial use, recorded at admission and on the consuming `scene/stage#STAGE` page
- marker: `; python_version<'3.15'` — `usd-core 26.5` declares `Requires-Python: >=3.9, <3.15` and ships no cp315 wheel, so `pxr` imports after `LanePolicy.offload` reaches the sub-3.15 native-VTK worker. Reflection is blocked on the active interpreter; this catalog uses the installed cp313 distribution's live Boost.Python surface and author/export/stats/package round-trip.
- asset: C++/Boost.Python extension runtime (`_<module>.so` per submodule, abi3 cp313 build); not pure-Python and no cp315 wheel
- capability: in-memory or on-disk USD stage authoring (`Usd.Stage.CreateNew`/`CreateInMemory`/`Open`); typed-prim definition over the `UsdGeom` schema family (`Xform`/`Mesh`/`Points`/`Camera`/`Scope`/`PointInstancer`/`Subset`/`BasisCurves`); per-prim attribute authoring (`CreatePointsAttr`/`CreateFaceVertexCountsAttr`/`CreateFaceVertexIndicesAttr`/`CreateNormalsAttr`/`CreateSubdivisionSchemeAttr`/`CreateExtentAttr`/`CreateDisplayColorPrimvar`/`CreateWidthsAttr`/`CreateIdsAttr`) over the `Vt`/`Gf` value vocabulary populated zero-copy from numpy through `Vt.<Type>Array.FromNumpy(ndarray)`; primvar authoring via `UsdGeom.PrimvarsAPI`; stage metadata (`SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim`); material/shader graphs (`UsdShade.Material`/`Shader`) bound through `UsdShade.MaterialBindingAPI`; semantic labeling (`UsdSemantics.LabelsAPI`/`LabelsQuery`); stage statistics (`UsdUtils.ComputeUsdStageStats`); layer save/export to `.usdc` (crate binary), `.usda` (ASCII), or `.usd` (`Stage.Save`/`Stage.Export`/`Sdf.Layer.Export`); USDZ packaging of an authored layer plus its dependency-asset closure (`UsdUtils.CreateNewUsdzPackage`, the ARKit/QuickLook-constrained `CreateNewARKitUsdzPackage`); USDZ extraction (`UsdUtils.ExtractUsdzPackage`); and asset-dependency discovery/rewrite (`UsdUtils.ComputeAllDependencies`/`ModifyAssetPaths`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stage, prim, and layer roots
- rail: scene — `pxr.Usd`, `pxr.Sdf`

`Usd.Stage` is the single authoring root; the source kind (a new on-disk layer, an in-memory anonymous layer, or an opened existing layer) is a static-method choice on one type, never a parallel stage class — `CreateNew` takes a file path, `CreateInMemory` allocates an anonymous root layer, and `Open` resolves an existing layer. `Usd.Prim` is the one scene-graph node every typed schema (`UsdGeom.Mesh`, `UsdGeom.Xform`,...) wraps via `Define`/`Get`; `Sdf.Layer` is the persisted substrate (`.usdc`/`.usda`) a stage composes and saves through. There is no per-format stage subtype — the layer format is the file extension `Stage.Export`/`Sdf.Layer.Export` writes (`.usdc` crate-binary, `.usda` ASCII), discriminated by suffix at export, never a parallel writer class. `Sdf.AssetPath` is the resolvable asset-path value `UsdUtils.CreateNewUsdzPackage` and `ComputeAllDependencies` take (a bare `str` is auto-converted to `Sdf.AssetPath` by Sdf's registered from-Python converter, so `Sdf.AssetPath(s)` is the idiomatic explicit form, not an error-avoidance requirement).

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [CAPABILITY]                                                                        |
| :-----: | :------------------------------ | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Usd.Stage`                     | authoring root   | the composed scene root; statics + members at [01] below                            |
|  [02]   | `Usd.Prim`                      | scene-graph node | one scene-graph node; members at [02] below                                         |
|  [03]   | `Usd.Attribute`                 | typed attribute  | typed prim property; `Set`/`Get`/`GetTypeName` — the `Vt`/`Gf` value carrier        |
|  [04]   | `Usd.TimeCode`                  | sample time      | `Default()` / a numeric frame; the `Attribute.Set` time axis                        |
|  [05]   | `Usd.ModelAPI`                  | model schema     | model kind, asset info; `Kind.Tokens.{model,component,assembly,group,subcomponent}` |
|  [06]   | `Sdf.Layer`                     | layer substrate  | the persisted `.usdc`/`.usda`; `CreateNew`/`CreateAnonymous`/`FindOrOpen`           |
|  [07]   | `Sdf.AssetPath`                 | asset reference  | resolvable asset-path (`Sdf.AssetPath(str)`); bare `str` auto-converts              |
|  [08]   | `Sdf.Path`                      | namespace path   | scene-graph path (`/Root/Mesh`); the `DefinePrim`/`<Schema>.Define` key             |
|  [09]   | `Sdf.ValueTypeNames`            | value-type table | SdfValueTypeName registry (`Point3fArray`/`Token`/...); keys `CreateAttribute`      |
|  [10]   | `Sdf.Reference` / `Sdf.Payload` | composition arc  | layer-reference / deferred-payload arc; the edges `ComputeAllDependencies` walks    |

- [01]-[USD_STAGE]: statics `CreateNew(path)`/`CreateInMemory()`/`Open(path)`; `DefinePrim`/`OverridePrim`/`GetPrimAtPath`/`GetPseudoRoot`/`GetDefaultPrim`/`Traverse`/`TraverseAll`; `SetDefaultPrim`/`SetMetadata`/`GetRootLayer`/`GetEditTarget`/`SetEditTarget`/`Flatten`; `Save`/`Export(path)`/`Load`/`Unload`.
- [02]-[USD_PRIM]: `GetAttribute`/`CreateAttribute`/`SetTypeName`/`GetPath`/`GetReferences`/`GetPayloads`/`GetVariantSets`/`GetPrimStack`/`IsValid`; the node every `UsdGeom`/`UsdShade`/`UsdSemantics` typed schema applies to.
- [03]-[USD_COMPOSITION]: `prim.GetReferences().AddReference(Sdf.Reference(asset))` authors an eager layer arc; `prim.GetPayloads().AddPayload(Sdf.Payload(asset))` authors its deferred-load counterpart.

[PUBLIC_TYPE_SCOPE]: typed geometry prims and value vocabulary
- rail: scene — `pxr.UsdGeom`, `pxr.UsdShade`, `pxr.UsdSemantics`, `pxr.Gf`, `pxr.Vt`

`UsdGeom.*` are the typed prim schemas applied to a `Usd.Prim` through one `Define(stage, path)` static per schema — a `Mesh`, an `Xform`, a `Camera` are three schemas over the one `Usd.Prim` node, never three parallel prim classes. `Gf.*` are the fixed-size math-value scalars and `Vt.*` are the typed homogeneous arrays the geometry attributes carry — `Vt.Vec3fArray` for points, `Vt.IntArray` for face indices — so a mesh's point buffer is one typed-array `Set`, never a per-vertex object list. `UsdShade.Material`/`Shader` are the material-graph nodes bound through `UsdShade.MaterialBindingAPI`. `UsdSemantics.LabelsAPI` applies taxonomy labels onto a prim (the AEC discipline/classification plane). `UsdGeom.Tokens` carries the schema token values — `y`/`z` up-axis, `none`/`catmullClark` subdivision, `perspective`/`orthographic` projection, the interpolation modes. `UsdLux.DistantLight` emits along local `-Z`, aimed by the xform-op stack.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]   | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------- | :-------------- | :---------------------------------------------------------------------------- |
|  [01]   | `UsdGeom.Xform`                      | transform prim  | `Define`; `Xformable` transform-op stack (`AddTranslateOp`/...)               |
|  [02]   | `UsdGeom.Mesh`                       | polygon mesh    | `Define`; the authored polygon-mesh surface                                   |
|  [03]   | `UsdGeom.Points`                     | point cloud     | `Define`; point cloud (`CreatePointsAttr`/`CreateWidthsAttr`/`CreateIdsAttr`) |
|  [04]   | `UsdGeom.BasisCurves`                | curve prim      | `Define`; curve geometry (`CreateCurveVertexCountsAttr`/`CreateTypeAttr`)     |
|  [05]   | `UsdGeom.Camera`                     | camera prim     | `Define`; the `GfCamera`-backed projection/focal/clipping schema              |
|  [06]   | `UsdGeom.Scope`                      | grouping prim   | `Define`; a pure namespace-grouping prim, no transform                        |
|  [07]   | `UsdGeom.PointInstancer`             | instancer prim  | `Define`; prototype + per-instance index/position/orientation arrays          |
|  [08]   | `UsdGeom.Subset`                     | face subset     | `Define`; per-face `Mesh` subset (`CreateIndicesAttr`)                        |
|  [09]   | `UsdGeom.PrimvarsAPI`                | primvar API     | `CreatePrimvar`/`GetPrimvar`/`HasPrimvar`; `displayColor`/`st` primvar        |
|  [10]   | `UsdGeom.Tokens`                     | token table     | schema token table: up-axis, subdiv scheme, projection, interpolation modes   |
|  [11]   | `UsdGeom.XformCache` / `BBoxCache`   | traversal cache | cached world-transform / world-bound over a prim subtree                      |
|  [12]   | `UsdShade.Material`                  | material node   | `Define`; `CreateSurfaceOutput`/`CreateInput` — the bound PBR material        |
|  [13]   | `UsdShade.Shader`                    | shader node     | `Define`; `CreateIdAttr`/`CreateInput`/`CreateOutput` shader graph            |
|  [14]   | `UsdShade.MaterialBindingAPI`        | binding API     | `Apply(prim)`/`Bind(material)`/`ComputeBoundMaterial` — bind                  |
|  [15]   | `UsdSemantics.LabelsAPI`             | semantic labels | `Apply(prim, taxonomy)`/`CreateLabelsAttr` — taxonomy-keyed AEC labels        |
|  [16]   | `UsdSemantics.LabelsQuery`           | semantic query  | `ComputeUnique{Direct,Inherited}Labels` — inherited-label resolve             |
|  [17]   | `Gf.Vec2f` / `Gf.Vec3f` / `Gf.Vec3d` | math vector     | fixed 2/3-vector — clipping-range, point/normal/translate value               |
|  [18]   | `Gf.Matrix4d` / `Gf.Quatf`           | math transform  | 4x4 matrix and quaternion transform values for the xform-op stack             |
|  [19]   | `Vt.{Vec3f,Int,Float,Quath}Array`    | typed array     | typed homogeneous buffers; `FromNumpy` zero-copy (no `TokenArray.FromNumpy`)  |
|  [20]   | `UsdLux.DistantLight`                | light prim      | `Define`; `CreateIntensityAttr`/`CreateColorAttr`/`CreateAngleAttr`           |

[PUBLIC_TYPE_SCOPE]: error reporting
- rail: scene — `pxr.Tf`

USD reports authoring faults through `pxr.Tf.ErrorException`, and Boost.Python reports wrong overloads through `Boost.Python.ArgumentError`; `scene/render#SCENE` maps both at `async_boundary`. False packaging returns become `PackageFault` before `scene/export#EXPORT` maps them to `<usd-failed>`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `Tf.ErrorException`          | runtime fault | the USD runtime error (missing asset, malformed layer, write failure) |
|  [02]   | `Boost.Python.ArgumentError` | binding fault | a wrong-arity/type call into a `pxr` C++ overload                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage authoring and layer export
- rail: scene — `pxr.Usd`, `pxr.UsdGeom`, `pxr.Sdf`

`Usd.Stage.CreateNew(path)` opens a new on-disk root layer; `CreateInMemory()` allocates an anonymous layer for an author-then-export-then-package flow with no intermediate file. `<Schema>.Define(stage, primPath)` (per `UsdGeom` schema) defines the typed prim, then `Create<Attr>Attr(value)` authors each attribute over a `Vt` typed array fed by `FromNumpy`. `SetStageUpAxis(stage, UsdGeom.Tokens.y)` and `SetStageMetersPerUnit(stage, 1.0)` set the stage axes; `SetDefaultPrim(prim)` names the root (a top-level prim is the valid USDZ default-prim). `Stage.GetRootLayer().Save()` (or `Stage.Export(path)`) persists to `.usdc`/`.usda` by suffix. Whole-stage authoring is lazy — prims compose into the layer stack and write on `Save`/`Export`.

| [INDEX] | [SURFACE]                                  | [CAPABILITY]                                                                         |
| :-----: | :----------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Usd.Stage.CreateNew`                      | `CreateNew(identifier, load=LoadAll)` -> `Usd.Stage`; new on-disk root-layer stage   |
|  [02]   | `Usd.Stage.CreateInMemory`                 | `CreateInMemory(identifier='')` -> `Usd.Stage`; anonymous in-memory stage            |
|  [03]   | `Usd.Stage.Open`                           | `Open(filePath, load=LoadAll)` -> `Usd.Stage`; open a `.usd`/`.usdc`/`.usda`/`.usdz` |
|  [04]   | `Usd.Stage.DefinePrim`                     | `DefinePrim(path, typeName='')` -> `Usd.Prim`; define/fetch a prim at a path         |
|  [05]   | `UsdGeom.<Schema>.Define`                  | `Mesh.Define(stage, path)` -> `UsdGeom.Mesh` (per schema); typed geometry prim       |
|  [06]   | `UsdGeom.Mesh.CreatePointsAttr`            | `CreatePointsAttr(default, writeSparsely=False)` -> `Usd.Attribute`                  |
|  [07]   | `UsdGeom.Mesh.CreateSubdivisionSchemeAttr` | `CreateSubdivisionSchemeAttr(...)`; `none` render mesh / `catmullClark` subdiv       |
|  [08]   | `UsdGeom.Mesh.CreateExtentAttr`            | `CreateExtentAttr(...)` -> `Usd.Attribute`; `[min,max]` bbox extent for AR framing   |
|  [09]   | `UsdGeom.PrimvarsAPI.CreatePrimvar`        | `CreatePrimvar(name, typeName, interpolation)`; `displayColor`/`st` primvar          |
|  [10]   | `UsdShade.MaterialBindingAPI.Apply`        | `MaterialBindingAPI.Apply(prim)` + `.Bind(material)`; bind to prim/`Subset`          |
|  [11]   | `UsdSemantics.LabelsAPI.Apply`             | `LabelsAPI.Apply(prim, taxonomy)` + `.CreateLabelsAttr`; taxonomy-keyed AEC labels   |
|  [12]   | `UsdGeom.SetStageUpAxis`                   | `SetStageUpAxis(stage, upAxis)` -> `bool`; stage up-axis (`UsdGeom.Tokens.y`/`z`)    |
|  [13]   | `UsdGeom.SetStageMetersPerUnit`            | `SetStageMetersPerUnit(stage, metersPerUnit)` -> `bool`; stage linear scale          |
|  [14]   | `Usd.Stage.SetDefaultPrim`                 | `SetDefaultPrim(prim)` -> `None`; name the default root prim (USDZ ref)              |
|  [15]   | `Usd.Stage.GetRootLayer` / `Save`          | `GetRootLayer()` -> `Sdf.Layer`; `Sdf.Layer.Save()` -> `bool`; persist edits         |
|  [16]   | `Usd.Stage.Export`                         | `Export(filename, addSourceFileComment=True)` -> `bool`; flatten + export            |
|  [17]   | `Sdf.Layer.Export`                         | `Export(filename)` -> `bool`; export one layer (format by suffix)                    |
|  [18]   | `Vt.<Type>Array.FromNumpy`                 | `Vt.<Type>Array.FromNumpy(array)`; zero-copy typed array, no `TokenArray`            |

[ENTRYPOINT_SCOPE]: USDZ packaging, asset dependencies, and stage stats
- rail: scene — `pxr.UsdUtils`

`UsdUtils.CreateNewUsdzPackage(assetPath, usdzFilePath)` is the `PackageOp.Package` standard arm over a `MeshAuthor`- or source-built `RenderExport`-written `.usdc`. It embeds the dependency closure and returns `bool`; `CreateNewARKitUsdzPackage` applies the ARKit policy. `ExtractUsdzPackage` is the inverse. `ComputeAllDependencies` discovers layers, references, and assets; `ModifyAssetPaths` rewrites them before packaging; and `ComputeUsdStageStats` supplies receipt evidence.

| [INDEX] | [SURFACE]                            | [CAPABILITY]                                                                                   |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `UsdUtils.CreateNewUsdzPackage`      | `CreateNewUsdzPackage(assetPath, usdzFilePath, ...)` -> `bool`; layer + closure into `.usdz`   |
|  [02]   | `UsdUtils.CreateNewARKitUsdzPackage` | `CreateNewARKitUsdzPackage(assetPath, usdzFilePath, ...)` -> `bool`; ARKit/QuickLook USDZ      |
|  [03]   | `UsdUtils.ExtractUsdzPackage`        | `ExtractUsdzPackage(usdzFile, extractDir, recurse, verbose, force)` -> `bool`; unpack `.usdz`  |
|  [04]   | `UsdUtils.ComputeAllDependencies`    | `ComputeAllDependencies(assetPath, ...)` -> `(layers, references, assets)` closure             |
|  [05]   | `UsdUtils.ModifyAssetPaths`          | `ModifyAssetPaths(layer, modifyFn)` -> `None`; rewrite every asset path via a callback         |
|  [06]   | `UsdUtils.ComputeUsdStageStats`      | `ComputeUsdStageStats(stage \| path)` -> `dict`; stats (`totalPrimCount`/`usedLayerCount`/...) |
|  [07]   | `UsdUtils.ComplianceChecker`         | `CheckCompliance(path)`; `GetErrors()`/`GetFailedChecks()`                                     |

## [04]-[IMPLEMENTATION_LAW]

[SCENE_USDZ_PACKAGING]:
- packaging axis: `scene/stage#STAGE`'s `PackageOp.Package(source, sink, profile)` selects `CreateNewUsdzPackage` or `CreateNewARKitUsdzPackage`; both embed the dependency closure and return success on `Result[PackageFacts, PackageFault]` — the facts and fault owners declared beside `PackageOp` on that page. `ComputeAllDependencies` and `ModifyAssetPaths` serve `PackageOp.Relocate`, not package assembly.
- authoring axis: a USD scene authored from scratch (the `scene/stage#STAGE` `StageOp.MeshAuthor` arm, rather than `StageOp.RenderExport` from VTK) is `Usd.Stage.CreateInMemory()` (or `CreateNew(path)`), then `UsdGeom.<Schema>.Define(stage, primPath)` per typed prim, then `Create<Attr>Attr(Vt.<Type>Array.FromNumpy(buf))` per attribute over the `Gf`/`Vt` value vocabulary, then `CreateSubdivisionSchemeAttr(UsdGeom.Tokens.none)`/`CreateExtentAttr` mesh metadata, then `SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim(mesh.GetPrim())` stage metadata, then `GetRootLayer().Save()` or `Export(path)` — one stage root, typed prims defined by schema, never a parallel per-geometry-kind writer. Layer format (`.usdc` crate-binary / `.usda` ASCII) is the file suffix, discriminated by extension at export.
- value axis: a geometry attribute carries a `Vt` typed array directly (`Vt.Vec3fArray` for points/normals, `Vt.IntArray` for face counts/indices, `Vt.FloatArray` for widths), and a transform-op a `Gf` value (`Gf.Vec3f`/`Gf.Matrix4d`/`Gf.Quatf`) — there is no per-vertex or per-element wrapper object, so a mesh's whole point buffer is one `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(points))` `Set` over the zero-copy numpy bridge, never a `.tolist()` Python-list copy nor a vertex-by-vertex append. A bare `Vt.Vec3fArray(ndarray)` ctor raises `TypeError` (no registered converter), so `FromNumpy` is the buffer path; `FromNumpy` widens dtype (float64 `(N,3)` -> `Vec3fArray`). `Vt.QuathArray.FromNumpy` admits a float16/float32 `(N, 4)` buffer and reads each row in Gf quaternion memory order — imaginary first, real last — so a `(w, x, y, z)` orientation buffer rolls one slot left (`np.roll(a, -1, axis=1)`) before the call. Interpolation/string tokens (which have no `TokenArray.FromNumpy`) construct from a Python sequence.
- material axis: a PBR material is `UsdShade.Material.Define` + `UsdShade.Shader.Define` (the `UsdPreviewSurface` node via `CreateIdAttr`/`CreateInput`/`CreateOutput`) wired through `ConnectableAPI`, then ATTACHED through `UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)` — the binding API is the actual attach, distinct from defining the material; a per-material face region is a `UsdGeom.Subset` the binding targets, so one mesh carries N material regions without N meshes.
- semantic axis: AEC discipline / classification / sheet-type labels travel on the prim through `UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(...)`, resolved across inheritance via `LabelsQuery.ComputeUniqueInheritedLabels` — the documentation-telos label plane the IFC/classification seam writes onto the exported geometry, never an out-of-band sidecar.
- dependency axis: `ComputeAllDependencies(Sdf.AssetPath(layer))` returns the layer/reference/asset closure, and `ModifyAssetPaths(layer, fn)` rewrites relocation paths before packaging. `AssetArc.Reference` and `AssetArc.Payload` author eager and deferred composition edges.
- stats axis: `ComputeUsdStageStats(stage)` supplies prim and layer counts; `StageOp.apply` adds up-axis, scale, extent diagonal, and resolved labels; and the export worker merges them with staged point/cell counts on `ArtifactReceipt.Scene.facts`.
- fault axis: `Tf.ErrorException` and `Boost.Python.ArgumentError` map at `async_boundary`; false package/extract returns and compliance issues become closed `scene/stage#STAGE` `PackageFault` cases before `scene/export#EXPORT` maps them to its `ExportError("<usd-failed>")`.
- evidence: `ArtifactReceipt.Scene` retains the pre-run input key, target, byte count, and one `facts` band; `facts.address` carries the payload content address, while USD stage statistics and package compliance merge into that same band.

[STACKING]:
- worker-seam stack: `pxr` imports after `lane.offload(dispatched, "<kernel>", ..., modality=Modality.PROCESS)` reaches the sub-3.15 native-VTK worker. `scene/export#EXPORT`'s `render_export`/`render_ingest` compose `scene/stage#STAGE`'s `StageOp`/`PackageOp`; the runtime `LanePolicy.capacity` bounds native fan-out. `PackageFault` carries false packaging or compliance outcomes before `scene/export#EXPORT` maps them to `ExportError`, and `async_boundary` maps named native exceptions onto `RuntimeRail`.
- numpy buffer stack (`libs/python/.api/numpy.md`): the `StageOp.MeshAuthor` arm authors a USD layer from the `scene/render#SCENE` `surface_arrays` worker's `.points`/`.regular_faces`/`.point_normals` `NDArray` buffers directly — `Vt.Vec3fArray.FromNumpy(points)` / `Vt.IntArray.FromNumpy(faces.reshape(-1))` / `Vt.FloatArray.FromNumpy(widths)` is the zero-copy bridge from the numpy substrate into the `Vt` typed arrays, so a pure-geometry USD export does not require a full render pass and never round-trips through a Python list. That same `Export` arm reaches this path when the target is `USD` and the dataset surface is already extracted.
- vtk front-half stack (source-build-gated): the alternate `StageOp.RenderExport` arm's front half is `vtk` `vtkmodules.vtkIOUSD.vtkUSDExporter` (`SetRenderWindow(Plotter.render_window)`/`SetFileName`/`Write`) writing the rendered scene to a `.usdc` layer, which `UsdUtils.CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` then packages — BUT the official `vtk` wheel ships no `vtkmodules.vtkIOUSD` (USD I/O requires a VTK source build against OpenUSD; `libs/python/artifacts/.api/vtk.md`), so this arm activates only on a source-built VTK and `usd-core`'s `MeshAuthor` numpy path is the wheel-available default. When present, the `vtk` row owns render-to-layer and the `usd-core` row owns layer-to-package — a USDZ with zero hand-rolled zip alignment.
- content-key + receipt stack (`core/plan#PLAN` / `core/receipt#RECEIPT`): `scene/render#SCENE`'s `Scene3d._key` mints from admitted input before work, `facts.address` derives from produced bytes, and `usd-core` contributes stage evidence to the single `ArtifactReceipt.Scene` case.
- structlog evidence stack (`libs/python/.api/structlog.md`): `async_boundary("scene.<op>", ...)` owns terminal fault observation, so `pxr` bodies carry no duplicate logging path.
- ARKit policy stack: `scene/stage#STAGE`'s `UsdzProfile.ARKIT` selects `CreateNewARKitUsdzPackage` and ARKit compliance; `UsdzProfile.STANDARD` selects `CreateNewUsdzPackage` on the same `PackageOp` owner.

[RAIL_LAW]:
- Package: `usd-core`
- Owns: USD stage authoring (`Usd.Stage` create/open/define/save/export), typed geometry prims (`UsdGeom.Xform`/`Mesh`/`Points`/`Camera`/`Scope`/`BasisCurves`/`Subset`/`PointInstancer` + `SetStageUpAxis`/`SetStageMetersPerUnit`), per-attribute authoring over the `Gf`/`Vt` math-value and typed-array vocabulary with the zero-copy `FromNumpy` numpy bridge, primvars (`UsdGeom.PrimvarsAPI`), material/shader graphs bound through `UsdShade.MaterialBindingAPI`, semantic labels (`UsdSemantics.LabelsAPI`/`LabelsQuery`), stage statistics (`UsdUtils.ComputeUsdStageStats`), layer serialization to `.usdc`/`.usda` (`Sdf.Layer`), USDZ packaging/extraction and asset-dependency discovery/rewrite (`UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage`/`ExtractUsdzPackage`/`ComputeAllDependencies`/`ModifyAssetPaths`)
- Accept: `StageOp.MeshAuthor` over `surface_arrays`; source-built `StageOp.RenderExport`; qualified-name process dispatch through `LanePolicy`; explicit `Sdf.AssetPath`; `UsdzProfile.ARKIT`; dependency closure and relocation; `ComputeUsdStageStats`; `UsdSemantics.LabelsAPI`; `Result[PackageFacts, PackageFault]`
