"""Anchors from outlines: mark anchors at the original's clearance in a vowel and a wide class, ligature component anchors and carets, composite marks attached, `calt` dot groups moved clear of a joined neighbor, and `mark` lifts of marks clear of neighbors."""

# mypy: disable-error-code="no-untyped-call"

lazy from functools import partial
lazy from graphlib import TopologicalSorter
lazy from itertools import groupby, pairwise
lazy import math

import msgspec
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.misc.transform import Offset, Transform
lazy from ufo2ft.filters.propagateAnchors import AnchorData, anchors_traversing_components, get_xy_rotation, rename_anchor_for_scale
lazy from ufoLib2.objects import Component

from rasm_typography._faces.declaration import Em, Ratio, Script
from rasm_typography._rules.measures import _dotted, _middle, _nuqta, _placement, Rule
lazy from rasm_records import Outcome
lazy from rasm_typography._faces.glyphs import _Anchor, _Glyph, _Outline, _Side, Letter, Ligature, Mark, Source
lazy from rasm_typography._faces.layout import _Adjustment, _Chain, Contextual, Lifts
lazy from rasm_typography._outlines.measure import _gap
lazy from rasm_typography._outlines.paths import _area, _bounds, _Box, _pieces, _strip, _transformed, _union
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Anchors(Rule, frozen=True, kw_only=True):
    """Anchors from outlines at the original's vowel clearance, the vowel class over a column `wide` vowels across and wider marks over the widest, spaces anchored like the stem and joining glyph, dots held `dots` off a joined neighbor, and lifts up to `lifts` (non-joining, joined)."""

    script: Script
    stack: Em
    clearance: Em
    lifts: tuple[Em, Em] = (0.1, 0.46)
    dots: Em = 0.015
    wide: Ratio = 1.15


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLEARANCE]


def _clearances(original: Binary, corpus: Corpus, vowels: tuple[int, int], /) -> tuple[float, float]:
    """Median outline gap of the above vowel over and the below vowel under each form of each corpus letter shaped on the original, in its units."""
    marked = original.marks
    over, under = (
        _middle(
            tuple(
                distance
                for letter in corpus.letters
                for form in corpus.forms(letter, chr(vowel))
                for language in corpus.languages
                for run in (original.shaped(form, language=language),)
                for outlines in (tuple(zip(run.placed, original.drawn(run), strict=False)),)
                if (bases := _union(*(contours for placed, contours in outlines if placed.glyph not in marked)))
                and (held := _union(*(contours for placed, contours in outlines if placed.glyph in marked)))
                and (distance := _gap(bases, held)) > 0
            )
        )
        for vowel in vowels
    )
    return (over, under)


# --- [ANCHORS]


def _anchors(glyph: _Glyph, x: float, above: float, below: float, widths: tuple[float, float], suffix: str = "", /) -> tuple[_Anchor, ...]:
    """Top and bottom anchors of the vowel and wide classes over and under the outline column of each width at x."""
    return tuple(
        anchor
        for tier, width in zip(("", "wide"), widths, strict=False)
        for column in (_bounds(_strip(glyph.contours, x - width / 2, x + width / 2)) or _bounds(glyph.contours),)
        if column
        for anchor in (_Anchor(name=f"top{tier}{suffix}", x=x, y=column.ymax + above), _Anchor(name=f"bottom{tier}{suffix}", x=x, y=column.ymin - below))
    )


