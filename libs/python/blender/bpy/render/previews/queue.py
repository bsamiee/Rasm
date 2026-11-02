"""Preview jobs under one timer budget the user sets: the feeds producers register, the requests and failures they hold, the demand each slot keeps, and the latest request a single feed renders."""

from collections.abc import Callable, Generator, Hashable, Iterable
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum, IntEnum
from functools import partial
import time
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.extension.events import Handler, Timer
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.outputs.results import built
from blender.bpy.ui.elements.panels import Preference
from blender.records.faults import accumulated, Outcome
from blender.records.stores import ContentHash

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


class Budget(bpy.types.PropertyGroup):
    """Preview renders one queue step starts and the seconds within which it starts more than one."""

    count: bpy.props.IntProperty(name="Renders per Step", description="Previews one queue step renders at most", min=1, soft_max=8, default=2)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    span: bpy.props.FloatProperty(name="Step Time", description="Seconds within which a queue step starts another render", subtype="TIME_ABSOLUTE", min=0.0, soft_max=1.0, default=0.25)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Job(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview request keyed by content hash or held render demand."""

    place: Hashable
    key: Hashable
    rank: Rank
    run: Callable[[], Outcome[None]]
    playback: bool = False


class Demand[B](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Held render request over what it renders and the content hash keying its output, the demand itself keying output with no content hash."""

    basis: B
    content: Option[ContentHash]

    @property
    def key(self) -> Hashable:
        """Content hash, else the demand itself."""
        return self.content.default_value(self)


# --- [SERVICES] -------------------------------------------------------------------------


class Request:
    """Render request with state its run writes."""

    __slots__ = ("job", "state")

    def __init__(self, *, job: Job) -> None:
        """Holds the job, pending."""
        self.job = job
        self.state: Phase | Outcome[None] = Phase.PENDING

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
        self.requests = {(owner, spot): request for (owner, spot), request in self.requests.items() if owner not in replaced or (owner, spot) in arrivals}
        for place, job in arrivals.items():
            match self.requests.get(place):
                case Request(state=Phase.PENDING) as request:
                    request.job = job
                case Request(job=Job(key=key), state=Result(tag="error")) as request if key == job.key:
                    request.job = job
                case _:
                    self.requests.pop(place, None)
                    self.requests[place] = Request(job=job)


BACKLOG: Final = Resource[Backlog](made=lambda _identity: nullcontext(Backlog()), renewed=Some(lambda held: Backlog(feeds=held.feeds)))


class Demands[K: Hashable, B]:
    """Demand per slot, kept while its basis stays equal."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Holds no demand."""
        self.held: dict[K, Demand[B]] = {}

    def demanded(self, slot: K, basis: B, content: Callable[[], Outcome[Option[ContentHash]]]) -> Outcome[Demand[B]]:
        """Returns the slot's held demand over an equal basis, else holds a demand over the basis and the content read, the read skipped while the held one stands."""

        def holding(read: Option[ContentHash]) -> Demand[B]:
            self.held[slot] = demand = Demand(basis=basis, content=read)
            return demand

        return Option.of_optional(self.held.get(slot)).filter(lambda prior: prior.basis == basis).map(Ok).default_with(lambda: content().map(holding))

    def holds(self, slot: K, demand: Demand[B]) -> bool:
        """Whether the slot holds a demand keying the same output, a demand dropped and read again over unchanged content included."""
        return Option.of_optional(self.held.get(slot)).filter(lambda held: held.key == demand.key).is_some()

    def forgotten(self) -> tuple[K, ...]:
        """Drops every held demand, returning the slots dropped."""
        dropped = tuple(self.held)
        self.held.clear()
        return dropped


class Latest[R, V]:
    """Latest-request-wins cell of one feed: the wanted request, an equal one keeping it and its value, and the value published while its request stays wanted."""

    __slots__ = ("value", "wanted")

    def __init__(self) -> None:
        """Holds no request or value."""
        self.wanted: Option[R] = Nothing
        self.value: Option[V] = Nothing

    def wanting(self, request: R) -> bool:
        """Holds the request as wanted, dropping the value published for another, returning whether the wanted request changed."""
        if changed := self.wanted.filter(lambda held: held == request).is_none():
            self.wanted, self.value = Some(request), Nothing
        return changed

    def published(self, request: R, value: V) -> bool:
        """Holds the value while its request stays wanted, returning whether it held it."""
        if wanted := self.wanted.filter(lambda held: held == request).is_some():
            self.value = Some(value)
        return wanted

    def cleared(self) -> bool:
        """Drops the wanted request and its value, returning whether either was held."""
        changed = self.wanted.is_some() or self.value.is_some()
        self.wanted, self.value = Nothing, Nothing
        return changed


class PreviewQueue(msgspec.Struct, frozen=True, kw_only=True):
    """Registration row owning preview runs and timer steps under the budget preference, every instance equal, one extension holding one queue."""

    interval: ClassVar[float] = 0.15
    preference: ClassVar[Preference[Budget]] = Preference(key="preview_queue", form=Budget)

    # --- [EXECUTION]

    @staticmethod
    def executed(identity: Identity, job: Job, *, feed: Option[Feed] = Nothing) -> Outcome[None]:
        """Runs a request and stores its result, skipping a key failed at its place.

        Args:
            identity: Extension owning the request's queue.
            job: Request run now or picked by a timer step.
            feed: Feed scoping request place, `Nothing` outside feeds.

        Returns:
            Render result, `Ok(None)` for a failed key.
        """
        backlog, place = identity.resources[BACKLOG], (feed, job.place)
        match backlog.requests.get(place):
            case Request(job=Job(key=key), state=Result(tag="error")) if key == job.key:
                return Ok(None)
            case _:
                backlog.requests[place] = request = Request(job=job)
                return request.executed()

    @staticmethod
    def forced(identity: Identity, places: Iterable[Hashable]) -> None:
        """Drops held failures at places, rendering them again.

        Args:
            identity: Extension holding the failures.
            places: Places a producer refresh names.
        """
        backlog, dropped = identity.resources[BACKLOG], frozenset(places)
        backlog.requests = {(owner, spot): request for (owner, spot), request in backlog.requests.items() if spot not in dropped or request.state is Phase.PENDING}

    def ticked(self, identity: Identity) -> Outcome[Option[float]]:
        """Merges feed requests and runs pending ones within the budget, holding faults in reports.

        Args:
            identity: Extension with reports holding budget, feed, and render faults.

        Returns:
            `Ok` of next interval on every tick.
        """
        context, backlog = bpy.context, identity.resources[BACKLOG]

        def stepped(manager: bpy.types.WindowManager) -> Outcome[None]:
            budget = self.preference.read(identity)
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
                    if index == 0 or time.perf_counter() - start < limits.span
                    for request in backlog.pending(playing=playing).to_list()
                ),
            )).map(lambda _: None)

        match context.window_manager:
            case bpy.types.WindowManager(is_interface_locked=False) as manager:
                shifted = backlog.slot.map(lambda slot: built("RENDER_RESULT").map(lambda image: image.render_slots.active_index != slot)).default_value(Ok(False))
                return Ok(shifted.bind(lambda held: Ok(None) if held else stepped(manager)).map_error(identity.reports.hold).map(lambda _: Some(self.interval)).default_value(Some(self.interval)))
            case _:
                return Ok(Some(self.interval))

    # --- [LIFECYCLE]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Builds the backlog cell, budget preference, timer, and the deferred record of each finished user render's shown slot.

        Args:
            identity: Extension with reports running deferred main-thread writes.

        Yields:
            `Ok` of state cell, preference, and event rows.
        """

        def remembered(image: bpy.types.Image) -> Option[str]:
            identity.resources[BACKLOG].slot = Option.of_optional(image.render_slots.active_index)
            return Nothing

        with identity.reports.deferred(lambda: built("RENDER_RESULT").map(remembered)) as remember:
            yield Ok((BACKLOG, self.preference, Timer(body=partial(self.ticked, identity)), Handler(handlers=bpy.app.handlers.render_complete, body=lambda: Ok(remember()))))


class PreviewFeed(msgspec.Struct, frozen=True, kw_only=True):
    """Producer's feed of wanted preview jobs, read by the extension's preview queue each step while the row stays registered."""

    feed: Feed

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Preview queue reading the feed."""
        return (PreviewQueue(),)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Registers the feed for the row's scope.

        Args:
            identity: Extension with the queue reading the feed.

        Yields:
            `Ok` of no further rows while the queue reads the feed.
        """

        def unfed() -> None:
            """Drops feed and its requests from the backlog held at release, which a file load may have renewed."""
            backlog = identity.resources[BACKLOG]
            backlog.feeds.remove(self.feed)
            backlog.requests = {(owner, spot): request for (owner, spot), request in backlog.requests.items() if owner != Some(self.feed)}

        with ExitStack() as stack:
            identity.resources[BACKLOG].feeds.append(self.feed)
            stack.callback(unfed)
            yield Ok(())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BACKLOG", "Backlog", "Budget", "Demand", "Demands", "Feed", "Job", "Latest", "Phase", "PreviewFeed", "PreviewQueue", "Rank", "Request"]
