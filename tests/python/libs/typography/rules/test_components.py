"""Native pen geometry keeps component references coherent under whole-source maps."""

# mypy: disable-error-code="no-untyped-call"

from fontTools.misc.transform import Transform
from fontTools.pens.recordingPen import DecomposingRecordingPen
from fontTools.pens.transformPen import TransformPen
import msgspec
import numpy as np
import pytest

from typography.faces.glyphs import Anchor, Component, Source
from typography.faces.names import GlyphName
from typography.outlines.paths import Box
from typography.rules.joins import joined, Joins
from typography.rules.measures import band, mapped

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("target,retained", [(Transform(2, 0, 1, 3, 5, -7), True), (Transform(0, 0, 0, 2, 0, 0), False)])
def test_component_maps_compose_through_nested_and_singular_targets(source: Source, target: Transform, *, retained: bool) -> None:
    """Different noncommuting owner maps preserve complete pen geometry, decomposing only references to singular targets."""
    base = msgspec.structs.replace(source.named[".notdef"], name=GlyphName(stem="base"), contours=(Box(xmin=0, ymin=0, xmax=100, ymax=60).contour,))
    alias = msgspec.structs.replace(base, name=GlyphName(stem="alias"), contours=(), components=(Component(name="base", transform=Transform(1, 0.5, 0.25, 1, 40, 20)),))
    root = msgspec.structs.replace(
        base,
        name=GlyphName(stem="root"),
        contours=(Box(xmin=300, ymin=30, xmax=320, ymax=50).contour,),
        anchors=(Anchor(name="top", x=12, y=34),),
        components=(Component(name="alias", transform=Transform(1, 0, 0.5, 1, 15, -10)),),
    )
    source = msgspec.structs.replace(source, glyphs=(base, alias, root), lookups=())
    matrices = {"base": target, "alias": Transform(2, 0, 0.5, 1, -3, 8), "root": Transform(1, 0.25, 0.5, 1, 7, 11)}
    done = mapped(source, matrices)
    expected, actual = DecomposingRecordingPen(source.named), DecomposingRecordingPen(done.named)
    root.draw(TransformPen(expected, matrices["root"]))
    drawn = done.drawn("root")
    drawn.draw(actual)
    assert [operation for operation, _ in actual.value] == [operation for operation, _ in expected.value]
    np.testing.assert_allclose([*points for _, points in actual.value], [*points for _, points in expected.value], rtol=1e-9, atol=1e-9)
    assert bool(done.named["alias"].components) is retained
    assert len(done.named["root"].components) == 1
    assert not drawn.components
    assert tuple((anchor.x, anchor.y) for anchor in done.named["root"].anchors) == (matrices["root"].transformPoint((12, 34)),)


def test_join_band_measures_both_edges_of_a_component_only_medial(source: Source) -> None:
    """A scaled component spanning both advance edges supplies its actual ten-to-ninety join band."""
    base = msgspec.structs.replace(source.named[".notdef"], contours=(Box(xmin=0, ymin=0, xmax=100, ymax=40).contour,))
    medial = msgspec.structs.replace(source.named["uni0628.medi"], advance=100, contours=(), components=(Component(name=base.name.text, transform=Transform(1, 0, 0, 2, 0, 10)),))
    assert band(msgspec.structs.replace(source, glyphs=(base, medial), lookups=())) == (10, 90)


def test_join_translation_preserves_a_scaled_component_letter(source: Source) -> None:
    """Moving a forty-unit join-band foot to zero translates a composite letter's complete scaled ink once."""
    base = msgspec.structs.replace(source.named[".notdef"], name=GlyphName(stem="base"), contours=(Box(xmin=0, ymin=0, xmax=100, ymax=50).contour,))
    medial = msgspec.structs.replace(source.named["uni0628.medi"], advance=200, contours=(Box(xmin=0, ymin=40, xmax=200, ymax=80).contour,))
    letter = msgspec.structs.replace(source.named["uni0628.fina"], codes=frozenset(), contours=(), components=(Component(name="base", transform=Transform(2, 0, 0, 2, 30, 70)),))
    alias = msgspec.structs.replace(base, name=GlyphName(stem=(0xFE90,)), codes=frozenset({0xFE90}), contours=(), components=(Component(name="uni0628.fina"),))
    nested = msgspec.structs.replace(base, name=GlyphName(stem="nested"), contours=(), components=(Component(name="uniFE90", transform=Transform(1, 0, 0, 2, 10, 5)),))
    source = msgspec.structs.replace(source, glyphs=(base, medial, letter, alias, nested), lookups=())
    done = joined(source, Joins(overlap=0, kashida=None)).default_with(lambda faults: pytest.fail(str(faults)))
    assert done.bounds("base") == source.bounds("base")
    assert done.bounds("uni0628.fina") == done.bounds("uniFE90") == (30, 30, 230, 130)
    assert done.bounds("nested") == (40, 65, 240, 265)
    assert done.named["uniFE90"].components
    assert done.named["nested"].components
    assert done.bounds("uni0628.medi") == (0, 0, 200, 40)
