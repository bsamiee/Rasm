"""Anchors from ink: mark anchors at the original's clearance in a vowel and a wide class, ligature component anchors and carets, composite marks attached, `calt` dot groups moved clear of a joined neighbor, and `mark` lifts of marks clear of neighbors."""

lazy from itertools import groupby, pairwise
lazy import math

import msgspec
lazy from fontTools.misc.transform import Offset

from typography.faces.declaration import Em, Ratio, Script
from typography.rules.measures import Rule
lazy from typography.faces.faults import Outcome
lazy from typography.faces.glyphs import Anchor, Component, Glyph, Letter, Ligature, Mark, Outline, Side, Source
lazy from typography.faces.layout import Adjustment, Context, Contextual, Lift, Lifts
lazy from typography.faces.names import GlyphName
lazy from typography.outlines.measure import gap
lazy from typography.outlines.paths import area, bounds, Box, pieces, strip, transformed, union
lazy from typography.rules.measures import dotted, inked, key, middle, nuqta, placement
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary, drawn, marks, shaped

# --- [MODELS] ---------------------------------------------------------------------------


class Anchors(Rule, frozen=True, kw_only=True):
    """Anchors from ink at the original's vowel clearance, the vowel class over a column `wide` vowels across and wider marks over the widest, spaces anchored like the stem and joining glyph, dots held `dots` off a joined neighbor, and lifts up to `lifts` (non-joining, joined)."""

    script: Script
    stack: Em
    clearance: Em
    lifts: tuple[Em, Em] = (0.1, 0.46)
    dots: Em = 0.015
    wide: Ratio = 1.15


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [CLEARANCE]


def _clearances(original: Binary, corpus: Corpus, vowels: tuple[int, int], /) -> tuple[float, float]:
    """Median ink gap of the above vowel over and the below vowel under each form of each corpus letter shaped on the original, in its units."""
    marked = marks(original)
    over, under = (
        middle(
            tuple(
                distance
                for letter in corpus.letters
                for form in corpus.forms(letter, chr(vowel))
                for run in (shaped(original, form),)
                for outlines in (tuple(zip(run.placed, drawn(original, run), strict=True)),)
                if (bases := tuple(contours for placed, contours in outlines if placed.glyph not in marked and contours))
                and (held := tuple(contours for placed, contours in outlines if placed.glyph in marked))
                and math.isfinite(distance := gap(bases[0], held[0]))
                and distance > 0
            )
        )
        for vowel in vowels
    )
    return (over, under)


# --- [SEATS]


def _seats(glyph: Glyph, x: float, above: float, below: float, widths: tuple[float, float], suffix: str = "", /) -> tuple[Anchor, ...]:
    """Top and bottom anchors of the vowel and wide classes over and under the ink column of each width at x."""
    return tuple(
        anchor
        for tier, width in zip(("", "wide"), widths, strict=True)
        for column in (bounds(strip(glyph.contours, x - width / 2, x + width / 2)) or bounds(glyph.contours),)
        if column
        for anchor in (Anchor(name=f"top{tier}{suffix}", x=x, y=column.ymax + above), Anchor(name=f"bottom{tier}{suffix}", x=x, y=column.ymin - below))
    )


def _based(glyph: Glyph, above: float, below: float, widths: tuple[float, float], stem: Box, /) -> Glyph:
    """Anchors a base's marks over its own ink, a ligature's over each component arm read above 0.8 of the stem height with a caret between arms, or at equal shares of its advance where arms and components differ in count."""
    match bounds(glyph.contours), glyph.role:
        case None, _:
            return glyph
        case _, Ligature(components=components):
            arms = tuple(sorted((box for piece in pieces(strip(glyph.contours, stem.ymin + 0.8 * stem.height, 1e5, axis="y")) if (box := bounds(piece))), key=lambda box: -box.xmax))[: len(components)]
            step = glyph.advance / len(components)
            middles = tuple(arm.middle[0] for arm in arms) if len(arms) == len(components) else tuple(glyph.advance - (index - 0.5) * step for index in range(1, len(components) + 1))
            carets = (
                tuple((right.xmin + left.xmax) / 2 for right, left in pairwise(arms)) if len(arms) == len(components) else tuple(glyph.advance - index * step for index in range(1, len(components)))
            )
            return msgspec.structs.replace(
                glyph,
                anchors=(
                    *(*_seats(glyph, x, above, below, widths, f"_{index}") for index, x in enumerate(middles, 1)),
                    *(Anchor(name=f"caret_{index}", x=x, y=0.0) for index, x in enumerate(carets, 1)),
                ),
            )
        case box, _:
            seat = next((anchor.x for anchor in glyph.anchors if anchor.name == "top"), box.middle[0])
            return msgspec.structs.replace(glyph, anchors=_seats(glyph, min(max(seat, box.xmin, 0.0), box.xmax, glyph.advance), above, below, widths))


