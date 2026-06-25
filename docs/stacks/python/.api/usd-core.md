# [PY_ARTIFACTS_API_USD_CORE]

`usd-core` is the binary redistribution of Pixar/Apple OpenUSD (`import pxr`) for the artifacts scene-packaging rail: it owns the composed `Usd.Stage` scene graph (layers, references, payloads, variants, instancing, value clips, edit targets), the `Sdf` flat layer/spec data model with its native `.usdz` `ZipFile`/`ZipFileWriter` reader-writer, the `UsdGeom` typed geometry schemas (`Mesh`/`Points`/`PointInstancer`/`Camera`/`Xformable`), the `Gf`/`Vt` linear-algebra and typed-array value tier (with zero-copy `numpy` buffer interop), and the `UsdUtils` packaging surface (`CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage`/`ExtractUsdzPackage`/`ComplianceChecker`/`ComputeAllDependencies`/`FlattenLayerStack`). The artifacts owner composes a `Stage` from typed schema prims, writes `numpy` mesh buffers as `Vt` arrays through `UsdGeom` attributes, and serializes to a crate `.usdc`/`.usda` layer or a single-archive `.usdz`/ARKit package — it never hand-rolls a USD crate codec, a zip-alignment writer, or the composition engine the wheel already owns. It is the host-neutral USD owner beside the `vtk` USD exporter (which writes from a VTK pipeline); this catalog owns full-fidelity authored scene description, `vtk` owns render-mesh extraction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `usd-core`
- package: `usd-core`
- import: `import pxr` then `from pxr import Usd, UsdGeom, Sdf, Gf, Vt, UsdUtils, ...`; the bundled `pxr/pluginfo` plugin registry is discovered at import, so schemas, file formats, and resolvers register without any `PXR_PLUGINPATH_NAME` setup
- owner: `artifacts`
- rail: scene-packaging
- version: `26.5` (OpenUSD `Usd.GetVersion() == (0, 26, 5)`); floor `usd-core; python_version<'3.15'`
- license: `LicenseRef-TOST-1.0` — the Tomorrow Open Source Technology License (a modified Apache-2.0 with an attribution/anti-trademark clause); the 49 KB `LICENSE.txt` ships in `dist-info/licenses/`. NOT plain Apache-2.0; redistribution carries the TOST attribution obligation.
- abi: per-interpreter binary wheels `cp39`..`cp314` for `macosx_10_15_universal2` / `manylinux_2_28_x86_64` / `win_amd64`; the C++ core is `delocate`/`auditwheel`-vendored inside the wheel (`Root-Is-Purelib: true` despite native `.so`/`.dylib` payload). NO `none-any` and NO `abi3` wheel — every CPython minor needs its own build.
- marker: COMPANION-GATED. The package metadata pins `Requires-Python: >=3.9, <3.15` (a HARD upper bound baked into the wheel, not merely the manifest), and no `cp315` wheel is published. On the active 3.15 interpreter `usd-core` does not install and `assay api resolve usd-core` cannot reflect it; this catalog is verified against the real `cp314` wheel (OpenUSD 26.5) installed on a sibling 3.14 interpreter, the authoritative surface until OpenUSD ships a 3.15 build and the manifest marker is removed.
- depends-on: none (self-contained; `Gf`/`Vt` provide the math/array tier, so `numpy` is optional and used only at the buffer-protocol boundary)
- entry points: none (library only; `usdcat`/`usdzip`/`usdchecker` CLIs are NOT in the `usd-core` wheel — their behavior is exposed programmatically through `Sdf.Layer.Export`, `UsdUtils.CreateNewUsdzPackage`, and `UsdUtils.ComplianceChecker`)
- namespaces (29 `pxr` submodules): foundation `Tf`, `Gf`, `Vt`, `Work`, `Trace`, `Plug`, `Ar`, `Kind`; scene-description core `Sdf`, `Pcp`, `Usd`; typed schemas `UsdGeom`, `UsdShade`, `UsdSkel`, `UsdLux`, `UsdPhysics`, `UsdVol`, `UsdMedia`, `UsdProc`, `UsdRender`, `UsdRi`, `UsdUI`, `UsdHydra`, `UsdShaders`, `UsdSemantics`; shader metadata `Sdr`; animation splines `Ts`; tooling `UsdUtils`, `UsdValidation`
- capability: in-memory or file-backed `Stage` authoring; composition arcs (sublayers, references/payloads, variant sets, inherits/specializes, instancing, value clips) under scoped `EditTarget`s; typed geometry/shading/skinning/lighting/physics schemas; `Gf` linear algebra and `Vt` typed arrays with `numpy` round-trip; crate `.usdc` / ASCII `.usda` serialization; single-archive `.usdz`/ARKit packaging with the native zip writer; dependency discovery, layer flattening, asset localization, USDZ compliance checking, and clip stitching

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: composed scene graph (`pxr.Usd`)
- rail: scene-packaging
- The runtime composed view. A `Stage` is the lazily-composed result of the `Sdf` layer stack; `Prim`/`Attribute`/`Relationship` are composed handles (not authored storage). Authoring goes through the active `EditTarget`; composition arcs are edited through the arc list-editor handles, never by string-poking layer text.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]   | [RAIL]                                                                         |
| :-----: | :------------------------------------------ | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `Usd.Stage`                                 | scene root      | composed stage; `CreateNew`/`CreateInMemory`/`Open`/`Export`/`Flatten`/`Save`   |
|  [02]   | `Usd.Prim`                                  | composed prim   | typed node; `CreateAttribute`/`ApplyAPI`/`GetReferences`/`GetVariantSets`        |
|  [03]   | `Usd.Attribute`                             | composed attr   | typed value slot; `Set`/`Get(time)`/`AddConnection`/`GetResolveInfo`             |
|  [04]   | `Usd.Relationship`                          | composed rel    | typed target list; `AddTarget`/`SetTargets`/`GetForwardedTargets`               |
|  [05]   | `Usd.Property` / `Usd.Object`               | composed base   | shared metadata/customData/assetInfo surface for attributes/relationships/prims |
|  [06]   | `Usd.EditTarget`                            | authoring sink  | which layer + variant-context authored edits land in; `ForLocalDirectVariant`   |
|  [07]   | `Usd.References` / `Usd.Payloads`           | arc editor      | `Add{,Internal}Reference`/`AddPayload`/`SetReferences`/`ClearReferences`         |
|  [08]   | `Usd.Inherits` / `Usd.Specializes`          | arc editor      | class-inheritance and fallback-specialization arc list editors                  |
|  [09]   | `Usd.VariantSet` / `Usd.VariantSets`        | variant editor  | `AddVariant`/`SetVariantSelection`/`GetVariantEditContext` (author-into-variant) |
|  [10]   | `Usd.PrimRange`                             | traversal       | depth-first prim iterator; `PreAndPostVisit` / `AllPrims` pruning modes          |
|  [11]   | `Usd.TimeCode`                              | time key        | `Default()` vs numeric sample time; `EarliestTime`/`PreTime` for clip blending   |
|  [12]   | `Usd.StageCache` / `Usd.StageCacheContext`  | stage pool      | process-wide stage de-dup by `Id`; share one composed stage across call sites    |
|  [13]   | `Usd.SchemaRegistry`                        | schema metadata | concrete/applied/API schema definitions, fallback prim types, schema versioning  |
|  [14]   | `Usd.ModelAPI` / `Usd.ClipsAPI`             | applied API     | model-kind/asset-info authoring; value-clip asset-path/active/template authoring |
|  [15]   | `Usd.CollectionAPI`                         | applied API     | include/exclude membership + `PathExpression` rules; `ComputeIncludedPaths`      |
|  [16]   | `Usd.Notice`                                | change notice   | `ObjectsChanged`/`StageContentsChanged`/`StageEditTargetChanged` Tf notices      |

