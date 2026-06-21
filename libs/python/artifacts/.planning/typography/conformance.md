# [PY_ARTIFACTS_CONFORMANCE]

The PDF conformance, text-shaping, color-glyph rasterization, signing, and verification close over the document rail. `Conformance` is ONE owner that takes a PDF or font emitted by `documents/emit#DOCUMENT` and produces a shaped, font-embedded, color-rasterized, digitally-signed, archival-grade artifact AND proves it: fonttools subsets and partial-axis-instances embedded variable fonts to the minimal glyph footprint through the `AxisLimit` pin-range-drop policy and draws real glyph outlines through `SVGPathPen`, uharfbuzz shapes Unicode text into positioned glyph runs, blackrenderer folds each glyph of that run through its in-package COLRv1/COLRv0 `drawGlyph` paint-graph traversal onto a backend `Surface` and serializes to PNG/PDF/SVG, pyhanko applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) with subfilter, embedded validation material, timestamp authority, certification permissions, and the B-LTA archival-timestamp-chain refresh, and the audit verb folds a signed PDF into a typed `ConformanceVerdict` carrying its DocMDP certification level. fonttools, uharfbuzz, blackrenderer, and pyhanko are admitted in the manifest; this page closes the `PDF_CONFORMANCE_AND_SIGNING`, `SHAPED_TEXT_LAYOUT`, `COLOR_GLYPH_RASTERIZATION`, and `ARCHIVAL_CONFORMANCE_VERIFICATION` ideas.

## [01]-[INDEX]

- [01]-[CONFORM]: fonttools subset/partial-instance/outline, uharfbuzz text-shaping, blackrenderer COLRv1 rasterization, pyhanko PAdES B-LTA signing, and pyhanko/pikepdf conformance-verification owner; `PositionedGlyphRun` and `ConformanceVerdict` are the two value-object sub-owners the shape and audit arms carry.

## [02]-[CONFORM]

