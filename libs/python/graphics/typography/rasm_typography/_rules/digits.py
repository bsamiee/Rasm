"""Digits: a second script's digits sharing the first's drawings, one digit pen, a held zero, a default advance kind, and both `pnum` and `tnum`."""

lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Transform

from rasm_typography._faces.declaration import Advance, Em, Ratio, Script
from rasm_typography._rules import measures
lazy from rasm_records import Ok, Outcome
lazy from rasm_typography._faces.glyphs import Digit, Source
lazy from rasm_typography._faces.layout import Lookups, Single
lazy from rasm_typography._outlines import measure, paths

# --- [MODELS] ---------------------------------------------------------------------------


class Digits(measures.Rule, frozen=True, kw_only=True):
    """Digits of the script's lead and second digit scripts sharing drawings for the shared decimal values, on one pen, the zero width held to a share of the stem height, and the default advance kind."""

    script: Script
    zero: Ratio | None
    advance: Advance
    bearing: Em | None
    tolerance: Ratio = 0.08

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Draws the digits of two scripts on one pen and sets their default advances, `pnum` and `tnum` each reaching its kind from either."""
        (lead, second), shared = self.script.digits, self.script.shared
        own = {
            drawn.name.text: drawn
            for glyph in source.glyphs
            if glyph.codes and isinstance(glyph.role, Digit) and glyph.role.zero in {lead, second}
            for drawn in (source.drawn(glyph.name.text),)
            if drawn.contours
        }
        primary = {unicodedata.decimal(chr(glyph.name.codes[0])): name for name, glyph in own.items() if glyph.role == Digit(zero=lead)}
        models = {
            name: primary[value] if glyph.role == Digit(zero=second) and (value := unicodedata.decimal(chr(glyph.name.codes[0]))) in shared and value in primary else name
            for name, glyph in own.items()
        }
        drawn = {model: own[model].contours for model in dict.fromkeys(models.values())}
        strokes = {code: measure.stroke(contours) for code, contours in drawn.items()}
        pen = measures.middle(tuple(strokes.values()))
        penned = {code: measure.restroked(contours, pen) if pen and abs(strokes[code] / pen - 1) > self.tolerance else contours for code, contours in drawn.items()}
        stem = paths.Box.of(rect) if (rect := source.bounds(source.key(self.script.stem))) is not None else None
        zero = paths.bounds(penned[primary[0]]) if 0 in primary else None
        scaled = (
            {primary[0]: Transform().translate(*zero.middle).scale(self.zero * stem.height / zero.width).translate(-zero.middle[0], -zero.middle[1])} if zero and stem and self.zero is not None else {}
        )
        penned |= {name: paths.transformed(penned[name], matrix) for name, matrix in scaled.items()}
        if not (boxes := {model: box for model, contours in penned.items() if (box := paths.bounds(contours))}):
            return Ok(source)
        names = tuple(name for name in own if models[name] in boxes)
        broadest = min(boxes, key=lambda name: (-boxes[name].width, own[name].name.codes))
        sides = tuple(side for name in names for box in (boxes[models[name]],) for side in (box.xmin, own[name].advance - box.xmax) if side >= 0)
        even = (own[broadest].advance - boxes[broadest].width) / 2
        bearing = measures.middle(sides)
        margin = self.bearing * source.upm if self.bearing is not None else max(bearing, even)
        widths = {name: {Advance.TABULAR: boxes[broadest].width + 2 * (even if even >= 0 else bearing), Advance.PROPORTIONAL: boxes[models[name]].width + 2 * margin} for name in names}
        other = self.advance.other
        aliases = {name: msgspec.structs.replace(own[name].name, suffixes=(*own[name].name.suffixes, other.feature)) for name in names}
        glyphs = tuple(
            msgspec.structs.replace(
                glyph,
                name=alias,
                codes=codes,
                advance=widths[name][kind],
                contours=paths.transformed(penned[model], shift),
                components=(),
                anchors=tuple(msgspec.structs.replace(anchor, x=x, y=y) for anchor in own[model].anchors for x, y in (matrix.transformPoint((anchor.x, anchor.y)),)),
            )
            for name in names
            for glyph, model in ((own[name], models[name]),)
            for alias, kind, codes in ((glyph.name, self.advance, glyph.codes), (aliases[name], other, frozenset()))
            for shift in (boxes[model].centered(widths[name][kind]),)
            for matrix in (shift.transform(scaled[model]) if model in scaled else shift,)
        )
        order = max((lookup.order for lookup in source.lookups), default=0) + 1
        mapping = {name: alias.text for name, alias in aliases.items()}
        lookups = (
            Single(features=frozendict({other.feature: frozendict()}), order=order, mapping=frozendict(mapping)),
            Single(features=frozendict({self.advance.feature: frozendict()}), order=order + 1, mapping=frozendict({held: default for default, held in mapping.items()})),
        )
        affected = frozenset((*aliases, *(alias.text for alias in aliases.values())))
        features = frozenset(*lookup.features for lookup in lookups)

        def retained(lookup: Lookups, /) -> tuple[Lookups, ...]:
            match lookup:
                case Single(mapping=values) if features & lookup.features.keys():
                    untouched = frozendict({tag: systems for tag, systems in lookup.features.items() if tag not in features})
                    selected = frozendict({tag: systems for tag, systems in lookup.features.items() if tag in features})
                    remaining = frozendict({name: target for name, target in values.items() if name not in affected})
                    return (
                        *((msgspec.structs.replace(lookup, features=untouched),) if untouched else ()),
                        *((msgspec.structs.replace(lookup, features=selected, mapping=remaining),) if remaining else ()),
                    )
                case _:
                    return (lookup,)

        return Ok(msgspec.structs.replace(source.replaced(*glyphs), lookups=(*(*retained(lookup) for lookup in source.lookups), *lookups)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Digits"]
