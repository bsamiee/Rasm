"""Dot rules, derived signs, and the requirements catching the set's defects, each on a probe font drawn to hold the defect."""

from collections.abc import Mapping
from io import BytesIO
import math

from fontTools.fontBuilder import FontBuilder
from fontTools.otlLib.builder import LOOKUP_FLAG_IGNORE_MARKS
from fontTools.pens.statisticsPen import StatisticsPen
from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.ttLib import TTFont
import msgspec
import pathops
import pytest

from rasm_testing import settled
from rasm_typography import (
    Anchor,
    Anchors,
    area,
    Binary,
    Contextual,
    Corpus,
    DotMatrix,
    Face,
    Identity,
    imported,
    Joining,
    Joins,
    Lifts,
    Locale,
    MarkClearance,
    MarkSide,
    Metrics,
    offset,
    OrnamentFace,
    Outlines,
    Pairs,
    PartClearance,
    pieces,
    planned,
    Point,
    Seam,
    Source,
    Spacing,
    TextNaskh,
    union,
)

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
    builder.setupHorizontalMetrics({name: (advance, min((rect[0] for rect in rects), default=0)) for name in order for advance, rects in (glyphs.get(name, (500, ())),)})
    builder.setupHorizontalHeader(ascent=800, descent=-300)
    builder.setupNameTable({"familyName": "Probe", "styleName": "Regular"})
    builder.setupOS2(usWinAscent=800, usWinDescent=300)
    builder.setupPost()
    builder.addOpenTypeFeatures(features)
    buffer = BytesIO()
    builder.save(buffer)
    return buffer.getvalue()


def _ruled(face: Face, data: bytes, kind: type[Outlines | Locale | Joins], /) -> Source:
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


def _corpus(*, zwnj: tuple[str, ...] = (), ligatures: tuple[str, ...] = (), words: tuple[str, ...] = (), seam: tuple[str, ...] = (), letters: tuple[int, ...] = ()) -> Corpus:
    return Corpus(
        prose=frozendict({"fa": ()}),
        marked=(),
        annotated=(),
        deep=(),
        words=words,
        digits=(),
        punctuation=(),
        joins=(),
        seam=seam,
        zwnj=zwnj,
        ligatures=ligatures,
        letters=letters,
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
    found = PartClearance(gap=0.03).measured(Binary(data=_font(glyphs, {0x06CC: "uni06CC", 0x062E: "uni062E"})), corpus=_corpus(zwnj=("یخ",)), joinings=frozendict())
    assert [subject for subject, _, _ in found] == ["uni062E uni06CC in یخ"]


def test_mark_side_finds_a_kasra_over_the_baseline() -> None:
    """A kasra seated above the baseline, as Mansub's original seats it on `لا`."""
    glyphs = {"uni0628": (600, ((0, 0, 600, 150),)), "uni0650": (0, ((-50, 0, 50, 50),))}
    features = "markClass uni0650 <anchor 0 0> @BELOW;\nfeature mark { pos base uni0628 <anchor 300 100> mark @BELOW; } mark;\ntable GDEF { GlyphClassDef [uni0628], , [uni0650], ; } GDEF;"
    data = _font(glyphs, {0x0628: "uni0628", 0x0650: "uni0650"}, features)
    found = MarkSide.measured(Binary(data=data), corpus=_corpus(ligatures=("بِ",)))
    assert [subject for subject, _, _ in found] == ["uni0650 in بِ"]


@pytest.mark.parametrize("face,dotted", [(Face(family="Probe", style="Regular"), True), (Face(family="Probe", style="Regular", genre=DotMatrix()), False)])
def test_dot_rules_read_whether_letters_carry_dots(face: Face, *, dotted: bool) -> None:
    """A dot-matrix face draws every letter in dots, so no piece of it is a dot the dot rules move or check."""
    assert face.dotted is dotted


@pytest.mark.parametrize("stroke,found", [((-11, 0, 100, 150), ()), ((-11, 200, 100, 350), ("uni0627.fina uni0628.init in با",))])
def test_mark_clearance_reads_a_stencil_stroke_at_a_join_as_body(stroke: _Rect, found: tuple[str, ...]) -> None:
    """A piece of dot size crossing the baseline at its joining edge is a stencil face's joining stroke and overlaps its neighbour by the join, one above the baseline a loose piece colliding with it."""
    glyphs = {
        "uni0628": (600, ((0, 0, 600, 150), (265, -200, 335, -130))),
        "uni0628.init": (600, ((120, 0, 600, 150), stroke, (300, -200, 370, -130))),
        "uni0627": (200, ((50, 0, 150, 700),)),
        "uni0627.fina": (200, ((50, 0, 200, 700),)),
    }
    features = "feature init { sub uni0628 by uni0628.init; } init;\nfeature fina { sub uni0627 by uni0627.fina; } fina;"
    binary = Binary(data=_font(glyphs, {0x0628: "uni0628", 0x0627: "uni0627"}, features))
    assert tuple(subject for subject, _, _ in MarkClearance(gap=0.03).measured(binary, corpus=_corpus(words=("با",)), joinings=frozendict())) == found


def test_spacing_kerns_a_ligature_ending_a_part_across_its_marks() -> None:
    """A lam-alef ligature reaching under the next part is kerned the floor clear of it, the pair matching across marks typed between."""
    glyphs = {
        "uni0627": (300, ((100, 0, 200, 700),)),
        "uni0644": (300, ((0, 0, 300, 700),)),
        "uni0644_uni0627": (600, ((-150, -100, 600, 0), (100, 0, 200, 700), (400, 0, 500, 700))),
        "uni0628": (600, ((0, 0, 600, 150),)),
        "uni0628.init": (600, ((0, 0, 600, 150),)),
    }
    features = "feature init { sub uni0628 by uni0628.init; } init;\nfeature rlig { sub uni0644 uni0627 by uni0644_uni0627; } rlig;"
    data = _font(glyphs, {0x0627: "uni0627", 0x0628: "uni0628", 0x0644: "uni0644"}, features)
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Spacing))
    kern = next(lookup for lookup in settled(rule.applied(settled(imported(data, face, frozendict())), original=Binary(data=data), corpus=_corpus())).lookups if isinstance(lookup, Pairs))
    assert kern.flags & LOOKUP_FLAG_IGNORE_MARKS
    assert kern.values["uni0644_uni0627", "uni0628.init"].advance >= 150 + rule.floor * 1000