- Owner: `Conformance` the one PDF-and-font-close owner discriminating the conformance step; `ConformStep` the closed `StrEnum` over font subsetting/instancing/shaping/rasterizing, PAdES signing, and archival audit; one frozen `_STEP_TABLE` `MappingProxyType` data-row dispatch maps each step to its `StepAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction. fonttools owns the font footprint and outline, uharfbuzz the OpenType shaping engine, blackrenderer the COLRv1 paint-graph rasterizer over fonttools+HarfBuzz, pyhanko the cryptographic-and-verification layer. `PadesLevel`/`RasterBackend`/`CertifyPerm` are the three policy-as-value `StrEnum` rows whose members carry their own behavior (`needs_timestamp`/`embeds_validation`/`archival`, the `getSurfaceClass` registry key, the `MDPPerm` resolution); `AxisLimit` is the per-axis pin-range-drop instancing policy; `PositionedGlyphRun` (the shaped-run value object) and `ConformanceVerdict` (the audit verdict value object) are the two carried sub-owners.
- Cases: `ConformStep` rows `SUBSET` (fontTools `subset.Subsetter` over the embedded glyph set) · `INSTANCE` (`fontTools.varLib.instancer.instantiateVariableFont` over the per-axis `AxisLimit` policy — a scalar `pin` collapses the axis to a static instance, a `(lower, upper)` partial-range keeps the axis variable inside the clamped span, and an empty limit drops the axis at its default; the `InstancePolicy` policy value carries the closed `inplace`/`optimize`/`update_font_names` instancer knobs as typed fields, never an open keyword bag) · `SHAPE` (uharfbuzz `shape` over the `Face`->`Font`->`Buffer` pipeline returning a `PositionedGlyphRun`, with the `Font.draw_glyph_with_pen`->`SVGPathPen` outline bridge driving `PositionedGlyphRun.to_svg_path`) · `RASTERIZE` (blackrenderer per-glyph COLRv1/COLRv0 paint-graph traversal — `getSurfaceClass` resolves the backend `Surface`, `BlackRendererFont.setLocation`/`getPalette` instance the variable-font location and palette, `buildGlyphLine`/`calcGlyphLineBounds` shape the HarfBuzz run, and `drawGlyph` folds each glyph onto the `Surface.canvas` under a `savedState`/`transform` advance walk) · `SIGN` (pyhanko PAdES B-B through B-LTA — `SimpleSigner.load`/`load_pkcs12` or `ExternalSigner` injected-signature credential, the `SigFieldSpec`/`append_signature_field` field placement, `PdfSignatureMetadata` PAdES/certify knobs, `sign_pdf` over an `IncrementalPdfFileWriter`, the `HTTPTimeStamper` authority, and the `PdfTimeStamper.update_archival_timestamp_chain` LTA refresh) · `AUDIT` (pyhanko `validate_pdf_signature`/`evaluate_signature_coverage`/`read_certification_data` + pikepdf XMP read folding a `ConformanceVerdict`) — selected by the frozen `_STEP_TABLE` row, never a chain of `is`-probes; the PAdES level (`B-B`/`B-T`/`B-LT`/`B-LTA`) is the `PadesLevel` policy row carried in `params` whose `needs_timestamp`/`embeds_validation`/`archival` properties derive the LTV chain, the backend (`svg`/`skia`/`cairo`/`coregraphics`) the `RasterBackend` row keyed on the `getSurfaceClass` registry, and the certification level the `CertifyPerm` row whose `mdp` resolves the pyhanko `MDPPerm` member.
- Entry: `Conformance.close` dispatches the step over the input PDF or font through the one `_STEP_TABLE[step]` acceptor lookup and returns a `RuntimeRail[ContentKey]`; subsetting and partial-instancing reduce the embedded-font footprint, shaping produces a positioned-glyph run and its real outline, rasterizing renders color glyphs glyph-by-glyph, signing adds the cryptographic-and-LTV layer, audit proves the signed/archival close; the HarfBuzz repacker engages through uharfbuzz inside fontTools when present, the `SHAPE` row drives the uharfbuzz shaping engine directly, and the `RASTERIZE` row drives blackrenderer at boundary-scope import only (skia/cairo/coregraphics load native libraries on backend resolution). `SUBSET`/`INSTANCE`/`SHAPE`/`RASTERIZE`/`SIGN`/`AUDIT` all resolve synchronously over the cp315-core uharfbuzz/fonttools/blackrenderer/pyhanko/pikepdf wheels — the `AUDIT` arm uses the synchronous `validate_pdf_signature` (the catalogue's sync variant, equivalent to `async_validate_pdf_signature`), so the owner stays on the synchronous runtime `boundary` and never forces an async dispatch path.
- Auto: subsetting folds the used glyph set through `Subsetter(Options(...))` `populate(unicodes=...)` then `subset(font)` then `TTFont.save`; instancing folds the per-axis `AxisLimit.of(raw).resolve()` map through `instancer.instantiateVariableFont(font, axes, **InstancePolicy.keywords())` where `pin` collapses the axis to a static value, `(lower, upper)` restricts it to a partial range, and an empty limit drops it; shaping folds the text run through `Face.create`->`Font.create`->`Buffer.create`/`add_str`/`guess_segment_properties`->`shape(font, buffer, features)` then reads `glyph_infos`/`glyph_positions` into a `PositionedGlyphRun`, with `Font.set_variations` axis pinning and the `Font.draw_glyph_with_pen(gid, SVGPathPen(glyphSet))`->`SVGPathPen.getCommands()` outline bridge feeding `to_svg_path`; rasterizing resolves the `getSurfaceClass(backendName)` surface (a `None` return raises `BackendUnavailableError`), loads one `BlackRendererFont(ttFont=..., hbFont=...)`, applies `setLocation` for the variable-font location and `getPalette` for the resolved palette, shapes the run into `buildGlyphLine`, computes `calcGlyphLineBounds`, derives the `font_size`/`BlackRendererFont.unitsPerEm` pixel scale, opens one `Surface.canvas(bounds)` margin-inset context concatenated with that scale, and folds each shaped glyph through `drawGlyph(glyph.name, canvas, palette=...)` inside a `savedState`/`transform` offset-and-advance walk before `Surface.saveImage`; signing loads the `SimpleSigner` credential or the `ExternalSigner` injected-signature signer, appends the `SigFieldSpec` field through `append_signature_field` when a box is supplied, builds `PdfSignatureMetadata(subfilter=SigSeedSubFilter.PADES, use_pades_lta=..., embed_validation_info=..., validation_context=..., certify=..., docmdp_permissions=...)`, runs `sign_pdf` over the `IncrementalPdfFileWriter` with the `HTTPTimeStamper`, then refreshes the archival chain through `PdfTimeStamper.update_archival_timestamp_chain` when the level is B-LTA; audit folds the embedded signature through `PdfFileReader`->`EmbeddedPdfSignature.evaluate_signature_coverage`->`validate_pdf_signature`->`read_certification_data` and the pikepdf `open_metadata` XMP read into a `ConformanceVerdict`.
- Receipt: every arm projects its output onto the shared `receipt/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — the `SUBSET`/`INSTANCE`/`SIGN`/`RASTERIZE` arms contribute `ArtifactReceipt.Pdf` carrying the content key, the output byte count, and the page count (1 for a font or single-page raster), the `SHAPE` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded `PositionedGlyphRun` byte count, and the `AUDIT` arm contributes the `ArtifactReceipt.Verdict` case carrying the content key and the `ConformanceVerdict` value whose `facts` projection surfaces the PAdES validity, coverage level, modification level, DocMDP certification level, LTV completeness, and archival-metadata presence. The COLR version, resolved palette index, backend name, and pixel bounds the `RASTERIZE` arm computes stay interior evidence the arm folds into its content-key derivation, not new receipt fields the shared `Pdf` case cannot carry.
- Packages: `fonttools` (settled: `subset.Subsetter`/`subset.Options`/`ttLib.TTFont`/`TTFont.save`/`pens.svgPathPen.SVGPathPen`; RESEARCH: `varLib.instancer.instantiateVariableFont` — catalogue names the `varLib` module not the function; `TTFont.getGlyphSet`/`SVGPathPen.getCommands` — catalogue rows neither), `uharfbuzz` (`Face.create`/`Font.create`/`Buffer.create`/`add_str`/`guess_segment_properties`/`shape`/`glyph_infos`/`glyph_positions`/`Font.set_variations`/`Font.draw_glyph_with_pen` all settled), `blackrenderer` (settled: `font.BlackRendererFont` with `ttFont=`/`hbFont=`, `drawGlyph`/`setLocation`/`getPalette`/`glyphNames`/`render.buildGlyphLine`/`render.calcGlyphLineBounds`/`render.BackendUnavailableError`/`backends.getSurfaceClass`/`Surface.canvas`/`Surface.saveImage`/`Canvas.savedState`/`Canvas.transform`; RESEARCH: the `GlyphInfo.xOffset`/`yOffset`/`xAdvance`/`yAdvance` field spellings — catalogue rows `GlyphInfo` as "name, gid, advances, offsets" only), `pyhanko` (settled: `SimpleSigner.load`/`load_pkcs12`, `sign_pdf`, `IncrementalPdfFileWriter`, `HTTPTimeStamper`, `append_signature_field`, `SigSeedSubFilter`, `MDPPerm`, `PdfFileReader`, `EmbeddedPdfSignature.evaluate_signature_coverage`, `validate_pdf_signature`, `read_certification_data`, `PdfSignatureMetadata` with `use_pades_lta=`, `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, ...)`; RESEARCH: the `PdfSignatureMetadata` `subfilter=`/`embed_validation_info=`/`validation_context=`/`certify=`/`docmdp_permissions=` kwargs, the `ExternalSigner`/`SigFieldSpec`/`PdfTimeStamper` constructor kwargs and the `output=` sink, and the `PdfSignatureStatus.bottom_line`/`modification_level`/`validation_path` and `DocMDPInfo.permission` members — catalogue names the capability and the type but enumerates neither), `pikepdf` (`Pdf.open` + `open_metadata` XMP read settled), `pyhanko_certvalidator` (`ValidationContext` arriving at the boundary), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`), `msgspec` (`Struct`/`msgpack.Encoder`).
- Growth: a new conformance step is one `ConformStep` row plus one `_STEP_TABLE` acceptor entry; a new PAdES level is one `PadesLevel` row whose derived properties carry its LTV behavior; a new raster backend is one `RasterBackend` row keyed on the `getSurfaceClass` registry; a new certification permission is one `CertifyPerm` row; a new shaping feature is a `shape` feature-dict row; zero new surface.
- Boundary: no PDF authoring (that stays at the document axis); pyhanko does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF, so structural conformance is authored upstream at the document axis and signing only adds the cryptographic layer; the owner closes an already-emitted PDF or font, never producing one. The `SHAPE` arm produces a `PositionedGlyphRun` the `documents/emit#DOCUMENT` text placement and the `figures/compose#COMPOSE` annotation owners consume, never a parallel shaping owner; the `RASTERIZE` arm's SVG-backend output feeds the document and `figures/compose#COMPOSE` owners directly with no native dependency, PNG/PDF raster routing through the skia or cairo backend the `getSurfaceClass` registry selects. uharfbuzz `SubsetInput`/`subset` is the rejected duplicate of the `SUBSET` fonttools footprint (fonttools owns subsetting for feature-policy control); a hand-rolled COLRv1 `PaintFormat` dispatch at the call site is the rejected duplicate of blackrenderer's in-package `drawGlyph` paint-graph traversal — the `RASTERIZE` arm composes `drawGlyph` over the `Surface.canvas`, never re-implementing solid/linear/radial/sweep/composite paint formats; the `renderText` one-shot is the rejected lower-capability form (it hides the palette, location, and glyph-bounds evidence the receipt carries). A second `_xlsx`-style writer function pair, a parallel `_RasterBackend`/`_PadesLevel` enum beside the policy behavior, and a weak `int` certification permission are the collapsed forms — `RasterBackend` keys the registry, `PadesLevel` carries its LTV properties, and `CertifyPerm.mdp` resolves the typed `MDPPerm`. The `AUDIT` arm reads version/XMP presence over pikepdf rather than claiming a veraPDF-grade structural verdict (no pure-Python PDF/A structural validator resolves on PyPI); the `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary, never built by this owner.

