"""Mark anchors and dot alternates of the `Anchors` rebuild: a hamza moved off its neighbour, a kasra dropped under a tail, below marks under the baseline, and dot groups kept near their seat."""

import functools

from fontTools.otlLib.builder import LOOKUP_FLAG_IGNORE_MARKS

from rasm.typography import Anchors, Contextual, Face, Joining, Lifts, Single, Source, TextNaskh

# --- [CONSTANTS] ------------------------------------------------------------------------

NASKH = Face(family="Probe", style="Regular", genre=TextNaskh())

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_anchors_move_a_hamza_off_its_neighbour_and_drop_a_kasra_under_a_tail(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
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
    source = rebuilt(NASKH, font(glyphs, {0x0626: "uni0626", 0x0627: "uni0627", 0x0628: "uni0628", 0x064A: "uni064A", 0x0650: "uni0650"}, features), Anchors)
    dots = next(lookup for lookup in source.lookups if isinstance(lookup, Contextual))
    moved = next(
        single.mapping["uni0626.init"]
        for chain in dots.rules
        for names, applied in chain.input
        if names == ("uni0626.init",) and chain.lookahead == (("uni0627.fina",),)
        for single in applied
        if isinstance(single, Single)
    )
    assert dots.flags & LOOKUP_FLAG_IGNORE_MARKS
    assert source.drawn(moved).contours != source.drawn("uni0626.init").contours
    drops = next(lookup for lookup in source.lookups if isinstance(lookup, Lifts) and lookup.mark_filtering == frozenset({"uni0650"}))
    assert any(
        "uni0628.init" in chain.input[0][0] and chain.lookahead and "uni064A.fina" in chain.lookahead[0] and (adjustment := chain.input[1][1]) is not None and adjustment.dy < 0
        for chain in drops.rules
    )


def test_anchors_seat_a_below_mark_under_the_baseline_of_a_raised_letter(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """An isolated hamza drawn over the baseline takes its kasratan under the baseline, where every other letter takes it."""
    glyphs = {"uni0627": (300, ((100, 0, 200, 700),)), "uni0621": (400, ((100, 150, 300, 350),)), "uni064D": (0, ((-60, -80, 60, 0),))}
    data = font(glyphs, {0x0627: "uni0627", 0x0621: "uni0621", 0x064D: "uni064D"})
    source = rebuilt(NASKH, data, Anchors, joinings=frozendict({0x0621: (Joining.NONE, "No_Joining_Group")}))
    assert [anchor.y <= 0 for anchor in source.named["uni0621"].anchors if anchor.name.startswith("bottom")] == [True, True]


def test_anchors_move_a_dot_group_no_further_than_its_height(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A dash under a final beh beside a stroke descending far below it keeps its seat, where clearing the stroke vertically would drop it under the stroke's foot."""
    glyphs = {
        "uni0627": (300, ((100, 0, 200, 700),)),
        "uni0628": (600, ((0, 0, 600, 150), (250, -300, 350, -200))),
        "uni0628.init": (600, ((0, 0, 600, 150), (0, -600, 10, 150))),
        "uni0628.fina": (600, ((0, 0, 600, 150), (0, -200, 590, -170))),
    }
    features = "feature init { sub uni0628 by uni0628.init; } init;\nfeature fina { sub uni0628 by uni0628.fina; } fina;"
    source = rebuilt(NASKH, font(glyphs, {0x0627: "uni0627", 0x0628: "uni0628"}, features), Anchors)
    lowest = [box[1] for glyph in source.glyphs if glyph.name.text.startswith("uni0628.fina") for box in (source.bounds(glyph.name.text),) if box]
    assert min(lowest) >= -200 - 30