[PUBLIC_TYPE_SCOPE]: flat layer + spec data model and native zip (`pxr.Sdf`)
- rail: scene-packaging
- The authored storage tier beneath composition. A `Sdf.Layer` is one `.usd{a,c,z}` file (or anonymous in-memory layer) holding a tree of `PrimSpec`/`PropertySpec`. Crucially, the `.usdz` archive reader-writer (`ZipFile`/`ZipFileWriter`) lives HERE in `Sdf`, NOT in `Usd` and NOT in `UsdUtils`.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]   | [RAIL]                                                                       |
| :-----: | :------------------------------------------------ | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Sdf.Layer`                                        | authored layer  | `CreateNew`/`CreateAnonymous`/`FindOrOpen`/`Export`/`ExportToString`/`Save`   |
|  [02]   | `Sdf.PrimSpec` / `Sdf.PropertySpec`                | authored spec   | direct opinion storage in one layer; specifier/typeName/variant authoring     |
|  [03]   | `Sdf.AttributeSpec` / `Sdf.RelationshipSpec`       | authored spec   | typed value/connection storage independent of composition                     |
|  [04]   | `Sdf.Path`                                         | scene path      | hashable namespace path (`/World/Mesh.points`); property/target/variant paths  |
|  [05]   | `Sdf.PathExpression` / `Sdf.PathPattern`           | path matcher    | predicate path-expression language backing `CollectionAPI` membership          |
|  [06]   | `Sdf.Reference` / `Sdf.Payload`                    | arc value       | immutable arc descriptors (asset path + prim path + layer offset)             |
|  [07]   | `Sdf.{Reference,Payload,Path,Token,String}ListOp`  | list editor     | the add/prepend/append/delete/explicit list-op algebra all arcs compose from   |
|  [08]   | `Sdf.ValueTypeNames`                               | type registry   | the canonical attribute type table (`Point3fArray`→`point3f[]`, `Asset`, ...)  |
|  [09]   | `Sdf.AssetPath` / `Sdf.AssetPathArray`             | asset ref       | unresolved+resolved asset path pair; the `Ar`-resolvable reference value       |
|  [10]   | `Sdf.ZipFile`                                       | usdz reader     | `Open`/`GetFile`/`GetFileInfo`/`GetFileNames`/`DumpContents` over a `.usdz`     |
|  [11]   | `Sdf.ZipFileWriter`                                 | usdz writer     | `CreateNew`/`AddFile`/`Save`/`Discard` — the alignment-correct usdz archiver    |
|  [12]   | `Sdf.FileFormat` / `Sdf.UsdFileFormat`             | format plugin   | the pluggable layer codec registry (`.usda`/`.usdc`/`.usdz` + custom formats)  |
|  [13]   | `Sdf.ChangeBlock` / `Sdf.BatchNamespaceEdit`       | batch edit      | coalesce many spec edits into one change-notification round (authoring speed)  |
|  [14]   | `Sdf.VariableExpression`                           | layer var       | `${VAR}` expression evaluation over layer expression-variables                 |

[PUBLIC_TYPE_SCOPE]: typed geometry schemas (`pxr.UsdGeom`)
- rail: scene-packaging
- The geometry/imageable IsA-schema family. Each is a typed wrapper applied over a `Usd.Prim` via `.Define(stage, path)`; attribute authoring is the `Create<Name>Attr(value)` family, never raw `prim.CreateAttribute`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]   | [RAIL]                                                                  |
| :-----: | :------------------------------------------------------- | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `UsdGeom.Mesh`                                            | gprim           | polygon/subdiv mesh; points/faceVertexCounts/faceVertexIndices/normals  |
|  [02]   | `UsdGeom.Points` / `UsdGeom.PointInstancer`              | gprim           | point cloud / instanced-prototype scatter (`protoIndices`/`positions`)  |
|  [03]   | `UsdGeom.BasisCurves` / `UsdGeom.NurbsCurves`            | gprim           | curve geometry (hair/strands, NURBS)                                    |
|  [04]   | `UsdGeom.Cube`/`Sphere`/`Cylinder`/`Cone`/`Capsule`/`Plane` | gprim        | analytic primitive shapes                                               |
|  [05]   | `UsdGeom.Subset`                                          | face subset     | named face-index subset of a `Mesh` (per-material assignment)           |
|  [06]   | `UsdGeom.Xform` / `UsdGeom.Xformable`                    | transformable   | xformOpOrder stack; `AddTranslateOp`/`AddRotateXYZOp`/`AddTransformOp`   |
|  [07]   | `UsdGeom.XformCommonAPI`                                  | applied API     | TRS shortcut over the xformOp stack; `SetTranslate`/`SetRotate`/`SetScale` |
|  [08]   | `UsdGeom.XformCache` / `UsdGeom.BBoxCache`               | compute cache   | cached world-xform / world-bound queries across a traversal             |
|  [09]   | `UsdGeom.Camera`                                          | gprim           | USD camera; `GetCamera(time)` → a `Gf.Camera` frustum                   |
|  [10]   | `UsdGeom.Primvar` / `UsdGeom.PrimvarsAPI`               | primvar         | interpolated per-vertex/face/point user data (UVs, colors, weights)     |
|  [11]   | `UsdGeom.Imageable` / `UsdGeom.Gprim` / `UsdGeom.Scope` | base/grouping   | visibility/purpose base, renderable base, and pure namespace grouping   |

[PUBLIC_TYPE_SCOPE]: shading, skinning, lighting, physics schemas
- rail: scene-packaging
- The non-geometry authored-asset families. `UsdShade` is the material/shader network; `UsdSkel` skeletal skinning; `UsdLux` lights; `UsdPhysics` rigid-body/joint description.

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]  | [RAIL]                                                               |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `UsdShade.Material` / `UsdShade.Shader`              | shading        | material with surface/displacement/volume outputs; shader node        |
|  [02]   | `UsdShade.NodeGraph` / `UsdShade.ConnectableAPI`     | shading        | encapsulated subgraph; the input/output connection algebra            |
|  [03]   | `UsdShade.Input` / `UsdShade.Output`                 | shading port   | typed connectable port with `ConnectToSource`/`GetValueProducingAttrs` |
|  [04]   | `UsdShade.MaterialBindingAPI`                        | applied API    | bind a `Material` to geometry (direct + collection-based binding)     |
|  [05]   | `UsdSkel.Skeleton` / `UsdSkel.Animation`            | skinning       | joint hierarchy + sampled joint transforms                            |
|  [06]   | `UsdSkel.BindingAPI` / `UsdSkel.BlendShape`         | applied API    | bind geometry to a skeleton; blendshape targets                       |
|  [07]   | `UsdSkel.Cache` / `UsdSkel.SkeletonQuery` / `UsdSkel.SkinningQuery` | query | resolved skinning evaluation handles                       |
|  [08]   | `UsdLux.{Distant,Sphere,Rect,Disk,Cylinder,Dome}Light` | lighting    | analytic + IBL light prims                                            |
|  [09]   | `UsdLux.LightAPI` / `UsdLux.ShadowAPI` / `UsdLux.ShapingAPI` | applied API | light intensity/color/shadow/cone authoring on any prim         |
|  [10]   | `UsdPhysics.RigidBodyAPI` / `UsdPhysics.CollisionAPI` | applied API   | rigid-body dynamics + collision geometry flags                       |
|  [11]   | `UsdPhysics.{Fixed,Revolute,Prismatic,Spherical,Distance}Joint` | physics | articulation joint schemas                                  |
|  [12]   | `UsdPhysics.MassAPI` / `UsdPhysics.MaterialAPI`     | applied API    | mass/density and friction/restitution authoring                      |

[PUBLIC_TYPE_SCOPE]: linear-algebra and typed-array value tier (`pxr.Gf`, `pxr.Vt`)
- rail: scene-packaging
- The value tier every attribute is typed against. `Gf` is the scalar/vector/matrix math; `Vt` is the C++-backed typed array with the buffer protocol. A `numpy` array crosses into USD as a `Vt` array and back as a zero-copy `memoryview`.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]  | [RAIL]                                                                 |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `Gf.Vec3f` / `Vec3d` / `Vec2f` / `Vec4f` (+ `h`/`i`)   | vector         | fixed-width float/double/half/int vectors                              |
|  [02]   | `Gf.Matrix4d` / `Matrix3d` / `Matrix4f`               | matrix         | row-major transform matrices; `SetRotate`/`SetLookAt`/`ExtractRotation` |
|  [03]   | `Gf.Quatf` / `Quatd` / `Quath` / `Gf.Rotation`        | rotation       | quaternion + axis-angle rotation; `Gf.Slerp` interpolation             |
|  [04]   | `Gf.Range3d` / `Gf.BBox3d` / `Gf.Frustum`             | bounds         | axis-aligned range, matrix-carrying bbox, camera frustum               |
|  [05]   | `Gf.Camera` / `Gf.Transform`                          | camera/xform   | film-back camera model; decomposed TRS transform                       |
|  [06]   | `Gf.ColorSpace` / `Gf.Color`                          | color          | OCIO-aligned color-space conversion (`ConvertLinearToDisplay`)         |
|  [07]   | `Vt.Vec3fArray` / `Vt.Vec2fArray` / `Vt.IntArray`     | typed array    | the attribute array storage; `.FromNumpy(ndarray)` / `np.array(arr)`    |
|  [08]   | `Vt.FloatArray` / `Vt.DoubleArray` / `Vt.TokenArray`  | typed array    | scalar/string typed arrays; all support the Python buffer protocol     |
|  [09]   | `Vt.Matrix4dArray` / `Vt.QuatfArray`                  | typed array    | matrix/quaternion arrays (skinning transforms, instancer orientations)  |

[PUBLIC_TYPE_SCOPE]: asset resolution, plugins, splines, diagnostics
- rail: scene-packaging
- The foundation infrastructure. `Ar` resolves asset paths (the seam where `.usdz`-relative and external references resolve); `Tf` is the error/notice/type foundation; `Ts` is the value-spline tier; `Plug` is the plugin registry.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [RAIL]                                                                |
| :-----: | :------------------------------------------------ | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `Ar.GetResolver()` / `Ar.Resolver`                | asset resolver | the active path resolver; `Resolve`/`CreateIdentifier`/anchoring       |
|  [02]   | `Ar.ResolvedPath` / `Ar.AssetInfo`                | resolved asset | resolved filesystem/package path + asset metadata                     |
|  [03]   | `Ar.ResolverContext` / `Ar.ResolverContextBinder` | resolver scope | bind search paths/context for a resolution scope                      |
|  [04]   | `Ar.DefaultResolver` / `Ar.GetRegisteredURISchemes` | resolver     | filesystem resolver + registered URI scheme handlers                  |
|  [05]   | `Tf.ErrorException` / `Tf.Error` / `Tf.Diagnostic` | diagnostics    | USD's Python binding RAISES `Tf.ErrorException`; `Tf.Diagnostic` is the non-fatal stream |
|  [06]   | `Tf.Notice` / `Tf.Type` / `Tf.Token`              | foundation     | observer notices, the C++ type system, and interned string tokens     |
|  [07]   | `Ts.Spline` / `Ts.Knot` / `Ts.KnotMap`            | value spline   | Bezier/Hermite/linear animation splines on attribute values (26.x)    |
|  [08]   | `Plug.Registry` / `Plug.Plugin`                   | plugin registry | the schema/format/resolver plugin discovery surface (`pluginfo` dirs) |
|  [09]   | `Kind.Registry` / `Kind.Tokens`                   | model kind     | the `component`/`group`/`assembly` model-hierarchy kind taxonomy      |
|  [10]   | `Sdr.Registry` / `Sdr.ShaderNode`                 | shader defs    | shader-node definition registry feeding `UsdShade` material authoring  |
|  [11]   | `UsdValidation.ValidationRegistry` / `ValidationContext` / `Validator` | validation | the modern validation framework superseding `UsdUtils.ComplianceChecker`; `Validate(stage)` + `ValidationFixer` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stage lifecycle and serialization
- rail: scene-packaging
- A stage is created in memory or on disk, authored under an edit target, and serialized to a layer or a usdz package. `Export` writes a NEW file (format inferred from extension); `Save` flushes dirty layers in place; `Flatten` collapses the composed result into one anonymous layer.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [RAIL]                                                            |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `Usd.Stage.CreateNew(path)` / `Usd.Stage.CreateInMemory()`            | stage build     | new file-backed / anonymous stage                                 |
|  [02]   | `Usd.Stage.Open(path_or_layer, load=Usd.Stage.LoadAll)`              | stage open      | open existing layer/usdz with a payload load rule                 |
|  [03]   | `Usd.Stage.Export(path)` / `Usd.Stage.ExportToString()`             | serialize       | write composed-root layer to `.usda`/`.usdc`/`.usdz` or ASCII str  |
|  [04]   | `Usd.Stage.Save()` / `Usd.Stage.Flatten()`                          | persist/collapse | flush dirty layers / return one flattened anonymous `Sdf.Layer`   |
|  [05]   | `stage.DefinePrim(path, typeName)` / `stage.OverridePrim(path)`     | prim author     | create a defined typed prim or a sparse override prim             |
|  [06]   | `stage.SetDefaultPrim(prim)` / `stage.GetDefaultPrim()`            | root prim       | the package default prim (REQUIRED for a valid referenceable usdz) |
|  [07]   | `stage.GetEditTarget()` / `stage.SetEditTarget(target)`            | authoring sink  | route subsequent edits to a layer/variant                        |
|  [08]   | `stage.Traverse()` / `Usd.PrimRange.PreAndPostVisit(prim)`         | traverse        | depth-first composed-prim iteration                              |
|  [09]   | `UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.y)` / `SetStageMetersPerUnit` | stage meta | the up-axis + linear-unit metadata every consumer reads           |

[ENTRYPOINT_SCOPE]: typed schema authoring
- rail: scene-packaging
- Every schema is applied through `.Define(stage, path)` (IsA/typed) or `.Apply(prim)` (API/applied); attributes through the `Create<Name>Attr(value)` family that returns the `Usd.Attribute`.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                                                           |
| :-----: | :----------------------------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `UsdGeom.Mesh.Define(stage, path)`                                 | schema define  | create a typed Mesh prim                                        |
|  [02]   | `mesh.CreatePointsAttr(Vt.Vec3fArray)` / `CreateFaceVertexIndicesAttr` | attr author | author geometry buffers as typed Vt arrays                     |
|  [03]   | `mesh.CreateNormalsAttr` / `CreateExtentAttr` / `CreateSubdivisionSchemeAttr` | attr author | normals, bounds, subdivision policy             |
|  [04]   | `UsdGeom.XformCommonAPI(prim).SetTranslate/SetRotate/SetScale`     | xform author   | TRS without manual xformOp stack management                    |
|  [05]   | `UsdShade.Material.Define(stage, path)` / `material.CreateSurfaceOutput()` | material  | material + surface/displacement/volume outputs                 |
|  [06]   | `UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)`           | material bind  | bind material to geometry                                      |
|  [07]   | `prim.GetReferences().AddReference(asset, primPath)` / `AddPayload` | composition   | author a reference/payload arc                                 |
|  [08]   | `prim.GetVariantSets().AddVariantSet(name)` + `vset.GetVariantEditContext()` | variant | author opinions into a named variant                          |
|  [09]   | `Usd.ModelAPI(prim).SetKind(Kind.Tokens.component)`               | model kind     | tag the asset's model-hierarchy kind                          |

[ENTRYPOINT_SCOPE]: USDZ packaging, dependencies, compliance (`pxr.UsdUtils`)
- rail: scene-packaging
- The packaging tooling. `CreateNewUsdzPackage` walks a stage's full dependency closure and archives it; `CreateNewARKitUsdzPackage` additionally enforces the ARKit profile (single binary layer, restricted schemas); `ComplianceChecker` runs the `usdchecker` rule set programmatically.

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]  | [RAIL]                                                       |
| :-----: | :---------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `UsdUtils.CreateNewUsdzPackage(assetPath, usdzPath)`                                 | package         | archive a stage + all dependencies into one `.usdz`          |
|  [02]   | `UsdUtils.CreateNewARKitUsdzPackage(assetPath, usdzPath)`                            | package         | ARKit-profile usdz (flattened binary root, schema-restricted) |
|  [03]   | `UsdUtils.ExtractUsdzPackage(usdzPath, destDir)` / `UsdUtils.UsdzAssetIterator`     | unpack/iterate  | expand a usdz / iterate `.AllAssets` and `.UsdAssets`        |
|  [04]   | `UsdUtils.ComputeAllDependencies(layerPath)` → `(layers, assets, unresolvedPaths)`  | dependency scan | the full external-reference + asset closure of a layer       |
|  [05]   | `UsdUtils.ExtractExternalReferences(layerPath)`                                      | dependency scan | sublayer / reference / payload asset-path extraction         |
|  [06]   | `UsdUtils.LocalizeAsset(assetPath, destDir)` / `UsdUtils.ModifyAssetPaths(layer, fn)` | localize      | copy+rewrite an asset's dependencies into a portable bundle  |
|  [07]   | `UsdUtils.FlattenLayerStack(stage)` → `Sdf.Layer`                                   | flatten         | collapse the layer stack to one layer (composition-preserving) |
|  [08]   | `UsdValidation.ValidationContext(...).Validate(stage)` → `[ValidationError]`        | validate        | the modern validation framework; `ValidationRegistry.GetOrLoadAllValidators()` then `Validate`, with `ValidationFixer` auto-fixes |
|  [09]   | `UsdUtils.ComplianceChecker(arkit=True).CheckCompliance(stage)` + `.GetFailedChecks()` | validate (deprecated) | the legacy usdz/ARKit rule runner — DEPRECATED in 26.5, migrate to `UsdValidation`; still functional as the usdz publish gate until removed |
|  [10]   | `UsdUtils.ComputeUsdStageStats(stagePath)`                                          | stats           | prim/instance/asset counts for a stage receipt               |
|  [11]   | `UsdUtils.StitchClips(resultLayer, clipLayers, clipPath)` / `StitchClipsTemplate`  | value clips     | author a value-clip set stitching per-frame layers            |

[ENTRYPOINT_SCOPE]: native usdz zip archive (`pxr.Sdf`)
- rail: scene-packaging
- The byte-level archive primitive under packaging. `ZipFileWriter` writes the 64-byte-aligned, uncompressed-`.usdc` layout USDZ mandates (so a runtime can mmap the crate in place); use it directly only for custom archive composition — `UsdUtils.CreateNewUsdzPackage` is the dependency-aware front door.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                                            |
| :-----: | :--------------------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Sdf.ZipFileWriter.CreateNew(usdzPath)`                          | usdz write     | open a writer (use as a context manager; `Save` on clean exit)    |
|  [02]   | `writer.AddFile(filePath, archiveName='')`                       | usdz write     | add a layer/texture, alignment-correct, returns the stored name   |
|  [03]   | `writer.Save()` / `writer.Discard()`                            | usdz finalize  | commit the central directory / abandon the partial archive       |
|  [04]   | `Sdf.ZipFile.Open(usdzPath)`                                    | usdz read      | open an archive for reading                                      |
|  [05]   | `zf.GetFileNames()` / `zf.GetFile(name)` / `zf.GetFileInfo(name)` | usdz read     | enumerate, read bytes, or stat a member (`FileInfo`: `dataOffset`/`size`/`uncompressedSize`/`crc`/`compressionMethod`/`encrypted`) |