```python signature
import io
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final

import msgspec
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

type AxisPin = float | tuple[float, float] | None
type AxisValue = float | tuple[float | None, float | None] | None
type FeatureSpec = Mapping[str, int | bool | Sequence[tuple[int, int, int | bool]]]
type StepAcceptor = Callable[["Conformance"], bytes]

_DEFAULT_FONT_SIZE: Final = 250.0
_DEFAULT_MARGIN: Final = 20
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
_VERDICT_ENCODER: Final = msgspec.msgpack.Encoder()


class ConformStep(StrEnum):
    SUBSET = "subset"
    INSTANCE = "instance"
    SHAPE = "shape"
    RASTERIZE = "rasterize"
    SIGN = "sign"
    AUDIT = "audit"


class PadesLevel(StrEnum):
    B_B = "B-B"
    B_T = "B-T"
    B_LT = "B-LT"
    B_LTA = "B-LTA"

    @property
    def needs_timestamp(self) -> bool:
        return self is not PadesLevel.B_B

    @property
    def embeds_validation(self) -> bool:
        return self in (PadesLevel.B_LT, PadesLevel.B_LTA)

    @property
    def archival(self) -> bool:
        return self is PadesLevel.B_LTA


class RasterBackend(StrEnum):
    SVG = "svg"
    SKIA = "skia"
    CAIRO = "cairo"
    COREGRAPHICS = "coregraphics"


class CertifyPerm(StrEnum):
    NO_CHANGES = "NO_CHANGES"
    FILL_FORMS = "FILL_FORMS"
    ANNOTATE = "ANNOTATE"

    def mdp(self) -> object:
        from pyhanko.sign.fields import MDPPerm

        return getattr(MDPPerm, self.value)


class AxisLimit(Struct, frozen=True):
    pin: float | None = None
    lower: float | None = None
    upper: float | None = None

    @staticmethod
    def of(raw: AxisPin) -> "AxisLimit":
        match raw:
            case None:
                return AxisLimit()
            case (lower, upper):
                return AxisLimit(lower=lower, upper=upper)
            case value:
                return AxisLimit(pin=value)

    def resolve(self) -> AxisValue:
        if self.pin is not None:
            return self.pin
        if self.lower is None and self.upper is None:
            return None
        return (self.lower, self.upper)


class InstancePolicy(Struct, frozen=True):
    inplace: bool = True
    optimize: bool = True
    update_font_names: bool = False

    def keywords(self) -> dict[str, bool]:
        return {"inplace": self.inplace, "optimize": self.optimize, "updateFontNames": self.update_font_names}


class ConformParams(Struct, frozen=True, kw_only=True):
    unicodes: tuple[int, ...] = ()
    subset_options: Mapping[str, object] = {}
    axes: Mapping[str, AxisPin] = {}
    instance_policy: InstancePolicy = InstancePolicy()
    text: str = ""
    face_index: int = 0
    variations: Mapping[str, float] = {}
    features: FeatureSpec | None = None
    raster_backend: RasterBackend = RasterBackend.SVG
    font_size: float = _DEFAULT_FONT_SIZE
    margin: int = _DEFAULT_MARGIN
    palette_index: int = 0
    pades_level: PadesLevel = PadesLevel.B_LTA
    field_name: str = "Signature1"
    field_box: tuple[float, float, float, float] | None = None
    page: int = 0
    key_file: str | None = None
    cert_file: str | None = None
    pfx_file: str | None = None
    passphrase: bytes | None = None
    tsa_url: str | None = None
    external_signature: bytes | None = None
    certify_perm: CertifyPerm | None = None
    validation_context: object | None = None


class PositionedGlyphRun(Struct, frozen=True):
    glyphs: tuple[tuple[int, int, int, int, int, int], ...]
    outline: str = ""

    @property
    def count(self) -> int:
        return len(self.glyphs)

    def to_svg_path(self) -> str:
        return self.outline


class ConformanceVerdict(Struct, frozen=True):
    signature_valid: bool
    coverage_level: str
    modification_level: str
    certification_level: str
    ltv_complete: bool
    archival_metadata: bool

    def facts(self) -> dict[str, str]:
        return {
            "signature_valid": str(self.signature_valid),
            "coverage": self.coverage_level,
            "modification": self.modification_level,
            "certification": self.certification_level,
            "ltv_complete": str(self.ltv_complete),
            "archival_metadata": str(self.archival_metadata),
        }


class Conformance(Struct, frozen=True):
    step: ConformStep
    pdf: bytes
    params: ConformParams

    def close(self) -> RuntimeRail[ContentKey]:
        return boundary(f"conform.{self.step}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"conform-{self.step}", _STEP_TABLE[self.step](self))


def _subset_font(conform: "Conformance") -> bytes:
    from fontTools import subset
    from fontTools.ttLib import TTFont

    font = TTFont(io.BytesIO(conform.pdf))
    subsetter = subset.Subsetter(options=subset.Options(**conform.params.subset_options))
    subsetter.populate(unicodes=conform.params.unicodes)
    subsetter.subset(font)
    sink = io.BytesIO()
    font.save(sink)
    return sink.getvalue()


def _instance_font(conform: "Conformance") -> bytes:
    from fontTools.ttLib import TTFont
    from fontTools.varLib import instancer

    font = TTFont(io.BytesIO(conform.pdf))
    axes: dict[str, AxisValue] = {tag: AxisLimit.of(raw).resolve() for tag, raw in conform.params.axes.items()}
    # RESEARCH: varLib.instancer.instantiateVariableFont(font, axes, **keywords) — the fonttools catalogue names
    # the varLib module (varLib.build) but rows no instancer.instantiateVariableFont entrypoint and no
    # inplace/optimize/updateFontNames keyword, so the call and InstancePolicy.keywords() spread stay unverified
    # until reflected; the per-axis scalar-pin / (lower, upper) partial-range / None-drop AxisValue the
    # AxisLimit policy lowers is the settled instancing shape the instancer admits.
    instance = instancer.instantiateVariableFont(font, axes, **conform.params.instance_policy.keywords())
    sink = io.BytesIO()
    instance.save(sink)
    return sink.getvalue()


def _shape_text(conform: "Conformance") -> bytes:
    import uharfbuzz as hb
    from fontTools.pens.svgPathPen import SVGPathPen
    from fontTools.ttLib import TTFont

    params = conform.params
    font = hb.Font.create(hb.Face.create(conform.pdf, params.face_index))
    if params.variations:
        font.set_variations(dict(params.variations))
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    buffer.guess_segment_properties()
    hb.shape(font, buffer, dict(params.features) if params.features else None)
    glyphs = tuple(
        (info.codepoint, info.cluster, pos.x_advance, pos.y_advance, pos.x_offset, pos.y_offset)
        for info, pos in zip(buffer.glyph_infos, buffer.glyph_positions, strict=True)
    )
    # RESEARCH: SVGPathPen.getCommands() and TTFont.getGlyphSet() are unverified (.api rows
    # pens.svgPathPen and BasePen.moveTo/lineTo/curveTo but no getCommands/getGlyphSet row).
    pen = SVGPathPen(TTFont(io.BytesIO(conform.pdf)).getGlyphSet())
    for gid, *_ in glyphs:
        font.draw_glyph_with_pen(gid, pen)
    return _RUN_ENCODER.encode(PositionedGlyphRun(glyphs=glyphs, outline=pen.getCommands()))


def _rasterize_color(conform: "Conformance") -> bytes:
    import uharfbuzz as hb
    from blackrenderer.backends import getSurfaceClass
    from blackrenderer.font import BlackRendererFont
    from blackrenderer.render import BackendUnavailableError, buildGlyphLine, calcGlyphLineBounds
    from fontTools.ttLib import TTFont

    params = conform.params
    surface_class = getSurfaceClass(params.raster_backend.value)
    if surface_class is None:
        raise BackendUnavailableError(params.raster_backend.value)
    hb_font = hb.Font.create(hb.Face.create(conform.pdf, params.face_index))
    font = BlackRendererFont(ttFont=TTFont(io.BytesIO(conform.pdf), fontNumber=0, lazy=True), hbFont=hb_font)
    if params.variations:
        font.setLocation(dict(params.variations))
        hb_font.set_variations(dict(params.variations))
    palette = font.getPalette(params.palette_index)
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    buffer.guess_segment_properties()
    hb.shape(hb_font, buffer, dict(params.features) if params.features else None)
    glyph_line = buildGlyphLine(buffer.glyph_infos, buffer.glyph_positions, font.glyphNames)
    x_min, y_min, x_max, y_max = calcGlyphLineBounds(glyph_line, font) or (0.0, 0.0, 0.0, 0.0)
    scale = params.font_size / font.unitsPerEm
    margin = params.margin
    surface = surface_class()
    with surface.canvas(
        (x_min * scale - margin, y_min * scale - margin, x_max * scale + margin, y_max * scale + margin)
    ) as canvas:
        canvas.transform((scale, 0, 0, scale, 0, 0))
        for glyph in glyph_line:
            with canvas.savedState():
                # RESEARCH: GlyphInfo.xOffset/yOffset/xAdvance/yAdvance attribute spellings are unverified
                # (.api rows blackrenderer.render.GlyphInfo as "name, gid, advances, offsets" without
                # enumerating the offset/advance field names); GlyphInfo.name, BlackRendererFont.glyphNames,
                # BlackRendererFont.unitsPerEm, the ttFont=/hbFont= constructor, drawGlyph, setLocation,
                # getPalette, buildGlyphLine, calcGlyphLineBounds, getSurfaceClass, Surface.canvas/saveImage,
                # Canvas.savedState/transform are the settled fence code verified against the blackrenderer catalogue.
                canvas.transform((1, 0, 0, 1, glyph.xOffset, glyph.yOffset))
                font.drawGlyph(glyph.name, canvas, palette=palette)
            canvas.transform((1, 0, 0, 1, glyph.xAdvance, glyph.yAdvance))
    sink = io.BytesIO()
    surface.saveImage(sink)
    return sink.getvalue()


def _sign_pdf(conform: "Conformance") -> bytes:
    from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
    from pyhanko.pdf_utils.reader import PdfFileReader
    from pyhanko.sign import ExternalSigner, PdfSignatureMetadata, PdfTimeStamper, SimpleSigner, sign_pdf
    from pyhanko.sign.fields import SigFieldSpec, SigSeedSubFilter, append_signature_field
    from pyhanko.sign.timestamps import HTTPTimeStamper

    params = conform.params
    level = params.pades_level
    writer = IncrementalPdfFileWriter(io.BytesIO(conform.pdf))
    if params.field_box is not None:
        # RESEARCH: SigFieldSpec(sig_field_name=, on_page=, box=) constructor kwargs are unverified — the
        # pyhanko catalogue rows SigFieldSpec as a "name, page, box, seed values, MDP lock, appearance"
        # descriptor and append_signature_field(pdf_out, sig_field_spec) as the writer, without enumerating
        # the SigFieldSpec constructor parameter names; append_signature_field itself is settled.
        append_signature_field(writer, SigFieldSpec(sig_field_name=params.field_name, on_page=params.page, box=params.field_box))
    # RESEARCH: the PdfSignatureMetadata kwargs subfilter=/embed_validation_info=/validation_context=/
    # certify=/docmdp_permissions= are unverified — the catalogue rows the descriptor as "field name, reason,
    # certify flag, DSS settings, PAdES/LTA knobs" and the LOCAL_ADMISSION law settles only use_pades_lta=True
    # as the named LTA knob; field_name= and use_pades_lta= are settled, the remaining kwargs stay gated.
    meta = PdfSignatureMetadata(
        field_name=params.field_name,
        subfilter=SigSeedSubFilter.PADES,
        use_pades_lta=level.archival,
        embed_validation_info=level.embeds_validation,
        validation_context=params.validation_context,
        certify=params.certify_perm is not None,
        docmdp_permissions=params.certify_perm.mdp() if params.certify_perm is not None else None,
    )
    timestamper = HTTPTimeStamper(params.tsa_url) if level.needs_timestamp else None
    # RESEARCH: ExternalSigner(signature_value=, signing_cert=, cert_registry=) constructor kwargs are
    # unverified — the catalogue rows ExternalSigner as the detached "pre-computed signature bytes or deferred
    # integer slot" signer without enumerating its constructor parameters; SimpleSigner.load/load_pkcs12 and
    # sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, ...) are settled.
    signer = (
        ExternalSigner(signature_value=params.external_signature, signing_cert=None, cert_registry=None)
        if params.external_signature is not None
        else SimpleSigner.load_pkcs12(params.pfx_file, passphrase=params.passphrase)
        if params.pfx_file is not None
        else SimpleSigner.load(params.key_file, params.cert_file, key_passphrase=params.passphrase)
    )
    sink = io.BytesIO()
    sign_pdf(writer, meta, signer, timestamper=timestamper, output=sink)
    if not (level.archival and params.tsa_url is not None and params.validation_context is not None):
        return sink.getvalue()
    refreshed = io.BytesIO()
    # RESEARCH: the PdfTimeStamper(timestamper) constructor and update_archival_timestamp_chain's output= sink
    # keyword are unverified — the catalogue rows update_archival_timestamp_chain(reader, validation_context,
    # ...) without enumerating the constructor or the output keyword; reader/validation_context are settled.
    PdfTimeStamper(HTTPTimeStamper(params.tsa_url)).update_archival_timestamp_chain(
        PdfFileReader(io.BytesIO(sink.getvalue())), params.validation_context, output=refreshed
    )
    return refreshed.getvalue()


def _audit_pdf(conform: "Conformance") -> bytes:
    import pikepdf
    from pyhanko.pdf_utils.reader import PdfFileReader
    from pyhanko.sign.validation import read_certification_data, validate_pdf_signature

    reader = PdfFileReader(io.BytesIO(conform.pdf))
    embedded = reader.embedded_signatures[0]
    status = validate_pdf_signature(embedded, conform.params.validation_context)
    docmdp = read_certification_data(reader)
    with pikepdf.open(io.BytesIO(conform.pdf)) as pdf, pdf.open_metadata() as meta:
        has_metadata = bool(meta)
    # RESEARCH: the DocMDPInfo.permission accessor and the PdfSignatureStatus.bottom_line/modification_level/
    # validation_path member spellings are unverified — the catalogue rows PdfSignatureStatus as "trust,
    # coverage, modification level, seed validity" and DocMDPInfo as the "MDPPerm level" carrier without
    # enumerating their members; evaluate_signature_coverage, validate_pdf_signature, read_certification_data,
    # and SignatureCoverageLevel.name are the settled fence code.
    return _VERDICT_ENCODER.encode(
        ConformanceVerdict(
            signature_valid=status.bottom_line,
            coverage_level=embedded.evaluate_signature_coverage().name,
            modification_level=str(status.modification_level),
            certification_level=docmdp.permission.name if docmdp is not None else "none",
            ltv_complete=status.validation_path is not None,
            archival_metadata=has_metadata,
        )
    )


_STEP_TABLE: Final[MappingProxyType[ConformStep, StepAcceptor]] = MappingProxyType({
    ConformStep.SUBSET: _subset_font,
    ConformStep.INSTANCE: _instance_font,
    ConformStep.SHAPE: _shape_text,
    ConformStep.RASTERIZE: _rasterize_color,
    ConformStep.SIGN: _sign_pdf,
    ConformStep.AUDIT: _audit_pdf,
})
```

