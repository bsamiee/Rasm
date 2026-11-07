"""Outline fits: union and tidy, straight art snapped, needle spurs cut at the join band, one nuqta, open counters, and the nuqta floor."""

from functools import partial

import msgspec
lazy from fontTools.misc.transform import Offset
lazy from pathops import PathOp
lazy from scipy.optimize import root_scalar

from rasm_typography._faces.declaration import Em, Ratio
from rasm_typography._rules.measures import _band, _dotted, _nuqta, Rule
lazy from rasm_typography._faces.glyphs import _Glyph, _Outline, _Source, Letter
lazy from rasm_typography._faces.names import _Form
lazy from rasm_typography._outlines.measure import _counters, _gap, _inradius
lazy from rasm_typography._outlines.paths import _area, _bounds, _combined, _offset, _transformed, _union
lazy from rasm_typography._outlines.tidy import _despiked, _snapped, _tidied

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


def _unified(contours: _Outline, canonical: _Outline, area_: float, single: tuple[float, float], /) -> _Outline:
    """Replaces a dot with an area ratio to the nuqta inside the single-dot band by the canonical nuqta at the dot's outline middle."""
    match _bounds(contours), _bounds(canonical):
        case (dot, model) if dot and model and single[0] < _area(contours) / area_ < single[1]:
            return _transformed(canonical, (1.0, 0.0, 0.0, 1.0, dot.middle[0] - model.middle[0], dot.middle[1] - model.middle[1]))
        case _:
            return contours


def _floored(body: _Outline, dots: tuple[_Outline, ...], floor: float, /) -> _Outline:
    """Moves a dot group vertically away from its letter's middle until the group stands the floor off the body."""
    group = _union(*dots)
    match _bounds(body), _bounds(group):
        case (held, moving) if held and moving and _gap(body, group) < floor:
            above = moving.middle[1] > held.middle[1]
            bound = abs(held.ymax - moving.ymin if above else moving.ymax - held.ymin) + held.height + moving.height + floor
            fit = root_scalar(lambda distance: _gap(body, _transformed(group, Offset(0.0, distance if above else -distance))) - floor, bracket=(0.0, bound))
            return (*body, *_transformed(group, Offset(0.0, fit.root if above else -fit.root)))
        case _:
            return (*body, *group)


def _opened(contours: _Outline, width: float, /) -> _Outline:
    """Widens counters whose largest inscribed diameter falls below the requested width."""
    narrow = tuple((void, span) for void in _counters(contours) if (span := 2 * _inradius(void)) < width)
    return _combined(contours, _union(*(_offset(void, (width - span) / 2) for void, span in narrow)), PathOp.DIFFERENCE) if narrow else contours


def _fit(rule: Outlines, joins: tuple[float, float] | None, canonical: _Outline, counted: frozenset[str], em: float, glyph: _Glyph, /) -> _Glyph:
    """Snaps straight art, tidies, cuts spurs under the join band, opens narrow counters, and redraws the single dots of one glyph as the nuqta."""
    straight = sum(not point.on for contour in glyph.contours for point in contour) <= 0.05 * sum(map(len, glyph.contours))
    contours = _tidied(_snapped(glyph.contours, rule.snap * em) if rule.snap is not None and straight else glyph.contours, merge=rule.merge * em, corner=rule.corner, sliver=rule.sliver * em)
    contours = _despiked(contours, joins) if joins and isinstance(glyph.role, Letter) and glyph.role.form in {_Form.INIT, _Form.MEDI} else contours
    contours = _opened(contours, rule.opened * em) if rule.opened is not None and glyph.name.text in counted else contours
    area_ = _area(canonical)
    body, dots = _dotted(msgspec.structs.replace(glyph, contours=contours), area_) if isinstance(glyph.role, Letter) else (contours, ())
    redrawn = tuple(_unified(piece, canonical, area_, rule.single) for piece in dots)
    return msgspec.structs.replace(glyph, contours=_floored(body, redrawn, rule.floor * em) if rule.floor is not None and redrawn else (*body, *(*piece for piece in redrawn)))


def _fitted(source: _Source, rule: Outlines, /) -> _Source:
    """Cleans every drawn glyph and redraws each letter's single dots as the face's nuqta, at least the floor off the body."""
    counted = frozenset(source.key(code, form) for code in rule.letters for form in _Form)
    fit = partial(_fit, rule, _band(source) if rule.spurs else None, _nuqta(source, rule.dot) if rule.dot is not None else (), counted, source.upm)
    return source.replaced(*(fit(glyph) for glyph in source.glyphs if glyph.contours))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Outlines"]
