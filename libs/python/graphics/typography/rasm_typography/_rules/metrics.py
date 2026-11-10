"""Vertical metrics from the corpus set on the face itself inside a pitch band, win over every glyph and mark stack, and the records measured from the drawing and its slant."""

lazy from functools import reduce
lazy import math

import msgspec
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.ttLib import TTFont

from rasm_typography._faces.declaration import Band, Em
from rasm_typography._rules import measures
lazy from rasm_records import Ok, Outcome
lazy from rasm_typography._faces.glyphs import Letter, Ligature, Mark, Side, Source, VerticalMetrics
lazy from rasm_typography._faces.names import Form
lazy from rasm_typography._outlines import measure, paths
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Metrics(measures.Rule, frozen=True, kw_only=True):
    """Typo and hhea at the outline extent the plain, marked, annotated, and deep corpus reaches set on the face inside a pitch band, or the face's own pair without one, and win over every mapped glyph and a stack of `stacks` marks over each base anchor."""

    pitch: Band | None
    stack: Em
    stacks: int = 3

    def measured(self, source: Source, /, *, corpus: Corpus, binary: Binary[TTFont]) -> VerticalMetrics:
        """Measures final vertical metrics on compiled native layout, retaining the source's records.

        Args:
            source: Source of the compiled font.
            corpus: Runs and languages the metrics cover.
            binary: Compiled source font before its final vertical metrics are set.

        Returns:
            Typo and hhea within the pitch band, the marked extent fit to its integer budget padded toward its minimum with an odd unit above, and win over every glyph, a stack of
            the tallest marks on every base anchor, and actual corpus placement.
        """
        languages, vertical, stack = corpus.languages, source.metrics, self.stack * source.upm
        reached = _reach(binary, tuple((text, language) for text in (*corpus.lines, *corpus.matrix) for language in languages))
        boxes = {name: paths.Box.of(rect) for name in source.named if (rect := source.bounds(name)) is not None}
        feet = {side: tuple((glyph, foot) for glyph in source.by_role(Mark) if glyph.role == Mark(side=side) for foot in glyph.anchors if foot.name.startswith("_")) for side in Side}
        rises = {side: max((abs(anchor.y - foot.y) for glyph, foot in held for anchor in glyph.anchor(foot.name[1:]).to_list()), default=stack) for side, held in feet.items()}
        reaches = {side: max((abs((box.ymax if side is Side.ABOVE else box.ymin) - foot.y) for glyph, foot in held if (box := boxes.get(glyph.name.text))), default=0.0) for side, held in feet.items()}
        stacked = {side: (self.stacks - 1) * rises[side] + reaches[side] for side in Side}
        anchors = tuple(anchor for glyph in source.glyphs if isinstance(glyph.role, Letter | Ligature) for anchor in glyph.anchors)
        win = (
            math.ceil(max((reached[0], *(box.ymax for box in boxes.values()), *(anchor.y + stacked[Side.ABOVE] for anchor in anchors if anchor.name.startswith(Side.ABOVE.anchor))))),
            math.ceil(-min((reached[1], *(box.ymin for box in boxes.values()), *(anchor.y - stacked[Side.BELOW] for anchor in anchors if anchor.name.startswith(Side.BELOW.anchor))))),
        )
        match self.pitch:
            case (least, most):
                plain = _reach(binary, tuple((text, language) for language, texts in corpus.prose.items() for text in texts))
                marked = _reach(binary, tuple((text, language) for text in (*corpus.marked, *corpus.annotated, *corpus.deep) for language in languages))
                low, high = math.ceil(least * source.upm), math.floor(most * source.upm)
                plain_top, plain_foot, marked_top, marked_foot = math.ceil(plain[0]), math.floor(plain[1]), math.ceil(marked[0]), math.floor(marked[1])
                top = max(plain_top, min(max(plain_top, marked_top), min(plain_foot, marked_foot) + high))
                foot = max(min(plain_foot, marked_foot), min(plain_foot, top - high))
                pad, extra = divmod(max(low - (top - foot), 0), 2)
                typo = (top + pad + extra, foot - pad)
            case None:
                typo = (vertical.ascender, vertical.descender)
        return msgspec.structs.replace(vertical, ascender=typo[0], descender=typo[1], win_ascent=max(win[0], typo[0]), win_descent=max(win[1], -typo[1]))


class Records(measures.Rule, frozen=True, kw_only=True):
    """Lowest readable size one pixel across the median counter of the letters, or of every glyph, the x and cap heights at the outline tops of `x` and `H` as OS/2 defines them, and the italic angle and caret shift of a slant about the stem's middle."""

    lowest: int | None = None
    letters: tuple[int, ...] = ()
    stem: int | None = None
    slant: float = 0.0

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Sets the lowest readable size one pixel across the median counter of the letters, or of every glyph, the x and cap heights at the outline tops of `x` and `H`, and the slant's italic angle and caret shift."""
        names = frozenset(source.key(code, form) for code in self.letters for form in Form)
        counters = tuple(
            width
            for glyph in source.glyphs
            if (not self.letters or glyph.name.text in names) and (width := min((2 * measure.inradius(void) for void in measure.counters(source.drawn(glyph.name.text).contours)), default=0.0)) > 0
        )
        x_height, cap_height = (otRound(rect[3]) if (rect := source.bounds(source.key(code))) is not None else 0 for code in map(ord, "xH"))
        held = source.metrics
        about = paths.Box.of(rect).middle[1] if self.stem is not None and (rect := source.bounds(source.key(self.stem))) is not None else 0.0
        return Ok(
            msgspec.structs.replace(
                source,
                metrics=msgspec.structs.replace(
                    held,
                    lowest_ppem=self.lowest or (math.ceil(source.upm / measures.middle(counters)) if counters else held.lowest_ppem),
                    x_height=x_height,
                    cap_height=cap_height,
                    italic_angle=self.slant,
                    caret_offset=otRound(math.tan(math.radians(self.slant)) * about),
                ),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _reach(binary: Binary[TTFont], texts: tuple[tuple[str, str | None], ...], /) -> tuple[float, float]:
    """Baseline and actual outline extrema after native substitutions and positioning under each run's language."""
    return reduce(lambda held, box: (max(held[0], box.ymax), min(held[1], box.ymin)), (box for text, language in texts for _, box in binary.placed_extents(text, language)), (0.0, 0.0))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Metrics", "Records"]