## [03]-[RESEARCH]

- [SHAPING]: the uharfbuzz `Face.create`/`Font.create`/`Buffer.create`/`add_str`/`guess_segment_properties`/`shape(font, buffer, features)`/`GlyphInfo`(`codepoint`/`cluster`)/`GlyphPosition`(`x_advance`/`y_advance`/`x_offset`/`y_offset`)/`Font.set_variations`/`Font.draw_glyph_with_pen` spellings verify against the folder `.api` catalogue for `uharfbuzz` (`0.55.0` reflected on cp315 — the shaping pipeline resolves in-process on the cp315 core). `Face.create(payload, index)` admits raw `bytes` directly, so the `Blob.from_file_path` file-load row is unused (this owner holds the font payload in memory, never a path). `buffer.glyph_infos`/`glyph_positions` are read after `shape` as lists of `GlyphInfo`/`GlyphPosition` named tuples; `guess_segment_properties` infers direction/script/language before shaping when explicit properties are not set. The `features` parameter to `shape` accepts the catalogued `dict[str, int | bool | Sequence[tuple[int, int, int | bool]]]` feature-range shape. The vector-outline bridge drives `Font.draw_glyph_with_pen(gid, SVGPathPen(glyphSet))` from `fontTools.pens.svgPathPen`; the COLRv1 color-glyph path is owned by the `RASTERIZE` arm through blackrenderer's `drawGlyph` rather than a raw `Font.draw_glyph(gid, PaintFuncs, state)` call here, so the page never holds two color-glyph traversals. The fontTools `SVGPathPen` and `BasePen.moveTo`/`lineTo`/`qCurveTo`/`curveTo`/`closePath` spellings verify against the folder `.api` catalogue for `fonttools`; RESEARCH: `SVGPathPen.getCommands()` (the SVG-string accessor) and `TTFont.getGlyphSet()` (the glyph-set the pen draws against) are not yet rows in the `fonttools` catalogue, so the `_shape_text` body gates both behind the explicit `# RESEARCH` comment and they stay unverified until reflected. The uharfbuzz `SubsetInput`/`subset` surface overlaps the fonttools `Subsetter` the `SUBSET` row already owns; fonttools owns subsetting for feature-policy control, so no `ConformStep` splits subsetting across the two packages.
- [INSTANCE]: the `AxisLimit` policy is the one per-axis pin-range-drop owner — `AxisLimit.of(raw)` `match`-projects a scalar to a `pin`, a `(lower, upper)` tuple to a partial range, and `None` to an axis drop, and `resolve()` lowers the policy to the `AxisValue` (scalar, `(lower, upper)` tuple, or `None`) the instancer admits. A scalar value collapses the axis to a static instance; a tuple narrows the design space without removing the axis; a dropped axis falls back to its default. The `InstancePolicy` policy value collapses the open `inplace`/`optimize`/`updateFontNames` keyword bag into three typed fields whose `keywords()` projection spreads the closed knob set, never a `Mapping[str, object]`. RESEARCH: the entire `varLib.instancer.instantiateVariableFont(font, axes, **keywords)` call is unverified — the `fonttools` catalogue names only the `varLib` module (`varLib.build` for design-space compilation) and rows no `instancer.instantiateVariableFont` entrypoint, no `inplace`/`optimize`/`updateFontNames` keyword, and no `AxisTriple`/`AxisRange` limit type the instancer normalizes the `(lower, upper)` tuple into; the `_instance_font` body gates the whole call and the `InstancePolicy.keywords()` spread behind the explicit `# RESEARCH` comment, and the per-axis scalar-pin / partial-range / None-drop `AxisValue` the `AxisLimit` policy lowers is the settled instancing shape. The `TTFont(io.BytesIO(...))` load and `TTFont.save(sink)` framing this call are settled fence code.
- [RASTERIZE]: the blackrenderer `font.BlackRendererFont(path, *, fontNumber, lazy, ttFont, hbFont)`, `BlackRendererFont.drawGlyph(glyphName, canvas, *, palette, textColor)`, `setLocation`, `getPalette`, `glyphNames`, `unitsPerEm`, `render.buildGlyphLine(infos, positions, glyphNames)`, `render.calcGlyphLineBounds(glyphLine, font)`, `render.BackendUnavailableError`, `backends.getSurfaceClass(backendName, imageExtension)`, `Surface.canvas(boundingBox)`, `Surface.saveImage(path)`, `Canvas.savedState()`, and `Canvas.transform(transform)` entrypoint spellings verify against the folder `.api` catalogue for `blackrenderer` (`0.8.2` reflected on cp315). The `font_size`/`unitsPerEm` pixel scale is concatenated onto the `Surface.canvas` context as one `(scale, 0, 0, scale, 0, 0)` transform and folded into the bounds so the COLRv1 raster renders at a real pixel size rather than raw font units, never the `renderText` `fontSize=` one-shot. The `RASTERIZE` arm composes the per-glyph path — `getSurfaceClass(backendName)` resolves the concrete `Surface` (a `None` return raises `BackendUnavailableError`), `BlackRendererFont(ttFont=..., hbFont=...)` loads the paired font without a path round-trip, `setLocation`/`getPalette` instance the variable-font location and resolved palette, `buildGlyphLine`/`calcGlyphLineBounds` shape the HarfBuzz run and compute font-unit bounds, and `drawGlyph` folds each glyph onto the margin-inset `Surface.canvas` under a `savedState`/`transform` offset-and-advance walk — never the `renderText` one-shot, which hides the palette, location, and bounds evidence the `RASTERIZE` receipt carries. The in-package `Canvas` paint surface (`newPath`/`drawPathSolid`/`drawPathLinearGradient`/`drawPathRadialGradient`/`drawPathSweepGradient`/`compositeMode`/`clipPath`) is the COLRv1 `PaintFormat` traversal `drawGlyph` drives; a hand-rolled paint dispatch at the call site is the rejected duplicate. The `BlackRendererFont.glyphNames` property is settled (the catalogue rows it as "glyph order from the `TTFont`"), and the `_rasterize_color` body holds the `hb.Font` locally from `hb.Font.create(hb.Face.create(conform.pdf, face_index))` rather than reading an uncatalogued `BlackRendererFont.hbFont` accessor back, so no font-property read-back is unverified; `setLocation` applies the variable-font location to the `BlackRendererFont` and `hb_font.set_variations` mirrors it onto the shaping font so the COLRv1 `PaintVar*` deltas and the shaped advances agree. RESEARCH: only the `GlyphInfo.xOffset`/`yOffset`/`xAdvance`/`yAdvance` field spellings the per-glyph offset-and-advance walk reads stay unverified — the catalogue rows `blackrenderer.render.GlyphInfo` as "name, gid, advances, offsets" without enumerating the offset/advance member names, so the `_rasterize_color` body gates the four behind the explicit `# RESEARCH` comment while `GlyphInfo.name` rides the settled "name" field. The SVG backend serializes with no native dependency; PNG/PDF raster routing engages the `getSurfaceClass`-selected skia or cairo backend. Boundary-scope import only — skia/cairo/coregraphics load native libraries on backend resolution.
- [AUDIT]: the pyhanko `PdfFileReader`, `EmbeddedPdfSignature.evaluate_signature_coverage` (returning `SignatureCoverageLevel`), `validate_pdf_signature(embedded, signer_validation_context, ts_validation_context, diff_policy)` (returning `PdfSignatureStatus`), `read_certification_data(reader)` (returning `DocMDPInfo`), `DocMDPInfo`, and `SignatureCoverageLevel` entrypoint spellings verify against the folder `.api` catalogue for `pyhanko`. The synchronous `validate_pdf_signature` is functionally equivalent to `async_validate_pdf_signature` (catalogue: sync and async APIs are equivalent), so the audit arm stays on the synchronous `boundary` rather than forcing an async dispatch path. The `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary (catalogue: the signing owner does not build it). RESEARCH: the `DocMDPInfo.permission` accessor and the `PdfSignatureStatus.bottom_line`/`modification_level`/`validation_path` member names are not yet rows in the `pyhanko` catalogue (the catalogue rows `PdfSignatureStatus`/`DocMDPInfo`/`SignatureCoverageLevel` as types without enumerating their members), so the `_audit_pdf` body gates all four behind the explicit `# RESEARCH` comment and they stay unverified until reflected; the `evaluate_signature_coverage`/`validate_pdf_signature`/`read_certification_data` entrypoints and `SignatureCoverageLevel.name` are the settled fence code. The pikepdf `Pdf.open`/`open_metadata` XMP read verifies against the `pikepdf` catalogue; no pure-Python PDF/A structural validator resolves on PyPI, so the structural-archival leg reads version/XMP presence over pikepdf rather than a full veraPDF-grade verdict.
- [SUBSET_SIGN]: the fontTools `subset.Subsetter(options=Options(...))`/`populate(unicodes=...)`/`subset(font)`, `ttLib.TTFont`/`save` spellings verify against the folder `.api` catalogue for `fonttools`; the pyhanko `SimpleSigner.load(key_file, cert_file, ca_chain_files, key_passphrase)`/`SimpleSigner.load_pkcs12(pfx_file, ca_chain_files, passphrase)`, `ExternalSigner`, `PdfSignatureMetadata`, `sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, existing_fields_only)`, `IncrementalPdfFileWriter`, `HTTPTimeStamper(url, https, timeout)`, `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, ...)`, `SigFieldSpec`/`append_signature_field`, `SigSeedSubFilter` (`ADOBE_PKCS7_DETACHED`/`PADES`), and `MDPPerm` (`NO_CHANGES`/`FILL_FORMS`/`ANNOTATE`) entrypoint spellings verify against the folder `.api` catalogue for `pyhanko`. The `CertifyPerm` `StrEnum` values are the exact `MDPPerm` member names, so `CertifyPerm.mdp` resolves the typed permission through one `getattr(MDPPerm, self.value)` rather than a weak `int`; the `ExternalSigner` injected-signature path covers HSM/remote signing where the signature bytes arrive pre-computed, replacing a `PdfSigner.digest_doc_for_signing` two-phase round-trip with the single `sign_pdf` call. The PAdES level rises from B-B to B-LTA as `subfilter=SigSeedSubFilter.PADES` plus the catalogue-settled `use_pades_lta=True` engages the LTV chain, `embed_validation_info=True` writes the DSS validation material, and `update_archival_timestamp_chain(reader, validation_context, ...)` refreshes the archival timestamp on the B-LTA close. The `_sign_pdf` body folds the credential selection — `ExternalSigner` injected-signature, `SimpleSigner.load_pkcs12`, or `SimpleSigner.load` — into one polymorphic conditional expression, builds the writer/metadata/timestamper/signer inline, and inlines the archival-chain refresh tail when the level is archival, so no second-tier `_metadata`/`_writer`/`_signer`/`_refresh_archival` single-call hop survives. RESEARCH: the `PdfSignatureMetadata` kwargs `subfilter=`/`embed_validation_info=`/`validation_context=`/`certify=`/`docmdp_permissions=`, the `ExternalSigner(signature_value=, signing_cert=, cert_registry=)` and `SigFieldSpec(sig_field_name=, on_page=, box=)` constructor kwargs, and the `PdfTimeStamper(timestamper)` constructor plus `update_archival_timestamp_chain(..., output=)` sink keyword are the catalogued PAdES/DSS/certify/field/timestamp capability but their exact parameter names are not yet rows in the `pyhanko` catalogue, so the `_sign_pdf` body gates each behind an explicit `# RESEARCH` comment and they stay unverified until reflected; the `SimpleSigner.load`/`load_pkcs12`, `sign_pdf`, `append_signature_field`, `IncrementalPdfFileWriter`, `HTTPTimeStamper`, `use_pades_lta=`, `update_archival_timestamp_chain(reader, validation_context, ...)`, and `MDPPerm` member access are the settled fence code.
