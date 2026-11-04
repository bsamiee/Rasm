"""Spacing: the word space from the gap ratio over the original's shaped words, and `kern` pairs holding non-joining tails to the median inside gap."""

from typing import Annotated
lazy from graphlib import TopologicalSorter
lazy from itertools import groupby, pairwise
lazy import math

import msgspec
lazy from fontTools.misc.roundTools import otRound

from typography.faces.declaration import Band, Em, Ratio, Script
from typography.rules.measures import Rule
lazy from typography.faces.faults import Outcome
lazy from typography.faces.glyphs import Joining, Letter, Outline, Source
lazy from typography.faces.layout import Adjustment, Pairs
lazy from typography.faces.names import Form
lazy from typography.outlines.measure import gap
lazy from typography.outlines.paths import area, bounds, pieces, strip, union
lazy from typography.rules.measures import middle, placement
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Spacing(Rule, frozen=True, kw_only=True):
    """Word space at the ratio times the inside gap within a band, and `kern` pairs holding non-joining tails within a tolerance of the gap after the script's standing letters, never under the ink floor."""

    script: Script
    ratio: Ratio
    band: Band
    tolerance: Em = 0.03
    floor: Em = 0.02
    rows: Annotated[int, msgspec.Meta(gt=0)] = 32


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [WORD_SPACE]


def _gaps(original: Binary, corpus: Corpus, stem: int, space: int, /) -> tuple[float, float, float]:
    """Median ink gap between the letter groups of one word, median ink gap across a space, and the space advance, shaped on the original in its units."""
    blank, marked = original.encoded.get(space), original.marks
    body = 0.25 * area(original.outline(original.encoded[stem]))
    runs = tuple(original.shaped(line, language=language) for language, lines in corpus.prose.items() for line in lines)
    words = tuple(
        tuple(
            union(*(contours for placed, contours in group if placed.glyph not in marked))
            for is_space, group in groupby(zip(run.placed, original.drawn(run), strict=False), key=lambda pair: pair[0].glyph == blank)
            if not is_space
        )
        for run in runs
    )
    bodies = tuple(
        tuple(
            piece
            for piece, _ in sorted(
                ((piece, box) for piece in pieces(word) for box in (bounds(piece),) if area(piece) >= body and box is not None),
                key=lambda held: (held[1].xmin, held[1].ymin, held[1].xmax, held[1].ymax),
            )
        )
        for line in words
        for word in line
    )
    inside = tuple(distance for word in bodies for left, right in pairwise(word) if 0 < (distance := gap(left, right)) < math.inf)
    across = tuple(distance for line in words for left, right in pairwise(line) if 0 < (distance := gap(left, right)) < math.inf)
    advance = next((placed.advance for run in runs for placed in run.placed if placed.glyph == blank), 0.0)
    return (middle(inside), middle(across), advance)


# --- [PAIRS]


def _profile(contours: Outline, low: float, high: float, rows: int, /) -> tuple[tuple[float, float] | None, ...]:
    """Leftmost and rightmost ink of each of `rows` equal horizontal bands between two heights, `None` for a band without ink."""
    step = (high - low) / rows
    return tuple((box.xmin, box.xmax) if (box := bounds(strip(contours, low + index * step, low + (index + 1) * step, axis="y"))) else None for index in range(rows))


def _apart(tail: tuple[tuple[float, float] | None, ...], head: tuple[tuple[float, float] | None, ...], advance: float, /) -> float:
    """Least white between a tail's left ink and the right ink of the letter set after it, band by band where both hold ink."""
    return min((right[0] - (left[1] - advance) for right, left in zip(tail, head, strict=False) if right and left), default=math.inf)


def _pairs(source: Source, rule: Spacing, /) -> Pairs | None:
    """Holds each non-joining final before a letter starting a group to the median gap above the baseline after a standing letter, never under the ink floor."""
    letters = tuple(drawn for glyph in source.by_role(Letter) if len(glyph.name.codes) == 1 and (drawn := source.drawn(glyph.name.text)).contours)
    tails = tuple(glyph for glyph in letters if glyph.role in {Letter(joining=Joining.RIGHT, form=Form.ISOL), Letter(joining=Joining.RIGHT, form=Form.FINA)})
    heads = tuple(glyph for glyph in letters if glyph.name.form in {Form.ISOL, Form.INIT})
    boxes = tuple(box for glyph in letters if (box := bounds(glyph.contours)))
    low, high, rows = min((box.ymin for box in boxes), default=0.0), max((box.ymax for box in boxes), default=0.0), rule.rows
    profiles = {name: _profile(glyph.contours, low, high, rows) for name, glyph in {glyph.name.text: glyph for glyph in (*tails, *heads)}.items()}
    above = next((index for index in range(rows) if low + index * (high - low) / rows >= 0), rows)
    gaps = {(tail.name.text, head.name.text): _apart(profiles[tail.name.text][above:], profiles[head.name.text][above:], head.advance) for tail in tails for head in heads}
    rhythm = middle(tuple(distance for (tail, _), distance in gaps.items() if source.named[tail].name.codes[0] in rule.script.standing and math.isfinite(distance)))
    tolerance, floor = rule.tolerance * source.upm, rule.floor * source.upm
    values = {
        pair: value
        for pair, distance in gaps.items()
        if math.isfinite(distance) and abs(rhythm - distance) > tolerance
        for value in (otRound(max(rhythm - distance, floor - _apart(profiles[pair[0]], profiles[pair[1]], source.named[pair[1]].advance))),)
        if value
    }
    return Pairs(features=frozendict(kern=frozendict()), values=frozendict({pair: Adjustment(dx=value, advance=value) for pair, value in values.items()})) if values else None


def spaced(source: Source, rule: Spacing, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Sets the word spaces and the glyphs drawn as them to the gap ratio times the inside gap within a band, and kerns non-joining tails to the median gap after standing letters.

    Returns:
        Ok(spaced source with its kerning), `ABSENT` for a face without an inked stem.
    """
    script = rule.script
    return placement(source, original, script.stem).map(lambda held: _spaced(source, rule, *(held[2].xx * value / source.upm for value in _gaps(original, corpus, script.stem, script.spaces[0]))))


def _spaced(source: Source, rule: Spacing, inside: float, across: float, advance: float, /) -> Source:
    """Sets every word space and every glyph drawn as one from the measured gaps in em and adds the tail pairs in place of any held."""
    target = min(max(rule.ratio * inside, rule.band[0]), rule.band[1])
    width = (target - (across - advance)) * source.upm if across else rule.band[0] * source.upm
    spaces = frozenset(rule.script.spaces)
    blanks = {glyph.name.text for glyph in source.glyphs if spaces.intersection(glyph.codes) and source.bounds(glyph.name.text) is None}
    for name in TopologicalSorter({glyph.name.text: tuple(part.name for part in glyph.components) for glyph in source.glyphs}).static_order():
        glyph = source.named[name]
        if len(glyph.components) == 1 and glyph.components[0].name in blanks and source.bounds(name) is None:
            blanks.add(name)
    words = tuple(msgspec.structs.replace(glyph, advance=width) for glyph in source.glyphs if glyph.name.text in blanks)
    pairs = _pairs(source, rule)
    return msgspec.structs.replace(
        source.replaced(*words),
        lookups=(
            *(
                msgspec.structs.replace(lookup, features=features)
                for lookup in source.lookups
                for features in (frozendict({tag: systems for tag, systems in lookup.features.items() if not (isinstance(lookup, Pairs) and tag == "kern")}),)
                if features
            ),
            *((pairs,) if pairs else ()),
        ),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Spacing", "spaced"]
