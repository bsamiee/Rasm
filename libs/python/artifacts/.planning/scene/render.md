# [PY_ARTIFACTS_SCENE_RENDER]

The 3D scientific-visualization render owner. `Scene3d` renders pyvista datasets on the VTK engine — `UnstructuredGrid`/`PolyData`/`MultiBlock`, scalar fields, the `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp`/`glyph`/`streamlines`/`decimate`/`surface` filter family, the surface/volume/point-cloud render styles, PBR material, the publication-quality render-control family (SSAO, depth-peeling, eye-dome lighting, shadows, anti-aliasing, parallel projection), scalar-bar/axes overlays, and a `CameraPose` viewpoint — to a host-free offscreen image, and emits the orbit/turntable rgb24 frame sequence the cross-folder `media/video#MEDIA` encoder consumes. `SceneOp` is ONE closed-payload `expression.tagged_union` over the `Image`/`Export`/`Frames` modalities, each case carrying its typed payload, dispatched by one total `match` returning `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` so every render mints its content key AND its evidence receipt in one carrier, exactly as the `media/video#MEDIA` encode sibling does. `RenderSpec` is the one frozen render-policy value — window, scalar field, colormap, clim, opacity, the `RenderStyle` surface/volume/points discriminant, the PBR `pbr`/`metallic`/`roughness`/`show_edges` material band, the `RenderFeature` overlay/quality `frozenset`, the `anti_aliasing` mode, the `CameraPose` viewpoint, the background, transparency, and the `FieldFilter` pre-render filter chain — folded into every arm through its bound `staged`/`added`/`viewed` projections, never loose constructor fields the implementer re-derives per call. `FieldFilter` is the one closed-payload `tagged_union` over the catalogued pyvista filter family, each case carrying its typed geometry/range/factor payload and folding to a new `DataSet` through one total `apply` `match`, so a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a parallel filtered-mesh type. The raw mesh is admitted EXACTLY ONCE through `pyvista.wrap` (the catalogued zero-copy entry) on the worker, so the interior never re-validates a provider shape and the filter fold runs over an admitted `DataSet`, never the raw cross-seam payload. The render path is offscreen software GL (osmesa/EGL) so a scene rasterizes with zero display, browser, or GPU. The owner rides the worker native VTK floor: no runtime VTK admission, so the runtime process imports neither `pyvista` nor `vtk`, and the whole render crosses the runtime subprocess lane onto the sub-3.13 companion-floor worker — floor-gated planned capability, not a blocked spike. The `Frames` arm returns the `tuple[NDArray[np.uint8], ...]` keyed rgb24 raster `media/video#MEDIA` ingests directly through `VideoFrame.from_ndarray`, never a lossy PNG-bytes intermediary. The `Export` file-target arms (`scene/export#EXPORT`) compose this owner's offscreen plotter for the `GLTF`/`VRML`/`OBJ`/`HTML` exporters, while the USD/USDZ arm authors the admitted dataset's extracted surface through `scene/stage#STAGE` (`usd-core`) with no render pass — both ride this owner's worker, never re-owning the dataset/filter/render policy. This page closes the `SCENE_TIMESERIES_FRAMES` and `SCENE_FILTER_PIPELINE` ideas.

## [01]-[INDEX]

