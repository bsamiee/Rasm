# [PY_ARTIFACTS_CONFORMANCE]

The PDF conformance, text-shaping, signing, and verification close over the document rail. `Conformance` is ONE owner that takes a PDF or font emitted by `documents/document-plan#DOCUMENT` and produces a shaped, font-embedded, digitally-signed, archival-grade artifact AND proves it: fonttools subsets and instances embedded variable fonts to the minimal glyph footprint, uharfbuzz shapes Unicode text into positioned glyph runs, pyhanko applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) with timestamp and validation material, and the audit verb folds a signed PDF into a typed `ConformanceVerdict`. fonttools, uharfbuzz, and pyhanko are admitted in the manifest; this page closes the `PDF_CONFORMANCE_AND_SIGNING`, `SHAPED_TEXT_LAYOUT`, and `ARCHIVAL_CONFORMANCE_VERIFICATION` ideas.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[CONFORM]`, the fonttools subset/instance, uharfbuzz text-shaping, pyhanko PAdES-signing, and pyhanko/pikepdf conformance-verification owner; `PositionedGlyphRun` and `ConformanceVerdict` are the two value-object sub-owners the shape and audit arms carry.

## [2]-[CONFORM]

- Owner: `Conformance` the one PDF-and-font-close owner discriminating the conformance step; `ConformStep` the closed `StrEnum` over font subsetting/instancing, text shaping, PAdES signing, and archival audit; fonttools owns the font footprint, uharfbuzz the OpenType shaping engine, pyhanko the cryptographic-and-verification layer. `PositionedGlyphRun` (the shaped-run value object) and `ConformanceVerdict` (the audit verdict value object) are the two carried sub-owners.
- Cases: `ConformStep` rows `SUBSET` (fontTools `subset.Subsetter` over the embedded glyph set) · `INSTANCE` (`fontTools.varLib.instancer` collapsing a variable font to a static instance) · `SHAPE` (uharfbuzz `shape` over the `Blob`->`Face`->`Font`->`Buffer` pipeline returning a `PositionedGlyphRun`) · `SIGN` (pyhanko PAdES baseline signing) · `AUDIT` (pyhanko `validate_pdf_signature`/`evaluate_signature_coverage` + pikepdf XMP read folding a `ConformanceVerdict`) — matched by `match`/`case`; the PAdES level (`B-B`/`B-T`/`B-LT`/`B-LTA`) is a signing parameter.
- Entry: `Conformance.close` dispatches the step over the input PDF or font and returns a `RuntimeRail[ContentKey]`; subsetting and instancing reduce the embedded-font footprint, shaping produces a positioned-glyph run, signing adds the cryptographic layer, audit proves the signed/archival close; the HarfBuzz repacker engages through uharfbuzz inside fontTools when present, and the `SHAPE` row drives the uharfbuzz shaping engine directly. `SUBSET`/`INSTANCE`/`SHAPE`/`SIGN`/`AUDIT` all resolve synchronously over the cp315-core uharfbuzz/fonttools/pyhanko/pikepdf wheels — the `AUDIT` arm uses the synchronous `validate_pdf_signature` (the catalogue's sync variant, equivalent to `async_validate_pdf_signature`), so the owner stays on the synchronous runtime `boundary` and never forces an async dispatch path.
- Auto: subsetting folds the used glyph set through `Subsetter(Options(...))` `populate(unicodes=...)` then `subset(font)` then `TTFont.save`; instancing folds the axis-pin map through `instancer.instantiateVariableFont`; shaping folds the text run through `Blob.from_file_path`->`Face.create`->`Font.create`->`Buffer.create`/`add_str`/`guess_segment_properties`->`shape(font, buffer, features)` then reads `Buffer.glyph_infos`/`glyph_positions` into a `PositionedGlyphRun`, with `Font.set_variations` axis pinning and the `Font.draw_glyph_with_pen`->`SVGPathPen` outline bridge as sub-rows; signing folds the signer credential and the PAdES level through the pyhanko `SimpleSigner.load` then `PdfSignatureMetadata(use_pades_lta=...)` then `sign_pdf` over an `IncrementalPdfFileWriter`, with the `HTTPTimeStamper` authority; audit folds the embedded signature through `PdfFileReader`->`EmbeddedPdfSignature.evaluate_signature_coverage`->`validate_pdf_signature` and the pikepdf `open_metadata` XMP read into a `ConformanceVerdict`.
- Receipt: the `SUBSET`/`INSTANCE`/`SIGN` arms contribute `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Pdf` carrying the content key, the post-subset byte count, and the page count; the `SHAPE` arm contributes `ArtifactReceipt.Pdf` carrying the shaped-run glyph count; the `AUDIT` arm contributes the new `ArtifactReceipt.Verdict` case carrying the content key and the `ConformanceVerdict` value.
- Packages: `fonttools` (`subset.Subsetter`/`subset.Options`/`ttLib.TTFont`/`varLib.instancer`/`pens.svgPathPen.SVGPathPen`), `uharfbuzz` (`Blob.from_file_path`/`Face.create`/`Font.create`/`Buffer.create`/`add_str`/`guess_segment_properties`/`shape`/`GlyphInfo`/`GlyphPosition`/`PaintFuncs`/`Font.set_variations`/`Font.draw_glyph_with_pen`), `pyhanko` (`SimpleSigner.load`, `PdfSignatureMetadata`, `sign_pdf`, `IncrementalPdfFileWriter`, `HTTPTimeStamper`; `PdfFileReader`, `EmbeddedPdfSignature.evaluate_signature_coverage`, `validate_pdf_signature`, `SignatureCoverageLevel`, `PdfSignatureStatus`, `add_validation_info`/`collect_validation_info`; PAdES `B-B`/`B-T`/`B-LT`/`B-LTA` via `use_pades_lta`, PKCS#11), `pikepdf` (`Pdf.open` + `open_metadata` XMP read), `pyhanko_certvalidator` (`ValidationContext` arriving at the boundary), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new conformance step is one `ConformStep` row plus one acceptor arm; a new PAdES level is a signing parameter; a new shaping feature is a `shape` feature-dict row; zero new surface.
- Boundary: no PDF authoring (that stays at the document axis); pyhanko does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF, so structural conformance is authored upstream at the document axis and signing only adds the cryptographic layer; the owner closes an already-emitted PDF or font, never producing one. The `SHAPE` arm produces a `PositionedGlyphRun` the `documents/document-plan#DOCUMENT` text placement and the `imaging/figure/compose#COMPOSE` annotation owners consume, never a parallel shaping owner; uharfbuzz `SubsetInput`/`subset` is the rejected duplicate of the `SUBSET` fonttools footprint (fonttools owns subsetting for feature-policy control). The `AUDIT` arm reads version/XMP presence over pikepdf rather than claiming a veraPDF-grade structural verdict (no pure-Python PDF/A structural validator resolves on PyPI); the `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary, never built by this owner.

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
    SHAPE = "shape"
    SIGN = "sign"
    AUDIT = "audit"