[ENTRYPOINT_SCOPE]: layer-level data model and asset resolution (`pxr.Sdf`, `pxr.Ar`)
- rail: scene-packaging

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `Sdf.Layer.CreateNew(path)` / `Sdf.Layer.CreateAnonymous(tag)`  | layer build    | new on-disk / anonymous in-memory layer                       |
|  [02]   | `Sdf.Layer.FindOrOpen(path)` / `layer.Export(path)`            | layer io       | resolve+open / write a layer to any registered format         |
|  [03]   | `Sdf.CreatePrimInLayer(layer, path)` / `Sdf.JustCreatePrimInLayer` | spec author | create a `PrimSpec` directly (composition-independent)         |
|  [04]   | `Sdf.Path('/World/Mesh.points')` / `path.AppendChild` / `path.IsPropertyPath` | path | construct/navigate scene paths                                |
|  [05]   | `Sdf.ChangeBlock()` (context manager)                          | batch edit     | coalesce many spec edits into one notification round          |
|  [06]   | `Ar.GetResolver().Resolve(assetPath)` / `.CreateIdentifier`    | resolve        | resolve an asset reference to a `ResolvedPath`                 |
|  [07]   | `Ar.ResolverContextBinder(ctx)` (context manager)             | resolve scope  | bind a resolver context for a resolution scope                |