def _based(glyph: _Glyph, above: float, below: float, widths: tuple[float, float], stem: _Box, /) -> _Glyph:
    """Anchors a base's marks over its own outline, a ligature's over each component arm read above 0.8 of the stem height with a caret between arms, or at equal shares of its advance where arms and components differ in count."""
    match _bounds(glyph.contours), glyph.role:
        case None, _:
            return glyph
        case box, Ligature(components=components):
            threshold = stem.ymin + 0.8 * stem.height
            arms = (
                tuple(sorted((arm for piece in _pieces(_strip(glyph.contours, threshold, box.ymax, axis="y")) if (arm := _bounds(piece))), key=lambda arm: -arm.xmax))[: len(components)]
                if threshold < box.ymax
                else ()
            )
            step = glyph.advance / len(components)
            middles = tuple(arm.middle[0] for arm in arms) if len(arms) == len(components) else tuple(glyph.advance - (index - 0.5) * step for index in range(1, len(components) + 1))
            carets = (
                tuple((right.xmin + left.xmax) / 2 for right, left in pairwise(arms)) if len(arms) == len(components) else tuple(glyph.advance - index * step for index in range(1, len(components)))
            )
            return msgspec.structs.replace(
                glyph,
                anchors=(
                    *(*_anchors(glyph, x, above, below, widths, f"_{index}") for index, x in enumerate(middles, 1)),
                    *(_Anchor(name=f"caret_{index}", x=x, y=0.0) for index, x in enumerate(carets, 1)),
                ),
            )
        case box, _:
            x = next((anchor.x for anchor in glyph.anchors if anchor.name == "top"), box.middle[0])
            return msgspec.structs.replace(glyph, anchors=_anchors(glyph, min(max(x, box.xmin, 0.0), box.xmax, glyph.advance), above, below, widths))


def _marked(glyph: _Glyph, stack: float, wide: float, /) -> _Glyph:
    """Anchors a mark at its outline foot, in the wide class past the wide width, and marks stacked on it the stack gap past its head in both classes."""
    match _bounds(glyph.contours), glyph.role:
        case _Box(middle=(x, _), ymin=ymin, ymax=ymax, width=width), Mark(side=side):
            name, (foot, head) = side.anchor, (ymin, ymax + stack) if side is _Side.ABOVE else (ymax, ymin - stack)
            return msgspec.structs.replace(
                glyph, anchors=(_Anchor(name=f"_{name}{'wide' if width > wide else ''}", x=x, y=foot), *(_Anchor(name=f"{name}{tier}", x=x, y=head) for tier in ("", "wide")))
            )
        case _:
            return glyph


# --- [NEIGHBOURS]


def _near(own: _Box, other: _Box, shift: float, reach: float, /) -> bool:
    """Whether another box moved by a shift stands within a reach of a box."""
    return (own.grown(reach) & other.shifted(shift, 0)) is not None


def _short(own: _Outline, other: _Outline, shift: float, reach: float, /) -> float:
    """How far an outline stands inside the reach of another outline moved by a shift."""
    return max(reach - _gap(own, _transformed(other, Offset(shift, 0))), 0.0)


def _cleared(source: Source, rule: Anchors, /) -> tuple[tuple[_Glyph, ...], tuple[Contextual, ...]]:
    """Alternates of letters whose dot group stands within the dot clearance of a joined neighbor, the group moved away from it inside the advance, and the `calt` rules choosing them."""
    letters, reach, unit = tuple(drawn for glyph in source.by_role(Letter) if (drawn := source.drawn(glyph.name.text)).contours), rule.dots * source.upm, rule.stack * source.upm / 2
    boxes, area_ = {glyph.name.text: box for glyph in letters if (box := _bounds(glyph.contours))}, _area(_nuqta(source, rule.script.dotted))
    groups = {glyph.name.text: (body, group, _bounds(group)) for glyph in letters for body, dots in (_dotted(glyph, area_),) if dots for group in (_union(*dots),)}
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
        and (move := otRound(math.copysign(min(math.ceil(short / unit) * unit if unit else short, max(glyph.advance - box.xmax if after else box.xmin, 0.0)), 1 if after else -1)))
    )
    shifts = {
        (name, move): msgspec.structs.replace(held, variant=f"{held.variant}.{suffix}" if held.variant else suffix)
        for index, (name, move) in enumerate(dict.fromkeys((name, move) for name, _, move, _ in moves), 1)
        for held, suffix in ((source.named[name].name, f"calt{index}"),)
    }
    alternates = tuple(
        msgspec.structs.replace(source.drawn(name), name=alias, codes=frozenset(), contours=(*groups[name][0], *_transformed(groups[name][1], Offset(move, 0))), components=())
        for (name, move), alias in shifts.items()
    )
    rules = tuple(
        _Chain(input=(((name,), shifts[name, move].text),), lookahead=(others,)) if after else _Chain(backtrack=(others,), input=(((name,), shifts[name, move].text),))
        for (name, after, move), group in groupby(moves, key=lambda row: row[:3])
        for others in (tuple(other for *_, other in group),)
    )
    return (alternates, (Contextual(features=frozendict(calt=frozendict()), order=max((lookup.order for lookup in source.lookups), default=0) + 1, rules=rules),) if rules else ())


