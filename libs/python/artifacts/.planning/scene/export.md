# [PY_ARTIFACTS_SCENE_EXPORT]

The 3D scene-file export law owner. This page owns everything a `SceneTarget` keys: one primary `ROW` policy table carrying each target's whole export law — `suffix`, the `Sink` write strategy, the `surface` pre-pass flag, the multi-file `bundle` flag, the format `options`, the USDZ `profile` — the closed `Sink` write-strategy vocabulary, and the `plotted`/`authored`/`captured` machinery each strategy runs. It holds the `geometry/mesh` boundary: a scene file is a rendered-scene serialization (camera, lights, PBR-mapped surfaces), distinct from the raw mesh-interchange codec.

`SceneTarget` is `scene/render#SCENE`'s, hoisted there and composed downward, so the scene module graph runs export → render only. This module imports only `scene/render#SCENE` (vocabulary) and `scene/stage#STAGE` (USD authoring) — the two ledgered edges — and reaches nothing in the package plane. The `scene/render#SCENE` worker-boundary seam entries `render_export`/`render_ingest` are the sole composers of the machinery; the runtime render module imports nothing here. Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary, collapsed to one `ExportError` at the process edge `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail`. A scene BUNDLE deliverable is `package/archive#ARCHIVE`'s emit whose `parents` are the scene-file content keys — a work-graph DATA edge, never an import — so this page carries no byte-parity contract with any package-plane ZIP.

## [01]-[INDEX]

- [02]-[EXPORT]: the per-target export law over `scene/render#SCENE`'s hoisted `SceneTarget`, dispatched by the `Sink` write-strategy vocabulary through the render worker-boundary seam entries.

## [02]-[EXPORT]

- Owner: `SceneTarget` composed downward from `scene/render#SCENE` — this page never re-mints it, and nothing here is a parallel per-format owner. `ROW` the one `Final[Map[SceneTarget, ExportRow]]` policy table: a new target is one row, a new policy one column, never an inline literal. `ExportRow` a frozen `dataclass` config row, never a wire `msgspec.Struct`. `Sink` the closed write-strategy vocabulary the dispatch matches, split by the `RENDER_SINKS`/`USD_SINKS` membership sets over routing and fault-cause. `plotted`/`authored`/`captured` the public machinery, composed ONLY by the render worker-boundary seam entries.
- Cases: the two render sinks route through `plotted` (a live offscreen plotter, closed in its own `try`/`finally`); the two USD sinks through `authored` (plotter-free). `PLOTTER` reads the exporter from the `_EXPORTER[kind]` bound-closure row and splats the row `options`, so the four exporters are one arm keyed by a vocabulary row. `USDZ_PACKAGE` authors the `.usdc` then `package_usdz` packages the AR `.usdz`, the typed `profile` column resolving the `scene/stage#STAGE` packager, the `False` verdict railing `<usd-failed>`. The `GLTF` row's `surface` flag folds `FieldFilter.Surface()` into the `RenderSpec.filters` chain before render because the VTK glTF exporter handles only `PolyData`; the `OBJ` row's `bundle` flag captures the `.obj`+`.mtl` pair the exporter writes, never the lone `.obj` a flat `read_bytes` drops. `authored` takes the worker-extracted `surface_arrays` as an ARGUMENT — extracted once at the seam entry — so a USD export pays no offscreen-GL render, reaches no `vtkUSDExporter` the standard vtk wheel omits, and delivers a coloured, PBR, AEC-classified mesh, not a bare grey one.
- Entry: NONE — the seam entries live at the `scene/render#SCENE` worker boundary. `render_export` re-admits the crossed `target`, reads `ROW[kind]`, opens one `TemporaryDirectory` sink, and splits the `Sink` `match`. `render_ingest` is the round-trip inverse — a USD target rails `<write-failed>`, an imported scene carrying no grid. `captured` reads the output (single-file `read_bytes` or the deterministic container). The returned `bytes` key once through `ContentIdentity.key(target.value, data)`, the `SceneTarget` value the content-key format tag.
- Auto: the `surface` pre-pass folds into the existing `RenderSpec.staged` reduce — one more filter in the chain, never a branch. The `OBJ` capture is one `stream_zip` over the sorted temp-dir files at the fixed `_EPOCH`/`_ZIP_LEVEL` with `extended_timestamps=False`: determinism for the content key, never a byte-parity contract with the package plane. `USD`/`USDZ` author from the surface buffers through `scene/stage#STAGE`'s wheel-available numpy path, never a render window nor the source-build-gated `export_usd`/`vtkUSDExporter`.
- Receipt: each export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)`, minted once at the `scene/render#SCENE` `Export` arm; this owner contributes the serialized payload and, for the USD sinks, the `scene/stage#STAGE` stats band the seam entry threads back through `authored`. Never a parallel per-format rail.
- Growth: a new scene file-export is one `SceneTarget` member (on `scene/render#SCENE`) plus one `ROW` entry; a new write strategy is one `Sink` member plus one `match` arm and one `RENDER_SINKS`/`USD_SINKS` membership; a deliverable variant (external-`.bin` glTF, ARKit `.usdz`, X3D) is one `ROW` entry with its own `options`/`bundle`/`sink`/`profile`, zero new arm; a new multi-file format flips the row `bundle` flag; a new USD metadata field threads once in `authored`; a new fault is one `ExportFault` member; a new importable round-trip source is one `scene/render#SCENE` `SceneSource` member plus one `import_*` arm. A scene BUNDLE deliverable is a `package/archive#ARCHIVE` emit over scene-file parent keys — a DATA edge, never an edge from this page. Zero new owner.

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
_ZIP_LEVEL: int = 9  # fixed deflate level -> content-key determinism over the multi-file capture, not package-plane parity
RENDER_SINKS: frozenset[Sink] = frozenset({Sink.RASTER, Sink.PLOTTER})  # write over a live offscreen plotter
USD_SINKS: frozenset[Sink] = frozenset({Sink.USD_LAYER, Sink.USDZ_PACKAGE})  # author a USD layer, no render pass

# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ExportRow:
    suffix: str
    sink: Sink
    surface: bool = False
    bundle: bool = False
    options: Options = field(default_factory=frozendict)
    # typed row column read only by USDZ_PACKAGE; never in the `options` bag the exporter closures splat
    profile: UsdzProfile = UsdzProfile.STANDARD


# --- [ERRORS] --------------------------------------------------------------------------


class ExportError(Exception):
    # closed-cause capsule the process edge raises, never a bare provider raise crossing the seam.
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

# the four `Sink.PLOTTER` exporters as one target-keyed closure table, never a string-built
# `getattr(plotter, f"export_{kind.value}")` restating the SceneTarget the program already names.
_EXPORTER: Final[Map[SceneTarget, Callable[[object, str, Options], object]]] = Map.of_seq([
    (SceneTarget.GLTF, lambda plotter, out, options: plotter.export_gltf(out, **options)),
    (SceneTarget.VRML, lambda plotter, out, options: plotter.export_vrml(out, **options)),
    (SceneTarget.OBJ, lambda plotter, out, options: plotter.export_obj(out, **options)),
    (SceneTarget.HTML, lambda plotter, out, options: plotter.export_html(out, **options)),
])

# --- [OPERATIONS] ----------------------------------------------------------------------


def plotted(plotter: object, kind: SceneTarget, row: ExportRow, spec: RenderSpec, out: Path, /) -> None:
    # RASTER and PLOTTER both write to `out`, which `captured` reads uniformly; the plotter closes here so
    # the native VTK window never leaks to GC.
    try:
        match row.sink:
            case Sink.RASTER:
                plotter.screenshot(str(out), window_size=list(spec.window), transparent_background=spec.transparent)
            case Sink.PLOTTER:
                _EXPORTER[kind](plotter, str(out), row.options)
            case _:
                raise ExportError("<write-failed>")  # USD sinks never reach a live plotter — routed to `authored`
    finally:
        plotter.close()  # deterministic VTK render-window teardown (BasePlotter.close)


def authored(surface: Surface, row: ExportRow, spec: RenderSpec, out: Path, /) -> frozendict[str, float | str]:
    # wheel-available `scene/stage#STAGE` numpy path — no render, no plotter, no source-build-gated `vtkUSDExporter`
    # (absent from the standard vtk wheel). The seam entry hands the extracted buffers so the authored `MeshScene`
    # carries colour, PBR material, and AEC labels, never a bare grey mesh.
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
    # a `bundle` row zips every exporter-written file (`.obj`+`.mtl`) into one deterministic `stream_zip` (fixed
    # epoch/level, no extended timestamps) so the payload crosses the seam whole and keys stably; packaging LAW is
    # `package/archive#ARCHIVE`'s, this is seam mechanics.
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
    # PBR Material only when the render requested PBR; a scalar-coloured surface drives diffuseColor from the
    # `displayColor` primvar so the colour survives under the finish rather than a flat diffuse overriding it.
    if not spec.pbr:
        return None
    return Material(
        metallic=spec.metallic or 0.0,
        roughness=spec.roughness if spec.roughness is not None else 0.5,
        diffuse_primvar="displayColor" if colored else None,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
