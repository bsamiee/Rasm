"""Font rules preserve mappings, spacing, geometry, and native shaping behavior."""

from pathlib import Path
lazy from io import BytesIO
lazy import math

from expression import Result
import msgspec
from pathops import PathOp
import pytest
lazy from fontTools.misc.bezierTools import segmentPointAtT
lazy from fontTools.ttLib import TTFont
lazy import ufoLib2

from tests.python.libs.typography.fonts import encoded, rectangles
from typography.builds.pipeline import compiled, constructed
from typography.faces.declaration import Face, OrnamentFace, Recipe, Values
from typography.faces.faults import Ok, Reason
from typography.faces.glyphs import Component, Metrics as Vertical, Ornament, Side, Source
from typography.faces.importing import imported
from typography.faces.names import GlyphName
from typography.outlines.measure import counter, gap
from typography.outlines.paths import area, bounds, Box, combined, pieces, segments, union
from typography.rules.anchors import anchored, Anchors
from typography.rules.fits import fitted, Outlines
from typography.rules.joins import Joins
from typography.rules.locale import Locale
from typography.rules.marks import Marks, penned
from typography.rules.metrics import Metrics, recorded, Records
from typography.rules.ornaments import Ornaments
from typography.rules.plans import executed, planned, Rules
from typography.rules.scale import Scale, scaled
from typography.rules.spacing import Spacing
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary
from typography.verification.requirements import Coverage, verified

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_explicit_zero_overrides_survive_plan_resolution(face: Face) -> None:
    """Legal zero geometry values remain overrides rather than reverting to genre defaults."""
    rules = planned(msgspec.structs.replace(face, values=Values(stack_gap=0, nuqta_floor=0, alif=0)))
    assert next(rule.floor for rule in rules if isinstance(rule, Outlines)) == 0
    assert next(rule.height for rule in rules if isinstance(rule, Scale)) == 0
    assert next(rule.stack for rule in rules if isinstance(rule, Anchors)) == 0


def test_scale_converts_all_font_unit_records_with_native_half_ties(face: Face, sample: Corpus) -> None:
    """Changing 4096 units to 2048 scales every font-unit metric while retaining angle and pixel-valued records."""
    data = encoded(dict.fromkeys((".notdef", "alef"), (500, rectangles((0, 0, 100, 100)))), {0x0627: "alef"}, "", upm=4096)
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    source = msgspec.structs.replace(
        source, metrics=Vertical(ascender=3001, descender=-1001, win_ascent=3101, win_descent=1101, x_height=1301, cap_height=2101, caret_offset=1, italic_angle=11.5, lowest_ppem=7)
    )
    done = scaled(source, Scale(script=face.genre.script, height=None, cap=4), original=original, corpus=sample).default_with(lambda faults: pytest.fail(str(faults)))
    assert done.upm == 2048
    assert done.metrics == Vertical(ascender=1501, descender=-500, win_ascent=1551, win_descent=551, x_height=651, cap_height=1051, caret_offset=1, italic_angle=11.5, lowest_ppem=7)


@pytest.mark.parametrize("order", [("far", "near"), ("near", "far")])
def test_anchor_clearance_uses_every_native_glyph_of_a_split_vowel(face: Face, sample: Corpus, order: tuple[str, str]) -> None:
    """Both multiple-substitution orders retain the nearby mark's twenty-unit gap instead of selecting one output glyph."""
    data = encoded(
        {
            ".notdef": (500, rectangles()),
            "alef": (500, rectangles((0, 0, 200, 100))),
            "fatha": (0, rectangles((0, 0, 50, 20))),
            "kasra": (0, rectangles((0, -20, 50, 0))),
            "far": (0, rectangles((0, 200, 50, 220))),
            "near": (0, rectangles((0, 20, 50, 40))),
        },
        {0x0627: "alef", 0x064E: "fatha", 0x0650: "kasra"},
        "languagesystem arab dflt; markClass [fatha far near] <anchor 0 0> @TOP; markClass kasra <anchor 0 0> @BOTTOM; "
        f"feature ccmp {{ sub fatha by {' '.join(order)}; }} ccmp; "
        "feature mark { pos base alef <anchor 100 100> mark @TOP <anchor 100 -20> mark @BOTTOM; } mark; "
        "table GDEF { GlyphClassDef [alef], , [fatha kasra far near], ; } GDEF;",
    )
    original = Binary(data=data)
    assert sorted(box.ymin for placed, box in original.inked("اَ", "") if placed.glyph in original.marks) == [120, 300]
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    corpus = msgspec.structs.replace(sample, letters=(0x0627,))
    rule = next(rule for rule in planned(face) if isinstance(rule, Anchors))
    done = anchored(source, rule, original=original, corpus=corpus).default_with(lambda faults: pytest.fail(str(faults)))
    _, _, font = constructed(done, Metrics(pitch=None, stack=rule.stack), corpus).default_with(lambda faults: pytest.fail(str(faults)))
    binary = Binary(data=font)
    run = binary.shaped("اَ")
    ink = tuple(zip(run.placed, binary.drawn(run), strict=True))
    assert gap(union(*(outline for placed, outline in ink if placed.glyph not in binary.marks)), union(*(outline for placed, outline in ink if placed.glyph in binary.marks))) == pytest.approx(
        20, rel=1e-9, abs=0
    )


