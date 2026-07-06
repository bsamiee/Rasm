# [PY_ARTIFACTS_SCENE_EXPORT]

The 3D scene-file export LAW owner. `SceneTarget` — the closed file-target vocabulary — is `scene/render#SCENE`'s (hoisted there; this page composes it downward, so the scene module graph runs export → render only and the render⇄export cycle is disposed). This page owns everything the target keys: one primary `ROW` `Map[SceneTarget, ExportRow]` policy table carrying each target's whole export law — the temp-file `suffix`, the `Sink` strategy that writes it, the `surface` pre-pass flag, the multi-file `bundle` flag, the format `options`, and the USDZ `profile` — the closed `Sink` write-strategy vocabulary (`RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE`), and the machinery each strategy runs: `plotted` writes over a live offscreen plotter (`RASTER` screenshots the offscreen PNG; `PLOTTER` reads the catalogued exporter from the `_EXPORTER` bound-closure row and projects the `options` policy to its keywords so the `GLTF`/`VRML`/`OBJ`/`HTML` exporters are one arm and not four), `authored` authors the triangulated surface — WITH its scalar-mapped per-vertex `display_color`, its render-band PBR `Material`, its AEC classification `labels`, and the `up_axis`/`meters_per_unit` AR metadata — directly through `scene/stage#STAGE`'s `author_mesh(MeshScene(...))`/`package_usdz`, NO render pass, NO offscreen plotter, and no phantom `vtkUSDExporter` (the standard `vtk` wheel ships no `vtkmodules.vtkIOUSD` module; USD authoring is `usd-core`'s alone), and `captured` reads the output back. The `scene/render#SCENE` worker-boundary seam entries `render_export`/`render_ingest` are the sole composers of this machinery: they re-admit the crossed target, read `ROW[kind]`, and split the `Sink` `match` between the render sinks and the USD sinks — this module imports only `scene/render#SCENE` (vocabulary) and `scene/stage#STAGE` (USD authoring), the two ledgered edges, and reaches NOTHING in the package plane. Scene emits FILES: a scene BUNDLE deliverable is `package/archive#ARCHIVE`'s emit whose `parents` are the scene-file content keys — a work-graph DATA edge, never an import — and this page carries no byte-parity contract with any package-plane ZIP path; the `OBJ` `.obj`+`.mtl` multi-file capture is seam mechanics only, one `stream_zip` container at a fixed epoch/level with `extended_timestamps=False` so the companion material file crosses the process seam whole (never dropped by a lone `read_bytes`) and the content key over the payload is stable. Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary collapsed to one `ExportError` at the process edge `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail`. `render_ingest` is the round-trip inverse the `scene/render#SCENE` `SceneOp.Ingest` case dispatches — an existing glTF/OBJ/VRML scene imported through `import_plotter`, re-tuned by `RenderSpec.viewed`, and re-serialized to any render-sink `SceneTarget` — an asset-conditioning capability (ingest a third-party 3D asset, re-render a preview or re-emit a normalized scene file) the catalogued `import_gltf`/`import_obj`/`import_vrml` inverse admits over the same `plotted` dispatch. This owner holds the `geometry/mesh` boundary: a scene file is a rendered-scene serialization (camera/lights/PBR-mapped surfaces), distinct from the raw mesh-interchange codec. This page closes the scene-file-export half of the `SCENE_FILE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[EXPORT]: the export LAW over `scene/render#SCENE`'s hoisted `SceneTarget` vocabulary — the primary `ROW` `Map[SceneTarget, ExportRow]` policy table carrying per-target `suffix`/`sink`/`surface`/`bundle`/`options`/`profile`, the closed `Sink` write-strategy vocabulary (`RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE`) the worker-boundary seam entries dispatch by one total `match`, the `PLOTTER` `_EXPORTER` target-keyed `Map` of bound exporter closures over the `GLTF`/`VRML`/`OBJ`/`HTML` exporters with the `options` `**kwargs` projection, the `GLTF` `FieldFilter.Surface` pre-pass row, the `OBJ` deterministic multi-file capture (fixed `_EPOCH`/`_ZIP_LEVEL`, `extended_timestamps=False` — seam mechanics for a stable content key, never a package-plane parity contract; a bundle DELIVERABLE is `package/archive#ARCHIVE`'s emit over scene-file parent keys, a work-graph DATA edge), the `USD`/`USDZ` plotter-free surface authoring through `scene/stage#STAGE` (threading the `scene/render#SCENE` mapped per-vertex colour, PBR `Material`, AEC `labels`, and AR metadata into `MeshScene`), the `render_ingest` round-trip inverse (`import_gltf`/`import_obj`/`import_vrml` re-tune-re-export over `plotted`), the closed `ExportFault` vocabulary collapsed to `ExportError` at the process edge, and the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam; `pyvista` `Plotter.export_gltf(inline_data=, save_normals=, rotate_scene=)`/`export_vrml`/`export_obj`/`export_html`/`import_gltf`/`import_obj`/`import_vrml` and `stream-zip` `stream_zip`/`ZIP_AUTO(size, level)`/`ZipError` settled against the both-tier `.api`, the `scene/stage#STAGE` `author_mesh(MeshScene(prim=PrimKind.Mesh(points, faces, normals), display_color=..., material=..., labels=...))` surface-author seam replacing the phantom render-window->`vtkUSDExporter` path (no `vtkmodules.vtkIOUSD` in the vtk wheel).

## [02]-[EXPORT]

- Owner: `SceneTarget` composed downward from `scene/render#SCENE` — render owns the file-target vocabulary its `SceneOp.Export`/`Ingest` payloads key; this page never re-mints it, and nothing here is a parallel per-format export owner. `ROW` the one primary `Final[Map[SceneTarget, ExportRow]]` policy table whose row carries each target's whole export law — `suffix`, `sink`, `surface`, `bundle`, `options`, `profile` — so a new target is one row and a new policy is one column, never an inline literal or a per-arm special case. `ExportRow` the frozen `dataclass` row owner (a compact value-equal config row, never a wire `msgspec.Struct`); `Sink` the closed `StrEnum` of write strategies — `RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE` — the strategy axis one vocabulary the dispatch matches, never four parallel exporter arms; `RENDER_SINKS`/`USD_SINKS` the two membership sets the seam entries split routing and fault-cause over. `plotted`/`authored`/`captured` the module's public machinery surface — the live-plotter write, the plotter-free USD surface author, and the output capture — composed ONLY by the `scene/render#SCENE` worker-boundary seam entries `render_export`/`render_ingest`; the runtime render module imports nothing from this page.
- Cases: the `Sink` `match` arms the seam entries split — the two render sinks route through `plotted` (a live offscreen plotter, closed deterministically in its own `try`/`finally`) and the two USD sinks through `authored` (plotter-free): `RASTER` (screenshots the offscreen raster straight to the temp sink through the catalogued `screenshot(window_size=, transparent_background=)` keywords, the same PNG artifact the `Image` arm produces) · `PLOTTER` (reads the catalogued exporter from the `_EXPORTER[kind]` bound-closure row and projects the row `options` to its keywords, so `GLTF` carries `inline_data`/`save_normals`/`rotate_scene` and `VRML`/`OBJ`/`HTML` carry the empty default — one arm over four exporters keyed by a vocabulary row, never four arms re-deriving the temp-file sink nor a string-built `getattr` lookup) · `USD_LAYER` (authors the triangulated surface `author_mesh(MeshScene(prim=PrimKind.Mesh(points, faces, normals), display_color=colors, material=_material(spec, ...), labels=spec.labels, up_axis=UpAxis(spec.up_axis), meters_per_unit=spec.meters_per_unit))` straight to the `.usdc` layer via `scene/stage#STAGE`, no render pass) · `USDZ_PACKAGE` (authors the `.usdc` layer then `package_usdz` packages the AR `.usdz`, the typed `profile` row column resolving the `UsdzProfile.ARKIT`/`STANDARD` packager `scene/stage#STAGE` selects, the `False` verdict railing `<usd-failed>`). The `GLTF` row's `surface` flag folds `FieldFilter.Surface()` into the `RenderSpec.filters` chain at the seam entry before render because the VTK glTF exporter handles only `PolyData`; the `OBJ` row's `bundle` flag captures the `.obj`+`.mtl` pair the exporter writes, never the lone `.obj` a flat `read_bytes` would drop. `authored` receives the worker-extracted `surface_arrays` buffers as an ARGUMENT — extracted once at the seam entry — so a USD export pays no offscreen-GL render, reaches no `vtkUSDExporter` the standard vtk wheel does not ship, and delivers a coloured, PBR-material, AEC-classified mesh rather than a bare grey one.
- Entry: NONE — this page is the export-law substrate; the seam entries live at the `scene/render#SCENE` worker boundary. `render_export(grid, target, spec)` re-admits the crossed `target` once through `SceneTarget(target)`, reads `ROW[kind]`, opens one `TemporaryDirectory` sink, and splits the `Sink` `match`: the render sinks surface the spec then hand `render_plotter(grid, surfaced)` to `plotted`; the USD sinks hand the worker-extracted `surface_arrays(grid, spec)` to `authored`. `render_ingest(scene, source, target, spec)` is the sibling round-trip — `import_plotter` imports an existing scene, `plotted` re-serializes it to a render-sink target (a USD target rails `<write-failed>`, an imported scene carrying no grid). `captured` then reads the output (single-file `read_bytes` or the deterministic multi-file container). Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary — `<write-failed>`/`<usd-failed>`/`<empty-output>`/`<bundle-failed>` — collapsed to one `ExportError` raised at the process edge, which `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail` `BoundaryFault`; no bare `RuntimeError`/`Tf.ErrorException`/`ZipError` crosses the seam unconverted. The returned `bytes` cross back to the `_emit` `Export`/`Ingest` case, keyed once through `ContentIdentity.key(target.value, data)` — the `SceneTarget` value the content-key format tag.
- Auto: the `surface` pre-pass folds into the existing `RenderSpec.staged` reduce at the seam entry — the glTF surface-extract is one more filter in the chain, never a branch — so a clip-then-glTF export runs the clip then the surface extract then the serialize through one filter fold; the `PLOTTER` exporter is the `_EXPORTER[kind]` bound-closure row so the four exporters are one arm with the `options` policy the only per-target variation; the `OBJ` capture is one `stream_zip` over the sorted temp-dir files at the fixed `_EPOCH`/`_ZIP_LEVEL` with `extended_timestamps=False`, so the `.obj`+`.mtl` payload is byte-stable and the `ContentIdentity.key` over it is reproducible — determinism for the content key, never a byte-parity contract with the package plane (the reproducible-ZIP PACKAGING convention is `package/archive#ARCHIVE`'s; a scene bundle deliverable is its emit over scene-file parent keys, a work-graph DATA edge); the `render_plotter` capsule the render sinks share with the image render carries the same scalar coloring, overlays, and camera; the `USD`/`USDZ` arms author from the worker-extracted surface buffers through `scene/stage#STAGE`'s wheel-available `author_mesh(MeshScene(...))` numpy path — the per-vertex `display_color` the render colormap mapped, the PBR `Material` (`diffuse_primvar="displayColor"` so the scalar colour survives under the finish rather than a flat diffuse overriding it), the `labels` discipline/classification taxonomy, and the AR `up_axis`/`meters_per_unit` — never a render window nor the source-build-gated `export_usd`/`vtkUSDExporter`.
- Receipt: each export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` keyed by the content key the `_emit` arm derives — the `SceneTarget` value the target string, the returned payload's `len` the byte evidence, and, for the USD sinks, the `scene/stage#STAGE` `ComputeUsdStageStats` prim/layer/up-axis/meters-per-unit/extent-diagonal band the seam entry's `(bytes, facts)` return threads back through `authored`; minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes`, this owner contributing the serialized scene-file payload and its stats band, never a parallel per-format receipt rail.
- Growth: a new scene file-export is one `SceneTarget` member (on `scene/render#SCENE`) plus one `ROW` entry here; a new write strategy is one `Sink` member plus one `match` arm (and one `RENDER_SINKS`/`USD_SINKS` membership); a deliverable variant (external-`.bin` glTF, ARKit `.usdz`, X3D) is one `ROW` entry carrying its own `options`/`bundle`/`sink`/`profile` with zero new arm (an ARKit `.usdz` is one `ROW` row at `profile=UsdzProfile.ARKIT`); a new multi-file format flips the row `bundle` flag; a new USD-deliverable metadata field is threaded once in `authored` from the `RenderSpec` band; a new export fault is one `ExportFault` member; a new importable round-trip source is one `scene/render#SCENE` `SceneSource` member plus one `import_*` arm in `import_plotter`; a scene BUNDLE deliverable is a `package/archive#ARCHIVE` emit whose `parents` are scene-file content keys — a work-graph DATA edge, never an edge from this page. The import/round-trip inverse is REALIZED — the `scene/render#SCENE` `SceneOp.Ingest` case dispatches `render_ingest`, which imports through `import_plotter` and re-serializes over the same `plotted` render-sink dispatch, so a re-tune-re-export reuses the exporter rows with zero new export surface. Zero new owner — the file-target axis stays render's one vocabulary, the export law one `ROW` table, the strategy one `Sink` vocabulary.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass, field
from datetime import UTC, datetime
from enum import StrEnum
from pathlib import Path
from typing import Final, Literal

import numpy as np
from expression.collections import Map
from numpy.typing import NDArray

from artifacts.scene.render import RenderSpec, SceneTarget
from artifacts.scene.stage import Material, MeshScene, PrimKind, UpAxis, UsdzProfile, author_mesh, package_usdz

lazy from stream_zip import ZIP_AUTO, ZipError, stream_zip

# --- [TYPES] ---------------------------------------------------------------------------

type ExportFault = Literal["<write-failed>", "<usd-failed>", "<empty-output>", "<bundle-failed>"]
type Options = frozendict[str, bool]
type Surface = tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32], NDArray[np.float32] | None]


