"""Test font of beh, alef, lam, a lam-alef ligature, two above marks, and two Persian digits, built whole or with one part perturbed."""

from io import BytesIO
from pathlib import Path

from fontTools.feaLib.builder import addOpenTypeFeaturesFromString
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
import pytest

from typography.faces.declaration import Face
from typography.faces.glyphs import Source
from typography.faces.importing import imported
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

# --- [TYPES] ----------------------------------------------------------------------------

type Rect = tuple[int, int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------

CODES = {0x20: "space", 0x0627: "uni0627", 0x0628: "uni0628", 0x0644: "uni0644", 0x064E: "uni064E", 0x064F: "uni064F", 0x06F1: "uni06F1", 0x06F2: "uni06F2"}

# --- [OPERATIONS] -----------------------------------------------------------------------


def _drawn(*boxes: Rect) -> object:
    pen = TTGlyphPen(None)
    for xmin, ymin, xmax, ymax in boxes:
        pen.moveTo((xmin, ymin))
        pen.lineTo((xmax, ymin))
        pen.lineTo((xmax, ymax))
        pen.lineTo((xmin, ymax))
        pen.closePath()
    return pen.glyph()


def built(
    *,
    overlap: int = 20,
    fatha: int = 200,
    two: tuple[Rect, ...] = ((100, 0, 160, 500), (100, 440, 350, 500)),
    ligature: bool = True,
    dot: Rect = (180, -150, 240, -90),
    damma: int = 0,
    kern: int = 0,
    win: int = 1200,
) -> Binary:
    """Compiles the test font into a binary, each keyword one part a perturbation moves.

    Args:
        overlap: Units each beh bar reaches past its advance on either side.
        fatha: Height of the beh top anchor the marks attach at.
        two: Boxes of ۲.
        ligature: Whether the lam-alef ligature holds ink.
        dot: Box of the beh dot.
        damma: Height the damma rides over its anchor.
        kern: Kern value between joined behs.
        win: Win ascent.

    Returns:
        The binary of the font file.
    """
    advance = 450
    beh: tuple[Rect, ...] = ((-overlap, 0, advance + overlap, 100), dot)
    glyphs: dict[str, tuple[Rect, ...]] = {
        ".notdef": ((50, 0, 400, 700),),
        "space": (),
        **dict.fromkeys(("uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"), beh),
        **dict.fromkeys(("uni0627", "uni0627.fina", "uni0644", "uni0644.init"), ((200, 0, 260, 700),)),
        "uni0644_uni0627.fina": ((100, 0, 160, 700), (300, 0, 360, 700)) if ligature else (),
        "uni064E": ((-100, 0, 100, 40),),
        "uni064F": ((-80, damma, 80, damma + 40),),
        "uni06F1": ((100, 0, 160, 500),),
        "uni06F2": two,
    }
    builder = FontBuilder(1000, isTTF=True)
    builder.setupGlyphOrder(list(glyphs))
    builder.setupCharacterMap(CODES)
    builder.setupGlyf({name: _drawn(*boxes) for name, boxes in glyphs.items()})
    builder.setupHorizontalMetrics({name: (0 if name.startswith("uni064") else 2 * advance if "_" in name else advance, min((box[0] for box in boxes), default=0)) for name, boxes in glyphs.items()})
    builder.setupHorizontalHeader(ascent=1100, descent=-800)
    builder.setupNameTable({"familyName": "Verification Test", "styleName": "Regular"})
    builder.setupOS2(sTypoAscender=1100, sTypoDescender=-800, usWinAscent=win, usWinDescent=900)
    builder.setupPost()
    addOpenTypeFeaturesFromString(
        builder.font,
        f"""
        languagesystem DFLT dflt;
        languagesystem arab dflt;
        markClass [uni064E uni064F] <anchor 0 0> @TOP;
        feature init {{ sub uni0628 by uni0628.init; sub uni0644 by uni0644.init; }} init;
        feature medi {{ sub uni0628 by uni0628.medi; }} medi;
        feature fina {{ sub uni0628 by uni0628.fina; sub uni0627 by uni0627.fina; }} fina;
        feature rlig {{ sub uni0644.init uni0627.fina by uni0644_uni0627.fina; }} rlig;
        feature kern {{ pos uni0628.init uni0628.medi {kern}; pos uni0628.medi uni0628.fina {kern}; }} kern;
        feature mark {{
            pos base [uni0628 uni0628.init uni0628.medi uni0628.fina] <anchor 225 {fatha}> mark @TOP;
            pos base [uni0627 uni0627.fina uni0644 uni0644.init] <anchor 230 800> mark @TOP;
        }} mark;
        table GDEF {{ GlyphClassDef [uni0628 uni0628.init uni0628.medi uni0628.fina uni0627 uni0627.fina uni0644 uni0644.init uni06F1 uni06F2], [uni0644_uni0627.fina], [uni064E uni064F], ; }} GDEF;
        """,
    )
    stream = BytesIO()
    builder.save(stream)
    return Binary.of(stream.getvalue()).ok


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def binary() -> Binary:
    """Binary of the unperturbed test font."""
    return built()


@pytest.fixture(scope="session")
def path(tmp_path_factory: pytest.TempPathFactory, binary: Binary) -> Path:
    """File of the unperturbed test font."""
    (written := tmp_path_factory.mktemp("fonts") / "Verification-Regular.ttf").write_bytes(binary.data)
    return written


@pytest.fixture(scope="session")
def source(path: Path) -> Source:
    """Source imported from the unperturbed test font."""
    return imported(path, Face(style="Regular", original=path.name, family="Verification")).ok


@pytest.fixture(scope="session")
def sample() -> Corpus:
    """Corpus over beh, alef, lam, and the two marks."""
    return Corpus(
        prose=frozendict(fa=("ببب لا",), ar=("بَب",)),
        marked=("بَبُب",),
        annotated=(),
        deep=(),
        words=("بَ بُ",),
        digits=("۱۲",),
        punctuation=(),
        joins=(),
        seam=("ببب",),
        zwnj=(),
        ligatures=("لا",),
        letters=(0x0627, 0x0628),
        vowel_stacks=("َ", "ُ"),
        stacks=(),
    )
