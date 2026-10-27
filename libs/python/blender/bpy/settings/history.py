"""History retention settings of extension preferences, and record or snapshot histories stored on host data and stepped for undo and redo."""

from collections.abc import Callable
import datetime
from functools import partial
import operator
from types import GenericAlias
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import custom_read, Struct, typed
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome, paired, RecordReason
from blender.records.trees import Dated, History, Retention

# --- [CONSTANTS] ------------------------------------------------------------------------

_SET: Final[Callable[[Struct, str, str], Result[object, TypeError | ReferenceError]]] = catch(catch(operator.setitem, exception=ReferenceError), exception=TypeError)

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

    @property
    def codec(self) -> msgspec.json.Decoder[History[T]]:
        """JSON decoder of the history under the record form."""
        decoder: msgspec.json.Decoder[History[T]] = msgspec.json.Decoder(GenericAlias(History, (self.form,)))
        return decoder

    def held(self, host: O) -> Outcome[History[T]]:
        """Returns stored history."""
        storage = self.storage(host)
        request = f"{storage!r}[{self.key!r}]"

        def decoded(text: str) -> Outcome[History[T]]:
            return catch(self.codec.decode, exception=msgspec.DecodeError)(text).map_error(lambda error: Fault.of(RecordReason.OUTSIDE, request, text, member=str(error)))

        def found(text: object) -> Outcome[History[T]]:
            return Ok(History[T]()) if text is None else typed(text, str, request).bind(decoded)

        return custom_read(storage, self.key).map_error(partial(Fault.of, BlenderReason.REFUSED, request)).bind(found)

    def kept(self, host: O, history: History[T]) -> Outcome[None]:
        """Stores history."""
        storage = self.storage(host)
        return _SET(storage, self.key, msgspec.json.encode(history).decode()).map_error(partial(Fault.of, BlenderReason.REFUSED, f"{storage!r}[{self.key!r}]")).map(lambda _: None)

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
