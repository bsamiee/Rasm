# [PY_ARTIFACTS_SCENE_STAGE]

`StageOp` owns USD/USDZ stage authoring as one closed Pixar OpenUSD source family over `RenderExport` and `MeshAuthor`. `RenderExport` writes a rendered scene through the `vtkIOUSD`-gated `vtkUSDExporter`; `MeshAuthor` authors a `pxr.Usd.Stage` from admitted numpy buffers over `UsdGeom`, `Vt`, and `Gf`; and both fold to a serialized layer through one total `apply` match. `PackageOp` owns package, extraction, compliance, and relocation closes, while `packaged` returns one `Result[PackageFacts, PackageFault]` layer. `MeshScene` owns the recursive `PrimKind` graph, stage metadata, appearance, semantics, model kind, and eager-reference or deferred-payload `AssetArc` composition. Offscreen rendering stays at `scene/render#SCENE`, and raw mesh interchange stays at `geometry/mesh`.

`pxr` imports eagerly at module scope — this module loads only on the worker floor — and every annotation remains parse-floor-safe; the package resolves from the Forge python-overlay `.pth` seated on the venv path tail rather than from a venv distribution, so an absent `pxr` diagnoses at the overlay and never at the lockfile, and every `pxr` member this page names is reachable ungated. One guarded `_usd_exporter` resolver owns the optional `vtkmodules.vtkIOUSD` import. `scene/export#EXPORT` delegates `USD` and `USDZ` to the numpy authoring path, and its terminal adapter maps `PackageFault` onto `ExportFault`; no false provider return or failed compliance result can masquerade as a successful output.

## [01]-[INDEX]

- [02]-[STAGE]: `StageOp` folds its closed source family to a serialized layer, `MeshScene` authors the recursive `PrimKind` graph, and `packaged` folds `PackageOp` onto `Result[PackageFacts, PackageFault]`.

## [02]-[STAGE]

- Cases: `RenderExport` writes a `.usdc` or `.usda` layer through `SetRenderWindow`, `SetFileName`, and `Write`. `MeshAuthor` folds every admitted `PrimKind` into its schema, then `MeshScene.author` binds metadata, appearance, labels, model kind, and `AssetArc` composition. `InputSource` splits three ways over EVERY connectable `SurfaceInput` — `constant` sets the value, `texture` connects one `UsdUVTexture` output port, `primvar` connects an arity-selected `UsdPrimvarReader_*` — so a primvar-driven occlusion and a packed-sheet roughness are the same case set a diffuse colour takes, and no input carries a modality another lacks. `PackageOp.Package`, `Extract`, `Verify`, and `Relocate` fold through `packaged`; every arm MEASURES its own close — `_dependencies` reads the settled artifact's layer, reference, and asset rosters and the compliance run adds the rule and warning counts it exercised — so `PackageFacts` carries evidence at every arm and `PackageFacts.combined` accrues a multi-op close instead of keeping the terminal arm's alone. Every provider false return, unavailable layer, error, or failed check carries a closed `PackageFault` case, while a warning passes and rides the band.
- Auto: `MeshAuthor` folds numpy buffers through `Vt.<Type>Array.FromNumpy`, never per-element append or `.tolist()` copies; orientation buffers carry the page-wide `(w, x, y, z)` convention and roll into Gf quaternion memory order once at `_instancer`. `PrimKind` admits point, face, normal, curve, instancing, camera, and light invariants — sibling and subset names unique, so no two prims silently merge onto one path — before authoring. `CreateSubdivisionSchemeAttr(UsdGeom.Tokens.none)` preserves an exported triangulation. Each boundable schema authors its extent, while `UsdGeom.BBoxCache` derives the composed world diagonal. `instancer` authors a prototype roster with range-checked indices beside its placement arrays; `FaceSubset` binds material regions; `camera` selects perspective or orthographic projection through `Lens.projection`; and `sun` aims a `UsdLux.DistantLight` from solar azimuth and elevation. `ComputeUsdStageStats` rejects an empty stage, and `UsdSemantics.LabelsQuery.ComputeUniqueInheritedLabels` adds resolved taxonomies to the facts band.
- Auto: `Material` carries the whole preview-surface vector as one `SurfaceInput`-keyed map and `_SURFACE` is the ONE law row a lowering reads — value type, arity, primvar-reader id, and unit window — so `bind` folds the map with no per-input arm and an unauthored input resolves to the schema's own fallback rather than a re-asserted copy. Admission runs over every entry BEFORE the material prim is defined, so a refused value leaves no half-authored shader graph; port legality DERIVES from arity (the `rgb` port carries float3, every single port float) rather than a per-input legal-port list.
- Auto: prim identity is VALUE-derived at every boundary, so the graph a material authors is a function of what it declares and never of how a caller spelled it. Every `st` reader keys by primvar NAME, so a twelve-map set authors one reader and a second uv set is one more name; the sampler and its placement key by the `Texture` value's own digest, so a packed sheet feeding three inputs authors ONE `UsdUVTexture` the three read on three ports — the reason the port is a required column rather than a defaulted one — where a slot-derived path authors that file once per input and pays a decode for each. `bind` folds SLOT-SORTED and `_dress` taxonomy-sorted because authoring order is serialization order: an insertion-ordered fold exports two byte streams for one appearance and forks its content key. `Texture` carries the sampler's whole surface — per-axis wrap, source colour space, the `value * scale + bias` decode an 8-bit tangent-space normal needs, the unresolved-file fallback, and an optional `UsdTransform2d` placement whose absence leaves the reader as the st stream rather than an identity node.
- Growth: a new USD schema is one `PrimKind` case with one `define` arm; placement is one `XformOp` case; a new surface input is one `SurfaceInput` member with its `_SURFACE` row, which `admitted` and `wire` both pick up unedited; a new binding modality is one `InputSource` case with one `wire` arm; a new sampler knob is one `Texture` field, which joins the identity digest with no `_sampler` edit and correctly splits two bindings that now differ on it; semantics enrich `PrimNode`; composition is one `AssetArc` case; layer format is one `LayerFormat` row; packaging is one `PackageOp` and `PackageFault` case pair plus its measure inside `packaged`; a new package measure is one `PackageFacts` field with its `combined` join. Source, graph, and package axes retain one owner each.
- Boundary: USD token vocabularies stay foreign spellings held at this boundary — `Wrap`, `ColorSpace`, `OutputPort`, and `SurfaceInput` carry the schema's own strings verbatim so the shader graph needs no translation column, and the cross-branch channel roster the texture sub-domain transcribes never reaches these members. Texture BYTES, their egress leaf names, and the lowering that binds a canonical channel role onto a preview-surface slot are `graphic/texture/set#TEXTURE_SET`'s; this page consumes a resolved asset path with the slot, the port, and the source colour space a caller already chose, and authors the graph that reads it. The caller states that space from the plane's own declared transfer, never from the `AUTO` default, because `AUTO` defers to the renderer's file sniff and a raw parameter plane in an eight- or sixteen-bit container sniffs as display-encoded — the one silent decode the whole transfer roster exists to foreclose.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from hashlib import blake2b
from importlib import import_module
from importlib.util import find_spec
from pathlib import Path
from typing import Final, Literal, assert_never

