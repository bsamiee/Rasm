"""Marks on one pen and glyphs set from the face's own text: recipe marks from its letters and nuqta, small letter marks as miniatures, word signs stacked into a block, and tanwin before the iqlab meem composed to its vowel."""

lazy import math
lazy import unicodedata

import msgspec
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset, Transform

from typography.faces.declaration import Em, Ratio
from typography.rules.scripts import Recipe, Rule, Script
lazy from typography.faces.faults import Outcome
lazy from typography.faces.glyphs import Glyph, Mark, Outline, Point, role, Side, Source
lazy from typography.faces.layout import Context, Contextual
lazy from typography.faces.names import Form, GlyphName
lazy from typography.outlines.measure import moments, restroked, stroke
lazy from typography.outlines.nib import monoline
lazy from typography.outlines.paths import bounds, Box, transformed, union
lazy from typography.rules.measures import inked, key, nuqta, placement, text_ink
lazy from typography.shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Marks(Rule, frozen=True, kw_only=True):
    """Marks on one pen, the vowel stroke over the stem stroke held to a band and floor: the face's own marks offset to it where they stray past a tolerance, and the script's recipe marks it lacks drawn from its stem, head, and teeth letters and nuqta, small marks at a share of the mark box."""

    script: Script
    band: tuple[Ratio, Ratio]
    ratio: Ratio | None
    floor: Ratio | None
    drawn: bool
    angle: float | None
    length: Em | None
    stack: Em
    small: Ratio = 0.75
    tolerance: Ratio = 0.08


class QuranicMarks(Rule, frozen=True, kw_only=True):
    """Small letter marks the face lacks as miniatures of the face's own text within a mark box, hairline signs at the vowel pen, and tanwin before the script's iqlab meems composed in `ccmp` to its vowel."""

    codes: frozenset[int]
    script: Script
    width: Em = 0.42
    height: Ratio = 0.45


class WordSigns(Rule, frozen=True, kw_only=True):
    """The script's word signs the face lacks set from rows of the face's own shaped text, a sign of several rows stacked the gap apart within a block of stem heights."""

    script: Script
    stack: Em
    block: tuple[Ratio, Ratio] = (1.5, 1.6)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [SHAPES]


def _fit(contours: Outline, box: Box, /, *, width: float | None = None, height: float | None = None) -> Outline:
    """Scales contours uniformly to a width or height and moves their ink onto a box's middle and foot."""
    match bounds(contours):
        case None:
            return contours
        case own:
            factor = width / own.width if width is not None else (box.height if height is None else height) / own.height
            return transformed(contours, Transform().translate(box.middle[0] - factor * own.middle[0], box.ymin - factor * own.ymin).scale(factor))


def _ring(box: Box, pen: float, /) -> Outline:
    """A round ring inside a box on a monoline pen, eight quadratic arcs on the circle through the box middle."""
    radius, (x, y) = (min(box.width, box.height) - pen) / 2, box.middle
    reach = tuple(radius / (math.cos(math.pi / 8) if step % 2 else 1.0) for step in range(8))
    points = tuple(Point(x=x + reach[step] * math.cos(math.pi * step / 4), y=y + reach[step] * math.sin(math.pi * step / 4), on=not step % 2) for step in range(8))
    return monoline(((*points, points[0]),), pen)


def _rebuilt(recipe: Recipe, box: Box, models: tuple[Outline, Outline, Outline, Outline], /, *, pen: float, angle: float, stack: float) -> Outline:
    """Draws one mark by recipe from the face's own stem, head, teeth, and nuqta inside the box the original mark held."""
    stem, head, teeth, dot = models
    axis = moments(stem).angle if stem else 90.0
    bar = restroked(
        transformed(_fit(stem, box, height=math.hypot(box.width, box.height) - pen), Transform().translate(*box.middle).rotate(math.radians(angle - axis)).translate(-box.middle[0], -box.middle[1])),
        pen,
    )
    damma = restroked(_fit(head, box), pen)
    match recipe:
        case Recipe.BAR:
            return bar
        case Recipe.DOUBLED:
            return union(bar, transformed(bar, Offset(0.0, (pen + stack) / max(math.cos(math.radians(angle)), 0.1))))
        case Recipe.HEAD:
            return damma
        case Recipe.HEADS:
            return union(damma, transformed(damma, Transform().translate(box.xmin - stack, box.ymin + 0.35 * box.height).scale(0.75).translate(-box.xmax, -box.ymin)))
        case Recipe.TEETH:
            return restroked(_fit(teeth, box, width=box.width), pen)
        case Recipe.RING:
            return _ring(box, pen)
        case Recipe.STEM:
            return restroked(_fit(stem, box), pen)
        case Recipe.DOTS:
            unit = (bounds(dot) or box).width + stack
            return union(*(transformed(dot, Offset(*shift)) for shift in ((box.middle[0] - unit / 2, box.ymin), (box.middle[0] + unit / 2, box.ymin), (box.middle[0], box.ymin + unit))))


