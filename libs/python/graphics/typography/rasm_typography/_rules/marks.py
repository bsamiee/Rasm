"""Marks on one pen and glyphs set from the face's own text: recipe marks from its letters and nuqta, small letter marks as miniatures, word signs stacked into a block, and tanwin before the iqlab meem composed to its vowel."""

lazy from collections.abc import Collection
lazy from itertools import accumulate
lazy import math
lazy import unicodedata

import msgspec
lazy from expression import Option, Result, Some
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset, Transform
lazy import numpy as np
lazy import pathops
lazy from scipy.special import cosdg, sindg

from rasm_typography._faces.declaration import Em, Ratio, Recipe, Script
from rasm_typography._rules import measures
lazy from rasm_records import Error, Nothing, Ok, Outcome
lazy from rasm_typography._faces.glyphs import Glyph, Mark, Point, Side, Source, traced
lazy from rasm_typography._faces.layout import Chain, Contextual
lazy from rasm_typography._faces.names import Form, GlyphName
lazy from rasm_typography._outlines import measure, paths
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Marks(measures.Rule, frozen=True, kw_only=True):
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

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Offsets or draws each recipe mark at the pen the stem stroke, the band, the stated ratio, and the floor give, `ABSENT` naming the stem outline a face lacks."""

        def rebuilt(code: int, recipe: Recipe, /) -> tuple[tuple[Point, ...], ...]:
            small = self.small if " SMALL " in unicodedata.name(chr(code)) else 1.0
            box, stack = paths.Box(xmin=-small * span / 2, ymin=0.0, xmax=small * span / 2, ymax=small * span / 2), self.stack * source.upm
            match recipe:
                case Recipe.BAR | Recipe.DOUBLED:
                    degrees = np.float64(math.remainder(slant, 360.0))
                    bar = measure.restroked(
                        paths.transformed(
                            _fit(stem, box, height=math.hypot(box.width, box.height) - pen),
                            Transform().translate(*box.middle).rotate(math.radians(degrees - measure.moments(stem).angle)).translate(-box.middle[0], -box.middle[1]),
                        ),
                        pen,
                    )
                    return bar if recipe is Recipe.BAR else paths.union(bar, paths.transformed(bar, Offset(-(pen + stack) * sindg(degrees), (pen + stack) * cosdg(degrees))))
                case Recipe.HEAD | Recipe.HEADS:
                    damma = measure.restroked(_fit(head, box), pen)
                    return (
                        damma
                        if recipe is Recipe.HEAD
                        else paths.union(damma, paths.transformed(damma, Transform().translate(box.xmin - stack, box.ymin + 0.35 * box.height).scale(0.75).translate(-box.xmax, -box.ymin)))
                    )
                case Recipe.TEETH:
                    return measure.restroked(_fit(teeth, box, width=box.width), pen)
                case Recipe.RING:
                    radius, (x, y), path = (min(box.width, box.height) - pen) / 2, box.middle, pathops.Path()
                    path.moveTo(x + radius, y)
                    path.arcTo(radius, radius, 0, pathops.ArcSize.SMALL, pathops.Direction.CCW, x - radius, y)
                    path.arcTo(radius, radius, 0, pathops.ArcSize.SMALL, pathops.Direction.CCW, x + radius, y)
                    path.close()
                    path.stroke(pen, pathops.LineCap.ROUND_CAP, pathops.LineJoin.ROUND_JOIN, 1.0)
                    path.convertConicsToQuads()
                    return paths.union(traced(path.draw))
                case Recipe.STEM:
                    return measure.restroked(_fit(stem, box), pen)
                case Recipe.DOTS:
                    match paths.bounds(dot):
                        case None:
                            return ()
                        case own:
                            unit = own.width + stack
                            return paths.union(
                                *(
                                    paths.transformed(dot, Offset(x - own.middle[0], y - own.ymin))
                                    for x, y in ((box.middle[0] - unit / 2, box.ymin), (box.middle[0] + unit / 2, box.ymin), (box.middle[0], box.ymin + unit))
                                )
                            )

        def changed(glyph: Glyph, /) -> Glyph:
            match recipes.get(glyph.name.text):
                case None:
                    return msgspec.structs.replace(glyph, advance=0.0)
                case code, recipe:
                    if (contours := source.drawn(glyph.name.text).contours) and (self.drawn or abs(measure.stroke(contours) / pen - 1) <= self.tolerance):
                        return msgspec.structs.replace(glyph, codes=glyph.codes | {code}, advance=0.0)
                    return _with_contours(glyph, code, 0.0, measure.restroked(contours, pen) if contours else rebuilt(code, recipe))

        match measures.bounded_glyph(source, self.script.stem):
            case Result(tag="ok", ok=(drawn, height)):
                script, (low, high), width = self.script, self.band, measure.stroke(drawn.contours)
                model = source.drawn(name) if (name := source.key(script.vowels[0])) in source.named else None
                held = measure.stroke(model.contours) / width if model and model.contours else low
                pen = min(max((self.ratio or min(max(held, low), high)) * width, (self.floor or 0.0) * height.height), max(high, self.ratio or 0.0) * width)
                slant = self.angle if self.angle is not None else measure.moments(model.contours).angle if model and model.contours else 0.0
                span = self.length * source.upm if self.length is not None else 0.3 * height.height
                stem, head, teeth = (source.drawn(name).contours if name in source.named else () for name in (source.key(script.stem), source.key(script.head), source.key(script.teeth, Form.MEDI)))
                dot = measures.nuqta(source, script.dotted)
                targets = _targets(source, script.recipes)
                recipes = {glyph.name.text: (code, script.recipes[code]) for glyph, code in targets if glyph is not None}
                return Ok(
                    source.replaced(
                        *(changed(glyph) for glyph in source.by_role(Mark)), *(_with_contours(None, code, 0.0, rebuilt(code, script.recipes[code])) for glyph, code in targets if glyph is None)
                    )
                )
            case Result(error=faults):
                return Error(faults)


class QuranicMarks(measures.Rule, frozen=True, kw_only=True):
    """Small letter marks the face lacks as miniatures of the face's own text within a mark box, hairline signs at the vowel pen, and tanwin before the script's iqlab meems composed in `ccmp` to its vowel."""

    codes: frozenset[int]
    script: Script
    width: Em = 0.42
    height: Ratio = 0.45

    def applied(self, source: Source, /, *, original: Binary, **_: object) -> Outcome[Source]:
        """Shapes each spelled small mark on the original, maps it into the source, holds it to the mark box at the vowel pen, and adds the iqlab composition, `ABSENT` naming the stem outline a face lacks."""
        model = source.drawn(name) if (name := source.key(self.script.vowels[0])) in source.named else None
        pen = measure.stroke(model.contours) if model and model.contours else 0.0
        spelled: list[tuple[Glyph | None, int, Option[str]]] = []
        for glyph, code in ((glyph, code) for glyph, code in _targets(source, self.codes) if pen and (glyph is None or not source.drawn(glyph.name.text).contours)):
            script, rest = unicodedata.name(chr(code)).partition(" SMALL ")[::2]
            match rest.split():
                case ["HIGH" | "LOW", "LIGATURE", *words]:
                    parts = " ".join(words).split(" WITH ")
                    letters = tuple(*_letter(script, part).to_list() for part in parts)
                    spelling = Some("\N{ZERO WIDTH JOINER}".join(letters)).filter(lambda _: len(letters) == len(parts))
                case ["HIGH" | "LOW", first, second]:
                    spelling = _letter(script, f"{first} {second}").or_else_with(lambda: _letter(script, first).map2(str.__add__, _letter(script, second)))
                case ["HIGH" | "LOW", *words, "INITIAL", "FORM"]:
                    spelling = _letter(script, " ".join(words)).map(lambda letter: f"{letter}\N{ZERO WIDTH JOINER}")
                case ["HIGH" | "LOW", *words, "ISOLATED", "FORM"] | ["HIGH" | "LOW", *words]:
                    spelling = _letter(script, " ".join(words))
                case _:
                    spelling = Nothing
            spelled.append((glyph, code, spelling))
        thin = tuple(
            _with_contours(glyph, code, glyph.advance, measure.restroked(contours, pen))
            for glyph, code in _targets(source, self.script.hairlines)
            if glyph is not None and (contours := source.drawn(glyph.name.text).contours) and measure.stroke(contours) < 0.85 * pen
        )
        tanwin = {
            code: vowel
            for glyph in source.by_role(Mark)
            for code in glyph.name.codes[:1]
            if code in glyph.codes and unicodedata.category(chr(code)) == "Mn" and (name := unicodedata.name(chr(code))).endswith("TAN")
            for vowel in catch(unicodedata.lookup, exception=KeyError)(name.removesuffix("TAN")).to_option().to_list()
        }
        rules = tuple(
            Chain(input=(((source.key(code),), source.key(ord(vowel))),), lookahead=((source.key(meem),),))
            for code, vowel in sorted(tanwin.items())
            for meem in self.script.iqlab
            if Side.of(meem) is Side.of(code) and meem in source.encoded and ord(vowel) in source.encoded
        )
        return measures.placement(source, original, self.script.stem).map(
            lambda held: msgspec.structs.replace(
                source.replaced(
                    *(
                        _with_contours(glyph, code, 0.0, measure.restroked(paths.transformed(contours, (factor, 0, 0, factor, -factor * box.middle[0], -factor * foot)), pen))
                        for glyph, code, contours in (
                            (glyph, code, paths.transformed(measures.text_outline(original, text), held[2])) for glyph, code, spelling in spelled for text in spelling.to_list()
                        )
                        if (box := paths.bounds(contours))
                        for foot in ((box.ymin if Side.of(code) is Side.ABOVE else box.ymax),)
                        for factor in (min(self.width * source.upm / box.width, self.height * held[1].height / box.height),)
                    ),
                    *thin,
                ),
                lookups=(*source.lookups, *((Contextual(features=frozendict(ccmp=frozendict()), order=-2, rules=rules),) if rules else ())),
            )
        )


class WordSigns(measures.Rule, frozen=True, kw_only=True):
    """Word signs of the script the face lacks, set from rows of the face's own shaped text, a sign of several rows stacked the gap apart within a block of stem heights."""

    script: Script
    stack: Em
    block: tuple[Ratio, Ratio] = (1.5, 1.6)

    def applied(self, source: Source, /, *, original: Binary, **_: object) -> Outcome[Source]:
        """Shapes each sign's rows on the original, maps them into the source, stacks them top to bottom the gap apart, each centered on the first, and places the outline at the stem's bearings, `ABSENT` naming the stem outline a face lacks."""
        (wide, tall), gap = self.block, self.stack * source.upm
        return measures.placement(source, original, self.script.stem).map(
            lambda held: source.replaced(
                *(
                    _with_contours(
                        own,
                        code,
                        factor * extent.width + 2 * bearing,
                        paths.transformed(drawn, (factor, 0.0, 0.0, factor, bearing - factor * extent.xmin, held[1].middle[1] - factor * extent.middle[1] if factor != 1.0 else 0.0)),
                    )
                    for bearing in ((held[0].advance - held[1].width) / 2,)
                    for own, code in _targets(source, self.script.signs)
                    if own is None or not source.drawn(own.name.text).contours
                    for rows in (tuple(paths.transformed(measures.text_outline(original, row), held[2]) for row in self.script.signs[code]),)
                    for boxes in (tuple(paths.bounds(row) or paths.Box(xmin=0, ymin=0, xmax=0, ymax=0) for row in rows),)
                    for drawn in (
                        paths.union(
                            *(
                                paths.transformed(row, (1.0, 0.0, 0.0, 1.0, boxes[0].middle[0] - box.middle[0], boxes[0].ymax - top - box.ymax))
                                for row, box, top in zip(rows, boxes, accumulate((box.height + gap for box in boxes), initial=0.0), strict=False)
                            )
                        ),
                    )
                    if (extent := paths.bounds(drawn))
                    for factor in ((min(1.0, wide * held[1].height / extent.width, tall * held[1].height / extent.height) if len(rows) > 1 else 1.0),)
                )
            )
        )


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


