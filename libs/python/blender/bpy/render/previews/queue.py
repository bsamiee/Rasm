"""Preview feeds and jobs under one timer budget, held demands, and source request generations with native invalidation."""

from collections.abc import Callable, Generator, Hashable, Iterable
from contextlib import contextmanager, ExitStack, nullcontext
from enum import auto, Enum, IntEnum
from functools import partial
from operator import eq
import time
from typing import ClassVar, Final

import bpy
from expression import Option, Result, Some
import msgspec

from blender.bpy.extension.events import updated
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration, Handler, Resource, Timer
from blender.bpy.render.outputs.results import built, INTERMEDIATES
from blender.bpy.ui.elements.panels import Preference
from blender.records.faults import accumulated, Nothing, Ok, Outcome
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
        return Option.of_optional(
            min((request for request in self.requests.values() if request.state is Phase.PENDING and (request.job.playback or not playing)), key=lambda queued: queued.job.rank, default=None)  # ty: ignore[unresolved-attribute]
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


class Demands[K: Hashable]:
    """Current content hash or opaque uncacheable request key per slot."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Holds no demand."""
        self.held: dict[K, Hashable] = {}

    def demanded(self, slot: K, content: Option[ContentHash]) -> Hashable:
        """Holds current content's hash or a fresh opaque request key."""
        self.held[slot] = key = content.default_with(object)
        return key  # ty: ignore[unsound-return-statement]

    def holds(self, slot: K, key: Hashable) -> bool:
        """Whether the slot still requests this content or opaque generation."""
        return (slot, key) in self.held.items()

    def forgotten(self) -> dict[K, Hashable]:
        """Releases the held demand map and starts with no demand."""
        dropped, self.held = self.held, {}
        return dropped


class Latest[K: Hashable, R, V]:
    """Current request generation and optional published value per source slot."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Holds no request or value."""
        self.held: dict[K, tuple[R, object, Option[V]]] = {}

    def wanting(self, slot: K, request: R, *, same: Callable[[R, R], bool] = eq) -> tuple[bool, object, Option[V]]:
        """Sets a source request and returns its change flag, publication token, and held value."""
        match self.held.get(slot):
            case (prior, token, value) if same(prior, request):
                return False, token, value
            case _:
                self.held[slot] = request, (token := object()), Nothing
                return True, token, Nothing

    def value(self, slot: K, request: R) -> Option[V]:
        """Returns a published value for the current matching request."""
        match self.held.get(slot):
            case (prior, _token, value) if prior == request:
                return value
            case _:
                return Nothing

    def shown(self, slot: K) -> Option[tuple[R, V]]:
        """Returns a source request with its published value."""
        match self.held.get(slot):
            case (request, _token, Option(tag="some", some=value)):
                return Some((request, value))
            case _:
                return Nothing

    def published(self, slot: K, token: object, value: V) -> bool:
        """Publishes a value only while its request generation remains current."""
        match self.held.get(slot):
            case (request, prior, _) if prior is token:
                self.held[slot] = request, token, Some(value)
                return True
            case _:
                return False

    def cleared(self, stale: Callable[[R], bool] = lambda _request: True) -> bool:
        """Removes matching requests and values, returning whether any source changed."""
        dropped = tuple(slot for slot, (request, _token, _value) in self.held.items() if stale(request))
        for slot in dropped:
            del self.held[slot]
        return bool(dropped)

    @staticmethod
    @contextmanager
    def invalidating(
        identity: Identity,
        resource: Resource["Latest[K, R, V]"],
        stale: Callable[[R, bpy.types.Scene], bool],
        notify: Callable[[], Outcome[None]],
        *,
        ids: tuple[type[bpy.types.ID], ...] = (bpy.types.ID,),
    ) -> Generator[Acquisitions]:
        """Scopes dependency invalidation and file, undo, and redo resets, queueing notices outside preview renders."""
        with identity.reports.deferred(lambda: notify().map(lambda _: Nothing)) as later:

            def cleared(predicate: Callable[[R], bool]) -> Outcome[None]:
                if resource[identity].cleared(predicate):
                    later()
                return Ok(None)

            yield (
                resource,
                INTERMEDIATES,
                *(Handler(handlers=handlers, body=lambda: cleared(lambda _request: True)) for handlers in (bpy.app.handlers.load_post, bpy.app.handlers.undo_post, bpy.app.handlers.redo_post)),
                Handler(
                    handlers=bpy.app.handlers.depsgraph_update_post,
                    body=partial(updated, ids, lambda scene, _updates: Ok(None) if INTERMEDIATES[identity] else cleared(lambda request: stale(request, scene))),
                ),
            )


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
        backlog, place = BACKLOG[identity], (feed, job.place)
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
        backlog, dropped = BACKLOG[identity], frozenset(places)
        backlog.requests = {(owner, spot): request for (owner, spot), request in backlog.requests.items() if spot not in dropped or request.state is Phase.PENDING}

    def ticked(self, identity: Identity) -> Outcome[Option[float]]:
        """Replaces each read feed's requests, cancels them on read failure, and runs pending jobs while reporting every fault.

        Args:
            identity: Extension with reports holding budget, feed, and render faults.

        Returns:
            `Ok` of next interval on every tick.
        """
        context, backlog = bpy.context, BACKLOG[identity]

        def stepped(manager: bpy.types.WindowManager) -> Outcome[None]:
            budget = self.preference.read(identity)
            reads = tuple((feed, feed(identity, context)) for feed in tuple(backlog.feeds))
            backlog.merged(tuple((feed, read.default_value(())) for feed, read in reads if feed in backlog.feeds))
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
                shifted = backlog.slot.map(lambda slot: built("RENDER_RESULT").map(lambda image: image.render_slots.active_index != slot)).default_value(Ok(value=False))
                return Ok(shifted.bind(lambda held: Ok(None) if held else stepped(manager)).map_error(identity.reports.hold).map(lambda _: Some(self.interval)).default_value(Some(self.interval)))
            case _:
                return Ok(Some(self.interval))

    # --- [LIFECYCLE]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Builds the backlog cell, budget preference, timer, and the deferred record of each finished user render's shown slot.

        Args:
            identity: Extension with reports running deferred main-thread writes.

        Yields:
            State cell, preference, and event rows.
        """

        def remembered(image: bpy.types.Image) -> Option[str]:
            BACKLOG[identity].slot = Option.of_optional(image.render_slots.active_index)
            return Nothing

        with identity.reports.deferred(lambda: built("RENDER_RESULT").map(remembered)) as remember:
            yield (BACKLOG, self.preference, Timer(body=partial(self.ticked, identity)), Handler(handlers=bpy.app.handlers.render_complete, body=lambda: Ok(remember())))


class PreviewFeed(msgspec.Struct, frozen=True, kw_only=True):
    """Producer's feed of wanted preview jobs, read by the extension's preview queue each step while the row stays registered."""

    feed: Feed

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Preview queue reading the feed."""
        return (PreviewQueue(),)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Registers the feed for the row's scope.

        Args:
            identity: Extension with the queue reading the feed.

        Yields:
            No further rows while the queue reads the feed.
        """

        def unfed() -> None:
            """Drops feed and its requests from the backlog held at release, which a file load may have renewed."""
            backlog = BACKLOG[identity]
            backlog.feeds.remove(self.feed)
            backlog.requests = {(owner, spot): request for (owner, spot), request in backlog.requests.items() if owner != Some(self.feed)}

        with ExitStack() as stack:
            BACKLOG[identity].feeds.append(self.feed)
            stack.callback(unfed)
            yield ()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BACKLOG", "Backlog", "Budget", "Demands", "Feed", "Job", "Latest", "Phase", "PreviewFeed", "PreviewQueue", "Rank", "Request"]
