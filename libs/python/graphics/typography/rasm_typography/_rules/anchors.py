"""Anchors from outlines: mark anchors at the original's clearance in a vowel and a wide class, ligature component anchors and carets, composite marks attached, `calt` dot groups moved clear of a joined neighbor, and `mark` lifts of marks clear of neighbors."""

lazy from functools import partial
lazy from graphlib import TopologicalSorter
lazy from itertools import groupby, pairwise
lazy import math

import msgspec
lazy from expression import Result
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.misc.transform import Offset, Transform
lazy from ufo2ft.filters.propagateAnchors import AnchorData, anchors_traversing_components, get_xy_rotation, rename_anchor_for_scale
lazy from ufoLib2.objects import Component

from rasm_typography._faces.declaration import Band, Em, Ratio, Script
from rasm_typography._rules import measures
lazy from rasm_records import Error, Ok, Outcome
lazy from rasm_typography._faces.glyphs import Anchor, Glyph, Letter, Ligature, Mark, Point, Side, Source
lazy from rasm_typography._faces.layout import Adjustment, Chain, Contextual, Lifts
lazy from rasm_typography._outlines import measure, paths
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Anchors(measures.Rule, frozen=True, kw_only=True):
    """Anchors from outlines at the original's vowel clearance, the vowel class over a column `wide` vowels across and wider marks over the widest, spaces anchored like the stem and joining glyph, dots held `dots` off a joined neighbor, and lifts up to `lifts` (non-joining, joined)."""

    script: Script
    stack: Em
    clearance: Em
    lifts: Band = (0.1, 0.46)
    dots: Em = 0.015
    wide: Ratio = 1.15

    def applied(self, source: Source, /, *, original: Binary, corpus: Corpus, **_: object) -> Outcome[Source]:
        """Sets every base, ligature, and mark anchor from outlines at the original's median mark clearance in each corpus letter form, attaches composite marks, and adds the `calt` dot alternates and neighbor lifts.

        Returns:
            Ok(anchored source with its alternates and lifts), `ABSENT` for a face without a drawn stem.
        """
        script, stack, marked = self.script, self.stack * source.upm, original.marks
        match measures.placement(source, original, script.stem):
            case Result(tag="ok", ok=(_, stem, matrix)):
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
                letters, reach, step = tuple(drawn for glyph in done.by_role(Letter) if (drawn := done.drawn(glyph.name.text)).contours), self.dots * done.upm, self.stack * done.upm / 2
                boxes, nuqta = {glyph.name.text: box for glyph in letters if (box := paths.bounds(glyph.contours))}, paths.area(measures.nuqta(done, script.dotted))
                groups = {glyph.name.text: (body, group, paths.bounds(group)) for glyph in letters for body, dots in (measures.dotted(glyph, nuqta),) if dots for group in (paths.union(*dots),)}
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
                    and (move := otRound(math.copysign(min(math.ceil(short / step) * step if step else short, max(glyph.advance - box.xmax if after else box.xmin, 0.0)), 1 if after else -1)))
                )
                shifts = {
                    (name, move): msgspec.structs.replace(held, suffixes=(*held.suffixes, suffix))
                    for index, (name, move) in enumerate(dict.fromkeys((name, move) for name, _, move, _ in moves), 1)
                    for held, suffix in ((done.named[name].name, f"calt{index}"),)
                }
                alternates = tuple(
                    msgspec.structs.replace(done.drawn(name), name=alias, codes=frozenset(), contours=(*groups[name][0], *paths.transformed(groups[name][1], Offset(move, 0))), components=())
                    for (name, move), alias in shifts.items()
                )
                dotted = tuple(
                    Chain(input=(((name,), shifts[name, move].text),), lookahead=(others,)) if after else Chain(backtrack=(others,), input=(((name,), shifts[name, move].text),))
                    for (name, after, move), group in groupby(moves, key=lambda row: row[:3])
                    for others in (tuple(other for *_, other in group),)
                )
                vowel = paths.union(
                    *(
                        paths.transformed(glyph.contours, Offset(-anchor.x, -anchor.y))
                        for raw in done.glyph(done.key(script.vowels[0])).to_list()
                        for glyph in (done.drawn(raw.name.text),)
                        for anchor in glyph.anchor("_top").to_list()
                    )
                )
                tops = {
                    glyph.name.text: (outline, box)
                    for glyph in letters
                    for top in glyph.anchor("top").to_list()
                    for outline in (paths.transformed(vowel, Offset(top.x, top.y)),)
                    if (box := paths.bounds(outline))
                }
                lifted = sorted(
                    (owner.name.text, after, otRound(min(math.ceil(short / step) * step if step else short, self.lifts[first.name.form.left] * done.upm)), other.name.text)
                    for first in letters
                    for second in letters
                    if first.name.form.left == second.name.form.right
                    for owner, other, after, shift in ((first, second, True, -second.advance), (second, first, False, second.advance))
                    if owner.name.text in tops
                    and _near(tops[owner.name.text][1], boxes[other.name.text], shift, self.clearance * done.upm)
                    and (short := _short(tops[owner.name.text][0], other.contours, shift, self.clearance * done.upm))
                )
                raised = tuple(glyph.name.text for glyph in done.by_role(Mark) if glyph.role == Mark(side=Side.ABOVE))
                lifts = tuple(
                    Chain(input=(((name,), None), (raised, Adjustment(dy=lift))), lookahead=(others,)) if after else Chain(backtrack=(others,), input=(((name,), None), (raised, Adjustment(dy=lift))))
                    for (name, after, lift), group in groupby(lifted, key=lambda row: row[:3])
                    for others in (tuple(other for *_, other in group),)
                )
                return Ok(
                    msgspec.structs.replace(
                        done.replaced(*alternates),
                        lookups=(
                            *done.lookups,
                            *((Contextual(features=frozendict(calt=frozendict()), order=max((lookup.order for lookup in done.lookups), default=0) + 1, rules=dotted),) if dotted else ()),
                            *((Lifts(features=frozendict(mark=frozendict()), rules=lifts),) if lifts and raised else ()),
                        ),
                    )
                )
            case Result(error=faults):
                return Error(faults)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ANCHORS]