## [04]-[IMPLEMENTATION_LAW]

[USD_TOPOLOGY]:
- composition-vs-storage law: `Usd.*` is the composed read/author VIEW; `Sdf.*` is the flat authored STORAGE. Read and author through `Usd.Stage`/`Usd.Prim`/`Usd.Attribute`; reach into `Sdf.Layer`/`Sdf.PrimSpec` only for batch spec authoring, layer metadata, or direct layer surgery. Never edit composed text by string manipulation — author opinions through the active `EditTarget`.
- edit-target law: every authored opinion lands in `stage.GetEditTarget()`. Author into a sublayer or a variant by setting the edit target (`stage.SetEditTarget(Usd.EditTarget(layer))`) or entering `variantSet.GetVariantEditContext()`; the default target is the root layer.
- schema law: author geometry/shading/skinning/lighting/physics through the typed schema wrappers — `UsdGeom.Mesh.Define(...)`, `mesh.CreatePointsAttr(...)`, `UsdGeom.XformCommonAPI(prim).SetTranslate(...)` — never `prim.CreateAttribute("points", Sdf.ValueTypeNames.Point3fArray)` by hand. Applied API schemas (`...API`) attach with `.Apply(prim)`; typed IsA schemas instantiate with `.Define(stage, path)`.
- value-tier law: attribute values are `Gf`/`Vt` types, not Python lists. Cross a `numpy` mesh buffer into USD with `Vt.Vec3fArray.FromNumpy(ndarray)` and read it back with `np.array(vtArray)`/`memoryview(vtArray)` over the buffer protocol (Vt exposes the C buffer protocol, NOT `__array_interface__`) — never element-wise Python loops over points/indices.
- packaging law: a `.usdz` is produced by `UsdUtils.CreateNewUsdzPackage` (general) or `CreateNewARKitUsdzPackage` (ARKit/QuickLook), which resolve and bundle the full `ComputeAllDependencies` (a `(layers, assets, unresolvedPaths)` 3-tuple) closure; only drop to `Sdf.ZipFileWriter` (a context manager — `Save` on clean exit, `Discard` on abort) for custom archive layouts. A valid referenceable usdz REQUIRES a `defaultPrim` and a single root layer.
- crate-vs-ascii law: `.usdc` (crate, binary, mmap-able) is the production format; `.usda` (ASCII) is for diffs/debugging; the extension passed to `Export`/`CreateNew` selects the `Sdf.FileFormat` plugin. A usdz bundles `.usdc` layers uncompressed-and-aligned so the runtime mmaps them in place.
- diagnostics law: USD's Python binding RAISES `Tf.ErrorException` for posted errors (e.g. `Stage.Open` on a missing/invalid layer), so a fallible USD span is guarded with `try/except Tf.ErrorException`; install a `UsdUtils.CoalescingDiagnosticDelegate` and drain it with `TakeCoalescedDiagnostics()` to capture the non-fatal warning/status stream into the receipt rather than letting USD's default delegate print to stderr. (`TfErrorMark` is a C++-only primitive — it is NOT exposed in the Python binding.)
- stage-sharing law: a stage opened repeatedly is de-duplicated through `Usd.StageCache` under a `Usd.StageCacheContext`; share one composed stage across pipeline stages instead of re-`Open`-ing the same asset.

