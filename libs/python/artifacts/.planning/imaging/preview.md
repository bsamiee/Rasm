# [PY_ARTIFACTS_PREVIEW]

The raster image-processing and preview owner. `Preview` is ONE owner over the host-free imaging pipeline: pillow (I/O, resize, thumbnail, format conversion, montage) and scikit-image (scientific transforms, segmentation, measurement) on the gated `python_version<'3.15'` band, segno (QR/Micro-QR generation with dependency-free SVG/PNG/PDF serialization) and python-magic (libmagic MIME detection) on the cp315 core. One preview/raster surface, not a per-media-type class family. The cp315-core process imports no gated distribution, so the pillow/scikit-image raster arms run on the runtime subprocess seam. Every operation returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[PREVIEW]`, the raster/preview/QR/media-detection owner over pillow, scikit-image, segno, and python-magic.

## [2]-[PREVIEW]

- Owner: `Preview` the one raster-and-preview owner discriminating operation; `PreviewOp` the closed `StrEnum` of raster operations; the pillow image is the working surface, scikit-image the scientific-transform extension, segno the code-generation arm, python-magic the media-type gate.
- Cases: `PreviewOp` rows `THUMBNAIL` (pillow `Image.thumbnail`) · `CONVERT` (pillow `Image.save` to a target format) · `MONTAGE` (pillow grid composite) · `TRANSFORM` (scikit-image transform/segmentation/measure on the gated floor) · `QR` (segno `make` then `QRCode.save` to a `kind` row — SVG/PNG/PDF with no raster dependency) — matched by `match`/`case`.
- Entry: `Preview.of` is `async` over the runtime `async_boundary`, detects the media type through python-magic `from_buffer(mime=True)`, dispatches the operation, and returns a `RuntimeRail[ContentKey]`; the `QR` arm bypasses media detection and renders the code in-process with no Pillow dependency, the pillow/scikit-image raster arms ride the subprocess seam.
- Auto: `_emit` folds the op — `QR` through `segno.make` then `QRCode.save(sink, kind=...)` in-process, the serializer kind a call row; `THUMBNAIL`/`CONVERT`/`MONTAGE`/`TRANSFORM` through the gated-band worker, where pillow `Image.open`/`thumbnail`/`save` and the scikit-image transform/segmentation/measure members run at module scope.
- Receipt: each operation contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Preview` carrying the content key and the pixel dimensions.
- Packages: `segno` (`make`/`QRCode.save`/`symbol_size`, dependency-free serializers), `python-magic` (`from_buffer`/`Magic`) on the cp315 core; `pillow` (`Image`/`thumbnail`/`save`) and `scikit-image` (transform/segmentation/measure) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new raster operation is one `PreviewOp` row plus one acceptor arm; a new QR serializer is one segno `kind` row; zero new surface.
- Boundary: a per-media-type preview class family is the deleted form; no UI, no live viewer. segno owns QR/Micro-QR generation and serialization with no Pillow dependency, removing the former `qrcode` Pillow leak; the cp315-core `QR` arm and media detection run in-process; `pillow` and `scikit-image` ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so the raster arms dispatch onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports them at module scope — neither a module-top nor a lazy gated import lands on the core page.

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
    QR = "qr"


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
            else _qr(self.payload, self.params)
        )
        return ContentIdentity.of(f"preview-{self.op}", data)

    @staticmethod
    def media_type(payload: bytes) -> str:
        import magic

        return magic.from_buffer(payload, mime=True)


def _qr(payload: bytes, params: dict[str, object]) -> bytes:
    import segno

    code = segno.make(payload.decode(), error=params.get("error"))
    sink = BytesIO()
    code.save(sink, kind=params["kind"], scale=params.get("scale", 1), border=params.get("border"))
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

No open items. The in-process `segno.make`/`QRCode.save(kind=...)` and `python-magic.from_buffer(mime=True)` spellings verify against the folder `.api` catalogues for `segno`/`python-magic` on the cp315 core; segno serializes SVG/PNG/PDF with no Pillow dependency, closing the former `qrcode` Pillow leak. `_gated_raster` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, importing `PIL`/`skimage` at boundary scope inside the gated-band worker, never on the cp315-core owner; the pillow `Image.open`/`thumbnail`/`save` and the scikit-image `io.imread`/`segmentation.slic`/`mark_boundaries`/`measure.regionprops_table`/`util.img_as_ubyte` spellings verify against the folder `.api` catalogues for `pillow`/`scikit-image`. `_montage` composes the pillow grid composite over `Image.new`/`paste`.
