"""Glyph records with outlines, anchors, and roles, and the face source holding them."""

from datetime import datetime
from enum import StrEnum
from functools import cached_property
from typing import Annotated, Literal, Self
lazy import unicodedata

from expression import Option
import msgspec
lazy from youseedee import ucd_data

from typography.faces.declaration import Faces
from typography.faces.layout import Lookups
from typography.faces.names import Form, GlyphName

# --- [TYPES] ----------------------------------------------------------------------------

type Contour = tuple[Point, ...]
type Outline = tuple[Contour, ...]
type _Category = Literal["base", "ligature", "mark", "unassigned"]


class Joining(StrEnum):
    """Joining type, each value its UCD `Joining_Type` code."""

    DUAL = "D"
    RIGHT = "R"
    LEFT = "L"
    NONE = "U"
    CAUSING = "C"
    TRANSPARENT = "T"

    @classmethod
    def of(cls, code: int, /) -> Joining:
        """Returns the joining type `ArabicShaping.txt` states, transparent for unlisted marks and format controls."""
        return cls(ucd_data(code).get("Joining_Type", cls.TRANSPARENT if unicodedata.category(chr(code)) in {"Mn", "Me", "Cf"} else cls.NONE))


class Side(StrEnum):
    """Side of its base a mark attaches to."""

    ABOVE = "above"
    BELOW = "below"

    @property
    def anchor(self) -> str:
        """Base anchor name of the side, its mark anchor prefixed by `_`."""
        return "top" if self is Side.ABOVE else "bottom"

    @classmethod
    def of(cls, code: int, /) -> Side:
        """Returns the side a mark's canonical combining class states, below for class 220 and the kasra and kasratan classes."""
        return cls.BELOW if unicodedata.combining(chr(code)) in {220, *map(unicodedata.combining, "ٍِ")} else cls.ABOVE


# --- [MODELS] ---------------------------------------------------------------------------
# --- [GEOMETRY]