[STACKS_WITH]:
- `vtk` (the USD exporter beside this owner): `vtk`'s `vtkUSDExporter` writes a render-mesh stage from a VTK render pipeline; this catalog owns full-fidelity AUTHORED scene description (variants, references, materials, skinning, physics, value clips). Route render/visualization meshes through `vtk`; route authored asset packaging, composition, and usdz through `pxr`. They meet at the `.usd{c,z}` layer on disk — never duplicate one's mesh-write path in the other.
- `numpy` (`.api/numpy.md`): the geometry buffer seam. `Vt.Vec3fArray.FromNumpy(points)` / `Vt.IntArray.FromNumpy(indices)` move a `numpy` mesh into USD attributes; `np.asarray(attr.Get())` reads it back zero-copy. `trimesh`/`meshio`/`open3d` meshes cross into a `UsdGeom.Mesh` through their `numpy` point/face arrays — the array IS the interop contract, no per-format USD writer.
- `msgspec`/`pydantic` (`.api/msgspec.md`, `.api/pydantic.md`): a typed `Struct`/model owns the authoring INTENT (asset id, up-axis, unit, material spec, variant selections); a single projector folds that validated model into `Stage`/schema calls. Decode the asset request with `msgspec.json.Decoder(type=AssetSpec)`, then author — the wire model and the USD authoring are one rail, never ad-hoc kwargs threaded through the pipeline. `UsdUtils.ComputeUsdStageStats`/`ComplianceChecker.GetErrors()` map field-for-field onto a `msgspec.Struct` receipt.
- `expression` (`.api/expression.md`): every USD boundary (`Stage.Open`, `Export`, `CreateNewUsdzPackage`, `UsdValidation.ValidationContext.Validate`) is lifted into `Result`. Guard the span with `try/except Tf.ErrorException` and return `Error(BoundaryFault(...))` carrying the message plus the drained `CoalescingDiagnosticDelegate` diagnostics — a composition error, a missing asset, or a validation error is a typed `Error`, never an escaped exception or a silently empty stage.
- `anyio` (`.api/anyio.md`): USD authoring/serialization is CPU-bound and releases work to a native thread pool internally (`Work.SetConcurrencyLimit`), but the Python API holds the GIL — offload a whole `Export`/`CreateNewUsdzPackage` call through `anyio.to_thread.run_sync(..., limiter=...)` so a packaging job does not block the event loop. Bound the usdz-write subsystem with a dedicated `CapacityLimiter`. Tie `Work.SetConcurrencyLimit` to the same budget the offload limiter enforces.
- `stamina` (`.api/stamina.md`): an asset-resolution or remote-layer `Stage.Open`/`Ar.GetResolver().Resolve` that fails transiently rides `stamina.retry_context(on=BoundaryFault, ...)`; the resolve is the retried effect, and a `ComplianceChecker` failure is NON-retryable (it surfaces immediately).
- `structlog`/`opentelemetry` (`.api/structlog.md`, `.api/opentelemetry-*.md`): one OTel span per packaging operation; bind `UsdUtils.ComputeUsdStageStats` (prim/instance/asset counts) and the `UsdValidation` error count onto the structlog receipt. A `UsdUtils.CoalescingDiagnosticDelegate` captures and coalesces USD's own diagnostic emissions; `delegate.TakeCoalescedDiagnostics()` drains them as structured warnings into the receipt rather than letting the default delegate print to stderr.
- `fsspec`/`obstore`/`universal-pathlib` (`.api/fsspec.md`, `.api/obstore.md`): USD resolves assets through `Ar`, not `fsspec`; to read/write a stage from object storage, materialize the layer bytes to/from a real local path (USD layers and usdz mmap require a filesystem path) and let `Ar.DefaultResolver` resolve the local closure. The object-store hop is the boundary adapter around `Stage.Export`/`Open`, never inside the resolver.

