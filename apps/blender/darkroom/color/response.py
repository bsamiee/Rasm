"""Darkroom tone response: the chain's tone and formation entries sampled across the allocation's stop window outside drawing, charted over the compositor viewer with middle-gray guides."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial
from typing import ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.effects import Enabled
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.ui.drawing.canvas import Chart, GRID, Label, Layer, TRACE
from blender.bpy.ui.drawing.handlers import Reading, Signal
from blender.bpy.ui.images.views import image_type, overlaid
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.color.processors import DisplayView, GREY, processor
from blender.imaging.files.buffers import applied
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.stores import content_hash, ContentHash

from ..catalog import chain
from ..chain.stages import Stage
from ..effects.formation.display import formation

# --- [MODELS] ---------------------------------------------------------------------------


class _Request(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Tone response sampling request with its host handles and the state it observed."""

    scene: bpy.types.Scene
    entries: tuple[bpy.types.SceneCompositorEffect, ...]
    guide: PyOpenColorIO.CPUProcessor
    window: tuple[float, float]
    columns: int
    observed: tuple[tuple[Snapshot, ...], tuple[Option[ContentHash], ...], tuple[int, int, float, int], str, str, tuple[float, float], tuple[str, ...]]
    content: Option[ContentHash]


# --- [SERVICES] -------------------------------------------------------------------------


class _Feed:
    """Plot columns the chart last drew, the wanted request, and the sampled response it answered."""

    __slots__ = ("cached", "columns", "wanted")

    def __init__(self) -> None:
        """Holds no columns, request, or response."""
        self.columns = 0
        self.wanted: Option[_Request] = Nothing
        self.cached: Option[tuple[_Request, npt.NDArray[np.float32], tuple[float, ...]]] = Nothing

    def wanting(self, request: _Request) -> bool:
        """Holds the request as wanted, dropping a response to another, answering whether the wanted request changed."""
        changed = self.wanted.filter(lambda held: held is request).is_none()
        self.wanted = Some(request)
        if changed:
            self.cached = Nothing
        return changed

    def published(self, request: _Request, values: npt.NDArray[np.float32], codes: tuple[float, ...]) -> bool:
        """Holds the response while its request stays wanted, answering whether it held it."""
        if self.wanted.filter(lambda current: current is request).is_none():
            return False
        self.cached = Some((request, values, codes))
        return True

    def cleared(self) -> bool:
        """Drops the wanted request and its response, answering whether either was held."""
        changed = self.wanted.is_some() or self.cached.is_some()
        self.wanted, self.cached = Nothing, Nothing
        return changed


_FEED: Final = Resource[_Feed](made=lambda _identity: nullcontext(_Feed()))


