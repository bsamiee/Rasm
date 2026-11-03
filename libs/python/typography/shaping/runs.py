"""Shaped runs of a compiled font through HarfBuzz, their `hb-shape` text, and the outlines they place."""

from enum import StrEnum
from functools import cache, partial
from itertools import accumulate, groupby
from math import fsum
from time import perf_counter
from typing import Final, Literal, Self
lazy from collections.abc import Iterable
lazy from pathlib import Path

import msgspec
lazy from expression import Ok
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset
lazy from uharfbuzz import Buffer, Face, Font, OTLayoutGlyphClass, shape

lazy from typography.faces.faults import Fault, Outcome, Reason
lazy from typography.faces.glyphs import Outline
lazy from typography.outlines.paths import traced, transformed

# --- [TYPES] ----------------------------------------------------------------------------

type Direction = Literal["ltr", "rtl", "ttb", "btt"]


class Shaper(StrEnum):
    """HarfBuzz shaper a run goes through, each value the name `hb_shape_full` reads."""

    OT = "ot"
    CORETEXT = "coretext"


# --- [MODELS] ---------------------------------------------------------------------------


class Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph of a shaped run at its pen position plus offsets, in font units from the run's left edge."""

    glyph: str
    cluster: int
    x: float
    y: float
    advance: float
    dx: int
    dy: int


class Shaped(msgspec.Struct, frozen=True, kw_only=True):
    """Text shaped under one language, its glyphs in visual order and the glyph list `hb-shape` prints between its brackets, the form fontspector expectations hold."""

    text: str
    language: str
    placed: tuple[Placed, ...]
    serialized: str

    @property
    def width(self) -> float:
        """Sum of glyph advances."""
        return fsum(placed.advance for placed in self.placed)

    @property
    def names(self) -> tuple[str, ...]:
        """Glyph keys in visual order."""
        return tuple(placed.glyph for placed in self.placed)

    @property
    def clusters(self) -> tuple[tuple[Placed, ...], ...]:
        """Glyphs grouped by cluster in visual order, a base with the marks HarfBuzz merged into its cluster."""
        return tuple(tuple(group) for _, group in groupby(self.placed, key=lambda placed: placed.cluster))


class Binary(msgspec.Struct, frozen=True, kw_only=True, cache_hash=True):
    """Compiled font bytes with the units per em and the glyph names HarfBuzz reads from the face, in glyph order."""

    data: bytes
    upm: int
    order: tuple[str, ...]

    @classmethod
    def of(cls, data: bytes, /) -> Outcome[Self]:
        """Reads units per em and glyph names through HarfBuzz.

        Args:
            data: TrueType or OpenType font file contents.

        Returns:
            Ok(binary), `UNREADABLE` fault for bytes HarfBuzz reads no glyph from.
        """
        return (
            Ok(Font(Face(data)))
            .filter_with(lambda font: font.face.glyph_count > 0, lambda _: Fault.of(Reason.UNREADABLE, "font binary", "HarfBuzz reads no glyph"))
            .map(lambda font: cls(data=data, upm=font.face.upem, order=tuple(map(font.glyph_to_string, range(font.face.glyph_count)))))
        )

    @classmethod
    def read(cls, path: Path, /) -> Outcome[Self]:
        """Reads a font file through `of`.

        Returns:
            Ok(binary), `UNREADABLE` fault for a file that cannot be read or bytes HarfBuzz reads no glyph from.
        """
        return catch(Path.read_bytes, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNREADABLE, str(path))).bind(cls.of)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FONTS]


_font: Final = cache(lambda binary: Font(Face(binary.data)))


@cache
def encoded(binary: Binary, /) -> frozendict[int, str]:
    """Maps each code point the font's cmap holds to the glyph key it reaches."""
    font = _font(binary)
    return frozendict({code: binary.order[font.get_nominal_glyph(code)] for code in font.face.unicodes})


@cache
def marks(binary: Binary, /) -> frozenset[str]:
    """Glyph keys the font's GDEF classes as marks."""
    face = _font(binary).face
    return frozenset(name for glyph, name in enumerate(binary.order) if face.get_layout_glyph_class(glyph) == OTLayoutGlyphClass.MARK)


# --- [SHAPING]


def shaped(binary: Binary, text: str, /, *, language: str = "", features: frozendict[str, bool] = frozendict(), shaper: Shaper = Shaper.OT, direction: Direction | None = None) -> Shaped:
    """Shapes text with script and direction guessed from its characters.

    Args:
        binary: Font to shape with.
        text: Characters of the run.
        language: BCP 47 tag selecting the font's language system, empty for its default system.
        features: Feature tags switched on or off over the whole run.
        shaper: HarfBuzz shaper to run.
        direction: Run direction in place of the guessed one.

    Returns:
        The run with each glyph at its accumulated pen position.
    """
    font, buffer = _font(binary), Buffer()
    buffer.add_utf8(text.encode())
    buffer.guess_segment_properties()
    buffer.language = language
    buffer.direction = direction or buffer.direction
    shape(font, buffer, dict(features), shapers=[shaper.value])
    advances = tuple(position.x_advance for position in buffer.glyph_positions)
    return Shaped(
        text=text,
        language=language,
        placed=tuple(
            Placed(glyph=binary.order[info.codepoint], cluster=info.cluster, x=pen + position.x_offset, y=position.y_offset, advance=advance, dx=position.x_offset, dy=position.y_offset)
            for info, position, advance, pen in zip(buffer.glyph_infos, buffer.glyph_positions, advances, accumulate(advances, initial=0), strict=False)
        ),
        serialized=buffer.serialize(font).removeprefix("[").removesuffix("]"),
    )


def timed(binary: Binary, texts: Iterable[str], /) -> float:
    """Measures seconds one pass of `shaped` over the texts takes."""
    start = perf_counter()
    for text in texts:
        shaped(binary, text)
    return perf_counter() - start


# --- [DRAWING]


@cache
def outline(binary: Binary, glyph: str, /) -> Outline:
    """Draws a glyph through HarfBuzz as quadratic contours in drawing order and direction, `()` for a glyph without ink."""
    font = _font(binary)
    return traced(partial(font.draw_glyph_with_pen, font.get_glyph_from_name(glyph)))


def drawn(binary: Binary, run: Shaped, /) -> tuple[Outline, ...]:
    """Moves each glyph outline of a run to its placed position, in visual order."""
    return tuple(transformed(outline(binary, placed.glyph), Offset(placed.x, placed.y)) for placed in run.placed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Binary", "Direction", "Placed", "Shaped", "Shaper", "drawn", "encoded", "marks", "outline", "shaped", "timed"]
