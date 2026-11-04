"""Fixture building an Arabic test font with positional forms, a lam-alef ligature, mark anchors, and a GDEF."""

lazy from pathlib import Path

import pytest
lazy from fontTools.pens.ttGlyphPen import TTGlyphPen

lazy from tests.python.libs.typography.fonts import encoded
lazy from typography.faces.declaration import Face, Kufi
lazy from typography.faces.glyphs import Source
lazy from typography.faces.importing import imported
lazy from typography.shaping.corpus import Corpus

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def arabic(tmp_path_factory: pytest.TempPathFactory) -> Path:
    """Returns the path of a TrueType font mapping beh, lam, alef, fatha, kasra, space, a beh presentation form, and a Private Use code point."""
    order = [".notdef", "space", "beh", "beh.i", "beh.m", "beh.f", "lam", "lam.i", "alef", "alef.f", "lamalef", "fatha", "kasra", "orphan"]
    pens = {name: TTGlyphPen(None) for name in order}
    for name, pen in pens.items():
        if name != "space":
            pen.moveTo((0, 0))
            pen.lineTo((0, 100))
            pen.qCurveTo((50, 150), (100, 100))
            pen.lineTo((100, 0))
            pen.closePath()
    features = """
languagesystem arab dflt;
markClass fatha <anchor 50 80> @ABOVE;
markClass kasra <anchor 50 -80> @BELOW;
feature init { sub beh by beh.i; sub lam by lam.i; } init;
feature medi { sub beh by beh.m; } medi;
feature fina { sub beh by beh.f; sub alef by alef.f; } fina;
feature rlig { sub lam.i alef.f by lamalef; } rlig;
feature mark {
    pos base [beh beh.i beh.m beh.f lam alef] <anchor 250 600> mark @ABOVE <anchor 250 -200> mark @BELOW;
    pos ligature lamalef <anchor 150 700> mark @ABOVE <anchor 150 -200> mark @BELOW ligComponent <anchor 400 700> mark @ABOVE <anchor 400 -200> mark @BELOW;
} mark;
table GDEF { GlyphClassDef [beh beh.i beh.m beh.f lam lam.i alef alef.f], [lamalef], [fatha kasra], ; } GDEF;
"""
    path = tmp_path_factory.mktemp("fonts") / "Arabic-Regular.ttf"
    path.write_bytes(
        encoded(
            {name: (0 if name in {"fatha", "kasra"} else 500, pen.glyph()) for name, pen in pens.items()},
            {0x0020: "space", 0x00A0: "space", 0x0628: "beh", 0xFE90: "beh.f", 0x0644: "lam", 0x0627: "alef", 0x064E: "fatha", 0x0650: "kasra", 0xE000: "beh"},
            features,
            ascent=800,
            descent=-200,
        )
    )
    return path


@pytest.fixture(scope="session")
def face(arabic: Path) -> Face:
    """Declares the native fixture as a text face."""
    return Face(style="Regular", original=arabic.name, family="Test", genre=Kufi())


@pytest.fixture(scope="session")
def source(arabic: Path, face: Face) -> Source:
    """Imports the fixture once for immutable source behavior tests."""
    return imported(arabic, face).default_with(lambda faults: pytest.fail(str(faults)))


@pytest.fixture(scope="session")
def sample() -> Corpus:
    """Defines the imported forms, marks, and ligature that native construction measures."""
    return Corpus(
        prose=frozendict(ar=("ببب", "لا")),
        marked=("بَ", "بِ", "لَا"),
        annotated=(),
        deep=(),
        words=(),
        digits=(),
        punctuation=(),
        joins=(),
        seam=(),
        zwnj=(),
        ligatures=("لا",),
        letters=(0x0628, 0x0644, 0x0627),
        vowel_stacks=("َ", "ِ"),
        stacks=(),
    )