def _lifts(source: Source, rule: Anchors, /) -> tuple[Lifts, ...]:
    """Lifts above marks clear of neighboring letters, continuously for zero stack gap or rounded up to its half-step, within the joined and non-joining limits."""
    letters, reach, step = tuple(drawn for glyph in source.by_role(Letter) if (drawn := source.drawn(glyph.name.text)).contours), rule.clearance * source.upm, rule.stack * source.upm / 2
    stack = _union(
        *(
            _transformed(glyph.contours, Offset(-anchor.x, -anchor.y))
            for raw in source.glyph(source.key(rule.script.vowels[0])).to_list()
            for glyph in (source.drawn(raw.name.text),)
            for anchor in glyph.anchor("_top").to_list()
        )
    )
    placed = {glyph.name.text: (outline, box) for glyph in letters for top in glyph.anchor("top").to_list() for outline in (_transformed(stack, Offset(top.x, top.y)),) if (box := _bounds(outline))}
    boxes = {glyph.name.text: box for glyph in letters if (box := _bounds(glyph.contours))}
    lifted = sorted(
        (owner.name.text, after, otRound(min(math.ceil(short / step) * step if step else short, rule.lifts[first.name.form.left] * source.upm)), other.name.text)
        for first in letters
        for second in letters
        if first.name.form.left == second.name.form.right
        for owner, other, after, shift in ((first, second, True, -second.advance), (second, first, False, second.advance))
        if owner.name.text in placed and _near(placed[owner.name.text][1], boxes[other.name.text], shift, reach) and (short := _short(placed[owner.name.text][0], other.contours, shift, reach))
    )
    above = tuple(glyph.name.text for glyph in source.by_role(Mark) if glyph.role == Mark(side=_Side.ABOVE))
    rules = tuple(
        _Chain(input=(((name,), None), (above, _Adjustment(dy=lift))), lookahead=(others,)) if after else _Chain(backtrack=(others,), input=(((name,), None), (above, _Adjustment(dy=lift))))
        for (name, after, lift), group in groupby(lifted, key=lambda row: row[:3])
        for others in (tuple(other for *_, other in group),)
    )
    return (Lifts(features=frozendict(mark=frozendict()), rules=rules),) if rules and above else ()


# --- [COMPOSITION]


