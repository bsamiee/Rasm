"""Render queue jobs, cameras, and frame ranges, the progress record of a queue run, and its duration forecast."""

from collections import Counter
from collections.abc import Mapping
import datetime
from enum import auto, Enum, IntEnum
from fractions import Fraction
from functools import partial
from graphlib import TopologicalSorter
import hashlib
from itertools import groupby
import math
from operator import add
from pathlib import Path, PurePath
from statistics import fmean
from typing import Annotated, Self

from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Outcome, Reason
from blender.records.fields import Case, Count
from blender.records.stores import content_hash, ContentHash, RecordFile
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type _Frame = Annotated[int, msgspec.Meta(ge=0)]
type _Digest = Annotated[bytes, msgspec.Meta(min_length=hashlib.sha256().digest_size, max_length=hashlib.sha256().digest_size)]
type JobCamera = MarkedCamera | FixedCamera
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

# --- [QUEUE]


class Frames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame range with step."""

    start: _Frame
    end: _Frame
    step: _Frame

    def __post_init__(self) -> None:
        """Validates start at or before end."""
        if self.start > self.end:
            raise ValueError((self.start, self.end))

    @property
    def numbers(self) -> range:
        """Frames a render writes."""
        return range(self.start, self.end + 1, self.step) if self.step else range(self.start, self.start + 1)


class Marker(msgspec.Struct, frozen=True, kw_only=True):
    """Timeline marker binding camera from its frame on."""

    frame: int
    camera: str


class MarkedCamera(Case, frozen=True, kw_only=True):
    """Scene camera switched at camera markers."""

    camera: str | None = None
    markers: tuple[Marker, ...] = ()

    def at(self, frame: int) -> Option[str]:
        """Returns camera of last marker reached, else first marker, else scene camera."""
        by_frame = lambda marker: marker.frame
        marker = max((marker for marker in self.markers if marker.frame <= frame), key=by_frame, default=None) or min(self.markers, key=by_frame, default=None)
        return Option.of_optional(marker).map(lambda marker: marker.camera).or_else_with(lambda: Option.of_optional(self.camera))


class FixedCamera(Case, frozen=True, kw_only=True):
    """One camera for every job frame."""

    camera: str


class RenderFrames(Case, frozen=True, kw_only=True):
    """Scene frames a farm worker renders."""

    scene: str
    frames: Annotated[tuple[_Frame, ...], msgspec.Meta(min_length=1)]


class PackedFile(msgspec.Struct, frozen=True, kw_only=True):
    """Local file a job sends to the farm, at its POSIX path in the job's pack, with its SHA-256."""

    source: str
    path: str
    digest: _Digest


class Job(msgspec.Struct, frozen=True, kw_only=True):
    """Scene or view layer render over frame range, sending its pack, the blend alone at its file name when the pack is empty."""

    file: str
    digest: _Digest
    scene: str
    layer: str | None = None
    engine: str
    frames: Frames
    camera: JobCamera
    pixels: tuple[Count, Count]
    samples: Count | None = None
    output_root: str
    outputs: Mapping[Output, str]
    passes: tuple[str, ...]
    prerequisites: tuple[_Frame, ...] = ()
    pack: tuple[PackedFile, ...] = ()

    def __post_init__(self) -> None:
        """Validates a pack holding the job's blend."""
        if self.pack and all(placed.source != self.file for placed in self.pack):
            raise ValueError(self.file)

    @property
    def placements(self) -> tuple[PackedFile, ...]:
        """Files the job sends."""
        return self.pack or (PackedFile(source=self.file, path=PurePath(self.file).name, digest=self.digest),)

    @property
    def blend(self) -> PackedFile:
        """Placement of the job's blend."""
        return next(placed for placed in self.placements if placed.source == self.file)

    @property
    def title(self) -> str:
        """Scene, layer when set, and frame range."""
        return " ".join((self.scene, *Option.of_optional(self.layer).to_list(), f"{self.frames.start}-{self.frames.end}"))

    @property
    def weight(self) -> int:
        """Rendered pixels times samples, pixels alone for an engine whose sample count no host member states."""
        width, height = self.pixels
        return width * height * Option.of_optional(self.samples).default_value(1)


