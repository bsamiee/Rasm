# [PY_ARTIFACTS_SCENE_STAGE]

The USD/USDZ stage-authoring owner. `StageOp` is the one Pixar OpenUSD source-axis family over the closed `RenderExport`/`MeshAuthor` cases — `RenderExport(render_window)` (the source-build-gated `vtkUSDExporter` writes the `scene/render#SCENE` rendered scene to a `.usdc`/`.usda` layer over the offscreen `Plotter.render_window`) and `MeshAuthor(scene)` (the wheel-available path: a `pxr.Usd.Stage` authored directly from the `scene/render#SCENE` `surface_arrays` `numpy` point/index buffer over the `UsdGeom`/`Vt`/`Gf` typed-prim schema family, bypassing the render pass) — each folding to a serialized USD layer through one total `apply` `match`, plus the `package_usdz` close that zips an authored `.usdc` (with its dependency-asset closure) into the `.usdz` AR/Apple deliverable. `MeshScene` is the rich authoring payload the `MeshAuthor` case carries: the `PrimKind` geometry (mesh/points/curves), the `UpAxis`/`meters_per_unit` AR-real-world metadata, the `SubdivScheme.NONE` triangulation gate (a render-exported mesh authors `none` or viewers re-subdivide under the `catmullClark` default), the `CreateExtentAttr` bounding box the USDZ QuickLook viewer frames on, the `displayColor` primvar, the `UsdShade.MaterialBindingAPI` PBR bind, and the `UsdSemantics.LabelsAPI` discipline/classification labels the AEC documentation plane rides. `LayerFormat` is the closed crate/ASCII `StrEnum` keyed by the file suffix `Stage.Export` writes (`.usdc` binary, `.usda` ASCII), never a parallel per-format stage writer.

