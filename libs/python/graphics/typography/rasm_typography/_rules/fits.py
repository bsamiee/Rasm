"""Outline fits: union and tidy, straight art snapped, needle spurs cut at the join band, one nuqta, open counters, and the nuqta floor."""

from functools import partial
lazy from collections.abc import Iterable
lazy from itertools import combinations, starmap
lazy import math
lazy from statistics import fmean

import msgspec
lazy from fontTools.misc.transform import Offset
lazy from pathops import PathOp
lazy from scipy.optimize import root_scalar

from rasm_typography._faces.declaration import Em, Face, Ratio
from rasm_typography._rules import measures
lazy from rasm_records import Ok, Outcome
lazy from rasm_typography._faces.glyphs import Glyph, Letter, Point, Source
lazy from rasm_typography._faces.names import Form
lazy from rasm_typography._outlines import measure, paths

# --- [MODELS] ---------------------------------------------------------------------------


class Outlines(measures.Rule, frozen=True, kw_only=True):
    """Points merged, joins smoothed, slivers dropped, straight art snapped, spurs under the join band cut from the body, counters opened, single dots redrawn as the nuqta and spread to the gaps they had, and held the floor off their letter."""

    merge: Em = 0.004
    corner: float = 25.0
    sliver: Em = 0.004
    floor: Em | None = None
    spurs: bool = False
    snap: Em | None = None
    opened: Em | None = None
    letters: tuple[int, ...] = ()
    dot: int | None = None
    single: tuple[Ratio, Ratio] = (0.6, 1.5)
    shape: Ratio = 1.5
    spread: Ratio = 3.0

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Cleans every drawn glyph and redraws each letter's single dots as the face's nuqta, at least the floor off the body."""
        counted = frozenset(source.key(code, form) for code in self.letters for form in Form)
        fit = partial(_fit, self, measures.band(source) if self.spurs else None, measures.nuqta(source, self.dot) if self.dot is not None and isinstance(source.face, Face) and source.face.dotted else (), counted, source.upm)
        return Ok(source.replaced(*(fit(glyph) for glyph in source.glyphs if glyph.contours)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _levels(values: Iterable[float], tolerance: float, /) -> frozendict[float, float]:
    """Maps each value to the mean of its run, a run holding sorted values within `tolerance` of its first."""
    runs: list[list[float]] = []
    for value in sorted(set(values)):
        if runs and value - runs[-1][0] <= tolerance:
            runs[-1].append(value)
        else:
            runs.append([value])
    return frozendict({value: centre for run in runs for centre in (fmean(run),) for value in run})


def _fit(rule: Outlines, joins: tuple[float, float] | None, canonical: tuple[tuple[Point, ...], ...], counted: frozenset[str], em: float, glyph: Glyph, /) -> Glyph:
    """Snaps straight art, merges near points, drops slivers, smooths shallow joins, cuts spurs under the join band, opens narrow counters, and redraws the single dots of one glyph as the nuqta."""
    points = tuple(*contour for contour in glyph.contours)
    snapped = (
        tuple(tuple(Point(x=xs[point.x], y=ys[point.y], on=point.on) for point in contour) for contour in glyph.contours)
        if rule.snap is not None
        and sum(not point.on for point in points) <= 0.05 * len(points)
        and (xs := _levels((point.x for point in points), rule.snap * em))
        and (ys := _levels((point.y for point in points), rule.snap * em))
        else glyph.contours
    )
    merged: list[tuple[Point, ...]] = []
    for contour in snapped:
        held: list[Point] = []
        for point in contour:
            if not (held and math.dist((held[-1].x, held[-1].y), (point.x, point.y)) < rule.merge * em):
                held.append(point)
            elif point.on and not held[-1].on:
                held[-1] = point
        merged.append(tuple(held))
    smoothed: list[tuple[Point, ...]] = []
    for contour in (contour for piece in paths.pieces(paths.union(tuple(merged))) if measure.stroke(piece) >= rule.sliver * em for contour in piece):
        kept: list[Point] = []
        for point, after in zip(contour, (*contour[1:], contour[0]), strict=True):
            before = (kept or contour)[-1]
            ax, ay, bx, by = point.x - before.x, point.y - before.y, after.x - point.x, after.y - point.y
            if not (point.on and before.on == after.on and abs(math.degrees(math.atan2(ax * by - ay * bx, ax * bx + ay * by))) < rule.corner):
                kept.append(point)
        if len(kept) > 2:
            smoothed.append(tuple(kept))
    nuqta, model = paths.area(canonical), paths.bounds(canonical)
    stretch = measure.moments(canonical).elongation if model else 0.0
    body, dots = measures.dotted(msgspec.structs.replace(glyph, contours=tuple(smoothed)), nuqta) if isinstance(glyph.role, Letter) else (tuple(smoothed), ())
    if joins and isinstance(glyph.role, Letter) and glyph.role.form in {Form.INIT, Form.MEDI}:
        low, high = joins
        match paths.bounds(body):
            case paths.Box(xmin=xmin, ymin=ymin, xmax=xmax) if ymin < low:
                under = paths.combined(body, (paths.Box(xmin=xmin, ymin=ymin, xmax=xmax, ymax=low).contour,), PathOp.INTERSECTION)
                body = paths.combined(body, tuple(*piece for piece in paths.pieces(under) if (extent := paths.bounds(piece)) and extent.width < (high - low) / 2), PathOp.DIFFERENCE)
            case _:
                body = paths.union(body)
    if rule.opened is not None and glyph.name.text in counted and (narrow := tuple((void, span) for void in measure.counters(body) if (span := 2 * measure.inradius(void)) < rule.opened * em)):
        body = paths.combined(body, paths.union(*(paths.offset(void, (rule.opened * em - span) / 2) for void, span in narrow)), PathOp.DIFFERENCE)
    singles = tuple(
        (piece, box, model is not None and rule.single[0] < paths.area(piece) / nuqta < rule.single[1] and max(own := measure.moments(piece).elongation, stretch) < rule.shape * min(own, stretch))
        for piece in dots
        if (box := paths.bounds(piece))
    )
    sizes = tuple(paths.area(piece) for piece, _, single in singles if single)
    grown, (x, y) = math.sqrt(nuqta / fmean(sizes)) if sizes else 1.0, (paths.bounds(tuple(*piece for piece in dots)) or paths.Box(xmin=0.0, ymin=0.0, xmax=0.0, ymax=0.0)).middle

    def spaced(spread: float, /) -> tuple[tuple[tuple[Point, ...], ...], ...]:
        return tuple(
            paths.transformed(canonical if single and model else piece, Offset(x + spread * (box.middle[0] - x) - own.middle[0], y + spread * (box.middle[1] - y) - own.middle[1]))
            for piece, box, single in singles
            for own in ((model if single and model else box),)
        )

    def apart(spread: float, /) -> float:
        return min(starmap(measure.gap, combinations(spaced(spread), 2)), default=math.inf) - grown * min(starmap(measure.gap, combinations(dots, 2)), default=0.0)

    redrawn = spaced(grown if apart(grown) >= 0 else rule.spread * grown if apart(rule.spread * grown) < 0 else root_scalar(apart, bracket=(grown, rule.spread * grown)).root)
    if rule.floor is None or not redrawn:
        return msgspec.structs.replace(glyph, contours=(*body, *(*piece for piece in redrawn)))
    group, floor = paths.union(*redrawn), rule.floor * em
    match paths.bounds(body), paths.bounds(group):
        case (held_box, moving) if held_box and moving and measure.gap(body, group) < floor:
            sign = 1.0 if (above := moving.middle[1] > held_box.middle[1]) else -1.0
            bound = abs(held_box.ymax - moving.ymin if above else moving.ymax - held_box.ymin) + held_box.height + moving.height + floor
            fit = root_scalar(lambda distance: measure.gap(body, paths.transformed(group, Offset(0.0, sign * distance))) - floor, bracket=(0.0, bound))
            return msgspec.structs.replace(glyph, contours=(*body, *paths.transformed(group, Offset(0.0, sign * fit.root))))
        case _:
            return msgspec.structs.replace(glyph, contours=(*body, *group))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Outlines"]