def _marked(glyph: Glyph, stack: float, wide: float, /) -> Glyph:
    """Anchors a mark at its ink foot, in the wide class past the wide width, and marks stacked on it the stack gap past its head in both classes."""
    match bounds(glyph.contours), glyph.role:
        case Box(middle=(x, _), ymin=ymin, ymax=ymax, width=width), Mark(side=side):
            name, (foot, head) = side.anchor, (ymin, ymax + stack) if side is Side.ABOVE else (ymax, ymin - stack)
            return msgspec.structs.replace(glyph, anchors=(Anchor(name=f"_{name}{'wide' if width > wide else ''}", x=x, y=foot), *(Anchor(name=f"{name}{tier}", x=x, y=head) for tier in ("", "wide"))))
        case _:
            return glyph


def _seated(source: Source, glyph: Glyph, /) -> Glyph:
    """Moves a composite's mark component onto its base component's anchor and gives the composite the base's anchors."""
    match tuple(source.named.get(part.name) for part in glyph.components):
        case (Glyph() as base, Glyph(role=Mark()) as mark):
            offset = next(((seat.x - side.x, seat.y - side.y) for side in mark.anchors if side.name.startswith("_") for seat in base.anchor(side.name[1:]).to_list()), (0.0, 0.0))
            return msgspec.structs.replace(glyph, anchors=base.anchors, components=(glyph.components[0], Component(name=mark.name.text, transform=(1.0, 0.0, 0.0, 1.0, *offset))))
        case (Glyph() as base,):
            return msgspec.structs.replace(glyph, anchors=base.anchors)
        case _:
            return glyph


# --- [NEIGHBOURS]


def _near(ink: Box, other: Box, shift: float, reach: float, /) -> bool:
    """Whether another box moved by a shift stands within a reach of a box."""
    return (ink.grown(reach) & msgspec.structs.replace(other, xmin=other.xmin + shift, xmax=other.xmax + shift)) is not None


def _short(ink: Outline, other: Outline, shift: float, reach: float, /) -> float:
    """How far ink stands inside the reach of other ink moved by a shift."""
    return max(reach - gap(ink, transformed(other, Offset(shift, 0))), 0.0)


def _cleared(source: Source, rule: Anchors, /) -> tuple[tuple[Glyph, ...], tuple[Contextual, ...]]:
    """Alternates of letters whose dot group stands within the dot clearance of a joined neighbor, the group moved away from it inside the advance, and the `calt` rules choosing them."""
    letters, reach, unit = tuple(glyph for glyph in source.by_role(Letter) if glyph.contours), rule.dots * source.upm, rule.stack * source.upm / 2
    boxes, area_ = {glyph.name.text: box for glyph in letters if (box := bounds(glyph.contours))}, area(nuqta(source, rule.script.dotted))
    groups = {glyph.name.text: (body, union(*dots), bounds(union(*dots))) for glyph in letters for body, dots in (dotted(glyph, area_),) if dots}
    moves = sorted(
        (glyph.name.text, after, move, other.name.text)
        for glyph in letters
        if glyph.name.text in groups
        for _, group, box in (groups[glyph.name.text],)
        if box
        for other in letters
        for after, shift in ((True, -other.advance), (False, glyph.advance))
        if (glyph.name.form.left and other.name.form.right if after else other.name.form.left and glyph.name.form.right)
        and _near(box, boxes[other.name.text], shift, reach)
        and (short := _short(group, other.contours, shift, reach))
        and (move := round(math.copysign(min(math.ceil(short / unit) * unit, max(glyph.advance - box.xmax if after else box.xmin, 0.0)), 1 if after else -1)))
    )
    shifts = {(name, move): GlyphName.parse(f"{name}.calt{index}") for index, (name, move) in enumerate(dict.fromkeys((name, move) for name, _, move, _ in moves), 1)}
    alternates = tuple(msgspec.structs.replace(source.named[name], name=alias, contours=(*groups[name][0], *transformed(groups[name][1], Offset(move, 0)))) for (name, move), alias in shifts.items())
    rules = tuple(
        Context(input=((name,),), lookahead=(others,), replacement=(shifts[name, move].text,)) if after else Context(backtrack=(others,), input=((name,),), replacement=(shifts[name, move].text,))
        for (name, after, move), group in groupby(moves, key=lambda row: row[:3])
        for others in (tuple(other for *_, other in group),)
    )
    return (alternates, (Contextual(feature="calt", order=max((lookup.order for lookup in source.lookups), default=0) + 1, rules=rules),) if rules else ())


