"""Measurements rules share: glyphs by code point, the original's placement in source units, glyph maps, outline strips, nuqta groups, and vowels seated on a base."""

from statistics import median
lazy from collections.abc import Collection, Mapping
lazy from functools import cache

import msgspec
lazy from expression import Option, Result
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset, Transform

from rasm_typography._faces.names import Form, GlyphName
lazy from rasm_records import Fault, Outcome, paired, Reason
lazy from rasm_typography._faces.glyphs import Anchor, Component, Glyph, Joinings, Letter, Point, role, Side, Source
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


def made(joinings: Joinings, name: GlyphName, advance: float, /, *, contours: tuple[tuple[Point, ...], ...] = (), components: tuple[Component, ...] = ()) -> Glyph:
    """Returns a glyph of a name in the role its name states."""
    return Glyph(
        name=name,
        role=role(joinings, name),
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
    """Splits a letter into its body and its dots and hamzas, any piece, with the pieces inside it, of up to two and a half nuqtas that does not cross the baseline at a joining edge, none for a zero nuqta."""
    match sorted(paths.groups(glyph.contours), key=paths.area, reverse=True) if nuqta else ():
        case [body, *rest]:
            loose = tuple(piece for piece in rest if paths.area(piece) <= 2.5 * nuqta and not joining(glyph.name.form, glyph.advance, paths.bounds(piece)))
            return (tuple(contour for piece in (body, *rest) if piece not in loose for contour in piece), loose)
        case _:
            return (glyph.contours, ())


def joining(form: Form, advance: float, box: paths.Box | None, /) -> bool:
    """Whether a piece crosses the baseline at an edge where its form joins, a joining stroke of a letter drawn in pieces."""
    return box is not None and box.ymin <= 0 <= box.ymax and ((form.left and box.xmin <= 0) or (form.right and box.xmax >= advance))


def nuqta(source: Source, dotted: int | None, /) -> tuple[tuple[Point, ...], ...]:
    """Returns the face's one nuqta, the smallest piece of a one-dot letter, empty where it draws none or its letters carry no dot apart."""
    held = paths.groups(source.drawn(name).contours) if dotted is not None and (name := source.key(dotted)) in source.named else ()
    return min(held, key=paths.area) if len(held) > 1 else ()


def vowels(source: Source, recipes: Collection[int], side: Side, /) -> tuple[tuple[str, tuple[tuple[Point, ...], ...]], ...]:
    """Recipe marks of a side the face draws, each anchor tier's marks united about their foot, with the base anchor the tier attaches to."""
    return tuple(
        (f"{side.anchor}{tier}", outline)
        for tier in ("", "wide")
        for outline in (
            paths.union(
                *(
                    paths.transformed(glyph.contours, Offset(-anchor.x, -anchor.y))
                    for code in recipes
                    if Side.of(code) is side
                    for raw in source.glyph(source.key(code)).to_list()
                    for glyph in (source.drawn(raw.name.text),)
                    for anchor in glyph.anchor(f"_{side.anchor}{tier}").to_list()
                )
            ),
        )
        if outline
    )


def seated(glyph: Glyph, vowels: tuple[tuple[str, tuple[tuple[Point, ...], ...]], ...], /) -> tuple[tuple[Point, ...], ...]:
    """Vowel tiers set at a glyph's base anchors of their tier, on every component of a ligature."""
    return paths.union(*(paths.transformed(outline, Offset(anchor.x, anchor.y)) for name, outline in vowels for anchor in glyph.anchors if anchor.name.partition("_")[0] == name))


def middle(values: tuple[float, ...], /, default: float = 0.0) -> float:
    """Returns the median of measured values, the default where nothing was measured."""
    return median(values) if values else default


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rule", "band", "bounded_glyph", "dotted", "joining", "made", "mapped", "middle", "nuqta", "placement", "seated", "text_outline", "vowels"]
