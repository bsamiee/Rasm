# [PY_ARTIFACTS_SCENE_RENDER_WORKER]

`render_image`, `render_export`, `render_frames`, `render_ingest`, and `render_compose` are the worker-floor kernel bodies the runtime `execution/workers#FABRIC` `shipped` gate resolves by qualified name against the spec floor's `WORKER_MODULE`. Crossing, shipping classification, deadline enforcement, idempotency-gated worker-death retry, and the tblib fidelity latch all arrive settled from that owner, so this module composes one runtime symbol — the `covered` roster witness — and spends every line on render capability. Native providers import eagerly at module scope because this module loads only on the worker floor, and `_hydrate` constructs each provider dataset from the admitted `SceneGrid` without revalidation.

Worker imports sit at the top of the acyclic scene graph: this module composes `scene/export#EXPORT`'s `plotted`, `authored`, and `captured` beside the spec floor, export alone reaches `scene/stage#STAGE`, the `pxr`-only stage leaf imports no scene sibling, and the runtime-only `rasm.artifacts.scene.render` is never imported here. Each offscreen `Plotter(off_screen=True)` is one render capsule retained only for its kernel invocation and closed on every exit path — `plotted` owns the close on the export folds, `_snapped` and the kernels own it everywhere else.

## [01]-[INDEX]

- [02]-[WORKER]: the shipped kernel bodies over the eager native providers — dataset hydration, scalar-band normalization, capture custody, orbit sweep, boolean CSG, and the per-target export folds the export law supplies.

## [02]-[WORKER]

- Owner: `_hydrate` trusts `SceneGrid` evidence — `of_mesh` buffers land through `pv.PolyData.from_regular_faces`, a volume field lands as `pv.ImageData`, a rectilinear or curvilinear field lands as `pv.RectilinearGrid`/`pv.StructuredGrid` — each scalar band raveled Fortran-order — GLB bytes round-trip through one temporary file into `pv.read` with a `MultiBlock` collapsed by `combine()`, and a `source` primitive is one `_SOURCE` factory row applied to the case's params. `_scene` builds the render capsule — hydrate, stage, add, light, view — and closes it on any raise so a failed body never leaks a render window.
- Cases: `render_compose` surface-extracts and triangulates both grids, refuses a non-manifold boolean operand through `is_manifold` and an empty composition before any render, because the boolean family is watertight-`PolyData`-only — and a watertight fold can still spin unboundedly on coincident surfaces, so compose and the orbit-looping frames body are the two `Enforcement.TERMINAL` arms `scene/render#SCENE` declares while every single-window kernel stays `COOPERATIVE` under the pool reaper; `render_ingest` re-admits the crossed scene through `_IMPORTER` from one temporary file; `render_export` threads the staged dataset's `points`/`cells` counts and the USD stats band back beside the captured bytes.
- Entry: the kernels are the roster the runtime `shipped` gate resolves, and this module's import-time `covered(WORKER_MODULE, _KERNELS)` witness proves every roster name resolves at worker import — a railed miss collapses to a module-level raise, so a misspelled roster refuses to import rather than faulting mid-offload; each kernel re-admits its crossed `.value` token through `EnumType(value)` so an unknown token faults at the seam, and a kernel raise crosses back with worker frames intact as the offload boundary fault `scene/render#SCENE` flattens.
- Auto: `_normed` pre-maps the `ColorNorm.premapped` scalar bands because no plotter arm accepts a matplotlib norm, while `log` rides `add_mesh(log_scale=)` natively; `_orbit` co-iterates admitted `OrbitPath` intervals through strict `zip` over `numpy.linspace` samples, so a turntable, dolly, or helix is one generator whose cyclic sweep drops the duplicate terminal frame while an open arc reaches its endpoint; `surface_arrays` re-associates a cell band through `cell_data_to_point_data`, guards the USD `int32` index range, and hands `np.ascontiguousarray` buffers across the `Vt.*Array.FromNumpy` and pickle seams, where a sliced view is otherwise handed non-contiguous; every capture rides `screenshot(scale=)`, so a publication raster supersamples to `window`-times-`scale` and the receipt reports the rasterized dims.
- Output: every kernel returns owned material — PNG bytes, an rgb24 frame tuple, or a bytes-plus-facts pair — built from crossed arguments and `TemporaryDirectory`-scoped writes alone; `idempotent=True` at `scene/render#SCENE` is therefore a truthful declaration, and a worker-death retry re-runs any body safely.
- Growth: a new two-operand op is one `BoolOp` member plus one `_BOOL` row; a new importable format is one `SceneSource` member plus one `_IMPORTER` row; a new parametric primitive is one `SourceKind` member plus one `_SOURCE` row — an import-time coverage gate proves the three tables span their vocabularies, so an unruled member fails at worker import; a new kernel is one function plus its `_KERNELS` roster row and one offload arm at `scene/render#SCENE`.
- Boundary: named provider faults, a failed `PackageOp`, an unsupported round-trip target, an empty output, and `ZipError` converge on `ExportError` inside the export folds; an unexpected exception remains a defect and propagates.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import assert_never