def _anchors(glyph: Glyph, x: float, above: float, below: float, widths: tuple[float, float], component: int | None = None, /) -> tuple[Anchor, ...]:
    """Top and bottom anchors of the vowel and wide classes over and under the outline column of each width at x, named `_<component>` on a ligature component."""
    return tuple(
        Anchor(name=f"{side}{tier}" if component is None else f"{side}{tier}_{component}", x=x, y=y)
        for tier, width in zip(("", "wide"), widths, strict=False)
        for column in (paths.bounds(paths.strip(glyph.contours, x - width / 2, x + width / 2)) or paths.bounds(glyph.contours),)
        if column
        for side, y in (("top", column.ymax + above), ("bottom", column.ymin - below))
    )


def _based(glyph: Glyph, above: float, below: float, widths: tuple[float, float], stem: paths.Box, /) -> Glyph:
    """Anchors a base's marks over its own outline, a ligature's over each component arm read above 0.8 of the stem height with a caret between arms, or at equal shares of its advance where arms and components differ in count."""
    match paths.bounds(glyph.contours), glyph.role:
        case None, _:
            return glyph
        case box, Ligature(components=components):
            threshold = stem.ymin + 0.8 * stem.height
            arms = (
                tuple(sorted((arm for piece in paths.pieces(paths.strip(glyph.contours, threshold, box.ymax, axis="y")) if (arm := paths.bounds(piece))), key=lambda arm: -arm.xmax))[: len(components)]
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


def _near(own: paths.Box, other: paths.Box, shift: float, reach: float, /) -> bool:
    """Whether another box moved by a shift stands within a reach of a box."""
    return (own.grown(reach) & other.shifted(shift, 0)) is not None


def _short(own: tuple[tuple[Point, ...], ...], other: tuple[tuple[Point, ...], ...], shift: float, reach: float, /) -> float:
    """How far an outline stands inside the reach of another outline moved by a shift."""
    return max(reach - measure.gap(own, paths.transformed(other, Offset(shift, 0))), 0.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anchors"]
