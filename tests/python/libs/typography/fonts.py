"""Native font construction shared by typography's behavioral fixtures."""

# mypy: disable-error-code="no-untyped-call"

lazy from collections.abc import Mapping
lazy from io import BytesIO

lazy from fontTools.fontBuilder import FontBuilder
lazy from fontTools.pens.ttGlyphPen import TTGlyphPen
lazy from fontTools.ttLib.tables._g_l_y_f import Glyph

# --- [OPERATIONS] -----------------------------------------------------------------------


def rectangles(*boxes: tuple[int, int, int, int]) -> Glyph:
    """Returns a native glyph containing each supplied rectangular contour."""
    pen = TTGlyphPen(None)
    for xmin, ymin, xmax, ymax in boxes:
        pen.moveTo((xmin, ymin))
        pen.lineTo((xmin, ymax))
        pen.lineTo((xmax, ymax))
        pen.lineTo((xmax, ymin))
        pen.closePath()
    return pen.glyph()


def encoded(
    glyphs: Mapping[str, tuple[int, Glyph]],
    cmap: Mapping[int, str],
    features: str,
    /,
    *,
    family: str = "Test",
    upm: int = 1000,
    ascent: int = 900,
    descent: int = -300,
    win_ascent: int = 900,
    win_descent: int = 300,
) -> bytes:
    """Builds a TrueType original from native glyphs, mappings, features, and vertical metrics."""
    builder = FontBuilder(upm, isTTF=True)
    builder.setupGlyphOrder(list(glyphs))
    builder.setupCharacterMap(dict(cmap))
    builder.setupGlyf({name: glyph for name, (_, glyph) in glyphs.items()})
    builder.setupHorizontalMetrics({name: (advance, glyph.xMin if glyph.numberOfContours else 0) for name, (advance, glyph) in glyphs.items()})
    builder.setupHorizontalHeader(ascent=ascent, descent=descent)
    builder.setupNameTable({"familyName": family, "styleName": "Regular", "copyright": "Test copyright", "designer": "Test designer"})
    builder.setupOS2(achVendID="TEST", sTypoAscender=ascent, sTypoDescender=descent, usWinAscent=win_ascent, usWinDescent=win_descent)
    builder.setupPost()
    if features:
        builder.addOpenTypeFeatures(features)
    stream = BytesIO()
    builder.save(stream)
    return stream.getvalue()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["encoded", "rectangles"]