def test_dependent_rules_stop_with_the_first_missing_measure(source: Source, face: Face, original: Binary, sample: Corpus) -> None:
    """Missing stem and medial ink report the first dependent rule without accumulating later faults."""
    missing = source.without(*(glyph.name.text for glyph in source.glyphs if glyph.name.codes))
    rules = tuple(rule for rule in planned(face) if isinstance(rule, Scale | Joins))
    match executed(missing, rules, original=original, corpus=sample):
        case Result(error=faults):
            assert len(faults) == 1
            assert faults[0].reason is Reason.ABSENT
        case outcome:
            pytest.fail(str(outcome))


def test_locale_composes_missing_forms_marks_controls_and_derived_art(phases: frozendict[type[Rules], Source]) -> None:
    """Required aliases and composed marks retain their parts while controls draw blank and dotted circles retain every dot."""
    done = phases[Locale]
    assert tuple(part.name for part in done.named["uniFE8F"].components) == ("uni0628",)
    assert tuple(part.name for part in done.named["uniFE70"].components) == ("space", "uni064B")
    assert tuple(part.name for part in done.named["uni06C0"].components) == ("uni06D5", "uni0654")
    assert len(pieces(done.named["uni06D5"].contours)) == 1
    assert (done.named["uni200B"].advance, done.named["uni200B"].contours) == (0, ())
    assert len(pieces(done.named["uni25CC"].contours)) == 12


def test_complete_plan_shapes_marks_and_digit_width_features(built: Binary, face: Face) -> None:
    """Generated recipes map natively, marks stay at zero advance, and both digit-width features remain reachable."""
    assert face.genre.script.recipes.keys() <= built.encoded.keys()
    assert built.face.upem == 2048
    assert tuple(placed.advance for placed in built.shaped("بَ").placed if placed.glyph in built.marks) == (0,)
    assert all(built.tables["hmtx"][name][0] == 0 for name in built.marks)
    tabular = built.shaped("۱۲", features={"tnum": True}).placed
    proportional = built.shaped("۱۲", features={"pnum": True}).placed
    assert len(tabular) == len(proportional) == 2
    assert tabular[0].advance == tabular[1].advance
    assert tuple(placed.glyph for placed in tabular) != tuple(placed.glyph for placed in proportional)
    assert built.shaped("۱۲", features={"pnum": True, "tnum": True}).names == tuple(placed.glyph for placed in tabular)
    meem = next(code for code in face.genre.script.iqlab if Side.of(code) is Side.ABOVE)
    text = f"بً{chr(meem)}"
    assert built.encoded[0x064E] in built.shaped(text).names
    assert built.encoded[0x064B] in built.shaped(text, features={"ccmp": False}).names


def test_calt_moves_neighboring_dots_within_the_advance(built: Binary) -> None:
    """A contextual alternate moves an initial beh's dot clear of the following reh."""
    enabled, disabled = built.shaped("بر"), built.shaped("بر", features={"calt": False})
    moved, held = next((a, b) for a, b in zip(enabled.placed, disabled.placed, strict=True) if a.glyph != b.glyph)
    after, before = (bounds(min(pieces(built.outline(glyph.glyph)), key=area)) for glyph in (moved, held))
    assert after is not None
    assert before is not None
    assert before.xmin < after.xmin < after.xmax <= moved.advance


