# [PY_ARTIFACTS_SCENE_STAGE]

The USD/USDZ stage-authoring owner. `UsdStage` is the one Pixar OpenUSD scene-composition owner over the closed `StageOp` family — `RenderExport` (the `vtkUSDExporter` writes the `scene/render#SCENE` rendered scene to a `.usdc`/`.usda` layer over the offscreen `Plotter.render_window`) and `MeshAuthor` (a `pxr.Usd.Stage` authored directly from a `numpy` point/index buffer over the `UsdGeom`/`Vt`/`Gf` typed-prim schema family, bypassing the render pass) — each case folding to a serialized USD layer through one total `apply` `match`, plus the `UsdUtils.CreateNewUsdzPackage` packaging close that zips an authored `.usdc` layer (with its dependency-asset closure) into the `.usdz` AR/Apple deliverable. `LayerFormat` is the closed crate/ASCII `StrEnum` keyed by the file suffix the `Stage.Export` writes (`.usdc` binary, `.usda` ASCII), never a parallel per-format stage writer. The owner rides the wider worker band — OpenUSD declares a HARD `runtime requirement: >=3.9, <3.15` upper cap and ships no package, so `pxr` imports neither on the runtime nor at module scope but only behind the `scene/render#SCENE` `anyio.to_process.run_sync` subprocess seam on the same sub-3.13 vtk worker the render already crosses onto (the `usd-core` worker marker co-resolving with the `vtk` worker floor on one worker). The `scene/export#EXPORT` `USD`/`USDZ` `SceneTarget` arms delegate here, handing the `Plotter.render_window` for `RenderExport` and the `.usdc` path for `package_usdz`. This owner authors and packages only — the offscreen render stays at `scene/render#SCENE`/`pyvista`/`vtk`, and raw mesh-file interchange stays at `geometry/mesh`. This page closes the `USD_USDZ_SCENE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[STAGE]: the one `UsdStage` owner over the closed `StageOp` family — `RenderExport`/`MeshAuthor` folding to a serialized USD layer through one total `apply` `match`, the `LayerFormat` crate/ASCII `StrEnum` keyed by file suffix, the `UsdUtils.CreateNewUsdzPackage(Sdf.AssetPath, str)` USDZ packaging close over the dependency-asset closure, the `vtkUSDExporter` render-to-layer write over `Plotter.render_window`, and the direct `Usd.Stage.CreateInMemory`/`UsdGeom.Mesh.Define`/`Vt.Vec3fArray`/`SetStageUpAxis`/`Export` mesh-authoring path; `usd-core` `pxr.Usd.Stage`/`pxr.UsdGeom`/`pxr.Sdf`/`pxr.Gf`/`pxr.Vt`/`pxr.UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage` settled against the folder `.api`; the `vtkmodules.vtkIOUSD.vtkUSDExporter` over `SetRenderWindow`/`SetFileName`/`Write` and the `Plotter.render_window` accessor carried as [03]-[RESEARCH] catalogue-deepen seams.

## [02]-[STAGE]

- Cases: `StageOp` cases — `RenderExport(render_window)` (the `vtkmodules.vtkIOUSD.vtkUSDExporter` writes the `scene/render#SCENE` rendered scene — surface geometry, color-mapped surfaces, lights, camera, PBR — to a `.usdc`/`.usda` layer over `SetRenderWindow(render_window)`/`SetFileName(path)`/`Write()`, the `Plotter.render_window` the `scene/export#EXPORT` `USD`/`USDZ` arm hands across; the rendered-scene-to-layer half of the export) · `MeshAuthor(points, faces, up_axis, meters_per_unit)` (a `pxr.Usd.Stage.CreateInMemory()` authored directly from a `numpy` point/index buffer — `UsdGeom.Mesh.Define(stage, "/Root/Mesh")` then `CreatePointsAttr(Vt.Vec3fArray(points))`/`CreateFaceVertexCountsAttr`/`CreateFaceVertexIndicesAttr(Vt.IntArray(faces))`, `SetStageUpAxis(stage, UsdGeom.Tokens.y)`/`SetStageMetersPerUnit(stage, meters_per_unit)`/`SetDefaultPrim`, then `Export(path)` — the pure-geometry USD export `author_mesh` exposes when no render is needed, bypassing the VTK render pass entirely) — matched by one total `match`/`case`, the layer serialized to the `LayerFormat`-suffixed path, the modality recovered from the `StageOp` discriminant. The USDZ packaging is not a third case but the `package_usdz` close over either arm's written `.usdc`: `CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` gathers the layer's dependency-asset closure and writes the `.usdz`, with `CreateNewARKitUsdzPackage` the Apple QuickLook-constrained variant as a data choice on the same close, never a per-platform package owner.
- Auto: the `RenderExport` arm reads the `Plotter.render_window` directly — the VTK exporter writes whatever the offscreen render composed (the `RenderSpec.added` scalar-mapped surfaces, the `RenderSpec.viewed` camera and lights), so a USD export carries the same scene state the image render would; the `MeshAuthor` arm folds the numpy point buffer into one `CreatePointsAttr(Vt.Vec3fArray(points))` `Set` and the face buffer into one `CreateFaceVertexIndicesAttr(Vt.IntArray(faces))` `Set`, never a per-vertex append, the `Vt` typed array carrying the whole buffer; `SetStageUpAxis(stage, UsdGeom.Tokens.y)`/`SetStageMetersPerUnit(stage, meters_per_unit)`/`SetDefaultPrim(mesh.GetPrim())` set the AR/real-world stage metadata before export; the `package_usdz` close hands the authored `.usdc` to `CreateNewUsdzPackage`, which itself gathers and embeds the full dependency-asset closure, so a scene authored with referenced textures packages a complete self-contained `.usdz` rather than one with dangling references; the `LayerFormat` suffix selects crate-binary vs ASCII at `Export`.
- Receipt: each USD/USDZ export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes)` minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes` — the content key the `_emit` arm derives, the `SceneTarget` value (`"usdc"`/`"usdz"`) the target string, and the serialized USD-layer / packaged-USDZ byte count the evidence slot; this owner contributes that serialized payload, never a parallel per-format receipt rail. The authored-prim-count, up-axis, meters-per-unit, and dependency-asset-count facts land when the `core/receipt#RECEIPT` `scene` case widens beyond `(target, bytes)`, exactly as the `scene/render#SCENE` point/cell/window evidence does.
- Growth: a new USD prim schema (`Points` point-cloud, `PointInstancer` for large repeated geometry, `Camera` viewpoint prim) is one `UsdGeom.<Schema>.Define` call in the `MeshAuthor` arm; a new layer format is one `LayerFormat` row keyed by suffix; a new packaging policy (ARKit-constrained) is the `CreateNewARKitUsdzPackage` data choice on the `package_usdz` close; a new material binding is a `UsdShade.Material`/`Shader` graph authored over the prim; a new stage source is one `StageOp` case folding into the `apply` `match`; zero new owner — the source axis stays two cases (`RenderExport`/`MeshAuthor`) on one owner, the packaging one close, every addition a schema call, row, case, or data choice.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from numpy.typing import NDArray

