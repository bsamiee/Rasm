"""History retention settings of extension preferences, and record or snapshot histories stored on host data and stepped for undo and redo."""

from collections.abc import Callable
import datetime
from types import GenericAlias

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, paired, RecordReason
from blender.records.trees import Dated, History, Retention

# --- [MODELS] ---------------------------------------------------------------------------


class Retaining(bpy.types.PropertyGroup):
    """History capacity and merge interval of extension preferences, read as the retention record."""

    capacity: bpy.props.IntProperty(name="Capacity", description="Entries kept, newest first", min=1, max=100, default=Retention().capacity)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    interval: bpy.props.FloatProperty(name="Merge Interval", description="Seconds within which next entry replaces newest", unit="TIME_ABSOLUTE", min=0.0, max=600.0, default=Retention().interval)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def retention(self) -> Retention:
        """Retention record of the settings."""
        return Retention(capacity=self.capacity, interval=self.interval)


class ValueHistory[T: msgspec.Struct, O, U](msgspec.Struct, frozen=True, kw_only=True):
    """Record or snapshot history read and written through value and storage owners."""

    form: type[T]
    read: Callable[[O], Outcome[T]]
    write: Callable[[O, T], Outcome[U]]
    storage: Callable[[O], Struct]
    key: str

    # --- [STORAGE]

    def held(self, host: O) -> Outcome[History[T]]:
        """Returns stored history."""
        storage = self.storage(host)
        try:
            text = storage.get(self.key)
        except (TypeError, ReferenceError) as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject=f"{storage!r}[{self.key!r}]", cause=error),))
        try:
            return Ok(msgspec.json.decode(text, type=GenericAlias(History, (self.form,))) if text is not None else History[T]())
        except (msgspec.DecodeError, TypeError) as error:
            return Error((Fault(reason=RecordReason.OUTSIDE, subject=f"{storage!r}[{self.key!r}]", member=str(error), cause=str(text)),))

    def kept(self, host: O, history: History[T]) -> Outcome[None]:
        """Stores history."""
        storage = self.storage(host)
        try:
            storage[self.key] = msgspec.json.encode(history).decode()
        except (TypeError, ReferenceError) as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject=f"{storage!r}[{self.key!r}]", cause=error),))
        return Ok(None)

    # --- [EDITS]

    def replaced[V](self, host: O, write: Callable[[], Outcome[Option[V]]], moment: datetime.datetime, retention: Retention) -> Outcome[Option[V]]:
        """Runs edit, adding the prior value when it reports a change."""

        def recorded(record: T, history: History[T]) -> Outcome[Option[V]]:
            return write().bind(lambda change: change.map(lambda _: self.kept(host, history.add(Dated(moment=moment, value=record), retention)).map(lambda _: change)).default_value(Ok(change)))

        return paired(self.read(host), self.held(host)).bind(lambda pair: recorded(*pair))

    # --- [NAVIGATION]

    def stepped(self, host: O, offset: int, moment: datetime.datetime, retention: Retention) -> Outcome[Option[U]]:
        """Undoes for positive offset, redoes for negative offset, a refused write keeping both branches with the current value across from the target."""

        def navigated(record: T, history: History[T]) -> Outcome[Option[U]]:
            def moved(target: T, arrived: History[T], parked: History[T]) -> Outcome[Option[U]]:
                answer = self.write(host, target)
                return paired(answer, self.kept(host, answer.map(lambda _: arrived).default_value(parked))).map(lambda pair: Some(pair[0]))

            return history.step(offset, Dated(moment=moment, value=record), retention).map(lambda found: moved(*found)).default_value(Ok(Nothing))

        return paired(self.read(host), self.held(host)).bind(lambda pair: navigated(*pair))

    def removed(self, host: O, offset: int) -> Outcome[Option[T]]:
        """Removes value at signed one-based offset, `Nothing` past either end, leaving the live value unchanged."""
        return self.held(host).bind(lambda history: history.at(offset).map(lambda entry: self.kept(host, history.remove(entry.value)).map(lambda _: Some(entry.value))).default_value(Ok(Nothing)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Retaining", "ValueHistory"]
