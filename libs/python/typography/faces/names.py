"""Glyph names derived from code points, joining form, and variant suffix."""

from enum import StrEnum
from typing import ClassVar, override, Self

import msgspec
lazy from fontTools import agl

# --- [TYPES] ----------------------------------------------------------------------------


class Form(StrEnum):
    """Positional form of a joining letter, each value its OpenType feature tag."""

    ISOL = "isol"
    INIT = "init"
    MEDI = "medi"
    FINA = "fina"

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


class GlyphName(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph name as code points per component with form and variant, or a raw base name for an unencoded glyph, `LIMIT` the length the `post` table and the AGL specification allow."""

    LIMIT: ClassVar[int] = 63
    codes: tuple[int, ...] = ()
    form: Form = Form.ISOL
    variant: str = ""
    base: str | None = None

    def __post_init__(self) -> None:
        """Refuses a name holding both code points and a base name, or neither.

        Raises:
            ValueError: If code points and base name are both present or both absent.
        """
        if bool(self.codes) == (self.base is not None):
            raise ValueError(f"{self!r} holds code points and a base name, or neither")

    @override
    def __str__(self) -> str:
        """Returns the rendered name."""
        return self.text

    @property
    def text(self) -> str:
        """Rendered name: AGL names below U+0600, `uniXXXX` above, components joined by `_`, suffixes after `.`."""
        stem = (
            self.base
            if self.base is not None
            else "_".join(agl.UV2AGL[code] if code < 0x0600 and code in agl.UV2AGL else f"uni{code:04X}" if code <= 0xFFFF else f"u{code:05X}" for code in self.codes)
        )
        return ".".join((stem, *(suffix for suffix in (self.form.value if self.form is not Form.ISOL else "", self.variant) if suffix)))

    @classmethod
    def parse(cls, text: str, /) -> Self:
        """Returns the name a rendered text states, its stem a base name where any component decodes to no code point."""
        stem, *suffixes = (text,) if text.startswith(".") else text.split(".")
        decoded = tuple(agl.toUnicode(part) for part in stem.split("_"))
        return cls(
            codes=tuple(map(ord, "".join(decoded))) if all(decoded) else (),
            form=next((Form(suffix) for suffix in suffixes if suffix in Form), Form.ISOL),
            variant=".".join(suffix for suffix in suffixes if suffix not in Form),
            base=None if all(decoded) else stem,
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Form", "GlyphName"]
