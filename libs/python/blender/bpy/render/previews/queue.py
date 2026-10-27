"""Preview jobs, failed requests, and ranked renders under one timer budget."""

from collections.abc import Callable, Generator, Hashable, Iterable
from contextlib import contextmanager, nullcontext
from enum import auto, Enum, IntEnum
from functools import partial
import time
from typing import Annotated, ClassVar, Final, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.extension.events.handlers import SceneHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.scoped import Scoped
from blender.bpy.render.outputs.results import built
from blender.records.faults import accumulated, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Rank(IntEnum):
    """Job run order per step, arrival order within a rank."""

    ACTIVE = auto()
    SHOWN = auto()
    REST = auto()


class Phase(Enum):
    """Request state before and during its render."""

    PENDING = auto()
    RUNNING = auto()


type Feed = Callable[[Identity, bpy.types.Context], Outcome[tuple[Job, ...]]]


# --- [MODELS] ---------------------------------------------------------------------------


class Budget(msgspec.Struct, frozen=True, kw_only=True):
    """Per-step render count and time limits."""

    count: Annotated[int, msgspec.Meta(ge=1, le=8, title="Previews per Step", description="Most previews one step renders")] = 2
    time: Annotated[float, msgspec.Meta(ge=0.02, le=2.0, title="Time Budget", description="Time each step starts renders within")] = 0.25


