"""Native compilation and shaping give emitted substitutions and positioning their meaning."""

import msgspec
import pytest
lazy from ufo2ft import compileTTF
lazy from ufo2ft.featureWriters import CursFeatureWriter, GdefFeatureWriter, MarkFeatureWriter

from tests.python.libs.typography.fonts import encoded, rectangles
from typography.builds.pipeline import compiled, constructed
from typography.faces.glyphs import Source
from typography.faces.layout import Adjustment, Chain, Contextual, Lifts, Ligatures, Pairs, Positions, Single
from typography.rules.metrics import Metrics
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary, Direction

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_every_lookup_kind_changes_the_native_run_it_addresses(source: Source, sample: Corpus) -> None:
    """Compiled single, ligature, contextual, single-position, pair, and chained-position lookups preserve their operands."""
    lookups = (
        Single(features=frozendict({"ss01": frozendict({("arab", "FAR "): False})}), mapping=frozendict({"uni0628": "uni0631"})),
        Single(features=frozendict({"foo ": frozendict()}), mapping=frozendict({"uni0628": "uni0631"})),
        Ligatures(features=frozendict({"ss02": frozendict()}), rules=((("uni0644", "uni0627"), "uni0644_uni0627"),)),
        Contextual(features=frozendict({"ss03": frozendict()}), rules=(Chain(input=((("uni0628",), "uni0627"),), lookahead=(("uni0631",),)),)),
        Positions(features=frozendict({"ss04": frozendict()}), values=frozendict({"uni0628": Adjustment(dx=20, dy=30, advance=40)})),
        Pairs(features=frozendict({"ss05": frozendict()}), values=frozendict({("uni0628", "uni0631"): Adjustment(dx=20, advance=-30)})),
        Lifts(features=frozendict({"ss06": frozendict()}), rules=(Chain(input=((("uni0628",), None), (("uni064E",), Adjustment(dy=40))), lookahead=(("uni0631",),)),)),
    )
    binary = Binary(
        data=constructed(msgspec.structs.replace(source, lookups=lookups), Metrics(pitch=None, stack=0), sample)
        .bind(lambda product: compiled(product[0], product[2]))
        .default_with(lambda faults: pytest.fail(str(faults)))
    )
    assert binary.shaped("ب", language="fa", features={"ss01": True}, direction="ltr").names == ("uni0631",)
    assert binary.shaped("ب", language="ar", features={"ss01": True}, direction="ltr").names == ("uni0628",)
    assert binary.shaped("ب", features={"foo ": True}, direction="ltr").names == ("uni0631",)
    assert binary.shaped("لا", features={"ss02": True}, direction="rtl").names == ("uni0644_uni0627",)
    assert binary.shaped("بر", features={"ss03": True}, direction="rtl").names == ("uni0631", "uni0627")
    assert binary.shaped("بب", features={"ss03": True}, direction="rtl").names == ("uni0628", "uni0628")
    before = binary.shaped("ب", direction="ltr").placed[0]
    after = binary.shaped("ب", features={"ss04": True}, direction="ltr").placed[0]
    assert (after.x - before.x, after.y - before.y, after.advance - before.advance) == (20, 30, 40)
    before_pair = binary.shaped("ابر", direction="rtl").placed
    after_pair = binary.shaped("ابر", features={"ss05": True}, direction="rtl").placed
    assert (after_pair[1].x - before_pair[1].x, after_pair[1].advance - before_pair[1].advance, after_pair[2].x - before_pair[2].x) == (20, -30, -30)
    before_marks = binary.shaped("بَر", direction="rtl").placed
    after_marks = binary.shaped("بَر", features={"ss06": True}, direction="rtl").placed
    assert tuple(after.y - before.y for before, after in zip(before_marks, after_marks, strict=True)) == (0, 40, 0)


@pytest.mark.parametrize("sparse", [False, True])
def test_native_pair_compression_preserves_the_complete_sparse_relation(source: Source, sample: Corpus, *, sparse: bool) -> None:
    """Native compilation preserves each pair's full adjustment and every absent cell while compressing its lookup."""
    pairs = {**{(first, second): -30 for first in ("uni0631", "uni0631.fina") for second in ("uni0628", "uni0628.init")}, **({("uni0627", "uni0628"): -15} if sparse else {})}
    source = msgspec.structs.replace(
        source,
        lookups=(
            Single(features=frozendict({"ss01": frozendict()}), mapping=frozendict({"uni0631": "uni0631.fina"})),
            Single(features=frozendict({"ss02": frozendict()}), mapping=frozendict({"uni0628": "uni0628.init"})),
            Pairs(features=frozendict({"kern": frozendict()}), values=frozendict({pair: Adjustment(advance=value) for pair, value in pairs.items()})),
        ),
    )
    binary = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    for first in ("uni0631", "uni0631.fina", "uni0627"):
        for second in ("uni0628", "uni0628.init", "uni0627"):
            text = "".join(chr(source.named[name].name.codes[0]) for name in (first, second))
            options = {"ss01": first == "uni0631.fina", "ss02": second == "uni0628.init"}
            before = binary.shaped(text, features={**options, "kern": False}, direction="rtl")
            after = binary.shaped(text, features={**options, "kern": True}, direction="rtl")
            adjustment = pairs.get((first, second), 0)
            assert after.names == before.names == (second, first)
            assert after.width - before.width == adjustment
            assert (after.placed[1].advance - before.placed[1].advance, after.placed[1].x - before.placed[1].x, after.placed[1].y - before.placed[1].y, after.placed[0].x - before.placed[0].x) == (
                adjustment,
                0,
                0,
                0,
            )


