# [PY_ARTIFACTS_PREVIEW]

The raster image-processing and preview owner. `Preview` is ONE owner over the host-free imaging pipeline: pillow (I/O, resize, thumbnail, format conversion, montage) and scikit-image (scientific transforms, segmentation, measurement) on the gated `python_version<'3.15'` band, qrcode (QR generation with SVG/PNG factories) and python-magic (libmagic MIME detection) on the cp315 core. One preview/raster surface, not a per-media-type class family. The cp315-core process imports no gated distribution, so the pillow/scikit-image raster arms run on the runtime subprocess seam. Every operation returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[PREVIEW]`, the raster/preview/QR/media-detection owner over pillow, scikit-image, qrcode, and python-magic.

## [2]-[PREVIEW]

- Owner: `Preview` the one raster-and-preview owner discriminating operation; `PreviewOp` the closed `StrEnum` of raster operations; the pillow image is the working surface, scikit-image the scientific-transform extension, qrcode the code-generation arm, python-magic the media-type gate.
- Cases: `PreviewOp` rows `THUMBNAIL` (pillow `Image.thumbnail`) · `CONVERT` (pillow `Image.save` to a target format) · `MONTAGE` (pillow grid composite) · `TRANSFORM` (scikit-image transform/segmentation/measure on the gated floor) · `QR` (qrcode `QRCode` with the SVG/PNG image factory) — matched by `match`/`case`.
- Entry: `Preview.of` is `async` over the runtime `async_boundary`, detects the media type through python-magic `from_buffer(mime=True)`, dispatches the operation, and returns a `RuntimeRail[ContentKey]`; the `QR` arm bypasses media detection and renders the code in-process, the pillow/scikit-image raster arms ride the subprocess seam.
- Auto: `_emit` folds the op — `QR` through `qrcode.QRCode` with the chosen image factory in-process; `THUMBNAIL`/`CONVERT`/`MONTAGE`/`TRANSFORM` through the gated-band worker, where pillow `Image.open`/`thumbnail`/`save` and the scikit-image transform/segmentation/measure members run at module scope.
- Receipt: each operation contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Preview` carrying the content key and the pixel dimensions.
- Packages: `qrcode` (`QRCode`/SVG-PNG factory), `python-magic` (`from_buffer`/`Magic`) on the cp315 core; `pillow` (`Image`/`thumbnail`/`save`) and `scikit-image` (transform/segmentation/measure) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane)).
- Growth: a new raster operation is one `PreviewOp` row plus one acceptor arm; zero new surface.
- Boundary: a per-media-type preview class family is the deleted form; no UI, no live viewer. The cp315-core `QR` arm and media detection run in-process; `pillow` and `scikit-image` ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so the raster arms dispatch onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports them at module scope — neither a module-top nor a lazy gated import lands on the core page.

```python signature
from enum import StrEnum
from io import BytesIO

import magic
import qrcode
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
        return magic.from_buffer(payload, mime=True)


def _qr(payload: bytes, params: dict[str, object]) -> bytes:
    code = qrcode.QRCode(image_factory=params["factory"])
    code.add_data(payload.decode())
    sink = BytesIO()
    code.make_image().save(sink)
    return sink.getvalue()
```

## [3]-[RESEARCH]

- [GATED_RASTER]: `_gated_raster` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, so the pillow `Image.open`/`thumbnail`/`save` thumbnail/convert/montage arms and the scikit-image transform/segmentation/measure arm import at module scope inside the gated-band worker, never on the cp315-core owner. The pillow and scikit-image call spellings verify against the folder `.api` catalogues for `pillow`/`scikit-image` once the gated band installs; the in-process `qrcode.QRCode`/`add_data`/`make_image`/`image_factory` and `python-magic.from_buffer(mime=True)` spellings verify against the `qrcode`/`python-magic` catalogues on the cp315 core.
