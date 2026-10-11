"""Stage marks of long folds and the render queue's progress record."""

import datetime
from fractions import Fraction
from pathlib import Path
import sys
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec

from rasm.imaging.hashing import ContentHash
from rasm.imaging.parameters import Choice, Presentation, Quantity
from rasm.rendering.batch.queue import load, RenderQueue
from rasm.rendering.faults import RenderingReadFault, RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Liveness | Ending


class Liveness(Choice):
    """State of an unfinished run."""

    ACTIVE = 1, "Rendering"
    STOPPED = 2, "Stopped"


class Ending(Choice):
    """State of a finished run."""

    SUCCEEDED = 1, "Finished"
    FAILED = 2, "Failed"
    CANCELLED = 3, "Cancelled"


# --- [MODELS] ---------------------------------------------------------------------------


class Heartbeat(msgspec.Struct, frozen=True, kw_only=True):
    """Touch interval of a running queue and the beats missed before it reads as stopped."""

    interval: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Heartbeat", description="Seconds between touches of the progress record while a queue runs"),
        Presentation(quantity=Some(Quantity.DURATION)),
    ] = 3.0
    misses: Annotated[int, msgspec.Meta(ge=2, title="Missed Beats", description="Heartbeats missed before a run reads as stopped")] = 10


class StageMark(msgspec.Struct, frozen=True, kw_only=True):
    """Finished step of a long fold with its measured seconds and peak memory."""

    part: Annotated[int, msgspec.Meta(ge=0)]
    item: int
    seconds: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max)]
    peak: Annotated[int, msgspec.Meta(ge=0)] | None = None


class Progress(msgspec.Struct, frozen=True, kw_only=True):
    """Progress record of one queue run."""

    queue: ContentHash
    process: Annotated[int, msgspec.Meta(ge=1)]
    marks: tuple[StageMark, ...] = ()
    ending: Ending | None = None
    pending: tuple[tuple[int, int], ...] = ()

    @classmethod
    def begun(cls, found: Option[Self], queue: RenderQueue, process: int) -> Self:
        """Starts a run, resuming a found record of the same queue."""
        key = queue.key
        return (
            found
            .filter(lambda held: held.queue == key and Option.of_optional(held.ending) != Some(Ending.SUCCEEDED))
            .map(lambda held: msgspec.structs.replace(held, process=process, ending=None))
            .default_with(lambda: cls(queue=key, process=process))
        )

    def remaining(self, queue: RenderQueue) -> tuple[tuple[int, int], ...]:
        """Answers the renders no mark covers."""
        marked = {(mark.part, mark.item) for mark in self.marks}
        return tuple(step for step in queue.steps if step not in marked)

    def done(self, queue: RenderQueue) -> Fraction:
        """Answers the fraction of a queue's renders a mark covers."""
        total = len(queue.steps)
        return Fraction(total - len(self.remaining(queue)), total)

    def phase(self, beat: datetime.datetime, now: datetime.datetime, heartbeat: Heartbeat) -> Phase:
        """Answers the run's state."""
        ending: Option[Phase] = Option.of_optional(self.ending)
        origin = datetime.datetime.min.replace(tzinfo=datetime.UTC)
        elapsed = (now - origin) - (beat - origin)
        return ending.default_with(
            lambda: (
                Liveness.ACTIVE
                if Fraction(elapsed // datetime.timedelta.resolution, datetime.timedelta(seconds=1) // datetime.timedelta.resolution) < Fraction(heartbeat.interval) * heartbeat.misses
                else Liveness.STOPPED
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def observed(path: Path) -> RenderingResult[Option[tuple[Progress, datetime.datetime]]]:
    """Reads a progress record with its heartbeat."""
    try:
        beat = datetime.datetime.fromtimestamp(path.stat().st_mtime, datetime.UTC)
    except FileNotFoundError:
        return Ok(Nothing)
    except OSError as error:
        return Error((RenderingReadFault(request=str(path), error=error),))
    return load(path, Progress).map(lambda progress: Some((progress, beat)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ending", "Heartbeat", "Liveness", "Phase", "Progress", "StageMark", "observed"]
