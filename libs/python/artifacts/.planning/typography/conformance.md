# [PY_ARTIFACTS_CONFORMANCE]

The PDF conformance and signing close over the document rail. `Conformance` is ONE owner that takes a PDF emitted by `documents/document-plan#DOCUMENT` and produces a font-embedded, digitally-signed, archival-grade artifact: fonttools subsets and instances embedded variable fonts to the minimal glyph footprint, and pyhanko applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) with timestamp and validation material. fonttools and pyhanko are admitted in the manifest with no design-page home; this page closes the gap the `PDF_CONFORMANCE_AND_SIGNING` idea raises.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[CONFORM]`, the fonttools subset/instance and pyhanko PAdES-signing conformance owner.

## [2]-[CONFORM]

- Owner: `Conformance` the one PDF-close owner discriminating the conformance step; `ConformStep` the closed `StrEnum` over font subsetting/instancing and PAdES signing; fonttools owns the font footprint, pyhanko the cryptographic layer.
- Cases: `ConformStep` rows `SUBSET` (fontTools `subset.Subsetter` over the embedded glyph set) · `INSTANCE` (`fontTools.varLib.instancer` collapsing a variable font to a static instance) · `SIGN` (pyhanko PAdES baseline signing) — matched by `match`/`case`; the PAdES level (`B-B`/`B-T`/`B-LT`/`B-LTA`) is a signing parameter.
- Entry: `Conformance.close` dispatches the step over the input PDF and returns a `RuntimeRail[ContentKey]`; subsetting and instancing reduce the embedded-font footprint, signing adds the cryptographic layer with the timestamp and validation material; the HarfBuzz repacker engages through uharfbuzz when present.
- Auto: subsetting folds the used glyph set through `Subsetter.subset`; instancing folds the axis-pin map through `instancer.instantiateVariableFont`; signing folds the signer credential and the PAdES level through the pyhanko signing pipeline with the timestamp authority and the validation-info embedding.
- Receipt: each close contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Pdf` carrying the content key, the post-subset byte count, and the page count.
- Packages: `fonttools` (`subset.Subsetter`/`varLib.instancer`), `pyhanko` (PAdES `B-B`/`B-T`/`B-LT`/`B-LTA` signing, PKCS#11), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new conformance step is one `ConformStep` row plus one acceptor arm; a new PAdES level is a signing parameter; zero new surface.
- Boundary: no PDF authoring (that stays at the document axis); pyhanko does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF, so structural conformance is authored upstream at the document axis and signing only adds the cryptographic layer; the owner closes an already-emitted PDF, never producing one.

```python signature
from enum import StrEnum
from typing import assert_never

from fontTools import subset
from fontTools.varLib import instancer
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
```

## [3]-[RESEARCH]

- [CONFORM_SPELLINGS]: the fontTools `subset.Subsetter`/`varLib.instancer.instantiateVariableFont` spellings and the pyhanko PAdES `B-B`/`B-T`/`B-LT`/`B-LTA` signing-pipeline, timestamp-authority, and PKCS#11 member spellings verify against folder `.api` catalogues authored for `fonttools` and `pyhanko`; the uharfbuzz repacker engagement and the validation-info embedding confirm on the cp315 floor.
