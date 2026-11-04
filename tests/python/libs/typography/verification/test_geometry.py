"""Geometric requirements distinguish physical changes across native languages and pixel grids."""

from io import BytesIO

from fontTools.ttLib import TTFont
import msgspec
import pytest

from tests.python.libs.typography.fonts import rectangles
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary
from typography.verification.geometry import clearance, distinct, identity, join_parity, ligature_ink, mark_line, metrics, reachable, seam

from .font import built

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_clearance_detects_mark_and_neighboring_dot_intrusions(binary: Binary, sample: Corpus) -> None:
    """Moving either a mark into its base or a dot into the next letter changes a passing clearance into overlap."""
    assert not clearance(binary, sample, 0.03)
    assert any(isinstance(measured, float) and measured < 0 for _, measured, _ in clearance(built(fatha=40), sample, 0.03))
    assert any(" uni0628" in subject for subject, _, _ in clearance(built(dot=(480, 20, 540, 80)), sample, 0.03))


def test_mark_line_measures_every_native_language(binary: Binary, sample: Corpus) -> None:
    """A Persian-only alternate rises while Arabic remains aligned, even when Arabic is measured first."""
    corpus = msgspec.structs.replace(sample, prose=frozendict(ar=sample.prose["ar"], fa=sample.prose["fa"]))
    changed = built(damma_far=60)
    assert changed.shaped("بُ", language="ar").names != changed.shaped("بُ", language="fa").names
    assert not mark_line(binary, corpus, 0.04)
    assert mark_line(changed, corpus, 0.04) == tuple((glyph, 0.06, 0.04) for glyph in ("uni0627", "uni0627.fina", "uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"))


@pytest.mark.parametrize("post_format", [2, 3])
def test_ligature_ink_rejects_an_empty_ligature_with_native_glyph_identifiers(binary: Binary, post_format: int) -> None:
    """An emptied spacing ligature fails both bounds with named and glyph-ID-only post tables."""
    assert not ligature_ink(binary, 0.5)
    font, stream = TTFont(BytesIO(built(ligature=False).data)), BytesIO()
    glyph_id = font.getGlyphID("uni0644_uni0627.fina")
    font["post"].formatType = post_format  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    font.save(stream)
    changed = Binary(data=stream.getvalue())
    glyph = changed.order[glyph_id]
    assert ligature_ink(changed, 0.5) == ((f"{glyph} ink", 0, 0.5), (f"{glyph} span", 0, 0.5))


def test_ligature_zero_references_meet_their_zero_lower_bounds(binary: Binary) -> None:
    """Empty zero-advance components and ligature meet both physical lower bounds without fabricated divisors."""
    font, stream = TTFont(BytesIO(binary.data)), BytesIO()
    for name in ("uni0644.init", "uni0627.fina", "uni0644_uni0627.fina"):
        font["glyf"][name] = rectangles()
        font["hmtx"][name] = (0, 0)
    font["post"].formatType = 3  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    font.save(stream)
    assert not ligature_ink(Binary(data=stream.getvalue()), 0.5)


def test_distinct_rejects_a_duplicated_digit(binary: Binary) -> None:
    """Replacing Persian two by one's actual outline crosses the intersection-over-union bound."""
    assert not distinct(binary, 0.9)
    assert distinct(built(two=((100, 0, 160, 500),)), 0.9) == (("uni06F1 uni06F2", 1.0, 0.9),)


def test_join_parity_detects_kern_moving_a_join(binary: Binary, sample: Corpus) -> None:
    """A kern pair pulling joined behs together changes the physical distance between their pen origins."""
    assert not join_parity(binary, sample)
    assert {subject.split(" in ")[0] for subject, _, _ in join_parity(built(kern=-40), sample)} == {"uni0628.medi uni0628.init"}


def test_seam_distinguishes_overlap_from_abutting_edges(binary: Binary, sample: Corpus) -> None:
    """Twenty-unit overlaps composite within the band; abutting bars leave a dark-ink deficit at twelve ppem."""
    band = (0.87, 1.58)
    assert not seam(binary, sample, band, (12, 16, 24, 96))
    assert any(isinstance(measured, float) and measured < band[0] for _, measured, _ in seam(built(overlap=0), sample, band, (12,)))


def test_pixel_grid_is_unchanged_by_an_outlying_glyph(sample: Corpus) -> None:
    """An unrelated digit changes run height without resizing or rephasing the pixel cells measuring beh joins."""
    font, stream = TTFont(BytesIO(built(overlap=0).data)), BytesIO()
    font["glyf"]["uni06F1"] = rectangles((100, 0, 160, 501))
    font["hmtx"]["uni06F1"] = (500, 100)
    font.save(stream)
    changed = Binary(data=stream.getvalue())
    before = msgspec.structs.replace(sample, seam=("ببب",))
    after = msgspec.structs.replace(sample, seam=("ببب۱",))
    deficits = tuple((value, bound) for _, value, bound in seam(changed, before, (0.87, 1.58), (16,)))
    assert deficits
    assert deficits == tuple((value, bound) for _, value, bound in seam(changed, after, (0.87, 1.58), (16,)))


def test_metrics_include_deep_native_mark_stacks(binary: Binary, sample: Corpus) -> None:
    """A valid stack rising past twice every individual glyph still reports the actual ascent needed by its ink."""
    changed = built(stack=200)
    text = "ب" + "َ" * 8
    corpus = msgspec.structs.replace(sample, deep=(text,))
    top = max(box.ymax for _, box in changed.inked(text, "fa"))
    tallest = max(box.ymax for name in changed.order if (box := changed.extent(name)))
    assert top > 2 * tallest
    assert any(
        subject == f"win ascent at {text}" and measured == top / changed.face.upem and bound == binary.tables["OS/2"].usWinAscent / binary.face.upem  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        for subject, measured, bound in metrics(changed, binary, corpus, None)
    )
    assert not metrics(binary, binary, sample, None)
    assert not metrics(binary, binary, sample, (1.7, 2.04))
    assert {subject.split(" at ")[0] for subject, _, _ in metrics(built(win=600), binary, sample, (1.0, 1.5))} == {"line pitch", "win ascent"}


def test_reachable_accumulates_missing_glyphs_and_native_backend_failures(binary: Binary, sample: Corpus) -> None:
    """One unavailable engine does not hide an unmapped letter under an available engine."""
    assert not reachable(binary, sample, ("ot",))
    corpus = msgspec.structs.replace(sample, words=("جب",))
    found = reachable(binary, corpus, ("missing", "ot"))
    assert sum(bound == "available shaper" for _, _, bound in found) == len(corpus.lines) * len(corpus.languages)
    assert {(subject, measured, bound) for subject, measured, bound in found if bound == 0} == {(f"جب under ot {language or 'dflt'}", 1.0, 0.0) for language in corpus.languages}


def test_identity_detects_the_original_dot_moved_to_a_disjoint_position(binary: Binary, sample: Corpus) -> None:
    """Each affected form loses its old dot and gains a disjoint one while the body stays fixed."""
    assert not identity(binary, binary, sample)
    assert identity(binary, built(dot=(60, -150, 120, -90)), sample) == tuple((glyph, 2.0, 0.0) for glyph in ("uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"))
