"""Measurements rules share: glyphs by code point, the original's placement in source units, glyph maps, ink strips, and nuqta groups."""

from statistics import median
lazy import unicodedata

import msgspec
lazy from expression import Option
lazy from fontTools.misc.transform import Transform

from typography.faces.names import Form
lazy from typography.faces.faults import Fault, Outcome, paired, Reason
lazy from typography.faces.glyphs import Anchor, Component, Glyph, Letter, Outline, Source
lazy from typography.faces.names import GlyphName
lazy from typography.outlines.paths import area, bounds, Box, Matrix, pieces, strip, transformed, union
lazy from typography.shaping.runs import Binary, drawn, encoded, outline, shaped

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [GLYPHS]


def key(code: int, form: Form = Form.ISOL, /) -> str:
    """Returns the glyph key of a code point in a form."""
    return GlyphName(codes=(code,), form=form).text


def inked(source: Source, code: int, /) -> Outcome[tuple[Glyph, Box]]:
    """Returns the isolated glyph of a code point with its ink extent, `ABSENT` naming the key the face lacks or draws blank."""
    return (
        source
        .glyph(key(code))
        .bind(lambda glyph: Option.of_optional(bounds(glyph.contours)).map(lambda box: (glyph, box)))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, source.face.stem, member=f"ink of {key(code)}"))
    )


def mapped(glyph: Glyph, matrix: Matrix, /) -> Glyph:
    """Maps a glyph's contours, anchors, and advance through an affine matrix, and its component offsets through the matrix's linear part, since each component glyph takes the same map."""
    xx, xy, yx, yy, dx, dy = matrix
    return msgspec.structs.replace(
        glyph,
        advance=glyph.advance * xx,
        contours=transformed(glyph.contours, matrix),
        anchors=tuple(Anchor(name=anchor.name, x=xx * anchor.x + yx * anchor.y + dx, y=xy * anchor.x + yy * anchor.y + dy) for anchor in glyph.anchors),
        components=tuple(
            Component(name=part.name, transform=(*part.transform[:4], xx * part.transform[4] + yx * part.transform[5], xy * part.transform[4] + yy * part.transform[5])) for part in glyph.components
        ),
    )


# --- [PLACEMENT]


def placement(source: Source, original: Binary, stem: int, /) -> Outcome[Transform]:
    """Returns the map from original font units onto the source, fitted on the stem letter's ink height and foot."""
    return paired(
        inked(source, stem).map(lambda held: held[1]),
        Option.of_optional(bounds(outline(original, encoded(original).get(stem, ".notdef")))).to_result_with(lambda: Fault.of(Reason.ABSENT, "original", member=f"ink of {key(stem)}")),
    ).map(lambda boxes: Transform(scale := boxes[0].height / boxes[1].height, 0, 0, scale, 0, boxes[0].ymin - scale * boxes[1].ymin))


def text_ink(original: Binary, text: str, /) -> Outline:
    """Returns the ink of text shaped on a binary, glyphs the font lacks left out so no `.notdef` box counts as ink."""
    run = shaped(original, text)
    return union(*(contours for placed, contours in zip(run.placed, drawn(original, run), strict=True) if placed.glyph != original.order[0]))


# --- [INK]


def band(source: Source, /) -> tuple[float, float] | None:
    """Returns the join band, the median ink extent at the joining edges of the medial forms, `None` for a face without them."""
    edges = tuple(
        box
        for glyph in source.glyphs
        if isinstance(glyph.role, Letter) and glyph.role.form is Form.MEDI
        for low, high in ((glyph.advance - source.upm / 100, glyph.advance), (0.0, source.upm / 100))
        if (box := bounds(strip(glyph.contours, low, high)))
    )
    return (middle(tuple(box.ymin for box in edges)), middle(tuple(box.ymax for box in edges))) if edges else None


def centred(contours: Outline, advance: float, /) -> Outline:
    """Returns contours moved so their ink sits midway across an advance."""
    match bounds(contours):
        case Box(xmin=xmin, width=width):
            return transformed(contours, (1.0, 0.0, 0.0, 1.0, (advance - width) / 2 - xmin, 0.0))
        case _:
            return contours


def dotted(glyph: Glyph, nuqta: float, /) -> tuple[Outline, tuple[Outline, ...]]:
    """Splits a letter into its body and its dots, a dot any piece of up to two and a half nuqtas, letters a canonical decomposition names holding none."""
    held = sorted(pieces(glyph.contours), key=area, reverse=True)
    match held:
        case [body, *rest] if nuqta and not any(unicodedata.decomposition(chr(code)) for code in glyph.name.codes):
            return ((*body, *(contour for piece in rest if area(piece) > 2.5 * nuqta for contour in piece)), tuple(piece for piece in rest if area(piece) <= 2.5 * nuqta))
        case _:
            return (glyph.contours, ())


def nuqta(source: Source, dotted: int, /) -> Outline:
    """Returns the face's one nuqta, the smallest piece of a one-dot letter, empty where it draws none."""
    held = pieces(source.named[key(dotted)].contours) if key(dotted) in source.named else ()
    return min(held, key=area) if len(held) > 1 else ()


def middle(values: tuple[float, ...], /, default: float = 0.0) -> float:
    """Returns the median of measured values, the default where nothing was measured."""
    return median(values) if values else default


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["band", "centred", "dotted", "inked", "key", "mapped", "middle", "nuqta", "placement", "text_ink"]
