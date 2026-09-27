"""Report lines every in-application run writes and the host decodes, and the setting rows a run converges into change lines."""

from collections.abc import Callable, Iterator, Mapping
from enum import auto, StrEnum
from functools import partial
import hashlib
from itertools import chain
from typing import Final

# --- [TYPES] ----------------------------------------------------------------------------


class Kind(StrEnum):
    """Report line kind its first cell names."""

    HEADER = auto()
    CHANGE = auto()
    SKIP = auto()
    MEASURE = auto()
    PLUGIN = auto()
    ERROR = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

ABSENT: Final = "absent"

# --- [MODELS] ---------------------------------------------------------------------------


class Row:
    """Setting as its label, read, target, and write of the target."""

    __slots__ = ("label", "read", "target", "write")

    def __init__[T](self, *, label: str, read: Callable[[], object], write: Callable[[T], object], target: T) -> None:
        """Bind the fields with the write applied to the target."""
        self.label, self.read, self.target, self.write = label, read, target, partial(write, target)


# --- [OPERATIONS] -----------------------------------------------------------------------


def line(kind: Kind, *cells: str) -> str:
    """Report line of one row, its kind and cells joined by tabs."""
    return "\t".join((kind, *cells))


def digest(content: bytes) -> str:
    """First twelve hex digits of the content's sha256."""
    return hashlib.sha256(content).hexdigest()[:12]


def changes(label: str, before: object, target: object) -> Iterator[str]:
    """Change line for each value a write moves off its held value, per key at every mapping depth."""
    match before, target:
        case Mapping() as held, Mapping() as wanted:
            yield from chain.from_iterable(changes(f"{label} {name}", held.get(name), value) for name, value in wanted.items())
        case _ if before != target:
            yield line(Kind.CHANGE, label, repr(before), repr(target))


def converged(row: Row, plain: Callable[[object], object]) -> Iterator[str]:
    """Change lines of the row, its target written when the host's plain form of its value differs."""
    before, target = plain(row.read()), plain(row.target)
    if before != target:
        row.write()
    return changes(row.label, before, target)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ABSENT", "Kind", "Row", "changes", "converged", "digest", "line"]
