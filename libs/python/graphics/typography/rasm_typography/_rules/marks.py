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

from rasm_typography._faces.declaration import Em, Ratio, Recipe, Script
from rasm_typography._rules.measures import _made, _nuqta, _placement, _text_outline, bounded_glyph, Rule
lazy from rasm_records import Outcome
lazy from rasm_typography._faces.glyphs import _Glyph, _Outline, _Side, _traced, Mark, Source
lazy from rasm_typography._faces.layout import _Chain, Contextual
lazy from rasm_typography._faces.names import _Form, _GlyphName
lazy from rasm_typography._outlines.measure import _moments, _restroked, _stroke
lazy from rasm_typography._outlines.paths import _bounds, _Box, _transformed, _union
lazy from rasm_typography._shaping.runs import Binary

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


def _targets(source: Source, codes: Collection[int], /) -> tuple[tuple[_Glyph | None, int], ...]:
    """Requested native glyphs once, preferring their canonical code over the lowest requested alias."""
    targets: dict[str, tuple[_Glyph | None, int]] = {}
    for code in sorted(codes):
        if (name := source.key(code)) not in targets:
            glyph = source.named.get(name)
            selected = next((part for part in glyph.name.codes if part in glyph.codes and part in codes), code) if glyph is not None else code
            targets[name] = (glyph, selected)
    return tuple(targets.values())


def _with_contours(glyph: _Glyph | None, code: int, advance: float, contours: _Outline, /) -> _Glyph:
    """Writes generated contours, preserving an existing native glyph's identity and every cmap mapping."""
    return (
        msgspec.structs.replace(glyph, codes=glyph.codes | {code}, advance=advance, contours=contours, components=())
        if glyph is not None
        else _made(_GlyphName(stem=(code,)), advance, contours=contours)
    )


# --- [SHAPES]


def _fit(contours: _Outline, box: _Box, /, *, width: float | None = None, height: float | None = None) -> _Outline:
    """Scales contours uniformly to a width or height and moves their outline onto a box's middle and foot."""
    match _bounds(contours):
        case None:
            return contours
        case own:
            factor = width / own.width if width is not None else (box.height if height is None else height) / own.height
            return _transformed(contours, Transform().translate(box.middle[0] - factor * own.middle[0], box.ymin - factor * own.ymin).scale(factor))


def _rebuilt(recipe: Recipe, box: _Box, models: tuple[_Outline, _Outline, _Outline, _Outline], /, *, pen: float, angle: float, stack: float) -> _Outline:
    """Draws one mark by recipe from the face's own stem, head, teeth, and nuqta inside the box the original mark held."""
    stem, head, teeth, dot = models
    match recipe:
        case Recipe.BAR | Recipe.DOUBLED:
            degrees = math.remainder(angle, 360.0)
            bar = _restroked(
                _transformed(
                    _fit(stem, box, height=math.hypot(box.width, box.height) - pen),
                    Transform().translate(*box.middle).rotate(math.radians(degrees - _moments(stem).angle)).translate(-box.middle[0], -box.middle[1]),
                ),
                pen,
            )
            return bar if recipe is Recipe.BAR else _union(bar, _transformed(bar, Offset(-(pen + stack) * sindg(degrees), (pen + stack) * cosdg(degrees))))  # ty: ignore[no-matching-overload]
        case Recipe.HEAD | Recipe.HEADS:
            damma = _restroked(_fit(head, box), pen)
            return (
                damma
                if recipe is Recipe.HEAD
                else _union(damma, _transformed(damma, Transform().translate(box.xmin - stack, box.ymin + 0.35 * box.height).scale(0.75).translate(-box.xmax, -box.ymin)))
            )
        case Recipe.TEETH:
            return _restroked(_fit(teeth, box, width=box.width), pen)
        case Recipe.RING:
            radius, (x, y), path = (min(box.width, box.height) - pen) / 2, box.middle, pathops.Path()
            path.moveTo(x + radius, y)
            path.arcTo(radius, radius, 0, pathops.ArcSize.SMALL, pathops.Direction.CCW, x - radius, y)
            path.arcTo(radius, radius, 0, pathops.ArcSize.SMALL, pathops.Direction.CCW, x + radius, y)
            path.close()
            path.stroke(pen, pathops.LineCap.ROUND_CAP, pathops.LineJoin.ROUND_JOIN, 1.0)
            path.convertConicsToQuads()
            return _union(_traced(path.draw))
        case Recipe.STEM:
            return _restroked(_fit(stem, box), pen)
        case Recipe.DOTS:
            match _bounds(dot):
                case None:
                    return ()
                case own:
                    unit = own.width + stack
                    return _union(
                        *(
                            _transformed(dot, Offset(x - own.middle[0], y - own.ymin))
                            for x, y in ((box.middle[0] - unit / 2, box.ymin), (box.middle[0] + unit / 2, box.ymin), (box.middle[0], box.ymin + unit))
                        )
                    )