def test_anchors_move_a_hamza_off_its_neighbour_and_drop_a_kasra_under_a_tail() -> None:
    """The hamza of `ئ` touching the alef after it moves to a `calt` alternate matched across marks, and a kasra over a yeh's returning tail drops below it."""
    glyphs = {
        "uni0627": (300, ((100, 0, 200, 700),)),
        "uni0627.fina": (300, ((100, 0, 300, 700),)),
        "uni0628": (600, ((0, 0, 600, 150), (265, -200, 335, -130))),
        "uni0628.init": (600, ((0, 0, 600, 150), (265, -200, 335, -130))),
        "uni0626": (500, ((0, 0, 500, 150), (200, 250, 300, 330))),
        "uni0626.init": (400, ((0, 0, 400, 150), (5, 250, 105, 330))),
        "uni064A": (500, ((0, 0, 500, 150),)),
        "uni064A.fina": (500, ((0, 0, 500, 150), (500, -300, 1000, -200))),
        "uni0650": (0, ((-60, -40, 60, 0),)),
    }
    features = "feature init { sub uni0628 by uni0628.init; sub uni0626 by uni0626.init; } init;\nfeature fina { sub uni0627 by uni0627.fina; sub uni064A by uni064A.fina; } fina;"
    data = _font(glyphs, {0x0626: "uni0626", 0x0627: "uni0627", 0x0628: "uni0628", 0x064A: "uni064A", 0x0650: "uni0650"}, features)
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Anchors))
    source = settled(rule.applied(settled(imported(data, face, frozendict())), original=Binary(data=data), corpus=_corpus()))
    dots = next(lookup for lookup in source.lookups if isinstance(lookup, Contextual))
    moved = next(target for chain in dots.rules for names, target in chain.input if names == ("uni0626.init",) and chain.lookahead == (("uni0627.fina",),) and target is not None)
    assert dots.flags & LOOKUP_FLAG_IGNORE_MARKS
    assert source.drawn(moved).contours != source.drawn("uni0626.init").contours
    drops = next(lookup for lookup in source.lookups if isinstance(lookup, Lifts) and lookup.mark_filtering == frozenset({"uni0650"}))
    assert any(
        "uni0628.init" in chain.input[0][0] and chain.lookahead and "uni064A.fina" in chain.lookahead[0] and (adjustment := chain.input[1][1]) is not None and adjustment.dy < 0
        for chain in drops.rules
    )


