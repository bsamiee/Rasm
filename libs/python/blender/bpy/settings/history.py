"""Record and snapshot undo and redo history."""

from collections.abc import Callable
import datetime
from types import GenericAlias
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.imaging.faults import Access, ImagingFileFault, paired
from blender.imaging.parameters import Presentation, Quantity

# --- [TYPES] ----------------------------------------------------------------------------

type Moment = Annotated[datetime.datetime, msgspec.Meta(tz=True)]

# --- [MODELS] ---------------------------------------------------------------------------


class Retention(msgspec.Struct, frozen=True, kw_only=True):
    """History capacity and merge interval."""

    capacity: Annotated[int, msgspec.Meta(ge=1, le=100, description="Entries kept, newest first")] = 10
    interval: Annotated[
        float, msgspec.Meta(ge=0.0, le=600.0, title="Merge Interval", description="Seconds within which next entry replaces newest"), Presentation(quantity=Some(Quantity.DURATION))
    ] = 5.0


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Past and future values nearest first, with last edit time."""

    past: tuple[tuple[Moment, T], ...] = ()
    future: tuple[tuple[Moment, T], ...] = ()
    moment: Moment | None = None


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
            return Error((ImagingFileFault(access=Access.READ, request=f"{storage!r}[{self.key!r}]", error=error),))

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
        """Runs edit, recording prior value when it reports a change."""

        def recorded(pair: tuple[T, History[T]]) -> BlenderResult[Option[V]]:
            """Records prior value on change, keeping first value of merged edits and clearing redo branch."""
            record, history = pair
            merging = bool(history.past) and not history.future and Option.of_optional(history.moment).map(lambda held: 0 <= (moment - held).total_seconds() < retention.interval).default_value(False)
            past = history.past if merging else ((moment, record), *history.past)
            return write().bind(lambda change: change.map(lambda _: self.kept(host, History(past=past[: retention.capacity], moment=moment)).map(lambda _: change)).default_value(Ok(change)))

        return paired(self.read(host), self.held(host)).bind(recorded)

    # --- [NAVIGATION]

    def stepped(self, host: O, offset: int, moment: datetime.datetime, retention: Retention) -> BlenderResult[Option[U]]:
        """Undoes for positive offset, redoes for negative offset."""

        def navigated(pair: tuple[T, History[T]]) -> BlenderResult[Option[U]]:
            """Moves cursor once write succeeds, keeping both branches on refusal."""
            record, history = pair

            def moved(target: T) -> BlenderResult[Option[U]]:
                distance, current = abs(offset), (moment, record)
                source = history.past if offset > 0 else history.future
                traversed = (*reversed(source[: distance - 1]), current)
                arrived = (
                    History(past=history.past[distance:], future=(*traversed, *history.future)[: retention.capacity])
                    if offset > 0
                    else History(past=(*traversed, *history.past)[: retention.capacity], future=history.future[distance:])
                )
                refused = (
                    History(past=history.past, future=(current, *history.future)[: retention.capacity])
                    if offset > 0
                    else History(past=(current, *history.past)[: retention.capacity], future=history.future)
                )
                answer = self.write(host, target)
                return paired(answer, self.kept(host, answer.map(lambda _: arrived).default_value(refused))).map(lambda pair: Some(pair[0]))

            return picked(history, offset).map(lambda entry: moved(entry[1])).default_value(Ok(Nothing))

        return paired(self.read(host), self.held(host)).bind(navigated)

    def removed(self, host: O, offset: int) -> BlenderResult[Option[T]]:
        """Removes value at signed one-based offset, `Nothing` past either end."""

        def dropped(history: History[T], value: T) -> BlenderResult[Option[T]]:
            """Removes selected value, leaving live value unchanged."""
            distance = abs(offset)
            return self.kept(
                host,
                msgspec.structs.replace(history, past=history.past[: distance - 1] + history.past[distance:], moment=None)
                if offset > 0
                else msgspec.structs.replace(history, future=history.future[: distance - 1] + history.future[distance:], moment=None),
            ).map(lambda _: Some(value))

        return self.held(host).bind(lambda history: picked(history, offset).map(lambda entry: dropped(history, entry[1])).default_value(Ok(Nothing)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def picked[T](history: History[T], offset: int) -> Option[tuple[datetime.datetime, T]]:
    """Returns entry at signed one-based offset, past for positive and future for negative, `Nothing` at zero or past either end."""
    source, distance = history.past if offset > 0 else history.future, abs(offset)
    return Some(source[distance - 1]) if 0 < distance <= len(source) else Nothing


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["History", "Moment", "Retention", "ValueHistory", "picked"]
