# [PY_ARTIFACTS_API_USD_CORE]

`usd-core` supplies Pixar OpenUSD stage authoring and USDZ packaging for the `scene/stage` rail: a `pxr.Usd.Stage` root defines typed `UsdGeom` prims, binds `UsdShade` materials, carries `UsdSemantics` AEC labels, and fills `Gf`/`Vt` value arrays zero-copy from numpy through `Vt.<Type>Array.FromNumpy`, then `UsdUtils` packages the authored `.usdc`/`.usda` layer with its dependency closure into a `.usdz`. Authoring from a numpy point/index buffer is the wheel-available path; the `vtkUSDExporter` render-to-layer front half needs a source-built VTK. It never re-implements the C++ runtime's layer formats, composition-arc stack, Hydra render graph, or USDZ zip record, and authoring stays on the sub-3.15 offload worker.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `usd-core`
- package: `usd-core` (`LicenseRef-TOST-1.0`, Pixar OpenUSD)
- module: `pxr`
- namespaces: `Usd`, `UsdGeom`, `UsdShade`, `UsdSemantics`, `UsdLux`, `Sdf`, `Gf`, `Vt`, `Tf`, `Kind`, `UsdUtils`
- abi: C++/Boost.Python native extension (`_<module>.so` per submodule), no cp315 wheel; `pxr` imports only inside the sub-3.15 offload worker
- rail: scene — `scene/stage`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stage, prim, and layer roots
- rail: scene — `pxr.Usd`, `pxr.Sdf`

`Usd.Stage` is the one authoring root — the source kind is a static-method choice (`CreateNew`/`CreateInMemory`/`Open`), never a parallel stage class — and `Usd.Prim` is the one node every typed schema wraps through `Define`/`Get`; layer format is the export suffix (`.usdc` crate-binary, `.usda` ASCII), never a per-format writer class. A bare `str` auto-converts to `Sdf.AssetPath` through Sdf's registered converter, so `Sdf.AssetPath(s)` is the explicit form, not an error guard.

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
- rail: scene — `pxr.UsdGeom`, `pxr.UsdShade`, `pxr.UsdSemantics`, `pxr.Gf`, `pxr.Vt`, `pxr.UsdLux`

Every `UsdGeom` schema applies to the one `Usd.Prim` through `Define(stage, path)` — a `Mesh`, `Xform`, and `Camera` are three schemas over one node, never three prim classes; `Gf` scalars and `Vt` typed homogeneous arrays carry geometry values, so a mesh's point buffer is one typed-array set, never a per-vertex object list.

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

USD authoring faults raise `Tf.ErrorException`; a wrong-arity or wrong-type call into a `pxr` C++ overload raises `Boost.Python.ArgumentError`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `Tf.ErrorException`          | runtime fault | the USD runtime error (missing asset, malformed layer, write failure) |
|  [02]   | `Boost.Python.ArgumentError` | binding fault | a wrong-arity/type call into a `pxr` C++ overload                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage authoring and layer export
- rail: scene — `pxr.Usd`, `pxr.UsdGeom`, `pxr.Sdf`

Whole-stage authoring is lazy — prims compose into the layer stack and write on `Save`/`Export`, the format following the path suffix.

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

`CreateNewUsdzPackage` embeds the dependency closure into a `.usdz`; `CreateNewARKitUsdzPackage` applies the ARKit/QuickLook policy, `ExtractUsdzPackage` inverts, and `ComputeAllDependencies`/`ModifyAssetPaths` discover and rewrite asset paths before packaging.

| [INDEX] | [SURFACE]                            | [CAPABILITY]                                                                                   |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `UsdUtils.CreateNewUsdzPackage`      | `CreateNewUsdzPackage(assetPath, usdzFilePath, ...)` -> `bool`; layer + closure into `.usdz`   |
|  [02]   | `UsdUtils.CreateNewARKitUsdzPackage` | `CreateNewARKitUsdzPackage(assetPath, usdzFilePath, ...)` -> `bool`; ARKit/QuickLook USDZ      |
|  [03]   | `UsdUtils.ExtractUsdzPackage`        | `ExtractUsdzPackage(usdzFile, extractDir, recurse, verbose, force)` -> `bool`; unpack `.usdz`  |
|  [04]   | `UsdUtils.ComputeAllDependencies`    | `ComputeAllDependencies(assetPath, ...)` -> `(layers, references, assets)` closure             |
|  [05]   | `UsdUtils.ModifyAssetPaths`          | `ModifyAssetPaths(layer, modifyFn)` -> `None`; rewrite every asset path via a callback         |
|  [06]   | `UsdUtils.ComputeUsdStageStats`      | `ComputeUsdStageStats(stage \| path)` -> `dict`; stats (`totalPrimCount`/`usedLayerCount`/...) |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- authoring: one `Usd.Stage` root, `<Schema>.Define(stage, primPath)` per typed prim, `Create<Attr>Attr(...)` per attribute over the `Gf`/`Vt` vocabulary, then `SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim` metadata, then `GetRootLayer().Save()` or `Export(path)`; the layer format is the file suffix, never a parallel per-geometry-kind writer.
- value: a geometry attribute carries one `Vt` typed array (`Vt.Vec3fArray` points/normals, `Vt.IntArray` counts/indices, `Vt.FloatArray` widths) and a transform-op one `Gf` value, so a point buffer is one `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(points))` set, never a per-vertex append. A bare `Vt.Vec3fArray(ndarray)` ctor raises `TypeError`, so `FromNumpy` is the buffer path and widens dtype (`float64 (N,3)` -> `Vec3fArray`).
- quaternion: `Vt.QuathArray.FromNumpy` reads Gf order — imaginary first, real last — so a `(w,x,y,z)` buffer rolls one slot left (`np.roll(a, -1, axis=1)`); token arrays have no `FromNumpy` and build from a Python sequence.
- material: a PBR material is `UsdShade.Material.Define` + `UsdShade.Shader.Define` (the `UsdPreviewSurface` node) wired through `ConnectableAPI`, then attached through `UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)` — the bind is distinct from the define; a per-face region is a `UsdGeom.Subset` the binding targets, so one mesh carries N material regions without N meshes.
- semantic: AEC discipline/classification labels travel on the prim through `UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(...)`, resolved across inheritance via `LabelsQuery.ComputeUniqueInheritedLabels`, never an out-of-band sidecar.
- composition: `prim.GetReferences().AddReference(Sdf.Reference(asset))` authors an eager arc and `GetPayloads().AddPayload(Sdf.Payload(asset))` its deferred counterpart; `ComputeAllDependencies` walks the layer/reference/asset closure and `ModifyAssetPaths` rewrites relocation paths before packaging.
- packaging: `CreateNewUsdzPackage` and `CreateNewARKitUsdzPackage` both embed the dependency closure and return `bool`, the ARKit arm adding QuickLook constraints.