def _anchored(source: Source, rule: Anchors, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Sets every base, ligature, and mark anchor from outlines at the original's mark clearance, attaches composite marks, and adds the `calt` dot alternates and neighbor lifts.

    Returns:
        Ok(anchored source with its alternates and lifts), `ABSENT` for a face without a drawn stem.
    """
    script = rule.script
    return _placement(source, original, script.stem).map(lambda held: _positioned(source, rule, held[1], held[2].xx, _clearances(original, corpus, script.vowels)))


def _positioned(source: Source, rule: Anchors, stem: _Box, scale: float, clearances: tuple[float, float], /) -> Source:
    """Anchors bases, marks, spaces, and composites at the clearances scaled onto the source, then moves dot groups and lifts marks clear of neighbors."""
    script, stack, (above, below) = rule.script, rule.stack * source.upm, (scale * clearances[0], scale * clearances[1])
    marks = tuple((glyph, _Box.of(rect)) for glyph in source.by_role(Mark) if (rect := source.bounds(glyph.name.text)) is not None)
    widths = tuple(max((box.width for glyph, box in marks if not codes or glyph.name.codes[:1] in codes), default=stem.width) for codes in ({(code,) for code in script.recipes}, set()))
    carrier = source.key(script.carrier) if script.carrier is not None else None
    bases = tuple(
        _based(glyph, above, below, (rule.wide * widths[0], widths[1]), stem)
        for glyph in source.glyphs
        if (isinstance(glyph.role, Letter | Ligature) or glyph.name.text == carrier) and glyph.contours and not glyph.components
    )
    placed = source.replaced(*bases, *(_marked(glyph, stack, rule.wide * widths[0]) for glyph in source.by_role(Mark) if not glyph.components))
    models, spaces = {"top": placed.key(script.stem), "bottom": placed.key(script.kashida) if script.kashida is not None else ""}, frozenset(script.spaces)
    spaced = placed.replaced(
        *(
            msgspec.structs.replace(
                glyph,
                anchors=tuple(
                    _Anchor(name=anchor.name, x=glyph.advance / 2, y=anchor.y)
                    for side, model in models.items()
                    for held in placed.glyph(model).to_list()
                    for anchor in held.anchors
                    if anchor.name.removesuffix("wide") == side
                ),
            )
            for glyph in placed.glyphs
            if spaces.intersection(glyph.codes) and not glyph.components
        )
    )
    named = dict(spaced.named)
    propagated: dict[str, list[AnchorData]] = {}
    base_counts: dict[str, int] = {}
    for name in TopologicalSorter({name: tuple(part.name for part in glyph.components) for name, glyph in named.items()}).static_order():
        glyph = named[name]
        match glyph.components:
            case (base_part, mark_part) if isinstance(named[mark_part.name].role, Mark):
                base, mark = named[base_part.name], named[mark_part.name]
                base_map, mark_map = Transform(*base_part.transform), Transform(*mark_part.transform)
                base_scale, mark_scale = get_xy_rotation(base_map), get_xy_rotation(mark_map)
                anchors = {rename_anchor_for_scale(anchor.name, *base_scale): base_map.transformPoint((anchor.x, anchor.y)) for anchor in base.anchors}
                if alignment := next(
                    (
                        (point, mark_map.transformPoint((foot.x, foot.y)))
                        for foot in mark.anchors
                        if foot.name.startswith("_") and (point := anchors.get(rename_anchor_for_scale(foot.name, *mark_scale).removeprefix("_"))) is not None
                    ),
                    None,
                ):
                    (bx, by), (mx, my) = alignment
                    glyph = msgspec.structs.replace(glyph, components=(base_part, msgspec.structs.replace(mark_part, transform=Offset(bx - mx, by - my).transform(mark_part.transform))))
        propagate = partial(
            anchors_traversing_components,
            glyph_name=name,
            components=[Component(part.name, part.transform) for part in glyph.components],
            is_mark=isinstance(glyph.role, Mark),
            is_ligature=isinstance(glyph.role, Ligature),
            done_anchors=propagated,
            base_glyph_counts=base_counts,
        )
        inherited = propagate(existing_anchors=glyph.anchors)
        if glyph.components and not inherited:
            drawn = glyph.decomposed(named)
            own = (
                _marked(drawn, stack, rule.wide * widths[0]).anchors
                if isinstance(glyph.role, Mark)
                else _based(drawn, above, below, (rule.wide * widths[0], widths[1]), stem).anchors
                if isinstance(glyph.role, Letter | Ligature) or name == carrier
                else ()
            )
            inherited = propagate(existing_anchors=own) if own else inherited
        propagated[name] = inherited
        named[name] = msgspec.structs.replace(glyph, anchors=tuple(_Anchor(name=anchor.name, x=anchor.x, y=anchor.y) for anchor in propagated[name]))
    done = msgspec.structs.replace(spaced, glyphs=tuple(named[glyph.name.text] for glyph in spaced.glyphs))
    alternates, calt = _cleared(done, rule)
    return msgspec.structs.replace(done.replaced(*alternates), lookups=(*done.lookups, *calt, *_lifts(done, rule)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anchors"]