lazy from pxr import Sdf, Usd, UsdGeom, UsdUtils, Vt

# --- [TYPES] ---------------------------------------------------------------------------

type PrimKindTag = Literal["mesh", "points"]

# --- [MODELS] --------------------------------------------------------------------------


@tagged_union(frozen=True)
class PrimKind:
    tag: PrimKindTag = tag()
    mesh: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None] = case()
    points: tuple[NDArray[np.float32], NDArray[np.float32] | None] = case()

    @staticmethod
    def Mesh(
        points: NDArray[np.float32],
        faces: NDArray[np.int32],
        normals: NDArray[np.float32] | None = None,
    ) -> "PrimKind":
        return PrimKind(mesh=(points, faces, normals))

    @staticmethod
    def Points(points: NDArray[np.float32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        return PrimKind(points=(points, widths))

    def define(self, stage: object, path: object) -> object:
        match self:
            case PrimKind(tag="mesh", mesh=(verts, faces, normals)):
                mesh = UsdGeom.Mesh.Define(stage, path)
                mesh.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(verts))  # zero-copy buffer bridge; bare Vt.Vec3fArray(ndarray) ctor raises TypeError
                counts = np.full(faces.shape[0], faces.shape[1], dtype=np.int32)
                mesh.CreateFaceVertexCountsAttr(Vt.IntArray.FromNumpy(counts))
                mesh.CreateFaceVertexIndicesAttr(Vt.IntArray.FromNumpy(faces.reshape(-1)))
                if normals is not None:
                    mesh.CreateNormalsAttr(Vt.Vec3fArray.FromNumpy(normals))
                return mesh.GetPrim()
            case PrimKind(tag="points", points=(coords, widths)):
                cloud = UsdGeom.Points.Define(stage, path)
                cloud.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(coords))
                if widths is not None:
                    cloud.CreateWidthsAttr(Vt.FloatArray.FromNumpy(widths))
                return cloud.GetPrim()
            case _:
                assert_never(self)

