"""Native shaping, feature ranges, vertical advances, drawing, and reader lifetime."""

from io import BytesIO
from itertools import pairwise
from pathlib import Path
import sys
from time import perf_counter
lazy import gc
lazy from weakref import ref

from expression import Result
from fontTools.ttLib import TTFont
import pytest
lazy from pytest_benchmark.fixture import BenchmarkFixture

from typography.faces.faults import Ok, Reason
from typography.faces.glyphs import Point
from typography.outlines.paths import bounds, Box, union
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary, Direction

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_file_and_byte_readers_keep_native_cmap_and_mark_classes(tmp_path: Path, binary: Binary) -> None:
    """Reader snapshots preserve font bytes, code mappings, and GDEF mark membership."""
    path = tmp_path / "test.ttf"
    path.write_bytes(binary.data)
    assert Binary.read(path) == Ok(binary)
    assert (binary.face.upem, binary.order[:3]) == (1000, (".notdef", "space", "uni0628"))
    assert binary.encoded == frozendict({0x20: "space", 0x0628: "uni0628", 0x064E: "uni064E"})
    assert binary.marks == frozenset({"uni064E"})


def test_native_table_readers_keep_their_captured_layout_when_the_input_changes(binary: Binary) -> None:
    """Opened readers retain their table snapshot while a new reader sees a changed native cmap."""
    font = TTFont(BytesIO(binary.data))
    before = Binary(data=font)
    captured = before.shaped("ب")
    for table in font["cmap"].tables:
        if table.isUnicode():
            table.cmap[0x0628] = "space"
    after = Binary(data=font)
    assert before.shaped("ب") == captured
    assert captured.names == (binary.encoded[0x0628],)
    assert after.shaped("ب").names == (binary.encoded[0x20],)


@pytest.mark.parametrize("data", [b"junk", None])
def test_bad_bytes_and_missing_files_keep_the_unreadable_reason(tmp_path: Path, data: bytes | None) -> None:
    """Unreadable bytes and absent files fail at their native input boundaries."""
    path = tmp_path / "test.ttf"
    if data is not None:
        path.write_bytes(data)
    match Binary.read(path):
        case Result(error=faults):
            assert tuple(fault.reason for fault in faults) == (Reason.UNREADABLE,)
        case outcome:
            pytest.fail(str(outcome))


def test_empty_text_shapes_and_draws_as_empty_ink(binary: Binary) -> None:
    """Valid empty text reaches the native empty-run result without an absent-position failure."""
    run = binary.shaped("")
    assert (run.serialized, run.placed, binary.drawn(run)) == ("", (), ())


def test_visual_joining_order_and_utf8_clusters_match_harfbuzz_text(binary: Binary) -> None:
    """Right-to-left joining and byte clusters retain the serialization consumed by expectations."""
    run = binary.shaped("ببب", language="fa")
    assert (run.names, run.width, tuple(placed.x for placed in run.placed), run.language) == (("uni0628.fina", "uni0628.medi", "uni0628.init"), 1500, (0, 500, 1000), "fa")
    assert run.serialized == "uni0628.fina=4+500|uni0628.medi=2+500|uni0628.init=0+500"
    assert tuple(tuple(placed.cluster for placed in binary.shaped("1 ببب", direction=direction).placed) for direction in (None, "ltr")) == ((6, 4, 2, 1, 0), (0, 1, 2, 4, 6))


def test_feature_ranges_and_ordered_shaper_fallback_keep_native_semantics(binary: Binary) -> None:
    """A byte-cluster range changes one form while unknown shapers fall through to the next native engine."""
    assert binary.shaped("ببب", features=frozendict(init=False, medi=False, fina=False)).names == ("uni0628",) * 3
    assert binary.shaped("ببب", features={"init": [(0, 1, False)]}).names == ("uni0628.fina", "uni0628.medi", "uni0628")
    assert binary.shaped("ببب", shapers=("missing", "ot")).names == binary.shaped("ببب", shapers=()).names