The owner rides the wider worker band. OpenUSD declares a HARD `>=3.9, <3.15` upper cap and ships no cp315 wheel, so `pxr` imports neither on the runtime nor at module scope — the `lazy from pxr` bindings reify only when a worker function first runs INSIDE the `scene/render#SCENE` `anyio.to_process.run_sync` subprocess (the sub-3.13 native-VTK worker the render already crosses onto, the `usd-core` `<3.15` marker co-resolving with the tighter `vtk` floor). This owner authors and packages only — the offscreen render stays at `scene/render#SCENE`/`pyvista`/`vtk`, raw mesh-file interchange stays at `geometry/mesh`, and the `ArtifactReceipt.Scene` case is minted once at the `scene/render#SCENE` `Export` arm (no parallel receipt rail). The `scene/export#EXPORT` `USD`/`USDZ` `SceneTarget` arms delegate here through the wheel-available `author_mesh(MeshScene(prim=PrimKind.Mesh(*surface_arrays(grid))))` numpy authoring path — no render pass — then `package_usdz` for the `.usdz` close; `export_usd` (the `vtkUSDExporter` render-window front-half) is the source-build-gated enhancement the export arm reaches only on a VTK built against OpenUSD (the standard `vtk` wheel ships no `vtkmodules.vtkIOUSD`). This page closes the `USD_USDZ_SCENE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[STAGE]: the one `StageOp` closed source-axis family — `RenderExport`/`MeshAuthor` folding to a serialized USD layer through one total `apply` `match`; the `MeshScene` authoring payload over the `PrimKind` mesh/points/curves geometry (`UsdGeom.Mesh`/`Points`/`BasisCurves.Define` + the zero-copy `Vt.<Type>Array.FromNumpy` buffer bridge), the `SubdivScheme.NONE` triangulation gate, the `CreateExtentAttr` AR-framing box computed from the numpy point extremes, the `UsdGeom.PrimvarsAPI` `displayColor` primvar, the `Material`/`UsdShade.MaterialBindingAPI` `UsdPreviewSurface` bind, and the `UsdSemantics.LabelsAPI` taxonomy-label AEC plane; the `LayerFormat` crate/ASCII `StrEnum` keyed by suffix; the `UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage` packaging close over the dependency-asset closure, the `UsdUtils.ExtractUsdzPackage` import inverse, and the `UsdUtils.ComputeAllDependencies`/`ModifyAssetPaths` external-asset relocation; the `UsdUtils.ComputeUsdStageStats` prim-count gate; the `vtkmodules.vtkIOUSD.vtkUSDExporter` render-to-layer front-half carried as the source-build-gated [03]-[RESEARCH] seam. `usd-core` `pxr.{Usd,UsdGeom,UsdShade,UsdSemantics,Sdf,Gf,Vt,Tf,UsdUtils}` settled against the folder `.api` (reflected against `usd-core 26.5` cp313).

## [02]-[STAGE]

- Cases: `StageOp` cases — `RenderExport(render_window)` (the `vtkmodules.vtkIOUSD.vtkUSDExporter` writes the `scene/render#SCENE` rendered scene — surface geometry, color-mapped surfaces, lights, camera, PBR — to a `.usdc`/`.usda` layer over `SetRenderWindow(render_window)`/`SetFileName(path)`/`Write()`, the `Plotter.render_window` the `scene/export#EXPORT` `USD`/`USDZ` arm hands across; the rendered-scene-to-layer half, source-build-gated because the official `vtk` wheel ships no `vtkIOUSD`) · `MeshAuthor(scene)` (a `Usd.Stage.CreateInMemory()` authored directly from the `MeshScene` payload's `numpy` point/index buffers — `PrimKind.define` folds the geometry into `UsdGeom.Mesh`/`Points`/`BasisCurves.Define` + `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(...))` and its `FaceVertexCounts`/`Indices`/`CurveVertexCounts`/`Normals`/`Widths` siblings, `CreateSubdivisionSchemeAttr(none)`, and `CreateExtentAttr`, then `MeshScene.author` folds the `displayColor` primvar, the `MaterialBindingAPI` PBR bind, the `UsdSemantics.LabelsAPI` taxonomy labels, and the `SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim` stage metadata — the pure-geometry USD export `author_mesh` exposes when no render is needed, the wheel-available default path) — matched by one total `match`/`case`, the layer serialized to the `LayerFormat`-suffixed path, the modality recovered from the `StageOp` discriminant. The USDZ packaging is not a third case but the `package_usdz` close over either arm's written `.usdc`: `CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` gathers the layer's full dependency-asset closure and writes the aligned-zip `.usdz`, with `CreateNewARKitUsdzPackage` the Apple QuickLook-constrained variant as the `UsdzProfile.ARKIT` policy value on the same close, never a per-platform package owner; `extract_usdz` is the `ExtractUsdzPackage` import inverse, and `relocate_assets` the `ComputeAllDependencies`+`ModifyAssetPaths` external-asset relocation before packaging.
- Auto: the `RenderExport` arm reads the `Plotter.render_window` directly — the VTK exporter writes whatever the offscreen render composed, so a USD export carries the same scene state the image render would; the `MeshAuthor` arm folds each numpy buffer into ONE `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(points))`/`CreateFaceVertexIndicesAttr(Vt.IntArray.FromNumpy(faces.reshape(-1)))` `Set` over the zero-copy `FromNumpy` bridge (the bare `Vt.Vec3fArray(ndarray)` ctor raises `TypeError`, no registered converter), never a per-vertex append nor a `.tolist()` Python-list copy; `CreateSubdivisionSchemeAttr(UsdGeom.Tokens.none)` marks the triangulation non-subdivision-surface so a viewer does not re-subdivide it under the `catmullClark` default (a real render-export correctness fix); `CreateExtentAttr([min, max])` authors the bounding box `Vt.Vec3fArray.FromNumpy(np.stack([pts.min(0), pts.max(0)]))` computes from the admitted buffer once (the extent the USDZ AR/QuickLook viewer frames the model on, a `BBoxCache` traversal for a composed hierarchy but the direct numpy extremes for the single authored prim); `PrimvarsAPI(prim).CreatePrimvar("displayColor", Color3fArray, vertex|constant)` carries a scalar-mapped or per-vertex colour so a coloured surface keeps its colour on the deliverable; `MaterialBindingAPI.Apply(prim).Bind(material)` attaches the `UsdPreviewSurface` PBR material `Material.bind` authors; `UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(labels))` writes the discipline/classification/sheet-type labels the AEC documentation plane owns onto the exported geometry (resolved across inheritance via `LabelsQuery.ComputeUniqueInheritedLabels`); `ComputeUsdStageStats(stage)["totalPrimCount"]` gates the authored stage before export so a zero-prim authoring rails rather than writing an empty layer; `SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim(prim)` set the AR/real-world stage metadata; the `LayerFormat` suffix selects crate-binary vs ASCII at `Export`; the `package_usdz` close hands the authored `.usdc` to `CreateNewUsdzPackage`, which itself gathers and embeds the full dependency-asset closure, so a scene with referenced textures packages a complete self-contained `.usdz` rather than one with dangling references.
- Receipt: each USD/USDZ export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes` — the content key the `_emit` arm derives, the `SceneTarget` value (`"usdc"`/`"usdz"`) the target string, the serialized USD-layer / packaged-USDZ byte count the evidence slot, and the `facts` `frozendict` band the stage stats fill; this owner contributes that serialized payload, never a parallel per-format receipt rail. The `ComputeUsdStageStats` `totalPrimCount`/`usedLayerCount`, the up-axis, and the meters-per-unit are COMPUTED now (the prim-count gates the empty-stage fault) and thread back to the receipt as the `apply` `(bytes, facts)` return — the `mesh_author` arm folds them into the `frozendict` band the `scene/export#EXPORT` `render_export` propagates to the `scene/render#SCENE` `_emit` `Export` arm, which mints them onto the widened `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` band, exactly as the `scene/render#SCENE` window evidence fills it.
- Growth: a new USD prim schema (`Xform` placement, `Camera` viewpoint, `PointInstancer` for repeated geometry, `Subset` per-material face region) is one `PrimKind` case plus one `define` arm or one `UsdGeom.<Schema>.Define` call in `MeshScene.author`; a new material knob is one `Material` field bound into `bind`; a new semantic taxonomy is one `MeshScene.labels` key; a new layer format is one `LayerFormat` row keyed by suffix; a new packaging policy (ARKit-constrained) is one `UsdzProfile` row on the `package_usdz` close; a new stage source is one `StageOp` case folding into the `apply` `match` (the `extract_usdz` USD-import round-trip is a `scene/render#SCENE` `SceneOp` case reusing this dispatch); zero new owner — the source axis stays two cases (`RenderExport`/`MeshAuthor`) on one family, the packaging one close, every addition a schema call, row, case, field, or data choice.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable, Mapping
from enum import StrEnum
from pathlib import Path
from typing import Literal, assert_never