[LOCAL_ADMISSION]:
- Author through `Usd.Stage` + typed `UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux`/`UsdPhysics` schema wrappers; never raw `prim.CreateAttribute` with a hand-named `Sdf.ValueTypeNames` type when a `Create<Name>Attr` exists.
- Move geometry as `Vt` arrays via `.FromNumpy`/`np.asarray`; never Python-loop over points or face indices.
- Produce usdz through `UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage` (dependency-closure aware); drop to `Sdf.ZipFileWriter` only for a custom archive layout, and always set a `defaultPrim` first.
- Guard every USD boundary with `try/except Tf.ErrorException` and lift to `Result`; install a `UsdUtils.CoalescingDiagnosticDelegate` and drain `TakeCoalescedDiagnostics()` into the receipt rather than letting diagnostics hit stderr (`TfErrorMark` is C++-only — never cite it in Python).
- Offload `Export`/packaging/validation through `anyio.to_thread.run_sync` with a bounded `CapacityLimiter`; never call them inline on the event loop.
- Set up-axis and meters-per-unit (`UsdGeom.SetStageUpAxis`/`SetStageMetersPerUnit`) on every authored stage; an unset stage-up-axis is a downstream-consumer defect.
- Validate any usdz destined for ARKit/QuickLook before publishing via `UsdValidation.ValidationContext.Validate` (the modern framework); `UsdUtils.ComplianceChecker(arkit=True)` is the deprecated-but-functional fallback. Treat a non-empty validation-error list as a publish gate.