import numpy as np
import pyvista as pv
from builtins import frozendict
from matplotlib import colormaps
from matplotlib.colors import ListedColormap, LogNorm, Normalize, SymLogNorm, TwoSlopeNorm
from msgspec.structs import replace
from numpy.typing import NDArray
from pxr import Tf

from rasm.runtime.workers import covered

from rasm.artifacts.scene.export import ROW, ExportError, ExportRow, authored, captured, plotted
from rasm.artifacts.scene.spec import (
    BoolOp,
    ColorNorm,
    FieldAssociation,
    OrbitPath,
    RenderSpec,
    SceneGrid,
    SceneLight,
    SceneSource,
    SceneTarget,
    SourceKind,
    Vec3,
    WORKER_MODULE,
)

# --- [CONSTANTS] -----------------------------------------------------------------------

_FIELD = "field"

# dispatchable roster the import-time witness proves; a kernel missing here never ships.
_KERNELS = ("render_image", "render_export", "render_frames", "render_ingest", "render_compose")

# --- [TABLES] --------------------------------------------------------------------------

_BOOL: frozendict[BoolOp, Callable[[pv.PolyData, pv.PolyData], pv.DataSet]] = frozendict({
    BoolOp.UNION: lambda a, b: a.boolean_union(b),
    BoolOp.DIFFERENCE: lambda a, b: a.boolean_difference(b),
    BoolOp.INTERSECTION: lambda a, b: a.boolean_intersection(b),
    BoolOp.SAMPLE: lambda a, b: a.sample(b),
})

_IMPORTER: frozendict[SceneSource, Callable[[pv.Plotter, str], None]] = frozendict({
    SceneSource.GLTF: lambda plotter, path: plotter.import_gltf(path),
    SceneSource.GLB: lambda plotter, path: plotter.import_gltf(path),
    SceneSource.OBJ: lambda plotter, path: plotter.import_obj(path),
    SceneSource.VRML: lambda plotter, path: plotter.import_vrml(path),
})

_SOURCE: frozendict[SourceKind, Callable[..., pv.PolyData]] = frozendict({
    SourceKind.SPHERE: pv.Sphere,
    SourceKind.ICOSPHERE: pv.Icosphere,
    SourceKind.CUBE: pv.Cube,
    SourceKind.BOX: pv.Box,
    SourceKind.CYLINDER: pv.Cylinder,
    SourceKind.CONE: pv.Cone,
    SourceKind.TUBE: pv.Tube,
    SourceKind.PLANE: pv.Plane,
    SourceKind.DISC: pv.Disc,
    SourceKind.CIRCLE: pv.Circle,
    SourceKind.POLYGON: pv.Polygon,
    SourceKind.ARROW: pv.Arrow,
    SourceKind.LINE: pv.Line,
    SourceKind.TEXT: pv.Text3D,
})

if frozenset(_BOOL) != frozenset(BoolOp) or frozenset(_IMPORTER) != frozenset(SceneSource) or frozenset(_SOURCE) != frozenset(SourceKind):
    raise RuntimeError("worker tables do not cover their vocabularies")

# --- [OPERATIONS] ----------------------------------------------------------------------


