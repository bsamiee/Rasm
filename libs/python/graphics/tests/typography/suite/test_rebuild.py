"""Dot rules, derived signs, and the requirements catching the set's defects, each on a probe font drawn to hold the defect."""

from collections.abc import Mapping
from io import BytesIO
import math

from fontTools.fontBuilder import FontBuilder
from fontTools.pens.statisticsPen import StatisticsPen
from fontTools.pens.ttGlyphPen import TTGlyphPen
import pathops
import pytest

from rasm_testing import settled
from rasm_typography import Binary, Corpus, DotMatrix, Face, imported, Locale, MarkSide, Outlines, PartClearance, planned, Source, TextNaskh

# --- [TYPES] ----------------------------------------------------------------------------

type _Rect = tuple[int, int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------

_BEH: dict[str, tuple[int, tuple[_Rect, ...]]] = {"uni0628": (600, ((0, 0, 600, 150), (265, -200, 335, -130)))}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _medial(name: str, /) -> str:
    return f"feature medi {{ sub {name} by {name}.medi; }} medi;"


def _font(glyphs: Mapping[str, tuple[int, tuple[_Rect, ...]]], cmap: Mapping[int, str], features: str = "", /) -> bytes:
    """A 1000-unit TrueType font of rectangle glyphs, its cmap, and feature code."""
    order = [".notdef", *glyphs]
    builder = FontBuilder(1000, isTTF=True)
    builder.setupGlyphOrder(order)
    builder.setupCharacterMap(dict(cmap))
    pens = {name: TTGlyphPen(None) for name in order}
    for name, (_, rects) in glyphs.items():
        for xmin, ymin, xmax, ymax in rects:
            pens[name].moveTo((xmin, ymin))
            pens[name].lineTo((xmin, ymax))
            pens[name].lineTo((xmax, ymax))
            pens[name].lineTo((xmax, ymin))
            pens[name].closePath()
    builder.setupGlyf({name: pen.glyph() for name, pen in pens.items()})
    builder.setupHorizontalMetrics({name: (glyphs.get(name, (500, ()))[0], 0) for name in order})
    builder.setupHorizontalHeader(ascent=800, descent=-300)
    builder.setupNameTable({"familyName": "Probe", "styleName": "Regular"})
    builder.setupOS2(usWinAscent=800, usWinDescent=300)
    builder.setupPost()
    builder.addOpenTypeFeatures(features)
    buffer = BytesIO()
    builder.save(buffer)
    return buffer.getvalue()


def _ruled(face: Face, data: bytes, kind: type[Outlines | Locale], /) -> Source:
    rule = next(rule for rule in planned(face) if isinstance(rule, kind))
    return settled(rule.applied(settled(imported(data, face, frozendict())), joinings=frozendict()))


def _outers(source: Source, name: str, /) -> tuple[tuple[float, float], ...]:
    """Area and elongation of each outer contour of a glyph, smallest first."""
    path = pathops.Path()
    source.drawn(name).draw(path.getPen())
    held: list[tuple[float, float]] = []
    for contour in pathops.simplify(path, fix_winding=True, clockwise=False).contours:
        pen = StatisticsPen()
        contour.draw(pen)
        middle, spread = (pen.varianceX + pen.varianceY) / 2, math.hypot((pen.varianceX - pen.varianceY) / 2, pen.covariance)
        held.extend(() if contour.clockwise else ((float(contour.area), math.sqrt((middle + spread) / (middle - spread))),))
    return tuple(sorted(held))


def _corpus(*, zwnj: tuple[str, ...] = (), ligatures: tuple[str, ...] = ()) -> Corpus:
    return Corpus(
        prose=frozendict({"fa": ()}),
        marked=(),
        annotated=(),
        deep=(),
        words=(),
        digits=(),
        punctuation=(),
        joins=(),
        seam=(),
        zwnj=zwnj,
        ligatures=ligatures,
        letters=(),
        vowel_stacks=(),
        stacks=(),
    )


def test_spur_cut_keeps_dots_under_the_join_band() -> None:
    """Dots of a medial yeh narrower than half the join band stay dots, no longer cut as needle spurs, so `جیم` never reads `جبم`."""
    glyphs = _BEH | {"uni06CC": (500, ((0, 0, 500, 150),)), "uni06CC.medi": (500, ((0, 0, 500, 150), (150, -220, 210, -160), (290, -220, 350, -160)))}
    source = _ruled(Face(family="Probe", style="Regular", genre=TextNaskh()), _font(glyphs, {0x0628: "uni0628", 0x06CC: "uni06CC"}, _medial("uni06CC")), Outlines)
    assert len(_outers(source, "uni06CC.medi")) == 3


def test_two_dot_dash_keeps_its_shape() -> None:
    """A two-dot dash of nuqta area keeps its length where a single dot would turn `ت` into `ن`."""
    glyphs = _BEH | {"uni062A": (500, ((0, 0, 500, 150),)), "uni062A.medi": (500, ((0, 0, 500, 150), (190, 250, 310, 300)))}
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Outlines))
    source = _ruled(face, _font(glyphs, {0x0628: "uni0628", 0x062A: "uni062A"}, _medial("uni062A")), Outlines)
    (_, dash), (_, dot) = (_outers(source, name)[0] for name in ("uni062A.medi", "uni0628"))
    assert len(_outers(source, "uni062A.medi")) == 2
    assert dash >= rule.shape * dot


