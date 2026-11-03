"""Plans and rules over the fixture fonts: order, resolved values, the full fold, composed code points, marks, digits, joins, dot clearance, word space, metrics, and ornament encoding."""

from functools import reduce
lazy from pathlib import Path

from expression import Result
import pytest

from typography.faces.declaration import Face, OrnamentFace
from typography.faces.glyphs import Glyph, Mark, Ornament, Source
from typography.faces.importing import imported
from typography.faces.layout import Contextual, Single
from typography.outlines.paths import bounds, Box, pieces
from typography.rules.anchors import Anchors
from typography.rules.fits import Outlines
from typography.rules.joins import Joins
from typography.rules.locale import Locale
from typography.rules.marks import QuranicMarks
from typography.rules.metrics import Metrics
from typography.rules.ornaments import Ornaments
from typography.rules.plans import applied, executed, Plan, planned
from typography.rules.scale import Scale
from typography.rules.spacing import Spacing
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

# --- [OPERATIONS] -----------------------------------------------------------------------


def _through(source: Source, plan: Plan, kind: type, /, *, original: Binary, corpus: Corpus) -> Source:
    """Applies a plan's rules up to and including the first of a kind."""
    rules = plan.rules[: next(index for index, rule in enumerate(plan.rules) if isinstance(rule, kind)) + 1]
    return reduce(lambda held, rule: applied(held, rule, original=original, corpus=corpus).ok, rules, source)


def _done(source: Source, plan: Plan, /, *, original: Binary, corpus: Corpus) -> Source:
    match executed(source, plan, original=original, corpus=corpus):
        case Result(tag="ok", ok=done):
            return done
        case Result(error=faults):
            raise AssertionError([fault.message for fault in faults])


def test_planned_orders_rules_and_resolves_values(face: Face) -> None:
    """Text plans follow the repair order, each value the face's override or its genre's default."""
    plan = planned(face)
    joins = next(rule for rule in plan.rules if isinstance(rule, Joins))
    scale = next(rule for rule in plan.rules if isinstance(rule, Scale))
    assert [type(rule).__name__ for rule in plan.rules] == ["Locale", "Digits", "Outlines", "Marks", "QuranicMarks", "Joins", "Scale", "Anchors", "Spacing", "WordSigns", "Metrics", "Records"]
    assert (joins.overlap, scale.height, scale.cap) == (face.genre.join_overlap, face.genre.alif, face.genre.pitch[1])
    assert next(rule.floor for rule in plan.rules if isinstance(rule, Outlines)) == face.values.nuqta_floor


def test_plan_refuses_anchors_before_scale(face: Face) -> None:
    """Anchors set before the scale move with it, so the plan refuses that order."""
    rules = planned(face).rules
    anchors, scale = (next(rule for rule in rules if isinstance(rule, kind)) for kind in (Anchors, Scale))
    with pytest.raises(ValueError, match="out of order"):
        Plan(genre=face.genre, treatments=(), rules=(anchors, scale))