def test_zero_stack_executes_real_neighbor_clearance(phases: frozendict[type[Rules], Source], face: Face, original: Binary, sample: Corpus) -> None:
    """A zero stacking gap remains usable for active contextual dot and vowel movement beside a neighboring letter."""
    rule = msgspec.structs.replace(next(rule for rule in planned(face) if isinstance(rule, Anchors)), stack=0, clearance=1)
    source = anchored(phases[Scale], rule, original=original, corpus=sample).default_with(lambda faults: pytest.fail(str(faults)))
    _, _, font = constructed(source, Metrics(pitch=None, stack=0), sample).default_with(lambda faults: pytest.fail(str(faults)))
    binary = Binary(data=font)
    assert binary.shaped("بر").names != binary.shaped("بر", features={"calt": False}).names
    alone, beside = (next(placed.y for placed in binary.shaped(text, features={"calt": False}).placed if placed.glyph == binary.encoded[0x064E]) for text in ("بَ", "بَر"))
    assert beside > alone


def test_join_edges_spacing_and_metrics_follow_measured_domains(phases: frozendict[type[Rules], Source], product: tuple[Source, ufoLib2.Font, TTFont], face: Face) -> None:
    """Joins overlap by the declared distance, spaces follow measured gaps, and metrics enclose mapped ink."""
    joined, spaced, done = phases[Joins], phases[Spacing], product[0]
    medial = joined.named["uni0628.medi"]
    box = bounds(medial.contours)
    assert box is not None
    assert (box.xmin, box.xmax) == pytest.approx((-face.genre.join_overlap * joined.upm, medial.advance + face.genre.join_overlap * joined.upm))
    assert face.genre.word_band[0] <= spaced.named["space"].advance / spaced.upm <= face.genre.word_band[1]
    assert spaced.named["space"].advance / spaced.upm == pytest.approx(face.genre.word_ratio * 0.080 - (0.066 - 0.026), abs=1e-3)
    metrics = done.metrics
    assert face.genre.pitch[0] - 0.01 <= (metrics.ascender - metrics.descender) / done.upm <= face.genre.pitch[1] + 0.01
    assert metrics.win_ascent >= max(rect[3] for rect in filter(None, map(done.bounds, done.named)))


def test_counter_opening_measures_the_hole_as_filled_ink(source: Source) -> None:
    """A twenty-unit counter expands to sixty units rather than being measured as a fill without holes."""
    glyph = msgspec.structs.replace(source.named["uni0628"], contours=(Box(xmin=0, ymin=0, xmax=500, ymax=700).contour, tuple(reversed(Box(xmin=100, ymin=100, xmax=120, ymax=300).contour))))
    done = fitted(source.replaced(glyph), Outlines(opened=0.06, letters=(0x0628,)))
    assert counter(done.named["uni0628"].contours) == pytest.approx(60, abs=0.5)


def test_counter_opening_widens_actual_voids_and_retains_the_ink_island(source: Source) -> None:
    """Opening the narrow bounded channel preserves the center island rather than subtracting its enclosing hole."""
    ink = (Box(xmin=0, ymin=0, xmax=100, ymax=100).contour, tuple(reversed(Box(xmin=10, ymin=10, xmax=90, ymax=90).contour)), Box(xmin=20, ymin=20, xmax=80, ymax=80).contour)
    glyph = msgspec.structs.replace(source.named["uni0628"], contours=ink)
    opened = fitted(source.replaced(glyph), Outlines(merge=0, sliver=0, opened=0.03, letters=(0x0628,))).named["uni0628"].contours
    assert counter(opened) >= 30 - 0.5
    assert area(opened) < area(ink)
    assert area(combined(opened, (Box(xmin=45, ymin=45, xmax=55, ymax=55).contour,), PathOp.INTERSECTION)) == 100


