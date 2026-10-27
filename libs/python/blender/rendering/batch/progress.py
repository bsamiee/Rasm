"""Stage marks, queue progress record, and its duration forecast."""

from collections import Counter
import datetime
from enum import auto, Enum, IntEnum
from fractions import Fraction
from pathlib import Path
from statistics import mean
import sys
import threading
from typing import Annotated, Self

from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import NonNegative
from blender.records.identity import ContentHash
from blender.records.stores import RecordFile
from blender.rendering.batch.queue import RenderQueue

# --- [TYPES] ----------------------------------------------------------------------------

type _Phase = Liveness | Ending


class Liveness(Enum):
    """State of unfinished run."""

    ACTIVE = auto()
    STOPPED = auto()


class Ending(IntEnum):
    """State of finished run."""

    SUCCEEDED = 1
    FAILED = 2
    CANCELLED = 3


# --- [MODELS] ---------------------------------------------------------------------------


class Heartbeat(msgspec.Struct, frozen=True, kw_only=True):
    """Seconds between progress record touches and missed touches before a run reads as stopped."""

    interval: Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=threading.TIMEOUT_MAX)] = 3.0
    misses: Annotated[int, msgspec.Meta(ge=2)] = 10


class StageMark(msgspec.Struct, frozen=True, kw_only=True):
    """Measured seconds of one finished step."""

    part: Annotated[int, msgspec.Meta(ge=0)]
    item: int
    seconds: NonNegative


class Forecast(msgspec.Struct, frozen=True, kw_only=True):
    """Render duration left with finish time."""

    seconds: Fraction
    finish: Option[datetime.datetime]


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

    def phase(self, beat: datetime.datetime, now: datetime.datetime, heartbeat: Heartbeat) -> _Phase:
        """Returns ending when set, else liveness from heartbeat age."""
        ending: Option[_Phase] = Option.of_optional(self.ending)
        return ending.default_with(lambda: Liveness.ACTIVE if (now - beat).total_seconds() < heartbeat.interval * heartbeat.misses else Liveness.STOPPED)

    def forecast(self, queue: RenderQueue, now: datetime.datetime) -> Option[Forecast]:
        """Forecasts renders left at each job's measured seconds per weight, the mean rate for unmeasured jobs, Nothing before any mark."""
        counts = Counter(part for part, _ in self.remaining(queue))
        jobs = dict(enumerate(queue.jobs))
        timed = {part: times for part in jobs if (times := tuple(Fraction(mark.seconds) for mark in self.marks if mark.part == part))}
        if not (rates := {part: mean(times[1:] or times) / jobs[part].weight for part, times in timed.items()}):
            return Nothing
        average = mean(rates.values())
        seconds = sum((counts[part] * job.weight * rates.get(part, average) for part, job in jobs.items()), Fraction())

        @catch(exception=OverflowError)
        def finished() -> datetime.datetime:
            return now + datetime.timedelta(microseconds=round(seconds * (datetime.timedelta(seconds=1) // datetime.timedelta.resolution)))

        return Some(Forecast(seconds=seconds, finish=finished().to_option()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def observed(path: Path) -> Outcome[Option[tuple[Progress, datetime.datetime]]]:
    """Reads progress record with heartbeat moment, Nothing when absent."""
    return (
        catch(path.stat, exception=OSError)()
        .map(lambda stat: datetime.datetime.fromtimestamp(stat.st_mtime, datetime.UTC))
        .map(lambda beat: RecordFile(path=path, form=Progress).read().map(lambda progress: Some((progress, beat))))
        .default_with(
            lambda error: (
                Ok(error).filter_with(lambda held: isinstance(held, FileNotFoundError), lambda held: (Fault(reason=RecordReason.UNREADABLE, subject=str(path), cause=held),)).map(lambda _: Nothing)
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ending", "Forecast", "Heartbeat", "Liveness", "Progress", "StageMark", "observed"]