def _hydrate(grid: SceneGrid, scalars: str | None) -> pv.DataSet:
    match grid:
        case SceneGrid(tag="mesh", mesh=(points, faces, fields)):
            mesh = pv.PolyData(points) if faces is None else pv.PolyData.from_regular_faces(points, faces)
            for name, field in fields.items():
                match field.association:
                    case FieldAssociation.POINT:
                        mesh.point_data[name] = field.values
                    case FieldAssociation.CELL:
                        mesh.cell_data[name] = field.values
                    case _ as unreachable:
                        assert_never(unreachable)
            return mesh
        case SceneGrid(tag="volume", volume=(field, spacing, origin)):
            image = pv.ImageData(dimensions=field.shape, spacing=spacing, origin=origin)
            image.point_data[scalars or _FIELD] = field.ravel(order="F")
            return image
        case SceneGrid(tag="rect", rect=(field, x, y, z)):
            rectilinear = pv.RectilinearGrid(x, y, z)
            rectilinear.point_data[scalars or _FIELD] = field.ravel(order="F")
            return rectilinear
        case SceneGrid(tag="struct", struct=(field, x, y, z)):
            curvilinear = pv.StructuredGrid(x, y, z)
            curvilinear.point_data[scalars or _FIELD] = field.ravel(order="F")
            return curvilinear
        case SceneGrid(tag="glb", glb=data):
            with TemporaryDirectory() as work:
                path = Path(work) / "mesh.glb"
                path.write_bytes(data)
                loaded = pv.read(path)
            return loaded.combine() if isinstance(loaded, pv.MultiBlock) else loaded
        case SceneGrid(tag="source", source=(kind, params)):
            return _SOURCE[kind](**params)  # param names are caller data; the factory refuses an unknown kwarg here
        case _:
            assert_never(grid)


def _sunlight(light: SceneLight) -> pv.Light:
    lamp = pv.Light(color=light.color, intensity=light.intensity, light_type="scene light")
    lamp.set_direction_angle(light.elevation, light.azimuth)
    return lamp


def _norm(norm: ColorNorm, lo: float, hi: float) -> Normalize:
    if not np.isfinite((lo, hi)).all() or hi < lo:
        raise ValueError("color limits must be finite and ordered")
    match norm:
        case ColorNorm(tag="linear"):
            return Normalize(vmin=lo, vmax=hi)
        case ColorNorm(tag="log"):
            if lo <= 0.0 or hi <= lo:
                raise ValueError("log normalization requires positive increasing limits")
            return LogNorm(vmin=lo, vmax=hi)
        case ColorNorm(tag="symlog", symlog=linthresh):
            if linthresh <= 0.0 or not np.isfinite(linthresh):
                raise ValueError("symlog normalization requires a positive finite linear threshold")
            return SymLogNorm(linthresh=linthresh, vmin=lo, vmax=hi)
        case ColorNorm(tag="diverging", diverging=vcenter):
            if not lo < vcenter < hi:
                raise ValueError("diverging normalization requires its center inside the limits")
            return TwoSlopeNorm(vcenter=vcenter, vmin=lo, vmax=hi)
        case _:
            assert_never(norm)


def _normed(dataset: pv.DataSet, spec: RenderSpec) -> tuple[pv.DataSet, RenderSpec]:
    if not spec.norm.premapped or spec.scalars is None:
        return dataset, spec
    values = dataset.point_data.get(spec.scalars, dataset.cell_data.get(spec.scalars))
    if values is None or np.asarray(values).ndim != 1:
        return dataset, spec
    field = np.asarray(values, dtype=np.float64)
    lo, hi = spec.clim if spec.clim is not None else (float(field.min()), float(field.max()))
    band = dataset.point_data if spec.scalars in dataset.point_data else dataset.cell_data
    band[spec.scalars] = np.asarray(_norm(spec.norm, lo, hi)(field), dtype=np.float64)
    return dataset, replace(spec, clim=(0.0, 1.0))


def _scene(dataset: pv.DataSet, spec: RenderSpec) -> pv.Plotter:
    staged, tuned = _normed(dataset, spec)
    plotter = pv.Plotter(off_screen=True, lighting=tuned.light_preset.value)
    try:
        tuned.added(plotter, staged)
        for light in tuned.lights:
            plotter.add_light(_sunlight(light))
        tuned.viewed(plotter)
        return plotter
    except BaseException:
        plotter.close()
        raise


def render_plotter(grid: SceneGrid, spec: RenderSpec) -> pv.Plotter:
    return _scene(spec.staged(_hydrate(grid, spec.scalars)), spec)


def import_plotter(scene: bytes, source: str, spec: RenderSpec) -> pv.Plotter:
    plotter = pv.Plotter(off_screen=True, lighting=spec.light_preset.value)
    try:
        kind = SceneSource(source)
        with TemporaryDirectory() as work:
            path = Path(work) / f"scene.{kind.value}"
            path.write_bytes(scene)
            _IMPORTER[kind](plotter, str(path))
        spec.viewed(plotter)
        return plotter
    except BaseException:
        plotter.close()
        raise


