# [PY_ARTIFACTS_SCENE_SPEC]

`SceneGrid`, `RenderSpec`, and the closed scene vocabularies form the parse-floor seam both interpreters resolve: every value crossing the pickled process lane is admitted, frozen, and provider-free here, and a worker kernel crosses by qualified name through the runtime `execution/workers#FABRIC` `Kernel`/`shipped` gate — `WORKER_MODULE` names the worker floor once as this page's composed constant. Provider types remain quoted `TYPE_CHECKING` annotations, so importing this module loads no native provider on either floor.

Pickle-by-reference re-imports the defining module on the far interpreter, so this module parses and imports on both floors by construction — no gated native, no runtime-only syntax. `SceneGrid.spans` feeds the `scene/render#SCENE` identity preimage; `SceneTarget` and `SceneSource` compose downward into `scene/export#EXPORT` and the worker tables; `render.py`, `render_worker.py`, and `export.py` import this floor, `media/container#CONTAINER` composes its `framed`/`CANON` identity-preimage discipline, the `pxr`-only `scene/stage#STAGE` leaf stands apart, and this floor imports no scene module.

## [01]-[INDEX]

- [02]-[SPEC]: the admitted `SceneGrid` owner, the `RenderSpec` policy value with its bound `staged`/`added`/`viewed` projections over the closed style, camera, normalization, feature, and filter families, and the `WORKER_MODULE` floor anchor the runtime kernel crossing resolves against.

## [02]-[SPEC]

- Owner: `RenderSpec` carries its own `staged`/`added`/`viewed` projections so a new render knob is one band field bound into an existing projection, never a constructor-parameter tail; `scalars` and `colormap` default, so a field-free GLB or CSG render is spellable and `_pruned` drops the absent scalar band at every provider call. `Style` is one closed-payload style family whose every case carries exactly its own provider band, so each behavior travels with the style that selects it; `LinesBand.passes` derives wireframe and silhouette behavior through `_LINE`.
- Cases: `ColorNorm` closes the scalar-normalization vocabulary, `symlog` and `diverging` carrying their thresholds as payload while the worker `_norm` arm owns each matplotlib projection. `Camera` holds `auto` framing, an explicit `pose`, or a `standard` drafting viewpoint over `StandardView`. `SceneLight` carries the azimuth/elevation/intensity/color values both `pv.Light.set_direction_angle` and the authored `UsdLux.DistantLight` consume; `LightPreset` selects the `Plotter(lighting=)` base rig, and `RenderFeature.SHADOWS` casts the resulting lights. `RenderFeature` collapses publication behavior into the `_FEATURE`-folded `frozenset`. `OrbitPath` parameterizes azimuth, radius, and elevation intervals, so one generator spans turntables, partial arcs, reverse sweeps, dollies, and helices.
- Entry: `WORKER_MODULE` names the worker floor once — a kernel travels as its `(module, name)` pair inside the runtime `Kernel` value, never as an object whose defining module the far interpreter cannot load, and the crossing law lives at `execution/workers#FABRIC`, composed here with zero re-derivation. `framed` and `CANON` fix the length-framed, deterministic-msgpack identity-preimage discipline; `scene/render#SCENE` and `media/container#CONTAINER` compose them rather than forking a local copy.
- Auto: `SceneGrid` factories prove each payload once — every structural, finiteness, bounds, and cardinality invariant its case carries — returning `Result[SceneGrid, GridFault]`, so every downstream consumer trusts the evidence and constructs without revalidation. `source` carries a `SourceKind` parametric primitive whose param names stay caller data — the worker factory row alone refuses an unknown kwarg — so context massing and furniture never route through a mesh round-trip. `spans` lowers each case onto the byte chunks the render identity preimage digests. `RenderSpec.staged` folds `FieldFilter` through `reduce`; `RenderSpec.added` binds only its selected `Style` band; every `FieldFilter` factory and the `SceneLight`/`RenderSpec` mints prove their policy scalars at construction (finiteness, ordered ranges, positive counts), so no unproven render value crosses to the worker; `_LINE` derives line passes; `aa_samples` tunes the `AntiAlias.MSAA` arm alone; `OrbitPath.of` proves samples, finiteness, and a positive radius before any interval reaches a generator; an import-time coverage gate proves `_FEATURE`, `_LINE`, and `_VIEW` span their vocabularies, so an unruled member fails at import, never as a runtime key miss.
- Growth: a new render knob is one band field bound into an existing projection or capture call; a new quality toggle is one `RenderFeature` member plus one `_FEATURE` row; a new line pass is one `LinePass` member plus one `_LINE` row; a new render style is one `Style` case carrying its own band plus one `added` arm; a new filter is one `FieldFilter` case plus one `apply` arm, and a filter kwarg is one payload slot on its existing case; a new scalar normalization is one `ColorNorm` case with its worker `_norm` arm; a new viewpoint preset is one `StandardView` member plus one `_VIEW` row; a new light is one `SceneLight` value, and a new base rig is one `LightPreset` member; orbit growth changes the three interval values rather than the type surface; a new grid source is one `SceneGrid` case plus one admission gate, one `spans` arm, and one worker `_hydrate` arm; a new parametric primitive is one `SourceKind` member plus one worker `_SOURCE` row; a new export target or importable format is one `SceneTarget` or `SceneSource` member, priced at the consumer table that proves coverage.
- Boundary: `FieldFilter.apply` and the `RenderSpec` projections take live provider handles only as quoted parameter types — the worker floor supplies the objects, this floor owns the dispatch bodies over them; the single-operand `FieldFilter` is why binary CSG rides `scene/render#SCENE`'s dedicated two-operand modality.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from itertools import chain
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from builtins import frozendict
from expression import Error, Ok, Result, case, option, tag, tagged_union
from msgspec import Struct
from msgspec.msgpack import Encoder
from numpy.typing import NDArray

