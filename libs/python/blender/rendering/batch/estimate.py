"""Queue duration forecasts from stage marks."""

from collections import Counter
import datetime
from fractions import Fraction
from statistics import mean
from typing import Self

from expression import Nothing, Option, Some
from expression.extra.result import catch
import msgspec

from blender.rendering.batch.progress import Progress
from blender.rendering.batch.queue import RenderQueue

# --- [MODELS] ---------------------------------------------------------------------------


class Forecast(msgspec.Struct, frozen=True, kw_only=True):
    """Render duration left with finish time."""

    seconds: Fraction
    finish: Option[datetime.datetime]

    @classmethod
    def queued(cls, progress: Progress, queue: RenderQueue, now: datetime.datetime) -> Option[Self]:
        """Forecasts renders progress leaves at each job's measured seconds per weight, the mean rate for unmeasured jobs, Nothing before any mark."""
        counts = Counter(part for part, _ in progress.remaining(queue))
        jobs = dict(enumerate(queue.jobs))
        timed = {part: times for part in jobs if (times := tuple(Fraction(mark.seconds) for mark in progress.marks if mark.part == part))}
        if not (rates := {part: mean(times[1:] or times) / jobs[part].weight for part, times in timed.items()}):
            return Nothing
        average = mean(rates.values())
        seconds = sum((counts[part] * job.weight * rates.get(part, average) for part, job in jobs.items()), Fraction())
        finish = catch(lambda: now + datetime.timedelta(microseconds=round(seconds * (datetime.timedelta(seconds=1) // datetime.timedelta.resolution))), exception=OverflowError)().to_option()
        return Some(cls(seconds=seconds, finish=finish))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Forecast"]
