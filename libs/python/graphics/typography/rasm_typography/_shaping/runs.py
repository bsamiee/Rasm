"""Shaped runs of a compiled font through HarfBuzz, their `hb-shape` text, and the outlines they place."""

from dataclasses import dataclass
from functools import cache, cached_property, partial
from typing import Literal, override
lazy from collections.abc import Callable, Iterable, Mapping, Sequence
lazy from graphlib import TopologicalSorter
lazy from io import BytesIO
lazy from itertools import accumulate, groupby
lazy from math import fsum
lazy from pathlib import Path
lazy from time import perf_counter

import msgspec
lazy from fontTools.misc.transform import Offset
lazy from fontTools.ttLib import getTableClass, TTFont
lazy from fontTools.ttLib.ttFont import GlyphOrder
lazy from uharfbuzz import Buffer, Face, Font, OTLayoutGlyphClass, shape  # type: ignore[import-untyped]  # ty: ignore[unresolved-import]

lazy from rasm_records import Fault, Ok, Outcome, read, Reason
lazy from rasm_typography._faces.glyphs import _Outline, _traced
lazy from rasm_typography._outlines.paths import _bounds, _Box, _transformed

# --- [TYPES] ----------------------------------------------------------------------------

type _Direction = Literal["ltr", "rtl", "ttb", "btt"]

# --- [MODELS] ---------------------------------------------------------------------------