def surface_arrays(dataset: pv.DataSet, spec: RenderSpec) -> tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32], NDArray[np.float32] | None]:
    surf = dataset.extract_surface().triangulate()
    surf = surf.cell_data_to_point_data() if spec.scalars in surf.cell_data else surf
    faces = surf.regular_faces
    if faces.size and int(faces.max()) > np.iinfo(np.int32).max:
        raise ValueError("surface indices exceed the USD int32 range")
    return (
        np.ascontiguousarray(surf.points, dtype=np.float32),  # contiguity guard: FromNumpy and the pickle seam receive sliced views otherwise
        np.ascontiguousarray(faces, dtype=np.int32),
        np.ascontiguousarray(surf.point_normals, dtype=np.float32),
        _mapped_rgb(surf, spec),
    )


def _mapped_rgb(surf: pv.PolyData, spec: RenderSpec) -> NDArray[np.float32] | None:
    match None if spec.scalars is None else surf.point_data.get(spec.scalars):
        case None:
            return None
        case values:
            field = np.asarray(values, dtype=np.float64)
            scalars = np.linalg.norm(field, axis=1) if field.ndim == 2 else field
            if not scalars.size or not np.isfinite(scalars).all():
                raise ValueError("mapped scalar field must be nonempty and finite")
            lo, hi = spec.clim if spec.clim is not None else (float(scalars.min()), float(scalars.max()))
            cmap = ListedColormap(list(spec.colormap)) if isinstance(spec.colormap, tuple) else colormaps[spec.colormap]
            return np.asarray(cmap(_norm(spec.norm, lo, hi)(scalars))[:, :3], dtype=np.float32)


def shoot(plotter: pv.Plotter, spec: RenderSpec) -> NDArray[np.uint8]:
    # frames egress is rgb24 for the media encoder — no alpha crosses the media seam and no provider movie writer
    # exists, so this capture loop is the whole animation surface; scale supersamples each frame to window-times-scale.
    return plotter.screenshot(filename=None, return_img=True, window_size=list(spec.window), scale=spec.scale, transparent_background=False)


def _snapped(plotter: pv.Plotter, spec: RenderSpec) -> bytes:
    # raster capture custody for every PNG-returning kernel: the capsule closes on every exit, and the raster
    # round-trips through a worker-local TemporaryDirectory because the provider writes PNG to disk alone.
    try:
        with TemporaryDirectory() as work:
            out = Path(work) / "scene.png"
            plotter.screenshot(str(out), window_size=list(spec.window), scale=spec.scale, transparent_background=spec.transparent)
            return out.read_bytes()
    finally:
        plotter.close()


# --- [KERNELS]


def render_image(grid: SceneGrid, spec: RenderSpec) -> bytes:
    return _snapped(render_plotter(grid, spec), spec)


def render_compose(grid_a: SceneGrid, grid_b: SceneGrid, op: str, spec: RenderSpec) -> bytes:
    boolean = BoolOp(op)
    a = _hydrate(grid_a, spec.scalars).extract_surface().triangulate()
    b = _hydrate(grid_b, spec.scalars).extract_surface().triangulate()
    if boolean is not BoolOp.SAMPLE and not (a.is_manifold and b.is_manifold):
        raise ValueError("boolean CSG requires watertight manifold operands")
    composed = _BOOL[boolean](a, b)
    if composed.n_points == 0:
        raise ValueError("composition produced an empty dataset")  # a silent empty union renders a blank frame as if it succeeded
    return _snapped(_scene(spec.staged(composed), spec), spec)


