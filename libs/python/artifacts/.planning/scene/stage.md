# [PY_ARTIFACTS_SCENE_STAGE]

The USD/USDZ stage-authoring owner. `StageOp` is the one Pixar OpenUSD source-axis family over the closed `RenderExport`/`MeshAuthor` cases — `RenderExport` writes the rendered scene to a layer through the source-build-gated `vtkUSDExporter`, `MeshAuthor` authors a `pxr.Usd.Stage` directly from the `scene/render#SCENE` `surface_arrays` numpy buffers over the `UsdGeom`/`Vt`/`Gf` schema family with no render pass — each folding to a serialized USD layer through one total `apply` `match`, plus the `package_usdz` close that zips an authored `.usdc` (with its dependency-asset closure) into the `.usdz` AR deliverable. `MeshScene` is the rich `MeshAuthor` payload over a recursive `PrimKind` scene-graph — a `mesh`/`points`/`curves` leaf, an `xform` group of placed `PrimNode` children, a `PointInstancer` array, or a `Camera` — carrying the `UpAxis`/`meters_per_unit` AR metadata, the `displayColor` primvar, the PBR `Material` bind, the `UsdSemantics.LabelsAPI` AEC labels, the `Usd.ModelAPI` `Kind` asset marking, and the `Sdf.Reference` composition arcs. This owner authors and packages only: the offscreen render stays at `scene/render#SCENE`, raw mesh-file interchange at `geometry/mesh`.

The owner rides the wider worker band. OpenUSD declares a HARD `>=3.9, <3.15` upper cap and ships no cp315 wheel, so `pxr` imports neither on the runtime nor at module scope — the `lazy from pxr` bindings reify only when a worker function first runs INSIDE the `scene/render#SCENE` runtime process lane subprocess (the sub-3.13 native-VTK worker the render already crosses onto). The `scene/export#EXPORT` `USD`/`USDZ` `SceneTarget` arms delegate here through the wheel-available `author_mesh(MeshScene(...))` numpy path — no render pass — threading the render spec's mapped per-vertex `display_color`, its PBR `Material`, and its AEC `labels` so the deliverable carries colour+material+classification rather than a bare grey mesh; `package_usdz` then closes the `.usdz`. `export_usd` (the `vtkUSDExporter` render-window front-half) is the source-build-gated enhancement reached only on a VTK built against OpenUSD (the standard `vtk` wheel ships no `vtkmodules.vtkIOUSD`). The `ArtifactReceipt.Scene` case is minted once at the `scene/render#SCENE` `Export` arm, no parallel receipt rail. The `pxr` `Tf.ErrorException`, the `Boost.Python.ArgumentError` (the `Sdf.AssetPath`-vs-`str` mismatch), and the empty-stage `ValueError` guard all map at the `scene/render#SCENE` `async_boundary` worker edge into the `RuntimeRail` fault, never surfaced bare across the interpreter seam; a `bool False` return from `CreateNewUsdzPackage`/`ExtractUsdzPackage` is the packaging-failure rail.

## [01]-[INDEX]

- [02]-[STAGE]: the one `StageOp` closed source-axis family folding to a serialized USD layer through one total `apply` `match`, the `MeshScene` recursive `PrimKind` scene-graph the wheel-available `MeshAuthor` path authors and `package_usdz` closes into the `.usdz`.

## [02]-[STAGE]

