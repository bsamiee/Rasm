"""Glyph names derived from code points, joining form, and variant suffixes."""

from enum import auto, StrEnum
from functools import cached_property
from typing import Annotated, ClassVar, override, Self

import msgspec
lazy from fontTools import agl

# --- [TYPES] ----------------------------------------------------------------------------


class Form(StrEnum):
    """Positional form of a joining letter, each value its OpenType feature tag."""

    ISOL = auto()
    INIT = auto()
    MEDI = auto()
    FINA = auto()

    @property
    def right(self) -> bool:
        """Whether the form joins the letter before it."""
        return self in {Form.MEDI, Form.FINA}

    @property
    def left(self) -> bool:
        """Whether the form joins the letter after it."""
        return self in {Form.INIT, Form.MEDI}

    @classmethod
    def joined(cls, *, right: bool, left: bool) -> Form:
        """Returns the form joining on the stated sides."""
        return next(form for form in cls if (form.right, form.left) == (right, left))


# --- [MODELS] ---------------------------------------------------------------------------


class GlyphName(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Glyph stem as nonempty code points or an unencoded name, with its joining form and variant suffixes, `LIMIT` the length the `post` table and AGL allow."""

    LIMIT: ClassVar[int] = 63
    stem: str | Annotated[tuple[int, ...], msgspec.Meta(min_length=1)]
    form: Form = Form.ISOL
    suffixes: tuple[str, ...] = ()

    @override
    def __str__(self) -> str:
        """Returns the rendered name."""
        return self.text

    @property
    def codes(self) -> tuple[int, ...]:
        """Unicode code points the stem names, empty for an unencoded glyph."""
        return () if isinstance(self.stem, str) else self.stem

    @cached_property
    def text(self) -> str:
        """Rendered name: AGL names below U+0600, `uniXXXX` above, components joined by `_`, suffixes after `.`."""
        stem = (
            self.stem
            if isinstance(self.stem, str)
            else "_".join(agl.UV2AGL[code] if code < 0x0600 and code in agl.UV2AGL else f"uni{code:04X}" if code <= 0xFFFF else f"u{code:05X}" for code in self.stem)
        )
        return ".".join((stem, *((self.form.value,) if self.form is not Form.ISOL else ()), *self.suffixes))

    @classmethod
    def parse(cls, text: str, /) -> Self:
        """Returns the name a rendered text states, its stem a base name where any component decodes to no code point."""
        stem, *suffixes = (text,) if text.startswith(".") else text.split(".")
        decoded = tuple(agl.toUnicode(part) for part in stem.split("_"))
        return cls(
            stem=tuple(map(ord, "".join(decoded))) if all(decoded) else stem,
            form=next((Form(suffix) for suffix in suffixes if suffix in Form), Form.ISOL),
            suffixes=tuple(suffix for suffix in suffixes if suffix not in Form),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Form", "GlyphName"]