def test_spacing_never_kerns_a_tatweel() -> None:
    """A tatweel joins on both sides, so a kashida run stays one stroke and no pair parts it."""
    glyphs = {"uni0627": (300, ((100, 0, 200, 700),)), "uni0628": (600, ((0, 0, 600, 150),)), "uni0640": (300, ((-10, 0, 310, 100),))}
    data = _font(glyphs, {0x0627: "uni0627", 0x0628: "uni0628", 0x0640: "uni0640"})
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Spacing))
    joinings = frozendict({0x0640: (Joining.CAUSING, "No_Joining_Group"), 0x0628: (Joining.DUAL, "BEH")})
    source = settled(rule.applied(settled(imported(data, face, joinings)), original=Binary(data=data), corpus=_corpus()))
    assert not any("uni0640" in pair for lookup in source.lookups if isinstance(lookup, Pairs) for pair in lookup.values)


def test_offset_grows_an_outline_whose_raw_rim_refuses_the_boolean() -> None:
    """Mohreh Italic's sheared hamza grown by a half stem, the stroke rim the native union refused before it was simplified on the trace grid."""
    outline = tuple(
        tuple(Point(x=x, y=y, on=on) for x, y, on in contour)
        for contour in (
            (
                (-90.0413818359375, 1936.89013671875, True),
                (-157.8560028076172, 1936.89013671875, False),
                (-207.75071716308594, 1869.0755615234375, False),
                (-181.08322143554688, 1794.8248291015625, True),
                (-162.07447814941406, 1743.8013916015625, True),
                (-149.36181640625, 1709.3712158203125, False),
                (-171.96389770507812, 1678.771484375, False),
                (-202.09288024902344, 1678.771484375, True),
                (-228.10482788085938, 1678.771484375, True),
                (-181.08322143554688, 1549.712158203125, True),
                (245.11264038085938, 1549.712158203125, True),
                (198.09103393554688, 1678.771484375, True),
                (56.0257453918457, 1678.771484375, True),
                (-9.004137992858887, 1678.771484375, False),
                (-44.748207092285156, 1776.8775634765625, False),
                (-22.189058303833008, 1807.830810546875, False),
                (9.004137992858887, 1807.830810546875, True),
                (151.06942749023438, 1807.830810546875, True),
                (104.04781341552734, 1936.89013671875, True),
            ),
            (
                (227.4772186279297, 1136.5223388671875, True),
                (124.05701446533203, 1420.65283203125, True),
                (-134.06161499023438, 1420.65283203125, True),
                (-30.764169692993164, 1136.5223388671875, True),
            ),
            ((743.7144165039062, 1136.5223388671875, True), (640.2942504882812, 1420.65283203125, True), (382.1756286621094, 1420.65283203125, True), (485.5958251953125, 1136.5223388671875, True)),
        )
    )
    assert area(offset(outline, 387.17793103448275)) > area(outline)


def test_union_of_near_coincident_seam_parts_keeps_both() -> None:
    """Two Mohreh Italic seam strips sharing an edge within a hundredth of a unit, whose unsnapped union kept 1628 of their 5797 square units."""
    first = (
        (
            Point(x=9066.6669921875, y=258.0),
            Point(x=9066.6669921875, y=-0.0036072274670004845),
            Point(x=9067.3310546875, y=0.0, on=False),
            Point(x=9068.0, y=0.0),
            Point(x=9088.0, y=0.0),
            Point(x=9088.0, y=258.0),
        ),
    )
    second = (
        (
            Point(x=9066.6669921875, y=313.0),
            Point(x=9066.6669921875, y=-1.1368683772161603e-13),
            Point(x=9072.0, y=0.0),
            Point(x=9072.0, y=197.0),
            Point(x=9088.0, y=152.6056365966797),
            Point(x=9088.0, y=258.0),
            Point(x=9072.0, y=258.0),
            Point(x=9072.0, y=313.0),
        ),
    )
    assert area(union(first, second)) == pytest.approx(area(first) + (9072.0 - 9066.6669921875) * (313.0 - 258.0), abs=2.0)


def test_join_bridge_leaves_a_dot_inside_the_band_apart() -> None:
    """A final jeem's dot inside the band height near its joining edge stays a dot, the bridge running from the stroke at the edge, as Abnus ExtraBold's `ج` and `ن` keep theirs."""
    glyphs = _BEH | {
        "uni0628.medi": (600, ((0, 0, 600, 150),)),
        "uni062C": (600, ((480, 0, 600, 150), (0, -300, 600, -200), (420, 30, 460, 90))),
        "uni062C.fina": (600, ((480, 0, 600, 150), (0, -300, 600, -200), (420, 30, 460, 90))),
    }
    features = f"{_medial('uni0628')}\nfeature fina {{ sub uni062C by uni062C.fina; }} fina;"
    source = _ruled(Face(family="Probe", style="Regular", genre=TextNaskh()), _font(glyphs, {0x0628: "uni0628", 0x062C: "uni062C"}, features), Joins)
    assert len(pieces(source.drawn("uni062C.fina").contours)) == 3


