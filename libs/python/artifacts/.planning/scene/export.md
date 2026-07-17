# [PY_ARTIFACTS_SCENE_EXPORT]

`ROW` carries every `SceneTarget` export law as one closed `ExportRow` case: a plotter row owns its `write` closure, `Prepass`, `Capture`, and `options`; a USD-layer row is payload-free; and a USDZ-package row owns its `UsdzProfile`. Every file suffix is the `SceneTarget` value itself, never a per-row literal restating the key. `plotted`, `authored`, and `captured` fold those cases directly. An import-time coverage gate proves `ROW` spans `SceneTarget`. Scene files serialize camera, lights, and PBR-mapped surfaces; raw mesh interchange remains a distinct boundary.

`SceneTarget` is `scene/spec#SPEC`'s, composed downward from the parse-floor module. This module imports `rasm.artifacts.scene.spec` and `rasm.artifacts.scene.stage`, reaches nothing in the package plane, and never imports the runtime-only `rasm.artifacts.scene.render`. Worker-side imports are eager and floor-legal. `scene/render_worker#WORKER`'s `render_export` and `render_ingest` alone compose this machinery. Named provider faults, a failed `PackageOp`, an unsupported round-trip target, an empty output, and `ZipError` converge on `ExportError`; unexpected exceptions remain defects.

## [01]-[INDEX]

- [02]-[EXPORT]: the per-target export law over `scene/spec#SPEC`'s `SceneTarget`, dispatched directly through the closed `ExportRow` family at the render worker boundary.

## [02]-[EXPORT]

- Owner: `SceneTarget` composes downward from `scene/spec#SPEC`. `ROW` is the one `Final[frozendict[SceneTarget, ExportRow]]` correspondence; each `ExportRow` case carries only its strategy's payload, its `capture` projection derives through one total match, and the file suffix reads off the `SceneTarget` value. `plotted`, `authored`, and `captured` are the worker seam's strategy folds.
- Cases: plotter rows route through `plotted`, whose `try`/`finally` closes the live render window after the row's `write` closure runs. `PNG` selects `screenshot`; `SVG`/`PDF`/`EPS`/`PS`/`TEX` select the direct `vtkGL2PSExporter` write over `Plotter.render_window` — BSP depth sort, uncompressed output — because a painter-sorted raster hybrid loses the linework; `GLTF`/`VRML`/`OBJ`/`HTML` select their verified exporter. `Prepass.SURFACE` makes the `GLTF` row's `prepared` projection append `FieldFilter.Surface()`, and `Capture.BUNDLE` makes the `OBJ` row capture the `.obj`/`.mtl` pair. USD rows route through `authored`, which always authors one root group containing the dressed geometry plus every `PrimKind.Sun`; `ExportRow.usdz_package` then selects `PackageOp.Package` and `PackageOp.Verify` with its own `UsdzProfile`.
- Entry: consumers read `ROW[kind]` and match `ExportRow` directly — no wrapper narrows the correspondence. An ingest round-trip refuses a USD target with `<unsupported-target>` because an imported render window carries no admitted mesh graph. `captured` matches `Capture.SINGLE` or `Capture.BUNDLE`; returned `bytes` key once through the `SceneTarget` format tag.
- Auto: `ExportRow.prepared` folds `Prepass.SURFACE` into `RenderSpec.staged`. `Capture.BUNDLE` streams each sorted member through bounded chunks into one `stream_zip` at fixed timestamp and compression policy; `Capture.SINGLE` reads the one output. `USD`/`USDZ` author the surface buffers through the numpy stage path without a render window.
- Receipt: each export contributes `core/receipt#RECEIPT` `ArtifactReceipt.Scene(key, target, bytes, facts)`, minted once at the `scene/render#SCENE` `Export` arm; this owner contributes the serialized payload and, for the USD sinks, the `scene/stage#STAGE` stats band (prim/layer counts, extent diagonal, the USDZ `compliant` verdict) the worker entry threads back through `authored`. Never a parallel per-format rail.
- Growth: a new scene-file export is one `SceneTarget` member plus one `ROW` entry; the coverage gate rejects an unruled member. A new plotter variant changes `write`, `Prepass`, `Capture`, or `options` inside that row. A new strategy is one `ExportRow` case plus its total projections and worker fold arm. A new USD metadata field threads once through `authored`, a new fault is one `ExportFault` member, and a new round-trip source is one `SceneSource` member plus one `scene/render_worker#WORKER` `_IMPORTER` row. `ROW` remains the single target correspondence.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable, Iterator
from datetime import UTC, datetime
from enum import StrEnum
from functools import partial, reduce
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from builtins import frozendict
from expression import Ok, Result, case, tag, tagged_union
from msgspec.structs import replace
from numpy.typing import NDArray
from stream_zip import ZIP_AUTO, ZipError, stream_zip
from vtkmodules.vtkIOExportGL2PS import vtkGL2PSExporter

