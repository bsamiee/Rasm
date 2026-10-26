"""Stage marks and queue progress record."""

import datetime
from fractions import Fraction
from pathlib import Path
import threading
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.records.faults import FileFault
from blender.records.fields import Choice, NonNegative, Presentation, Quantity
from blender.records.identity import ContentHash
from blender.records.stores import RecordFile
from blender.rendering.batch.queue import RenderQueue
from blender.rendering.faults import RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Liveness | Ending


class Liveness(Choice):
    """State of unfinished run."""

    ACTIVE = 1, "Rendering"
    STOPPED = 2


class Ending(Choice):
    """State of finished run."""

    SUCCEEDED = 1, "Finished"
    FAILED = 2
    CANCELLED = 3, "Canceled"


# --- [MODELS] ---------------------------------------------------------------------------


class Heartbeat(msgspec.Struct, frozen=True, kw_only=True):
    """Record touch interval and stop threshold."""

    interval: Annotated[
        float, msgspec.Meta(gt=0.0, le=threading.TIMEOUT_MAX, title="Heartbeat", description="Seconds between progress record touches"), Presentation(quantity=Some(Quantity.DURATION))
    ] = 3.0
    misses: Annotated[int, msgspec.Meta(ge=2, title="Missed Beats", description="Missed heartbeats before run reads as stopped")] = 10


class StageMark(msgspec.Struct, frozen=True, kw_only=True):
    """Measured seconds and peak memory of one finished step."""

    part: Annotated[int, msgspec.Meta(ge=0)]
    item: int
    seconds: NonNegative
    peak: Annotated[int, msgspec.Meta(ge=0)] | None = None


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


def observed(path: Path) -> RenderingResult[Option[tuple[Progress, datetime.datetime]]]:
    """Reads progress record with heartbeat moment, Nothing when absent."""
    try:
        beat = datetime.datetime.fromtimestamp(path.stat().st_mtime, datetime.UTC)
    except FileNotFoundError:
        return Ok(Nothing)
    except OSError as error:
        return Error((FileFault(access="read", request=path, error=error),))
    return RecordFile(path=path, form=Progress).read().map(lambda progress: Some((progress, beat)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ending", "Heartbeat", "Liveness", "Phase", "Progress", "StageMark", "observed"]
