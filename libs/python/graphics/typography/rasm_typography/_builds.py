"""Face construction from an original: rules executed over the imported source, its UFO, the ufo2ft binary with metrics measured on native layout, hinted TrueType bytes, and WOFF2."""

from dataclasses import dataclass
from datetime import UTC
lazy from functools import partial, reduce
lazy from io import BytesIO
lazy from pathlib import Path

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

from rasm_typography._faces.glyphs import Category, fetched, Source, VerticalMetrics
lazy from rasm_records import Error, Fault, Faults, Ok, Outcome, Reason
lazy from rasm_typography._faces.declaration import Face, Faces
lazy from rasm_typography._faces.importing import imported
lazy from rasm_typography._rules.emission import FeatureWriter
lazy from rasm_typography._rules.metrics import Metrics
lazy from rasm_typography._rules.plans import planned
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINKED = (400, 700)

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


def _named(rule: str, faults: Faults, /) -> Faults:
    """Prefixes each fault's member with the rule that refused."""
    head, *rest = (msgspec.structs.replace(fault, member=rule if fault.member is None else f"{rule} {fault.member}") for fault in faults)
    return (head, *rest)


def _built(rule: Metrics, corpus: Corpus, source: Source, /) -> Construction:
    """Constructs a source's UFO and ufo2ft font with vertical metrics measured on the font's own layout, its style map and `STAT` values read from its weight class and italic angle, an ornament face at the regular weight class and its other absent fields left to ufo2ft fallbacks."""
    face = source.face
    writer = FeatureWriter(source=source)
    info, metrics, languages = source.info, source.metrics, list(source.scripts)
    categories = {glyph.name.text: glyph.category for glyph in source.glyphs}
    weight, italic = face.weight if isinstance(face, Face) else _LINKED[0], metrics.italic_angle != 0
    slope = "Italic" if italic else "Roman"
    upright = face.style.removesuffix(slope).strip() or "Regular"
    ufo = ufoLib2.Font(
        features=ufoLib2.objects.Features(writer.substitutions),
        info=ufoLib2.objects.Info(
            familyName=face.typographic,
            styleName=face.style,
            styleMapFamilyName=face.typographic if weight in _LINKED else f"{face.typographic} {upright}",
            styleMapStyleName=" ".join((*(("bold",) if weight == _LINKED[1] else ()), *(("italic",) if italic else ()))) or "regular",
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
            openTypeOS2WeightClass=weight,
            openTypeOS2Selection=[7, 8],
            openTypeOS2Type=[2],
            openTypeHeadCreated=f"{info.created.astimezone(UTC):%Y/%m/%d %H:%M:%S}",
            openTypeHeadFlags=[0, 1],
            openTypeHeadLowestRecPPEM=metrics.lowest_ppem,
        ),
    )
    ufo.info.openTypeOS2WidthClass = face.width if isinstance(face, Face) else None
    ufo.info.openTypeGaspRangeRecords = [GaspRangeRecord(rangeMaxPPEM=0xFFFF, rangeGaspBehavior=list(GaspBehavior))]
    ufo.lib.update({
        **({OPENTYPE_CATEGORIES_KEY: categories} if {Category.MARK, Category.LIGATURE} & {*categories.values()} else {}),
        TRUETYPE_INSTRUCTIONS_KEY: {"formatVersion": "1", "controlValueProgram": "PUSHW[ ] 511 SCANCTRL[ ] PUSHB[ ] 4 SCANTYPE[ ]"},
        **({OPENTYPE_META_KEY: {"slng": languages, **({"dlng": sorted(face.genre.script.scripts)} if isinstance(face, Face) else {})}} if languages else {}),
    })
    for glyph in source.glyphs:
        drawn = ufoLib2.objects.Glyph(glyph.name.text, width=glyph.advance, unicodes=sorted(glyph.codes))
        drawn.anchors = [ufoLib2.objects.Anchor(anchor.x, anchor.y, anchor.name) for anchor in glyph.anchors]
        glyph.draw(drawn.getPen())
        ufo.addGlyph(drawn)
    ufo.glyphOrder = [glyph.name.text for glyph in source.glyphs]
    font = compileTTF(ufo, featureWriters=[writer], removeOverlaps=True, overlapsBackend="pathops", reverseDirection=True, useProductionNames=False, ftConfig={COMPRESSION_LEVEL: 9})
    vertical = rule.measured(source, corpus=corpus, binary=Binary(data=font))
    metered(font, ufo.info, vertical)
    buildStatTable(
        font,
        [
            {"tag": "wght", "name": "Weight", "values": [{"value": weight, "name": upright, "flags": ELIDABLE_AXIS_VALUE_NAME * (weight == _LINKED[0])}]},
            {"tag": "ital", "name": "Italic", "values": [{"value": int(italic), "name": slope, "flags": ELIDABLE_AXIS_VALUE_NAME * (not italic)}]},
        ],
        macNames=False,
    )
    font["head"].modified = font["head"].created
    font.recalcTimestamp = False
    return Construction(source=msgspec.structs.replace(source, metrics=vertical), ufo=ufo, font=font)


def metered(font: TTFont, info: ufoLib2.objects.Info, metrics: VerticalMetrics, /) -> None:
    """Writes the line and win metrics of a record into a compiled font's `hhea` and `OS/2` tables and the UFO info it compiles from, so source and binary agree."""
    info.ascender, info.descender, info.openTypeOS2WinAscent, info.openTypeOS2WinDescent = metrics.ascender, metrics.descender, metrics.win_ascent, metrics.win_descent
    font["hhea"].ascent, font["hhea"].descent = metrics.ascender, metrics.descender
    vars(font["OS/2"]).update(sTypoAscender=metrics.ascender, sTypoDescender=metrics.descender, usWinAscent=metrics.win_ascent, usWinDescent=metrics.win_descent)


def constructed(original: Binary, face: Faces, corpus: Corpus, /, *, cache: Path) -> Outcome[Construction]:
    """Imports an original under a face's design record, folds the face's drawing rules over it, and constructs its UFO and ufo2ft font with metrics measured on its own layout.

    Args:
        original: Font the face is rebuilt from.
        face: Design record whose plan the rules execute.
        corpus: Runs the rules and metrics measure.
        cache: Folder holding the pinned Unicode joining table, downloaded where it lacks a copy.

    Returns:
        Ok(construction), the faults of the import and of the first refused rule, its name leading each fault's member, and `FAILED` for an outline operation the native boolean refuses.
    """
    plan = planned(face)
    try:
        executed: Outcome[Source] = fetched(cache).bind(
            lambda joinings: reduce(
                lambda held, rule: held.bind(lambda source: rule.applied(source, original=original, corpus=corpus, joinings=joinings).map_error(partial(_named, type(rule).__name__.lower()))),
                (rule for rule in plan if not isinstance(rule, Metrics)),
                imported(original.data, face, joinings),
            )
        )
        return executed.map(partial(_built, next(rule for rule in plan if isinstance(rule, Metrics)), corpus))
    except PathOpsError as error:
        return Error(Fault.of(Reason.FAILED, face.postscript, error))


def compressed(data: bytes, /) -> bytes:
    """Returns the WOFF2 compression of TrueType or OpenType bytes."""
    buffer = BytesIO()
    compress(BytesIO(data), buffer)
    return buffer.getvalue()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Construction", "compressed", "constructed", "metered"]
