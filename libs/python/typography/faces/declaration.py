"""Font set declarations: families, faces, genres, treatments, value overrides, and ornament sets."""

from collections import Counter
from enum import StrEnum
from functools import partialmethod
from pathlib import Path
from typing import Annotated, Literal
lazy from functools import partial
lazy import math
lazy import unicodedata

from expression.extra.result import catch
import msgspec

from typography.faces.coverage import Coverage
lazy from typography.faces.faults import Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Name = Annotated[str, msgspec.Meta(min_length=1)]
type _Style = Annotated[str, msgspec.Meta(pattern=r"^[A-Z][A-Za-z]*(?: [A-Z][A-Za-z]*)*$")]
type _Code = Annotated[int, msgspec.Meta(ge=0, le=0x10FFFF)]
type Em = Annotated[float, msgspec.Meta(ge=0.0, le=4.0)]
type Ratio = Annotated[float, msgspec.Meta(gt=0.0)]
type Band = tuple[Em, Em]


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
# --- [SCRIPTS]


class Derived(msgspec.Struct, frozen=True, kw_only=True):
    """Glyph drawn from a model glyph: turned with its foot at a height, scaled to a share of a reference glyph's height, or its lowest small piece repeated in a row or around a circle."""

    code: _Code
    variant: str = ""
    model: str
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
    language: str
    rlig: frozendict[_Code, frozenset[_Code]]
    derived: tuple[Derived, ...]
    digits: tuple[str, str]
    shared: frozenset[int]
    signs: frozendict[_Code, tuple[str, ...]]

    @staticmethod
    def spelled(*codes: int) -> frozendict[int, tuple[str, ...]]:
        """Returns word signs by the rows of words their compatibility decomposition spells, as many rows as the square root of the word count rounds up to."""
        words = {code: "".join(chr(int(part, 16)) for part in unicodedata.decomposition(chr(code)).split()[1:]).split(" ") for code in codes}
        sizes = {code: math.ceil(len(held) / math.ceil(math.sqrt(len(held)))) for code, held in words.items()}
        return frozendict({code: tuple(" ".join(held[start : start + sizes[code]]) for start in range(0, len(held), sizes[code])) for code, held in words.items()})


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
    marks_drawn: bool | None = None
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

    weight: Annotated[int, msgspec.Meta(ge=100, le=1000)] = 400
    width: Annotated[int, msgspec.Meta(ge=1, le=9)] = 5
    treatments: tuple[Treatments, ...] = ()
    values: Values = Values()
    genre: Genres = TextNaskh()


class OrnamentFace(_Member, frozen=True, kw_only=True):
    """Ornament face re-encoded from a first Private Use code point."""

    first: _Code = 0xE000


type Faces = Face | OrnamentFace

# --- [SETS]


class _Set[F: Faces](msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Named group of faces under one original folder, each face filled with the group name and every other field both declare."""

    name: _Name
    original: str
    faces: Annotated[tuple[F, ...], msgspec.Meta(min_length=1)]
    notes: str = ""

    def __post_init__(self) -> None:
        """Fills each face's family name and every group field outside the base set fields the face declares (`genre`)."""
        shared = {field: getattr(self, field) for field in self.__struct_fields__ if field not in _Set.__struct_fields__}
        msgspec.structs.force_setattr(
            self, "faces", tuple(msgspec.structs.replace(face, family=self.name, **{field: value for field, value in shared.items() if field in face.__struct_fields__}) for face in self.faces)
        )


class Family(_Set[Face], frozen=True, kw_only=True):
    """Text family under one genre, its faces filled with its name and genre."""

    genre: Genres


class OrnamentSet(_Set[OrnamentFace], frozen=True, kw_only=True):
    """Ornament family, its faces filled with its name."""


class FontSet(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Declared font set: families, ornament sets, the shaping corpus, and the output folder, paths relative to the declaration."""

    name: _Name
    corpus: str
    output: str
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


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Decoded font set with the folder its relative paths resolve against."""

    set: FontSet
    root: Path

    @property
    def corpus(self) -> Path:
        """Shaping corpus file."""
        return self.root / self.set.corpus

    def original(self, face: Faces, /) -> Path:
        """Returns the original binary a face reads."""
        return self.root / next(group.original for group in self.set.groups if group.name == face.family) / face.original

    def _output(self, folder: str, suffix: str, face: Faces, /) -> Path:
        """Returns a face's output file under a folder, an empty folder name selecting the family folder."""
        return self.root / self.set.output / (folder or face.family.replace(" ", "")) / f"{face.stem}{suffix}"

    source = partialmethod(_output, "source", ".ufo")
    binary = partialmethod(_output, "", ".ttf")
    check = partialmethod(_output, "check", ".json")
    approved = partialmethod(_output, "approved", ".json")


# --- [OPERATIONS] -----------------------------------------------------------------------


@catch(exception=msgspec.DecodeError)
def _decoded(raw: bytes, /) -> FontSet:
    return msgspec.toml.decode(raw, type=FontSet)


def declared(path: Path, /) -> Outcome[Declared]:
    """Reads and decodes a set declaration, `UNREADABLE` for a failed read and `INVALID` for a refused decode."""
    return (
        catch(Path.read_bytes, exception=OSError)(path)
        .map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))
        .bind(lambda raw: _decoded(raw).map_error(partial(Fault.of, Reason.INVALID, str(path))))
        .map(lambda font_set: Declared(set=font_set, root=path.parent))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
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
