"""Vertical metrics from the corpus set on the face itself inside a pitch band, win over every glyph and mark stack, and the records measured from the drawing and its slant."""

lazy from functools import partial, reduce
lazy import math

import msgspec

from typography.faces.declaration import Band, Em
from typography.rules.scripts import Rule
lazy from typography.faces.glyphs import Glyph, Joining, Letter, Ligature, Mark, Metrics as Vertical, Side, Source
lazy from typography.faces.names import Form
lazy from typography.outlines.measure import counter
lazy from typography.outlines.paths import bounds, Box
lazy from typography.rules.measures import key, middle
lazy from typography.shaping.corpus import Corpus

# --- [MODELS] ---------------------------------------------------------------------------


class Metrics(Rule, frozen=True, kw_only=True):
    """Typo and hhea at the ink the plain, marked, annotated, and deep corpus reaches set on the face inside a pitch band, or the face's own pair without one, and win over every mapped glyph and a stack of `stacks` marks over each seat."""

    pitch: Band | None
    stack: Em
    stacks: int = 3


class Records(Rule, frozen=True, kw_only=True):
    """Lowest readable size one pixel across the median counter of the letters, or of every glyph, the x and cap heights of the two height letters, and the italic angle and caret shift of a slant about the stem's middle."""

    heights: tuple[int, int]
    lowest: int | None = None
    letters: tuple[int, ...] = ()
    stem: int | None = None
    slant: float = 0.0


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [EXTENTS]


def _set(source: Source, line: str, /) -> tuple[Glyph, ...]:
    """Glyphs a line sets on the face: each letter in the form its neighbours' joining types give, each code point the face lacks left out."""
    codes = tuple(map(ord, line))
    kinds = tuple(map(Joining.of, codes))
    letters = tuple(index for index, kind in enumerate(kinds) if kind is not Joining.TRANSPARENT)
    forms = {
        index: Form.joined(
            right=before is not None and kinds[before] in {Joining.DUAL, Joining.LEFT, Joining.CAUSING} and kinds[index] in {Joining.DUAL, Joining.RIGHT, Joining.CAUSING},
            left=after is not None and kinds[index] in {Joining.DUAL, Joining.LEFT, Joining.CAUSING} and kinds[after] in {Joining.DUAL, Joining.RIGHT, Joining.CAUSING},
        )
        for before, index, after in zip((None, *letters[:-1]), letters, (*letters[1:], None), strict=True)
    }
    return tuple(glyph for index, code in enumerate(codes) for glyph in (source.named.get(key(code, forms.get(index, Form.ISOL))) or source.named.get(key(code)),) if glyph)


def _stacked(boxes: frozendict[str, Box], state: tuple[float, float, frozendict[Side, tuple[Glyph, float]]], glyph: Glyph, /) -> tuple[float, float, frozendict[Side, tuple[Glyph, float]]]:
    """Extends the reach by a glyph: a mark raised from its attaching anchor onto the seat of the glyph it stacks on, every other glyph where it stands."""
    high, low, seats = state
    match glyph.role:
        case Mark(side=side) if side in seats:
            base, rise = seats[side]
            lift = rise + next((seat.y - foot.y for foot in glyph.anchors if foot.name.startswith("_") for seat in base.anchor(foot.name[1:]).to_list()), 0.0)
            seated = frozendict(seats | {side: (glyph, lift)})
        case _:
            lift, seated = 0.0, frozendict(dict.fromkeys(Side, (glyph, 0.0)))
    box = boxes.get(glyph.name.text)
    return (max(high, box.ymax + lift), min(low, box.ymin + lift), seated) if box else (high, low, seated)


def _reach(source: Source, lines: tuple[str, ...], /) -> tuple[float, float]:
    """Highest and lowest ink lines set on the face reach, `.notdef` never among them."""
    boxes = frozendict({glyph.name.text: box for glyph in source.glyphs if (box := bounds(glyph.contours))})
    initial: tuple[float, float, frozendict[Side, tuple[Glyph, float]]] = (0.0, 0.0, frozendict())
    reached = tuple(reduce(partial(_stacked, boxes), _set(source, line), initial)[:2] for line in lines)
    return (max((top for top, _ in reached), default=0.0), min((foot for _, foot in reached), default=0.0))


