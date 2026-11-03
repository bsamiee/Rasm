"""Fixture fonts for the rules: an Arabic face with joining forms, a lam-alef ligature, harakat, digits, and mark anchors, and an ornament face."""

from io import BytesIO
lazy from pathlib import Path

import pytest
lazy from fontTools.fontBuilder import FontBuilder
lazy from fontTools.pens.ttGlyphPen import TTGlyphPen

lazy from typography.faces.declaration import Face, OrnamentFace, TextNaskh, Values
lazy from typography.faces.glyphs import Source
lazy from typography.faces.importing import imported
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary

# --- [TYPES] ----------------------------------------------------------------------------

type Box = tuple[int, int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------

DIGITS = {f"uni{code:04X}": ((40, 0, 260 + 20 * (code % 10), 500),) for code in (*range(0x06F0, 0x06FA), *range(0x0660, 0x066A))}
ARABIC: dict[str, tuple[int, tuple[Box, ...]]] = {
    ".notdef": (500, ((50, 0, 450, 700),)),
    "space": (26, ()),
    "alef": (180, ((50, 0, 130, 700),)),
    "alef.fina": (180, ((50, 0, 130, 700), (0, 0, 180, 100))),
    "beh": (500, ((0, 0, 500, 100), (220, -200, 300, -120))),
    "beh.init": (300, ((0, 0, 300, 100), (10, -200, 90, -120))),
    "beh.medi": (300, ((0, 0, 300, 100), (120, -200, 200, -120))),
    "beh.fina": (500, ((0, 0, 500, 100), (220, -200, 300, -120))),
    "lam": (400, ((300, 0, 380, 750), (0, 0, 380, 100))),
    "lam.init": (180, ((100, 0, 180, 750), (0, 0, 180, 100))),
    "reh": (300, ((80, -200, 220, 150),)),
    "reh.fina": (300, ((80, -200, 300, 150),)),
    "dal": (300, ((20, 0, 280, 300),)),
    "waw": (300, ((20, -150, 280, 300),)),
    "seen": (600, ((0, 0, 600, 100), (100, 0, 160, 200), (300, 0, 360, 200), (500, 0, 560, 200))),
    "seen.medi": (600, ((0, 0, 600, 100), (100, 0, 160, 200), (300, 0, 360, 200), (500, 0, 560, 200))),
    "tatweel": (300, ((0, 0, 300, 100),)),
    "lamalef": (400, ((0, 0, 400, 100), (40, 0, 120, 750), (260, 0, 340, 750))),
    "fatha": (200, ((-80, 0, 80, 40),)),
    "kasra": (200, ((-80, -40, 80, 0),)),
    "damma": (200, ((-50, 0, 50, 120),)),
    "shadda": (200, ((-80, 0, 80, 70),)),
    "sukun": (200, ((-50, 0, 50, 100),)),
    "fathatan": (200, ((-80, 0, 80, 40), (-80, 70, 80, 110))),
    "hamzaabove": (200, ((-50, 0, 50, 80),)),
    "smallmeem": (200, ((-40, 0, 40, 60),)),
    "tehmarbuta": (300, ((20, 0, 280, 250), (60, 300, 120, 360), (180, 300, 240, 360))),
    "theh": (500, ((0, 0, 500, 100), (200, 150, 260, 210), (300, 150, 360, 210), (250, 230, 310, 290))),
    **{name: (600, boxes) for name, boxes in DIGITS.items()},
}
CMAP = {
    0x0020: "space",
    0x0627: "alef",
    0x0628: "beh",
    0x0644: "lam",
    0x0631: "reh",
    0x062F: "dal",
    0x0648: "waw",
    0x0633: "seen",
    0x0640: "tatweel",
    0x064E: "fatha",
    0x0650: "kasra",
    0x064F: "damma",
    0x0651: "shadda",
    0x0652: "sukun",
    0x064B: "fathatan",
    0x0654: "hamzaabove",
    0x06E2: "smallmeem",
    0x0629: "tehmarbuta",
    0x062B: "theh",
    **{int(name[3:], 16): name for name in DIGITS},
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def _drawn(boxes: tuple[Box, ...], /) -> object:
    pen = TTGlyphPen(None)
    for xmin, ymin, xmax, ymax in boxes:
        pen.moveTo((xmin, ymin))
        pen.lineTo((xmin, ymax))
        pen.lineTo((xmax, ymax))
        pen.lineTo((xmax, ymin))
        pen.closePath()
    return pen.glyph()


def _font(glyphs: dict[str, tuple[int, tuple[Box, ...]]], cmap: dict[int, str], features: str, family: str, /) -> bytes:
    builder = FontBuilder(1000, isTTF=True)
    builder.setupGlyphOrder(list(glyphs))
    builder.setupCharacterMap(cmap)
    builder.setupGlyf({name: _drawn(boxes) for name, (_, boxes) in glyphs.items()})
    builder.setupHorizontalMetrics({name: (advance, min((box[0] for box in boxes), default=0)) for name, (advance, boxes) in glyphs.items()})
    builder.setupHorizontalHeader(ascent=900, descent=-300)
    builder.setupNameTable({"familyName": family, "styleName": "Regular", "copyright": "Test copyright", "designer": "Test designer"})
    builder.setupOS2(achVendID="TEST", usWinAscent=900, usWinDescent=300)
    builder.setupPost()
    if features:
        builder.addOpenTypeFeatures(features)
    stream = BytesIO()
    builder.save(stream)
    return stream.getvalue()


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def face() -> Face:
    """Text Naskh face of the fixture font, its nuqta floor and counter stated."""
    return Face(style="Regular", original="Arabic-Regular.ttf", family="Test", genre=TextNaskh(), values=Values(nuqta_floor=0.05, counter=0.06))


@pytest.fixture(scope="session")
def font(tmp_path_factory: pytest.TempPathFactory) -> Path:
    """Path of the fixture Arabic font."""
    path = tmp_path_factory.mktemp("rules") / "Arabic-Regular.ttf"
    features = """
    languagesystem DFLT dflt;
    languagesystem arab dflt;
    markClass [fatha damma shadda sukun fathatan hamzaabove smallmeem] <anchor 0 0> @ABOVE;
    markClass kasra <anchor 0 0> @BELOW;
    feature init { sub beh by beh.init; sub lam by lam.init; } init;
    feature medi { sub beh by beh.medi; sub seen by seen.medi; } medi;
    feature fina { sub beh by beh.fina; sub alef by alef.fina; sub reh by reh.fina; } fina;
    feature liga { sub lam.init alef.fina by lamalef; } liga;
    feature mark {
        pos base [alef alef.fina beh beh.init beh.medi beh.fina lam lam.init reh reh.fina dal waw seen seen.medi tatweel tehmarbuta theh] <anchor 150 800> mark @ABOVE <anchor 150 -250> mark @BELOW;
        pos ligature lamalef <anchor 300 850> mark @ABOVE <anchor 300 -150> mark @BELOW ligComponent <anchor 80 850> mark @ABOVE <anchor 80 -150> mark @BELOW;
    } mark;
    table GDEF { GlyphClassDef [alef alef.fina beh beh.init beh.medi beh.fina lam lam.init reh reh.fina dal waw seen seen.medi tatweel tehmarbuta theh], [lamalef], [fatha kasra damma shadda sukun fathatan hamzaabove smallmeem], ; } GDEF;
    """
    path.write_bytes(_font(ARABIC, CMAP, features, "Test"))
    return path


@pytest.fixture(scope="session")
def original(font: Path) -> Binary:
    """Fixture Arabic font read for shaping."""
    return Binary.read(font).ok


@pytest.fixture(scope="session")
def source(font: Path, face: Face) -> Source:
    """Fixture Arabic font imported into a source."""
    return imported(font, face).ok


@pytest.fixture(scope="session")
def sample() -> Corpus:
    """Corpus over the fixture letters."""
    return Corpus(
        prose=frozendict(fa=("بب با رب",), ar=("لا بر سبب",)),
        marked=("بَ بِ",),
        annotated=(),
        deep=(),
        words=("بب",),
        digits=("۱۲",),
        punctuation=(),
        joins=("بـب",),
        seam=("ببب",),
        zwnj=(),
        ligatures=("لا",),
        letters=(0x0628, 0x0644, 0x0627),
        vowel_stacks=("َ",),
        stacks=(),
    )


@pytest.fixture(scope="session")
def ornament(tmp_path_factory: pytest.TempPathFactory) -> tuple[Path, OrnamentFace]:
    """Ornament font with a Latin template glyph, two copies of one square, a tall piece, a tile overlapping its advance, a blank slot, and art at a Private Use code point, and its face."""
    glyphs: dict[str, tuple[int, tuple[Box, ...]]] = {
        ".notdef": (500, ((50, 0, 450, 700),)),
        "space": (26, ()),
        "A": (600, ((50, 0, 550, 700),)),
        "AIS_1": (600, ((100, 0, 400, 300),)),
        "AIF_1": (500, ((50, 0, 350, 300),)),
        "AIS_2": (700, ((60, -100, 600, 800),)),
        "IF_1": (400, ((-20, 0, 410, 200),)),
        "blank": (300, ()),
        "STAR": (500, ((50, 0, 150, 100), (250, 0, 350, 100))),
    }
    cmap = {0x0020: "space", 0x0041: "A", 0xFE8D: "AIS_1", 0xFE8E: "AIF_1", 0xFE8F: "AIS_2", 0xFE90: "IF_1", 0xFE91: "blank", 0xE010: "STAR"}
    path = tmp_path_factory.mktemp("ornaments") / "Orn-Symbols.ttf"
    path.write_bytes(_font(glyphs, cmap, "", "Orn"))
    return (path, OrnamentFace(style="Regular", original=path.name, family="Orn"))
