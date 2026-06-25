# [PY_ARTIFACTS_SCENE_STAGE]

The USD/USDZ stage-authoring owner. `UsdStage` is the one Pixar OpenUSD scene-composition owner over the closed `StageOp` family — `RenderExport` (the `vtkUSDExporter` writes the `scene/render#SCENE` rendered scene to a `.usdc`/`.usda` layer over the offscreen `Plotter.render_window`) and `MeshAuthor` (a `pxr.Usd.Stage` authored directly from a `numpy` point/index buffer over the `UsdGeom`/`Vt`/`Gf` typed-prim schema family, bypassing the render pass) — each case folding to a serialized USD layer through one total `apply` `match`, plus the `UsdUtils.CreateNewUsdzPackage` packaging close that zips an authored `.usdc` layer (with its dependency-asset closure) into the `.usdz` AR/Apple deliverable. `LayerFormat` is the closed crate/ASCII `StrEnum` keyed by the file suffix the `Stage.Export` writes (`.usdc` binary, `.usda` ASCII), never a parallel per-format stage writer. The owner rides the wider gated `python_version<'3.15'` band — OpenUSD declares a HARD `Requires-Python: >=3.9, <3.15` upper cap and ships no cp315 wheel, so `pxr` imports neither on the cp315 core nor at module scope but only behind the `scene/render#SCENE` `anyio.to_process.run_sync` subprocess seam on the same sub-3.13 vtk worker the render already crosses onto (the `usd-core` `python_version<'3.15'` marker co-resolving with the `vtk` `python_version<'3.13'` floor on one worker). The `scene/export#EXPORT` `USD`/`USDZ` `SceneTarget` arms delegate here, handing the `Plotter.render_window` for `RenderExport` and the `.usdc` path for `package_usdz`. This owner authors and packages only — the offscreen render stays at `scene/render#SCENE`/`pyvista`/`vtk`, and raw mesh-file interchange stays at `geometry/mesh`. This page closes the `USD_USDZ_SCENE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[STAGE]: the one `UsdStage` owner over the closed `StageOp` family — `RenderExport`/`MeshAuthor` folding to a serialized USD layer through one total `apply` `match`, the `LayerFormat` crate/ASCII `StrEnum` keyed by file suffix, the `UsdUtils.CreateNewUsdzPackage(Sdf.AssetPath, str)` USDZ packaging close over the dependency-asset closure, the `vtkUSDExporter` render-to-layer write over `Plotter.render_window`, and the direct `Usd.Stage.CreateInMemory`/`UsdGeom.Mesh.Define`/`Vt.Vec3fArray`/`SetStageUpAxis`/`Export` mesh-authoring path; `usd-core` `pxr.Usd.Stage`/`pxr.UsdGeom`/`pxr.Sdf`/`pxr.Gf`/`pxr.Vt`/`pxr.UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage`/`ComputeAllDependencies` settled against the folder `.api`; the `vtkmodules.vtkIOUSD.vtkUSDExporter` over `SetRenderWindow`/`SetFileName`/`Write` and the `Plotter.render_window` accessor carried as [03]-[RESEARCH] catalogue-deepen seams.

## [02]-[STAGE]

- Owner: `UsdStage` the one USD/USDZ stage-authoring owner discriminating the source modality over the closed `StageOp` `tagged_union`; `StageOp` whose every case carries its own typed payload — `RenderExport(render_window, fmt)` the VTK-rendered-scene-to-layer write, `MeshAuthor(points, faces, fmt, up_axis, meters_per_unit)` the direct numpy-buffer stage-authoring — never a shared erased params bag nor a per-source stage subclass; `LayerFormat` the closed `StrEnum` of layer serialization formats keyed by the file suffix the `Stage.Export`/`Sdf.Layer.Export` writes — `USDC` (`"usdc"` crate-binary) / `USDA` (`"usda"` ASCII) — never a parallel per-format writer class, the format discriminated by the export suffix; `Usd.Stage` the single authoring root whose source kind (a new on-disk layer `CreateNew`, an in-memory anonymous layer `CreateInMemory`, an opened existing layer `Open`) is a static-method choice on one type, never a parallel stage class; `UsdGeom.<Schema>.Define(stage, path)` the one typed-prim definition surface — a `Mesh`, an `Xform`, a `Camera` are three schemas over the one `Usd.Prim` node, never three parallel prim classes; `Vt.Vec3fArray`/`Vt.IntArray` the homogeneous typed-array point/index buffers a geometry attribute `Set` carries in one call, never a per-vertex object list; `UsdUtils.CreateNewUsdzPackage(asset_path, usdz_path)` the one USDZ builder whose FIRST argument is an `Sdf.AssetPath` (NOT a bare `str` — a raw-string call raises `Boost.Python.ArgumentError`), gathering the root layer plus every dependency asset and writing the aligned-zip `.usdz`; the `pxr` package imports only on the gated sub-3.13 worker, never on the cp315-core page.
- Cases: `StageOp` cases — `RenderExport(render_window, fmt)` (the `vtkmodules.vtkIOUSD.vtkUSDExporter` writes the `scene/render#SCENE` rendered scene — surface geometry, color-mapped surfaces, lights, camera, PBR — to a `.usdc`/`.usda` layer over `SetRenderWindow(render_window)`/`SetFileName(path)`/`Write()`, the `Plotter.render_window` the `scene/export#EXPORT` `USD`/`USDZ` arm hands across; the rendered-scene-to-layer half of the export) · `MeshAuthor(points, faces, fmt, up_axis, meters_per_unit)` (a `pxr.Usd.Stage.CreateInMemory()` authored directly from a `numpy` point/index buffer — `UsdGeom.Mesh.Define(stage, "/Root/Mesh")` then `CreatePointsAttr(Vt.Vec3fArray(points))`/`CreateFaceVertexCountsAttr`/`CreateFaceVertexIndicesAttr(Vt.IntArray(faces))`, `SetStageUpAxis(stage, UsdGeom.Tokens.y)`/`SetStageMetersPerUnit(stage, meters_per_unit)`/`SetDefaultPrim`, then `Export(path)` — the pure-geometry USD export the same arm reaches when no render is needed, bypassing the VTK render pass entirely) — matched by one total `match`/`case`, the layer serialized to the `LayerFormat`-suffixed path, the modality recovered from the `StageOp` discriminant. The USDZ packaging is not a third case but the `package_usdz` close over either arm's written `.usdc`: `CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` gathers the layer's dependency-asset closure and writes the `.usdz`, with `CreateNewARKitUsdzPackage` the Apple QuickLook-constrained variant as a data choice on the same close, never a per-platform package owner.
- Entry: `export_usd(render_window, path)` and `author_mesh(points, faces, path, ...)` and `package_usdz(source, sink)` are module-level worker-arm functions the `scene/export#EXPORT` `render_export` `USD`/`USDZ` arms dispatch on the gated sub-3.13 companion-floor worker (the cp315-core process imports no `pxr` nor `vtkmodules.vtkIOUSD`), importing `pxr`/`vtkmodules.vtkIOUSD` at boundary scope inside the worker only. `export_usd` constructs the `vtkUSDExporter`, wires `SetRenderWindow`/`SetFileName`, and calls `Write()`; `author_mesh` opens an in-memory stage, defines the typed mesh prim from the numpy buffers over the `Vt` arrays, sets the stage axes, and exports the layer; `package_usdz` wraps the `.usdc` path in `Sdf.AssetPath`, optionally runs `ComputeAllDependencies` to embed the asset closure, and calls `CreateNewUsdzPackage`. The serialized layer `bytes` (or the packaged `.usdz` `bytes`) cross the seam back to `scene/export#EXPORT`'s `render_export`, which returns them to `scene/render#SCENE`'s `_emit` `Export` case for `ContentIdentity.of(target.value, data)` keying — the `USD` value `"usdc"`, the `USDZ` value `"usdz"`, the content-key format tag.
- Auto: the `RenderExport` arm reads the `Plotter.render_window` directly — the VTK exporter writes whatever the offscreen render composed (the `RenderSpec.added` scalar-mapped surfaces, the `RenderSpec.viewed` camera and lights), so a USD export carries the same scene state the image render would; the `MeshAuthor` arm folds the numpy point buffer into one `CreatePointsAttr(Vt.Vec3fArray(points))` `Set` and the face buffer into one `CreateFaceVertexIndicesAttr(Vt.IntArray(faces))` `Set`, never a per-vertex append, the `Vt` typed array carrying the whole buffer; `SetStageUpAxis(stage, UsdGeom.Tokens.y)`/`SetStageMetersPerUnit(stage, meters_per_unit)`/`SetDefaultPrim(mesh.GetPrim())` set the AR/real-world stage metadata before export; the `package_usdz` close runs `ComputeAllDependencies(Sdf.AssetPath(usdc))` to embed every referenced asset before `CreateNewUsdzPackage`, so a scene authored with referenced textures packages a complete self-contained `.usdz` rather than one with dangling references; the `LayerFormat` suffix selects crate-binary vs ASCII at `Export`.
- Receipt: each USD/USDZ export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target)` keyed by the content key the `scene/render#SCENE` `_emit` arm derives, the `SceneTarget` value (`"usdc"`/`"usdz"`) carried as the target string; the receipt is minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes`, this owner contributing the serialized USD layer / packaged USDZ payload plus its authored-prim-count, up-axis, meters-per-unit, and dependency-asset-count facts, never a parallel per-format receipt rail.
- Packages: `usd-core` (import `pxr`; `pxr.Usd.Stage.CreateInMemory`/`CreateNew`/`DefinePrim`/`Export`/`GetRootLayer`/`Save`, `pxr.UsdGeom.Mesh.Define`/`CreatePointsAttr`/`CreateFaceVertexCountsAttr`/`CreateFaceVertexIndicesAttr`/`CreateNormalsAttr`/`CreateDisplayColorAttr`, `pxr.UsdGeom.Xform`/`Camera`/`Scope`, `pxr.UsdGeom.SetStageUpAxis`/`SetStageMetersPerUnit`/`Tokens`, `pxr.Sdf.AssetPath`/`Sdf.Path`/`Sdf.Layer`/`Sdf.ValueTypeNames`, `pxr.Gf.Vec3f`/`Matrix4d`, `pxr.Vt.Vec3fArray`/`Vt.IntArray`, `pxr.UsdShade.Material`/`Shader`, `pxr.UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage`/`ExtractUsdzPackage`/`ComputeAllDependencies`/`ModifyAssetPaths`, the `pxr.Tf.ErrorException`/`Boost.Python.ArgumentError` fault map all settled against the folder `.api`) gated `python_version<'3.15'`, resolving on the sub-3.13 vtk worker; `vtk` (the native render engine; `vtkmodules.vtkIOUSD.vtkUSDExporter` over `SetRenderWindow`/`SetFileName`/`Write` [03]-[RESEARCH]) gated `python_version<'3.13'`; `pyvista` (`Plotter.render_window` accessor feeding the `vtkUSDExporter.SetRenderWindow` argument [03]-[RESEARCH]) gated `python_version<'3.13'`; `numpy` (the point/index buffer the `MeshAuthor` arm folds into `Vt.Vec3fArray`/`Vt.IntArray`, imported only on the gated-band worker); runtime (the `anyio.to_process.run_sync` lane the export crosses, the `ContentIdentity.of` keying inherited from `scene/render#SCENE`).
- Growth: a new USD prim schema (`Points` point-cloud, `PointInstancer` for large repeated geometry, `Camera` viewpoint prim) is one `UsdGeom.<Schema>.Define` call in the `MeshAuthor` arm; a new layer format is one `LayerFormat` row keyed by suffix; a new packaging policy (ARKit-constrained) is the `CreateNewARKitUsdzPackage` data choice on the `package_usdz` close; a new material binding is a `UsdShade.Material`/`Shader` graph authored over the prim; a new stage source is one `StageOp` case folding into the `apply` `match`; zero new owner — the source axis stays two cases (`RenderExport`/`MeshAuthor`) on one owner, the packaging one close, every addition a schema call, row, case, or data choice.
- Boundary: usd-core owns USD stage authoring (`Usd.Stage`/`UsdGeom`/`UsdShade`/`Sdf`), layer serialization to `.usdc`/`.usda` (`LayerFormat` by suffix), and USDZ packaging/extraction over the dependency-asset closure (`UsdUtils`) on the gated sub-3.13 worker — the `RenderExport` arm packages the `vtk`-written rendered scene (camera/lights/PBR surfaces), distinct from the `geometry/mesh` raw mesh-file codec (the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam records that scene = visualization-scene export and geometry mesh = mesh-file codec, no shared owner), so a future pass cannot collapse the USD scene export into a mesh-export owner. Offscreen RENDERING is NOT this owner's concern — the rendered raster stays at `scene/render#SCENE`/`pyvista`/`vtk`, this owner authors and packages only; live USD viewers/Hydra render delegates stay outside the `usd-core` wheel (it ships no Storm/embree delegate). The `pxr` package rides the wider `python_version<'3.15'` band (the HARD `<3.15` metadata ceiling, not merely a wheel gap), and `vtk` the `python_version<'3.13'` floor, both resolving on the one sub-3.13 vtk worker the `scene/render#SCENE` render already crosses onto — none resolving in the cp315-core process, so `pxr`/`vtkmodules.vtkIOUSD` import at module scope inside the worker only, never on the cp315-core page. The prior `pxr.UsdUtils.UsdUtilsCreateNewUsdzPackage` phantom spelling (the Python binding DROPS the `UsdUtils` C++ module prefix — the real name is `UsdUtils.CreateNewUsdzPackage`) and the bare-`str` first-argument USDZ call (which raises `Boost.Python.ArgumentError` where `Sdf.AssetPath` is required) are the deleted forms — the USD authoring/packaging surface is now one `UsdStage` owner over the closed `StageOp` family plus the `LayerFormat` suffix vocabulary and the `Sdf.AssetPath`-keyed `CreateNewUsdzPackage` close, the render-export and direct-mesh-author sources sharing one stage authoring path, never a per-format writer or a phantom-spelled raw-string packaging call.

