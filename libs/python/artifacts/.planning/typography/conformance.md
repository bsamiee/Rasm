# [PY_ARTIFACTS_CONFORMANCE]

The PDF conformance and signing close over the document rail. `Conformance` is ONE owner that takes a PDF emitted by `documents/document-plan#DOCUMENT` and produces a font-embedded, digitally-signed, archival-grade artifact: fonttools subsets and instances embedded variable fonts to the minimal glyph footprint, and pyhanko applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) with timestamp and validation material. fonttools and pyhanko are admitted in the manifest with no design-page home; this page closes the gap the `PDF_CONFORMANCE_AND_SIGNING` idea raises.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[CONFORM]`, the fonttools subset/instance and pyhanko PAdES-signing conformance owner.

## [2]-[CONFORM]

- Owner: `Conformance` the one PDF-close owner discriminating the conformance step; `ConformStep` the closed `StrEnum` over font subsetting/instancing and PAdES signing; fonttools owns the font footprint, pyhanko the cryptographic layer.
- Cases: `ConformStep` rows `SUBSET` (fontTools `subset.Subsetter` over the embedded glyph set) · `INSTANCE` (`fontTools.varLib.instancer` collapsing a variable font to a static instance) · `SIGN` (pyhanko PAdES baseline signing) — matched by `match`/`case`; the PAdES level (`B-B`/`B-T`/`B-LT`/`B-LTA`) is a signing parameter.
- Entry: `Conformance.close` dispatches the step over the input PDF and returns a `RuntimeRail[ContentKey]`; subsetting and instancing reduce the embedded-font footprint, signing adds the cryptographic layer with the timestamp and validation material; the HarfBuzz repacker engages through uharfbuzz when present.
- Auto: subsetting folds the used glyph set through `Subsetter(Options(...))` `populate(unicodes=...)` then `subset(font)` then `TTFont.save`; instancing folds the axis-pin map through `instancer.instantiateVariableFont`; signing folds the signer credential and the PAdES level through the pyhanko `SimpleSigner.load` then `PdfSignatureMetadata(use_pades_lta=...)` then `sign_pdf` over an `IncrementalPdfFileWriter`, with the `HTTPTimeStamper` authority and the validation-info embedding.
- Receipt: each close contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Pdf` carrying the content key, the post-subset byte count, and the page count.
- Packages: `fonttools` (`subset.Subsetter`/`subset.Options`/`ttLib.TTFont`/`varLib.instancer`), `pyhanko` (`SimpleSigner.load`, `PdfSignatureMetadata`, `sign_pdf`, `IncrementalPdfFileWriter`, `HTTPTimeStamper`; PAdES `B-B`/`B-T`/`B-LT`/`B-LTA` via `use_pades_lta`, PKCS#11), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new conformance step is one `ConformStep` row plus one acceptor arm; a new PAdES level is a signing parameter; zero new surface.
- Boundary: no PDF authoring (that stays at the document axis); pyhanko does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF, so structural conformance is authored upstream at the document axis and signing only adds the cryptographic layer; the owner closes an already-emitted PDF, never producing one.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class ConformStep(StrEnum):
    SUBSET = "subset"
    INSTANCE = "instance"
    SIGN = "sign"


class Conformance(Struct, frozen=True):
    step: ConformStep
    pdf: bytes
    params: dict[str, object]

    def close(self) -> RuntimeRail[ContentKey]:
        return boundary(f"conform.{self.step}", self._emit)

    def _emit(self) -> ContentKey:
        data = _apply(self.step, self.pdf, self.params)
        return ContentIdentity.of(f"conform-{self.step}", data)


def _apply(step: ConformStep, payload: bytes, params: dict[str, object]) -> bytes:
    match step:
        case ConformStep.SUBSET:
            return _subset_font(payload, params)
        case ConformStep.INSTANCE:
            return _instance_font(payload, params)
        case ConformStep.SIGN:
            return _sign_pdf(payload, params)
        case _:
            assert_never(step)


def _subset_font(payload: bytes, params: dict[str, object]) -> bytes:
    from fontTools import subset
    from fontTools.ttLib import TTFont

    font = TTFont(BytesIO(payload))
    subsetter = subset.Subsetter(options=subset.Options(**params.get("options", {})))
    subsetter.populate(unicodes=params["unicodes"])
    subsetter.subset(font)
    sink = BytesIO()
    font.save(sink)
    return sink.getvalue()


def _instance_font(payload: bytes, params: dict[str, object]) -> bytes:
    from fontTools.ttLib import TTFont
    from fontTools.varLib import instancer

    instance = instancer.instantiateVariableFont(TTFont(BytesIO(payload)), params["axes"])
    sink = BytesIO()
    instance.save(sink)
    return sink.getvalue()


def _sign_pdf(payload: bytes, params: dict[str, object]) -> bytes:
    from pyhanko.pdf_utils import IncrementalPdfFileWriter
    from pyhanko.sign import PdfSignatureMetadata, SimpleSigner, sign_pdf
    from pyhanko.sign.timestamps import HTTPTimeStamper

    signer = SimpleSigner.load(params["key_file"], params["cert_file"], key_passphrase=params.get("passphrase"))
    writer = IncrementalPdfFileWriter(BytesIO(payload))
    meta = PdfSignatureMetadata(field_name=params["field"], use_pades_lta=params.get("pades_lta", False))
    timestamper = HTTPTimeStamper(params["tsa_url"]) if "tsa_url" in params else None
    sink = BytesIO()
    sign_pdf(writer, meta, signer, timestamper=timestamper, output=sink)
    return sink.getvalue()
```

## [3]-[RESEARCH]

No open items. The fontTools `subset.Subsetter(options=Options(...))`/`populate(unicodes=...)`/`subset(font)`, `ttLib.TTFont`/`save`, and `varLib.instancer.instantiateVariableFont` spellings verify against the folder `.api` catalogue for `fonttools`; the pyhanko `SimpleSigner.load`, `PdfSignatureMetadata(field_name, use_pades_lta)`, `sign_pdf(writer, meta, signer, timestamper, output)`, `IncrementalPdfFileWriter`, and `HTTPTimeStamper` spellings verify against the folder `.api` catalogue for `pyhanko`. The PAdES level rises from B-B to B-LTA as `use_pades_lta=True` engages the LTV chain; the uharfbuzz repacker engages transparently inside fontTools when present.