import numpy as np
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

lazy from pxr import Gf, Sdf, Usd, UsdGeom, UsdSemantics, UsdShade, UsdUtils, Vt
lazy from vtkmodules.vtkIOUSD import vtkUSDExporter  # source-build-gated: the official vtk wheel ships no vtkIOUSD

# --- [TYPES] ---------------------------------------------------------------------------

type Vec3 = tuple[float, float, float]
type PrimKindTag = Literal["mesh", "points", "curves"]
type StageOpTag = Literal["render_export", "mesh_author"]

# --- [MODELS] --------------------------------------------------------------------------


class LayerFormat(StrEnum):
    CRATE = "usdc"  # crate binary
    ASCII = "usda"  # human-readable ASCII (diff/inspect); Stage.Export discriminates by suffix natively

    @staticmethod
    def of_suffix(path: str) -> "LayerFormat":
        for fmt in LayerFormat:
            if path.endswith(f".{fmt.value}"):
                return fmt
        raise ValueError(f"not a USD layer suffix: {path!r}")  # a non-.usdc/.usda path to the USD arm is a fault


class UpAxis(StrEnum):
    Y = "y"
    Z = "z"

    @property
    def token(self) -> object:
        return UsdGeom.Tokens.y if self is UpAxis.Y else UsdGeom.Tokens.z  # resolved inside the worker only