class RenderQueue(msgspec.Struct, frozen=True, kw_only=True):
    """Jobs in render order with Blender and extension versions."""

    jobs: Annotated[tuple[Job, ...], msgspec.Meta(min_length=1)]
    blender: str
    extension: str

    def __post_init__(self) -> None:
        """Validates prerequisites as acyclic job indices."""
        if unknown := ({prerequisite for job in self.jobs for prerequisite in job.prerequisites}.difference(range(len(self.jobs)))):
            raise ValueError(unknown)
        TopologicalSorter({part: job.prerequisites for part, job in enumerate(self.jobs)}).prepare()

    @property
    def key(self) -> ContentHash:
        """Content hash identifying queue."""
        return content_hash(RenderQueue, self)

    @property
    def stem(self) -> str:
        """Hex key naming queue files and folders."""
        return self.key.hex()

    @property
    def steps(self) -> tuple[tuple[int, int], ...]:
        """Job index and frame of every render, in job order."""
        return tuple((index, frame) for index, job in enumerate(self.jobs) for frame in job.frames.numbers)


# --- [PROGRESS]


class StageMark(msgspec.Struct, frozen=True, kw_only=True):
    """Measured render time of one finished step."""

    part: int
    item: int
    elapsed: datetime.timedelta


class Forecast(msgspec.Struct, frozen=True, kw_only=True):
    """Render duration left with finish time."""

    left: datetime.timedelta
    finish: Option[datetime.datetime]


class Progress(msgspec.Struct, frozen=True, kw_only=True):
    """Progress record of one queue run."""

    queue: ContentHash
    process: int
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

    @property
    def elapsed(self) -> datetime.timedelta:
        """Measured render time of every mark."""
        return sum((mark.elapsed for mark in self.marks), datetime.timedelta())

    def remaining(self, queue: RenderQueue) -> tuple[tuple[int, int], ...]:
        """Returns renders no mark covers."""
        marked = {(mark.part, mark.item) for mark in self.marks}
        return tuple(step for step in queue.steps if step not in marked)

    def done(self, queue: RenderQueue) -> Fraction:
        """Returns share of queue renders marks cover."""
        total = len(queue.steps)
        return Fraction(total - len(self.remaining(queue)), total)

    def phase(self, beat: datetime.datetime, now: datetime.datetime, stale: datetime.timedelta, /) -> _Phase:
        """Returns ending when set, else stopped once the heartbeat is `stale` old, else active."""
        ending: Option[_Phase] = Option.of_optional(self.ending)
        return ending.default_with(lambda: Liveness.ACTIVE if now - beat < stale else Liveness.STOPPED)

    def forecast(self, queue: RenderQueue, now: datetime.datetime) -> Option[Forecast]:
        """Forecasts renders left at each job's measured seconds per weight, the mean rate for unmeasured jobs, Nothing before any mark or past the longest duration."""
        counts = Counter(part for part, _ in self.remaining(queue))
        jobs = dict(enumerate(queue.jobs))
        timed = {part: tuple(mark.elapsed.total_seconds() for mark in marks) for part, marks in groupby(sorted(self.marks, key=lambda mark: mark.part), key=lambda mark: mark.part)}
        if not (rates := {part: fmean(times[1:] or times) / jobs[part].weight for part, times in timed.items()}):
            return Nothing
        average = fmean(rates.values())
        seconds = math.fsum(counts[part] * job.weight * rates.get(part, average) for part, job in jobs.items())
        return catch(datetime.timedelta, exception=OverflowError)(seconds=seconds).to_option().map(lambda left: Forecast(left=left, finish=catch(add, exception=OverflowError)(now, left).to_option()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def observed(path: Path) -> Outcome[Option[tuple[Progress, datetime.datetime]]]:
    """Reads progress record with heartbeat moment, Nothing when absent."""
    return (
        catch(path.stat, exception=OSError)()
        .map(lambda stat: datetime.datetime.fromtimestamp(stat.st_mtime, datetime.UTC))
        .map(lambda beat: RecordFile(path=path, form=Progress).read().map(lambda progress: Some((progress, beat))))
        .default_with(lambda error: Ok(error).filter_with(lambda held: isinstance(held, FileNotFoundError), partial(Fault.of, Reason.UNREADABLE, str(path))).map(lambda _: Nothing))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ending", "FixedCamera", "Forecast", "Frames", "Job", "JobCamera", "Liveness", "MarkedCamera", "Marker", "PackedFile", "Progress", "RenderFrames", "RenderQueue", "StageMark", "observed"]
