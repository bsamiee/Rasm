# [PY_ARTIFACTS_SCENE_STAGE]

The USD/USDZ stage-authoring owner. `StageOp` is the one Pixar OpenUSD source-axis family over the closed `RenderExport`/`MeshAuthor` cases — `RenderExport(render_window)` (the source-build-gated `vtkUSDExporter` writes the `scene/render#SCENE` rendered scene to a `.usdc`/`.usda` layer over the offscreen `Plotter.render_window`) and `MeshAuthor(scene)` (the wheel-available path: a `pxr.Usd.Stage` authored directly from the `scene/render#SCENE` `surface_arrays` `numpy` point/index/color buffers over the `UsdGeom`/`Vt`/`Gf` typed-prim schema family, bypassing the render pass) — each folding to a serialized USD layer through one total `apply` `match`, plus the `package_usdz` close that zips an authored `.usdc` (with its dependency-asset closure) into the `.usdz` AR/Apple deliverable. `MeshScene` is the rich authoring payload the `MeshAuthor` case carries: the `PrimKind` scene-graph (a `mesh`/`points`/`curves` leaf OR an `xform` group of placed `PrimNode` children OR a `PointInstancer` array OR a `Camera`), the root `transform` op stack (`translate`/`rotate`/`scale`/`orient`/`transform`), the `UpAxis`/`meters_per_unit` AR-real-world metadata, the `SubdivScheme.NONE` triangulation gate (a render-exported mesh authors `none` or viewers re-subdivide under the `catmullClark` default), the `CreateExtentAttr` per-prim bounding box the USDZ QuickLook viewer frames on, the `displayColor` primvar, the `UsdShade.MaterialBindingAPI` PBR bind (flat OR textured `UsdPreviewSurface`), the `UsdSemantics.LabelsAPI` discipline/classification labels the AEC documentation plane rides, the `Usd.ModelAPI` `Kind` scene-as-asset marking (`component`/`assembly`) for BIM/USD-pipeline interop, and the `Sdf.Reference` composition arcs a shared component/detail library rides. `LayerFormat` is the closed crate/ASCII `StrEnum` keyed by the file suffix `Stage.Export` writes (`.usdc` binary, `.usda` ASCII), never a parallel per-format stage writer.

`PrimKind` is a real scene-graph, not a single prim: the recursive `xform` case authors a `UsdGeom.Xform` group whose ordered `XformOp` stack places it and whose `PrimNode` children each carry their own placement, geometry, and per-element `displayColor`/`Material`/`labels` — the AEC assembly-of-positioned-components case; the `instancer` case authors a `UsdGeom.PointInstancer` (one prototype defined once, placed N times by the per-instance `positions`/`proto_indices`/`scales`/`orientations` arrays fed zero-copy through `Vt.<Type>Array.FromNumpy`) — the columns/bolts/panels/fixtures array-of-identical-elements win; the `camera` case authors a `UsdGeom.Camera` from a `Lens` look-at so the deliverable frames correctly in QuickLook/AR; and a `mesh` leaf carries `FaceSubset` regions each authoring a `UsdGeom.Subset` the `MaterialBindingAPI` targets — one mesh, N per-face material regions, never N meshes. The aggregate world extent of a composed hierarchy is read once through `UsdGeom.BBoxCache.ComputeWorldBound`, threaded onto the receipt as the AR-framing diagonal.

The owner rides the wider worker band. OpenUSD declares a HARD `>=3.9, <3.15` upper cap and ships no cp315 wheel, so `pxr` imports neither on the runtime nor at module scope — the `lazy from pxr` bindings reify only when a worker function first runs INSIDE the `scene/render#SCENE` `anyio.to_process.run_sync` subprocess (the sub-3.13 native-VTK worker the render already crosses onto, the `usd-core` `<3.15` marker co-resolving with the tighter `vtk` worker floor). This owner authors and packages only — the offscreen render stays at `scene/render#SCENE`/`pyvista`/`vtk`, raw mesh-file interchange stays at `geometry/mesh`, and the `ArtifactReceipt.Scene` case is minted once at the `scene/render#SCENE` `Export` arm (no parallel receipt rail). The `scene/export#EXPORT` `USD`/`USDZ` `SceneTarget` arms delegate here through the wheel-available `author_mesh(MeshScene(...))` numpy authoring path — no render pass — threading the render spec's mapped per-vertex `display_color`, its PBR `Material`, and its AEC `labels` so the deliverable carries colour+material+classification rather than a bare grey mesh; `package_usdz` then closes the `.usdz`; `export_usd` (the `vtkUSDExporter` render-window front-half) is the source-build-gated enhancement the export arm reaches only on a VTK built against OpenUSD (the standard `vtk` wheel ships no `vtkmodules.vtkIOUSD`). This page closes the `USD_USDZ_SCENE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[STAGE]: the one `StageOp` closed source-axis family — `RenderExport`/`MeshAuthor` folding to a serialized USD layer through one total `apply` `match`; the `MeshScene` authoring payload over the recursive `PrimKind` scene-graph (`mesh`/`points`/`curves` leaves via `UsdGeom.Mesh`/`Points`/`BasisCurves.Define` + the zero-copy `Vt.<Type>Array.FromNumpy` buffer bridge, the `xform` group via `UsdGeom.Xform.Define` + the `XformOp` `AddTranslateOp`/`AddRotateXYZOp`/`AddScaleOp`/`AddOrientOp`/`AddTransformOp` op stack over recursive `PrimNode` children, the `instancer` array via `UsdGeom.PointInstancer.Define` + `CreatePrototypesRel`/`CreateProtoIndicesAttr`/`CreatePositionsAttr`/`CreateScalesAttr`/`CreateOrientationsAttr`, and the `camera` via `UsdGeom.Camera.Define` + `CreateProjectionAttr`/`CreateFocalLengthAttr`/`CreateClippingRangeAttr`), the `SubdivScheme.NONE` triangulation gate, the per-prim `CreateExtentAttr` AR-framing box plus the `UsdGeom.BBoxCache.ComputeWorldBound` aggregate-diagonal for a composed hierarchy, the `UsdGeom.PrimvarsAPI` `displayColor` primvar, the `Material`/`UsdShade.MaterialBindingAPI` `UsdPreviewSurface` bind (flat `Color3f` OR a textured `UsdUVTexture`->`UsdPrimvarReader_float2('st')` graph, and a `UsdGeom.Subset`-targeted bind for a multi-material mesh), the `UsdSemantics.LabelsAPI` taxonomy-label AEC plane, the `Usd.ModelAPI.SetKind` (`Kind.Tokens.component`/`assembly`) scene-as-asset marking, and the `Usd.Prim.GetReferences().AddReference(Sdf.Reference(...))` composition arc; the `LayerFormat` crate/ASCII `StrEnum` keyed by suffix; the `UsdUtils.CreateNewUsdzPackage`/`CreateNewARKitUsdzPackage` packaging close over the dependency-asset closure, the `UsdUtils.ExtractUsdzPackage` import inverse, and the `UsdUtils.ComputeAllDependencies`/`ModifyAssetPaths` external-asset relocation; the `UsdUtils.ComputeUsdStageStats` prim-count gate; the `vtkmodules.vtkIOUSD.vtkUSDExporter` render-to-layer front-half carried as the source-build-gated [03]-[RESEARCH] seam. `usd-core` `pxr.{Usd,UsdGeom,UsdShade,UsdSemantics,Sdf,Gf,Vt,Tf,Kind,UsdUtils}` settled against the folder `.api` (reflected against `usd-core 26.5` cp313).