@pytest.mark.parametrize("direction", ["ltr", "rtl", "ttb", "btt"])
def test_both_pen_axes_accumulate_advances_separately_from_offsets(binary: Binary, direction: Direction) -> None:
    """Successive origins differ by the prior glyph advance on both axes even when marks have offsets."""
    run = binary.shaped("بَب", direction=direction)
    assert run.placed
    for before, after in pairwise(run.placed):
        assert ((after.x - after.dx) - (before.x - before.dx), (after.y - after.dy) - (before.y - before.dy)) == pytest.approx((before.advance, before.y_advance))
    assert any(placed.y_advance for placed in run.placed) if direction in {"ttb", "btt"} else any(placed.advance for placed in run.placed)


def test_mark_anchors_clusters_and_joiner_forms_reach_their_drawings(binary: Binary) -> None:
    """Marks attach to the base and joiners expose every positional form without adding ink."""
    run = binary.shaped("بَ")
    assert run.serialized == "uni064E=0@250,600+0|uni0628=0+500"
    mark = next(placed for placed in run.placed if placed.glyph in binary.marks)
    assert (mark.x, mark.y, mark.dx, mark.dy) == (250, 600, 250, 600)
    assert tuple(tuple(placed.glyph for placed in cluster) for cluster in binary.shaped("بَب").clusters) == (("uni0628.fina",), ("uni064E", "uni0628.init"))
    assert tuple(tuple(name for name in binary.shaped(text).names if name.startswith("uni0628")) for text in Corpus.forms(0x0628)) == (
        ("uni0628",),
        ("uni0628.init",),
        ("uni0628.medi",),
        ("uni0628.fina",),
    )
    placed = binary.drawn(binary.shaped("ببب"))
    assert tuple(map(bounds, placed)) == tuple(Box(xmin=x, ymin=0, xmax=x + 500, ymax=100) for x in (0, 500, 1000))
    assert placed[1][0][0] == Point(x=500, y=0)
    assert bounds(union(*binary.drawn(run))) == Box(xmin=0, ymin=0, xmax=500, ymax=650)
    start = perf_counter()
    elapsed = binary.timed(("ببب", "بَب"))
    assert 0 < elapsed <= perf_counter() - start


def test_quadratic_drawing_survives_fonts_without_post_names(binary: Binary) -> None:
    """Native glyph identifiers draw the same contours when a post table omits glyph names."""
    font, stream = TTFont(BytesIO(binary.data)), BytesIO()
    font["post"].formatType = 3  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    font.save(stream)
    nameless = Binary(data=stream.getvalue())
    expected = ((Point(x=0, y=0), Point(x=500, y=0), Point(x=500, y=100, on=False), Point(x=400, y=100), Point(x=0, y=100)),)
    assert nameless.outline(nameless.encoded[0x0628]) == binary.outline(binary.encoded[0x0628]) == expected
    assert nameless.outline(nameless.encoded[0x20]) == ()


@pytest.mark.parametrize("native_tables", [False, True])
def test_font_readers_and_geometry_caches_release_with_their_owner(binary: Binary, *, native_tables: bool) -> None:
    """Both byte and native-table owners preserve shaping and leave no process cache retaining their readers."""
    temporary = Binary(data=TTFont(BytesIO(binary.data)) if native_tables else binary.data)
    owner, tables = ref(temporary), ref(temporary.tables)
    assert temporary.shaped("بَب").serialized == binary.shaped("بَب").serialized
    temporary.inked("بَب", "fa")
    temporary.drawn(temporary.shaped("ببب"))
    del temporary
    gc.collect()
    assert owner() is None
    assert tables() is None


@pytest.mark.skipif(sys.platform != "darwin", reason="CoreText shaper exists on macOS alone")
@pytest.mark.parametrize("native_tables", [False, True])
def test_coretext_and_opentype_reach_the_same_fixture_glyphs(binary: Binary, *, native_tables: bool) -> None:
    """Both font storage forms expose complete table data so native engines agree on joined marked text."""
    reader = Binary(data=TTFont(BytesIO(binary.data))) if native_tables else binary
    assert reader.shaped("ببَب", shapers=("coretext",)).names == reader.shaped("ببَب").names


@pytest.mark.parametrize("letters", [1, 1000])
def bench_shaping_keeps_each_letter_and_advance(benchmark: BenchmarkFixture, binary: Binary, letters: int) -> None:
    """Real shaping workload preserves glyph count and advance while recording native timing."""
    run = benchmark(binary.shaped, "ب" * letters)
    assert len(run.placed) == letters
    assert run.width == 500 * letters
