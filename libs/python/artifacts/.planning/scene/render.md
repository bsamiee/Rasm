# [PY_ARTIFACTS_SCENE_RENDER]

The 3D scientific-visualization render owner. `Scene3d` renders pyvista datasets on the VTK engine — `UnstructuredGrid`/`PolyData`/`MultiBlock`, scalar fields, the clip/slice/slice-orthogonal/threshold/contour/warp/glyph/streamlines/decimate filter family, volume rendering, scalar-bar/axes overlays, light-kit shading, and a `CameraPose` viewpoint — to a host-free offscreen image, and emits the orbit/turntable rgb24 frame sequence the cross-folder `media/video#MEDIA` encoder consumes. `SceneOp` is ONE closed-payload `expression.tagged_union` over the render-target and frame-sequence modalities, each case carrying its typed payload, dispatched by one total `match` returning `RuntimeRail[ContentKey]`. `RenderSpec` is the one frozen render-policy value — window size, scalar field, colormap, scalar clim, opacity, volume flag, background, transparency, scalar-bar/axes/lighting flags, the `CameraPose` viewpoint, and the `FieldFilter` pre-render filter chain — folded into every arm through its bound `staged`/`added`/`viewed` projections, never loose constructor fields the implementer re-derives per call. `FieldFilter` is the one closed-payload `tagged_union` over the catalogued pyvista filter family — `Clip`/`Slice`/`SliceOrthogonal`/`Threshold`/`Contour`/`Warp`/`Glyph`/`Streamlines`/`Decimate`/`Surface` — each case carrying its typed geometry/range/factor payload and folding to a new `DataSet` through one total `apply` `match`, so a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a parallel filtered-mesh type. The render path is offscreen software GL (osmesa/EGL) so a scene rasterizes with zero display, browser, or GPU. The owner rides the gated `python_version<'3.13'` native VTK floor: no cp315 VTK wheel, so the cp315-core process imports neither `pyvista` nor `vtk` and the whole render crosses the runtime subprocess lane onto the sub-3.13 companion-floor worker — floor-gated planned capability, not a blocked spike. Every render returns a `RuntimeRail[ContentKey]`; the frame-sequence arm returns the `tuple[NDArray[np.uint8], ...]` keyed rgb24 raster payload `media/video#MEDIA` ingests directly through `VideoFrame.from_ndarray`, never a lossy PNG-bytes intermediary. The `Export` file-target arms (`scene/export#EXPORT`) and the USD/USDZ stage-authoring arm (`scene/stage#STAGE`) compose this owner's offscreen plotter, never re-owning the dataset/filter/render policy. This page closes the `SCENE_TIMESERIES_FRAMES` and `SCENE_FILTER_PIPELINE` ideas.

## [01]-[INDEX]

- [02]-[SCENE]: the one `Scene3d` owner over the closed-payload `SceneOp` family — `Image`/`Frames` (plus the `Export` arm delegated to `scene/export#EXPORT` and the USD/USDZ arm to `scene/stage#STAGE`) folding into one `RuntimeRail[ContentKey]`, the `RenderSpec` render-policy value with its bound `staged`/`added`/`viewed` projections folded into every arm, the ten-case `FieldFilter` closed-payload filter family folded by `RenderSpec.staged` before render, the `CameraPose` viewpoint value folded by `RenderSpec.viewed`, the behavior-bearing `OrbitPath` `factor`-column vocabulary keyed inside the `Frames` payload, the `tuple[NDArray[np.uint8], ...]` rgb24 frame seam to `media/video#MEDIA`; `pyvista` `Plotter(off_screen=True)`/`add_mesh`/`add_volume`/`screenshot`/`extract_surface`/`clip`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`glyph`/`streamlines`/`MultiBlock` settled against the folder `.api`, the `screenshot(return_img=True)` array raster, `PolyData.decimate`, `Plotter(lighting=)`/`Plotter.show_axes`/`Plotter.camera`/`Plotter.set_background`, and `Plotter.generate_orbital_path`/`path.points`/`Plotter.set_position` carried as [03]-[RESEARCH] catalogue-deepen seams.

