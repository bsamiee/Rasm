# [PY_ARTIFACTS_SCENE_EXPORT]

The 3D scene-file export owner. `SceneTarget` is the closed file-target `StrEnum` keyed inside the `scene/render#SCENE` `SceneOp.Export` payload — `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ` — and one primary `ROW` `frozendict[SceneTarget, ExportRow]` carries each target's whole export law as policy: the temp-file `suffix`, the `Sink` strategy that writes it, the `surface` pre-pass flag, the multi-file `bundle` flag, and the format `options`. `render_export` reads `ROW[kind]`, folds the surface pre-pass into the shared `render_plotter` capsule the `scene/render#SCENE` owner produces, and dispatches the closed `Sink` vocabulary over one total `match` — `RASTER` writes the offscreen PNG through `scene/render#SCENE`'s `png` sink, `PLOTTER` derives the catalogued exporter `getattr(plotter, f"export_{kind.value}")` and projects the `options` policy to its keywords so the `GLTF`/`VRML`/`OBJ`/`HTML` exporters are one arm and not four, `USD_LAYER`/`USDZ_PACKAGE` delegate the `Plotter.render_window` to `scene/stage#STAGE`'s `export_usd`/`package_usdz` — never a parallel per-format export surface. Multi-file deliverables (`OBJ`'s `.obj`+`.mtl`) capture the whole temp-dir output through one reproducible `stream_zip` bundle, never dropping the companion material file the lone `read_bytes` discards. Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary collapsed to one `ExportError` at the `anyio.to_process.run_sync` process edge `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail`. The owner rides the native-VTK worker lane: `render_export` imports `pyvista` and `stream_zip` inside the subprocess seam. This owner holds the `geometry/mesh` boundary: a scene file is a rendered-scene serialization (camera/lights/PBR-mapped surfaces), distinct from the raw mesh-interchange codec. This page closes the scene-file-export half of the `SCENE_FILE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[EXPORT]: the `SceneTarget` closed file-target `StrEnum`, the primary `ROW` `frozendict[SceneTarget, ExportRow]` policy table carrying per-target `suffix`/`sink`/`surface`/`bundle`/`options`, the closed `Sink` write-strategy vocabulary (`RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE`) dispatched by one total `match` in `render_export`, the `PLOTTER` `_EXPORTER` target-keyed `frozendict` of bound exporter closures over the `GLTF`/`VRML`/`OBJ`/`HTML` exporters with the `options` `**kwargs` projection, the `GLTF` `FieldFilter.Surface` pre-pass, the `OBJ` `stream_zip` reproducible multi-file bundle, the `USD`/`USDZ` delegation to `scene/stage#STAGE`, the closed `ExportFault` vocabulary collapsed to `ExportError` at the process edge, and the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam; `pyvista` `Plotter.export_gltf(inline_data=, save_normals=, rotate_scene=)`/`export_vrml`/`export_obj`/`export_html` and `stream-zip` `stream_zip`/`ZIP_AUTO`/`ZipError` settled against the both-tier `.api`, the `Plotter.render_window` accessor feeding the `scene/stage#STAGE` USD arm carried as a [03]-[RESEARCH] catalogue-deepen seam.

## [02]-[EXPORT]

