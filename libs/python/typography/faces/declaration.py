"""Font set declarations: families, faces, genres, treatments, value overrides, and ornament sets."""

from collections import Counter
from functools import partialmethod
from pathlib import Path
from typing import Annotated, Literal
lazy from functools import partial

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

# --- [MODELS] ---------------------------------------------------------------------------
# --- [GENRES]


class Genre(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Face class with the coverage, bands, and values its rules measure against, `spurs` where needle spurs at the join band are cut, each case stating its class defaults."""

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
    """Heavy display Naskh, marks falling off slower than stems."""

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
        """Fills each face's family name and the fields the group shares with it beyond its own identity."""
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
        """Refuses a family name or face file stem declared twice.

        Raises:
            ValueError: If a family name or face stem repeats.
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
        """Returns a face's output file under a folder, an empty folder naming its family's."""
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
    "Ringed",
    "Ruqah",
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
