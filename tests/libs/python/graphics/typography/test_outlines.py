"""Outline rebuilds of `Outlines` and `Joins`: dots kept apart from spurs, dashes keeping their shape, redrawn dots kept apart, the floor held per side, and join bridges leaving dots apart."""

from collections.abc import Callable
import functools

import pathops
import pytest

from rasm.typography import Face, Joins, Outlines, pieces, Source, TextNaskh

# --- [CONSTANTS] ------------------------------------------------------------------------

NASKH = Face(family="Probe", style="Regular", genre=TextNaskh())
BEH = {"uni0628": (600, ((0, 0, 600, 150), (265, -200, 335, -130)))}

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_spur_cut_keeps_dots_under_the_join_band(font: functools.partial[bytes], rebuilt: functools.partial[Source], outers: Callable[[Source, str], tuple[tuple[float, float], ...]]) -> None:
    """Dots of a medial yeh narrower than half the join band stay dots, no longer cut as needle spurs, so `جیم` never reads `جبم`."""
    glyphs = BEH | {"uni06CC": (500, ((0, 0, 500, 150),)), "uni06CC.medi": (500, ((0, 0, 500, 150), (150, -220, 210, -160), (290, -220, 350, -160)))}
    source = rebuilt(NASKH, font(glyphs, {0x0628: "uni0628", 0x06CC: "uni06CC"}, "feature medi { sub uni06CC by uni06CC.medi; } medi;"), Outlines)
    assert len(outers(source, "uni06CC.medi")) == 3


def test_two_dot_dash_keeps_its_shape(font: functools.partial[bytes], rebuilt: functools.partial[Source], outers: Callable[[Source, str], tuple[tuple[float, float], ...]]) -> None:
    """A 120 by 50 two-dot dash of nuqta area keeps its 2.4 elongation where a single round dot of elongation 1 would turn `ت` into `ن`."""
    glyphs = BEH | {"uni062A": (500, ((0, 0, 500, 150),)), "uni062A.medi": (500, ((0, 0, 500, 150), (190, 250, 310, 300)))}
    source = rebuilt(NASKH, font(glyphs, {0x0628: "uni0628", 0x062A: "uni062A"}, "feature medi { sub uni062A by uni062A.medi; } medi;"), Outlines)
    (_, dash), *body = outers(source, "uni062A.medi")
    assert len(body) == 1
    assert dash == pytest.approx(120 / 50, rel=0.02)


def test_redrawn_dots_stay_apart(font: functools.partial[bytes], rebuilt: functools.partial[Source], outers: Callable[[Source, str], tuple[tuple[float, float], ...]]) -> None:
    """Dots redrawn at the larger nuqta of beh spread from their middle until each keeps its original gap, so none merge."""
    glyphs = BEH | {"uni062B": (500, ((0, 0, 500, 150),)), "uni062B.medi": (500, ((0, 0, 500, 150), (180, 250, 240, 310), (250, 250, 310, 310)))}
    source = rebuilt(NASKH, font(glyphs, {0x0628: "uni0628", 0x062B: "uni062B"}, "feature medi { sub uni062B by uni062B.medi; } medi;"), Outlines)
    assert len(outers(source, "uni062B.medi")) == 3


def test_floor_holds_each_side_of_a_letter_apart(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A flourish drawn 10 units over a beh's body rises off it while the dot under the body stays at -200, as Shabtab's crescent and star do, never dragged through the body as one group."""
    glyphs = BEH | {"uni0628.medi": (600, ((0, 0, 600, 150), (200, 160, 400, 200), (265, -200, 335, -130)))}
    path = pathops.Path()
    rebuilt(NASKH, font(glyphs, {0x0628: "uni0628"}, "feature medi { sub uni0628 by uni0628.medi; } medi;"), Outlines).drawn("uni0628.medi").draw(path.getPen())
    held = tuple(pathops.Path() for _ in path.contours)
    for contour, own in zip(path.contours, held, strict=True):
        contour.draw(own.getPen())
    (dot, body, flourish) = sorted((own.bounds for own in held), key=lambda bounds: bounds[1])
    assert (dot[1], body[1:4:2]) == (pytest.approx(-200, abs=1), (0, 150))
    assert flourish[1] > 160
    assert flourish[3] - flourish[1] == pytest.approx(40, abs=1)


def test_join_bridge_leaves_a_dot_inside_the_band_apart(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A final jeem's dot inside the band height near its joining edge stays a dot, the bridge running from the stroke at the edge, as Abnus ExtraBold's `ج` and `ن` keep theirs."""
    glyphs = BEH | {
        "uni0628.medi": (600, ((0, 0, 600, 150),)),
        "uni062C": (600, ((480, 0, 600, 150), (0, -300, 600, -200), (420, 30, 460, 90))),
        "uni062C.fina": (600, ((480, 0, 600, 150), (0, -300, 600, -200), (420, 30, 460, 90))),
    }
    features = "feature medi { sub uni0628 by uni0628.medi; } medi;\nfeature fina { sub uni062C by uni062C.fina; } fina;"
    source = rebuilt(NASKH, font(glyphs, {0x0628: "uni0628", 0x062C: "uni062C"}, features), Joins)
    assert len(pieces(source.drawn("uni062C.fina").contours)) == 3
