"""Arabic test original with beh forms, alef, a lam-alef ligature and a fatha on mark anchors, its declaration, corpus, and imported source."""

from pathlib import Path

from expression import Result
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
import pytest

from typography.faces.declaration import Declared, Face, Family, FontSet, Kufi
from typography.faces.glyphs import Source
from typography.faces.importing import imported
from typography.shaping.corpus import Corpus

# --- [CONSTANTS] ------------------------------------------------------------------------

ADVANCES = {".notdef": 500, "space": 250, "beh": 500, "beh.i": 500, "beh.m": 500, "beh.f": 500, "lam": 300, "lam.i": 300, "alef": 250, "alef.f": 250, "lamalef": 550, "fatha": 0}

# --- [COMPOSITION] ----------------------------------------------------------------------


def _drawn(name: str, /) -> object:
    pen = TTGlyphPen(None)
    if name != "space":
        width = 200 if name == "fatha" else ADVANCES[name] - 50
        pen.moveTo((0, 0))
        pen.lineTo((0, 100))
        pen.qCurveTo((width / 2, 160), (width, 100))
        pen.lineTo((width, 0))
        pen.closePath()
    return pen.glyph()


@pytest.fixture
def declared(tmp_path: Path) -> Declared:
    """Declaration of one Kufi face over a FontBuilder original in a temporary folder."""
    builder = FontBuilder(1000, isTTF=True)
    builder.setupGlyphOrder(list(ADVANCES))
    builder.setupCharacterMap({0x0020: "space", 0x0628: "beh", 0x0644: "lam", 0x0627: "alef", 0x064E: "fatha"})
    builder.setupGlyf({name: _drawn(name) for name in ADVANCES})
    builder.setupHorizontalMetrics({name: (advance, 0) for name, advance in ADVANCES.items()})
    builder.setupHorizontalHeader(ascent=800, descent=-200)
    builder.setupNameTable({"familyName": "Test", "styleName": "Regular", "copyright": "Test copyright", "designer": "Test designer"})
    builder.setupOS2(achVendID="TEST", usWinAscent=900, usWinDescent=300, sTypoAscender=800, sTypoDescender=-200)
    builder.setupPost()
    builder.addOpenTypeFeatures("""
languagesystem arab dflt;
markClass fatha <anchor 100 80> @ABOVE;
feature init { sub beh by beh.i; sub lam by lam.i; } init;
feature medi { sub beh by beh.m; } medi;
feature fina { sub beh by beh.f; sub alef by alef.f; } fina;
feature rlig { sub lam.i alef.f by lamalef; } rlig;
feature mark { pos base [beh beh.i beh.m beh.f lam alef] <anchor 250 600> mark @ABOVE; } mark;
table GDEF { GlyphClassDef [beh beh.i beh.m beh.f lam lam.i alef alef.f], [lamalef], [fatha], ; } GDEF;
""")
    (tmp_path / "original").mkdir()
    builder.save(str(tmp_path / "original" / "Test-Regular.ttf"))
    family = Family(name="Test", original="original", genre=Kufi(), faces=(Face(style="Regular", original="Test-Regular.ttf"),))
    return Declared(set=FontSet(name="Set", corpus="corpus.toml", output="build", families=(family,)), root=tmp_path)


@pytest.fixture
def texts() -> Corpus:
    """Corpus over beh, lam, and alef."""
    lines = ("بلا", "ببب")
    return Corpus(
        prose=frozendict(fa=lines, ar=lines),
        marked=("بَب",),
        annotated=(),
        deep=(),
        words=lines,
        digits=(),
        punctuation=(),
        joins=(),
        seam=lines,
        zwnj=(),
        ligatures=("لا",),
        letters=(0x0628, 0x0644, 0x0627),
        vowel_stacks=("َ",),
        stacks=(),
    )


@pytest.fixture
def source(declared: Declared) -> Source:
    """Source the declared face imports from its original."""
    face = declared.set.faces[0]
    match imported(declared.original(face), face):
        case Result(tag="ok", ok=found):
            return found
        case Result(error=faults):
            raise AssertionError(faults)