def test_locale_composes_from_own_glyphs(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """Presentation forms alias their nominal glyph, a base and mark compose, a missing base takes a joining-group sibling's dotless body, controls blank at zero advance, and the dotted circle rings twelve nuqtas."""
    done = _through(source, planned(face), Locale, original=original, corpus=sample)
    assert [part.name for part in done.named["uniFE8F"].components] == ["uni0628"]
    assert [part.name for part in done.named["uniFE70"].components] == ["space", "uni064B"]
    assert [part.name for part in done.named["uni06C0"].components] == ["uni06D5", "uni0654"]
    assert len(pieces(done.named["uni06D5"].contours)) == 1
    assert (done.named["uni200B"].advance, done.named["uni200B"].contours) == (0.0, ())
    assert len(pieces(done.named["uni25CC"].contours)) == 12


def test_executed_runs_every_rule(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """The fold reaches the target units per em with zero mark advances, seated marks and composites, recipe marks the face lacked, and every layout feature."""
    done = _done(source, planned(face), original=original, corpus=sample)
    tanwin = next(lookup for lookup in done.lookups if isinstance(lookup, Contextual) and lookup.feature == "ccmp")
    assert done.upm == 2048
    assert all(glyph.advance == 0 for glyph in done.by_role(Mark))
    assert done.named["uni064E"].anchor("_top").is_some()
    assert {"uni0618", "uni06DB", "uni06EC", "uni08F1"} <= done.named.keys()
    assert done.named["uni06C0"].components[1].transform != (1.0, 0.0, 0.0, 1.0, 0.0, 0.0)
    assert tanwin.rules[0].replacement == ("uni064E",)
    assert {"rlig", "locl", "pnum", "tnum", "kern", "ccmp", "calt"} <= {lookup.feature for lookup in done.lookups}
    assert next(lookup for lookup in done.lookups if isinstance(lookup, Single) and lookup.feature == "tnum").mapping["uni06F1.pnum"] == "uni06F1"


def test_calt_moves_dots_clear_of_neighbour(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """Initial beh dots standing within the clearance of a following final reh move right in a `calt` alternate, inside the advance."""
    done = _through(source, planned(face), Anchors, original=original, corpus=sample)
    calt = next(lookup for lookup in done.lookups if isinstance(lookup, Contextual) and lookup.feature == "calt")
    rule = next(rule for rule in calt.rules if rule.input == (("uni0628.init",),))
    moved, held = (bounds(pieces(done.named[name].contours)[-1]) for name in (rule.replacement[0], "uni0628.init"))
    assert "uni0631.fina" in rule.lookahead[0]
    assert moved is not None
    assert held is not None
    assert moved.xmin > held.xmin
    assert moved.xmax <= done.named["uni0628.init"].advance


def test_joins_overlap_their_neighbour(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """Every joining edge of a medial form reaches the declared overlap past its advance."""
    joined = _through(source, planned(face), Joins, original=original, corpus=sample)
    medial = joined.named["uni0628.medi"]
    box = bounds(medial.contours)
    assert box is not None
    assert (box.xmin, box.xmax) == pytest.approx((-face.genre.join_overlap * joined.upm, medial.advance + face.genre.join_overlap * joined.upm))


def test_word_space_from_gap_ratio(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """Inside gap 0.080 em (the reh bearing) times the word ratio, less the 0.066 em word gap the 0.026 em space leaves, sets the space."""
    spaced = _through(source, planned(face), Spacing, original=original, corpus=sample)
    assert spaced.named["space"].advance / spaced.upm == pytest.approx(face.genre.word_ratio * 0.080 - (0.066 - 0.026), abs=1e-3)


def test_metrics_within_pitch_band(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """Typo spans the face's own corpus ink inside the pitch band, rounded to 10 units, and win reaches every mapped glyph."""
    done = _through(source, planned(face), Metrics, original=original, corpus=sample)
    metrics = done.metrics
    assert metrics is not None
    assert face.genre.pitch[0] - 0.01 <= (metrics.ascender - metrics.descender) / done.upm <= face.genre.pitch[1] + 0.01
    assert metrics.ascender % 10 == metrics.descender % 10 == 0
    assert metrics.win_ascent >= max(box.ymax for glyph in done.glyphs if glyph.name.codes and (box := bounds(glyph.contours)))


def test_quranic_marks_skip_drawn(face: Face) -> None:
    """Quranic marks plan only the required marks no recipe draws."""
    rule = next(rule for rule in planned(face).rules if isinstance(rule, QuranicMarks))
    assert 0x0613 in rule.codes
    assert not rule.codes & rule.script.recipes.keys()


def test_ornaments_reencoded(ornament: tuple[Path, OrnamentFace]) -> None:
    """Template text and blank slots leave, copies of one drawing fold, art keeps its Private Use code point or takes the next free one, tiles keep their bearings, typo stays the face's own, and win reaches the art and the typo."""
    path, face = ornament
    blank = Corpus(prose=frozendict(), marked=(), annotated=(), deep=(), words=(), digits=(), punctuation=(), joins=(), seam=(), zwnj=(), ligatures=(), letters=(), vowel_stacks=(), stacks=())
    original = Binary.read(path).ok
    plan = planned(face)
    source = imported(path, face).ok
    done = _done(source, plan, original=original, corpus=blank)
    art: tuple[Glyph, ...] = done.by_role(Ornament)
    rule = next(rule for rule in plan.rules if isinstance(rule, Ornaments))
    tile = next(glyph for glyph in art if glyph.name.codes == (rule.first + 2,))
    assert sorted(*glyph.name.codes for glyph in art) == [rule.first, rule.first + 1, rule.first + 2, 0xE010]
    assert (tile.advance, bounds(tile.contours)) == (400, Box(xmin=-20, ymin=0, xmax=410, ymax=200))
    assert done.named["space"].advance == pytest.approx(rule.space * done.upm)
    assert done.metrics is not None
    assert source.metrics is not None
    assert (done.metrics.ascender, done.metrics.descender, done.metrics.win_ascent) == (source.metrics.ascender, source.metrics.descender, max(800, source.metrics.ascender))
