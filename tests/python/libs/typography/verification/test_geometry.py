"""Geometric requirement bodies passing on the test font and breached by the one part a perturbation moves."""

import msgspec
import pytest

from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary, Shaper
from typography.verification.geometry import clearance, distinct, identity, join_parity, ligature_ink, mark_line, metrics, reachable, seam

from .conftest import built

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_clearance_holds_marks_off_their_base_and_breaks_on_a_mark_moved_into_it(binary: Binary, sample: Corpus) -> None:
    """Fatha 100 units over the bar, then sunk 60 units into it."""
    assert not clearance(binary, sample, 0.03)
    match clearance(built(fatha=40), sample, 0.03):
        case ((subject, float(measured), 0.0), *_) if " uni064" in subject and measured < 0:
            pass
        case found:
            pytest.fail(str(found))


def test_clearance_breaks_on_a_dot_under_the_next_letter(sample: Corpus) -> None:
    """Beh dot pushed past its advance into the bar of the beh beside it."""
    assert any(" uni0628" in subject for subject, _, _ in clearance(built(dot=(480, 20, 540, 80)), sample, 0.03))


def test_mark_line_holds_one_bottom_and_breaks_on_a_raised_mark(binary: Binary, sample: Corpus) -> None:
    """Fatha and damma share one bottom until the damma sits higher than the tolerance."""
    assert not mark_line(binary, sample, 0.04)
    assert mark_line(built(damma=60), sample, 0.04) == tuple((glyph, 0.06, 0.04) for glyph in ("uni0627", "uni0627.fina", "uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"))


def test_ligature_ink_holds_and_breaks_on_an_emptied_ligature(binary: Binary) -> None:
    """Lam-alef holding both strokes, then holding no ink."""
    assert not ligature_ink(binary, 0.5)
    assert {subject for subject, _, _ in ligature_ink(built(ligature=False), 0.5)} == {"uni0644_uni0627.fina ink", "uni0644_uni0627.fina span"}


def test_distinct_holds_and_breaks_on_a_duplicated_digit(binary: Binary) -> None:
    """Persian two drawn apart from Persian one, then drawn as one."""
    assert not distinct(binary, 0.9)
    assert distinct(built(two=((100, 0, 160, 500),)), 0.9) == (("uni06F1 uni06F2", 1.0, 0.9),)


def test_join_parity_holds_and_breaks_on_a_kern_moving_a_join(binary: Binary, sample: Corpus) -> None:
    """Joined behs at one distance with kern on and off, then a kern pair pulling them 40 units together."""
    assert not join_parity(binary, sample)
    assert {subject.split(" in ")[0] for subject, _, _ in join_parity(built(kern=-40), sample)} == {"uni0628.medi uni0628.init"}


def test_seam_holds_overlapping_joins_and_breaks_on_abutting_ones(binary: Binary, sample: Corpus) -> None:
    """Bars overlapping 20 units composite inside the band, abutting bars dip under it."""
    band = (0.87, 1.58)
    assert not seam(binary, sample, band, (12, 16, 24, 96))
    match seam(built(overlap=0), sample, band, (12,)):
        case (("ببب at 12 ppem", float(measured), float(low)),) if low == band[0]:
            assert measured < low
        case found:
            pytest.fail(str(found))


def test_metrics_holds_and_breaks_on_a_short_win_and_a_narrow_band(binary: Binary, sample: Corpus) -> None:
    """Pitch 1.9 em inside its band and at the original's pitch with win over the fatha on alef, then win under it and a band below the pitch."""
    assert not metrics(binary, binary, sample, None)
    assert not metrics(binary, binary, sample, (1.7, 2.04))
    assert [subject.split(" at ")[0] for subject, _, _ in metrics(built(win=600), binary, sample, (1.0, 1.5))] == ["line pitch", "win ascent"]


def test_reachable_counts_glyph_zero_under_each_shaper(binary: Binary, sample: Corpus) -> None:
    """Corpus the font maps, then a line holding an unmapped jeem."""
    assert not reachable(binary, sample, (Shaper.OT, Shaper.CORETEXT))
    assert len(reachable(binary, msgspec.structs.replace(sample, words=("جب",)), (Shaper.OT, Shaper.CORETEXT))) == 2 * len(sample.languages)


def test_identity_holds_the_original_and_breaks_on_a_moved_dot(binary: Binary, sample: Corpus) -> None:
    """Dots against the same font, then against an original whose dot sits 120 units over, each form missing its dot and the original's."""
    assert not identity(binary, binary, sample)
    assert identity(binary, built(dot=(60, -150, 120, -90)), sample) == tuple((glyph, 2.0, 0.0) for glyph in ("uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"))
