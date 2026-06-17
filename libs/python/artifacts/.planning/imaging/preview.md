# [PY_ARTIFACTS_PREVIEW]

The raster image-processing and preview owner. `Preview` is ONE owner over the 2025 host-free imaging pipeline: pillow (I/O, resize, thumbnail, format conversion, montage, final output), scikit-image (scientific transforms, segmentation, measurement on the gated `cp < 3.15` floor), qrcode (QR generation with SVG/PNG factories), and python-magic (libmagic MIME detection). One preview/raster surface, not a per-media-type class family. Every operation returns a `RuntimeRail[ContentKey]`.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[PREVIEW]`, the raster/preview/QR/media-detection owner over pillow, scikit-image, qrcode, and python-magic.

## [2]-[PREVIEW]

- Owner: `Preview` the one raster-and-preview owner discriminating operation; `PreviewOp` the closed `StrEnum` of raster operations; the pillow image is the working surface, scikit-image the scientific-transform extension, qrcode the code-generation arm, python-magic the media-type gate.
- Cases: `PreviewOp` rows `THUMBNAIL` (pillow `Image.thumbnail`) · `CONVERT` (pillow `Image.save` to a target format) · `MONTAGE` (pillow grid composite) · `TRANSFORM` (scikit-image transform/segmentation/measure on the gated floor) · `QR` (qrcode `QRCode` with the SVG/PNG image factory) — matched by `match`/`case`.
- Entry: `Preview.of` detects the media type through python-magic `from_buffer(mime=True)`, dispatches the operation through pillow or scikit-image, and returns a `RuntimeRail[ContentKey]`; the QR arm bypasses media detection and renders the code directly.
- Auto: `_apply` folds the op — thumbnail/convert/QR through pillow and qrcode inline, montage and the gated scikit-image scientific transforms through `_composite`; thumbnailing folds the source bytes through pillow `Image.open`/`thumbnail`/`save`, QR folds the data string through `qrcode.QRCode` with the chosen image factory.
- Receipt: each operation contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Preview` carrying the content key and the pixel dimensions.
- Packages: `pillow` (`Image`/`thumbnail`/`save`), `scikit-image` (transform/segmentation/measure, gated `cp < 3.15`), `qrcode` (`QRCode`/SVG-PNG factory), `python-magic` (`from_buffer`/`Magic`), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new raster operation is one `PreviewOp` row plus one acceptor arm; zero new surface.
- Boundary: a per-media-type preview class family is the deleted form; no UI, no live viewer; the scikit-image transforms ride the gated `cp < 3.15` floor; the pillow chain finalizes once the image toolchain installs.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

import magic
import qrcode
from PIL import Image
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class PreviewOp(StrEnum):
    THUMBNAIL = "thumbnail"
    CONVERT = "convert"
    MONTAGE = "montage"
    TRANSFORM = "transform"
    QR = "qr"


class Preview(Struct, frozen=True):
    op: PreviewOp
    payload: bytes
    params: dict[str, object]

    def of(self) -> RuntimeRail[ContentKey]:
        return boundary(f"preview.{self.op}", self._emit)

    def _emit(self) -> ContentKey:
        data = _apply(self.op, self.payload, self.params)
        return ContentIdentity.key(f"preview-{self.op}", data)

    @staticmethod
    def media_type(payload: bytes) -> str:
        return magic.from_buffer(payload, mime=True)


def _apply(op: PreviewOp, payload: bytes, params: dict[str, object]) -> bytes:
    match op:
        case PreviewOp.THUMBNAIL:
            image = Image.open(BytesIO(payload))
            image.thumbnail(params["size"])
            sink = BytesIO()
            image.save(sink, format=image.format)
            return sink.getvalue()
        case PreviewOp.CONVERT:
            sink = BytesIO()
            Image.open(BytesIO(payload)).save(sink, format=params["format"])
            return sink.getvalue()
        case PreviewOp.QR:
            code = qrcode.QRCode(image_factory=params["factory"])
            code.add_data(payload.decode())
            sink = BytesIO()
            code.make_image().save(sink)
            return sink.getvalue()
        case PreviewOp.MONTAGE | PreviewOp.TRANSFORM:
            return _composite(op, payload, params)
        case _:
            assert_never(op)
```

## [3]-[RESEARCH]

- [PREVIEW_SPELLINGS]: the pillow `Image.open`/`thumbnail`/`save`, qrcode `QRCode`/`add_data`/`make_image`/`image_factory`, and `python-magic.from_buffer(mime=True)` spellings verify against the branch `.api` catalogue for `pillow`/`qrcode`/`python-magic`; the `_composite` body — the pillow montage grid layout and the scikit-image transform/segmentation/measure members on the gated `cp < 3.15` floor — confirms once the image toolchain and the scikit-image floor install.
