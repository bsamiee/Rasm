# [PY_ARTIFACTS_VISUAL_EXPORT]

The VisualSpec to ExportPlan axis, preview, and compression. `VisualSpec` is ONE visual-to-export axis collapsing 2D chart engines (altair/plotly/matplotlib) and 3D scientific visualization (pyvista over its vtk engine), with export backends (vl-convert-python/kaleido/pillow) as rows. `Preview` is the single image/preview/media-detection owner; `Compression` is one algorithm-row owner collapsing zstd/lz4/brotli/7z. Outputs key by runtime `ContentIdentity`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                          |
| :-----: | :----------- | :------------------------------------------------------------- |
|   [1]   | VISUAL       | the visual-to-export axis (2D + 3D)                            |
|   [2]   | EXPORT       | the export-backend rows                                        |
|   [3]   | PREVIEW      | image/preview/media-detection                                  |
|   [4]   | COMPRESSION  | the algorithm-row compression owner                            |

## [2]-[VISUAL]

- Owner: `VisualSpec` — the one visual axis discriminating engine; the 2D chart engines and the 3D pyvista scene are cases on one owner.
- Cases: `VisualSpec` cases `Vega(spec)` (altair Vega-Lite) · `Plotly(figure)` (plotly graph_objects) · `Matplotlib(figure)` · `Scene3d(grid, scalars)` (pyvista unstructured grids, scalar fields, slices, 3D scenes consuming data/compute arrays over the vtk engine) — matched by `match`/`case`.
- Entry: `VisualSpec.compose` builds the visual object; the result feeds `ExportPlan.render`.
- Packages: `altair` (`Chart`/`mark_*`/`encode`), `plotly` (`graph_objects.Figure`), `matplotlib` (`Figure`), `pyvista` (`UnstructuredGrid`/`Plotter`/scalar fields over `vtk`), runtime.
- Growth: a new chart engine or a new 3D scene kind is one `VisualSpec` case; zero new surface.
- Boundary: no live dashboard, UI event state, AppUi render surface, or browser runtime; `pyvista`/`vtk` are 3D scientific viz feeding the export surface (reassigned here from the former data catalogue), riding the native floor; a parallel per-engine chart class family is the deleted form. `SPIKE` where the engine rides the pillow-blocked or native-VTK floor.

```python signature
from typing import Literal

from expression import case, tag, tagged_union


@tagged_union(frozen=True)
class VisualSpec:
    tag: Literal["vega", "plotly", "matplotlib", "scene3d"] = tag()
    vega: dict[str, object] = case()
    plotly: object = case()
    matplotlib: object = case()
    scene3d: tuple[object, str] = case()

    @staticmethod
    def Vega(spec: dict[str, object]) -> "VisualSpec":
        return VisualSpec(vega=spec)

    @staticmethod
    def Plotly(figure: object) -> "VisualSpec":
        return VisualSpec(plotly=figure)

    @staticmethod
    def Matplotlib(figure: object) -> "VisualSpec":
        return VisualSpec(matplotlib=figure)

    @staticmethod
    def Scene3d(grid: object, scalars: str) -> "VisualSpec":
        return VisualSpec(scene3d=(grid, scalars))
```

## [3]-[EXPORT]

- Owner: `ExportPlan` — the typed export over backend rows; `ExportBackend` the policy table mapping a visual case + target format to its backend.
- Cases: `ExportBackend` rows vega-to-svg/png (vl-convert-python) · plotly-to-png (kaleido) · raster-resize (pillow) — backend rows, never parallel exporters.
- Entry: `ExportPlan.render` dispatches the visual case through its backend and returns a `RuntimeRail[bytes]` keyed by `ContentIdentity`, contributing an `ArtifactReceipt.export` row.
- Packages: `vl-convert-python` (`vegalite_to_svg`/`vegalite_to_png`), `kaleido` (figure-to-image), `pillow` (`Image`), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new export backend is one `ExportBackend` row; zero new surface.
- Boundary: a per-backend export class family is the deleted form; the export keys by one runtime content owner. `SPIKE` where kaleido/pillow ride the blocked set.