import msgspec
import numpy as np
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray
from pxr import Gf, Kind, Sdf, Usd, UsdGeom, UsdLux, UsdSemantics, UsdShade, UsdUtils, Vt

# --- [TYPES] ----------------------------------------------------------------------------

type Vec2 = tuple[float, float]
type Vec3 = tuple[float, float, float]
type Vec4 = tuple[float, float, float, float]
type Quat = tuple[float, float, float, float]
type Matrix4 = tuple[float, ...]
type PrimKindTag = Literal["mesh", "points", "curves", "xform", "instancer", "camera", "sun"]
type StageOpTag = Literal["render_export", "mesh_author"]
type XformOpTag = Literal["translate", "rotate", "scale", "orient", "transform"]
type PackageOpTag = Literal["package", "extract", "verify", "relocate"]
type PackageFaultTag = PackageOpTag
type AssetArcTag = Literal["reference", "payload"]
type InputSourceTag = Literal["constant", "texture", "primvar"]


class LayerFormat(StrEnum):
    CRATE = "usdc"
    ASCII = "usda"

    @staticmethod
    def of_suffix(path: str) -> "LayerFormat":
        found = next((fmt for fmt in LayerFormat if path.endswith(f".{fmt.value}")), None)
        if found is None:
            raise ValueError(f"not a USD layer suffix: {path!r}")
        return found


class Projection(StrEnum):
    PERSPECTIVE = "perspective"
    ORTHOGRAPHIC = "orthographic"

    @property
    def token(self) -> object:
        return getattr(UsdGeom.Tokens, self.value)


class UpAxis(StrEnum):
    Y = "y"
    Z = "z"

    @property
    def token(self) -> object:
        return getattr(UsdGeom.Tokens, self.value)


class SubdivScheme(StrEnum):
    NONE = "none"
    CATMULL_CLARK = "catmullClark"

    @property
    def token(self) -> object:
        return getattr(UsdGeom.Tokens, self.value)


class UsdzProfile(StrEnum):
    STANDARD = "standard"
    ARKIT = "arkit"

    @property
    def arkit(self) -> bool:
        return self is UsdzProfile.ARKIT


class Wrap(StrEnum):
    BLACK = "black"
    CLAMP = "clamp"
    REPEAT = "repeat"
    MIRROR = "mirror"
    METADATA = "useMetadata"


class ColorSpace(StrEnum):
    RAW = "raw"
    SRGB = "sRGB"
    AUTO = "auto"


class OutputPort(StrEnum):
    R = "r"
    G = "g"
    B = "b"
    A = "a"
    RGB = "rgb"


class SurfaceInput(StrEnum):
    DIFFUSE_COLOR = "diffuseColor"
    EMISSIVE_COLOR = "emissiveColor"
    SPECULAR_COLOR = "specularColor"
    METALLIC = "metallic"
    ROUGHNESS = "roughness"
    CLEARCOAT = "clearcoat"
    CLEARCOAT_ROUGHNESS = "clearcoatRoughness"
    OPACITY = "opacity"
    OPACITY_THRESHOLD = "opacityThreshold"
    IOR = "ior"
    NORMAL = "normal"
    DISPLACEMENT = "displacement"
    OCCLUSION = "occlusion"