## [02]-[STAGE]

- Cases: `StageOp` cases — `RenderExport(render_window)` (the `vtkmodules.vtkIOUSD.vtkUSDExporter` writes the `scene/render#SCENE` rendered scene — surface geometry, color-mapped surfaces, lights, camera, PBR — to a `.usdc`/`.usda` layer over `SetRenderWindow(render_window)`/`SetFileName(path)`/`Write()`, the `Plotter.render_window` the `scene/export#EXPORT` `USD`/`USDZ` arm hands across; the rendered-scene-to-layer half, source-build-gated because the official `vtk` wheel ships no `vtkIOUSD`) · `MeshAuthor(scene)` (a `Usd.Stage.CreateInMemory()` authored directly from the `MeshScene` payload — `PrimKind.define` folds each geometry leaf into `UsdGeom.Mesh`/`Points`/`BasisCurves.Define` + `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(...))` and its `FaceVertexCounts`/`Indices`/`CurveVertexCounts`/`Normals`/`Widths` siblings, `CreateSubdivisionSchemeAttr(none)`, and `CreateExtentAttr`; folds the `xform` group into `UsdGeom.Xform.Define` + the `XformOp` op stack over recursive `PrimNode` children; folds the `instancer` case into `UsdGeom.PointInstancer.Define` + the per-instance `Vt` arrays; and folds the `camera` case into `UsdGeom.Camera.Define` + a look-at `AddTransformOp` — then `_dress` folds the `displayColor` primvar, the `MaterialBindingAPI` PBR bind, the `UsdSemantics.LabelsAPI` taxonomy labels, and `MeshScene.author` folds the `SetStageUpAxis`/`SetStageMetersPerUnit`/`Usd.ModelAPI.SetKind`/`GetReferences().AddReference`/`SetDefaultPrim` stage metadata — the wheel-available default path the `scene/export#EXPORT` USD arm always takes) — matched by one total `match`/`case`, the layer serialized to the `LayerFormat`-suffixed path, the modality recovered from the `StageOp` discriminant. The USDZ packaging is not a third case but the `package_usdz` close over either arm's written `.usdc`: `CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` gathers the layer's full dependency-asset closure and writes the aligned-zip `.usdz`, with `CreateNewARKitUsdzPackage` the Apple QuickLook-constrained variant as the `UsdzProfile.ARKIT` policy value on the same close, never a per-platform package owner; `extract_usdz` is the `ExtractUsdzPackage` import inverse, and `relocate_assets` the `ComputeAllDependencies`+`ModifyAssetPaths` external-asset relocation before packaging.
- Auto: the `RenderExport` arm reads the `Plotter.render_window` directly — the VTK exporter writes whatever the offscreen render composed, so a USD export carries the same scene state the image render would; the `MeshAuthor` arm folds each numpy buffer into ONE `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(points))`/`CreateFaceVertexIndicesAttr(Vt.IntArray.FromNumpy(faces.reshape(-1)))` `Set` over the zero-copy `FromNumpy` bridge (the bare `Vt.Vec3fArray(ndarray)` ctor raises `TypeError`, no registered converter), never a per-vertex append nor a `.tolist()` Python-list copy; `CreateSubdivisionSchemeAttr(UsdGeom.Tokens.none)` marks the triangulation non-subdivision-surface so a viewer does not re-subdivide it under the `catmullClark` default (a real render-export correctness fix); `CreateExtentAttr([min, max])` authors each Boundable prim's box from its own buffer once, and a composed hierarchy's aggregate world box reads through `UsdGeom.BBoxCache(Usd.TimeCode.Default(), [UsdGeom.Tokens.default_]).ComputeWorldBound(prim).ComputeAlignedRange()` (the `Range3d.IsEmpty()`-guarded diagonal threaded onto the receipt as the AR-framing evidence a single-prim numpy extreme cannot span). The `xform` group folds its `XformOp` stack onto `AddTranslateOp`/`AddRotateXYZOp`/`AddScaleOp`/`AddOrientOp`/`AddTransformOp` and recurses into each `PrimNode`, so a multi-element placed scene (an AEC assembly of positioned components) is one `PrimKind.Group` payload; the `instancer` case defines the prototype once under a `UsdGeom.Scope`, wires `CreatePrototypesRel().AddTarget(proto)`, and authors `CreateProtoIndicesAttr`/`CreatePositionsAttr`/`CreateScalesAttr`/`CreateOrientationsAttr` from the per-instance `Vt` arrays, so N identical elements author one prototype and N transforms rather than N meshes; the `camera` case authors `CreateProjectionAttr(perspective)`/`CreateFocalLengthAttr`/`CreateClippingRangeAttr` and a look-at `AddTransformOp` so the deliverable frames its subject; `PrimvarsAPI(prim).CreatePrimvar("displayColor", Color3fArray, vertex|constant)` carries the render's scalar-mapped per-vertex colour so a coloured surface keeps its colour on the deliverable; `MaterialBindingAPI.Apply(prim).Bind(material)` attaches the flat-or-textured `UsdPreviewSurface` PBR material `Material.bind` authors, and a `FaceSubset` binds it to a `UsdGeom.Subset` region so one mesh carries N materials; `UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(labels))` writes the discipline/classification/sheet-type labels the AEC documentation plane owns onto the exported geometry (resolved across inheritance via `LabelsQuery.ComputeUniqueInheritedLabels`); `Usd.ModelAPI(prim).SetKind(Kind.Tokens.component)` marks the scene as a BIM/USD-pipeline asset and `GetReferences().AddReference(Sdf.Reference(asset))` composes a shared component/detail library into the root; `ComputeUsdStageStats(stage)["totalPrimCount"]` gates the authored stage before export so a zero-prim authoring rails rather than writing an empty layer; the `LayerFormat` suffix selects crate-binary vs ASCII at `Export`; the `package_usdz` close hands the authored `.usdc` to `CreateNewUsdzPackage`, which itself gathers and embeds the full dependency-asset closure, so a scene with referenced textures/components packages a complete self-contained `.usdz` rather than one with dangling references.
- Receipt: each USD/USDZ export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes` — the content key the `_emit` arm derives, the `SceneTarget` value (`"usdc"`/`"usdz"`) the target string, the serialized USD-layer / packaged-USDZ byte count the evidence slot, and the `facts` `frozendict` band the stage stats fill; this owner contributes that serialized payload, never a parallel per-format receipt rail. The `ComputeUsdStageStats` `totalPrimCount`/`usedLayerCount`, the up-axis, the meters-per-unit, and the `BBoxCache` aggregate `extent_diag` are COMPUTED now (the prim-count gates the empty-stage fault) and thread back to the receipt as the `apply` `(bytes, facts)` return — the `mesh_author` arm folds them into the `frozendict[str, float | str]` band the `scene/export#EXPORT` `render_export` propagates to the `scene/render#SCENE` `_emit` `Export` arm, which mints them onto the widened `ArtifactReceipt.Scene` band (a new fact is one band key, never a new receipt case), exactly as the `scene/render#SCENE` window evidence fills it.
- Growth: a new USD prim schema is one `PrimKind` case plus one `define` arm (the `xform` placement, `instancer` repeated geometry, and `camera` viewpoint are exactly that growth, each a case, a factory, and a `define` arm); a per-element placement is one `XformOp` case; a multi-material region is one `FaceSubset` on the `mesh` payload; a per-element appearance is one `PrimNode` field; a new material knob is one `Material` field bound into `bind` (the `texture` `UsdUVTexture` graph is that growth); a new semantic taxonomy is one `MeshScene.labels` key; an asset-composition arc is one `MeshScene.references` entry; an asset-kind marking is one `ModelKind` row; a new layer format is one `LayerFormat` row keyed by suffix; a new packaging policy (ARKit-constrained) is one `UsdzProfile` row on the `package_usdz` close; a new stage source is one `StageOp` case folding into the `apply` `match` (the `extract_usdz` USD-import round-trip is a `scene/render#SCENE` `SceneOp` case reusing this dispatch); a new receipt fact is one `frozendict` band key; zero new owner — the source axis stays two cases (`RenderExport`/`MeshAuthor`) on one family, the geometry one recursive `PrimKind`, the packaging one close, every addition a schema call, row, case, field, or data choice.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable, Mapping
from dataclasses import dataclass, field
from enum import StrEnum
from pathlib import Path
from typing import Literal, assert_never