class SubdivScheme(StrEnum):
    NONE = "none"  # a render-exported triangulation authors `none` so viewers do NOT re-subdivide it
    CATMULL_CLARK = "catmullClark"

    @property
    def token(self) -> object:
        return getattr(UsdGeom.Tokens, self.value)  # the StrEnum value IS the USD subdivision token


class UsdzProfile(StrEnum):
    STANDARD = "CreateNewUsdzPackage"    # general USDZ
    ARKIT = "CreateNewARKitUsdzPackage"  # Apple QuickLook-constrained; the StrEnum value IS the UsdUtils packager name


class Material(Struct, frozen=True, kw_only=True):
    diffuse: Vec3 = (0.18, 0.18, 0.18)
    metallic: float = 0.0
    roughness: float = 0.5
    opacity: float = 1.0

    def bind(self, stage: object, prim: object, path: object) -> None:
        material = UsdShade.Material.Define(stage, path)
        shader = UsdShade.Shader.Define(stage, path.AppendChild("PreviewSurface"))
        shader.CreateIdAttr("UsdPreviewSurface")
        shader.CreateInput("diffuseColor", Sdf.ValueTypeNames.Color3f).Set(Gf.Vec3f(*self.diffuse))
        shader.CreateInput("metallic", Sdf.ValueTypeNames.Float).Set(self.metallic)
        shader.CreateInput("roughness", Sdf.ValueTypeNames.Float).Set(self.roughness)
        shader.CreateInput("opacity", Sdf.ValueTypeNames.Float).Set(self.opacity)
        material.CreateSurfaceOutput().ConnectToSource(shader.ConnectableAPI(), "surface")
        UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)  # the actual attach, distinct from defining the material


@tagged_union(frozen=True)
class PrimKind:
    tag: PrimKindTag = tag()
    mesh: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None] = case()
    points: tuple[NDArray[np.float32], NDArray[np.float32] | None] = case()
    curves: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None] = case()

    @staticmethod
    def Mesh(points: NDArray[np.float32], faces: NDArray[np.int32], normals: NDArray[np.float32] | None = None) -> "PrimKind":
        return PrimKind(mesh=(points, faces, normals))

    @staticmethod
    def Points(coords: NDArray[np.float32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        return PrimKind(points=(coords, widths))

    @staticmethod
    def Curves(points: NDArray[np.float32], vertex_counts: NDArray[np.int32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        return PrimKind(curves=(points, vertex_counts, widths))

    def define(self, stage: object, path: object, subdiv: SubdivScheme) -> object:
        match self:
            case PrimKind(tag="mesh", mesh=(verts, faces, normals)):
                mesh = UsdGeom.Mesh.Define(stage, path)
                mesh.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(verts))
                mesh.CreateFaceVertexCountsAttr(Vt.IntArray.FromNumpy(np.full(faces.shape[0], faces.shape[1], dtype=np.int32)))
                mesh.CreateFaceVertexIndicesAttr(Vt.IntArray.FromNumpy(faces.reshape(-1)))
                mesh.CreateSubdivisionSchemeAttr(subdiv.token)  # `none` for a triangulation: viewers do not re-subdivide
                if normals is not None:
                    mesh.CreateNormalsAttr(Vt.Vec3fArray.FromNumpy(normals))
                mesh.CreateExtentAttr(_extent(verts))
                return mesh.GetPrim()
            case PrimKind(tag="points", points=(coords, widths)):
                cloud = UsdGeom.Points.Define(stage, path)
                cloud.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(coords))
                if widths is not None:
                    cloud.CreateWidthsAttr(Vt.FloatArray.FromNumpy(widths))
                cloud.CreateExtentAttr(_extent(coords))
                return cloud.GetPrim()
            case PrimKind(tag="curves", curves=(pts, counts, widths)):
                curve = UsdGeom.BasisCurves.Define(stage, path)  # drawing-edge / linework geometry
                curve.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(pts))
                curve.CreateCurveVertexCountsAttr(Vt.IntArray.FromNumpy(counts))
                curve.CreateTypeAttr(UsdGeom.Tokens.linear)
                if widths is not None:
                    curve.CreateWidthsAttr(Vt.FloatArray.FromNumpy(widths))
                curve.CreateExtentAttr(_extent(pts))
                return curve.GetPrim()
            case _:
                assert_never(self)


class MeshScene(Struct, frozen=True, kw_only=True):
    prim: PrimKind
    up_axis: UpAxis = UpAxis.Z
    meters_per_unit: float = 1.0
    subdiv: SubdivScheme = SubdivScheme.NONE
    display_color: NDArray[np.float32] | None = None
    material: Material | None = None
    labels: Mapping[str, tuple[str, ...]] = {}  # taxonomy -> semantic labels; the AEC discipline/classification plane

    def author(self, stage: object) -> object:
        UsdGeom.SetStageUpAxis(stage, self.up_axis.token)
        UsdGeom.SetStageMetersPerUnit(stage, self.meters_per_unit)
        prim = self.prim.define(stage, Sdf.Path("/Root"), self.subdiv)
        if self.display_color is not None:
            colors = np.atleast_2d(self.display_color)
            interp = UsdGeom.Tokens.vertex if self.display_color.ndim == 2 else UsdGeom.Tokens.constant
            UsdGeom.PrimvarsAPI(prim).CreatePrimvar("displayColor", Sdf.ValueTypeNames.Color3fArray, interp).Set(Vt.Vec3fArray.FromNumpy(colors))
        if self.material is not None:
            self.material.bind(stage, prim, Sdf.Path("/Root/Material"))
        for taxonomy, labels in self.labels.items():
            UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(list(labels)))
        stage.SetDefaultPrim(prim)  # a top-level prim is a valid USDZ default prim
        return prim