class ModelKind(StrEnum):
    MODEL = "model"
    COMPONENT = "component"
    ASSEMBLY = "assembly"
    GROUP = "group"
    SUBCOMPONENT = "subcomponent"

    @property
    def token(self) -> object:
        return getattr(Kind.Tokens, self.value)


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AssetArc:
    tag: AssetArcTag = tag()
    reference: str = case()
    payload: str = case()

    @staticmethod
    def Reference(asset: str) -> "AssetArc":
        return AssetArc(reference=asset)

    @staticmethod
    def Payload(asset: str) -> "AssetArc":
        return AssetArc(payload=asset)

    def compose(self, prim: object) -> None:
        match self:
            case AssetArc(tag="reference", reference=asset):
                prim.GetReferences().AddReference(Sdf.Reference(asset))
            case AssetArc(tag="payload", payload=asset):
                prim.GetPayloads().AddPayload(Sdf.Payload(asset))
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class XformOp:
    tag: XformOpTag = tag()
    translate: Vec3 = case()
    rotate: Vec3 = case()
    scale: Vec3 = case()
    orient: Quat = case()
    transform: Matrix4 = case()

    @staticmethod
    def Translate(offset: Vec3) -> "XformOp":
        XformOp._finite(offset, "translation")
        return XformOp(translate=offset)

    @staticmethod
    def Rotate(euler_xyz: Vec3) -> "XformOp":
        XformOp._finite(euler_xyz, "rotation")
        return XformOp(rotate=euler_xyz)

    @staticmethod
    def Scale(factor: Vec3) -> "XformOp":
        XformOp._finite(factor, "scale")
        if any(cell == 0.0 for cell in factor):
            raise ValueError("scale contains a zero axis")
        return XformOp(scale=factor)

    @staticmethod
    def Orient(quat: Quat) -> "XformOp":
        XformOp._finite(quat, "orientation")
        if np.linalg.norm(quat) == 0.0:
            raise ValueError("orientation quaternion has zero norm")
        return XformOp(orient=quat)

    @staticmethod
    def Transform(matrix: Matrix4) -> "XformOp":
        if len(matrix) != 16 or not np.isfinite(matrix).all():
            raise ValueError("transform requires sixteen finite cells")
        return XformOp(transform=matrix)

    @staticmethod
    def _finite(values: tuple[float, ...], name: str) -> None:
        if not np.isfinite(values).all():
            raise ValueError(f"{name} contains a non-finite value")

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


class SurfaceLaw(Struct, frozen=True):
    value_type: object
    reader: str
    channels: int
    bounded: bool = True


_SURFACE: Final[frozendict[SurfaceInput, SurfaceLaw]] = frozendict({
    SurfaceInput.DIFFUSE_COLOR: SurfaceLaw(Sdf.ValueTypeNames.Color3f, "UsdPrimvarReader_float3", 3),
    SurfaceInput.EMISSIVE_COLOR: SurfaceLaw(Sdf.ValueTypeNames.Color3f, "UsdPrimvarReader_float3", 3),
    SurfaceInput.SPECULAR_COLOR: SurfaceLaw(Sdf.ValueTypeNames.Color3f, "UsdPrimvarReader_float3", 3),
    SurfaceInput.METALLIC: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.ROUGHNESS: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.CLEARCOAT: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.CLEARCOAT_ROUGHNESS: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.OPACITY: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.OPACITY_THRESHOLD: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.OCCLUSION: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1),
    SurfaceInput.IOR: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1, bounded=False),
    SurfaceInput.DISPLACEMENT: SurfaceLaw(Sdf.ValueTypeNames.Float, "UsdPrimvarReader_float", 1, bounded=False),
    SurfaceInput.NORMAL: SurfaceLaw(Sdf.ValueTypeNames.Normal3f, "UsdPrimvarReader_float3", 3, bounded=False),
})

_COVERED: Final[tuple[tuple[frozenset[object], frozenset[object]], ...]] = ((frozenset(_SURFACE), frozenset(SurfaceInput)),)
if any(rows != vocabulary for rows, vocabulary in _COVERED):
    raise RuntimeError("stage tables do not cover their vocabularies")


class UvTransform(Struct, frozen=True, gc=False, kw_only=True):
    rotation: float = 0.0
    scale: Vec2 = (1.0, 1.0)
    translation: Vec2 = (0.0, 0.0)


class Texture(Struct, frozen=True, kw_only=True):
    file: str
    st: str = "st"
    wrap_s: Wrap = Wrap.REPEAT
    wrap_t: Wrap = Wrap.REPEAT
    color_space: ColorSpace = ColorSpace.AUTO
    scale: Vec4 = (1.0, 1.0, 1.0, 1.0)
    bias: Vec4 = (0.0, 0.0, 0.0, 0.0)
    fallback: Vec4 = (0.0, 0.0, 0.0, 1.0)
    transform: UvTransform | None = None


class PbrMap(Struct, frozen=True, kw_only=True):
    texture: Texture
    channel: OutputPort