def _with_contours(glyph: Glyph | None, code: int, advance: float, contours: tuple[tuple[Point, ...], ...], /) -> Glyph:
    """Writes generated contours, preserving an existing native glyph's identity and every cmap mapping."""
    return (
        msgspec.structs.replace(glyph, codes=glyph.codes | {code}, advance=advance, contours=contours, components=())
        if glyph is not None
        else measures.made(GlyphName(stem=(code,)), advance, contours=contours)
    )


# --- [SHAPES]


def _fit(contours: tuple[tuple[Point, ...], ...], box: paths.Box, /, *, width: float | None = None, height: float | None = None) -> tuple[tuple[Point, ...], ...]:
    """Scales contours uniformly to a width or height and moves their outline onto a box's middle and foot."""
    match paths.bounds(contours):
        case None:
            return contours
        case own:
            factor = width / own.width if width is not None else (box.height if height is None else height) / own.height
            return paths.transformed(contours, Transform().translate(box.middle[0] - factor * own.middle[0], box.ymin - factor * own.ymin).scale(factor))


# --- [MINIATURES]


def _letter(script: str, name: str, /) -> Option[str]:
    """Letter of a script a Unicode name gives after `<script> LETTER`, Nothing for a name Unicode lacks."""
    return catch(unicodedata.lookup, exception=KeyError)(f"{script} LETTER {name}").to_option()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Marks", "QuranicMarks", "WordSigns"]