@tagged_union(frozen=True)
class StageOp:
    tag: StageOpTag = tag()
    render_export: object = case()  # the live vtk render_window (source-build-gated vtkUSDExporter front-half)
    mesh_author: MeshScene = case()  # the wheel-available numpy pxr authoring path

    @staticmethod
    def RenderExport(render_window: object) -> "StageOp":
        return StageOp(render_export=render_window)

    @staticmethod
    def MeshAuthor(scene: MeshScene) -> "StageOp":
        return StageOp(mesh_author=scene)

    def apply(self, path: str) -> tuple[bytes, frozendict[str, float | str]]:
        LayerFormat.of_suffix(path)  # boundary guard: validate the crate/ASCII suffix Stage.Export writes by
        match self:
            case StageOp(tag="render_export", render_export=render_window):
                exporter = vtkUSDExporter()
                exporter.SetRenderWindow(render_window)
                exporter.SetFileName(path)
                exporter.Write()
                return Path(path).read_bytes(), frozendict()  # the VTK layer carries no in-process stage handle; the render-window path leaves the receipt band empty
            case StageOp(tag="mesh_author", mesh_author=scene):
                stage = Usd.Stage.CreateInMemory(path)
                scene.author(stage)
                stats = UsdUtils.ComputeUsdStageStats(stage)  # single-arg form -> {"totalPrimCount", "usedLayerCount", ...}
                if not stats["totalPrimCount"]:  # Exemption: an empty authoring rails at the native boundary the worker seam converts
                    raise ValueError("empty stage: zero prims authored")
                stage.Export(path)
                facts: frozendict[str, float | str] = frozendict({
                    "prims": int(stats["totalPrimCount"]), "layers": int(stats["usedLayerCount"]),
                    "up_axis": scene.up_axis.value, "meters_per_unit": scene.meters_per_unit,
                })  # the authored-prim evidence the `core/receipt#RECEIPT` `ArtifactReceipt.Scene` band carries, threaded back through the export worker
                return Path(path).read_bytes(), facts
            case _:
                assert_never(self)

