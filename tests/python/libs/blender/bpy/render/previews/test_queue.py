"""Preview publication generations, native scene isolation, and pending work selection."""

from collections.abc import Callable, Generator, Hashable
from contextlib import AbstractContextManager, ExitStack, nullcontext
from functools import partial
from threading import get_ident
from typing import override

import bpy
from expression import identity, Option, Result, Some
import numpy as np
import pytest

from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable, Resource
from blender.bpy.extension.reports import rendered
from blender.bpy.render.outputs.results import intermediate
from blender.bpy.render.previews.queue import BACKLOG, Backlog, Demands, Feed, Job, Latest, PreviewFeed, PreviewQueue, Rank
from blender.bpy.ui.elements.panels import PreferencesPage
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, Reason
from blender.records.stores import ContentHash
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [SERVICES] -------------------------------------------------------------------------


class PreviewInvalidation(Registrable):
    """Native invalidation row recording callback thread and ready scene outputs."""

    __slots__ = ("notices", "resource", "slots")

    def __init__(self, slots: tuple[int, ...]) -> None:
        """Owns a cache for native scene slots and records each invalidation notice."""
        self.slots = slots
        self.resource = Resource[Latest[int, int, int]](made=lambda _identity: nullcontext(Latest()))
        self.notices: list[tuple[int, tuple[Option[tuple[int, int]], ...]]] = []

    def notified(self, identity: Identity) -> Outcome[None]:
        """Records callback thread and ready outputs."""
        cell = self.resource[identity]
        self.notices.append((get_ident(), tuple(map(cell.shown, self.slots))))
        return Ok(None)

    @override
    def acquisitions(self, identity: Identity, /) -> AbstractContextManager[Acquisitions]:
        return Latest.invalidating(identity, self.resource, lambda basis, scene: basis == scene.session_uid, partial(self.notified, identity))


# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.fixture
def preview_scenes() -> Generator[tuple[bpy.types.Scene, bpy.types.Scene]]:
    """Yields native scene owners and releases their IDs after each preview case."""
    with ExitStack() as release:
        scenes = bpy.data.scenes.new("Preview A"), bpy.data.scenes.new("Preview B")
        release.callback(bpy.data.batch_remove, ids=scenes)
        yield scenes


def test_equal_request_retains_generation_and_ready_value(preview_scenes: tuple[bpy.types.Scene, bpy.types.Scene]) -> None:
    """Repeated demand reuses ready output without scheduling another generation."""
    scene, _other = preview_scenes
    slot, basis = scene.session_uid, (scene.session_uid, "transient")
    cell = Latest[int, tuple[int, str], int]()
    changed, token, value = cell.wanting(slot, basis)
    assert (changed, value) == (True, Nothing)
    assert cell.value(slot, basis) == Nothing
    assert cell.published(slot, token, 7)
    repeated, current, value = cell.wanting(slot, basis)
    assert (repeated, value) == (False, Some(7))
    assert current is token
    assert cell.value(slot, basis) == Some(7)
    assert cell.value(slot, (slot, "other")) == Nothing
    assert not cell.cleared(lambda request: request[1] == "unrequested")
    assert cell.shown(slot) == Some((basis, 7))


def test_clear_then_equal_request_rejects_old_completion(preview_scenes: tuple[bpy.types.Scene, bpy.types.Scene]) -> None:
    """Equal demand after invalidation starts a fresh generation that rejects stale publication."""
    scene, _other = preview_scenes
    slot, basis = scene.session_uid, (scene.session_uid, "transient")
    cell = Latest[int, tuple[int, str], int]()
    _, original, _ = cell.wanting(slot, basis)
    assert cell.published(slot, original, 7)
    assert cell.cleared()
    changed, replacement, value = cell.wanting(slot, basis)
    assert (changed, value) == (True, Nothing)
    assert replacement is not original
    assert not cell.published(slot, original, 11)
    assert cell.shown(slot) == Nothing
    assert cell.published(slot, replacement, 13)
    assert cell.shown(slot) == Some((basis, 13))


def test_scene_invalidation_preserves_other_scene_output(preview_scenes: tuple[bpy.types.Scene, bpy.types.Scene]) -> None:
    """Invalidating one native scene's demand keeps another scene's ready output."""
    first, other = (scene.session_uid for scene in preview_scenes)
    cell = Latest[int, tuple[int, str], int]()
    basis, other_basis = (first, "transient"), (other, "independent")
    _, token, _ = cell.wanting(first, basis)
    _, independent, _ = cell.wanting(other, other_basis)
    assert cell.published(first, token, 7)
    assert cell.published(other, independent, 23)
    assert cell.cleared(lambda request: request[0] == first)
    assert cell.shown(first) == Nothing
    assert cell.shown(other) == Some((other_basis, 23))
    assert not cell.published(first, token, 11)
    assert cell.shown(other) == Some((other_basis, 23))