def _lifts(source: Source, rule: Anchors, /) -> tuple[Lifts, ...]:
    """Lifts the above marks of a letter clear of the letter before or after it, one rule per letter, side, and lift rounded up to a step, joined pairs up to the joined limit and others up to the non-joining one."""
    letters, reach, step = tuple(glyph for glyph in source.by_role(Letter) if glyph.contours), rule.clearance * source.upm, rule.stack * source.upm / 2
    stack = union(*(transformed(glyph.contours, Offset(-seat.x, -seat.y)) for glyph in source.glyph(key(rule.script.vowels[0])).to_list() for seat in glyph.anchor("_top").to_list()))
    placed = {glyph.name.text: (ink, box) for glyph in letters for top in glyph.anchor("top").to_list() for ink in (transformed(stack, Offset(top.x, top.y)),) if (box := bounds(ink))}
    boxes = {glyph.name.text: box for glyph in letters if (box := bounds(glyph.contours))}
    lifted = sorted(
        (owner.name.text, after, round(min(math.ceil(short / step) * step, rule.lifts[first.name.form.left] * source.upm)), other.name.text)
        for first in letters
        for second in letters
        if first.name.form.left == second.name.form.right
        for owner, other, after, shift in ((first, second, True, -second.advance), (second, first, False, second.advance))
        if owner.name.text in placed and _near(placed[owner.name.text][1], boxes[other.name.text], shift, reach) and (short := _short(placed[owner.name.text][0], other.contours, shift, reach))
    )
    above = tuple(glyph.name.text for glyph in source.by_role(Mark) if glyph.role == Mark(side=Side.ABOVE))
    rules = tuple(
        Lift(input=((name,), above), lookahead=(others,), adjustments=(None, Adjustment(dy=lift)))
        if after
        else Lift(backtrack=(others,), input=((name,), above), adjustments=(None, Adjustment(dy=lift)))
        for (name, after, lift), group in groupby(lifted, key=lambda row: row[:3])
        for others in (tuple(other for *_, other in group),)
    )
    return (Lifts(feature="mark", rules=rules),) if rules and above else ()


# --- [COMPOSITION]


def seated(source: Source, rule: Anchors, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Sets every base, ligature, and mark anchor from ink at the original's mark clearance, attaches composite marks, and adds the `calt` dot alternates and neighbor lifts.

    Returns:
        Ok(anchored source with its alternates and lifts), `ABSENT` for a face without an inked stem.
    """
    script = rule.script
    return inked(source, script.stem).bind(
        lambda held: placement(source, original, script.stem).map(lambda move: _anchored(source, rule, held[1], move[0], _clearances(original, corpus, script.vowels)))
    )


def _anchored(source: Source, rule: Anchors, stem: Box, scale: float, clearances: tuple[float, float], /) -> Source:
    """Anchors bases, marks, spaces, and composites at the clearances scaled onto the source, then moves dot groups and lifts marks clear of neighbors."""
    script, stack, (above, below) = rule.script, rule.stack * source.upm, (scale * clearances[0], scale * clearances[1])
    widths = tuple(
        max((box.width for glyph in source.by_role(Mark) if (not codes or glyph.codes[:1] in codes) and (box := bounds(glyph.contours))), default=stem.width)
        for codes in ({(code,) for code in script.recipes}, set())
    )
    bases = tuple(
        _based(glyph, above, below, (rule.wide * widths[0], widths[1]), stem)
        for glyph in source.glyphs
        if (isinstance(glyph.role, Letter | Ligature) or glyph.name.codes == (script.carrier,)) and glyph.contours
    )
    seats = source.replaced(*bases, *(_marked(glyph, stack, rule.wide * widths[0]) for glyph in source.by_role(Mark)))
    models, spaces = {"top": key(script.stem), "bottom": key(script.kashida) if script.kashida is not None else ""}, frozenset((space,) for space in script.spaces)
    spaced = seats.replaced(
        *(
            msgspec.structs.replace(
                glyph,
                anchors=tuple(
                    Anchor(name=anchor.name, x=glyph.advance / 2, y=anchor.y)
                    for side, model in models.items()
                    for held in seats.glyph(model).to_list()
                    for anchor in held.anchors
                    if anchor.name.removesuffix("wide") == side
                ),
            )
            for glyph in seats.glyphs
            if glyph.name.codes in spaces and not glyph.components
        )
    )
    done = spaced.replaced(*(_seated(spaced, glyph) for glyph in spaced.glyphs if glyph.components))
    alternates, calt = _cleared(done, rule)
    return msgspec.structs.replace(done.replaced(*alternates), lookups=(*(lookup for lookup in done.lookups if lookup.feature != "calt" and not isinstance(lookup, Lifts)), *calt, *_lifts(done, rule)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anchors", "seated"]