@pytest.mark.parametrize("reach,found", [(math.ceil(Seam().overlap * 1000), ()), (11, ("ab at 12 ppem",))])
def test_seam_holds_a_join_centred_in_a_pixel_at_the_overlap_joins_derive(reach: int, found: tuple[str, ...]) -> None:
    """Two glyphs overlapping by twice the reach about the middle of a 12 ppem pixel composite at the band's floor or above, the former 11 units each under it."""
    glyphs = {"a": (542, ((0, 0, 542 + reach, 833),)), "b": (500, ((-reach, 0, 500, 833),))}
    findings = Seam().measured(Binary(data=_font(glyphs, {0x0061: "a", 0x0062: "b"})), corpus=_corpus(seam=("ab",)))
    assert tuple(subject for subject, _, _ in findings) == found


@pytest.mark.parametrize("tail,dots,found", [((100, 850, 160, 871), 2, ()), ((100, 850, 161, 870), 1, ("uni062A",))])
def test_identity_pairs_a_piece_across_the_fragment_floor_and_finds_a_lost_dot(tail: _Rect, dots: int, found: tuple[str, ...]) -> None:
    """A wasla tail at a quarter nuqta in the original and just over it in the build is the same piece, as on Sorb's `ٱ`, and a teh drawn with one dot loses one."""
    stem, head, teh = (100, 0, 160, 700), (60, 760, 260, 820), ((0, 0, 600, 150), (200, 250, 270, 320), (330, 250, 400, 320))
    cmap = {0x0628: "uni0628", 0x062A: "uni062A", 0x0671: "uni0671"}
    original = Binary(data=_font(_BEH | {"uni062A": (600, teh), "uni0671": (300, (stem, head, (100, 850, 161, 870)))}, cmap))
    built = Binary(data=_font(_BEH | {"uni062A": (600, teh[: 1 + dots]), "uni0671": (300, (stem, head, tail))}, cmap))
    assert tuple(subject for subject, _, _ in Identity().measured(built, original=original, corpus=_corpus(letters=tuple(cmap)))) == found


def test_locale_keeps_a_final_alef_madda_over_its_stem() -> None:
    """A final alef's madda over its stem stays, the stem read at its top, not across the joining stroke, as Pahna's `ـآ` keeps its madda apart."""
    glyphs = {"uni0627": (300, ((100, 0, 160, 700),)), "uni0622": (300, ((100, 0, 160, 700), (60, 760, 200, 800))), "uni0622.fina": (600, ((100, 0, 160, 700), (160, 0, 600, 60), (60, 760, 200, 800)))}
    features = "feature fina { sub uni0622 by uni0622.fina; } fina;"
    source = _ruled(Face(family="Probe", style="Regular", genre=TextNaskh()), _font(glyphs, {0x0627: "uni0627", 0x0622: "uni0622"}, features), Locale)
    assert len(pieces(source.drawn("uni0622.fina").contours)) == 2
    assert source.bounds("uni0622.fina") == (60.0, 0.0, 600.0, 800.0)


def test_ornament_win_metrics_cover_its_art_alone() -> None:
    """An ornament face's win metrics reach its own art and line, no corpus run shaping to glyph 0 stacking marks past it, as Tazhib's 3732 units over its 906 did."""
    data = _font({"uniE000": (1000, ((0, -200, 1000, 900),))}, {0xE000: "uniE000"})
    face = OrnamentFace(family="Probe", style="Regular")
    rule = next(rule for rule in planned(face) if isinstance(rule, Metrics))
    vertical = rule.measured(settled(imported(data, face, frozendict())), corpus=_corpus(words=("\N{ZERO WIDTH JOINER}یّٰٓ\N{ZERO WIDTH JOINER}", "ءٍّۭ")), binary=Binary(data=TTFont(BytesIO(data))))
    assert (vertical.win_ascent, vertical.win_descent) == (900, 300)


