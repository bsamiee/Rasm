"""Stage marks and queue progress record."""

import datetime
from enum import IntEnum
from fractions import Fraction
from pathlib import Path
import threading
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import NonNegative
from blender.records.identity import ContentHash
from blender.records.stores import RecordFile
from blender.rendering.batch.queue import RenderQueue

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Liveness | Ending


class Liveness(IntEnum):
    """State of unfinished run."""

    ACTIVE = 1
    STOPPED = 2


class Ending(IntEnum):
    """State of finished run."""

    SUCCEEDED = 1
    FAILED = 2
    CANCELLED = 3


# --- [MODELS] ---------------------------------------------------------------------------


class Heartbeat(msgspec.Struct, frozen=True, kw_only=True):
    """Record touch interval and stop threshold."""

    interval: Annotated[float, msgspec.Meta(gt=0.0, le=threading.TIMEOUT_MAX, description="Seconds between progress record touches")] = 3.0
    misses: Annotated[int, msgspec.Meta(ge=2, description="Missed heartbeats before run reads as stopped")] = 10


class StageMark(msgspec.Struct, frozen=True, kw_only=True):
    """Measured seconds of one finished step."""

    part: Annotated[int, msgspec.Meta(ge=0)]
    item: int
    seconds: NonNegative


class Progress(msgspec.Struct, frozen=True, kw_only=True):
    """Progress record of one queue run."""

    queue: ContentHash
    process: Annotated[int, msgspec.Meta(ge=1)]
    marks: tuple[StageMark, ...] = ()
    ending: Ending | None = None
    pending: tuple[tuple[int, int], ...] = ()

    @classmethod
    def resumed(cls, found: Option[tuple[Self, datetime.datetime]], queue: RenderQueue, process: int) -> Self:
        """Starts run, resuming the observed record of the same queue that did not succeed."""
        key = queue.key
        return (
            found
            .map(lambda observed: observed[0])
            .filter(lambda held: held.queue == key and held.ending is not Ending.SUCCEEDED)
            .map(lambda held: msgspec.structs.replace(held, process=process, ending=None))
            .default_with(lambda: cls(queue=key, process=process))
        )

    def remaining(self, queue: RenderQueue) -> tuple[tuple[int, int], ...]:
        """Returns renders no mark covers."""
        marked = {(mark.part, mark.item) for mark in self.marks}
        return tuple(step for step in queue.steps if step not in marked)

    def done(self, queue: RenderQueue) -> Fraction:
        """Returns share of queue renders marks cover."""
        total = len(queue.steps)
        return Fraction(total - len(self.remaining(queue)), total)

    def phase(self, beat: datetime.datetime, now: datetime.datetime, heartbeat: Heartbeat) -> Phase:
        """Returns ending when set, else liveness from heartbeat age."""
        ending: Option[Phase] = Option.of_optional(self.ending)
        return ending.default_with(lambda: Liveness.ACTIVE if (now - beat).total_seconds() < heartbeat.interval * heartbeat.misses else Liveness.STOPPED)


# --- [OPERATIONS] -----------------------------------------------------------------------


def observed(path: Path) -> Outcome[Option[tuple[Progress, datetime.datetime]]]:
    """Reads progress record with heartbeat moment, Nothing when absent."""
    return (
        catch(path.stat, exception=OSError)()
        .map(lambda stat: datetime.datetime.fromtimestamp(stat.st_mtime, datetime.UTC))
        .map(lambda beat: RecordFile(path=path, form=Progress).read().map(lambda progress: Some((progress, beat))))
        .default_with(lambda error: Ok(Nothing) if isinstance(error, FileNotFoundError) else Error((Fault(reason=RecordReason.UNREADABLE, subject=str(path), cause=error),)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ending", "Heartbeat", "Liveness", "Phase", "Progress", "StageMark", "observed"]