- [02]-[SCENE]: the one `Scene3d` owner over the closed-payload `SceneOp` family — `Image`/`Frames` (plus the `Export` arm delegated to `scene/export#EXPORT` and the USD/USDZ arm to `scene/stage#STAGE`) folding into one `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]`, the `RenderSpec` render-policy value with its bound `staged`/`added`/`viewed` projections, the twelve-case `FieldFilter` closed-payload filter family folded by `RenderSpec.staged` before render, the `RenderStyle` surface/volume/points style discriminant and PBR material band the `added` projection reads, the `RenderFeature` overlay/quality `frozenset` the `_FEATURE` table folds in `viewed`, the `CameraPose` viewpoint the `viewed` projection writes onto `camera_position`, the `OrbitPath` `(factor, elevation)`-column vocabulary keyed inside the `Frames` payload, the `tuple[NDArray[np.uint8], ...]` rgb24 frame seam to `media/video#MEDIA`, and the `ArtifactReceipt.Scene(key, target, bytes)` evidence each arm mints; `pyvista` `wrap`/`Plotter(off_screen=True)`/`add_mesh`/`add_volume`/`add_points`/`screenshot`/`set_background`/`add_axes`/`add_scalar_bar`/`camera_position`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_anti_aliasing`/`enable_parallel_projection`/`extract_surface`/`triangulate`/`clip`/`slice`/`threshold`/`contour`/`warp_by_scalar`/`decimate` plus the `.points`/`.regular_faces`/`.point_normals` numpy accessors the `surface_arrays` USD seam reads settled against the folder `.api`, with the `glyph`/`streamlines`/`slice_orthogonal`/`clip_box`/`clip_scalar` kwarg spellings the lone [03]-[RESEARCH] catalogue-deepen seam, and the orbit `camera_position` walk computed through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`.

## [02]-[SCENE]