class Sink(StrEnum):
    RASTER = "raster"
    PLOTTER = "plotter"
    USD_LAYER = "usd-layer"
    USDZ_PACKAGE = "usdz-package"


# --- [CONSTANTS] -----------------------------------------------------------------------

_EPOCH: datetime = datetime(1980, 1, 1, tzinfo=UTC)  # fixed zip member stamp -> byte-stable capture, stable content key
_ZIP_LEVEL: int = 9  # fixed deflate level: determinism for the content key over the multi-file capture, never a package-plane parity contract
RENDER_SINKS: frozenset[Sink] = frozenset({Sink.RASTER, Sink.PLOTTER})  # sinks that write over a live offscreen plotter
USD_SINKS: frozenset[Sink] = frozenset({Sink.USD_LAYER, Sink.USDZ_PACKAGE})  # sinks that author a USD layer from the surface arrays, no render pass

# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ExportRow:
    suffix: str
    sink: Sink
    surface: bool = False
    bundle: bool = False
    options: Options = field(default_factory=frozendict)
    # USDZ packaging profile as a typed row column (read only by USDZ_PACKAGE); never stuffed into the `options` bag the exporter closures splat.
    profile: UsdzProfile = UsdzProfile.STANDARD


# --- [ERRORS] --------------------------------------------------------------------------


