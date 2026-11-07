"""Marks on one pen and glyphs set from the face's own text: recipe marks from its letters and nuqta, small letter marks as miniatures, word signs stacked into a block, and tanwin before the iqlab meem composed to its vowel."""

lazy from collections.abc import Collection
lazy from itertools import accumulate
lazy import math
lazy import unicodedata

import msgspec
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset, Transform
lazy import pathops
lazy from scipy.special import cosdg, sindg

from typography.faces.declaration import Em, Ratio, Recipe, Script
from typography.rules.measures import bounded_glyph, made, nuqta, placement, Rule, text_outline
lazy from typography.faces.faults import Outcome
lazy from typography.faces.glyphs import Glyph, Mark, Outline, Side, Source, traced
lazy from typography.faces.layout import Chain, Contextual
lazy from typography.faces.names import Form, GlyphName
lazy from typography.outlines.measure import moments, restroked, stroke
lazy from typography.outlines.paths import bounds, Box, transformed, union
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
    """Word signs of the script the face lacks, set from rows of the face's own shaped text, a sign of several rows stacked the gap apart within a block of stem heights."""

    script: Script
    stack: Em
    block: tuple[Ratio, Ratio] = (1.5, 1.6)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TARGETS]


def _targets(source: Source, codes: Collection[int], /) -> tuple[tuple[Glyph | None, int], ...]:
    """Requested native glyphs once, preferring their canonical code over the lowest requested alias."""
    targets: dict[str, tuple[Glyph | None, int]] = {}
    for code in sorted(codes):
        if (name := source.key(code)) not in targets:
            glyph = source.named.get(name)
            selected = next((part for part in glyph.name.codes if part in glyph.codes and part in codes), code) if glyph is not None else code
            targets[name] = (glyph, selected)
    return tuple(targets.values())


def _with_contours(glyph: Glyph | None, code: int, advance: float, contours: Outline, /) -> Glyph:
    """Writes generated contours, preserving an existing native glyph's identity and every cmap mapping."""
    return (
        msgspec.structs.replace(glyph, codes=glyph.codes | {code}, advance=advance, contours=contours, components=())
        if glyph is not None
        else made(GlyphName(stem=(code,)), advance, contours=contours)
    )


# --- [SHAPES]


def _fit(contours: Outline, box: Box, /, *, width: float | None = None, height: float | None = None) -> Outline:
    """Scales contours uniformly to a width or height and moves their outline onto a box's middle and foot."""
    match bounds(contours):
        case None:
            return contours
        case own:
            factor = width / own.width if width is not None else (box.height if height is None else height) / own.height
            return transformed(contours, Transform().translate(box.middle[0] - factor * own.middle[0], box.ymin - factor * own.ymin).scale(factor))


def _rebuilt(recipe: Recipe, box: Box, models: tuple[Outline, Outline, Outline, Outline], /, *, pen: float, angle: float, stack: float) -> Outline:
    """Draws one mark by recipe from the face's own stem, head, teeth, and nuqta inside the box the original mark held."""
    stem, head, teeth, dot = models
    match recipe:
        case Recipe.BAR | Recipe.DOUBLED:
            degrees = math.remainder(angle, 360.0)
            bar = restroked(
                transformed(
                    _fit(stem, box, height=math.hypot(box.width, box.height) - pen),
                    Transform().translate(*box.middle).rotate(math.radians(degrees - moments(stem).angle)).translate(-box.middle[0], -box.middle[1]),
                ),
                pen,
            )
            return bar if recipe is Recipe.BAR else union(bar, transformed(bar, Offset(-(pen + stack) * sindg(degrees), (pen + stack) * cosdg(degrees))))  # ty: ignore[no-matching-overload]
        case Recipe.HEAD | Recipe.HEADS:
            damma = restroked(_fit(head, box), pen)
            return (
                damma if recipe is Recipe.HEAD else union(damma, transformed(damma, Transform().translate(box.xmin - stack, box.ymin + 0.35 * box.height).scale(0.75).translate(-box.xmax, -box.ymin)))
            )
        case Recipe.TEETH:
            return restroked(_fit(teeth, box, width=box.width), pen)
        case Recipe.RING:
            radius, (x, y), path = (min(box.width, box.height) - pen) / 2, box.middle, pathops.Path()
            path.moveTo(x + radius, y)
            path.arcTo(radius, radius, 0, pathops.ArcSize.SMALL, pathops.Direction.CCW, x - radius, y)
            path.arcTo(radius, radius, 0, pathops.ArcSize.SMALL, pathops.Direction.CCW, x + radius, y)
            path.close()
            path.stroke(pen, pathops.LineCap.ROUND_CAP, pathops.LineJoin.ROUND_JOIN, 1.0)
            path.convertConicsToQuads()
            return union(traced(path.draw))
        case Recipe.STEM:
            return restroked(_fit(stem, box), pen)
        case Recipe.DOTS:
            match bounds(dot):
                case None:
                    return ()
                case own:
                    unit = own.width + stack
                    return union(
                        *(
                            transformed(dot, Offset(x - own.middle[0], y - own.ymin))
                            for x, y in ((box.middle[0] - unit / 2, box.ymin), (box.middle[0] + unit / 2, box.ymin), (box.middle[0], box.ymin + unit))
                        )
                    )


