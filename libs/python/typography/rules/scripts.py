"""The rule record base and the script facts rules read: reference letters, mark recipes, derived glyphs, language forms, and word sign texts, declared once per script."""

from enum import StrEnum
from typing import Literal
lazy import math
lazy import unicodedata

import msgspec

# --- [TYPES] ----------------------------------------------------------------------------


class Recipe(StrEnum):
    """Construction of a rebuilt mark from the face's own letters and nuqta."""

    BAR = "bar"
    DOUBLED = "doubled"
    HEAD = "head"
    HEADS = "heads"
    TEETH = "teeth"
    RING = "ring"
    STEM = "stem"
    DOTS = "dots"


# --- [MODELS] ---------------------------------------------------------------------------


class Rule(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Repair of one defect class, holding every value and code point it reads."""


class Derived(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph drawn from a model glyph: turned with its foot at a height, scaled to a share of a reference glyph's height, or its lowest small piece repeated in a row or around a circle."""

    code: int
    variant: str = ""
    model: str
    turned: bool = False
    foot: Literal["own", "base", "middle"] = "own"
    reference: int | None = None
    height: float | None = None
    repeats: int = 0
    around: int = 0
    kept: bool = True


class Script(msgspec.Struct, frozen=True, kw_only=True):
    """Code points and texts the rules read for one writing system, every set stated here and none in a rule."""

    scripts: frozenset[str]
    stem: int
    dotted: int
    head: int
    teeth: int
    kashida: int | None
    carrier: int | None
    spaces: tuple[int, ...]
    vowels: tuple[int, int]
    recipes: frozendict[int, Recipe]
    iqlab: tuple[int, ...]
    hairlines: frozenset[int]
    counters: tuple[int, ...]
    standing: frozenset[int]
    locl: frozendict[int, int]
    language: str
    rlig: frozendict[int, frozenset[int]]
    derived: tuple[Derived, ...]
    digits: tuple[str, str]
    shared: frozenset[int]
    signs: frozendict[int, tuple[str, ...]]
    heights: tuple[int, int]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _spelled(*codes: int) -> frozendict[int, tuple[str, ...]]:
    """Word signs by the rows of words their compatibility decomposition spells, as many rows as the square root of the word count rounds up to."""
    words = {code: "".join(chr(int(part, 16)) for part in unicodedata.decomposition(chr(code)).split()[1:]).split(" ") for code in codes}
    sizes = {code: math.ceil(len(held) / math.ceil(math.sqrt(len(held)))) for code, held in words.items()}
    return frozendict({code: tuple(" ".join(held[start : start + sizes[code]]) for start in range(0, len(held), sizes[code])) for code, held in words.items()})


# --- [SCRIPTS] --------------------------------------------------------------------------

ARABIC = Script(
    scripts=frozenset({"Arab"}),
    stem=0x0627,
    dotted=0x0628,
    head=0x0648,
    teeth=0x0633,
    kashida=0x0640,
    carrier=0x25CC,
    spaces=(0x0020, 0x00A0),
    vowels=(0x064E, 0x0650),
    recipes=frozendict({
        **dict.fromkeys((0x064E, 0x0650, 0x0618, 0x061A), Recipe.BAR),
        **dict.fromkeys((0x064B, 0x064D, 0x08F0, 0x08F2), Recipe.DOUBLED),
        **dict.fromkeys((0x064F, 0x0619), Recipe.HEAD),
        **dict.fromkeys((0x064C, 0x08F1), Recipe.HEADS),
        0x0651: Recipe.TEETH,
        **dict.fromkeys((0x0652, 0x06DF, 0x06E0, 0x06EA, 0x06EB, 0x06EC), Recipe.RING),
        0x0670: Recipe.STEM,
        0x06DB: Recipe.DOTS,
    }),
    iqlab=(0x06E2, 0x06ED),
    hairlines=frozenset({0x06DD, 0x06DE, 0x06E9, 0xFD3E, 0xFD3F, 0x060D, 0x060E, 0x060F}),
    counters=(0x0645, 0x0641, 0x0642, 0x0648, 0x0624, 0x0647, 0x0635, 0x0637, 0x0639, 0x0629),
    standing=frozenset({0x0627, 0x062F, 0x0630}),
    locl=frozendict({0x0643: 0x06A9, 0x064A: 0x06CC, 0x0664: 0x06F4, 0x0665: 0x06F5, 0x0666: 0x06F6}),
    language="arab/FAR ",
    rlig=frozendict({0x0644: frozenset({0x0627, 0x0622, 0x0623, 0x0625, 0x0671})}),
    derived=(
        Derived(code=0x06F7, model="uni06F8", turned=True, kept=False),
        Derived(code=0x066B, model="uni0631", reference=0x06F1, height=0.6),
        Derived(code=0x066B, variant="far", model="slash"),
        Derived(code=0x066C, model="uni060C", turned=True, foot="middle", reference=0x06F1),
        Derived(code=0x066C, variant="far", model="uni060C", turned=True, foot="base"),
        Derived(code=0x0609, model="uni066A", repeats=1),
        Derived(code=0x060A, model="uni066A", repeats=2),
        Derived(code=0x2010, model="hyphen"),
        Derived(code=0x25CC, model="uni062B", reference=0x0627, height=0.75, around=12),
    ),
    digits=("farsi", "arab"),
    shared=frozenset({0, 1, 2, 3, 7, 8, 9}),
    signs=_spelled(0xFDF2, 0xFDF4, 0xFDFA, 0xFDFB, 0xFDFC) | {0xFDFD: ("بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",), 0x060B: ("فرٰ",)},
    heights=(0x0078, 0x0048),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ARABIC", "Derived", "Recipe", "Rule", "Script"]