def penned(source: Source, rule: Marks, /) -> Outcome[Source]:
    """Holds every recipe mark to one pen measured against the stem stroke at zero advance, a recipe mark the face lacks drawn in a default box.

    Returns:
        Ok(source with penned marks), `ABSENT` for a face without an inked stem.
    """
    return inked(source, rule.script.stem).map(lambda held: _marked(source, rule, stroke(held[0].contours), held[1]))


def _marked(source: Source, rule: Marks, width: float, height: Box, /) -> Source:
    """Offsets or draws each recipe mark at the pen the stem stroke, the band, the stated ratio, and the floor give."""
    script, (low, high) = rule.script, rule.band
    model = source.named.get(key(script.vowels[0]))
    held = stroke(model.contours) / width if model and model.contours else low
    pen = min(max((rule.ratio or min(max(held, low), high)) * width, (rule.floor or 0.0) * height.height), max(high, rule.ratio or 0.0) * width)
    slant = rule.angle if rule.angle is not None else moments(model.contours).angle if model and model.contours else 0.0
    span = rule.length * source.upm if rule.length is not None else 0.3 * height.height
    stem, head, teeth = (source.named[name].contours if name in source.named else () for name in (key(script.stem), key(script.head), key(script.teeth, Form.MEDI)))
    models = (stem, head, teeth, nuqta(source, script.dotted))
    lacking = tuple(Glyph(name=GlyphName(codes=(code,)), role=role(GlyphName(codes=(code,))), advance=0.0) for code in script.recipes if key(code) not in source.named)
    return source.replaced(
        *(
            msgspec.structs.replace(glyph, advance=0.0, contours=contours)
            for glyph in (*source.by_role(Mark), *lacking)
            for recipe in (script.recipes.get(glyph.codes[0]) if glyph.codes else None,)
            for small in (rule.small if glyph.codes and " SMALL " in unicodedata.name(chr(glyph.codes[0]), "") else 1.0,)
            for contours in (
                glyph.contours
                if recipe is None or rule.drawn or (glyph.contours and abs(stroke(glyph.contours) / pen - 1) <= rule.tolerance)
                else restroked(glyph.contours, pen)
                if glyph.contours
                else _rebuilt(recipe, Box(xmin=-small * span / 2, ymin=0.0, xmax=small * span / 2, ymax=small * span / 2), models, pen=pen, angle=slant, stack=rule.stack * source.upm),
            )
        )
    )


# --- [MINIATURES]


def _letter(script: str, name: str, /) -> str:
    """Letter of a script a Unicode name gives after `<script> LETTER`, empty for a name Unicode lacks."""
    return catch(unicodedata.lookup, exception=KeyError)(f"{script} LETTER {name}").default_value("")


def _text(code: int, /) -> str:
    """Letters a small mark's Unicode name spells, joined, in the form it names, empty for a name spelling no letter."""
    script, _, rest = unicodedata.name(chr(code)).partition(" SMALL ")
    match rest.split():
        case ["HIGH" | "LOW", "LIGATURE", *words]:
            spelled = tuple(_letter(script, letter) for letter in " ".join(words).split(" WITH "))
            return "\N{ZERO WIDTH JOINER}".join(spelled) if all(spelled) else ""
        case ["HIGH" | "LOW", first, second] if all(pair := (_letter(script, first), _letter(script, second))) and not _letter(script, f"{first} {second}"):
            return "".join(pair)
        case ["HIGH" | "LOW", *words, "INITIAL" | "ISOLATED" as form, "FORM"]:
            return _letter(script, " ".join(words)) + ("\N{ZERO WIDTH JOINER}" if form == "INITIAL" and _letter(script, " ".join(words)) else "")
        case ["HIGH" | "LOW", *words]:
            return _letter(script, " ".join(words))
        case _:
            return ""


def quranic(source: Source, rule: QuranicMarks, /, *, original: Binary) -> Outcome[Source]:
    """Draws each small letter mark its Unicode name spells as the face's own shaped text held to a mark box and the vowel pen, offsets hairline signs to the pen, and composes tanwin before an iqlab meem.

    Returns:
        Ok(source with its miniatures), `ABSENT` for a face without an inked stem.
    """
    return placement(source, original, rule.script.stem).map(lambda move: _miniatures(source, rule, original, move))


