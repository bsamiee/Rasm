"""Chronological record and snapshot undo and redo, independent of render-result browsing."""

from collections.abc import Callable
import datetime
from types import GenericAlias
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option
import msgspec

from rasm.blender.faults import BlenderEditFault, BlenderReadFault, BlenderResult
from rasm.imaging.faults import paired
from rasm.rendering.history import Retention

# --- [MODELS] ---------------------------------------------------------------------------


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Previous and next value occurrences, nearest first, and the last edit time."""

    past: tuple[tuple[Annotated[datetime.datetime, msgspec.Meta(tz=True)], T], ...] = ()
    future: tuple[tuple[Annotated[datetime.datetime, msgspec.Meta(tz=True)], T], ...] = ()
    moment: Annotated[datetime.datetime, msgspec.Meta(tz=True)] | None = None


class ValueHistory[T: msgspec.Struct, O, U](msgspec.Struct, frozen=True, kw_only=True):
    """Chronological record or snapshot navigation through its native value and storage owners."""

    form: type[T]
    read: Callable[[O], BlenderResult[T]]
    write: Callable[[O, T], BlenderResult[U]]
    storage: Callable[[O], bpy.types.bpy_struct]  # type: ignore[type-arg]  # ty: ignore[missing-type-argument]
    key: str

    # --- [STORAGE]

    def held(self, host: O) -> BlenderResult[History[T]]:
        """Returns the stored chronological history or its typed decode fault."""
        storage = self.storage(host)
        try:
            text = storage.get(self.key)
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{storage!r}[{self.key!r}]", error=error),))
        try:
            history: History[T] = msgspec.json.decode(text, type=GenericAlias(History, (self.form,))) if text is not None else History[T]()
        except (msgspec.DecodeError, TypeError) as error:
            return Error((BlenderReadFault(request=f"{storage!r}[{self.key!r}]", error=error),))
        return Ok(history)

    def kept(self, host: O, history: History[T]) -> BlenderResult[None]:
        """Stores the chronological native record or returns the native slot refusal."""
        storage = self.storage(host)
        text = msgspec.json.encode(history, order="deterministic").decode()
        try:
            storage[self.key] = text
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{storage!r}[{self.key!r}]", error=error),))
        return Ok(None)

    # --- [EDITS]

    def replaced[V](self, host: O, write: Callable[[], BlenderResult[V]], moment: datetime.datetime, retention: Retention) -> BlenderResult[V]:
        """Records an edit's preimage and returns the actual write outcome or its faults."""

        def recorded(pair: tuple[T, History[T]]) -> BlenderResult[None]:
            """Retains the first preimage of a merged edit and clears the abandoned redo branch."""
            record, history = pair
            merging = (
                bool(history.past)
                and not history.future
                and Option
                .of_optional(history.moment)
                .map(lambda held: 0 <= (moment.astimezone(datetime.UTC) - held.astimezone(datetime.UTC)).total_seconds() < retention.interval)
                .default_value(False)
            )
            past = history.past if merging else ((moment, record), *history.past)
            return self.kept(host, History(past=past[: retention.capacity], moment=moment))

        return paired(self.read(host), self.held(host)).bind(recorded).bind(lambda _: write())

    # --- [NAVIGATION]

    def stepped(self, host: O, offset: int, moment: datetime.datetime, retention: Retention) -> BlenderResult[Option[U]]:
        """Undoes a positive count or redoes a negative count, returning read or write faults."""

        def landed(pair: tuple[T, History[T]]) -> BlenderResult[Option[U]]:
            """Moves on success and retains both recovery branches after a partial refusal."""
            record, history = pair
            source = history.past if offset > 0 else history.future
            if not (distance := abs(offset)) or distance > len(source):
                return Ok(Nothing)
            _, target = source[distance - 1]
            current = (moment, record)
            traversed = (*reversed(source[: distance - 1]), current)
            moved = (
                History(past=history.past[distance:], future=(*traversed, *history.future)[: retention.capacity])
                if offset > 0
                else History(past=(*traversed, *history.past)[: retention.capacity], future=history.future[distance:])
            )
            failed = (
                History(past=history.past, future=(current, *history.future)[: retention.capacity])
                if offset > 0
                else History(past=(current, *history.past)[: retention.capacity], future=history.future)
            )
            answer = self.write(host, target)
            return paired(answer, self.kept(host, answer.map(lambda _: moved).default_value(failed))).map(lambda _: answer.to_option())

        return paired(self.read(host), self.held(host)).bind(landed)

    def removed(self, host: O, offset: int) -> BlenderResult[None]:
        """Removes one past or future occurrence by its signed one-based cursor offset."""

        def removed(history: History[T]) -> BlenderResult[None]:
            """Removes the selected occurrence without altering the live value."""
            source = history.past if offset > 0 else history.future
            if not (distance := abs(offset)) or distance > len(source):
                return Ok(None)
            return self.kept(
                host,
                msgspec.structs.replace(history, past=history.past[: distance - 1] + history.past[distance:], moment=None)
                if offset > 0
                else msgspec.structs.replace(history, future=history.future[: distance - 1] + history.future[distance:], moment=None),
            )

        return self.held(host).bind(removed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["History", "ValueHistory"]