from rasm.artifacts.scene.spec import FieldFilter, RenderSpec, SceneTarget, Style, SurfaceBand
from rasm.artifacts.scene.stage import ColorSource, Material, MeshScene, PackageFacts, PackageOp, PrimKind, PrimNode, UpAxis, UsdzProfile, author_mesh, packaged

if TYPE_CHECKING:
    import pyvista as pv

# --- [TYPES] ---------------------------------------------------------------------------

type ExportFault = Literal["<write-failed>", "<usd-failed>", "<empty-output>", "<bundle-failed>", "<unsupported-target>"]
type ExportRowTag = Literal["plotter", "usd_layer", "usdz_package"]
type Options = frozendict[str, bool]
type Surface = tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.float32], NDArray[np.float32] | None]
type Write = Callable[["pv.Plotter", str, RenderSpec, Options], object]


class Prepass(StrEnum):
    DIRECT = "direct"
    SURFACE = "surface"


class Capture(StrEnum):
    SINGLE = "single"
    BUNDLE = "bundle"


# --- [CONSTANTS] -----------------------------------------------------------------------

_EPOCH: datetime = datetime(1980, 1, 1, tzinfo=UTC)
_ZIP_LEVEL: int = 9
_CHUNK: int = 1 << 16
_SUN_NITS: float = 2500.0
_BUNDLE_SUFFIXES: Final[frozenset[str]] = frozenset({".obj", ".mtl"})  # the declared Capture.BUNDLE member pair pv.export_obj writes

# --- [MODELS] --------------------------------------------------------------------------


@tagged_union(frozen=True)
class ExportRow:
    tag: ExportRowTag = tag()
    plotter: tuple[Write, Prepass, Capture, Options] = case()
    usd_layer: None = case()
    usdz_package: UsdzProfile = case()

    @staticmethod
    def Plotted(
        write: Write,
        *,
        prepass: Prepass = Prepass.DIRECT,
        capture: Capture = Capture.SINGLE,
        options: Options = frozendict(),
    ) -> "ExportRow":
        return ExportRow(plotter=(write, prepass, capture, options))

    @staticmethod
    def Layer() -> "ExportRow":
        return ExportRow(usd_layer=None)

    @staticmethod
    def Package(profile: UsdzProfile = UsdzProfile.STANDARD) -> "ExportRow":
        return ExportRow(usdz_package=profile)

    @property
    def capture(self) -> Capture:
        match self:
            case ExportRow(tag="plotter", plotter=(_, _, capture, _)):
                return capture
            case ExportRow(tag="usd_layer") | ExportRow(tag="usdz_package"):
                return Capture.SINGLE
            case _ as unreachable:
                assert_never(unreachable)

    def prepared(self, spec: RenderSpec, /) -> RenderSpec:
        match self:
            case ExportRow(tag="plotter", plotter=(_, Prepass.SURFACE, _, _)):
                return replace(spec, filters=(*spec.filters, FieldFilter.Surface()))
            case ExportRow(tag="plotter", plotter=(_, Prepass.DIRECT, _, _)) | ExportRow(tag="usd_layer") | ExportRow(tag="usdz_package"):
                return spec
            case _ as unreachable:
                assert_never(unreachable)