def _miniatures(source: Source, rule: QuranicMarks, original: Binary, move: Transform, /) -> Source:
    """Shapes each spelled mark on the original, maps it into the source, holds it to the mark box at the vowel pen, and adds the iqlab composition."""
    tall, model = bounds(source.named[key(rule.script.stem)].contours), source.named.get(key(rule.script.vowels[0]))
    pen = stroke(model.contours) if model and model.contours else 0.0
    drawn = {code: transformed(text_ink(original, text), move) for code in sorted(rule.codes) if tall and pen and not ((own := source.named.get(key(code))) and own.contours) and (text := _text(code))}
    glyphs = tuple(
        Glyph(
            name=GlyphName(codes=(code,)),
            role=role(GlyphName(codes=(code,))),
            advance=0.0,
            contours=restroked(transformed(contours, (factor, 0, 0, factor, -factor * box.middle[0], -factor * foot)), pen),
        )
        for code, contours in drawn.items()
        if (box := bounds(contours)) and tall
        for foot in ((box.ymin if Side.of(code) is Side.ABOVE else box.ymax),)
        for factor in (min(rule.width * source.upm / box.width, rule.height * tall.height / box.height),)
    )
    thin = tuple(
        msgspec.structs.replace(glyph, contours=restroked(glyph.contours, pen))
        for code in sorted(rule.script.hairlines)
        if (glyph := source.named.get(key(code))) and glyph.contours and stroke(glyph.contours) < 0.85 * pen
    )
    tanwin = {
        code: vowel
        for code in source.encoded
        if unicodedata.category(chr(code)) == "Mn"
        and (name := unicodedata.name(chr(code), "")).endswith("TAN")
        and (vowel := catch(unicodedata.lookup, exception=KeyError)(name.removesuffix("TAN")).default_value(""))
    }
    rules = tuple(
        Context(input=((key(code),),), lookahead=((key(meem),),), replacement=(key(ord(vowel)),))
        for code, vowel in sorted(tanwin.items())
        for meem in rule.script.iqlab
        if Side.of(meem) is Side.of(code) and key(meem) in source.named and key(ord(vowel)) in source.named
    )
    return msgspec.structs.replace(source.replaced(*glyphs, *thin), lookups=(*source.lookups, *((Contextual(feature="ccmp", order=-2, rules=rules),) if rules else ())))


# --- [SIGNS]


def _stacked(rows: tuple[Outline, ...], gap: float, /) -> Outline:
    """Stacks row inks top to bottom, each centred on the first, the gap between their ink."""
    boxes = tuple(bounds(row) or Box(xmin=0, ymin=0, xmax=0, ymax=0) for row in rows)
    tops = tuple(-sum(box.height + gap for box in boxes[:index]) for index in range(len(boxes)))
    return union(*(transformed(row, (1.0, 0.0, 0.0, 1.0, boxes[0].middle[0] - box.middle[0], top + boxes[0].ymax - box.ymax)) for row, box, top in zip(rows, boxes, tops, strict=True)))


def signed(source: Source, rule: WordSigns, /, *, original: Binary) -> Outcome[Source]:
    """Sets each word sign the face lacks from its rows shaped on the original at the stem's side bearings, a sign of several rows stacked and held to a block in stem heights about the stem's middle.

    Returns:
        Ok(source with its word signs), `ABSENT` for a face without an inked stem.
    """
    return inked(source, rule.script.stem).bind(lambda held: placement(source, original, rule.script.stem).map(lambda move: source.replaced(*_signs(source, rule, original, move, held))))


def _signs(source: Source, rule: WordSigns, original: Binary, move: Transform, held: tuple[Glyph, Box], /) -> tuple[Glyph, ...]:
    """Shapes each sign's rows on the original, maps them into the source, stacks them, and places the ink at the stem's bearings."""
    (glyph, stem), (wide, tall) = held, rule.block
    bearing = (glyph.advance - stem.width) / 2
    return tuple(
        Glyph(
            name=GlyphName(codes=(code,)),
            role=role(GlyphName(codes=(code,))),
            advance=factor * extent.width + 2 * bearing,
            contours=transformed(drawn, (factor, 0.0, 0.0, factor, bearing - factor * extent.xmin, stem.middle[1] - factor * extent.middle[1] if factor != 1.0 else 0.0)),
        )
        for code, rows in sorted(rule.script.signs.items())
        if not (own := source.named.get(key(code))) or not own.contours
        for drawn in (_stacked(tuple(transformed(text_ink(original, row), move) for row in rows), rule.stack * source.upm),)
        if (extent := bounds(drawn))
        for factor in ((min(1.0, wide * stem.height / extent.width, tall * stem.height / extent.height) if len(rows) > 1 else 1.0),)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Marks", "QuranicMarks", "WordSigns", "penned", "quranic", "signed"]