if TYPE_CHECKING:
    import pyvista as pv

# --- [TYPES] ---------------------------------------------------------------------------

type Vec3 = tuple[float, float, float]
type Plane = tuple[Vec3, Vec3]
type Bounds = tuple[float, float, float, float, float, float]
type ScalarRange = tuple[float, float]
type Frames = tuple[NDArray[np.uint8], ...]
type Palette = str | tuple[str, ...]
type SourceParam = float | str | Vec3 | Bounds
type GridFault = Literal["<empty>", "<bad-rank>", "<non-finite>", "<index-out-of-range>", "<field-mismatch>", "<bad-axis>", "<not-glb>"]
type OrbitFault = Literal["<bad-samples>", "<non-finite>", "<bad-radius>"]
type ContourMethod = Literal["contour", "marching_cubes", "flying_edges"]
type FlowDirection = Literal["both", "backward", "forward"]
type GridTag = Literal["mesh", "volume", "rect", "struct", "glb", "source"]
type StyleTag = Literal["surface", "volume", "points", "lines", "arrows", "labels"]
type CameraTag = Literal["auto", "pose", "standard"]
type NormTag = Literal["linear", "log", "symlog", "diverging"]
type FieldFilterTag = Literal[
    "clip",
    "clip_box",
    "clip_scalar",
    "slice",
    "slice_orthogonal",
    "threshold",
    "contour",
    "warp",
    "warp_vector",
    "glyph",
    "streamlines",
    "decimate",
    "surface",
    "feature_edges",
    "smooth",
    "subdivide",
    "fill_holes",
    "clean",
    "cell_to_point",
    "point_to_cell",
]

# --- [CONSTANTS] -----------------------------------------------------------------------

_GLB_MAGIC = b"glTF"
_ORIGIN: Vec3 = (0.0, 0.0, 0.0)
_UP: Vec3 = (0.0, 0.0, 1.0)
# one worker-floor anchor: render.py builds its Kernel values against this module name, and the worker
# floor's import-time `covered` witness proves every kernel name resolves inside it.
WORKER_MODULE = "rasm.artifacts.scene.render_worker"


class SceneTarget(StrEnum):
    PNG = "png"
    SVG = "svg"
    PDF = "pdf"
    EPS = "eps"
    PS = "ps"
    TEX = "tex"
    GLTF = "gltf"
    VRML = "vrml"
    OBJ = "obj"
    HTML = "html"
    USD = "usdc"
    USDZ = "usdz"


class SceneSource(StrEnum):
    GLTF = "gltf"
    GLB = "glb"
    OBJ = "obj"
    VRML = "vrml"


class SourceKind(StrEnum):
    SPHERE = "sphere"
    ICOSPHERE = "icosphere"
    CUBE = "cube"
    BOX = "box"
    CYLINDER = "cylinder"
    CONE = "cone"
    TUBE = "tube"
    PLANE = "plane"
    DISC = "disc"
    CIRCLE = "circle"
    POLYGON = "polygon"
    ARROW = "arrow"
    LINE = "line"
    TEXT = "text"