```python signature
from enum import StrEnum
from typing import Final

from msgspec import Struct

from rasm.runtime.rails_resilience import RuntimeRail, boundary


class ExportFormat(StrEnum):
    SVG = "svg"
    PNG = "png"
    PDF = "pdf"


class ExportBackend(Struct, frozen=True):
    fmt: ExportFormat
    package: str


class ExportPlan(Struct, frozen=True):
    visual: VisualSpec
    fmt: ExportFormat

    def render(self) -> "RuntimeRail[bytes]":
        return boundary(f"export.{self.fmt}", lambda: _export(self.visual, self.fmt))
```

## [4]-[PREVIEW]

- Owner: `Preview` — image/preview/media-detection generation over pillow + qrcode + python-magic (one owner, not scattered per type).
- Entry: `Preview.of` detects the media type through python-magic, renders a preview through pillow, and produces a QR code through qrcode where the kind requires it, returning a `RuntimeRail[bytes]`.
- Packages: `pillow` (`Image`/`thumbnail`), `qrcode` (`QRCode`/SVG image factory), `python-magic` (`from_buffer`/`Magic`), runtime.
- Growth: a new preview kind is one branch in `Preview.of`; zero new surface.
- Boundary: a per-media-type preview class family is the deleted form. `SPIKE` on the pillow toolchain.

```python signature
import magic
import qrcode


class Preview:
    @staticmethod
    def media_type(payload: bytes) -> str:
        return magic.from_buffer(payload, mime=True)

    @staticmethod
    def qr(data: str) -> "RuntimeRail[bytes]":
        return boundary("preview.qr", lambda: qrcode.make(data).get_image().tobytes())
```

## [5]-[COMPRESSION]

- Owner: `Compression` — one algorithm-row owner collapsing zstandard/lz4/brotli/py7zr; `CompressionAlgo` the closed `StrEnum`.
- Cases: `CompressionAlgo` rows `ZSTD` · `LZ4` · `BROTLI` · `SEVEN_Z` — matched by `match`/`case`, each binding the algorithm package.
- Entry: `Compression.pack` compresses a payload through the selected algorithm and returns a `RuntimeRail[bytes]` keyed by `ContentIdentity` over the compressed bytes.
- Packages: `zstandard` (`ZstdCompressor`), `lz4` (`frame`), `brotli` (`compress`), `py7zr` (`SevenZipFile`), runtime (`RuntimeRail`/`ContentIdentity`).
- Growth: a new algorithm is one `CompressionAlgo` row; zero new surface.
- Boundary: a per-algorithm compression class family is the deleted form; FINALIZED (cp315-reflected backends).

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

import brotli
import lz4.frame
import py7zr
import zstandard


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    SEVEN_Z = "7z"


def pack(payload: bytes, algo: CompressionAlgo) -> "RuntimeRail[bytes]":
    return boundary(f"compress.{algo}", lambda: _compress(payload, algo))


def _compress(payload: bytes, algo: CompressionAlgo) -> bytes:
    match algo:
        case CompressionAlgo.ZSTD:
            return zstandard.ZstdCompressor().compress(payload)
        case CompressionAlgo.LZ4:
            return lz4.frame.compress(payload, content_checksum=1)
        case CompressionAlgo.BROTLI:
            return brotli.compress(payload, mode=brotli.MODE_GENERIC, quality=11)
        case CompressionAlgo.SEVEN_Z:
            sink = BytesIO()
            with py7zr.SevenZipFile(sink, mode="w") as archive:
                archive.writef(BytesIO(payload), "payload")
            return sink.getvalue()
        case _:
            assert_never(algo)
```

## [6]-[RESEARCH]

- [EXPORT_BACKENDS]: the `vl-convert-python` `vegalite_to_svg`/`vegalite_to_png`, `kaleido` figure-to-image, `pyvista.Plotter` 3D scene render, and `python-magic.from_buffer(mime=True)` spellings are verified against the cp315-reflected `.api/api-vl-convert-python.md`, `.api/api-kaleido.md`, `.api/api-python-magic.md` and the native-floor `.api/api-pyvista.md`; the `pillow`-backed resize confirms once the image toolchain installs (suite TASKLOG `PY_API_004`).