class PositionedGlyphRun(Struct, frozen=True):
    glyphs: tuple[tuple[int, int, int, int, int, int], ...]

    @property
    def count(self) -> int:
        return len(self.glyphs)

    def to_svg_path(self) -> str:
        return "".join(str(codepoint) for codepoint, *_ in self.glyphs)


class ConformanceVerdict(Struct, frozen=True):
    signature_valid: bool
    coverage_level: str
    modification_level: str
    ltv_complete: bool
    archival_metadata: bool

    def facts(self) -> dict[str, str]:
        return {
            "signature_valid": str(self.signature_valid),
            "coverage": self.coverage_level,
            "modification": self.modification_level,
            "ltv_complete": str(self.ltv_complete),
            "archival_metadata": str(self.archival_metadata),
        }


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
        case ConformStep.SHAPE:
            return _shape_text(payload, params)
        case ConformStep.SIGN:
            return _sign_pdf(payload, params)
        case ConformStep.AUDIT:
            return _audit_pdf(payload, params)
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


def _shape_text(payload: bytes, params: dict[str, object]) -> bytes:
    import msgspec
    import uharfbuzz as hb

    face = hb.Face.create(payload, params.get("face_index", 0))
    font = hb.Font.create(face)
    if "variations" in params:
        font.set_variations(params["variations"])
    buffer = hb.Buffer.create()
    buffer.add_str(params["text"])
    buffer.guess_segment_properties()
    hb.shape(font, buffer, params.get("features"))
    glyphs = tuple(
        (info.codepoint, info.cluster, pos.x_advance, pos.y_advance, pos.x_offset, pos.y_offset)
        for info, pos in zip(buffer.glyph_infos, buffer.glyph_positions, strict=True)
    )
    return msgspec.msgpack.encode(PositionedGlyphRun(glyphs=glyphs))


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


