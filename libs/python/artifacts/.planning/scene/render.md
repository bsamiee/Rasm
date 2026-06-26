# [PY_ARTIFACTS_SCENE_RENDER]

The 3D scientific-visualization render owner. `Scene3d` renders pyvista datasets on the VTK engine — `UnstructuredGrid`/`PolyData`/`MultiBlock`, scalar fields, the `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp`/`glyph`/`streamlines`/`decimate`/`surface` filter family, the surface/volume/point-cloud render styles, PBR material, the publication-quality render-control family (SSAO, depth-peeling, eye-dome lighting, shadows, anti-aliasing, parallel projection), scalar-bar/axes overlays, and a `CameraPose` viewpoint — to a host-free offscreen image, and emits the orbit/turntable rgb24 frame sequence the cross-folder `media/video#MEDIA` encoder consumes. `SceneOp` is ONE closed-payload `expression.tagged_union` over the `Image`/`Export`/`Frames` modalities, each case carrying its typed payload, dispatched by one total `match` returning `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` so every render mints its content key AND its evidence receipt in one carrier, exactly as the `media/video#MEDIA` encode sibling does. `RenderSpec` is the one frozen render-policy value — window, scalar field, colormap, clim, opacity, the `RenderStyle` surface/volume/points discriminant, the PBR `pbr`/`metallic`/`roughness`/`show_edges` material band, the `RenderFeature` overlay/quality `frozenset`, the `anti_aliasing` mode, the `CameraPose` viewpoint, the background, transparency, and the `FieldFilter` pre-render filter chain — folded into every arm through its bound `staged`/`added`/`viewed` projections, never loose constructor fields the implementer re-derives per call. `FieldFilter` is the one closed-payload `tagged_union` over the catalogued pyvista filter family, each case carrying its typed geometry/range/factor payload and folding to a new `DataSet` through one total `apply` `match`, so a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a parallel filtered-mesh type. The raw mesh is admitted EXACTLY ONCE through `pyvista.wrap` (the catalogued zero-copy entry) on the worker, so the interior never re-validates a provider shape and the filter fold runs over an admitted `DataSet`, never the raw cross-seam payload. The render path is offscreen software GL (osmesa/EGL) so a scene rasterizes with zero display, browser, or GPU. The owner rides the gated `python_version<'3.13'` native VTK floor: no cp315 VTK wheel, so the cp315-core process imports neither `pyvista` nor `vtk`, and the whole render crosses the runtime subprocess lane onto the sub-3.13 companion-floor worker — floor-gated planned capability, not a blocked spike. The `Frames` arm returns the `tuple[NDArray[np.uint8], ...]` keyed rgb24 raster `media/video#MEDIA` ingests directly through `VideoFrame.from_ndarray`, never a lossy PNG-bytes intermediary. The `Export` file-target arms (`scene/export#EXPORT`) and the USD/USDZ stage-authoring arm (`scene/stage#STAGE`) compose this owner's offscreen plotter, never re-owning the dataset/filter/render policy. This page closes the `SCENE_TIMESERIES_FRAMES` and `SCENE_FILTER_PIPELINE` ideas.

## [01]-[INDEX]

- [02]-[SCENE]: the one `Scene3d` owner over the closed-payload `SceneOp` family — `Image`/`Frames` (plus the `Export` arm delegated to `scene/export#EXPORT` and the USD/USDZ arm to `scene/stage#STAGE`) folding into one `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]`, the `RenderSpec` render-policy value with its bound `staged`/`added`/`viewed` projections, the twelve-case `FieldFilter` closed-payload filter family folded by `RenderSpec.staged` before render, the `RenderStyle` surface/volume/points style discriminant and PBR material band the `added` projection reads, the `RenderFeature` overlay/quality `frozenset` the `_FEATURE` table folds in `viewed`, the `CameraPose` viewpoint the `viewed` projection writes onto `camera_position`, the `OrbitPath` `(factor, elevation)`-column vocabulary keyed inside the `Frames` payload, the `tuple[NDArray[np.uint8], ...]` rgb24 frame seam to `media/video#MEDIA`, and the `ArtifactReceipt.Scene(key, target, bytes)` evidence each arm mints; `pyvista` `wrap`/`Plotter(off_screen=True)`/`add_mesh`/`add_volume`/`add_points`/`screenshot`/`set_background`/`add_axes`/`add_scalar_bar`/`camera_position`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_anti_aliasing`/`enable_parallel_projection`/`extract_surface`/`clip`/`slice`/`threshold`/`contour`/`warp_by_scalar`/`decimate` settled against the folder `.api`, with the `glyph`/`streamlines`/`slice_orthogonal`/`clip_box`/`clip_scalar` kwarg spellings the lone [03]-[RESEARCH] catalogue-deepen seam, and the orbit `camera_position` walk computed through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`.