[STACKING]:
- worker-seam: `pxr` imports after `lane.offload(Kernel.of((WORKER_MODULE, "<kernel>"), KernelTrait.HOSTILE), ...)` reaches the sub-3.15 native-VTK worker; `scene/export`'s `render_export`/`render_ingest` compose `scene/stage`'s `StageOp`/`PackageOp`, and `LanePolicy.capacity` bounds native fan-out.
- numpy (`libs/python/.api/numpy.md`): `StageOp.MeshAuthor` authors a layer from `scene/render`'s `surface_arrays` `.points`/`.regular_faces`/`.point_normals` `NDArray` buffers through `Vt.Vec3fArray.FromNumpy(points)` / `Vt.IntArray.FromNumpy(faces.reshape(-1))` / `Vt.FloatArray.FromNumpy(widths)` — zero-copy, no render pass, no Python list round-trip.
- vtk (`.api/vtk.md`, source-build-gated): `StageOp.RenderExport`'s front half is `vtkmodules.vtkIOUSD.vtkUSDExporter` (`SetRenderWindow(Plotter.render_window)`/`SetFileName`/`Write`) writing a `.usdc` that `UsdUtils.CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` packages; the official `vtk` wheel ships no `vtkIOUSD`, so the arm activates only on a source-built VTK and `MeshAuthor` is the wheel default — `vtk` owns render-to-layer, `usd-core` layer-to-package.
- content-key + receipt (`core/plan` / `core/receipt`): `Scene3d._key` mints from admitted input before work, `facts.address` derives from produced bytes, and `ComputeUsdStageStats` contributes stage evidence to the single `ArtifactReceipt.Scene` case.
- structlog (`libs/python/.api/structlog.md`): `async_boundary("scene.<op>", ...)` owns terminal fault observation and maps `Tf.ErrorException`/`Boost.Python.ArgumentError` onto `RuntimeRail`, so `pxr` bodies carry no logging path; `PackageFault` carries false packaging outcomes to `scene/export`'s `ExportError("<usd-failed>")`.
- ARKit: `scene/stage`'s `UsdzProfile.ARKIT` selects `CreateNewARKitUsdzPackage` and `UsdzProfile.STANDARD` selects `CreateNewUsdzPackage` on the same `PackageOp` owner.

[LOCAL_ADMISSION]:
- Author with `CreateInMemory()` or `CreateNew(path)`, define typed prims by `<Schema>.Define`, and write on `Save`/`Export`; never a parallel per-geometry-kind writer.
- Carry every geometry buffer as one `Vt.<Type>Array.FromNumpy` set; never a `.tolist()` copy or a per-vertex append.
- Package through `CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage`; never hand-roll USDZ zip alignment.
- Import `pxr` only inside the sub-3.15 offload worker.

[RAIL_LAW]:
- Package: `usd-core`
- Owns: USD stage authoring (`Usd.Stage`), typed geometry prims (`UsdGeom` family + `SetStageUpAxis`/`SetStageMetersPerUnit`), per-attribute authoring over the `Gf`/`Vt` vocabulary with the zero-copy `FromNumpy` bridge, primvars (`UsdGeom.PrimvarsAPI`), material/shader graphs bound through `UsdShade.MaterialBindingAPI`, semantic labels (`UsdSemantics.LabelsAPI`/`LabelsQuery`), layer serialization to `.usdc`/`.usda` (`Sdf.Layer`), and USDZ packaging/extraction with asset-dependency discovery/rewrite (`UsdUtils`)
- Accept: `StageOp.MeshAuthor` over `surface_arrays`; source-built `StageOp.RenderExport`; qualified-name process offload through `LanePolicy`; explicit `Sdf.AssetPath`; `UsdzProfile.ARKIT`; dependency closure and relocation; `ComputeUsdStageStats`; `Result[PackageFacts, PackageFault]`
- Reject: a per-vertex or `.tolist()` buffer copy where `FromNumpy` is zero-copy; a parallel per-geometry-kind writer class; hand-rolled USDZ zip alignment; a re-implemented crate/composition/Hydra runtime; an out-of-band semantic sidecar