# --- [OPERATIONS] ----------------------------------------------------------------------


def author_scene(prim: PrimKind, path: str, up_axis: str = "z", meters_per_unit: float = 1.0) -> None:
    stage = Usd.Stage.CreateInMemory(path)
    UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.y if up_axis == "y" else UsdGeom.Tokens.z)
    UsdGeom.SetStageMetersPerUnit(stage, meters_per_unit)
    stage.SetDefaultPrim(prim.define(stage, Sdf.Path("/Root")))  # top-level prim is a valid USDZ default prim
    stage.Export(path)  # usd-core discriminates .usdc crate-binary vs .usda ASCII by the path suffix natively


def package_usdz(source: str, sink: str, arkit: bool = False) -> bool:
    asset = Sdf.AssetPath(source)
    package = UsdUtils.CreateNewARKitUsdzPackage if arkit else UsdUtils.CreateNewUsdzPackage
    return package(asset, sink)  # CreateNew*UsdzPackage gathers and embeds the full dependency-asset closure itself; a bare ComputeAllDependencies query before it embeds nothing and is dead
```

The `StageOp.apply` arms and the `export_usd`/`author_mesh`/`package_usdz` module worker-arm functions reference the module-scope `lazy from pxr`/`lazy from vtkmodules.vtkIOUSD` bindings, reified inside the worker worker only — the `pxr` Boost.Python extension does NOT import on the runtime, so the import lives behind the `scene/render#SCENE` `anyio.to_process.run_sync` subprocess seam on the same sub-3.13 worker the `pyvista`/`vtk` render already crosses onto, and the `to_process.run_sync` seam dispatches the three module-level functions by qualified name (it cannot target a bound method or closure), each forwarding to the `StageOp.apply` fold so the source axis stays one owner. The `Vt.Vec3fArray(points.tolist())`/`Vt.IntArray(...)` construction folds the numpy point/index buffers into the typed USD arrays the catalogue settles (the zero-copy `Vt.<Type>Array.FromNumpy` bridge is the [USD_AUTHOR]-tracked catalogue-deepen seam); the `package_usdz` close hands the `.usdc` to `CreateNewUsdzPackage`, which gathers and embeds the dependency-asset closure itself, and selects `CreateNewARKitUsdzPackage` over `CreateNewUsdzPackage` on the `arkit` data choice; the `Tf.ErrorException` (missing asset, malformed layer, write failure) and `Boost.Python.ArgumentError` (a wrong-typed argument, the `Sdf.AssetPath`-vs-`str` mismatch) are mapped at the worker boundary into the runtime `RuntimeRail` fault, never surfaced bare across the interpreter seam, and a `bool` `False` return from `CreateNewUsdzPackage` is a packaging-failure rail the `package_usdz` `bool` return carries.
