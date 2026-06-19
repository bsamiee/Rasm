# [PY_ARTIFACTS_PREVIEW]

The raster image-processing and preview owner. `Preview` is ONE owner over the host-free imaging pipeline: pillow (I/O, resize, thumbnail, format conversion, montage) and scikit-image (scientific transforms, segmentation, measurement) on the gated `python_version<'3.15'` band, segno (QR/Micro-QR generation with dependency-free SVG/PNG/PDF serialization) and python-magic (libmagic MIME detection) on the cp315 core. One preview/raster surface, not a per-media-type class family. The cp315-core process imports no gated distribution, so the pillow/scikit-image raster arms run on the runtime subprocess seam. Every operation returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[PREVIEW]`, the raster/preview/QR/media-detection owner over pillow, scikit-image, segno, and python-magic.

## [2]-[PREVIEW]

- Owner: `Preview` the one raster-and-preview owner discriminating operation; `PreviewOp` the closed `StrEnum` of raster operations; the pillow image is the working surface, scikit-image the scientific-transform extension, segno the code-generation arm, python-magic the media-type gate.
- Cases: `PreviewOp` rows `THUMBNAIL` (pillow `Image.thumbnail`) · `CONVERT` (pillow `Image.save` to a target format) · `MONTAGE` (pillow grid composite) · `TRANSFORM` (scikit-image transform/segmentation/measure on the gated floor) · `MARK` (the machine-readable-mark arm carrying a `Symbology` sub-axis — QR/Micro-QR over segno and the linear (1D) symbologies over the python-barcode registry, all serializing to the dependency-free SVG path) — matched by `match`/`case`; the former QR-only `PreviewOp.QR` literal is COLLAPSED into the `MARK` row whose `Symbology` discriminates the encoder, never a sibling op per symbology.
- Entry: `Preview.of` is `async` over the runtime `async_boundary`, detects the media type through python-magic `from_buffer(mime=True)`, dispatches the operation, and returns a `RuntimeRail[ContentKey]`; the `MARK` arm bypasses media detection and renders the code in-process with no Pillow dependency (segno serializers and the python-barcode `SVGWriter` need none), the pillow/scikit-image raster arms ride the subprocess seam.
- Auto: `_emit` folds the op — `MARK` through `_mark` which discriminates the `Symbology` sub-axis (QR/Micro-QR through `segno.make`/`make_micro` then `QRCode.save(kind="svg")` in-process; the linear (1D) symbologies through the python-barcode `get_barcode_class(name)` registry then `SVGWriter` render, the symbology resolved by name not a hand-picked sub-enum); `THUMBNAIL`/`CONVERT`/`MONTAGE`/`TRANSFORM` through the gated-band worker, where pillow `Image.open`/`thumbnail`/`save` and the scikit-image transform/segmentation/measure members run at module scope.
- Receipt: each operation contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Preview` carrying the content key and the pixel dimensions.
- Packages: `segno` (`make`/`make_micro`/`QRCode.save`/`symbol_size`, dependency-free serializers), `python-barcode` (`get_barcode_class`/`PROVIDED_BARCODES`/`SVGWriter` linear (1D) symbologies, RESEARCH-pending the catalogue sync), `python-magic` (`from_buffer`/`Magic`) on the cp315 core; `pillow` (`Image`/`thumbnail`/`save`) and `scikit-image` (transform/segmentation/measure) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new raster operation is one `PreviewOp` row plus one acceptor arm; a new QR serializer is one segno `kind` row; a new linear symbology is already covered by the python-barcode `PROVIDED_BARCODES` registry (no new row); a new 2D-matrix symbology (DataMatrix/PDF417) is a SEPARATE future owner, never a python-barcode member; zero new surface.
- Boundary: a per-media-type preview class family and a per-symbology code class are the deleted forms; no UI, no live viewer. segno owns QR/Micro-QR generation and serialization with no Pillow dependency, removing the former `qrcode` Pillow leak; the python-barcode linear arm uses `SVGWriter` ONLY on the in-process core path (its `ImageWriter` PNG path needs Pillow and would re-introduce the leak segno removed); python-barcode is strictly linear (1D) — DataMatrix/PDF417 are DROPPED and route to a separate 2D-matrix owner, never a phantom python-barcode member; the cp315-core `MARK` arm and media detection run in-process; `pillow` and `scikit-image` ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so the raster arms dispatch onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports them at module scope — neither a module-top nor a lazy gated import lands on the core page.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


class PreviewOp(StrEnum):
    THUMBNAIL = "thumbnail"
    CONVERT = "convert"
    MONTAGE = "montage"
    TRANSFORM = "transform"
    MARK = "mark"


class Symbology(StrEnum):
    QR = "qr"
    MICRO_QR = "micro-qr"
    CODE128 = "code128"
    CODE39 = "code39"
    EAN13 = "ean13"
    EAN8 = "ean8"
    UPCA = "upca"
    ITF = "itf"
    CODABAR = "codabar"
    ISBN13 = "isbn13"
    ISSN = "issn"
    PZN = "pzn"
    GS1_128 = "gs1_128"

    @property
    def is_qr(self) -> bool:
        return self in (Symbology.QR, Symbology.MICRO_QR)


GATED: frozenset[PreviewOp] = frozenset(
    {PreviewOp.THUMBNAIL, PreviewOp.CONVERT, PreviewOp.MONTAGE, PreviewOp.TRANSFORM}
)