# --- [OPERATIONS] ----------------------------------------------------------------------


def _extent(points: NDArray[np.float32]) -> object:
    return Vt.Vec3fArray.FromNumpy(np.stack([points.min(axis=0), points.max(axis=0)]))  # the [min, max] AR-framing box


def export_usd(render_window: object, path: str) -> tuple[bytes, frozendict[str, float | str]]:
    return StageOp.RenderExport(render_window).apply(path)


def author_mesh(scene: MeshScene, path: str) -> tuple[bytes, frozendict[str, float | str]]:
    return StageOp.MeshAuthor(scene).apply(path)


def package_usdz(source: str, sink: str, profile: UsdzProfile = UsdzProfile.STANDARD) -> bool:
    return getattr(UsdUtils, profile.value)(Sdf.AssetPath(source), sink)  # profile.value IS the UsdUtils packager name; gathers + embeds the full dependency-asset closure itself


def extract_usdz(package: str, into: str) -> bool:
    return UsdUtils.ExtractUsdzPackage(package, into, True, False, True)  # recurse, quiet, force — the packaging inverse


def relocate_assets(layer_path: str, rewrite: Callable[[str], str]) -> tuple[int, int]:
    layer = Sdf.Layer.FindOrOpen(layer_path)
    UsdUtils.ModifyAssetPaths(layer, rewrite)  # rewrite every asset path (relocation before packaging)
    _layers, references, assets = UsdUtils.ComputeAllDependencies(Sdf.AssetPath(layer_path))
    return len(references), len(assets)  # the relocated-edge counts; CreateNewUsdzPackage embeds the closure, so this is the audit/relocation path only
```

The `StageOp.apply` arms and the `export_usd`/`author_mesh`/`package_usdz`/`extract_usdz`/`relocate_assets` module-level functions run INSIDE the `scene/render#SCENE` `to_process.run_sync` worker (the `scene/export#EXPORT` `render_export` calls them synchronously, itself offloaded once by the `scene/render#SCENE` `_emit` `Export` arm) — the `lazy from pxr`/`lazy from vtkmodules.vtkIOUSD` bindings reify only there, so the `pxr` Boost.Python extension never loads on the 3.15 runtime nor at module scope, and stage.md mints no worker seam of its own. `Vt.Vec3fArray.FromNumpy(points)`/`Vt.IntArray.FromNumpy(faces.reshape(-1))`/`Vt.FloatArray.FromNumpy(widths)` fold the numpy buffers zero-copy into the typed USD arrays (`Vt.TokenArray(list(labels))` for the label tokens, which have no `FromNumpy`); the `package_usdz` close hands the `.usdc` to the packager the `UsdzProfile` policy value names (`ARKIT` selecting `CreateNewARKitUsdzPackage`, `STANDARD` the general `CreateNewUsdzPackage`). The `pxr` `Tf.ErrorException` (missing asset, malformed layer, write failure), the `Boost.Python.ArgumentError` (a wrong-typed argument, the `Sdf.AssetPath`-vs-`str` mismatch), and the empty-stage `ValueError` guard all map at the `scene/render#SCENE` `async_boundary` worker edge into the `runtime` `RuntimeRail` fault, never surfaced bare across the interpreter seam, and a `bool False` return from `CreateNewUsdzPackage`/`ExtractUsdzPackage` is a packaging-failure rail the `bool` return carries.

## [03]-[RESEARCH]