def test_redrawn_dots_stay_apart() -> None:
    """Dots redrawn at the larger nuqta of beh spread from their middle until each keeps its original gap, so none merge."""
    glyphs = _BEH | {"uni062B": (500, ((0, 0, 500, 150),)), "uni062B.medi": (500, ((0, 0, 500, 150), (180, 250, 240, 310), (250, 250, 310, 310)))}
    source = _ruled(Face(family="Probe", style="Regular", genre=TextNaskh()), _font(glyphs, {0x0628: "uni0628", 0x062B: "uni062B"}, _medial("uni062B")), Outlines)
    assert len(_outers(source, "uni062B.medi")) == 3


def test_locale_derives_the_six_signs_persian_dimensions_need() -> None:
    """Plus-minus, almost-equal, not-equal, section, prime, and double prime come from the face's own glyphs, plus-minus centred on the plus."""
    glyphs = {
        "plus": (600, ((260, 100, 340, 500), (100, 260, 500, 340))),
        "minus": (600, ((100, 260, 500, 340),)),
        "asciitilde": (600, ((100, 280, 500, 340),)),
        "equal": (600, ((100, 200, 500, 260), (100, 340, 500, 400))),
        "slash": (400, ((150, -100, 250, 700),)),
        "S": (500, ((100, 0, 400, 80), (100, 0, 180, 350), (100, 310, 400, 390), (320, 310, 400, 700), (100, 620, 400, 700))),
        "quotesingle": (200, ((70, 500, 130, 700),)),
    }
    cmap = {0x002B: "plus", 0x2212: "minus", 0x007E: "asciitilde", 0x003D: "equal", 0x002F: "slash", 0x0053: "S", 0x0027: "quotesingle"}
    source = _ruled(Face(family="Probe", style="Regular", genre=TextNaskh()), _font(glyphs, cmap), Locale)
    assert {0x00B1, 0x00A7, 0x2032, 0x2033, 0x2248, 0x2260} <= source.encoded.keys()
    plus, plusminus = (source.bounds(source.encoded[code]) for code in (0x002B, 0x00B1))
    assert plus is not None
    assert plusminus is not None
    assert abs((plus[1] + plus[3]) / 2 - (plusminus[1] + plusminus[3]) / 2) <= 1
    assert len(_outers(source, source.encoded[0x2033])) == 2


def test_locale_seats_a_strayed_madda_over_the_alef() -> None:
    """A madda drawn right of the alef, over the letter after it, is centred on the stem."""
    glyphs = {"uni0627": (300, ((100, 0, 160, 700),)), "uni0622": (600, ((100, 0, 160, 700), (300, 760, 500, 800)))}
    source = _ruled(Face(family="Probe", style="Regular", genre=DotMatrix()), _font(glyphs, {0x0627: "uni0627", 0x0622: "uni0622"}), Locale)
    alef, madda = source.bounds("uni0627"), source.bounds("uni0622")
    assert alef is not None
    assert madda is not None
    assert abs((madda[0] + madda[2]) / 2 - (alef[0] + alef[2]) / 2) <= (alef[2] - alef[0]) / 2


def test_part_clearance_finds_a_tail_under_the_next_part() -> None:
    """A returning yeh drawn under the next part, the collision across ZWNJ no published checker reports."""
    glyphs = {"uni06CC": (400, ((-300, -100, 400, 100),)), "uni062E": (400, ((0, 0, 400, 300),))}
    found = PartClearance(gap=0.03).measured(Binary(data=_font(glyphs, {0x06CC: "uni06CC", 0x062E: "uni062E"})), corpus=_corpus(zwnj=("یخ",)))
    assert [subject for subject, _, _ in found] == ["uni062E uni06CC in یخ"]


def test_mark_side_finds_a_kasra_over_the_baseline() -> None:
    """A kasra seated above the baseline, as Mansub's original seats it on `لا`."""
    glyphs = {"uni0628": (600, ((0, 0, 600, 150),)), "uni0650": (0, ((-50, 0, 50, 50),))}
    features = "markClass uni0650 <anchor 0 0> @BELOW;\nfeature mark { pos base uni0628 <anchor 300 100> mark @BELOW; } mark;\ntable GDEF { GlyphClassDef [uni0628], , [uni0650], ; } GDEF;"
    data = _font(glyphs, {0x0628: "uni0628", 0x0650: "uni0650"}, features)
    found = MarkSide.measured(Binary(data=data), corpus=_corpus(ligatures=("بِ",)))
    assert [subject for subject, _, _ in found] == ["uni0650 in بِ"]


@pytest.mark.parametrize("face,dotted", [(Face(family="Probe", style="Regular"), True), (Face(family="Probe", style="Regular", genre=DotMatrix()), False)])
def test_dot_rules_read_whether_letters_carry_dots(face: Face, dotted: bool) -> None:
    """A dot-matrix face draws every letter in dots, so no piece of it is a dot the dot rules move or check."""
    assert face.dotted is dotted