- Owner: `Scene3d` the one 3D-scene render owner discriminating modality over the closed `SceneOp` family; `SceneOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Scene3d` subclass; `RenderSpec` the one frozen render-policy value carrying its own `staged` filter-fold, `added` mesh-add, and `viewed` camera/overlay/quality projections so a new render knob is one `RenderSpec` field bound into an existing projection, never a constructor-parameter tail nor a re-derived add-mesh/camera call; `RenderStyle` the closed `StrEnum` (`SURFACE`/`VOLUME`/`POINTS`) the `added` `match` reads to dispatch `add_mesh`/`add_volume`/`add_points`, collapsing the prior two-body `volume: bool` flag into one style vocabulary that admits the point-cloud modality the flag could not name; `RenderFeature` the closed overlay/quality `StrEnum` (`AXES`/`SCALAR_BAR`/`SSAO`/`DEPTH_PEEL`/`EYE_DOME`/`SHADOWS`/`PARALLEL`) whose membership `frozenset` the `_FEATURE` `frozendict` folds onto `add_axes`/`add_scalar_bar`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_parallel_projection`, collapsing eight parallel bool fields into one behavior-carrying set so a new render-quality toggle is one member plus one row; `CameraPose` the closed `NamedTuple` viewpoint value (`position`/`focal_point`/`view_up`) the `viewed` projection writes onto the catalogued `Plotter.camera_position` property as the `[position, focal_point, view_up]` triple, so a static viewpoint and the orbit trajectory share one camera-policy value, never loose camera kwargs nor a phantom settable-azimuth accessor; `FieldFilter` the closed-payload `tagged_union` over the catalogued pyvista filter family — `Clip`/`ClipBox`/`ClipScalar`/`Slice`/`SliceOrthogonal`/`Threshold`/`Contour`/`Warp`/`Glyph`/`Streamlines`/`Decimate`/`Surface` — each case carrying its typed geometry/range/factor payload and folding to a new `DataSet` through one total `apply` `match`, so a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a per-filter mesh wrapper nor an erased filter-name bag; `OrbitPath` the closed orbit-trajectory `Enum` carrying its `(factor, elevation)` columns (`AZIMUTH`/`WIDE`/`TIGHT`) so the orbit radius multiplier and height fraction are member-bound values the numpy `camera_position` walk reads directly, never a `StrEnum` whose string value is mis-passed as a float; `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` the one carrier every arm returns, the `media/video#MEDIA` `tuple[NDArray[np.uint8], ...]` rgb24 frame payload keyed through `ContentIdentity.of` so the producer hands MEDIA the raw `screenshot(return_img=True)` raster array `VideoFrame.from_ndarray(array, format="rgb24")` ingests directly, never a lossy PNG-bytes round-trip; the offscreen `Plotter(off_screen=True)` is the one render capsule per modality, never retained across arms, and the `Export`/USD arms re-enter the same `render_plotter` capsule through `scene/export#EXPORT`/`scene/stage#STAGE`.
- Cases: `SceneOp` cases — `Image(grid, spec)` (offscreen `Plotter.screenshot(path, window_size=)` PNG raster to a temp-file sink, the `RenderSpec.window` carried onto the catalogued `screenshot` keyword, the `RenderSpec.staged` filter chain applied over the `pyvista.wrap`-admitted dataset before the `RenderSpec.added` style/scalar/material mesh-add and the `RenderSpec.viewed` background/overlay/quality/camera fold) · `Export(grid, target, spec)` (one file-target axis whose `SceneTarget` row keys the exporter arm owned by `scene/export#EXPORT` for `GLTF`/`VRML`/`OBJ`/`HTML` and by `scene/stage#STAGE` for `USD`/`USDZ`, the format discriminated by the typed `SceneTarget` value crossing the seam as its `.value` and re-admitted once at the worker dispatch — never a parallel per-format export surface) · `Frames(grid, orbit, steps, spec)` (one orbit/turntable frame-sequence axis whose `OrbitPath` `(factor, elevation)` columns key the numpy `camera_position` orbit walk computed from the auto-framed viewpoint, the per-step loop placing the camera through the catalogued `camera_position` property and emitting one offscreen `screenshot(return_img=True)` rgb24 raster array per camera step folded into the `tuple[NDArray[np.uint8], ...]` sequence `media/video#MEDIA` ingests through `VideoFrame.from_ndarray` with zero file round-trip — never a parallel rotating-scene producer, the rotating-scene and chart-over-time frame sources sharing this one arm) — matched by one total `match`/`case`, the `Image`/`Export`/`Frames` modality recovered from the `SceneOp` discriminant, never a name suffix.
- Auto: `render_plotter` folds the cross-seam payload into a `DataSet` through one `pyvista.wrap` (the catalogued zero-copy entry over a numpy buffer / VTK object / `trimesh`/`meshio` mesh), so the interior is total over an admitted dataset; `RenderSpec.staged` folds the `FieldFilter` tuple over it through `functools.reduce`, each case's `apply` `match` routing to the catalogued `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`glyph`/`streamlines`/`decimate`/`extract_surface` filter and returning a new `DataSet`; `RenderSpec.added` `match`es the `RenderStyle` to `add_mesh`/`add_volume`/`add_points`, binding the `scalars` active name, the `colormap` from `graphic/color/derive#DERIVE` (the `ColorReceipt.coords` palette the derivation owner resolves), the `clim` window and `opacity` through one filtered kwarg dict that drops `None`, and for `SURFACE` the `pbr`/`metallic`/`roughness`/`show_edges` material band; `RenderSpec.viewed` folds `background` onto `set_background`, the `RenderFeature` `frozenset` onto the `_FEATURE` enable-table (`add_axes`/`add_scalar_bar`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_parallel_projection`), the `anti_aliasing` mode onto `enable_anti_aliasing`, and a non-empty `CameraPose` onto the `camera_position` triple; the `RenderSpec.window`/`RenderSpec.transparent` fold onto the catalogued `screenshot(window_size=, transparent_background=)` keywords; the `Frames` arm reads the auto-framed `camera_position` for the orbit center and radius, scales the radius by `OrbitPath.factor` and the height by `OrbitPath.elevation`, walks the `numpy.linspace` azimuth sweep placing `camera_position` per step, and reads each step's offscreen `shoot` raster array into the sequence.
- Receipt: every arm returns `(key, ArtifactReceipt.Scene(key, target, bytes))` so the receipt is minted inside `_emit` over the produced payload, exactly as the `media/video#MEDIA` sibling mints `ArtifactReceipt.Media` in its `_emit`. The `Image` arm contributes the `"png"` target plus `len(data)`, the `Export` arm the `SceneTarget` value (`scene/export#EXPORT`/`scene/stage#STAGE`) plus the serialized scene-file byte count, and the `Frames` arm the `_FRAME_FORMAT` `"rgb24"` target plus the frame-sequence byte total `sum(frame.nbytes for ...)` — the raw frame-sequence artifact's own evidence, a distinct content-addressed unit from the encoded video `media/video#MEDIA` separately keys and mints `ArtifactReceipt.Media` over, so the two artifacts carry two keys and two receipts, never a double-counted single rail. The point/cell/window render evidence the `pyvista` `.api` evidence row names lands when the `core/receipt#RECEIPT` `scene` case widens beyond `(target, bytes)`.
- Growth: a new render knob is one `RenderSpec` field bound into the existing `staged`/`added`/`viewed` projection; a new render-quality toggle (legend, bounding box, orientation widget, SSAA tier) is one `RenderFeature` member plus one `_FEATURE` row, or one `RenderSpec` field plus one `viewed` line; a new material knob is one `RenderSpec` field bound into the `added` `SURFACE` arm; a new render style is one `RenderStyle` member plus one `added` `match` arm; a new field-visualization filter is one `FieldFilter` case plus one `apply` `match` arm over the catalogued dataset-filter family, dispatched by the same `RenderSpec.staged` reduce — never a parallel filtered-mesh type; a new camera control is one `CameraPose` field the `viewed` projection reads; a new orbit trajectory is one `OrbitPath` member carrying its own `(factor, elevation)` columns; a new scene file-export is one `SceneTarget` row plus one exporter arm in `scene/export#EXPORT` or `scene/stage#STAGE`; a new render-evidence fact is one slot on the `core/receipt#RECEIPT` `scene` case; zero new surface — the modality space stays three cases (`Image`/`Export`/`Frames`) on one owner, every addition a row, field, case, member, or arm.