@tagged_union(frozen=True)
class InputSource:
    tag: InputSourceTag = tag()
    constant: float | Vec3 = case()
    texture: PbrMap = case()
    primvar: str = case()

    @staticmethod
    def Constant(value: float | Vec3) -> "InputSource":
        return InputSource(constant=value)

    @staticmethod
    def Texture(texture: Texture, channel: OutputPort) -> "InputSource":
        return InputSource(texture=PbrMap(texture=texture, channel=channel))

    @staticmethod
    def Primvar(name: str) -> "InputSource":
        return InputSource(primvar=name)

    def admitted(self, slot: SurfaceInput, /) -> None:
        law = _SURFACE[slot]
        match self:
            case InputSource(tag="constant", constant=value):
                cells = value if isinstance(value, tuple) else (value,)
                if len(cells) != law.channels or not np.isfinite(cells).all():
                    raise ValueError(f"{slot.value} takes {law.channels} finite component(s)")
                if law.bounded and any(not 0.0 <= cell <= 1.0 for cell in cells):
                    raise ValueError(f"{slot.value} constant leaves the unit window")
            case InputSource(tag="texture", texture=bound):
                if (bound.channel is OutputPort.RGB) is not (law.channels == 3):
                    raise ValueError(f"{slot.value} takes {law.channels} component(s) and cannot read the {bound.channel.value} port")
                if not bound.texture.file or not bound.texture.st:
                    raise ValueError(f"{slot.value} texture needs a file and an st primvar name")
                place = bound.texture.transform or UvTransform()
                if not np.isfinite(
                    (*bound.texture.scale, *bound.texture.bias, *bound.texture.fallback, place.rotation, *place.scale, *place.translation)
                ).all():
                    raise ValueError(f"{slot.value} sampler decode and placement must be finite")
            case InputSource(tag="primvar", primvar=name):
                if not name:
                    raise ValueError(f"{slot.value} primvar name cannot be empty")
            case _ as unreachable:
                assert_never(unreachable)

    def wire(self, stage: object, shader: object, path: object, slot: SurfaceInput, /) -> None:
        law = _SURFACE[slot]
        target = shader.CreateInput(slot.value, law.value_type)
        match self:
            case InputSource(tag="constant", constant=(r, g, b)):
                target.Set(Gf.Vec3f(r, g, b))
            case InputSource(tag="constant", constant=scalar):
                target.Set(float(scalar))
            case InputSource(tag="texture", texture=bound):
                target.ConnectToSource(_sampler(stage, path, bound.texture).ConnectableAPI(), bound.channel.value)
            case InputSource(tag="primvar", primvar=name):
                reader = UsdShade.Shader.Define(stage, path.AppendChild(f"{slot.value}Primvar"))
                reader.CreateIdAttr(law.reader)
                reader.CreateInput("varname", Sdf.ValueTypeNames.String).Set(name)
                target.ConnectToSource(reader.ConnectableAPI(), "result")
            case _ as unreachable:
                assert_never(unreachable)


class Material(Struct, frozen=True, kw_only=True):
    inputs: frozendict[SurfaceInput, InputSource] = frozendict()
    specular_workflow: bool = False

    def bind(self, stage: object, prim: object, path: object) -> None:
        ordered = sorted(self.inputs.items(), key=lambda entry: entry[0].value)
        for slot, source in ordered:
            source.admitted(slot)
        material = UsdShade.Material.Define(stage, path)
        shader = UsdShade.Shader.Define(stage, path.AppendChild("PreviewSurface"))
        shader.CreateIdAttr("UsdPreviewSurface")
        shader.CreateInput("useSpecularWorkflow", Sdf.ValueTypeNames.Int).Set(int(self.specular_workflow))
        for slot, source in ordered:
            source.wire(stage, shader, path, slot)
        material.CreateSurfaceOutput().ConnectToSource(shader.ConnectableAPI(), "surface")
        if SurfaceInput.DISPLACEMENT in self.inputs:
            material.CreateDisplacementOutput().ConnectToSource(
                shader.CreateOutput("displacement", Sdf.ValueTypeNames.Token)
            )
        UsdShade.MaterialBindingAPI.Apply(prim).Bind(material)


class FaceSubset(Struct, frozen=True, kw_only=True):
    name: str
    faces: NDArray[np.int32]
    material: Material


class Lens(Struct, frozen=True, gc=False, kw_only=True):
    eye: Vec3
    target: Vec3 = (0.0, 0.0, 0.0)
    up: Vec3 = (0.0, 0.0, 1.0)
    projection: Projection = Projection.PERSPECTIVE
    focal_length: float = 50.0
    clip: tuple[float, float] = (0.1, 100000.0)


class Instancing(Struct, frozen=True, kw_only=True):
    prototypes: tuple["PrimKind", ...]
    positions: NDArray[np.float32]
    proto_indices: NDArray[np.int32]
    scales: NDArray[np.float32] | None = None
    orientations: NDArray[np.float32] | None = None


