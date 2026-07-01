# [PY_ARTIFACTS_SCENE_EXPORT]

The 3D scene-file export owner. `SceneTarget` is the closed file-target `StrEnum` keyed inside the `scene/render#SCENE` `SceneOp.Export` payload — `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ` — and one primary `ROW` `frozendict[SceneTarget, ExportRow]` carries each target's whole export law as policy: the temp-file `suffix`, the `Sink` strategy that writes it, the `surface` pre-pass flag, the multi-file `bundle` flag, and the format `options`. `render_export` reads `ROW[kind]`, folds the surface pre-pass into the shared `render_plotter` capsule the `scene/render#SCENE` owner produces, and dispatches the closed `Sink` vocabulary over one total `match` — `RASTER` writes the offscreen PNG through `scene/render#SCENE`'s `png` sink, `PLOTTER` reads the catalogued exporter from the `_EXPORTER` bound-closure row and projects the `options` policy to its keywords so the `GLTF`/`VRML`/`OBJ`/`HTML` exporters are one arm and not four, and `USD_LAYER`/`USDZ_PACKAGE` author the triangulated surface directly through `scene/stage#STAGE`'s `author_mesh(MeshScene(prim=PrimKind.Mesh(*surface_arrays(grid))))`/`package_usdz` — NO render pass, NO offscreen plotter, and no phantom `vtkUSDExporter` (the standard `vtk` wheel ships no `vtkmodules.vtkIOUSD` module, so a render-window->USD exporter cannot resolve; USD authoring is `usd-core`'s alone). The render sinks and the USD sinks split at the `match`: `_plotted` builds and closes one offscreen plotter for `RASTER`/`PLOTTER`, `_authored` runs the plotter-free surface author for the two USD sinks — never a parallel per-format export surface, never a wasted render for a layer that needs none. Multi-file deliverables (`OBJ`'s `.obj`+`.mtl`) capture the whole temp-dir output through one reproducible `stream_zip` bundle whose `get_compressobj` binds the shared `zlib_ng` SIMD raw-DEFLATE at a fixed level, byte-identical to the `package/archive#ARCHIVE` ZIP path, never dropping the companion material file the lone `read_bytes` discards. Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary collapsed to one `ExportError` at the `anyio.to_process.run_sync` process edge `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail`. The owner rides the native-VTK worker lane: `render_export`/`render_ingest` compose the `scene/render#SCENE` `render_plotter`/`import_plotter` capsules and `stream_zip`/`zlib_ng` inside the subprocess seam. `render_ingest` is the round-trip inverse the `scene/render#SCENE` `SceneOp.Ingest` case dispatches — an existing glTF/OBJ/VRML scene imported through `import_plotter`, re-tuned by `RenderSpec.viewed`, and re-serialized to any render-sink `SceneTarget` — an asset-conditioning capability (ingest a third-party 3D asset, re-render a preview or re-emit a normalized scene file) the catalogued `import_gltf`/`import_obj`/`import_vrml` inverse admits over this same `_plotted` dispatch. This owner holds the `geometry/mesh` boundary: a scene file is a rendered-scene serialization (camera/lights/PBR-mapped surfaces), distinct from the raw mesh-interchange codec. This page closes the scene-file-export half of the `SCENE_FILE_EXPORT` idea.

## [01]-[INDEX]

- [02]-[EXPORT]: the `SceneTarget` closed file-target `StrEnum`, the primary `ROW` `frozendict[SceneTarget, ExportRow]` policy table carrying per-target `suffix`/`sink`/`surface`/`bundle`/`options`, the closed `Sink` write-strategy vocabulary (`RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE`) dispatched by one total `match` in `render_export`, the `PLOTTER` `_EXPORTER` target-keyed `frozendict` of bound exporter closures over the `GLTF`/`VRML`/`OBJ`/`HTML` exporters with the `options` `**kwargs` projection, the `GLTF` `FieldFilter.Surface` pre-pass, the `OBJ` `stream_zip` reproducible multi-file bundle (its `get_compressobj` bound to the shared `zlib_ng` SIMD raw-DEFLATE at a fixed level for byte-parity with the `package/archive#ARCHIVE` ZIP path), the `USD`/`USDZ` plotter-free surface authoring through `scene/stage#STAGE`, the `render_ingest` round-trip inverse (`import_gltf`/`import_obj`/`import_vrml` re-tune-re-export over `_plotted`), the closed `ExportFault` vocabulary collapsed to `ExportError` at the process edge, and the `geometry/mesh/repair#MESH_CODEC_BOUNDARY` seam; `pyvista` `Plotter.export_gltf(inline_data=, save_normals=, rotate_scene=)`/`export_vrml`/`export_obj`/`export_html`/`import_gltf`/`import_obj`/`import_vrml` and `stream-zip` `stream_zip`/`ZIP_AUTO(size, level)`/`ZipError`/`get_compressobj` settled against the both-tier `.api`, the `scene/stage#STAGE` `author_mesh(MeshScene(prim=PrimKind.Mesh(*surface_arrays(grid))))` surface-author seam replacing the phantom render-window->`vtkUSDExporter` path (no `vtkmodules.vtkIOUSD` in the vtk wheel).

## [02]-[EXPORT]

- Owner: `SceneTarget` the one closed `StrEnum` of file-export targets keyed inside the `scene/render#SCENE` `SceneOp.Export` payload — `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ`, each value the catalogued exporter stem (`"gltf"`/`"vrml"`/`"obj"`/`"html"`) the `PLOTTER` arm derives its method from, or the crate/package suffix (`"usdc"`/`"usdz"`/`"png"`); never a parallel per-format export owner. `ROW` the one primary `frozendict[SceneTarget, ExportRow]` policy table whose row carries each target's whole export law — `suffix`, `sink`, `surface`, `bundle`, `options` — so a new target is one row and a new policy is one column, never an inline literal or a per-arm special case. `ExportRow` the frozen `dataclass` row owner (a compact value-equal config row, never a wire `msgspec.Struct`); `Sink` the closed `StrEnum` of write strategies — `RASTER`/`PLOTTER`/`USD_LAYER`/`USDZ_PACKAGE` — the strategy axis one vocabulary the dispatch matches, never four parallel exporter arms. `render_export` the one worker arm the `scene/render#SCENE` `_emit` `Export` case dispatches over `to_process.run_sync(render_export, grid, target.value, spec)`, re-admitting the crossed `target` once through `SceneTarget(target)`, reading `ROW[kind]`, and rendering through the shared offscreen `render_plotter` capsule the `scene/render#SCENE` owner produces so a file export and an image render share one render path.
- Cases: the `Sink` `match` arms — the two render sinks route through `_plotted` (which builds and closes one offscreen plotter) and the two USD sinks through `_authored` (plotter-free): `RASTER` (writes the offscreen raster through `scene/render#SCENE`'s `png` temp-file sink, the same PNG artifact the `Image` arm produces) · `PLOTTER` (reads the catalogued exporter from the `_EXPORTER[kind]` bound-closure row and projects the row `options` to its keywords, so `GLTF` carries `inline_data`/`save_normals`/`rotate_scene` and `VRML`/`OBJ`/`HTML` carry the empty default — one arm over four exporters keyed by a vocabulary row, never four arms re-deriving the temp-file sink nor a string-built `getattr` lookup) · `USD_LAYER` (authors the triangulated surface `author_mesh(MeshScene(prim=PrimKind.Mesh(*surface_arrays(grid))))` straight to the `.usdc` layer via `scene/stage#STAGE`, no render pass) · `USDZ_PACKAGE` (authors the `.usdc` layer then `package_usdz` packages the AR `.usdz`, the typed `profile` row column resolving the `UsdzProfile.ARKIT`/`STANDARD` packager `scene/stage#STAGE` selects, the `False` verdict railing `<usd-failed>`) — the top-level `render_export` `match` splitting `RASTER | PLOTTER` from `USD_LAYER | USDZ_PACKAGE` and closing on `assert_never`. The `GLTF` row's `surface` flag folds `FieldFilter.Surface()` into the `RenderSpec.filters` chain through `msgspec.structs.replace` before render because the VTK glTF exporter handles only `PolyData`; the `OBJ` row's `bundle` flag captures the `.obj`+`.mtl` pair the exporter writes, never the lone `.obj` the prior flat `read_bytes` dropped. The USD arms never construct a plotter — `surface_arrays` extracts the `.points`/`.regular_faces`/`.point_normals` buffers `scene/stage#STAGE` authors from, so a USD export pays no offscreen-GL render and reaches no `vtkUSDExporter` the standard vtk wheel does not ship.
- Entry: `render_export(grid, target, spec)` runs behind the `scene/render#SCENE` `to_process.run_sync` seam; it re-admits the crossed `target` through `SceneTarget(target)`, reads `ROW[kind]`, opens one `TemporaryDirectory` sink, and splits the `Sink` `match` between the render sinks and the USD sinks. The render sinks (`RASTER`/`PLOTTER`) surface the spec then hand `render_plotter(grid, surfaced)` to `_plotted`, which brackets the offscreen plotter in its own `try`/`finally` `plotter.close()` (deterministic VTK render-window teardown) and writes `out`; the USD sinks (`USD_LAYER`/`USDZ_PACKAGE`) run `_authored`, which authors `out` from `surface_arrays(grid)` through `scene/stage#STAGE` with no plotter constructed at all — the wasted offscreen render the old render-window->`vtkUSDExporter` path forced is gone. `_captured` then reads the output (single-file `read_bytes` or the `bundle` `stream_zip`). `render_ingest(scene, source, target, spec)` is the sibling round-trip worker — `import_plotter` imports an existing scene, `_plotted` re-serializes it to a render-sink target (a USD target rails `<write-failed>`, an imported scene carrying no grid). Every provider raise across `pyvista`/`vtk`/`pxr`, the `package_usdz` `False` verdict, the empty render, and the bundle failure converge on the closed `ExportFault` vocabulary — `<write-failed>`/`<usd-failed>`/`<empty-output>`/`<bundle-failed>` — collapsed to one `ExportError` raised at the process edge, which `scene/render#SCENE`'s `async_boundary` mints into the `RuntimeRail` `BoundaryFault`; no bare `RuntimeError`/`Tf.ErrorException`/`ZipError` crosses the seam unconverted. The returned `bytes` cross back to the `_emit` `Export`/`Ingest` case, keyed once through `ContentIdentity.key(target.value, data)` — the `SceneTarget` value the content-key format tag.
- Auto: the `surface` pre-pass folds into the existing `RenderSpec.staged` reduce — the glTF surface-extract is one more filter in the chain, never a branch — so a clip-then-glTF export runs the clip then the surface extract then the serialize through one filter fold; the `PLOTTER` exporter is the `_EXPORTER[kind]` bound-closure row so the four exporters are one arm with the `options` policy the only per-target variation; the `OBJ` bundle is one `stream_zip` over the sorted temp-dir files at a fixed `_EPOCH` with `extended_timestamps=False` AND `get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=_ZIP_LEVEL)` with the matching `ZIP_AUTO(size, _ZIP_LEVEL)` (the `ZIP_AUTO` builder ignores the function-level `get_compressobj` and binds its own deflate, so both must be set), so the `.obj`+`.mtl` bundle is byte-stable AND byte-identical to the `package/archive#ARCHIVE` ZIP path on the one shared SIMD deflate substrate and the `ContentIdentity.key` key over it is reproducible; the `render_plotter` capsule the render sinks share with the image render carries the same scalar coloring, overlays, and camera; the `USD`/`USDZ` arms author the mesh surface from `surface_arrays(grid)` through `scene/stage#STAGE`'s wheel-available `author_mesh(MeshScene(...))` numpy path, never a render window nor the source-build-gated `export_usd`/`vtkUSDExporter`.
- Receipt: each export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)` keyed by the content key the `_emit` arm derives — the `SceneTarget` value the target string, the returned payload's `len` the byte evidence, and, for the USD sinks, the `scene/stage#STAGE` `ComputeUsdStageStats` prim/layer/up-axis/meters-per-unit band the `render_export` `(bytes, facts)` return threads back through `_authored`; minted once at the `scene/render#SCENE` `Export` arm over the returned `bytes`, this owner contributing the serialized scene-file payload and its stats band, never a parallel per-format receipt rail.
- Growth: a new scene file-export is one `SceneTarget` row plus one `ROW` entry; a new write strategy is one `Sink` member plus one `match` arm (and one `_RENDER_SINKS`/`_USD_SINKS` membership); a deliverable variant (external-`.bin` glTF, ARKit `.usdz`, X3D) is one `ROW` entry carrying its own `options`/`bundle`/`sink`/`profile` with zero new arm (an ARKit `.usdz` is one `ROW` row at `profile=UsdzProfile.ARKIT`); a new multi-file format flips the row `bundle` flag; a new export fault is one `ExportFault` member; a new importable round-trip source is one `scene/render#SCENE` `SceneSource` member plus one `import_*` arm in `import_plotter`. The import/round-trip inverse is REALIZED — the `scene/render#SCENE` `SceneOp.Ingest` case dispatches `render_ingest`, which imports through `import_plotter` and re-serializes over the same `_plotted` render-sink dispatch, so a re-tune-re-export reuses the exporter rows with zero new export surface. Zero new owner — the file-target axis stays one `StrEnum`, the export law one `ROW` table, the strategy one `Sink` vocabulary.

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
from artifacts.scene.render_worker import import_plotter, png, render_plotter, surface_arrays
from artifacts.scene.stage import MeshScene, PrimKind, UsdzProfile, author_mesh, package_usdz

lazy from stream_zip import ZIP_AUTO, ZipError, stream_zip
lazy from zlib_ng import zlib_ng

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
_ZIP_LEVEL: int = 9  # match the `package/archive#ARCHIVE` reproducible-ZIP deflate level so the OBJ bundle is byte-identical on the shared zlib_ng SIMD substrate
_RENDER_SINKS: frozenset[Sink] = frozenset({Sink.RASTER, Sink.PLOTTER})  # sinks that write over a live offscreen plotter
_USD_SINKS: frozenset[Sink] = frozenset({Sink.USD_LAYER, Sink.USDZ_PACKAGE})  # sinks that author a USD layer from the surface arrays, no render pass

# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ExportRow:
    suffix: str
    sink: Sink
    surface: bool = False
    bundle: bool = False
    options: Options = field(default_factory=frozendict)
    profile: UsdzProfile = UsdzProfile.STANDARD  # USDZ packaging profile as a typed row column (read only by USDZ_PACKAGE); never stuffed into the bool `options` bag the exporter closures splat


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


def render_export(grid: object, target: str, spec: RenderSpec) -> tuple[bytes, frozendict[str, float | str]]:
    kind = SceneTarget(target)
    row = ROW[kind]
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{row.suffix}"
        facts: frozendict[str, float | str] = frozendict()  # USD sinks fill it from `_authored`'s `ComputeUsdStageStats` band; the render sinks leave it empty (the `_emit` arm mints window facts from `spec.window`)
        try:
            match row.sink:
                case Sink.RASTER | Sink.PLOTTER:  # the render sinks build+close one offscreen plotter over the admitted grid
                    surfaced = replace(spec, filters=(*spec.filters, FieldFilter.Surface())) if row.surface else spec
                    _plotted(render_plotter(grid, surfaced), kind, row, spec, out)
                case Sink.USD_LAYER | Sink.USDZ_PACKAGE:  # the USD sinks author the surface directly — no render pass, no plotter
                    facts = _authored(grid, row, out)
                case _:
                    assert_never(row.sink)
        except ExportError:
            raise
        except Exception as failed:  # noqa: BLE001 — every pyvista/VTK/pxr provider raise converges on the closed cause here
            raise ExportError("<usd-failed>" if row.sink in _USD_SINKS else "<write-failed>") from failed
        return _captured(Path(work), out, row.bundle), facts


def render_ingest(scene: bytes, source: str, target: str, spec: RenderSpec) -> tuple[bytes, frozendict[str, float | str]]:
    # the render-tune-re-export round-trip the `scene/render#SCENE` `SceneOp.Ingest` case dispatches: an existing
    # glTF/OBJ/VRML scene is imported into an offscreen plotter (`import_plotter`), re-tuned through `RenderSpec.viewed`,
    # and re-serialized to a render-sink `SceneTarget`. USD re-export is unsupported (an imported scene carries no grid
    # for `surface_arrays`), so a USD target rails `<write-failed>` rather than authoring a half-built layer.
    kind = SceneTarget(target)
    row = ROW[kind]
    with TemporaryDirectory() as work:
        out = Path(work) / f"scene.{row.suffix}"
        try:
            if row.sink not in _RENDER_SINKS:
                raise ExportError("<write-failed>")
            _plotted(import_plotter(scene, source, spec), kind, row, spec, out)
        except ExportError:
            raise
        except Exception as failed:  # noqa: BLE001
            raise ExportError("<write-failed>") from failed
        return _captured(Path(work), out, row.bundle), frozendict()  # an imported scene carries no authored-stage stats; the round-trip re-export band is empty


def _plotted(plotter: object, kind: SceneTarget, row: ExportRow, spec: RenderSpec, out: Path, /) -> None:
    # the render-sink write over a live plotter (a grid render OR an imported scene): RASTER screenshots the offscreen
    # PNG to `out`, PLOTTER re-serializes through the `_EXPORTER` bound-closure row, and `_captured` reads `out`
    # uniformly. The plotter closes deterministically here so the native VTK render window never leaks to GC.
    try:
        match row.sink:
            case Sink.RASTER:
                png(plotter, out, spec)
            case Sink.PLOTTER:
                _EXPORTER[kind](plotter, str(out), row.options)  # one target-keyed exporter row over the four PLOTTER targets
            case _:
                raise ExportError("<write-failed>")  # USD sinks never reach a live plotter — the caller routes them to `_authored`
    finally:
        plotter.close()  # deterministic VTK render-window teardown (BasePlotter.close)


def _authored(grid: object, row: ExportRow, out: Path, /) -> frozendict[str, float | str]:
    # USD authoring rides `scene/stage#STAGE`'s WHEEL-AVAILABLE `MeshAuthor` numpy path (its own [RENDER_EXPORT]
    # research names this the path the export arm should prefer): the triangulated surface's points/faces/normals
    # wrap in a `MeshScene` and `author_mesh` writes the `.usdc` layer directly — no offscreen render, no plotter, and
    # no source-build-gated `vtkUSDExporter` (absent from the standard vtk wheel: no `vtkmodules.vtkIOUSD` module).
    # `author_mesh` returns the `ComputeUsdStageStats` prim/layer/up-axis/meters-per-unit band the `Scene` receipt band carries.
    layer = out if row.sink is Sink.USD_LAYER else out.with_suffix(".usdc")
    _data, facts = author_mesh(MeshScene(prim=PrimKind.Mesh(*surface_arrays(grid))), str(layer))
    if row.sink is Sink.USDZ_PACKAGE and not package_usdz(str(layer), str(out), row.profile):
        raise ExportError("<usd-failed>")
    return facts


def _captured(root: Path, out: Path, bundle: bool, /) -> bytes:
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
        # bind the shared zlib_ng SIMD raw-DEFLATE at the fixed level AND pass the matching level to each ZIP_AUTO
        # (which ignores the function-level get_compressobj), so the OBJ+MTL bundle is byte-identical to the
        # `package/archive#ARCHIVE` ZIP path — one reproducible-ZIP convention across every stream_zip user.
        return b"".join(stream_zip(members, extended_timestamps=False, get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=_ZIP_LEVEL)))
    except ZipError as broken:
        raise ExportError("<bundle-failed>") from broken
```