```python signature
import os
from collections.abc import Callable
from enum import Enum, StrEnum
from functools import reduce
from typing import Final, Literal, NamedTuple, assert_never

from anyio import CapacityLimiter, to_process
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

lazy from artifacts.scene.export import SceneTarget, render_export        # cyclically coupled peer (export imports render) — deferred to break the cycle
lazy from artifacts.scene.render_worker import render_frames, render_image

type Vec3 = tuple[float, float, float]
type Plane = tuple[Vec3, Vec3]
type Bounds = tuple[float, float, float, float, float, float]
type ScalarRange = tuple[float, float]
type FieldFilterTag = Literal[
    "clip", "clip_box", "clip_scalar", "slice", "slice_orthogonal", "threshold",
    "contour", "warp", "glyph", "streamlines", "decimate", "surface",
]
type SceneOpTag = Literal["image", "export", "frames"]

_FRAME_FORMAT = "rgb24"
_ORIGIN: Vec3 = (0.0, 0.0, 0.0)
_UP: Vec3 = (0.0, 0.0, 1.0)

# bounds the off-loop render fan-out: every modality crosses the companion-floor `to_process` lane under
# this one limiter, so concurrent offscreen-GL render subprocesses are capped at the boundary, never the per-loop default.
_SCENE_LIMITER: Final[CapacityLimiter] = CapacityLimiter(os.process_cpu_count() or 1)


class RenderStyle(StrEnum):
    SURFACE = "surface"
    VOLUME = "volume"
    POINTS = "points"


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


# feature -> plotter-enable correspondence; `viewed` folds the policy `frozenset` over it, so a new
# render-quality toggle is one `RenderFeature` member plus one row, never a parallel bool field + `if`.
_FEATURE: frozendict[RenderFeature, Callable[[object], object]] = frozendict({
    RenderFeature.AXES: lambda plotter: plotter.add_axes(),
    RenderFeature.SCALAR_BAR: lambda plotter: plotter.add_scalar_bar(),
    RenderFeature.SSAO: lambda plotter: plotter.enable_ssao(),
    RenderFeature.DEPTH_PEEL: lambda plotter: plotter.enable_depth_peeling(),
    RenderFeature.EYE_DOME: lambda plotter: plotter.enable_eye_dome_lighting(),
    RenderFeature.SHADOWS: lambda plotter: plotter.enable_shadows(),
    RenderFeature.PARALLEL: lambda plotter: plotter.enable_parallel_projection(),
})


class OrbitPath(Enum):
    AZIMUTH = (1.0, 0.0)
    WIDE = (2.0, 0.3)
    TIGHT = (0.5, 0.5)

    def __init__(self, factor: float, elevation: float) -> None:
        self.factor = factor
        self.elevation = elevation


class CameraPose(NamedTuple):
    position: Vec3 | None = None
    focal_point: Vec3 | None = None
    view_up: Vec3 | None = None


@tagged_union(frozen=True)
class FieldFilter:
    tag: FieldFilterTag = tag()
    clip: Plane = case()
    clip_box: Bounds = case()
    clip_scalar: tuple[float, bool] = case()
    slice: Plane = case()
    slice_orthogonal: Vec3 = case()
    threshold: ScalarRange = case()
    contour: tuple[float, ...] = case()
    warp: float = case()
    glyph: float = case()
    streamlines: Vec3 = case()
    decimate: float = case()
    surface: bool = case()

    @staticmethod
    def Clip(normal: Vec3, origin: Vec3) -> "FieldFilter":
        return FieldFilter(clip=(normal, origin))

    @staticmethod
    def ClipBox(bounds: Bounds) -> "FieldFilter":
        return FieldFilter(clip_box=bounds)

    @staticmethod
    def ClipScalar(value: float, invert: bool = False) -> "FieldFilter":
        return FieldFilter(clip_scalar=(value, invert))

    @staticmethod
    def Slice(normal: Vec3, origin: Vec3) -> "FieldFilter":
        return FieldFilter(slice=(normal, origin))

    @staticmethod
    def SliceOrthogonal(center: Vec3) -> "FieldFilter":
        return FieldFilter(slice_orthogonal=center)

    @staticmethod
    def Threshold(lo: float, hi: float) -> "FieldFilter":
        return FieldFilter(threshold=(lo, hi))

    @staticmethod
    def Contour(*isovalues: float) -> "FieldFilter":
        return FieldFilter(contour=isovalues)

    @staticmethod
    def Warp(factor: float) -> "FieldFilter":
        return FieldFilter(warp=factor)

    @staticmethod
    def Glyph(factor: float) -> "FieldFilter":
        return FieldFilter(glyph=factor)

    @staticmethod
    def Streamlines(source_center: Vec3) -> "FieldFilter":
        return FieldFilter(streamlines=source_center)

    @staticmethod
    def Decimate(reduction: float) -> "FieldFilter":
        return FieldFilter(decimate=reduction)

    @staticmethod
    def Surface() -> "FieldFilter":
        return FieldFilter(surface=True)

    def apply(self, dataset: object, scalars: str) -> object:
        match self:
            case FieldFilter(tag="clip", clip=(normal, origin)):
                return dataset.clip(normal=normal, origin=origin)
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
            case FieldFilter(tag="contour", contour=isovalues):
                return dataset.contour(isosurfaces=list(isovalues), scalars=scalars)
            case FieldFilter(tag="warp", warp=factor):
                return dataset.warp_by_scalar(scalars=scalars, factor=factor)
            case FieldFilter(tag="glyph", glyph=factor):
                return dataset.glyph(scale=scalars, factor=factor)
            case FieldFilter(tag="streamlines", streamlines=source_center):
                return dataset.streamlines(vectors=scalars, source_center=source_center)
            case FieldFilter(tag="decimate", decimate=reduction):
                return dataset.decimate(target_reduction=reduction)
            case FieldFilter(tag="surface", surface=_):
                return dataset.extract_surface()
            case _:
                assert_never(self)


class RenderSpec(Struct, frozen=True):
    scalars: str
    colormap: str
    window: tuple[int, int] = (1024, 768)
    background: str = "white"
    clim: ScalarRange | None = None
    opacity: float | None = None
    style: RenderStyle = RenderStyle.SURFACE
    filters: tuple[FieldFilter, ...] = ()
    transparent: bool = False
    pbr: bool = False
    metallic: float | None = None
    roughness: float | None = None
    show_edges: bool = False
    features: frozenset[RenderFeature] = frozenset()
    anti_aliasing: AntiAlias | None = None
    camera: CameraPose = CameraPose()

    def staged(self, dataset: object) -> object:
        return reduce(lambda field, flt: flt.apply(field, self.scalars), self.filters, dataset)

    def added(self, plotter: object, mesh: object) -> None:
        shared = {"scalars": self.scalars, "cmap": self.colormap, "clim": self.clim, "opacity": self.opacity}
        match self.style:
            case RenderStyle.SURFACE:
                material = {"pbr": self.pbr or None, "metallic": self.metallic, "roughness": self.roughness, "show_edges": self.show_edges or None}
                plotter.add_mesh(mesh, **{key: value for key, value in (shared | material).items() if value is not None})
            case RenderStyle.VOLUME:
                plotter.add_volume(mesh, **{key: value for key, value in shared.items() if value is not None})
            case RenderStyle.POINTS:
                plotter.add_points(mesh, **{key: value for key, value in shared.items() if value is not None})
            case _:
                assert_never(self.style)

    def viewed(self, plotter: object) -> None:
        plotter.set_background(self.background)
        for feature in self.features:
            _FEATURE[feature](plotter)
        if self.anti_aliasing is not None:
            plotter.enable_anti_aliasing(self.anti_aliasing.value)
        if (pose := self.camera).position is not None:
            plotter.camera_position = [pose.position, pose.focal_point or _ORIGIN, pose.view_up or _UP]


@tagged_union(frozen=True)
class SceneOp:
    tag: SceneOpTag = tag()
    image: tuple[object, RenderSpec] = case()
    export: tuple[object, SceneTarget, RenderSpec] = case()
    frames: tuple[object, OrbitPath, int, RenderSpec] = case()

    @staticmethod
    def Image(grid: object, spec: RenderSpec) -> "SceneOp":
        return SceneOp(image=(grid, spec))

    @staticmethod
    def Export(grid: object, target: SceneTarget, spec: RenderSpec) -> "SceneOp":
        return SceneOp(export=(grid, target, spec))

    @staticmethod
    def Frames(grid: object, orbit: OrbitPath, steps: int, spec: RenderSpec) -> "SceneOp":
        return SceneOp(frames=(grid, orbit, steps, spec))


class Scene3d(Struct, frozen=True):
    op: SceneOp

    async def render(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"scene.{self.op.tag}", self._emit)

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        match self.op:
            case SceneOp(tag="image", image=(grid, spec)):
                data = await to_process.run_sync(render_image, grid, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.of(SceneTarget.PNG.value, data)
                return key, ArtifactReceipt.Scene(key, SceneTarget.PNG.value, len(data))
            case SceneOp(tag="export", export=(grid, target, spec)):
                data = await to_process.run_sync(render_export, grid, target.value, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.of(target.value, data)
                return key, ArtifactReceipt.Scene(key, target.value, len(data))
            case SceneOp(tag="frames", frames=(grid, orbit, steps, spec)):
                sequence = await to_process.run_sync(render_frames, grid, orbit, steps, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.of(
                    _FRAME_FORMAT, tuple(ContentIdentity.of(_FRAME_FORMAT, frame.tobytes()) for frame in sequence)
                )
                return key, ArtifactReceipt.Scene(key, _FRAME_FORMAT, sum(frame.nbytes for frame in sequence))
            case _:
                assert_never(self.op)
```

