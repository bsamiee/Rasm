"""Shaped runs, their `hb-shape` serialization, glyph drawing, and placed outlines over the test font."""

from pathlib import Path
import sys

from expression import Ok, Result
import pytest

from typography.faces.faults import Fault, Reason
from typography.faces.glyphs import Point
from typography.outlines.paths import bounds, Box, union
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary, drawn, encoded, marks, outline, shaped, Shaper, timed

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_binary_reads_order_and_units(binary: Binary) -> None:
    """Glyph order from the font and its units per em."""
    assert (binary.order[:3], binary.upm) == ((".notdef", "space", "uni0628"), 1000)


def test_binary_reads_a_font_file_and_its_marks(tmp_path: Path, binary: Binary) -> None:
    """Font file read to the same record, its cmap, and the fatha alone classed as a mark."""
    path = tmp_path / "test.ttf"
    path.write_bytes(binary.data)
    assert (Binary.read(path), marks(binary), encoded(binary)) == (Ok(binary), frozenset({"uni064E"}), frozendict({0x20: "space", 0x0628: "uni0628", 0x064E: "uni064E"}))


@pytest.mark.parametrize("data", [b"junk", None])
def test_binary_refuses_bytes_outside_a_font_and_a_missing_file(tmp_path: Path, data: bytes | None) -> None:
    """Bytes with no sfnt header and a missing file, both unreadable."""
    path = tmp_path / "test.ttf"
    if data is not None:
        path.write_bytes(data)
    match Binary.read(path):
        case Result(error=(Fault(reason=Reason.UNREADABLE),)):
            pass
        case refused:
            pytest.fail(f"{refused}")


def test_word_takes_joining_forms_in_visual_order(binary: Binary) -> None:
    """Three behs, final first in visual order, each advancing the pen by its width."""
    run = shaped(binary, "ببب", language="fa")
    assert (run.names, run.width, [placed.x for placed in run.placed], run.language) == (("uni0628.fina", "uni0628.medi", "uni0628.init"), 1500, [0, 500, 1000], "fa")
    assert run.serialized == "uni0628.fina=4+500|uni0628.medi=2+500|uni0628.init=0+500"


def test_features_switched_off_keep_isolated_forms(binary: Binary) -> None:
    """Joining features off, every beh isolated."""
    assert shaped(binary, "ببب", features=frozendict(init=False, medi=False, fina=False)).names == ("uni0628",) * 3


def test_mark_sits_on_base_anchor_with_offsets_serialized(binary: Binary) -> None:
    """Fatha placed at the beh's top anchor, its offsets written after its cluster."""
    run = shaped(binary, "بَ")
    assert run.serialized == "uni064E=0@250,600+0|uni0628=0+500"
    assert (run.placed[0].x, run.placed[0].y, run.placed[0].dx, run.placed[0].dy) == (250, 600, 250, 600)
    assert tuple(tuple(placed.glyph for placed in cluster) for cluster in shaped(binary, "بَب").clusters) == (("uni0628.fina",), ("uni064E", "uni0628.init"))


def test_direction_guessed_from_text_unless_given(binary: Binary) -> None:
    """Arabic after a digit guessed right to left, byte clusters descending, a given left-to-right direction ascending."""
    assert tuple(tuple(placed.cluster for placed in shaped(binary, "1 ببب", direction=direction).placed) for direction in (None, "ltr")) == ((6, 4, 2, 1, 0), (0, 1, 2, 4, 6))


def test_forms_through_joiner_select_each_form(binary: Binary) -> None:
    """Beh wrapped in U+200D on each joining side, shaped to its isolated, initial, medial, and final glyph."""
    forms = Corpus.forms(0x0628)
    assert tuple(tuple(name for name in shaped(binary, text).names if name.startswith("uni0628")) for text in forms) == (("uni0628",), ("uni0628.init",), ("uni0628.medi",), ("uni0628.fina",))


@pytest.mark.skipif(sys.platform != "darwin", reason="CoreText shaper exists on macOS alone")
def test_coretext_shaper_reaches_the_same_glyphs(binary: Binary) -> None:
    """Word with a mark, same glyphs under the `ot` and `coretext` shapers."""
    assert shaped(binary, "ببَب", shaper=Shaper.CORETEXT).names == shaped(binary, "ببَب").names


def test_one_pass_timed_above_zero(binary: Binary) -> None:
    """Ten runs of three behs, their pass timed above zero."""
    assert timed(binary, ("ببب",) * 10) > 0


def test_outline_keeps_quadratic_points_as_drawn(binary: Binary) -> None:
    """Beh contour with its off-curve corner, the closing point dropped, and an empty glyph with no contour."""
    assert outline(binary, "uni0628") == ((Point(x=0, y=0), Point(x=500, y=0), Point(x=500, y=100, on=False), Point(x=400, y=100), Point(x=0, y=100)),)
    assert outline(binary, "space") == ()


def test_drawn_outlines_follow_placed_positions(binary: Binary) -> None:
    """Joined word, each glyph box at its pen position and their union across the word width."""
    placed = drawn(binary, shaped(binary, "ببب"))
    assert tuple(map(bounds, placed)) == tuple(Box(xmin=x, ymin=0, xmax=x + 500, ymax=100) for x in (0, 500, 1000))
    assert placed[1][0][0] == Point(x=500, y=0)
    assert bounds(union(*placed)) == Box(xmin=0, ymin=0, xmax=1500, ymax=100)