def penned(source: Source, rule: Marks, /) -> Outcome[Source]:
    """Holds every recipe mark to one pen measured against the stem stroke at zero advance, a recipe mark the face lacks drawn in a default box.

    Returns:
        Ok(source with penned marks), `ABSENT` for a face without a drawn stem.
    """
    return bounded_glyph(source, rule.script.stem).map(lambda held: _marked(source, rule, stroke(held[0].contours), held[1]))


def _marked(source: Source, rule: Marks, width: float, height: Box, /) -> Source:
    """Offsets or draws each recipe mark at the pen the stem stroke, the band, the stated ratio, and the floor give."""
    script, (low, high) = rule.script, rule.band
    model = source.drawn(name) if (name := source.key(script.vowels[0])) in source.named else None
    held = stroke(model.contours) / width if model and model.contours else low
    pen = min(max((rule.ratio or min(max(held, low), high)) * width, (rule.floor or 0.0) * height.height), max(high, rule.ratio or 0.0) * width)
    slant = rule.angle if rule.angle is not None else moments(model.contours).angle if model and model.contours else 0.0
    span = rule.length * source.upm if rule.length is not None else 0.3 * height.height
    stem, head, teeth = (source.drawn(name).contours if name in source.named else () for name in (source.key(script.stem), source.key(script.head), source.key(script.teeth, Form.MEDI)))
    models = (stem, head, teeth, nuqta(source, script.dotted))
    targets = _targets(source, script.recipes)
    recipes = {glyph.name.text: (code, script.recipes[code]) for glyph, code in targets if glyph is not None}

    def rebuilt(code: int, recipe: Recipe, /) -> Outline:
        small = rule.small if " SMALL " in unicodedata.name(chr(code)) else 1.0
        return _rebuilt(recipe, Box(xmin=-small * span / 2, ymin=0.0, xmax=small * span / 2, ymax=small * span / 2), models, pen=pen, angle=slant, stack=rule.stack * source.upm)

    def changed(glyph: Glyph, /) -> Glyph:
        match recipes.get(glyph.name.text):
            case None:
                return msgspec.structs.replace(glyph, advance=0.0)
            case code, recipe:
                if (contours := source.drawn(glyph.name.text).contours) and (rule.drawn or abs(stroke(contours) / pen - 1) <= rule.tolerance):
                    return msgspec.structs.replace(glyph, codes=glyph.codes | {code}, advance=0.0)
                return _with_contours(glyph, code, 0.0, restroked(contours, pen) if contours else rebuilt(code, recipe))

    return source.replaced(*(changed(glyph) for glyph in source.by_role(Mark)), *(_with_contours(None, code, 0.0, rebuilt(code, script.recipes[code])) for glyph, code in targets if glyph is None))


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
        case ["HIGH" | "LOW", first, second]:
            return combined if (combined := _letter(script, f"{first} {second}")) else "".join(pair) if all(pair := (_letter(script, first), _letter(script, second))) else ""
        case ["HIGH" | "LOW", *words, "INITIAL" | "ISOLATED" as form, "FORM"]:
            letter = _letter(script, " ".join(words))
            return letter + ("\N{ZERO WIDTH JOINER}" if form == "INITIAL" and letter else "")
        case ["HIGH" | "LOW", *words]:
            return _letter(script, " ".join(words))
        case _:
            return ""


def quranic(source: Source, rule: QuranicMarks, /, *, original: Binary) -> Outcome[Source]:
    """Draws each small letter mark its Unicode name spells as the face's own shaped text held to a mark box and the vowel pen, offsets hairline signs to the pen, and composes tanwin before an iqlab meem.

    Returns:
        Ok(source with its miniatures), `ABSENT` for a face without a drawn stem.
    """
    return placement(source, original, rule.script.stem).map(lambda held: _miniatures(source, rule, original, held[2], held[1]))