- Owner: `SceneTarget` the one closed `StrEnum` of file-export targets keyed inside the `scene/render#SCENE` `SceneOp.Export` payload — `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ`, each value the catalogued exporter stem (`"gltf"`/`"vrml"`/`"obj"`/`"html"`) the `PLOTTER` arm derives its method from, or the crate/package suffix (`"usdc"`/`"usdz"`/`"png"`); never a parallel per-format export owner. `ROW` the one primary `frozendict[SceneTarget, ExportRow]` policy table whose row carries each target's whole export law — `suffix`, `sink`, `surface`, `bundle`, `options` — so a new target is one row and a new policy is one column, never an inline literal or a per-arm special case. `ExportRow` the frozen `dataclass` row owner (a compact value-equal config row, never a wire `msgspec.Struct`); `Sink` the closed `StrEnum` of write strategies — `RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE` — the strategy axis one vocabulary the dispatch matches, never four parallel exporter arms. `render_export` the one worker arm the `scene/render#SCENE` `_emit` `Export` case dispatches over `to_process.run_sync(render_export, grid, target.value, spec)`, re-admitting the crossed `target` once through `SceneTarget(target)`, reading `ROW[kind]`, and rendering through the shared offscreen `render_plotter` capsule the `scene/render#SCENE` owner produces so a file export and an image render share one render path.
- Cases: the `Sink` `match` arms in `render_export` — `RASTER` (writes the offscreen raster through `scene/render#SCENE`'s `png` temp-file sink, the same PNG artifact the `Image` arm produces) · `PLOTTER` (reads the catalogued exporter from the `_EXPORTER[kind]` bound-closure row and projects the row `options` to its keywords, so `GLTF` carries `inline_data`/`save_normals`/`rotate_scene` and `VRML`/`OBJ`/`HTML` carry the empty default — one arm over four exporters keyed by a vocabulary row, never four arms re-deriving the temp-file sink nor a string-built `getattr` lookup) · `USD_LAYER` (delegates the `Plotter.render_window` to `scene/stage#STAGE`'s `export_usd`, the `vtkUSDExporter` writing the `.usdc` layer) · `USDZ_PACKAGE` (writes the `.usdc` layer then `package_usdz` packages the AR `.usdz`, the `arkit` row option selecting its ARKit variant, the `False` verdict railing `<usd-failed>`) — matched by one total `match`/`case` over the row's `Sink`, closed by `assert_never`. The `GLTF` row's `surface` flag folds `FieldFilter.Surface()` into the `RenderSpec.filters` chain through `msgspec.structs.replace` before render because the VTK glTF exporter handles only `PolyData`; the `OBJ` row's `bundle` flag captures the `.obj`+`.mtl` pair the exporter writes, never the lone `.obj` the prior flat `read_bytes` dropped.
- Entry: `render_export(grid, target, spec)` runs behind the `scene/render#SCENE` `to_process.run_sync` seam; it re-admits the crossed `target` through `SceneTarget(target)`, reads `ROW[kind]`, conditionally surfaces the spec, constructs the `render_plotter` capsule bracketed in a `try`/`finally` `plotter.close()` (deterministic VTK render-window teardown trailing every sink, the USD arms reading `plotter.render_window` before the finally), opens one `TemporaryDirectory` sink, dispatches the `Sink` `match`, and captures the output through `_captured` (single-file `read_bytes` or the `bundle` `stream_zip`). Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary — `<write-failed>`/`<usd-failed>`/`<empty-output>`/`<bundle-failed>` — collapsed to one `ExportError` raised at the process edge, which `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail` `BoundaryFault`; no bare `RuntimeError`/`Tf.ErrorException`/`ZipError` crosses the seam unconverted. The returned `bytes` cross back to the `_emit` `Export` case, keyed once through `ContentIdentity.of(target.value, data)` — the `SceneTarget` value the content-key format tag.
- Auto: the `surface` pre-pass folds into the existing `RenderSpec.staged` reduce — the glTF surface-extract is one more filter in the chain, never a branch — so a clip-then-glTF export runs the clip then the surface extract then the serialize through one filter fold; the `PLOTTER` exporter is the `_EXPORTER[kind]` bound-closure row so the four exporters are one arm with the `options` policy the only per-target variation; the `OBJ` bundle is one `stream_zip` over the sorted temp-dir files at a fixed `_EPOCH` with `extended_timestamps=False`, so the `.obj`+`.mtl` bundle is byte-stable and the `ContentIdentity.of` key over it is reproducible; the `render_plotter` capsule the export shares with the image render carries the same scalar coloring, overlays, and camera; the `USD`/`USDZ` arms hand the `Plotter.render_window` to `scene/stage#STAGE`.
- Receipt: each export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes_)` keyed by the content key the `_emit` arm derives, the `SceneTarget` value the target string and the returned payload's `len` the `bytes_` evidence slot the prior call left defaulted `0`; minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes`, this owner contributing the serialized scene-file payload, never a parallel per-format receipt rail.
- Growth: a new scene file-export is one `SceneTarget` row plus one `ROW` entry; a new write strategy is one `Sink` member plus one `match` arm; a deliverable variant (external-`.bin` glTF, ARKit `.usdz`, X3D) is one `ROW` entry carrying its own `options`/`bundle`/`sink` with zero new arm; a new multi-file format flips the row `bundle` flag; a new export fault is one `ExportFault` member. The import/round-trip inverse (`import_gltf`/`import_obj`/`import_vrml`) is one `scene/render#SCENE` `SceneOp` case reusing this dispatch, the catalogued inverse the growth axis absorbs. Zero new owner — the file-target axis stays one `StrEnum`, the export law one `ROW` table, the strategy one `Sink` vocabulary.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass, field
from datetime import UTC, datetime
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Literal, assert_never

from builtins import frozendict
from msgspec.structs import replace

from artifacts.scene.render import FieldFilter, RenderSpec
from artifacts.scene.render_worker import png, render_plotter
from artifacts.scene.stage import export_usd, package_usdz

lazy from stream_zip import ZIP_AUTO, ZipError, stream_zip

# --- [TYPES] ---------------------------------------------------------------------------

type ExportFault = Literal["<write-failed>", "<usd-failed>", "<empty-output>", "<bundle-failed>"]
type Options = frozendict[str, bool]


class SceneTarget(StrEnum):
    PNG = "png"
    GLTF = "gltf"
    VRML = "vrml"
    OBJ = "obj"
    HTML = "html"
    USD = "usdc"
    USDZ = "usdz"


class Sink(StrEnum):
    RASTER = "raster"
    PLOTTER = "plotter"
    USD_LAYER = "usd-layer"
    USDZ_PACKAGE = "usdz-package"