# --- [ERRORS] --------------------------------------------------------------------------


class ExportError(Exception):
    def __init__(self, cause: ExportFault, /) -> None:
        super().__init__(cause)
        self.cause: ExportFault = cause


# --- [TABLES] --------------------------------------------------------------------------

_GL2PS: Final[frozendict[SceneTarget, str]] = frozendict({
    SceneTarget.SVG: "SetFileFormatToSVG",
    SceneTarget.PDF: "SetFileFormatToPDF",
    SceneTarget.EPS: "SetFileFormatToEPS",
    SceneTarget.PS: "SetFileFormatToPS",
    SceneTarget.TEX: "SetFileFormatToTeX",
})


def _vector(target: SceneTarget, /) -> Write:
    def write(plotter: "pv.Plotter", out: str, _spec: RenderSpec, _options: Options) -> None:
        exporter = vtkGL2PSExporter()
        exporter.SetRenderWindow(plotter.render_window)
        getattr(exporter, _GL2PS[target])()  # format setter resolves at the call seam; the row carries the member name
        exporter.SetFilePrefix(str(Path(out).with_suffix("")))  # GL2PS appends the format suffix itself
        exporter.SetSortToBSP()
        exporter.SetCompress(False)
        exporter.Write()

    return write


ROW: Final[frozendict[SceneTarget, ExportRow]] = frozendict({
    SceneTarget.PNG: ExportRow.Plotted(
        lambda plotter, out, spec, _opt: plotter.screenshot(out, window_size=list(spec.window), scale=spec.scale, transparent_background=spec.transparent)
    ),
    **{target: ExportRow.Plotted(_vector(target)) for target in _GL2PS},
    SceneTarget.GLTF: ExportRow.Plotted(
        lambda plotter, out, _spec, options: plotter.export_gltf(out, **options),
        prepass=Prepass.SURFACE,
        options=frozendict({"inline_data": True, "save_normals": True, "rotate_scene": True}),
    ),
    SceneTarget.VRML: ExportRow.Plotted(lambda plotter, out, _spec, options: plotter.export_vrml(out, **options)),
    SceneTarget.OBJ: ExportRow.Plotted(lambda plotter, out, _spec, options: plotter.export_obj(out, **options), capture=Capture.BUNDLE),
    SceneTarget.HTML: ExportRow.Plotted(lambda plotter, out, _spec, options: plotter.export_html(out, **options)),
    SceneTarget.USD: ExportRow.Layer(),
    SceneTarget.USDZ: ExportRow.Package(),
})

if frozenset(ROW) != frozenset(SceneTarget):
    raise RuntimeError("ROW does not cover SceneTarget")

# --- [OPERATIONS] ----------------------------------------------------------------------


def plotted(plotter: "pv.Plotter", row: ExportRow, spec: RenderSpec, out: Path, /) -> None:
    match row:
        case ExportRow(tag="plotter", plotter=(write, _, _, options)):
            try:
                write(plotter, str(out), spec, options)
            except (OSError, RuntimeError, ValueError) as refused:
                # the provider write set — a filesystem refusal, a VTK/pyvista render-window fault, a rejected
                # export argument — converges on the one export fault; an unexpected raise stays a defect.
                raise ExportError("<write-failed>") from refused
            finally:
                plotter.close()  # unconditional: the live render window closes on success, fault, and defect alike
        case _ as unreachable:
            assert_never(unreachable)


