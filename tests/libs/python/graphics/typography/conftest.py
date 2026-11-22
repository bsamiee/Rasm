"""Probe fonts drawn to hold one defect each, the empty corpus a case fills, a face's planned rule applied over a probe, and the outer contours of a rebuilt glyph read apart from the library under test."""

from collections.abc import Callable, Mapping
import functools
from io import BytesIO
import math

from fontTools.fontBuilder import FontBuilder
from fontTools.pens.statisticsPen import StatisticsPen
from fontTools.pens.ttGlyphPen import TTGlyphPen
import pathops
import pytest

from rasm.testing import settled
from rasm.typography import Anchors, Binary, Corpus, Faces, imported, Joining, Joins, Locale, Outlines, planned, Source, Spacing

# --- [OPERATIONS] -----------------------------------------------------------------------


def _drawn(glyphs: Mapping[str, tuple[int, tuple[tuple[int, int, int, int], ...]]], cmap: Mapping[int, str], features: str = "", /) -> bytes:
    order = [".notdef", *glyphs]
    builder = FontBuilder(1000, isTTF=True)
    builder.setupGlyphOrder(order)
    builder.setupCharacterMap(dict(cmap))
    pens = {name: TTGlyphPen(None) for name in order}
    for name, (_, rects) in glyphs.items():
        for xmin, ymin, xmax, ymax in rects:
            pens[name].moveTo((xmin, ymin))
            pens[name].lineTo((xmin, ymax))
            pens[name].lineTo((xmax, ymax))
            pens[name].lineTo((xmax, ymin))
            pens[name].closePath()
    builder.setupGlyf({name: pen.glyph() for name, pen in pens.items()})
    builder.setupHorizontalMetrics({name: (advance, min((rect[0] for rect in rects), default=0)) for name in order for advance, rects in (glyphs.get(name, (500, ())),)})
    builder.setupHorizontalHeader(ascent=800, descent=-300)
    builder.setupNameTable({"familyName": "Probe", "styleName": "Regular"})
    builder.setupOS2(usWinAscent=800, usWinDescent=300)
    builder.setupPost()
    builder.addOpenTypeFeatures(features)
    buffer = BytesIO()
    builder.save(buffer)
    return buffer.getvalue()


def _outers(source: Source, name: str, /) -> tuple[tuple[float, float], ...]:
    path = pathops.Path()
    source.drawn(name).draw(path.getPen())
    held: list[tuple[float, float]] = []
    for contour in pathops.simplify(path, fix_winding=True, clockwise=False).contours:
        pen = StatisticsPen()
        contour.draw(pen)
        middle, spread = (pen.varianceX + pen.varianceY) / 2, math.hypot((pen.varianceX - pen.varianceY) / 2, pen.covariance)
        held.extend(() if contour.clockwise else ((float(contour.area), math.sqrt((middle + spread) / (middle - spread))),))
    return tuple(sorted(held))


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def font() -> functools.partial[bytes]:
    """Returns the builder of a 1000-unit TrueType probe from each glyph's advance and rectangles, its cmap, and its feature code."""
    return functools.partial(_drawn)


@pytest.fixture
def corpus() -> Corpus:
    """Returns the corpus holding no text, which a case fills with the runs its defect needs."""
    return Corpus(prose=frozendict({"fa": ()}), marked=(), annotated=(), deep=(), words=(), digits=(), punctuation=(), joins=(), seam=(), zwnj=(), ligatures=(), letters=(), vowel_stacks=(), stacks=())


@pytest.fixture
def rebuilt(corpus: Corpus) -> functools.partial[Source]:
    """Returns the source a face's first planned rule of a kind makes of probe data, imported under the joinings given, over the probe as original and the corpus given, else the empty one."""

    def rebuilding(
        face: Faces, data: bytes, kind: type[Anchors | Joins | Locale | Outlines | Spacing], /, *, joinings: frozendict[int, tuple[Joining, str]] = frozendict(), corpus: Corpus = corpus
    ) -> Source:
        rule = next(rule for rule in planned(face) if isinstance(rule, kind))
        return settled(rule.applied(settled(imported(data, face, joinings)), original=Binary(data=data), corpus=corpus, joinings=joinings))

    return functools.partial(rebuilding)


@pytest.fixture
def outers() -> Callable[[Source, str], tuple[tuple[float, float], ...]]:
    """Returns the area and elongation of each outer contour of a rebuilt glyph, smallest first, measured by skia-pathops and fontTools apart from the library."""
    return _outers