@tagged_union(frozen=True)
class PrimKind:
    tag: PrimKindTag = tag()
    mesh: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None, tuple[FaceSubset, ...]] = case()
    points: tuple[NDArray[np.float32], NDArray[np.float32] | None] = case()
    curves: tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32] | None] = case()
    xform: tuple[tuple[XformOp, ...], tuple["PrimNode", ...]] = case()
    instancer: Instancing = case()
    camera: Lens = case()
    sun: tuple[float, float, float, Vec3] = case()

    @staticmethod
    def Mesh(
        points: NDArray[np.float32], faces: NDArray[np.int32], normals: NDArray[np.float32] | None = None, subsets: tuple[FaceSubset, ...] = ()
    ) -> "PrimKind":
        PrimKind._points(points, "mesh")
        if faces.ndim != 2 or faces.shape[1] < 3:
            raise ValueError("mesh faces require a rank-two polygon index buffer")
        if faces.size and (faces.min() < 0 or faces.max() >= len(points)):
            raise ValueError("face indices exceed the point range")
        if normals is not None and (normals.shape != points.shape or not np.isfinite(normals).all()):
            raise ValueError("mesh normals must be finite and align with points")
        if any(subset.faces.ndim != 1 or (subset.faces.size and (subset.faces.min() < 0 or subset.faces.max() >= len(faces))) for subset in subsets):
            raise ValueError("face subset indices exceed the face range")
        if len({subset.name for subset in subsets}) != len(subsets):
            raise ValueError("face subsets require unique names")
        return PrimKind(mesh=(points, faces, normals, subsets))

    @staticmethod
    def Points(coords: NDArray[np.float32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        PrimKind._points(coords, "point cloud")
        if widths is not None and (widths.shape != (len(coords),) or not np.isfinite(widths).all() or np.any(widths <= 0.0)):
            raise ValueError("point widths must be positive, finite, and align with points")
        return PrimKind(points=(coords, widths))

    @staticmethod
    def Curves(points: NDArray[np.float32], vertex_counts: NDArray[np.int32], widths: NDArray[np.float32] | None = None) -> "PrimKind":
        PrimKind._points(points, "curve")
        if vertex_counts.ndim != 1 or np.any(vertex_counts < 2) or int(vertex_counts.sum()) != len(points):
            raise ValueError("curve vertex counts must partition the point buffer into segments")
        if widths is not None and (widths.shape != (len(points),) or not np.isfinite(widths).all() or np.any(widths <= 0.0)):
            raise ValueError("curve widths must be positive, finite, and align with points")
        return PrimKind(curves=(points, vertex_counts, widths))

    @staticmethod
    def Group(children: tuple["PrimNode", ...], *ops: XformOp) -> "PrimKind":
        if len({child.name for child in children}) != len(children):
            raise ValueError("group children require unique prim names")
        return PrimKind(xform=(ops, children))

    @staticmethod
    def Instancer(instancing: Instancing) -> "PrimKind":
        PrimKind._points(instancing.positions, "instancer")
        count = len(instancing.positions)
        if not instancing.prototypes:
            raise ValueError("instancer requires at least one prototype")
        if instancing.proto_indices.shape != (count,) or instancing.proto_indices.min() < 0 or instancing.proto_indices.max() >= len(instancing.prototypes):
            raise ValueError("prototype indices must align with positions and select a declared prototype")
        if instancing.scales is not None and (
            instancing.scales.shape != (count, 3) or not np.isfinite(instancing.scales).all() or np.any(instancing.scales <= 0.0)
        ):
            raise ValueError("instance scales must be positive, finite, and align with positions")
        if instancing.orientations is not None and (
            instancing.orientations.shape != (count, 4)
            or not np.isfinite(instancing.orientations).all()
            or np.any(np.linalg.norm(instancing.orientations, axis=1) == 0.0)
        ):
            raise ValueError("instance orientations must be finite nonzero quaternions aligned with positions")
        return PrimKind(instancer=instancing)

    @staticmethod
    def Camera(lens: Lens) -> "PrimKind":
        if lens.focal_length <= 0.0 or lens.clip[0] <= 0.0 or lens.clip[0] >= lens.clip[1] or not np.isfinite((*lens.eye, *lens.target, *lens.up, lens.focal_length, *lens.clip)).all():
            raise ValueError("camera values must be finite with positive focal length and ordered clipping planes")
        return PrimKind(camera=lens)

    @staticmethod
    def Sun(azimuth: float, elevation: float, intensity: float = 50000.0, color: Vec3 = (1.0, 1.0, 1.0)) -> "PrimKind":
        if (
            not np.isfinite((azimuth, elevation, intensity, *color)).all()
            or not -90.0 <= elevation <= 90.0
            or intensity < 0.0
            or len(color) != 3
            or any(not 0.0 <= channel <= 1.0 for channel in color)
        ):
            raise ValueError("sun requires finite values, bounded elevation, nonnegative intensity, and a three-channel [0, 1] color")
        return PrimKind(sun=(azimuth, elevation, intensity, color))

    @staticmethod
    def _points(points: NDArray[np.float32], owner: str) -> None:
        if points.ndim != 2 or points.shape[1] != 3 or not len(points) or not np.isfinite(points).all():
            raise ValueError(f"{owner} points require a nonempty finite (N, 3) buffer")

    def define(self, stage: object, path: object, subdiv: SubdivScheme) -> object:
        match self:
            case PrimKind(tag="mesh", mesh=(verts, faces, normals, subsets)):
                mesh = UsdGeom.Mesh.Define(stage, path)
                mesh.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(verts))
                mesh.CreateFaceVertexCountsAttr(Vt.IntArray.FromNumpy(np.full(faces.shape[0], faces.shape[1], dtype=np.int32)))
                mesh.CreateFaceVertexIndicesAttr(Vt.IntArray.FromNumpy(faces.reshape(-1)))
                mesh.CreateSubdivisionSchemeAttr(subdiv.token)
                if normals is not None:
                    mesh.CreateNormalsAttr(Vt.Vec3fArray.FromNumpy(normals))
                mesh.CreateExtentAttr(_extent(verts))
                for subset in subsets:
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
                curve = UsdGeom.BasisCurves.Define(stage, path)
                curve.CreatePointsAttr(Vt.Vec3fArray.FromNumpy(pts))
                curve.CreateCurveVertexCountsAttr(Vt.IntArray.FromNumpy(counts))
                curve.CreateTypeAttr(UsdGeom.Tokens.linear)
                if widths is not None:
                    curve.CreateWidthsAttr(Vt.FloatArray.FromNumpy(widths))
                curve.CreateExtentAttr(_extent(pts))
                return curve.GetPrim()
            case PrimKind(tag="xform", xform=(ops, children)):
                group = UsdGeom.Xform.Define(stage, path)
                for op in ops:
                    op.add(group)
                for child in children:
                    child.define(stage, path, subdiv)
                return group.GetPrim()
            case PrimKind(tag="instancer", instancer=inst):
                return _instancer(stage, path, inst, subdiv)
            case PrimKind(tag="camera", camera=lens):
                return _camera(stage, path, lens)
            case PrimKind(tag="sun", sun=(azimuth, elevation, intensity, color)):
                light = UsdLux.DistantLight.Define(stage, path)
                light.CreateIntensityAttr(intensity)
                light.CreateColorAttr(Gf.Vec3f(*color))
                light.CreateAngleAttr(0.53)
                XformOp.Rotate((elevation - 90.0, 0.0, -azimuth)).add(light)
                return light.GetPrim()
            case _:
                assert_never(self)


class PrimNode(Struct, frozen=True, kw_only=True):
    name: str
    kind: PrimKind
    transform: tuple[XformOp, ...] = ()
    display_color: NDArray[np.float32] | None = None
    material: Material | None = None
    labels: frozendict[str, tuple[str, ...]] = frozendict()

    def define(self, stage: object, parent: object, subdiv: SubdivScheme, /) -> object:
        path = parent.AppendChild(self.name)
        prim = self.kind.define(stage, path, subdiv)
        for op in self.transform:
            op.add(UsdGeom.Xformable(prim))
        _dress(stage, prim, path, self.display_color, self.material, self.labels)
        return prim


class MeshScene(Struct, frozen=True, kw_only=True):
    prim: PrimKind
    up_axis: UpAxis = UpAxis.Z
    meters_per_unit: float = 1.0
    subdiv: SubdivScheme = SubdivScheme.NONE
    transform: tuple[XformOp, ...] = ()
    display_color: NDArray[np.float32] | None = None
    material: Material | None = None
    labels: frozendict[str, tuple[str, ...]] = frozendict()
    model_kind: ModelKind | None = None
    arcs: tuple[AssetArc, ...] = ()

    def author(self, stage: object) -> object:
        if not np.isfinite(self.meters_per_unit) or self.meters_per_unit <= 0.0:
            raise ValueError("meters_per_unit must be positive and finite")
        UsdGeom.SetStageUpAxis(stage, self.up_axis.token)
        UsdGeom.SetStageMetersPerUnit(stage, self.meters_per_unit)
        root = Sdf.Path("/Root")
        prim = self.prim.define(stage, root, self.subdiv)
        for op in self.transform:
            op.add(UsdGeom.Xformable(prim))
        _dress(stage, prim, root, self.display_color, self.material, self.labels)
        for arc in self.arcs:
            arc.compose(prim)
        if self.model_kind is not None:
            Usd.ModelAPI(prim).SetKind(self.model_kind.token)
        stage.SetDefaultPrim(prim)
        return prim


class PackageFacts(Struct, frozen=True, gc=False):
    layers: int = 0
    references: int = 0
    assets: int = 0
    checks: int = 0
    warnings: int = 0

    @staticmethod
    def combined(left: "PackageFacts", right: "PackageFacts", /) -> "PackageFacts":
        return PackageFacts(
            layers=max(left.layers, right.layers),
            references=max(left.references, right.references),
            assets=max(left.assets, right.assets),
            checks=left.checks + right.checks,
            warnings=left.warnings + right.warnings,
        )


@tagged_union(frozen=True)
class PackageFault:
    tag: PackageFaultTag = tag()
    package: str = case()
    extract: str = case()
    verify: tuple[str, tuple[str, ...]] = case()
    relocate: str = case()


@tagged_union(frozen=True)
class StageOp:
    tag: StageOpTag = tag()
    render_export: object = case()
    mesh_author: MeshScene = case()

    @staticmethod
    def RenderExport(render_window: object) -> "StageOp":
        return StageOp(render_export=render_window)

    @staticmethod
    def MeshAuthor(scene: MeshScene) -> "StageOp":
        return StageOp(mesh_author=scene)

    def apply(self, path: str) -> tuple[bytes, frozendict[str, float | str]]:
        LayerFormat.of_suffix(path)
        match self:
            case StageOp(tag="render_export", render_export=render_window):
                exporter = _usd_exporter()()
                exporter.SetRenderWindow(render_window)
                exporter.SetFileName(path)
                exporter.Write()
                out = Path(path)
                if not out.exists() or not out.stat().st_size:
                    raise ValueError(f"USD render export produced no output: {path}")
                return out.read_bytes(), frozendict()
            case StageOp(tag="mesh_author", mesh_author=scene):
                stage = Usd.Stage.CreateInMemory(path)
                prim = scene.author(stage)
                stats = UsdUtils.ComputeUsdStageStats(stage)
                if not stats["totalPrimCount"]:
                    raise ValueError("empty stage: zero prims authored")
                if not stage.Export(path):
                    raise ValueError(f"USD stage export failed: {path}")
                resolved = {
                    f"labels:{taxonomy}": ",".join(sorted(UsdSemantics.LabelsQuery(taxonomy, Usd.TimeCode.Default()).ComputeUniqueInheritedLabels(prim)))
                    for taxonomy in scene.labels
                }
                facts: frozendict[str, float | str] = frozendict({
                    "prims": float(stats["totalPrimCount"]),
                    "layers": float(stats["usedLayerCount"]),
                    "up_axis": scene.up_axis.value,
                    "meters_per_unit": scene.meters_per_unit,
                    "extent_diag": _diagonal(prim),
                    **resolved,
                })
                return Path(path).read_bytes(), facts
            case _:
                assert_never(self)


@tagged_union(frozen=True)
class PackageOp:
    tag: PackageOpTag = tag()
    package: tuple[str, str, UsdzProfile] = case()
    extract: tuple[str, str] = case()
    verify: tuple[str, UsdzProfile] = case()
    relocate: tuple[str, Callable[[str], str]] = case()

    @staticmethod
    def Package(source: str, sink: str, profile: UsdzProfile = UsdzProfile.STANDARD) -> "PackageOp":
        return PackageOp(package=(source, sink, profile))

    @staticmethod
    def Extract(package: str, into: str) -> "PackageOp":
        return PackageOp(extract=(package, into))

    @staticmethod
    def Verify(package: str, profile: UsdzProfile = UsdzProfile.STANDARD) -> "PackageOp":
        return PackageOp(verify=(package, profile))

    @staticmethod
    def Relocate(layer: str, rewrite: Callable[[str], str]) -> "PackageOp":
        return PackageOp(relocate=(layer, rewrite))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _extent(points: NDArray[np.float32]) -> object:
    return Vt.Vec3fArray.FromNumpy(np.stack([points.min(axis=0), points.max(axis=0)]))


def _identity(texture: Texture, /) -> str:
    return blake2b(msgspec.json.encode(texture), digest_size=8).hexdigest()


def _placed(stage: object, path: object, token: str, reader: object, transform: UvTransform | None, /) -> object:
    if transform is None:
        return reader
    placed = UsdShade.Shader.Define(stage, path.AppendChild(f"Sampler{token}Place"))
    placed.CreateIdAttr("UsdTransform2d")
    placed.CreateInput("in", Sdf.ValueTypeNames.Float2).ConnectToSource(reader.ConnectableAPI(), "result")
    placed.CreateInput("rotation", Sdf.ValueTypeNames.Float).Set(transform.rotation)
    placed.CreateInput("scale", Sdf.ValueTypeNames.Float2).Set(Gf.Vec2f(*transform.scale))
    placed.CreateInput("translation", Sdf.ValueTypeNames.Float2).Set(Gf.Vec2f(*transform.translation))
    return placed


def _sampler(stage: object, path: object, texture: Texture, /) -> object:
    token = _identity(texture)
    reader = UsdShade.Shader.Define(stage, path.AppendChild(f"{texture.st}Reader"))
    reader.CreateIdAttr("UsdPrimvarReader_float2")
    reader.CreateInput("varname", Sdf.ValueTypeNames.String).Set(texture.st)
    sampler = UsdShade.Shader.Define(stage, path.AppendChild(f"Sampler{token}"))
    sampler.CreateIdAttr("UsdUVTexture")
    sampler.CreateInput("file", Sdf.ValueTypeNames.Asset).Set(Sdf.AssetPath(texture.file))
    sampler.CreateInput("st", Sdf.ValueTypeNames.Float2).ConnectToSource(_placed(stage, path, token, reader, texture.transform).ConnectableAPI(), "result")
    sampler.CreateInput("wrapS", Sdf.ValueTypeNames.Token).Set(texture.wrap_s.value)
    sampler.CreateInput("wrapT", Sdf.ValueTypeNames.Token).Set(texture.wrap_t.value)
    sampler.CreateInput("sourceColorSpace", Sdf.ValueTypeNames.Token).Set(texture.color_space.value)
    sampler.CreateInput("scale", Sdf.ValueTypeNames.Float4).Set(Gf.Vec4f(*texture.scale))
    sampler.CreateInput("bias", Sdf.ValueTypeNames.Float4).Set(Gf.Vec4f(*texture.bias))
    sampler.CreateInput("fallback", Sdf.ValueTypeNames.Float4).Set(Gf.Vec4f(*texture.fallback))
    return sampler


def _dress(
    stage: object,
    prim: object,
    path: object,
    display_color: NDArray[np.float32] | None,
    material: Material | None,
    labels: frozendict[str, tuple[str, ...]],
    /,
) -> None:
    if display_color is not None:
        if display_color.ndim not in (1, 2) or display_color.shape[-1] != 3 or not display_color.size or not np.isfinite(display_color).all():
            raise ValueError("display_color requires a nonempty finite RGB or (N, 3) buffer")
        colors = np.atleast_2d(display_color)
        interp = UsdGeom.Tokens.vertex if display_color.ndim == 2 else UsdGeom.Tokens.constant
        UsdGeom.PrimvarsAPI(prim).CreatePrimvar("displayColor", Sdf.ValueTypeNames.Color3fArray, interp).Set(Vt.Vec3fArray.FromNumpy(colors))
    if material is not None:
        material.bind(stage, prim, path.AppendChild("Material"))
    for taxonomy, tags in sorted(labels.items()):
        UsdSemantics.LabelsAPI.Apply(prim, taxonomy).CreateLabelsAttr(Vt.TokenArray(list(tags)))


def _instancer(stage: object, path: object, inst: Instancing, subdiv: SubdivScheme, /) -> object:
    instancer = UsdGeom.PointInstancer.Define(stage, path)
    protos = UsdGeom.Scope.Define(stage, path.AppendChild("Prototypes")).GetPath()
    prototypes = instancer.CreatePrototypesRel()
    for rank, prototype in enumerate(inst.prototypes):
        prototypes.AddTarget(prototype.define(stage, protos.AppendChild(f"proto{rank}"), subdiv).GetPath())
    instancer.CreateProtoIndicesAttr(Vt.IntArray.FromNumpy(inst.proto_indices))
    instancer.CreatePositionsAttr(Vt.Vec3fArray.FromNumpy(inst.positions))
    if inst.scales is not None:
        instancer.CreateScalesAttr(Vt.Vec3fArray.FromNumpy(inst.scales))
    if inst.orientations is not None:
        wxyz = np.ascontiguousarray(np.roll(inst.orientations, -1, axis=1), dtype=np.float32)
        instancer.CreateOrientationsAttr(Vt.QuathArray.FromNumpy(wxyz))
    instancer.CreateExtentAttr(_extent(inst.positions))
    return instancer.GetPrim()


def _camera(stage: object, path: object, lens: Lens, /) -> object:
    camera = UsdGeom.Camera.Define(stage, path)
    camera.CreateProjectionAttr(lens.projection.token)
    camera.CreateFocalLengthAttr(lens.focal_length)
    camera.CreateClippingRangeAttr(Gf.Vec2f(*lens.clip))
    camera.AddTransformOp().Set(Gf.Matrix4d(*_look_at(lens.eye, lens.target, lens.up)))
    return camera.GetPrim()


def _look_at(eye: Vec3, target: Vec3, up: Vec3, /) -> Matrix4:
    e = np.asarray(eye, dtype=np.float64)
    z = e - np.asarray(target, dtype=np.float64)
    z_norm = float(np.linalg.norm(z))
    if z_norm == 0.0:
        raise ValueError("degenerate camera frame: eye == target")
    z = z / z_norm
    x = np.cross(np.asarray(up, dtype=np.float64), z)
    x_norm = float(np.linalg.norm(x))
    if x_norm == 0.0:
        raise ValueError("degenerate camera frame: up parallel to the view direction")
    x = x / x_norm
    y = np.cross(z, x)
    basis = np.eye(4, dtype=np.float64)
    basis[0, :3], basis[1, :3], basis[2, :3], basis[3, :3] = x, y, z, e
    return tuple(float(cell) for cell in basis.reshape(-1))


def _diagonal(prim: object, /) -> float:
    box = UsdGeom.BBoxCache(Usd.TimeCode.Default(), [UsdGeom.Tokens.default_]).ComputeWorldBound(prim).ComputeAlignedRange()
    return 0.0 if box.IsEmpty() else float(np.linalg.norm(np.asarray(box.GetMax(), dtype=np.float64) - np.asarray(box.GetMin(), dtype=np.float64)))


def _usd_exporter() -> type:
    try:
        present = find_spec("vtkmodules.vtkIOUSD") is not None
    except ModuleNotFoundError:
        present = False
    if not present:
        raise ImportError("vtkUSDExporter needs a VTK build enabling vtkIOUSD against OpenUSD; the resident build ships no vtkmodules.vtkIOUSD")
    return import_module("vtkmodules.vtkIOUSD").vtkUSDExporter


def export_usd(render_window: object, path: str) -> tuple[bytes, frozendict[str, float | str]]:
    return StageOp.RenderExport(render_window).apply(path)


def author_mesh(scene: MeshScene, path: str) -> tuple[bytes, frozendict[str, float | str]]:
    return StageOp.MeshAuthor(scene).apply(path)


def _dependencies(asset: str, /) -> PackageFacts:
    layers, references, assets = UsdUtils.ComputeAllDependencies(Sdf.AssetPath(asset))
    return PackageFacts(layers=len(layers), references=len(references), assets=len(assets))


def packaged(op: PackageOp) -> Result[PackageFacts, PackageFault]:
    match op:
        case PackageOp(tag="package", package=(source, sink, profile)):
            built = UsdUtils.CreateNewARKitUsdzPackage if profile.arkit else UsdUtils.CreateNewUsdzPackage
            return Ok(_dependencies(sink)) if built(Sdf.AssetPath(source), sink) else Error(PackageFault(package=sink))
        case PackageOp(tag="extract", extract=(package, into)):
            return Ok(_dependencies(package)) if UsdUtils.ExtractUsdzPackage(package, into, True, False, True) else Error(PackageFault(extract=package))
        case PackageOp(tag="verify", verify=(package, profile)):
            checker = UsdUtils.ComplianceChecker(arkit=profile.arkit)
            checker.CheckCompliance(package)
            issues = tuple((*checker.GetErrors(), *checker.GetFailedChecks()))
            measured = msgspec.structs.replace(_dependencies(package), checks=len(checker.GetRules()), warnings=len(checker.GetWarnings()))
            return Ok(measured) if not issues else Error(PackageFault(verify=(package, issues)))
        case PackageOp(tag="relocate", relocate=(layer_path, rewrite)):
            layer = Sdf.Layer.FindOrOpen(layer_path)
            if layer is None:
                return Error(PackageFault(relocate=layer_path))
            UsdUtils.ModifyAssetPaths(layer, rewrite)
            if not layer.Save():
                return Error(PackageFault(relocate=layer_path))
            return Ok(_dependencies(layer_path))
        case _:
            assert_never(op)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
