"""Lookup records layout rules return and the feature emitter writes."""

from typing import Annotated

import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Annotated[str, msgspec.Meta(min_length=4, max_length=4, pattern=r"\A[!-~]+ *\Z")]
type _GlyphClass = tuple[str, ...]

# --- [MODELS] ---------------------------------------------------------------------------
# --- [VALUES]


class Adjustment(msgspec.Struct, frozen=True, kw_only=True):
    """GPOS value record in font units."""

    dx: int = 0
    dy: int = 0
    advance: int = 0


class Chain[T](msgspec.Struct, frozen=True, kw_only=True):
    """Chaining context with each input glyph class paired with its replacement or adjustment."""

    backtrack: tuple[_GlyphClass, ...] = ()
    input: tuple[tuple[_GlyphClass, T], ...]
    lookahead: tuple[_GlyphClass, ...] = ()


# --- [LOOKUPS]


class _Lookup(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Lookup shared by feature tags and their language systems, with native order and mark qualifiers."""

    features: frozendict[Tag, frozendict[tuple[Tag, Tag], bool]]
    order: int = 0
    flags: int = 0
    mark_attachment: frozenset[str] | None = None
    mark_filtering: frozenset[str] | None = None


class Single(_Lookup, frozen=True, kw_only=True):
    """GSUB single substitution, a variant feature tag (`ss01`) making it an alternate set."""

    mapping: frozendict[str, str]


class Ligatures(_Lookup, frozen=True, kw_only=True):
    """Ordered GSUB ligature substitutions, first matching component sequence applied."""

    rules: tuple[tuple[tuple[str, ...], str], ...]


class Contextual(_Lookup, frozen=True, kw_only=True):
    """GSUB chaining contextual substitution."""

    rules: tuple[Chain[str], ...]


class Positions(_Lookup, frozen=True, kw_only=True):
    """GPOS single adjustment per glyph."""

    values: frozendict[str, Adjustment]


class Pairs(_Lookup, frozen=True, kw_only=True):
    """Native GPOS pair placement and advance, retaining independent coordinates and lookup stages."""

    values: frozendict[tuple[str, str], Adjustment]


class Lifts(_Lookup, frozen=True, kw_only=True):
    """GPOS chaining contextual positioning, emitted in `mark` after generated attachment lookups."""

    rules: tuple[Chain[Adjustment | None], ...]


type Lookups = Single | Ligatures | Contextual | Positions | Pairs | Lifts

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "Chain", "Contextual", "Lifts", "Ligatures", "Lookups", "Pairs", "Positions", "Single", "Tag"]
