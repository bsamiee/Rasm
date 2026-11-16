"""Tails: the returning tails of final and isolated letters shortened along their flattest single stroke toward a share of the stem height."""

lazy from itertools import accumulate, groupby, pairwise

import msgspec
lazy from expression import Ok
lazy from fontTools.misc.bezierTools import segmentSegmentIntersections
lazy from fontTools.misc.transform import Offset, Transform

from rasm.typography._faces.declaration import Code, Em, Ratio, Script
from rasm.typography._rules import measures
lazy from rasm.records import Outcome
lazy from rasm.typography._faces.glyphs import Glyph, Joinings, Letter, Source
lazy from rasm.typography._outlines import paths

# --- [MODELS] ---------------------------------------------------------------------------


class Tails(measures.Rule, frozen=True, kw_only=True):
    """Final and isolated letters of the joining groups returning a tail, each past `length` stem heights shortened by the excess along its longest run of single-stroke columns `step` apart rising at most `slope` a step, kept at `least` of its length, the `dot` letter's dots moved with it."""

    script: Script
    length: Ratio
    dot: Code | None = None
    groups: frozenset[str] = frozenset({"BEH", "NOON", "YEH", "FARSI YEH", "KAF", "GAF"})
    step: Em = 0.004
    margin: Em = 0.02
    slope: Ratio = 0.35
    least: Ratio = 0.5

    def applied(self, source: Source, /, *, joinings: Joinings, **_: object) -> Outcome[Source]:
        """Compresses each long tail along its flattest single stroke, the outline right of the stroke moved left by the cut with the advance, dots and anchors moved with the outline under them."""
        step, margin = self.step * source.upm, self.margin * source.upm
        nuqta, stem = paths.area(measures.nuqta(source, self.dot)), source.bounds(source.key(self.script.stem))

        def shortened(own: Glyph, /) -> Glyph:
            glyph = source.drawn(own.name.text)
            match paths.bounds(glyph.contours), stem:
                case paths.Box() as box, (_, low, _, high) if (excess := box.width - self.length * (high - low)) > 0:
                    body, dots = measures.dotted(glyph, nuqta)
                    edges = paths.segments(body)
                    columns = {
                        index: (ys[0] + ys[1]) / 2
                        for index in range(int((box.width - 2 * margin) / step) + 1)
                        for x in (box.xmin + margin + index * step,)
                        for ys in (sorted(hit.pt[1] for edge in edges for hit in segmentSegmentIntersections(edge, ((x, box.ymin - 1), (x, box.ymax + 1))) if 0 <= hit.t1 < 1),)
                        if len(ys) == 2
                    }
                    ordered = sorted(columns)
                    breaks = accumulate((after != before + 1 or abs(columns[after] - columns[before]) > self.slope * step for before, after in pairwise(ordered)), initial=0)
                    run = max((tuple(index for index, _ in group) for _, group in groupby(zip(ordered, breaks, strict=True), key=lambda pair: pair[1])), key=len, default=(0,))
                    x0, x1 = box.xmin + margin + run[0] * step, box.xmin + margin + run[-1] * step
                    cut, middle = min(excess, (1 - self.least) * (x1 - x0)), (paths.bounds(tuple(*dot for dot in dots)) or box).middle[0]
                    return (
                        own
                        if cut <= 0
                        else msgspec.structs.replace(
                            glyph,
                            advance=glyph.advance - cut,
                            contours=paths.union(
                                paths.strip(body, box.xmin - 1, x0 + 1),
                                paths.transformed(paths.strip(body, x0, x1), Transform().translate(x0, 0).scale(1 - cut / (x1 - x0), 1).translate(-x0, 0)),
                                paths.transformed(paths.strip(body, x1 - 1, box.xmax + 1), Offset(-cut, 0)),
                                *(paths.transformed(dot, Offset(-cut * min(max((middle - x0) / (x1 - x0), 0.0), 1.0), 0)) for dot in dots),
                            ),
                            anchors=tuple(msgspec.structs.replace(anchor, x=anchor.x - cut * min(max((anchor.x - x0) / (x1 - x0), 0.0), 1.0)) for anchor in glyph.anchors),
                        )
                    )
                case _:
                    return own

        return Ok(source.replaced(*(shortened(own) for own in source.by_role(Letter) if not own.name.form.left and joinings.get(own.name.codes[0], (None, ""))[1] in self.groups)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Tails"]