def render_export(grid: SceneGrid, target: str, spec: RenderSpec) -> tuple[bytes, frozendict[str, float | str]]:
    # raw-token admission kernel: an unknown target's ValueError/KeyError converts HERE, so the crossing sees the
    # typed ExportError marker, never a bare provider raise the seam cannot classify.
    try:
        kind = SceneTarget(target)
        row = ROW[kind]
    except (ValueError, KeyError) as unknown:
        raise ExportError("<unsupported-target>") from unknown
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{kind.value}"
        prepared = row.prepared(spec)
        facts: frozendict[str, float | str] = frozendict()
        try:
            dataset = prepared.staged(_hydrate(grid, prepared.scalars))
            facts = frozendict({"points": float(dataset.n_points), "cells": float(dataset.n_cells)})
            match row:
                case ExportRow(tag="plotter"):
                    plotted(_scene(dataset, prepared), row, prepared, out)
                case ExportRow(tag="usd_layer") | ExportRow(tag="usdz_package"):
                    facts = frozendict({**facts, **authored(surface_arrays(dataset, prepared), row, prepared, out)})
                case _ as unreachable:
                    assert_never(unreachable)
        except ExportError:
            raise
        except (OSError, RuntimeError, ValueError, Tf.ErrorException) as failed:
            raise ExportError("<write-failed>" if row.tag == "plotter" else "<usd-failed>") from failed
        return captured(Path(work), out, row.capture), facts


def render_ingest(scene: bytes, source: str, target: str, spec: RenderSpec) -> tuple[bytes, frozendict[str, float | str]]:
    try:
        kind = SceneTarget(target)
        row = ROW[kind]
    except (ValueError, KeyError) as unknown:
        raise ExportError("<unsupported-target>") from unknown
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{kind.value}"
        try:
            match row:
                case ExportRow(tag="plotter"):
                    plotted(import_plotter(scene, source, spec), row, spec, out)
                case ExportRow(tag="usd_layer") | ExportRow(tag="usdz_package"):
                    raise ExportError("<unsupported-target>")
                case _ as unreachable:
                    assert_never(unreachable)
        except ExportError:
            raise
        except (OSError, RuntimeError, ValueError) as failed:
            raise ExportError("<write-failed>") from failed
        return captured(Path(work), out, row.capture), frozendict()


def _orbit(plotter: pv.Plotter, orbit: OrbitPath) -> tuple[tuple[Vec3, Vec3, Vec3], ...]:
    start, focal, up = plotter.camera_position
    center = np.asarray(focal, dtype=np.float64)
    distance = float(np.linalg.norm(np.asarray(start, dtype=np.float64) - center))
    # a cyclic sweep drops its duplicate terminal sample; an open arc, dolly, or helix reaches its endpoint
    cyclic = (orbit.azimuth[1] - orbit.azimuth[0]) % 360.0 == 0.0 and orbit.radius[0] == orbit.radius[1] and orbit.elevation[0] == orbit.elevation[1]
    azimuths = np.deg2rad(np.linspace(*orbit.azimuth, orbit.samples, endpoint=not cyclic))
    radii = distance * np.linspace(*orbit.radius, orbit.samples, endpoint=not cyclic)
    heights = float(center[2]) + distance * np.linspace(*orbit.elevation, orbit.samples, endpoint=not cyclic)
    focal3: Vec3 = (float(center[0]), float(center[1]), float(center[2]))
    up3: Vec3 = (float(up[0]), float(up[1]), float(up[2]))
    return tuple(
        ((float(center[0] + radius * np.cos(angle)), float(center[1] + radius * np.sin(angle)), float(height)), focal3, up3)
        for angle, radius, height in zip(azimuths, radii, heights, strict=True)
    )


def render_frames(grid: SceneGrid, orbit: OrbitPath, spec: RenderSpec) -> tuple[NDArray[np.uint8], ...]:
    plotter = render_plotter(grid, spec)
    try:

        def _capture(view: tuple[Vec3, Vec3, Vec3]) -> NDArray[np.uint8]:
            plotter.camera_position = list(view)
            return shoot(plotter, spec)

        return tuple(_capture(view) for view in _orbit(plotter, orbit))
    finally:
        plotter.close()


# --- [ENTRY] -----------------------------------------------------------------------------

# import-time coverage witness: every roster name resolves through the same walk the runtime `shipped` gate runs, and a
# railed miss collapses to a raise — a floor that cannot resolve its roster refuses to import, never faults mid-offload.
if (_COVERAGE := covered(WORKER_MODULE, _KERNELS)).is_error():
    raise RuntimeError(f"kernel roster does not resolve: {_COVERAGE.error}")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [WORKER_PROVIDERS_CP315]-[BLOCKED]: which `vtk` and `usd-core` releases publish cp315 wheels, admitting the `PROCESS` worker floor (the spawned 3.15 interpreter) to import both natives; verify the PyPI wheel tags per release, then drop the two `python_version<'3.15'` manifest gates.