import numpy as np
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

lazy from pxr import Gf, Kind, Sdf, Usd, UsdGeom, UsdSemantics, UsdShade, UsdUtils, Vt
lazy from vtkmodules.vtkIOUSD import vtkUSDExporter  # source-build-gated: the official vtk wheel ships no vtkIOUSD

# --- [TYPES] ---------------------------------------------------------------------------

type Vec3 = tuple[float, float, float]
type Quat = tuple[float, float, float, float]      # (real, i, j, k) -> Gf.Quatf for AddOrientOp / PointInstancer orientations
type Matrix4 = tuple[float, ...]                    # 16 row-major cells -> Gf.Matrix4d for AddTransformOp
type PrimKindTag = Literal["mesh", "points", "curves", "xform", "instancer", "camera"]
type StageOpTag = Literal["render_export", "mesh_author"]
type XformOpTag = Literal["translate", "rotate", "scale", "orient", "transform"]


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


class ModelKind(StrEnum):
    # Usd.ModelAPI.SetKind marks a prim as a scene-graph asset for BIM/USD-pipeline interop; the value IS the Kind.Tokens name
    COMPONENT = "component"
    ASSEMBLY = "assembly"
    GROUP = "group"
    SUBCOMPONENT = "subcomponent"

    @property
    def token(self) -> object:
        return getattr(Kind.Tokens, self.value)