def test_dot_floor_solves_the_actual_diagonal_clearance(source: Source) -> None:
    """A three-by-four diagonal gap grows to the requested clearance under vertical movement."""
    glyph = msgspec.structs.replace(source.named["uni0628"], contours=(Box(xmin=0, ymin=0, xmax=100, ymax=100).contour, Box(xmin=103, ymin=104, xmax=113, ymax=114).contour))
    done = fitted(source.replaced(glyph), Outlines(merge=0, sliver=0, floor=0.01, dot=0x0628))
    body, dot = sorted(pieces(done.named["uni0628"].contours), key=area, reverse=True)
    assert gap(body, dot) == pytest.approx(10, abs=1e-3)


def test_composed_hamza_moves_the_following_vowel_above_its_own_ink(built: Binary, face: Face) -> None:
    """A composed heh-hamza keeps a following fatha above the hamza rather than sharing its attachment point."""
    run = built.shaped("ۀَ")
    ink = tuple(zip(run.placed, built.drawn(run), strict=True))
    base = union(*(contours for placed, contours in ink if placed.glyph not in built.marks))
    marks = union(*(contours for placed, contours in ink if placed.glyph in built.marks))
    assert base
    assert marks
    assert gap(base, marks) >= face.genre.stack_gap * built.face.upem - 0.5


def test_win_metrics_enclose_nested_fractional_component_ink(source: Source, sample: Corpus) -> None:
    """Component-only aliases retain transformed nested ink, and Win ascent rounds its fractional top outward."""
    base = msgspec.structs.replace(source.named[".notdef"], contours=(Box(xmin=0, ymin=0, xmax=100, ymax=100).contour,), anchors=())
    alias = msgspec.structs.replace(
        source.named["uni0628"], name=GlyphName(stem="alias"), codes=frozenset(), contours=(), anchors=(), components=(Component(name=".notdef", transform=(1, 0, 0, 1, 0, 1000.25)),)
    )
    nested = msgspec.structs.replace(alias, name=GlyphName(stem=(0x0628,)), codes=frozenset({0x0628}), components=(Component(name="alias", transform=(1, 0, 0, 1, 0, 1000.25)),))
    drawn = msgspec.structs.replace(source, glyphs=(base, alias, nested), lookups=())
    done, _, font = constructed(drawn, Metrics(pitch=None, stack=0), msgspec.structs.replace(sample, marked=(), vowel_stacks=())).default_with(lambda faults: pytest.fail(str(faults)))
    assert drawn.bounds("uni0628") == (0, 2000.5, 100, 2100.5)
    assert done.metrics.win_ascent == 2101
    binary = Binary(data=font)
    ink = binary.extent(binary.encoded[0x0628])
    assert ink is not None
    assert ink.ymax <= done.metrics.win_ascent


def test_counter_records_follow_nested_component_geometry(source: Source) -> None:
    """A selected component-only letter uses its transformed forty-unit counter for minimum readable size."""
    ring = msgspec.structs.replace(
        source.named[".notdef"], name=GlyphName(stem="ring"), contours=(Box(xmin=0, ymin=0, xmax=100, ymax=100).contour, tuple(reversed(Box(xmin=40, ymin=40, xmax=60, ymax=60).contour)))
    )
    alias = msgspec.structs.replace(ring, name=GlyphName(stem="alias"), contours=(), components=(Component(name="ring", transform=(2, 0, 0, 2, 0, 0)),))
    glyph = msgspec.structs.replace(source.named["uni0628"], contours=(), components=(Component(name="alias", transform=(1, 0, 0, 1, 10, 20)),))
    done = recorded(source.replaced(ring, alias, glyph), Records(letters=(0x0628,)))
    assert done.metrics.lowest_ppem == math.ceil(source.upm / 40)