class BoolOp(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"
    SAMPLE = "sample"


class AntiAlias(StrEnum):
    SSAA = "ssaa"
    MSAA = "msaa"
    FXAA = "fxaa"


class RenderFeature(StrEnum):
    AXES = "axes"
    SCALAR_BAR = "scalar_bar"
    SSAO = "ssao"
    DEPTH_PEEL = "depth_peel"
    EYE_DOME = "eye_dome"
    SHADOWS = "shadows"
    PARALLEL = "parallel"
    HIDDEN_LINE = "hidden_line"


class LightPreset(StrEnum):
    LIGHT_KIT = "light kit"
    THREE_LIGHTS = "three lights"
    NONE = "none"


class LinePass(StrEnum):
    WIREFRAME = "wireframe"
    SILHOUETTE = "silhouette"


class FieldAssociation(StrEnum):
    POINT = "point"
    CELL = "cell"


class StandardView(StrEnum):
    PLAN = "plan"
    FRONT = "front"
    SIDE = "side"
    ISOMETRIC = "isometric"


# --- [MODELS] --------------------------------------------------------------------------


class OrbitPath(Struct, frozen=True, gc=False):
    samples: int
    azimuth: tuple[float, float] = (0.0, 360.0)  # degrees; radius and elevation intervals scale the framed camera distance the worker derives
    radius: tuple[float, float] = (1.0, 1.0)
    elevation: tuple[float, float] = (0.0, 0.0)

    @staticmethod
    def of(
        samples: int,
        *,
        azimuth: tuple[float, float] = (0.0, 360.0),
        radius: tuple[float, float] = (1.0, 1.0),
        elevation: tuple[float, float] = (0.0, 0.0),
    ) -> Result["OrbitPath", OrbitFault]:
        values = (*azimuth, *radius, *elevation)
        return (
            Error("<bad-samples>")
            if samples < 1
            else Error("<non-finite>")
            if not np.isfinite(values).all()
            else Error("<bad-radius>")
            if min(radius) <= 0.0
            else Ok(OrbitPath(samples=samples, azimuth=azimuth, radius=radius, elevation=elevation))
        )


class SurfaceBand(Struct, frozen=True, gc=False):
    pbr: bool = False
    metallic: float | None = None
    roughness: float | None = None
    show_edges: bool = False
    ambient: float | None = None
    diffuse: float | None = None
    specular: float | None = None
    smooth_shading: bool = False
    culling: Literal["back", "front"] | None = None


class VolumeBand(Struct, frozen=True, gc=False):
    transfer: str | None = None
    blending: str | None = None
    mapper: str | None = None
    shade: bool = False


class PointsBand(Struct, frozen=True, gc=False):
    point_size: float | None = None
    spheres: bool = False


class LinesBand(Struct, frozen=True):
    width: float | None = None
    silhouette_angle: float | None = None
    passes: frozenset[LinePass] = frozenset({LinePass.WIREFRAME})


class ArrowsBand(Struct, frozen=True, gc=False):
    mag: float = 1.0


class LabelsBand(Struct, frozen=True, gc=False):
    font_size: int | None = None
    always_visible: bool = False


class SceneLight(Struct, frozen=True, gc=False):
    azimuth: float
    elevation: float
    intensity: float = 1.0
    color: Vec3 = (1.0, 1.0, 1.0)

    def __post_init__(self) -> None:
        # msgspec runs no field validation, so the mint itself proves the light — the Camera.Pose raise pattern at struct scope.
        if not np.isfinite((self.azimuth, self.elevation, self.intensity, *self.color)).all() or self.intensity < 0.0:
            raise ValueError("scene light requires finite azimuth/elevation/color and a non-negative intensity")


class FieldData(Struct, frozen=True):
    values: NDArray[np.float64]
    association: FieldAssociation = FieldAssociation.POINT


@tagged_union(frozen=True)
class Style:
    tag: StyleTag = tag()
    surface: SurfaceBand = case()
    volume: VolumeBand = case()
    points: PointsBand = case()
    lines: LinesBand = case()
    arrows: ArrowsBand = case()
    labels: LabelsBand = case()

    @staticmethod
    def Surface(band: SurfaceBand = SurfaceBand()) -> "Style":
        return Style(surface=band)

    @staticmethod
    def Volume(band: VolumeBand = VolumeBand()) -> "Style":
        return Style(volume=band)

    @staticmethod
    def Points(band: PointsBand = PointsBand()) -> "Style":
        return Style(points=band)

    @staticmethod
    def Lines(band: LinesBand = LinesBand()) -> "Style":
        return Style(lines=band)

    @staticmethod
    def Arrows(band: ArrowsBand = ArrowsBand()) -> "Style":
        return Style(arrows=band)

    @staticmethod
    def Labels(band: LabelsBand = LabelsBand()) -> "Style":
        return Style(labels=band)


@tagged_union(frozen=True)
class Camera:
    tag: CameraTag = tag()
    auto: None = case()
    pose: tuple[Vec3, Vec3 | None, Vec3 | None] = case()
    standard: tuple[StandardView, bool] = case()

    @staticmethod
    def Auto() -> "Camera":
        return Camera(auto=None)

    @staticmethod
    def Pose(position: Vec3, focal_point: Vec3 | None = None, view_up: Vec3 | None = None) -> "Camera":
        focal = focal_point if focal_point is not None else _ORIGIN
        if not np.isfinite((*position, *focal, *(view_up if view_up is not None else _UP))).all() or tuple(position) == tuple(focal):
            raise ValueError("camera pose requires finite values and a position distinct from the focal point")
        return Camera(pose=(position, focal_point, view_up))

    @staticmethod
    def Standard(view: StandardView, negative: bool = False) -> "Camera":
        return Camera(standard=(view, negative))


@tagged_union(frozen=True)
class ColorNorm:
    tag: NormTag = tag()
    linear: None = case()
    log: None = case()
    symlog: float = case()
    diverging: float = case()

    @staticmethod
    def Linear() -> "ColorNorm":
        return ColorNorm(linear=None)

    @staticmethod
    def Log() -> "ColorNorm":
        return ColorNorm(log=None)

    @staticmethod
    def Symlog(linthresh: float) -> "ColorNorm":
        # SymLogNorm law proven at mint: linthresh is the strictly positive linear-band half-width — zero or
        # non-finite degenerates the mapping at the worker, far from the caller who chose it.
        if not (np.isfinite(linthresh) and linthresh > 0.0):
            raise ValueError(f"symlog linthresh is a finite positive half-width: {linthresh!r}")
        return ColorNorm(symlog=linthresh)

    @staticmethod
    def Diverging(vcenter: float = 0.0) -> "ColorNorm":
        if not np.isfinite(vcenter):
            raise ValueError(f"diverging vcenter is finite: {vcenter!r}")
        return ColorNorm(diverging=vcenter)

    @property
    def premapped(self) -> bool:
        # symlog and diverging pre-map at the worker because no plotter arm accepts a matplotlib norm; log rides add_mesh(log_scale=) natively
        return self.tag in ("symlog", "diverging")


@tagged_union(frozen=True)
class SceneGrid:
    tag: GridTag = tag()
    mesh: tuple[NDArray[np.float64], NDArray[np.int64] | None, frozendict[str, FieldData]] = case()
    volume: tuple[NDArray[np.float64], Vec3, Vec3] = case()
    rect: tuple[NDArray[np.float64], NDArray[np.float64], NDArray[np.float64], NDArray[np.float64]] = case()
    struct: tuple[NDArray[np.float64], NDArray[np.float64], NDArray[np.float64], NDArray[np.float64]] = case()
    glb: bytes = case()
    source: tuple[SourceKind, frozendict[str, SourceParam]] = case()

    @staticmethod
    def of_mesh(
        points: NDArray[np.float64], faces: NDArray[np.int64] | None = None, fields: frozendict[str, FieldData] = frozendict()
    ) -> Result["SceneGrid", GridFault]:
        return (
            Error("<empty>")
            if points.size == 0
            else Error("<bad-rank>")
            if points.ndim != 2 or points.shape[1] != 3 or (faces is not None and (faces.ndim != 2 or faces.shape[1] < 3))
            else Error("<non-finite>")
            if not np.isfinite(points).all() or any(not np.isfinite(field.values).all() for field in fields.values())
            else Error("<index-out-of-range>")
            if faces is not None and faces.size and (faces.min() < 0 or faces.max() >= len(points))
            else Error("<field-mismatch>")
            if any(
                field.values.ndim not in (1, 2)
                or field.values.shape[0] != (len(points) if field.association is FieldAssociation.POINT else (0 if faces is None else len(faces)))
                for field in fields.values()
            )
            else Ok(SceneGrid(mesh=(points, faces, fields)))
        )

    @staticmethod
    def of_volume(field: NDArray[np.float64], spacing: Vec3 = (1.0, 1.0, 1.0), origin: Vec3 = _ORIGIN) -> Result["SceneGrid", GridFault]:
        return (
            Error("<empty>")
            if field.size == 0
            else Error("<bad-rank>")
            if field.ndim != 3
            else Error("<non-finite>")
            if not np.isfinite(field).all() or not np.isfinite(spacing).all() or not np.isfinite(origin).all() or any(step <= 0.0 for step in spacing)
            else Ok(SceneGrid(volume=(field, spacing, origin)))
        )

    @staticmethod
    def of_rect(
        field: NDArray[np.float64], x: NDArray[np.float64], y: NDArray[np.float64], z: NDArray[np.float64]
    ) -> Result["SceneGrid", GridFault]:
        axes = (x, y, z)
        return (
            Error("<empty>")
            if field.size == 0
            else Error("<bad-rank>")
            if field.ndim != 3 or any(axis.ndim != 1 for axis in axes) or field.shape != tuple(len(axis) for axis in axes)
            else Error("<non-finite>")
            if not np.isfinite(field).all() or any(not np.isfinite(axis).all() for axis in axes)
            else Error("<bad-axis>")
            if any(axis.size < 2 or (axis[1:] <= axis[:-1]).any() for axis in axes)
            else Ok(SceneGrid(rect=(field, x, y, z)))
        )

    @staticmethod
    def of_struct(
        field: NDArray[np.float64], x: NDArray[np.float64], y: NDArray[np.float64], z: NDArray[np.float64]
    ) -> Result["SceneGrid", GridFault]:
        return (
            Error("<empty>")
            if field.size == 0
            else Error("<bad-rank>")
            if field.ndim != 3 or any(axis.shape != field.shape for axis in (x, y, z))
            else Error("<non-finite>")
            if not np.isfinite(field).all() or any(not np.isfinite(axis).all() for axis in (x, y, z))
            else Ok(SceneGrid(struct=(field, x, y, z)))
        )

    @staticmethod
    def of_glb(data: bytes) -> Result["SceneGrid", GridFault]:
        # full GLB 2.0 container proof, never a header sniff: after the 12-byte header, the chunk walk — each 8-byte
        # chunk header in bounds, 4-aligned payloads, the mandatory JSON chunk first, BIN only as its immediate
        # successor — must consume the declared length exactly, so a truncated, overflowing, or misordered
        # container refuses at admission instead of faulting inside the far-floor importer.
        if len(data) < 12 or data[:4] != _GLB_MAGIC or int.from_bytes(data[4:8], "little") != 2 or int.from_bytes(data[8:12], "little") != len(data):
            return Error("<not-glb>")
        offset, kinds = 12, []
        while offset < len(data):  # chunk-table walk over hostile bytes: the one measured parse kernel this factory owns
            if offset + 8 > len(data):
                return Error("<not-glb>")
            span = int.from_bytes(data[offset : offset + 4], "little")
            kinds.append(data[offset + 4 : offset + 8])
            offset += 8 + span
            if span % 4 or offset > len(data):
                return Error("<not-glb>")
        ordered = kinds[:1] == [b"JSON"] and b"JSON" not in kinds[1:] and all(kind != b"BIN\x00" for kind in kinds[2:])
        return Ok(SceneGrid(glb=data)) if ordered else Error("<not-glb>")

    @staticmethod
    def of_source(kind: SourceKind, params: frozendict[str, SourceParam] = frozendict()) -> Result["SceneGrid", GridFault]:
        finite = all(np.isfinite(np.asarray(value, dtype=np.float64)).all() for value in params.values() if not isinstance(value, str))
        return Ok(SceneGrid(source=(kind, params))) if finite else Error("<non-finite>")

    def spans(self) -> tuple[bytes, ...]:
        # Canonical byte projection for the pre-run identity preimage: fixed chunk strides keep the fold injective.
        match self:
            case SceneGrid(tag="mesh", mesh=(points, faces, fields)):
                return (
                    b"mesh",
                    *_shaped(points),
                    *((b"", b"", b"") if faces is None else _shaped(faces)),
                    *(chunk for name, field in sorted(fields.items()) for chunk in (name.encode(), field.association.value.encode(), *_shaped(field.values))),
                )
            case SceneGrid(tag="volume", volume=(field, spacing, origin)):
                return (b"volume", *_shaped(field), np.asarray(spacing, dtype="<f8").tobytes(), np.asarray(origin, dtype="<f8").tobytes())
            case SceneGrid(tag="rect", rect=(field, x, y, z)) | SceneGrid(tag="struct", struct=(field, x, y, z)):
                return (self.tag.encode(), *_shaped(field), *_shaped(x), *_shaped(y), *_shaped(z))
            case SceneGrid(tag="glb", glb=data):
                return (b"glb", data)
            case SceneGrid(tag="source", source=(kind, params)):
                return (
                    b"source",
                    kind.value.encode(),
                    *(
                        chunk
                        for name, value in sorted(params.items())
                        for chunk in (name.encode(), value.encode() if isinstance(value, str) else np.asarray(value, dtype="<f8").tobytes())
                    ),
                )
            case _:
                assert_never(self)


def _proved[T](value: T, ok: bool, rule: str, /) -> T:
    # one factory proof kernel: each FieldFilter constructor states its admission rule as data beside the value,
    # so `apply` forwards only proven filters and a malformed argument raises at the mint, never inside the provider.
    if not ok:
        raise ValueError(f"field filter requires {rule}")
    return value


def _finite(*values: float) -> bool:
    return bool(np.isfinite(values).all())


@tagged_union(frozen=True)
class FieldFilter:
    tag: FieldFilterTag = tag()
    clip: tuple[Plane, bool] = case()
    clip_box: Bounds = case()
    clip_scalar: tuple[float, bool] = case()
    slice: Plane = case()
    slice_orthogonal: Vec3 = case()
    threshold: ScalarRange = case()
    contour: tuple[tuple[float, ...], ContourMethod] = case()
    warp: float = case()
    warp_vector: float = case()
    glyph: tuple[float, bool, float | None, bool] = case()
    streamlines: tuple[Vec3, float | None, int, FlowDirection, float | None] = case()
    decimate: tuple[float, bool] = case()
    surface: None = case()
    feature_edges: float = case()
    smooth: int = case()
    subdivide: int = case()
    fill_holes: float = case()
    clean: None = case()
    cell_to_point: None = case()
    point_to_cell: None = case()

    @staticmethod
    def Clip(normal: Vec3, origin: Vec3, crinkle: bool = False) -> "FieldFilter":
        return _proved(FieldFilter(clip=((normal, origin), crinkle)), _finite(*normal, *origin), "a finite clip normal and origin")

    @staticmethod
    def ClipBox(bounds: Bounds) -> "FieldFilter":
        return _proved(FieldFilter(clip_box=bounds), _finite(*bounds), "finite clip-box bounds")

    @staticmethod
    def ClipScalar(value: float, invert: bool = False) -> "FieldFilter":
        return _proved(FieldFilter(clip_scalar=(value, invert)), _finite(value), "a finite clip-scalar value")

    @staticmethod
    def Slice(normal: Vec3, origin: Vec3) -> "FieldFilter":
        return _proved(FieldFilter(slice=(normal, origin)), _finite(*normal, *origin), "a finite slice normal and origin")

    @staticmethod
    def SliceOrthogonal(center: Vec3) -> "FieldFilter":
        return _proved(FieldFilter(slice_orthogonal=center), _finite(*center), "a finite slice center")

    @staticmethod
    def Threshold(lo: float, hi: float) -> "FieldFilter":
        return _proved(FieldFilter(threshold=(lo, hi)), _finite(lo, hi) and lo <= hi, "a finite ordered threshold range")

    @staticmethod
    def Contour(*isovalues: float, method: ContourMethod = "contour") -> "FieldFilter":
        return _proved(FieldFilter(contour=(isovalues, method)), not isovalues or _finite(*isovalues), "finite isovalues")

    @staticmethod
    def Warp(factor: float) -> "FieldFilter":
        return _proved(FieldFilter(warp=factor), _finite(factor), "a finite warp factor")

    @staticmethod
    def WarpVector(factor: float = 1.0) -> "FieldFilter":
        return _proved(FieldFilter(warp_vector=factor), _finite(factor), "a finite warp factor")

    @staticmethod
    def Glyph(factor: float, orient: bool = True, tolerance: float | None = None, clamping: bool = False) -> "FieldFilter":
        proven = _finite(factor) and (tolerance is None or (_finite(tolerance) and tolerance >= 0.0))
        return _proved(FieldFilter(glyph=(factor, orient, tolerance, clamping)), proven, "a finite glyph factor and non-negative tolerance")

    @staticmethod
    def Streamlines(
        source_center: Vec3,
        source_radius: float | None = None,
        n_points: int = 100,
        integration_direction: FlowDirection = "both",
        max_time: float | None = None,
    ) -> "FieldFilter":
        proven = (
            _finite(*source_center)
            and n_points >= 1
            and (source_radius is None or (_finite(source_radius) and source_radius > 0.0))
            and (max_time is None or (_finite(max_time) and max_time > 0.0))
        )
        return _proved(
            FieldFilter(streamlines=(source_center, source_radius, n_points, integration_direction, max_time)),
            proven,
            "a finite source center, positive n_points, and positive radius/max_time when given",
        )

    @staticmethod
    def Decimate(reduction: float, volume_preservation: bool = False) -> "FieldFilter":
        return _proved(FieldFilter(decimate=(reduction, volume_preservation)), _finite(reduction) and 0.0 <= reduction < 1.0, "a reduction in [0, 1)")

    @staticmethod
    def Surface() -> "FieldFilter":
        return FieldFilter(surface=None)

    @staticmethod
    def FeatureEdges(feature_angle: float = 30.0) -> "FieldFilter":
        return _proved(FieldFilter(feature_edges=feature_angle), _finite(feature_angle) and feature_angle > 0.0, "a positive feature angle")

    @staticmethod
    def Smooth(n_iter: int = 20) -> "FieldFilter":
        return _proved(FieldFilter(smooth=n_iter), n_iter >= 1, "a positive smoothing count")

    @staticmethod
    def Subdivide(nsub: int = 1) -> "FieldFilter":
        return _proved(FieldFilter(subdivide=nsub), nsub >= 1, "a positive subdivision count")

    @staticmethod
    def FillHoles(hole_size: float) -> "FieldFilter":
        return _proved(FieldFilter(fill_holes=hole_size), _finite(hole_size) and hole_size > 0.0, "a positive hole size")

    @staticmethod
    def Clean() -> "FieldFilter":
        return FieldFilter(clean=None)

    @staticmethod
    def CellToPoint() -> "FieldFilter":
        return FieldFilter(cell_to_point=None)

    @staticmethod
    def PointToCell() -> "FieldFilter":
        return FieldFilter(point_to_cell=None)

    def apply(self, dataset: "pv.DataSet", scalars: str | None) -> "pv.DataSet":
        match self:
            case FieldFilter(tag="clip", clip=((normal, origin), crinkle)):
                return dataset.clip(normal=normal, origin=origin, crinkle=crinkle)
            case FieldFilter(tag="clip_box", clip_box=bounds):
                return dataset.clip_box(bounds=bounds)
            case FieldFilter(tag="clip_scalar", clip_scalar=(value, invert)):
                return dataset.clip_scalar(scalars=scalars, value=value, invert=invert)
            case FieldFilter(tag="slice", slice=(normal, origin)):
                return dataset.slice(normal=normal, origin=origin)
            case FieldFilter(tag="slice_orthogonal", slice_orthogonal=(x, y, z)):
                return dataset.slice_orthogonal(x=x, y=y, z=z)
            case FieldFilter(tag="threshold", threshold=value_range):
                return dataset.threshold(value=value_range, scalars=scalars)
            case FieldFilter(tag="contour", contour=(isovalues, method)):
                return dataset.contour(isosurfaces=list(isovalues), scalars=scalars, method=method)
            case FieldFilter(tag="warp", warp=factor):
                return dataset.warp_by_scalar(scalars=scalars, factor=factor)
            case FieldFilter(tag="warp_vector", warp_vector=factor):
                return dataset.warp_by_vector(vectors=scalars, factor=factor)
            case FieldFilter(tag="glyph", glyph=(factor, orient, tolerance, clamping)):
                return dataset.glyph(scale=scalars, factor=factor, orient=orient, tolerance=tolerance, clamping=clamping)
            case FieldFilter(tag="streamlines", streamlines=(source_center, source_radius, n_points, direction, max_time)):
                return dataset.streamlines(
                    vectors=scalars,
                    source_center=source_center,
                    source_radius=source_radius,
                    n_points=n_points,
                    integration_direction=direction,
                    max_time=max_time,
                )
            case FieldFilter(tag="decimate", decimate=(reduction, volume_preservation)):
                return dataset.extract_surface().triangulate().decimate(target_reduction=reduction, volume_preservation=volume_preservation)
            case FieldFilter(tag="surface", surface=_):
                return dataset.extract_surface()
            case FieldFilter(tag="feature_edges", feature_edges=angle):
                return dataset.extract_surface().extract_feature_edges(feature_angle=angle)
            case FieldFilter(tag="smooth", smooth=n_iter):
                return dataset.extract_surface().smooth(n_iter=n_iter)
            case FieldFilter(tag="subdivide", subdivide=nsub):
                return dataset.extract_surface().triangulate().subdivide(nsub)
            case FieldFilter(tag="fill_holes", fill_holes=hole_size):
                return dataset.extract_surface().fill_holes(hole_size)
            case FieldFilter(tag="clean", clean=_):
                return dataset.extract_surface().clean()
            case FieldFilter(tag="cell_to_point", cell_to_point=_):
                return dataset.cell_data_to_point_data()
            case FieldFilter(tag="point_to_cell", point_to_cell=_):
                return dataset.point_data_to_cell_data()
            case _:
                assert_never(self)


class RenderSpec(Struct, frozen=True):
    scalars: str | None = None
    colormap: Palette = "viridis"
    norm: ColorNorm = ColorNorm(linear=None)
    window: tuple[int, int] = (1024, 768)
    scale: int | None = None  # screenshot(scale=) supersample: the capture rasterizes at window-times-scale, and receipts report the rasterized dims
    background: str = "white"
    clim: ScalarRange | None = None
    opacity: float | None = None
    style: Style = Style(surface=SurfaceBand())
    filters: tuple[FieldFilter, ...] = ()
    transparent: bool = False
    features: frozenset[RenderFeature] = frozenset()
    anti_aliasing: AntiAlias | None = None
    aa_samples: int | None = None
    camera: Camera = Camera(auto=None)
    zoom: float | None = None
    light_preset: LightPreset = LightPreset.LIGHT_KIT
    lights: tuple[SceneLight, ...] = ()
    title: str | None = None
    annotations: tuple[tuple[str, str], ...] = ()
    up_axis: Literal["y", "z"] = "z"
    meters_per_unit: float = 1.0
    labels: frozendict[str, tuple[str, ...]] = frozendict()

    def __post_init__(self) -> None:
        # render-policy admission at the mint: every scalar the worker forwards to the plotter proves here, so an
        # invalid window, scale, opacity, clim, zoom, sample count, or unit scale never crosses the render boundary.
        flawed = (
            len(self.window) != 2  # arity proven before positivity — a 1- or 3-tuple would pass a bare extent sweep
            or any(extent <= 0 for extent in self.window)
            or (self.scale is not None and self.scale < 1)
            or (self.opacity is not None and not (np.isfinite(self.opacity) and 0.0 <= self.opacity <= 1.0))
            or (self.clim is not None and not (np.isfinite(self.clim).all() and self.clim[0] <= self.clim[1]))
            or (self.zoom is not None and not (np.isfinite(self.zoom) and self.zoom > 0.0))
            or (self.aa_samples is not None and self.aa_samples < 1)
            or not (np.isfinite(self.meters_per_unit) and self.meters_per_unit > 0.0)
        )
        if flawed:
            raise ValueError("render spec requires a positive window/scale/zoom/unit-scale, opacity in [0, 1], an ordered finite clim, and aa_samples >= 1")

    def staged(self, dataset: "pv.DataSet") -> "pv.DataSet":
        return reduce(lambda field, flt: flt.apply(field, self.scalars), self.filters, dataset)

    def added(self, plotter: "pv.Plotter", mesh: "pv.DataSet") -> None:
        # a tuple palette crosses as a color list — the provider colormap registry keys str alone
        palette = list(self.colormap) if isinstance(self.colormap, tuple) else self.colormap
        # log axis rides EVERY add_mesh-family arm (surface, points, the wireframe line pass), never surface alone —
        # a log norm on a line or point style otherwise renders linear silently; add_volume owns no log axis kwarg.
        log_scale = True if self.norm.tag == "log" else None
        shared = {"scalars": self.scalars, "cmap": palette, "clim": self.clim, "opacity": self.opacity}
        match self.style:
            case Style(tag="surface", surface=band):
                material = {
                    "pbr": band.pbr or None,
                    "metallic": band.metallic,
                    "roughness": band.roughness,
                    "show_edges": band.show_edges or None,
                    "ambient": band.ambient,
                    "diffuse": band.diffuse,
                    "specular": band.specular,
                    "smooth_shading": band.smooth_shading or None,
                    "culling": band.culling,
                    "log_scale": log_scale,
                }
                plotter.add_mesh(mesh, **_pruned(shared | material))
            case Style(tag="volume", volume=band):
                volume = {**shared, "opacity": band.transfer or self.opacity, "blending": band.blending, "mapper": band.mapper, "shade": band.shade or None}
                plotter.add_volume(mesh, **_pruned(volume))
            case Style(tag="points", points=band):
                plotter.add_points(mesh, **_pruned(shared | {"point_size": band.point_size, "render_points_as_spheres": band.spheres or None, "log_scale": log_scale}))
            case Style(tag="lines", lines=band):
                tuple(_LINE[line_pass](plotter, mesh, shared | {"log_scale": log_scale}, band) for line_pass in sorted(band.passes))
            case Style(tag="arrows", arrows=band):
                if self.scalars is None or self.scalars not in mesh.point_data:
                    raise ValueError("arrows style requires a point-data field named by scalars")
                plotter.add_arrows(mesh.points, mesh[self.scalars], mag=band.mag)
            case Style(tag="labels", labels=band):
                if self.scalars is None or self.scalars not in mesh.point_data:
                    raise ValueError("labels style requires a point-data field named by scalars")
                plotter.add_point_labels(mesh.points, mesh[self.scalars], **_pruned({"font_size": band.font_size, "always_visible": band.always_visible or None}))
            case _:
                assert_never(self.style)

    def viewed(self, plotter: "pv.Plotter") -> None:
        plotter.set_background(self.background)
        tuple(_FEATURE[feature](plotter) for feature in sorted(self.features))
        option.of_optional(self.title).map(lambda title: plotter.add_text(title, position="upper_edge"))
        tuple(plotter.add_text(text, position=position) for text, position in self.annotations)
        option.of_optional(self.anti_aliasing).map(lambda aa: plotter.enable_anti_aliasing(aa.value, **_pruned({"multi_samples": self.aa_samples})))
        match self.camera:
            case Camera(tag="auto"):
                pass
            case Camera(tag="pose", pose=(position, focal, up)):
                plotter.camera_position = [position, focal if focal is not None else _ORIGIN, up if up is not None else _UP]
            case Camera(tag="standard", standard=(view, negative)):
                _VIEW[view](plotter, negative)
            case _:
                assert_never(self.camera)
        option.of_optional(self.zoom).map(plotter.camera.zoom)


# --- [TABLES] --------------------------------------------------------------------------

_FEATURE: frozendict[RenderFeature, Callable[["pv.Plotter"], object]] = frozendict({
    RenderFeature.AXES: lambda plotter: plotter.add_axes(),
    RenderFeature.SCALAR_BAR: lambda plotter: plotter.add_scalar_bar(),
    RenderFeature.SSAO: lambda plotter: plotter.enable_ssao(),
    RenderFeature.DEPTH_PEEL: lambda plotter: plotter.enable_depth_peeling(),
    RenderFeature.EYE_DOME: lambda plotter: plotter.enable_eye_dome_lighting(),
    RenderFeature.SHADOWS: lambda plotter: plotter.enable_shadows(),
    RenderFeature.PARALLEL: lambda plotter: plotter.enable_parallel_projection(),
    RenderFeature.HIDDEN_LINE: lambda plotter: plotter.enable_hidden_line_removal(),
})

_LINE: frozendict[LinePass, Callable[["pv.Plotter", "pv.DataSet", dict[str, object], LinesBand], object]] = frozendict({
    LinePass.WIREFRAME: lambda plotter, mesh, shared, band: plotter.add_mesh(mesh, style="wireframe", **_pruned(shared | {"line_width": band.width})),
    LinePass.SILHOUETTE: lambda plotter, mesh, _shared, band: plotter.add_silhouette(
        mesh, **_pruned({"line_width": band.width, "feature_angle": band.silhouette_angle})
    ),
})

_VIEW: frozendict[StandardView, Callable[["pv.Plotter", bool], object]] = frozendict({
    StandardView.PLAN: lambda plotter, negative: plotter.view_xy(negative=negative),
    StandardView.FRONT: lambda plotter, negative: plotter.view_xz(negative=negative),
    StandardView.SIDE: lambda plotter, negative: plotter.view_yz(negative=negative),
    StandardView.ISOMETRIC: lambda plotter, negative: plotter.view_isometric(negative=negative),
})

if frozenset(_FEATURE) != frozenset(RenderFeature) or frozenset(_LINE) != frozenset(LinePass) or frozenset(_VIEW) != frozenset(StandardView):
    raise RuntimeError("spec tables do not cover their vocabularies")

# --- [OPERATIONS] ----------------------------------------------------------------------


def _pruned(row: dict[str, object]) -> dict[str, object]:
    return {key: value for key, value in row.items() if value is not None}


def _shaped(array: NDArray[np.float64] | NDArray[np.int64], /) -> tuple[bytes, bytes, bytes]:
    # canonical array projection for the identity preimage: the dtype token, a little-endian shape header, and the
    # little-endian canonical-dtype body — so equal arrays digest identically across host byte order and
    # admitted-dtype drift, and same-byte arrays of different kind stay distinct.
    canonical = np.ascontiguousarray(array, dtype=np.dtype(np.int64 if array.dtype.kind in "iub" else np.float64).newbyteorder("<"))
    return (canonical.dtype.str.encode(), np.asarray(canonical.shape, dtype="<i8").tobytes(), canonical.tobytes())


def _canonized(raw: object) -> object:
    # msgpack enc_hook: a frozendict canonicalizes as its dict view under the deterministic key order; a frozenset sorts.
    if isinstance(raw, frozendict):
        return dict(raw)
    if isinstance(raw, frozenset):
        return sorted(raw)
    raise NotImplementedError(type(raw).__name__)


CANON: Encoder = Encoder(order="deterministic", enc_hook=_canonized)


def framed(*chunks: bytes) -> tuple[bytes, ...]:
    # Identity fold digests chunks by concatenation, so the preimage count-frames the tuple and length-frames every variable-width field.
    return (len(chunks).to_bytes(4, "big"), *chain.from_iterable((len(chunk).to_bytes(8, "big"), chunk) for chunk in chunks))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