```python signature
from enum import StrEnum
from pathlib import Path
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from numpy.typing import NDArray

type Vec3 = tuple[float, float, float]
type StageOpTag = Literal["render_export", "mesh_author"]


class LayerFormat(StrEnum):
    USDC = "usdc"
    USDA = "usda"

    @staticmethod
    def of(path: str) -> "LayerFormat":
        return LayerFormat.USDA if Path(path).suffix == ".usda" else LayerFormat.USDC


@tagged_union(frozen=True)
class StageOp:
    tag: StageOpTag = tag()
    render_export: object = case()
    mesh_author: tuple[NDArray[np.float32], NDArray[np.int32], str, float] = case()

    @staticmethod
    def RenderExport(render_window: object) -> "StageOp":
        return StageOp(render_export=render_window)

    @staticmethod
    def MeshAuthor(
        points: NDArray[np.float32],
        faces: NDArray[np.int32],
        up_axis: str = "y",
        meters_per_unit: float = 1.0,
    ) -> "StageOp":
        return StageOp(mesh_author=(points, faces, up_axis, meters_per_unit))

    def apply(self, path: str) -> None:
        match self:
            case StageOp(tag="render_export", render_export=render_window):
                from vtkmodules.vtkIOUSD import vtkUSDExporter

                exporter = vtkUSDExporter()
                exporter.SetRenderWindow(render_window)
                exporter.SetFileName(path)
                exporter.Write()
            case StageOp(tag="mesh_author", mesh_author=(points, faces, up_axis, meters_per_unit)):
                from pxr import Sdf, Usd, UsdGeom, Vt

                stage = Usd.Stage.CreateInMemory(path)
                UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.y if up_axis == "y" else UsdGeom.Tokens.z)
                UsdGeom.SetStageMetersPerUnit(stage, meters_per_unit)
                mesh = UsdGeom.Mesh.Define(stage, Sdf.Path("/Root/Mesh"))
                mesh.CreatePointsAttr(Vt.Vec3fArray(points.tolist()))
                counts = np.full(faces.shape[0], faces.shape[1], dtype=np.int32)
                mesh.CreateFaceVertexCountsAttr(Vt.IntArray(counts.tolist()))
                mesh.CreateFaceVertexIndicesAttr(Vt.IntArray(faces.reshape(-1).tolist()))
                stage.SetDefaultPrim(mesh.GetPrim())
                stage.Export(str(Path(path).with_suffix(f".{LayerFormat.of(path)}")))
            case _:
                assert_never(self)


def export_usd(render_window: object, path: str) -> None:
    StageOp.RenderExport(render_window).apply(path)


def author_mesh(
    points: NDArray[np.float32],
    faces: NDArray[np.int32],
    path: str,
    up_axis: str = "y",
    meters_per_unit: float = 1.0,
) -> None:
    StageOp.MeshAuthor(points, faces, up_axis, meters_per_unit).apply(path)


def package_usdz(source: str, sink: str, arkit: bool = False) -> bool:
    from pxr import Sdf, UsdUtils

    asset = Sdf.AssetPath(source)
    UsdUtils.ComputeAllDependencies(asset)
    package = UsdUtils.CreateNewARKitUsdzPackage if arkit else UsdUtils.CreateNewUsdzPackage
    return package(asset, sink)
```