- [MESH_AUTHOR] [RESOLVED]: the wheel-available primary path — `Usd.Stage.CreateInMemory()` then `UsdGeom.Mesh`/`Points`/`BasisCurves.Define(stage, Sdf.Path("/Root"))` per `PrimKind` case, then `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(buf))` and its `FaceVertexCounts`/`FaceVertexIndices`/`CurveVertexCounts`/`Normals`/`Widths` siblings, then `CreateSubdivisionSchemeAttr(UsdGeom.Tokens.none)`/`CreateExtentAttr`, then `SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim`, then `Export(path)` — verify against the folder `.api/usd-core.md` entrypoint rows `[01]`-`[16]` and public-type rows. The zero-copy `Vt.<Type>Array.FromNumpy(ndarray)` bridge is the settled buffer path (the bare `Vt.<Type>Array(ndarray)` ctor raises `TypeError`, no registered converter); `FromNumpy` widens dtype (float64 `(N,3)` -> `Vec3fArray`). `Vt.TokenArray` has NO `FromNumpy`, so the label tokens construct from a Python list. The `CreateSubdivisionSchemeAttr(none)` triangulation gate and the `CreateExtentAttr([min,max])` AR-framing box are the two render-export correctness fixes the catalogue flags — a triangulation without `none` re-subdivides under the `catmullClark` default and a `.usdz` without extent frames poorly in QuickLook.
- [MATERIAL_AND_SEMANTIC] [RESOLVED]: the `Material.bind` PBR graph — `UsdShade.Material.Define` + `UsdShade.Shader.Define` (the `UsdPreviewSurface` node via `CreateIdAttr("UsdPreviewSurface")`/`CreateInput`) wired through `ConnectableAPI`, then attached through `UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)` — verify against `.api/usd-core.md` rows `[12]`-`[14]` and entrypoint `[10]`; the binding API is the actual attach, distinct from defining the material, and a per-material face region is a `UsdGeom.Subset` the binding targets (one mesh, N material regions, the growth edge). The `UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(labels))` semantic-label plane — rows `[15]`/`[16]` and entrypoint `[11]` — travels the AEC discipline/classification/sheet-type labels ON the prim (resolved across inheritance via `LabelsQuery.ComputeUniqueInheritedLabels`), the documentation-telos plane the `drawing/`+`specification/` classification vocabularies feed as the `MeshScene.labels` ingress, never an out-of-band sidecar.
- [PACKAGING] [RESOLVED]: `CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` is the single USDZ builder the `package_usdz` close calls — it gathers the root layer plus its full dependency-asset closure ITSELF and writes the aligned-zip `.usdz`, returning `bool` — verify against `.api/usd-core.md` entrypoint rows `[01]`-`[06]`. Because the closure is embedded internally, a bare `ComputeAllDependencies` before packaging embeds nothing and is dead; `ComputeAllDependencies`/`ModifyAssetPaths` (the `relocate_assets` audit/relocation path) rewrite asset paths BEFORE packaging for the external-asset case. `CreateNewARKitUsdzPackage` is the ARKit/QuickLook-constrained variant the `UsdzProfile.ARKIT` policy value selects; `ExtractUsdzPackage(usdz, dir, recurse, verbose, force)` (5 positionals) is the import inverse. `ComputeUsdStageStats(stage)` returns the `totalPrimCount`/`usedLayerCount` dict (single-arg form, NOT the legacy `(path, outDict)` out-param), computed NOW to gate the empty-stage fault AND folded into the `apply` `(bytes, facts)` return that threads the prim/layer/up-axis/meters-per-unit band onto the widened `core/receipt#RECEIPT` `ArtifactReceipt.Scene` `facts` slot.
- [RENDER_EXPORT] [RESEARCH]: the source-build-gated front half — `vtk` `vtkmodules.vtkIOUSD.vtkUSDExporter` (`SetRenderWindow(Plotter.render_window)`/`SetFileName`/`Write`) writing the rendered scene to a `.usdc` layer, which `package_usdz` then packages — is present ONLY on a VTK source-built against OpenUSD (the official wheel ships no `vtkmodules.vtkIOUSD`, `.api/vtk.md`), so the `lazy from vtkmodules.vtkIOUSD import vtkUSDExporter` binding reifies only on that build and `MeshAuthor`'s numpy path is the wheel-available default the `scene/export#EXPORT` USD/USDZ arm should prefer. When present, `vtk` owns render-to-layer and `usd-core` owns layer-to-package.