# --- [CONSTANTS] -----------------------------------------------------------------------

_EPOCH: datetime = datetime(1980, 1, 1, tzinfo=UTC)  # fixed zip member stamp -> byte-stable, content-addressable bundle

# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ExportRow:
    suffix: str
    sink: Sink
    surface: bool = False
    bundle: bool = False
    options: Options = field(default_factory=frozendict)


# --- [ERRORS] --------------------------------------------------------------------------


class ExportError(Exception):
    # the closed-cause capsule the process edge raises; `scene/render#SCENE`'s `async_boundary`
    # mints it into the `RuntimeRail` `BoundaryFault`, never a bare provider raise crossing the seam.
    def __init__(self, cause: ExportFault, /) -> None:
        super().__init__(cause)
        self.cause: ExportFault = cause


# --- [TABLES] --------------------------------------------------------------------------

ROW: frozendict[SceneTarget, ExportRow] = frozendict({
    SceneTarget.PNG: ExportRow(suffix="png", sink=Sink.RASTER),
    SceneTarget.GLTF: ExportRow(
        suffix="gltf", sink=Sink.PLOTTER, surface=True,
        options=frozendict({"inline_data": True, "save_normals": True, "rotate_scene": True}),
    ),
    SceneTarget.VRML: ExportRow(suffix="vrml", sink=Sink.PLOTTER),
    SceneTarget.OBJ: ExportRow(suffix="obj", sink=Sink.PLOTTER, bundle=True),
    SceneTarget.HTML: ExportRow(suffix="html", sink=Sink.PLOTTER),
    SceneTarget.USD: ExportRow(suffix="usdc", sink=Sink.USD_LAYER),
    SceneTarget.USDZ: ExportRow(suffix="usdz", sink=Sink.USDZ_PACKAGE),
})

# the four `Sink.PLOTTER` exporters as one target-keyed table of bound closures (the runtime plotter is
# the closure argument), so the `export_<target>` correspondence is a vocabulary row, never a string-built
# `getattr(plotter, f"export_{kind.value}")` lookup that restates the SceneTarget value the program already names.
_EXPORTER: frozendict[SceneTarget, Callable[[object, str, Options], object]] = frozendict({
    SceneTarget.GLTF: lambda plotter, out, options: plotter.export_gltf(out, **options),
    SceneTarget.VRML: lambda plotter, out, options: plotter.export_vrml(out, **options),
    SceneTarget.OBJ: lambda plotter, out, options: plotter.export_obj(out, **options),
    SceneTarget.HTML: lambda plotter, out, options: plotter.export_html(out, **options),
})

# --- [OPERATIONS] ----------------------------------------------------------------------


def render_export(grid: object, target: str, spec: RenderSpec) -> bytes:
    kind = SceneTarget(target)
    row = ROW[kind]
    surfaced = replace(spec, filters=(*spec.filters, FieldFilter.Surface())) if row.surface else spec
    plotter = render_plotter(grid, surfaced)
    try:
        with TemporaryDirectory() as work:
            root = Path(work)
            out = root / f"scene.{row.suffix}"
            try:
                match row.sink:
                    case Sink.RASTER:
                        return png(plotter, out, spec)
                    case Sink.PLOTTER:
                        _EXPORTER[kind](plotter, str(out), row.options)  # one target-keyed exporter row over the four PLOTTER targets
                    case Sink.USD_LAYER:
                        export_usd(plotter.render_window, str(out))
                    case Sink.USDZ_PACKAGE:
                        layer = out.with_suffix(".usdc")
                        export_usd(plotter.render_window, str(layer))
                        if not package_usdz(str(layer), str(out), arkit=row.options.get("arkit", False)):
                            raise ExportError("<usd-failed>")
                    case _:
                        assert_never(row.sink)
            except ExportError:
                raise
            except Exception as failed:  # noqa: BLE001 — every pyvista/VTK/pxr provider raise converges on the closed cause here
                raise ExportError("<usd-failed>" if row.sink in (Sink.USD_LAYER, Sink.USDZ_PACKAGE) else "<write-failed>") from failed
            return _captured(root, out, row.bundle)
    finally:
        plotter.close()  # deterministic VTK render-window teardown (BasePlotter.close); the USD arms read plotter.render_window before this finally, so close trails every sink including the raster return


def _captured(root: Path, out: Path, bundle: bool, /) -> bytes:
    if not bundle:
        data = out.read_bytes()
        if not data:
            raise ExportError("<empty-output>")
        return data
    files = sorted(member for member in root.iterdir() if member.is_file())
    if not files:
        raise ExportError("<empty-output>")
    members = ((member.name, _EPOCH, 0o600, ZIP_AUTO(member.stat().st_size), (member.read_bytes(),)) for member in files)
    try:
        return b"".join(stream_zip(members, extended_timestamps=False))
    except ZipError as broken:
        raise ExportError("<bundle-failed>") from broken
```
