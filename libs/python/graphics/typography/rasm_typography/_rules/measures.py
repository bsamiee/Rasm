"""Measurements rules share: glyphs by code point, the original's placement in source units, glyph maps, outline strips, and nuqta groups."""

# mypy: disable-error-code="no-untyped-call"

from statistics import median
lazy from collections.abc import Mapping
lazy from functools import cache
lazy import unicodedata

import msgspec
lazy from expression import Option, Result
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Transform

from rasm_typography._faces.names import _Form, _GlyphName
lazy from rasm_records import Fault, Outcome, paired, Reason
lazy from rasm_typography._faces.glyphs import _Anchor, _Component, _Glyph, _Outline, _role, Letter, Source
lazy from rasm_typography._outlines.paths import _area, _bounds, _Box, _pieces, _strip, _transformed, _union
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Rule(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Repair of one defect class, holding every value and code point it reads."""


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GLYPHS]


def _key(code: int, form: _Form = _Form.ISOL, /) -> str:
    """Returns the glyph key of a code point in a form."""
    return _GlyphName(stem=(code,), form=form).text


def _made(name: _GlyphName, advance: float, /, *, contours: _Outline = (), components: tuple[_Component, ...] = ()) -> _Glyph:
    """Returns a glyph of a name in the role its name states."""
    return _Glyph(
        name=name,
        role=_role(name),
        advance=advance,
        codes=frozenset(name.codes) if len(name.codes) == 1 and name.form is _Form.ISOL and not name.suffixes else frozenset(),
        contours=contours,
        components=components,
    )


def bounded_glyph(source: Source, code: int, /) -> Outcome[tuple[_Glyph, _Box]]:
    """Returns the isolated glyph of a code point with its outline extent, `ABSENT` naming the key the face lacks or draws blank."""
    return (
        source
        .glyph(source.key(code))
        .map(lambda glyph: source.drawn(glyph.name.text))
        .bind(lambda glyph: Option.of_optional(_bounds(glyph.contours)).map(lambda box: (glyph, box)))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, source.face.postscript, member=f"outline of {_key(code)}"))
    )


def _mapped(source: Source, matrices: Mapping[str, Transform], /) -> Source:
    """Maps glyphs and their references together, retaining native components wherever the target map is invertible."""
    inverse = cache(catch(Transform.inverse, exception=ZeroDivisionError))

    def changed(glyph: _Glyph, /) -> _Glyph:
        matrix = matrices[glyph.name.text]
        kept: list[_Component] = []
        for part in glyph.components:
            match inverse(matrices[part.name]):
                case Result(tag="ok", ok=backward):
                    kept.append(_Component(name=part.name, transform=matrix.transform(part.transform).transform(backward)))
                case Result(error=_):
                    contours = source.drawn(glyph.name.text).contours
                    kept.clear()
                    break
        else:
            contours = glyph.contours
        return msgspec.structs.replace(
            glyph,
            advance=glyph.advance * matrix.xx,
            contours=_transformed(contours, matrix),
            anchors=tuple(_Anchor(name=anchor.name, x=x, y=y) for anchor in glyph.anchors for x, y in (matrix.transformPoint((anchor.x, anchor.y)),)),
            components=tuple(kept),
        )

    return msgspec.structs.replace(source, glyphs=tuple(map(changed, source.glyphs)))


# --- [PLACEMENT]


def _placement(source: Source, original: Binary, stem: int, /) -> Outcome[tuple[_Glyph, _Box, Transform]]:
    """Acquires the source stem, its outline box and the map from its original height and foot together."""
    return paired(
        bounded_glyph(source, stem),
        Option
        .of_optional(original.encoded.get(stem))
        .bind(lambda name: Option.of_optional(original.extent(name)))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, "original", member=f"outline of {_key(stem)}")),
    ).map(lambda pair: (*pair[0], Transform(scale := pair[0][1].height / pair[1].height, 0, 0, scale, 0, pair[0][1].ymin - scale * pair[1].ymin)))


def _text_outline(original: Binary, text: str, /) -> _Outline:
    """Returns the outline of text shaped on a binary, `.notdef` glyphs left out."""
    run = original.shaped(text)
    return _union(*(contours for placed, contours in zip(run.placed, original.drawn(run), strict=True) if placed.glyph != original.order[0]))


# --- [OUTLINES]


def _band(source: Source, /) -> tuple[float, float] | None:
    """Returns the join band, the median outline extent at the joining edges of the medial forms, `None` for a face without them."""
    edges = tuple(
        box
        for own in source.glyphs
        if isinstance(own.role, Letter) and own.role.form is _Form.MEDI
        for glyph in (source.drawn(own.name.text),)
        for low, high in ((glyph.advance - source.upm / 100, glyph.advance), (0.0, source.upm / 100))
        if (box := _bounds(_strip(glyph.contours, low, high)))
    )
    return (_middle(tuple(box.ymin for box in edges)), _middle(tuple(box.ymax for box in edges))) if edges else None


def _centred(contours: _Outline, advance: float, /) -> _Outline:
    """Returns contours moved to center their outline across an advance."""
    match _bounds(contours):
        case _Box() as box:
            return _transformed(contours, box.centered(advance))
        case _:
            return contours


def _dotted(glyph: _Glyph, nuqta: float, /) -> tuple[_Outline, tuple[_Outline, ...]]:
    """Splits a letter into its body and its dots, a dot any piece of up to two and a half nuqtas, a letter with a decomposition holding none."""
    held = sorted(_pieces(glyph.contours), key=_area, reverse=True)
    match held:
        case [body, *rest] if nuqta and not any(unicodedata.decomposition(chr(code)) for code in glyph.name.codes):
            return ((*body, *(contour for piece in rest if _area(piece) > 2.5 * nuqta for contour in piece)), tuple(piece for piece in rest if _area(piece) <= 2.5 * nuqta))
        case _:
            return (glyph.contours, ())


def _nuqta(source: Source, dotted: int, /) -> _Outline:
    """Returns the face's one nuqta, the smallest piece of a one-dot letter, empty where it draws none."""
    name = source.key(dotted)
    held = _pieces(source.drawn(name).contours) if name in source.named else ()
    return min(held, key=_area) if len(held) > 1 else ()


def _middle(values: tuple[float, ...], /, default: float = 0.0) -> float:
    """Returns the median of measured values, the default where nothing was measured."""
    return median(values) if values else default


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rule", "bounded_glyph"]