- Cases: `RenderExport` writes the rendered scene to a `.usdc`/`.usda` layer over `SetRenderWindow`/`SetFileName`/`Write`, source-build-gated because the official `vtk` wheel ships no `vtkIOUSD`. `MeshAuthor` is the wheel-available default the `scene/export#EXPORT` USD arm always takes: `PrimKind.define` folds each leaf/group/instancer/camera into its schema, `_dress` folds the `displayColor` primvar, PBR bind, and labels, and `MeshScene.author` folds the stage metadata. USDZ packaging is not a third case but the `package_usdz` close over either arm's `.usdc`: `CreateNewUsdzPackage` gathers the full dependency-asset closure and writes the aligned-zip `.usdz`, `CreateNewARKitUsdzPackage` the Apple QuickLook variant as the `UsdzProfile.ARKIT` policy value on the same close, never a per-platform owner. `extract_usdz` is the import inverse; `relocate_assets` the external-asset relocation before packaging.
- Auto: the `MeshAuthor` arm folds each numpy buffer through the zero-copy `Vt.<Type>Array.FromNumpy` bridge (the bare ctor raises `TypeError`, no registered converter), never a per-vertex append nor a `.tolist()` copy. `CreateSubdivisionSchemeAttr(none)` marks the triangulation non-subdivision-surface so a viewer does not re-subdivide it under the `catmullClark` default. Each Boundable prim's `CreateExtentAttr` box authors from its own buffer once, and a composed hierarchy's aggregate world box reads through `UsdGeom.BBoxCache` (the `IsEmpty`-guarded diagonal a single-prim numpy extreme cannot span). The `instancer` case defines the prototype once under a `UsdGeom.Scope` so N identical elements author one prototype and N transforms rather than N meshes; a `FaceSubset` binds a material to a `UsdGeom.Subset` region so one mesh carries N materials. `ComputeUsdStageStats` gates the authored stage so a zero-prim authoring rails rather than writing an empty layer. `CreateNewUsdzPackage` itself embeds the full dependency closure, so a scene with referenced textures/components packages a self-contained `.usdz`.
- Receipt: each USD/USDZ export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` minted once at the `scene/render#SCENE` `Export` arm; this owner contributes the serialized payload, never a parallel per-format rail. The `ComputeUsdStageStats` prim/layer counts, up-axis, meters-per-unit, and the `BBoxCache` `extent_diag` thread back as the `apply` `(bytes, facts)` return, minted onto the widened band — a new fact is one band key, never a new receipt case.
- Growth: a new USD prim schema is one `PrimKind` case plus one `define` arm; a per-element placement is one `XformOp` case; a multi-material region is one `FaceSubset`; a per-element appearance is one `PrimNode` field; a new material knob is one `Material` field bound into `bind`; a new semantic taxonomy is one `MeshScene.labels` key; an asset-composition arc is one `MeshScene.references` entry; an asset-kind marking is one `ModelKind` row; a new layer format is one `LayerFormat` row keyed by suffix; a new packaging policy is one `UsdzProfile` row on the `package_usdz` close; a new stage source is one `StageOp` case folding into the `apply` `match`; a new receipt fact is one `frozendict` band key. Zero new owner — the source axis stays two cases on one family, the geometry one recursive `PrimKind`, the packaging one close.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable, Mapping
from dataclasses import dataclass, field
from enum import StrEnum
from pathlib import Path
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

lazy from pxr import Gf, Kind, Sdf, Usd, UsdGeom, UsdSemantics, UsdShade, UsdUtils, Vt
lazy from vtkmodules.vtkIOUSD import vtkUSDExporter  # source-build-gated: the official vtk wheel ships no vtkIOUSD

# --- [TYPES] ---------------------------------------------------------------------------