class ExportError(Exception):
    # the closed-cause capsule the process edge raises; `scene/render#SCENE`'s `async_boundary`
    # mints it into the `RuntimeRail` `BoundaryFault`, never a bare provider raise crossing the seam.
    def __init__(self, cause: ExportFault, /) -> None:
        super().__init__(cause)
        self.cause: ExportFault = cause


# --- [TABLES] --------------------------------------------------------------------------

ROW: Final[Map[SceneTarget, ExportRow]] = Map.of_seq([
    (SceneTarget.PNG, ExportRow(suffix="png", sink=Sink.RASTER)),
    (
        SceneTarget.GLTF,
        ExportRow(suffix="gltf", sink=Sink.PLOTTER, surface=True, options=frozendict({"inline_data": True, "save_normals": True, "rotate_scene": True})),
    ),
    (SceneTarget.VRML, ExportRow(suffix="vrml", sink=Sink.PLOTTER)),
    (SceneTarget.OBJ, ExportRow(suffix="obj", sink=Sink.PLOTTER, bundle=True)),
    (SceneTarget.HTML, ExportRow(suffix="html", sink=Sink.PLOTTER)),
    (SceneTarget.USD, ExportRow(suffix="usdc", sink=Sink.USD_LAYER)),
    (SceneTarget.USDZ, ExportRow(suffix="usdz", sink=Sink.USDZ_PACKAGE)),
])

