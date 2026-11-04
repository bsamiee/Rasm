"""Code points a Persian and Arabic text face maps, the ones it leaves out by reason, and the gaps between a font and both."""

lazy import unicodedata

import msgspec
lazy from expression import Option
lazy from youseedee import ucd_data

# --- [MODELS] ---------------------------------------------------------------------------


class Excluded(msgspec.Struct, frozen=True, kw_only=True):
    """Code points a face leaves unmapped with the reason they leave, presentation forms of them included."""

    codes: frozenset[int]
    reason: str


class Coverage(msgspec.Struct, frozen=True, kw_only=True):
    """Required code points, exclusions by reason, and Unicode blocks with other code points a face keeps unrequired, defaulting to the Persian and Arabic text set."""

    required: frozenset[int] = frozenset({
        *range(0x0609, 0x061C + 1),
        *range(0x0621, 0x063A + 1),
        *range(0x0640, 0x0656 + 1),
        *range(0x0660, 0x066D + 1),
        *range(0x06D6, 0x06ED + 1),
        *range(0x06F0, 0x06F9 + 1),
        *range(0x08F0, 0x08F2 + 1),
        *range(0x200C, 0x200F + 1),
        *(0x061F, 0x0670, 0x0671, 0x067E, 0x0686, 0x0698, 0x06A9, 0x06AF, 0x06C0, 0x06CC),
        *(0x00AB, 0x00BB, 0x2010, 0x2011, 0x2026, 0x2039, 0x203A, 0x2212, 0x25CC),
        *(0xFD3E, 0xFD3F, 0xFDF2, 0xFDF4, *range(0xFDFA, 0xFDFD + 1)),
    })
    excluded: tuple[Excluded, ...] = (
        Excluded(
            codes=frozenset({0x0604, 0x0657, 0x0658, 0x0674, 0x0679, 0x0688, 0x0691, 0x06AA, 0x06BA, 0x06BE, 0x06C1, 0x06C2, 0x06C3, 0x06D2, 0x06D3, 0x06D4}),
            reason="South Asian letter or sign outside Persian and Arabic",
        ),
        Excluded(codes=frozenset(range(0x0600, 0x0603 + 1)), reason="Subtending sign drawn at one width over any digit count"),
        Excluded(codes=frozenset({0x00AD}), reason="Soft hyphen, hosts hide it and draw their own hyphen at a break"),
        Excluded(codes=frozenset({0x0274}), reason="Latin small capital N the originals hang a hamza sign on"),
        Excluded(codes=frozenset({0x037E}), reason="Greek question mark, canonically U+003B and absent from normalized text"),
        Excluded(codes=frozenset({0x0000, 0x000D}), reason="Control character no host draws"),
        Excluded(codes=frozenset(range(0xE000, 0xF8FF + 1)), reason="Private Use art, re-encoded in ornament sets alone"),
    )
    blocks: frozenset[str] = frozenset({
        "Basic Latin",
        "Latin-1 Supplement",
        "Spacing Modifier Letters",
        "Arabic",
        "Arabic Supplement",
        "Arabic Extended-A",
        "General Punctuation",
        "Currency Symbols",
        "Letterlike Symbols",
        "Number Forms",
        "Mathematical Operators",
        "Geometric Shapes",
        "Arabic Presentation Forms-A",
        "Arabic Presentation Forms-B",
    })

    def exclusion(self, code: int, /) -> Option[Excluded]:
        """Returns the exclusion a code point falls under, by itself or by a code point its decomposition names."""
        named = {code, *(int(part, 16) for part in unicodedata.decomposition(chr(code)).split() if not part.startswith("<"))}
        return Option.of_optional(next((excluded for excluded in self.excluded if named & excluded.codes), None))

    def uncovered(self, encoded: frozendict[int, str], /) -> tuple[int, ...]:
        """Returns the required code points a font leaves unmapped, ascending."""
        return tuple(sorted(self.required - encoded.keys()))

    def unrecorded(self, encoded: frozendict[int, str], /) -> tuple[int, ...]:
        """Returns the mapped code points neither required, excluded, nor inside a kept block, ascending."""
        return tuple(sorted(code for code in encoded.keys() - self.required if self.exclusion(code).is_none() and ucd_data(code).get("Block") not in self.blocks))  # type: ignore[no-untyped-call]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Coverage", "Excluded"]
