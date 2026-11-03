"""Ornament sets: template text removed, copies of one drawing merged, art keeps its Private Use code point or takes the next from the first at even bearings, overlapping tiles keep theirs, and the word space is set."""

lazy from functools import partial, reduce
lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Offset

from typography.faces.declaration import Em, Ratio
from typography.rules.measures import Rule
lazy from typography.faces.glyphs import Component, Glyph, Ornament, Source, Space
lazy from typography.faces.names import GlyphName
lazy from typography.outlines.paths import bounds, Box, iou, transformed
lazy from typography.rules.measures import centred, middle
lazy from typography.shaping.runs import Binary, encoded as mapped

# --- [MODELS] ---------------------------------------------------------------------------


class Ornaments(Rule, frozen=True, kw_only=True):
    """Ornament art on Private Use code points from the first: glyphs the original maps at text code points outside the Private Use Area removed, copies of one drawing at `same` overlap merged, bearings evened except on tiles with ink crossing their advance, and word spaces set to `space`."""

    first: int
    space: Em = 0.217
    same: Ratio = 0.998


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FOLDING]


def _aligned(glyph: Glyph, /) -> tuple[Box, Glyph]:
    box = bounds(glyph.contours) or Box(xmin=0, ymin=0, xmax=0, ymax=0)
    return (box, msgspec.structs.replace(glyph, contours=transformed(glyph.contours, Offset(-box.xmin, -box.ymin))))


def _folded(same: float, groups: tuple[tuple[Glyph, ...], ...], glyph: Glyph, /) -> tuple[tuple[Glyph, ...], ...]:
    """Adds a glyph to the first group with a head drawing it repeats at any placement, the two inks overlapping at least `same` of their union, or opens a group."""
    box, shape = _aligned(glyph)
    match next(
        (
            index
            for index, group in enumerate(groups)
            for head, model in (_aligned(group[0]),)
            if abs(head.width - box.width) <= 1 and abs(head.height - box.height) <= 1 and iou(model.contours, shape.contours) >= same
        ),
        None,
    ):
        case None:
            return (*groups, (glyph,))
        case index:
            return (*groups[:index], (*groups[index], glyph), *groups[index + 1 :])


def _even(group: tuple[Glyph, ...], spare: float, /) -> float:
    """Advance of the group's slot with ink inside it and the most even bearings, else the ink width with the art's median bearing a side."""
    slots = tuple((abs(box.xmin - (glyph.advance - box.xmax)), glyph.advance) for glyph in group if (box := bounds(glyph.contours)) and box.xmin >= 0 and glyph.advance >= box.xmax)
    return min(slots)[1] if slots else ((box.width if (box := bounds(group[0].contours)) else 0.0) + 2 * spare)


# --- [ENCODING]


def encoded(source: Source, rule: Ornaments, /, *, original: Binary) -> Source:
    """Keeps the art of an ornament face on its own Private Use code points or the next ones from the first, tiles at their drawn advance and the rest at even bearings, with the word space set."""
    codes = mapped(original)
    spaces = tuple(sorted({*(code for code in codes if unicodedata.category(chr(code)) == "Zs"), ord("\N{NO-BREAK SPACE}")}))
    held = {name: code for code, name in sorted(codes.items(), reverse=True) if unicodedata.category(chr(code)) == "Co"}
    text = frozenset(name for code, name in codes.items() if unicodedata.category(chr(code)) not in {"Co", "Zs"})
    art = tuple(glyph for glyph in source.glyphs if isinstance(glyph.role, Ornament) and glyph.contours and glyph.advance > 1 and glyph.name.text not in text)
    initial: tuple[tuple[Glyph, ...], ...] = ()
    groups = reduce(partial(_folded, rule.same), art, initial)
    owned = {group[0].name.text: min(own) for group in groups if (own := tuple(held[glyph.name.text] for glyph in group if glyph.name.text in held))}
    fresh = (code for code in range(rule.first, 0x110000) if code not in owned.values())
    points = {group[0].name.text: owned.get(group[0].name.text) or next(fresh) for group in groups}
    spare = middle(tuple(side for glyph in art if (box := bounds(glyph.contours)) for side in (box.xmin, glyph.advance - box.xmax) if side > 0))
    kept = tuple(
        Glyph(name=GlyphName(codes=(points[head.name.text],)), role=Ornament(), advance=advance, contours=head.contours if tile else centred(head.contours, advance))
        for group in groups
        for head in (group[0],)
        for box in (bounds(head.contours),)
        for tile in (bool(box and (box.xmin < 0 or box.xmax > head.advance)),)
        for advance in ((head.advance if tile else _even(group, spare)),)
    )
    blanks = {code: glyph for glyph in source.glyphs for code in spaces if glyph.name.codes == (code,)}
    words = tuple(
        msgspec.structs.replace(blanks[code], advance=rule.space * source.upm)
        if code in blanks
        else Glyph(name=GlyphName(codes=(code,)), role=Space(), advance=rule.space * source.upm, components=(Component(name=next(iter(blanks.values())).name.text),))
        for code in spaces
        if blanks
    )
    worded = frozenset((code,) for code in spaces)
    rest = tuple(glyph for glyph in source.glyphs if not isinstance(glyph.role, Ornament) and glyph.name.codes not in worded)
    return msgspec.structs.replace(source, glyphs=(*rest, *words, *kept), metrics=msgspec.structs.replace(source.metrics, x_height=0, cap_height=0) if source.metrics else None, lookups=())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ornaments", "encoded"]