def test_superseded_and_reset_requests_reject_completion(preview_scenes: tuple[bpy.types.Scene, bpy.types.Scene]) -> None:
    """Replaced demand and reset slots reject outstanding completions and published values."""
    scene, _other = preview_scenes
    slot, basis = scene.session_uid, (scene.session_uid, "transient")
    cell = Latest[int, tuple[int, str], int]()
    _, original, _ = cell.wanting(slot, basis)
    assert cell.published(slot, original, 7)
    changed, replacement, value = cell.wanting(slot, (slot, "changed"))
    assert (changed, value) == (True, Nothing)
    assert replacement is not original
    assert not cell.published(slot, original, 17)
    assert cell.cleared()
    assert not cell.published(slot, replacement, 19)
    assert cell.shown(slot) == Nothing
    assert not cell.cleared()


def test_native_image_content_refreshes_demand_without_dependency_events() -> None:
    """Native pixel writes change content keys, while transient native images renew opaque generations."""
    with ExitStack() as release:
        image = bpy.data.images.new("Preview Content", width=2, height=1, alpha=True, float_buffer=True)
        transient = bpy.data.images.new("Preview Tiles", width=2, height=1, tiled=True)
        release.callback(bpy.data.batch_remove, ids=(image, transient))
        first_pixels = np.asarray((1, 0, 0, 1, 0, 1, 0, 1), dtype=np.float32)
        image.pixels.foreach_set(first_pixels)  # type: ignore[arg-type]
        image.update()
        demands = Demands[int]()
        original = fingerprinted(image).map(partial(demands.demanded, image.session_uid))
        repeated = fingerprinted(image).map(partial(demands.demanded, image.session_uid))
        image.pixels.foreach_set(first_pixels[::-1].copy())  # type: ignore[arg-type]
        image.update()
        changed = fingerprinted(image).map(partial(demands.demanded, image.session_uid))
        known, unknown = fingerprinted(image), fingerprinted(transient)
        assert unknown == Ok(Nothing)
        assert known.map(partial(demands.demanded, image.session_uid)) == changed
        match original, repeated, changed, known, unknown:
            case Result(tag="ok", ok=original_key), Result(tag="ok", ok=repeated_key), Result(tag="ok", ok=changed_key), Result(tag="ok", ok=known_key), Result(tag="ok", ok=unknown_key):
                assert repeated_key == original_key
                assert changed_key != original_key
                assert demands.holds(image.session_uid, changed_key)
                assert not demands.holds(image.session_uid, original_key)
                assert known_key.is_some()
                first = demands.demanded(transient.session_uid, unknown_key)
                second = demands.demanded(transient.session_uid, unknown_key)
                assert first is not second
                assert not demands.holds(transient.session_uid, first)
                assert demands.holds(transient.session_uid, second)
                assert demands.holds(image.session_uid, changed_key)
                released = demands.forgotten()
                assert released == {image.session_uid: changed_key, transient.session_uid: second}
                assert not demands.holds(image.session_uid, changed_key)
                assert not demands.holds(transient.session_uid, second)
                cell = Latest[int, Option[ContentHash], int]()
                reusable: Callable[[Option[ContentHash], Option[ContentHash]], bool] = lambda before, after: after.is_some() and before == after
                _, stable, _ = cell.wanting(image.session_uid, known_key, same=reusable)
                assert cell.published(image.session_uid, stable, 7)
                assert cell.wanting(image.session_uid, known_key, same=reusable) == (False, stable, Some(7))
                _, stale, _ = cell.wanting(transient.session_uid, unknown_key, same=reusable)
                assert cell.published(transient.session_uid, stale, 11)
                fresh, current, value = cell.wanting(transient.session_uid, unknown_key, same=reusable)
                assert (fresh, value) == (True, Nothing)
                assert current is not stale
                assert not cell.published(transient.session_uid, stale, 13)
            case _:
                pytest.fail(repr((original, repeated, changed, known, unknown)))


def test_native_update_invalidates_before_notice_and_skips_intermediates(acquired: Acquired, preview_scenes: tuple[bpy.types.Scene, bpy.types.Scene]) -> None:
    """Native object changes clear scene demand before notice while intermediate renders preserve demand."""
    first, other = preview_scenes
    slots = first.session_uid, other.session_uid
    invalidating = PreviewInvalidation(slots)
    with acquired((invalidating,)) as identity, ExitStack() as release:
        obj = bpy.data.objects.new("Preview dependency", None)
        release.callback(bpy.data.objects.remove, obj, do_unlink=True)
        first.collection.objects.link(obj)
        for scene in preview_scenes:
            scene.view_layers[0].update()
        cell = invalidating.resource[identity]
        tokens = tuple(cell.wanting(slot, slot)[1] for slot in slots)
        for slot, token in zip(slots, tokens, strict=True):
            assert cell.published(slot, token, slot)
        with intermediate(identity, first):
            obj.location.x += 1.0
            first.view_layers[0].update()
        assert tuple(map(cell.shown, slots)) == tuple(Some((slot, slot)) for slot in slots)
        assert invalidating.notices == []
        obj.location.x += 1.0
        first.view_layers[0].update()
        assert cell.shown(slots[0]) == Nothing
        assert cell.shown(slots[1]) == Some((slots[1], slots[1]))
        assert not cell.published(slots[0], tokens[0], 1)
        assert invalidating.notices == [(get_ident(), (Nothing, Some((slots[1], slots[1]))))]


