"""Test font with beh forms and a fatha on a top anchor, and a corpus over beh."""

from io import BytesIO

from fontTools.feaLib.builder import addOpenTypeFeaturesFromString
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
import pytest

from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

# --- [CONSTANTS] ------------------------------------------------------------------------

ADVANCES = {".notdef": 500, "space": 250, "uni0628": 500, "uni0628.init": 500, "uni0628.medi": 500, "uni0628.fina": 500, "uni064E": 0}

# --- [COMPOSITION] ----------------------------------------------------------------------


def _glyph(name: str, /) -> object:
    pen = TTGlyphPen(None)
    if name.startswith("uni0628"):
        pen.moveTo((0, 0))
        pen.lineTo((500, 0))
        pen.qCurveTo((500, 100), (400, 100))
        pen.lineTo((0, 100))
        pen.closePath()
    if name == "uni064E":
        pen.moveTo((-100, 0))
        pen.lineTo((100, 0))
        pen.lineTo((100, 50))
        pen.lineTo((-100, 50))
        pen.closePath()
    return pen.glyph()


@pytest.fixture(scope="session")
def binary() -> Binary:
    """Compiled test font, beh drawn with one quadratic corner and the fatha as a bar."""
    builder = FontBuilder(1000, isTTF=True)
    builder.setupGlyphOrder(list(ADVANCES))
    builder.setupCharacterMap({0x20: "space", 0x0628: "uni0628", 0x064E: "uni064E"})
    builder.setupGlyf({name: _glyph(name) for name in ADVANCES})
    builder.setupHorizontalMetrics({name: (advance, 0) for name, advance in ADVANCES.items()})
    builder.setupHorizontalHeader(ascent=800, descent=-200)
    builder.setupNameTable({"familyName": "Shaping Test", "styleName": "Regular"})
    builder.setupOS2()
    builder.setupPost()
    addOpenTypeFeaturesFromString(
        builder.font,
        """
        languagesystem DFLT dflt;
        languagesystem arab dflt;
        languagesystem arab FAR;
        markClass uni064E <anchor 0 0> @TOP;
        feature init { sub uni0628 by uni0628.init; } init;
        feature medi { sub uni0628 by uni0628.medi; } medi;
        feature fina { sub uni0628 by uni0628.fina; } fina;
        feature mark { pos base [uni0628 uni0628.init uni0628.medi uni0628.fina] <anchor 250 600> mark @TOP; } mark;
        table GDEF { GlyphClassDef [uni0628 uni0628.init uni0628.medi uni0628.fina], , [uni064E], ; } GDEF;
        """,
    )
    stream = BytesIO()
    builder.save(stream)
    return Binary.of(stream.getvalue()).ok


@pytest.fixture(scope="session")
def sample() -> Corpus:
    """Corpus over beh, one line repeated across kinds."""
    return Corpus(
        prose=frozendict(fa=("بب",), ar=("ببب",)),
        marked=("بَ",),
        annotated=(),
        deep=(),
        words=("ب ب",),
        digits=(),
        punctuation=(),
        joins=("ببب",),
        seam=("ببب",),
        zwnj=(),
        ligatures=(),
        letters=(0x0628,),
        vowel_stacks=("َ",),
        stacks=(),
    )