def test_native_kern_stages_accumulate_all_position_components(source: Source, sample: Corpus) -> None:
    """Two independent kern lookups both apply their placement and advance changes to one pair."""
    source = msgspec.structs.replace(
        source,
        lookups=tuple(
            Pairs(features=frozendict({"kern": frozendict()}), order=index, values=frozendict({("uni0631", "uni0628"): adjustment}))
            for index, adjustment in enumerate((Adjustment(dx=7, dy=3, advance=-20), Adjustment(dx=-2, dy=4, advance=-10)))
        ),
    )
    binary = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    before, after = (binary.shaped("ارب", features={"kern": enabled}, direction="rtl").placed for enabled in (False, True))
    assert (after[1].x - before[1].x, after[1].y - before[1].y, after[1].advance - before[1].advance, after[2].x - before[2].x) == (5, 7, -30, -30)


@pytest.mark.parametrize("direction,ligature_names,adjustments", [("ltr", ("lam", "alef"), (0, 0, 0, 0)), ("rtl", ("ligature",), (5, 7, -30, -30))])
def test_native_arabic_direction_controls_multiglyph_lookup_order(direction: Direction, ligature_names: tuple[str, ...], adjustments: tuple[int, int, int, int]) -> None:
    """A native FontBuilder original exposes Arabic sequence order and independent pair-stage accumulation under both requested directions."""
    binary = Binary(
        data=encoded(
            {name: (advance, rectangles((0, 0, 100, 100))) for name, advance in {".notdef": 500, "lam": 420, "alef": 180, "beh": 520, "reh": 310, "ligature": 380}.items()},
            {0x0644: "lam", 0x0627: "alef", 0x0628: "beh", 0x0631: "reh"},
            """
            languagesystem arab dflt;
            feature ss02 { sub lam alef by ligature; } ss02;
            lookup First { pos reh beh <7 3 -20 0>; } First;
            lookup Second { pos reh beh <-2 4 -10 0>; } Second;
            feature kern { lookup First; lookup Second; } kern;
            """,
        )
    )
    assert binary.shaped("لا", features={"ss02": True}, direction=direction).names == ligature_names
    before, after = (binary.shaped("ارب", features={"kern": enabled}, direction=direction).placed for enabled in (False, True))
    assert (after[1].x - before[1].x, after[1].y - before[1].y, after[1].advance - before[1].advance, after[2].x - before[2].x) == adjustments


def test_scoped_positioning_preserves_native_attachments_without_default_leakage(source: Source, sample: Corpus) -> None:
    """Persian lifts retain generated mark attachment while an Arabic-default adjustment stays outside Persian."""
    source = msgspec.structs.replace(
        source,
        lookups=(
            Positions(features=frozendict({"mark": frozendict({("arab", "dflt"): False})}), values=frozendict({"uni064E": Adjustment(dy=13)})),
            Lifts(features=frozendict({"mark": frozendict({("arab", "FAR "): False})}), rules=(Chain(input=((("uni0628",), None), (("uni064E",), Adjustment(dy=40)))),)),
        ),
    )
    _, ufo, font = constructed(source, Metrics(pitch=None, stack=0), sample).default_with(lambda faults: pytest.fail(str(faults)))
    binary = Binary(data=font)
    assert tuple(next(placed.y for placed in binary.shaped("بَ", language=language, direction="ltr").placed if placed.glyph in binary.marks) for language in ("ar", "fa")) == (813, 840)
    retained = Binary(data=compileTTF(ufo, featureWriters=[CursFeatureWriter, MarkFeatureWriter, GdefFeatureWriter], useProductionNames=False))  # type: ignore[no-untyped-call]
    assert tuple(next(placed.y for placed in retained.shaped("بَ", language=language, direction="ltr").placed if placed.glyph in retained.marks) for language in ("ar", "fa")) == (813, 840)
