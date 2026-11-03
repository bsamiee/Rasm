"""Digits: a second script's digits sharing the first's drawings, one digit pen, a held zero, a default advance kind, and both `pnum` and `tnum`."""

from typing import Literal
lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Transform

from typography.faces.declaration import Em, Ratio
from typography.rules.scripts import Rule, Script
lazy from typography.faces.glyphs import Digit, Glyph, role, Source
lazy from typography.faces.layout import Single
lazy from typography.faces.names import GlyphName
lazy from typography.outlines.measure import restroked, stroke
lazy from typography.outlines.paths import bounds, transformed
lazy from typography.rules.measures import centred, key, middle

# --- [MODELS] ---------------------------------------------------------------------------


class Digits(Rule, frozen=True, kw_only=True):
    """Digits of the script's lead and second digit scripts sharing drawings for the shared decimal values, on one pen, the zero held to a share of the stem height, and the default advance kind."""

    script: Script
    zero: Ratio | None
    advance: Literal["tabular", "proportional"]
    bearing: Em | None
    tolerance: Ratio = 0.08


# --- [OPERATIONS] -----------------------------------------------------------------------


def figured(source: Source, rule: Digits, /) -> Source:
    """Draws the digits of two scripts on one pen and sets their default advances, `pnum` and `tnum` each reaching its kind from either."""
    (lead, second), shared = rule.script.digits, rule.script.shared
    own = {glyph.name.codes[0]: glyph for glyph in source.glyphs if glyph.codes and isinstance(glyph.role, Digit) and glyph.role.script in {lead, second} and glyph.contours}
    leads = {unicodedata.decimal(chr(code)): code for code, glyph in own.items() if isinstance(glyph.role, Digit) and glyph.role.script == lead}
    drawn = {
        code: own[leads[value]].contours if code not in leads.values() and (value := unicodedata.decimal(chr(code))) in shared and value in leads else glyph.contours for code, glyph in own.items()
    }
    strokes = {code: stroke(contours) for code, contours in drawn.items()}
    pen = middle(tuple(strokes.values()))
    penned = {code: restroked(contours, pen) if pen and abs(strokes[code] / pen - 1) > rule.tolerance else contours for code, contours in drawn.items()}
    stem = bounds(source.named[key(rule.script.stem)].contours) if key(rule.script.stem) in source.named else None
    zero = bounds(penned[leads[0]]) if 0 in leads else None
    penned |= (
        {leads[0]: transformed(penned[leads[0]], Transform().translate(*zero.middle).scale(rule.zero * stem.height / zero.width).translate(-zero.middle[0], -zero.middle[1]))}
        if zero and stem and rule.zero is not None
        else {}
    )
    if not (boxes := {code: box for code, contours in penned.items() if (box := bounds(contours))}):
        return source
    broadest = max(boxes, key=lambda code: boxes[code].width)
    sides = tuple(side for code, box in boxes.items() for side in (box.xmin, own[code].advance - box.xmax) if side >= 0)
    even = (own[broadest].advance - boxes[broadest].width) / 2
    margin = rule.bearing * source.upm if rule.bearing is not None else max(middle(sides), even)
    widths = {code: {"tabular": boxes[broadest].width + 2 * (even if even >= 0 else middle(sides)), "proportional": box.width + 2 * margin} for code, box in boxes.items()}
    variant, other = ("pnum", "proportional") if rule.advance == "tabular" else ("tnum", "tabular")
    glyphs = tuple(
        Glyph(name=name, role=role(GlyphName(codes=(code,))), advance=widths[code][kind], contours=centred(penned[code], widths[code][kind]))
        for code in boxes
        for name, kind in ((GlyphName(codes=(code,)), rule.advance), (GlyphName(codes=(code,), variant=variant), other))
    )
    order = max((lookup.order for lookup in source.lookups), default=0) + 1
    mapping = {key(code): f"{key(code)}.{variant}" for code in boxes}
    lookups = (
        Single(feature=variant, order=order, mapping=frozendict(mapping)),
        Single(feature="tnum" if variant == "pnum" else "pnum", order=order + 1, mapping=frozendict({held: default for default, held in mapping.items()})),
    )
    return msgspec.structs.replace(source.replaced(*glyphs), lookups=(*(lookup for lookup in source.lookups if lookup.feature not in {"pnum", "tnum"}), *lookups))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Digits", "figured"]
