"""Lookup records every layout rule returns and one feature emitter consumes."""

from typing import Annotated

import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type _Feature = Annotated[str, msgspec.Meta(pattern=r"^[a-z0-9]{4}$")]
type _LanguageSystem = Annotated[str, msgspec.Meta(pattern=r"^[A-Za-z]{4}/[A-Za-z ]{4}$")]
type _GlyphClass = tuple[str, ...]

# --- [MODELS] ---------------------------------------------------------------------------
# --- [VALUES]


class Adjustment(msgspec.Struct, frozen=True, kw_only=True):
    """GPOS value record in font units."""

    dx: int = 0
    dy: int = 0
    advance: int = 0


class _Chain(msgspec.Struct, frozen=True, kw_only=True):
    """Chaining rule context: backtrack, input, and lookahead glyph classes."""

    backtrack: tuple[_GlyphClass, ...] = ()
    input: tuple[_GlyphClass, ...]
    lookahead: tuple[_GlyphClass, ...] = ()


class Context(_Chain, frozen=True, kw_only=True):
    """Chaining substitution rule, one replacement glyph per input position."""

    replacement: tuple[str, ...]

    def __post_init__(self) -> None:
        """Refuses a replacement count other than the input count.

        Raises:
            ValueError: If replacement and input differ in length.
        """
        if len(self.replacement) != len(self.input):
            raise ValueError(self)


class Lift(_Chain, frozen=True, kw_only=True):
    """Chaining positioning rule, one optional adjustment per input position, the first input a letter."""

    adjustments: tuple[Adjustment | None, ...]

    def __post_init__(self) -> None:
        """Refuses an adjustment count other than the input count.

        Raises:
            ValueError: If adjustments and input differ in length.
        """
        if len(self.adjustments) != len(self.input):
            raise ValueError(self)


# --- [LOOKUPS]


class _Lookup(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Lookup under a feature tag, its language systems (empty for every registered one), and its emission order."""

    feature: _Feature
    languages: tuple[_LanguageSystem, ...] = ()
    order: int = 0


class Single(_Lookup, frozen=True, kw_only=True):
    """GSUB single substitution, a variant feature tag (`ss01`) making it an alternate set."""

    mapping: frozendict[str, str]


class Ligatures(_Lookup, frozen=True, kw_only=True):
    """GSUB ligature substitution from component sequences."""

    rules: tuple[tuple[tuple[str, ...], str], ...]


class Contextual(_Lookup, frozen=True, kw_only=True):
    """GSUB chaining contextual substitution."""

    rules: tuple[Context, ...]


class Positions(_Lookup, frozen=True, kw_only=True):
    """GPOS single adjustment per glyph."""

    values: frozendict[str, Adjustment]


class Pairs(_Lookup, frozen=True, kw_only=True):
    """GPOS pair adjustment, written as kerning groups under the `kern` feature."""

    values: frozendict[tuple[str, str], Adjustment]


class Lifts(_Lookup, frozen=True, kw_only=True):
    """GPOS chaining contextual positioning, emitted in `mark` ahead of the generated mark lookups."""

    rules: tuple[Lift, ...]


type Lookups = Single | Ligatures | Contextual | Positions | Pairs | Lifts

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "Context", "Contextual", "Lift", "Lifts", "Ligatures", "Lookups", "Pairs", "Positions", "Single"]