def _penned(source: Source, rule: Marks, /) -> Outcome[Source]:
    """Holds every recipe mark to one pen measured against the stem stroke at zero advance, a recipe mark the face lacks drawn in a default box.

    Returns:
        Ok(source with penned marks), `ABSENT` for a face without a drawn stem.
    """
    return bounded_glyph(source, rule.script.stem).map(lambda held: _marked(source, rule, _stroke(held[0].contours), held[1]))


def _marked(source: Source, rule: Marks, width: float, height: _Box, /) -> Source:
    """Offsets or draws each recipe mark at the pen the stem stroke, the band, the stated ratio, and the floor give."""
    script, (low, high) = rule.script, rule.band
    model = source.drawn(name) if (name := source.key(script.vowels[0])) in source.named else None
    held = _stroke(model.contours) / width if model and model.contours else low
    pen = min(max((rule.ratio or min(max(held, low), high)) * width, (rule.floor or 0.0) * height.height), max(high, rule.ratio or 0.0) * width)
    slant = rule.angle if rule.angle is not None else _moments(model.contours).angle if model and model.contours else 0.0
    span = rule.length * source.upm if rule.length is not None else 0.3 * height.height
    stem, head, teeth = (source.drawn(name).contours if name in source.named else () for name in (source.key(script.stem), source.key(script.head), source.key(script.teeth, _Form.MEDI)))
    models = (stem, head, teeth, _nuqta(source, script.dotted))
    targets = _targets(source, script.recipes)
    recipes = {glyph.name.text: (code, script.recipes[code]) for glyph, code in targets if glyph is not None}

    def rebuilt(code: int, recipe: Recipe, /) -> _Outline:
        small = rule.small if " SMALL " in unicodedata.name(chr(code)) else 1.0
        return _rebuilt(recipe, _Box(xmin=-small * span / 2, ymin=0.0, xmax=small * span / 2, ymax=small * span / 2), models, pen=pen, angle=slant, stack=rule.stack * source.upm)

    def changed(glyph: _Glyph, /) -> _Glyph:
        match recipes.get(glyph.name.text):
            case None:
                return msgspec.structs.replace(glyph, advance=0.0)
            case code, recipe:
                if (contours := source.drawn(glyph.name.text).contours) and (rule.drawn or abs(_stroke(contours) / pen - 1) <= rule.tolerance):
                    return msgspec.structs.replace(glyph, codes=glyph.codes | {code}, advance=0.0)
                return _with_contours(glyph, code, 0.0, _restroked(contours, pen) if contours else rebuilt(code, recipe))

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


def _quranic(source: Source, rule: QuranicMarks, /, *, original: Binary) -> Outcome[Source]:
    """Draws each small letter mark its Unicode name spells as the face's own shaped text held to a mark box and the vowel pen, offsets hairline signs to the pen, and composes tanwin before an iqlab meem.

    Returns:
        Ok(source with its miniatures), `ABSENT` for a face without a drawn stem.
    """
    return _placement(source, original, rule.script.stem).map(lambda held: _miniatures(source, rule, original, held[2], held[1]))