class Job(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview request keyed by content hash or held render demand."""

    place: Hashable
    key: Hashable
    rank: Rank
    run: Callable[[], Outcome[None]]
    playback: bool = False


# --- [SERVICES] -------------------------------------------------------------------------


class Request:
    """Render request with state its run writes."""

    __slots__ = ("job", "state")

    def __init__(self, *, job: Job, state: Phase | Outcome[None] = Phase.PENDING) -> None:
        """Holds the job and its state."""
        self.job = job
        self.state: Phase | Outcome[None] = state

    def executed(self) -> Outcome[None]:
        """Marks request running, runs its job, and stores result."""
        self.state = Phase.RUNNING
        self.state = result = self.job.run()
        return result


class Backlog:
    """Registered feeds, requests in arrival order, and last user render slot."""

    __slots__ = ("feeds", "requests", "slot")

    def __init__(self, *, feeds: Iterable[Feed] = ()) -> None:
        """Holds feeds with no request and no render slot."""
        self.feeds: list[Feed] = list(feeds)
        self.requests: dict[tuple[Option[Feed], Hashable], Request] = {}
        self.slot: Option[int] = Nothing

    @property
    def jobs(self) -> tuple[tuple[Feed, Job], ...]:
        """Pending feed jobs in arrival order."""
        return tuple((feed, request.job) for (owner, _place), request in self.requests.items() if request.state is Phase.PENDING for feed in owner.to_list())

    @property
    def failed(self) -> dict[tuple[Option[Feed], Hashable], Hashable]:
        """Failed request keys by feed and place."""
        return {place: request.job.key for place, request in self.requests.items() if isinstance(request.state, Result) and request.state.is_error()}

    def pending(self, *, playing: bool) -> Option[Request]:
        """Earliest pending request of the lowest rank, playback requests alone while playing."""
        return (
            Block.of_seq(request for request in self.requests.values() if request.state is Phase.PENDING and (request.job.playback or not playing)).sort_with(lambda queued: queued.job.rank).try_head()
        )

    def merged(self, reads: tuple[tuple[Feed, tuple[Job, ...]], ...]) -> None:
        """Replaces each read feed's requests by its jobs, keeping a pending or equally failed request's place and failure."""
        arrivals = {(Some(feed), job.place): job for feed, jobs in reads for job in jobs}
        replaced = frozenset(Some(feed) for feed, _jobs in reads)
        self.requests = {place: request for place, request in self.requests.items() if place[0] not in replaced or place in arrivals}
        for place, job in arrivals.items():
            match self.requests.get(place):
                case Request(state=Phase.PENDING) as request:
                    request.job = job
                case Request(job=Job(key=key), state=Result(tag="error")) as request if key == job.key:
                    request.job = job
                case _:
                    self.requests.pop(place, None)
                    self.requests[place] = Request(job=job)

    def reset(self) -> Self:
        """Backlog of the same feeds after a file load, the previous file's requests and render slot dropped."""
        return type(self)(feeds=self.feeds)


BACKLOG: Final = Scoped[Backlog](made=lambda _identity: nullcontext(Backlog()), renewed=Some(Backlog.reset))


class PreviewQueue(msgspec.Struct, frozen=True, kw_only=True):
    """Registration row owning preview runs and timer steps."""

    budget: Callable[[Identity], Outcome[Budget]] = lambda _identity: Ok(Budget())
    interval: ClassVar[float] = 0.15

    # --- [EXECUTION]

    @staticmethod
    def executed(identity: Identity, job: Job, *, feed: Option[Feed] = Nothing) -> Outcome[None]:
        """Runs a request and stores its result, skipping a key failed at its place.

        Args:
            identity: Extension whose queue owns the request.
            job: Request run now or picked by a timer step.
            feed: Feed scoping request place, `Nothing` outside feeds.

        Returns:
            Render result, `Ok(None)` for a failed key.
        """
        backlog, place = identity.held[BACKLOG], (feed, job.place)
        if (place, job.key) in backlog.failed.items():
            return Ok(None)
        backlog.requests[place] = request = Request(job=job)
        return request.executed()

    @staticmethod
    def forced(identity: Identity, places: Iterable[Hashable]) -> None:
        """Drops held failures at places so they render again.

        Args:
            identity: Extension whose failures drop.
            places: Places a producer refresh names.
        """
        backlog, dropped = identity.held[BACKLOG], frozenset(places)
        backlog.requests = {place: request for place, request in backlog.requests.items() if place[1] not in dropped or request.state is Phase.PENDING}

    def ticked(self, identity: Identity) -> Outcome[Option[float]]:
        """Merges feed requests and runs pending ones within budget, holding faults in reports.

        Args:
            identity: Extension whose reports hold feed and render faults.

        Returns:
            `Ok` of next interval on every tick.
        """
        context, backlog = bpy.context, identity.held[BACKLOG]

        def stepped(manager: bpy.types.WindowManager) -> Outcome[None]:
            budget = self.budget(identity)
            reads = tuple((feed, feed(identity, context)) for feed in tuple(backlog.feeds))
            backlog.merged(tuple((feed, jobs) for feed, read in reads if feed in backlog.feeds for jobs in read.to_option().to_list()))
            playing = any(window.screen.is_animation_playing for window in manager.windows)
            start = time.perf_counter()
            return accumulated((
                budget.map(lambda _: None),
                *(read.map(lambda _: None) for _, read in reads),
                *(
                    request.executed()
                    for limits in budget.to_option().to_list()
                    for index in range(limits.count)
                    if index == 0 or time.perf_counter() - start < limits.time
                    for request in backlog.pending(playing=playing).to_list()
                ),
            )).map(lambda _: None)

        match context.window_manager:
            case bpy.types.WindowManager(is_interface_locked=False) as manager:
                shifted = backlog.slot.map(lambda slot: built("RENDER_RESULT").map(lambda image: image.render_slots.active_index != slot)).default_value(Ok(False))
                return Ok(shifted.bind(lambda held: Ok(None) if held else stepped(manager)).map_error(identity.reports.hold).map(lambda _: Some(self.interval)).default_value(Some(self.interval)))
            case _:
                return Ok(Some(self.interval))

    # --- [FEEDS]

    @staticmethod
    @contextmanager
    def fed(identity: Identity, feed: Feed) -> Generator[None]:
        """Registers a producer's feed for its acquisition scope.

        Args:
            identity: Extension whose queue registers before the producer.
            feed: Producer returning wanted requests each tick.

        Yields:
            None while queue reads feed.
        """
        identity.held[BACKLOG].feeds.append(feed)
        try:
            yield
        finally:
            backlog = identity.held[BACKLOG]
            backlog.feeds.remove(feed)
            backlog.requests = {place: request for place, request in backlog.requests.items() if place[0] != Some(feed)}

    # --- [LIFECYCLE]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Builds the backlog cell, timer, and the deferred record of each finished user render's shown slot.

        Args:
            identity: Extension whose reports run deferred main-thread writes.

        Yields:
            `Ok` of state cell and event rows.
        """

        def remembered(image: bpy.types.Image) -> Option[str]:
            identity.held[BACKLOG].slot = Option.of_optional(image.render_slots.active_index)
            return Nothing

        with identity.reports.deferred(lambda: built("RENDER_RESULT").map(remembered)) as remember:
            yield Ok((BACKLOG, Timer(body=partial(self.ticked, identity)), SceneHandler(handlers=bpy.app.handlers.render_complete, body=lambda _: Ok(remember()))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BACKLOG", "Backlog", "Budget", "Feed", "Job", "Phase", "PreviewQueue", "Rank", "Request"]