The worker bodies (`render_plotter`/`surface_arrays`/`shoot`/`png`/`render_image`/`_orbit`/`render_frames`) are module-level functions dispatched by qualified name across the `anyio.to_process.run_sync` subprocess seam (the seam cannot target a bound method or closure) under the shared `_SCENE_LIMITER` `CapacityLimiter` that bounds render fan-out at the boundary, so they import `pyvista`/`numpy` at module scope inside the gated sub-3.13 worker only, never on the runtime owner; `surface_arrays` is the pyvista mesh-extraction seam the `scene/export#EXPORT` USD/USDZ arm hands to `scene/stage#STAGE` (`usd-core`) to author a USD layer from the dataset's triangulated surface (`.points`/`.regular_faces`/`.point_normals`) with no render pass; each render arm brackets its `Plotter` in a `try`/`finally` `plotter.close()` so the native VTK render window closes deterministically and the GL context is never left for GC. `render_plotter` admits the raw mesh ONCE through `pyvista.wrap` (the catalogued zero-copy entry over a numpy buffer / VTK object / `trimesh`/`meshio` mesh), applies `RenderSpec.staged`, binds through `RenderSpec.added`, folds `RenderSpec.viewed`, and is the shared render window the `Image`/`Frames` sinks and the `scene/export#EXPORT`/`scene/stage#STAGE` arms all read; `_orbit` computes the azimuthal `camera_position` walk from the auto-framed viewpoint through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`, and `render_frames` walks it placing the camera per step through the catalogued `camera_position` property.

