"""Outline fits: union and tidy, straight art snapped, needle spurs cut at the join band, one nuqta, open counters, and the nuqta floor."""

from functools import partial

import msgspec

from typography.faces.declaration import Em, Ratio
from typography.rules.measures import Rule
lazy from typography.faces.glyphs import Glyph, Letter, Outline, Source
lazy from typography.faces.names import Form
lazy from typography.outlines.measure import counter, gap
lazy from typography.outlines.paths import area, bounds, combined, offset, Operation, pieces, transformed, union
lazy from typography.outlines.tidy import despiked, snapped, tidied
lazy from typography.rules.measures import band, dotted, key, nuqta

# --- [MODELS] ---------------------------------------------------------------------------


class Outlines(Rule, frozen=True, kw_only=True):
    """Points merged, joins smoothed, slivers dropped, straight art snapped, needle spurs cut under the join band, counters of the counter letters opened, single dots redrawn as the dotted letter's nuqta and held the floor off their letter."""

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


# --- [OPERATIONS] -----------------------------------------------------------------------


def _unified(contours: Outline, canonical: Outline, area_: float, single: tuple[float, float], /) -> Outline:
    """Replaces a dot with an area ratio to the nuqta inside the single-dot band by the canonical nuqta at the dot's ink middle."""
    match bounds(contours), bounds(canonical):
        case (dot, model) if dot and model and single[0] < area(contours) / area_ < single[1]:
            return transformed(canonical, (1.0, 0.0, 0.0, 1.0, dot.middle[0] - model.middle[0], dot.middle[1] - model.middle[1]))
        case _:
            return contours


def _floored(body: Outline, dots: tuple[Outline, ...], floor: float, /) -> Outline:
    """Moves a dot group vertically away from its letter's middle until the group stands the floor off the body."""
    group = union(*dots)
    match bounds(body), bounds(group):
        case (held, moving) if held and moving and (short := floor - gap(body, group)) > 0:
            return (*body, *transformed(group, (1.0, 0.0, 0.0, 1.0, 0.0, short if moving.middle[1] > held.middle[1] else -short)))
        case _:
            return (*body, *group)


def _opened(contours: Outline, width: float, /) -> Outline:
    """Grows each counter narrower than a width until its narrowest span reaches it."""
    narrow = tuple(hole for _, *holes in pieces(contours) for hole in holes if (span := counter((hole,))) < width and span > 0)
    return combined(contours, union(*(offset((hole,), (width - counter((hole,))) / 2) for hole in narrow)), Operation.DIFFERENCE) if narrow else contours


def _fit(rule: Outlines, joins: tuple[float, float] | None, canonical: Outline, counted: frozenset[str], em: float, glyph: Glyph, /) -> Glyph:
    """Snaps straight art, tidies, cuts spurs under the join band, opens narrow counters, and redraws the single dots of one glyph as the nuqta."""
    straight = sum(not point.on for contour in glyph.contours for point in contour) <= 0.05 * sum(map(len, glyph.contours))
    contours = tidied(snapped(glyph.contours, rule.snap * em) if rule.snap is not None and straight else glyph.contours, merge=rule.merge * em, corner=rule.corner, sliver=rule.sliver * em)
    contours = despiked(contours, joins) if joins and isinstance(glyph.role, Letter) and glyph.role.form in {Form.INIT, Form.MEDI} else contours
    contours = _opened(contours, rule.opened * em) if rule.opened is not None and glyph.name.text in counted else contours
    area_ = area(canonical)
    body, dots = dotted(msgspec.structs.replace(glyph, contours=contours), area_) if isinstance(glyph.role, Letter) else (contours, ())
    seated = tuple(_unified(piece, canonical, area_, rule.single) for piece in dots)
    return msgspec.structs.replace(glyph, contours=_floored(body, seated, rule.floor * em) if rule.floor is not None and seated else (*body, *(*piece for piece in seated)))


def fitted(source: Source, rule: Outlines, /) -> Source:
    """Cleans every drawn glyph and redraws each letter's single dots as the face's nuqta, at least the floor off the body."""
    counted = frozenset(key(code, form) for code in rule.letters for form in Form)
    fit = partial(_fit, rule, band(source) if rule.spurs else None, nuqta(source, rule.dot) if rule.dot is not None else (), counted, source.upm)
    return source.replaced(*(fit(glyph) for glyph in source.glyphs if glyph.contours))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Outlines", "fitted"]
