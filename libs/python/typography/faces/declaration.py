"""Font set declarations: families, faces, genres, treatments, value overrides, and ornament sets."""

from collections import Counter
from enum import auto, StrEnum
from functools import partialmethod
from pathlib import Path
from typing import Annotated, Literal
lazy from itertools import batched
lazy import math
lazy import unicodedata

import msgspec
from ufoLib2.objects.info import WidthClass

from typography.faces.coverage import Coverage
from typography.faces.layout import Tag
lazy from typography.faces.faults import loaded, Outcome

# --- [TYPES] ----------------------------------------------------------------------------

type _Name = Annotated[str, msgspec.Meta(min_length=1)]
type _Style = Annotated[str, msgspec.Meta(pattern=r"\A[A-Z][A-Za-z]*(?: [A-Z][A-Za-z]*)*\Z")]
type _Code = Annotated[int, msgspec.Meta(ge=0, le=0x10FFFF)]
type Em = Annotated[float, msgspec.Meta(ge=0.0, le=4.0)]
type Ratio = Annotated[float, msgspec.Meta(gt=0.0)]
type Band = tuple[Em, Em]
type _Members[F] = Annotated[tuple[F, ...], msgspec.Meta(min_length=1)]


class Recipe(StrEnum):
    """Construction of a rebuilt mark from the face's own letters and nuqta."""

    BAR = auto()
    DOUBLED = auto()
    HEAD = auto()
    HEADS = auto()
    TEETH = auto()
    RING = auto()
    STEM = auto()
    DOTS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

PROFILE = Path(__file__).parents[1] / "verification" / "fontspector.toml"

# --- [MODELS] ---------------------------------------------------------------------------
# --- [SCRIPTS]