# the four `Sink.PLOTTER` exporters as one target-keyed table of bound closures (the runtime plotter is
# the closure argument), so the `export_<target>` correspondence is a vocabulary row, never a string-built
# `getattr(plotter, f"export_{kind.value}")` lookup that restates the SceneTarget value the program already names.
_EXPORTER: Final[Map[SceneTarget, Callable[[object, str, Options], object]]] = Map.of_seq([
    (SceneTarget.GLTF, lambda plotter, out, options: plotter.export_gltf(out, **options)),
    (SceneTarget.VRML, lambda plotter, out, options: plotter.export_vrml(out, **options)),
    (SceneTarget.OBJ, lambda plotter, out, options: plotter.export_obj(out, **options)),
    (SceneTarget.HTML, lambda plotter, out, options: plotter.export_html(out, **options)),
])

# --- [OPERATIONS] ----------------------------------------------------------------------


def plotted(plotter: object, kind: SceneTarget, row: ExportRow, spec: RenderSpec, out: Path, /) -> None:
    # the render-sink write over a live plotter (a grid render OR an imported scene): RASTER screenshots the offscreen
    # PNG to `out`, PLOTTER re-serializes through the `_EXPORTER` bound-closure row, and `captured` reads `out`
    # uniformly. The plotter closes deterministically here so the native VTK render window never leaks to GC.
    try:
        match row.sink:
            case Sink.RASTER:
                plotter.screenshot(str(out), window_size=list(spec.window), transparent_background=spec.transparent)
            case Sink.PLOTTER:
                _EXPORTER[kind](plotter, str(out), row.options)  # one target-keyed exporter row over the four PLOTTER targets
            case _:
                raise ExportError("<write-failed>")  # USD sinks never reach a live plotter — the seam entry routes them to `authored`
    finally:
        plotter.close()  # deterministic VTK render-window teardown (BasePlotter.close)


