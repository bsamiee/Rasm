"""Requirements measured over compiled binaries, each catching one defect a probe draws: tails under the next part, kasras over the baseline, stencil strokes at a join, seams at the pixel, and lost dots."""

import functools
import math

import msgspec
import pytest

from rasm.typography import Binary, Corpus, Identity, MarkClearance, MarkSide, PartClearance, Seam

# --- [TYPES] ----------------------------------------------------------------------------

type _Rect = tuple[int, int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------

BEH = {"uni0628": (600, ((0, 0, 600, 150), (265, -200, 335, -130)))}

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_part_clearance_finds_a_tail_under_the_next_part(font: functools.partial[bytes], corpus: Corpus) -> None:
    """A returning yeh drawn under the next part, the collision across ZWNJ no published checker reports."""
    glyphs = {"uni06CC": (400, ((-300, -100, 400, 100),)), "uni062E": (400, ((0, 0, 400, 300),))}
    binary = Binary(data=font(glyphs, {0x06CC: "uni06CC", 0x062E: "uni062E"}))
    found = PartClearance(gap=0.03, dotted=True).measured(binary, corpus=msgspec.structs.replace(corpus, zwnj=("یخ",)), joinings=frozendict())
    assert [subject for subject, _, _ in found] == ["uni062E uni06CC in یخ"]


def test_mark_side_finds_a_kasra_over_the_baseline(font: functools.partial[bytes], corpus: Corpus) -> None:
    """A kasra seated above the baseline, as Mansub's original seats it on `لا`."""
    glyphs = {"uni0628": (600, ((0, 0, 600, 150),)), "uni0650": (0, ((-50, 0, 50, 50),))}
    features = "markClass uni0650 <anchor 0 0> @BELOW;\nfeature mark { pos base uni0628 <anchor 300 100> mark @BELOW; } mark;\ntable GDEF { GlyphClassDef [uni0628], , [uni0650], ; } GDEF;"
    found = MarkSide.measured(Binary(data=font(glyphs, {0x0628: "uni0628", 0x0650: "uni0650"}, features)), corpus=msgspec.structs.replace(corpus, ligatures=("بِ",)))
    assert [subject for subject, _, _ in found] == ["uni0650 in بِ"]


@pytest.mark.parametrize("stroke,found", [((-11, 0, 100, 150), ()), ((-11, 200, 100, 350), ("uni0627.fina uni0628.init in با",))])
def test_mark_clearance_reads_a_stencil_stroke_at_a_join_as_body(font: functools.partial[bytes], corpus: Corpus, stroke: _Rect, found: tuple[str, ...]) -> None:
    """A piece of dot size crossing the baseline at its joining edge is a stencil face's joining stroke and overlaps its neighbour by the join, one above the baseline a loose piece colliding with it."""
    glyphs = {
        "uni0628": (600, ((0, 0, 600, 150), (265, -200, 335, -130))),
        "uni0628.init": (600, ((120, 0, 600, 150), stroke, (300, -200, 370, -130))),
        "uni0627": (200, ((50, 0, 150, 700),)),
        "uni0627.fina": (200, ((50, 0, 200, 700),)),
    }
    features = "feature init { sub uni0628 by uni0628.init; } init;\nfeature fina { sub uni0627 by uni0627.fina; } fina;"
    binary = Binary(data=font(glyphs, {0x0628: "uni0628", 0x0627: "uni0627"}, features))
    measured = MarkClearance(gap=0.03, dotted=True).measured(binary, corpus=msgspec.structs.replace(corpus, words=("با",)), joinings=frozendict())
    assert tuple(subject for subject, _, _ in measured) == found


@pytest.mark.parametrize("reach,found", [(math.ceil(Seam().overlap * 1000), ()), (11, ("ab at 12 ppem",))])
def test_seam_holds_a_join_centred_in_a_pixel_at_the_overlap_joins_derive(font: functools.partial[bytes], corpus: Corpus, reach: int, found: tuple[str, ...]) -> None:
    """Two glyphs overlapping by twice the reach about the middle of a 12 ppem pixel composite at the band's floor or above, the former 11 units each under it."""
    glyphs = {"a": (542, ((0, 0, 542 + reach, 833),)), "b": (500, ((-reach, 0, 500, 833),))}
    findings = Seam().measured(Binary(data=font(glyphs, {0x0061: "a", 0x0062: "b"})), corpus=msgspec.structs.replace(corpus, seam=("ab",)))
    assert tuple(subject for subject, _, _ in findings) == found


@pytest.mark.parametrize("tail,dots,found", [((100, 850, 160, 871), 2, ()), ((100, 850, 161, 870), 1, ("uni062A",))])
def test_identity_pairs_a_piece_across_the_fragment_floor_and_finds_a_lost_dot(font: functools.partial[bytes], corpus: Corpus, tail: _Rect, dots: int, found: tuple[str, ...]) -> None:
    """A wasla tail at a quarter nuqta in the original and just over it in the build is the same piece, as on Sorb's `ٱ`, and a teh drawn with one dot loses one."""
    stem, head, teh = (100, 0, 160, 700), (60, 760, 260, 820), ((0, 0, 600, 150), (200, 250, 270, 320), (330, 250, 400, 320))
    cmap = {0x0628: "uni0628", 0x062A: "uni062A", 0x0671: "uni0671"}
    original = Binary(data=font(BEH | {"uni062A": (600, teh), "uni0671": (300, (stem, head, (100, 850, 161, 870)))}, cmap))
    built = Binary(data=font(BEH | {"uni062A": (600, teh[: 1 + dots]), "uni0671": (300, (stem, head, tail))}, cmap))
    assert tuple(subject for subject, _, _ in Identity().measured(built, original=original, corpus=msgspec.structs.replace(corpus, letters=tuple(cmap)))) == found