class Derived(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph drawn from a model glyph: turned with its foot at a height, scaled to a share of a reference glyph's height, or its lowest small piece repeated in a row or around a circle."""

    code: _Code
    variant: str = ""
    model: _Code | _Name
    turned: bool = False
    foot: Literal["own", "base", "middle"] = "own"
    reference: _Code | None = None
    height: Ratio | None = None
    repeats: int = 0
    around: int = 0
    kept: bool = True


class Script(msgspec.Struct, frozen=True, kw_only=True):
    """Code points and texts the rules of one writing system read, stated once here and in no rule."""

    scripts: frozenset[str]
    stem: _Code
    dotted: _Code
    head: _Code
    teeth: _Code
    kashida: _Code | None
    carrier: _Code | None
    spaces: tuple[_Code, ...]
    vowels: tuple[_Code, _Code]
    recipes: frozendict[_Code, Recipe]
    iqlab: tuple[_Code, ...]
    hairlines: frozenset[_Code]
    counters: tuple[_Code, ...]
    standing: frozenset[_Code]
    locl: frozendict[_Code, _Code]
    language: tuple[Tag, Tag]
    rlig: frozendict[_Code, frozenset[_Code]]
    derived: tuple[Derived, ...]
    digits: tuple[_Code, _Code]
    shared: frozenset[int]
    signs: frozendict[_Code, tuple[str, ...]]

    @staticmethod
    def spelled(*codes: int) -> frozendict[int, tuple[str, ...]]:
        """Returns word signs by the rows of words their compatibility decomposition spells, as many rows as the square root of the word count rounds up to."""
        return frozendict({
            code: tuple(" ".join(row) for row in batched(words, math.ceil(len(words) / math.ceil(math.sqrt(len(words))))))
            for code in codes
            if (words := unicodedata.normalize("NFKD", chr(code)).split())
        })


_PERSO_ARABIC = Script(
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
    language=("arab", "FAR "),
    rlig=frozendict({0x0644: frozenset({0x0627, 0x0622, 0x0623, 0x0625, 0x0671})}),
    derived=(
        Derived(code=0x06F7, model=0x06F8, turned=True, kept=False),
        Derived(code=0x066B, model=0x0631, reference=0x06F1, height=0.6),
        Derived(code=0x066B, variant="far", model=0x002F),
        Derived(code=0x066C, model=0x060C, turned=True, foot="middle", reference=0x06F1),
        Derived(code=0x066C, variant="far", model=0x060C, turned=True, foot="base"),
        Derived(code=0x0609, model=0x066A, repeats=1),
        Derived(code=0x060A, model=0x066A, repeats=2),
        Derived(code=0x2010, model=0x002D),
        Derived(code=0x25CC, model=0x062B, reference=0x0627, height=0.75, around=12),
    ),
    digits=(0x06F0, 0x0660),
    shared=frozenset({0, 1, 2, 3, 7, 8, 9}),
    signs=Script.spelled(0xFDF2, 0xFDF4, 0xFDFA, 0xFDFB, 0xFDFC) | {0xFDFD: ("بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",), 0x060B: ("فرٰ",)},
)

# --- [GENRES]


class Genre(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Face class with the script, coverage, bands, and values its rules measure against, `spurs` set where rules cut needle spurs under the join band, each case stating its class defaults."""

    script: Script = _PERSO_ARABIC
    coverage: Coverage = Coverage()
    mark: tuple[Ratio, Ratio] = (0.48, 0.82)
    pitch: Band = (1.70, 2.04)
    alif: Em | None = 0.70
    word_ratio: Ratio = 3.4
    word_band: Band = (0.213, 0.411)
    join_overlap: Em = 11 / 1024
    nuqta_floor: Em = 1 / 24
    stack_gap: Em = 0.03
    clearance: Em = 0.03
    mark_floor: Ratio | None = None
    spurs: bool = False
    hinting: bool = False


class TextNaskh(Genre, frozen=True, kw_only=True):
    """Book Naskh read at text sizes, the one class with a mark floor."""

    mark_floor: Ratio | None = 0.05
    spurs: bool = True


class BlackNaskh(Genre, frozen=True, kw_only=True):
    """Heavy display Naskh with marks lighter against its stems."""

    mark: tuple[Ratio, Ratio] = (0.32, 0.57)
    spurs: bool = True


class Thuluth(Genre, frozen=True, kw_only=True):
    """Thuluth display hand."""

    spurs: bool = True


class Kufi(Genre, frozen=True, kw_only=True):
    """Kufi and geometric faces, kept at their drawn size."""

    mark: tuple[Ratio, Ratio] = (0.30, 0.42)
    alif: Em | None = None


class DotMatrix(Kufi, frozen=True, kw_only=True):
    """Faces drawn as dots on a matrix pitch, kept at their drawn size."""


class Ruqah(Genre, frozen=True, kw_only=True):
    """Ruqah hand with close word spacing."""

    word_ratio: Ratio = 1.88


class Diwani(Ruqah, frozen=True, kw_only=True):
    """Diwani hand with close word spacing."""


type Genres = TextNaskh | BlackNaskh | Thuluth | Kufi | DotMatrix | Ruqah | Diwani

# --- [TREATMENTS]


class Treatment(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Effect a face applies over its planned outlines."""


class Sheared(Treatment, frozen=True, kw_only=True):
    """Italic shear in degrees, positive leaning left."""

    angle: Annotated[float, msgspec.Meta(gt=-45.0, lt=45.0)]


class Ringed(Treatment, frozen=True, kw_only=True):
    """Outline face stroked inward at a ring width."""

    ring: Em


class Contour(Treatment, frozen=True, kw_only=True):
    """Contour face, a ring and a channel around a core."""

    ring: Em
    channel: Em


class Shadow(Treatment, frozen=True, kw_only=True):
    """Ringed face casting a shadow along a vector in em."""

    ring: Em
    vector: tuple[float, float]


class Segmented(Treatment, frozen=True, kw_only=True):
    """Face built of separate segments, each letter several fills."""


type Treatments = Sheared | Ringed | Contour | Shadow | Segmented

# --- [FACES]


class Values(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Per-face overrides of genre values and rule inputs, unset where the genre decides."""

    mark: Ratio | None = None
    alif: Em | None = None
    word_ratio: Ratio | None = None
    nuqta_floor: Em | None = None
    stack_gap: Em | None = None
    counter: Em | None = None
    zero: Ratio | None = None
    digit_pen: str | None = None
    digit_advance: Literal["tabular", "proportional"] | None = None
    digit_bearing: Em | None = None
    marks_drawn: bool = False
    inside_bearings: bool | None = None
    fatha_angle: float | None = None
    fatha_length: Em | None = None
    ring_open: Ratio | None = None
    rail: Ratio | None = None
    tail: Ratio | None = None
    latin: Ratio | None = None
    punctuation: Ratio | None = None
    lowest_ppem: Annotated[int, msgspec.Meta(ge=1)] | None = None


class _Member(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower, forbid_unknown_fields=True):
    """Face of a declared family or ornament set, its family name filled by its owner."""

    style: _Style
    original: str
    suffix: str = ""
    family: str = ""

    @property
    def typographic(self) -> str:
        """Typographic family name, the suffix naming a face that forms its own family (`Bannai Outline`)."""
        return f"{self.family} {self.suffix}".strip()

    @property
    def stem(self) -> str:
        """File stem of the face's builds, names joined without spaces (`BannaiOutline-Regular`)."""
        return f"{self.typographic}-{self.style}".replace(" ", "")


class Face(_Member, frozen=True, kw_only=True):
    """Text face with weight, width, treatments, and value overrides, its genre filled from its family."""

    weight: Annotated[int, msgspec.Meta(ge=1, le=1000)] = 400
    width: WidthClass = WidthClass.NORMAL
    treatments: tuple[Treatments, ...] = ()
    values: Values = Values()
    genre: Genres = TextNaskh()


class OrnamentFace(_Member, frozen=True, kw_only=True):
    """Ornament face re-encoded from a first Private Use code point."""

    first: _Code = 0xE000


type Faces = Face | OrnamentFace

# --- [SETS]


class _Set(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Named group under one original folder."""

    name: _Name
    original: str
    notes: str = ""


class Family(_Set, frozen=True, kw_only=True):
    """Text family under one genre, its faces filled with its name and genre."""

    genre: Genres
    faces: _Members[Face]

    def __post_init__(self) -> None:
        """Fills each face's family name and genre."""
        msgspec.structs.force_setattr(self, "faces", tuple(msgspec.structs.replace(face, family=self.name, genre=self.genre) for face in self.faces))


class OrnamentSet(_Set, frozen=True, kw_only=True):
    """Ornament family, its faces filled with its name."""

    faces: _Members[OrnamentFace]

    def __post_init__(self) -> None:
        """Fills each ornament's family name."""
        msgspec.structs.force_setattr(self, "faces", tuple(msgspec.structs.replace(face, family=self.name) for face in self.faces))


class FontSet(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Declared font set: families, ornament sets, and the shaping corpus, its path relative to the declaration."""

    name: _Name
    corpus: str
    families: tuple[Family, ...]
    ornaments: tuple[OrnamentSet, ...] = ()

    def __post_init__(self) -> None:
        """Refuses a group name or face stem declared twice.

        Raises:
            ValueError: If a group name or face stem repeats.
        """
        if repeated := tuple(name for name, count in Counter((*(group.name for group in self.groups), *(face.stem for face in self.faces))).items() if count > 1):
            raise ValueError(f"declared twice: {', '.join(repeated)}")

    @property
    def groups(self) -> tuple[Family | OrnamentSet, ...]:
        """Every family and ornament set in declaration order."""
        return (*self.families, *self.ornaments)

    @property
    def faces(self) -> tuple[Faces, ...]:
        """Every text and ornament face in declaration order."""
        return tuple(*group.faces for group in self.groups)


class _Shaping(msgspec.Struct, frozen=True, kw_only=True):
    test_directory: str


class _Fontspector(msgspec.Struct, frozen=True, kw_only=True):
    """Fontspector table a set reads from the library's profile: the shaping test directory, relative to the set's declaration."""

    shaping: _Shaping


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Decoded font set with the folder its relative paths resolve against, the folder its builds write to, and the folder of its approved shaping expectations."""

    set: FontSet
    root: Path
    output: Path
    approvals: Path

    @property
    def corpus(self) -> Path:
        """Shaping corpus file."""
        return self.root / self.set.corpus

    def original(self, face: Faces, /) -> Path:
        """Returns the original binary a face reads."""
        return self.root / next(group.original for group in self.set.groups if group.name == face.family) / face.original

    def approved(self, face: Faces, /) -> Path:
        """Returns the approved shaping expectations of a face."""
        return self.approvals / f"{face.stem}.json"

    def _output(self, folder: str, suffix: str, face: Faces, /) -> Path:
        """Returns a face's output file under a folder, an empty folder name selecting the family folder."""
        return self.output / (folder or face.family.replace(" ", "")) / f"{face.stem}{suffix}"

    source = partialmethod(_output, "source", ".ufo")
    binary = partialmethod(_output, "", ".ttf")
    check = partialmethod(_output, "check", ".json")
    stamp = partialmethod(_output, "", ".json")


# --- [OPERATIONS] -----------------------------------------------------------------------


def declared(path: Path, output: Path, /) -> Outcome[Declared]:
    """Reads a set declaration, its approvals in the profile's test directory under the declaration's folder and its builds under an output folder.

    Args:
        path: Set declaration TOML.
        output: Folder the set's sources, binaries, and check reports write to.

    Returns:
        Ok(declared), `UNREADABLE` for a failed read and `INVALID` for a refused decode.
    """
    return loaded(FontSet, path).bind(
        lambda font_set: loaded(_Fontspector, PROFILE).map(lambda found: Declared(set=font_set, root=path.parent, output=output, approvals=path.parent / found.shaping.test_directory))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "PROFILE",
    "Band",
    "BlackNaskh",
    "Contour",
    "Declared",
    "Derived",
    "Diwani",
    "DotMatrix",
    "Em",
    "Face",
    "Faces",
    "Family",
    "FontSet",
    "Genre",
    "Genres",
    "Kufi",
    "OrnamentFace",
    "OrnamentSet",
    "Ratio",
    "Recipe",
    "Ringed",
    "Ruqah",
    "Script",
    "Segmented",
    "Shadow",
    "Sheared",
    "TextNaskh",
    "Thuluth",
    "Treatment",
    "Treatments",
    "Values",
    "declared",
]