The `StageOp.apply` arms and the `export_usd`/`author_mesh`/`package_usdz` module worker-arm functions import `pxr`/`vtkmodules.vtkIOUSD` at boundary scope inside the gated-band worker only — the `pxr` Boost.Python extension does NOT import on the cp315 core, so the import lives behind the `scene/render#SCENE` `anyio.to_process.run_sync` subprocess seam on the same sub-3.13 worker the `pyvista`/`vtk` render already crosses onto, and the `to_process.run_sync` seam dispatches the three module-level functions by qualified name (it cannot target a bound method or closure), each forwarding to the `StageOp.apply` fold so the source axis stays one owner. The `Vt.Vec3fArray(points.tolist())`/`Vt.IntArray(...)` construction folds the numpy point/index buffers into the typed USD arrays the catalogue settles (the zero-copy `Vt.<Type>Array.FromNumpy` bridge is the [USD_AUTHOR]-tracked catalogue-deepen seam); the `package_usdz` close runs `ComputeAllDependencies(Sdf.AssetPath(source))` to embed the dependency-asset closure before packaging and selects `CreateNewARKitUsdzPackage` over `CreateNewUsdzPackage` on the `arkit` data choice; the `Tf.ErrorException` (missing asset, malformed layer, write failure) and `Boost.Python.ArgumentError` (a wrong-typed argument, the `Sdf.AssetPath`-vs-`str` mismatch) are mapped at the worker boundary into the runtime `RuntimeRail` fault, never surfaced bare across the interpreter seam, and a `bool` `False` return from `CreateNewUsdzPackage` is a packaging-failure rail the `package_usdz` `bool` return carries.

