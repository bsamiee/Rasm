"""Preview jobs, failed requests, and ranked renders under one timer budget."""

from collections.abc import Callable, Generator, Hashable, Iterable
from contextlib import contextmanager, ExitStack
from enum import auto, Enum, IntEnum
from functools import partial
from pathlib import Path
import time
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.data.ids.images import built
from blender.bpy.extension.events.handlers import PathHandler, SceneHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderResult
from blender.imaging.faults import accumulated
from blender.imaging.parameters import Presentation, Quantity

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


type Feed = Callable[[Identity, bpy.types.Context], BlenderResult[tuple[Job, ...]]]


# --- [MODELS] ---------------------------------------------------------------------------


class Budget(msgspec.Struct, frozen=True, kw_only=True):
    """Per-step render count and time limits."""

    count: Annotated[int, msgspec.Meta(ge=1, le=8, title="Previews per Step", description="Most previews one step renders")] = 2
    time: Annotated[float, msgspec.Meta(ge=0.02, le=2.0, title="Time Budget", description="Time each step starts renders within"), Presentation(quantity=Some(Quantity.DURATION))] = 0.25


class Job(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview request keyed by content hash or held render demand."""

    place: Hashable
    key: Hashable
    rank: Rank
    run: Callable[[], BlenderResult[None]]
    playback: bool = False


# --- [SERVICES] -------------------------------------------------------------------------


class Request(msgspec.Struct, kw_only=True, eq=False):
    """Render request with state its run writes."""

    job: Job
    state: Phase | BlenderResult[None] = Phase.PENDING

    def executed(self) -> BlenderResult[None]:
        """Marks request running, runs its job, and stores result."""
        job = self.job
        self.state = Phase.RUNNING
        result = job.run()
        self.state = result
        return result


class Backlog(msgspec.Struct, kw_only=True):
    """Registered feeds, requests in arrival order, and last user render slot."""

    feeds: list[Feed] = msgspec.field(default_factory=list)
    requests: dict[tuple[Option[Feed], Hashable], Request] = msgspec.field(default_factory=dict)
    slot: Option[int] = Nothing

    @property
    def jobs(self) -> tuple[tuple[Feed, Job], ...]:
        """Pending feed jobs in arrival order."""
        return tuple((feed, request.job) for (owner, _place), request in self.requests.items() if request.state is Phase.PENDING for feed in owner.to_list())

    @property
    def failed(self) -> dict[tuple[Option[Feed], Hashable], Hashable]:
        """Failed request keys by feed and place."""
        return {place: request.job.key for place, request in self.requests.items() if isinstance(request.state, Result) and request.state.is_error()}


class PreviewQueue(msgspec.Struct, frozen=True, kw_only=True):
    """Registration row owning preview runs and timer steps."""

    budget: Callable[[Identity], BlenderResult[Budget]] = lambda _identity: Ok(Budget())
    interval: ClassVar[float] = 0.15
    held: ClassVar[dict[str, Backlog]] = {}

    # --- [EXECUTION]

    @staticmethod
    def executed(identity: Identity, job: Job, *, feed: Option[Feed] = Nothing) -> BlenderResult[None]:
        """Runs a request and stores its result, skipping a key failed at its place.

        Args:
            identity: Extension whose queue owns the request.
            job: Request run now or picked by a timer step.
            feed: Feed scoping request place, `Nothing` outside feeds.

        Returns:
            Render result, `Ok(None)` for a failed key.
        """
        backlog, place = PreviewQueue.held[identity.package], (feed, job.place)
        if (place, job.key) in backlog.failed.items():
            return Ok(None)
        request = Request(job=job)
        backlog.requests[place] = request
        return request.executed()

    @staticmethod
    def forced(identity: Identity, places: Iterable[Hashable]) -> None:
        """Drops held failures at places so they render again.

        Args:
            identity: Extension whose failures drop.
            places: Places a producer refresh names.
        """
        backlog, dropped = PreviewQueue.held[identity.package], frozenset(places)
        backlog.requests = {place: request for place, request in backlog.requests.items() if place[1] not in dropped or request.state is Phase.PENDING}

    def ticked(self, identity: Identity) -> BlenderResult[Option[float]]:
        """Merges feed requests and runs pending ones within budget.

        Args:
            identity: Extension whose reports hold feed and render faults.

        Returns:
            `Ok` of next interval on every tick, faults held in reports.
        """
        context, again = bpy.context, Some(PreviewQueue.interval)
        backlog = PreviewQueue.held[identity.package]

        def stepped(manager: bpy.types.WindowManager) -> BlenderResult[None]:
            """Merges successful feed reads into pending requests and runs them."""
            budget = self.budget(identity)
            reads = Block.of_seq((feed, feed(identity, context)) for feed in tuple(backlog.feeds))
            wanted = reads.choose(lambda read: read[1].map(lambda jobs: (read[0], jobs)).to_option()).filter(lambda read: read[0] in backlog.feeds)
            arrivals = {(Some(feed), job.place): job for feed, jobs in wanted for job in jobs}
            replaced = frozenset(Some(feed) for feed, _jobs in wanted)
            backlog.requests = {place: request for place, request in backlog.requests.items() if place[0] not in replaced or place in arrivals}
            for place, job in arrivals.items():
                match backlog.requests.get(place):
                    case Request(state=Phase.PENDING) as request:
                        request.job = job
                    case Request(job=Job(key=key), state=Result(tag="error")) as request if key == job.key:
                        request.job = job
                    case _:
                        backlog.requests.pop(place, None)
                        backlog.requests[place] = Request(job=job)
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
                    for request in (
                        Block
                        .of_seq(request for request in backlog.requests.values() if request.state is Phase.PENDING and (request.job.playback or not playing))
                        .sort_with(lambda queued: queued.job.rank)
                        .try_head()
                        .to_list()
                    )
                ),
            )).map(lambda _: None)

        match context.window_manager:
            case bpy.types.WindowManager(is_interface_locked=False) as manager:
                shifted = backlog.slot.map(lambda slot: built("RENDER_RESULT").map(lambda image: image.render_slots.active_index != slot)).default_value(Ok(False))
                return Ok(shifted.bind(lambda held: Ok(None) if held else stepped(manager)).map(lambda _: again).map_error(identity.reports.hold).default_value(again))
            case _:
                return Ok(again)

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
        PreviewQueue.held[identity.package].feeds.append(feed)
        try:
            yield
        finally:
            backlog = PreviewQueue.held[identity.package]
            backlog.feeds.remove(feed)
            backlog.requests = {place: request for place, request in backlog.requests.items() if place[0] != Some(feed)}

    # --- [LIFECYCLE]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Builds state, timer, render-complete, and file-load acquisitions.

        Args:
            identity: Extension whose reports run deferred main-thread writes.

        Yields:
            `Ok` of state scope and event rows.
        """
        with ExitStack() as scopes:

            def remembered(image: bpy.types.Image) -> Option[str]:
                """Records finished user render's shown slot on main thread."""
                PreviewQueue.held[identity.package].slot = Some(image.render_slots.active_index)  # type: ignore[arg-type]  # ty: ignore[invalid-assignment]
                return Nothing

            remember = scopes.enter_context(identity.reports.deferred(lambda: built("RENDER_RESULT").map(remembered)))

            @contextmanager
            def held() -> Generator[None]:
                """Holds package state, closing deferred write before release."""
                PreviewQueue.held[identity.package] = Backlog()
                try:
                    yield
                finally:
                    scopes.close()
                    backlog = PreviewQueue.held.pop(identity.package)
                    backlog.feeds.clear()
                    backlog.requests.clear()

            def cleared(_: Option[Path]) -> BlenderResult[None]:
                """Drops requests naming previous file's data-blocks."""
                identity.reports.cancelled(remember)
                backlog = PreviewQueue.held[identity.package]
                PreviewQueue.held[identity.package] = Backlog(feeds=backlog.feeds.copy())
                backlog.feeds.clear()
                backlog.requests.clear()
                return Ok(None)

            yield Ok((
                (Band.RESOURCES, held()),
                Timer(body=partial(self.ticked, identity)),
                SceneHandler(handlers=bpy.app.handlers.render_complete, body=lambda _: Ok(remember())),
                PathHandler(handlers=bpy.app.handlers.load_post, body=cleared),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Backlog", "Budget", "Feed", "Job", "Phase", "PreviewQueue", "Rank", "Request"]