def _typo(plain: tuple[float, float], marked: tuple[float, float], pitch: tuple[float, float], /) -> tuple[int, int]:
    """Ascender and descender spanning the marked text within the pitch band, the plain text held whole, the overrun cut above first, rounded out to 10 units and in at the ceiling."""
    low, high = pitch
    top = max(plain[0], min(max(plain[0], marked[0]), min(plain[1], marked[1]) + high))
    foot = max(min(plain[1], marked[1]), min(plain[1], top - high))
    pad = max(low - (top - foot), 0.0) / 2
    out = (math.ceil((top + pad) / 10) * 10, math.floor((foot - pad) / 10) * 10)
    return out if out[0] - out[1] <= high else (math.floor((top + pad) / 10) * 10, math.ceil((foot - pad) / 10) * 10)


def _win(source: Source, stack: float, stacks: int, /) -> tuple[int, int]:
    """Win ascent and descent over every mapped glyph's ink and a stack of the tallest marks on every base seat, each mark rising by its own seat over its foot."""
    boxes = {glyph.name.text: box for glyph in source.glyphs if glyph.name.codes and (box := bounds(glyph.contours))}
    feet = {side: tuple((glyph, foot) for glyph in source.by_role(Mark) if glyph.role == Mark(side=side) for foot in glyph.anchors if foot.name.startswith("_")) for side in Side}
    rises = {side: max((abs(seat.y - foot.y) for glyph, foot in held for seat in glyph.anchor(foot.name[1:]).to_list()), default=stack) for side, held in feet.items()}
    reaches = {side: max((abs((box.ymax if side is Side.ABOVE else box.ymin) - foot.y) for glyph, foot in held if (box := bounds(glyph.contours))), default=0.0) for side, held in feet.items()}
    stacked = {side: (stacks - 1) * rises[side] + reaches[side] for side in Side}
    seats = tuple(anchor for glyph in source.glyphs if isinstance(glyph.role, Letter | Ligature) for anchor in glyph.anchors)
    top = max((*(box.ymax for box in boxes.values()), *(anchor.y + stacked[Side.ABOVE] for anchor in seats if anchor.name.startswith(Side.ABOVE.anchor))), default=0.0)
    foot = min((*(box.ymin for box in boxes.values()), *(anchor.y - stacked[Side.BELOW] for anchor in seats if anchor.name.startswith(Side.BELOW.anchor))), default=0.0)
    return (math.ceil(top), math.ceil(-foot))


def measured(source: Source, rule: Metrics, /, *, corpus: Corpus) -> Source:
    """Sets typo and hhea at the corpus ink inside the pitch band, or keeps the face's own pair without a band, and win over every mapped glyph and mark stack."""
    win = _win(source, rule.stack * source.upm, rule.stacks)
    held = source.metrics or Vertical(ascender=win[0], descender=-win[1], win_ascent=win[0], win_descent=win[1])
    typo = (
        _typo(_reach(source, corpus.plain), _reach(source, (*corpus.marked, *corpus.annotated, *corpus.deep)), (rule.pitch[0] * source.upm, rule.pitch[1] * source.upm))
        if rule.pitch
        else (held.ascender, held.descender)
    )
    return msgspec.structs.replace(source, metrics=msgspec.structs.replace(held, ascender=typo[0], descender=typo[1], win_ascent=max(win[0], typo[0]), win_descent=max(win[1], -typo[1])))


# --- [RECORDS]


def recorded(source: Source, rule: Records, /) -> Source:
    """Sets the lowest readable size one pixel across the median counter of the letters, or of every glyph, the x and cap heights at the ink tops of the two height letters, and the slant's italic angle and caret shift."""
    names = frozenset(key(code, form) for code in rule.letters for form in Form)
    counters = tuple(width for glyph in source.glyphs if (not rule.letters or glyph.name.text in names) and (width := counter(glyph.contours)) > 0)
    x_height, cap_height = (round(box.ymax) if (glyph := source.named.get(key(code))) and (box := bounds(glyph.contours)) else 0 for code in rule.heights)
    held = source.metrics or Vertical(ascender=0, descender=0, win_ascent=0, win_descent=0)
    about = box.middle[1] if rule.stem is not None and (glyph := source.named.get(key(rule.stem))) and (box := bounds(glyph.contours)) else 0.0
    return msgspec.structs.replace(
        source,
        metrics=msgspec.structs.replace(
            held,
            lowest_ppem=rule.lowest or (math.ceil(source.upm / middle(counters)) if counters else held.lowest_ppem),
            x_height=x_height,
            cap_height=cap_height,
            italic_angle=rule.slant,
            caret_offset=round(math.tan(math.radians(rule.slant)) * about),
        ),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Metrics", "Records", "measured", "recorded"]
