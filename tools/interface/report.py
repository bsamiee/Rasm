"""Report lines every in-application run writes and the host decodes, the setting rows a run converges into change lines, and the single-precision form a float32 store holds."""

from collections.abc import Callable, Generator, Iterator, Mapping
from contextlib import contextmanager
import ctypes
from functools import partial
import hashlib
from itertools import chain
from pathlib import Path
import sys
import traceback
from typing import Final, override

from attrs import astuple, evolve, frozen

# --- [TYPES] ----------------------------------------------------------------------------

type Item = Row | Line
type Update = float | Item

# --- [CONSTANTS] ------------------------------------------------------------------------

ABSENT: Final = "absent"

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Line:
    """Report line, rendered as TSV with its class name lower-cased as the first cell and its fields after it."""

    @override
    def __str__(self) -> str:
        """TSV line of the class name and fields."""
        return "\t".join((type(self).__name__.lower(), *astuple(self)))


@frozen(slots=False)
class Header(Line):
    """Application version and settings folder a report opens with."""

    version: str
    folder: str


@frozen(slots=False)
class Change(Line):
    """Setting values before and after a write."""

    label: str
    before: str
    after: str


@frozen(slots=False)
class Skip(Line):
    """Omission of an absent, disabled, or empty add-on, plug-in, or library."""

    name: str


@frozen(slots=False)
class Measurement(Line):
    """JSON record an application measures for the host's file updates."""

    record: str


@frozen(slots=False)
class Error(Line):
    """Expected failure an application reports or a host operation returns."""

    text: str

    @override
    def __str__(self) -> str:
        """TSV line with the text's whitespace collapsed to single spaces."""
        return Line.__str__(evolve(self, text=" ".join(self.text.split())))


@frozen
class Refused:
    """Answer of a write the host declined, with the reason the host gave."""

    reason: str


class Row:
    """Setting as its label, read, target, write of the target, and the plain form its read and target compare in, a write answering `Refused` when the host declined it."""

    label: str
    read: Callable[[], object]
    target: object
    write: Callable[[], object]
    plain: Callable[[object], object]

    def __init__[T](self, *, label: str, read: Callable[[], object], write: Callable[[T], object], target: T, plain: Callable[[object], object] = lambda value: value) -> None:
        """Bind the fields with the write applied to the target."""
        self.label, self.read, self.target, self.write, self.plain = label, read, target, partial(write, target), plain


class Action(Row):
    """Setting an action reaches without receiving its target, the target naming the state the action leaves."""

    def __init__(self, *, label: str, read: Callable[[], object], act: Callable[[], object], target: object, plain: Callable[[object], object] = lambda value: value) -> None:
        """Bind the fields with the action as the write."""
        self.label, self.read, self.target, self.write, self.plain = label, read, target, act, plain


# --- [OPERATIONS] -----------------------------------------------------------------------


def digest(content: bytes) -> str:
    """First twelve hex digits of the content's sha256."""
    return hashlib.sha256(content).hexdigest()[:12]


def subscript(label: str, *keys: str | int) -> str:
    """Label with each key appended, a name as `["name"]` and an index as `[n]`."""
    return label + "".join(f'["{key}"]' if isinstance(key, str) else f"[{key}]" for key in keys)


def single(value: float) -> float:
    """Value at the single precision a float32 store holds it in."""
    return ctypes.c_float(value).value


def changes(label: str, before: object, target: object) -> Iterator[Change]:
    """Change line for each value a write moves off its held value, per key of either mapping at every depth, a name labeled `["name"]`, an index `[n]`, and `None` spelled `absent`."""
    match before, target:
        case Mapping() as held, Mapping() as wanted:
            yield from chain.from_iterable(changes(subscript(label, name), held.get(name), wanted.get(name)) for name in dict.fromkeys((*wanted, *held)))
        case _ if before != target:
            yield Change(label, *(ABSENT if value is None else repr(value) for value in (before, target)))


def converged(row: Row) -> Iterator[Change | Error]:
    """Change lines of the row, its target written when the plain form of its value differs, an error line naming a refused write, and the label noted on an exception its read or write raises."""
    target = row.plain(row.target)
    try:
        written = row.write() if (before := row.plain(row.read())) != target else None
    except Exception as error:
        error.add_note(row.label)
        raise
    match written:
        case Refused(reason=reason):
            return iter((Error(f"{row.label} {reason}"),))
        case _:
            return changes(row.label, before, target)


@contextmanager
def reported(path: str) -> Generator[list[Line]]:
    """Body a run extends, written to the path as TSV lines on exit with an error line holding the traceback of the exception the run raised."""
    body: list[Line] = []
    try:
        yield body
    finally:
        failure = () if (error := sys.exception()) is None else (Error("".join(traceback.format_exception(error))),)
        Path(path).write_text("".join(f"{line}\n" for line in (*body, *failure)), encoding="utf-8")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ABSENT", "Action", "Change", "Error", "Header", "Item", "Line", "Measurement", "Refused", "Row", "Skip", "Update", "changes", "converged", "digest", "reported", "single", "subscript"]
