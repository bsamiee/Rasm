# [PY_ARTIFACTS_SCENE_RENDER]

The 3D scientific-visualization render owner. `Scene3d` renders pyvista datasets on the VTK engine — `UnstructuredGrid`/`PolyData`/`MultiBlock`/`ImageData`, scalar fields, the full `DataObjectFilters`/`DataSetFilters` family (`clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`warp_by_vector`/`glyph`/`streamlines`/`decimate`/`extract_surface` PLUS the mesh-repair `smooth`/`subdivide`/`fill_holes`/`clean` and the `cell_data_to_point_data`/`point_data_to_cell_data` field-transfer siblings), the surface/volume/point-cloud/vector-arrow/point-label/wireframe-linework render styles, the PBR `metallic`/`roughness` and the classic-lighting `ambient`/`diffuse`/`specular`/`smooth_shading` material bands, the `add_volume` opacity-transfer-function/blending/mapper volumetric band, the publication-quality render-control family (SSAO, depth-peeling, eye-dome lighting, shadows, anti-aliasing, parallel projection), scalar-bar/axes overlays, `add_text` title/annotation callouts, and a `CameraPose` viewpoint — to a host-free offscreen image, and emits the orbit/turntable rgb24 frame sequence the cross-folder `media/container#CONTAINER` encoder consumes. `SceneOp` is ONE closed-payload `expression.tagged_union` over the `Image`/`Export`/`Frames`/`Ingest`/`Compose` modalities (the `Ingest` case the `import_gltf`/`import_obj`/`import_vrml` render-tune-re-export round-trip inverse over an existing scene file, the `Compose` case the two-operand boolean-CSG/field-sample the single-operand filter cannot own), each case carrying its typed payload, dispatched by one total `match` returning `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` so every render mints its content key AND its evidence receipt in one carrier, exactly as the `media/container#CONTAINER` encode sibling does. `RenderSpec` is the one frozen render-policy value — window, scalar field, colormap (a matplotlib name OR the `graphic/color/derive#DERIVE` resolved palette LIST), clim, opacity, the `RenderStyle` surface/volume/points/arrows/labels/lines discriminant, the PBR + classic-lighting material band, the `add_volume` transfer-function/blending/mapper band, the `RenderFeature` overlay/quality `frozenset`, the `anti_aliasing` mode, the `CameraPose` viewpoint, the `title`/`annotations` callout band, the background, transparency, the `FieldFilter` pre-render filter chain, AND the `up_axis`/`meters_per_unit`/`labels` AEC-deliverable metadata band the USD export threads — folded into every arm through its bound `staged`/`added`/`viewed` projections, never loose constructor fields the implementer re-derives per call. `FieldFilter` is the one closed-payload `tagged_union` over the full catalogued pyvista filter family (20 cases: clip/slice/threshold/contour, warp-scalar/warp-vector, glyph/streamlines/decimate/surface, the mesh-repair smooth/subdivide/fill-holes/clean, the cell↔point field-transfer siblings, and the MultiBlock-combine block-merge), each case carrying its typed geometry/range/factor payload at the depth the catalogued pyvista kwargs admit (clip carries crinkle, contour its extraction method, glyph orient/tolerance/clamping, streamlines source-radius/n-points/direction/max-time, decimate volume-preservation) and folding to a new `DataSet` through one total `apply` `match`, so a repair-then-slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple, never a parallel filtered-mesh type. The raw mesh is admitted EXACTLY ONCE through `pyvista.wrap` (the catalogued zero-copy entry) on the worker, so the interior never re-validates a provider shape and the filter fold runs over an admitted `DataSet`, never the raw cross-seam payload. The render path is offscreen software GL (osmesa/EGL) so a scene rasterizes with zero display, browser, or GPU. The owner rides the native-VTK worker lane: the runtime process imports neither `pyvista` nor `vtk`, and the whole render crosses the runtime subprocess lane. The `Frames` arm returns the `tuple[NDArray[np.uint8], ...]` keyed rgb24 raster `media/container#CONTAINER` ingests directly through `VideoFrame.from_ndarray`, never a lossy PNG-bytes intermediary. The `Export` file-target arms (`scene/export#EXPORT`) compose this owner's offscreen plotter for the `GLTF`/`VRML`/`OBJ`/`HTML` exporters, while the USD/USDZ arm authors the admitted dataset's extracted surface — with its scalar-mapped per-vertex colour, its PBR material, and its AEC labels — through `scene/stage#STAGE` (`usd-core`) with no render pass; both ride this owner's worker, never re-owning the dataset/filter/render policy. `SceneTarget` — the closed file-target vocabulary those arms key — lives HERE (hoisted; `scene/export#EXPORT` composes it downward), so the eager scene module graph runs export → render only and the runtime render module imports nothing from export. This page closes the `SCENE_TIMESERIES_FRAMES` and `SCENE_FILTER_PIPELINE` ideas.

## [01]-[INDEX]

- [02]-[SCENE]: the one `Scene3d` owner over the closed-payload `SceneOp` family — `Image`/`Frames`/`Ingest`/`Compose` (plus the `Export` arm delegated to `scene/export#EXPORT` and the USD/USDZ arm to `scene/stage#STAGE`) folding into one `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]`, the `RenderSpec` render-policy value with its bound `staged`/`added`/`viewed` projections, the twenty-case `FieldFilter` closed-payload filter family (clip/slice/threshold/contour, warp-scalar/warp-vector, glyph/streamlines/decimate/surface, mesh-repair smooth/subdivide/fill-holes/clean, cell↔point transfer, MultiBlock-combine) folded by `RenderSpec.staged` before render, the `RenderStyle` surface/volume/points/arrows/labels/lines style discriminant with the PBR + classic-lighting material band and the `add_volume` transfer-function/blending/mapper band the `added` projection reads, the `RenderFeature` overlay/quality `frozenset` the `_FEATURE` table folds in `viewed`, the `title`/`annotations` `add_text` callout band, the `CameraPose` viewpoint the `viewed` projection writes onto `camera_position`, the `OrbitPath` `(factor, elevation)`-column vocabulary keyed inside the `Frames` payload, the render-owned `SceneTarget` closed file-target vocabulary keyed inside the `Export`/`Ingest` payloads (hoisted here — `scene/export#EXPORT` composes it downward), the `SceneSource` glTF/OBJ/VRML import vocabulary keyed inside the `Ingest` payload, the `BoolOp` union/difference/intersection/sample vocabulary keyed inside the `Compose` payload, the `up_axis`/`meters_per_unit`/`labels` AEC-deliverable metadata band the USD export reads, the `tuple[NDArray[np.uint8], ...]` rgb24 frame seam to `media/container#CONTAINER`, and the `ArtifactReceipt.Scene(key, target, bytes, facts)` evidence each arm mints (the `facts` `frozendict` band carrying the render window/frame counts, the boolean op, and the `scene/stage#STAGE` USD prim/layer/up-axis/meters-per-unit/extent-diagonal stats); `pyvista` `wrap`/`Plotter(off_screen=True)`/`add_mesh`(with `ambient`/`diffuse`/`specular`/`smooth_shading`, `style="wireframe"`/`line_width`)/`add_volume`(with `opacity`/`blending`/`mapper`)/`add_points`/`add_arrows`/`add_point_labels`/`add_text`/`import_gltf`/`import_obj`/`import_vrml`/`screenshot`/`set_background`/`add_axes`/`add_scalar_bar`/`camera_position`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_anti_aliasing`/`enable_parallel_projection`/`extract_surface`/`triangulate`/`clip`/`slice`/`threshold`/`contour`/`warp_by_scalar`/`warp_by_vector`/`decimate`/`smooth`/`subdivide`/`fill_holes`/`clean`/`cell_data_to_point_data`/`point_data_to_cell_data`/`MultiBlock.combine`/`boolean_union`/`boolean_difference`/`boolean_intersection`/`sample` plus the `.points`/`.regular_faces`/`.point_normals`/`.point_data` numpy accessors the `surface_arrays` USD seam reads settled against the folder `.api`, the scalar->colormap per-vertex RGB mapped through `matplotlib.colormaps`/`ListedColormap`/`Normalize`, and the orbit `camera_position` walk computed through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`.

## [02]-[SCENE]

- Owner: `Scene3d` the one 3D-scene render owner discriminating modality over the closed `SceneOp` family; `SceneOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` bag nor a per-modality `Scene3d` subclass; `RenderSpec` the one frozen render-policy value carrying its own `staged` filter-fold, `added` mesh-add, and `viewed` camera/overlay/quality/callout projections so a new render knob is one `RenderSpec` field bound into an existing projection, never a constructor-parameter tail nor a re-derived add-mesh/camera call; `RenderStyle` the closed `StrEnum` (`SURFACE`/`VOLUME`/`POINTS`/`ARROWS`/`LABELS`/`LINES`) the `added` `match` reads to dispatch `add_mesh`/`add_volume`/`add_points`/`add_arrows`/`add_point_labels`/`add_mesh(style="wireframe")`, collapsing the prior two-body `volume: bool` flag into one style vocabulary that admits the point-cloud, vector-arrow, point-label, and drawing-edge-linework modalities the flag could not name — the `SURFACE` arm carrying BOTH the `pbr`/`metallic`/`roughness` PBR band AND the classic-lighting `ambient`/`diffuse`/`specular`/`smooth_shading` band, the `VOLUME` arm carrying the `add_volume` opacity-transfer-function/`blending`/`mapper` band that a raw scalar `opacity` float cannot name, and the `LINES` arm carrying the `line_width` wireframe representation the AEC drawing-edge plane renders through; `RenderFeature` the closed overlay/quality `StrEnum` (`AXES`/`SCALAR_BAR`/`SSAO`/`DEPTH_PEEL`/`EYE_DOME`/`SHADOWS`/`PARALLEL`) whose membership `frozenset` the `_FEATURE` `frozendict` folds onto `add_axes`/`add_scalar_bar`/`enable_ssao`/`enable_depth_peeling`/`enable_eye_dome_lighting`/`enable_shadows`/`enable_parallel_projection`, collapsing eight parallel bool fields into one behavior-carrying set so a new render-quality toggle is one member plus one row; `CameraPose` the closed `NamedTuple` viewpoint value (`position`/`focal_point`/`view_up`) the `viewed` projection writes onto the catalogued `Plotter.camera_position` property as the `[position, focal_point, view_up]` triple, so a static viewpoint and the orbit trajectory share one camera-policy value, never loose camera kwargs nor a phantom settable-azimuth accessor; `FieldFilter` the closed-payload `tagged_union` over the full catalogued pyvista `DataObjectFilters`/`DataSetFilters` family — `Clip`/`ClipBox`/`ClipScalar`/`Slice`/`SliceOrthogonal`/`Threshold`/`Contour`/`Warp`/`WarpVector`/`Glyph`/`Streamlines`/`Decimate`/`Surface` plus the mesh-repair `Smooth`/`Subdivide`/`FillHoles`/`Clean`, the `CellToPoint`/`PointToCell` field-transfer siblings, and the `Combine` MultiBlock-merge — each case carrying its typed payload at the depth the pyvista kwargs admit (the marker cases carrying a `bool`, `Clip` a `crinkle` flag, `Contour` its `method`, `Glyph` `orient`/`tolerance`/`clamping`, `Streamlines` `source_radius`/`n_points`/`integration_direction`/`max_time`, `Decimate` `volume_preservation`) and folding to a new `DataSet` through one total `apply` `match`, so a clean-then-fill-holes-then-slice-then-threshold-then-glyph repair-and-visualize chain is a `RenderSpec.filters` tuple folded by `functools.reduce`, never a per-filter mesh wrapper nor an erased filter-name bag; the binary CSG `boolean_union`/`boolean_difference`/`boolean_intersection` and the `sample(target)` field-transfer are NOT filter cases — they need a second fielded mesh operand `FieldFilter.apply` (which runs pyvista-free over `object` at the OWNER fence) cannot construct, so they ride the dedicated two-operand `SceneOp.Compose(grid_a, grid_b, BoolOp, spec)` modality whose `render_compose` worker extracts both surfaces, folds the `BoolOp`-keyed boolean/sample, and renders the composite — the CSG-of-building-elements (a wall cut by an opening, intersected solids) AEC case and the composite-scientific-mesh pub case a single-operand filter genuinely cannot own; `BoolOp` the closed two-operand `StrEnum` (`UNION`/`DIFFERENCE`/`INTERSECTION`/`SAMPLE`) keyed inside the `Compose` payload; `OrbitPath` the closed orbit-trajectory `Enum` carrying its `(factor, elevation)` columns (`AZIMUTH`/`WIDE`/`TIGHT`) so the orbit radius multiplier and height fraction are member-bound values the numpy `camera_position` walk reads directly, never a `StrEnum` whose string value is mis-passed as a float; `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` the one carrier every arm returns, the `media/container#CONTAINER` `tuple[NDArray[np.uint8], ...]` rgb24 frame payload keyed through the synchronous bare `ContentIdentity.key` so the producer hands MEDIA the raw `screenshot(return_img=True)` raster array `VideoFrame.from_ndarray(array, format="rgb24")` ingests directly, never a lossy PNG-bytes round-trip; the offscreen `Plotter(off_screen=True)` is the one render capsule per modality, never retained across arms — the `Export` render-sink arms (`GLTF`/`VRML`/`OBJ`/`HTML`) re-enter the same `render_plotter` capsule through `scene/export#EXPORT`, while the USD/USDZ arm authors plotter-free through `scene/stage#STAGE` from the `surface_arrays` buffers (now carrying the mapped per-vertex colour), paying no offscreen-GL render at all.
- Cases: `SceneOp` cases — `Image(grid, spec)` (offscreen `Plotter.screenshot(path, window_size=)` PNG raster to a temp-file sink, the `RenderSpec.window` carried onto the catalogued `screenshot` keyword, the `RenderSpec.staged` filter chain applied over the `pyvista.wrap`-admitted dataset before the `RenderSpec.added` style/scalar/material mesh-add and the `RenderSpec.viewed` background/overlay/quality/callout/camera fold) · `Export(grid, target, spec)` (one file-target axis whose `SceneTarget` row keys the exporter arm owned by `scene/export#EXPORT` for `GLTF`/`VRML`/`OBJ`/`HTML` and by `scene/stage#STAGE` for `USD`/`USDZ`, the format discriminated by the typed `SceneTarget` value crossing the seam as its `.value` and re-admitted once at the worker dispatch — never a parallel per-format export surface) · `Frames(grid, orbit, steps, spec)` (one orbit/turntable frame-sequence axis whose `OrbitPath` `(factor, elevation)` columns key the numpy `camera_position` orbit walk computed from the auto-framed viewpoint, the per-step loop placing the camera through the catalogued `camera_position` property and emitting one offscreen `screenshot(return_img=True)` rgb24 raster array per camera step folded into the `tuple[NDArray[np.uint8], ...]` sequence `media/container#CONTAINER` ingests through `VideoFrame.from_ndarray` with zero file round-trip — never a parallel rotating-scene producer, the rotating-scene and chart-over-time frame sources sharing this one arm) · `Ingest(scene, source, target, spec)` (the render-tune-re-export round-trip inverse — an existing glTF/OBJ/VRML scene file's `bytes` re-admitted through the pyvista `import_gltf`/`import_obj`/`import_vrml` importer keyed by the `SceneSource` discriminant, re-tuned by the `RenderSpec.viewed` camera/feature/quality/callout fold, then re-serialized to the `SceneTarget` through the `scene/export#EXPORT` `render_ingest` worker — the asset-conditioning inverse the catalogued in-process scene importer admits, keyed to `target` exactly as `Export`) · `Compose(grid_a, grid_b, op, spec)` (the two-operand boolean-CSG/field-sample modality a single-operand `FieldFilter` cannot carry — `render_compose` wraps and surface-extracts BOTH grids, folds the `BoolOp` `boolean_union`/`boolean_difference`/`boolean_intersection` watertight CSG or the `sample` field-transfer, then renders the composite through the shared `render_plotter` to a PNG raster, the boolean op carried onto the receipt `facts`; a wall cut by an opening, two intersected solids, or a field resampled across meshes) — matched by one total `match`/`case`, the `Image`/`Export`/`Frames`/`Ingest`/`Compose` modality recovered from the `SceneOp` discriminant, never a name suffix.
- Auto: `render_plotter` folds the cross-seam payload into a `DataSet` through one `pyvista.wrap` (the catalogued zero-copy entry over a numpy buffer / VTK object / `trimesh`/`meshio` mesh), so the interior is total over an admitted dataset; `RenderSpec.staged` folds the `FieldFilter` tuple over it through `functools.reduce`, each case's `apply` `match` routing to the catalogued `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`warp_by_scalar`/`warp_by_vector`/`glyph`/`streamlines`/`decimate`/`extract_surface`/`smooth`/`subdivide`/`fill_holes`/`clean`/`cell_data_to_point_data`/`point_data_to_cell_data`/`combine` filter at its full catalogued kwarg depth and returning a new `DataSet`, so a mesh-repair pre-pass (`clean`→`fill_holes`→`smooth`) or a `slice_orthogonal`→`combine` block-merge composes into the same tuple as a `slice`→`threshold`→`glyph` visualization; `RenderSpec.added` `match`es the `RenderStyle` to `add_mesh`/`add_volume`/`add_points`/`add_arrows`/`add_point_labels`/`add_mesh(style="wireframe")`, binding the `scalars` active name, the `colormap` from `graphic/color/derive#DERIVE` (a matplotlib name string OR the `ColorReceipt.notation` CSS-colour LIST `add_mesh(cmap=)` accepts directly), the `clim` window and `opacity` through one shared `drop`-`None` kwarg dict, and for `SURFACE` the `pbr`/`metallic`/`roughness`/`show_edges` PBR band PLUS the classic-lighting `ambient`/`diffuse`/`specular`/`smooth_shading` band, for `VOLUME` the `add_volume` `opacity` transfer-function name (winning over the scalar float) with `blending` and `mapper`, for `LINES` the `style="wireframe"` representation with `line_width`, and for `ARROWS`/`LABELS` the `mesh.points` centers with the `mesh[scalars]` vector/label field; `RenderSpec.viewed` folds `background` onto `set_background`, the `RenderFeature` `frozenset` onto the `_FEATURE` enable-table, the `title`/`annotations` onto `add_text` (the figure title at `upper_edge` and each `(text, position)` callout), the `anti_aliasing` mode onto `enable_anti_aliasing`, and a non-empty `CameraPose` onto the `camera_position` triple; the `RenderSpec.window`/`RenderSpec.transparent` fold onto the catalogued `screenshot(window_size=, transparent_background=)` keywords; the `Frames` arm reads the auto-framed `camera_position` for the orbit center and radius, scales the radius by `OrbitPath.factor` and the height by `OrbitPath.elevation`, walks the `numpy.linspace` azimuth sweep placing `camera_position` per step, and reads each step's offscreen `shoot` raster array into the sequence; the `Compose` arm surface-extracts both operands and folds the `BoolOp`-keyed `boolean_*`/`sample` before the shared `render_plotter`, so a CSG composite renders through the same style/material/overlay policy as a single mesh.
- Receipt: every arm returns `(key, ArtifactReceipt.Scene(key, target, bytes, facts))` so the receipt is minted inside `_emit` over the produced payload, exactly as the `media/container#CONTAINER` sibling mints `ArtifactReceipt.Media` in its `_emit`. The `Image`/`Compose` arms contribute the `"png"` target plus `len(data)` (the `Compose` arm adding the `boolean` op onto `facts`), the `Export` arm the `SceneTarget` value (`scene/export#EXPORT`/`scene/stage#STAGE`) plus the serialized scene-file byte count, and the `Frames` arm the `_FRAME_FORMAT` `"rgb24"` target plus the frame-sequence byte total `sum(frame.nbytes for ...)` — the raw frame-sequence artifact's own evidence, a distinct content-addressed unit from the encoded video `media/container#CONTAINER` separately keys and mints `ArtifactReceipt.Media` over, so the two artifacts carry two keys and two receipts, never a double-counted single rail. The window render evidence lands on the widened `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` `frozendict` band the `_emit` `Image`/`Frames`/`Compose` arms fill from `spec.window` (plus the frame count / boolean op), and the `scene/stage#STAGE` `ComputeUsdStageStats` prim/layer/up-axis/meters-per-unit/extent-diagonal stats thread back through the `render_export` `(bytes, facts)` return into the same band on the `Export` arm; the `pyvista` point/cell counts the `.api` evidence row names land as one more band key once the render worker returns them, never a new receipt case.
- Growth: a new render knob is one `RenderSpec` field bound into the existing `staged`/`added`/`viewed` projection; a new render-quality toggle (legend, bounding box, orientation widget, SSAA tier) is one `RenderFeature` member plus one `_FEATURE` row, or one `RenderSpec` field plus one `viewed` line; a new scene callout is one `annotations` entry; a new material knob is one `RenderSpec` field bound into the `added` `SURFACE` arm; a new render style is one `RenderStyle` member plus one `added` `match` arm; a new field-visualization filter is one `FieldFilter` case plus one `apply` `match` arm over the catalogued dataset-filter family, dispatched by the same `RenderSpec.staged` reduce, and a filter kwarg is one payload slot on its existing case — never a parallel filtered-mesh type; a new two-operand op is one `BoolOp` member plus one `render_compose` arm; a new camera control is one `CameraPose` field the `viewed` projection reads; a new orbit trajectory is one `OrbitPath` member carrying its own `(factor, elevation)` columns; a new scene file-export is one `SceneTarget` row plus one exporter arm in `scene/export#EXPORT` or `scene/stage#STAGE`; a new USD-deliverable metadata field is one `RenderSpec` band field the `scene/export#EXPORT` `authored` reads; a new render-evidence fact is one key on the `core/receipt#RECEIPT` `ArtifactReceipt.Scene` `facts` `frozendict` band, never a new receipt case nor a widened positional tuple; a new importable scene format is one `SceneSource` member plus one `import_*` arm in the `render_ingest` worker; zero new surface — the modality space stays five cases (`Image`/`Export`/`Frames`/`Ingest`/`Compose`) on one owner, every addition a row, field, case, member, or arm.

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

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

lazy from artifacts.scene.render_worker import render_compose, render_export, render_frames, render_image, render_ingest

type Vec3 = tuple[float, float, float]
type Plane = tuple[Vec3, Vec3]
type Bounds = tuple[float, float, float, float, float, float]
type ScalarRange = tuple[float, float]
type Palette = (
    str | tuple[str, ...]
)  # a matplotlib colormap NAME or the graphic/color/derive#DERIVE resolved CSS-colour LIST; add_mesh(cmap=) accepts both
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
    "smooth",
    "subdivide",
    "fill_holes",
    "clean",
    "cell_to_point",
    "point_to_cell",
    "combine",
]
type SceneOpTag = Literal["image", "export", "frames", "ingest", "compose"]

_FRAME_FORMAT = "rgb24"
_ORIGIN: Vec3 = (0.0, 0.0, 0.0)
_UP: Vec3 = (0.0, 0.0, 1.0)

# bounds the off-loop render fan-out: every modality crosses the scene `to_process` lane under
# this one limiter, so concurrent offscreen-GL render subprocesses are capped at the boundary, never the per-loop default.
_SCENE_LIMITER: Final[CapacityLimiter] = CapacityLimiter(os.process_cpu_count() or 1)


class RenderStyle(StrEnum):
    SURFACE = "surface"
    VOLUME = "volume"
    POINTS = "points"
    ARROWS = "arrows"  # add_arrows vector-field glyphs over the active vector field
    LABELS = "labels"  # add_point_labels text annotation at each point
    LINES = "lines"  # add_mesh(style="wireframe") drawing-edge linework for the AEC drawing plane


class SceneTarget(StrEnum):
    # the closed file-export vocabulary this owner's SceneOp.Export/Ingest payloads key — hoisted HERE so
    # scene/export#EXPORT composes it downward and the eager scene module graph runs export -> render only.
    PNG = "png"
    GLTF = "gltf"
    VRML = "vrml"
    OBJ = "obj"
    HTML = "html"
    USD = "usdc"
    USDZ = "usdz"


class SceneSource(StrEnum):
    # the closed set of scene formats the pyvista importer round-trips (import_gltf/import_obj/import_vrml);
    # a subset of SceneTarget — PNG/USD/USDZ are export-only, never an import inverse.
    GLTF = "gltf"
    OBJ = "obj"
    VRML = "vrml"


class BoolOp(StrEnum):
    # the two-operand composite ops the single-operand FieldFilter cannot carry; dispatched by the worker `_BOOL` closure table (SAMPLE the field-transfer)
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
    clip: tuple[Plane, bool] = case()  # (plane, crinkle) — crinkle keeps whole boundary cells rather than a clean cut
    clip_box: Bounds = case()
    clip_scalar: tuple[float, bool] = case()
    slice: Plane = case()
    slice_orthogonal: Vec3 = case()
    threshold: ScalarRange = case()
    contour: tuple[tuple[float, ...], str] = case()  # (isovalues, method='contour'/'marching_cubes'/'flying_edges')
    warp: float = case()
    glyph: tuple[float, bool, float | None, bool] = case()  # (factor, orient, tolerance, clamping)
    streamlines: tuple[Vec3, float | None, int, str, float | None] = (
        case()
    )  # (source_center, source_radius, n_points, integration_direction, max_time)
    decimate: tuple[float, bool] = case()  # (reduction, volume_preservation)
    surface: bool = case()
    warp_vector: float = case()  # warp_by_vector displacement over the active vector field
    smooth: int = case()  # n_iter Laplacian surface smoothing (mesh repair)
    subdivide: int = case()  # nsub recursive triangle subdivision (mesh refinement)
    fill_holes: float = case()  # hole_size boundary-hole fill (mesh repair)
    clean: bool = case()  # merge duplicate/degenerate points (mesh repair)
    cell_to_point: bool = case()  # cell_data_to_point_data field transfer
    point_to_cell: bool = case()  # point_data_to_cell_data field transfer
    combine: bool = case()  # MultiBlock.combine merge (merge_points), e.g. after slice_orthogonal

    @staticmethod
    def Clip(normal: Vec3, origin: Vec3, crinkle: bool = False) -> "FieldFilter":
        return FieldFilter(clip=((normal, origin), crinkle))

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
    def Contour(*isovalues: float, method: str = "contour") -> "FieldFilter":
        return FieldFilter(contour=(isovalues, method))

    @staticmethod
    def Warp(factor: float) -> "FieldFilter":
        return FieldFilter(warp=factor)

    @staticmethod
    def Glyph(factor: float, orient: bool = True, tolerance: float | None = None, clamping: bool = False) -> "FieldFilter":
        return FieldFilter(glyph=(factor, orient, tolerance, clamping))

    @staticmethod
    def Streamlines(
        source_center: Vec3,
        source_radius: float | None = None,
        n_points: int = 100,
        integration_direction: str = "both",
        max_time: float | None = None,
    ) -> "FieldFilter":
        return FieldFilter(streamlines=(source_center, source_radius, n_points, integration_direction, max_time))

    @staticmethod
    def Decimate(reduction: float, volume_preservation: bool = False) -> "FieldFilter":
        return FieldFilter(decimate=(reduction, volume_preservation))

    @staticmethod
    def Surface() -> "FieldFilter":
        return FieldFilter(surface=True)

    @staticmethod
    def WarpVector(factor: float = 1.0) -> "FieldFilter":
        return FieldFilter(warp_vector=factor)

    @staticmethod
    def Smooth(n_iter: int = 20) -> "FieldFilter":
        return FieldFilter(smooth=n_iter)

    @staticmethod
    def Subdivide(nsub: int = 1) -> "FieldFilter":
        return FieldFilter(subdivide=nsub)

    @staticmethod
    def FillHoles(hole_size: float) -> "FieldFilter":
        return FieldFilter(fill_holes=hole_size)

    @staticmethod
    def Clean() -> "FieldFilter":
        return FieldFilter(clean=True)

    @staticmethod
    def CellToPoint() -> "FieldFilter":
        return FieldFilter(cell_to_point=True)

    @staticmethod
    def PointToCell() -> "FieldFilter":
        return FieldFilter(point_to_cell=True)

    @staticmethod
    def Combine(merge_points: bool = False) -> "FieldFilter":
        return FieldFilter(combine=merge_points)

    def apply(self, dataset: object, scalars: str) -> object:
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
                return dataset.decimate(target_reduction=reduction, volume_preservation=volume_preservation)
            case FieldFilter(tag="surface", surface=_):
                return dataset.extract_surface()
            case FieldFilter(tag="warp_vector", warp_vector=factor):
                return dataset.warp_by_vector(vectors=scalars, factor=factor)
            case FieldFilter(tag="smooth", smooth=n_iter):
                return dataset.smooth(n_iter=n_iter)
            case FieldFilter(tag="subdivide", subdivide=nsub):
                return dataset.subdivide(nsub)
            case FieldFilter(tag="fill_holes", fill_holes=hole_size):
                return dataset.fill_holes(hole_size)
            case FieldFilter(tag="clean", clean=_):
                return dataset.clean()
            case FieldFilter(tag="cell_to_point", cell_to_point=_):
                return dataset.cell_data_to_point_data()
            case FieldFilter(tag="point_to_cell", point_to_cell=_):
                return dataset.point_data_to_cell_data()
            case FieldFilter(tag="combine", combine=merge_points):
                return dataset.combine(merge_points=merge_points)
            case _:
                assert_never(self)


class RenderSpec(Struct, frozen=True):
    scalars: str
    colormap: Palette
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
    line_width: float | None = None  # RenderStyle.LINES wireframe pen width (AEC drawing-edge linework)
    ambient: float | None = None  # add_mesh classic-lighting Phong band, orthogonal to the pbr metallic/roughness pair
    diffuse: float | None = None
    specular: float | None = None
    smooth_shading: bool = False  # per-vertex Gouraud shading (vs the default per-face flat)
    volume_opacity: str | None = None  # add_volume opacity transfer-function name ('linear'/'sigmoid'/'sigmoid_6'), overriding the scalar float
    blending: str | None = None  # add_volume ray blending ('composite'/'maximum'/'minimum'/'average'/'additive')
    volume_mapper: str | None = None  # add_volume mapper ('smart'/'gpu'/'fixed_point'/'ugrid')
    features: frozenset[RenderFeature] = frozenset()
    anti_aliasing: AntiAlias | None = None
    camera: CameraPose = CameraPose()
    title: str | None = None  # add_text figure title at the upper edge
    annotations: tuple[tuple[str, str], ...] = ()  # add_text (text, position) callouts for the documentation-figure plane
    up_axis: Literal["y", "z"] = "z"  # AEC/USD deliverable metadata: the scene/stage#STAGE UpAxis the USD export threads
    meters_per_unit: float = 1.0  # AEC/USD deliverable metadata: the real-world scale the USD/AR export anchors on
    labels: frozendict[str, tuple[str, ...]] = (
        frozendict()
    )  # AEC/USD deliverable metadata: taxonomy -> discipline/classification labels the USD MeshScene.labels carries

    def staged(self, dataset: object) -> object:
        return reduce(lambda field, flt: flt.apply(field, self.scalars), self.filters, dataset)

    def added(self, plotter: object, mesh: object) -> None:
        shared = {"scalars": self.scalars, "cmap": self.colormap, "clim": self.clim, "opacity": self.opacity}
        drop = lambda row: {
            key: value for key, value in row.items() if value is not None
        }  # every provider kwarg dropped when unset, never a None passed through
        match self.style:
            case RenderStyle.SURFACE:
                material = {
                    "pbr": self.pbr or None,
                    "metallic": self.metallic,
                    "roughness": self.roughness,
                    "show_edges": self.show_edges or None,
                    "ambient": self.ambient,
                    "diffuse": self.diffuse,
                    "specular": self.specular,
                    "smooth_shading": self.smooth_shading or None,
                }
                plotter.add_mesh(mesh, **drop(shared | material))
            case RenderStyle.VOLUME:
                volume = {**shared, "opacity": self.volume_opacity or self.opacity, "blending": self.blending, "mapper": self.volume_mapper}
                plotter.add_volume(mesh, **drop(volume))  # opacity is the transfer-function NAME when set, else the scalar float
            case RenderStyle.POINTS:
                plotter.add_points(mesh, **drop(shared))
            case RenderStyle.LINES:
                plotter.add_mesh(mesh, style="wireframe", **drop(shared | {"line_width": self.line_width}))  # drawing-edge linework
            case RenderStyle.ARROWS:
                plotter.add_arrows(mesh.points, mesh[self.scalars])  # centers = points, directions = the active vector field
            case RenderStyle.LABELS:
                plotter.add_point_labels(mesh.points, mesh[self.scalars])  # text labels rendered from the active scalar field
            case _:
                assert_never(self.style)

    def viewed(self, plotter: object) -> None:
        plotter.set_background(self.background)
        for feature in self.features:
            _FEATURE[feature](plotter)
        if self.title is not None:
            plotter.add_text(self.title, position="upper_edge")
        for text, position in self.annotations:
            plotter.add_text(text, position=position)
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
    ingest: tuple[bytes, SceneSource, SceneTarget, RenderSpec] = case()
    compose: tuple[object, object, BoolOp, RenderSpec] = case()

    @staticmethod
    def Image(grid: object, spec: RenderSpec) -> "SceneOp":
        return SceneOp(image=(grid, spec))

    @staticmethod
    def Export(grid: object, target: SceneTarget, spec: RenderSpec) -> "SceneOp":
        return SceneOp(export=(grid, target, spec))

    @staticmethod
    def Frames(grid: object, orbit: OrbitPath, steps: int, spec: RenderSpec) -> "SceneOp":
        return SceneOp(frames=(grid, orbit, steps, spec))

    @staticmethod
    def Ingest(scene: bytes, source: SceneSource, target: SceneTarget, spec: RenderSpec) -> "SceneOp":
        return SceneOp(ingest=(scene, source, target, spec))

    @staticmethod
    def Compose(grid_a: object, grid_b: object, op: BoolOp, spec: RenderSpec) -> "SceneOp":
        return SceneOp(compose=(grid_a, grid_b, op, spec))


class Scene3d(Struct, frozen=True):
    op: SceneOp

    async def render(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"scene.{self.op.tag}", self._emit)

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        match self.op:
            case SceneOp(tag="image", image=(grid, spec)):
                data = await to_process.run_sync(render_image, grid, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.key(
                    SceneTarget.PNG.value, data
                )  # bare synchronous accessor: the rendered PNG bytes are an infallible whole-byte source, so `_emit` mints off a bare `ContentKey`, never the railed `of`
                return key, ArtifactReceipt.Scene(
                    key, SceneTarget.PNG.value, len(data), frozendict({"width": spec.window[0], "height": spec.window[1]})
                )
            case SceneOp(tag="export", export=(grid, target, spec)):
                data, facts = await to_process.run_sync(render_export, grid, target.value, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.key(target.value, data)
                return (
                    key,
                    ArtifactReceipt.Scene(key, target.value, len(data), facts),
                )  # `facts` carries the `scene/stage#STAGE` `ComputeUsdStageStats` prim/layer/up-axis/meters-per-unit/extent band on the USD arms, empty on the render-sink arms
            case SceneOp(tag="frames", frames=(grid, orbit, steps, spec)):
                sequence = await to_process.run_sync(render_frames, grid, orbit, steps, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.key(
                    _FRAME_FORMAT, tuple(ContentIdentity.key(_FRAME_FORMAT, frame.tobytes()) for frame in sequence)
                )  # merkle over the per-frame bare keys: each infallible whole-byte frame keys through the synchronous `key`, then the parent joins them, never the railed `of`
                return key, ArtifactReceipt.Scene(
                    key,
                    _FRAME_FORMAT,
                    sum(frame.nbytes for frame in sequence),
                    frozendict({"frames": len(sequence), "width": spec.window[0], "height": spec.window[1]}),
                )
            case SceneOp(tag="ingest", ingest=(scene, source, target, spec)):
                data, facts = await to_process.run_sync(render_ingest, scene, source.value, target.value, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.key(target.value, data)
                return key, ArtifactReceipt.Scene(key, target.value, len(data), facts)
            case SceneOp(tag="compose", compose=(grid_a, grid_b, op, spec)):
                data = await to_process.run_sync(render_compose, grid_a, grid_b, op.value, spec, limiter=_SCENE_LIMITER)
                key = ContentIdentity.key(SceneTarget.PNG.value, data)
                return key, ArtifactReceipt.Scene(
                    key, SceneTarget.PNG.value, len(data), frozendict({"boolean": op.value, "width": spec.window[0], "height": spec.window[1]})
                )
            case _:
                assert_never(self.op)
```

The worker bodies (`render_plotter`/`import_plotter`/`surface_arrays`/`shoot`/`png`/`render_image`/`_orbit`/`render_frames`/`render_compose`/`render_export`/`render_ingest`) are module-level functions dispatched by qualified name across the `anyio.to_process.run_sync` subprocess seam (the seam cannot target a bound method or closure) under the shared `_SCENE_LIMITER` `CapacityLimiter` that bounds render fan-out at the boundary, so they import `pyvista`/`numpy`/`matplotlib` at module scope inside the gated sub-3.13 worker only, never on the runtime owner; `render_export`/`render_ingest` are the `Export`/`Ingest` SEAM ENTRIES: they re-admit the crossed `SceneTarget`, read the `scene/export#EXPORT` `ROW` law, and compose its `plotted`/`authored`/`captured` machinery — the worker boundary is the sole composer of the export module, the runtime render module imports nothing from it, and the eager module graph (worker → export → render) is acyclic; `import_plotter` is the `scene/export#EXPORT` `render_ingest` round-trip's plotter, building an offscreen plotter from an imported glTF/OBJ/VRML scene file (`import_gltf`/`import_obj`/`import_vrml`) that the export re-tunes and re-serializes; `surface_arrays` is the pyvista mesh-extraction seam the `scene/export#EXPORT` USD/USDZ arm hands to `scene/stage#STAGE` (`usd-core`) to author a USD layer from the dataset's triangulated surface (`.points`/`.regular_faces`/`.point_normals`) PLUS the scalar-mapped per-vertex RGB (`_mapped_rgb` through `matplotlib.colormaps`/`ListedColormap`/`Normalize`) so the USD `displayColor` primvar carries the render's coloring, with no render pass; `render_compose` extracts both operand surfaces, folds the `BoolOp` `boolean_*`/`sample`, and renders the composite; each render arm brackets its `Plotter` in a `try`/`finally` `plotter.close()` so the native VTK render window closes deterministically and the GL context is never left for GC. `render_plotter` admits the raw mesh ONCE through `pyvista.wrap` (the catalogued zero-copy entry over a numpy buffer / VTK object / `trimesh`/`meshio` mesh), applies `RenderSpec.staged`, binds through `RenderSpec.added`, folds `RenderSpec.viewed`, and is the shared render window the `Image`/`Frames`/`Compose` sinks and the `scene/export#EXPORT`/`scene/stage#STAGE` arms all read; `_orbit` computes the azimuthal `camera_position` walk from the auto-framed viewpoint through `numpy` `linspace`/`cos`/`sin`/`linalg.norm`, and `render_frames` walks it placing the camera per step through the catalogued `camera_position` property.

```python signature
from collections.abc import Callable
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import assert_never

import numpy as np
import pyvista as pv
from builtins import frozendict
from matplotlib import colormaps
from matplotlib.colors import ListedColormap, Normalize
from msgspec.structs import replace
from numpy.typing import NDArray

from artifacts.scene.export import RENDER_SINKS, ROW, USD_SINKS, ExportError, Sink, authored, captured, plotted
from artifacts.scene.render import BoolOp, FieldFilter, OrbitPath, RenderSpec, SceneTarget, Vec3

# the BoolOp two-operand PolyData ops as one closure table (mirroring the scene/export#EXPORT `_EXPORTER` row), never a string-built getattr
_BOOL: frozendict[BoolOp, Callable[[object, object], object]] = frozendict({
    BoolOp.UNION: lambda a, b: a.boolean_union(b),
    BoolOp.DIFFERENCE: lambda a, b: a.boolean_difference(b),
    BoolOp.INTERSECTION: lambda a, b: a.boolean_intersection(b),
    BoolOp.SAMPLE: lambda a, b: a.sample(b),
})


def render_plotter(grid: object, spec: RenderSpec) -> "pv.Plotter":
    plotter = pv.Plotter(off_screen=True)
    spec.added(plotter, spec.staged(pv.wrap(grid)))
    spec.viewed(plotter)
    return plotter


def import_plotter(scene: bytes, source: str, spec: RenderSpec) -> "pv.Plotter":
    # the `scene/export#EXPORT` `render_ingest` round-trip's plotter: import an existing glTF/OBJ/VRML scene file into
    # an offscreen plotter, then re-tune it through `RenderSpec.viewed` (the imported scene's camera overridden only
    # when the spec carries a `CameraPose`). The import loads the whole scene into the render window, so the temp file
    # is free to drop before `viewed` runs; `source` is the `SceneSource` token crossed as its `.value`.
    plotter = pv.Plotter(off_screen=True)
    with TemporaryDirectory() as work:
        path = Path(work) / f"scene.{source}"
        path.write_bytes(scene)
        match source:
            case "gltf":
                plotter.import_gltf(str(path))
            case "obj":
                plotter.import_obj(str(path))
            case _:  # "vrml"
                plotter.import_vrml(str(path))
    spec.viewed(plotter)
    return plotter


def surface_arrays(grid: object, spec: RenderSpec) -> tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32], NDArray[np.float32] | None]:
    surf = pv.wrap(grid).extract_surface().triangulate()  # admit ONCE, reduce to a uniform-triangle boundary so regular_faces is (M,3)
    return (
        np.asarray(surf.points, dtype=np.float32),
        surf.regular_faces.astype(np.int32),
        np.asarray(surf.point_normals, dtype=np.float32),
        _mapped_rgb(
            surf, spec
        ),  # the scalar->colormap per-vertex RGB the `scene/stage#STAGE` displayColor primvar carries; None when the field is absent
    )


def _mapped_rgb(surf: object, spec: RenderSpec) -> NDArray[np.float32] | None:
    # map the active scalar field through the render colormap (a matplotlib NAME or the graphic/color/derive#DERIVE palette LIST) to per-vertex RGB
    if spec.scalars not in surf.point_data:
        return None
    scalars = np.asarray(surf.point_data[spec.scalars], dtype=np.float64)
    lo, hi = spec.clim if spec.clim is not None else (float(scalars.min()), float(scalars.max()))
    cmap = ListedColormap(list(spec.colormap)) if isinstance(spec.colormap, tuple) else colormaps[spec.colormap]
    rgba = cmap(Normalize(vmin=lo, vmax=hi)(scalars))
    return np.asarray(rgba[:, :3], dtype=np.float32)


def shoot(plotter: "pv.Plotter", spec: RenderSpec) -> NDArray[np.uint8]:
    return plotter.screenshot(filename=None, return_img=True, window_size=list(spec.window), transparent_background=spec.transparent)


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


def render_compose(grid_a: object, grid_b: object, op: str, spec: RenderSpec) -> bytes:
    # the two-operand composite: extract both watertight surfaces, fold the BoolOp CSG/field-sample, render the result.
    # `boolean_*`/`sample` live on PolyData, so both operands reduce to triangulated surfaces first (the second-operand
    # `FieldFilter.apply` cannot construct); `render_plotter` re-wraps the already-wrapped composite (a no-op) and renders it.
    a = pv.wrap(grid_a).extract_surface().triangulate()
    b = pv.wrap(grid_b).extract_surface().triangulate()
    composite = _BOOL[BoolOp(op)](a, b)  # the BoolOp closure table over boolean_union/difference/intersection/sample, never a string-built getattr
    plotter = render_plotter(composite, spec)
    try:
        with TemporaryDirectory() as work:
            return png(plotter, Path(work) / "scene.png", spec)
    finally:
        plotter.close()


def render_export(grid: object, target: str, spec: RenderSpec) -> tuple[bytes, frozendict[str, float | str]]:
    # the SceneOp.Export seam entry: re-admit the crossed target once, read the scene/export#EXPORT ROW law, split
    # the Sink match between the render sinks (live offscreen plotter) and the USD sinks (plotter-free surface author).
    kind = SceneTarget(target)
    row = ROW[kind]
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{row.suffix}"
        facts: frozendict[str, float | str] = frozendict()  # USD sinks fill it from `authored`'s ComputeUsdStageStats band; render sinks leave it empty
        try:
            match row.sink:
                case Sink.RASTER | Sink.PLOTTER:
                    surfaced = replace(spec, filters=(*spec.filters, FieldFilter.Surface())) if row.surface else spec
                    plotted(render_plotter(grid, surfaced), kind, row, spec, out)
                case Sink.USD_LAYER | Sink.USDZ_PACKAGE:
                    facts = authored(surface_arrays(grid, spec), row, spec, out)
                case _:
                    assert_never(row.sink)
        except ExportError:
            raise
        except Exception as failed:  # noqa: BLE001 — every pyvista/VTK/pxr provider raise converges on the closed cause here
            raise ExportError("<usd-failed>" if row.sink in USD_SINKS else "<write-failed>") from failed
        return captured(Path(work), out, row.bundle), facts


def render_ingest(scene: bytes, source: str, target: str, spec: RenderSpec) -> tuple[bytes, frozendict[str, float | str]]:
    # the SceneOp.Ingest round-trip seam entry: import an existing glTF/OBJ/VRML scene, re-tune through
    # RenderSpec.viewed, re-serialize to a render-sink target; a USD target rails <write-failed> (no grid to extract).
    kind = SceneTarget(target)
    row = ROW[kind]
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{row.suffix}"
        try:
            if row.sink not in RENDER_SINKS:
                raise ExportError("<write-failed>")
            plotted(import_plotter(scene, source, spec), kind, row, spec, out)
        except ExportError:
            raise
        except Exception as failed:  # noqa: BLE001
            raise ExportError("<write-failed>") from failed
        return captured(Path(work), out, row.bundle), frozendict()  # an imported scene carries no authored-stage stats


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