class ToneResponse(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Tone response of the chain's entries at `stages`, sampled at middle gray scaled across the allocation's stop window and guided through an encoding view of the formation display."""

    sampling: PreviewScene = PreviewScene()
    stages: frozenset[Stage] = frozenset({Stage.TONE, Stage.FORMATION})
    allocation: PyOpenColorIO.AllocationTransform | str = "blender:agxlog_rec2020_scene"
    guide: Option[PyOpenColorIO.Transform] = Nothing
    enabled: Enabled = Enabled.RENDER
    signal: Signal = msgspec.field(default_factory=Signal)
    levels: ClassVar[tuple[float, ...]] = (*(GREY * 2.0**stop for stop in range(-4, 3)), 1.0)
    key: ClassVar[Key] = "tone_response"

    # --- [READOUT]

    def chart(self, identity: Identity, metrics: Metrics) -> Outcome[Option[Chart]]:
        """Charts the cached response of the wanted request, recording the plot columns it draws at."""
        feed, width = identity.held[_FEED], float(metrics.unit * 10)
        feed.columns = round(width)

        def plotted(held: tuple[_Request, npt.NDArray[np.float32], tuple[float, ...]]) -> Chart:
            request, values, guides = held
            x = (np.arange(request.columns, dtype=np.float32) + 0.5) / request.columns
            rows = np.asarray(guides, dtype=np.float32)
            return Chart(
                width=width,
                height=width * 0.5,
                layers=(
                    Layer.of("LINES", np.stack((np.column_stack((np.zeros_like(rows), rows)), np.column_stack((np.ones_like(rows), rows))), axis=1).reshape(-1, 2), GRID),
                    Layer.of("LINE_STRIP", np.column_stack((x, values.mean(axis=1))).astype(np.float32), TRACE, blend="ADDITIVE", width=1.5),
                ),
                labels=tuple(Label(text=f"{level:.2%}", x=0.0, y=code) for level, code in zip(self.levels, guides)),
                caption=(bpy.path.display_name(self.key),),
            )

        return Ok(feed.cached.filter(lambda held: feed.wanted.filter(lambda request: held[0] is request and request.columns == feed.columns).is_some()).map(plotted))

    # --- [DEMAND]

    def fed(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns the current sampling job, reusing the wanted request while its observed state holds."""
        if not (feed := identity.held[_FEED]).columns:
            return Ok(())

        def allocated(config: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.Processor]:
            def walked(transform: PyOpenColorIO.Transform, *, inverse: bool) -> Generator[tuple[PyOpenColorIO.AllocationTransform, bool]]:
                match transform:
                    case PyOpenColorIO.AllocationTransform():
                        yield transform, inverse
                    case PyOpenColorIO.GroupTransform():
                        for member in transform:
                            yield from walked(member, inverse=inverse ^ (transform.getDirection() == PyOpenColorIO.TRANSFORM_DIR_INVERSE))
                    case _:
                        pass

            def selected(space: PyOpenColorIO.ColorSpace) -> Outcome[PyOpenColorIO.Processor]:
                transform = (
                    Option
                    .of_optional(space.getTransform(PyOpenColorIO.COLORSPACE_DIR_FROM_REFERENCE))
                    .map(lambda value: (value, PyOpenColorIO.TRANSFORM_DIR_FORWARD))
                    .or_else_with(lambda: Option.of_optional(space.getTransform(PyOpenColorIO.COLORSPACE_DIR_TO_REFERENCE)).map(lambda value: (value, PyOpenColorIO.TRANSFORM_DIR_INVERSE)))
                )
                match tuple(pair for value, direction in transform.to_list() for pair in walked(value, inverse=direction == PyOpenColorIO.TRANSFORM_DIR_INVERSE)):
                    case ((value, inverse),):
                        return processor(config, PyOpenColorIO.GroupTransform([value], direction=PyOpenColorIO.TRANSFORM_DIR_FORWARD if inverse else PyOpenColorIO.TRANSFORM_DIR_INVERSE))
                    case _:
                        return Error(Fault.of(Reason.ABSENT, space.getName(), member=PyOpenColorIO.AllocationTransform.__name__))

            match self.allocation:
                case str(name):
                    return Result[PyOpenColorIO.ColorSpace, Faults].bind(
                        Option.of_optional(config.getColorSpace(name)).to_result_with(partial(Fault.of, Reason.ABSENT, config.getName(), member=name)), selected
                    )
                case transform:
                    return processor(config, PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE))

        def prepared(scene: bpy.types.Scene, config: PyOpenColorIO.Config) -> Outcome[tuple[Job, ...]]:
            def guided() -> Outcome[PyOpenColorIO.Processor]:
                def encoded(held: Option[DisplayView]) -> Outcome[PyOpenColorIO.Processor]:
                    display = held.map(lambda view: view.display).default_with(lambda: viewed(scene).display)
                    return Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], Faults].bind(
                        DisplayView.for_encoding(config, display),
                        lambda found: found.map(lambda pair: pair[1]).to_result_with(partial(Fault.of, Reason.ABSENT, display, member=DisplayView.for_encoding.__name__)),
                    )

                return self.guide.map(lambda transform: processor(config, transform)).default_with(
                    lambda: formation.current(identity, scene).map(lambda found: found.map(lambda record: record.display_view)).bind(encoded)
                )

            rows = tuple(held for held in chain.rows.owned(identity, scene) if self.enabled.descriptor.read(held.entry) is True and held.row.record.stage in self.stages)
            selected = tuple(held.entry for held in rows)
            locality = accumulated(held.row.read(identity, held.entry).map(lambda record: record.record.pointwise) for held in rows)
            observations = accumulated(paired(Snapshot.capture(entry), Option.of_optional(entry.node_group).map(fingerprinted).default_value(Ok(Nothing))) for entry in selected)
            encoding = paired(guided(), allocated(config)).map(lambda built: tuple(value.getDefaultCPUProcessor() for value in built))

            def requested(local: tuple[bool, ...], captured: tuple[tuple[Snapshot, Option[ContentHash]], ...], processors: tuple[PyOpenColorIO.CPUProcessor, ...]) -> Outcome[tuple[Job, ...]]:
                guide, allocation = processors
                low, high = np.log2(applied(allocation, np.repeat(np.asarray((0.0, 1.0), dtype=np.float32)[:, None], 3, axis=1))[:, 0] / GREY)
                window = float(low), float(high)
                if not all(local):
                    return self.cleared(identity)
                snapshots, fingerprints = tuple(row[0] for row in captured), tuple(row[1] for row in captured)
                frame = scene.session_uid, scene.frame_current, scene.frame_subframe, feed.columns
                observed = snapshots, fingerprints, frame, config.getCacheID(), guide.getCacheID(), window, tuple(entry.name for entry in selected)
                content = (
                    Some(
                        content_hash(
                            ToneResponse,
                            *snapshots,
                            *(key for value in fingerprints for key in value.to_list()),
                            config.getCacheID().encode(),
                            arrays=(np.asarray((*frame, *window), dtype=np.float64), np.frombuffer(guide.getCacheID().encode(), dtype=np.uint8)),
                        )
                    )
                    if all(value.is_some() for value in fingerprints)
                    else Nothing
                )
                request = feed.wanted.filter(lambda held: held.observed == observed and held.entries == selected).default_with(
                    lambda: _Request(scene=scene, entries=selected, guide=guide, window=window, columns=feed.columns, observed=observed, content=content)
                )
                changed = feed.wanting(request)
                if feed.cached.filter(lambda held: held[0] is request).is_some():
                    return Ok(())
                return (self.signal.fired() if changed else Ok(None)).map(
                    lambda _: (Job(place=(self.key, identity.package), key=request.content.default_value(request), rank=Rank.SHOWN, run=lambda: self.rendered(identity, request)),)
                )

            return paired(locality, observations, encoding).bind(lambda parts: requested(*parts))

        return (
            Option
            .of_optional(context.scene)
            .map(lambda scene: prepared(scene, PyOpenColorIO.GetCurrentConfig()))
            .default_with(lambda: self.cleared(identity))
            .or_else_with(lambda faults: paired(Error(faults), self.cleared(identity)).map(lambda _: ()))
        )

    def rendered(self, identity: Identity, request: _Request) -> Outcome[None]:
        """Samples the entries and publishes the response only while the request stays wanted."""
        positions = (np.arange(request.columns, dtype=np.float32) + 0.5) / request.columns
        low, high = request.window
        levels = (GREY * np.exp2(low + (high - low) * positions)).astype(np.float32)
        codes = tuple(map(float, applied(request.guide, np.repeat(np.asarray(self.levels, dtype=np.float32)[:, None], 3, axis=1)).mean(axis=1)))

        return self.sampling.sampled(identity, request.scene, request.entries, np.repeat(levels[None, :, None], 3, axis=2)).bind(
            lambda values: self.signal.fired() if identity.held[_FEED].published(request, values[0, :, :3], codes) else Ok(None)
        )

    # --- [LIFETIME]

    def cleared(self, identity: Identity) -> Outcome[tuple[Job, ...]]:
        """Clears the wanted request and its cached response."""
        return (self.signal.fired() if identity.held[_FEED].cleared() else Ok(None)).map(lambda _: ())

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds the feed cell, the queue feed, the chart over the compositor viewer, and the invalidations removed before the cell clears."""
        with ExitStack() as scopes:

            def invalidated() -> Outcome[Option[str]]:
                return self.cleared(identity).map(lambda _: Nothing) if identity.held[_FEED].wanted.filter(lambda request: request.content.is_none()).is_some() else Ok(Nothing)

            later = scopes.enter_context(identity.reports.deferred(invalidated))

            def loaded() -> Outcome[None]:
                return self.cleared(identity).map(lambda _: None)

            yield Ok((
                _FEED,
                (Band.RESOURCES, PreviewQueue.fed(identity, self.fed)),
                self.sampling,
                overlaid(Reading(read=lambda metrics: self.chart(identity, metrics)), image_type(("COMPOSITING",)), (self.signal,)),
                Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                Handler(
                    handlers=bpy.app.handlers.depsgraph_update_post,
                    body=partial(
                        updated,
                        (bpy.types.Scene, bpy.types.NodeTree, bpy.types.Image),
                        lambda scene, _updates: Ok(later()) if identity.held[_FEED].wanted.filter(lambda request: request.scene == scene).is_some() else Ok(None),
                    ),
                ),
            ))


# --- [COMPOSITION] ----------------------------------------------------------------------

response: Final = ToneResponse()
rows: Final = (response,)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ToneResponse", "response", "rows"]