## [03]-[RESEARCH]

- [USD_AUTHOR]: the direct USD stage-authoring surface verifies against the folder `.api` catalogue for `usd-core` (`26.5`, gated `python_version<'3.15'`, resolving on the sub-3.13 vtk worker) — `usd-core.md` `[02]-[PUBLIC_TYPES]` carries `Usd.Stage` (statics `CreateNew(path)`/`CreateInMemory()`/`Open(path)`, `DefinePrim`/`SetDefaultPrim`/`GetRootLayer`/`Save`/`Export`), `Usd.Prim`/`Usd.Attribute`, `Sdf.AssetPath`/`Sdf.Path`/`Sdf.Layer`/`Sdf.ValueTypeNames`, the `UsdGeom.Xform`/`Mesh`/`Points`/`Camera`/`Scope`/`PointInstancer` typed-prim schemas (`Define(stage, path)` static per schema, `Mesh.CreatePointsAttr`/`CreateFaceVertexCountsAttr`/`CreateFaceVertexIndicesAttr`/`CreateNormalsAttr`/`CreateDisplayColorAttr`), the `UsdGeom.SetStageUpAxis`/`SetStageMetersPerUnit`/`Tokens` stage axes, the `Gf.Vec3f`/`Matrix4d`/`Quatf` math values, the `Vt.Vec3fArray`/`IntArray`/`FloatArray`/`TokenArray` typed arrays, and the `UsdShade.Material`/`Shader` material graph, and `[03]-[ENTRYPOINTS]` carries `Usd.Stage.CreateInMemory(identifier='') -> Usd.Stage`, `UsdGeom.<Schema>.Define(stage, path)`, `Mesh.CreatePointsAttr(defaultValue: Vt.Vec3fArray = None, writeSparsely=False) -> Usd.Attribute`, `SetStageUpAxis(stage, upAxis: str) -> bool`, `SetStageMetersPerUnit(stage, metersPerUnit: float) -> bool`, `Stage.SetDefaultPrim(prim) -> None`, `Stage.GetRootLayer() -> Sdf.Layer` + `Sdf.Layer.Save() -> bool`, and `Stage.Export(filename, addSourceFileComment=True) -> bool`, so the `MeshAuthor` arm's stage authoring, typed-prim definition, attribute writing over the `Vt`/`Gf` value vocabulary, stage-metadata, and layer export are settled fence code. The `Vt.<Type>Array.FromNumpy(ndarray)` zero-copy numpy-to-USD-array bridge is the spelling the `MeshAuthor` numpy-buffer fold uses; the `usd-core.md` value axis catalogues `Vt.Vec3fArray(points)` construction directly from a buffer but does not enumerate the `FromNumpy` classmethod, so the `FromNumpy` spelling stays a catalogue-deepen seam (the `Vt.Vec3fArray(list)` construction is the settled fallback). The `LayerFormat` `.usdc`/`.usda` suffix discrimination at `Export` is settled (the layer format is the file extension, not a parallel writer class).
- [USDZ_PACKAGE]: the USDZ packaging surface verifies against the folder `.api` catalogue for `usd-core` — `usd-core.md` `[03]-[ENTRYPOINTS]` carries `UsdUtils.CreateNewUsdzPackage(assetPath: Sdf.AssetPath, usdzFilePath: str, firstLayerName='', editLayersInPlace=False) -> bool` (the FIRST argument an `Sdf.AssetPath`, NOT a bare `str` — a raw-string call raises `Boost.Python.ArgumentError`), `CreateNewARKitUsdzPackage` (the ARKit/QuickLook-compliant build), `ExtractUsdzPackage(usdzFile, extractDir, recurse=False) -> bool`, `ComputeAllDependencies(layerPath: Sdf.AssetPath) -> tuple[list[Sdf.Layer], list[str], list[str]]`, and `ModifyAssetPaths(layer, modifyFn) -> None`, so the `package_usdz` close, the `Sdf.AssetPath(source)` first-argument wrap, the `ComputeAllDependencies` dependency-closure embed, and the `CreateNewARKitUsdzPackage` ARKit variant are settled fence code. The `usd-core.md` `[RAIL_LAW]` Reject row records the phantom spelling `UsdUtils.UsdUtilsCreateNewUsdzPackage` (the Python binding DROPS the `UsdUtils` C++ module prefix) — the real name is `UsdUtils.CreateNewUsdzPackage`, the settled fence spelling. The `pxr` package is COMPANION-GATED: `Requires-Python: >=3.9, <3.15` is a HARD upper cap (not merely a wheel gap), so `pxr` imports only behind the gated subprocess seam on the sub-3.13 worker, never at module scope on the cp315 core.
- [USD_EXPORT] [RESEARCH]: the VTK USD exporter `vtkmodules.vtkIOUSD.vtkUSDExporter` over `SetRenderWindow(window)`/`SetFileName(path)`/`Write()` for the `RenderExport` arm is NOT in the folder `.api` catalogue for `vtk` — the `vtk.md` `[02]-[PUBLIC_TYPES]` I/O table lists `vtkSTLReader`/`vtkSTLWriter`/`vtkPLYReader`/`vtkPLYWriter`/`vtkOBJReader`/`vtkXMLPolyDataReader`/`vtkXMLPolyDataWriter` under `vtkmodules.vtkIOGeometry`/`vtkIOPLY`/`vtkIOXML` and the `vtkWindowToImageFilter`/`vtkPNGWriter` capture pair, but carries no `vtkIOUSD` module nor any exporter class (the catalogued render-export surface is reader/writer pairs, not the renderer-scene exporter family). VTK 9.6 shipped the real `vtkUSDExporter` writing surface geometry, color-mapped surfaces, lights, camera, and PBR from a `vtkRenderWindow`; the `export_usd` body and the `RenderExport` arm stay a marked RESEARCH catalogue-deepen seam until a `vtkmodules.vtkIOUSD` reflection pass lands on the gated `python_version<'3.13'` band. License `BSD-3-Clause` recorded in the catalogue at admission. Close-condition: `.api` catalogue carries `vtkUSDExporter` with `SetRenderWindow`/`SetFileName`/`Write`.
- [RENDER_WINDOW] [RESEARCH]: the `Plotter.render_window` property feeding the `vtkUSDExporter.SetRenderWindow` argument is NOT in the folder `.api` catalogue for `pyvista` — the `pyvista.md` `[03]-[ENTRYPOINTS]` tables catalogue `Plotter(off_screen=...)`, `Plotter.add_mesh`, `Plotter.screenshot`, `Plotter.show`, and the `export_gltf`/`export_vrml`/`export_obj`/`export_html`/`import_*` scene-IO family, but no `render_window` accessor exposing the underlying `vtkRenderWindow` the USD exporter requires. The `scene/export#EXPORT` `USD`/`USDZ` arms hand `plotter.render_window` to `export_usd`; the access stays a marked RESEARCH catalogue-deepen seam until a `Plotter.render_window` reflection pass lands on the gated `python_version<'3.13'` band. Close-condition: `.api` catalogue carries `Plotter.render_window`.
