"""Ornament sets: template text removed, copies of one drawing merged, art keeps its Private Use code point or takes the next from the first at even bearings, overlapping tiles keep theirs, and the word space is set."""

lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Offset

from rasm_typography._faces.declaration import Em, Ratio
from rasm_typography._rules import measures
lazy from rasm_records import Ok, Outcome
lazy from rasm_typography._faces.glyphs import Glyph, Ornament, Point, Source
lazy from rasm_typography._faces.names import GlyphName
lazy from rasm_typography._outlines import paths

# --- [MODELS] ---------------------------------------------------------------------------


class Ornaments(measures.Rule, frozen=True, kw_only=True):
    """Ornament art on Private Use code points from the first: glyphs the original maps at text code points outside the Private Use Area removed, copies of one drawing at `same` overlap merged, bearings evened except on tiles with outlines crossing their advance, and word spaces set to `space`."""

    first: int
    space: Em = 0.217
    same: Ratio = 0.998

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Keeps the art of an ornament face on its own Private Use code points or the next ones from the first, tiles at their drawn advance and the rest at even bearings, with the word space set."""

        def even(group: list[tuple[Glyph, paths.Box, tuple[tuple[Point, ...], ...]]], spare: float, /) -> float:
            slots = tuple((abs(box.xmin - (glyph.advance - box.xmax)), glyph.advance) for glyph, box, _ in group if box.xmin >= 0 and glyph.advance >= box.xmax)
            return min(slots)[1] if slots else group[0][1].width + 2 * spare

        codes = source.encoded
        held = frozenset(name for code, name in codes.items() if unicodedata.category(chr(code)) == "Co")
        text = frozenset(name for code, name in codes.items() if unicodedata.category(chr(code)) not in {"Co", "Zs"}) - held
        art = tuple(
            (glyph, box, paths.transformed(glyph.contours, Offset(-box.xmin, -box.ymin)))
            for own in source.by_role(Ornament)
            for glyph in (source.drawn(own.name.text),)
            if glyph.advance > 1 and glyph.name.text not in text and (box := paths.bounds(glyph.contours))
        )
        groups: list[list[tuple[Glyph, paths.Box, tuple[tuple[Point, ...], ...]]]] = []
        for prepared in art:
            box, shape = prepared[1:]
            match next(
                (group for group in groups for _, head, model in group[:1] if abs(head.width - box.width) <= 1 and abs(head.height - box.height) <= 1 and paths.iou(model, shape) >= self.same), None
            ):
                case None:
                    groups.append([prepared])
                case group:
                    group.append(prepared)
        heads = tuple((group[0], group) for group in groups)
        owned = {head.name.text: own for (head, _, _), group in heads if (own := tuple(sorted({code for glyph, _, _ in group for code in glyph.codes if unicodedata.category(chr(code)) == "Co"})))}
        reserved = frozenset(*values for values in owned.values())
        fresh = (code for code in range(self.first, 0x110000) if code not in reserved and unicodedata.category(chr(code)) == "Co")
        points = {head.name.text: owned.get(head.name.text) or (next(fresh),) for (head, _, _), _ in heads}
        spare = measures.middle(tuple(side for glyph, box, _ in art for side in (box.xmin, glyph.advance - box.xmax) if side > 0))
        kept = tuple(
            Glyph(
                name=GlyphName(stem=(points[head.name.text][0],)),
                role=Ornament(),
                advance=advance,
                codes=frozenset(points[head.name.text]),
                contours=head.contours if tile else paths.transformed(head.contours, box.centered(advance)),
            )
            for (head, box, _), group in heads
            for tile in (box.xmin < 0 or box.xmax > head.advance,)
            for advance in ((head.advance if tile else even(group, spare)),)
        )
        spaces = {glyph.name.text: values for glyph in source.glyphs for values in (frozenset(code for code in glyph.codes if unicodedata.category(chr(code)) == "Zs"),) if values}
        primary = min(spaces, key=lambda name: min(spaces[name])) if spaces else None
        nonbreaking = ord("\N{NO-BREAK SPACE}")
        words = tuple(
            msgspec.structs.replace(source.named[name], advance=self.space * source.upm, codes=values | {nonbreaking} if name == primary and nonbreaking not in codes else values)
            for name, values in spaces.items()
        )
        rest = tuple(glyph for glyph in source.glyphs if not isinstance(glyph.role, Ornament) and glyph.name.text not in spaces)
        return Ok(msgspec.structs.replace(source, glyphs=(*rest, *words, *kept), metrics=msgspec.structs.replace(source.metrics, x_height=0, cap_height=0), lookups=()))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ornaments"]
