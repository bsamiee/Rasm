"""Value histories under retention: undo and redo of records and snapshots, and any history of entries newest first."""

from collections.abc import Callable
import datetime
from types import GenericAlias
from typing import Annotated, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.records.faults import DomainFault, paired
from blender.records.fields import Presentation, Quantity

# --- [TYPES] ----------------------------------------------------------------------------

type Moment = Annotated[datetime.datetime, msgspec.Meta(tz=True)]

# --- [MODELS] ---------------------------------------------------------------------------


class Retention(msgspec.Struct, frozen=True, kw_only=True):
    """History capacity and merge interval."""

    capacity: Annotated[int, msgspec.Meta(ge=1, le=100, description="Entries kept, newest first")] = 10
    interval: Annotated[
        float, msgspec.Meta(ge=0.0, le=600.0, title="Merge Interval", description="Seconds within which next entry replaces newest"), Presentation(quantity=Some(Quantity.DURATION))
    ] = 5.0


class Dated[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value with the moment it entered a history."""

    moment: Moment
    value: T


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Past and future entries nearest first."""

    past: tuple[Dated[T], ...] = ()
    future: tuple[Dated[T], ...] = ()

    def add(self, entry: Dated[T], retention: Retention) -> Self:
        """Adds entry as the nearest past and drops the future, an entry within the merge interval of the nearest past merging into it, a held value leaving its place."""
        held = tuple(present for present in self.past if present.value != entry.value)
        match held:
            case (head, *rest) if not self.future and 0 <= (entry.moment - head.moment).total_seconds() < retention.interval:
                past = (msgspec.structs.replace(head, moment=entry.moment), *rest)
            case _:
                past = (entry, *held)
        return type(self)(past=past[: retention.capacity])

    def at(self, offset: int) -> Option[Dated[T]]:
        """Returns the entry at a signed one-based offset, past for positive and future for negative, `Nothing` at zero or past either end."""
        source, distance = self.past if offset > 0 else self.future, abs(offset)
        return Some(source[distance - 1]) if 0 < distance <= len(source) else Nothing

    def step(self, offset: int, current: Dated[T], retention: Retention) -> Option[tuple[T, Self]]:
        """Returns the value at a signed one-based offset with the history arrived there, current and the entries passed held across from it."""
        distance = abs(offset)
        source, other = (self.past, self.future) if offset > 0 else (self.future, self.past)
        moved, across = source[distance:], (*reversed(source[: distance - 1]), current, *other)[: retention.capacity]
        return self.at(offset).map(lambda entry: (entry.value, type(self)(past=moved, future=across) if offset > 0 else type(self)(past=across, future=moved)))

    def remove(self, value: T) -> Self:
        """Removes the entries holding value."""
        return type(self)(past=tuple(entry for entry in self.past if entry.value != value), future=tuple(entry for entry in self.future if entry.value != value))


class ValueHistory[T: msgspec.Struct, O, U](msgspec.Struct, frozen=True, kw_only=True):
    """Record or snapshot history read and written through value and storage owners."""

    form: type[T]
    read: Callable[[O], BlenderResult[T]]
    write: Callable[[O, T], BlenderResult[U]]
    storage: "Callable[[O], bpy.types.bpy_struct[object]]"
    key: str

    # --- [STORAGE]

    def held(self, host: O) -> BlenderResult[History[T]]:
        """Returns stored history."""
        storage = self.storage(host)
        try:
            text = storage.get(self.key)
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{storage!r}[{self.key!r}]", error=error),))
        try:
            return Ok(msgspec.json.decode(text, type=GenericAlias(History, (self.form,))) if text is not None else History[T]())
        except (msgspec.DecodeError, TypeError) as error:
            return Error((DomainFault(subject=f"{storage!r}[{self.key!r}]", value=text, accepted=str(error)),))

    def kept(self, host: O, history: History[T]) -> BlenderResult[None]:
        """Stores history."""
        storage = self.storage(host)
        try:
            storage[self.key] = msgspec.json.encode(history).decode()
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{storage!r}[{self.key!r}]", error=error),))
        return Ok(None)

    # --- [EDITS]

    def replaced[V](self, host: O, write: Callable[[], BlenderResult[Option[V]]], moment: datetime.datetime, retention: Retention) -> BlenderResult[Option[V]]:
        """Runs edit, adding the prior value when it reports a change."""

        def recorded(pair: tuple[T, History[T]]) -> BlenderResult[Option[V]]:
            record, history = pair
            return write().bind(lambda change: change.map(lambda _: self.kept(host, history.add(Dated(moment=moment, value=record), retention)).map(lambda _: change)).default_value(Ok(change)))

        return paired(self.read(host), self.held(host)).bind(recorded)

    # --- [NAVIGATION]

    def stepped(self, host: O, offset: int, moment: datetime.datetime, retention: Retention) -> BlenderResult[Option[U]]:
        """Undoes for positive offset, redoes for negative offset, a refused write keeping both branches with the current value across from the target."""

        def navigated(pair: tuple[T, History[T]]) -> BlenderResult[Option[U]]:
            record, history = pair
            current = Dated(moment=moment, value=record)
            refused = (
                History(past=history.past, future=(current, *history.future)[: retention.capacity])
                if offset > 0
                else History(past=(current, *history.past)[: retention.capacity], future=history.future)
            )

            def moved(target: T, arrived: History[T]) -> BlenderResult[Option[U]]:
                answer = self.write(host, target)
                return paired(answer, self.kept(host, answer.map(lambda _: arrived).default_value(refused))).map(lambda pair: Some(pair[0]))

            return history.step(offset, current, retention).map(lambda found: moved(*found)).default_value(Ok(Nothing))

        return paired(self.read(host), self.held(host)).bind(navigated)

    def removed(self, host: O, offset: int) -> BlenderResult[Option[T]]:
        """Removes value at signed one-based offset, `Nothing` past either end, leaving the live value unchanged."""
        return self.held(host).bind(lambda history: history.at(offset).map(lambda entry: self.kept(host, history.remove(entry.value)).map(lambda _: Some(entry.value))).default_value(Ok(Nothing)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Dated", "History", "Moment", "Retention", "ValueHistory"]
