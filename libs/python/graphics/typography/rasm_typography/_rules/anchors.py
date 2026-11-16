"""Anchors from outlines: mark anchors at the original's clearance in a vowel and a wide class, ligature component anchors and carets, composite marks attached, `calt` dot and hamza groups moved clear of a joined neighbor, and `mark` lifts and drops of marks clear of neighbors."""

from typing import Annotated
lazy from bisect import bisect_left
lazy from collections.abc import Hashable, Iterable, Mapping
lazy from functools import partial
lazy from graphlib import TopologicalSorter
lazy from itertools import groupby, pairwise
lazy import math

import msgspec
lazy from expression import Error, Ok
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.misc.transform import Offset, Transform
lazy from fontTools.otlLib.builder import LOOKUP_FLAG_IGNORE_MARKS
lazy from scipy.optimize import root_scalar
lazy from ufo2ft.filters.propagateAnchors import AnchorData, anchors_traversing_components, get_xy_rotation, rename_anchor_for_scale
lazy from ufoLib2.objects import Component

from rasm_typography._faces.declaration import Band, Code, Em, Ratio, Script
from rasm_typography._rules import measures
lazy from rasm_records import Outcome
lazy from rasm_typography._faces.glyphs import Anchor, Glyph, Letter, Ligature, Mark, Point, Side, Source
lazy from rasm_typography._faces.layout import Adjustment, Chain, Contextual, Lifts, Ligatures, Single
lazy from rasm_typography._outlines import measure, paths
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Anchors(measures.Rule, frozen=True, kw_only=True):
    """Anchors at the original's vowel clearance over a column `wide` vowels across, wider marks over the widest, spaces as the stem, the `dot` letter's dots `dots` off a joined neighbor, stacks of up to `depth` marks moved by their first mark up to `lifts` (unjoined, joined) or slid clear."""

    script: Script
    stack: Em
    clearance: Em
    lifts: Band = (0.1, 0.46)
    dots: Em = 0.015
    wide: Ratio = 1.15
    depth: Annotated[int, msgspec.Meta(gt=0)] = 2
    dot: Code | None = None

    def applied(self, source: Source, /, *, original: Binary, corpus: Corpus, **_: object) -> Outcome[Source]:
        """Sets every base, ligature, and mark anchor from outlines at the original's median mark clearance in each corpus letter form, attaches composite marks, and adds the `calt` dot alternates and the lifts and drops off a neighbor, apart for a neighbor carrying marks of the side.

        Returns:
            Ok(anchored source with its alternates and lifts), `ABSENT` for a face without a drawn stem.
        """
        script, stack, marked = self.script, self.stack * source.upm, original.marks
        match measures.placement(source, original, script.stem):
            case Ok((_, stem, matrix)):
                above, below = (
                    matrix.xx
                    * measures.middle(
                        tuple(
                            distance
                            for letter in corpus.letters
                            for form in corpus.forms(letter, chr(vowel))
                            for language in corpus.languages
                            for run in (original.shaped(form, language=language),)
                            for outlines in (tuple(zip(run.placed, original.drawn(run), strict=False)),)
                            if (base := paths.union(*(contours for placed, contours in outlines if placed.glyph not in marked)))
                            and (mark := paths.union(*(contours for placed, contours in outlines if placed.glyph in marked)))
                            and (distance := measure.gap(base, mark)) > 0
                        )
                    )
                    for vowel in script.vowels
                )
                marks = tuple((glyph, paths.Box.of(rect)) for glyph in source.by_role(Mark) if (rect := source.bounds(glyph.name.text)) is not None)
                widths = tuple(max((box.width for glyph, box in marks if not codes or glyph.name.codes[:1] in codes), default=stem.width) for codes in ({(code,) for code in script.recipes}, set()))
                carrier = source.key(script.carrier) if script.carrier is not None else None
                bases = tuple(
                    _based(glyph, above, below, (self.wide * widths[0], widths[1]), stem)
                    for glyph in source.glyphs
                    if (isinstance(glyph.role, Letter | Ligature) or glyph.name.text == carrier) and glyph.contours and not glyph.components
                )
                placed = source.replaced(*bases, *(_marked(glyph, stack, self.wide * widths[0]) for glyph in source.by_role(Mark) if not glyph.components))
                models, spaces = {"top": placed.key(script.stem), **({"bottom": placed.key(script.kashida)} if script.kashida is not None else {})}, frozenset(script.spaces)
                spaced = placed.replaced(
                    *(
                        msgspec.structs.replace(
                            glyph,
                            anchors=tuple(
                                Anchor(name=anchor.name, x=glyph.advance / 2, y=anchor.y)
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
                        case (base_part, mark_part) if isinstance(named[mark_part.name].role, Mark) and (
                            alignment := next(
                                (
                                    (point, mark_map.transformPoint((foot.x, foot.y)))
                                    for base_map, mark_map in ((Transform(*base_part.transform), Transform(*mark_part.transform)),)
                                    for base_scale, mark_scale in ((get_xy_rotation(base_map), get_xy_rotation(mark_map)),)
                                    for anchors in ({rename_anchor_for_scale(anchor.name, *base_scale): base_map.transformPoint((anchor.x, anchor.y)) for anchor in named[base_part.name].anchors},)
                                    for foot in named[mark_part.name].anchors
                                    if foot.name.startswith("_") and (point := anchors.get(rename_anchor_for_scale(foot.name, *mark_scale).removeprefix("_"))) is not None
                                ),
                                None,
                            )
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
                    own = (
                        (
                            _marked(glyph.decomposed(named), stack, self.wide * widths[0]).anchors
                            if isinstance(glyph.role, Mark)
                            else _based(glyph.decomposed(named), above, below, (self.wide * widths[0], widths[1]), stem).anchors
                            if isinstance(glyph.role, Letter | Ligature) or name == carrier
                            else ()
                        )
                        if glyph.components and not inherited
                        else ()
                    )
                    propagated[name] = propagate(existing_anchors=own) if own else inherited
                    named[name] = msgspec.structs.replace(glyph, anchors=tuple(Anchor(name=anchor.name, x=anchor.x, y=anchor.y) for anchor in propagated[name]))
                done = msgspec.structs.replace(spaced, glyphs=tuple(named[glyph.name.text] for glyph in spaced.glyphs))
                letters, ligatures = (tuple(drawn for glyph in done.by_role(role) if (drawn := done.drawn(glyph.name.text)).contours) for role in (Letter, Ligature))
                reach, step = self.dots * done.upm, self.stack * done.upm / 2
                boxes, nuqta = {glyph.name.text: box for glyph in (*letters, *ligatures) if (box := paths.bounds(glyph.contours))}, paths.area(measures.nuqta(done, self.dot))
                groups = {
                    glyph.name.text: (body, parts)
                    for glyph in letters
                    for body, dots in (measures.dotted(glyph, nuqta),)
                    for own in (paths.bounds(body),)
                    if own and dots
                    for parts in (
                        tuple(
                            (group, box)
                            for above in (True, False)
                            for group in (paths.union(*(dot for dot in dots for extent in (paths.bounds(dot),) if extent and (extent.middle[1] > own.middle[1]) is above)),)
                            for box in (paths.bounds(group),)
                            if box
                        ),
                    )
                    if parts
                }
                moves = sorted(
                    (glyph.name.text, after, motion, other.name.text)
                    for glyph in letters
                    if glyph.name.text in groups
                    for body, parts in (groups[glyph.name.text],)
                    for other in letters
                    for after, shift in ((True, -other.advance), (False, glyph.advance))
                    if (glyph.name.form.left and other.name.form.right if after else other.name.form.left and glyph.name.form.right)
                    for near, loose in ((groups[other.name.text][0], tuple(*group for group, _ in groups[other.name.text][1])) if other.name.text in groups else (other.contours, ()),)
                    for motion in (
                        tuple(
                            (dx, _cleared(body, group, (*near, *loose), shift, reach, dx))
                            for group, box in parts
                            for short in ((max(_short(group, near, shift, reach), _short(group, loose, shift, reach) / 2) if _near(box, boxes[other.name.text], shift, reach) else 0.0),)
                            for dx in (
                                (
                                    otRound(math.copysign(min(math.ceil(short / step) * step if step else short, max(glyph.advance - box.xmax if after else box.xmin, 0.0)), 1 if after else -1))
                                    if short
                                    else 0
                                ),
                            )
                        ),
                    )
                    if any(any(move) for move in motion)
                )
                shifts = {
                    (name, motion): msgspec.structs.replace(held, suffixes=(*held.suffixes, suffix))
                    for index, (name, motion) in enumerate(dict.fromkeys((name, motion) for name, _, motion, _ in moves), 1)
                    for held, suffix in ((done.named[name].name, f"calt{index}"),)
                }
                variants = {name: (name, *(alias.text for (owner, _), alias in shifts.items() if owner == name)) for name, _ in shifts}
                alternates = tuple(
                    msgspec.structs.replace(
                        done.drawn(name),
                        name=alias,
                        codes=frozenset(),
                        contours=(*groups[name][0], *(*paths.transformed(group, Offset(*move)) for (group, _), move in zip(groups[name][1], motion, strict=True))),
                        components=(),
                    )
                    for (name, motion), alias in shifts.items()
                )
                dotted = tuple(
                    Chain[tuple[Single | Ligatures, ...]](input=(((name,), (Single(features=frozendict(), mapping=frozendict({name: shifts[name, move].text})),)),), lookahead=(others,))
                    if after
                    else Chain[tuple[Single | Ligatures, ...]](backtrack=(others,), input=(((name,), (Single(features=frozendict(), mapping=frozendict({name: shifts[name, move].text})),)),))
                    for (name, after, move), group in groupby(moves, key=lambda row: row[:3])
                    for others in (tuple(*variants.get(other, (other,)) for *_, other in group),)
                )
                reach, held = self.clearance * done.upm, step or 1.0
                lifts = tuple(
                    Lifts(
                        features=frozendict(mark=frozendict()),
                        mark_filtering=frozenset(marks),
                        rules=tuple(
                            rule
                            for owners, others in (
                                (
                                    _classes(((name, (after, carrying, lift, other)) for name, after, carrying, lift, other in shifted), variants),
                                    _classes(((other, (name, after, carrying, lift)) for name, after, carrying, lift, other in shifted), variants),
                                ),
                            )
                            for owned, after, carrying, lift, other in sorted(
                                dict.fromkeys((owners[name], after, carrying, lift, others[other]) for name, after, carrying, lift, other in shifted), key=lambda row: not row[2]
                            )
                            for count in range(self.depth, 0, -1)
                            for moved in (((owned, None), (marks, Adjustment(dx=lift[0], dy=lift[1])), *((marks, None),) * (count - 1)),)
                            for rule in (
                                (Chain(input=moved, lookahead=(other, *(marks,) * carrying)),)
                                if after
                                else tuple(Chain(backtrack=(other, *(marks,) * before), input=moved) for before in range(int(carrying), self.depth + 1))
                            )
                        ),
                    )
                    for side, sign in ((Side.ABOVE, 1.0), (Side.BELOW, -1.0))
                    for marks in (tuple(glyph.name.text for glyph in done.by_role(Mark) if glyph.role == Mark(side=side)),)
                    for vowels in (measures.vowels(done, script.recipes, side),)
                    for seated in ({glyph.name.text: (outline, box) for glyph in (*letters, *ligatures) for outline in (measures.seated(glyph, vowels),) for box in (paths.bounds(outline),) if box},)
                    for shifted in (
                        sorted(
                            (owner.name.text, after, carrying, lift, other.name.text)
                            for first in (*letters, *ligatures)
                            for second in (*letters, *ligatures)
                            if first.name.form.left == second.name.form.right
                            for owner, other, after, shift in ((first, second, True, -second.advance), (second, first, False, second.advance))
                            if owner.name.text in seated and _near(seated[owner.name.text][1], boxes[other.name.text], shift, reach)
                            for (outline, seat), own in ((seated[owner.name.text], boxes[owner.name.text]),)
                            for carrying, near in ((False, other.contours), *(((True, paths.union(other.contours, seated[other.name.text][0])),) if other.name.text in seated else ()))
                            for short in (_short(outline, near, shift, reach),)
                            if short
                            for lift in (
                                next(
                                    (
                                        (otRound(ux * count * held), otRound(uy * count * held))
                                        for (ux, uy), limit in (
                                            ((0.0, sign), self.lifts[first.name.form.left] * done.upm),
                                            ((1.0, 0.0), own.xmax - seat.middle[0]) if after else ((-1.0, 0.0), seat.middle[0] - own.xmin),
                                        )
                                        for counts in (range(max(1, math.ceil(short / held)), math.floor(limit / held) + 1),)
                                        for clears in (partial(_clears, outline, near, shift, reach, ux * held, uy * held),)
                                        if counts and clears(counts[-1])
                                        for count in (counts[bisect_left(counts, x=True, key=clears)],)
                                    ),
                                    None,
                                ),
                            )
                            if lift
                        ),
                    )
                    if shifted and marks
                )
                return Ok(
                    msgspec.structs.replace(
                        done.replaced(*alternates),
                        lookups=(
                            *done.lookups,
                            *(
                                (Contextual(features=frozendict(calt=frozendict()), order=max((lookup.order for lookup in done.lookups), default=0) + 1, flags=LOOKUP_FLAG_IGNORE_MARKS, rules=dotted),)
                                if dotted
                                else ()
                            ),
                            *lifts,
                        ),
                    )
                )
            case Error(faults):
                return Error(faults)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ANCHORS]


def _anchors(glyph: Glyph, x: float, above: float, below: float, widths: tuple[float, float], component: int | None = None, /) -> tuple[Anchor, ...]:
    """Top and bottom anchors of the vowel and wide classes over the outline column of each width at x and under it or the baseline, named `_<component>` on a ligature component."""
    return tuple(
        Anchor(name=f"{side}{tier}" if component is None else f"{side}{tier}_{component}", x=x, y=y)
        for tier, width in zip(("", "wide"), widths, strict=False)
        for column in (paths.bounds(paths.strip(glyph.contours, x - width / 2, x + width / 2)) or paths.bounds(glyph.contours),)
        if column
        for side, y in (("top", column.ymax + above), ("bottom", min(column.ymin, 0.0) - below))
    )


def _based(glyph: Glyph, above: float, below: float, widths: tuple[float, float], stem: paths.Box, /) -> Glyph:
    """Anchors a base's marks over its own outline, a ligature's over each component arm read above 0.8 of the stem height, fills within half a stem joined, with a caret between arms, or at equal shares of its advance where arms and components differ in count."""
    match paths.bounds(glyph.contours), glyph.role:
        case None, _:
            return glyph
        case box, Ligature(components=components):
            threshold = stem.ymin + 0.8 * stem.height
            arms = (
                tuple(
                    sorted(
                        (
                            arm.grown(-stem.width / 2)
                            for piece in paths.pieces(paths.offset(paths.strip(glyph.contours, threshold, box.ymax, axis="y"), stem.width / 2))
                            if (arm := paths.bounds(piece))
                        ),
                        key=lambda arm: -arm.xmax,
                    )
                )[: len(components)]
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
                anchors=(*(*_anchors(glyph, x, above, below, widths, index) for index, x in enumerate(middles, 1)), *(Anchor(name=f"caret_{index}", x=x, y=0.0) for index, x in enumerate(carets, 1))),
            )
        case box, _:
            x = next((anchor.x for anchor in glyph.anchors if anchor.name == "top"), box.middle[0])
            return msgspec.structs.replace(glyph, anchors=_anchors(glyph, min(max(x, box.xmin, 0.0), box.xmax, glyph.advance), above, below, widths))


def _marked(glyph: Glyph, stack: float, wide: float, /) -> Glyph:
    """Anchors a mark at its outline foot, in the wide class past the wide width, and marks stacked on it the stack gap past its head in both classes."""
    match paths.bounds(glyph.contours), glyph.role:
        case paths.Box(middle=(x, _), ymin=ymin, ymax=ymax, width=width), Mark(side=side):
            name, (foot, head) = side.anchor, (ymin, ymax + stack) if side is Side.ABOVE else (ymax, ymin - stack)
            return msgspec.structs.replace(glyph, anchors=(Anchor(name=f"_{name}{'wide' if width > wide else ''}", x=x, y=foot), *(Anchor(name=f"{name}{tier}", x=x, y=head) for tier in ("", "wide"))))
        case _:
            return glyph


# --- [NEIGHBOURS]


def _classes(rows: Iterable[tuple[str, Hashable]], variants: Mapping[str, tuple[str, ...]], /) -> dict[str, tuple[str, ...]]:
    """Glyphs each mapped to the class of glyphs holding the same rest of every row naming them, with their `calt` variants, so rules over the classes stay class-based."""
    signatures = {name: frozenset(rest for _, rest in group) for name, group in groupby(sorted(rows, key=lambda row: row[0]), key=lambda row: row[0])}
    shared = {signature: tuple(variant for name in sorted(signatures) if signatures[name] == signature for variant in variants.get(name, (name,))) for signature in set(signatures.values())}
    return {name: shared[signature] for name, signature in signatures.items()}


def _near(own: paths.Box, other: paths.Box, shift: float, reach: float, /) -> bool:
    """Whether another box moved by a shift stands within a reach of a box."""
    return (own.grown(reach) & other.shifted(shift, 0)) is not None


def _short(own: tuple[tuple[Point, ...], ...], other: tuple[tuple[Point, ...], ...], shift: float, reach: float, /) -> float:
    """How far an outline stands inside the reach of another outline moved by a shift."""
    return max(reach - measure.gap(own, paths.transformed(other, Offset(shift, 0))), 0.0)


def _clears(mark: tuple[tuple[Point, ...], ...], other: tuple[tuple[Point, ...], ...], shift: float, reach: float, dx: float, dy: float, count: int, /) -> bool:
    """Whether a mark moved `count` steps of `(dx, dy)` stands out of the reach of a neighbour moved by a shift, true from the first clearing move on once past an overhang."""
    return not _short(paths.transformed(mark, Offset(count * dx, count * dy)), other, shift, reach)


def _cleared(body: tuple[tuple[Point, ...], ...], group: tuple[tuple[Point, ...], ...], other: tuple[tuple[Point, ...], ...], shift: float, reach: float, dx: float, /) -> int:
    """Vertical move away from its own body, up to the group's height, that takes a dot group moved by `dx` out of the reach of a neighbour moved by a shift, else off it, zero where the horizontal move clears it or no such move does."""
    placed = paths.transformed(other, Offset(shift, 0))
    match paths.bounds(body), paths.bounds(group):
        case paths.Box() as own, paths.Box() as dots if (start := measure.gap(paths.transformed(group, Offset(dx, 0.0)), placed)) < reach:
            sign = 1.0 if dots.middle[1] > own.middle[1] else -1.0
            end = measure.gap(paths.transformed(group, Offset(dx, sign * dots.height)), placed)
            return next(
                (
                    otRound(
                        sign * math.ceil(root_scalar(lambda dy, target: measure.gap(paths.transformed(group, Offset(dx, sign * dy)), placed) - target, args=(target,), bracket=(0.0, dots.height)).root)
                    )
                    for target in (reach, 0.0)
                    if start < target <= end
                ),
                0,
            )
        case _:
            return 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anchors"]