def _miniatures(source: Source, rule: QuranicMarks, original: Binary, move: Transform, tall: Box, /) -> Source:
    """Shapes each spelled mark on the original, maps it into the source, holds it to the mark box at the vowel pen, and adds the iqlab composition."""
    model = source.drawn(name) if (name := source.key(rule.script.vowels[0])) in source.named else None
    pen = stroke(model.contours) if model and model.contours else 0.0
    drawn = tuple(
        (glyph, code, transformed(text_outline(original, text), move))
        for glyph, code in _targets(source, rule.codes)
        if pen and (glyph is None or not source.drawn(glyph.name.text).contours) and (text := _text(code))
    )
    glyphs = tuple(
        _with_contours(glyph, code, 0.0, restroked(transformed(contours, (factor, 0, 0, factor, -factor * box.middle[0], -factor * foot)), pen))
        for glyph, code, contours in drawn
        if (box := bounds(contours))
        for foot in ((box.ymin if Side.of(code) is Side.ABOVE else box.ymax),)
        for factor in (min(rule.width * source.upm / box.width, rule.height * tall.height / box.height),)
    )
    thin = tuple(
        _with_contours(glyph, code, glyph.advance, restroked(contours, pen))
        for glyph, code in _targets(source, rule.script.hairlines)
        if glyph is not None and (contours := source.drawn(glyph.name.text).contours) and stroke(contours) < 0.85 * pen
    )
    tanwin = {
        code: vowel
        for glyph in source.by_role(Mark)
        for code in glyph.name.codes[:1]
        if code in glyph.codes
        and unicodedata.category(chr(code)) == "Mn"
        and (name := unicodedata.name(chr(code), "")).endswith("TAN")
        and (vowel := catch(unicodedata.lookup, exception=KeyError)(name.removesuffix("TAN")).default_value(""))
    }
    rules = tuple(
        Chain(input=(((source.key(code),), source.key(ord(vowel))),), lookahead=((source.key(meem),),))
        for code, vowel in sorted(tanwin.items())
        for meem in rule.script.iqlab
        if Side.of(meem) is Side.of(code) and meem in source.encoded and ord(vowel) in source.encoded
    )
    return msgspec.structs.replace(source.replaced(*glyphs, *thin), lookups=(*source.lookups, *((Contextual(features=frozendict(ccmp=frozendict()), order=-2, rules=rules),) if rules else ())))


# --- [SIGNS]


def _stacked(rows: tuple[Outline, ...], gap: float, /) -> Outline:
    """Stacks row outlines top to bottom, each centered on the first, the gap between their outlines."""
    boxes = tuple(bounds(row) or Box(xmin=0, ymin=0, xmax=0, ymax=0) for row in rows)
    tops = accumulate((box.height + gap for box in boxes), initial=0.0)
    return union(*(transformed(row, (1.0, 0.0, 0.0, 1.0, boxes[0].middle[0] - box.middle[0], boxes[0].ymax - top - box.ymax)) for row, box, top in zip(rows, boxes, tops, strict=False)))


def signed(source: Source, rule: WordSigns, /, *, original: Binary) -> Outcome[Source]:
    """Sets each word sign the face lacks from its rows shaped on the original at the stem's side bearings, a sign of several rows stacked and held to a block in stem heights about the stem's middle.

    Returns:
        Ok(source with its word signs), `ABSENT` for a face without a drawn stem.
    """
    return placement(source, original, rule.script.stem).map(lambda held: source.replaced(*_signs(source, rule, original, held[2], (held[0], held[1]))))


def _signs(source: Source, rule: WordSigns, original: Binary, move: Transform, held: tuple[Glyph, Box], /) -> tuple[Glyph, ...]:
    """Shapes each sign's rows on the original, maps them into the source, stacks them, and places the outline at the stem's bearings."""
    (glyph, stem), (wide, tall) = held, rule.block
    bearing = (glyph.advance - stem.width) / 2
    return tuple(
        _with_contours(
            own,
            code,
            factor * extent.width + 2 * bearing,
            transformed(drawn, (factor, 0.0, 0.0, factor, bearing - factor * extent.xmin, stem.middle[1] - factor * extent.middle[1] if factor != 1.0 else 0.0)),
        )
        for own, code in _targets(source, rule.script.signs)
        if own is None or not source.drawn(own.name.text).contours
        for rows in (rule.script.signs[code],)
        for drawn in (_stacked(tuple(transformed(text_outline(original, row), move) for row in rows), rule.stack * source.upm),)
        if (extent := bounds(drawn))
        for factor in ((min(1.0, wide * stem.height / extent.width, tall * stem.height / extent.height) if len(rows) > 1 else 1.0),)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Marks", "QuranicMarks", "WordSigns", "penned", "quranic", "signed"]