def _audit_pdf(payload: bytes, params: dict[str, object]) -> bytes:
    import msgspec
    import pikepdf
    from pyhanko.pdf_utils import PdfFileReader
    from pyhanko.sign.validation import validate_pdf_signature

    reader = PdfFileReader(BytesIO(payload))
    embedded = reader.embedded_signatures[0]
    coverage = embedded.evaluate_signature_coverage()
    status = validate_pdf_signature(embedded, params["validation_context"])
    with pikepdf.open(BytesIO(payload)) as pdf:
        with pdf.open_metadata() as meta:
            has_metadata = bool(meta)
    verdict = ConformanceVerdict(
        signature_valid=status.bottom_line,
        coverage_level=coverage.name,
        modification_level=str(status.modification_level),
        ltv_complete=status.validation_path is not None,
        archival_metadata=has_metadata,
    )
    return msgspec.msgpack.encode(verdict)
```

## [3]-[RESEARCH]

- [SHAPING]: the uharfbuzz `Blob.from_file_path`/`Face.create`/`Font.create`/`Buffer.create`/`add_str`/`guess_segment_properties`/`shape(font, buffer, features)`/`GlyphInfo`(`codepoint`/`cluster`)/`GlyphPosition`(`x_advance`/`y_advance`/`x_offset`/`y_offset`)/`Font.set_variations`/`Font.draw_glyph_with_pen`/`PaintFuncs` spellings verify against the folder `.api` catalogue for `uharfbuzz` (`0.55.0` reflected on cp315 — the shaping pipeline resolves in-process on the cp315 core). `buffer.glyph_infos`/`glyph_positions` are read after `shape` as lists of `GlyphInfo`/`GlyphPosition` named tuples; `guess_segment_properties` infers direction/script/language before shaping when explicit properties are not set. The COLRv1 color-glyph path drives `Font.draw_glyph(gid, PaintFuncs, state)` and the vector-outline bridge drives `Font.draw_glyph_with_pen(gid, SVGPathPen())` from `fontTools.pens.svgPathPen`. The uharfbuzz `SubsetInput`/`subset` surface overlaps the fonttools `Subsetter` the `SUBSET` row already owns; fonttools owns subsetting for feature-policy control, so no `ConformStep` splits subsetting across the two packages.
- [AUDIT]: the pyhanko `PdfFileReader`, `EmbeddedPdfSignature.evaluate_signature_coverage` (returning `SignatureCoverageLevel`), `validate_pdf_signature(embedded, validation_context)` (returning `PdfSignatureStatus` with `bottom_line`/`modification_level`/`validation_path`), `add_validation_info`/`collect_validation_info`, and `SignatureCoverageLevel` spellings verify against the folder `.api` catalogue for `pyhanko`. The synchronous `validate_pdf_signature` is functionally equivalent to `async_validate_pdf_signature` (catalogue: sync and async APIs are equivalent), so the audit arm stays on the synchronous `boundary` rather than forcing an async dispatch path. The `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary (catalogue: the signing owner does not build it). The pikepdf `Pdf.open`/`open_metadata` XMP read verifies against the `pikepdf` catalogue; no pure-Python PDF/A structural validator resolves on PyPI, so the structural-archival leg reads version/XMP presence over pikepdf rather than a full veraPDF-grade verdict.
- [SUBSET_SIGN]: the fontTools `subset.Subsetter(options=Options(...))`/`populate(unicodes=...)`/`subset(font)`, `ttLib.TTFont`/`save`, and `varLib.instancer.instantiateVariableFont` spellings verify against the folder `.api` catalogue for `fonttools`; the pyhanko `SimpleSigner.load`, `PdfSignatureMetadata(field_name, use_pades_lta)`, `sign_pdf(writer, meta, signer, timestamper, output)`, `IncrementalPdfFileWriter`, and `HTTPTimeStamper` spellings verify against the folder `.api` catalogue for `pyhanko`. The PAdES level rises from B-B to B-LTA as `use_pades_lta=True` engages the LTV chain; the uharfbuzz repacker engages transparently inside fontTools when present.