def test_native_metrics_include_the_selected_tall_ligature(face: Face, sample: Corpus) -> None:
    """Actual GSUB-selected ink determines plain-text ascent even when its nominal component glyphs are short."""
    data = encoded(
        {**dict.fromkeys((".notdef", "lam", "alef"), (500, rectangles((0, 0, 100, 100)))), "lamalef": (1000, rectangles((0, 0, 100, 2500)))},
        {0x0644: "lam", 0x0627: "alef"},
        "languagesystem arab dflt; feature rlig { sub lam alef by lamalef; } rlig;",
    )
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    corpus = msgspec.structs.replace(sample, prose=frozendict(ar=("لا",)), marked=(), annotated=(), deep=())
    done, ufo, font = constructed(source, Metrics(pitch=(2.5, 3), stack=0), corpus).default_with(lambda faults: pytest.fail(str(faults)))
    binary = Binary(data=font)
    assert len(binary.shaped("لا").placed) == 1
    top = max(box.ymax for _, box in binary.inked("لا", "ar"))
    assert top == 2500
    assert max(box.ymax for placed in binary.shaped("لا", features={"rlig": False}).placed if (box := binary.extent(placed.glyph))) == 100
    assert done.metrics.ascender >= top
    assert font["hhea"].ascent >= top
    assert ufo.info.ascender is not None
    assert ufo.info.ascender >= top


@pytest.mark.parametrize("drawn", [False, True])
def test_missing_ring_recipe_is_circular_even_when_own_marks_are_kept(source: Source, face: Face, *, drawn: bool) -> None:
    """A missing sukun receives circular ink while the drawn setting preserves existing recipe ink."""
    rule = next(rule for rule in planned(face) if isinstance(rule, Marks))
    script = msgspec.structs.replace(rule.script, recipes=frozendict({0x064E: Recipe.BAR, 0x0652: Recipe.RING}))
    missing = source.replaced(msgspec.structs.replace(source.named["uni0652"], contours=()))
    done = penned(missing, msgspec.structs.replace(rule, script=script, drawn=drawn)).default_with(lambda faults: pytest.fail(str(faults)))
    ink = done.named["uni0652"].contours
    if drawn:
        assert done.named["uni064E"].contours == source.named["uni064E"].contours
    box = bounds(ink)
    assert box is not None
    assert box.width == pytest.approx(box.height, abs=0.01)
    radii = tuple(math.dist(box.middle, segmentPointAtT(segment, at)) for segment in segments((ink[0],)) for at in (0, 0.5, 1))  # type: ignore[no-untyped-call]
    assert max(radii) - min(radii) < 1


def test_ornament_encoding_and_coverage_preserve_original_pua_gaps(ornament: tuple[Path, OrnamentFace], sample: Corpus, tmp_path: Path) -> None:
    """Duplicate art merges, tiles keep their placement, and a preserved PUA code remains valid across a gap."""
    path, face = ornament
    original = Binary.read(path).default_with(lambda faults: pytest.fail(str(faults)))
    source = imported(original.data, face).default_with(lambda faults: pytest.fail(str(faults)))
    done = executed(source, planned(face), original=original, corpus=sample).default_with(lambda faults: pytest.fail(str(faults)))
    art = done.by_role(Ornament)
    assert sorted(*glyph.codes for glyph in art) == [face.first, face.first + 1, face.first + 2, 0xE010]
    tile = next(glyph for glyph in art if glyph.codes == frozenset({face.first + 2}))
    assert (tile.advance, bounds(tile.contours)) == (400, Box(xmin=-20, ymin=0, xmax=410, ymax=200))
    done, _, native = constructed(done, Metrics(pitch=None, stack=0), sample).default_with(lambda faults: pytest.fail(str(faults)))
    rule = next(rule for rule in planned(face) if isinstance(rule, Ornaments))
    assert done.named["space"].advance == pytest.approx(rule.space * done.upm)
    assert (done.metrics.ascender, done.metrics.descender) == (source.metrics.ascender, source.metrics.descender)
    binary = Binary(data=compiled(done, native).default_with(lambda faults: pytest.fail(str(faults))))
    built_path = tmp_path / "Orn.ttf"
    built_path.write_bytes(binary.data)
    assert not verified(binary, (Coverage(),), source=done, original=original, corpus=sample, path=built_path, approved=tmp_path / "approval.json", checks=Ok(()))
    font, stream = TTFont(BytesIO(binary.data)), BytesIO()
    for table in font["cmap"].tables:
        if table.isUnicode():
            table.cmap.pop(0xE010, None)
    font.save(stream)
    missing = Binary(data=stream.getvalue())
    findings = verified(missing, (Coverage(),), source=done, original=original, corpus=sample, path=built_path, approved=tmp_path / "approval.json", checks=Ok(()))
    assert tuple((finding.subject, finding.measured) for finding in findings) == (("U+E010", "unmapped"),)