type Vec3 = tuple[float, float, float]
type Quat = tuple[float, float, float, float]  # (real, i, j, k) -> Gf.Quatf for AddOrientOp / PointInstancer orientations
type Matrix4 = tuple[float, ...]  # 16 row-major cells -> Gf.Matrix4d for AddTransformOp
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
    STANDARD = "CreateNewUsdzPackage"  # general USDZ
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
    # the placement op stack every UsdGeom.Xformable carries, composed left-to-right at author
    tag: XformOpTag = tag()
    translate: Vec3 = case()
    rotate: Vec3 = case()  # XYZ Euler degrees
    scale: Vec3 = case()
    orient: Quat = case()  # (real, i, j, k)
    transform: Matrix4 = case()  # 16 row-major cells

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
    texture: Texture | None = None  # when set, diffuseColor drives from a UsdUVTexture(st) node graph
    diffuse_primvar: str | None = (
        None  # when set (e.g. "displayColor"), diffuseColor reads the per-vertex primvar so scalar colour survives under PBR
    )

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
    # one per-face material region -> UsdGeom.Subset the MaterialBindingAPI targets
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
    # UsdGeom.PointInstancer: one prototype authored once, placed N times by the per-instance arrays
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
    def Mesh(
        points: NDArray[np.float32], faces: NDArray[np.int32], normals: NDArray[np.float32] | None = None, subsets: tuple[FaceSubset, ...] = ()
    ) -> "PrimKind":
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
    transform: tuple[XformOp, ...] = ()  # root placement op stack
    display_color: NDArray[np.float32] | None = None
    material: Material | None = None
    labels: Mapping[str, tuple[str, ...]] = {}  # taxonomy -> semantic labels; the AEC discipline/classification plane
    model_kind: ModelKind | None = None  # Usd.ModelAPI scene-as-asset marking (component/assembly) for BIM/USD interop
    references: tuple[str, ...] = ()  # Sdf.Reference composition arcs -> a shared component/detail library

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
        LayerFormat.of_suffix(path)  # boundary guard: the crate/ASCII suffix Stage.Export writes
        match self:
            case StageOp(tag="render_export", render_export=render_window):
                exporter = vtkUSDExporter()
                exporter.SetRenderWindow(render_window)
                exporter.SetFileName(path)
                exporter.Write()
                return Path(
                    path
                ).read_bytes(), frozendict()  # the VTK layer carries no in-process stage handle; the render-window path leaves the receipt band empty
            case StageOp(tag="mesh_author", mesh_author=scene):
                stage = Usd.Stage.CreateInMemory(path)
                prim = scene.author(stage)
                stats = UsdUtils.ComputeUsdStageStats(stage)  # single-arg form -> {"totalPrimCount", "usedLayerCount", ...}
                if not stats["totalPrimCount"]:  # an empty authoring rails at the native boundary the worker seam converts
                    raise ValueError("empty stage: zero prims authored")
                stage.Export(path)
                facts: frozendict[str, float | str] = frozendict({
                    "prims": int(stats["totalPrimCount"]),
                    "layers": int(stats["usedLayerCount"]),
                    "up_axis": scene.up_axis.value,
                    "meters_per_unit": scene.meters_per_unit,
                    "extent_diag": _diagonal(stage, prim),  # the BBoxCache aggregate diagonal a single-prim numpy extreme cannot span
                })  # the authored-prim evidence the `ArtifactReceipt.Scene` band carries, threaded back through the export worker
                return Path(path).read_bytes(), facts
            case _:
                assert_never(self)


# --- [OPERATIONS] ----------------------------------------------------------------------


def _extent(points: NDArray[np.float32]) -> object:
    return Vt.Vec3fArray.FromNumpy(np.stack([points.min(axis=0), points.max(axis=0)]))  # the per-prim [min, max] AR-framing box


def _dress(
    stage: object,
    prim: object,
    path: object,
    display_color: NDArray[np.float32] | None,
    material: Material | None,
    labels: Mapping[str, tuple[str, ...]],
    /,
) -> None:
    # displayColor primvar + material bind + AEC labels; shared by MeshScene.author and PrimNode.define so appearance is authored one way
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
    # the aggregate world bound of a composed hierarchy through UsdGeom.BBoxCache — AR-framing evidence a single-prim numpy extreme cannot span
    box = UsdGeom.BBoxCache(Usd.TimeCode.Default(), [UsdGeom.Tokens.default_]).ComputeWorldBound(prim).ComputeAlignedRange()
    return 0.0 if box.IsEmpty() else float(np.linalg.norm(np.asarray(box.GetMax(), dtype=np.float64) - np.asarray(box.GetMin(), dtype=np.float64)))


def export_usd(render_window: object, path: str) -> tuple[bytes, frozendict[str, float | str]]:
    return StageOp.RenderExport(render_window).apply(path)


def author_mesh(scene: MeshScene, path: str) -> tuple[bytes, frozendict[str, float | str]]:
    return StageOp.MeshAuthor(scene).apply(path)


def package_usdz(source: str, sink: str, profile: UsdzProfile = UsdzProfile.STANDARD) -> bool:
    return getattr(UsdUtils, profile.value)(
        Sdf.AssetPath(source), sink
    )  # profile.value IS the UsdUtils packager name; gathers + embeds the full dependency-asset closure itself


def extract_usdz(package: str, into: str) -> bool:
    return UsdUtils.ExtractUsdzPackage(package, into, True, False, True)  # recurse, quiet, force — the packaging inverse


def relocate_assets(layer_path: str, rewrite: Callable[[str], str]) -> tuple[int, int]:
    layer = Sdf.Layer.FindOrOpen(layer_path)
    UsdUtils.ModifyAssetPaths(layer, rewrite)  # rewrite every asset path (relocation before packaging)
    _layers, references, assets = UsdUtils.ComputeAllDependencies(Sdf.AssetPath(layer_path))
    return len(references), len(
        assets
    )  # the relocated-edge counts; CreateNewUsdzPackage embeds the closure, so this is the audit/relocation path only
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