## [02]-[SCENE]

- Owner: `Scene3d` the one 3D-scene render owner discriminating modality over the closed `SceneOp` family; `SceneOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Scene3d` subclass; `RenderSpec` the one frozen render-policy value (window size, scalar field, colormap, scalar clim, opacity, volume flag, background, transparency, the `scalar_bar`/`axes` overlay flags, the `lighting` kit flag, the `CameraPose` viewpoint value, and the `FieldFilter` pre-render filter chain) carrying its own `staged` filter-fold, `added` mesh-add, and `viewed` camera/overlay projections so a new render knob is one `RenderSpec` field bound into an existing projection, never a constructor-parameter tail nor a re-derived add-mesh/camera call; `CameraPose` the closed `NamedTuple` viewpoint value (`position`/`focal_point`/`view_up`/`azimuth`/`elevation`/`zoom`) the `viewed` projection folds onto `Plotter.camera`, so a static viewpoint and the orbit trajectory share one camera-policy value, never loose camera kwargs; `FieldFilter` the closed-payload `tagged_union` over the catalogued pyvista filter family — `Clip`/`Slice`/`SliceOrthogonal`/`Threshold`/`Contour`/`Warp`/`Glyph`/`Streamlines`/`Decimate`/`Surface` — each case carrying its typed geometry/range/factor payload and folding to a new `DataSet` through one total `apply` `match`, so a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a per-filter mesh wrapper nor an erased filter-name bag; `OrbitPath` the closed orbit-trajectory `Enum` carrying its `factor` float column (`AZIMUTH`/`WIDE`/`TIGHT`) so the trajectory radius is a member-bound value the `generate_orbital_path(factor=)` float argument reads directly, never a `StrEnum` whose string value is mis-passed as a float factor; `RuntimeRail[ContentKey]` the one carrier every arm returns, the cross-folder `media/video#MEDIA` `tuple[NDArray[np.uint8], ...]` rgb24 frame payload keyed through `ContentIdentity.of` so the producer hands MEDIA the raw `screenshot(return_img=True)` raster array `VideoFrame.from_ndarray(array, format="rgb24")` ingests directly, never a lossy PNG-bytes round-trip re-decoded before video encode; the offscreen `Plotter(off_screen=True)` is the one render capsule per modality, never retained across arms, and the `Export`/USD arms re-enter the same `_plotter` capsule through `scene/export#EXPORT`/`scene/stage#STAGE`.
- Cases: `SceneOp` cases — `Image(grid, spec)` (offscreen `Plotter.screenshot(path, window_size=)` PNG raster to a temp-file sink, the `RenderSpec.window` window size carried onto the catalogued `screenshot` keyword, the `RenderSpec.staged` filter chain applied before the `RenderSpec.added` scalar-field/colormap/clim/opacity/volume/scalar-bar mesh-add and the `RenderSpec.viewed` background/axes/camera fold) · `Export(grid, target, spec)` (one file-target axis whose `SceneTarget` row keys the exporter arm owned by `scene/export#EXPORT` for `GLTF`/`VRML`/`OBJ`/`HTML` and by `scene/stage#STAGE` for `USD`/`USDZ`, the format discriminated by the typed `SceneTarget` value crossing the seam as its `.value` and re-admitted once at the worker dispatch — never a parallel per-format export surface) · `Frames(grid, orbit, steps, spec)` (one orbit/turntable frame-sequence axis whose `OrbitPath.factor` float radius keys the `Plotter.generate_orbital_path(n_points=, factor=)` trajectory whose `path.points` viewpoints the per-step loop walks, placing the camera through `Plotter.set_position` and emitting one offscreen `screenshot(return_img=True)` rgb24 raster array per camera step folded into the `tuple[NDArray[np.uint8], ...]` sequence `media/video#MEDIA` ingests through `VideoFrame.from_ndarray` with zero file round-trip — never a parallel rotating-scene producer, the rotating-scene and chart-over-time frame sources sharing this one arm) — matched by one total `match`/`case`, the `Image`/`Export`/`Frames` modality recovered from the `SceneOp` discriminant, never a name suffix.
- Entry: `Scene3d.render` is `async` over the runtime `async_boundary`, dispatching the whole render onto the sub-3.13 companion-floor worker (the cp315-core process imports neither `pyvista` nor `vtk` nor `pxr`), keyed by the content key the `_emit` arm derives through `ContentIdentity.of`; the worker configures one offscreen `pyvista.Plotter(off_screen=True)` through `_plotter`, which applies the `RenderSpec.staged` filter chain to the dataset, binds it through `RenderSpec.added`, then folds the camera/overlay state through `RenderSpec.viewed`, and reads the modality through a split sink — `_png` (temp-file PNG file artifact) for the `Image`/`Export`-`PNG` raster, `_shoot` (`screenshot(return_img=True)` rgb24 array, no file) for the `Frames` sequence — so `Image` via `_png`/`screenshot(path, window_size=)`, `Export` via the `SceneTarget`-keyed `scene/export#EXPORT`/`scene/stage#STAGE` arm over the same `_plotter` render window, `Frames` via the `generate_orbital_path` viewpoint-walk per-step `set_position`/`_shoot` loop folded into the `tuple[NDArray[np.uint8], ...]` sequence. The `Image`/`Export` arms key one `ContentKey` over the single payload; the `Frames` arm keys one Merkle-parent `ContentKey` over the per-frame children through the `tuple[ContentKey, ...]` modality of `ContentIdentity.of` (each child keyed over the frame array's `tobytes()` content under the `_FRAME_FORMAT` `"rgb24"` tag, never a `"png"` tag for a payload that carries no PNG), so the frame sequence is one content-addressed unit MEDIA re-admits by reference and ingests array-by-array.
- Auto: dataset construction folds the array payload into a `pyvista.UnstructuredGrid`/`PolyData`/`MultiBlock` on the worker; `RenderSpec.staged` folds the `FieldFilter` tuple over the dataset through `functools.reduce`, each case's `apply` `match` routing to the catalogued `clip`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`glyph`/`streamlines`/`decimate`/`extract_surface` filter and returning a new `DataSet`; `RenderSpec.added` binds the `scalars` active scalar name, the `colormap` from `graphic/color/derive#DERIVE` (the `ColorReceipt.coords` palette the derivation owner resolves), the `clim` scalar window, and the `opacity` through one filtered `add_mesh`/`add_volume` kwarg dict that drops `None` values, with the `volume` flag selecting `add_volume`, the `scalar_bar` flag driving `show_scalar_bar=`, and the `lighting` flag driving the add-mesh `lighting=` and the `Plotter(lighting=)` kit; `RenderSpec.viewed` folds the `background` onto `set_background`, the `axes` flag onto `show_axes`, and the `CameraPose` `position`/`focal_point`/`view_up`/`azimuth`/`elevation`/`zoom` onto `Plotter.camera`; the `RenderSpec.window`/`RenderSpec.transparent` fold onto the catalogued `screenshot(window_size=, transparent_background=)` keywords at the `_png`/`_shoot` sink; the `Frames` arm derives the camera trajectory through `generate_orbital_path` keyed by the `OrbitPath.factor` float radius, walks the trajectory `path.points` placing the camera through `set_position` per step, and reads each step's offscreen `_shoot` raster array into the sequence.
- Receipt: each `Image` render contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target)` keyed by the content key, the `"png"` target string carried as the target; the `Export` arm contributes its `SceneTarget`-valued `ArtifactReceipt.Scene` through `scene/export#EXPORT`/`scene/stage#STAGE`; the `Frames` arm hands the keyed `tuple[NDArray[np.uint8], ...]` to `media/video#MEDIA`, which contributes its own `ArtifactReceipt` media case, so the scene render owner mints no media receipt.
- Packages: `pyvista` (`UnstructuredGrid`/`PolyData`/`MultiBlock`/`Plotter`/`Plotter.add_mesh`/`Plotter.add_volume`/`Plotter.screenshot`/`DataSet.extract_surface`/`DataSet.clip`/`DataSet.slice`/`DataSet.slice_orthogonal`/`DataSet.threshold`/`DataSet.contour`/`DataSet.warp_by_scalar`/`DataSet.glyph`/`DataSet.streamlines` settled against the folder `.api`; `screenshot(return_img=True)`/`PolyData.decimate`/`Plotter(lighting=)`/`Plotter.show_axes`/`Plotter.camera`/`Plotter.set_background`/`Plotter.generate_orbital_path`/`path.points`/`Plotter.set_position` [03]-[RESEARCH]) gated `python_version<'3.13'`; `numpy` (the `NDArray[np.uint8]` rgb24 frame-array backing the `_shoot`/`_render_frames` worker returns and MEDIA's `VideoFrame.from_ndarray` ingests, imported only on the gated-band worker, the owner page crossing the seam as `tuple[bytes-keyed ContentKey]` and never importing numpy); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new render knob is one `RenderSpec` field bound into the existing `staged`/`added`/`viewed` projection; a new overlay (legend, bounding box, orientation widget) is one `RenderSpec` flag plus one `added`/`viewed` line; a new field-visualization filter (contour-banded, glyph-oriented, tube) is one `FieldFilter` case plus one `apply` `match` arm over the catalogued dataset-filter family, dispatched by the same `RenderSpec.staged` reduce — never a parallel filtered-mesh type; a new camera modality is one `CameraPose` field the `viewed` projection reads; a new orbit trajectory is one `OrbitPath` member carrying its own `factor` float column; a new frame-visualization mode is one `Frames`-arm acceptor; a new scene file-export is one `SceneTarget` row plus one exporter arm in `scene/export#EXPORT` or `scene/stage#STAGE`; zero new surface — the modality space stays three cases (`Image`/`Export`/`Frames`) on one owner, every addition a row, field, case, or arm.
- Boundary: no interactive viewer, no UI window, no display server; the dataset arrays arrive from data/compute outputs as inputs and own no mesh-file interchange (that stays at `geometry`, the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam recording that scene = visualization-scene export and geometry mesh = mesh-file codec, no shared owner). The offscreen software-GL render is the host-free path; `pyvista`/`vtk` ride the gated `python_version<'3.13'` band, none resolving in the cp315-core process, so the entire render runs through the runtime subprocess lane (`anyio.to_process.run_sync`) onto the sub-3.13 companion-floor worker that imports them at module scope — neither a module-top nor a lazy gated import lands on the core page. The `Frames` arm is the one frame source feeding `media/video#MEDIA`, so MEDIA is no longer a phantom sink: the `tuple[NDArray[np.uint8], ...]` rgb24 raster sequence crosses the intra-folder counterpart seam keyed by `ContentIdentity.of`, and MEDIA encodes it cp315-core in-process over PyAV through `VideoFrame.from_ndarray(array, format="rgb24")` with no intervening PNG encode/decode. The prior flat `Scene3d(Struct, grid, scalars, colormap, target)` plain owner, the three repeated `TemporaryDirectory`-then-`read_bytes` sinks, the lossy `Frames` PNG-bytes round-trip that re-decoded every frame before MEDIA's `from_ndarray` re-encoded it, the `_plotter` thin wrapper hand-forwarding three add-mesh kwargs with no filter, volume, view, or camera axis, the dead `RenderSpec.camera` field consumed nowhere, and the six-case filter family that ignored the catalogued `slice_orthogonal`/`glyph`/`streamlines`/`decimate` filters and forfeited the scalar-bar/axes/lighting/camera overlay axis VTK exposes are the deleted forms — the modality, render-policy, filter, camera, and orbit-path axes are now one closed `SceneOp` family plus the ten-case `FieldFilter` filter union, the `CameraPose` viewpoint value, the `OrbitPath` `factor`-column vocabulary, and the `RenderSpec` policy value carrying its own `staged`/`added`/`viewed` projections folded through one `_png`/`_shoot` sink split (file for the PNG artifact, array for the MEDIA frame stream), never four parallel constructor fields, three duplicated sinks, a lossy double-encode, and a filter-thin overlay-blind single modality.

```python signature
from enum import Enum
from functools import reduce
from typing import Literal, NamedTuple, assert_never

from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from rasm.artifacts.scene.export import SceneTarget, render_export
from rasm.artifacts.scene.render_worker import render_frames, render_image

type Vec3 = tuple[float, float, float]
type Plane = tuple[Vec3, Vec3]
type ScalarRange = tuple[float, float]
type FieldFilterTag = Literal[
    "clip", "slice", "slice_orthogonal", "threshold", "contour", "warp", "glyph", "streamlines", "decimate", "surface"
]
type SceneOpTag = Literal["image", "export", "frames"]

_FRAME_FORMAT = "rgb24"


class OrbitPath(Enum):
    AZIMUTH = 1.0
    WIDE = 2.0
    TIGHT = 0.5

    def __init__(self, factor: float) -> None:
        self.factor = factor


class CameraPose(NamedTuple):
    position: Vec3 | None = None
    focal_point: Vec3 | None = None
    view_up: Vec3 | None = None
    azimuth: float = 0.0
    elevation: float = 0.0
    zoom: float = 1.0


@tagged_union(frozen=True)
class FieldFilter:
    tag: FieldFilterTag = tag()
    clip: Plane = case()
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

    def apply(self, grid: object, scalars: str) -> object:
        match self:
            case FieldFilter(tag="clip", clip=(normal, origin)):
                return grid.clip(normal=normal, origin=origin)
            case FieldFilter(tag="slice", slice=(normal, origin)):
                return grid.slice(normal=normal, origin=origin)
            case FieldFilter(tag="slice_orthogonal", slice_orthogonal=(x, y, z)):
                return grid.slice_orthogonal(x=x, y=y, z=z)
            case FieldFilter(tag="threshold", threshold=value_range):
                return grid.threshold(value=value_range, scalars=scalars)
            case FieldFilter(tag="contour", contour=isovalues):
                return grid.contour(isosurfaces=list(isovalues), scalars=scalars)
            case FieldFilter(tag="warp", warp=factor):
                return grid.warp_by_scalar(scalars=scalars, factor=factor)
            case FieldFilter(tag="glyph", glyph=factor):
                return grid.glyph(scale=scalars, factor=factor)
            case FieldFilter(tag="streamlines", streamlines=source_center):
                return grid.streamlines(vectors=scalars, source_center=source_center)
            case FieldFilter(tag="decimate", decimate=reduction):
                return grid.decimate(target_reduction=reduction)
            case FieldFilter(tag="surface", surface=_):
                return grid.extract_surface()
            case _:
                assert_never(self)


class RenderSpec(Struct, frozen=True):
    scalars: str
    colormap: str
    window: tuple[int, int] = (1024, 768)
    background: str = "white"
    clim: ScalarRange | None = None
    opacity: float | None = None
    volume: bool = False
    filters: tuple[FieldFilter, ...] = ()
    transparent: bool = False
    scalar_bar: bool = False
    axes: bool = False
    lighting: bool = True
    camera: CameraPose = CameraPose()

    def staged(self, grid: object) -> object:
        return reduce(lambda field, flt: flt.apply(field, self.scalars), self.filters, grid)

    def added(self, plotter: object, mesh: object) -> None:
        keys = {"scalars": self.scalars, "cmap": self.colormap, "clim": self.clim, "opacity": self.opacity}
        bound = {key: value for key, value in keys.items() if value is not None}
        (plotter.add_volume if self.volume else plotter.add_mesh)(
            mesh, show_scalar_bar=self.scalar_bar, lighting=self.lighting, **bound
        )

    def viewed(self, plotter: object) -> None:
        plotter.set_background(self.background)
        if self.axes:
            plotter.show_axes()
        camera, pose = plotter.camera, self.camera
        (camera.position, camera.focal_point, camera.up) = (
            pose.position or camera.position,
            pose.focal_point or camera.focal_point,
            pose.view_up or camera.up,
        )
        (camera.azimuth, camera.elevation) = (camera.azimuth + pose.azimuth, camera.elevation + pose.elevation)
        camera.zoom(pose.zoom)


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

    async def render(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"scene.{self.op.tag}", self._emit)

    async def _emit(self) -> ContentKey:
        match self.op:
            case SceneOp(tag="image", image=(grid, spec)):
                data = await to_process.run_sync(render_image, grid, spec)
                return ContentIdentity.of(SceneTarget.PNG.value, data)
            case SceneOp(tag="export", export=(grid, target, spec)):
                data = await to_process.run_sync(render_export, grid, target.value, spec)
                return ContentIdentity.of(target.value, data)
            case SceneOp(tag="frames", frames=(grid, orbit, steps, spec)):
                sequence = await to_process.run_sync(render_frames, grid, orbit.factor, steps, spec)
                return ContentIdentity.of(
                    _FRAME_FORMAT,
                    tuple(ContentIdentity.of(_FRAME_FORMAT, frame.tobytes()) for frame in sequence),
                )
            case _:
                assert_never(self.op)
```

The gated-band worker bodies are module-level functions dispatched by qualified name across the `anyio.to_process.run_sync` subprocess seam (the seam cannot target a bound method or closure), so they stay out of the `Scene3d` owner deliberately. `_plotter` configures the offscreen capsule, applies `RenderSpec.staged`, binds through `RenderSpec.added`, folds `RenderSpec.viewed`, and is the shared render window the `Image`/`Frames` sinks and the `scene/export#EXPORT`/`scene/stage#STAGE` arms all read.

```python signature
from pathlib import Path
from tempfile import TemporaryDirectory

import numpy as np
import pyvista as pv
from numpy.typing import NDArray

from rasm.artifacts.scene.render import RenderSpec


def render_plotter(grid: object, spec: RenderSpec) -> "pv.Plotter":
    plotter = pv.Plotter(off_screen=True, lighting="light_kit" if spec.lighting else "none")
    spec.added(plotter, spec.staged(grid))
    spec.viewed(plotter)
    return plotter


def shoot(plotter: "pv.Plotter", spec: RenderSpec) -> NDArray[np.uint8]:
    return plotter.screenshot(
        filename=None, return_img=True, window_size=list(spec.window), transparent_background=spec.transparent
    )


def png(plotter: "pv.Plotter", out: Path, spec: RenderSpec) -> bytes:
    plotter.screenshot(str(out), window_size=list(spec.window), transparent_background=spec.transparent)
    return out.read_bytes()


def render_image(grid: object, spec: RenderSpec) -> bytes:
    with TemporaryDirectory() as work:
        return png(render_plotter(grid, spec), Path(work) / "scene.png", spec)


def render_frames(grid: object, factor: float, steps: int, spec: RenderSpec) -> tuple[NDArray[np.uint8], ...]:
    plotter = render_plotter(grid, spec)
    path = plotter.generate_orbital_path(n_points=steps, factor=factor)

    def _capture(viewpoint: object) -> NDArray[np.uint8]:
        plotter.set_position(viewpoint)
        return shoot(plotter, spec)

    return tuple(_capture(viewpoint) for viewpoint in path.points)
```

## [03]-[RESEARCH]

- [ORBIT_FRAMES] [RESEARCH]: the orbit/turntable frame generator `Plotter.generate_orbital_path(n_points=, factor=)` deriving the camera trajectory, the trajectory `path.points` viewpoint array the per-step loop walks, and `Plotter.set_position(viewpoint)` placing the camera at each step are NOT in the folder `.api` catalogue for `pyvista` — the `pyvista.md` `[03]-[ENTRYPOINTS]` tables carry no orbit, orbital-path, camera-path, or camera-position member, only the render/filter/export/import family. The `render_frames` body and the `Frames` case stay a marked RESEARCH catalogue-deepen seam until a `Plotter.generate_orbital_path`/`path.points`/`Plotter.set_position` reflection pass lands on the gated `python_version<'3.13'` band; the `OrbitPath` `Enum` carrying its `factor` float column (`AZIMUTH=1.0`/`WIDE=2.0`/`TIGHT=0.5`) is the settled orbit-radius vocabulary the `_emit` arm reads as `orbit.factor` and hands to the float `generate_orbital_path(factor=)` argument (the `factor` is the orbit-radius multiplier, a float, never an enum string value), and the per-step `shoot` `screenshot(return_img=True)` rgb24 array writer and the `tuple[NDArray[np.uint8], ...]` array sequence the `media/video#MEDIA` PyAV encoder consumes are the settled producer shape (`av.md` `[03]-[ENTRYPOINTS]` `VideoFrame.from_ndarray(array, format="rgb24")` admits the rgb24 array directly with no decode, `stream.encode`/`OutputContainer.mux` muxing it cp315-core in-process) — the prior `tuple[bytes, ...]` PNG-bytes producer was a lossy encode/decode round-trip the array seam deletes. Close-condition: `.api` catalogue carries `Plotter.generate_orbital_path` with the `n_points`/`factor` axes, the `path.points` viewpoint array, and `Plotter.set_position`.
- [SCENE_VIEW] [RESEARCH]: the view/overlay/camera projection `RenderSpec.viewed` folds — `Plotter(lighting="light_kit"|"none")` constructor kit, `Plotter.show_axes()` orientation-axes overlay, the `add_mesh(show_scalar_bar=, lighting=)` overlay/shading kwargs, `Plotter.camera` and its `position`/`focal_point`/`up`/`azimuth`/`elevation`/`zoom` accessors, and the `PolyData.decimate(target_reduction=)` filter — are NOT enumerated in the folder `.api` catalogue for `pyvista`: the `pyvista.md` `[02]-[PUBLIC_TYPES]`/`[03]-[ENTRYPOINTS]` tables carry `add_mesh(mesh, scalars=, cmap=, clim=, pbr=, ...)`, the `MultiBlock` container, the `clip`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`glyph`/`streamlines`/`extract_surface` filters, the `PolyData.decimate` repair row, `set_background`, `add_axes`/`add_scalar_bar`, and the `camera_position` property, but no raw `Plotter.camera`/`show_axes`/`Plotter(lighting=)` member, while `vtk.md` `[02]-[PUBLIC_TYPES]` carries the underlying `vtkScalarBarActor`/`vtkAxesActor`/`vtkLight`/`vtkCamera`/`vtkDecimatePro` classes the pyvista wrappers drive. The `RenderSpec.viewed`/`RenderSpec.added` overlay-and-camera body, the `CameraPose` fold, the `FieldFilter.Glyph`/`FieldFilter.Streamlines`/`FieldFilter.Decimate` arms (the `glyph(scale=, factor=)`/`streamlines(vectors=, source_center=)`/`decimate(target_reduction=)` kwarg spellings), and the `slice_orthogonal(x=, y=, z=)` kwarg spelling stay a marked RESEARCH catalogue-deepen seam until a `Plotter.camera`/`show_axes`/`Plotter(lighting=)`/`add_mesh(show_scalar_bar=, lighting=)`/`PolyData.decimate`/`DataSet.glyph`/`DataSet.streamlines` reflection pass lands on the gated `python_version<'3.13'` band; the `CameraPose` `NamedTuple` (`position`/`focal_point`/`view_up`/`azimuth`/`elevation`/`zoom`) is the settled camera-policy vocabulary the `viewed` projection reads, the static-viewpoint and orbit-trajectory camera concerns sharing this one value. Close-condition: `.api` catalogue carries `Plotter.camera`, `Plotter.show_axes`, the `add_mesh(show_scalar_bar=, lighting=)` kwargs, the `Plotter(lighting=)` constructor axis, `PolyData.decimate(target_reduction=)`, and the `glyph`/`streamlines`/`slice_orthogonal` kwarg signatures.
- [PYVISTA_RENDER]: the `pyvista.Plotter(off_screen=True)` construction, the `Plotter.add_mesh(scalars=, cmap=, clim=, opacity=)` and `Plotter.add_volume` mesh-add rows, the `Plotter.screenshot(filename, window_size=)` raster row, the `MultiBlock` named-dataset container, and the `DataSet.extract_surface`/`clip`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`glyph`/`streamlines` filter family verify against the folder `.api` catalogue for `pyvista` (`0.48.4` on `vtk` `9.6.2`, the gated `python_version<'3.13'` sub-3.13 companion floor); the `pyvista.md` `[03]-[ENTRYPOINTS]` filter rows carry `clip` (plane normal/origin), `slice` (normal/origin, with `slice_orthogonal` named on the same row), `threshold` (scalar range), `contour` (isovalue list plus scalars), `warp_by_scalar` (scalar plus factor), and `extract_surface` (no-arg), the `[02]-[PUBLIC_TYPES]` mesh table carries `MultiBlock` (named dataset collection), and the render axis catalogues the `add_mesh(scalars=, cmap=, clim=)` kwargs and the `add_volume` volume row, so the `FieldFilter.apply` `clip`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp`/`surface` arms, the `MultiBlock` dataset construction, and the `RenderSpec.added` `add_mesh`/`add_volume` projection are settled fence code; the `glyph`/`streamlines` filter-axis names appear in `[04]-[IMPLEMENTATION_LAW]` but the `glyph(scale=, factor=)`/`streamlines(vectors=, source_center=)` kwarg spellings are not enumerated, so those two `apply` arms stay `[SCENE_VIEW]`-tracked RESEARCH; the `StructuredGrid`/`RectilinearGrid`/`ImageData` mesh rows are catalogued capacity the dataset growth axis absorbs. The `UnstructuredGrid`/`PolyData` mesh family, the `screenshot(path, window_size=)` PNG raster row (the `Image` arm writes to a temp-file sink through `png`), and the scene serializers are settled pyvista fence code; the osmesa/EGL offscreen software-GL backend is selected by the worker environment before `Plotter` construction. The `screenshot(return_img=True)` array-raster keyword (catalogued `screenshot(..., return_img=True, ...)`, returning the rgb24 NumPy array the `Frames` arm and `shoot` read), the `screenshot(transparent_background=)` keyword (catalogued in the same row), `Plotter.set_background` (catalogued), and the `[SCENE_VIEW]`-tracked `Plotter.camera`/`show_axes`/`Plotter(lighting=)`/`add_mesh(show_scalar_bar=, lighting=)`/`PolyData.decimate`, plus the `[ORBIT_FRAMES]`-tracked `Plotter.generate_orbital_path`/`path.points`/`Plotter.set_position`, are the [03]-[RESEARCH] catalogue-deepen seams; every other spelling is settled fence code.
- [SCENE_SEAM]: the `render_image`/`render_frames`/`render_plotter`/`shoot`/`png` bodies are module-level functions dispatched by qualified name across the `anyio.to_process.run_sync` subprocess seam the runtime `reliability/faults#FAULT` owner has already settled (`to_process.run_sync` cannot target a bound method or closure), so they stay out of the `Scene3d` owner deliberately, not as stray helpers; they import `pyvista` at module scope inside the gated-band worker, never on the cp315-core owner. The subprocess seam is settled fence code by inheritance from its owner; the `ContentIdentity.of` whole-payload and `tuple[ContentKey, ...]` Merkle-parent modalities the `Image` and `Frames` arms key through verify against the runtime `evidence/identity#IDENTITY` owner.
