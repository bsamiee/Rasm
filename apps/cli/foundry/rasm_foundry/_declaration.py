"""Font set declarations: families and ornament sets of faces, each face's original file, and the folders a set's originals, approvals, and builds live in."""

from collections import Counter
from functools import cached_property, partialmethod
from pathlib import Path
from typing import Annotated

import msgspec

from rasm_typography import Face, Genres, OrnamentFace

# --- [TYPES] ----------------------------------------------------------------------------

type _Name = Annotated[str, msgspec.Meta(min_length=1)]
type _Members[F] = Annotated[tuple[F, ...], msgspec.Meta(min_length=1)]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [FACES]


class _Text(Face, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Text face of a declared family with the original file it is rebuilt from."""

    original: str


class _Ornament(OrnamentFace, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Ornament face of a declared set with the original file it is rebuilt from."""

    original: str


type Faces = _Text | _Ornament


# --- [SETS]


class _Group(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, dict=True):
    """Named group under one original folder."""

    name: _Name
    original: str
    notes: str | None = None


class _Family(_Group, frozen=True, kw_only=True):
    """Text family under one genre, each face built from its table with the family's name and genre."""

    genre: Genres
    faces: _Members[dict[str, object]]

    @cached_property
    def members(self) -> tuple[_Text, ...]:
        """Every face of the family in declaration order."""
        return msgspec.convert([row | {"family": self.name, "genre": msgspec.to_builtins(self.genre)} for row in self.faces], type=tuple[_Text, ...])


class _OrnamentSet(_Group, frozen=True, kw_only=True):
    """Ornament family, each face built from its table with the set's name."""

    faces: _Members[dict[str, object]]

    @cached_property
    def members(self) -> tuple[_Ornament, ...]:
        """Every face of the set in declaration order."""
        return msgspec.convert([row | {"family": self.name} for row in self.faces], type=tuple[_Ornament, ...])


class FontSet(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Declared font set: families, ornament sets, the shaping corpus, and the folder of approved shaping, both paths relative to the declaration."""

    name: _Name
    corpus: str
    approvals: str = "shaping"
    families: tuple[_Family, ...]
    ornaments: tuple[_OrnamentSet, ...] = ()

    def __post_init__(self) -> None:
        """Refuses a group name or PostScript name declared twice.

        Raises:
            ValueError: If a group name or PostScript name repeats.
        """
        if repeated := tuple(name for name, count in Counter((*(group.name for group in self.groups), *(face.postscript for face in self.faces))).items() if count > 1):
            raise ValueError(f"declared twice: {', '.join(repeated)}")

    @property
    def groups(self) -> tuple[_Family | _OrnamentSet, ...]:
        """Every family and ornament set in declaration order."""
        return (*self.families, *self.ornaments)

    @property
    def faces(self) -> tuple[Faces, ...]:
        """Every text and ornament face in declaration order."""
        return tuple(*group.members for group in self.groups)


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Decoded font set with the folder its relative paths resolve against and the folder its builds write to."""

    set: FontSet
    root: Path
    output: Path

    @property
    def corpus(self) -> Path:
        """Shaping corpus file."""
        return self.root / self.set.corpus

    def original(self, face: Faces, /) -> Path:
        """Returns the original binary a face reads."""
        return self.root / next(group.original for group in self.set.groups if group.name == face.family) / face.original

    def approved(self, face: Faces, /) -> Path:
        """Returns the approved shaping of a face."""
        return self.root / self.set.approvals / f"{face.postscript}.json"

    def _output(self, folder: str | None, suffix: str, face: Faces, /) -> Path:
        """Returns a face's output file under a folder, the family's folder where none is named."""
        return self.output / (face.family.replace(" ", "") if folder is None else folder) / f"{face.postscript}{suffix}"

    source = partialmethod(_output, "source", ".ufo")
    binary = partialmethod(_output, None, ".ttf")
    check = partialmethod(_output, "check", ".json")
    stamp = partialmethod(_output, None, ".json")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Declared", "Faces", "FontSet"]