# --- [MODELS] --------------------------------------------------------------------------


@tagged_union(frozen=True)
class XformOp:
    # the placement op stack every UsdGeom.Xformable (an Xform group OR a leaf geometry OR a Camera) carries, ordered and composed left-to-right at author
    tag: XformOpTag = tag()
    translate: Vec3 = case()
    rotate: Vec3 = case()          # XYZ Euler degrees
    scale: Vec3 = case()
    orient: Quat = case()          # (real, i, j, k)
    transform: Matrix4 = case()    # 16 row-major cells

    @staticmethod
    def Translate(offset: Vec3) -> "XformOp":
        return XformOp(translate=offset)

    @staticmethod
    def Rotate(euler_xyz: Vec3) -> "XformOp":
        return XformOp(rotate=euler_xyz)

    @staticmethod
    def Scale(factor: Vec3) -> "XformOp":
        return XformOp(scale=factor)

    @staticmethod
    def Orient(quat: Quat) -> "XformOp":
        return XformOp(orient=quat)

    @staticmethod
    def Transform(matrix: Matrix4) -> "XformOp":
        return XformOp(transform=matrix)

    def add(self, xformable: object) -> None:
        match self:
            case XformOp(tag="translate", translate=offset):
                xformable.AddTranslateOp().Set(Gf.Vec3d(*offset))
            case XformOp(tag="rotate", rotate=euler):
                xformable.AddRotateXYZOp().Set(Gf.Vec3f(*euler))
            case XformOp(tag="scale", scale=factor):
                xformable.AddScaleOp().Set(Gf.Vec3f(*factor))
            case XformOp(tag="orient", orient=(w, x, y, z)):
                xformable.AddOrientOp().Set(Gf.Quatf(w, Gf.Vec3f(x, y, z)))
            case XformOp(tag="transform", transform=cells):
                xformable.AddTransformOp().Set(Gf.Matrix4d(*cells))
            case _:
                assert_never(self)


class Texture(Struct, frozen=True, kw_only=True):
    # a UsdUVTexture -> UsdPrimvarReader_float2('st') node pair driving UsdPreviewSurface.diffuseColor; the textured PBR path
    file: str
    wrap: Literal["repeat", "clamp", "mirror", "black"] = "repeat"


class Material(Struct, frozen=True, kw_only=True):
    diffuse: Vec3 = (0.18, 0.18, 0.18)
    metallic: float = 0.0
    roughness: float = 0.5
    opacity: float = 1.0
    texture: Texture | None = None          # when set, diffuseColor drives from a UsdUVTexture(st) node graph
    diffuse_primvar: str | None = None      # when set (e.g. "displayColor"), diffuseColor reads the per-vertex primvar so scalar colour survives under PBR rather than a flat Color3f overriding it

    def bind(self, stage: object, prim: object, path: object) -> None:
        material = UsdShade.Material.Define(stage, path)
        shader = UsdShade.Shader.Define(stage, path.AppendChild("PreviewSurface"))
        shader.CreateIdAttr("UsdPreviewSurface")
        shader.CreateInput("metallic", Sdf.ValueTypeNames.Float).Set(self.metallic)
        shader.CreateInput("roughness", Sdf.ValueTypeNames.Float).Set(self.roughness)
        shader.CreateInput("opacity", Sdf.ValueTypeNames.Float).Set(self.opacity)
        match self.texture, self.diffuse_primvar:  # base-colour source: a UV texture, a per-vertex primvar, or the flat Color3f
            case Texture() as texture, _:
                self._textured(stage, shader, path, texture)
            case None, str() as primvar:
                reader = UsdShade.Shader.Define(stage, path.AppendChild("colorReader"))
                reader.CreateIdAttr("UsdPrimvarReader_float3")
                reader.CreateInput("varname", Sdf.ValueTypeNames.Token).Set(primvar)
                shader.CreateInput("diffuseColor", Sdf.ValueTypeNames.Color3f).ConnectToSource(reader.ConnectableAPI(), "result")
            case _:
                shader.CreateInput("diffuseColor", Sdf.ValueTypeNames.Color3f).Set(Gf.Vec3f(*self.diffuse))
        material.CreateSurfaceOutput().ConnectToSource(shader.ConnectableAPI(), "surface")
        UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)  # the actual attach, distinct from defining the material

    def _textured(self, stage: object, shader: object, path: object, texture: Texture, /) -> None:
        # st reader -> UsdUVTexture -> diffuseColor: the standard UsdPreviewSurface texture graph replacing the flat Color3f
        reader = UsdShade.Shader.Define(stage, path.AppendChild("stReader"))
        reader.CreateIdAttr("UsdPrimvarReader_float2")
        reader.CreateInput("varname", Sdf.ValueTypeNames.Token).Set("st")
        sampler = UsdShade.Shader.Define(stage, path.AppendChild("diffuseTexture"))
        sampler.CreateIdAttr("UsdUVTexture")
        sampler.CreateInput("file", Sdf.ValueTypeNames.Asset).Set(Sdf.AssetPath(texture.file))
        sampler.CreateInput("st", Sdf.ValueTypeNames.Float2).ConnectToSource(reader.ConnectableAPI(), "result")
        sampler.CreateInput("wrapS", Sdf.ValueTypeNames.Token).Set(texture.wrap)
        sampler.CreateInput("wrapT", Sdf.ValueTypeNames.Token).Set(texture.wrap)
        shader.CreateInput("diffuseColor", Sdf.ValueTypeNames.Color3f).ConnectToSource(sampler.ConnectableAPI(), "rgb")


