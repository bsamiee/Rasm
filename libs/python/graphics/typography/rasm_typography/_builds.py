"""Face construction from an original: rules executed over the imported source, its UFO, the ufo2ft binary with metrics measured on native layout, hinted TrueType bytes, and WOFF2."""

# mypy: disable-error-code="no-untyped-call"

from dataclasses import dataclass
from datetime import UTC
lazy from functools import partial
lazy from io import BytesIO

from fontTools.ttLib import TTFont
import msgspec
import ufoLib2.objects
lazy from expression.extra.result import catch
lazy from fontTools import unicodedata
lazy from fontTools.otlLib.builder import buildStatTable
lazy from fontTools.otlLib.optimize.gpos import COMPRESSION_LEVEL
lazy from fontTools.ttLib.woff2 import compress
lazy from fontTools.varLib.instancer.names import ELIDABLE_AXIS_VALUE_NAME
lazy from pathops import PathOpsError
lazy from ttfautohint import TAError, ttfautohint
lazy from ufo2ft import compileTTF
lazy from ufo2ft.constants import OPENTYPE_CATEGORIES_KEY, OPENTYPE_META_KEY, TRUETYPE_INSTRUCTIONS_KEY
lazy from ufoLib2.objects.info import GaspBehavior, GaspRangeRecord