## [02]-[SCENE]

- Owner: `Scene3d` the one 3D-scene render owner discriminating modality over the closed `SceneOp` family; `SceneOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Scene3d` subclass; `RenderSpec` the one frozen render-policy value carrying its own `staged` filter-fold, `added` mesh-add, and `viewed` camera/overlay/quality projections so a new render knob is one `RenderSpec` field bound into an existing projection, never a constructor-parameter tail nor a re-derived add-mesh/camera call; `RenderStyle` the closed `StrEnum` (`SURFACE`/`VOLUME`/`POINTS`) the `added` `match` reads to dispatch `add_mesh`/`add_volume`/`add_points`, collapsing the prior two-body `volume: bool` flag into one style vocabulary that admits the point-cloud modality the flag could not name; `RenderFeature` the closed overlay/quality `StrEnum` (`AXES`/`SCALAR_BAR`/`SSAO`/`DEPTH_PEEL`/`EYE_DOME`/`SHADOWS`/`PARALLEL`) whose membership `frozenset` the `_FEATURE` `frozendict` folds onto `add_axes`/`add_scalar_bar`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_parallel_projection`, collapsing eight parallel bool fields into one behavior-carrying set so a new render-quality toggle is one member plus one row; `CameraPose` the closed `NamedTuple` viewpoint value (`position`/`focal_point`/`view_up`) the `viewed` projection writes onto the catalogued `Plotter.camera_position` property as the `[position, focal_point, view_up]` triple, so a static viewpoint and the orbit trajectory share one camera-policy value, never loose camera kwargs nor a phantom settable-azimuth accessor; `FieldFilter` the closed-payload `tagged_union` over the catalogued pyvista filter family — `Clip`/`ClipBox`/`ClipScalar`/`Slice`/`SliceOrthogonal`/`Threshold`/`Contour`/`Warp`/`Glyph`/`Streamlines`/`Decimate`/`Surface` — each case carrying its typed geometry/range/factor payload and folding to a new `DataSet` through one total `apply` `match`, so a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a per-filter mesh wrapper nor an erased filter-name bag; `OrbitPath` the closed orbit-trajectory `Enum` carrying its `(factor, elevation)` columns (`AZIMUTH`/`WIDE`/`TIGHT`) so the orbit radius multiplier and height fraction are member-bound values the numpy `camera_position` walk reads directly, never a `StrEnum` whose string value is mis-passed as a float; `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` the one carrier every arm returns, the `media/video#MEDIA` `tuple[NDArray[np.uint8], ...]` rgb24 frame payload keyed through `ContentIdentity.of` so the producer hands MEDIA the raw `screenshot(return_img=True)` raster array `VideoFrame.from_ndarray(array, format="rgb24")` ingests directly, never a lossy PNG-bytes round-trip; the offscreen `Plotter(off_screen=True)` is the one render capsule per modality, never retained across arms, and the `Export`/USD arms re-enter the same `render_plotter` capsule through `scene/export#EXPORT`/`scene/stage#STAGE`.
- Cases: `SceneOp` cases — `Image(grid, spec)` (offscreen `Plotter.screenshot(path, window_size=)` PNG raster to a temp-file sink, the `RenderSpec.window` carried onto the catalogued `screenshot` keyword, the `RenderSpec.staged` filter chain applied over the `pyvista.wrap`-admitted dataset before the `RenderSpec.added` style/scalar/material mesh-add and the `RenderSpec.viewed` background/overlay/quality/camera fold) · `Export(grid, target, spec)` (one file-target axis whose `SceneTarget` row keys the exporter arm owned by `scene/export#EXPORT` for `GLTF`/`VRML`/`OBJ`/`HTML` and by `scene/stage#STAGE` for `USD`/`USDZ`, the format discriminated by the typed `SceneTarget` value crossing the seam as its `.value` and re-admitted once at the worker dispatch — never a parallel per-format export surface) · `Frames(grid, orbit, steps, spec)` (one orbit/turntable frame-sequence axis whose `OrbitPath` `(factor, elevation)` columns key the numpy `camera_position` orbit walk computed from the auto-framed viewpoint, the per-step loop placing the camera through the catalogued `camera_position` property and emitting one offscreen `screenshot(return_img=True)` rgb24 raster array per camera step folded into the `tuple[NDArray[np.uint8], ...]` sequence `media/video#MEDIA` ingests through `VideoFrame.from_ndarray` with zero file round-trip — never a parallel rotating-scene producer, the rotating-scene and chart-over-time frame sources sharing this one arm) — matched by one total `match`/`case`, the `Image`/`Export`/`Frames` modality recovered from the `SceneOp` discriminant, never a name suffix.
- Entry: `Scene3d.render` is `async` over the runtime `async_boundary`, which converts any worker raise into the runtime `BoundaryFault` rail and records it on the active span once, so this owner mints no parallel render-fault vocabulary and the `to_process` `BrokenWorkerProcess` worker death lands as the `resource` case the `CLASSIFY` table already routes. The whole render dispatches onto the sub-3.13 companion-floor worker (the cp315-core process imports neither `pyvista` nor `vtk`); the worker admits the raw mesh through `pyvista.wrap`, configures one offscreen `Plotter(off_screen=True)` through `render_plotter`, applies `RenderSpec.staged`, binds through `RenderSpec.added`, folds the overlay/quality/camera state through `RenderSpec.viewed`, and reads the modality through a split sink — `png` (temp-file PNG file artifact) for the `Image`/`Export`-`PNG` raster, `shoot` (`screenshot(return_img=True)` rgb24 array, no file) for the `Frames` sequence. The `Image`/`Export` arms key one `ContentKey` over the single payload; the `Frames` arm keys one Merkle-parent `ContentKey` over the per-frame children through the `tuple[ContentKey, ...]` modality of `ContentIdentity.of` (each child keyed over the frame array's `tobytes()` content under the `_FRAME_FORMAT` `"rgb24"` tag, never a `"png"` tag for a payload that carries no PNG), so the frame sequence is one content-addressed unit MEDIA re-admits by reference and ingests array-by-array.
- Auto: `render_plotter` folds the cross-seam payload into a `DataSet` through one `pyvista.wrap` (the catalogued zero-copy entry over a numpy buffer / VTK object / `trimesh`/`meshio` mesh), so the interior is total over an admitted dataset; `RenderSpec.staged` folds the `FieldFilter` tuple over it through `functools.reduce`, each case's `apply` `match` routing to the catalogued `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`glyph`/`streamlines`/`decimate`/`extract_surface` filter and returning a new `DataSet`; `RenderSpec.added` `match`es the `RenderStyle` to `add_mesh`/`add_volume`/`add_points`, binding the `scalars` active name, the `colormap` from `graphic/color/derive#DERIVE` (the `ColorReceipt.coords` palette the derivation owner resolves), the `clim` window and `opacity` through one filtered kwarg dict that drops `None`, and for `SURFACE` the `pbr`/`metallic`/`roughness`/`show_edges` material band; `RenderSpec.viewed` folds `background` onto `set_background`, the `RenderFeature` `frozenset` onto the `_FEATURE` enable-table (`add_axes`/`add_scalar_bar`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_parallel_projection`), the `anti_aliasing` mode onto `enable_anti_aliasing`, and a non-empty `CameraPose` onto the `camera_position` triple; the `RenderSpec.window`/`RenderSpec.transparent` fold onto the catalogued `screenshot(window_size=, transparent_background=)` keywords; the `Frames` arm reads the auto-framed `camera_position` for the orbit center and radius, scales the radius by `OrbitPath.factor` and the height by `OrbitPath.elevation`, walks the `numpy.linspace` azimuth sweep placing `camera_position` per step, and reads each step's offscreen `shoot` raster array into the sequence.
- Receipt: every arm returns `(key, ArtifactReceipt.Scene(key, target, bytes))` so the receipt is minted inside `_emit` over the produced payload, exactly as the `media/video#MEDIA` sibling mints `ArtifactReceipt.Media` in its `_emit`. The `Image` arm contributes the `"png"` target plus `len(data)`, the `Export` arm the `SceneTarget` value (`scene/export#EXPORT`/`scene/stage#STAGE`) plus the serialized scene-file byte count, and the `Frames` arm the `_FRAME_FORMAT` `"rgb24"` target plus the frame-sequence byte total `sum(frame.nbytes for ...)` — the raw frame-sequence artifact's own evidence, a distinct content-addressed unit from the encoded video `media/video#MEDIA` separately keys and mints `ArtifactReceipt.Media` over, so the two artifacts carry two keys and two receipts, never a double-counted single rail. The point/cell/window render evidence the `pyvista` `.api` evidence row names lands when the `core/receipt#RECEIPT` `scene` case widens beyond `(target, bytes)`.
- Packages: `pyvista` (`wrap`/`Plotter(off_screen=True)`/`add_mesh`/`add_volume`/`add_points`/`screenshot`/`set_background`/`add_axes`/`add_scalar_bar`/`camera_position`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_anti_aliasing`/`enable_parallel_projection`/`extract_surface`/`clip`/`slice`/`threshold`/`contour`/`warp_by_scalar`/`decimate` all settled against the folder `.api`; the `glyph(scale=, factor=)`/`streamlines(vectors=, source_center=)`/`slice_orthogonal(x=, y=, z=)`/`clip_box(bounds=)`/`clip_scalar(scalars=, value=, invert=)` kwarg spellings the [03]-[RESEARCH] deepen seam, the filter NAMES catalogued in the filter axis) gated `python_version<'3.13'`; `numpy` (the orbit `linspace`/`cos`/`sin`/`linalg.norm`/`asarray` `camera_position` computation and the `NDArray[np.uint8]` rgb24 frame-array backing the `shoot`/`render_frames` worker returns and MEDIA's `VideoFrame.from_ndarray` ingests, imported only on the gated-band worker, the owner page crossing the seam as opaque payloads and never importing numpy); `frozendict` (the `_FEATURE` enable-table); `expression` (`tagged_union`/`tag`/`case`); the `core/receipt#RECEIPT` `ArtifactReceipt.Scene` evidence case; runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane).
- Growth: a new render knob is one `RenderSpec` field bound into the existing `staged`/`added`/`viewed` projection; a new render-quality toggle (legend, bounding box, orientation widget, SSAA tier) is one `RenderFeature` member plus one `_FEATURE` row, or one `RenderSpec` field plus one `viewed` line; a new material knob is one `RenderSpec` field bound into the `added` `SURFACE` arm; a new render style is one `RenderStyle` member plus one `added` `match` arm; a new field-visualization filter is one `FieldFilter` case plus one `apply` `match` arm over the catalogued dataset-filter family, dispatched by the same `RenderSpec.staged` reduce — never a parallel filtered-mesh type; a new camera control is one `CameraPose` field the `viewed` projection reads; a new orbit trajectory is one `OrbitPath` member carrying its own `(factor, elevation)` columns; a new scene file-export is one `SceneTarget` row plus one exporter arm in `scene/export#EXPORT` or `scene/stage#STAGE`; a new render-evidence fact is one slot on the `core/receipt#RECEIPT` `scene` case; zero new surface — the modality space stays three cases (`Image`/`Export`/`Frames`) on one owner, every addition a row, field, case, member, or arm.
- Boundary: no interactive viewer, no UI window, no display server; the dataset arrives from data/compute outputs and is `pyvista.wrap`-admitted once on the worker; the owner owns no mesh-file interchange (that stays at `geometry`, the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam recording that scene = visualization-scene export and geometry mesh = mesh-file codec, no shared owner). The offscreen software-GL render is the host-free path; `pyvista`/`vtk` ride the gated `python_version<'3.13'` band, none resolving in the cp315-core process, so the entire render runs through the runtime subprocess lane (`anyio.to_process.run_sync`) onto the sub-3.13 companion-floor worker that imports them at module scope. The `Frames` arm is the one frame source feeding `media/video#MEDIA`: the `tuple[NDArray[np.uint8], ...]` rgb24 raster sequence crosses the intra-folder counterpart seam keyed by `ContentIdentity.of`, and MEDIA encodes it cp315-core in-process over PyAV through `VideoFrame.from_ndarray(array, format="rgb24")` with no intervening PNG encode/decode. The deleted forms are the phantom `Plotter.camera`/`camera.azimuth`/`camera.elevation`/`camera.zoom()`/`show_axes()`/`show_scalar_bar=`/`Plotter(lighting=)` non-catalogued camera-and-overlay spellings and the illusory settable-azimuth camera body (replaced by the catalogued `camera_position` property plus `add_axes`/`add_scalar_bar`/the `enable_*` render-control family); the phantom `generate_orbital_path`/`set_position`/`path.points` orbit trio absent from the catalogue (replaced by the numpy `camera_position` walk); the missing ingress that fed the raw cross-seam payload straight to `.clip` (replaced by the one `pyvista.wrap` admission); the two-body `volume: bool` flag (replaced by the `RenderStyle` surface/volume/points vocabulary over `add_mesh`/`add_volume`/`add_points`); the eight parallel bool overlay/quality fields (collapsed into one `RenderFeature` `frozenset` over the `_FEATURE` table); the render-quality (SSAO/depth-peel/eye-dome/shadows/anti-aliasing/parallel-projection) and PBR (`pbr`/`metallic`/`roughness`/`show_edges`) capability the prior add-mesh-thin owner forfeited; and the receipt-less `RuntimeRail[ContentKey]` return that minted no evidence (replaced by the `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` carrying the `ArtifactReceipt.Scene` byte-count receipt the `core/plan#PLAN` planner reads).

```python signature
from collections.abc import Callable
from enum import Enum, StrEnum
from functools import reduce
from typing import Literal, NamedTuple, assert_never

from anyio import to_process
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.scene.export import SceneTarget, render_export
from rasm.artifacts.scene.render_worker import render_frames, render_image

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
                data = await to_process.run_sync(render_image, grid, spec)
                key = ContentIdentity.of(SceneTarget.PNG.value, data)
                return key, ArtifactReceipt.Scene(key, SceneTarget.PNG.value, len(data))
            case SceneOp(tag="export", export=(grid, target, spec)):
                data = await to_process.run_sync(render_export, grid, target.value, spec)
                key = ContentIdentity.of(target.value, data)
                return key, ArtifactReceipt.Scene(key, target.value, len(data))
            case SceneOp(tag="frames", frames=(grid, orbit, steps, spec)):
                sequence = await to_process.run_sync(render_frames, grid, orbit, steps, spec)
                key = ContentIdentity.of(
                    _FRAME_FORMAT, tuple(ContentIdentity.of(_FRAME_FORMAT, frame.tobytes()) for frame in sequence)
                )
                return key, ArtifactReceipt.Scene(key, _FRAME_FORMAT, sum(frame.nbytes for frame in sequence))
            case _:
                assert_never(self.op)
```

The gated-band worker bodies (`render_plotter`/`shoot`/`png`/`render_image`/`_orbit`/`render_frames`) are module-level functions dispatched by qualified name across the `anyio.to_process.run_sync` subprocess seam (the seam cannot target a bound method or closure), so they import `pyvista`/`numpy` at module scope inside the gated sub-3.13 worker only, never on the cp315-core owner. `render_plotter` admits the raw mesh ONCE through `pyvista.wrap` (the catalogued zero-copy entry over a numpy buffer / VTK object / `trimesh`/`meshio` mesh), applies `RenderSpec.staged`, binds through `RenderSpec.added`, folds `RenderSpec.viewed`, and is the shared render window the `Image`/`Frames` sinks and the `scene/export#EXPORT`/`scene/stage#STAGE` arms all read; `_orbit` computes the azimuthal `camera_position` walk from the auto-framed viewpoint through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`, and `render_frames` walks it placing the camera per step through the catalogued `camera_position` property.

```python signature
from pathlib import Path
from tempfile import TemporaryDirectory

import numpy as np
import pyvista as pv
from numpy.typing import NDArray

from rasm.artifacts.scene.render import OrbitPath, RenderSpec, Vec3


def render_plotter(grid: object, spec: RenderSpec) -> "pv.Plotter":
    plotter = pv.Plotter(off_screen=True)
    spec.added(plotter, spec.staged(pv.wrap(grid)))
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

    def _capture(view: tuple[Vec3, Vec3, Vec3]) -> NDArray[np.uint8]:
        plotter.camera_position = list(view)
        return shoot(plotter, spec)

    return tuple(_capture(view) for view in _orbit(plotter, orbit, steps))
```

## [03]-[RESEARCH]

- [FILTER_KWARGS] [RESEARCH]: the `glyph(scale=, factor=)`, `streamlines(vectors=, source_center=)`, `slice_orthogonal(x=, y=, z=)`, `clip_box(bounds=)`, and `clip_scalar(scalars=, value=, invert=)` kwarg spellings the `FieldFilter.apply` arms use are the lone deepen seam: the folder `.api` catalogue for `pyvista` NAMES every filter in its filter axis (`clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`extract_surface`/`warp_by_scalar`/`glyph`/`streamlines`/`decimate` are dataset-method filters returning a new dataset) and enumerates the full kwarg signature for `clip`/`slice`/`threshold`/`contour`/`extract_surface`/`warp_by_scalar`/`decimate`, but does NOT enumerate the kwargs for `glyph`/`streamlines`/`slice_orthogonal`/`clip_box`/`clip_scalar`, so those five `apply` arms stay marked RESEARCH until a kwarg-reflection pass lands on the gated `python_version<'3.13'` band. Close-condition: `.api` catalogue carries the `glyph`/`streamlines`/`slice_orthogonal`/`clip_box`/`clip_scalar` kwarg signatures.
- [PYVISTA_RENDER]: the render, ingest, filter, overlay, quality, camera, and capture surface verifies against the folder `.api` catalogue for `pyvista` (`0.48.4` on `vtk` `9.6.2`, the gated `python_version<'3.13'` sub-3.13 companion floor): `wrap` (the zero-copy `vtkDataObject`/`numpy.ndarray`/`trimesh`/`meshio` admission entry), `Plotter(off_screen=True)`, `add_mesh(scalars=, cmap=, clim=, opacity=, pbr=, metallic=, roughness=, show_edges=)`, `add_volume(scalars=, clim=, opacity=, cmap=)`, `add_points(style='points')`, `screenshot(filename=, return_img=True, window_size=, transparent_background=)`, `set_background(color, top=)`, `add_axes()`, `add_scalar_bar()`, `camera_position` (property, both read for the orbit and written for the static `CameraPose`), `enable_ssao()`/`enable_depth_peeling()`/`enable_eye_dome_lighting()`/`enable_shadows()`/`enable_parallel_projection()`/`enable_anti_aliasing('ssaa'|'msaa'|'fxaa')`, `extract_surface()`, `clip(normal=, origin=)`, `slice(normal=, origin=)`, `threshold(value=, scalars=)`, `contour(isosurfaces=, scalars=)`, `warp_by_scalar(scalars=, factor=)`, and `decimate(target_reduction=)` are all settled fence code. The `UnstructuredGrid`/`PolyData`/`MultiBlock` mesh family and the `StructuredGrid`/`RectilinearGrid`/`ImageData` rows are catalogued capacity the dataset growth axis absorbs; the osmesa/EGL offscreen software-GL backend is selected by the worker environment before `Plotter` construction. The orbit `camera_position` walk is computed through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`/`asarray` (verified against the universal `numpy` `.api`), reading the auto-framed viewpoint for the orbit center and radius rather than a non-catalogued `generate_orbital_path`/`set_position`/`path.points` trio.
- [SCENE_SEAM]: the `render_plotter`/`shoot`/`png`/`render_image`/`_orbit`/`render_frames` bodies are module-level functions dispatched by qualified name across the `anyio.to_process.run_sync` subprocess seam the runtime `reliability/faults#FAULT` owner has already settled (`to_process.run_sync` cannot target a bound method or closure), importing `pyvista`/`numpy` at module scope inside the gated-band worker only, never on the cp315-core owner. The `async_boundary` rail conversion (any worker raise to `BoundaryFault`, the `BrokenWorkerProcess` worker death to the `resource` case, the active-span record), the `ContentIdentity.of` whole-payload and `tuple[ContentKey, ...]` Merkle-parent modalities the `Image`/`Export` and `Frames` arms key through, and the `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes)` mint each arm returns are settled fence code by inheritance from their runtime and receipt owners.