class FaceSubset(Struct, frozen=True, kw_only=True):
    # one per-face material region of a single mesh -> UsdGeom.Subset the MaterialBindingAPI targets (one mesh, N materials, never N meshes)
    name: str
    faces: NDArray[np.int32]
    material: Material


class Lens(Struct, frozen=True, kw_only=True):
    # a UsdGeom.Camera authored from the render viewpoint so a USD/USDZ deliverable frames its subject in QuickLook/AR
    eye: Vec3
    target: Vec3 = (0.0, 0.0, 0.0)
    up: Vec3 = (0.0, 0.0, 1.0)
    focal_length: float = 50.0
    clip: tuple[float, float] = (0.1, 100000.0)


class Instancing(Struct, frozen=True, kw_only=True):
    # UsdGeom.PointInstancer: one prototype authored once, placed N times by per-instance arrays (the AEC array-of-identical-elements win)
    prototype: "PrimKind"
    positions: NDArray[np.float32]
    proto_indices: NDArray[np.int32]
    scales: NDArray[np.float32] | None = None
    orientations: NDArray[np.float32] | None = None  # (N,4) (real,i,j,k) -> Vt.QuathArray


@tagged_union(frozen=True)
class PrimKind:
    tag: PrimKindTag = tag()
    mesh: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None, tuple[FaceSubset, ...]] = case()
    points: tuple[NDArray[np.float32], NDArray[np.float32] | None] = case()
    curves: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None] = case()
    xform: tuple[tuple[XformOp, ...], tuple["PrimNode", ...]] = case()  # a scene-graph group: placement op stack + named children (recursive)
    instancer: Instancing = case()
    camera: Lens = case()

    @staticmethod
    def Mesh(points: NDArray[np.float32], faces: NDArray[np.int32], normals: NDArray[np.float32] | None = None, subsets: tuple[FaceSubset, ...] = ()) -> "PrimKind":
        return PrimKind(mesh=(points, faces, normals, subsets))

    @staticmethod
    def Points(coords: NDArray[np.float32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        return PrimKind(points=(coords, widths))

    @staticmethod
    def Curves(points: NDArray[np.float32], vertex_counts: NDArray[np.int32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        return PrimKind(curves=(points, vertex_counts, widths))

    @staticmethod
    def Group(children: tuple["PrimNode", ...], *ops: XformOp) -> "PrimKind":
        return PrimKind(xform=(ops, children))

    @staticmethod
    def Instancer(instancing: Instancing) -> "PrimKind":
        return PrimKind(instancer=instancing)

    @staticmethod
    def Camera(lens: Lens) -> "PrimKind":
        return PrimKind(camera=lens)

    def define(self, stage: object, path: object, subdiv: SubdivScheme) -> object:
        match self:
            case PrimKind(tag="mesh", mesh=(verts, faces, normals, subsets)):
                mesh = UsdGeom.Mesh.Define(stage, path)
                mesh.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(verts))
                mesh.CreateFaceVertexCountsAttr(Vt.IntArray.FromNumpy(np.full(faces.shape[0], faces.shape[1], dtype=np.int32)))
                mesh.CreateFaceVertexIndicesAttr(Vt.IntArray.FromNumpy(faces.reshape(-1)))
                mesh.CreateSubdivisionSchemeAttr(subdiv.token)  # `none` for a triangulation: viewers do not re-subdivide
                if normals is not None:
                    mesh.CreateNormalsAttr(Vt.Vec3fArray.FromNumpy(normals))
                mesh.CreateExtentAttr(_extent(verts))
                for subset in subsets:  # one UsdGeom.Subset per per-face material region; the binding targets the subset, not the whole mesh
                    region = UsdGeom.Subset.Define(stage, path.AppendChild(subset.name))
                    region.CreateElementTypeAttr(UsdGeom.Tokens.face)
                    region.CreateIndicesAttr(Vt.IntArray.FromNumpy(subset.faces))
                    subset.material.bind(stage, region.GetPrim(), path.AppendChild(f"{subset.name}Mat"))
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
            case PrimKind(tag="xform", xform=(ops, children)):
                group = UsdGeom.Xform.Define(stage, path)  # a scene-graph grouping/placement node over recursive children
                for op in ops:
                    op.add(group)
                for child in children:
                    child.define(stage, path, subdiv)
                return group.GetPrim()
            case PrimKind(tag="instancer", instancer=inst):
                return _instancer(stage, path, inst, subdiv)
            case PrimKind(tag="camera", camera=lens):
                return _camera(stage, path, lens)
            case _:
                assert_never(self)


@dataclass(frozen=True, slots=True, kw_only=True)
class PrimNode:
    # one placed child of an xform group: its own name, geometry, placement ops, and per-element appearance (material/color/labels)
    name: str
    kind: PrimKind
    transform: tuple[XformOp, ...] = ()
    display_color: NDArray[np.float32] | None = None
    material: Material | None = None
    labels: Mapping[str, tuple[str, ...]] = field(default_factory=dict)

    def define(self, stage: object, parent: object, subdiv: SubdivScheme, /) -> object:
        path = parent.AppendChild(self.name)
        prim = self.kind.define(stage, path, subdiv)
        for op in self.transform:
            op.add(UsdGeom.Xformable(prim))  # wrap the leaf prim as Xformable to place it under its parent group
        _dress(stage, prim, path, self.display_color, self.material, self.labels)
        return prim


class MeshScene(Struct, frozen=True, kw_only=True):
    prim: PrimKind
    up_axis: UpAxis = UpAxis.Z
    meters_per_unit: float = 1.0
    subdiv: SubdivScheme = SubdivScheme.NONE
    transform: tuple[XformOp, ...] = ()               # root placement op stack
    display_color: NDArray[np.float32] | None = None
    material: Material | None = None
    labels: Mapping[str, tuple[str, ...]] = {}        # taxonomy -> semantic labels; the AEC discipline/classification plane
    model_kind: ModelKind | None = None               # Usd.ModelAPI scene-as-asset marking (component/assembly) for BIM/USD interop
    references: tuple[str, ...] = ()                   # Sdf.Reference composition arcs -> a shared component/detail library

    def author(self, stage: object) -> object:
        UsdGeom.SetStageUpAxis(stage, self.up_axis.token)
        UsdGeom.SetStageMetersPerUnit(stage, self.meters_per_unit)
        root = Sdf.Path("/Root")
        prim = self.prim.define(stage, root, self.subdiv)
        for op in self.transform:
            op.add(UsdGeom.Xformable(prim))
        _dress(stage, prim, root, self.display_color, self.material, self.labels)
        for asset in self.references:
            prim.GetReferences().AddReference(Sdf.Reference(asset))  # compose a shared component/detail-library layer into the root
        if self.model_kind is not None:
            Usd.ModelAPI(prim).SetKind(self.model_kind.token)  # mark the scene an asset (component/assembly) for BIM/USD pipelines
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
                prim = scene.author(stage)
                stats = UsdUtils.ComputeUsdStageStats(stage)  # single-arg form -> {"totalPrimCount", "usedLayerCount", ...}
                if not stats["totalPrimCount"]:  # Exemption: an empty authoring rails at the native boundary the worker seam converts
                    raise ValueError("empty stage: zero prims authored")
                stage.Export(path)
                facts: frozendict[str, float | str] = frozendict({
                    "prims": int(stats["totalPrimCount"]), "layers": int(stats["usedLayerCount"]),
                    "up_axis": scene.up_axis.value, "meters_per_unit": scene.meters_per_unit,
                    "extent_diag": _diagonal(stage, prim),  # the BBoxCache aggregate-bound diagonal a single-prim numpy extreme cannot span
                })  # the authored-prim evidence the `core/receipt#RECEIPT` `ArtifactReceipt.Scene` band carries, threaded back through the export worker
                return Path(path).read_bytes(), facts
            case _:
                assert_never(self)

# --- [OPERATIONS] ----------------------------------------------------------------------


def _extent(points: NDArray[np.float32]) -> object:
    return Vt.Vec3fArray.FromNumpy(np.stack([points.min(axis=0), points.max(axis=0)]))  # the per-prim [min, max] AR-framing box


def _dress(stage: object, prim: object, path: object, display_color: NDArray[np.float32] | None, material: Material | None, labels: Mapping[str, tuple[str, ...]], /) -> None:
    # displayColor primvar + whole-prim material bind + AEC semantic labels; shared by MeshScene.author and each PrimNode.define so appearance is authored one way
    if display_color is not None:
        colors = np.atleast_2d(display_color)
        interp = UsdGeom.Tokens.vertex if display_color.ndim == 2 else UsdGeom.Tokens.constant
        UsdGeom.PrimvarsAPI(prim).CreatePrimvar("displayColor", Sdf.ValueTypeNames.Color3fArray, interp).Set(Vt.Vec3fArray.FromNumpy(colors))
    if material is not None:
        material.bind(stage, prim, path.AppendChild("Material"))
    for taxonomy, tags in labels.items():
        UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(list(tags)))


def _instancer(stage: object, path: object, inst: Instancing, subdiv: SubdivScheme, /) -> object:
    instancer = UsdGeom.PointInstancer.Define(stage, path)
    protos = UsdGeom.Scope.Define(stage, path.AppendChild("Prototypes")).GetPath()  # a pure-namespace Scope holds the prototype library
    proto = inst.prototype.define(stage, protos.AppendChild("proto0"), subdiv)
    instancer.CreatePrototypesRel().AddTarget(proto.GetPath())
    instancer.CreateProtoIndicesAttr(Vt.IntArray.FromNumpy(inst.proto_indices))
    instancer.CreatePositionsAttr(Vt.Vec3fArray.FromNumpy(inst.positions))
    if inst.scales is not None:
        instancer.CreateScalesAttr(Vt.Vec3fArray.FromNumpy(inst.scales))
    if inst.orientations is not None:
        instancer.CreateOrientationsAttr(Vt.QuathArray.FromNumpy(inst.orientations))
    instancer.CreateExtentAttr(_extent(inst.positions))
    return instancer.GetPrim()


def _camera(stage: object, path: object, lens: Lens, /) -> object:
    camera = UsdGeom.Camera.Define(stage, path)  # a Camera IS Xformable, so it carries the look-at transform op directly
    camera.CreateProjectionAttr(UsdGeom.Tokens.perspective)
    camera.CreateFocalLengthAttr(lens.focal_length)
    camera.CreateClippingRangeAttr(Gf.Vec2f(*lens.clip))
    camera.AddTransformOp().Set(Gf.Matrix4d(*_look_at(lens.eye, lens.target, lens.up)))
    return camera.GetPrim()


def _look_at(eye: Vec3, target: Vec3, up: Vec3, /) -> Matrix4:
    # camera-to-world (USD row-vector convention, camera looks down local -Z); returns the 16 row-major cells for Gf.Matrix4d
    e = np.asarray(eye, dtype=np.float64)
    z = e - np.asarray(target, dtype=np.float64)
    z = z / (float(np.linalg.norm(z)) or 1.0)
    x = np.cross(np.asarray(up, dtype=np.float64), z)
    x = x / (float(np.linalg.norm(x)) or 1.0)
    y = np.cross(z, x)
    basis = np.eye(4, dtype=np.float64)
    basis[0, :3], basis[1, :3], basis[2, :3], basis[3, :3] = x, y, z, e
    return tuple(float(cell) for cell in basis.reshape(-1))


def _diagonal(stage: object, prim: object, /) -> float:
    # the aggregate world bound of a composed hierarchy through UsdGeom.BBoxCache (row [11]); the AR-framing evidence a single-prim numpy extreme cannot span
    box = UsdGeom.BBoxCache(Usd.TimeCode.Default(), [UsdGeom.Tokens.default_]).ComputeWorldBound(prim).ComputeAlignedRange()
    return 0.0 if box.IsEmpty() else float(np.linalg.norm(np.asarray(box.GetMax(), dtype=np.float64) - np.asarray(box.GetMin(), dtype=np.float64)))


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

The `StageOp.apply` arms, the recursive `PrimKind.define`/`PrimNode.define`, and the `export_usd`/`author_mesh`/`package_usdz`/`extract_usdz`/`relocate_assets` module-level functions run INSIDE the `scene/render#SCENE` `to_process.run_sync` worker (the `scene/export#EXPORT` `render_export` calls them synchronously, itself offloaded once by the `scene/render#SCENE` `_emit` `Export` arm) — the `lazy from pxr`/`lazy from vtkmodules.vtkIOUSD` bindings reify only there, so the `pxr` Boost.Python extension never loads on the 3.15 runtime nor at module scope, and stage.md mints no worker seam of its own. `Vt.Vec3fArray.FromNumpy(points)`/`Vt.IntArray.FromNumpy(faces.reshape(-1))`/`Vt.FloatArray.FromNumpy(widths)`/`Vt.QuathArray.FromNumpy(orientations)` fold the numpy buffers zero-copy into the typed USD arrays (`Vt.TokenArray(list(labels))` for the label tokens, which have no `FromNumpy`); the `package_usdz` close hands the `.usdc` to the packager the `UsdzProfile` policy value names (`ARKIT` selecting `CreateNewARKitUsdzPackage`, `STANDARD` the general `CreateNewUsdzPackage`). The `pxr` `Tf.ErrorException` (missing asset, malformed layer, write failure), the `Boost.Python.ArgumentError` (a wrong-typed argument, the `Sdf.AssetPath`-vs-`str` mismatch), and the empty-stage `ValueError` guard all map at the `scene/render#SCENE` `async_boundary` worker edge into the `runtime` `RuntimeRail` fault, never surfaced bare across the interpreter seam, and a `bool False` return from `CreateNewUsdzPackage`/`ExtractUsdzPackage` is a packaging-failure rail the `bool` return carries.

## [03]-[RESEARCH]

- [MESH_AUTHOR] [RESOLVED]: the wheel-available primary path — `Usd.Stage.CreateInMemory()` then `UsdGeom.Mesh`/`Points`/`BasisCurves.Define(stage, path)` per leaf `PrimKind` case, then `CreatePointsAttr(Vt.Vec3fArray.FromNumpy(buf))` and its `FaceVertexCounts`/`FaceVertexIndices`/`CurveVertexCounts`/`Normals`/`Widths` siblings, then `CreateSubdivisionSchemeAttr(UsdGeom.Tokens.none)`/`CreateExtentAttr`, then `SetStageUpAxis`/`SetStageMetersPerUnit`/`SetDefaultPrim`, then `Export(path)` — verify against the folder `.api/usd-core.md` entrypoint rows `[01]`-`[16]` and public-type rows. The zero-copy `Vt.<Type>Array.FromNumpy(ndarray)` bridge is the settled buffer path (the bare `Vt.<Type>Array(ndarray)` ctor raises `TypeError`, no registered converter); `FromNumpy` widens dtype (float64 `(N,3)` -> `Vec3fArray`). `Vt.TokenArray` has NO `FromNumpy`, so the label tokens construct from a Python list. The `CreateSubdivisionSchemeAttr(none)` triangulation gate and the `CreateExtentAttr([min,max])` AR-framing box are the two render-export correctness fixes the catalogue flags — a triangulation without `none` re-subdivides under the `catmullClark` default and a `.usdz` without extent frames poorly in QuickLook.
- [SCENE_GRAPH] [RESOLVED]: the composed-hierarchy authoring the Growth section only named is realized — the `xform` `PrimKind` case authors `UsdGeom.Xform.Define` + the ordered `XformOp` op stack (`AddTranslateOp`/`AddRotateXYZOp`/`AddScaleOp`/`AddOrientOp`/`AddTransformOp`, public-type row `[01]`) over recursive `PrimNode` children (each `parent.AppendChild(name)` + its own transform + `_dress` appearance), so a multi-element placed AEC assembly is one payload; the `instancer` case authors `UsdGeom.PointInstancer.Define` + `CreatePrototypesRel().AddTarget` + `CreateProtoIndicesAttr`/`CreatePositionsAttr`/`CreateScalesAttr`/`CreateOrientationsAttr` (rows `[07]`/`[19]`), the prototype defined once under a `UsdGeom.Scope` (row `[06]`) so N identical elements author one prototype and N transforms; the `camera` case authors `UsdGeom.Camera.Define` + `CreateProjectionAttr`/`CreateFocalLengthAttr`/`CreateClippingRangeAttr` + a numpy look-at `AddTransformOp` (row `[05]`); the mesh `FaceSubset` regions author `UsdGeom.Subset.Define` + `CreateElementTypeAttr(face)`/`CreateIndicesAttr` bound by a subset-targeted `MaterialBindingAPI` (row `[08]`), the one-mesh-N-materials case the catalogue names; `Usd.ModelAPI(prim).SetKind(Kind.Tokens.component)` marks the asset (public-type row `[05]`), `GetReferences().AddReference(Sdf.Reference(...))` composes a shared library (row `[10]`), and `UsdGeom.BBoxCache.ComputeWorldBound` (row `[11]`) reads the aggregate extent a single-prim numpy extreme cannot span. The `Vt.QuathArray.FromNumpy(orientations)` orientation authoring is the one extrapolation of the generic per-array-type `FromNumpy` surface pending a reflected `QuathArray` row.
- [MATERIAL_AND_SEMANTIC] [RESOLVED]: the `Material.bind` PBR graph — `UsdShade.Material.Define` + `UsdShade.Shader.Define` (the `UsdPreviewSurface` node via `CreateIdAttr("UsdPreviewSurface")`/`CreateInput`) wired through `ConnectableAPI`, then attached through `UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)` — verify against `.api/usd-core.md` rows `[12]`-`[14]` and entrypoint `[10]`; the binding API is the actual attach, distinct from defining the material, and a textured surface swaps the flat `Color3f` for a `UsdUVTexture`->`UsdPrimvarReader_float2('st')` node graph (the `Texture` field, row `[13]`), while a per-material face region is a `UsdGeom.Subset` the binding targets (row `[08]`). The `UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(labels))` semantic-label plane — rows `[15]`/`[16]` and entrypoint `[11]` — travels the AEC discipline/classification/sheet-type labels ON the prim (resolved across inheritance via `LabelsQuery.ComputeUniqueInheritedLabels`), the documentation-telos plane the `drawing/`+`specification/` classification vocabularies feed as the `MeshScene.labels`/`PrimNode.labels` ingress, never an out-of-band sidecar.
- [PACKAGING] [RESOLVED]: `CreateNewUsdzPackage(Sdf.AssetPath(usdc), usdz)` is the single USDZ builder the `package_usdz` close calls — it gathers the root layer plus its full dependency-asset closure ITSELF and writes the aligned-zip `.usdz`, returning `bool` — verify against `.api/usd-core.md` entrypoint rows `[01]`-`[06]`. Because the closure is embedded internally, a bare `ComputeAllDependencies` before packaging embeds nothing and is dead; `ComputeAllDependencies`/`ModifyAssetPaths` (the `relocate_assets` audit/relocation path) rewrite asset paths BEFORE packaging for the external-asset case (now load-bearing with `Sdf.Reference` composition arcs in play). `CreateNewARKitUsdzPackage` is the ARKit/QuickLook-constrained variant the `UsdzProfile.ARKIT` policy value selects; `ExtractUsdzPackage(usdz, dir, recurse, verbose, force)` (5 positionals) is the import inverse. `ComputeUsdStageStats(stage)` returns the `totalPrimCount`/`usedLayerCount` dict (single-arg form, NOT the legacy `(path, outDict)` out-param), computed NOW to gate the empty-stage fault AND folded into the `apply` `(bytes, facts)` return that threads the prim/layer/up-axis/meters-per-unit/extent-diagonal band onto the widened `core/receipt#RECEIPT` `ArtifactReceipt.Scene` `facts` slot.
- [RENDER_EXPORT] [RESEARCH]: the source-build-gated front half — `vtk` `vtkmodules.vtkIOUSD.vtkUSDExporter` (`SetRenderWindow(Plotter.render_window)`/`SetFileName`/`Write`) writing the rendered scene to a `.usdc` layer, which `package_usdz` then packages — is present ONLY on a VTK source-built against OpenUSD (the official wheel ships no `vtkmodules.vtkIOUSD`, `.api/vtk.md`), so the `lazy from vtkmodules.vtkIOUSD import vtkUSDExporter` binding reifies only on that build and `MeshAuthor`'s numpy path is the wheel-available default the `scene/export#EXPORT` USD/USDZ arm should prefer. When present, `vtk` owns render-to-layer and `usd-core` owns layer-to-package.