```python signature
from pathlib import Path
from tempfile import TemporaryDirectory

import numpy as np
import pyvista as pv
from numpy.typing import NDArray

from artifacts.scene.render import OrbitPath, RenderSpec, Vec3


def render_plotter(grid: object, spec: RenderSpec) -> "pv.Plotter":
    plotter = pv.Plotter(off_screen=True)
    spec.added(plotter, spec.staged(pv.wrap(grid)))
    spec.viewed(plotter)
    return plotter


def surface_arrays(grid: object) -> tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32]]:
    surf = pv.wrap(grid).extract_surface().triangulate()  # admit ONCE, reduce to a uniform-triangle boundary so regular_faces is (M,3)
    return (
        np.asarray(surf.points, dtype=np.float32),
        surf.regular_faces.astype(np.int32),
        np.asarray(surf.point_normals, dtype=np.float32),
    )


def shoot(plotter: "pv.Plotter", spec: RenderSpec) -> NDArray[np.uint8]:
    return plotter.screenshot(
        filename=None, return_img=True, window_size=list(spec.window), transparent_background=spec.transparent
    )


def png(plotter: "pv.Plotter", out: Path, spec: RenderSpec) -> bytes:
    plotter.screenshot(str(out), window_size=list(spec.window), transparent_background=spec.transparent)
    return out.read_bytes()


def render_image(grid: object, spec: RenderSpec) -> bytes:
    plotter = render_plotter(grid, spec)
    try:
        with TemporaryDirectory() as work:
            return png(plotter, Path(work) / "scene.png", spec)
    finally:
        plotter.close()  # deterministic VTK render-window teardown (BasePlotter.close); the native GL context is never left for GC


def _orbit(plotter: "pv.Plotter", orbit: OrbitPath, steps: int) -> tuple[tuple[Vec3, Vec3, Vec3], ...]:
    start, focal, up = plotter.camera_position
    center = np.asarray(focal, dtype=np.float64)
    radius = float(np.linalg.norm(np.asarray(start, dtype=np.float64) - center)) * orbit.factor
    height = float(center[2]) + radius * orbit.elevation
    focal3: Vec3 = (float(center[0]), float(center[1]), float(center[2]))
    up3: Vec3 = (float(up[0]), float(up[1]), float(up[2]))
    return tuple(
        ((float(center[0] + radius * np.cos(a)), float(center[1] + radius * np.sin(a)), height), focal3, up3)
        for a in np.linspace(0.0, 2.0 * np.pi, steps, endpoint=False)
    )


def render_frames(grid: object, orbit: OrbitPath, steps: int, spec: RenderSpec) -> tuple[NDArray[np.uint8], ...]:
    plotter = render_plotter(grid, spec)
    try:
        def _capture(view: tuple[Vec3, Vec3, Vec3]) -> NDArray[np.uint8]:
            plotter.camera_position = list(view)
            return shoot(plotter, spec)

        return tuple(_capture(view) for view in _orbit(plotter, orbit, steps))
    finally:
        plotter.close()  # deterministic VTK render-window teardown (BasePlotter.close); the native GL context is never left for GC
```
