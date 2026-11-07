"""Vertical metrics from the corpus set on the face itself inside a pitch band, win over every glyph and mark stack, and the records measured from the drawing and its slant."""

lazy from functools import reduce
lazy import math

import msgspec
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.ttLib import TTFont

from rasm_typography._faces.declaration import Band, Em
from rasm_typography._rules.measures import _middle, Rule
lazy from rasm_typography._faces import glyphs
lazy from rasm_typography._faces.glyphs import _Side
lazy from rasm_typography._faces.names import _Form
lazy from rasm_typography._outlines.measure import _counter
lazy from rasm_typography._outlines.paths import _Box
lazy from rasm_typography._shaping.corpus import _Corpus
lazy from rasm_typography._shaping.runs import _Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Metrics(Rule, frozen=True, kw_only=True):
    """Typo and hhea at the outline extent the plain, marked, annotated, and deep corpus reaches set on the face inside a pitch band, or the face's own pair without one, and win over every mapped glyph and a stack of `stacks` marks over each base anchor."""

    pitch: Band | None
    stack: Em
    stacks: int = 3


class Records(Rule, frozen=True, kw_only=True):
    """Lowest readable size one pixel across the median counter of the letters, or of every glyph, the x and cap heights at the outline tops of `x` and `H` as OS/2 defines them, and the italic angle and caret shift of a slant about the stem's middle."""

    lowest: int | None = None
    letters: tuple[int, ...] = ()
    stem: int | None = None
    slant: float = 0.0


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [EXTENTS]


def _reach(binary: _Binary[TTFont], texts: tuple[tuple[str, str], ...], /) -> tuple[float, float]:
    """Baseline and actual outline extrema after native substitutions and positioning under each run's language."""
    return reduce(lambda held, box: (max(held[0], box.ymax), min(held[1], box.ymin)), (box for text, language in texts for _, box in binary.placed_extents(text, language)), (0.0, 0.0))


def _typo(plain: tuple[float, float], marked: tuple[float, float], pitch: tuple[float, float], /) -> tuple[int, int]:
    """Retains plain outline extents and fits marked outline extents to the integer pitch budget, padding toward its minimum with an odd unit above."""
    low, high = math.ceil(pitch[0]), math.floor(pitch[1])
    plain_top, plain_foot, marked_top, marked_foot = math.ceil(plain[0]), math.floor(plain[1]), math.ceil(marked[0]), math.floor(marked[1])
    top = max(plain_top, min(max(plain_top, marked_top), min(plain_foot, marked_foot) + high))
    foot = max(min(plain_foot, marked_foot), min(plain_foot, top - high))
    pad, extra = divmod(max(low - (top - foot), 0), 2)
    return (top + pad + extra, foot - pad)


def _win(source: glyphs._Source, stack: float, stacks: int, reached: tuple[float, float], /) -> tuple[int, int]:
    """Win ascent and descent over native corpus outlines, every glyph, and a stack of the tallest marks on every base anchor."""
    boxes = {name: _Box.of(rect) for name in source.named if (rect := source.bounds(name)) is not None}
    feet = {side: tuple((glyph, foot) for glyph in source.by_role(glyphs.Mark) if glyph.role == glyphs.Mark(side=side) for foot in glyph.anchors if foot.name.startswith("_")) for side in _Side}
    rises = {side: max((abs(anchor.y - foot.y) for glyph, foot in held for anchor in glyph.anchor(foot.name[1:]).to_list()), default=stack) for side, held in feet.items()}
    reaches = {side: max((abs((box.ymax if side is _Side.ABOVE else box.ymin) - foot.y) for glyph, foot in held if (box := boxes.get(glyph.name.text))), default=0.0) for side, held in feet.items()}
    stacked = {side: (stacks - 1) * rises[side] + reaches[side] for side in _Side}
    anchors = tuple(anchor for glyph in source.glyphs if isinstance(glyph.role, glyphs.Letter | glyphs.Ligature) for anchor in glyph.anchors)
    top = max((reached[0], *(box.ymax for box in boxes.values()), *(anchor.y + stacked[_Side.ABOVE] for anchor in anchors if anchor.name.startswith(_Side.ABOVE.anchor))))
    foot = min((reached[1], *(box.ymin for box in boxes.values()), *(anchor.y - stacked[_Side.BELOW] for anchor in anchors if anchor.name.startswith(_Side.BELOW.anchor))))
    return (math.ceil(top), math.ceil(-foot))


def _measured(source: glyphs._Source, rule: Metrics, /, *, corpus: _Corpus, binary: _Binary[TTFont]) -> glyphs._Metrics:
    """Measures final vertical metrics on compiled native layout, retaining the source's records.

    Args:
        source: Source of the compiled font.
        rule: Pitch band and minimum synthetic mark stack.
        corpus: Runs and languages the metrics cover.
        binary: Compiled source font before its final vertical metrics are set.

    Returns:
        Typo and hhea within the pitch band and win over every glyph, synthetic stack, and actual corpus placement.
    """
    languages = corpus.languages
    vertical = source.metrics
    win = _win(source, rule.stack * source.upm, rule.stacks, _reach(binary, tuple((text, language) for text in (*corpus.lines, *corpus.matrix) for language in languages)))
    typo = (
        _typo(
            _reach(binary, tuple((text, language) for language, texts in corpus.prose.items() for text in texts)),
            _reach(binary, tuple((text, language) for text in (*corpus.marked, *corpus.annotated, *corpus.deep) for language in languages)),
            (rule.pitch[0] * source.upm, rule.pitch[1] * source.upm),
        )
        if rule.pitch
        else (vertical.ascender, vertical.descender)
    )
    return msgspec.structs.replace(vertical, ascender=typo[0], descender=typo[1], win_ascent=max(win[0], typo[0]), win_descent=max(win[1], -typo[1]))


# --- [RECORDS]


def _recorded(source: glyphs._Source, rule: Records, /) -> glyphs._Source:
    """Sets the lowest readable size one pixel across the median counter of the letters, or of every glyph, the x and cap heights at the outline tops of `x` and `H`, and the slant's italic angle and caret shift."""
    names = frozenset(source.key(code, form) for code in rule.letters for form in _Form)
    counters = tuple(width for glyph in source.glyphs if (not rule.letters or glyph.name.text in names) and (width := _counter(source.drawn(glyph.name.text).contours)) > 0)
    x_height, cap_height = (otRound(rect[3]) if (rect := source.bounds(source.key(code))) is not None else 0 for code in map(ord, "xH"))
    held = source.metrics
    about = _Box.of(rect).middle[1] if rule.stem is not None and (rect := source.bounds(source.key(rule.stem))) is not None else 0.0
    return msgspec.structs.replace(
        source,
        metrics=msgspec.structs.replace(
            held,
            lowest_ppem=rule.lowest or (math.ceil(source.upm / _middle(counters)) if counters else held.lowest_ppem),
            x_height=x_height,
            cap_height=cap_height,
            italic_angle=rule.slant,
            caret_offset=otRound(math.tan(math.radians(rule.slant)) * about),
        ),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Metrics", "Records"]