def authored(surface: Surface, row: ExportRow, spec: RenderSpec, out: Path, /) -> frozendict[str, float | str]:
    points, faces, normals, colors = surface
    geometry = PrimKind.Mesh(points, faces, normals)
    scene = MeshScene(
        prim=PrimKind.Group((
            PrimNode(name="Geometry", kind=geometry, display_color=colors, material=_material(spec, colors)),
            *(
                PrimNode(name=f"Sun{rank}", kind=PrimKind.Sun(light.azimuth, light.elevation, light.intensity * _SUN_NITS, light.color))
                for rank, light in enumerate(spec.lights)
            ),
        )),
        up_axis=UpAxis(spec.up_axis),
        meters_per_unit=spec.meters_per_unit,
        labels=spec.labels,
    )
    match row:
        case ExportRow(tag="usd_layer"):
            return author_mesh(scene, str(out))[1]
        case ExportRow(tag="usdz_package", usdz_package=profile):
            layer = out.with_suffix(f".{SceneTarget.USD.value}")
            facts = author_mesh(scene, str(layer))[1]
            # `_closed` already raises `<usd-failed>` on a failed package or compliance close, so a reached
            # projection IS the witnessed verdict; the PackageFacts the Verify close returns carry the package
            # evidence onto the band rather than a hard-coded literal beside a discarded result.
            packaged_facts = _closed(PackageOp.Package(str(layer), str(out), profile), PackageOp.Verify(str(out), profile))
            return frozendict({
                **facts,
                "compliant": "true",
                "profile": profile.value,
                "references": float(packaged_facts.references),
                "assets": float(packaged_facts.assets),
            })
        case _ as unreachable:
            assert_never(unreachable)


def _closed(*ops: PackageOp) -> PackageFacts:
    match reduce(lambda railed, op: railed.bind(lambda _facts: packaged(op)), ops, Ok(PackageFacts())):
        case Result(tag="ok", ok=facts):
            return facts
        case Result(tag="error"):
            raise ExportError("<usd-failed>")
        case _ as unreachable:
            assert_never(unreachable)


def captured(root: Path, out: Path, capture: Capture, /) -> bytes:
    match capture:
        case Capture.SINGLE:
            try:
                data = out.read_bytes()
            except OSError as missing:
                # a vanished or unreadable single output is a write failure at the export boundary, never a raw OSError
                raise ExportError("<write-failed>") from missing
            if not data:
                raise ExportError("<empty-output>")
            return data
        case Capture.BUNDLE:
            try:
                # only the DECLARED bundle members archive — pv.export_obj writes exactly the out-stem .obj/.mtl pair,
                # so a stale sibling export or unrelated worker-dir file never rides into the deliverable — and every
                # filesystem read (iterdir, stat, the lazy _streamed pull inside the join) converges on the bundle fault;
                # the .obj is the mandatory member, the .mtl rides only when materials exist.
                declared = frozenset(f"{out.stem}{suffix}" for suffix in _BUNDLE_SUFFIXES)
                files = sorted(member for member in root.iterdir() if member.is_file() and member.name in declared)
                if not any(member.suffix == ".obj" for member in files):
                    raise ExportError("<empty-output>")
                members = ((member.name, _EPOCH, 0o600, ZIP_AUTO(member.stat().st_size, _ZIP_LEVEL), _streamed(member)) for member in files)
                return b"".join(stream_zip(members, extended_timestamps=False))
            except ZipError as broken:
                raise ExportError("<bundle-failed>") from broken
            except OSError as gone:
                raise ExportError("<bundle-failed>") from gone
        case _ as unreachable:
            assert_never(unreachable)


def _streamed(member: Path, /) -> Iterator[bytes]:
    with member.open("rb") as source:
        yield from iter(partial(source.read, _CHUNK), b"")


def _material(spec: RenderSpec, colors: NDArray[np.float32] | None, /) -> Material | None:
    match spec.style:
        case Style(tag="surface", surface=SurfaceBand(pbr=True) as band):
            return Material(
                source=ColorSource.Primvar("displayColor") if colors is not None else ColorSource.Flat(),
                metallic=band.metallic or 0.0,
                roughness=band.roughness if band.roughness is not None else 0.5,
            )
        case (
            Style(tag="surface")
            | Style(tag="volume")
            | Style(tag="points")
            | Style(tag="lines")
            | Style(tag="arrows")
            | Style(tag="labels")
        ):
            return None
        case _ as unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