from rasm_typography._faces.glyphs import _Category, Source
lazy from rasm_records import Error, Fault, Ok, Outcome, Reason
lazy from rasm_typography._faces.declaration import Face, Faces
lazy from rasm_typography._faces.importing import imported
lazy from rasm_typography._rules.emission import _FeatureWriter
lazy from rasm_typography._rules.metrics import _measured, Metrics
lazy from rasm_typography._rules.plans import _executed, planned
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, kw_only=True)
class Construction:
    """Constructed face: its source after every drawing rule, the UFO written from it, and the ufo2ft font whose final metrics were measured on its own layout."""

    source: Source
    ufo: ufoLib2.Font
    font: TTFont

    def compiled(self) -> Outcome[bytes]:
        """Serializes the font, hinting where the face's genre selects it, equal inputs giving equal bytes.

        Returns:
            Ok(bytes), `FAILED` for refused hinting.
        """
        face, buffer = self.source.face, BytesIO()
        self.font.save(buffer)
        hinting = partial(
            ttfautohint,
            no_info=True,
            epoch=round(self.source.info.created.timestamp()),
            **({"default_script": unicodedata.script(chr(face.genre.script.stem)).lower()} if isinstance(face, Face) else {}),
        )
        return Ok(buffer.getvalue()).bind(
            lambda data: catch(exception=TAError)(hinting)(in_buffer=data).map_error(partial(Fault.of, Reason.FAILED, face.postscript)) if isinstance(face, Face) and face.genre.hinting else Ok(data)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _font(writer: _FeatureWriter, /) -> ufoLib2.Font:
    """Returns a source's UFO: glyphs in order, records, vertical metrics, family flags, gasp, the dropout program, and emitted features, fields an ornament face lacks left to ufo2ft fallbacks."""
    source = writer.source
    face, info, metrics, languages = source.face, source.info, source.metrics, list(source.scripts)
    categories = {glyph.name.text: glyph.category for glyph in source.glyphs}
    font = ufoLib2.Font(
        features=ufoLib2.objects.Features(writer.substitutions),
        info=ufoLib2.objects.Info(
            familyName=face.typographic,
            styleName=face.style,
            versionMajor=info.version[0],
            versionMinor=info.version[1],
            copyright=info.copyright,
            openTypeNameDesigner=info.designer,
            openTypeOS2VendorID=info.vendor,
            unitsPerEm=source.upm,
            ascender=metrics.ascender,
            descender=metrics.descender,
            xHeight=metrics.x_height or None,
            capHeight=metrics.cap_height or None,
            italicAngle=metrics.italic_angle,
            openTypeHheaCaretOffset=metrics.caret_offset,
            openTypeOS2TypoLineGap=0,
            openTypeOS2WinAscent=metrics.win_ascent,
            openTypeOS2WinDescent=metrics.win_descent,
            openTypeOS2WeightClass=face.weight if isinstance(face, Face) else None,
            openTypeOS2Selection=[7, 8],
            openTypeOS2Type=[2],
            openTypeHeadCreated=f"{info.created.astimezone(UTC):%Y/%m/%d %H:%M:%S}",
            openTypeHeadFlags=[0, 1],
            openTypeHeadLowestRecPPEM=metrics.lowest_ppem,
        ),
    )
    font.info.openTypeOS2WidthClass = face.width if isinstance(face, Face) else None
    font.info.openTypeGaspRangeRecords = [GaspRangeRecord(rangeMaxPPEM=0xFFFF, rangeGaspBehavior=list(GaspBehavior))]
    font.lib.update({
        **({OPENTYPE_CATEGORIES_KEY: categories} if {_Category.MARK, _Category.LIGATURE} & {*categories.values()} else {}),
        TRUETYPE_INSTRUCTIONS_KEY: {"formatVersion": "1", "controlValueProgram": "PUSHW[ ] 511 SCANCTRL[ ] PUSHB[ ] 4 SCANTYPE[ ]"},
        **({OPENTYPE_META_KEY: {"slng": languages, **({"dlng": sorted(face.genre.script.scripts)} if isinstance(face, Face) else {})}} if languages else {}),
    })
    for glyph in source.glyphs:
        drawn = ufoLib2.objects.Glyph(glyph.name.text, width=glyph.advance, unicodes=sorted(glyph.codes))
        drawn.anchors = [ufoLib2.objects.Anchor(anchor.x, anchor.y, anchor.name) for anchor in glyph.anchors]
        glyph.draw(drawn.getPen())
        font.addGlyph(drawn)
    font.glyphOrder = [glyph.name.text for glyph in source.glyphs]
    return font


def _constructed(source: Source, rule: Metrics, corpus: Corpus, /) -> Construction:
    """Constructs both native font representations with metrics measured on the selected native layout."""
    writer = _FeatureWriter(source=source)
    ufo = _font(writer)
    font = compileTTF(ufo, featureWriters=[writer], removeOverlaps=True, overlapsBackend="pathops", reverseDirection=True, useProductionNames=False, ftConfig={COMPRESSION_LEVEL: 9})
    vertical = _measured(source, rule, corpus=corpus, binary=Binary(data=font))
    ufo.info.ascender, ufo.info.descender = vertical.ascender, vertical.descender
    ufo.info.openTypeOS2WinAscent, ufo.info.openTypeOS2WinDescent = vertical.win_ascent, vertical.win_descent
    font["hhea"].ascent, font["hhea"].descent = vertical.ascender, vertical.descender
    font["OS/2"].sTypoAscender, font["OS/2"].sTypoDescender = vertical.ascender, vertical.descender
    font["OS/2"].usWinAscent, font["OS/2"].usWinDescent = vertical.win_ascent, vertical.win_descent
    named = " ".join(word for word in source.face.style.split() if word != "Italic") or "Regular"
    upright = {"tag": "wght", "name": "Weight", "values": [{"value": font["OS/2"].usWeightClass, "name": named, "flags": ELIDABLE_AXIS_VALUE_NAME * (named == "Regular")}]}
    buildStatTable(font, [upright, *(({"tag": "ital", "name": "Italic", "values": [{"value": 1, "name": "Italic"}]},) if font["post"].italicAngle else ())], macNames=False)
    font["head"].modified = font["head"].created
    font.recalcTimestamp = False
    return Construction(source=msgspec.structs.replace(source, metrics=vertical), ufo=ufo, font=font)


def constructed(original: Binary, face: Faces, corpus: Corpus, /) -> Outcome[Construction]:
    """Imports an original under a face's design record, executes the face's drawing rules, and constructs its UFO and ufo2ft font.

    Args:
        original: Font the face is rebuilt from.
        face: Design record whose plan the rules execute.
        corpus: Runs the rules and metrics measure.

    Returns:
        Ok(construction), the faults of the import and of the first refused rule, and `FAILED` for an outline operation the native boolean refuses.
    """
    plan = planned(face)
    try:
        return (
            imported(original.data, face)
            .bind(lambda source: _executed(source, plan, original=original, corpus=corpus))
            .map(lambda source: _constructed(source, next(rule for rule in plan if isinstance(rule, Metrics)), corpus))
        )
    except PathOpsError as error:
        return Error(Fault.of(Reason.FAILED, face.postscript, error))


def compressed(data: bytes, /) -> bytes:
    """Returns the WOFF2 compression of TrueType or OpenType bytes."""
    buffer = BytesIO()
    compress(BytesIO(data), buffer)
    return buffer.getvalue()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Construction", "compressed", "constructed"]
