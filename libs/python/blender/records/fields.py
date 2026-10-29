"""Record field domains and names, record bases, curve points, dated histories under retention, and record lenses reading and rebuilding nested values."""

from collections.abc import Callable
import datetime
import sys
from types import MemberDescriptorType
from typing import Annotated, Final, Self

from expression import Nothing, Option, Some, try_downcast
import msgspec
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Seed = Annotated[int, msgspec.Meta(ge=0, le=SEED_BOUND)]
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
type Percent = Annotated[float, msgspec.Meta(ge=0.0, le=100.0)]
type Positive = Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=sys.float_info.max)]
type NonNegative = Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]
type AtLeastOne = Annotated[float, msgspec.Meta(ge=1.0, le=sys.float_info.max)]
type Count = Annotated[int, msgspec.Meta(ge=1)]
type Name = Annotated[str, msgspec.Meta(min_length=1)]
type Lens = tuple[object, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

SEED_BOUND: Final = 2 ** (np.finfo(np.float32).nmant + 1)

# --- [MODELS] ---------------------------------------------------------------------------
# --- [BASES]


class Knot[X: float, Y: float](msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    """Curve point with per-axis constraints."""

    x: X
    y: Y


class Frozen(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Record whose array fields, held directly or in an option, are read-only from construction."""

    def __post_init__(self) -> None:
        """Marks every array field read-only."""
        for value in msgspec.structs.astuple(self):
            match value:
                case (np.ndarray() as array) | Option(tag="some", some=np.ndarray() as array):
                    array.setflags(write=False)
                case _:
                    pass


class Case(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=True):
    """Union case record tagged by its class name."""


# --- [HISTORY]


class Retention(msgspec.Struct, frozen=True, kw_only=True):
    """History capacity in entries and the interval within which a new entry merges into the nearest past."""

    capacity: int = 10
    interval: datetime.timedelta = datetime.timedelta(seconds=5)


class Dated[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value with the moment it entered a history."""

    moment: Annotated[datetime.datetime, msgspec.Meta(tz=True)]
    value: T


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Past and future entries nearest first."""

    past: tuple[Dated[T], ...] = ()
    future: tuple[Dated[T], ...] = ()

    def add(self, entry: Dated[T], retention: Retention) -> Self:
        """Adds entry as the nearest past and drops the future, an entry within the merge interval of the nearest past merging into it, a held value leaving its place."""
        held = tuple(present for present in self.past if present.value != entry.value)
        match held:
            case (head, *rest) if not self.future and datetime.timedelta() <= entry.moment - head.moment < retention.interval:
                past = (msgspec.structs.replace(head, moment=entry.moment), *rest)
            case _:
                past = (entry, *held)
        return type(self)(past=past[: retention.capacity])

    def at(self, offset: int) -> Option[Dated[T]]:
        """Returns the entry at a signed one-based offset, past for positive and future for negative, `Nothing` at zero or past either end."""
        source, distance = self.past if offset > 0 else self.future, abs(offset)
        return Some(source[distance - 1]) if 0 < distance <= len(source) else Nothing

    def step(self, offset: int, current: Dated[T], retention: Retention) -> Option[tuple[T, Self, Self]]:
        """Returns the value at a signed one-based offset, the history arrived there with current and the entries passed held across from it, and the history a refused write leaves with both branches kept and current nearest across."""
        distance = abs(offset)
        source, other = (self.past, self.future) if offset > 0 else (self.future, self.past)

        def oriented(near: tuple[Dated[T], ...], far: tuple[Dated[T], ...]) -> tuple[tuple[Dated[T], ...], tuple[Dated[T], ...]]:
            return (near, far[: retention.capacity]) if offset > 0 else (far[: retention.capacity], near)

        (arrived_past, arrived_future), (kept_past, kept_future) = oriented(source[distance:], (*reversed(source[: distance - 1]), current, *other)), oriented(source, (current, *other))
        return self.at(offset).map(
            lambda entry: (entry.value, msgspec.structs.replace(self, past=arrived_past, future=arrived_future), msgspec.structs.replace(self, past=kept_past, future=kept_future))
        )

    def remove(self, value: T) -> Self:
        """Removes the entries holding value."""
        return type(self)(past=tuple(entry for entry in self.past if entry.value != value), future=tuple(entry for entry in self.future if entry.value != value))


# --- [OPERATIONS] -----------------------------------------------------------------------


def member[T: msgspec.Struct](kind: type[T], read: Callable[[type[T]], object]) -> object:
    """Returns a field read on a struct class through a `type[...]` receiver, the member descriptor a lens step or key part holds, which checkers type as the field's value and refuse on a generic struct's class."""
    return read(kind)


def _walked(value: object, lens: Lens) -> Option[tuple[object, Callable[[object], object]]]:
    match lens, value:
        case (), _:
            return Some((value, lambda new: new))
        case (MemberDescriptorType() as field, *rest), msgspec.Struct() if isinstance(value, field.__objclass__):
            return _walked(field.__get__(value), tuple(rest)).map(lambda found: (found[0], lambda new: msgspec.structs.replace(value, **{field.__name__: found[1](new)})))
        case (int(index), *rest), tuple() if 0 <= index < len(value):
            return _walked(value[index], tuple(rest)).map(lambda found: (found[0], lambda new: (*value[:index], found[1](new), *value[index + 1 :])))
        case _:
            return Nothing


def focused(value: object, lens: Lens) -> Option[object]:
    """Returns the value a lens of struct fields and 0-based tuple indexes reaches, `Nothing` where a field's struct misses the held value or an index falls outside its tuple."""
    return _walked(value, lens).map(lambda found: found[0])


def amended[T: msgspec.Struct](record: T, lens: Lens, change: Callable[[object], object]) -> Option[T]:
    """Returns record with the value a lens reaches replaced by its change, every enclosing struct and tuple rebuilt, `Nothing` where the lens reaches nothing."""
    return _walked(record, lens).bind(lambda found: Option.of_optional(try_downcast(type(record), found[1](change(found[0])))))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "SEED_BOUND",
    "AtLeastOne",
    "Case",
    "Count",
    "Dated",
    "Finite",
    "Frozen",
    "History",
    "Knot",
    "Lens",
    "Name",
    "NonNegative",
    "Percent",
    "Positive",
    "Retention",
    "Seed",
    "Unit",
    "amended",
    "focused",
    "member",
]
