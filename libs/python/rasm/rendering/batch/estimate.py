"""Queue time and memory estimates from stage marks and a clock passed in."""

from collections import Counter
from collections.abc import Iterable
import datetime
from fractions import Fraction
from statistics import mean
from typing import Annotated, Self

from expression import Nothing, Option, Some
import msgspec

from rasm.rendering.batch.progress import Progress, StageMark
from rasm.rendering.batch.queue import RenderQueue

# --- [MODELS] ---------------------------------------------------------------------------


class Work(msgspec.Struct, frozen=True, kw_only=True):
    """Work one part has left: its weight per item and the items left."""

    part: Annotated[int, msgspec.Meta(ge=0)]
    weight: Annotated[int, msgspec.Meta(ge=1)]
    count: Annotated[int, msgspec.Meta(ge=0)]


class Estimate(msgspec.Struct, frozen=True, kw_only=True):
    """Seconds left, the finish moment, and the greatest measured peak memory."""

    seconds: Fraction
    finish: Option[datetime.datetime]
    peak: Option[int]

    @classmethod
    def of(cls, marks: tuple[StageMark, ...], work: Iterable[Work], now: datetime.datetime) -> Option[Self]:
        """Estimates the work left from measured marks."""
        works = {row.part: row for row in work}
        timed = {part: times for part in works if (times := tuple(Fraction(mark.seconds) for mark in marks if mark.part == part))}
        rates = {part: mean(times[1:] or times) / works[part].weight for part, times in timed.items()}
        peaks = tuple(peak for mark in marks if mark.part in works for peak in Option.of_optional(mark.peak).to_list())
        if not rates:
            return Nothing
        average = mean(rates.values())
        seconds = sum((row.count * row.weight * rates.get(row.part, average) for row in works.values()), Fraction())
        microseconds_per_second = datetime.timedelta(seconds=1) // datetime.timedelta.resolution
        try:
            finish = Some(now + datetime.timedelta(microseconds=round(seconds * microseconds_per_second)))
        except OverflowError:
            finish = Nothing
        return Some(cls(seconds=seconds, finish=finish, peak=Option.of_optional(max(peaks, default=None))))

    @classmethod
    def queued(cls, progress: Progress, queue: RenderQueue, now: datetime.datetime) -> Option[Self]:
        """Estimates the renders a queue's record leaves."""
        counts = Counter(part for part, _ in progress.remaining(queue))
        return cls.of(progress.marks, (Work(part=index, weight=job.weight, count=counts[index]) for index, job in enumerate(queue.jobs)), now)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Estimate", "Work"]
