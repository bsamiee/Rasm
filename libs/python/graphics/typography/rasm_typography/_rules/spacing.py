"""Spacing: the word space from the gap ratio over the original's shaped words, widened under a mark it carries, and `kern` pairs holding every letter that ends a part, across a joining break or ZWNJ, to the median inside gap."""

from typing import Annotated
lazy from graphlib import TopologicalSorter
lazy from itertools import groupby, pairwise
lazy import math

import msgspec
lazy from expression import Result
lazy from fontTools.misc.roundTools import otRound

from rasm_typography._faces.declaration import Band, Em, Ratio, Script
from rasm_typography._rules import measures
lazy from rasm_records import Error, Ok, Outcome
lazy from rasm_typography._faces.glyphs import Letter, Mark, Source
lazy from rasm_typography._faces.layout import Adjustment, Chain, Lifts, Pairs
lazy from rasm_typography._faces.names import Form
lazy from rasm_typography._outlines import measure, paths
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Spacing(measures.Rule, frozen=True, kw_only=True):
    """Word space at the ratio times the inside gap within a band, and `kern` pairs holding each letter ending a part within a tolerance of the gap after the script's standing letters, never under the outline floor from the next part's first two letters or the next word's first."""

    script: Script
    ratio: Ratio
    band: Band
    tolerance: Em = 0.03
    floor: Em
    rows: Annotated[int, msgspec.Meta(gt=0)] = 32

    def applied(self, source: Source, /, *, original: Binary, corpus: Corpus, **_: object) -> Outcome[Source]:
        """Sets the word spaces and the glyphs drawn as them to the gap ratio times the inside gap within a band, and kerns non-joining tails to the median gap after standing letters.

        Returns:
            Ok(spaced source with its kerning), `ABSENT` for a face without a drawn stem.
        """
        script = self.script
        match measures.placement(source, original, script.stem):
            case Result(tag="ok", ok=(_, _, move)):
                blank, marked = original.encoded.get(script.spaces[0]), original.marks
                body = 0.25 * paths.area(original.outline(original.encoded[script.stem]))
                runs = tuple(original.shaped(line, language=language) for language, lines in corpus.prose.items() for line in lines)
                words = tuple(
                    tuple(
                        paths.union(*(contours for placed, contours in group if placed.glyph not in marked))
                        for is_space, group in groupby(zip(run.placed, original.drawn(run), strict=False), key=lambda pair: pair[0].glyph == blank)
                        if not is_space
                    )
                    for run in runs
                )
                bodies = tuple(
                    tuple(
                        piece
                        for piece, _ in sorted(
                            ((piece, box) for piece in paths.pieces(word) for box in (paths.bounds(piece),) if paths.area(piece) >= body and box is not None),
                            key=lambda held: (held[1].xmin, held[1].ymin, held[1].xmax, held[1].ymax),
                        )
                    )
                    for line in words
                    for word in line
                )
                measured = (
                    measures.middle(tuple(distance for word in bodies for left, right in pairwise(word) if 0 < (distance := measure.gap(left, right)) < math.inf)),
                    measures.middle(tuple(distance for line in words for left, right in pairwise(line) if 0 < (distance := measure.gap(left, right)) < math.inf)),
                    next((placed.advance for run in runs for placed in run.placed if placed.glyph == blank), 0.0),
                )
                letters = tuple(drawn for glyph in source.by_role(Letter) if len(glyph.name.codes) == 1 and (drawn := source.drawn(glyph.name.text)).contours)
                tails = tuple(glyph for glyph in letters if not glyph.name.form.left)
                heads = tuple(glyph for glyph in letters if glyph.name.form in {Form.ISOL, Form.INIT})
                seconds = tuple(glyph for glyph in letters if glyph.name.form.right)
                boxes = tuple(box for glyph in letters if (box := paths.bounds(glyph.contours)))
                low, high, rows = min((box.ymin for box in boxes), default=0.0), max((box.ymax for box in boxes), default=0.0), self.rows
                step = (high - low) / rows
                profiles = {
                    name: tuple((box.xmin, box.xmax) if (box := paths.bounds(paths.strip(glyph.contours, low + index * step, low + (index + 1) * step, axis="y"))) else None for index in range(rows))
                    for name, glyph in {glyph.name.text: glyph for glyph in (*tails, *heads, *seconds)}.items()
                }
                above = next((index for index in range(rows) if low + index * (high - low) / rows >= 0), rows)
                gaps = {
                    (tail.name.text, head.name.text): min(
                        (right[0] - (left[1] - head.advance) for right, left in zip(profiles[tail.name.text][above:], profiles[head.name.text][above:], strict=False) if right and left),
                        default=math.inf,
                    )
                    for tail in tails
                    for head in heads
                }
                rhythm = measures.middle(tuple(distance for (tail, _), distance in gaps.items() if source.named[tail].name.codes[0] in script.standing and math.isfinite(distance)))
                tolerance, floor = self.tolerance * source.upm, self.floor * source.upm
                band = math.ceil(floor / step) if step else 0
                edges = {glyph.name.text: tuple(None if row is None else row[1] - glyph.advance for row in profiles[glyph.name.text]) for glyph in (*heads, *seconds)}
                following = tuple(max((edge for glyph in seconds if (edge := edges[glyph.name.text][index]) is not None), default=None) for index in range(rows))

                def needed(tail: str, right: tuple[float | None, ...], space: float, /) -> float:
                    return max(
                        (floor - left[0] + edge - space for index, left in enumerate(profiles[tail]) if left for edge in right[max(index - band, 0) : index + band + 1] if edge is not None),
                        default=-math.inf,
                    )

                values = {
                    pair: value
                    for pair, distance in gaps.items()
                    if math.isfinite(distance)
                    for value in (
                        otRound(
                            max(
                                rhythm - distance if abs(rhythm - distance) > tolerance else 0.0,
                                needed(pair[0], edges[pair[1]], 0.0),
                                needed(pair[0], following, source.named[pair[1]].advance) if source.named[pair[1]].name.form.left else -math.inf,
                            )
                        ),
                    )
                    if value
                }
                inside, across, advance = (move.xx * value / source.upm for value in measured)
                width = (min(max(self.ratio * inside, self.band[0]), self.band[1]) - (across - advance)) * source.upm if across else self.band[0] * source.upm
                spaces = frozenset(script.spaces)
                blanks = {glyph.name.text for glyph in source.glyphs if spaces.intersection(glyph.codes) and source.bounds(glyph.name.text) is None}
                for name in TopologicalSorter({glyph.name.text: tuple(part.name for part in glyph.components) for glyph in source.glyphs}).static_order():
                    glyph = source.named[name]
                    if len(glyph.components) == 1 and glyph.components[0].name in blanks and source.bounds(name) is None:
                        blanks.add(name)
                reaching = {tail.name.text: value for tail in tails for value in (otRound(max((0.0, *(needed(tail.name.text, edges[head.name.text], width) for head in heads)))),) if value}
                spaced = {(tail, blank): value for tail, value in reaching.items() for blank in blanks}
                carried = tuple(
                    Chain(input=((tuple(sorted(blanks)), Adjustment(dx=otRound(grow / 2), advance=otRound(grow))), ((glyph.name.text,), None)))
                    for glyph in source.by_role(Mark)
                    if (rect := source.bounds(glyph.name.text)) is not None and (grow := rect[2] - rect[0] + 2 * floor - width) > 0
                )
                lookups = (
                    *(
                        msgspec.structs.replace(lookup, features=features)
                        for lookup in source.lookups
                        for features in (frozendict({tag: systems for tag, systems in lookup.features.items() if not (isinstance(lookup, Pairs) and tag == "kern")}),)
                        if features
                    ),
                    *(
                        (Pairs(features=frozendict(kern=frozendict()), values=frozendict({pair: Adjustment(dx=value, advance=value) for pair, value in (values | spaced).items()})),)
                        if values or spaced
                        else ()
                    ),
                    *((Lifts(features=frozendict(kern=frozendict()), rules=carried),) if carried and blanks else ()),
                )
                return Ok(msgspec.structs.replace(source.replaced(*(msgspec.structs.replace(glyph, advance=width) for glyph in source.glyphs if glyph.name.text in blanks)), lookups=lookups))
            case Result(error=faults):
                return Error(faults)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Spacing"]