class _Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph name and cluster with HarfBuzz advances, offsets, and placed origin in font units."""

    glyph: str
    cluster: int
    x: float
    y: float
    advance: float
    y_advance: float
    dx: int
    dy: int


class _Shaped(msgspec.Struct, frozen=True, kw_only=True):
    """Text shaped under one language, its glyphs in visual order and the glyph list `hb-shape` prints between its brackets, the form fontspector expectations hold."""

    text: str
    language: str
    placed: tuple[_Placed, ...]
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
    def clusters(self) -> tuple[tuple[_Placed, ...], ...]:
        """Glyphs grouped by cluster in visual order, a base with the marks HarfBuzz merged into its cluster."""
        return tuple(tuple(group) for _, group in groupby(self.placed, key=lambda placed: placed.cluster))


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, kw_only=True, eq=False)
class _Binary[T: bytes | TTFont = bytes]:
    """Font bytes or native tables with their HarfBuzz reader and geometry caches, released with the binary.

    Native tables compile once when opened; later edits enter shaping through a new reader.
    """

    # --- [IDENTITY]

    data: T

    @override
    def __eq__(self, other: object, /) -> bool:
        """Compares byte inputs by value and native table snapshots by reader identity."""
        return self is other or (isinstance(self.data, bytes) and isinstance(other, _Binary) and self.data == other.data)

    @override
    def __hash__(self) -> int:
        """Hashes the same byte value or native reader identity used by equality."""
        return hash(self.data) if isinstance(self.data, bytes) else object.__hash__(self)

    # --- [READS]

    @cached_property
    def face(self) -> Face:  # type: ignore[no-any-unimported]
        """Opens one native face from supplied bytes or compiled native tables."""
        if isinstance(data := self.data, TTFont):
            tables = {
                tag: data.getTableData(tag)
                for tag in TopologicalSorter({
                    tag: tuple(dependency for dependency in getTableClass(tag).dependencies if dependency in data)
                    for tag in data.keys()  # ruff: ignore[in-dict-keys]
                    if tag != GlyphOrder.__name__
                }).static_order()
            }
            face = Face.create_for_tables(lambda _face, tag, _data: tables.get(tag), None)
            face.set_get_table_tags_func(lambda _face, _data: list(tables))
            return face
        return Face(data)

    @cached_property
    def _font(self) -> Font:  # type: ignore[no-any-unimported]
        """Creates the private shaping font from its native face."""
        return Font(self.face)

    @staticmethod
    def of(data: bytes, /) -> Outcome[_Binary[bytes]]:
        """Opens font bytes through HarfBuzz.

        Args:
            data: TrueType or OpenType font file contents.

        Returns:
            Ok(binary), `UNREADABLE` fault for bytes HarfBuzz reads no glyph from.
        """
        return Ok(_Binary(data=data)).filter_with(lambda binary: binary.face.glyph_count > 0, lambda _: Fault.of(Reason.UNREADABLE, "font binary", "HarfBuzz reads no glyph"))

    @staticmethod
    def read(path: Path, /) -> Outcome[_Binary[bytes]]:
        """Reads a font file through `of`.

        Returns:
            Ok(binary), `UNREADABLE` fault for a file that cannot be read or bytes HarfBuzz reads no glyph from.
        """
        return read(path).bind(_Binary.of)

    @cached_property
    def order(self) -> tuple[str, ...]:
        """Glyph names in the native face's glyph order."""
        return tuple(map(self._font.glyph_to_string, range(self.face.glyph_count)))  # ty: ignore[unsound-return-statement]

    @cached_property
    def tables(self) -> TTFont:
        """Native table reader sharing the binary's lifetime."""
        return data if isinstance(data := self.data, TTFont) else TTFont(BytesIO(data), lazy=True)

    @cached_property
    def encoded(self) -> frozendict[int, str]:
        """Code points the native cmap maps to glyph names."""
        return frozendict({code: self.order[self._font.get_nominal_glyph(code)] for code in self.face.unicodes})

    @cached_property
    def marks(self) -> frozenset[str]:
        """Glyph names the native GDEF classes as marks."""
        return frozenset(name for glyph, name in enumerate(self.order) if self.face.get_layout_glyph_class(glyph) == OTLayoutGlyphClass.MARK)

    # --- [SHAPING]

    def shaped(
        self,
        text: str,
        /,
        *,
        language: str = "",
        features: Mapping[str, int | bool | Sequence[tuple[int, int, int | bool]]] = frozendict(),
        shapers: tuple[str, ...] = ("ot",),
        direction: _Direction | None = None,
    ) -> _Shaped:
        """Shapes text with script and direction guessed from its characters.

        Args:
            text: Characters of the run.
            language: BCP 47 tag selecting the font's language system, empty for its default system.
            features: Native feature selectors over the whole run or cluster ranges.
            shapers: Native shapers in fallback order, empty for the native default.
            direction: Run direction in place of the guessed one.

        Returns:
            The run with each glyph at its accumulated pen position.

        Notes:
            Unavailable requested shapers raise the native RuntimeError.
        """
        if not text:
            return _Shaped(text=text, language=language, placed=(), serialized="")
        font, buffer = self._font, Buffer()
        buffer.add_utf8(text.encode())
        if direction is not None:
            buffer.direction = direction
        buffer.guess_segment_properties()
        buffer.language = language
        shape(font, buffer, dict(features), shapers=list(shapers))
        positions = buffer.glyph_positions
        pens = zip(accumulate((position.x_advance for position in positions), initial=0), accumulate((position.y_advance for position in positions), initial=0), strict=False)
        return _Shaped(
            text=text,
            language=language,
            placed=tuple(
                _Placed(
                    glyph=self.order[info.codepoint],
                    cluster=info.cluster,
                    x=x + position.x_offset,
                    y=y + position.y_offset,
                    advance=position.x_advance,
                    y_advance=position.y_advance,
                    dx=position.x_offset,
                    dy=position.y_offset,
                )
                for info, position, (x, y) in zip(buffer.glyph_infos, positions, pens, strict=False)
            ),
            serialized=buffer.serialize(font).removeprefix("[").removesuffix("]"),
        )

    def timed(self, texts: Iterable[str], /) -> float:
        """Measures seconds one pass of real shaping over the texts takes."""
        start = perf_counter()
        for text in texts:
            self.shaped(text)
        return perf_counter() - start

    # --- [GEOMETRY]

    @cached_property
    def outline(self) -> Callable[[str], _Outline]:
        """Glyph drawings as quadratic contours, memoized for this binary."""
        font = self._font

        @cache
        def drawn(name: str, /) -> _Outline:
            return _traced(partial(font.draw_glyph_with_pen, font.glyph_from_string(name)))

        return drawn

    @cached_property
    def extent(self) -> Callable[[str], _Box | None]:
        """Curve extents of glyph drawings, memoized for this binary."""
        outlines = self.outline

        @cache
        def measured(glyph: str, /) -> _Box | None:
            return _bounds(outlines(glyph))

        return measured

    @cached_property
    def placed_extents(self) -> Callable[[str, str], tuple[tuple[_Placed, _Box], ...]]:
        """Shaped glyphs with their curve extents at their positions, memoized for this binary."""

        @cache
        def placed(text: str, language: str, /) -> tuple[tuple[_Placed, _Box], ...]:
            return tuple((glyph, box.shifted(glyph.x, glyph.y)) for glyph in self.shaped(text, language=language).placed if (box := self.extent(glyph.glyph)))

        return placed

    def drawn(self, run: _Shaped, /) -> tuple[_Outline, ...]:
        """Moves each glyph outline of a run to its placed position, in visual order."""
        return tuple(_transformed(self.outline(placed.glyph), Offset(placed.x, placed.y)) for placed in run.placed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