class Point(msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Contour point, off-curve points quadratic controls."""

    x: float
    y: float
    on: bool = True


class Anchor(msgspec.Struct, frozen=True, kw_only=True):
    """Named attachment point in the form the ufo2ft writers read (`top`, `_top`, `top_1`, `entry`, `caret_1`)."""

    name: str
    x: float
    y: float


class Component(msgspec.Struct, frozen=True, kw_only=True):
    """Reference to another glyph under an affine transform."""

    name: str
    transform: tuple[float, float, float, float, float, float] = (1.0, 0.0, 0.0, 1.0, 0.0, 0.0)


# --- [ROLES]


class _Role(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Part a glyph plays in text."""


class Letter(_Role, frozen=True, kw_only=True):
    """Joining letter in one positional form."""

    joining: Joining
    form: Form


class Mark(_Role, frozen=True, kw_only=True):
    """Combining mark on one side of its base."""

    side: Side


class Ligature(_Role, frozen=True, kw_only=True):
    """Ligature of component glyphs."""

    components: Annotated[tuple[str, ...], msgspec.Meta(min_length=2)]


class Digit(_Role, frozen=True, kw_only=True):
    """Decimal digit of a script: `arab`, `farsi`, or `latn`."""

    script: Literal["arab", "farsi", "latn"]


class Sign(_Role, frozen=True, kw_only=True):
    """Spacing glyph no other role holds: punctuation, symbols, unjoined letters, word signs."""


class Ornament(_Role, frozen=True, kw_only=True):
    """Ornament art."""


class Space(_Role, frozen=True, kw_only=True):
    """Space separator."""


class Control(_Role, frozen=True, kw_only=True):
    """Zero-width format control."""


type Roles = Letter | Mark | Ligature | Digit | Sign | Ornament | Space | Control

# --- [GLYPHS]


class Glyph(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph with its role, advance, quadratic contours, components, and anchors."""

    name: GlyphName
    role: Roles
    advance: float
    contours: Outline = ()
    components: tuple[Component, ...] = ()
    anchors: tuple[Anchor, ...] = ()

    @property
    def codes(self) -> tuple[int, ...]:
        """Code point the glyph is mapped at, empty for forms, ligatures, variants, and unencoded glyphs."""
        return self.name.codes if len(self.name.codes) == 1 and self.name.form is Form.ISOL and not self.name.variant else ()

    @property
    def category(self) -> _Category:
        """OpenType category for `public.openTypeCategories`, spaces and controls left unassigned."""
        match self.role:
            case Mark():
                return "mark"
            case Ligature():
                return "ligature"
            case Space() | Control():
                return "unassigned"
            case Letter() | Digit() | Sign() | Ornament():
                return "base"

    def anchor(self, name: str, /) -> Option[Anchor]:
        """Returns the anchor with a name."""
        return Option.of_optional(next((anchor for anchor in self.anchors if anchor.name == name), None))


# --- [FACES]


class Info(msgspec.Struct, frozen=True, kw_only=True):
    """Record fields read from the original."""

    created: Annotated[datetime, msgspec.Meta(tz=True)]
    version: tuple[int, int]
    copyright: str
    designer: str
    vendor: str


class Metrics(msgspec.Struct, frozen=True, kw_only=True):
    """Vertical metrics in font units, descenders negative and win descent positive."""

    ascender: int
    descender: int
    win_ascent: int
    win_descent: int
    x_height: int = 0
    cap_height: int = 0
    caret_offset: int = 0
    italic_angle: float = 0.0
    lowest_ppem: int = 6


class Source(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Face source: its declaration, units per em, glyphs in glyph order, records, metrics, and lookups."""

    face: Faces
    upm: int
    glyphs: tuple[Glyph, ...]
    info: Info
    metrics: Metrics | None = None
    lookups: tuple[Lookups, ...] = ()

    @cached_property
    def named(self) -> frozendict[str, Glyph]:
        """Glyphs by key."""
        return frozendict({glyph.name.text: glyph for glyph in self.glyphs})

    @cached_property
    def encoded(self) -> frozendict[int, str]:
        """Glyph keys by the code point each is mapped at."""
        return frozendict({code: glyph.name.text for glyph in self.glyphs for code in glyph.codes})

    def glyph(self, key: str, /) -> Option[Glyph]:
        """Returns the glyph of a key."""
        return Option.of_optional(self.named.get(key))

    def by_role(self, kind: type[Roles], /) -> tuple[Glyph, ...]:
        """Returns the glyphs of one role case in glyph order."""
        return tuple(glyph for glyph in self.glyphs if isinstance(glyph.role, kind))

    def replaced(self, *glyphs: Glyph) -> Self:
        """Returns a copy with each glyph replacing the one of its key, or appended where no glyph holds it."""
        changed = frozendict({glyph.name.text: glyph for glyph in glyphs})
        return msgspec.structs.replace(self, glyphs=(*(changed.get(glyph.name.text, glyph) for glyph in self.glyphs), *(glyph for key, glyph in changed.items() if key not in self.named)))

    def without(self, *keys: str) -> Self:
        """Returns a copy without the glyphs of the keys."""
        return msgspec.structs.replace(self, glyphs=tuple(glyph for glyph in self.glyphs if glyph.name.text not in keys))


# --- [OPERATIONS] -----------------------------------------------------------------------


def role(name: GlyphName, /, *, components: tuple[str, ...] = ()) -> Roles:
    """Returns the role a glyph name states through its component count, form, and the Unicode data of its first code point."""
    head = next(iter(name.codes), 0)
    match len(name.codes), name.form, unicodedata.category(chr(head)), ucd_data(head).get("Joining_Type"):
        case count, _, _, _ if count > 1:
            return Ligature(components=components or tuple(GlyphName(codes=(code,)).text for code in name.codes))
        case 1, form, _, _ if form is not Form.ISOL:
            return Letter(joining=Joining.of(head), form=form)
        case 1, _, "Mn" | "Me", _:
            return Mark(side=Side.of(head))
        case 1, _, "Cf" | "Cc", _:
            return Control()
        case 1, _, "Zs", _:
            return Space()
        case 1, _, "Nd", _:
            return Digit(script="farsi" if (title := unicodedata.name(chr(head))).startswith("EXTENDED ARABIC-INDIC") else "arab" if title.startswith("ARABIC-INDIC") else "latn")
        case 1, _, "Lo" | "Lm", str(joining):
            return Letter(joining=Joining(joining), form=Form.ISOL)
        case _:
            return Sign()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Anchor",
    "Component",
    "Contour",
    "Control",
    "Digit",
    "Glyph",
    "Info",
    "Joining",
    "Letter",
    "Ligature",
    "Mark",
    "Metrics",
    "Ornament",
    "Outline",
    "Point",
    "Roles",
    "Side",
    "Sign",
    "Source",
    "Space",
    "role",
]