def test_pending_rank_arrival_playback_and_execution_state_select_work() -> None:
    """Priority and arrival select pending work while running, completed, and failed requests stay excluded."""
    backlog = Backlog()
    during: list[tuple[Option[Hashable], Option[Hashable]]] = []

    def running() -> Outcome[None]:
        """Records pending work during active request execution."""
        during.append((backlog.pending(playing=False).map(lambda request: request.job.place), backlog.pending(playing=True).map(lambda request: request.job.place)))
        return Ok(None)

    outcomes: tuple[Outcome[None], ...] = Ok(None), Error(Fault.of(Reason.FAILED, "preview", RuntimeError("cancelled")))
    completed = tuple(Job(place=object(), key=object(), rank=Rank.ACTIVE, run=partial(identity, outcome), playback=True) for outcome in outcomes)
    executing = Job(place=object(), key=object(), rank=Rank.ACTIVE, run=running, playback=True)
    jobs = tuple(Job(place=(rank, playback), key=(rank, playback), rank=rank, run=lambda: Ok(None), playback=playback) for rank in reversed(Rank) for playback in (False, True))
    pending = *completed, executing, *jobs
    feed: Feed = lambda _identity, _context: Ok(pending)
    backlog.merged(((feed, pending),))
    for job, outcome in zip(completed, outcomes, strict=True):
        assert backlog.requests[Some(feed), job.place].executed() == outcome
    assert backlog.requests[Some(feed), executing.place].executed() == Ok(None)
    expected = Some((Rank.ACTIVE, False)), Some((Rank.ACTIVE, True))
    assert during == [expected]
    assert (backlog.pending(playing=False).map(lambda request: request.job.place), backlog.pending(playing=True).map(lambda request: request.job.place)) == expected
    backlog.merged(((feed, ()),))
    assert backlog.pending(playing=False) == Nothing


def test_native_feed_failure_cancels_stale_work_and_keeps_other_feed(acquired: Acquired) -> None:
    """A deleted native source cancels its pending closure while another feed still executes and the fault remains reported."""
    with ExitStack() as release:
        sources = tuple(bpy.data.images.new(name, width=2, height=1) for name in ("Deleted Preview", "Surviving Preview"))
        names = tuple(map(IdName.of, sources))
        handles = tuple(map(Handle.of, sources))
        release.callback(lambda: bpy.data.batch_remove(ids=tuple(image for name in names for image in name.resolve(bpy.types.Image).to_list())))
        attempted: list[Handle[bpy.types.Image]] = []
        completed: list[int] = []

        def run(handle: Handle[bpy.types.Image]) -> Outcome[None]:
            """Records the invoked closure before resolving its actual native source."""
            attempted.append(handle)
            return handle.resolve().map(lambda image: completed.append(image.session_uid))

        def offered(handle: Handle[bpy.types.Image], _identity: Identity, _context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
            """Resolves a native source before offering its pending preview."""
            return handle.resolve().map(lambda image: (Job(place=image.session_uid, key=image.session_uid, rank=Rank.ACTIVE, run=partial(run, handle)),))

        feeds: tuple[Feed, ...] = tuple(partial(offered, handle) for handle in handles)
        with acquired((*(PreviewFeed(feed=feed) for feed in feeds), PreferencesPage())) as identity:
            backlog = BACKLOG[identity]
            reads = tuple(feed(identity, bpy.context) for feed in feeds)
            assert all(read.is_ok() for read in reads), reads
            backlog.merged(tuple((feed, jobs) for feed, read in zip(feeds, reads, strict=True) for jobs in read.to_option().to_list()))
            bpy.data.images.remove(sources[0])
            refused = handles[0].resolve()
            assert refused.map_error(lambda faults: tuple((fault.reason, fault.subject, fault.member) for fault in faults)) == Error(((Reason.ABSENT, bpy.types.Image.__name__, str(handles[0].uid)),))
            queue = PreviewQueue()
            assert queue.ticked(identity) == Ok(Some(queue.interval))
            assert attempted == [handles[1]]
            assert completed == [handles[1].uid]
            assert (Some(feeds[0]), handles[0].uid) not in backlog.requests
            assert backlog.requests[Some(feeds[1]), handles[1].uid].state == Ok(None)
            assert refused.map_error(rendered) == Error(identity.reports.entries)