class Preview(Struct, frozen=True):
    op: PreviewOp
    payload: bytes
    params: dict[str, object]

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"preview.{self.op}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(_gated_raster, self.op.value, self.payload, self.params)
            if self.op in GATED
            else _mark(self.payload, self.params)
        )
        return ContentIdentity.of(f"preview-{self.op}", data)

    @staticmethod
    def media_type(payload: bytes) -> str:
        import magic

        return magic.from_buffer(payload, mime=True)


def _mark(payload: bytes, params: dict[str, object]) -> bytes:
    symbology = Symbology(params["symbology"])
    return _qr(payload, params) if symbology.is_qr else _barcode(payload, symbology, params)


def _qr(payload: bytes, params: dict[str, object]) -> bytes:
    import segno

    micro = Symbology(params["symbology"]) is Symbology.MICRO_QR
    code = segno.make_micro(payload.decode(), error=params.get("error")) if micro else segno.make(payload.decode(), error=params.get("error"))
    sink = BytesIO()
    code.save(sink, kind="svg", scale=params.get("scale", 1), border=params.get("border"))
    return sink.getvalue()


def _barcode(payload: bytes, symbology: Symbology, params: dict[str, object]) -> bytes:
    import barcode

    symbology_class = barcode.get_barcode_class(symbology.value)
    code = symbology_class(payload.decode(), writer=barcode.writer.SVGWriter())
    sink = BytesIO()
    code.write(sink, options=params.get("writer_options"))
    return sink.getvalue()


def _gated_raster(op: str, payload: bytes, params: dict[str, object]) -> bytes:
    from io import BytesIO

    from PIL import Image

    match PreviewOp(op):
        case PreviewOp.THUMBNAIL:
            image = Image.open(BytesIO(payload))
            image.thumbnail((params["width"], params["height"]))
            sink = BytesIO()
            image.save(sink, format=params["format"])
            return sink.getvalue()
        case PreviewOp.CONVERT:
            sink = BytesIO()
            Image.open(BytesIO(payload)).save(sink, format=params["format"])
            return sink.getvalue()
        case PreviewOp.MONTAGE:
            return _montage(payload, params)
        case PreviewOp.TRANSFORM:
            return _transform(payload, params)
        case _:
            assert_never(op)


def _transform(payload: bytes, params: dict[str, object]) -> bytes:
    from io import BytesIO

    import numpy as np
    from skimage import io as skio, measure, segmentation, util

    image = skio.imread(BytesIO(payload))
    labels = segmentation.slic(image, n_segments=params["segments"], channel_axis=-1)
    overlay = util.img_as_ubyte(segmentation.mark_boundaries(image, labels))
    table = measure.regionprops_table(labels, properties=("label", "area", "centroid"))
    sink = BytesIO()
    np.savez(sink, overlay=overlay, **table)
    return sink.getvalue()
```

## [3]-[RESEARCH]

- [QR_SETTLED]: the in-process `segno.make`/`make_micro`/`QRCode.save(kind="svg")` and `python-magic.from_buffer(mime=True)` spellings verify against the folder `.api` catalogues for `segno`/`python-magic` on the cp315 core; segno serializes SVG with no Pillow dependency, so the QR/Micro-QR `Symbology` rows are SETTLED fence code, closing the former `qrcode` Pillow leak. The `Symbology.is_qr` discriminant routes QR/Micro-QR to `_qr` and the linear rows to `_barcode`.
- [BARCODE] [RESEARCH] [BLOCKED]: the python-barcode `get_barcode_class(name)`/`PROVIDED_BARCODES`/`barcode.writer.SVGWriter`/`Barcode.write(fp, options)` registry and writer surface is AUTHORED-PENDING-VERIFICATION in the folder `.api` catalogue for `python-barcode` — the package is present in the lockfile but NOT yet synced into the active venv (catalogue header: `RESEARCH-capture-pending-on-uv-sync`), so the `_barcode` body and the linear `Symbology` rows (`CODE128`/`CODE39`/`EAN13`/`EAN8`/`UPCA`/`ITF`/`CODABAR`/`ISBN13`/`ISSN`/`PZN`/`GS1_128`) stay a marked RESEARCH seam until `assay api` reflection on uv-sync confirms the registry. The symbologies are registry-name-keyed through `get_barcode_class` (resolving the full `PROVIDED_BARCODES` set, never a hand-picked sub-enum that silently drops Codabar/ISBN). python-barcode is strictly linear (1D): DataMatrix/PDF417 are DROPPED from the `Symbology` axis (the catalogue header confirms linear-only) and route to a separate future 2D-matrix owner, never a phantom python-barcode member. Close-condition: uv-sync resolves python-barcode and `ApiPackage.reflect` confirms `get_barcode_class`/`SVGWriter`/`PROVIDED_BARCODES`. The `SVGWriter` is the ONLY admitted writer (the `ImageWriter` PNG path needs Pillow and re-introduces the leak segno removed).
- [RASTER]: `_gated_raster` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, importing `PIL`/`skimage` at boundary scope inside the gated-band worker, never on the cp315-core owner; the pillow `Image.open`/`thumbnail`/`save` and the scikit-image `io.imread`/`segmentation.slic`/`mark_boundaries`/`measure.regionprops_table`/`util.img_as_ubyte` spellings verify against the folder `.api` catalogues for `pillow`/`scikit-image`. `_montage` composes the pillow grid composite over `Image.new`/`paste`.