def _miniatures(source: Source, rule: QuranicMarks, original: Binary, move: Transform, tall: _Box, /) -> Source:
    """Shapes each spelled mark on the original, maps it into the source, holds it to the mark box at the vowel pen, and adds the iqlab composition."""
    model = source.drawn(name) if (name := source.key(rule.script.vowels[0])) in source.named else None
    pen = _stroke(model.contours) if model and model.contours else 0.0
    drawn = tuple(
        (glyph, code, _transformed(_text_outline(original, text), move))
        for glyph, code in _targets(source, rule.codes)
        if pen and (glyph is None or not source.drawn(glyph.name.text).contours) and (text := _text(code))
    )
    glyphs = tuple(
        _with_contours(glyph, code, 0.0, _restroked(_transformed(contours, (factor, 0, 0, factor, -factor * box.middle[0], -factor * foot)), pen))
        for glyph, code, contours in drawn
        if (box := _bounds(contours))
        for foot in ((box.ymin if _Side.of(code) is _Side.ABOVE else box.ymax),)
        for factor in (min(rule.width * source.upm / box.width, rule.height * tall.height / box.height),)
    )
    thin = tuple(
        _with_contours(glyph, code, glyph.advance, _restroked(contours, pen))
        for glyph, code in _targets(source, rule.script.hairlines)
        if glyph is not None and (contours := source.drawn(glyph.name.text).contours) and _stroke(contours) < 0.85 * pen
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
        _Chain(input=(((source.key(code),), source.key(ord(vowel))),), lookahead=((source.key(meem),),))
        for code, vowel in sorted(tanwin.items())
        for meem in rule.script.iqlab
        if _Side.of(meem) is _Side.of(code) and meem in source.encoded and ord(vowel) in source.encoded
    )
    return msgspec.structs.replace(source.replaced(*glyphs, *thin), lookups=(*source.lookups, *((Contextual(features=frozendict(ccmp=frozendict()), order=-2, rules=rules),) if rules else ())))


# --- [SIGNS]


def _stacked(rows: tuple[_Outline, ...], gap: float, /) -> _Outline:
    """Stacks row outlines top to bottom, each centered on the first, the gap between their outlines."""
    boxes = tuple(_bounds(row) or _Box(xmin=0, ymin=0, xmax=0, ymax=0) for row in rows)
    tops = accumulate((box.height + gap for box in boxes), initial=0.0)
    return _union(*(_transformed(row, (1.0, 0.0, 0.0, 1.0, boxes[0].middle[0] - box.middle[0], boxes[0].ymax - top - box.ymax)) for row, box, top in zip(rows, boxes, tops, strict=False)))


def _signed(source: Source, rule: WordSigns, /, *, original: Binary) -> Outcome[Source]:
    """Sets each word sign the face lacks from its rows shaped on the original at the stem's side bearings, a sign of several rows stacked and held to a block in stem heights about the stem's middle.

    Returns:
        Ok(source with its word signs), `ABSENT` for a face without a drawn stem.
    """
    return _placement(source, original, rule.script.stem).map(lambda held: source.replaced(*_signs(source, rule, original, held[2], (held[0], held[1]))))


def _signs(source: Source, rule: WordSigns, original: Binary, move: Transform, held: tuple[_Glyph, _Box], /) -> tuple[_Glyph, ...]:
    """Shapes each sign's rows on the original, maps them into the source, stacks them, and places the outline at the stem's bearings."""
    (glyph, stem), (wide, tall) = held, rule.block
    bearing = (glyph.advance - stem.width) / 2
    return tuple(
        _with_contours(
            own,
            code,
            factor * extent.width + 2 * bearing,
            _transformed(drawn, (factor, 0.0, 0.0, factor, bearing - factor * extent.xmin, stem.middle[1] - factor * extent.middle[1] if factor != 1.0 else 0.0)),
        )
        for own, code in _targets(source, rule.script.signs)
        if own is None or not source.drawn(own.name.text).contours
        for rows in (rule.script.signs[code],)
        for drawn in (_stacked(tuple(_transformed(_text_outline(original, row), move) for row in rows), rule.stack * source.upm),)
        if (extent := _bounds(drawn))
        for factor in ((min(1.0, wide * stem.height / extent.width, tall * stem.height / extent.height) if len(rows) > 1 else 1.0),)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Marks", "QuranicMarks", "WordSigns"]