[RAIL_LAW]:
- Package: `usd-core` (`pxr`)
- Owns: composed `Usd.Stage` scene-graph authoring, `Sdf` layer/spec storage + native usdz zip, typed `UsdGeom`/`UsdShade`/`UsdSkel`/`UsdLux`/`UsdPhysics` schemas, the `Gf`/`Vt` value tier with `numpy` interop, crate/ASCII serialization, and `UsdUtils` dependency/flatten/localize/compliance/usdz packaging
- Accept: `Stage.{CreateNew,CreateInMemory,Open,Export,Flatten}`, typed-schema `.Define`/`.Apply` + `Create<Name>Attr`, `Vt.*Array.FromNumpy`/`np.asarray`, `UsdUtils.{CreateNewUsdzPackage,CreateNewARKitUsdzPackage,ComputeAllDependencies,FlattenLayerStack,LocalizeAsset}`, `UsdValidation.ValidationContext.Validate` (with `ComplianceChecker` as deprecated fallback), `Sdf.ZipFileWriter` for custom archives, `try/except Tf.ErrorException` boundaries lifted to `Result`, `anyio.to_thread` offload
- Reject: raw `prim.CreateAttribute` where a typed schema getter exists, Python-loop geometry construction, hand-rolled usdz/zip archiving or crate codecs, string-editing composed layer text, swallowing `Tf` diagnostics, inline (non-offloaded) `Export`/packaging on an event loop, publishing a usdz without a `defaultPrim`/compliance gate, assuming a `cp315` wheel exists while the `python_version<'3.15'` marker stands
