"""Measurements rules share: glyphs by code point, the original's placement in source units, glyph maps, outline strips, and nuqta groups."""

from statistics import median
lazy from collections.abc import Mapping
lazy from functools import cache
lazy import unicodedata

import msgspec
lazy from expression import Option, Result
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Transform

from rasm_typography._faces.names import Form, GlyphName
lazy from rasm_records import Fault, Outcome, paired, Reason
lazy from rasm_typography._faces.glyphs import Anchor, Component, Glyph, Letter, Point, role, Source
lazy from rasm_typography._outlines import paths
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Rule(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Repair of one defect class, holding every value and code point it reads."""


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GLYPHS]


def _key(code: int, form: Form = Form.ISOL, /) -> str:
    """Returns the glyph key of a code point in a form."""
    return GlyphName(stem=(code,), form=form).text


def made(name: GlyphName, advance: float, /, *, contours: tuple[tuple[Point, ...], ...] = (), components: tuple[Component, ...] = ()) -> Glyph:
    """Returns a glyph of a name in the role its name states."""
    return Glyph(
        name=name,
        role=role(name),
        advance=advance,
        codes=frozenset(name.codes) if len(name.codes) == 1 and name.form is Form.ISOL and not name.suffixes else frozenset(),
        contours=contours,
        components=components,
    )


def bounded_glyph(source: Source, code: int, /) -> Outcome[tuple[Glyph, paths.Box]]:
    """Returns the isolated glyph of a code point with its outline extent, `ABSENT` naming the key the face lacks or draws blank."""
    return (
        source
        .glyph(source.key(code))
        .map(lambda glyph: source.drawn(glyph.name.text))
        .bind(lambda glyph: Option.of_optional(paths.bounds(glyph.contours)).map(lambda box: (glyph, box)))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, source.face.postscript, member=f"outline of {_key(code)}"))
    )


def mapped(source: Source, matrices: Mapping[str, Transform], /) -> Source:
    """Maps glyphs and their references together, retaining native components wherever the target map is invertible."""
    inverse = cache(catch(Transform.inverse, exception=ZeroDivisionError))

    def changed(glyph: Glyph, /) -> Glyph:
        matrix = matrices[glyph.name.text]
        kept: list[Component] = []
        for part in glyph.components:
            match inverse(matrices[part.name]):
                case Result(tag="ok", ok=backward):
                    kept.append(Component(name=part.name, transform=matrix.transform(part.transform).transform(backward)))
                case Result(error=_):
                    contours = source.drawn(glyph.name.text).contours
                    kept.clear()
                    break
        else:
            contours = glyph.contours
        return msgspec.structs.replace(
            glyph,
            advance=glyph.advance * matrix.xx,
            contours=paths.transformed(contours, matrix),
            anchors=tuple(Anchor(name=anchor.name, x=x, y=y) for anchor in glyph.anchors for x, y in (matrix.transformPoint((anchor.x, anchor.y)),)),
            components=tuple(kept),
        )

    return msgspec.structs.replace(source, glyphs=tuple(map(changed, source.glyphs)))


# --- [PLACEMENT]


def placement(source: Source, original: Binary, stem: int, /) -> Outcome[tuple[Glyph, paths.Box, Transform]]:
    """Acquires the source stem, its outline box and the map from its original height and foot together."""
    return paired(
        bounded_glyph(source, stem),
        Option
        .of_optional(original.encoded.get(stem))
        .bind(lambda name: Option.of_optional(original.extent(name)))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, "original", member=f"outline of {_key(stem)}")),
    ).map(lambda pair: (*pair[0], Transform(scale := pair[0][1].height / pair[1].height, 0, 0, scale, 0, pair[0][1].ymin - scale * pair[1].ymin)))


def text_outline(original: Binary, text: str, /) -> tuple[tuple[Point, ...], ...]:
    """Returns the outline of text shaped on a binary, `.notdef` glyphs left out."""
    run = original.shaped(text)
    return paths.union(*(contours for placed, contours in zip(run.placed, original.drawn(run), strict=True) if placed.glyph != original.order[0]))


# --- [OUTLINES]


def band(source: Source, /) -> tuple[float, float] | None:
    """Returns the join band, the median outline extent at the joining edges of the medial forms, `None` for a face without them."""
    edges = tuple(
        box
        for own in source.glyphs
        if isinstance(own.role, Letter) and own.role.form is Form.MEDI
        for glyph in (source.drawn(own.name.text),)
        for low, high in ((glyph.advance - source.upm / 100, glyph.advance), (0.0, source.upm / 100))
        if (box := paths.bounds(paths.strip(glyph.contours, low, high)))
    )
    return (middle(tuple(box.ymin for box in edges)), middle(tuple(box.ymax for box in edges))) if edges else None


def dotted(glyph: Glyph, nuqta: float, /) -> tuple[tuple[tuple[Point, ...], ...], tuple[tuple[tuple[Point, ...], ...], ...]]:
    """Splits a letter into its body and its dots, a dot any piece of up to two and a half nuqtas, a letter with a decomposition holding none."""
    held = sorted(paths.pieces(glyph.contours), key=paths.area, reverse=True)
    match held:
        case [body, *rest] if nuqta and not any(unicodedata.decomposition(chr(code)) for code in glyph.name.codes):
            return ((*body, *(contour for piece in rest if paths.area(piece) > 2.5 * nuqta for contour in piece)), tuple(piece for piece in rest if paths.area(piece) <= 2.5 * nuqta))
        case _:
            return (glyph.contours, ())


def nuqta(source: Source, dotted: int, /) -> tuple[tuple[Point, ...], ...]:
    """Returns the face's one nuqta, the smallest piece of a one-dot letter, empty where it draws none."""
    name = source.key(dotted)
    held = paths.pieces(source.drawn(name).contours) if name in source.named else ()
    return min(held, key=paths.area) if len(held) > 1 else ()


def middle(values: tuple[float, ...], /, default: float = 0.0) -> float:
    """Returns the median of measured values, the default where nothing was measured."""
    return median(values) if values else default


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rule", "band", "bounded_glyph", "dotted", "made", "mapped", "middle", "nuqta", "placement", "text_outline"]