def test_spacing_tightens_no_pair_into_the_vowels_its_letters_carry() -> None:
    """A loose waw before qaf tightens toward the rhythm alef sets only until a kasra under the qaf would meet the waw's descender."""
    glyphs = {"uni0627": (300, ((50, 0, 150, 700),)), "uni0648": (400, ((100, 0, 400, 300), (-200, -300, 400, -200))), "uni0642": (500, ((0, 0, 500, 300),)), "uni0650": (0, ((-60, -40, 60, 0),))}
    data = _font(glyphs, {0x0627: "uni0627", 0x0648: "uni0648", 0x0642: "uni0642", 0x0650: "uni0650"})
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Spacing))
    joinings = frozendict({0x0627: (Joining.RIGHT, "ALEF"), 0x0648: (Joining.RIGHT, "WAW"), 0x0642: (Joining.DUAL, "QAF")})
    held = settled(imported(data, face, joinings))
    anchored = held.replaced(
        msgspec.structs.replace(held.named["uni0642"], anchors=(Anchor(name="bottom", x=450.0, y=-150.0),)),
        msgspec.structs.replace(held.named["uni0650"], anchors=(Anchor(name="_bottom", x=0.0, y=0.0),)),
    )
    bare, voweled = (
        next(lookup for lookup in settled(rule.applied(source, original=Binary(data=data), corpus=_corpus())).lookups if isinstance(lookup, Pairs)).values.get(("uni0648", "uni0642"))
        for source in (held, anchored)
    )
    assert bare is not None
    assert bare.advance < 0
    assert voweled is None or voweled.advance > bare.advance


def test_anchors_seat_a_below_mark_under_the_baseline_of_a_raised_letter() -> None:
    """An isolated hamza drawn over the baseline takes its kasratan under the baseline, where every other letter takes it."""
    glyphs = {"uni0627": (300, ((100, 0, 200, 700),)), "uni0621": (400, ((100, 150, 300, 350),)), "uni064D": (0, ((-60, -80, 60, 0),))}
    data = _font(glyphs, {0x0627: "uni0627", 0x0621: "uni0621", 0x064D: "uni064D"})
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Anchors))
    source = settled(rule.applied(settled(imported(data, face, frozendict({0x0621: (Joining.NONE, "No_Joining_Group")}))), original=Binary(data=data), corpus=_corpus()))
    assert [anchor.y <= 0 for anchor in source.named["uni0621"].anchors if anchor.name.startswith("bottom")] == [True, True]


def test_floor_holds_each_side_of_a_letter_apart() -> None:
    """A flourish over a beh too near its body rises the floor clear while the dot under it stays, as Shabtab's crescent and star do, never dragged through the body as one group."""
    glyphs = _BEH | {"uni0628.medi": (600, ((0, 0, 600, 150), (200, 160, 400, 200), (265, -200, 335, -130)))}
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Outlines))
    path = pathops.Path()
    _ruled(face, _font(glyphs, {0x0628: "uni0628"}, _medial("uni0628")), Outlines).drawn("uni0628.medi").draw(path.getPen())
    held = tuple(pathops.Path() for _ in path.contours)
    for contour, own in zip(path.contours, held, strict=True):
        contour.draw(own.getPen())
    lows = sorted(own.bounds[1] for own in held)
    assert rule.floor is not None
    assert lows[0] == pytest.approx(-200, abs=1)
    assert lows[-1] >= 150 + rule.floor * 1000 - 1


def test_anchors_move_a_dot_group_no_further_than_its_height() -> None:
    """A dash under a final beh beside a stroke descending far below it keeps its seat, where clearing the stroke vertically would drop it under the stroke's foot."""
    glyphs = {
        "uni0627": (300, ((100, 0, 200, 700),)),
        "uni0628": (600, ((0, 0, 600, 150), (250, -300, 350, -200))),
        "uni0628.init": (600, ((0, 0, 600, 150), (0, -600, 10, 150))),
        "uni0628.fina": (600, ((0, 0, 600, 150), (0, -200, 590, -170))),
    }
    features = "feature init { sub uni0628 by uni0628.init; } init;\nfeature fina { sub uni0628 by uni0628.fina; } fina;"
    data = _font(glyphs, {0x0627: "uni0627", 0x0628: "uni0628"}, features)
    face = Face(family="Probe", style="Regular", genre=TextNaskh())
    rule = next(rule for rule in planned(face) if isinstance(rule, Anchors))
    source = settled(rule.applied(settled(imported(data, face, frozendict())), original=Binary(data=data), corpus=_corpus()))
    lowest = [box[1] for glyph in source.glyphs if glyph.name.text.startswith("uni0628.fina") for box in (source.bounds(glyph.name.text),) if box]
    assert min(lowest) >= -200 - 30
