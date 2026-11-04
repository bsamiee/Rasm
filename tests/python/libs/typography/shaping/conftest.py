"""Test font with beh forms and a fatha on a top anchor, and a corpus over beh."""

from fontTools.pens.ttGlyphPen import TTGlyphPen
import pytest

from tests.python.libs.typography.fonts import encoded, rectangles
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def binary() -> Binary:
    """Compiled test font, beh drawn with one quadratic corner and the fatha as a bar."""
    pen = TTGlyphPen(None)
    pen.moveTo((0, 0))
    pen.lineTo((500, 0))
    pen.qCurveTo((500, 100), (400, 100))
    pen.lineTo((0, 100))
    pen.closePath()
    glyphs = {
        ".notdef": (500, rectangles()),
        "space": (250, rectangles()),
        **dict.fromkeys(("uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"), (500, pen.glyph())),
        "uni064E": (0, rectangles((-100, 0, 100, 50))),
    }
    features = """
        languagesystem DFLT dflt;
        languagesystem arab dflt;
        languagesystem arab FAR;
        markClass uni064E <anchor 0 0> @TOP;
        feature init { sub uni0628 by uni0628.init; } init;
        feature medi { sub uni0628 by uni0628.medi; } medi;
        feature fina { sub uni0628 by uni0628.fina; } fina;
        feature mark { pos base [uni0628 uni0628.init uni0628.medi uni0628.fina] <anchor 250 600> mark @TOP; } mark;
        table GDEF { GlyphClassDef [uni0628 uni0628.init uni0628.medi uni0628.fina], , [uni064E], ; } GDEF;
        """
    return Binary(data=encoded(glyphs, {0x20: "space", 0x0628: "uni0628", 0x064E: "uni064E"}, features, family="Shaping Test", ascent=800, descent=-200))


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
