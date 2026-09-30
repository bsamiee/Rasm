"""Report lines every in-application run writes and the host decodes, the setting rows a run converges into change lines, and the single-precision form a float32 store holds."""

from collections.abc import Callable, Iterator, Mapping
import ctypes
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
    MEASUREMENT = auto()
    ERROR = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

ABSENT: Final = "absent"

# --- [MODELS] ---------------------------------------------------------------------------


class Row:
    """Setting as its label, read, target, and write of the target, a write returning text naming the refusal of a host that declined it."""

    __slots__ = ("label", "read", "target", "write")
    label: str
    read: Callable[[], object]
    target: object
    write: Callable[[], object]

    def __init__[T](self, *, label: str, read: Callable[[], object], write: Callable[[T], object], target: T) -> None:
        """Bind the fields with the write applied to the target."""
        self.label, self.read, self.target, self.write = label, read, target, partial(write, target)


class Action(Row):
    """Setting an action reaches without receiving its target, the target naming the state the action leaves."""

    __slots__ = ()

    def __init__(self, *, label: str, read: Callable[[], object], act: Callable[[], object], target: object) -> None:
        """Bind the fields with the action as the write."""
        self.label, self.read, self.target, self.write = label, read, target, act


# --- [OPERATIONS] -----------------------------------------------------------------------


def line(kind: Kind, *cells: str) -> str:
    """Report line of one row, its kind and cells joined by tabs."""
    return "\t".join((kind, *cells))


def digest(content: bytes) -> str:
    """First twelve hex digits of the content's sha256."""
    return hashlib.sha256(content).hexdigest()[:12]


def subscript(label: str, *keys: str | int) -> str:
    """Label with each key appended, a name as `["name"]` and an index as `[n]`."""
    return label + "".join(f'["{key}"]' if isinstance(key, str) else f"[{key}]" for key in keys)


def single(value: float) -> float:
    """Value at the single precision a float32 store holds it in."""
    return ctypes.c_float(value).value


def changes(label: str, before: object, target: object) -> Iterator[str]:
    """Change line for each value a write moves off its held value, per key of either mapping at every depth, a name labeled `["name"]`, an index `[n]`, and `None` spelled `absent`."""
    match before, target:
        case Mapping() as held, Mapping() as wanted:
            yield from chain.from_iterable(changes(subscript(label, name), held.get(name), wanted.get(name)) for name in dict.fromkeys((*wanted, *held)))
        case _ if before != target:
            yield line(Kind.CHANGE, label, *(ABSENT if value is None else repr(value) for value in (before, target)))


def converged(row: Row, plain: Callable[[object], object] = lambda value: value) -> Iterator[str]:
    """Change lines of the row, its target written when the plain form of its value differs, an error line naming a refused write, and the label noted on an exception its read or write raises."""
    target = plain(row.target)
    try:
        written = row.write() if (before := plain(row.read())) != target else None
    except Exception as error:
        error.add_note(row.label)
        raise
    match written:
        case str() as refused:
            return iter((line(Kind.ERROR, f"{row.label} {refused}"),))
        case _:
            return changes(row.label, before, target)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ABSENT", "Action", "Kind", "Row", "changes", "converged", "digest", "line", "single", "subscript"]