def authored(surface: Surface, row: ExportRow, spec: RenderSpec, out: Path, /) -> frozendict[str, float | str]:
    # USD authoring rides `scene/stage#STAGE`'s WHEEL-AVAILABLE `MeshAuthor` numpy path — no offscreen render, no plotter,
    # and no source-build-gated `vtkUSDExporter` (absent from the standard vtk wheel: no `vtkmodules.vtkIOUSD` module).
    # The seam entry extracts `surface_arrays` once on the worker and hands the buffers here, so the authored `MeshScene`
    # carries the scalar colour (`display_color`), the render PBR band (`_material`), the AEC taxonomy (`spec.labels`),
    # and the AR metadata (`up_axis`/`meters_per_unit`) — a coloured, classified deliverable, never a bare grey mesh.
    points, faces, normals, colors = surface
    scene = MeshScene(
        prim=PrimKind.Mesh(points, faces, normals),
        up_axis=UpAxis(spec.up_axis),
        meters_per_unit=spec.meters_per_unit,
        display_color=colors,
        material=_material(spec, colors is not None),
        labels=spec.labels,
    )
    layer = out if row.sink is Sink.USD_LAYER else out.with_suffix(".usdc")
    _data, facts = author_mesh(scene, str(layer))
    if row.sink is Sink.USDZ_PACKAGE and not package_usdz(str(layer), str(out), row.profile):
        raise ExportError("<usd-failed>")
    return facts


def captured(root: Path, out: Path, bundle: bool, /) -> bytes:
    # single-file targets read back whole; a `bundle` row captures every file the exporter wrote (`.obj`+`.mtl`) as one
    # deterministic `stream_zip` container — fixed epoch/level, no extended timestamps — so the payload crosses the
    # process seam whole and keys stably. Packaging LAW lives on `package/archive#ARCHIVE`; this is seam mechanics.
    if not bundle:
        data = out.read_bytes()
        if not data:
            raise ExportError("<empty-output>")
        return data
    files = sorted(member for member in root.iterdir() if member.is_file())
    if not files:
        raise ExportError("<empty-output>")
    members = ((member.name, _EPOCH, 0o600, ZIP_AUTO(member.stat().st_size, _ZIP_LEVEL), (member.read_bytes(),)) for member in files)
    try:
        return b"".join(stream_zip(members, extended_timestamps=False))
    except ZipError as broken:
        raise ExportError("<bundle-failed>") from broken


def _material(spec: RenderSpec, colored: bool, /) -> Material | None:
    # thread the render PBR band into a stage Material only when the render requested PBR; a scalar-coloured surface
    # drives diffuseColor from the `displayColor` primvar so the colour survives under the finish (a flat diffuse would override it).
    if not spec.pbr:
        return None
    return Material(
        metallic=spec.metallic or 0.0,
        roughness=spec.roughness if spec.roughness is not None else 0.5,
        diffuse_primvar="displayColor" if colored else None,
    )
```
